using log4net;
using System;
using System.Threading;
using MarshallingDelegation;
using MarshallingDelegation.Mvvm;
using MarshallingDelegation.ObjectModel;

namespace DemoApp.BusinessModel
{
    public class DemoModel : MarshallingBindableBase
    {
        public static readonly ILog _logger = LogManager.GetLogger(typeof(DemoModel));
        private readonly MarshallingDelegate<int> _notifyDelegate = new MarshallingDelegate<int>();
        private int _notificationId;

        public DemoModel()
        {
        }

        public event EventHandler<int> Notify
        {
            add { _notifyDelegate.Add(value); }
            remove { _notifyDelegate.Remove(value); }
        }

        public MarshallingObservableCollection<HandlerRecord> Records { get; } = new MarshallingObservableCollection<HandlerRecord>();

        public void AddEventHandlerRecord(MarshalOption option, int notificationId)
        {
            lock (Records)
            {
                var r = new HandlerRecord()
                {
                    HandlerThreadId = Thread.CurrentThread.ManagedThreadId,
                    MarshalOption = option,
                    NotificationId = notificationId
                };

                Records.Add(r);
            }
        }

        public void RaiseEvent()
        {
            try
            {
                _notificationId++;
                _notifyDelegate.Invoke(this, _notificationId);
            }
            catch (Exception ex)
            {
                _logger.Error($"{nameof(RaiseEvent)}: {ex.GetType().Name} while invoking delegate.", ex);
            }
        }

        public void RaiseEventAsync()
        {
            _notificationId++;
            _notifyDelegate.InvokeAsync(this, _notificationId).ConfigureAwait(false);
        }
    }
}
