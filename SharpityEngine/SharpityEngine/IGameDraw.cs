
using SharpityEngine.Graphics;

namespace SharpityEngine
{
    public interface IGameDraw
    {
        // Properties
        int DrawOrder { get; }

        // Methods
        void OnBeforeDraw();

        void OnDraw(BatchRenderer batchRenderer);

        void OnAfterDraw();
    }
}
