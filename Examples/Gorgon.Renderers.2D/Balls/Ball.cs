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
// Created: Monday, February 20, 2012 6:36:59 PM
// 
#endregion

using SlimMath;

namespace Gorgon.Graphics.Example
{
	/// <summary>
	/// A ball for bouncing.
	/// </summary>
	public class Ball
	{
		#region Variables.
		/// <summary>Position of the ball.</summary>
		public Vector2 Position;
		/// <summary>Delta for the position of the ball.</summary>
		public Vector2 PositionDelta;
		/// <summary>Scale of the ball.</summary>
		public float Scale;
		/// <summary>Delta for the scale of the ball</summary>
		public float ScaleDelta;
		/// <summary>Rotation of the ball.</summary>
		public float Rotation;
		/// <summary>Rotation delta for the ball.</summary>
		public float RotationDelta;
		/// <summary>Color of the ball.</summary>
		public GorgonColor Color;
		/// <summary>Opacity of the ball.</summary>
		public float Opacity;
		/// <summary>Opacity of the ball.</summary>
		public float OpacityDelta;
		/// <summary><c>true</c> to use the checker ball, <c>false</c> to use the bubble.</summary>
		public bool Checkered;
		#endregion
	}
}
