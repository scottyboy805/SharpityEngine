
namespace SharpityEditor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class EditorDrawerAttribute : Attribute
    {
        // Private
        private Type drawerType = null;

        // Properties
        public Type DrawerType
        {
            get { return drawerType; }
        }

        // Constructor
        public EditorDrawerAttribute(Type drawerType)
        {
            this.drawerType = drawerType;
        }
    }
}
