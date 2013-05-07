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
// Created: Monday, March 05, 2012 10:31:32 AM
// 
#endregion

using GorgonLibrary.Graphics;
using SlimMath;

namespace GorgonLibrary.Renderers
{	
	/// <summary>
	/// A camera interface.
	/// </summary>
	public interface ICamera
	{
		#region Properties.
		/// <summary>
		/// Property to return the pre-multiplied projection/view matrix.
		/// </summary>
		Matrix ViewProjection
		{
			get;
		}

		/// <summary>
		/// Property to return the projection matrix for the camera.
		/// </summary>
		Matrix Projection
		{
			get;
		}

		/// <summary>
		/// Property to return the view matrix for the camera.
		/// </summary>
		Matrix View
		{
			get;
		}

		/// <summary>
		/// Property to return whether the projection matrix needs updating.
		/// </summary>
		bool NeedsProjectionUpdate
		{
			get;
		}

		/// <summary>
		/// Property to return whether the view matrix needs updating.
		/// </summary>
		bool NeedsViewUpdate
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to calculate the projection matrix.
		/// </summary>
		void CalculateProjectionMatrix();

		/// <summary>
		/// Function to update the projection matrix from the current target.
		/// </summary>
		/// <param name="target">Target to use when updating.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="target"/> parameter is NULL (Nothing in VB.Net).</exception>
		void UpdateFromTarget(GorgonRenderTarget target);

		/// <summary>
		/// Function to update the camera if necessary.
		/// </summary>
		void Update();

		/// <summary>
		/// Function to draw the camera icon.
		/// </summary>
		void Draw();
		#endregion
	}
}
