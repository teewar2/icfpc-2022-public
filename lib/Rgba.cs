using System;

namespace lib;

public record Rgba(int R, int G, int B, int A)
{
    public double DiffTo(Rgba other)
    {
        var rDist = (R - other.R) * (R - other.R);
        var gDist = (G - other.G) * (G - other.G);
        var bDist = (B - other.B) * (B - other.B);
        var aDist = (A - other.A) * (A - other.A);
        var distance = Math.Sqrt(rDist + gDist + bDist + aDist);
        return distance;
    }

    public override string ToString() => $"[{R}, {G}, {B}, {A}]";

    public virtual bool Equals(Rgba? other)
    {
        if (ReferenceEquals(null, other))
            return false;
        if (ReferenceEquals(this, other))
            return true;
        return R == other.R && G == other.G && B == other.B && A == other.A;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(R, G, B, A);
    }
}
