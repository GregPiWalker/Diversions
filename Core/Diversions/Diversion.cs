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
        private static string _DefaultDiverter;

        /// <summary>
        /// Static constructor adds the default available Diverters.  Other classes may add more Diverters later.
        /// </summary>
        static Diversion()
        {
            // Set the 'current thread' option as the default initially.
            AddDiverter(MarshalOption.CurrentThread, string.Empty, null, null, null, true);
            AddDiverter(MarshalOption.RunTask, Task.Factory, "StartNew", new List<KeyValuePair<Type, object>>().AddKey(typeof(Action)).AddValue(CancellationToken.None).AddValue(TaskCreationOptions.DenyChildAttach).AddValue(TaskScheduler.Default));
            //TODO: make this a ThreadPool diverter?
            //AddDiverter(MarshalOption.StartNewTask, Task.Factory, "StartNew", new List<KeyValuePair<Type, object>>().AddKey(typeof(Action)));
        }

        /// <summary>
        /// 
        /// </summary>
        public static string DefaultDiverter
        {
            get { return _DefaultDiverter; }
            set
            {
                _DefaultDiverter = value;
                _Logger.Debug($"{nameof(DiversionAttribute)}: \"{nameof(DefaultDiverter)}\" was set to {value}.");
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="optionKey">A key used to store and reference the Diverter that is being added.</param>
        /// <param name="marshallerInstance"></param>
        /// <param name="marshalMethod"></param>
        /// <param name="marshalMethodInputs"></param>
        /// <param name="syncContext"></param>
        /// <param name="makeDefault"></param>
        public static void AddDiverter(string optionKey, object marshallerInstance, string marshalMethod, List<KeyValuePair<Type, object>> marshalMethodInputs, SynchronizationContext syncContext = null, bool makeDefault = false)
        {
            try
            {
                MarshalInfo marshaller = null;
                if (marshallerInstance != null && !string.IsNullOrEmpty(marshalMethod))
                {
                    marshaller = new MarshalInfo(marshallerInstance, marshalMethod, marshalMethodInputs == null ? Array.Empty<KeyValuePair<Type, object>>() : marshalMethodInputs.ToArray(), syncContext);
                }

                _Marshallers.Add(optionKey, marshaller);

                if (makeDefault)
                {
                    DefaultDiverter = optionKey;
                }
            }
            catch (Exception ex)
            {
                _Logger.Error($"{nameof(AddDiverter)}: {ex.GetType().Name} while trying to add a new diverter.", ex);
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="marshalOption"></param>
        /// <param name="marshallerInstance"></param>
        /// <param name="marshalMethod"></param>
        /// <param name="marshalMethodInputs"></param>
        /// <param name="syncContext">The synchronizationContext with which to raise </param>
        /// <param name="makeDefault"></param>
        public static void AddDiverter(MarshalOption marshalOption, object marshallerInstance, string marshalMethod, List<KeyValuePair<Type, object>> marshalMethodInputs, SynchronizationContext syncContext = null, bool makeDefault = false)
        {
            if (marshalOption == MarshalOption.UserDefined)
            {
                throw new InvalidOperationException($"User defined marshal options must call {nameof(AddDiverter)} passing a string argument into the first parameter.");
            }

            AddDiverter(marshalOption.ToString(), marshallerInstance, marshalMethod, marshalMethodInputs, syncContext, makeDefault);
        }

        internal static MarshalInfo GetInfo(MarshalOption option)
        {
            return GetInfo(option.ToString());
        }

        internal static MarshalInfo GetInfo(string option)
        {
            return _Marshallers[option];
        }
    }
}
