using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace Diversions.ObjectModel
{
    /// <summary>
    /// This class marshals it's CRUD operations onto the application domain's <see cref="SynchronizationContext"/>,
    /// if one exists.  It obtains the SynchronizationContext indirectly via the use of <see cref="DiversionDelegate{TArg}"/>s
    /// for the <see cref="CollectionChanged"/> and <see cref="PropertyChanged"/> events.  Because <see cref="DiversionDelegate{TArg}"/>s
    /// are used for the events, this class is also suitable for crossing thread boundaries at the behest of the invoked EventHandlers.
    /// 
    /// NOTE: There are various implementations of <see cref="ObservableCollection{T}"/>s on the web that try to do
    /// the same thing, yet fail.  They do not override the CRUD operations; instead they simply invoke the <see cref="CollectionChanged"/>
    /// event on the application Dispatcher or send the invocation to the SynchronizationContext.  Superficially, this works but it
    /// creates a race-condition that can cause an ItemsViewCollection to fail while validating collection changes.  This implementation
    /// does not create that race-condition.
    /// </summary>
    /// <typeparam name="TItem">The type of the contained items.</typeparam>
    public class DivertingObservableCollection<TItem> : ObservableCollection<TItem>
    {
        private readonly DiversionDelegate<NotifyCollectionChangedEventArgs> _collectionChangedDelegate = new DiversionDelegate<NotifyCollectionChangedEventArgs>();
        private readonly DiversionDelegate<PropertyChangedEventArgs> _propertyChangedDelegate = new DiversionDelegate<PropertyChangedEventArgs>();
        private SynchronizationContext _synchronizationContext;

        /// <inheritdoc cref="ObservableCollection{T}"/>
        public override event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add 
            {
                // If ANY of the event handlers require use of a SynchronizationContext, then store
                // that context so that the Collection manipulation operations can be sent to that
                // thread context.  Any event handlers that define their own marshalling mechanism
                // will be dealt out appropriately by the MarshallingDelegate.
                var added = _collectionChangedDelegate.Add(value);
                if (added.MarshalInfo != null && added.MarshalInfo.SynchronizationContext != null)
                {
                    _synchronizationContext = added.MarshalInfo.SynchronizationContext;
                }
            }
            remove { _collectionChangedDelegate.Remove(value); }
        }

        /// <inheritdoc cref="ObservableCollection{T}"/>
        protected override event PropertyChangedEventHandler PropertyChanged
        {
            add
            {
                // If ANY of the event handlers require use of a SynchronizationContext, then store
                // that context so that the Collection manipulation operations can be sent to that
                // thread context.  Any event handlers that define their own marshalling mechanism
                // will be dealt out appropriately by the MarshallingDelegate.
                var added = _propertyChangedDelegate.Add(value);
                if (added.MarshalInfo != null && added.MarshalInfo.SynchronizationContext != null)
                {
                    _synchronizationContext = added.MarshalInfo.SynchronizationContext;
                }
            }
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
            if (_synchronizationContext == null)
            {
                base.InsertItem(index, item);
            }
            else
            {
                _synchronizationContext.Send(new SendOrPostCallback((s) => base.InsertItem(index, item)), null);
            }
        }

        /// <inheritdoc cref="ObservableCollection{T}"/>
        protected override void MoveItem(int oldIndex, int newIndex)
        {
            // If any of the event handlers exhibit a SynchronizationContext, make sure to use it here.
            if (_synchronizationContext == null)
            {
                base.MoveItem(oldIndex, newIndex);
            }
            else
            {
                _synchronizationContext.Send(new SendOrPostCallback((s) => base.MoveItem(oldIndex, newIndex)), null);
            }
        }

        /// <inheritdoc cref="ObservableCollection{T}"/>
        protected override void RemoveItem(int index)
        {
            // If any of the event handlers exhibit a SynchronizationContext, make sure to use it here.
            if (_synchronizationContext == null)
            {
                base.RemoveItem(index);
            }
            else
            {
                _synchronizationContext.Send(new SendOrPostCallback((s) => base.RemoveItem(index)), null);
            }
        }

        /// <inheritdoc cref="ObservableCollection{T}"/>
        protected override void SetItem(int index, TItem item)
        {
            // If any of the event handlers exhibit a SynchronizationContext, make sure to use it here.
            if (_synchronizationContext == null)
            {
                base.SetItem(index, item);
            }
            else
            {
                _synchronizationContext.Send(new SendOrPostCallback((s) => base.SetItem(index, item)), null);
            }
        }

        /// <inheritdoc cref="ObservableCollection{T}"/>
        protected override void ClearItems()
        {
            // If any of the event handlers exhibit a SynchronizationContext, make sure to use it here.
            if (_synchronizationContext == null)
            {
                base.ClearItems();
            }
            else
            {
                _synchronizationContext.Send(new SendOrPostCallback((s) => base.ClearItems()), null);
            }
        }
    }
}
