
namespace SharpityEngine
{
    public interface IGameUpdate
    {
        // Properties
        int Priority { get; }

        // Methods
        void OnStart();

        void OnUpdate(GameTime gameTime);
    }
}
