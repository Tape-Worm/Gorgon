// Triangulator source code by Nick Gravelyn.
// https://github.com/nickgravelyn/Triangulator
//
// Licensed under the MIT license.
// https://github.com/nickgravelyn/Triangulator/blob/master/LICENSE

using System;
using SharpDX;

namespace GorgonTriangulator
{
    internal struct Vertex
        : IEquatable<Vertex>
    {
        public readonly Vector2 Position;
        public readonly int Index;

        public Vertex(Vector2 position, int index)
        {
            Position = position;
            Index = index;
        }

        public override bool Equals(object obj) => obj is Vertex vertex ? Equals(vertex) : base.Equals(obj);

        public bool Equals(Vertex obj) => obj.Position.Equals(Position) && obj.Index == Index;

        public override int GetHashCode()
        {
            unchecked
            {
                return (Position.GetHashCode() * 397) ^ Index;
            }
        }

        public override string ToString() => $"{Position} ({Index})";
    }
}