using System;
using System.Runtime.CompilerServices;
using log4net;

namespace Diversions
{
    public class DiversionAttribute : Attribute
    {
        public static readonly ILog _Logger = LogManager.GetLogger(typeof(DiversionAttribute));

        public DiversionAttribute(MarshalOption option)
        {
            SelectedDiverter = option;
            MarshalInfo = Diversion.GetInfo(SelectedDiverter);
        }

        public DiversionAttribute([CallerMemberName] string callerName = null)
        {
            SelectedDiverter = Diversion.DefaultDiverter;
            MarshalInfo = Diversion.GetInfo(SelectedDiverter);
            _Logger.Debug($"{nameof(DiversionAttribute)}: method \"{callerName}\" is using the default diverter option '{Diversion.DefaultDiverter}'.");
        }

        /// <summary>
        /// Gets the <see cref="MarshalInfo"/> that encapsulates the thread marshalling mechanism for this <see cref="DiversionAttribute"/>
        /// </summary>
        internal MarshalInfo MarshalInfo { get; set; }

        internal MarshalOption SelectedDiverter { get; private set; }
    }
}
