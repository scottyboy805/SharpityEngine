using System;
using System.Reflection;
using System.Runtime.Serialization;

namespace SharpityEngine
{
    [DataContract]
    public class GameEventPersistentListener : GameEventListener, IContentCallback
    {
        // Private
        [DataMember(Name = "InvokeElement")]
        private Object invokeElement = null;
        [DataMember(Name = "MethodDeclaringType")]
        private string methodDeclaringType = "";
        [DataMember(Name = "InvokeElement")]
        private string methodName = "";

        // Properties
        public Object TargetElement
        {
            get { return invokeElement; }
        }

        public string MethodName
        {
            get { return methodName; }
        }

        // Constructor
        internal GameEventPersistentListener() { }

        internal GameEventPersistentListener(Object targetInstance, MethodInfo targetMethod)
            : base(targetInstance, targetMethod)
        {
            // Check for target method
            if (targetMethod == null)
                throw new ArgumentNullException(nameof(targetMethod));

            this.invokeElement = targetInstance;
        }

        // Methods
        public void ResolveTargetMethod()
        {
            if(invokeMethod == null)
            {
                // Try to get type
                Type targetType = Type.GetType(methodDeclaringType);

                if(targetType != null)
                {
                    // Try to get method
                    invokeMethod = targetType.GetMethod(methodName);
                }
            }
        }

        void IContentCallback.OnBeforeContentSave()
        {
            methodDeclaringType = invokeMethod.DeclaringType.FullName;
            methodName = invokeMethod.Name;
        }

        void IContentCallback.OnAfterContentLoad()
        {
            ResolveTargetMethod();
        }
    }
}
