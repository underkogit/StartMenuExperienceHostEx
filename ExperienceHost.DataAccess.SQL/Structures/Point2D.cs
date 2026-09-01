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


    public static Point2D operator +(Point2D pt, Point2D sz) => Add(pt, sz);


    public static Point2D operator -(Point2D pt, Point2D sz) => Subtract(pt, sz);


    public static bool operator ==(Point2D left, Point2D right) => left.X == right.X && left.Y == right.Y;


    public static bool operator !=(Point2D left, Point2D right) => !(left == right);


    public static Point2D Add(Point2D pt, Point2D sz) =>
        new Point2D(unchecked(pt.X + sz.X), unchecked(pt.Y + sz.Y));


    public static Point2D Subtract(Point2D pt, Point2D sz) =>
        new Point2D(unchecked(pt.X - sz.X), unchecked(pt.Y - sz.Y));


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