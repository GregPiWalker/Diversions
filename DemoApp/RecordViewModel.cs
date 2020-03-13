using System;
using DemoApp.BusinessModel;
using MarshallingDelegation.Mvvm;

namespace DemoApp
{
    public class RecordViewModel : MarshallingBindableBase
    {
        private int _delegateInvokeThreadId;

        public HandlerRecord Model { get; set; }

        public int DelegateInvokeThreadId
        {
            get => _delegateInvokeThreadId;
            set => SetProperty(ref _delegateInvokeThreadId, value, nameof(DelegateInvokeThreadId));
        }
    }
}
