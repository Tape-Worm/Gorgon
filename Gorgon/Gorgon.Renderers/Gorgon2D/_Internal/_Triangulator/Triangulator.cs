// Triangulator source code by Nick Gravelyn.
// https://github.com/nickgravelyn/Triangulator
//
// Licensed under the MIT license.
// https://github.com/nickgravelyn/Triangulator/blob/master/LICENSE
//

using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using Gorgon.Diagnostics;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.Renderers;
using System.Numerics;
using DX = SharpDX;
using Gorgon.Renderers.Geometry;

namespace GorgonTriangulator
{
    /// <summary>
    /// A class exposing methods for triangulating 2D polygons. This is the sole public
    /// class in the entire library; all other classes/structures are intended as internal-only
    /// objects used only to assist in triangulation.
    /// 
    /// This class makes use of the DEBUG conditional and produces quite verbose output when built
    /// in Debug mode. This is quite useful for debugging purposes, but can slow the process down
    /// quite a bit. For optimal performance, build the library in Release mode.
    /// 
    /// The triangulation is also not optimized for garbage sensitive processing. The point of the
    /// library is a robust, yet simple, system for triangulating 2D shapes. It is intended to be
    /// used as part of your content pipeline or at load-time. It is not something you want to be
    /// using each and every frame unless you really don't care about garbage.
    /// </summary>
    internal class Triangulator
    {
        #region Fields
        private readonly IndexableCyclicalLinkedList<Vertex> _polygonVertices = new();
        private readonly IndexableCyclicalLinkedList<Vertex> _earVertices = new();
        private readonly CyclicalList<Vertex> _convexVertices = new();
        private readonly CyclicalList<Vertex> _reflexVertices = new();
        private readonly List<Triangle> _triangles = new();
        // The log used for debug messages.
        private readonly IGorgonLog _log;
        #endregion

        #region Constructor
        /// <summary>
        /// Initializes a new instance of the <see cref="Triangulator"/> class.
        /// </summary>
        /// <param name="log">The log used for debug messages..</param>
        public Triangulator(IGorgonLog log) => _log = log ?? GorgonLog.NullLog;
        #endregion


        #region Public Methods

        /// <summary>
        /// Triangulates a 2D polygon produced the indexes required to render the points as a triangle list.
        /// </summary>
        /// <param name="inputVertices">The polygon vertices in counter-clockwise winding order.</param>
        /// <param name="desiredWindingOrder">The desired output winding order.</param>
        /// <returns>A tuple containing a list of vertices, indices and bounds.</returns>
        public (int[] indices, DX.RectangleF bounds) Triangulate(Gorgon2DVertex[] inputVertices, WindingOrder desiredWindingOrder)
        {
            Log("\nBeginning triangulation...");

            //make sure we have our vertices wound properly
            if (DetermineWindingOrder(inputVertices) == WindingOrder.Clockwise)
            {
                ReverseWindingOrder(inputVertices);
            }

            //clear all of the lists
            _triangles.Clear();
            _polygonVertices.Clear();
            _earVertices.Clear();
            _convexVertices.Clear();
            _reflexVertices.Clear();

            //generate the cyclical list of vertices in the polygon
            for (int i = 0; i < inputVertices.Length; i++)
            {
                _polygonVertices.AddLast(new Vertex(new Vector2(inputVertices[i].Position.X, inputVertices[i].Position.Y), i));
            }

            //categorize all of the vertices as convex, reflex, and ear
            FindConvexAndReflexVertices();
            FindEarVertices();

            //clip all the ear vertices
            while (_polygonVertices.Count > 3 && _earVertices.Count > 0)
            {
                ClipNextEar();
            }

            //if there are still three points, use that for the last triangle
            if (_polygonVertices.Count == 3)
            {
                _triangles.Add(new Triangle(
                    _polygonVertices[0].Value,
                    _polygonVertices[1].Value,
                    _polygonVertices[2].Value));
            }

            //add all of the triangle indices to the output array
            int[] outputIndices = new int[_triangles.Count * 3];

            //move the if statement out of the loop to prevent all the
            //redundant comparisons
            if (desiredWindingOrder == WindingOrder.CounterClockwise)
            {
                for (int i = 0; i < _triangles.Count; i++)
                {
                    Triangle triangle = _triangles[i];
                    outputIndices[(i * 3)] = triangle.A.Index;
                    outputIndices[(i * 3) + 1] = triangle.B.Index;
                    outputIndices[(i * 3) + 2] = triangle.C.Index;
                }
            }
            else
            {
                for (int i = 0; i < _triangles.Count; i++)
                {
                    Triangle triangle = _triangles[i];
                    outputIndices[(i * 3)] = triangle.C.Index;
                    outputIndices[(i * 3) + 1] = triangle.B.Index;
                    outputIndices[(i * 3) + 2] = triangle.A.Index;
                }
            }

            var minMax = new DX.RectangleF();

            // Calculate bounds.
            for (int i = 0; i < inputVertices.Length; ++i)
            {
                ref Vector4 pos = ref inputVertices[i].Position;

                minMax.Left = minMax.Left.Min(pos.X);
                minMax.Top = minMax.Top.Min(pos.Y);
                minMax.Right = minMax.Right.Max(pos.X);
                minMax.Bottom = minMax.Bottom.Max(pos.Y);
            }

            return (outputIndices, minMax);
        }

