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
// Created: Thursday, August 7, 2014 11:43:55 PM
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
	/// A light to shine on our sad primitives
	/// </summary>
	class Light
		: IDisposable
    {
        #region Value Types.
        /// <summary>
        /// Data for a light to pass to the GPU.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = 64, Pack = 4)]
	    private struct LightData
        {
            /// <summary>
            /// Color of the light.
            /// </summary>
            public Vector4 LightColor;
            /// <summary>
            /// Specular color for the light.
            /// </summary>
            public Vector4 SpecularColor;
            /// <summary>
            /// Position of the light in world space.
            /// </summary>
            public Vector3 LightPosition;
            /// <summary>
            /// Specular highlight power.
            /// </summary>
            public float SpecularPower;
            /// <summary>
            /// Attenuation falloff.
            /// </summary>
            public float Attenuation;
        }
        #endregion

        #region Variables.
        // Flag to indicate that the object was disposed.
		private bool _disposed;
		// The constant buffer to update.
		private readonly GorgonConstantBuffer _buffer;
        // Data to send to the GPU.
	    private LightData _lightData;
		#endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the attenuation falloff for the light.
        /// </summary>
	    public float Attenuation
	    {
	        get
	        {
	            return _lightData.Attenuation;
	        }
	        set
	        {
	            _lightData.Attenuation = value;
                _buffer.Update(ref _lightData);
	        }
	    }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the color for the light.
        /// </summary>
        /// <param name="color">Color to use.</param>
	    public void UpdateColor(ref GorgonColor color)
        {
            _lightData.LightColor = color;
            _buffer.Update(ref _lightData);
        }

        /// <summary>
        /// Function to update the specular power and color of the light.
        /// </summary>
        /// <param name="specColor">Specular color.</param>
        /// <param name="power">Specular power.</param>
	    public void UpdateSpecular(ref GorgonColor specColor, float power)
        {
            _lightData.SpecularColor = specColor;
            _lightData.SpecularPower = power;

            _buffer.Update(ref _lightData);
        }

		/// <summary>
		/// Function to update the world matrix.
		/// </summary>
		/// <param name="position">The new position of the light.</param>
		public void UpdateLightPosition(ref Vector3 position)
		{
		    _lightData.LightPosition = position;
            _buffer.Update(ref _lightData);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="WorldViewProjection"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface to use.</param>
		public Light(GorgonGraphics graphics)
		{
			_buffer = graphics.Buffers.CreateConstantBuffer("LightBuffer",
			                                                new GorgonConstantBufferSettings
			                                                {
				                                                SizeInBytes = DirectAccess.SizeOf<LightData>(),
				                                                Usage = BufferUsage.Default
			                                                });


		    _lightData.Attenuation = 6.0f;
		    _lightData.LightColor = GorgonColor.White;
		    _lightData.LightPosition = Vector3.Zero;
		    _lightData.SpecularColor = GorgonColor.White;
		    _lightData.SpecularPower = 512.0f;

            _buffer.Update(ref _lightData);

			graphics.Shaders.PixelShader.ConstantBuffers[0] = _buffer;
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
				if (_buffer != null)
				{
					_buffer.Dispose();
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
