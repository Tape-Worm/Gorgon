// Triangulator source code by Nick Gravelyn.
// https://github.com/nickgravelyn/Triangulator
//
// Licensed under the MIT license.
// https://github.com/nickgravelyn/Triangulator/blob/master/LICENSE

using System;

namespace GorgonTriangulator
{
	/// <summary>
	/// A basic triangle structure that holds the three vertices that make up a given triangle.
	/// </summary>
	internal struct Triangle
        : IEquatable<Triangle>
	{
		public readonly Vertex A;
		public readonly Vertex B;
		public readonly Vertex C;

		public Triangle(Vertex a, Vertex b, Vertex c)
		{
			A = a;
			B = b;
			C = c;
		}

		public bool ContainsPoint(Vertex point)
		{
			//return true if the point to test is one of the vertices
			if (point.Equals(A) || point.Equals(B) || point.Equals(C))
				return true;

			bool oddNodes = CheckPointToSegment(C, A, point);

		    if (CheckPointToSegment(A, B, point))
				oddNodes = !oddNodes;
			if (CheckPointToSegment(B, C, point))
				oddNodes = !oddNodes;

			return oddNodes;
		}

        public static bool ContainsPoint(Vertex a, Vertex b, Vertex c, Vertex point) => new Triangle(a, b, c).ContainsPoint(point);

        private static bool CheckPointToSegment(Vertex sA, Vertex sB, Vertex point)
		{
			if ((sA.Position.Y < point.Position.Y && sB.Position.Y >= point.Position.Y) ||
				(sB.Position.Y < point.Position.Y && sA.Position.Y >= point.Position.Y))
			{
				float x = 
					sA.Position.X + 
					((point.Position.Y - sA.Position.Y) / 
					(sB.Position.Y - sA.Position.Y) * 
					(sB.Position.X - sA.Position.X));
				
				if (x < point.Position.X)
					return true;
			}

			return false;
		}

        public override bool Equals(object obj) => obj is Triangle triangle ? Equals(triangle) : base.Equals(obj);

        public bool Equals(Triangle obj) =>
            // ReSharper disable ImpureMethodCallOnReadonlyValueField
            obj.A.Equals(A) && obj.B.Equals(B) && obj.C.Equals(C);// ReSharper restore ImpureMethodCallOnReadonlyValueField

        public override int GetHashCode()
		{
			unchecked
			{
				int result = A.GetHashCode();
				result = (result * 397) ^ B.GetHashCode();
				result = (result * 397) ^ C.GetHashCode();
				return result;
			}
		}
	}
}