using SharpityEngine;

namespace SharpityEditor.UI
{
    public delegate void UIBindCommand();
    public delegate void UIBindCommand<T>(T value);

    public sealed class UIBind<T>
    {
        // Events
        public readonly GameEvent<T> OnChanged = new GameEvent<T>();
        internal readonly GameEvent OnUserChanged = new GameEvent();

        // Private
        private T data = default;

        // Properties
        public T Value
        {
            get { return data; }
            set
            {
                data = value;
                OnUserChanged.Raise();
            }
        }

        // Constructor
        public UIBind() { }

        public UIBind(T value) 
        {
            this.data = value;
        }

        // Methods
        internal void UIChangedValue(T value)
        {
            data = value;
            OnChanged.Raise(value);
        }

        public static implicit operator UIBind<T>(T value)
        {
            return new UIBind<T>(value);
        }
    }

    public class UIBindCollection<T>
    {
        // Events
        public readonly GameEvent<int> OnChanged = new GameEvent<int>();
        internal readonly GameEvent OnUserChanged = new GameEvent();

        // Protected
        protected int selected = 0;
        protected List<T> data = null;

        // Properties
        public int Selected
        {
            get { return selected; }
            set
            {
                if(value >= 0 && value < data.Count)
                {
                    selected = value;
                    OnUserChanged.Raise();
                }
            }
        }

        public T SelectedValue
        {
            get { return data.Count > 0 ? data[selected] : default; }
            set
            {
                // Try to get index
                int index = data.IndexOf(value);

                // Check for valid
                if (index != -1)
                    Selected = index;
            }
        }

        public IList<T> Values
        {
            get { return data; }
        }

        public int ValueCount
        {
            get { return data != null ? data.Count : 0; }
        }

        // Constructor
        public UIBindCollection() { }

        public UIBindCollection(IEnumerable<T> values) 
        {
            this.data = new List<T>(values);
        }

        // Methods
        internal void UIChangedValue(int selection)
        {
            selected = selection;
            OnChanged.Raise(selection);
        }

        public static implicit operator UIBindCollection<T>(T[] values)
        {
            return new UIBindCollection<T>(values);
        }

        public static implicit operator UIBindCollection<T>(List<T> values)
        {
            return new UIBindCollection<T>(values);
        }
    }

    public sealed class UIBindCollectionEnum<T> : UIBindCollection<T> where T : struct, Enum
    {
        // Constructor
        public UIBindCollectionEnum(T value)
        {
            this.data = new List<T>(Enum.GetValues<T>());
            this.selected = data.IndexOf(value);
        }

        // Methods
        public static implicit operator UIBindCollectionEnum<T>(T value)
        {
            return new UIBindCollectionEnum<T>(value);
        }
    }
}

