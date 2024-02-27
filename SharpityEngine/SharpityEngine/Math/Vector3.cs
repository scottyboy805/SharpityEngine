using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace SharpityEngine
{
    [DataContract]
    public struct Vector3 : IEquatable<Vector3>
    {
        // Public
        public static readonly Vector3 Zero = new Vector3();
        public static readonly Vector3 One = new Vector3(1f);
        public static readonly Vector3 Up = new Vector3(0f, 1f, 0f);
        public static readonly Vector3 Down = new Vector3(0f, -1f, 0f);
        public static readonly Vector3 Left = new Vector3(-1f, 0f, 0f);
        public static readonly Vector3 Right = new Vector3(1f, 0f, 0f);
        public static readonly Vector3 Forward = new Vector3(0f, 0f, 1f);
        public static readonly Vector3 Backward = new Vector3(0f, 0f, -1f);

        [DataMember]
        public float X;
        [DataMember]
        public float Y;
        [DataMember]
        public float Z;

        // Properties
        public float Magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (float)Math.Sqrt(X * X + Y * Y + Z * Z); }
        }

        public float SqrMagnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return X * X + Y * Y + Z * Z; }
        }

        public Vector3 Normalized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Vector3 result;
                result.X = X;
                result.Y = Y;
                result.Z = Z;
                result.Normalize();
                return result;
            }
        }

        public Vector3 XOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector3(X, 0f, 0f); }
        }

        public Vector3 YOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector3(0f, Y, 0f); }
        }

        public Vector3 ZOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector3(0f, 0f, Z); }
        }

        // Constructor
        public Vector3(float value)
        {
            this.X = value;
            this.Y = value;
            this.Z = value;
        }

        public Vector3(float x, float y)
        {
            this.X = x;
            this.Y = y;
            this.Z = 0f;
        }

        public Vector3(float x, float y, float z)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
        }

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(in Vector3 scale)
        {
            X *= scale.X;
            Y *= scale.Y;
            Z *= scale.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float mag = Magnitude;
            if(mag > 1E-05f)
            {
                X /= mag;
                Y /= mag;
                Z /= mag;
            }
            else
            {
                this = Zero;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Point3 RoundToPoint()
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
        public void FillArray(float[] arr, int offset, int count = 3)
        {
            if (count > 0) arr[offset] = X;
            if (count > 1) arr[offset + 1] = Y;
            if (count > 2) arr[offset + 2] = Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(in Vector3 from, in Vector3 to)
        {
            float mag = (float)Math.Sqrt(from.SqrMagnitude * to.SqrMagnitude);
            if (mag < 1E-15f)
                return 0f;

            float limit = Mathf.Clamp(Dot(from, to) / mag, -1f, 1f);
            return (float)Math.Acos(limit) * Mathf.RadToDeg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Distance(in Vector3 a, in Vector3 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            return (float)Math.Sqrt(x * x + y * y + z * z);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DistanceSqr(in Vector3 a, in Vector3 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            return x * x + y * y + z * z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(in Vector3 a, in Vector3 b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Lerp(in Vector3 a, in Vector3 b, float t)
        {
            t = Mathf.Clamp01(t);
            Vector3 result;
            result.X = a.X + (b.X - a.X) * t;
            result.Y = a.Y + (b.Y - a.Y) * t;
            result.Z = a.Z + (b.Z - a.Z) * t;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 LerpExtrap(in Vector3 a, in Vector3 b, float t)
        {
            Vector3 result;
            result.X = a.X + (b.X - a.X) * t;
            result.Y = a.Y + (b.Y - a.Y) * t;
            result.Z = a.Z + (b.Z - a.Z) * t;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 MoveTowards(in Vector3 a, in Vector3 b, float maxDelta)
        {
            float x = b.X - a.X;
            float y = b.Y - a.Y;
            float z = b.Z - a.Z;
            float magSqr = x * x + y * y + z * z;
            if (magSqr == 0f || (maxDelta >= 0f && magSqr <= maxDelta * maxDelta))
                return b;

            float mag = (float)Math.Sqrt(magSqr);

            Vector3 result;
            result.X = a.X + x / mag * maxDelta;
            result.Y = a.Y + y / mag * maxDelta;
            result.Z = a.Z + z / mag * maxDelta;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Max(in Vector3 a, in Vector3 b)
        {
            Vector3 result;
            result.X = Math.Max(a.X, b.X);
            result.Y = Math.Max(a.Y, b.Y);
            result.Z = Math.Max(a.Z, b.Z);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Min(in Vector3 a, in Vector3 b)
        {
            Vector3 result;
            result.X = Math.Min(a.X, b.X);
            result.Y = Math.Min(a.Y, b.Y);
            result.Z = Math.Min(a.Z, b.Z);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 Reflect(in Vector3 direction, in Vector3 normal)
        {
            float dot = -2f * Dot(normal, direction);

            Vector3 result;
            result.X = dot * normal.X + direction.X;
            result.Y = dot * normal.Y + direction.Y;
            result.Z = dot * normal.Z + direction.Z;
            return result;
        }

        public static Vector3 FromArray(float[] arr, int offset, int count = 3)
        {
            Vector3 result = new Vector3();
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
            if (obj is Vector3)
                return Equals((Vector3)obj);

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Vector3 other)
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
        public static Vector3 operator +(in Vector3 a, in Vector3 b)
        {
            Vector3 result;
            result.X = a.X + b.X;
            result.Y = a.Y + b.Y;
            result.Z = a.Z + b.Z;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(in Vector3 a, in Vector3 b)
        {
            Vector3 result;
            result.X = a.X - b.X;
            result.Y = a.Y - b.Y;
            result.Z = a.Z - b.Z;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(in Vector3 a, in Vector3 b)
        {
            Vector3 result;
            result.X = a.X * b.X;
            result.Y = a.Y * b.Y;
            result.Z = a.Z * b.Z;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator /(in Vector3 a, in Vector3 b)
        {
            Vector3 result;
            result.X = a.X / b.X;
            result.Y = a.Y / b.Y;
            result.Z = a.Z / b.Z;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator -(in Vector3 a)
        {
            Vector3 result;
            result.X = -a.X;
            result.Y = -a.Y;
            result.Z = -a.Z;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(in Vector3 a, float val)
        {
            Vector3 result;
            result.X = a.X * val;
            result.Y = a.Y * val;
            result.Z = a.Z * val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator *(float val, in Vector3 a)
        {
            Vector3 result;
            result.X = a.X * val;
            result.Y = a.Y * val;
            result.Z = a.Z * val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Vector3 operator /(in Vector3 a, float val)
        {
            Vector3 result;
            result.X = a.X / val;
            result.Y = a.Y / val;
            result.Z = a.Z / val;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Vector3 a, in Vector3 b)
        {
            float x = a.X - b.X;
            float y = a.Y - b.Y;
            float z = a.Z - b.Z;
            return x * x + y * y + z * z < 9.99999944E-11f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Vector3 a, in Vector3 b)
        {
            return !(a == b);
        }
#endregion

#region Conversion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point2(Vector3 vector)
        {
            Point2 result;
            result.X = (int)vector.X;
            result.Y = (int)vector.Y;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Point3(Vector3 vector)
        {
            Point3 result;
            result.X = (int)vector.X;
            result.Y = (int)vector.Y;
            result.Z = (int)vector.Z;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Vector2(Vector3 vector)
        {
            Vector2 result;
            result.X = vector.X;
            result.Y = vector.Y;
            return result;
        }
#endregion
    }
}