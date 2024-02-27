using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace SharpityEngine
{
    // Based on Matrix3x3 from sharpdx
    [DataContract]
    public struct Matrix3 : IEquatable<Matrix3>
    {
        // Public
        public static readonly Matrix3 Zero = new Matrix3();
        public static readonly Matrix3 Identity = new Matrix3(1f, 0f, 0f,
                                                              0f, 1f, 0f,
                                                              0f, 0f, 1f);

        [DataMember]
        public float M11;
        [DataMember]
        public float M12;
        [DataMember]
        public float M13;
        [DataMember]
        public float M21;
        [DataMember]
        public float M22;
        [DataMember]
        public float M23;
        [DataMember]
        public float M31;
        [DataMember]
        public float M32;
        [DataMember]
        public float M33;

        // Constructor
        public Matrix3(float val)
        {
            this.M11 = val;
            this.M12 = val;
            this.M13 = val;
            this.M21 = val;
            this.M22 = val;
            this.M23 = val;
            this.M31 = val;
            this.M32 = val;
            this.M33 = val;
        }

        public Matrix3(float m11, float m12, float m13, float m21, float m22, float m23, float m31, float m32, float m33)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M13 = m13;
            this.M21 = m21;
            this.M22 = m22;
            this.M23 = m23;
            this.M31 = m31;
            this.M32 = m32;
            this.M33 = m33;
        }

        // Methods
        public static Matrix3 Orthographic(float left, float right, float bottom, float top)
        {
            Matrix3 result = Matrix3.Identity;

            result.M11 = 2f / (right - left);
            result.M22 = 2f / (top - bottom);
            result.M33 = 1f;

            result.M31 = -(right + left) / (right - left);
            result.M32 = -(top + bottom) / (top - bottom);


            //result.M11 = 2.0f / (right - left);
            //result.M12 = 0f;
            //result.M13 = 0f;

            //result.M21 = 0f;
            //result.M22 = 2.0f / (top - bottom);            
            //result.M23 = 0f;

            //result.M31 = (left + right) / (left - right);
            //result.M32 = (top + bottom) / (bottom - top);
            //result.M33 =  near / (near - far);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 Translate(in Vector2 position)
        {
            Matrix3 result;
            result.M11 = 1f;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M21 = 0f;
            result.M22 = 1f;
            result.M23 = 0f;
            result.M31 = position.X;
            result.M32 = position.Y;
            result.M33 = 1.0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 Translate(float x, float y)
        {
            Matrix3 result;
            result.M11 = 1f;
            result.M12 = 0f;
            result.M13 = 0f;
            result.M21 = 0f;
            result.M22 = 1f;
            result.M23 = 0f;
            result.M31 = x;
            result.M32 = y;
            result.M33 = 1.0f;
            return result;
        }

        public static Matrix3 Rotate(float angleDegrees) 
        {
            // Get radians
            float radians = (float)Math.IEEERemainder(Mathf.DegToRad * angleDegrees, Math.PI * 2);

            float c, s;
            const float epsilon = 0.001f * (float)Math.PI / 180f;     // 0.1% of a degree

            if (radians > -epsilon && radians < epsilon)
            {
                // Exact case for zero rotation.
                c = 1;
                s = 0;
            }
            else if (radians > Math.PI / 2 - epsilon && radians < Math.PI / 2 + epsilon)
            {
                // Exact case for 90 degree rotation.
                c = 0;
                s = 1;
            }
            else if (radians < -Math.PI + epsilon || radians > Math.PI - epsilon)
            {
                // Exact case for 180 degree rotation.
                c = -1;
                s = 0;
            }
            else if (radians > -Math.PI / 2 - epsilon && radians < -Math.PI / 2 + epsilon)
            {
                // Exact case for 270 degree rotation.
                c = 0;
                s = -1;
            }
            else
            {
                // Arbitrary rotation.
                c = (float)Math.Cos(radians);
                s = (float)Math.Sin(radians);
            }

            Matrix3 result;
            // [  c  s  0 ]
            // [ -s  c  0 ]
            // [  0  0  1 ]
            result.M11 = c;
            result.M12 = s;
            result.M13 = 0.0f;
            result.M21 = -s;
            result.M22 = c;
            result.M23 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 Scale(in Vector2 scale)
        {
            Matrix3 result;
            result.M11 = scale.X;
            result.M12 = 0.0f;
            result.M21 = 0.0f;
            result.M13 = 0.0f;
            result.M22 = scale.Y;
            result.M23 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 Scale(float x, float y)
        {
            Matrix3 result;
            result.M11 = x;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = y;
            result.M23 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix3 Scale(float scale)
        {
            Matrix3 result;
            result.M11 = scale;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = scale;
            result.M23 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;
            return result;
        }

        public static Matrix3 Negate(in Matrix3 value)
        {
            Matrix3 result;
            result.M11 = -value.M11;
            result.M12 = -value.M12;
            result.M13 = -value.M13;
            result.M21 = -value.M21;
            result.M22 = -value.M22;
            result.M23 = -value.M23;
            result.M31 = -value.M31;
            result.M32 = -value.M32;
            result.M33 = -value.M33;
            return result;
        }

        public static Matrix3 Transpose(in Matrix3 value)
        {
            Matrix3 result;
            result.M11 = value.M11;
            result.M12 = value.M21;
            result.M13 = value.M31;
            result.M21 = value.M12;
            result.M22 = value.M22;
            result.M23 = value.M32;
            result.M31 = value.M13;
            result.M32 = value.M23;
            result.M33 = value.M33;
            return result;
        }

        public static bool operator ==(Matrix3 value1, Matrix3 value2)
        {
            return (value1.M11 == value2.M11 && value1.M22 == value2.M22 && // Check diagonal element first for early out.
                                                value1.M12 == value2.M12 &&
                    value1.M21 == value2.M21 &&
                    value1.M31 == value2.M31 && value1.M32 == value2.M32);
        }

        public static bool operator !=(Matrix3 value1, Matrix3 value2)
        {
            return (value1.M11 != value2.M11 || value1.M12 != value2.M12 ||
                    value1.M21 != value2.M21 || value1.M22 != value2.M22 ||
                    value1.M31 != value2.M31 || value1.M32 != value2.M32);
        }

        public bool Equals(Matrix3 other)
        {
            return (M11 == other.M11 && M22 == other.M22 && // Check diagonal element first for early out.
                                        M12 == other.M12 &&
                    M21 == other.M21 &&
                    M31 == other.M31 && M32 == other.M32);
        }

        public override bool Equals(object obj)
        {
            if (obj is Matrix3)
            {
                return Equals((Matrix3)obj);
            }

            return false;
        }

        public override string ToString()
        {
            CultureInfo ci = CultureInfo.CurrentCulture;
            return String.Format(ci, "{{ {{M11:{0} M12:{1}}} {{M21:{2} M22:{3}}} {{M31:{4} M32:{5}}} }}",
                                 M11.ToString(ci), M12.ToString(ci),
                                 M21.ToString(ci), M22.ToString(ci),
                                 M31.ToString(ci), M32.ToString(ci));
        }

        public override int GetHashCode()
        {
            return M11.GetHashCode() + M12.GetHashCode() +
                   M21.GetHashCode() + M22.GetHashCode() +
                   M31.GetHashCode() + M32.GetHashCode();
        }

        public static Matrix3 operator*(in Matrix3 a, in Matrix3 b)
        {
            Matrix3 result = default;
            result.M11 = (a.M11 * b.M11) + (a.M12 * b.M21) + (a.M13 * b.M31);
            result.M12 = (a.M11 * b.M12) + (a.M12 * b.M22) + (a.M13 * b.M32);
            result.M13 = (a.M11 * b.M13) + (a.M12 * b.M23) + (a.M13 * b.M33);
            result.M21 = (a.M21 * b.M11) + (a.M22 * b.M21) + (a.M23 * b.M31);
            result.M22 = (a.M21 * b.M12) + (a.M22 * b.M22) + (a.M23 * b.M32);
            result.M23 = (a.M21 * b.M13) + (a.M22 * b.M23) + (a.M23 * b.M33);
            result.M31 = (a.M31 * b.M11) + (a.M32 * b.M21) + (a.M33 * b.M31);
            result.M32 = (a.M31 * b.M12) + (a.M32 * b.M22) + (a.M33 * b.M32);
            result.M33 = (a.M31 * b.M13) + (a.M32 * b.M23) + (a.M33 * b.M33);
            return result;
        }
    }
}
