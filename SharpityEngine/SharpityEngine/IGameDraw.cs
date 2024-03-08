
namespace SharpityEngine
{
    public interface IGameDraw
    {
        // Properties
        int DrawOrder { get; }

        bool Visible { get; }

        // Methods
        void OnBeforeDraw();

        void OnDraw();

        void OnAfterDraw();
    }
}
