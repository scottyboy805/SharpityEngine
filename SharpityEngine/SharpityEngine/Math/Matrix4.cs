using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace SharpityEngine
{
    // Based on Matrix4x4 from numerics
    [DataContract]
    public struct Matrix4 : IEquatable<Matrix4>
    {
        // Public
        public static readonly Matrix4 Zero = new Matrix4();
        public static readonly Matrix4 Identity = new Matrix4(1f, 0f, 0f, 0f,
                                                              0f, 1f, 0f, 0f,
                                                              0f, 0f, 1f, 0f,
                                                              0f, 0f, 0f, 1f);

        [DataMember]
        public float M11;
        [DataMember]
        public float M12;
        [DataMember]
        public float M13;
        [DataMember]
        public float M14;
        [DataMember]
        public float M21;
        [DataMember]
        public float M22;
        [DataMember]
        public float M23;
        [DataMember]
        public float M24;
        [DataMember]
        public float M31;
        [DataMember]
        public float M32;
        [DataMember]
        public float M33;
        [DataMember]
        public float M34;
        [DataMember]
        public float M41;
        [DataMember]
        public float M42;
        [DataMember]
        public float M43;
        [DataMember]
        public float M44;

        // Properties
        public Vector3 Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Vector3 result = new Vector3();
                result.X = M41;
                result.Y = M42;
                result.Z = M43;
                return result;
            }
        }

        public Matrix4 Inverted
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get
            {
                Matrix4 result;
                float a = M11, b = M12, c = M13, d = M14;
                float e = M21, f = M22, g = M23, h = M24;
                float i = M31, j = M32, k = M33, l = M34;
                float m = M41, n = M42, o = M43, p = M44;

                float kp_lo = k * p - l * o;
                float jp_ln = j * p - l * n;
                float jo_kn = j * o - k * n;
                float ip_lm = i * p - l * m;
                float io_km = i * o - k * m;
                float in_jm = i * n - j * m;

                float a11 = +(f * kp_lo - g * jp_ln + h * jo_kn);
                float a12 = -(e * kp_lo - g * ip_lm + h * io_km);
                float a13 = +(e * jp_ln - f * ip_lm + h * in_jm);
                float a14 = -(e * jo_kn - f * io_km + g * in_jm);

                float det = a * a11 + b * a12 + c * a13 + d * a14;

                if (Math.Abs(det) < float.Epsilon)
                {
                    result = new Matrix4(float.NaN);
                    return result;
                }

                float invDet = 1.0f / det;

                result.M11 = a11 * invDet;
                result.M21 = a12 * invDet;
                result.M31 = a13 * invDet;
                result.M41 = a14 * invDet;

                result.M12 = -(b * kp_lo - c * jp_ln + d * jo_kn) * invDet;
                result.M22 = +(a * kp_lo - c * ip_lm + d * io_km) * invDet;
                result.M32 = -(a * jp_ln - b * ip_lm + d * in_jm) * invDet;
                result.M42 = +(a * jo_kn - b * io_km + c * in_jm) * invDet;

                float gp_ho = g * p - h * o;
                float fp_hn = f * p - h * n;
                float fo_gn = f * o - g * n;
                float ep_hm = e * p - h * m;
                float eo_gm = e * o - g * m;
                float en_fm = e * n - f * m;

                result.M13 = +(b * gp_ho - c * fp_hn + d * fo_gn) * invDet;
                result.M23 = -(a * gp_ho - c * ep_hm + d * eo_gm) * invDet;
                result.M33 = +(a * fp_hn - b * ep_hm + d * en_fm) * invDet;
                result.M43 = -(a * fo_gn - b * eo_gm + c * en_fm) * invDet;

                float gl_hk = g * l - h * k;
                float fl_hj = f * l - h * j;
                float fk_gj = f * k - g * j;
                float el_hi = e * l - h * i;
                float ek_gi = e * k - g * i;
                float ej_fi = e * j - f * i;

                result.M14 = -(b * gl_hk - c * fl_hj + d * fk_gj) * invDet;
                result.M24 = +(a * gl_hk - c * el_hi + d * ek_gi) * invDet;
                result.M34 = -(a * fl_hj - b * el_hi + d * ej_fi) * invDet;
                result.M44 = +(a * fk_gj - b * ek_gi + c * ej_fi) * invDet;

                return result;
            }
        }

        // Constructor
        public Matrix4(float val)
        {
            this.M11 = val;
            this.M12 = val;
            this.M13 = val;
            this.M14 = val;
            this.M21 = val;
            this.M22 = val;
            this.M23 = val;
            this.M24 = val;
            this.M31 = val;
            this.M32 = val;
            this.M33 = val;
            this.M34 = val;
            this.M41 = val;
            this.M42 = val;
            this.M43 = val;
            this.M44 = val;
        }

        public Matrix4(float m11, float m12, float m13, float m14,
                       float m21, float m22, float m23, float m24,
                       float m31, float m32, float m33, float m34,
                       float m41, float m42, float m43, float m44)
        {
            this.M11 = m11;
            this.M12 = m12;
            this.M13 = m13;
            this.M14 = m14;
            this.M21 = m21;
            this.M22 = m22;
            this.M23 = m23;
            this.M24 = m24;
            this.M31 = m31;
            this.M32 = m32;
            this.M33 = m33;
            this.M34 = m34;
            this.M41 = m41;
            this.M42 = m42;
            this.M43 = m43;
            this.M44 = m44;
        }

        public Matrix4(float[] mat4)
        {
            this.M11 = mat4[0];
            this.M12 = mat4[1];
            this.M13 = mat4[2];
            this.M14 = mat4[3];
            this.M21 = mat4[4];
            this.M22 = mat4[5];
            this.M23 = mat4[6];
            this.M24 = mat4[7];
            this.M31 = mat4[8];
            this.M32 = mat4[9];
            this.M33 = mat4[10];
            this.M34 = mat4[11];
            this.M41 = mat4[12];
            this.M42 = mat4[13];
            this.M43 = mat4[14];
            this.M44 = mat4[15];
        }

        public Matrix4(float[,] mat4)
        {
            this.M11 = mat4[0, 0];
            this.M12 = mat4[0, 1];
            this.M13 = mat4[0, 2];
            this.M14 = mat4[0, 3];
            this.M21 = mat4[1, 0];
            this.M22 = mat4[1, 1];
            this.M23 = mat4[1, 2];
            this.M24 = mat4[1, 3];
            this.M31 = mat4[2, 0];
            this.M32 = mat4[2, 1];
            this.M33 = mat4[2, 2];
            this.M34 = mat4[2, 3];
            this.M41 = mat4[3, 0];
            this.M42 = mat4[3, 1];
            this.M43 = mat4[3, 2];
            this.M44 = mat4[3, 3];
        }

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 PerspectiveFieldOfView(float fovDegrees, float aspectRatio, float nearPlane, float farPlane)
        {
            float fov = Mathf.DegToRad * fovDegrees;

            if (fov <= 0.0f || fov >= Math.PI)
                throw new ArgumentOutOfRangeException(nameof(fovDegrees));

            if (nearPlane <= 0.0f)
                throw new ArgumentOutOfRangeException(nameof(nearPlane));

            if (farPlane <= 0.0f)
                throw new ArgumentOutOfRangeException(nameof(farPlane));

            if (farPlane >= nearPlane)
                throw new ArgumentOutOfRangeException(nameof(nearPlane));

            float yScale = 1.0f / (float)Math.Tan(fov * 0.5f);
            float xScale = yScale / aspectRatio;

            Matrix4 result;
            result.M11 = xScale;
            result.M12 = result.M13 = result.M14 = 0.0f;
            result.M22 = yScale;
            result.M21 = result.M23 = result.M24 = 0.0f;
            result.M31 = result.M32 = 0.0f;
            result.M33 = farPlane / (nearPlane - farPlane);
            result.M34 = -1.0f;
            result.M41 = result.M42 = result.M44 = 0.0f;
            result.M43 = nearPlane * farPlane / (nearPlane - farPlane);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 Perspective(float width, float height, float nearPlane, float farPlane)
        {
            if (nearPlane <= 0.0f)
                throw new ArgumentOutOfRangeException(nameof(nearPlane));

            if (farPlane <= 0.0f)
                throw new ArgumentOutOfRangeException(nameof(farPlane));

            if (nearPlane >= farPlane)
                throw new ArgumentOutOfRangeException(nameof(nearPlane));

            Matrix4 result;
            result.M11 = 2.0f * nearPlane / width;
            result.M12 = result.M13 = result.M14 = 0.0f;
            result.M22 = 2.0f * nearPlane / height;
            result.M21 = result.M23 = result.M24 = 0.0f;
            result.M33 = farPlane / (nearPlane - farPlane);
            result.M31 = result.M32 = 0.0f;
            result.M34 = -1.0f;
            result.M41 = result.M42 = result.M44 = 0.0f;
            result.M43 = nearPlane * farPlane / (nearPlane - farPlane);
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 Orthographic(float width, float height, float nearPlane, float farPlane)
        {
            Matrix4 result;
            result.M11 = 2.0f / width;
            result.M12 = result.M13 = result.M14 = 0.0f;
            result.M22 = 2.0f / height;
            result.M21 = result.M23 = result.M24 = 0.0f;
            result.M33 = 1.0f / (nearPlane - farPlane);
            result.M31 = result.M32 = result.M34 = 0.0f;
            result.M41 = result.M42 = 0.0f;
            result.M43 = nearPlane / (nearPlane - farPlane);            
            result.M44 = 1.0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 OrthographicOffCenter(float left, float right, float bottom, float top, float nearPlane, float farPlane)
        {
            Matrix4 result;
            result.M11 = 2.0f / (right - left);
            result.M12 = result.M13 = result.M14 = 0.0f;
            result.M22 = 2.0f / (top - bottom);
            result.M21 = result.M23 = result.M24 = 0.0f;
            result.M33 = 1.0f / (nearPlane - farPlane);
            result.M31 = result.M32 = result.M34 = 0.0f;
            result.M41 = (left + right) / (left - right);
            result.M42 = (top + bottom) / (bottom - top);
            result.M43 = nearPlane / (nearPlane - farPlane);
            result.M44 = 1.0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Vector3 MultiplyPoint(in Vector3 point)
        {
            Vector3 result;
            result.X = point.X * M11 + point.Y * M21 + point.Z * M31 + M41;
            result.Y = point.X * M12 + point.Y * M22 + point.Z * M32 + M42;
            result.Z = point.X * M13 + point.Y * M23 + point.Z * M33 + M43;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 Transpose(in Matrix4 matrix)
        {
            Matrix4 result;
            result.M11 = matrix.M11;
            result.M12 = matrix.M21;
            result.M13 = matrix.M31;
            result.M14 = matrix.M41;
            result.M21 = matrix.M12;
            result.M22 = matrix.M22;
            result.M23 = matrix.M32;
            result.M24 = matrix.M42;
            result.M31 = matrix.M13;
            result.M32 = matrix.M23;
            result.M33 = matrix.M33;
            result.M34 = matrix.M43;
            result.M41 = matrix.M14;
            result.M42 = matrix.M24;
            result.M43 = matrix.M34;
            result.M44 = matrix.M44;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 Translate(in Vector3 translate)
        {
            Matrix4 result;
            result.M11 = 1.0f;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = 1.0f;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;
            result.M34 = 0.0f;

            result.M41 = translate.X;
            result.M42 = translate.Y;
            result.M43 = translate.Z;
            result.M44 = 1.0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 Translate(float x, float y, float z)
        {
            Matrix4 result;
            result.M11 = 1.0f;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = 1.0f;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;
            result.M34 = 0.0f;

            result.M41 = x;
            result.M42 = y;
            result.M43 = z;
            result.M44 = 1.0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 TranslateX(float x)
        {
            Matrix4 result;
            result.M11 = 1.0f;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = 1.0f;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;
            result.M34 = 0.0f;

            result.M41 = x;
            result.M42 = 0f;
            result.M43 = 0f;
            result.M44 = 1.0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 TranslateY(float y)
        {
            Matrix4 result;
            result.M11 = 1.0f;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = 1.0f;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;
            result.M34 = 0.0f;

            result.M41 = 0f;
            result.M42 = y;
            result.M43 = 0f;
            result.M44 = 1.0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 TranslateZ(float z)
        {
            Matrix4 result;
            result.M11 = 1.0f;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = 1.0f;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;
            result.M34 = 0.0f;

            result.M41 = 0f;
            result.M42 = 0f;
            result.M43 = z;
            result.M44 = 1.0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 RotationX(float xDegrees)
        {
            float radians = xDegrees * Mathf.DegToRad;
            float c = (float)Math.Cos(radians);
            float s = (float)Math.Sin(radians);

            Matrix4 result;
            result.M11 = 1.0f;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = c;
            result.M23 = s;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = -s;
            result.M33 = c;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 RotationY(float yDegrees)
        {
            float radians = yDegrees * Mathf.DegToRad;
            float c = (float)Math.Cos(radians);
            float s = (float)Math.Sin(radians);

            Matrix4 result;
            result.M11 = c;
            result.M12 = 0.0f;
            result.M13 = -s;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = 1.0f;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = s;
            result.M32 = 0.0f;
            result.M33 = c;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 RotationZ(float zDegrees)
        {
            float radians = Mathf.DegToRad * zDegrees;
            float c = (float)Math.Cos(radians);
            float s = (float)Math.Sin(radians);

            Matrix4 result;
            result.M11 = c;
            result.M12 = s;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = -s;
            result.M22 = c;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1.0f;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 AxisAngle(in Vector3 axis, float angleDegrees)
        {
            float angle = Mathf.DegToRad * angleDegrees;

            float x = axis.X, y = axis.Y, z = axis.Z;
            float sa = (float)Math.Sin(angle), ca = (float)Math.Cos(angle);
            float xx = x * x, yy = y * y, zz = z * z;
            float xy = x * y, xz = x * z, yz = y * z;

            Matrix4 result;
            result.M11 = xx + ca * (1.0f - xx);
            result.M12 = xy - ca * xy + sa * z;
            result.M13 = xz - ca * xz - sa * y;
            result.M14 = 0.0f;
            result.M21 = xy - ca * xy - sa * z;
            result.M22 = yy + ca * (1.0f - yy);
            result.M23 = yz - ca * yz + sa * x;
            result.M24 = 0.0f;
            result.M31 = xz - ca * xz + sa * y;
            result.M32 = yz - ca * yz - sa * x;
            result.M33 = zz + ca * (1.0f - zz);
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 Scale(in Vector3 scale)
        {
            Matrix4 result;
            result.M11 = scale.X;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = scale.Y;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = scale.Z;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 Scale(float x, float y, float z)
        {
            Matrix4 result;
            result.M11 = x;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = y;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = z;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 ScaleX(float x)
        {
            Matrix4 result;
            result.M11 = x;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = 1f;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1f;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 ScaleY(float y)
        {
            Matrix4 result;
            result.M11 = 1f;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = y;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = 1f;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 ScaleZ(float z)
        {
            Matrix4 result;
            result.M11 = 1f;
            result.M12 = 0.0f;
            result.M13 = 0.0f;
            result.M14 = 0.0f;
            result.M21 = 0.0f;
            result.M22 = 1f;
            result.M23 = 0.0f;
            result.M24 = 0.0f;
            result.M31 = 0.0f;
            result.M32 = 0.0f;
            result.M33 = z;
            result.M34 = 0.0f;
            result.M41 = 0.0f;
            result.M42 = 0.0f;
            result.M43 = 0.0f;
            result.M44 = 1.0f;
            return result;
        }

#region Object Overrides
        public override bool Equals(object obj)
        {
            if (obj is Matrix4)
                return Equals((Matrix4)obj);

            return false;
        }

        public bool Equals(Matrix4 other)
        {
            return (M11 == other.M11 && M22 == other.M22 && M33 == other.M33 && M44 == other.M44 &&
                    M12 == other.M12 && M13 == other.M13 && M14 == other.M14 &&
                    M21 == other.M21 && M23 == other.M23 && M24 == other.M24 &&
                    M31 == other.M31 && M32 == other.M32 && M34 == other.M34 &&
                    M41 == other.M41 && M42 == other.M42 && M43 == other.M43);
        }

        public override int GetHashCode()
        {
            return M11.GetHashCode() + M12.GetHashCode() + M13.GetHashCode() + M14.GetHashCode() +
                   M21.GetHashCode() + M22.GetHashCode() + M23.GetHashCode() + M24.GetHashCode() +
                   M31.GetHashCode() + M32.GetHashCode() + M33.GetHashCode() + M34.GetHashCode() +
                   M41.GetHashCode() + M42.GetHashCode() + M43.GetHashCode() + M44.GetHashCode();
        }
#endregion

#region Operators
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 operator+(in Matrix4 a, in Matrix4 b)
        {
            Matrix4 result;
            result.M11 = a.M11 + b.M11;
            result.M12 = a.M12 + b.M12;
            result.M13 = a.M13 + b.M13;
            result.M14 = a.M14 + b.M14;
            result.M21 = a.M21 + b.M21;
            result.M22 = a.M22 + b.M22;
            result.M23 = a.M23 + b.M23;
            result.M24 = a.M24 + b.M24;
            result.M31 = a.M31 + b.M31;
            result.M32 = a.M32 + b.M32;
            result.M33 = a.M33 + b.M33;
            result.M34 = a.M34 + b.M34;
            result.M41 = a.M41 + b.M41;
            result.M42 = a.M42 + b.M42;
            result.M43 = a.M43 + b.M43;
            result.M44 = a.M44 + b.M44;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 operator-(in Matrix4 a, in Matrix4 b)
        {
            Matrix4 result;
            result.M11 = a.M11 - b.M11;
            result.M12 = a.M12 - b.M12;
            result.M13 = a.M13 - b.M13;
            result.M14 = a.M14 - b.M14;
            result.M21 = a.M21 - b.M21;
            result.M22 = a.M22 - b.M22;
            result.M23 = a.M23 - b.M23;
            result.M24 = a.M24 - b.M24;
            result.M31 = a.M31 - b.M31;
            result.M32 = a.M32 - b.M32;
            result.M33 = a.M33 - b.M33;
            result.M34 = a.M34 - b.M34;
            result.M41 = a.M41 - b.M41;
            result.M42 = a.M42 - b.M42;
            result.M43 = a.M43 - b.M43;
            result.M44 = a.M44 - b.M44;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 operator*(in Matrix4 a, in Matrix4 b)
        {
            Matrix4 result;
            // First row
            result.M11 = a.M11 * b.M11 + a.M12 * b.M21 + a.M13 * b.M31 + a.M14 * b.M41;
            result.M12 = a.M11 * b.M12 + a.M12 * b.M22 + a.M13 * b.M32 + a.M14 * b.M42;
            result.M13 = a.M11 * b.M13 + a.M12 * b.M23 + a.M13 * b.M33 + a.M14 * b.M43;
            result.M14 = a.M11 * b.M14 + a.M12 * b.M24 + a.M13 * b.M34 + a.M14 * b.M44;
                         
            // Second row
            result.M21 = a.M21 * b.M11 + a.M22 * b.M21 + a.M23 * b.M31 + a.M24 * b.M41;
            result.M22 = a.M21 * b.M12 + a.M22 * b.M22 + a.M23 * b.M32 + a.M24 * b.M42;
            result.M23 = a.M21 * b.M13 + a.M22 * b.M23 + a.M23 * b.M33 + a.M24 * b.M43;
            result.M24 = a.M21 * b.M14 + a.M22 * b.M24 + a.M23 * b.M34 + a.M24 * b.M44;
                         
            // Third row
            result.M31 = a.M31 * b.M11 + a.M32 * b.M21 + a.M33 * b.M31 + a.M34 * b.M41;
            result.M32 = a.M31 * b.M12 + a.M32 * b.M22 + a.M33 * b.M32 + a.M34 * b.M42;
            result.M33 = a.M31 * b.M13 + a.M32 * b.M23 + a.M33 * b.M33 + a.M34 * b.M43;
            result.M34 = a.M31 * b.M14 + a.M32 * b.M24 + a.M33 * b.M34 + a.M34 * b.M44;
                         
            // Fourth row
            result.M41 = a.M41 * b.M11 + a.M42 * b.M21 + a.M43 * b.M31 + a.M44 * b.M41;
            result.M42 = a.M41 * b.M12 + a.M42 * b.M22 + a.M43 * b.M32 + a.M44 * b.M42;
            result.M43 = a.M41 * b.M13 + a.M42 * b.M23 + a.M43 * b.M33 + a.M44 * b.M43;
            result.M44 = a.M41 * b.M14 + a.M42 * b.M24 + a.M43 * b.M34 + a.M44 * b.M44;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 operator*(in Matrix4 a, float b)
        {
            Matrix4 result;
            result.M11 = a.M11 * b;
            result.M12 = a.M12 * b;
            result.M13 = a.M13 * b;
            result.M14 = a.M14 * b;
            result.M21 = a.M21 * b;
            result.M22 = a.M22 * b;
            result.M23 = a.M23 * b;
            result.M24 = a.M24 * b;
            result.M31 = a.M31 * b;
            result.M32 = a.M32 * b;
            result.M33 = a.M33 * b;
            result.M34 = a.M34 * b;
            result.M41 = a.M41 * b;
            result.M42 = a.M42 * b;
            result.M43 = a.M43 * b;
            result.M44 = a.M44 * b;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Matrix4 operator-(in Matrix4 m)
        {
            Matrix4 result;
            result.M11 = -m.M11;
            result.M12 = -m.M12;
            result.M13 = -m.M13;
            result.M14 = -m.M14;
            result.M21 = -m.M21;
            result.M22 = -m.M22;
            result.M23 = -m.M23;
            result.M24 = -m.M24;
            result.M31 = -m.M31;
            result.M32 = -m.M32;
            result.M33 = -m.M33;
            result.M34 = -m.M34;
            result.M41 = -m.M41;
            result.M42 = -m.M42;
            result.M43 = -m.M43;
            result.M44 = -m.M44;
            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator==(in Matrix4 a, in Matrix4 b)
        {
            return (a.M11 == b.M11 && a.M22 == b.M22 && a.M33 == b.M33 && a.M44 == b.M44 && 
                    a.M12 == b.M12 && a.M13 == b.M13 && a.M14 == b.M14 &&
                    a.M21 == b.M21 && a.M23 == b.M23 && a.M24 == b.M24 &&
                    a.M31 == b.M31 && a.M32 == b.M32 && a.M34 == b.M34 &&
                    a.M41 == b.M41 && a.M42 == b.M42 && a.M43 == b.M43);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator!=(in Matrix4 a, in Matrix4 b)
        {
            return (a.M11 != b.M11 || a.M12 != b.M12 || a.M13 != b.M13 || a.M14 != b.M14 ||
                    a.M21 != b.M21 || a.M22 != b.M22 || a.M23 != b.M23 || a.M24 != b.M24 ||
                    a.M31 != b.M31 || a.M32 != b.M32 || a.M33 != b.M33 || a.M34 != b.M34 ||
                    a.M41 != b.M41 || a.M42 != b.M42 || a.M43 != b.M43 || a.M44 != b.M44);
        }
#endregion
    }
}