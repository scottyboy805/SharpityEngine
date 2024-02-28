using System.Runtime.CompilerServices;

namespace SharpityEngine
{
    public static class Mathf
    {
        // Public
        public const float RadToDeg = 57.2958f;
        public const float DegToRad = (float)Math.PI / 180f;
        public const float PI = (float)Math.PI;

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Abs(float f)
        {
            return Math.Abs(f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Abs(int value)
        {
            return Math.Abs(value);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Acos(float f)
        {
            return (float)Math.Acos(f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Atan(float f)
        {
            return (float)Math.Atan(f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Atan2(float y, float x)
        {
            return (float)Math.Atan2(y, x);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp(float min, float max, float value)
        {
            // Check for too low
            if (value < min)
                return min;

            // Check for too high
            if (value > max)
                return max;

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Clamp01(float value)
        {
            // Check for too low
            if (value < 0f)
                return 0f;

            // Check for too high
            if (value > 1f)
                return 1f;

            return value;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Cos(float f)
        {
            return (float)Math.Cos(f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float DeltaAngle(float current, float target)
        {
            float num = Repeat(target - current, 360f);
            if (num > 180f)
            {
                num -= 360f;
            }
            return num;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Floor(float f)
        {
            return (float)Math.Floor(f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int FloorToInt(float f)
        {
            return (int)Math.Floor(f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float InverseLerp(float a, float b, float value)
        {
            if(a != b)
            {
                return Clamp01((value - a) / (b - a));
            }
            return 0f;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Clamp01(t);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float LerpExtrap(float a, float b, float t)
        {
            return a + (b - a) * t;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Max(float a, float b)
        {
            return (a > b) ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Max(int a, int b)
        {
            return (a > b) ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Min(float a, float b)
        {
            return (a < b) ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Min(int a, int b)
        {
            return (a < b) ? a : b;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MoveTowards(float current, float target, float maxDelta)
        {
            if (Abs(target - current) <= maxDelta)
            {
                return target;
            }
            return current + Sign(target - current) * maxDelta;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float MoveTowardsAngle(float current, float target, float maxDelta)
        {
            float num = DeltaAngle(current, target);
            if (0f - maxDelta < num && num < maxDelta)
            {
                return target;
            }
            target = current + num;
            return MoveTowards(current, target, maxDelta);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Repeat(float t, float length)
        {
            return Clamp(t - Floor(t / length) * length, 0f, length);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Round(float f)
        {
            return (float)Math.Round(f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int RoundToInt(float f)
        {
            return (int)Math.Round(f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sign(float f)
        {
            return (f >= 0f) ? 1f : (-1f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sin(float f)
        {
            return (float)Math.Sin(f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Sqrt(float f)
        {
            return (float)Math.Sqrt(f);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Tan(float f)
        {
            return (float)Math.Tan(f);
        }
    }
}