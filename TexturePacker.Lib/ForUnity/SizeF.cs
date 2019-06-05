#if UNITY

using System.Globalization;

namespace System.Drawing
{
    public struct SizeF : IEquatable<SizeF>
    {
        public static readonly SizeF Empty = new SizeF();

        public static explicit operator PointF(SizeF size)
        {
            return new PointF(size.Width, size.Height);
        }

        //public static implicit operator SizeF(UnityEngine.Vector2 v)
        //{
        //    return new SizeF(v.x, v.y);
        //}

        public static SizeF operator +(SizeF sz1, SizeF sz2)
        {
            return Add(sz1, sz2);
        }

        public static SizeF operator -(SizeF sz1, SizeF sz2)
        {
            return Subtract(sz1, sz2);
        }

        public static bool operator ==(SizeF sz1, SizeF sz2)
        {
            return sz1.Width == sz2.Width && sz1.Height == sz2.Height;
        }

        public static bool operator !=(SizeF sz1, SizeF sz2)
        {
            return !(sz1 == sz2);
        }

        public float Height { get; set; }

        public bool IsEmpty => Width == 0 && Height == 0;

        public float Width { get; set; }

        public SizeF(SizeF size) : this()
        {
            Width = size.Width;
            Height = size.Height;
        }

        public SizeF(PointF pt) : this()
        {
            Width = pt.X;
            Height = pt.Y;
        }

        public SizeF(float width, float height) : this()
        {
            Width = width;
            Height = height;
        }

        public bool Equals(SizeF other)
        {
            return Width.Equals(other.Width) && Height.Equals(other.Height);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;

            return obj is SizeF && Equals((SizeF)obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public PointF ToPointF()
        {
            return (PointF)this;
        }

        public Size ToSize()
        {
            return Size.Truncate(this);
        }

        public override string ToString()
        {
            return "{Width=" + Width.ToString(CultureInfo.CurrentCulture) + ", Height=" +
                   Height.ToString(CultureInfo.CurrentCulture) + "}";
        }

        public static SizeF Add(SizeF sz1, SizeF sz2)
        {
            return new SizeF(sz1.Width + sz2.Width, sz1.Height + sz2.Height);
        }

        public static SizeF Subtract(SizeF sz1, SizeF sz2)
        {
            return new SizeF(sz1.Width - sz2.Width, sz1.Height - sz2.Height);
        }
    }
}

#endif