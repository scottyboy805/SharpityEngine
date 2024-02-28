using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace SharpityEngine
{
    [DataContract]
    public struct Quaternion
    {
        // Private
        private const float KEpsilon = 0.000001F;

        // Public
        public static readonly Quaternion Identity = new Quaternion(0f, 0f, 0f, 1f);

        [DataMember]
        public float X;
        [DataMember]
        public float Y;
        [DataMember]
        public float Z;
        [DataMember]
        public float W;

        // Properties
        public Vector3 EulerAngles
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get 
            { 
                return MakePositive(ToEulerRad(this) * Mathf.RadToDeg); 
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set 
            { 
                this = FromEulerRad(value * Mathf.DegToRad); 
            }
        }

        public Quaternion Normalized
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get 
            {
                Quaternion result;
                result.X = X;
                result.Y = Y;
                result.Z = Z;
                result.W = W;
                result.Normalize();
                return result;
            }
        }

        public float Magnitude
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return (float)Math.Sqrt(Dot(this, this)); }
        }

        // Constructor
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Quaternion(float x, float y, float z, float w)
        {
            this.X = x;
            this.Y = y;
            this.Z = z;
            this.W = w;
        }

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Normalize()
        {
            float mag = (float)Math.Sqrt(Dot(this, this));
            if (mag > KEpsilon)
            {
                X /= mag;
                Y /= mag;
                Z /= mag;
                W /= mag;
            }
            else
            {
                this = Identity;
            }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Euler(float x, float y, float z) 
        { 
            return FromEulerRad(new Vector3(x, y, z) * Mathf.DegToRad); 
        
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion Euler(Vector3 euler) 
        { 
            return FromEulerRad(euler * Mathf.DegToRad); 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Angle(Quaternion a, Quaternion b)
        {
            float dot = Mathf.Min(Mathf.Abs(Dot(a, b)), 1.0F);
            return IsEqualUsingDot(dot) ? 0.0f : Mathf.Acos(dot) * 2.0F * Mathf.RadToDeg;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ToAngleAxis(out float angle, out Vector3 axis) 
        {
            ToAxisAngleRad(this, out axis, out angle); angle *= Mathf.RadToDeg; 
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Dot(Quaternion a, Quaternion b)
        {
            return a.X * b.X + a.Y * b.Y + a.Z * b.Z + a.W * b.W;
        }

        private static Vector3 ToEulerRad(Quaternion rotation)
        {
            float sqw = rotation.W * rotation.W;
            float sqx = rotation.X * rotation.X;
            float sqy = rotation.Y * rotation.Y;
            float sqz = rotation.Z * rotation.Z;
            float unit = sqx + sqy + sqz + sqw; // if normalised is one, otherwise is correction factor
            float test = rotation.X * rotation.W - rotation.Y * rotation.Z;
            Vector3 v;

            if (test > 0.4995f * unit)
            { // singularity at north pole
                v.Y = 2f * Mathf.Atan2(rotation.Y, rotation.X);
                v.X = Mathf.PI / 2;
                v.Z = 0;
                return NormalizeAngles(v * Mathf.RadToDeg);
            }
            if (test < -0.4995f * unit)
            { // singularity at south pole
                v.Y = -2f * Mathf.Atan2(rotation.Y, rotation.X);
                v.X = -Mathf.PI / 2;
                v.Z = 0;
                return NormalizeAngles(v * Mathf.RadToDeg);
            }
            Quaternion q = new Quaternion(rotation.W, rotation.Z, rotation.X, rotation.Y);
            v.Y = (float)Math.Atan2(2f * q.X * q.W + 2f * q.Y * q.Z, 1 - 2f * (q.Z * q.Z + q.W * q.W));     // Yaw
            v.X = (float)Math.Asin(2f * (q.X * q.Z - q.W * q.Y));                             // Pitch
            v.Z = (float)Math.Atan2(2f * q.X * q.Y + 2f * q.Z * q.W, 1 - 2f * (q.Y * q.Y + q.Z * q.Z));      // Roll
            return NormalizeAngles(v * Mathf.RadToDeg);
        }

        private static Quaternion FromEulerRad(Vector3 euler)
        {
            var yaw = euler.X;
            var pitch = euler.Y;
            var roll = euler.Z;
            float rollOver2 = roll * 0.5f;
            float sinRollOver2 = (float)Math.Sin((double)rollOver2);
            float cosRollOver2 = (float)Math.Cos((double)rollOver2);
            float pitchOver2 = pitch * 0.5f;
            float sinPitchOver2 = (float)Math.Sin((double)pitchOver2);
            float cosPitchOver2 = (float)Math.Cos((double)pitchOver2);
            float yawOver2 = yaw * 0.5f;
            float sinYawOver2 = (float)Math.Sin((double)yawOver2);
            float cosYawOver2 = (float)Math.Cos((double)yawOver2);
            Quaternion result;
            result.X = cosYawOver2 * cosPitchOver2 * cosRollOver2 + sinYawOver2 * sinPitchOver2 * sinRollOver2;
            result.Y = cosYawOver2 * cosPitchOver2 * sinRollOver2 - sinYawOver2 * sinPitchOver2 * cosRollOver2;
            result.Z = cosYawOver2 * sinPitchOver2 * cosRollOver2 + sinYawOver2 * cosPitchOver2 * sinRollOver2;
            result.W = sinYawOver2 * cosPitchOver2 * cosRollOver2 - cosYawOver2 * sinPitchOver2 * sinRollOver2;
            return result;
        }

        private static void ToAxisAngleRad(Quaternion q, out Vector3 axis, out float angle)
        {
            if (Math.Abs(q.W) > 1.0f)
                q.Normalize();

            angle = 2.0f * (float)Math.Acos(q.W); // angle
            float den = (float)Math.Sqrt(1.0 - q.W * q.W);
            if (den > 0.0001f)
            {
                axis = new Vector3(q.X, q.Y, q.Z) / den;
            }
            else
            {
                // This occurs when the angle is zero. 
                // Not a problem: just set an arbitrary normalized axis.
                axis = new Vector3(1, 0, 0);
            }
        }

        private static Vector3 NormalizeAngles(Vector3 angles)
        {
            angles.X = NormalizeAngle(angles.X);
            angles.Y = NormalizeAngle(angles.Y);
            angles.Z = NormalizeAngle(angles.Z);
            return angles;
        }

        private static float NormalizeAngle(float angle)
        {
            float modAngle = angle % 360.0f;

            if (modAngle < 0.0f)
                return modAngle + 360.0f;
            else
                return modAngle;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsEqualUsingDot(float dot)
        {
            // Returns false in the presence of NaN values.
            return dot > 1.0f - KEpsilon;
        }

        private static Vector3 MakePositive(Vector3 euler)
        {
            float negativeFlip = -0.0001f * Mathf.RadToDeg;
            float positiveFlip = 360.0f + negativeFlip;

            if (euler.X < negativeFlip)
                euler.X += 360.0f;
            else if (euler.X > positiveFlip)
                euler.X -= 360.0f;

            if (euler.Y < negativeFlip)
                euler.Y += 360.0f;
            else if (euler.Y > positiveFlip)
                euler.Y -= 360.0f;

            if (euler.Z < negativeFlip)
                euler.Z += 360.0f;
            else if (euler.Z > positiveFlip)
                euler.Z -= 360.0f;

            return euler;
        }

        #region Object Overrides
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return X.GetHashCode() ^ (Y.GetHashCode() << 2) ^ (Z.GetHashCode() >> 2) ^ (W.GetHashCode() >> 1);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other)
        {
            if (!(other is Quaternion)) return false;

            return Equals((Quaternion)other);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Quaternion other)
        {
            return X.Equals(other.X) && Y.Equals(other.Y) && Z.Equals(other.Z) && W.Equals(other.W);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString()
        {
            return ToString(null, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format)
        {
            return ToString(format, null);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public string ToString(string format, IFormatProvider formatProvider)
        {
            if (string.IsNullOrEmpty(format))
                format = "F5";
            if (formatProvider == null)
                formatProvider = CultureInfo.InvariantCulture.NumberFormat;
            return string.Format("({0}, {1}, {2}, {3})", X.ToString(format, formatProvider), Y.ToString(format, formatProvider), Z.ToString(format, formatProvider), W.ToString(format, formatProvider));
        }
        #endregion

        #region Operators
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Quaternion lhs, Quaternion rhs)
        {
            return IsEqualUsingDot(Dot(lhs, rhs));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Quaternion lhs, Quaternion rhs)
        {
            // Returns true in the presence of NaN values.
            return !(lhs == rhs);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Quaternion operator *(Quaternion lhs, Quaternion rhs)
        {
            return new Quaternion(
                lhs.W * rhs.X + lhs.X * rhs.W + lhs.Y * rhs.Z - lhs.Z * rhs.Y,
                lhs.W * rhs.Y + lhs.Y * rhs.W + lhs.Z * rhs.X - lhs.X * rhs.Z,
                lhs.W * rhs.Z + lhs.Z * rhs.W + lhs.X * rhs.Y - lhs.Y * rhs.X,
                lhs.W * rhs.W - lhs.X * rhs.X - lhs.Y * rhs.Y - lhs.Z * rhs.Z);
        }

        public static Vector3 operator *(Quaternion rotation, Vector3 point)
        {
            float x = rotation.X * 2F;
            float y = rotation.Y * 2F;
            float z = rotation.Z * 2F;
            float xx = rotation.X * x;
            float yy = rotation.Y * y;
            float zz = rotation.Z * z;
            float xy = rotation.X * y;
            float xz = rotation.X * z;
            float yz = rotation.Y * z;
            float wx = rotation.W * x;
            float wy = rotation.W * y;
            float wz = rotation.W * z;

            Vector3 res;
            res.X = (1F - (yy + zz)) * point.X + (xy - wz) * point.Y + (xz + wy) * point.Z;
            res.Y = (xy + wz) * point.X + (1F - (xx + zz)) * point.Y + (yz - wx) * point.Z;
            res.Z = (xz - wy) * point.X + (yz + wx) * point.Y + (1F - (xx + yy)) * point.Z;
            return res;
        }
        #endregion
    }
}
