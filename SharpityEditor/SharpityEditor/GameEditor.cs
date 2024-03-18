using SharpityEditor.Content;

namespace SharpityEditor
{
    public sealed class GameEditor
    {
        // Private
        private string projectPath = null;
        private string projectFolder = null;
        private ContentDatabase contentDatabase = null;

        // Properties
        public string ProjectPath
        {
            get { return projectPath; }
        }

        public string ProjectFolder
        {
            get { return projectFolder; }
        }

        public bool IsProjectOpen
        {
            get { return projectPath != null; }
        }

        public ContentDatabase ContentDatabase
        {
            get { return contentDatabase; }
        }

        // Methods
        public void OpenProject(string projectPath)
        {
            // Check for invalid
            if (string.IsNullOrEmpty(projectPath) == true)
                throw new ArgumentException("Project path cannot be null or empty");

            // Check for found
            if (File.Exists(projectPath) == false)
                throw new ArgumentException("Project file path does not exist");

            // Initialize project
            this.projectPath = projectPath;
            this.projectFolder = Directory.GetParent(projectPath).FullName;
            this.contentDatabase = new ContentDatabase(projectFolder);

            // Start loading assets
            contentDatabase.ScanContent();
        }
    }
}