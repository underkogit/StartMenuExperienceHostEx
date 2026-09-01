using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;

namespace ExperienceHost.DataAccess.SQL.Structures;

[Serializable]
 
public struct Point2D : IEquatable<Point2D>
{
    public static readonly Point2D Empty;

    private int x;

    private int y;


    public Point2D(int x, int y)
    {
        this.x = x;
        this.y = y;
    }


    public Point2D(Size sz)
    {
        x = sz.Width;
        y = sz.Height;
    }


    public Point2D(int dw)
    {
        x = LowInt16(dw);
        y = HighInt16(dw);
    }


    [Browsable(false)] public readonly bool IsEmpty => x == 0 && y == 0;


    public int X
    {
        readonly get => x;
        set => x = value;
    }


    public int Y
    {
        readonly get => y;
        set => y = value;
    }


    public static implicit operator PointF(Point2D p) => new PointF(p.X, p.Y);


    public static explicit operator Size(Point2D p) => new Size(p.X, p.Y);


    public static Point2D operator +(Point2D pt, Size sz) => Add(pt, sz);


    public static Point2D operator -(Point2D pt, Size sz) => Subtract(pt, sz);


    public static bool operator ==(Point2D left, Point2D right) => left.X == right.X && left.Y == right.Y;


    public static bool operator !=(Point2D left, Point2D right) => !(left == right);


    public static Point2D Add(Point2D pt, Size sz) =>
        new Point2D(unchecked(pt.X + sz.Width), unchecked(pt.Y + sz.Height));


    public static Point2D Subtract(Point2D pt, Size sz) =>
        new Point2D(unchecked(pt.X - sz.Width), unchecked(pt.Y - sz.Height));


    public static Point2D Ceiling(PointF value) =>
        new Point2D(unchecked((int)Math.Ceiling(value.X)), unchecked((int)Math.Ceiling(value.Y)));


    public static Point2D Truncate(PointF value) => new Point2D(unchecked((int)value.X), unchecked((int)value.Y));


    public static Point2D Round(PointF value) =>
        new Point2D(unchecked((int)Math.Round(value.X)), unchecked((int)Math.Round(value.Y)));


    public override readonly bool Equals([NotNullWhen(true)] object? obj) => obj is Point2D && Equals((Point2D)obj);

    public readonly bool Equals(Point2D other) => this == other;


    public override readonly int GetHashCode() => HashCode.Combine(X, Y);


    public void Offset(int dx, int dy)
    {
        unchecked
        {
            X += dx;
            Y += dy;
        }
    }


    public void Offset(Point2D p) => Offset(p.X, p.Y);


    public override readonly string ToString() => $"{{X={X},Y={Y}}}";

    private static short HighInt16(int n) => unchecked((short)((n >> 16) & 0xffff));

    private static short LowInt16(int n) => unchecked((short)(n & 0xffff));
}