using SharpityEngine.Content;
using System.Reflection;
using System.Runtime.Serialization;

namespace SharpityEngine
{
    public abstract class GameElement
    {
        // Events
        public readonly GameEvent OnWillDestroy = new GameEvent();

        // Private
        private Game game = null;
        private ContentBundle contentBundle = null;
        private bool isDestroying = false;
        private bool isDestroyed = false;

        private string name = "";
        private string type = "";
        private string guid = "";
        private string contentPath = "";

        // Protected
        protected readonly object syncLock = new object();

        // Internal
        internal static ConstructorInfo initializer = typeof(GameElement).GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance, null, System.Type.EmptyTypes, null);

        internal bool isReadOnly = false;
        internal Type elementType = null;
        internal float scheduledDestroyTime = 0f;

        // Properties
        public bool IsReadOnly
        {
            get { return isReadOnly; }
        }

        public bool IsDestroying
        {
            get { return isDestroying; }
        }

        public bool IsDestroyed
        {
            get { return isDestroyed; }
        }

        [DataMember]
        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        [DataMember]
        public string Type
        {
            get { return type; }
            internal set { type = value; }
        }

        [DataMember]
        public string Guid
        {
            get
            {
                // Check for invalid - create on demand
                if (string.IsNullOrEmpty(guid) == true)
                    guid = System.Guid.NewGuid().ToString();

                return guid;
            }
            internal set { guid = value; }
        }

        public ContentBundle ContentBundle
        {
            get { return contentBundle; }
            internal set { contentBundle = value; }
        }

        public string ContentPath
        {
            get { return contentPath; }
            internal set { contentPath = value; }
        }

        public bool HasContentPath
        {
            get { return string.IsNullOrEmpty(contentPath) == false; }
        }

        public TypeManager TypeManager
        {
            get
            {
                CheckDestroyed();
                return game.TypeManager; 
            }
        }

        public Game Game
        {
            get
            {
                CheckDestroyed();
                return game;
            }
        }

        public GameWindow Window
        {
            get
            {
                CheckDestroyed();
                return game.Window;
            }
        }

        //public ContentProvider Content
        //{
        //    get
        //    {
        //        CheckDestroyed();
        //        return gameContext.Content;
        //    }
        //}

        // Constructor
        protected GameElement()
        {
            game = Game.Current;

            // Game settings is a special type that gets initialized very early
            if ((this is GameSettings) == false && game != null)
            {
                // Get element type
                elementType = TypeManager.GetUsableType(GetType());

                // Get type name
                type = TypeManager.GetTypeName(elementType, false);

                // Initialize instance
                TypeManager.InitializeElementTypeInstance(this);
            }
            else
            {
                elementType = GetType();
            }

            // Get name
            name = elementType.Name;
        }

        // Methods
        public override string ToString()
        {
            string info = (string.IsNullOrEmpty(contentPath) == false) ? contentPath : guid;
            return string.Format("{0}({1})", GetType().FullName, info);
        }

        //protected virtual GameElement OnInstantiate()
        //{
        //    return DoDefaultInstantiate();
        //}

        //protected GameElement DoDefaultInstantiate()
        //{
        //    GameElement result = null;
        //    GameElement instantiateRoot = null;

        //    // Check for script
        //    if (this is GameScript)
        //    {
        //        result = DoInferredInstantiate(out instantiateRoot);
        //    }
        //    else
        //    {
        //        result = DoExplicitInstantiate();
        //        instantiateRoot = result;
        //    }

        //    // Run content callbacks
        //    if (instantiateRoot != null)
        //    {
        //        if (instantiateRoot is IContentCallback)
        //            ((IContentCallback)instantiateRoot).OnAfterContentLoad();
        //    }

        //    return result;
        //}

        //protected internal GameElement DoInferredInstantiate(out GameElement instantiateRoot)
        //{
        //    // Get game script
        //    GameScript script = this as GameScript;

        //    // Get root element
        //    GameElement clone = (this as GameScript).Element;

        //    // Create new instance
        //    GameElement result = TypeManager.CreateTypeInstanceAs<GameElement>(clone.elementType);

        //    // Set result
        //    instantiateRoot = result;

        //    // Copy data
        //    TypeManager.CopyTypeInstance(script.Element, script.Element.elementType, ref result);

        //    // Get component
        //    if (result != null)
        //        result = (result as GameSceneElement).GetScript(script.elementType);

