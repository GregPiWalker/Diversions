using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using System.Threading;

namespace Diversions.ObjectModel
{
    /// <summary>
    /// This class is a derivation of the <see cref="ObservableCollection{T}"/>.
    /// It marshals it's CRUD operations onto the application domain's <see cref="SynchronizationContext"/>,
    /// if one exists.  It obtains the SynchronizationContext indirectly via the use of <see cref="DiversionDelegate{TArg}"/>s
    /// for the <see cref="INotifyCollectionChanged.CollectionChanged"/> and <see cref="INotifyPropertyChanged.PropertyChanged"/>
    /// events.  Because <see cref="DiversionDelegate{TArg}"/>s are used for the events, this class is also suitable for
    /// crossing thread boundaries at the behest of the invoked EventHandlers.
    /// Furthermore, if the <see cref="DiversionAttribute"/> classes static <see cref="DiversionAttribute.DefaultDiverter"/>
    /// property is set for a UI Dispatcher, then event handlers on the <see cref="INotifyPropertyChanged.PropertyChanged"/>
    /// event will be marshalled onto the dispatcher automatically.
    /// 
    /// NOTE: There are various implementations of <see cref="ObservableCollection{T}"/>s on the web that try to do
    /// the same thing, but are flawed.  They do not override the CRUD operations; instead they simply invoke the
    /// <see cref="INotifyCollectionChanged.CollectionChanged"/> event on the application Dispatcher or send the
    /// invocation to the SynchronizationContext.  Superficially, this works but it creates a race-condition that
    /// can cause an <see cref="Windows.Data.CollectionView"/> to fail while validating collection changes
    /// inside its ValidateCollectionChangedEventArgs method.  
    /// This implementation does not create that race-condition.
    /// </summary>
    /// <typeparam name="TItem">The type of the contained items.</typeparam>
    public class DivertingObservableCollection<TItem> : ObservableCollection<TItem>
    {
        internal delegate object DispatcherInvoke(Delegate d, params object[] a);
        private readonly DiversionDelegate<NotifyCollectionChangedEventArgs> _collectionChangedDelegate = new DiversionDelegate<NotifyCollectionChangedEventArgs>();
        private readonly DiversionDelegate<PropertyChangedEventArgs> _propertyChangedDelegate = new DiversionDelegate<PropertyChangedEventArgs>();

        Delegate _insertItemDel;
        Delegate _removeItemDel;
        Delegate _moveItemDel;
        Delegate _setItemDel;
        Delegate _clearItemsDel;

        public DivertingObservableCollection()
        {
            // this technique found at https://stackoverflow.com/questions/3631547/select-right-generic-method-with-reflection
            _insertItemDel = new Action<int, TItem>(base.InsertItem);
            _removeItemDel = new Action<int>(base.RemoveItem);
            _moveItemDel = new Action<int, int>(base.MoveItem);
            _setItemDel = new Action<int, TItem>(base.SetItem);
            _clearItemsDel = new Action(base.ClearItems);
        }

        /// <summary>
        /// Gets or sets the <see cref="SynchronizationContext"/> for the domain Dispatcher.
        /// This is used to marshall CRUD operations onto a Dispatcher Thread.  Because
        /// this class uses Diversions for notification events, event handlers may still
        /// be marshalled to other syncronization contexts as desired.
        /// </summary>
        public SynchronizationContext DispatcherSyncContext { get; set; }

        /// <inheritdoc cref="ObservableCollection{T}"/>
        public override event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { _collectionChangedDelegate.Add(value); }
            remove { _collectionChangedDelegate.Remove(value); }
        }

        /// <inheritdoc cref="ObservableCollection{T}"/>
        protected override event PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChangedDelegate.Add(value); }
            remove { _propertyChangedDelegate.Remove(value); }
        }

        /// <inheritdoc cref="ObservableCollection{T}"/>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            using (BlockReentrancy())
            {
                _collectionChangedDelegate.Invoke(this, e);
            }
        }

        /// <inheritdoc cref="ObservableCollection{T}"/>
        protected override void InsertItem(int index, TItem item)
        {
            // If any of the event handlers exhibit a SynchronizationContext, make sure to use it here.
            if (DispatcherDelegates.InvokeDelegate == null)
            {
                base.InsertItem(index, item);
            }
            else
            {
                DispatcherDelegates.InvokeDelegate.Invoke(_insertItemDel, index, item);
            }
        }

        /// <inheritdoc cref="ObservableCollection{T}"/>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            // If any of the event handlers exhibit a SynchronizationContext, make sure to use it here.
            if (DispatcherDelegates.InvokeDelegate == null)
            {
                base.MoveItem(oldIndex, newIndex);
            }
            else
            {
                DispatcherDelegates.InvokeDelegate.Invoke(_moveItemDel, oldIndex, newIndex);
            }
        }

        /// <inheritdoc cref="ObservableCollection{T}"/>
        protected override void RemoveItem(int index)
        {
            // If any of the event handlers exhibit a SynchronizationContext, make sure to use it here.
            if (DispatcherDelegates.InvokeDelegate == null)
            {
                base.RemoveItem(index);
            }
            else
            {
                DispatcherDelegates.InvokeDelegate.Invoke(_removeItemDel, index);
            }
        }

        /// <inheritdoc cref="ObservableCollection{T}"/>
        protected override void SetItem(int index, TItem item)
        {
            // If any of the event handlers exhibit a SynchronizationContext, make sure to use it here.
            if (DispatcherDelegates.InvokeDelegate == null)
            {
                base.SetItem(index, item);
            }
            else
            {
                DispatcherDelegates.InvokeDelegate.Invoke(_setItemDel, index, item);
            }
        }

        /// <inheritdoc cref="ObservableCollection{T}"/>
        protected override void ClearItems()
        {
            // If any of the event handlers exhibit a SynchronizationContext, make sure to use it here.
            if (DispatcherDelegates.InvokeDelegate == null)
            {
                base.ClearItems();
            }
            else
            {
                DispatcherDelegates.InvokeDelegate.Invoke(_clearItemsDel);
            }
        }
    }

    /// <summary>
    /// 
    /// </summary>
    public static class DispatcherDelegates
    {
        internal delegate object DispatcherInvoke(Delegate d, params object[] a);

        internal static DispatcherInvoke InvokeDelegate { get; private set; }

        public static void CreateInvokeDelegate(object marshaller, MethodInfo marshalMethod)
        {
            if (marshaller == null || marshalMethod == null)
            {
                InvokeDelegate = null;
            }
            else
            {
                // Create a delegate to the Invoke instance method.
                InvokeDelegate = (DispatcherInvoke)Delegate.CreateDelegate(typeof(DispatcherInvoke), marshaller, marshalMethod);
            }
        }
    }
}
