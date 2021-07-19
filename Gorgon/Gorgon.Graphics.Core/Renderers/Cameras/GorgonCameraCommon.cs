#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: January 13, 2021 7:35:06 PM
// 
#endregion

using System;
using System.Linq;
using System.Numerics;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Renderers.Cameras
{
    /// <summary>
    /// Flags representing the changes made to a camera.
    /// </summary>
    [Flags()]
    public enum CameraChange
    {
        /// <summary>
        /// No changes.
        /// </summary>
        None = 0,
        /// <summary>
        /// The projection matrix needs refreshing.
        /// </summary>
        Projection = 1,
        /// <summary>
        /// The view matrix needs refreshing.
        /// </summary>
        View = 2,
        /// <summary>
        /// The position of the camera was changed.
        /// </summary>
        Position = 4,
        /// <summary>
        /// The scale of the camera was changed.
        /// </summary>
        Scale = 8,
        /// <summary>
        /// The rotation of the camera was changed.
        /// </summary>
        Rotation = 0x10,
        /// <summary>
        /// All items have changed.
        /// </summary>
        All = Projection | View | Position | Scale | Rotation
    }

    /// <summary>
    /// Common functionality for a camera.
    /// </summary>
    public abstract class GorgonCameraCommon
        : IGorgonGraphicsObject, IGorgonNamedObject
    {
        #region Variables.
        // The raw projection matrix.
        private Matrix4x4 _projectionMatrix = Matrix4x4.Identity;
        // The raw view matrix.
        private Matrix4x4 _viewMatrix = Matrix4x4.Identity;
        // View projection dimensions.
        private DX.Size2F _viewDimensions;
        // Maximum depth.
        private float _maxDepth;
        // Minimum depth value.
        private float _minDepth;
        // The target to use for this camera.
        private WeakReference<GorgonRenderTargetView> _target;
        // The position of the camera.
        private Vector3 _position;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return direct access to the position data by reference.
        /// </summary>
        protected ref Vector3 PositionRef => ref _position;

        /// <summary>
        /// Property to set or return the render target to use for camera calculations.
        /// </summary>
        /// <remarks>
        /// If this value is <b>null</b>, then the first render target on <see cref="GorgonGraphics.RenderTargets"/> is used.
        /// </remarks>
        public GorgonRenderTargetView Target
        {
            get
            {
                GorgonRenderTargetView target = null;
                _target?.TryGetTarget(out target);
                return target;
            }
            set
            {
                GorgonRenderTargetView target = null;

                _target?.TryGetTarget(out target);

                if (target == value)
                {
                    return;
                }

                if (value is null)
                {
                    _target = null;
                    Changes = CameraChange.None;
                    return;
                }

                _target = new WeakReference<GorgonRenderTargetView>(value);
                Changes |= CameraChange.All;
            }
        }

        /// <summary>Property to return what has changed on the camera since the last update.</summary>
        public CameraChange Changes
        {
            get;
            protected set;
        } = CameraChange.All;

        /// <summary>
        /// Property to return graphics instance for this camera.
        /// </summary>
        public GorgonGraphics Graphics
        {
            get;
        }

        /// <summary>
        /// Property to return the horizontal and vertical aspect ratio for the camera view area.
        /// </summary>
        public Vector2 AspectRatio
        {
            get
            {
#pragma warning disable IDE0046 // Convert to conditional expression
                if (GetTarget() is not null)
                {
                    return TargetWidth > TargetHeight
                            ? new Vector2((float)TargetWidth / TargetHeight, 1)
                            : new Vector2(1.0f, (float)TargetHeight / TargetWidth);
                }

                return ViewDimensions.Width > ViewDimensions.Height
                            ? new Vector2(ViewDimensions.Width / ViewDimensions.Height, 1)
                            : new Vector2(1.0f, ViewDimensions.Height / ViewDimensions.Width);
#pragma warning restore IDE0046 // Convert to conditional expression
            }
        }

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
                Changes |= CameraChange.Projection;
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
                Changes |= CameraChange.Projection;
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
                Changes |= CameraChange.Projection;
            }
        }

        /// <summary>
        /// Property to set or return the camera position.
        /// </summary>
        public Vector3 Position
        {
            get => _position;
            set
            {
                if (value == _position)
                {
                    return;
                }

                _position = value;
                Changes |= CameraChange.View | CameraChange.Position;
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
        /// Property to return the width of the current target.
        /// </summary>
        public int TargetWidth => GetTarget()?.Width ?? (int)ViewDimensions.Width;

        /// <summary>
        /// Property to return the height of the current target.
        /// </summary>
        public int TargetHeight => GetTarget()?.Height ?? (int)ViewDimensions.Height;

        /// <summary>
        /// Property to return the name of this object.
        /// </summary>
        public string Name
        {
            get;
        }

        /// <summary>
        /// Property to return the viewable region for the camera.
        /// </summary>
        /// <remarks>
        /// This represents the boundaries of viewable space for the camera using its coordinate system. The upper left of the region corresponds with the upper left of the active render target at minimum 
        /// Z depth, and the lower right of the region corresponds with the lower right of the active render target at minimum Z depth.
        /// </remarks>
        public abstract DX.RectangleF ViewableRegion
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the render target assigned to this camera.
        /// </summary>
        /// <returns>The render target bound to the camera.</returns>
        protected GorgonRenderTargetView GetTarget() => (_target is null) || (!_target.TryGetTarget(out GorgonRenderTargetView target)) ? Graphics.RenderTargets[0] : target;

        /// <summary>
        /// Function to update the view matrix.
        /// </summary>
        /// <param name="viewMatrix">The instance of the matrix to update.</param>
        protected abstract void UpdateViewMatrix(ref Matrix4x4 viewMatrix);

        /// <summary>
        /// Function to update the projection matrix.
        /// </summary>
        /// <param name="projectionMatrix">The instance of the matrix to update.</param>
        protected abstract void UpdateProjectionMatrix(ref Matrix4x4 projectionMatrix);

        /// <summary>
        /// Function to retrieve the view matrix for the camera.
        /// </summary>
        /// <returns>A read only reference to the view matrix.</returns>        
        public ref readonly Matrix4x4 GetViewMatrix()
        {
            if ((Changes & CameraChange.View) == CameraChange.View)
            {
                UpdateViewMatrix(ref _viewMatrix);
                Changes &= ~CameraChange.View;
            }
            
            return ref _viewMatrix;
        }

        /// <summary>
        /// Function to retrieve the projection matrix for the camera type.
        /// </summary>
        /// <returns>A read only reference to the projection matrix.</returns>
        public ref readonly Matrix4x4 GetProjectionMatrix()
        {
            if ((Changes & CameraChange.Projection) == CameraChange.Projection)
            {
                UpdateProjectionMatrix(ref _projectionMatrix);
                Changes &= ~CameraChange.Projection;
            }
            
            return ref _projectionMatrix;
        }

        /// <summary>
        /// Function to project a screen position into camera space.
        /// </summary>
        /// <param name="screenPosition">3D Position on the screen.</param>
        /// <param name="result">The resulting projected position.</param>
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <remarks>Use this to convert a position in screen space into the camera view/projection space.  If the <paramref name="includeViewTransform"/> is set to 
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        public void Project(ref Vector3 screenPosition, out Vector3 result, bool includeViewTransform = true) =>
            Project(ref screenPosition, out result, new DX.Size2(TargetWidth, TargetHeight), includeViewTransform);

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
        public void Project(ref Vector3 screenPosition, out Vector3 result, DX.Size2 targetSize, bool includeViewTransform = true)
        {
            Matrix4x4 transformMatrix;

            if ((Changes & CameraChange.View) == CameraChange.View)
            {
                GetViewMatrix();
                // Reset the flag so we can actually use the camera when rendering.
                Changes |= CameraChange.View;
            }

            if ((Changes & CameraChange.Projection) == CameraChange.Projection)
            {
                GetProjectionMatrix();
                Changes |= CameraChange.Projection;
            }

            if (includeViewTransform)
            {
                var viewProjection = Matrix4x4.Multiply(_viewMatrix, _projectionMatrix);
                Matrix4x4.Invert(viewProjection, out transformMatrix);
            }
            else
            {
                Matrix4x4.Invert(_projectionMatrix, out transformMatrix);
            }

            // Calculate relative position of our screen position.
            var relativePosition = new Vector3((2.0f * screenPosition.X / targetSize.Width) - 1.0f,
                                               1.0f - (screenPosition.Y / targetSize.Height * 2.0f), 0);

            // Transform our screen position by our inverse matrix.    
            result = Vector3.Transform(relativePosition, transformMatrix);
        }

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
        public void Unproject(in Vector3 worldSpacePosition, out Vector3 result, bool includeViewTransform = true) =>
            Unproject(in worldSpacePosition, out result, new DX.Size2(TargetWidth, TargetHeight), includeViewTransform);


        /// <summary>
        /// Function to unproject a world space position into screen space.
        /// </summary>
        /// <param name="worldSpacePosition">A position in world space.</param>
        /// <param name="result">The resulting projected position.</param>
        /// <param name="targetSize">The size of the render target.</param>
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <returns>The unprojected world space coordinates.</returns>
        /// <remarks>Use this to convert a position in world space into the screen space.  If the <paramref name="includeViewTransform"/> is set to 
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        public void Unproject(in Vector3 worldSpacePosition, out Vector3 result, DX.Size2 targetSize, bool includeViewTransform = true)
        {
            if ((Changes & CameraChange.View) == CameraChange.View)
            {
                GetViewMatrix();
                // Reset the flag so we can actually use the camera when rendering.
                Changes |= CameraChange.View;
            }

            if ((Changes & CameraChange.Projection) == CameraChange.Projection)
            {
                GetProjectionMatrix();
                Changes |= CameraChange.Projection;
            }

            Matrix4x4 transformMatrix;

            if (includeViewTransform)
            {
                transformMatrix = Matrix4x4.Multiply(_viewMatrix, _projectionMatrix);
            }
            else
            {
                transformMatrix = _viewMatrix;
            }

            var transform = Vector3.Transform(worldSpacePosition, transformMatrix);

            result = new Vector3((transform.X + 1.0f) * 0.5f * targetSize.Width,
                (1.0f - transform.Y) * 0.5f * targetSize.Height, 0);
        }

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
        public Vector3 Project(Vector3 screenPosition, DX.Size2 targetSize, bool includeViewTransform = true)
        {
            Project(ref screenPosition, out Vector3 result, targetSize, includeViewTransform);

            return result;
        }

        /// <summary>
        /// Function to project a screen position into camera space.
        /// </summary>
        /// <param name="screenPosition">3D Position on the screen.</param>
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <returns>
        /// The projected 3D position of the screen.
        /// </returns>
        /// <remarks>
        /// Use this to convert a position in screen space into the camera view/projection space.  If the <paramref name="includeViewTransform" /> is set to
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.
        /// </remarks>
        public Vector3 Project(Vector3 screenPosition, bool includeViewTransform = true)
        {
            Project(ref screenPosition, out Vector3 result, includeViewTransform);

            return result;
        }

        /// <summary>
        /// Function to unproject a world space position into screen space.
        /// </summary>
        /// <param name="worldSpacePosition">A position in world space.</param>
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <returns>The unprojected world space coordinates.</returns>
        /// <remarks>Use this to convert a position in world space into the screen space.  If the <paramref name="includeViewTransform"/> is set to 
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        public Vector3 Unproject(Vector3 worldSpacePosition, bool includeViewTransform = true)
        {
            Unproject(in worldSpacePosition, out Vector3 result, includeViewTransform);

            return result;
        }

        /// <summary>
        /// Function to unproject a world space position into screen space.
        /// </summary>
        /// <param name="worldSpacePosition">A position in world space.</param>
        /// <param name="targetSize">The size of the render target.</param>
        /// <param name="includeViewTransform">[Optional] <b>true</b> to include the view transformation in the projection calculations, <b>false</b> to only use the projection.</param>
        /// <returns>The unprojected world space coordinates.</returns>
        /// <remarks>Use this to convert a position in world space into the screen space.  If the <paramref name="includeViewTransform"/> is set to 
        /// <b>true</b>, then both the camera position, rotation and zoom will be taken into account when projecting.  If it is set to <b>false</b> only the projection will 
        /// be used to convert the position.  This means if the camera is moved or moving, then the converted screen point will not reflect that.</remarks>
        public Vector3 Unproject(Vector3 worldSpacePosition, DX.Size2 targetSize, bool includeViewTransform = true)
        {
            Unproject(in worldSpacePosition, out Vector3 result, targetSize, includeViewTransform);

            return result;
        }

        /// <summary>
        /// Function to discard pending changes on the camera.
        /// </summary>        
        public void DiscardChanges() => Changes = CameraChange.None;
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="GorgonCameraCommon" /> class.</summary>
        /// <param name="graphics">The graphics interface to use with this object.</param>
        /// <param name="viewDimensions">The view dimensions.</param>
        /// <param name="minDepth">[Optional] The minimum depth value.</param>
        /// <param name="maximumDepth">[Optional] The maximum depth value.</param>
        /// <param name="name">[Optional] The name of the camera.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
        protected GorgonCameraCommon(GorgonGraphics graphics, DX.Size2F viewDimensions, float minDepth, float maximumDepth, string name)
        {
            Graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));
            Name = string.IsNullOrWhiteSpace(name) ? $"{GetType().Name}_{Guid.NewGuid():N}" : name;
            ViewDimensions = viewDimensions;
            MinimumDepth = minDepth;
            MaximumDepth = maximumDepth.Max(1.0f);
        }
        #endregion

    }
}