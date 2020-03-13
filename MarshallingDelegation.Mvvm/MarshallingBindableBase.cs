using System.ComponentModel;
using Prism.Mvvm.Customized;

namespace MarshallingDelegation.Mvvm
{
    /// <summary>
    /// Implementation of <see cref="INotifyPropertyChanged"/> to simplify models.
    /// This is a modified implementation that uses <see cref="MarshallingDelegate{TArg}"/>s.
    /// </summary>
    public abstract class MarshallingBindableBase : BindableBase
    {
        private readonly MarshallingDelegate<PropertyChangedEventArgs> _propertyChangedDelegate = new MarshallingDelegate<PropertyChangedEventArgs>();

        /// <summary>
        /// Occurs when a property value changes.
        /// </summary>
        public override event PropertyChangedEventHandler PropertyChanged
        {
            add { _propertyChangedDelegate.Add(value); }
            remove { _propertyChangedDelegate.Remove(value); }
        }

        /// <summary>
        /// Raises this object's PropertyChanged event.
        /// </summary>
        /// <param name="args">The PropertyChangedEventArgs</param>
        protected override void OnPropertyChanged(PropertyChangedEventArgs args)
        {
            _propertyChangedDelegate?.Invoke(this, args);
        }
    }
}