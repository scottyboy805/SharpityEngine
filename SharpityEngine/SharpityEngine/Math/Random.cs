
namespace SharpityEngine
{
    public static class Random
    {
        // Private
        private static int seed = -1;
        private static System.Random rand = null;

        // Properties
        public static int Seed
        {
            get { return seed; }
            set
            {
                seed = value;
                rand = new System.Random(seed);
            }
        }

        // Constructor
        static Random()
        {
            Reseed();
        }

        // Methods
        public static void Reseed()
        {
            seed = Guid.NewGuid().GetHashCode();
            rand = new System.Random(seed);
        }

        public static int Next()
        {
            return rand.Next();
        }

        public static float NextF()
        {
            return (float)(float.MaxValue * 2.0 * (rand.NextDouble() - 0.5));
        }

        public static float NextFZeroOne()
        {
            return RangeF(0f, 1f);
        }

        public static double NextD()
        {
            return RangeD(double.MinValue, double.MaxValue);
        }

        public static double NextDZeroOne()
        {
            return RangeD(0d, 1d);
        }

        public static int Range(int min, int max)
        {
            return rand.Next(min, max);
        }

        public static float RangeF(float min, float max)
        {
            return (float)(rand.NextDouble() * (max - min) + min);
        }

        public static double RangeD(double min, double max)
        {
            return rand.NextDouble() * (max - min) + min;
        }

        public static Point2 Point2()
        {
            return new Point2(Next(), Next());
        }

        public static Point2 Point2Range(in Point2 min, in Point2 max)
        {
            return new Point2(Range(min.X, max.X), Range(min.Y, max.Y));
        }
    }
}