using System.Runtime.CompilerServices;
using System.Runtime.Serialization;

namespace SharpityEngine
{
    [DataContract]
    public struct RectInt : IEquatable<RectInt>
    {
        // Public
        public static readonly RectInt Zero = new RectInt();
        public static readonly RectInt NormalizedOne = new RectInt(0, 0, 1, 1);

        [DataMember]
        public int X;
        [DataMember]
        public int Y;
        [DataMember]
        public int Width;
        [DataMember]
        public int Height;

        // Properties
        public Point2 Position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Point2(X, Y); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                X = value.X;
                Y = value.Y;
            }
        }

        public Point2 Size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Point2(Width, Height); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Width = value.X;
                Height = value.Y;
            }
        }

        public Point2 Center
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Point2(X + Width / 2, Y + Height / 2); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                X = value.X - Width / 2;
                Y = value.Y - Height / 2;
            }
        }

        public int Area
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Width * Height; }
        }

        public int Left
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return X; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { X = value; }
        }

        public int Right
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return X + Width; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { X = value - Width; }
        }

        public int CenterX
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return X + Width / 2; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { X = value - Width / 2; }
        }

        public int Top
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Y; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Y = value; }
        }

        public int Bottom
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Y + Height; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Y = value - Height; }
        }

        public int CenterY
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return Y + Height / 2; }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { Y = value - Height / 2; }
        }

        public Point2 TopLeft
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Point2(Left, Top); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Left = value.X;
                Top = value.Y;
            }
        }

        public Point2 TopCenter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Point2(CenterX, Top); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                CenterX = value.X;
                Top = value.Y;
            }
        }

        public Point2 TopRight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Point2(Right, Top); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Right = value.X;
                Top = value.Y;
            }
        }

        public Point2 CenterLeft
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Point2(Left, CenterY); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Left = value.X;
                CenterY = value.Y;
            }
        }

        public Point2 CenterRight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Point2(Right, CenterY); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Right = value.X;
                CenterY = value.Y;
            }
        }

        public Point2 BottomLeft
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Point2(Left, Bottom); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Left = value.X;
                Bottom = value.Y;
            }
        }

        public Point2 BottomCenter
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Point2(CenterX, Bottom); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                CenterX = value.X;
                Bottom = value.Y;
            }
        }

        public Point2 BottomRight
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get { return new Point2(Right, Bottom); }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set
            {
                Right = value.X;
                Bottom = value.Y;
            }
        }

        // Constructor
        public RectInt(int val)
        {
            this.X = val;
            this.Y = val;
            this.Width = val;
            this.Height = val;
        }

        public RectInt(int x, int y, int width, int height)
        {
            this.X = x;
            this.Y = y;
            this.Width = width;
            this.Height = height;
        }

        public RectInt(int width, int height) 
            : this(0, 0, width, height) 
        {
        }

        public RectInt(Point2 min, Point2 max)
        {
            X = Math.Min(min.X, max.X);
            Y = Math.Min(min.Y, max.Y);
            Width = Math.Max(min.X, max.X) - X;
            Height = Math.Max(min.Y, max.Y) - Y;
        }

        // Methods
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in Point2 point)
        {
            return (point.X >= X && point.Y >= Y && point.X < X + Width && point.Y < Y + Height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(in RectInt rect)
        {
            return (Left < rect.Left && Top < rect.Top && Bottom > rect.Bottom && Right > rect.Right);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(in RectInt against)
        {
            return X + Width > against.X && Y + Height > against.Y && X < against.X + against.Width && Y < against.Y + against.Height;
        }

        public RectInt OverlapRect(in RectInt against)
        {
            var overlapX = X + Width > against.X && X < against.X + against.Width;
            var overlapY = Y + Height > against.Y && Y < against.Y + against.Height;

            RectInt r = new RectInt();

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
        public RectInt Scale(float scale)
        {
            return new RectInt((int)(X * scale), (int)(Y * scale), (int)(Width * scale), (int)(Height * scale));
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public RectInt Scale(in Point2 scale)
        {
            return new RectInt(X * scale.X, Y * scale.Y, Width * scale.X, Height * scale.Y);
        }

#region Object Overrides
        public override string ToString()
        {
            return string.Format("({0}, {1}, {2}, {3})", X, Y, Width, Height);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj)
        {
            if (obj is RectInt)
                return Equals((RectInt)obj);

            return false;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(RectInt other)
        {
            return X == other.X && Y == other.Y && Width == other.Width && Height == other.Height;
        }

        public override int GetHashCode()
        {
            int hash = 17;
            hash = hash * 23 + X;
            hash = hash * 23 + Y;
            hash = hash * 23 + Width;
            hash = hash * 23 + Height;
            return hash;
        }


#endregion

#region Operators
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectInt operator *(RectInt rect, int scaler)
        {
            return new RectInt(rect.X * scaler, rect.Y * scaler, rect.Width * scaler, rect.Height * scaler);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectInt operator *(RectInt rect, Point2 scaler)
        {
            return new RectInt(rect.X * scaler.X, rect.Y * scaler.Y, rect.Width * scaler.X, rect.Height * scaler.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectInt operator /(RectInt rect, int scaler)
        {
            return new RectInt(rect.X / scaler, rect.Y / scaler, rect.Width / scaler, rect.Height / scaler);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static RectInt operator /(RectInt rect, Point2 scaler)
        {
            return new RectInt(rect.X / scaler.X, rect.Y / scaler.Y, rect.Width / scaler.X, rect.Height / scaler.Y);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(RectInt a, RectInt b)
        {
            return a.X == b.X && a.Y == b.Y && a.Width == b.Width && a.Height == b.Height;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(RectInt a, RectInt b)
        {
            return !(a == b);
        }
#endregion

#region Conversion
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static explicit operator Rect(RectInt rect)
        {
            Rect result = new Rect();
            result.X = rect.X;
            result.Y = rect.Y;
            result.Width = rect.Width;
            result.Height = rect.Height;
            return result;
        }
#endregion
    }
}