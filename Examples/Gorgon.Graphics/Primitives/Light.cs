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
using System.Runtime.InteropServices;
using Gorgon.IO;
using Gorgon.Native;
using SlimMath;

namespace Gorgon.Graphics.Example
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
			/// Size of the data structure, in bytes.
			/// </summary>
	        public static readonly int Size = DirectAccess.SizeOf<LightData>();

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
	    private LightData[] _lightData = new LightData[8];
		// Backing store for lights.
		private GorgonDataStream _lightStore;
		#endregion

        #region Methods.
		/// <summary>
		/// Function to update the correct light buffer data to the constant buffer.
		/// </summary>
		/// <param name="index">Index of the light to update.</param>
		private unsafe void UpdateIndex(int index)
		{
			var data = (LightData*)_lightStore.UnsafePointer;

			for (int i = 0; i < _lightData.Length; ++i)
			{
				*(data++) = _lightData[i];
			}

			_buffer.Update(_lightData);
		}

		/// <summary>
		/// Function to update the attenuation falloff for the light.
		/// </summary>
		public void UpdateAttenuation(float value, int index)
		{
			_lightData[index].Attenuation = value;
			UpdateIndex(index);
		}

		/// <summary>
        /// Function to update the color for the light.
        /// </summary>
        /// <param name="color">Color to use.</param>
	    public void UpdateColor(ref GorgonColor color, int index)
        {
            _lightData[index].LightColor = color;
            UpdateIndex(index);
        }

        /// <summary>
        /// Function to update the specular power and color of the light.
        /// </summary>
        /// <param name="specColor">Specular color.</param>
        /// <param name="power">Specular power.</param>
	    public void UpdateSpecular(ref GorgonColor specColor, float power, int index)
        {
            _lightData[index].SpecularColor = specColor;
            _lightData[index].SpecularPower = power;
	        UpdateIndex(index);
        }

		/// <summary>
		/// Function to update the world matrix.
		/// </summary>
		/// <param name="position">The new position of the light.</param>
		public void UpdateLightPosition(ref Vector3 position, int index)
		{
		    _lightData[index].LightPosition = position;
            UpdateIndex(index);
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="WorldViewProjection"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface to use.</param>
		public Light(GorgonGraphics graphics)
		{
			_lightData[0].Attenuation = 6.0f;
			_lightData[0].LightColor = GorgonColor.White;
			_lightData[0].LightPosition = Vector3.Zero;
			_lightData[0].SpecularColor = GorgonColor.White;
			_lightData[0].SpecularPower = 512.0f;

			_buffer = graphics.Buffers.CreateConstantBuffer("LightBuffer",
			                                                new GorgonConstantBufferSettings
			                                                {
				                                                SizeInBytes = LightData.Size * _lightData.Length,
				                                                Usage = BufferUsage.Default
			                                                });
			
			_lightStore = new GorgonDataStream(_buffer.SizeInBytes);
			unsafe
			{
				DirectAccess.ZeroMemory(_lightStore.UnsafePointer, _buffer.SizeInBytes);
				var data = (LightData*)_lightStore.UnsafePointer;
				*data = _lightData[0];
			}

            _buffer.Update(_lightStore);

			graphics.Shaders.PixelShader.ConstantBuffers[1] = _buffer;
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
				if (_lightStore != null)
				{
					_lightStore.Dispose();
				}

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
