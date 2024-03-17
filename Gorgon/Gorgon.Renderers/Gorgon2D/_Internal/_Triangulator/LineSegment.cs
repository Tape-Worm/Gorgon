// Triangulator source code by Nick Gravelyn
// https://github.com/nickgravelyn/Triangulator
//
// Licensed under the MIT license
// https://github.com/nickgravelyn/Triangulator/blob/master/LICENSE

using System.Numerics;
using Gorgon.Math;

namespace GorgonTriangulator;

internal struct LineSegment(Vertex a, Vertex b)
{
    public Vertex A = a;
    public Vertex B = b;

    public readonly float? IntersectsWithRay(Vector2 origin, Vector2 direction)
    {
        float largestDistance = (A.Position.X - origin.X).Max(B.Position.X - origin.X) * 2.0f;
        LineSegment raySegment = new(new Vertex(origin, 0), new Vertex(origin + (direction * largestDistance), 0));

        Vector2? intersection = FindIntersection(this, raySegment);
        float? value = null;

        if (intersection is not null)
        {
            value = Vector2.Distance(origin, intersection.Value);
        }

        return value;
    }

    public static Vector2? FindIntersection(LineSegment a, LineSegment b)
    {
        float x1 = a.A.Position.X;
        float y1 = a.A.Position.Y;
        float x2 = a.B.Position.X;
        float y2 = a.B.Position.Y;
        float x3 = b.A.Position.X;
        float y3 = b.A.Position.Y;
        float x4 = b.B.Position.X;
        float y4 = b.B.Position.Y;

        float denom = ((y4 - y3) * (x2 - x1)) - ((x4 - x3) * (y2 - y1));

        float uaNum = ((x4 - x3) * (y1 - y3)) - ((y4 - y3) * (x1 - x3));
        float ubNum = ((x2 - x1) * (y1 - y3)) - ((y2 - y1) * (x1 - x3));

        float ua = uaNum / denom;
        float ub = ubNum / denom;


        // ReSharper disable CompareOfFloatsByEqualityOperator
        if (ua.Clamp(0, 1) != ua || ub.Clamp(0f, 1f) != ub)
        {
            return null;
        }
        // ReSharper restore CompareOfFloatsByEqualityOperator

        return a.A.Position + ((a.B.Position - a.A.Position) * ua);
    }
}
