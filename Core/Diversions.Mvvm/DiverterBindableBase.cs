using System.ComponentModel;
using Prism.Mvvm.Customized;

namespace Diversions.Mvvm
{
    /// <summary>
    /// Implementation of <see cref="INotifyPropertyChanged"/> to simplify models.
    /// This is a modified implementation that uses <see cref="DiversionDelegate{TArg}"/>s.
    /// Locally, the <see cref="PropertyChanged"/> event is be raised on synchronously on
    /// the caller's thread, but each event observer may divert the flow to a thread of
    /// their choosing.  If the <see cref="DiversionAttribute"/> classes static 
    /// <see cref="DiversionAttribute.DefaultDiverter"/> property is set for a UI Dispatcher,
    /// then data bindings on the <see cref="PropertyChanged"/> event will be automatically
    /// marshalled onto the Dispatcher.
    /// </summary>
    public abstract class DiverterBindableBase : BindableBase
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