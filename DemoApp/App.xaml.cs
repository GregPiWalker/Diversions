using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace DelegateTestApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            // This forces WPF to behave as version 4.0 did.  Versions 4.5 and greater began to 
            // use new instances of SychronizationContexts even for the same thread.
            // Refer to:
            // https://docs.microsoft.com/en-us/dotnet/api/system.windows.basecompatibilitypreferences.reusedispatchersynchronizationcontextinstance?redirectedfrom=MSDN&view=netframework-4.8
            // https://stackoverflow.com/questions/13500030/comparing-synchronizationcontext
            BaseCompatibilityPreferences.ReuseDispatcherSynchronizationContextInstance = true;
        }
    }
}
