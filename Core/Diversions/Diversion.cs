using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Threading;
using log4net;

namespace Diversions
{
    public static class Diversion
    {
        public static readonly ILog _Logger = LogManager.GetLogger(typeof(DiversionAttribute));
        private static readonly Dictionary<string, MarshalInfo> _Marshallers = new Dictionary<string, MarshalInfo>();
        //private static readonly Dictionary<Type, string> _ClassDiverters = new Dictionary<Type, string>();
        private static MarshalOption _DefaultDiverter;

        static Diversion()
        {
            // Set the 'current thread' option as the default initially.
            AddDiverter(MarshalOption.CurrentThread, string.Empty, null, null, null, true);
            AddDiverter(MarshalOption.RunTask, Task.Factory, "StartNew", new Type[] { typeof(Action) }, null, false, new object[] { CancellationToken.None, TaskCreationOptions.DenyChildAttach, TaskScheduler.Default });
            AddDiverter(MarshalOption.StartNewTask, Task.Factory, "StartNew", new Type[] { typeof(Action) });
            //AddOption(RedirectOption.TaskStartNew, "Task.Factory", "StartNew", new Type[] { typeof(Action) });
        }

        public static MarshalOption DefaultDiverter
        {
            get { return _DefaultDiverter; }
            set
            {
                _DefaultDiverter = value;
                _Logger.Debug($"{nameof(DiversionAttribute)}: \"{nameof(DefaultDiverter)}\" was set to {nameof(MarshalOption)}.{value.ToString()}.");
            }
        }

        public static void AddDiverter(MarshalOption option, object marshallerInstance, string marshalMethod, Type[] paramTypes, SynchronizationContext syncContext = null, bool makeDefault = false, object[] staticArguments = null)
        {
            try
            {
                MarshalInfo marshaller = null;
                if (marshallerInstance != null && !string.IsNullOrEmpty(marshalMethod))
                {
                    marshaller = new MarshalInfo(marshallerInstance, marshalMethod, paramTypes, syncContext, staticArguments);
                }

                _Marshallers.Add(option.ToString(), marshaller);

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

        internal static MarshalInfo GetInfo(MarshalOption option)
        {
            return GetInfo(option.ToString());
        }

        internal static MarshalInfo GetInfo(string option)
        {
            return _Marshallers[option];
        }

        //public static void SetClassDiverter(Type classType, MarshalOption option)
        //{
        //    _ClassDiverters[classType] = option.ToString();
        //}
    }
}
