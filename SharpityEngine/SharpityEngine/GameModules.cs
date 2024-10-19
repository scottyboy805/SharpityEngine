
using SharpityEngine.Graphics;

namespace SharpityEngine
{
    internal interface IGameModule : IGameUpdate, IGameDraw
    {
        // Methods
        void OnFrameStart();

        void OnFrameEnd();

        void OnDestroy();
    }

    internal sealed class GameModules : IGameModule
    {
        // Types
        private sealed class PriorityComparer : IComparer<IGameModule>
        {
            public int Compare(IGameModule x, IGameModule y)
            {
                return x.Priority.CompareTo(y.Priority);
            }
        }

        private sealed class DrawOrderComparer : IComparer<IGameModule>
        {
            public int Compare(IGameModule x, IGameModule y)
            {
                return x.DrawOrder.CompareTo(y.DrawOrder);
            }
        }

        // Private
        private static readonly PriorityComparer priorityComparer = new PriorityComparer();
        private static readonly DrawOrderComparer drawOrderComparer = new DrawOrderComparer();

        private readonly List<IGameModule> modules = new List<IGameModule>();
        private readonly List<IGameModule> updateModules = new List<IGameModule>();
        private readonly List<IGameModule> drawModules = new List<IGameModule>();

        // Properties
        public int Priority => throw new NotImplementedException();
        public bool Enabled => throw new NotImplementedException();
        public int DrawOrder => throw new NotImplementedException();

        // Methods
        public void AddModule(IGameModule module)
        {
            // Check for null or already added
            if (module == null || modules.Contains(module) == true)
                return;

            // Add module
            lock(modules)
            {
                modules.Add(module);
                updateModules.Add(module);
                drawModules.Add(module);

                // Sort modules
                updateModules.Sort(priorityComparer);
                drawModules.Sort(drawOrderComparer);
            }
        }

        public void RemoveModule(IGameModule module)
        {
            if(modules != null && modules.Contains(module) == true)
            {
                // Remove module
                lock(modules)
                {
                    modules.Remove(module);
                    updateModules.Remove(module);
                    drawModules.Remove(module);
                }
            }
        }

        public bool HasModule(IGameModule module)
        {
            return modules != null && module != null && modules.Contains(module) == true;
        }

        public T[] GetModulesOfType<T>() where T : IGameModule
        {
            List<T> results = new List<T>();

            foreach (IGameModule module in modules)
            {
                if (module is T)
                {
                    results.Add((T)module);
                }
            }

            return results.ToArray();
        }

        public IEnumerable<T> EnumerateModulesOfType<T>() where T : IGameModule
        {
            foreach (IGameModule module in modules)
            {
                if (module is T)
                {
                    yield return (T)module;
                }
            }
        }

        public IEnumerable<IGameModule> EnumerateDrawModules(int minDrawOrder, int maxDrawOrder)
        {
            foreach (IGameModule module in drawModules)
            {
                if (module.DrawOrder >= minDrawOrder && module.DrawOrder < maxDrawOrder)
                {
                    yield return module;
                }
            }
        }

        public void OnFrameStart()
        {
            // Call frame start
            SafeCallModule(modules, (IGameModule module)
                => module.OnFrameStart());
        }

        public void OnFrameEnd()
        {
            // Call frame end
            SafeCallModule(modules, (IGameModule module)
                => module.OnFrameEnd());
        }

        #region Update
        public void OnStart()
        {
            // Call start
            SafeCallModule(updateModules, (IGameModule module)
                => module.OnStart());
        }

        public void OnUpdate(GameTime gameTime)
        {
            // Call update
            SafeCallModule(updateModules, (IGameModule module)
                => module.OnUpdate(gameTime));
        }

        public void OnDestroy()
        {
            // Call destroy
            SafeCallModule(updateModules, (IGameModule module)
                => module.OnDestroy());
        }
        #endregion

        #region Draw
        public void OnBeforeDraw()
        {
            // Call before draw
            SafeCallModule(drawModules, (IGameModule module)
                => module.OnBeforeDraw());
        }

        public void OnDrawEarly(BatchRenderer batchRenderer)
        {
            // Call draw
            SafeCallModule(drawModules, (IGameModule module) =>
            {
                if(module.DrawOrder < 0)
                    module.OnDraw(batchRenderer);
            });
        }

        public void OnDrawLate(BatchRenderer batchRenderer)
        {
            // Call draw
            SafeCallModule(drawModules, (IGameModule module) =>
            {
                if (module.DrawOrder > 0)
                    module.OnDraw(batchRenderer);
            });
        }

        public void OnDraw(BatchRenderer batchRenderer)
        {
            // Call draw
            SafeCallModule(drawModules, (IGameModule module)
                => module.OnDraw(batchRenderer));
        }

        public void OnAfterDraw()
        {
            // Call after draw
            SafeCallModule(drawModules, (IGameModule module)
                => module.OnAfterDraw());
        }
        #endregion

        private static void SafeCallModule(IReadOnlyList<IGameModule> modules, Action<IGameModule> call)
        {
            foreach(IGameModule module in modules)
            {
                // Exception should not cause other modules to not be updated
                try
                {
                    call(module);
                }
                catch(Exception e)
                {
                    // Report exception
                    Debug.LogException(e);
                }
            }
        }
    }
}
