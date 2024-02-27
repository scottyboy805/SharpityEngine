
namespace SharpityEngine
{
    public interface IContentCallback
    {
        // Methods
        void OnBeforeContentSave();

        void OnAfterContentLoad();
    }
}
