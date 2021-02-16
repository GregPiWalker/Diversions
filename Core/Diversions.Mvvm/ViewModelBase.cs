///<remarks>
/// This is an auto-generated file.  Content is generated from DivertingBindableBase.txt and ViewModelBase.tt.
/// Use of a T4 template allows two different class implementations that both share a single implementation of
/// common <see cref="Prism.Mvvm.BindableBase"/> code that has been modified to offer the Diversion feature.
/// Inheritance was not an option, as this class must inherit from the abstract <see cref="DynamicObject"/>.
///</remarks>
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Dynamic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.CodeDom.Compiler;
using log4net;

namespace Diversions.Mvvm
{
    /// <summary>
    /// The abstract base class to be used for View Models that have a 1:1 directed association to a Model.
    ///
    /// This is a composite of <see cref="Prism.Mvvm.BindableBase"/> and <see cref="DynamicObject"/>.
    /// Extending <see cref="DynamicObject"/> allows this class to be a model proxy even for properties that it doesn't define.
    /// This approach prevents bind-through while avoiding the tedious and error-prone reimplementation of domain object properties.
    /// See: https://docs.microsoft.com/en-us/archive/msdn-magazine/2010/july/design-patterns-problems-and-solutions-with-model-view-viewmodel
    /// Since this needs to extend <see cref="DynamicObject"/> for dynamic property handling, the implementation of 
    /// <see cref="Prism.Mvvm.BindableBase"/> is duplicated here.  However, the implementation of <see cref="Prism.Mvvm.BindableBase"/>
    /// is altered slightly in order to support the use of <see cref="DiversionDelegate{TArg}"/>s.
    /// Specifically, the <see cref="INotifyPropertyChanged.PropertyChanged"/> event re-written to use a
    /// <see cref="DiversionDelegate{TArg}"/> event handler.  The event is raised synchronously on
    /// the caller's thread, but each event observer may divert the flow to a thread of
    /// their choosing.  
    /// 
    /// If the <see cref="DiversionAttribute"/> class' static 
    /// <see cref="DiversionAttribute.DefaultDiverter"/> property is set for a UI Dispatcher,
    /// then event handlers on the <see cref="INotifyPropertyChanged.PropertyChanged"/> event will be automatically
    /// marshalled onto the dispatcher.  While .NET marshalls data bindings on <see cref="INotifyPropertyChanged"/>
    /// to the UI thread internally, Diversions extend that automatic marshalling to
    /// UserControls and CustomControls that observe ViewModel/Model events.
    /// </summary>
    [GeneratedCodeAttribute("TextTemplatingFileGenerator", "1.0.0.0")]
    public abstract class ViewModelBase : DynamicObject, INotifyPropertyChanged
    {
        protected readonly Dictionary<string, object> _proxyProperties = new Dictionary<string, object>();
        protected readonly ILog _logger;
        protected object _model;
        protected Type _modelType;

        protected ViewModelBase(object model)
        {
            Model = model;
        }

        protected ViewModelBase(object model, ILog logger)
        {
            _logger = logger;
            Model = model;
        }

        /// <summary>
        /// This could become a base class Type.
        /// </summary>
        public object Model
        {
            get => _model;
            protected set
            {
                SubscribeToModel(false);

                // This needs to be set before SetProperty is called because the
                // call stack will use the value before SetProperty returns.
                _modelType = (value == null) ? null : value.GetType();

                if (SetProperty(ref _model, value))
                {
                    SubscribeToModel(true);
                }
            }
        }

        /// <summary>
        /// Right now, caching is not thread-safe.
        /// </summary>
        public bool EnablePropertyCaching { get; set; }


        #region DynamicObject overrides with Proxy Property support

        /// <summary>
        /// Override of <see cref="DynamicObject.TryGetMember(GetMemberBinder, out object)"/> that gives
        /// this class a Proxy Property capability.  If this class' Model has the desired property,
        /// then the get operation will propagate to the Model object.  However, if caching is
        /// enabled here, then the this will first attempt to get a value from the local cache.
        /// </summary>
        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            if (Model == null)
            {
                result = null;
                return false;
            }

