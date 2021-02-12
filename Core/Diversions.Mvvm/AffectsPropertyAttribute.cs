using System;

namespace Diversions.Mvvm
{
    /// <summary>
    /// This attribute is used to indicate that a change to the decorated property also
    /// causes another property's value to change. 
    /// THIS SHOULD ONLY BE USED IN MODELS, NOT VIEW-MODELS.
    /// See: https://docs.microsoft.com/en-us/archive/msdn-magazine/2010/july/design-patterns-problems-and-solutions-with-model-view-viewmodel
    /// </summary>
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = true)]
    public sealed class AffectsPropertyAttribute : Attribute
    {
        public AffectsPropertyAttribute(string affectedPropertyName)
        {
            AffectedProperty = affectedPropertyName;
        }

        public string AffectedProperty { get; private set; }
    }
}
