using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace SharpityEngine
{
    [DataContract]
    public struct Vector2 : IEquatable<Vector2>
    {
        // Public
        public static readonly Vector2 Zero = new Vector2();
        public static readonly Vector2 One = new Vector2(1f);
        public static readonly Vector2 Up = new Vector2(0f, 1f);
        public static readonly Vector2 Down = new Vector2(0f, -1f);
        public static readonly Vector2 Left = new Vector2(-1f, 0f);
        public static readonly Vector2 Right = new Vector2(1f, 0f);

        [DataMember]
        public float X;
        [DataMember]
        public float Y;

        // Properties        
        public float Magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (float)Math.Sqrt(X * X + Y * Y); }
        }

        public float SqrMagnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return X * X + Y * Y; }
        }

        public Vector2 Normalized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Vector2 result;
                result.X = X;
                result.Y = Y;
                result.Normalize();
                return result;
            }
        }

        public Vector2 XOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector2(X, 0f); }
        }

        public Vector2 YOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector2(0f, Y); }
        }

        // Constructor
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2(float value)
        {
            this.X = value;
            this.Y = value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector2(float x, float y)
        {
            this.X = x;
            this.Y = y;
        }

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(in Vector2 scale)
        {
            X *= scale.X;
            Y *= scale.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float mag = Magnitude;

            if(mag > 1E-05f)
            {
                X /= mag;
                Y /= mag;
            }
            else
            {
                this = Zero;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point2 RoundToPoint()
        {
            Point2 result;
            result.X = (int)Math.Round(X);
            result.Y = (int)Math.Round(Y);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point3 RoundToPoint3()
        {
            Point3 result;
            result.X = (int)Math.Round(X);
            result.Y = (int)Math.Round(Y);
            result.Z = 0;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillArray(float[] arr, int offset, int count = 2)
        {
            if (count > 0) arr[offset] = X;
            if (count > 1) arr[offset + 1] = Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(in Vector2 from, in Vector2 to)
        {
            float mag = (float)Math.Sqrt(from.SqrMagnitude * to.SqrMagnitude);
            if (mag < 1E-15f)
                return 0f;

            float limit = Mathf.Clamp(Dot(from, to) / mag, -1f, 1f);
            return (float)Math.Acos(limit) * Mathf.RadToDeg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(in Vector2 a, in Vector2 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            return (float)Math.Sqrt(x * x + y * y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSqr(in Vector2 a, in Vector2 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            return x * x + y * y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(in Vector2 a, in Vector2 b)
        {
            return a.X * b.Y + a.Y * b.Y;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Lerp(in Vector2 a, in Vector2 b, float t)
        {
            t = Mathf.Clamp01(t);
            Vector2 result = new Vector2();
            result.X = a.X + (b.X - a.X) * t;
            result.Y = a.Y + (b.Y - a.Y) * t;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 LerpExtrap(in Vector2 a, in Vector2 b, float t)
        {
            Vector2 result;
            result.X = a.X + (b.X - a.X) * t;
            result.Y = a.Y + (b.Y - a.Y) * t;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 MoveTowards(in Vector2 a, in Vector2 b, float maxDelta)
        {
            float x = b.X - a.X;
            float y = b.Y - a.Y;
            float magSqr = x * x + y * y;
            if (magSqr == 0f || (maxDelta >= 0f && magSqr <= maxDelta * maxDelta))
                return b;

            float mag = (float)Math.Sqrt(magSqr);

            Vector2 result;
            result.X = a.X + x / mag * maxDelta;
            result.Y = a.Y + y / mag * maxDelta;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Max(in Vector2 a, in Vector2 b)
        {
            Vector2 result;
            result.X = Math.Max(a.X, b.X);
            result.Y = Math.Max(a.Y, b.Y);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Min(in Vector2 a, in Vector2 b)
        {
            Vector2 result;
            result.X = Math.Min(a.X, b.X);
            result.Y = Math.Min(a.Y, b.Y);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 Reflect(in Vector2 direction, in Vector2 normal)
        {
            float dot = -2f * Dot(normal, direction);

            Vector2 result;
            result.X = dot * normal.X + direction.X;
            result.Y = dot * normal.Y + direction.Y;
            return result;
        }

        public static Vector2 FromArray(float[] arr, int offset, int count = 2)
        {
            Vector2 result = new Vector2();
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
            if (obj is Vector2)
                return Equals((Vector2)obj);

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector2 other)
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
        public static Vector2 operator+(in Vector2 a, in Vector2 b)
        {
            Vector2 result;
            result.X = a.X + b.X;
            result.Y = a.Y + b.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator -(in Vector2 a, in Vector2 b)
        {
            Vector2 result;
            result.X = a.X - b.X;
            result.Y = a.Y - b.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(in Vector2 a, in Vector2 b)
        {
            Vector2 result;
            result.X = a.X * b.X;
            result.Y = a.Y * b.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator /(in Vector2 a, in Vector2 b)
        {
            Vector2 result;
            result.X = a.X / b.X;
            result.Y = a.Y / b.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator -(in Vector2 a)
        {
            Vector2 result;
            result.X = -a.X;
            result.Y = -a.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(in Vector2 a, float val)
        {
            Vector2 result;
            result.X = a.X * val;
            result.Y = a.Y * val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator *(float val, in Vector2 a)
        {
            Vector2 result;
            result.X = a.X * val;
            result.Y = a.Y * val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector2 operator /(in Vector2 a, float val)
        {
            Vector2 result;
            result.X = a.X / val;
            result.Y = a.Y / val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Vector2 a, in Vector2 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            return x * x + y * y < 9.99999944E-11f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Vector2 a, in Vector2 b)
        {
            return !(a == b);
        }
#endregion

#region Conversion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point2(Vector2 vector)
        {
            Point2 result;
            result.X = (int)vector.X;
            result.Y = (int)vector.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point3(Vector2 vector)
        {
            Point3 result;
            result.X = (int)vector.X;
            result.Y = (int)vector.Y;
            result.Z = 0;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point4(Vector2 vector)
        {
            Point4 result;
            result.X = (int)vector.X;
            result.Y = (int)vector.Y;
            result.Z = 0;
            result.W = 0;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector3(Vector2 vector)
        {
            Vector3 result;
            result.X = vector.X;
            result.Y = vector.Y;
            result.Z = 0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector4(Vector2 vector)
        {
            Vector4 result;
            result.X = vector.X;
            result.Y = vector.Y;
            result.Z = 0f;
            result.W = 0f;
            return result;
        }
        #endregion
    }
}