        #region Not needed - Only works on concave polygons.
        /*/// <summary>
		/// Cuts a hole into a shape.
		/// </summary>
		/// <param name="shapeVerts">An array of vertices for the primary shape.</param>
		/// <param name="holeVerts">An array of vertices for the hole to be cut. It is assumed that these vertices lie completely within the shape verts.</param>
		/// <returns>The new array of vertices that can be passed to Triangulate to properly triangulate the shape with the hole.</returns>
		public Vector2[] CutHoleInShape(Gorgon2DVertex[] shapeVerts, Vector2[] holeVerts)
		{
			Log("\nCutting hole into shape...");

			//make sure the shape vertices are wound counter clockwise and the hole vertices clockwise
			EnsureWindingOrder(shapeVerts, WindingOrder.CounterClockwise);
			EnsureWindingOrder(holeVerts, WindingOrder.Clockwise);

			//clear all of the lists
			_polygonVertices.Clear();
			_earVertices.Clear();
			_convexVertices.Clear();
			_reflexVertices.Clear();

			//generate the cyclical list of vertices in the polygon
			for (int i = 0; i < shapeVerts.Length; i++)
				_polygonVertices.AddLast(new Vertex(shapeVerts[i], i));

			CyclicalList<Vertex> holePolygon = new CyclicalList<Vertex>();
			for (int i = 0; i < holeVerts.Length; i++)
				holePolygon.Add(new Vertex(holeVerts[i], i + _polygonVertices.Count));

#if DEBUG
			StringBuilder vString = new StringBuilder();
			foreach (Vertex v in _polygonVertices)
				vString.Append($"{v}, ");
			Log("Shape Vertices: {0}", vString);

			vString = new StringBuilder();
			foreach (Vertex v in holePolygon)
				vString.Append($"{v}, ");
			Log("Hole Vertices: {0}", vString);
#endif

			FindConvexAndReflexVertices();
			FindEarVertices();

			//find the hole vertex with the largest X value
			Vertex rightMostHoleVertex = holePolygon[0];
			foreach (Vertex v in holePolygon)
				if (v.Position.X > rightMostHoleVertex.Position.X)
					rightMostHoleVertex = v;

			//construct a list of all line segments where at least one vertex
			//is to the right of the rightmost hole vertex with one vertex
			//above the hole vertex and one below
			List<LineSegment> segmentsToTest = new List<LineSegment>();
			for (int i = 0; i < _polygonVertices.Count; i++)
			{
				Vertex a = _polygonVertices[i].Value;
				Vertex b = _polygonVertices[i + 1].Value;

				if ((a.Position.X > rightMostHoleVertex.Position.X || b.Position.X > rightMostHoleVertex.Position.X) &&
					((a.Position.Y >= rightMostHoleVertex.Position.Y && b.Position.Y <= rightMostHoleVertex.Position.Y) ||
					(a.Position.Y <= rightMostHoleVertex.Position.Y && b.Position.Y >= rightMostHoleVertex.Position.Y)))
					segmentsToTest.Add(new LineSegment(a, b));
			}

			//now we try to find the closest intersection point heading to the right from
			//our hole vertex.
			float? closestPoint = null;
			LineSegment closestSegment = new LineSegment();
			foreach (LineSegment segment in segmentsToTest)
			{
				float? intersection = segment.IntersectsWithRay(rightMostHoleVertex.Position, Vector2.UnitX);
			    if (intersection == null)
			    {
			        continue;
			    }

			    if (closestPoint != null && !(closestPoint.Value > intersection.Value))
			    {
			        continue;
			    }

			    closestPoint = intersection;
			    closestSegment = segment;
			}

			//if closestPoint is null, there were no collisions (likely from improper input data),
			//but we'll just return without doing anything else
			if (closestPoint == null)
				return shapeVerts;

			//otherwise we can find our mutually visible vertex to split the polygon
			Vector2 I = rightMostHoleVertex.Position + Vector2.UnitX * closestPoint.Value;
			Vertex P = (closestSegment.A.Position.X > closestSegment.B.Position.X) 
				? closestSegment.A 
				: closestSegment.B;

			//construct triangle MIP
			Triangle mip = new Triangle(rightMostHoleVertex, new Vertex(I, 1), P);

			//see if any of the reflex vertices lie inside of the MIP triangle
			List<Vertex> interiorReflexVertices = new List<Vertex>();
			foreach (Vertex v in _reflexVertices)
				if (mip.ContainsPoint(v))
					interiorReflexVertices.Add(v);

			//if there are any interior reflex vertices, find the one that, when connected
			//to our rightMostHoleVertex, forms the line closest to Vector2.UnitX
			if (interiorReflexVertices.Count > 0)
			{
				float closestDot = -1f;
				foreach (Vertex v in interiorReflexVertices)
				{
					//compute the dot product of the vector against the UnitX
					Vector2 d = Vector2.Normalize(v.Position - rightMostHoleVertex.Position);
					float dot = Vector2.Dot(Vector2.UnitX, d);

					//if this line is the closest we've found
				    if (!(dot > closestDot))
				    {
				        continue;
				    }

				    //save the value and save the vertex as P
				    closestDot = dot;
				    P = v;
				}
			}
            
			//now we just form our output array by injecting the hole vertices into place
			//we know we have to inject the hole into the main array after point P going from
			//rightMostHoleVertex around and then back to P.
			int mIndex = holePolygon.IndexOf(rightMostHoleVertex);
			int injectPoint = _polygonVertices.IndexOf(P);

			Log("Inserting hole at injection point {0} starting at hole vertex {1}.", 
				P,
				rightMostHoleVertex);
			for (int i = mIndex; i <= mIndex + holePolygon.Count; i++)
			{
				Log("Inserting vertex {0} after vertex {1}.", holePolygon[i], _polygonVertices[injectPoint].Value);
				_polygonVertices.AddAfter(_polygonVertices[injectPoint++], holePolygon[i]);
			}
			_polygonVertices.AddAfter(_polygonVertices[injectPoint], P);

#if DEBUG
			vString = new StringBuilder();
			foreach (Vertex v in _polygonVertices)
				vString.Append($"{v}, ");
			Log("New Shape Vertices: {0}\n", vString);
#endif

			//finally we write out the new polygon vertices and return them out
			Vector2[] newShapeVerts = new Vector2[_polygonVertices.Count];
			for (int i = 0; i < _polygonVertices.Count; i++)
				newShapeVerts[i] = _polygonVertices[i].Value.Position;

			return newShapeVerts;
		}*/
        #endregion

