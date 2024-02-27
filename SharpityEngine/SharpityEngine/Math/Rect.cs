using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace SharpityEngine
{
    [DataContract]
    public struct Rect : IEquatable<Rect>
    {
        // Public
        public static readonly Rect Zero = new Rect();
        public static readonly Rect NormalizedOne = new Rect(0f, 0f, 1f, 1f);

        [DataMember]
        public float X;
        [DataMember]
        public float Y;
        [DataMember]
        public float Width;
        [DataMember]
        public float Height;

        // Properties
        public Vector2 Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector2(X, Y); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public Vector2 Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector2(Width, Height); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        public Vector2 Center
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector2(X + Width / 2f, Y + Height / 2f); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                X = value.X - Width / 2f;
                Y = value.Y - Height / 2f;
            }
        }

        public float Area
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Math.Abs(Width) * Math.Abs(Height); }
        }

        public float Left
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return X; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { X = value; }
        }

        public float Right
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return X + Width; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { X = value - Width; }
        }

        public float CenterX
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return X + Width / 2; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { X = value - Width / 2; }
        }

        public float Top
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Y; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Y = value; }
        }

        public float Bottom
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Y + Height; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Y = value - Height; }
        }

        public float CenterY
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Y + Height / 2; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Y = value - Height / 2; }
        }

        public Vector2 TopLeft
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector2(Left, Top); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Left = value.X;
                Top = value.Y;
            }
        }

        public Vector2 TopCenter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector2(CenterX, Top); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                CenterX = value.X;
                Top = value.Y;
            }
        }

        public Vector2 TopRight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector2(Right, Top); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Right = value.X;
                Top = value.Y;
            }
        }

        public Vector2 CenterLeft
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector2(Left, CenterY); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Left = value.X;
                CenterY = value.Y;
            }
        }

        public Vector2 CenterRight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector2(Right, CenterY); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Right = value.X;
                CenterY = value.Y;
            }
        }

        public Vector2 BottomLeft
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector2(Left, Bottom); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Left = value.X;
                Bottom = value.Y;
            }
        }

        public Vector2 BottomCenter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector2(CenterX, Bottom); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                CenterX = value.X;
                Bottom = value.Y;
            }
        }

        public Vector2 BottomRight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Vector2(Right, Bottom); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Right = value.X;
                Bottom = value.Y;
            }
        }

        // Constructor
        public Rect(float val)
        {
            this.X = val;
            this.Y = val;
            this.Width = val;
            this.Height = val;
        }

        public Rect(float x, float y, float width, float height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public Rect(float width, float height)
            : this(0f, 0f, width, height)
        {
        }

        public Rect(Vector2 a, Vector2 b)
        {
            X = Math.Min(a.X, b.X);
            Y = Math.Min(a.Y, b.Y);
            Width = Math.Max(a.X, b.X) - X;
            Height = Math.Max(a.Y, b.Y) - Y;
        }

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in Vector2 point)
        {
            return (point.X >= X && point.Y >= Y && point.X < X + Width && point.Y < Y + Height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in Rect rect)
        {
            return (Left <= rect.Left && Top <= rect.Top && Bottom >= rect.Bottom && Right >= rect.Right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(in Rect against)
        {
            return X + Width > against.X && Y + Height > against.Y && X < against.X + against.Width && Y < against.Y + against.Height;
        }

        public Rect OverlapRect(in Rect against)
        {
            var overlapX = X + Width > against.X && X < against.X + against.Width;
            var overlapY = Y + Height > against.Y && Y < against.Y + against.Height;

            Rect r = new Rect();

            if (overlapX)
            {
                r.Left = Math.Max(Left, against.Left);
                r.Width = Math.Min(Right, against.Right) - r.Left;
            }

            if (overlapY)
            {
                r.Top = Math.Max(Top, against.Top);
                r.Height = Math.Min(Bottom, against.Bottom) - r.Top;
            }

            return r;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rect Scale(float scale)
        {
            return new Rect(X * scale, Y * scale, Width * scale, Height * scale);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Rect Scale(in Vector2 scale)
        {
            return new Rect(X * scale.X, Y * scale.Y, Width * scale.X, Height * scale.Y);
        }

#region Object Overrides
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3})", X, Y, Width, Height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj is Rect)
                return Equals((Rect)obj);

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Rect other)
        {
            return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + X.GetHashCode();
            hash = hash * 23 + Y.GetHashCode();
            hash = hash * 23 + Width.GetHashCode();
            hash = hash * 23 + Height.GetHashCode();
            return hash;
        }
#endregion

#region Operators
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect operator *(Rect a, Vector2 scaler)
        {
            return new Rect(a.X * scaler.X, a.Y * scaler.Y, a.Width * scaler.X, a.Height * scaler.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect operator +(Rect a, Vector2 b)
        {
            return new Rect(a.X + b.X, a.Y + b.Y, a.Width, a.Height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect operator -(Rect a, Vector2 b)
        {
            return new Rect(a.X - b.X, a.Y - b.Y, a.Width, a.Height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect operator *(Rect a, float scaler)
        {
            return new Rect(a.X * scaler, a.Y * scaler, a.Width * scaler, a.Height * scaler);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Rect operator /(Rect a, float scaler)
        {
            return new Rect(a.X / scaler, a.Y / scaler, a.Width / scaler, a.Height / scaler);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Rect a, Rect b)
        {
            return (a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Rect a, Rect b)
        {
            return !(a == b);
        }
#endregion

#region Conversion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator RectInt(Rect rect)
        {
            RectInt result = new RectInt();
            result.X = (int)rect.X;
            result.Y = (int)rect.Y;
            result.Width = (int)rect.Width;
            result.Height = (int)rect.Height;
            return result;
        }
#endregion
    }
}