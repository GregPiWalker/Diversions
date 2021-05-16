using System;
using System.ComponentModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Rhino.Mocks;

namespace Diversions.Mvvm.Tests
{
    /// <summary>
    /// Tests the overridden DynamicObject behaviors of a <see cref="ViewModelBase"/>.
    /// BindableBase behaviors are tested by Prism libs.
    /// </summary>
    [TestClass]
    public class ViewModelBaseTests
    {
        /// <summary>
        /// Test ViewModel notifies when setter called directly on its model's property.
        /// </summary>
        [TestMethod]
        public void SetModelsProperty_Given_ModelImplementsPropertyChanged_Expect_ViewModelRaisesPropertyChanged()
        {
            var subject = TestHelper.CreateViewModelTestUnit();
            var model = subject.Model as ModelBaseTestUnit;
            var propValue = new object();
            PropertyChangedEventArgs notificationArgs = null;

            subject.PropertyChanged += (sender, args) => notificationArgs = args;
            model.ModelProperty = propValue;

            Assert.IsNotNull(notificationArgs, "The ViewModel should have raised its PropertyChanged event.");
            Assert.AreEqual(nameof(model.ModelProperty), notificationArgs.PropertyName);
        }

        /// <summary>
        /// Test getter of a dynamic proxy property without caching.
        /// </summary>
        [TestMethod]
        public void TryGetMember_Given_CachingDisabled_When_GetVirtualProxyProperty_Expect_ValueFromModel()
        {
            var subject = TestHelper.CreateViewModelTestUnit();
            dynamic dynamicSubject = subject;
            subject.EnablePropertyCaching = false;
            var model = subject.Model as ModelBaseTestUnit;
            model.ModelProperty = new object();

            var propertyValue = dynamicSubject.ModelProperty;

            Assert.IsNotNull(model.GetType().GetProperty(nameof(model.ModelProperty)), "The Model should have the desired property.");
            Assert.IsNull(subject.GetType().GetProperty(nameof(model.ModelProperty)), "The ViewModel should not have the desired property.");
            Assert.AreEqual(model.ModelProperty, propertyValue, "The returned object should be the same object held by the Model.");
        }

        /// <summary>
        /// Test caching capability using the getter of a dynamic proxy property.
        /// </summary>
        [TestMethod]
        public void TryGetMember_Given_CachingEnabled_When_GetVirtualProxyPropertyTwice_Expect_CachedValue()
        {
            Assert.Inconclusive("Update to use Moq.  Rhino does not support .NET Core.");
            var mockedTestUnit = MockRepository.GenerateMock<ModelBaseTestUnit>();
            var subject = TestHelper.CreateViewModelTestUnit(mockedTestUnit);
            subject.EnablePropertyCaching = true;
            dynamic dynamicSubject = subject;
            var model = subject.Model as ModelBaseTestUnit;
            var propValue = new object();

            model.Stub(m => m.ModelProperty).Return(propValue);

            // The first getter call definitely should invoke the property getter on the Model.
            var propertyValue = dynamicSubject.ModelProperty;
            model.AssertWasCalled(m => m.ModelProperty, x => x.Repeat.Once());

            // Now need to simulate that the Model's property was changed, resulting in a PropertyChanged notification.
            model.Raise(m => m.PropertyChangedTestable += null, model, new PropertyChangedEventArgs("ModelProperty"));

            // Raising the event causes the ViewModel internally to get the Model's property value and cache it,
            // so technically we get two calls at this point.
            model.AssertWasCalled(m => m.ModelProperty, x => x.Repeat.Twice());

            // Now this final 'get' should be the cached value, so call count does not increase.
            propertyValue = dynamicSubject.ModelProperty;
            model.AssertWasCalled(m => m.ModelProperty, x => x.Repeat.Twice());
        }

