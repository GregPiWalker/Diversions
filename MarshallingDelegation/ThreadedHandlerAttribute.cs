using System;
using System.Reflection;
using Microsoft.CodeAnalysis.Scripting;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;
using log4net;

namespace MarshallingDelegation
{
    public class ThreadedHandlerAttribute : Attribute
    {
        public static readonly ILog _Logger = LogManager.GetLogger(typeof(ThreadedHandlerAttribute));
        private static readonly Dictionary<MarshalOption, MarshalInfo> _Marshallers = new Dictionary<MarshalOption, MarshalInfo>();
        private static readonly string[] _Imports = new string[] { "System", "System.Threading", "System.Threading.Tasks" };
        private static readonly Assembly[] _Assemblies = new Assembly[] { };
        private static ScriptOptions _CompileOptions;
        private static MarshalOption _DefaultOption;

        static ThreadedHandlerAttribute()
        {
            PopulateOptions();

            AddOption(MarshalOption.CurrentThread, string.Empty, null, null);
            AddOption(MarshalOption.RunTask, Task.Factory, "StartNew", new Type[] { typeof(Action) }, null, false, new object[] { CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default });
            AddOption(MarshalOption.StartNewTask, Task.Factory, "StartNew", new Type[] { typeof(Action) });
            //AddOption(RedirectOption.TaskStartNew, "Task.Factory", "StartNew", new Type[] { typeof(Action) });
        }

        public ThreadedHandlerAttribute(MarshalOption option)
        {
            SelectedOption = option;
            MarshalInfo = _Marshallers[option];
        }

        public ThreadedHandlerAttribute([CallerMemberName] string callerName = null)
        {
            SelectedOption = DefaultThreadOption;
            MarshalInfo = _Marshallers[DefaultThreadOption];
            _Logger.Debug($"{nameof(ThreadedHandlerAttribute)}: method \"{callerName}\" is using the default thread context-switching option.");
        }

        public ThreadedHandlerAttribute(string marshallerExpression, string marshalMethod, Type[] paramTypes, string[] additionalImports, Type[] additionalTypes, SynchronizationContext syncContext = null, object[] staticArguments = null)
        {
            SelectedOption = MarshalOption.UserDefined;
            UpdateOptions(additionalImports, additionalTypes);
            MarshalInfo = new MarshalInfo(marshallerExpression, marshalMethod, paramTypes, _CompileOptions, syncContext, staticArguments);
        }

        public static MarshalOption DefaultThreadOption 
        { 
            get => _DefaultOption;
            set
            {
                _DefaultOption = value;
                _Logger.Debug($"{nameof(ThreadedHandlerAttribute)}: \"{nameof(DefaultThreadOption)}\" was set to {nameof(MarshalOption)}.{value.ToString()}.");
            }
        }

        /// <summary>
        /// Gets the <see cref="MarshalInfo"/> that encapsulates the thread marshalling mechanism for this <see cref="ThreadedHandlerAttribute"/>
        /// </summary>
        internal MarshalInfo MarshalInfo { get; set; }

        internal MarshalOption SelectedOption { get; private set; }

        public static void AddOption(MarshalOption option, object marshallerInstance, string marshalMethod, Type[] paramTypes, SynchronizationContext syncContext = null, bool makeDefault = false, object[] staticArguments = null)
        {
            try
            {
                MarshalInfo marshaller = null;
                if (marshallerInstance != null && !string.IsNullOrEmpty(marshalMethod))
                {
                    marshaller = new MarshalInfo(marshallerInstance, marshalMethod, paramTypes, syncContext, staticArguments);
                }

                _Marshallers.Add(option, marshaller);

                if (makeDefault)
                {
                    DefaultThreadOption = option;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"{nameof(AddOption)}: {ex.GetType().Name} while trying to add a new context-switch marshaller.", ex);
            }
        }

        private static void AddOption(MarshalOption option, string marshallerLoader, string marshalMethod, Type[] paramTypes, SynchronizationContext syncContext = null, bool makeDefault = false, object[] staticArguments = null)
        {
            try
            {
                MarshalInfo loader = null;
                if (!string.IsNullOrEmpty(marshallerLoader) && !string.IsNullOrEmpty(marshalMethod))
                {
                    loader = new MarshalInfo(marshallerLoader, marshalMethod, paramTypes, _CompileOptions, syncContext, staticArguments);
                }

                _Marshallers.Add(option, loader);

                if (makeDefault)
                {
                    DefaultThreadOption = option;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"{nameof(AddOption)}: {ex.GetType().Name} while trying to add a new context-switch marshaller.", ex);
            }
        }

