using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace SharpityEngine
{
    // Still needs finishing

    [DataContract]
    public struct Point2 : IEquatable<Point2>
    {
        // Public
        public static readonly Point2 Zero = new Point2();
        public static readonly Point2 One = new Point2(1);
        public static readonly Point2 Up = new Point2(0, -1);
        public static readonly Point2 Down = new Point2(0, 1);
        public static readonly Point2 Right = new Point2(1, 0);
        public static readonly Point2 Left = new Point2(-1, 0);

        [DataMember]
        public int X;
        [DataMember]
        public int Y;

        // Properties
        public float Magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector2(X, Y).Magnitude; }
        }

        public float SqrMagnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector2(X, Y).SqrMagnitude; }
        }

        public Vector2 Normalized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector2(X, Y).Normalized; }
        }

        public Point2 XOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Point2(X, 0); }
        }

        public Point2 YOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Point2(0, Y); }
        }

        // Constructor
        public Point2(int val)
        {
            this.X = val;
            this.Y = val;
        }

        public Point2(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(in Point2 scale)
        {
            X *= scale.X;
            Y *= scale.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillArray(int[] arr, int offset, int count = 2)
        {
            if (count > 0) arr[offset] = X;
            if (count > 1) arr[offset + 1] = Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(in Point2 a, in Point2 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            return (float)Math.Sqrt(x * x + y * y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSqr(in Point2 a, in Point2 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            return x * x + y * y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Lerp(in Point2 a, in Point2 b, float t)
        {
            t = Mathf.Clamp01(t);
            Vector2 result;
            result.X = a.X + (b.X - a.X) * t;
            result.Y = a.Y + (b.Y - a.Y) * t;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 LerpExtrap(in Point2 a, in Point2 b, float t)
        {
            Vector2 result;
            result.X = a.X + (b.X - a.X) * t;
            result.Y = a.Y + (b.Y - a.Y) * t;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 MoveTowards(in Point2 a, in Point2 b, float maxDelta)
        {
            float x = b.X - a.X;
            float y = b.Y - a.Y;
            float magSqr = x * x + y * y;
            if (magSqr == 0f || (maxDelta >= 0f && magSqr <= maxDelta * maxDelta))
                return (Vector2)b;

            float mag = (float)Math.Sqrt(magSqr);

            Vector2 result;
            result.X = a.X + x / mag * maxDelta;
            result.Y = a.Y + y / mag * maxDelta;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point2 Max(in Point2 a, in Point2 b)
        {
            Point2 result;
            result.X = Math.Max(a.X, b.X);
            result.Y = Math.Max(a.Y, b.Y);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point2 Min(in Point2 a, in Point2 b)
        {
            Point2 result;
            result.X = Math.Min(a.X, b.X);
            result.Y = Math.Min(a.Y, b.Y);
            return result;
        }

        public static Point2 FromArray(int[] arr, int offset, int count = 2)
        {
            Point2 result = new Point2();
            if (count > 0) result.X = arr[offset];
            if (count > 1) result.Y = arr[offset + 1];
            return result;
        }

#region Object Overrides
        public override string ToString()
        {
            return string.Format("({0}, {1})", X, Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj is Point2)
                return Equals((Point2)obj);

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Point2 other)
        {
            return X == other.X && Y == other.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() << 4;
        }
#endregion

#region Operators
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point2 operator +(in Point2 a, in Point2 b)
        {
            Point2 result;
            result.X = a.X + b.X;
            result.Y = a.Y + b.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point2 operator -(in Point2 a, in Point2 b)
        {
            Point2 result;
            result.X = a.X - b.X;
            result.Y = a.Y - b.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point2 operator *(in Point2 a, in Point2 b)
        {
            Point2 result;
            result.X = a.X * b.X;
            result.Y = a.Y * b.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point2 operator /(in Point2 a, in Point2 b)
        {
            Point2 result;
            result.X = a.X / b.X;
            result.Y = a.Y / b.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point2 operator -(in Point2 a)
        {
            Point2 result;
            result.X = -a.X;
            result.Y = -a.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point2 operator *(in Point2 a, int val)
        {
            Point2 result;
            result.X = a.X * val;
            result.Y = a.Y * val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point2 operator *(int val, in Point2 a)
        {
            Point2 result;
            result.X = a.X * val;
            result.Y = a.Y * val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point2 operator /(in Point2 a, int val)
        {
            Point2 result;
            result.X = a.X / val;
            result.Y = a.Y / val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Point2 a, in Point2 b)
        {
            return a.X == b.X && a.Y == b.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Point2 a, in Point2 b)
        {
            return !(a == b);
        }
#endregion

#region Conversion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point3(Point2 point)
        {
            Point3 result;
            result.X = point.X;
            result.Y = point.Y;
            result.Z = 0;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point4(Point2 point)
        {
            Point4 result;
            result.X = point.X;
            result.Y = point.Y;
            result.Z = 0;
            result.W = 0;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector2(Point2 point)
        {
            Vector2 result;
            result.X = point.X;
            result.Y = point.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector3(Point2 point)
        {
            Vector3 result;
            result.X = point.X;
            result.Y = point.Y;
            result.Z = 0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector4(Point2 point)
        {
            Vector4 result;
            result.X = point.X;
            result.Y = point.Y;
            result.Z = 0f;
            result.W = 0f;
            return result;
        }
        #endregion
    }
}