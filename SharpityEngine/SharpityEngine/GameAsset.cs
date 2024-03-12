
namespace SharpityEngine
{
    public abstract class GameAsset : GameElement
    {
        // Private
        private bool loadedCallback = false;

        // Constructor
        protected GameAsset()
        {
        }

        // Methods
        protected virtual void OnLoaded() { }

        protected override void OnDestroy()
        {
            // Unload the asset rather than destroying
        }

        internal void OnAfterLoaded()
        {
            if(loadedCallback == false)
            {
                loadedCallback = true;
                OnLoaded();
            }
        }
    }
}
