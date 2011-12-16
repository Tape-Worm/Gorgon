#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Thursday, December 15, 2011 10:43:47 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Primitive type.
	/// </summary>
	public enum PrimitiveType
	{
		/// <summary>
		/// Unknown.
		/// </summary>
		Unknown = 0,
		/// <summary>
		/// A list of points.
		/// </summary>
		PointList = 1,
		/// <summary>
		/// A list of lines.
		/// </summary>
		LineList = 2,
		/// <summary>
		/// A strip of lines.
		/// </summary>
		LineStrip = 3,
		/// <summary>
		/// A list of triangles.
		/// </summary>
		TriangleList = 4,
		/// <summary>
		/// A strip of triangles.
		/// </summary>
		TriangleStrip = 5,
		/// <summary>
		/// A list of lines including adjacency information.
		/// </summary>
		LineListWithAdjacency = 10,
		/// <summary>
		/// A strip of lines including adjacency information.
		/// </summary>
		LineStripWithAdjacency = 11,
		/// <summary>
		/// A list of triangles including adjacency information.
		/// </summary>
		TriangleListWithAdjacency = 12,
		/// <summary>
		/// A strip of triangles including adjacency information.
		/// </summary>
		TriangleStripWithAdjacency = 13,
		/// <summary>
		/// A patch list with 1 control point.
		/// </summary>
		PatchListWith1ControlPoints = 33,
		/// <summary>
		/// A patch list with 2 control points.
		/// </summary>
		PatchListWith2ControlPoints = 34,
		/// <summary>
		/// A patch list with 3 control points.
		/// </summary>
		PatchListWith3ControlPoints = 35,
		/// <summary>
		/// A patch list with 4 control points.
		/// </summary>
		PatchListWith4ControlPoints = 36,
		/// <summary>
		/// A patch list with 5 control points.
		/// </summary>
		PatchListWith5ControlPoints = 37,
		/// <summary>
		/// A patch list with 6 control points.
		/// </summary>
		PatchListWith6ControlPoints = 38,
		/// <summary>
		/// A patch list with 7 control points.
		/// </summary>
		PatchListWith7ControlPoints = 39,
		/// <summary>
		/// A patch list with 8 control points.
		/// </summary>
		PatchListWith8ControlPoints = 40,
		/// <summary>
		/// A patch list with 9 control points.
		/// </summary>
		PatchListWith9ControlPoints = 41,
		/// <summary>
		/// A patch list with 10 control points.
		/// </summary>
		PatchListWith10ControlPoints = 42,
		/// <summary>
		/// A patch list with 11 control points.
		/// </summary>
		PatchListWith11ControlPoints = 43,
		/// <summary>
		/// A patch list with 12 control points.
		/// </summary>
		PatchListWith12ControlPoints = 44,
		/// <summary>
		/// A patch list with 13 control points.
		/// </summary>
		PatchListWith13ControlPoints = 45,
		/// <summary>
		/// A patch list with 14 control points.
		/// </summary>
		PatchListWith14ControlPoints = 46,
		/// <summary>
		/// A patch list with 15 control points.
		/// </summary>
		PatchListWith15ControlPoints = 47,
		/// <summary>
		/// A patch list with 16 control points.
		/// </summary>
		PatchListWith16ControlPoints = 48,
		/// <summary>
		/// A patch list with 17 control points.
		/// </summary>
		PatchListWith17ControlPoints = 49,
		/// <summary>
		/// A patch list with 18 control points.
		/// </summary>
		PatchListWith18ControlPoints = 50,
		/// <summary>
		/// A patch list with 19 control points.
		/// </summary>
		PatchListWith19ControlPoints = 51,
		/// <summary>
		/// A patch list with 20 control points.
		/// </summary>
		PatchListWith20ControlPoints = 52,
		/// <summary>
		/// A patch list with 21 control points.
		/// </summary>
		PatchListWith21ControlPoints = 53,
		/// <summary>
		/// A patch list with 22 control points.
		/// </summary>
		PatchListWith22ControlPoints = 54,
		/// <summary>
		/// A patch list with 23 control points.
		/// </summary>
		PatchListWith23ControlPoints = 55,
		/// <summary>
		/// A patch list with 24 control points.
		/// </summary>
		PatchListWith24ControlPoints = 56,
		/// <summary>
		/// A patch list with 25 control points.
		/// </summary>
		PatchListWith25ControlPoints = 57,
		/// <summary>
		/// A patch list with 26 control points.
		/// </summary>
		PatchListWith26ControlPoints = 58,
		/// <summary>
		/// A patch list with 27 control points.
		/// </summary>
		PatchListWith27ControlPoints = 59,
		/// <summary>
		/// A patch list with 28 control points.
		/// </summary>
		PatchListWith28ControlPoints = 60,
		/// <summary>
		/// A patch list with 29 control points.
		/// </summary>
		PatchListWith29ControlPoints = 61,
		/// <summary>
		/// A patch list with 30 control points.
		/// </summary>
		PatchListWith30ControlPoints = 62,
		/// <summary>
		/// A patch list with 31 control points.
		/// </summary>
		PatchListWith31ControlPoints = 63,
		/// <summary>
		/// A patch list with 32 control points.
		/// </summary>
		PatchListWith32ControlPoints = 64,
	}

	/// <summary>
	/// Manages the input bindings such as the vertex/index buffer, input layout and primitive types.
	/// </summary>
	public class GorgonInputBindings
	{
		#region Variables.
		private PrimitiveType _primitiveType = PrimitiveType.Unknown;		// Primitive type to use.
		private GorgonInputLayout _inputLayout = null;						// The current input layout.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the graphics instance that owns this object.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the primtive type.
		/// </summary>
		public PrimitiveType PrimitiveType
		{
			get
			{
				return _primitiveType;
			}
			set
			{
				if (_primitiveType != value)
				{
					_primitiveType = value;
					Graphics.Context.InputAssembler.PrimitiveTopology = (SharpDX.Direct3D.PrimitiveTopology)value;
				}
			}
		}

		/// <summary>
		/// Property to set or return the input layout.
		/// </summary>
		public GorgonInputLayout Layout
		{
			get
			{
				return _inputLayout;
			}
			set
			{
				if (_inputLayout != value)
				{
					_inputLayout = value;
					if (_inputLayout != null)
						Graphics.Context.InputAssembler.InputLayout = _inputLayout.Convert(Graphics.VideoDevice.D3DDevice);
					else
						Graphics.Context.InputAssembler.InputLayout = null;
				}
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputBindings"/> class.
		/// </summary>
		internal GorgonInputBindings(GorgonGraphics graphics)
		{
			Graphics = graphics;
			PrimitiveType = PrimitiveType.TriangleList;
		}
		#endregion
	}
}
