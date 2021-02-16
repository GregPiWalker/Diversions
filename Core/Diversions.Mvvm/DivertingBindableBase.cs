///<remarks>
/// This is an auto-generated file.  Content is generated from DivertingBindableBase.txt and DivertingBindableBase.tt.
/// Use of a T4 template allows two different class implementations that both share a single implementation of
/// common <see cref="Prism.Mvvm.BindableBase"/> code that has been modified to offer the Diversion feature.
/// Inheritance was not an option, as <see cref="ViewModelBase"/> must inherit from the abstract <see cref="DynamicObject"/>.
///</remarks>
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.CodeDom.Compiler;

namespace Diversions.Mvvm
{
    /// <summary>
    /// An abstract base class to be used for Models or View Models.
    ///
    /// An implementation of <see cref="INotifyPropertyChanged"/> to simplify models and view models.
    /// This is a derivation of BindableBase that uses <see cref="DiversionDelegate{TArg}"/>s.
    /// Locally, the <see cref="INotifyPropertyChanged.PropertyChanged"/> event is raised synchronously on
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
    public abstract class DivertingBindableBase : INotifyPropertyChanged
    {
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
    }
}