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

using System.Drawing;
using SlimMath;

namespace Gorgon.Renderers
{
	/// <summary>
	/// A camera interface.
	/// </summary>
	public interface I2DCamera
	{
		#region Properties.
		/// <summary>
		/// Property to return the horizontal and vertical aspect ratio for the camera view area.
		/// </summary>
		Vector2 AspectRatio
		{
			get;
		}

		/// <summary>
		/// Property to return the width of the current target.
		/// </summary>
		int TargetWidth
		{
			get;
		}

		/// <summary>
		/// Property to return the height of the current target.
		/// </summary>
		int TargetHeight
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
		/// Property to set or return the projection view dimensions for the camera.
		/// </summary>
		RectangleF ViewDimensions
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the minimum depth for the camera.
		/// </summary>
		float MinimumDepth
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the maximum depth for the camera.
		/// </summary>
		float MaximumDepth
		{
			get;
			set;
		}

        /// <summary>
        /// Property to return the combined view and projection matrix.
        /// </summary>
	    Matrix ViewProjection
	    {
	        get;
	    }

		/// <summary>
		/// Property to return whether the camera needs updating.
		/// </summary>
		bool NeedsUpdate
		{
			get;
		}

		/// <summary>
		/// Property to set or return whether the dimensions of the camera should be automatically adjusted to match the current render target.
		/// </summary>
		bool AutoUpdate
		{
			get;
			set;
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Function to project a screen position into camera space.
        /// </summary>
        /// <param name="screenPosition">3D Position on the screen.</param>
        /// <param name="result">The resulting projected position.</param>
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <remarks>Use this to convert a position in screen space into the camera view/projection space.  If the <paramref name="includeViewTransform"/> is set to 
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        void Project(ref Vector3 screenPosition, out Vector3 result, bool includeViewTransform = true);

        /// <summary>
        /// Function to unproject a world space position into screen space.
        /// </summary>
        /// <param name="worldSpacePosition">A position in world space.</param>
        /// <param name="result">The resulting projected position.</param>
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <returns>The unprojected world space coordinates.</returns>
        /// <remarks>Use this to convert a position in world space into the screen space.  If the <paramref name="includeViewTransform"/> is set to 
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        void Unproject(ref Vector3 worldSpacePosition, out Vector3 result, bool includeViewTransform = true);
        
        /// <summary>
	    /// Function to project a screen position into camera space.
	    /// </summary>
	    /// <param name="screenPosition">3D Position on the screen.</param>
	    /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
	    /// <returns>The projected 3D position of the screen.</returns>
	    /// <remarks>Use this to convert a position in screen space into the camera view/projection space.  If the <paramref name="includeViewTransform"/> is set to 
	    /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
	    /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
	    Vector3 Project(Vector3 screenPosition, bool includeViewTransform = true);

	    /// <summary>
	    /// Function to unproject a world space position into screen space.
	    /// </summary>
	    /// <param name="worldSpacePosition">A position in world space.</param>
	    /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
	    /// <returns>The unprojected world space coordinates.</returns>
	    /// <remarks>Use this to convert a position in world space into the screen space.  If the <paramref name="includeViewTransform"/> is set to 
	    /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
	    /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
	    Vector3 Unproject(Vector3 worldSpacePosition, bool includeViewTransform = true);

        /// <summary>
		/// Function to draw the camera icon.
		/// </summary>
		void Draw();

		/// <summary>
		/// Function to update the view projection matrix for the camera and populate a view/projection constant buffer.
		/// </summary>
		void Update();
		#endregion
	}
}
