#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: August 20, 2018 4:18:15 PM
// 
#endregion

using DX = SharpDX;
using Gorgon.Core;
using Gorgon.Graphics.Core;

namespace Gorgon.Renderers
{
    /// <summary>
    /// The common interface for a 2D camera for rendering a 2D scene.
    /// </summary>
    public interface IGorgon2DCamera
        : IGorgonNamedObject
    {
        #region Properties.
        /// <summary>
        /// Property to return whether or not the camera needs its data updated.
        /// </summary>
        bool NeedsUpdate
        {
            get;
        }

		/// <summary>
        /// Property to return whether the projection changed or not.
        /// </summary>
		bool ProjectionChanged
        {
            get;
        }

        /// <summary>
        /// Property to return whether the projection changed or not.
        /// </summary>
        bool ViewChanged
        {
            get;
        }

        /// <summary>
        /// Property to return the horizontal and vertical aspect ratio for the camera view area.
        /// </summary>
        DX.Vector2 AspectRatio
        {
            get;
        }

		/// <summary>
		/// Property to set or return the projection view dimensions for the camera.
		/// </summary>
		DX.Size2F ViewDimensions
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
	    /// Property to set or return a flag to indicate that the renderer should automatically update this camera when the render target size changes.
	    /// </summary>
	    /// <remarks>
	    /// <para>
	    /// When this flag is set to <b>true</b>, the renderer will automatically update the <see cref="ViewDimensions"/> for this camera when the current render target is resized (typically in response
	    /// to a window resize event). However, this is not always desirable, and when set to <b>false</b>, the camera <see cref="ViewDimensions"/> will not be resized.
	    /// </para>
	    /// <para>
	    /// If this value is set to <b>false</b>, then it is the responsibility of the developer to update the camera manually when required.
	    /// </para>
	    /// </remarks>
	    bool AllowUpdateOnResize
	    {
	        get;
	        set;
	    }

        /// <summary>
        /// Property to return the width of the first render target in <see cref="GorgonGraphics.RenderTargets"/>.
        /// </summary>
        int TargetWidth
        {
            get;
        }

        /// <summary>
        /// Property to return the width of the first render target in <see cref="GorgonGraphics.RenderTargets"/>.
        /// </summary>
        int TargetHeight
        {
            get;
        }

		/// <summary>
        /// Property to set or return the position of the camera.
        /// </summary>
		DX.Vector3 Position
        {
            get;
            set;
        }

		/// <summary>
        /// Property to return the viewable region for the camera.
        /// </summary>
        /// <remarks>
        /// This represents the boundaries of viewable space for the camera using its coordinate system. The upper left of the region corresponds with the upper left of the active render target at minimum 
        /// Z depth, and the lower right of the region corresponds with the lower right of the active render target at minimum Z depth.
        /// </remarks>
		DX.RectangleF ViewableRegion
        {
            get;
        }

		/// <summary>
		/// Property to return the 2D renderer assigned to this camera.
		/// </summary>
		Gorgon2D Renderer
		{
			get;
		}
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the view matrix for the camera.
        /// </summary>
        /// <param name="view">The view matrix.</param>
        void GetViewMatrix(out DX.Matrix view);

        /// <summary>
        /// Function to retrieve the projection matrix for the camera type.
        /// </summary>
        /// <param name="projection">The project matrix.</param>
        void GetProjectionMatrix(out DX.Matrix projection);

        /// <summary>
        /// Function to project a screen position into camera space.
        /// </summary>
        /// <param name="screenPosition">3D Position on the screen.</param>
        /// <param name="result">The resulting projected position.</param>
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <remarks>Use this to convert a position in screen space into the camera view/projection space.  If the <paramref name="includeViewTransform"/> is set to 
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        void Project(ref DX.Vector3 screenPosition, out DX.Vector3 result, bool includeViewTransform = true);

        /// <summary>
        /// Function to project a screen position into camera space.
        /// </summary>
        /// <param name="screenPosition">3D Position on the screen.</param>
        /// <param name="result">The resulting projected position.</param>
        /// <param name="targetSize">The size of the render target.</param>
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <remarks>Use this to convert a position in screen space into the camera view/projection space.  If the <paramref name="includeViewTransform"/> is set to 
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        void Project(ref DX.Vector3 screenPosition, out DX.Vector3 result, DX.Size2 targetSize, bool includeViewTransform = true);

        /// <summary>
        /// Function to unproject a world space position into screen space.
        /// </summary>
        /// <param name="worldSpacePosition">A position in world space.</param>
        /// <param name="result">The resulting projected position.</param>
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <remarks>Use this to convert a position in world space into the screen space.  If the <paramref name="includeViewTransform"/> is set to 
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        void Unproject(ref DX.Vector3 worldSpacePosition, out DX.Vector3 result, bool includeViewTransform = true);
        
        /// <summary>
	    /// Function to project a screen position into camera space.
	    /// </summary>
	    /// <param name="screenPosition">3D Position on the screen.</param>
	    /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
	    /// <returns>The projected 3D position of the screen.</returns>
	    /// <remarks>Use this to convert a position in screen space into the camera view/projection space.  If the <paramref name="includeViewTransform"/> is set to 
	    /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
	    /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        DX.Vector3 Project(DX.Vector3 screenPosition, bool includeViewTransform = true);

        /// <summary>
        /// Function to project a screen position into camera space.
        /// </summary>
        /// <param name="screenPosition">3D Position on the screen.</param>
        /// <param name="targetSize">The size of the render target</param>.
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <returns>
        /// The projected 3D position of the screen.
        /// </returns>
        /// <remarks>
        /// Use this to convert a position in screen space into the camera view/projection space.  If the <paramref name="includeViewTransform" /> is set to
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.
        /// </remarks>
        DX.Vector3 Project(DX.Vector3 screenPosition, DX.Size2 targetSize, bool includeViewTransform = true);

        /// <summary>
        /// Function to unproject a world space position into screen space.
        /// </summary>
        /// <param name="worldSpacePosition">A position in world space.</param>
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <returns>The unprojected world space coordinates.</returns>
        /// <remarks>Use this to convert a position in world space into the screen space.  If the <paramref name="includeViewTransform"/> is set to 
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        DX.Vector3 Unproject(DX.Vector3 worldSpacePosition, bool includeViewTransform = true);
        #endregion
    }
}
