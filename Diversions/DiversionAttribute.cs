using System;
using System.Reflection;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Threading;
using log4net;

namespace Diversions
{
    public class DiversionAttribute : Attribute
    {
        public static readonly ILog _Logger = LogManager.GetLogger(typeof(DiversionAttribute));
        private static readonly Dictionary<MarshalOption, MarshalInfo> _Marshallers = new Dictionary<MarshalOption, MarshalInfo>();
        private static readonly string[] _Imports = new string[] { "System", "System.Threading", "System.Threading.Tasks" };
        private static readonly Assembly[] _Assemblies = new Assembly[] { };
        private static MarshalOption _DefaultDiverter;

        static DiversionAttribute()
        {
            AddDiverter(MarshalOption.CurrentThread, string.Empty, null, null);
            AddDiverter(MarshalOption.RunTask, Task.Factory, "StartNew", new Type[] { typeof(Action) }, null, false, new object[] { CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default });
            AddDiverter(MarshalOption.StartNewTask, Task.Factory, "StartNew", new Type[] { typeof(Action) });
            //AddOption(RedirectOption.TaskStartNew, "Task.Factory", "StartNew", new Type[] { typeof(Action) });
        }

        public DiversionAttribute(MarshalOption option)
        {
            SelectedDiverter = option;
            MarshalInfo = _Marshallers[option];
        }

        public DiversionAttribute([CallerMemberName] string callerName = null)
        {
            SelectedDiverter = DefaultDiverter;
            MarshalInfo = _Marshallers[DefaultDiverter];
            _Logger.Debug($"{nameof(DiversionAttribute)}: method \"{callerName}\" is using the default diverter option '{DefaultDiverter}'.");
        }

        public static MarshalOption DefaultDiverter 
        { 
            get => _DefaultDiverter;
            set
            {
                _DefaultDiverter = value;
                _Logger.Debug($"{nameof(DiversionAttribute)}: \"{nameof(DefaultDiverter)}\" was set to {nameof(MarshalOption)}.{value.ToString()}.");
            }
        }

        /// <summary>
        /// Gets the <see cref="MarshalInfo"/> that encapsulates the thread marshalling mechanism for this <see cref="DiversionAttribute"/>
        /// </summary>
        internal MarshalInfo MarshalInfo { get; set; }

        internal MarshalOption SelectedDiverter { get; private set; }

        public static void AddDiverter(MarshalOption option, object marshallerInstance, string marshalMethod, Type[] paramTypes, SynchronizationContext syncContext = null, bool makeDefault = false, object[] staticArguments = null)
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
                    DefaultDiverter = option;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"{nameof(AddDiverter)}: {ex.GetType().Name} while trying to add a new diverter.", ex);
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
