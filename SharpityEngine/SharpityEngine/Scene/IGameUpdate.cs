
namespace SharpityEngine
{
    public interface IGameUpdate
    {
        // Properties
        int Priority { get; }

        bool Enabled { get; }

        // Methods
        void OnStart();

        void OnUpdate(GameTime gameTime);
    }
}
