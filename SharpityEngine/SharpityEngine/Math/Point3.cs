using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace SharpityEngine
{
    [DataContract]
    public struct Point3 : IEquatable<Point3>
    {
        // Public
        public static readonly Point3 Zero = new Point3();
        public static readonly Point3 One = new Point3(1);
        public static readonly Point3 Up = new Point3(0, 1, 0);
        public static readonly Point3 Down = new Point3(0, -1, 0);
        public static readonly Point3 Right = new Point3(1, 0, 0);
        public static readonly Point3 Left = new Point3(-1, 0, 0);
        public static readonly Point3 Forward = new Point3(0, 0, 1);
        public static readonly Point3 Backward = new Point3(0, 0, -1);

        [DataMember]
        public int X;
        [DataMember]
        public int Y;
        [DataMember]
        public int Z;

        // Properties
        public float Magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector3(X, Y, Z).Magnitude; }
        }

        public float SqrMagnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector3(X, Y, Z).SqrMagnitude; }
        }

        public Vector3 Normalized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector3(X, Y, Z).Normalized; }
        }

        public Point3 XOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Point3(X, 0, 0); }
        }

        public Point3 YOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Point3(0, Y, 0); }
        }

        public Point3 ZOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Point3(0, 0, Z); }
        }

        // Constructor
        public Point3(int val)
        {
            this.X = val;
            this.Y = val;
            this.Z = val;
        }

        public Point3(int x, int y)
        {
            this.X = x;
            this.Y = y;
            this.Z = 0;
        }

        public Point3(int x, int y, int z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(in Point3 scale)
        {
            X *= scale.X;
            Y *= scale.Y;
            Z *= scale.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillArray(int[] arr, int offset, int count = 3)
        {
            if (count > 0) arr[offset] = X;
            if (count > 1) arr[offset + 1] = Y;
            if (count > 2) arr[offset + 2] = Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(in Point3 a, in Point3 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSqr(in Point3 a, in Point3 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            return x * x + y * y + z * z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Lerp(in Point3 a, in Point3 b, float t)
        {
            t = Mathf.Clamp01(t);
            Vector3 result;
            result.X = a.X + (b.X - a.X) * t;
            result.Y = a.Y + (b.Y - a.Y) * t;
            result.Z = a.Z + (b.Z - a.Z) * t;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 LerpExtrap(in Point3 a, in Point3 b, float t)
        {
            Vector3 result;
            result.X = a.X + (b.X - a.X) * t;
            result.Y = a.Y + (b.Y - a.Y) * t;
            result.Z = a.Z + (b.Z - a.Z) * t;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 MoveTowards(in Point3 a, in Point3 b, float maxDelta)
        {
            float x = b.X - a.X;
            float y = b.Y - a.Y;
            float z = b.Z - a.Z;
            float magSqr = x * x + y * y + z * z;
            if (magSqr == 0f || (maxDelta >= 0f && magSqr <= maxDelta * maxDelta))
                return (Vector3)b;

            float mag = (float)Math.Sqrt(magSqr);

            Vector3 result;
            result.X = a.X + x / mag * maxDelta;
            result.Y = a.Y + y / mag * maxDelta;
            result.Z = a.Z + z / mag * maxDelta;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point3 Max(in Point3 a, in Point3 b)
        {
            Point3 result;
            result.X = Math.Max(a.X, b.X);
            result.Y = Math.Max(a.Y, b.Y);
            result.Z = Math.Max(a.Z, b.Z);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point3 Min(in Point3 a, in Point3 b)
        {
            Point3 result;
            result.X = Math.Min(a.X, b.X);
            result.Y = Math.Min(a.Y, b.Y);
            result.Z = Math.Min(a.Z, b.Z);
            return result;
        }

        public static Point3 FromArray(int[] arr, int offset, int count = 3)
        {
            Point3 result = new Point3();
            if (count > 0) result.X = arr[offset];
            if (count > 1) result.Y = arr[offset + 1];
            if (count > 2) result.Z = arr[offset + 2];
            return result;
        }

#region Object Overrides
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2})", X, Y, Z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj is Point3)
                return Equals((Point3)obj);

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Point3 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() << 4;
        }
#endregion

#region Operators
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point3 operator +(in Point3 a, in Point3 b)
        {
            Point3 result;
            result.X = a.X + b.X;
            result.Y = a.Y + b.Y;
            result.Z = a.Z + b.Z;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point3 operator -(in Point3 a, in Point3 b)
        {
            Point3 result;
            result.X = a.X - b.X;
            result.Y = a.Y - b.Y;
            result.Z = a.Z - b.Z;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point3 operator *(in Point3 a, in Point3 b)
        {
            Point3 result;
            result.X = a.X * b.X;
            result.Y = a.Y * b.Y;
            result.Z = a.Z * b.Z;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point3 operator /(in Point3 a, in Point3 b)
        {
            Point3 result;
            result.X = a.X / b.X;
            result.Y = a.Y / b.Y;
            result.Z = a.Z / b.Z;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point3 operator -(in Point3 a)
        {
            Point3 result;
            result.X = -a.X;
            result.Y = -a.Y;
            result.Z = -a.Z;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point3 operator *(in Point3 a, int val)
        {
            Point3 result;
            result.X = a.X * val;
            result.Y = a.Y * val;
            result.Z = a.Z * val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point3 operator *(int val, in Point3 a)
        {
            Point3 result;
            result.X = a.X * val;
            result.Y = a.Y * val;
            result.Z = a.Z * val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point3 operator /(in Point3 a, int val)
        {
            Point3 result;
            result.X = a.X / val;
            result.Y = a.Y / val;
            result.Z = a.Z / val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Point3 a, in Point3 b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Point3 a, in Point3 b)
        {
            return !(a == b);
        }
#endregion

#region Conversion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point2(Point3 point)
        {
            Point2 result;
            result.X = point.X;
            result.Y = point.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector2(Point3 point)
        {
            Vector2 result;
            result.X = point.X;
            result.Y = point.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector3(Point3 point)
        {
            Vector3 result;
            result.X = point.X;
            result.Y = point.Y;
            result.Z = point.Z;
            return result;
        }
#endregion
    }
}