        //    return result;
        //}

        //protected internal GameElement DoExplicitInstantiate()
        //{
        //    // Create new instance
        //    GameElement result = TypeManager.CreateTypeInstanceAs<GameElement>(elementType);

        //    // Copy data
        //    TypeManager.CopyTypeInstance(this, elementType, ref result);

        //    return result;
        //}

        protected virtual void OnDestroy() { }

        ///// <summary>
        ///// Destroy this element immediately.
        ///// The element will not be usable imediately after this call and should be treated as null.
        ///// Destroying <see cref="GameAsset"/> elements will cause the content to be unloaded from memory and the asset will need to be reloaded in order to be usable again.
        ///// Use <see cref="IsDestroyed"/> to determine whether a game element can be accessed.
        ///// </summary>
        //public void DestroyImmediate()
        //{
        //    if (isDestroyed == false)
        //    {
        //        // Set destroying flag
        //        isDestroying = true;

        //        try
        //        {
        //            // Trigger event
        //            OnWillDestroy.Raise();

        //            // Call destroy event
        //            OnDestroy();
        //        }
        //        catch (Exception e)
        //        {
        //            Log.Exception(this, e);
        //        }

        //        // Set destroyed flag
        //        isDestroyed = true;
        //        isDestroying = false;
        //    }
        //}

        ///// <summary>
        ///// Destroy this element at the end of the frame.
        ///// The element will still be usable by other elements and scripts until the current frame has completed.
        ///// Destroying <see cref="GameAsset"/> elements will cause the content to be unloaded from memory and the asset will need to be reloaded in order to be usable again.
        ///// Use <see cref="IsDestroyed"/> to determine whether a game element can be accessed.
        ///// </summary>
        //public void Destroy()
        //{
        //    // Check for destroyed
        //    if (isDestroyed == false)
        //    {
        //        // Destroy at end of frame
        //        Game.ScheduleDestruction(this);
        //    }
        //}

        ///// <summary>
        ///// Destroy this element after the specified amout of time has elapsed.
        ///// Use <see cref="IsDestroyed"/> to determine whether a game element can be accessed.
        ///// Destroying <see cref="GameAsset"/> elements will cause the content to be unloaded from memory and the asset will need to be reloaded in order to be usable again.
        ///// </summary>
        ///// <param name="delayTime">The amount of time in seconds before the element should be destroyed</param>
        //public void Destroy(float delayTime)
        //{
        //    // Check for destroyed
        //    if (isDestroyed == false)
        //    {
        //        // Destroy after time
        //        Game.ScheduleDestruction(this, delayTime);
        //    }
        //}

        //public GameElement Instantiate()
        //{
        //    // Check destroyed
        //    CheckDestroyed();

        //    // Clone this object
        //    return OnInstantiate();
        //}

        //public T Instantiate<T>() where T : class
        //{
        //    // Check destroyed
        //    CheckDestroyed();

        //    // Clone this object
        //    GameElement element = OnInstantiate();

        //    return element as T;
        //}

        //public GameElement Instantiate(GameScene targetScene)
        //{
        //    // Create instance
        //    GameElement result = Instantiate();

        //    // Add to scene
        //    if (result != null && targetScene != null)
        //    {
        //        if (result is GameSceneElement)
        //        {
        //            targetScene.AddElement(result as GameSceneElement);
        //        }
        //        else if (result is GameScript)
        //        {
        //            targetScene.AddElement((result as GameScript).Element);
        //        }
        //    }
        //    return result;
        //}

        //public T Instantiate<T>(GameScene targetScene) where T : class
        //{
        //    // Create instance
        //    T result = Instantiate<T>();

        //    // Add to scene
        //    if (result != null && targetScene != null)
        //    {
        //        if (result is GameSceneElement)
        //        {
        //            targetScene.AddElement(result as GameSceneElement);
        //        }
        //        else if (result is GameScript)
        //        {
        //            targetScene.AddElement((result as GameScript).Element);
        //        }
        //    }
        //    return result;
        //}

        protected internal void CheckDestroyed()
        {
            //if (isDestroyed == true)
            //    throw new GameElementDestroyedException(this);
        }

        protected internal void CheckReadOnly()
        {
            //if (isReadOnly == true)
            //    throw new GameElementReadOnlyException(this);
        }
    }
}
