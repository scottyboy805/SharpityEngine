using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace SharpityEngine
{
    public struct Point4 : IEquatable<Point4>
    {
        // Public
        public static readonly Point4 Zero = new Point4();
        public static readonly Point4 One = new Point4(1);
        public static readonly Point4 Up = new Point4(0, 1, 0, 0);
        public static readonly Point4 Down = new Point4(0, -1, 0, 0);
        public static readonly Point4 Right = new Point4(1, 0, 0, 0);
        public static readonly Point4 Left = new Point4(-1, 0, 0, 0);
        public static readonly Point4 Forward = new Point4(0, 0, 0, 1);
        public static readonly Point4 Backward = new Point4(0, 0, 0, -1);

        [DataMember]
        public int X;
        [DataMember]
        public int Y;
        [DataMember]
        public int Z;
        [DataMember]
        public int W;

        // Properties
        public float Magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector4(X, Y, Z, W).Magnitude; }
        }

        public float SqrMagnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector4(X, Y, Z, W).SqrMagnitude; }
        }

        public Vector4 Normalized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector4(X, Y, Z, W).Normalized; }
        }

        public Point4 XOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Point4(X, 0, 0, 0); }
        }

        public Point4 YOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Point4(0, Y, 0, 0); }
        }

        public Point4 ZOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Point4(0, 0, Z, 0); }
        }

        public Point4 WOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Point4(0, 0, 0, W); }
        }

        // Constructor
        public Point4(int val)
        {
            this.X = val;
            this.Y = val;
            this.Z = val;
            this.W = val;
        }

        public Point4(int x, int y, int z, int w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(in Point4 scale)
        {
            X *= scale.X;
            Y *= scale.Y;
            Z *= scale.Z;
            W *= scale.W;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillArray(int[] arr, int offset, int count = 4)
        {
            if (count > 0) arr[offset] = X;
            if (count > 1) arr[offset + 1] = Y;
            if (count > 2) arr[offset + 2] = Z;
            if (count > 3) arr[offset + 3] = W;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(in Point4 a, in Point4 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            float w = a.W - b.W;
            return (float)Math.Sqrt(x * x + y * y + z * z + w * w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSqr(in Point4 a, in Point4 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            float w = a.W - b.W;
            return x * x + y * y + z * z + w * w;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Lerp(in Point4 a, in Point4 b, float t)
        {
            t = Mathf.Clamp01(t);
            Vector4 result;
            result.X = a.X + (b.X - a.X) * t;
            result.Y = a.Y + (b.Y - a.Y) * t;
            result.Z = a.Z + (b.Z - a.Z) * t;
            result.W = a.W + (b.W - a.W) * t;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 LerpExtrap(in Point4 a, in Point4 b, float t)
        {
            Vector4 result;
            result.X = a.X + (b.X - a.X) * t;
            result.Y = a.Y + (b.Y - a.Y) * t;
            result.Z = a.Z + (b.Z - a.Z) * t;
            result.W = a.W + (b.W + a.W) * t;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 MoveTowards(in Point4 a, in Point4 b, float maxDelta)
        {
            float x = b.X - a.X;
            float y = b.Y - a.Y;
            float z = b.Z - a.Z;
            float w = b.W - a.W;
            float magSqr = x * x + y * y + z * z + w * w;
            if (magSqr == 0f || (maxDelta >= 0f && magSqr <= maxDelta * maxDelta))
                return (Vector4)b;

            float mag = (float)Math.Sqrt(magSqr);

            Vector4 result;
            result.X = a.X + x / mag * maxDelta;
            result.Y = a.Y + y / mag * maxDelta;
            result.Z = a.Z + z / mag * maxDelta;
            result.W = a.W + w / mag * maxDelta;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point4 Max(in Point4 a, in Point4 b)
        {
            Point4 result;
            result.X = Math.Max(a.X, b.X);
            result.Y = Math.Max(a.Y, b.Y);
            result.Z = Math.Max(a.Z, b.Z);
            result.W = Math.Max(a.W, b.W);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point4 Min(in Point4 a, in Point4 b)
        {
            Point4 result;
            result.X = Math.Min(a.X, b.X);
            result.Y = Math.Min(a.Y, b.Y);
            result.Z = Math.Min(a.Z, b.Z);
            result.W = Math.Min(a.W, b.W);
            return result;
        }

        public static Point4 FromArray(int[] arr, int offset, int count = 3)
        {
            Point4 result = new Point4();
            if (count > 0) result.X = arr[offset];
            if (count > 1) result.Y = arr[offset + 1];
            if (count > 2) result.Z = arr[offset + 2];
            if (count > 3) result.W = arr[offset + 3];
            return result;
        }

        #region Object Overrides
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3})", X, Y, Z, W);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj is Point4)
                return Equals((Point4)obj);

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Point4 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z && other.W == W;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode() << 4;
        }
        #endregion

        #region Operators
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point4 operator +(in Point4 a, in Point4 b)
        {
            Point4 result;
            result.X = a.X + b.X;
            result.Y = a.Y + b.Y;
            result.Z = a.Z + b.Z;
            result.W = a.W + b.W;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point4 operator -(in Point4 a, in Point4 b)
        {
            Point4 result;
            result.X = a.X - b.X;
            result.Y = a.Y - b.Y;
            result.Z = a.Z - b.Z;
            result.W = a.W - b.W;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point4 operator *(in Point4 a, in Point4 b)
        {
            Point4 result;
            result.X = a.X * b.X;
            result.Y = a.Y * b.Y;
            result.Z = a.Z * b.Z;
            result.W = a.W * b.W;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point4 operator /(in Point4 a, in Point4 b)
        {
            Point4 result;
            result.X = a.X / b.X;
            result.Y = a.Y / b.Y;
            result.Z = a.Z / b.Z;
            result.W = a.W / b.W;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point4 operator -(in Point4 a)
        {
            Point4 result;
            result.X = -a.X;
            result.Y = -a.Y;
            result.Z = -a.Z;
            result.W = -a.W;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point4 operator *(in Point4 a, int val)
        {
            Point4 result;
            result.X = a.X * val;
            result.Y = a.Y * val;
            result.Z = a.Z * val;
            result.W = a.W * val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point4 operator *(int val, in Point4 a)
        {
            Point4 result;
            result.X = a.X * val;
            result.Y = a.Y * val;
            result.Z = a.Z * val;
            result.W = a.W * val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Point4 operator /(in Point4 a, int val)
        {
            Point4 result;
            result.X = a.X / val;
            result.Y = a.Y / val;
            result.Z = a.Z / val;
            result.W = a.W / val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Point4 a, in Point4 b)
        {
            return a.X == b.X && a.Y == b.Y && a.Z == b.Z && a.W == b.W;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Point4 a, in Point4 b)
        {
            return !(a == b);
        }
        #endregion

        #region Conversion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point2(Point4 point)
        {
            Point2 result;
            result.X = point.X;
            result.Y = point.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point3(Point4 point)
        {
            Point3 result;
            result.X = point.X;
            result.Y = point.Y;
            result.Z = point.Z;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector2(Point4 point)
        {
            Vector2 result;
            result.X = point.X;
            result.Y = point.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector3(Point4 point)
        {
            Vector3 result;
            result.X = point.X;
            result.Y = point.Y;
            result.Z = point.Z;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector4(Point4 point)
        {
            Vector4 result;
            result.X = point.X;
            result.Y = point.Y;
            result.Z = point.Z;
            result.W = point.W;
            return result;
        }
        #endregion
    }
}
