using System.Reflection;
using System.Runtime.Serialization;

namespace SharpityEngine
{
    [DataContract]
    public class GameEventListener
    {
        // Protected
        protected object invokeInstance = null;
        protected MethodInfo invokeMethod = null;

        // Properties      
        public object TargetInstance
        {
            get { return invokeInstance; }
        }

        public MethodBase Method
        {
            get { return invokeMethod; }
        }

        // Constructor
        internal GameEventListener() { }

        internal GameEventListener(object targetInstance, MethodInfo targetMethod)
        {
            this.invokeInstance = targetInstance;
            this.invokeMethod = targetMethod;
        }

        // Methods
        public void DynamicInvoke()
        {
            if(invokeMethod != null)
            {
                if(invokeMethod.IsStatic == true)
                {
                    invokeMethod.Invoke(null, null);
                }
                else if(invokeInstance != null)
                {
                    invokeMethod.Invoke(invokeInstance, null);
                }
            }
        }

        public void DynamicInvoke(object[] args)
        {
            if (invokeMethod != null)
            {
                if (invokeMethod.IsStatic == true)
                {
                    invokeMethod.Invoke(null, args);
                }
                else if (invokeInstance != null)
                {
                    invokeMethod.Invoke(invokeInstance, args);
                }
            }
        }
    }
}
