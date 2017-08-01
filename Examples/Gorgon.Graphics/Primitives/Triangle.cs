#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Thursday, August 7, 2014 11:03:20 PM
// 
#endregion

using DX = SharpDX;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using Gorgon.Graphics.Core;
using Gorgon.Native;

namespace Gorgon.Graphics.Example
{
	class Triangle
		: MoveableMesh
	{
        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="Triangle" /> class.
        /// </summary>
        /// <param name="graphics">The graphics interface.</param>
        /// <param name="point1">The 1st point in the triangle.</param>
        /// <param name="point2">The 2nd point in the triangle.</param>
        /// <param name="point3">The 3rd point in the triangle.</param>
        public Triangle(GorgonGraphics graphics, Vertex3D point1, Vertex3D point2, Vertex3D point3)
            : base(graphics)
	    {
	        PrimitiveType = D3D.PrimitiveTopology.TriangleList;
	        VertexCount = 3;
	        IndexCount = 3;
	        TriangleCount = 1;

	        point1.Tangent = new DX.Vector4(1.0f, 0, 0, 1.0f);
	        point2.Tangent = new DX.Vector4(1.0f, 0, 0, 1.0f);
	        point3.Tangent = new DX.Vector4(1.0f, 0, 0, 1.0f);

	        unsafe
	        {
	            Vertex3D* points = stackalloc Vertex3D[3];
	            int* indices = stackalloc int[3];

	            points[0] = point1;
	            points[1] = point2;
	            points[2] = point3;
	            indices[0] = 0;
	            indices[1] = 1;
	            indices[2] = 2;

	            VertexBuffer = new GorgonVertexBuffer("TriVB",
	                                                  graphics,
	                                                  new GorgonVertexBufferInfo
	                                                  {
	                                                      Usage = D3D11.ResourceUsage.Immutable,
	                                                      SizeInBytes = DirectAccess.SizeOf<Vertex3D>() * 3
	                                                  },
	                                                  new GorgonPointerAlias(points, DirectAccess.SizeOf<Vertex3D>() * 3));

	            IndexBuffer = new GorgonIndexBuffer("TriIB",
	                                                graphics,
	                                                new GorgonIndexBufferInfo
	                                                {
	                                                    Usage = D3D11.ResourceUsage.Dynamic,
	                                                    Use16BitIndices = false,
	                                                    IndexCount = 3
	                                                },
	                                                new GorgonPointerAlias(indices, sizeof(int) * 3));
	        }
	    }
	    #endregion
	}
}