        /// <summary>
        /// Ensures that a set of vertices are wound in a particular order, reversing them if necessary.
        /// </summary>
        /// <param name="vertices">The vertices of the polygon.</param>
        /// <param name="windingOrder">The desired winding order.</param>
        /// <returns>A new set of vertices if the winding order didn't match; otherwise the original set.</returns>
        public void EnsureWindingOrder(Gorgon2DVertex[] vertices, WindingOrder windingOrder)
        {
            Log("\nEnsuring winding order of {0}...", windingOrder);
            if (DetermineWindingOrder(vertices) != windingOrder)
            {
                Log("Reversing vertices...");
                ReverseWindingOrder(vertices);
            }
            else
            {
                Log("No reversal needed.");
            }
        }

        /// <summary>
        /// Reverses the winding order for a set of vertices.
        /// </summary>
        /// <param name="vertices">The vertices of the polygon.</param>
        /// <returns>The new vertices for the polygon with the opposite winding order.</returns>
        public void ReverseWindingOrder(Gorgon2DVertex[] vertices)
        {
            Log("\nReversing winding order...");

#if DEBUG
            var vString = new StringBuilder();
            foreach (Gorgon2DVertex v in vertices)
            {
                vString.Append($"{v.Position}, ");
            }

            Log("Original Vertices: {0}", vString);
#endif
            int end = vertices.Length - 1;
            int start = 0;

            while (end > start)
            {
                Gorgon2DVertex temp = vertices[end];
                vertices[end] = vertices[start];
                vertices[start] = temp;
                --end;
                ++start;
            }

#if DEBUG
            vString = new StringBuilder();
            foreach (Gorgon2DVertex v in vertices)
            {
                vString.Append($"{v.Position}, ");
            }

            Log("New Vertices After Reversal: {0}\n", vString);
#endif
        }

