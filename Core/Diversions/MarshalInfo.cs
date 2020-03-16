using System;
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
        /// <param name="paramTypes"></param>
        /// <param name="staticArguments"></param>
        internal MarshalInfo(object instance, string targetMethod, Type[] paramTypes, SynchronizationContext syncContext = null, object[] staticArguments = null)
        {
            Marshaller = instance;
            StaticArguments = staticArguments;
            MethodParameters = paramTypes;
            SynchronizationContext = syncContext;

            if (staticArguments != null && staticArguments.Length > 0)
            {
                paramTypes = paramTypes.Concat(staticArguments.Select(o => o.GetType())).ToArray();
            }

            MarshalMethod = Marshaller.GetType().GetMethod(targetMethod, paramTypes);
        }

        public SynchronizationContext SynchronizationContext { get; private set; }

        public object[] StaticArguments { get; private set; }

        public Type[] MethodParameters { get; private set; }

        public object Marshaller { get; private set; }

        public MethodInfo MarshalMethod { get; private set; }
    }
}
