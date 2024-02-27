namespace SharpityEngine
{
    public static class Mathf
    {
        // Public
        public const float RadToDeg = 57.2958f;
        public const float DegToRad = (float)Math.PI / 180f;

        // Methods
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

        public static float InverseLerp(float a, float b, float value)
        {
            if(a != b)
            {
                return Clamp01((value - a) / (b - a));
            }
            return 0f;
        }

        public static float Lerp(float a, float b, float t)
        {
            return a + (b - a) * Clamp01(t);
        }

        public static float LerpExtrap(float a, float b, float t)
        {
            return a + (b - a) * t;
        }
    }
}