        /// <summary>
        /// Test setter of a dynamic proxy property.
        /// </summary>
        [TestMethod]
        public void TrySetMember_Given_CachingDisabled_When_SetVirtualProxyProperty_Expect_ModelSetterInvoked()
        {
            var subject = TestHelper.CreateViewModelTestUnit();
            subject.EnablePropertyCaching = false;
            dynamic dynamicSubject = subject;
            var model = subject.Model as ModelBaseTestUnit;
            int modelNotificationCount = 0;
            model.PropertyChanged += (sender, args) => modelNotificationCount++;

            Assert.IsNotNull(model.GetType().GetProperty(nameof(model.ModelProperty)), "The Model should have the desired property.");
            Assert.IsNull(subject.GetType().GetProperty(nameof(model.ModelProperty)), "The ViewModel should not have the desired property.");
            dynamicSubject.ModelProperty = new object();
            Assert.AreEqual(1, modelNotificationCount, "The Model's property setter should have been called once.");
            dynamicSubject.ModelProperty = new object();
            Assert.AreEqual(2, modelNotificationCount, "The Model's property setter should have been called twice.");
        }

        [TestMethod]
        public void TrySetMember_Given_CachingEnabled_When_SetVirtualProxyProperty_Expect_ValueIsCached()
        {
            Assert.Inconclusive("Update to use Moq.  Rhino does not support .NET Core.");
            var mockedTestUnit = MockRepository.GenerateMock<ModelBaseTestUnit>();
            var subject = TestHelper.CreateViewModelTestUnit(mockedTestUnit);
            subject.EnablePropertyCaching = true;
            dynamic dynamicSubject = subject;
            var model = subject.Model as ModelBaseTestUnit;
            var firstValue = new object();
            var updatedValue = new object();

            // Raising the propertyChanged event causes a getter invocation in the background when caching is enabled.  
            // Therefore, the first return is for the 'first' get in the event handler, subsequent returns are for 
            // the 'second' get with the updated value.
            model.Stub(m => m.ModelProperty).Return(firstValue).Repeat.Once();

            // This should set the first value in the model.
            dynamicSubject.ModelProperty = firstValue;

            // In order to set the cached value, need to simulate that the Model's property was changed.
            // The ViewModel's PropertyChanged handler should set the cached value.  This will result in the first
            // call to the property getter.
            model.Raise(m => m.PropertyChangedTestable += null, model, new PropertyChangedEventArgs("ModelProperty"));

            // Now add a new stub that returns the updated value from the Model from here on out.
            model.Stub(m => m.ModelProperty).Return(updatedValue);
            // Now get the property value. Should come from the cache.
            var finalValue = dynamicSubject.ModelProperty;

            Assert.AreEqual(firstValue, finalValue, "The first property value should have been cached and returned as the final value.");
        }

        [TestMethod]
        public void TrySetMember_Given_CachingDisabled_When_SetVirtualProxyProperty_Expect_ValueNotCached()
        {
            Assert.Inconclusive("Update to use Moq.  Rhino does not support .NET Core.");
            var mockedTestUnit = MockRepository.GenerateMock<ModelBaseTestUnit>();
            var subject = TestHelper.CreateViewModelTestUnit(mockedTestUnit);

            // Initially set caching enabled so that the first value gets cached.
            subject.EnablePropertyCaching = true;
            dynamic dynamicSubject = subject;
            var model = subject.Model as ModelBaseTestUnit;
            var firstValue = new object();
            var updatedValue = new object();

            // Tell the stub to return the first value only once.  Caching is off, so there won't be
            // a covert extra 'get' call in the background.
            model.Stub(m => m.ModelProperty).Return(firstValue).Repeat.Once();

            // This should set the first value in the model.
            dynamicSubject.ModelProperty = firstValue;

            // In order to set the cached value, need to simulate that the Model's property was changed.
            // The ViewModel's PropertyChanged handler should set the cached value.
            model.Raise(m => m.PropertyChangedTestable += null, model, new PropertyChangedEventArgs("ModelProperty"));

            // Now turn caching off so that we see the second value come from the Model, not the cache, even
            // though the cache has a value.
            subject.EnablePropertyCaching = false;

            // Now add a new stub that returns the updated value from the Model from here on out.
            model.Stub(m => m.ModelProperty).Return(updatedValue);

            // Now get the property value. Should come from the cache.
            var finalValue = dynamicSubject.ModelProperty;
            Assert.AreEqual(updatedValue, finalValue, "The property value should have been updated in the Model with the updated value returned as the final value.");

            // Now turning the cache back on should cause a getter to return the first cached value.
            subject.EnablePropertyCaching = true;
            finalValue = dynamicSubject.ModelProperty;
            Assert.AreEqual(firstValue, finalValue, "The first property value should have been cached with the cached value returned here.");
        }
    }
}