        private static void PopulateOptions()
        {
            _CompileOptions = ScriptOptions.Default.WithImports(_Imports);
            _CompileOptions = _CompileOptions.WithReferences(_Assemblies);
        }

        private static void UpdateOptions(string[] additionalImports, Type[] additionalTypes)
        {
            lock (_CompileOptions)
            {
                if (additionalImports != null && additionalImports.Length > 0)
                {
                    _CompileOptions = _CompileOptions.WithImports(additionalImports);
                }

                if (additionalTypes != null && additionalTypes.Length > 0)
                {
                    var assemblies = additionalTypes.Select(t => t.Assembly).ToArray();
                    _CompileOptions = _CompileOptions.WithReferences(assemblies);
                }
            }
        }
    }

    internal class MarshalInfo
    {
        public const string LambdaOperator = "()=>";

        /// <summary>
        /// Creates a <see cref="MarshalInfo"/> that loads using a given class instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="targetMethod"></param>
        /// <param name="paramTypes"></param>
        /// <param name="staticArguments"></param>
        internal MarshalInfo(object instance, string targetMethod, Type[] paramTypes, SynchronizationContext syncContext = null, object[] staticArguments = null)
        {
            Marshaller = instance;
            StaticArguments = staticArguments;
            MethodParameters = paramTypes;
            SynchronizationContext = syncContext;

            if (staticArguments != null && staticArguments.Length > 0)
            {
                paramTypes = paramTypes.Concat(staticArguments.Select(o => o.GetType())).ToArray();
            }

            MarshalMethod = Marshaller.GetType().GetMethod(targetMethod, paramTypes);
        }

        /// <summary>
        /// Creates a <see cref="MarshalInfo"/> that loads by compiling an expression.
        /// </summary>
        /// <param name="instanceLoader"></param>
        /// <param name="targetMethod"></param>
        /// <param name="paramTypes"></param>
        /// <param name="options"></param>
        /// <param name="staticArguments"></param>
        internal MarshalInfo(string instanceLoader, string targetMethod, Type[] paramTypes, ScriptOptions options, SynchronizationContext syncContext = null, object[] staticArguments = null)
        {
            StaticArguments = staticArguments;
            MethodParameters = paramTypes;
            SynchronizationContext = syncContext;

            if (staticArguments != null && staticArguments.Length > 0)
            {
                paramTypes = paramTypes.Concat(staticArguments.Select(o => o.GetType())).ToArray();
            }

            CompileExpression(instanceLoader, targetMethod, paramTypes, options);
        }

        public SynchronizationContext SynchronizationContext { get; private set; }

        public object[] StaticArguments { get; private set; }

        public Type[] MethodParameters { get; private set; }

        public object Marshaller { get; private set; }

        public MethodInfo MarshalMethod { get; private set; }

        private async void CompileExpression(string loaderExpression, string methodName, Type[] paramTypes, ScriptOptions options)
        {
            var scriptState = await CSharpScript.RunAsync<Func<object>>(LambdaOperator + loaderExpression, options);
            Marshaller = scriptState.ReturnValue();
            MarshalMethod = Marshaller.GetType().GetMethod(methodName, paramTypes);
        }

        //ParameterExpression it;
        //string text;
        //int textPos;
        //int textLen;
        //char ch;
        //Token token;
        //Dictionary<string, object> symbols = new Dictionary<string, object>();

        //Expression ParsePrimary()
        //{
        //    Expression expr = ParsePrimaryStart();
        //    while (true)
        //    {
        //        if (token.id == TokenId.Dot)
        //        {
        //            NextToken();
        //            expr = ParseMemberAccess(null, expr);
        //        }
        //        else
        //        {
        //            break;
        //        }
        //    }
        //    return expr;
        //}

        //Expression ParsePrimaryStart()
        //{
        //    switch (token.id)
        //    {
        //        case TokenId.Identifier:
        //            return ParseIdentifier();
        //        default:
        //            throw new Exception("Parse Error: expression expected");
        //    }
        //}

        //Expression ParseIdentifier()
        //{
        //    ValidateToken(TokenId.Identifier, "syntax error");
        //    object value;
        //    if (symbols.TryGetValue(token.text, out value))
        //    {
        //        Expression expr = value as Expression;
        //        if (expr == null)
        //        {
        //            expr = Expression.Constant(value);
        //        }
        //        else
        //        {
        //            LambdaExpression lambda = expr as LambdaExpression;
        //            //if (lambda != null) return ParseLambdaInvocation(lambda);
        //        }
        //        NextToken();
        //        return expr;
        //    }
        //    if (it != null) return ParseMemberAccess(null, it);
        //    throw new Exception("Parse Error: unknown identifier '" + token.text + "'");
        //}

