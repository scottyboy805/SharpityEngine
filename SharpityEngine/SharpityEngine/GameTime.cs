
namespace SharpityEngine
{
    public sealed class GameTime
    {
        // Private
        private TimeSpan time = default;
        private TimeSpan elapsed = default;
        private float fpsTarget = 0f;
        private float fps = 0f;
        private int currentFrame = 0;

        // Properties
        public TimeSpan Time
        {
            get { return time; }
        }

        public float TimeSeconds
        {
            get { return (float)time.TotalSeconds; }
        }

        public TimeSpan Elapsed
        {
            get { return elapsed; }
        }

        public float ElapsedSeconds
        {
            get { return (float)elapsed.TotalSeconds; }
        }

        public float FrameRateTarget
        {
            get { return fpsTarget; }
        }

        public float FrameRate
        {
            get { return fps; }
        }

        public bool IsRunningSlowy
        {
            get { return (fpsTarget > 0) ? fps < (fpsTarget * 0.66f) : false; }
        }

        public int CurrentFrame
        {
            get { return currentFrame; }
        }

        // Constructor
        internal GameTime() { }

        // Methods
        internal void Update(TimeSpan total, TimeSpan elapsed, float fpsTarget, float fps, int frame)
        {
            this.time = total;
            this.elapsed = elapsed;
            this.fpsTarget = fpsTarget;
            this.fps = fps;
            this.currentFrame = frame;
        }
    }
}
