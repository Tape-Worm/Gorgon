﻿#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Sunday, December 30, 2012 10:25:22 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using Gorgon.IO;
using Gorgon.Math;
using SlimMath;

namespace Gorgon.Graphics.Example
{
	/// <summary>
	/// An ico sphere object.
	/// </summary>
	class IcoSphere
		: MoveableMesh
	{
		#region Variables.
		// Initial orientation.
		private Matrix _orientation = Matrix.Identity;
		// A list of previously performed splits.
		private readonly Dictionary<Int64, int> _cachedSplits = new Dictionary<long, int>();
		private readonly List<Vector3> _vertices = new List<Vector3>();
		private int _index;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the radius of the sphere.
		/// </summary>
		public float Radius
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to add a vertex to the list.
		/// </summary>
		/// <param name="pos">Position of the vertex.</param>
		/// <returns>The new index.</returns>
		private int AddVertex(Vector3 pos)
		{
			pos.Normalize();
			_vertices.Add(pos);

			return _index++;
		}

		/// <summary>
		/// Function to retrieve a list of the base vertices.
		/// </summary>
		/// <returns>A list of vertices that form the base of the icosphere.</returns>
		private void GetBaseVertices()
		{
			float t = (1.0f + 5.0f.Sqrt()) * 0.5f;

			AddVertex(new Vector3(-1.0f, t, 0));
			AddVertex(new Vector3(1.0f, t, 0));
			AddVertex(new Vector3(-1.0f, -t, 0));
			AddVertex(new Vector3(1.0f, -t, 0));
			AddVertex(new Vector3(0, -1.0f, t));
			AddVertex(new Vector3(0, 1.0f, t));
			AddVertex(new Vector3(0, -1.0f, -t));
			AddVertex(new Vector3(0, 1.0f, -t));
			AddVertex(new Vector3(t, 0, -1.0f));
			AddVertex(new Vector3(t, 0, 1.0f));
			AddVertex(new Vector3(-t, 0, -1.0f));
			AddVertex(new Vector3(-t, 0, 1.0f));
		}

		/// <summary>
		/// Function to return the base indices for the icosphere.
		/// </summary>
		/// <returns>A list of triangle indices.</returns>
		private static List<int[]> GetBaseIndices()
		{
			return new List<int[]>
			       {
				       new[]
				       {
					       0, 11, 5
				       },
				       new[]
				       {
					       0, 5, 1
				       },
				       new[]
				       {
					       0, 1, 7
				       },
				       new[]
				       {
					       0, 7, 10
				       },
				       new[]
				       {
					       0, 10, 11
				       },
				       new[]
				       {
					       1, 5, 9
				       },
				       new[]
				       {
					       5, 11, 4
				       },
				       new[]
				       {
					       11, 10, 2
				       },
				       new[]
				       {
					       10, 7, 6
				       },
				       new[]
				       {
					       7, 1, 8
				       },
				       new[]
				       {
					       3, 9, 4
				       },
				       new[]
				       {
					       3, 4, 2
				       },
					   new []
					   {
						   3, 2, 6
					   },
					   new []
					   {
						   3, 6, 8
					   },
					   new []
					   {
						   3, 8, 9
					   },
					   new []
					   {
						   4, 9, 5
					   },
					   new []
					   {
						   2, 4, 11
					   },
					   new []
					   {
						   6, 2, 10
					   },
					   new []
					   {
						   8, 6, 7
					   },
					   new []
					   {
						   9, 8, 1
					   }
			       };
		}

		/// <summary>
		/// Function to retrieve the middle point between the two indices.
		/// </summary>
		/// <param name="index1">First index.</param>
		/// <param name="index2">Second index.</param>
		/// <returns>The middle index.</returns>
		private int GetMiddlePoint(int index1, int index2)
		{
			Int64 smallestIndex = index1.Min(index2);
			Int64 largestIndex = index1.Max(index2);
			Int64 key = (smallestIndex << 32) + largestIndex;
			int index;

			if (_cachedSplits.TryGetValue(key, out index))
			{
				return index;
			}

			Vector3 vertex1 = _vertices[index1];
			Vector3 vertex2 = _vertices[index2];

			index = AddVertex(new Vector3((vertex1.X + vertex2.X) * 0.5f,
			                              (vertex1.Y + vertex2.Y) * 0.5f,
			                              (vertex1.Z + vertex2.Z) * 0.5f));
			_cachedSplits.Add(key, index);

			return index;
		}

		/// <summary>
		/// Function to repair the unsightly seam that manifests with texture mapping.
		/// </summary>
		/// <param name="vertexList">List of vertices.</param>
		/// <param name="indexList">List of indices.</param>
		private static void FixSeam(List<Vertex3D> vertexList, List<int> indexList)
		{
			var newIndices = new List<int>();
			var corrections = new Dictionary<int, int>();

			for (int i = indexList.Count - 3; i >= 0; i -= 3)
			{
				// see if the texture coordinates appear in counter-clockwise order.
				// If so, the triangle needs to be rectified.
				var v0 = new Vector3(vertexList[indexList[i + 0]].UV, 0);
				var v1 = new Vector3(vertexList[indexList[i + 1]].UV, 0);
				var v2 = new Vector3(vertexList[indexList[i + 2]].UV, 0);

				Vector3 diff1;
				Vector3 diff2;
				Vector3 cross;

				Vector3.Subtract(ref v0, ref v1, out diff1);
				Vector3.Subtract(ref v2, ref v1, out diff2);

				Vector3.Cross(ref diff1, ref diff2, out cross);

				if (cross.Z <= 0)
				{
					newIndices.AddRange(indexList.GetRange(i, 3));
					continue;
				}

				for (int j = i; j < i + 3; ++j)
				{
					int index = indexList[j];
					Vertex3D vertex = vertexList[index];

					if (vertex.UV.X <= 0.8f)
					{
						newIndices.Add(index);
						continue;
					}

					int correctIndex;

					if (corrections.TryGetValue(index, out correctIndex))
					{
						newIndices.Add(correctIndex);
						continue;
					}

					Vector2 UV = vertex.UV;

					UV.X -= 1;
					vertex.UV = UV;

					vertexList.Add(vertex);

					correctIndex = vertexList.Count - 1;
					corrections.Add(index, correctIndex);
					newIndices.Add(correctIndex);
				}
			}

			indexList.Clear();
			indexList.AddRange(newIndices);
		}

		/// <summary>
		/// Function to build the Icosphere. 
		/// </summary>
		/// <param name="graphics">Graphics interface to use.</param>
		/// <param name="radius">Radius of the sphere.</param>
		/// <param name="tesselation">Tessellation factor for the sphere.</param>
		/// <param name="textureCoordinates">Texture coordinate offset and scale.</param>
		private void BuildSphere(GorgonGraphics graphics, float radius, int tesselation, RectangleF textureCoordinates)
		{
			GetBaseVertices();
			List<int[]> indices = GetBaseIndices();

			for (int i = 0; i < tesselation; ++i)
			{
				var subIndices = new List<int[]>();

				foreach (int[] index in indices)
				{
					int index0 = GetMiddlePoint(index[0], index[1]);
					int index1 = GetMiddlePoint(index[1], index[2]);
					int index2 = GetMiddlePoint(index[2], index[0]);

					subIndices.Add(new []
					               {
						               index[0],
						               index0,
						               index2
					               });

					subIndices.Add(new[]
					               {
						               index[1],
						               index1,
						               index0
					               });


					subIndices.Add(new[]
					               {
						               index[2],
						               index2,
						               index1
					               });

					subIndices.Add(new[]
					               {
						               index0,
						               index1,
						               index2
					               });
				}

				indices = subIndices;
				_cachedSplits.Clear();
			}

			// Perform texture coordinate calculations and vertex/normal transformations.
			const float piRecip = 1.0f / (float)System.Math.PI;
			const float pi2Recip = 1.0f / (2.0f * (float)System.Math.PI);

			// Final list.
			var vertexList = new List<Vertex3D>();
			var indexList = new List<int>();

			for (int i = 0; i < _vertices.Count; ++i)
			{
				Vector3 position = _vertices[i];
				Vector3 normal = position;
				Vector2 uv = Vector2.Zero;

				uv.X = (0.5f - position.X.ATan(position.Z) * pi2Recip) * textureCoordinates.Width + textureCoordinates.X;
				uv.Y = (0.5f - position.Y.ASin() * piRecip) * textureCoordinates.Height + textureCoordinates.Y;

				Vector3.Multiply(ref position, radius, out position);
				Vector3.TransformCoordinate(ref position, ref _orientation, out position);
				Vector3.TransformCoordinate(ref normal, ref _orientation, out normal);
				normal.Normalize();

				vertexList.Add(new Vertex3D
				               {
					               Position = new Vector4(position, 1.0f),
					               Normal = normal,
					               UV = uv
				               });
			}

			foreach (int[] index in indices)
			{
				for (int j = 0; j < 3; ++j)
				{
					indexList.Add(index[j]);
				}
			}

			FixSeam(vertexList, indexList);

			unsafe
			{
				using (var vertexData = new GorgonDataStream(vertexList.ToArray()))
				using (var indexData = new GorgonDataStream(indexList.ToArray()))
				{
					VertexCount = vertexList.Count;
					IndexCount = indexList.Count;
					TriangleCount = IndexCount / 3;

					CalculateTangents((Vertex3D*)vertexData.BasePointer, (int*)indexData.BasePointer);

					VertexBuffer = graphics.Buffers.CreateVertexBuffer("IcoSphereVertexBuffer",
						                                                new GorgonBufferSettings
						                                                {
																			SizeInBytes = (int)vertexData.Length,
																			Usage = BufferUsage.Immutable
						                                                },
						                                                vertexData);
					IndexBuffer = graphics.Buffers.CreateIndexBuffer("IcoSphereIndexbuffer",
						                                                new GorgonIndexBufferSettings
						                                                {
																			Usage = BufferUsage.Immutable,
																			Use32BitIndices = true,
																			SizeInBytes = (int)indexData.Length
						                                                },
						                                                indexData);
				}
			}
		}
		#endregion

		#region Constructor/Destructor.
	    /// <summary>
	    /// Initializes a new instance of the <see cref="Sphere" /> class.
	    /// </summary>
	    /// <param name="graphics">Graphics interface to use.</param>
	    /// <param name="radius">Radius of the sphere</param>
	    /// <param name="textureCoordinates">The texture coordinates to apply to the sphere.</param>
	    /// <param name="angle">The angle of rotation, in degrees.</param>
	    /// <param name="subDivisions">The tessellation level for the sphere.</param>
	    public IcoSphere(GorgonGraphics graphics, float radius, RectangleF textureCoordinates, Vector3 angle, int subDivisions = 2)
	    {
		    Radius = radius;
			PrimitiveType = PrimitiveType.TriangleList;
		    Quaternion orientation;
			Quaternion.RotationYawPitchRoll(angle.Y.ToRadians(), angle.X.ToRadians(), angle.Z.ToRadians(), out orientation);
		    Matrix.RotationQuaternion(ref orientation, out _orientation);

			BuildSphere(graphics, radius * 0.5f, subDivisions, textureCoordinates);
		}
		#endregion
	}
}
