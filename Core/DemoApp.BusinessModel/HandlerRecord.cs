using log4net;
using System;
using Diversions;
using Diversions.Mvvm;

namespace DemoApp.BusinessModel
{
    public class HandlerRecord : DivertingBindableBase
    {
        private int _handlerThreadId;
        private int _notificationId;
        private string _key;
        private MarshalOption _option;

        public int HandlerThreadId
        {
            get => _handlerThreadId;
            set => SetProperty(ref _handlerThreadId, value, nameof(HandlerThreadId));
        }

        public int NotificationId
        {
            get => _notificationId;
            set => SetProperty(ref _notificationId, value, nameof(NotificationId));
        }

        public string MarshalKey
        {
            get => _key;
            set => SetProperty(ref _key, value, nameof(MarshalKey));
        }

        public MarshalOption MarshalOption
        {
            get => _option;
            set => SetProperty(ref _option, value, nameof(MarshalOption));
        }
    }
}
