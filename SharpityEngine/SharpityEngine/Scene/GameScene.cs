﻿using SharpityEngine.Graphics;
using System.Runtime.Serialization;

namespace SharpityEngine.Scene
{
    public sealed class GameScene : GameAsset, IGameModule, IContentCallback
    {
        // Internal
        internal HashSet<IGameDraw> sceneDrawCalls = new HashSet<IGameDraw>();
        internal HashSet<IGameUpdate> sceneUpdateCalls = new HashSet<IGameUpdate>();
        internal bool startCalled = false;

        // Private
        private Queue<IGameUpdate> sceneNewObjectsThisFrame = new Queue<IGameUpdate>();

        [DataMember(Name = "Enabled")]
        private bool enabled = true;
        [DataMember(Name = "Priority")]
        private int priority = 0;
        [DataMember(Name = "GameObjects")]
        private List<GameObject> gameObjects = new List<GameObject>();

        // Properties
        public bool Enabled
        {
            get { return enabled; }
        }

        public int Priority
        {
            get { return priority; }
            set { priority = value; }
        }

        public int DrawOrder
        {
            get { return priority; }
        }

        public IReadOnlyList<GameObject> GameObjects
        {
            get { return gameObjects; }
        }

        // Methods
        public void Activate()
        {
            // Check for active
            if (Game.GameModules.HasModule(this) == true)
                throw new InvalidOperationException("Scene is already activated");

            // Send initial enabled event
            foreach (GameObject go in gameObjects)
                GameObject.DoGameObjectEnabledEvents(go, go.Enabled, true);

            // Add the module
            Game.GameModules.AddModule(this);
        }

        public void OnFrameStart() { }
        public void OnFrameEnd() { }

        public void OnBeforeDraw() { }
        public void OnAfterDraw() { }      
        public void OnDraw(BatchRenderer batchRenderer) { }

        public void OnStart()
        {
            // Set flag
            startCalled = true;

            // Start all objects
            foreach(IGameUpdate updateCall in sceneUpdateCalls)
            {
                try
                {
                    // Call start
                    updateCall.OnStart();
                }
                catch (Exception e)
                {
                    // Log exception
                    Debug.LogException(e);
                }
            }
        }

        public void OnUpdate(GameTime gameTime)
        {
            // Check for waiting objects
            while(sceneNewObjectsThisFrame.Count > 0)
            {
                // Get the update call
                IGameUpdate updateCall = sceneNewObjectsThisFrame.Dequeue();

                try
                {
                    // Call start
                    updateCall.OnStart();
                }
                catch (Exception e)
                {
                    // Log exception
                    Debug.LogException(e);
                }
            }


            // Update all objects
            foreach(IGameUpdate updateCall in sceneUpdateCalls)
            {
                try
                {
                    // Call update
                    updateCall.OnUpdate(gameTime);
                }
                catch (Exception e)
                {
                    // Log exception
                    Debug.LogException(e);
                }
            }
        }

        void IGameModule.OnDestroy()
        {
            OnDestroy();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
        }

        #region CreateGameObject
        public GameObject CreateEmptyObject(string name = null)
        {
            GameObject go = new GameObject(name);

            // Initialize the game object
            CreateObject(go);
            return go;
        }

        public GameObject CreatePrimitiveObject(GameObjectPrimitive primitive, string name = null)
        {
            GameObject go = new GameObject(name);

            // Add components

            // Initialize the game object
            CreateObject(go);
            return go;
        }

        public GameObject CreateObject(Type[] componentTypes, string name = null)
        {
            // Check for null
            if(componentTypes == null)
                throw new ArgumentNullException(nameof(componentTypes));

            GameObject go = new GameObject(name);

            // Initialize the game object
            CreateObject(go);

            // Add components
            foreach(Type componentType in componentTypes)
            {
                go.CreateComponent(componentType);
            }
            return go;
        }

        public Component CreateObject(Type mainComponentType, Type[] additionalComponentTypes = null, string name = null)
        {
            // Check for null
            if(mainComponentType == null)
                throw new ArgumentNullException(nameof(mainComponentType));

            // Create object
            GameObject go = new GameObject(name);

            // Initialize the game object
            CreateObject(go);

            // Add component
            Component result = go.CreateComponent(mainComponentType);

            // Add additional components
            if(additionalComponentTypes != null && additionalComponentTypes.Length > 0)
            {
                foreach(Type componentType in additionalComponentTypes)
                {
                    go.CreateComponent(componentType);
                }
            }

            return result;
        }

        public T CreateObject<T>(Type[] additionalComponentTypes = null, string name = null) where T : Component
        {
            // Create object
            GameObject go = new GameObject(name);

            // Initialize the game object
            CreateObject(go);

            // Add component
            T result = go.CreateComponent<T>();

            // Add additional components
            if (additionalComponentTypes != null && additionalComponentTypes.Length > 0)
            {
                foreach (Type componentType in additionalComponentTypes)
                {
                    go.CreateComponent(componentType);
                }
            }

            return result;
        }

        private void CreateObject(GameObject go)
        {
            // Register object
            gameObjects.Add(go);
            go.scene = this;

            // Initialize transform
            go.Transform = new Transform(go);

            // Trigger enable
            GameObject.DoGameObjectEnabledEvents(go, true, true);
        }

        void IContentCallback.OnBeforeContentSave()
        {
        }

        void IContentCallback.OnAfterContentLoad()
        {
            foreach (GameObject go in gameObjects)
                GameObject.DoGameObjectSceneInitialize(go, this);
        }
        #endregion
    }
}
