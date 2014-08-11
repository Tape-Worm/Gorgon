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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.IO;
using GorgonLibrary.Math;
using GorgonLibrary.Native;
using SlimMath;

namespace GorgonLibrary.Graphics.Example
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
        [StructLayout(LayoutKind.Sequential, Size = 192)]
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
        #endregion

        #region Variables.
        // Flag to indicate that the object was disposed.
		private bool _disposed;
		// The projection * view constant buffer to update.
		private readonly GorgonConstantBuffer _projViewBuffer;
        // World constant buffer to update.
	    private readonly GorgonConstantBuffer _worldBuffer;
		// Size of a matrix, in bytes.
		private readonly int _matrixSize = DirectAccess.SizeOf<Matrix>();
        // View/projection data.
	    private ViewProjectionData _viewProj;
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
        /// <param name="view">The view matrix to assign.</param>
	    public void UpdateViewMatrix(ref Matrix view)
	    {
            _viewProj.View = view;

            Matrix.Multiply(ref _viewProj.View, ref _viewProj.Projection, out _viewProj.ViewProjection);

            _projViewBuffer.Update(ref _viewProj);
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

			Matrix.PerspectiveFovLH(fov.Radians(), aspect, nearZ, farZ, out projection);

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

			graphics.Shaders.VertexShader.ConstantBuffers[0] = _projViewBuffer;
		    graphics.Shaders.VertexShader.ConstantBuffers[1] = _worldBuffer;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			if (disposing)
			{
			    if (_worldBuffer != null)
			    {
			        _worldBuffer.Dispose();
			    }

				if (_projViewBuffer != null)
				{
					_projViewBuffer.Dispose();
				}
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