        /// <summary>
        /// Determines the winding order of a polygon given a set of vertices.
        /// </summary>
        /// <param name="vertices">The vertices of the polygon.</param>
        /// <returns>The calculated winding order of the polygon.</returns>
        public WindingOrder DetermineWindingOrder(Gorgon2DVertex[] vertices)
        {
            int clockWiseCount = 0;
            int counterClockWiseCount = 0;
            Vector4 p1 = vertices[0].Position;

            for (int i = 1; i < vertices.Length; i++)
            {
                Vector4 p2 = vertices[i].Position;
                Vector4 p3 = vertices[(i + 1) % vertices.Length].Position;

                var e1 = Vector4.Subtract(p1, p2);
                var e2 = Vector4.Subtract(p3, p2);

                if ((e1.X * e2.Y) - (e1.Y * e2.X) >= 0)
                {
                    clockWiseCount++;
                }
                else
                {
                    counterClockWiseCount++;
                }

                p1 = p2;
            }

            return (clockWiseCount > counterClockWiseCount)
                ? WindingOrder.Clockwise
                : WindingOrder.CounterClockwise;
        }

        private void ClipNextEar()
        {
            //find the triangle
            Vertex ear = _earVertices[0].Value;
            Vertex prev = _polygonVertices[_polygonVertices.IndexOf(ear) - 1].Value;
            Vertex next = _polygonVertices[_polygonVertices.IndexOf(ear) + 1].Value;
            _triangles.Add(new Triangle(ear, next, prev));

            //remove the ear from the shape
            _earVertices.RemoveAt(0);
            _polygonVertices.RemoveAt(_polygonVertices.IndexOf(ear));
            Log("\nRemoved Ear: {0}", ear);

            //validate the neighboring vertices
            ValidateAdjacentVertex(prev);
            ValidateAdjacentVertex(next);

            //write out the states of each of the lists
#if DEBUG
            var rString = new StringBuilder();
            foreach (Vertex v in _reflexVertices)
            {
                rString.Append($"{v.Index}, ");
            }

            Log("Reflex Vertices: {0}", rString);

            var cString = new StringBuilder();
            foreach (Vertex v in _convexVertices)
            {
                cString.Append($"{v.Index}, ");
            }

            Log("Convex Vertices: {0}", cString);

            var eString = new StringBuilder();
            foreach (Vertex v in _earVertices)
            {
                eString.Append($"{v.Index}, ");
            }

            Log("Ear Vertices: {0}", eString);
#endif
        }

