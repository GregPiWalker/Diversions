using System.ComponentModel;
using Prism.Mvvm.Customized;

namespace Diversions.Mvvm
{
    /// <summary>
    /// Implementation of <see cref="INotifyPropertyChanged"/> to simplify models.
    /// This is a derivation of BindableBase that uses <see cref="DiversionDelegate{TArg}"/>s.
    /// Locally, the <see cref="INotifyPropertyChanged.PropertyChanged"/> event is raised synchronously on
    /// the caller's thread, but each event observer may divert the flow to a thread of
    /// their choosing.  
    /// 
    /// If the <see cref="DiversionAttribute"/> classes static 
    /// <see cref="DiversionAttribute.DefaultDiverter"/> property is set for a UI Dispatcher,
    /// then event handlers on the <see cref="INotifyPropertyChanged.PropertyChanged"/> event will be automatically
    /// marshalled onto the dispatcher.  While .NET marshalls data bindings on <see cref="INotifyPropertyChanged"/>
    /// to the UI thread internally, Diversions extend that automatic marshalling to 
    /// UserControls and CustomControls that observe ViewModel/Model events.
    /// </summary>
    public abstract class DivertingBindableBase : BindableBase
    {
        private readonly DiversionDelegate<PropertyChangedEventArgs> _propertyChangedDelegate = new DiversionDelegate<PropertyChangedEventArgs>();

        /// <summary>
        /// Notifies observers that a property value has changed. This event is backed
        /// by a <see cref="DiversionDelegate{TArg}"/>, which provides the plumbing for
        /// marshalling event handler invocations onto the appropriate threads.
        /// </summary>
        public override event PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChangedDelegate.Add(value); }
            remove { _propertyChangedDelegate.Remove(value); }
        }

        /// <summary>
        /// Raises this object's PropertyChanged event using a <see cref="DiversionDelegate{TArg}"/>.
        /// </summary>
        /// <param name="args">The PropertyChangedEventArgs</param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            _propertyChangedDelegate.Invoke(this, args);
        }
    }
}