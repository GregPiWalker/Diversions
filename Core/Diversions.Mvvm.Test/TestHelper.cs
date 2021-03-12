using Rhino.Mocks;
using System;

namespace Diversions.Mvvm.Tests
{
    public class TestHelper
    {
        public static ModelBaseTestUnit CreateModelBaseTestUnit()
        {
            var mockModel = new ModelBaseTestUnit();

            return mockModel;
        }

        public static ModelTestUnit CreateInsensitiveModelTestUnit()
        {
            var mockModel = new ModelTestUnit();

            return mockModel;
        }

        /// <summary>
        /// Create a ViewModelTestUnit that owns a mocked Model.
        /// </summary>
        /// <returns></returns>
        public static ViewModelBaseTestUnit CreateViewModelTestUnit(ModelBaseTestUnit mockModel = null)
        {
            return new ViewModelBaseTestUnit(mockModel ?? CreateModelBaseTestUnit());
        }

        /// <summary>
        /// Create a ViewModelTestUnit that owns a mocked Model.
        /// </summary>
        /// <returns></returns>
        public static ViewModelInsensitiveTestUnit CreateInsensitiveViewModelTestUnit(ModelTestUnit mockModel = null)
        {
            return new ViewModelInsensitiveTestUnit(mockModel ?? CreateInsensitiveModelTestUnit());
        }
    }
}