        private void ValidateAdjacentVertex(Vertex vertex)
        {
            Log("Validating: {0}...", vertex);

            if (_reflexVertices.Contains(vertex))
            {
                if (IsConvex(vertex))
                {
                    _reflexVertices.Remove(vertex);
                    _convexVertices.Add(vertex);
                    Log("Vertex: {0} now convex", vertex);
                }
                else
                {
                    Log("Vertex: {0} still reflex", vertex);
                }
            }

            if (!_convexVertices.Contains(vertex))
            {
                return;
            }

            bool wasEar = _earVertices.Contains(vertex);
            bool isEar = IsEar(vertex);

            if (wasEar && !isEar)
            {
                _earVertices.Remove(vertex);
                Log("Vertex: {0} no longer ear", vertex);
            }
            else if (!wasEar && isEar)
            {
                _earVertices.AddFirst(vertex);
                Log("Vertex: {0} now ear", vertex);
            }
            else
            {
                Log("Vertex: {0} still ear", vertex);
            }
        }

        private void FindConvexAndReflexVertices()
        {
            for (int i = 0; i < _polygonVertices.Count; i++)
            {
                Vertex v = _polygonVertices[i].Value;

                if (IsConvex(v))
                {
                    _convexVertices.Add(v);
                    Log("Convex: {0}", v);
                }
                else
                {
                    _reflexVertices.Add(v);
                    Log("Reflex: {0}", v);
                }
            }
        }

        private void FindEarVertices()
        {
            for (int i = 0; i < _convexVertices.Count; i++)
            {
                Vertex c = _convexVertices[i];

                if (!IsEar(c))
                {
                    continue;
                }

                _earVertices.AddLast(c);
                Log("Ear: {0}", c);
            }
        }

        private bool IsEar(Vertex c)
        {
            Vertex p = _polygonVertices[_polygonVertices.IndexOf(c) - 1].Value;
            Vertex n = _polygonVertices[_polygonVertices.IndexOf(c) + 1].Value;

            Log("Testing vertex {0} as ear with triangle {1}, {0}, {2}...", c, p, n);

            foreach (Vertex t in _reflexVertices)
            {
                if (t.Equals(p) || t.Equals(c) || t.Equals(n))
                {
                    continue;
                }

                if (!Triangle.ContainsPoint(p, c, n, t))
                {
                    continue;
                }

                Log("\tTriangle contains vertex {0}...", t);
                return false;
            }

            return true;
        }

        private bool IsConvex(Vertex c)
        {
            Vertex p = _polygonVertices[_polygonVertices.IndexOf(c) - 1].Value;
            Vertex n = _polygonVertices[_polygonVertices.IndexOf(c) + 1].Value;

            var d1 = Vector2.Normalize(c.Position - p.Position);
            var d2 = Vector2.Normalize(n.Position - c.Position);
            var n2 = new Vector2(-d2.Y, d2.X);

            return (Vector2.Dot(d1, n2) <= 0f);
        }

        [Conditional("DEBUG")]
        private void Log(string format, params object[] parameters)
        {
            if (_log == GorgonLog.NullLog)
            {
                return;
            }

            _log?.Print(format, LoggingLevel.Verbose, parameters);
        }

        #endregion
    }

    /// <summary>
    /// Specifies a desired winding order for the shape vertices.
    /// </summary>
    internal enum WindingOrder
    {
        /// <summary>
        /// Clockwise winding for vertices.
        /// </summary>
		Clockwise,
        /// <summary>
        /// Counterclockwise winding for vertices.
        /// </summary>
		CounterClockwise
    }
}
