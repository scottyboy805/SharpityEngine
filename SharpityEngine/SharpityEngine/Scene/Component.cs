using System.Runtime.Serialization;

namespace SharpityEngine
{
    public abstract class Component : GameElement
    {
        // Private
        [DataMember(Name = "Enabled")]
        private bool enabled = true;

        private GameObject gameObject = null;

        // Properties
        public bool Enabled
        {
            get { return enabled; }
            set 
            { 
                enabled = value; 

                // Trigger event
                if(this is IGameEnable)
                {
                    // 
                }
            }
        }

        public GameObject GameObject
        {
            get { return gameObject; }
        }

        // Methods
        public bool CompareTag(string tag)
        {
            return string.Compare(gameObject.Tag, tag, StringComparison.OrdinalIgnoreCase) == 0;
        }
    }
}
