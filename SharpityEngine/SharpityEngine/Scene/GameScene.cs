using System.Runtime.Serialization;

namespace SharpityEngine.Scene
{
    public sealed class GameScene : GameAsset
    {
        // Private
        [DataMember(Name = "Enabled")]
        private bool enabled = true;
        [DataMember(Name = "Priority")]
        private int priority = 0;
        [DataMember(Name = "GameObjects")]
        private List<GameObject> gameObjects = new List<GameObject>();

        // Properties
        public bool Enabled
        {
            get { return enabled; }
        }

        public int Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        public IReadOnlyList<GameObject> GameObjects
        {
            get { return gameObjects; }
        }
    }
}
