#region MIT.
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
// Created: Wednesday, April 25, 2012 12:03:58 PM
// 
#endregion

using SlimMath;

namespace Gorgon.Renderers
{
	/// <summary>
	/// An interface that describes moveable and renderable object.
	/// </summary>
	public interface IMoveable
	{
		/// <summary>
		/// Property to set or return the position of the renderable.
		/// </summary>
		Vector2 Position
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the angle of rotation (in degrees) for a renderable.
		/// </summary>
		float Angle
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the scale of the renderable.
		/// </summary>
		/// <remarks>This property uses scalar values to provide a relative scale.  To set an absolute scale (i.e. pixel coordinates), use the <see cref="P:Gorgon.Renderers.GorgonMoveable.Size">Size</see> property.
		/// <para>Setting this value to a 0 vector will cause undefined behaviour and is not recommended.</para>
		/// </remarks>
		Vector2 Scale
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the anchor point of the renderable.
		/// </summary>
		Vector2 Anchor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the "depth" of the renderable in a depth buffer.
		/// </summary>
		float Depth
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the size of the renderable.
		/// </summary>
		Vector2 Size
		{
			get;
			set;
		}
	}
}
