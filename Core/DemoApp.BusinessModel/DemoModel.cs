﻿using log4net;
using System;
using System.Threading;
using Diversions;
using Diversions.Mvvm;
using Diversions.ObjectModel;

namespace DemoApp.BusinessModel
{
    public class DemoModel : DivertingBindableBase
    {
        public static readonly ILog _logger = LogManager.GetLogger(typeof(DemoModel));
        private readonly DiversionDelegate<int> _notifyDelegate = new DiversionDelegate<int>();
        private int _notificationId;

        public DemoModel()
        {
        }

        public event EventHandler<int> Notify
        {
            add { _notifyDelegate.Add(value); }
            remove { _notifyDelegate.Remove(value); }
        }

        public DivertingObservableCollection<HandlerRecord> Records { get; } = new DivertingObservableCollection<HandlerRecord>();

        public void AddEventHandlerRecord(MarshalOption option, int notificationId)
        {
            lock (Records)
            {
                var r = new HandlerRecord()
                {
                    HandlerThreadId = Thread.CurrentThread.ManagedThreadId,
                    MarshalOption = option,
                    MarshalKey = option.ToString(),
                    NotificationId = notificationId
                };

                Records.Add(r);
            }
        }

        public void AddEventHandlerRecord(string optionKey, int notificationId)
        {
            lock (Records)
            {
                var r = new HandlerRecord()
                {
                    HandlerThreadId = Thread.CurrentThread.ManagedThreadId,
                    MarshalOption = MarshalOption.UserDefined,
                    MarshalKey = optionKey,
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
