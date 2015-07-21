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
// Created: Thursday, August 7, 2014 10:29:17 PM
// 
#endregion

using System;
using System.Runtime.InteropServices;
using Gorgon.Math;
using Gorgon.Native;
using SlimMath;

namespace Gorgon.Graphics.Example
{
	/// <summary>
	/// The world/view/projection matrix.
	/// </summary>
	class WorldViewProjection
		: IDisposable
    {
        #region Value Types
        /// <summary>
        /// View/projection matrices.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = 208)]
	    private struct ViewProjectionData
        {
            /// <summary>
            /// The view matrix.
            /// </summary>
            public Matrix View;
            /// <summary>
            /// The projection matrix.
            /// </summary>
            public Matrix Projection;
            /// <summary>
            /// The view * project matrix.
            /// </summary>
            public Matrix ViewProjection;
        }

		/// <summary>
		/// Camera data.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Size =  48, Pack = 4)]
		private struct CameraData
		{
			/// <summary>
			/// Camera position.
			/// </summary>
			public Vector3 CameraPosition;
			/// <summary>
			/// Camera look at target.
			/// </summary>
			public Vector3 CameraLookAt;
			/// <summary>
			/// Camera up vector.
			/// </summary>
			public Vector3 CameraUp;
		}
        #endregion

        #region Variables.
        // Flag to indicate that the object was disposed.
		private bool _disposed;
		// The projection * view constant buffer to update.
		private readonly GorgonConstantBuffer _projViewBuffer;
        // World constant buffer to update.
	    private readonly GorgonConstantBuffer _worldBuffer;
		// Camera constant buffer.
		private readonly GorgonConstantBuffer _cameraBuffer;
        // View/projection data.
	    private ViewProjectionData _viewProj;
		// Camera data.
		private CameraData _camData;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the world matrix.
		/// </summary>
		/// <param name="world">World matrix to update.</param>
		public void UpdateWorldMatrix(ref Matrix world)
		{
            _worldBuffer.Update(ref world);
		}

        /// <summary>
        /// Function to update the view matrix.
        /// </summary>
        /// <param name="eyePosition">Position of the eye.</param>
        /// <param name="lookAt">Point in space to look at.</param>
        /// <param name="up">The current up vector.</param>
	    public void UpdateViewMatrix(ref Vector3 eyePosition, ref Vector3 lookAt, ref Vector3 up)
        {
	        Matrix.LookAtLH(ref eyePosition, ref lookAt, ref up, out _viewProj.View);
            Matrix.Multiply(ref _viewProj.View, ref _viewProj.Projection, out _viewProj.ViewProjection);

	        _camData.CameraPosition = eyePosition;
	        _camData.CameraLookAt = lookAt;
	        _camData.CameraUp = up;
			
            _projViewBuffer.Update(ref _viewProj);
			_cameraBuffer.Update(ref _camData);
	    }

		/// <summary>
		/// Function to update the screen width/screen height.
		/// </summary>
		/// <param name="fov">Field of view, in degrees.</param>
		/// <param name="screenWidth">Width of the screen.</param>
		/// <param name="screenHeight">Height of the screen.</param>
		/// <param name="nearZ">Near Z plane.</param>
		/// <param name="farZ">Far Z plane.</param>
		public void UpdateProjection(float fov, float screenWidth, float screenHeight, float nearZ = 0.25f, float farZ = 1000.0f)
		{
			float aspect = screenWidth / screenHeight;
			Matrix projection;

			Matrix.PerspectiveFovLH(fov.ToRadians(), aspect, nearZ, farZ, out projection);

		    _viewProj.Projection = projection;

		    Matrix.Multiply(ref _viewProj.View, ref _viewProj.Projection, out _viewProj.ViewProjection);

            _projViewBuffer.Update(ref _viewProj);
		}
		#endregion

		#region Constructor.

		/// <summary>
		/// Initializes a new instance of the <see cref="WorldViewProjection"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface to use.</param>
		public WorldViewProjection(GorgonGraphics graphics)
		{
			Matrix dummy = Matrix.Identity;

			_projViewBuffer = graphics.Buffers.CreateConstantBuffer("WVPBuffer", 
			                                                new GorgonConstantBufferSettings
			                                                {
				                                                SizeInBytes = DirectAccess.SizeOf<ViewProjectionData>(),
				                                                Usage = BufferUsage.Default
			                                                });

		    _viewProj = new ViewProjectionData
		                {
		                    Projection = Matrix.Identity,
		                    View = Matrix.Identity,
		                    ViewProjection = Matrix.Identity
		                };

            _projViewBuffer.Update(ref _viewProj);

		    _worldBuffer = graphics.Buffers.CreateConstantBuffer("WorldBuffer", ref dummy, BufferUsage.Default);

			_camData.CameraLookAt = new Vector3(0, 0, -1.0f);
			_camData.CameraUp = new Vector3(0, 1, 0);
			_cameraBuffer = graphics.Buffers.CreateConstantBuffer("CameraBuffer", ref _camData, BufferUsage.Default);

			graphics.Shaders.VertexShader.ConstantBuffers[0] = _projViewBuffer;
		    graphics.Shaders.VertexShader.ConstantBuffers[1] = _worldBuffer;
			graphics.Shaders.PixelShader.ConstantBuffers[0] = _cameraBuffer;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
				_cameraBuffer?.Dispose();

				_worldBuffer?.Dispose();

				_projViewBuffer?.Dispose();
			}

			_disposed = true;
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(false);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
