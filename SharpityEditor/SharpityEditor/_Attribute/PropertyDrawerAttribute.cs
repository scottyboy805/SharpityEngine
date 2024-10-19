
namespace SharpityEditor
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public sealed class PropertyDrawerAttribute : Attribute
    {
        // Private
        private Type drawerType = null;

        // Properties
        public Type DrawerType
        {
            get { return drawerType; }
        }

        // Constructor
        public PropertyDrawerAttribute(Type drawerType)
        {
            this.drawerType = drawerType;
        }
    }
}