        //Expression ParseMemberAccess(Type type, Expression instance)
        //{
        //    if (instance != null) type = instance.Type;
        //    int errorPos = token.pos;
        //    string id = GetIdentifier();
        //    NextToken();
        //    if (token.id == TokenId.Dot)
        //    {
        //        bool isStatic = instance == null || type.IsValueType;
        //        MemberInfo member = FindPropertyOrField(type, id, isStatic);
        //        //if (member == null)
        //        //    throw ParseError(errorPos, Res.UnknownPropertyOrField,
        //        //        id, GetTypeName(type));
        //        Expression parameter = isStatic ? null : instance;
        //        bool isProperty = member is PropertyInfo;
        //        return isProperty ?
        //            Expression.Property(parameter, (PropertyInfo)member) :
        //            Expression.Field(parameter, (FieldInfo)member);
        //    }
            
        //    throw new Exception("Parse Error: dot operator expected");
        //}

        //string GetIdentifier()
        //{
        //    ValidateToken(TokenId.Identifier, "IdentifierExpected");
        //    string id = token.text;
        //    if (id.Length > 1 && id[0] == '@') id = id.Substring(1);
        //    return id;
        //}

        //void ValidateToken(TokenId t, string errorMessage)
        //{
        //    if (token.id != t) throw new Exception("ParseException: " + errorMessage);
        //}
        
        //MemberInfo FindPropertyOrField(Type type, string memberName, bool staticAccess)
        //{
        //    BindingFlags flags = BindingFlags.Public | BindingFlags.DeclaredOnly |
        //        (staticAccess ? BindingFlags.Static : BindingFlags.Instance);
        //    foreach (Type t in SelfAndBaseTypes(type))
        //    {
        //        MemberInfo[] members = t.FindMembers(MemberTypes.Property | MemberTypes.Field,
        //            flags, Type.FilterNameIgnoreCase, memberName);
        //        if (members.Length != 0) return members[0];
        //    }
        //    return null;
        //}

        //static IEnumerable<Type> SelfAndBaseTypes(Type type)
        //{
        //    if (type.IsInterface)
        //    {
        //        List<Type> types = new List<Type>();
        //        AddInterface(types, type);
        //        return types;
        //    }
        //    return SelfAndBaseClasses(type);
        //}

        //static IEnumerable<Type> SelfAndBaseClasses(Type type)
        //{
        //    while (type != null)
        //    {
        //        yield return type;
        //        type = type.BaseType;
        //    }
        //}

        //static void AddInterface(List<Type> types, Type type)
        //{
        //    if (!types.Contains(type))
        //    {
        //        types.Add(type);
        //        foreach (Type t in type.GetInterfaces()) AddInterface(types, t);
        //    }
        //}

        //void NextChar()
        //{
        //    if (textPos < textLen) textPos++;
        //    ch = textPos < textLen ? text[textPos] : '\0';
        //}

        //void NextToken()
        //{
        //    while (Char.IsWhiteSpace(ch)) NextChar();
        //    TokenId t;
        //    int tokenPos = textPos;
        //    switch (ch)
        //    {
        //        case '.':
        //            NextChar();
        //            t = TokenId.Dot;
        //            break;
        //        default:
        //            if (Char.IsLetter(ch) || ch == '@' || ch == '_')
        //            {
        //                //if (ch == 'i')
        //                //{
        //                //    NextChar();
        //                //    if (ch == 's')
        //                //    {
        //                //        t = TokenId.Is;
        //                //        NextChar();
        //                //        if (Char.IsWhiteSpace(ch))
        //                //        {
        //                //            break;
        //                //        }
        //                //    }
        //                //}
        //                t = TokenId.Identifier;
        //                do
        //                {
        //                    //Handle generic types
        //                    NextChar();
        //                    //if (ch == '<')
        //                    //{
        //                    //    t = TokenId.TypeIdentifier;
        //                    //}
        //                } while (Char.IsLetterOrDigit(ch) || ch == '_' || (t == TokenId.TypeIdentifier && (ch == '<' || ch == '>' || ch == ',')));
        //                break;
        //            }
        //            if (textPos == textLen)
        //            {
        //                t = TokenId.End;
        //                break;
        //            }
        //            throw new Exception("Parse Error: invalid character '" + ch + "' at " + textPos);
        //    }
        //    token.id = t;
        //    token.text = text.Substring(tokenPos, textPos - tokenPos);
        //    token.pos = tokenPos;
        //}

        //struct Token
        //{
        //    public TokenId id;
        //    public string text;
        //    public int pos;
        //}

        //enum TokenId
        //{
        //    Unknown,
        //    End,
        //    Identifier,
        //    TypeIdentifier,
        //    SubstitutionIdentifier,
        //    Dot
        //}
    }
}
