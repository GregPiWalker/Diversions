using log4net;
using System.ComponentModel;

namespace Diversions.Mvvm.Tests
{
    public class ViewModelBaseTestUnit : ViewModelBase
    {
        public static ILog TestLogger;

        public ViewModelBaseTestUnit(ModelBaseTestUnit model) : base(model, TestLogger)
        {
            // forward the virtual event to the real event
            PropertyChangedTestable += (sender, args) => OnPropertyChangedTestable(args);
        }

        /// <summary>
        /// Raise this event rather than <see cref="PropertyChanged"/> for testing with dynamic mocks.
        /// </summary>
        public virtual event PropertyChangedEventHandler PropertyChangedTestable;

        /// <summary>
        /// A proxy for a property that really exists on the model.
        /// </summary>
        public object RealProxyProperty
        {
            get => (Model as ModelBaseTestUnit).ModelProperty;
            set => (Model as ModelBaseTestUnit).ModelProperty = value;
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

    public class ViewModelInsensitiveTestUnit : ViewModelBase
    {
        public static ILog TestLogger;

        public ViewModelInsensitiveTestUnit(ModelTestUnit model) : base(model, TestLogger)
        {
            // forward the virtual event to the real event
            PropertyChangedTestable += (sender, args) => OnPropertyChangedTestable(args);
        }

        /// <summary>
        /// Raise this event rather than <see cref="PropertyChanged"/> for testing with dynamic mocks.
        /// </summary>
        public virtual event PropertyChangedEventHandler PropertyChangedTestable;

        /// <summary>
        /// A proxy for a property that really exists on the model.
        /// </summary>
        public object RealProxyProperty
        { 
            get => (Model as ModelTestUnit).ModelProperty; 
            set => (Model as ModelTestUnit).ModelProperty = value;
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
}
