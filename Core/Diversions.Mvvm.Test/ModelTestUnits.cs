using System;
using System.ComponentModel;

namespace Diversions.Mvvm.Tests
{
    public class ModelBaseTestUnit : ModelBase
    {
        private object _modelProperty;

        public ModelBaseTestUnit()
        {
            // forward the virtual event to the real event
            PropertyChangedTestable += (sender, args) => OnPropertyChangedTestable(args);
        }

        /// <summary>
        /// Raise this event rather than <see cref="PropertyChanged"/> for testing with dynamic mocks.
        /// </summary>
        public virtual event PropertyChangedEventHandler PropertyChangedTestable;

        public virtual object ModelProperty { get => _modelProperty; set => SetProperty(ref _modelProperty, value); }

        public virtual object Method()
        {
            return ModelProperty;
        }

        /// <summary>
        /// Provides a concrete implementation for a dynamic mock object to invoke.
        /// </summary>
        /// <param name="args"></param>
        protected void OnPropertyChangedTestable(PropertyChangedEventArgs args)
        {
            base.OnPropertyChanged(args);
        }
    }

    public class ModelTestUnit
    {
        private object _modelProperty;

        public event EventHandler ModelPropertyGetterCalled;
        public event EventHandler ModelPropertySetterCalled;

        public virtual object ModelProperty 
        {
            get
            {
                ModelPropertyGetterCalled?.Invoke(this, null);
                return _modelProperty;
            }
            set
            {
                ModelPropertySetterCalled?.Invoke(this, null);
                _modelProperty = value;
            }
        }

        public virtual object Method()
        {
            return ModelProperty;
        }
    }
}
