using SharpityEngine.Graphics;
using System.ComponentModel;
using System.Runtime.Serialization;

namespace SharpityEngine.Scene
{
    public sealed class GameScene : GameAsset, IGameModule
    {
        // Internal
        internal HashSet<IGameDraw> sceneDrawCalls = new HashSet<IGameDraw>();

        // Private
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
        public void OnFrameStart() { }
        public void OnFrameEnd() { }

        public void OnBeforeDraw() { }
        public void OnAfterDraw() { }      
        public void OnDraw(BatchRenderer batchRenderer) { }

        public void OnStart()
        {
        }

        public void OnUpdate(GameTime gameTime)
        {
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

            // Register object
            gameObjects.Add(go);
            go.scene = this;

            // Trigger enable
            GameObject.DoGameObjectEnabledEvents(go, true, true);

            return go;
        }


        #endregion
    }
}
