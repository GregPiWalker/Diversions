using System;
using System.Collections.Specialized;
using System.Threading;
using System.Windows;
using Prism.Commands;
using Diversions;
using Diversions.ObjectModel;
using Diversions.Mvvm;
using DemoApp.BusinessModel;

namespace DemoApp
{
    public sealed class DemoViewModel : DiverterBindableBase, IDisposable
    {
        private int _asyncThreadId = 0;

        static DemoViewModel()
        {
            // Add the option to use the UI Dispatcher, and set it to be the default option for implicit diversions.
            DiversionAttribute.AddDiverter(MarshalOption.Dispatcher, Application.Current.Dispatcher, "Invoke", new Type[] { typeof(Delegate), typeof(object[]) }, SynchronizationContext.Current, true);
        }

        public DemoViewModel()
        {
            Model = new DemoModel();

            StimulateCommand = new DelegateCommand(Model.RaiseEvent);
            StimulateAsyncCommand = new DelegateCommand(Model.RaiseEventAsync);

            Model.Records.CollectionChanged += HandleRecordCollectionChanged;
            Model.Notify += HandleNotifyOnCurrentThread;
            Model.Notify += HandleNotifyOnUiThread;
            Model.Notify += HandleNotifyOnNewTask;
            Model.Notify += HandleNotifyOnRunTask;
        }

        public DemoModel Model { get; private set; }

        public DivertingObservableCollection<RecordViewModel> EventRecords { get; } = new DivertingObservableCollection<RecordViewModel>();
        
        public DelegateCommand StimulateCommand { get; private set; }

        public DelegateCommand StimulateAsyncCommand { get; private set; }

        public void Dispose()
        {
            Model.Records.CollectionChanged -= HandleRecordCollectionChanged;
            Model.Notify -= HandleNotifyOnUiThread;
            Model.Notify -= HandleNotifyOnNewTask;
            Model.Notify -= HandleNotifyOnRunTask;
            Model.Notify -= HandleNotifyOnCurrentThread;
        }

        private void HandleNotifyOnUiThread(object sender, int arg)
        {
            Model.AddEventHandlerRecord(DiversionAttribute.DefaultDiverter, arg);
        }

        [Diversion(MarshalOption.StartNewTask)]
        private void HandleNotifyOnNewTask(object sender, int arg)
        {
            Model.AddEventHandlerRecord(MarshalOption.StartNewTask, arg);
        }

        [Diversion(MarshalOption.RunTask)]
        private void HandleNotifyOnRunTask(object sender, int arg)
        {
            Model.AddEventHandlerRecord(MarshalOption.RunTask, arg);
        }

        [Diversion(MarshalOption.CurrentThread)]
        private void HandleNotifyOnCurrentThread(object sender, int arg)
        {
            // Getting the thread ID here works because the handler uses the caller's thread, and also
            // because it is the first event handler added to the event.
            _asyncThreadId = Thread.CurrentThread.ManagedThreadId;
            Model.AddEventHandlerRecord(MarshalOption.CurrentThread, arg);
        }

        /// <summary>
        /// Use a task here to illustrate that the data binding hooked up to the <see cref="EventRecords"/> collection
        /// still marshalls onto the UI thread for the GUI update.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        [Diversion(MarshalOption.RunTask)]
        private void HandleRecordCollectionChanged(object sender, NotifyCollectionChangedEventArgs args)
        {
            // This method is multithreaded to demonstate the next marshalling stage for the UI update.
            // Synchronize it here to keep bad things from happening.
            lock (EventRecords)
            {
                if (args.NewItems != null)
                {
                    foreach (var record in args.NewItems)
                    {
                        var recordVM = new RecordViewModel() { Model = record as HandlerRecord, DelegateInvokeThreadId = _asyncThreadId };
                        EventRecords.Add(recordVM);
                    }
                }
            }
        }
    }
}
