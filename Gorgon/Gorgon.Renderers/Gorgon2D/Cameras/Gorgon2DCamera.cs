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
// Created: July 3, 2018 7:47:38 PM
// 
#endregion

using System;
using Gorgon.Core;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A base class for a 2D camera type containing common functionality.
    /// </summary>
    public abstract class Gorgon2DCamera
        : GorgonNamedObject
    {
        #region Variables.
        // View projection dimensions.
        private DX.Size2F _viewDimensions;								
        // Maximum depth.
        private float _maxDepth;										
        // Minimum depth value.
        private float _minDepth;						

        // These have to exposed as protected variables in order to access them by reference (for performance reasons).
        /// <summary>
        /// The raw projection matrix.
        /// </summary>
        protected DX.Matrix ProjectionMatrix = DX.Matrix.Identity;					
        /// <summary>
        /// The raw view matrix.
        /// </summary>
        protected DX.Matrix ViewMatrix = DX.Matrix.Identity;					
        // Projection view matrix.
        internal DX.Matrix ViewProjectionMatrix = DX.Matrix.Identity;				
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return a flag to indicate that the projection matrix needs updating.o;
        /// </summary>
        protected bool NeedsProjectionUpdate
        {
            get;
            set;
        }= true;

        /// <summary>
        /// Property set or return the flag to indicate that the view matrix needs updating.
        /// </summary>
        protected bool NeedsViewUpdate
        {
            get;
            set;
        }= true;

        /// <summary>
        /// Property to return whether or not the camera needs its data updated.
        /// </summary>
        internal bool NeedsUpdate => (NeedsProjectionUpdate) || (NeedsViewUpdate);

        /// <summary>
        /// Property to set or return whether the camera needs its data uploaded to the GPU.
        /// </summary>
        protected internal bool NeedsUpload
        {
            get;
            set;
        }

		/// <summary>
		/// Property to return the horizontal and vertical aspect ratio for the camera view area.
		/// </summary>
		public DX.Vector2 AspectRatio => new DX.Vector2(TargetWidth / (float)TargetHeight, TargetHeight / (float)TargetWidth);

		/// <summary>
		/// Property to set or return the projection view dimensions for the camera.
		/// </summary>
		public DX.Size2F ViewDimensions
		{
			get => _viewDimensions;
		    set
			{
				if (_viewDimensions.Equals(value))
				{
					return;
				}

				_viewDimensions = value;
				NeedsProjectionUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the minimum depth for the camera.
		/// </summary>
		public float MinimumDepth
		{
			get => _minDepth;
		    set
			{
			    if (_minDepth == value)
			    {
			        return;
			    }

				_minDepth = value;
				NeedsProjectionUpdate = true;
			}
		}

		/// <summary>
		/// Property to set or return the maximum depth for the camera.
		/// </summary>
		public float MaximumDepth
		{
			get => _maxDepth;
		    set
			{
				if (value < 1.0f)
				{
					value = 1.0f;
				}

			    if (_maxDepth == value)
			    {
			        return;
			    }

				_maxDepth = value;
				NeedsProjectionUpdate = true;
			}
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
	    public bool AllowUpdateOnResize
	    {
	        get;
	        set;
	    } = true;

		/// <summary>
        /// Property to return the view * projection matrix.
        /// </summary>
        public ref readonly DX.Matrix ViewProjection => ref ViewProjectionMatrix;

		/// <summary>
		/// Property to return the projection matrix for the camera.
		/// </summary>
		public ref readonly DX.Matrix Projection => ref ProjectionMatrix;

		/// <summary>
		/// Property to return the view matrix for the camera.
		/// </summary>
		public ref readonly DX.Matrix View => ref ViewMatrix;

	    /// <summary>
	    /// Property to return the width of the current target.
	    /// </summary>
	    public int TargetWidth => Renderer.Graphics.RenderTargets[0]?.Width ?? 0;

	    /// <summary>
	    /// Property to return the height of the current target.
	    /// </summary>
	    public int TargetHeight => Renderer.Graphics.RenderTargets[0]?.Height ?? 0;

		/// <summary>
		/// Property to return the 2D renderer assigned to this camera.
		/// </summary>
		public Gorgon2D Renderer
		{
			get;
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
        public abstract void Project(ref DX.Vector3 screenPosition, out DX.Vector3 result, bool includeViewTransform = true);

        /// <summary>
        /// Function to unproject a world space position into screen space.
        /// </summary>
        /// <param name="worldSpacePosition">A position in world space.</param>
        /// <param name="result">The resulting projected position.</param>
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <remarks>Use this to convert a position in world space into the screen space.  If the <paramref name="includeViewTransform"/> is set to 
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        public abstract void Unproject(ref DX.Vector3 worldSpacePosition, out DX.Vector3 result, bool includeViewTransform = true);
        
        /// <summary>
	    /// Function to project a screen position into camera space.
	    /// </summary>
	    /// <param name="screenPosition">3D Position on the screen.</param>
	    /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
	    /// <returns>The projected 3D position of the screen.</returns>
	    /// <remarks>Use this to convert a position in screen space into the camera view/projection space.  If the <paramref name="includeViewTransform"/> is set to 
	    /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
	    /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        public abstract DX.Vector3 Project(DX.Vector3 screenPosition, bool includeViewTransform = true);

	    /// <summary>
	    /// Function to unproject a world space position into screen space.
	    /// </summary>
	    /// <param name="worldSpacePosition">A position in world space.</param>
	    /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
	    /// <returns>The unprojected world space coordinates.</returns>
	    /// <remarks>Use this to convert a position in world space into the screen space.  If the <paramref name="includeViewTransform"/> is set to 
	    /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
	    /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
	    public abstract DX.Vector3 Unproject(DX.Vector3 worldSpacePosition, bool includeViewTransform = true);

		/// <summary>
		/// Function to update the view projection matrix for the camera and populate a view/projection constant buffer.
		/// </summary>
		public abstract void Update();
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DCamera" /> class.
        /// </summary>
        /// <param name="renderer">The 2D renderer to use with this camera.</param>
        /// <param name="viewDimensions">The view dimensions.</param>
        /// <param name="name">The name of the camera.</param>
        protected Gorgon2DCamera(Gorgon2D renderer, DX.Size2F viewDimensions, string name)
            : base(name)
        {
            Renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            _viewDimensions = viewDimensions;
        }
        #endregion
    }
}
