using System;

namespace SharpityEngine.Content
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ContentWriterAttribute : Attribute
    {
        // Private
        private Type contentType = null;
        private bool optimizedWriter = false;

        // Properties
        public Type ContentType
        {
            get { return contentType; }
        }

        public bool OptimizedWriter
        {
            get { return optimizedWriter; }
        }

        // Constructor
        public ContentWriterAttribute(Type contentType, bool optimizedWriter)
        {
            // Check for error
            if (contentType == null) 
                throw new ArgumentNullException(nameof(contentType));

            this.contentType = contentType;
            this.optimizedWriter = optimizedWriter;
        }
    }
}