            string propertyName = binder.Name;
            if (EnablePropertyCaching && _proxyProperties.ContainsKey(propertyName))
            {
                result = _proxyProperties[propertyName];

                //TODO: consider logging that now we are using a cached value from a proxy property.
                return true;
            }

            PropertyInfo property = _modelType.GetProperty(propertyName);
            if (property == null || property.CanRead == false)
            {
                result = null;
                return false;
            }

            result = GetModelPropertyValue(property);

            // Only update the cached value here if the model does not notify.  If the model does notify,
            // then we'll update the cached value in the notify handler or the proxy setter.
            // The value may or may not be null, doesn't matter at this point.
            if (EnablePropertyCaching && !(Model is INotifyPropertyChanged) && _proxyProperties.ContainsKey(propertyName))
            {
                // Now cache the property and it's value locally.  Later we'll look in the cache first.
                _proxyProperties[propertyName] = result;
            }

            // The property exists, whether it has a value or not, so return true;
            return true;
        }

        /// <summary>
        /// Override of <see cref="DynamicObject.TrySetMember(SetMemberBinder, object)"/> that gives
        /// this class a Proxy Property capability.  If this class' Model has the desired property,
        /// then the set operation will propagate to the Model object.  If that succeeds and caching is
        /// enabled here, then the value will also be saved in a local cache.
        /// </summary>
        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            if (Model == null)
            {
                return false;
            }

            string propertyName = binder.Name;
            PropertyInfo modelProperty = _modelType.GetProperty(propertyName);

            if (modelProperty == null || modelProperty.CanWrite == false)
            {
                return false;
            }

            modelProperty.SetValue(Model, value, null);

            // Only cache the value and raise change events here if the underlying model does not notify.
            // Otherwise, the event and new value will be handled in this class' notify handler, HandleModelPropertyChanged.
            if (EnablePropertyCaching && !(Model is INotifyPropertyChanged))
            {
                // Now cache the property and it's value locally.  Later we'll look in the cache first.
                _proxyProperties[propertyName] = value;
                RaisePropertyChanged(propertyName);

                // Now propagate the change to any affected local properties.
                NotifyAffectedModelProperties(modelProperty);
            }

