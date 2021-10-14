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
    /// if one exists.  It obtains the SynchronizationContext indirectly via the static <see cref="DispatcherDelegates"/>
    /// class; therefore, an application must call <see cref="DispatcherDelegates.CreateInvokeDelegate"/> sometime during
    /// set-up in order for thread marshalling to occur here.  Because <see cref="DiversionDelegate{TArg}"/>s are used 
    /// for the events, this class is also suitable for crossing thread boundaries at the behest of the invoked EventHandlers.
    /// Furthermore, if the <see cref="DiversionAttribute"/> classes static <see cref="DiversionAttribute.DefaultDiverter"/>
    /// property is set for a UI Dispatcher, then event handlers on the <see cref="INotifyPropertyChanged.PropertyChanged"/>
    /// event will be marshalled onto the dispatcher automatically.
    /// 
    /// NOTE: There are various implementations of <see cref="ObservableCollection{T}"/>s on the web that try 
    /// automatically perform their <see cref="CollectionChanged"/> notifications on the UI thread, but they are flawed.
    /// They do not override the CRUD operations; instead they simply invoke the
    /// <see cref="INotifyCollectionChanged.CollectionChanged"/> event on the application Dispatcher or send the
    /// invocation to the SynchronizationContext.  Superficially, this works but it creates a race-condition that
    /// can cause an <see cref="Windows.Data.CollectionView"/> to fail while validating collection changes
    /// inside its ValidateCollectionChangedEventArgs method.  
    /// This implementation has no such race-condition flaw.
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
        /// Occurs when the collection contents change.
        /// This event uses DiversionDelegates, so event handlers may still
        /// be marshalled to other syncronization contexts as defined by Diversion attributes.
        /// </summary>
        public override event NotifyCollectionChangedEventHandler CollectionChanged
        {
            add { _collectionChangedDelegate.Add(value); }
            remove { _collectionChangedDelegate.Remove(value); }
        }

        /// <summary>
        /// Occurs when a property value changes.
        /// This event uses DiversionDelegates, so event handlers may still
        /// be marshalled to other syncronization contexts as defined by Diversion attributes.
        /// </summary>
        protected override event PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChangedDelegate.Add(value); }
            remove { _propertyChangedDelegate.Remove(value); }
        }

        /// <summary>
        /// Adds an object to the end of the <see cref="Collection{T}"/>.
        /// Provided that <see cref="DispatcherDelegates.CreateInvokeDelegate"/>
        /// has been invoked at least once prior to this call, then
        /// element addition automatically occurs on the main application thread.
        /// </summary>
        /// <param name="item">The object to be added to the end of the collection.  The value can be null for reference types.</param>
        public new void Add(TItem item)
        {
            // This is merely overridden in order to provide custom meta-data.
            base.Add(item);
        }

        /// <summary>
        /// Removes the first occurrence of a specified object from the <see cref="Collection{T}"/>.
        /// Provided that <see cref="DispatcherDelegates.CreateInvokeDelegate"/>
        /// has been invoked at least once prior to this call, then
        /// element removal automatically occurs on the main application thread. 
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <param name="item">The item to remove.</param>
        /// <returns><c>true</c> if the item was removed; <c>false</c> otherwise.  This also returns
        /// false if the specified object was not found.</returns>
        public new bool Remove(TItem item)
        {
            // This is merely overridden in order to provide custom meta-data.
            return base.Remove(item);
        }

        /// <summary>
        /// Removes the element at the specified index of the <see cref="Collection{T}"/>.
        /// Provided that <see cref="DispatcherDelegates.CreateInvokeDelegate"/>
        /// has been invoked at least once prior to this call, then
        /// element removal automatically occurs on the main application thread.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"></exception>
        /// <param name="index">The zero-based index of the element to remove.</param>
        public new void RemoveAt(int index)
        {
            // This is merely overridden in order to provide custom meta-data.
            base.RemoveAt(index);
        }

        /// <summary>
        /// Removes all elements from the <see cref="Collection{T}"/>.
        /// Provided that <see cref="DispatcherDelegates.CreateInvokeDelegate"/>
        /// has been invoked at least once prior to this call, then
        /// element removal automatically occurs on the main application thread.
        /// </summary>
        public new void Clear()
        {
            // This is merely overridden in order to provide custom meta-data.
            base.Clear();
        }

        /// <inheritdoc cref="ObservableCollection{T}.OnCollectionChanged"/>
        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            using (BlockReentrancy())
            {
                _collectionChangedDelegate.Invoke(this, e);
            }
        }

        /// <inheritdoc cref="ObservableCollection{T}.InsertItem"/>
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

        /// <inheritdoc cref="ObservableCollection{T}.MoveItem"/>
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

        /// <inheritdoc cref="ObservableCollection{T}.RemoveItem"/>
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

        /// <inheritdoc cref="ObservableCollection{T}.SetItem"/>
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

        /// <inheritdoc cref="ObservableCollection{T}.ClearItems"/>
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
