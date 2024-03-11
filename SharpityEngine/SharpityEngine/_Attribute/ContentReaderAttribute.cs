using System;

namespace SharpityEngine.Content
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class ContentReaderAttribute : Attribute
    {
        // Private
        private string fileExtension = "";

        // Properties
        public string FileExtension
        {
            get { return fileExtension; }
        }

        // Constructor
        public ContentReaderAttribute(string fileExtension)
        {
            // Check for error
            if (string.IsNullOrEmpty(fileExtension) == true) 
                throw new ArgumentException("File extension cannot be null or empty");

            this.fileExtension = fileExtension;
        }
    }
}
