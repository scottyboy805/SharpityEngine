
namespace SharpityEngine
{
    public abstract class GameAsset : GameElement
    {
        // Methods
        protected override void OnDestroy()
        {
            // Unload the asset rather than destroying
        }
    }
}
