using System.Runtime.CompilerServices;

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

        public static byte NextB
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (byte)rand.Next(); }
        }

        public static short NextS
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (short)rand.Next(); }
        }

        public static int NextI
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return rand.Next(); }
        }

        public static float NextF
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (float)(float.MaxValue * 2.0 * (rand.NextDouble() - 0.5)); }
        }

        public static float NextF01
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (float)(float.MaxValue * 2.0 * (rand.NextDouble() - 0.5)); }
        }

        public static double NextD
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Range(double.MinValue, double.MaxValue); }
        }

        public static double NextD01
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Range(0d, 1d); }
        }

        public static Point2 NextPoint2
        {
            get { return new Point2(NextI, NextI); }
        }

        public static Point3 NextPoint3
        {
            get { return new Point3(NextI, NextI, NextI); }
        }

        public static Point4 NextPoint4
        {
            get { return new Point4(NextI, NextI, NextI, NextI); }
        }

        public static Vector2 NextVector2
        {
            get { return new Vector2(NextF, NextF); }
        }

        public static Vector3 NextVector3
        {
            get { return new Vector3(NextF, NextF, NextF); }
        }

        public static Vector4 NextVector4
        {
            get { return new Vector4(NextF, NextF, NextF, NextF); }
        }

        public static Vector2 NextUnitVector2
        {
            get { return NextVector2.Normalized; }
        }

        public static Vector3 NextUnitVector3
        {
            get { return NextVector3.Normalized; }
        }

        public static Vector4 NextUnitVector4
        {
            get { return NextVector4.Normalized; }
        }

        public static Quaternion NextRotation
        {
            get { return Quaternion.Euler(Range(0f, 360f), Range(0f, 360f), Range(0f, 360f)); }
        }

        public static Color NextColorRGB
        {
            get { return new Color(NextF01, NextF01, NextF01, 1f); }
        }

        public static Color NextColorRGBA
        {
            get { return new Color(NextF01, NextF01, NextF01, NextF01); }
        }

        public static Color32 NextColor32RGB
        {
            get { return new Color32(NextB, NextB, NextB, byte.MaxValue); }
        }

        public static Color32 NextColor32RGBA
        {
            get { return new Color32(NextB, NextB, NextB, NextB); }
        }

        public static Vector2 InsideUnitCircle
        {
            get
            {
                // Calculate theta
                float theta = NextF * 2f * Mathf.PI;

                // Get vector
                return new Vector2(Mathf.Cos(theta), Mathf.Sin(theta)) * Mathf.Sqrt(NextF);
            }
        }

        public static Vector3 InsideUnitSphere
        {
            get
            {
                return default;
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

        public static int Range(int min, int max)
        {
            return rand.Next(min, max);
        }

        public static float Range(float min, float max)
        {
            return (float)(rand.NextDouble() * (max - min) + min);
        }

        public static double Range(double min, double max)
        {
            return rand.NextDouble() * (max - min) + min;
        }

        public static Point2 Range(in Point2 min, in Point2 max)
        {
            return new Point2(Range(min.X, max.X), Range(min.Y, max.Y));
        }

        public static Point3 Range(in Point3 min, in Point3 max)
        {
            return new Point3(Range(min.X, max.X), Range(min.Y, max.Y), Range(min.Z, max.Z));
        }

        public static Point4 Range(in Point4 min, in Point4 max)
        {
            return new Point4(Range(min.X, max.X), Range(min.Y, max.Y), Range(min.Z, max.Z), Range(min.W, max.W));
        }

        public static Vector2 Range(in Vector2 min, in Vector2 max)
        {
            return new Vector2(Range(min.X, max.X), Range(min.Y, max.Y));
        }

        public static Vector3 Range(in Vector3 min, in Vector3 max)
        {
            return new Vector3(Range(min.X, max.X), Range(min.Y, max.Y), Range(min.Z, max.Z));
        }

        public static Vector4 Range(in Vector4 min, in Vector4 max)
        {
            return new Vector4(Range(min.X, max.X), Range(min.Y, max.Y), Range(min.Z, max.Z), Range(min.W, max.W));
        }

        public static Color Range(in Color min, in Color max)
        {
            return new Color(Range(min.R, max.R), Range(min.G, max.G), Range(min.B, max.B), Range(min.A, max.A));
        }

        public static Color32 Range(in Color32 min, in Color32 max)
        {
            return new Color32(Range(min.R, max.R), Range(min.G, max.G), Range(min.B, max.B), Range(min.A, max.A));
        }
    }
}