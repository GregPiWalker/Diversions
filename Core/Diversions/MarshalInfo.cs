using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Diversions
{
    internal class MarshalInfo
    {
        public const string LambdaOperator = "()=>";

        /// <summary>
        /// Creates a <see cref="MarshalInfo"/> that loads using a given class instance.
        /// </summary>
        /// <param name="instance"></param>
        /// <param name="targetMethod"></param>
        /// <param name="methodInputs"></param>
        /// <param name="syncContext"></param>
        internal MarshalInfo(object instance, string targetMethod, KeyValuePair<Type, object>[] methodInputs, SynchronizationContext syncContext = null)
        {
            Marshaller = instance;
            MethodInputs = methodInputs;
            SynchronizationContext = syncContext;
            Type[] inputTypes = methodInputs.Select(kp => kp.Key).ToArray();

            MarshalMethod = Marshaller.GetType().GetMethod(targetMethod, inputTypes);
            if (MarshalMethod == null && Marshaller is Type)
            {
                // Try again looking for static methods this time.
                MarshalMethod = (Marshaller as Type).GetMethod(targetMethod, BindingFlags.Public | BindingFlags.Static, null, inputTypes, null);
            }
        }

        /// <summary>
        /// Gets the SynchronizationContext upon which the Diverter's event handler will be marshalled.
        /// </summary>
        public SynchronizationContext SynchronizationContext { get; private set; }

        public KeyValuePair<Type, object>[] MethodInputs { get; private set; }

        public object Marshaller { get; private set; }

        public MethodInfo MarshalMethod { get; private set; }
    }
}
