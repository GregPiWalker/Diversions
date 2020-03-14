using System;
using System.Reflection;
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
        private static MarshalOption _DefaultOption;

        static ThreadedHandlerAttribute()
        {
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

        public SynchronizationContext SynchronizationContext { get; private set; }

        public object[] StaticArguments { get; private set; }

        public Type[] MethodParameters { get; private set; }

        public object Marshaller { get; private set; }

        public MethodInfo MarshalMethod { get; private set; }
    }
}
