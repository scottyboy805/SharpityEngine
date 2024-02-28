
using SharpityEngine.Graphics.Pipeline;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace SharpityEngine
{
    public struct Vector4 : IEquatable<Vector4>
    {
        // Public
        public static readonly Vector4 Zero = new Vector4();
        public static readonly Vector4 One = new Vector4(1f);
        public static readonly Vector4 Up = new Vector4(0f, 1f, 0f, 1f);
        public static readonly Vector4 Down = new Vector4(0f, -1f, 0f, 1f);
        public static readonly Vector4 Left = new Vector4(-1f, 0f, 0f, 1f);
        public static readonly Vector4 Right = new Vector4(1f, 0f, 0f, 1f);
        public static readonly Vector4 Forward = new Vector4(0f, 0f, 1f, 1f);
        public static readonly Vector4 Backward = new Vector4(0f, 0f, -1f, 1f);

        [DataMember]
        public float X;
        [DataMember]
        public float Y;
        [DataMember]
        public float Z;
        [DataMember]
        public float W;

        // Properties
        public float Magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Mathf.Sqrt(X * X + Y * Y + Z * Z + W * W); }
        }

        public float SqrMagnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return X * X + Y * Y + Z * Z + W * W; }
        }

        public Vector4 Normalized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Vector4 result;
                result.X = X;
                result.Y = Y;
                result.Z = Z;
                result.W = W;
                result.Normalize();
                return result;
            }
        }

        public Vector4 XOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector4(X, 0f, 0f, 0f); }
        }

        public Vector4 YOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector4(0f, Y, 0f, 0f); }
        }

        public Vector4 ZOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector4(0f, 0f, 1f, 0f); }
        }

        // Constructor
        public Vector4(float value)
        {
            this.X = value;
            this.Y = value;
            this.Z = value;
            this.W = 1f;
        }

        public Vector4(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(in Vector4 scale)
        {
            X *= scale.X;
            Y *= scale.Y;
            Z *= scale.Z;
            W *= scale.W;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float mag = Magnitude;
            if (mag > 1E-05f)
            {
                X /= mag;
                Y /= mag;
                Z /= mag;
                W /= mag;
            }
            else
            {
                this = Zero;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point4 RoundToPoint()
        {
            Point4 result;
            result.X = (int)Math.Round(X);
            result.Y = (int)Math.Round(Y);
            result.Z = (int)Math.Round(Z);
            result.W = (int)Math.Round(W);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point3 RoundToPoint3()
        {
            Point3 result;
            result.X = (int)Math.Round(X);
            result.Y = (int)Math.Round(Y);
            result.Z = (int)Math.Round(Z);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point2 RoundToPoint2()
        {
            Point2 result;
            result.X = (int)Math.Round(X);
            result.Y = (int)Math.Round(Y);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillArray(float[] arr, int offset, int count = 4)
        {
            if (count > 0) arr[offset] = X;
            if (count > 1) arr[offset + 1] = Y;
            if (count > 2) arr[offset + 2] = Z;
            if (count > 3) arr[offset + 3] = W;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(in Vector3 a, in Vector3 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            float w = a.X - b.X;
            return Mathf.Sqrt(x * x + y * y + z * z + w * w);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSqr(in Vector3 a, in Vector3 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            float w = a.X - b.X;
            return x * x + y * y + z * z + w * w;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(in Vector4 a, in Vector4 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Lerp(in Vector4 a, in Vector4 b, float t)
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
        public static Vector4 LerpExtrap(in Vector4 a, in Vector4 b, float t)
        {
            Vector4 result;
            result.X = a.X + (b.X - a.X) * t;
            result.Y = a.Y + (b.Y - a.Y) * t;
            result.Z = a.Z + (b.Z - a.Z) * t;
            result.W = a.W + (b.W - a.W) * t;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 MoveTowards(in Vector4 a, in Vector4 b, float maxDelta)
        {
            float x = b.X - a.X;
            float y = b.Y - a.Y;
            float z = b.Z - a.Z;
            float w = b.W - a.W;
            float magSqr = x * x + y * y + z * z + w * w;
            if (magSqr == 0f || (maxDelta >= 0f && magSqr <= maxDelta * maxDelta))
                return b;

            float mag = (float)Math.Sqrt(magSqr);

            Vector4 result;
            result.X = a.X + x / mag * maxDelta;
            result.Y = a.Y + y / mag * maxDelta;
            result.Z = a.Z + z / mag * maxDelta;
            result.W = a.W + w / mag * maxDelta;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Max(in Vector4 a, in Vector4 b)
        {
            Vector4 result;
            result.X = Math.Max(a.X, b.X);
            result.Y = Math.Max(a.Y, b.Y);
            result.Z = Math.Max(a.Z, b.Z);
            result.W = Math.Max(a.W, b.W);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Min(in Vector4 a, in Vector4 b)
        {
            Vector4 result;
            result.X = Math.Min(a.X, b.X);
            result.Y = Math.Min(a.Y, b.Y);
            result.Z = Math.Min(a.Z, b.Z);
            result.W = Math.Min(a.W, b.W);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 Reflect(in Vector4 direction, in Vector4 normal)
        {
            float dot = -2f * Dot(normal, direction);

            Vector4 result;
            result.X = dot * normal.X + direction.X;
            result.Y = dot * normal.Y + direction.Y;
            result.Z = dot * normal.Z + direction.Z;
            result.W = dot * normal.W + direction.W;
            return result;
        }

        public static Vector4 FromArray(float[] arr, int offset, int count = 3)
        {
            Vector4 result = new Vector4();
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
            if (obj is Vector4)
                return Equals((Vector4)obj);

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector4 other)
        {
            return X == other.X && Y == other.Y && Z == other.Z && W == other.W;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ Y.GetHashCode() ^ Z.GetHashCode() ^ W.GetHashCode() << 4;
        }
        #endregion

        #region Operators
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator +(in Vector4 a, in Vector4 b)
        {
            Vector4 result;
            result.X = a.X + b.X;
            result.Y = a.Y + b.Y;
            result.Z = a.Z + b.Z;
            result.W = a.W + b.W;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator -(in Vector4 a, in Vector4 b)
        {
            Vector4 result;
            result.X = a.X - b.X;
            result.Y = a.Y - b.Y;
            result.Z = a.Z - b.Z;
            result.W = a.W - b.W;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator *(in Vector4 a, in Vector4 b)
        {
            Vector4 result;
            result.X = a.X * b.X;
            result.Y = a.Y * b.Y;
            result.Z = a.Z * b.Z;
            result.W = a.W * b.W;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator /(in Vector4 a, in Vector4 b)
        {
            Vector4 result;
            result.X = a.X / b.X;
            result.Y = a.Y / b.Y;
            result.Z = a.Z / b.Z;
            result.W = a.W / b.W;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator -(in Vector4 a)
        {
            Vector4 result;
            result.X = -a.X;
            result.Y = -a.Y;
            result.Z = -a.Z;
            result.W = -a.W;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator *(in Vector4 a, float val)
        {
            Vector4 result;
            result.X = a.X * val;
            result.Y = a.Y * val;
            result.Z = a.Z * val;
            result.W = a.W * val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator *(float val, in Vector4 a)
        {
            Vector4 result;
            result.X = a.X * val;
            result.Y = a.Y * val;
            result.Z = a.Z * val;
            result.W = a.W * val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector4 operator /(in Vector4 a, float val)
        {
            Vector4 result;
            result.X = a.X / val;
            result.Y = a.Y / val;
            result.Z = a.Z / val;
            result.W = a.W / val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Vector4 a, in Vector4 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            float w = a.W - b.W;
            return x * x + y * y + z * z + w * w < 9.99999944E-11f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Vector4 a, in Vector4 b)
        {
            return !(a == b);
        }
        #endregion

        #region Conversion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point2(Vector4 vector)
        {
            Point2 result;
            result.X = (int)vector.X;
            result.Y = (int)vector.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point3(Vector4 vector)
        {
            Point3 result;
            result.X = (int)vector.X;
            result.Y = (int)vector.Y;
            result.Z = (int)vector.Z;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector2(Vector4 vector)
        {
            Vector2 result;
            result.X = vector.X;
            result.Y = vector.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector3(Vector4 vector)
        {
            Vector3 result;
            result.X = vector.X;
            result.Y = vector.Y;
            result.Z = vector.Z;
            return result;
        }
        #endregion
    }
}
