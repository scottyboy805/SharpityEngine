using System.Runtime.Serialization;

namespace SharpityEngine
{
    [DataContract]
    public sealed class GameSettings : DataScript
    {
        // Private
        // Game info
        [DataMember(Name = "CompanyName")]
        private string companyName = "Default Company";
        [DataMember(Name = "GameName")]
        private string gameName = "Default Game";
        [DataMember(Name = "GameVersion")]
        private Version gameVersion = new Version(1, 0, 0);

        // Presentation info
        [DataMember(Name = "PreferredScreenWidth")]
        private int preferredScreenWidth = 1280;
        [DataMember(Name = "PreferredScreenHeight")]
        private int preferredScreenHeight = 720;
        [DataMember(Name = "PreferredFullscreen")]
        private bool preferredFullscreen = false;
        [DataMember(Name = "ResizableWindow")]
        private bool resizableWindow = false;

        // Splash info
        [DataMember(Name = "ShowSplashScreen")]
        private bool showSplashScreen = true;
        [DataMember(Name = "SplashImage")]
        private string splashImage = "Splash";
        [DataMember(Name = "SplashMinDisplayTime")]
        private int splashMinDisplayMilliseconds = 3000;

        // Startup info
        [DataMember(Name = "GameAssemblies")]
        private List<string> gameAssemblies = new List<string>();
        [DataMember(Name = "PreloadBundles")]
        private List<string> preloadBundles = new List<string>();
        [DataMember(Name = "PreloadContent")]
        private List<string> preloadContent = new List<string>();
        [DataMember(Name = "StartupScenes")]
        private List<string> startupScenes = new List<string>();

        // Properties
        public string CompanyName
        {
            get { return companyName; }
        }

        public string GameName
        {
            get { return gameName; }
        }

        public Version GameVersion
        {
            get { return gameVersion; }
        }

        public int PreferredScreenWidth
        {
            get { return preferredScreenWidth; }
        }

        public int PreferredScreenHeight
        {
            get { return preferredScreenHeight; }
        }

        public bool PreferredFullscreen
        {
            get { return preferredFullscreen; }
        }

        public bool ResizableWindow
        {
            get { return resizableWindow; }
        }

        public bool ShowSplashScreen
        {
            get { return showSplashScreen; }
        }

        public string SplashImage
        {
            get { return splashImage; }
        }

        public int SplashMinDisplayMilliseconds
        {
            get { return splashMinDisplayMilliseconds; }
        }

        public IReadOnlyList<string> GameAssemblies
        {
            get { return gameAssemblies; }
        }

        public IReadOnlyList<string> PreloadBundles
        {
            get { return preloadBundles; }
        }

        public IReadOnlyList<string> PreloadContent
        {
            get { return preloadContent; }
        }

        public IReadOnlyList<string> StartupScenes
        {
            get { return startupScenes; }
        }
    }
}
