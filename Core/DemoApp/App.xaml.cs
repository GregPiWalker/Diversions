﻿using Diversions.ObjectModel;
using System;
using System.Windows;

namespace DemoApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        static App()
        {
            // This forces WPF to behave as version 4.0 did.  Versions 4.5 and greater began to 
            // use new instances of SychronizationContexts even for the same thread.  It can only
            // be set once and must be done before the Application is instantiated.
            // Refer to:
            // https://docs.microsoft.com/en-us/dotnet/api/system.windows.basecompatibilitypreferences.reusedispatchersynchronizationcontextinstance?redirectedfrom=MSDN&view=netframework-4.8
            // https://stackoverflow.com/questions/13500030/comparing-synchronizationcontext
            //BaseCompatibilityPreferences.ReuseDispatcherSynchronizationContextInstance = true;
        }

        public App()
        {
            // This prepares all DivertingObservableCollections to automatically marshal all their work onto the main application thread via the Dispatcher.
            DispatcherDelegates.CreateInvokeDelegate(Current.Dispatcher, Current.Dispatcher.GetType().GetMethod("Invoke", new Type[] { typeof(Delegate), typeof(object[]) }));
        }
    }
}
