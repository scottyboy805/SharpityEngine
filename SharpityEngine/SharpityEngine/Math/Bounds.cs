using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace SharpityEngine
{
    [DataContract]
    public struct Bounds : IEquatable<Bounds>
    {
        // Public
        public static readonly Bounds Zero = default;
        public static readonly Bounds One = new Bounds(Vector3.Zero, 1f);

        [DataMember]
        public Vector3 Center;
        [DataMember]
        public Vector3 Extents;

        // Properties
        public Vector3 Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Extents * 2f; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Extents = value * 0.5f; }
        }

        public Vector3 Min
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Center = Extents; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { SetMinMax(value, Max); }
        }

        public Vector3 Max
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Center + Extents; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { SetMinMax(Min, value); }
        }

        public Bounds XOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Bounds(new Vector3(Center.X, 0f, 0f), new Vector3(Extents.X, 0f, 0f)); }
        }

        public Bounds YOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Bounds(new Vector3(0f, Center.Y, 0f), new Vector3(0f, Extents.Y, 0f)); }
        }

        public Bounds ZOnly
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Bounds(new Vector3(0f, 0f, Center.Z), new Vector3(0f, 0f, Extents.Z)); }
        }

        // Constructor
        public Bounds(Vector3 center, float uniformExtents)
        {
            this.Center = center;
            this.Center = new Vector3(uniformExtents);
        }

        public Bounds(Vector3 center, Vector3 extents)
        {
            this.Center = center;
            this.Extents = extents;
        }

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Scale(in Vector3 scale)
        {
            Extents.Scale(scale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void FillArray(float[] arr, int offset, int count = 6)
        {
            // Write center
            if (count > 0)
                Center.FillArray(arr, offset, count);

            // Write extents
            if (count > 3)
                Extents.FillArray(arr, offset + 3, count - 3);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void SetMinMax(in Vector3 min, in Vector3 max)
        {
            Extents = (max - min) * 0.5f;
            Center = min + Extents;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Bounds FromArray(float[] arr, int offset, int count = 6)
        {
            Bounds result = default;

            // Get center
            if(count > 0)
                result.Center = Vector3.FromArray(arr, offset, count);

            // Get extents
            if (count > 3)
                result.Extents = Vector3.FromArray(arr, offset + 3, count - 3);

            return result;
        }

        #region Object Overrides
        public override string ToString()
        {
            return string.Format("Center: {0}, Extents: {1}", Center, Extents);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj is Bounds)
                return Equals((Bounds)obj);

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Bounds other)
        {
            return Center == other.Center && Extents == other.Extents;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode()
        {
            return Center.GetHashCode() ^ (Extents.GetHashCode() << 2);
        }
        #endregion

        #region Operators
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(in Bounds a, in Bounds b)
        {
            return a.Center == b.Center && a.Extents == b.Extents;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(in Bounds a, in Bounds b)
        {
            return !(a == b);
        }
        #endregion
    }
}