            return true;
        }

        #endregion DynamicObject overrides with Proxy Property support
        

        #region T4 Template: Begin Auto-Inserted Code

        #region Taken verbatim from Prism.Mvvm.BindableBase

        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            RaisePropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Checks if a property already matches a desired value. Sets the property and
        /// notifies listeners only when necessary.
        /// </summary>
        /// <typeparam name="T">Type of the property.</typeparam>
        /// <param name="storage">Reference to a property with both getter and setter.</param>
        /// <param name="value">Desired value for the property.</param>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers that
        /// support CallerMemberName.</param>
        /// <param name="onChanged">Action that is called after the property value has been changed.</param>
        /// <returns>True if the value was changed, false if the existing value matched the
        /// desired value.</returns>
        protected virtual bool SetProperty<T>(ref T storage, T value, Action onChanged, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(storage, value)) return false;

            storage = value;
            onChanged?.Invoke();
            RaisePropertyChanged(propertyName);

            return true;
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="propertyName">Name of the property used to notify listeners. This
        /// value is optional and can be provided automatically when invoked from compilers
        /// that support <see cref="CallerMemberNameAttribute"/>.</param>
        protected void RaisePropertyChanged([CallerMemberName] string propertyName = null)
        {
            OnPropertyChanged(new PropertyChangedEventArgs(propertyName));
        }

        /// <summary>
        /// Raises this object's PropertyChanged event using a <see cref="DiversionDelegate{TArg}"/>.
        /// </summary>
        /// <param name="args">The PropertyChangedEventArgs.</param>
        protected void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            _propertyChangedDelegate.Invoke(this, args);
        }

        #endregion Taken verbatim from Prism.Mvvm.BindableBase


        #region Prism.Mvvm.BindableBase Re-writes with DiversionDelegate Support

        private readonly DiversionDelegate<PropertyChangedEventArgs> _propertyChangedDelegate = new DiversionDelegate<PropertyChangedEventArgs>();

        /// <summary>
        /// Notifies observers that a property value has changed. This event is backed
        /// by a <see cref="DiversionDelegate{TArg}"/>, which provides the plumbing for
        /// marshalling event handler invocations onto the appropriate threads.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChangedDelegate.Add(value); }
            remove { _propertyChangedDelegate.Remove(value); }
        }

        #endregion Prism.Mvvm.BindableBase Re-writes with DiversionDelegate Support

        #endregion T4 Template: End Auto-Inserted Code


        /// <summary>
        /// Get the value of the given property from the Model object.
        /// </summary>
        /// <param name="property"></param>
        /// <returns></returns>
        public object GetModelPropertyValue(PropertyInfo property)
        {
            if (property == null || property.CanRead == false)
            {
                return null;
            }

            return property.GetValue(Model, null);
        }

        /// <summary>
        /// Subscribe to the model object.
        /// If the model raises property changed events, propagate them.
        /// This is important for the dynamically resolved properties, as they don't actually exist on the ViewModel.
        /// It should also be fine for UIElements that bind through directly onto the model because in that case the
        /// binding name won't match so only one PropertyChanged event will be observed.  For instance:
        /// When using {Binding Model.Address} instead of {Binding Address}, only the model's event will carry up to the binding;
        /// the ViewModel will just raise a superflous event that gets ignored.
        /// </summary>
        /// <param name="subscribe"></param>
        private void SubscribeToModel(bool subscribe)
        {
            if (_model is INotifyPropertyChanged)
            {
                var propertyNotifier = _model as INotifyPropertyChanged;

                if (subscribe)
                {
                    propertyNotifier.PropertyChanged += HandleModelPropertyChanged;
                }
                else
                {
                    propertyNotifier.PropertyChanged -= HandleModelPropertyChanged;
                }
            }
        }

        /// <summary>
        /// This handler propagates property change notifications that originate from the Model.
        /// First the immediate change is propagated, then notifications are propagated for any
        /// properties that are decorated as 'affected by'.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void HandleModelPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            PropertyInfo modelProperty = _modelType.GetProperty(args.PropertyName);

            // This method is only invoked if the Model is INotifyPropertyChanged.
            // Therefore, it's the correct time to add/update any cached value.
            if (EnablePropertyCaching/* && _proxyProperties.ContainsKey(args.PropertyName)*/)
            {
                _proxyProperties[args.PropertyName] = GetModelPropertyValue(modelProperty);
            }

            // Forward the event that came from the model.  If a binding targets the model, it
            // will just ignore this.  If a binding targets this object, this event will update it.
            // NOTE: This must usurp the original sender in the new event or else a binding to this as
            //       a DynamicObject will not get updated via the TryGetMember call.
            //       (.NET code must use the event sender as the dynamic object)
            RaisePropertyChanged(args.PropertyName);

            // Now propagate the change to any affected local properties.
            NotifyAffectedModelProperties(modelProperty);
        }

        /// <summary>
        /// Raise notifications for Model properties that are decorated as 'affected by'
        /// the given Model property change.
        /// </summary>
        /// <param name="property"></param>
        private void NotifyAffectedModelProperties(PropertyInfo modelProperty)
        {
            var affectedProps = modelProperty.GetCustomAttributes(typeof(AffectsPropertyAttribute), true);
            foreach (AffectsPropertyAttribute affectedProp in affectedProps)
            {
                RaisePropertyChanged(affectedProp.AffectedProperty);
            }
        }
    }
}