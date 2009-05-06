#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Friday, July 04, 2008 2:06:25 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a single interface for vertex and pixel shaders.
	/// </summary>
	public class DualShader
		: Shader
	{
		#region Variables.
		private PixelShader _pixelShader = null;			// Pixel shader to use.
		private VertexShader _vertexShader = null;			// Vertex shader to use.
		private DefaultPixelShader _defaultPS = null;		// Default pixel shader.
		private DefaultVertexShader _defaultVS = null;		// Default vertex shader.
		private bool _disposed = false;						// Flag to indicate that we're already disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether this shader is a binary (no source code) shader.
		/// </summary>
		public override bool IsBinary
		{
			get
			{
				bool result = false;

				if (VertexShader != null)
					result = VertexShader.IsBinary;
				if (PixelShader != null)
					result = (result) && (PixelShader.IsBinary);

				return result;
			}
			protected set
			{				
			}
		}

		/// <summary>
		/// Property to return the source code for the shaders.
		/// </summary>
		public override string ShaderSource
		{
			get
			{
				StringBuilder result = new StringBuilder();

				if ((VertexShader != null) && (!VertexShader.IsBinary))
				{
					result.Append("// Vertex shader:\n");
					result.Append(VertexShader.ShaderSource);
				}

				if ((PixelShader != null) && (!PixelShader.IsBinary))
				{
					if (result.Length > 0)
						result.Append("\n\n");
					result.Append("// Pixel shader:\n");
					result.Append(PixelShader.ShaderSource);
				}

				return result.ToString();
			}
			set
			{
				throw new NotSupportedException();
			}
		}

		/// <summary>
		/// Property to return whether the shader has been compiled yet or not.
		/// </summary>
		/// <value></value>
		public override bool IsCompiled
		{
			get
			{
				bool result = false;

				if (VertexShader != null)
					result = VertexShader.IsCompiled;
				if (PixelShader != null)
					result = (result) && (PixelShader.IsCompiled);

				return result;
			}
		}

		/// <summary>
		/// Property to set or return the pixel shader to use.
		/// </summary>
		/// <remarks>The user should try and ensure the pixel and vertex shaders are the same version, the default shader will automatically 
		/// default to the highest version supported by DirectX 9 and the video card.  If there's a version mismatch, for example, a vs_2_0 vertex
		/// shader and a ps_3_0 shader probably won't work as expected.</remarks>
		public PixelShader PixelShader
		{
			get
			{
				return _pixelShader;
			}
			set
			{
				_pixelShader = value;

				if ((_pixelShader != null) && (!_pixelShader.IsCompiled))
					throw new ArgumentException("The pixel shader must be compiled.");
				GetParameters();
			}
		}

		/// <summary>
		/// Property to set or return the vertex shader to use.
		/// </summary>
		/// <remarks>The user should try and ensure the pixel and vertex shaders are the same version, the default shader will automatically 
		/// default to the highest version supported by DirectX 9 and the video card.  If there's a version mismatch, for example, a vs_2_0 vertex
		/// shader and a ps_3_0 shader probably won't work as expected.</remarks>
		public VertexShader VertexShader
		{
			get
			{
				return _vertexShader;
			}
			set
			{
				_vertexShader = value;

				if ((_vertexShader != null) && (!_vertexShader.IsCompiled))
					throw new ArgumentException("The vertex shader must be compiled.");
				GetParameters();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);

			if (!_disposed)
			{
				if (disposing)
				{
					if (_defaultPS != null)
						_defaultPS.Dispose();
					if (_defaultVS != null)
						_defaultVS.Dispose();
				}
				_defaultPS = null;
				_defaultVS = null;
				_disposed = true;
			}
		}

		/// <summary>
		/// Function to retrieve the parameters for a shader.
		/// </summary>
		protected override void GetParameters()
		{
			Parameters.Clear();

			if (VertexShader != null)
			{
				foreach (ConstantShaderParameter param in VertexShader.Parameters)
					Parameters.Add(param);
			}

			if (PixelShader != null)
			{
				foreach (ConstantShaderParameter param in PixelShader.Parameters)
					Parameters.Add(param);
			}
		}

		/// <summary>
		/// Function called before the rendering begins with this shader.
		/// </summary>
		protected override void OnRenderBegin()
		{
			if ((VertexShader == null) && (PixelShader == null))
				throw new InvalidOperationException("DualShader needs a pixel and vertex shader before rendering.");

			if (VertexShader != null)
				((IShaderRenderer)VertexShader).Begin();
			if (PixelShader != null)
				((IShaderRenderer)PixelShader).Begin();
		}

		/// <summary>
		/// Function called when rendering with this shader.
		/// </summary>
		/// <param name="primitiveStyle">Type of primitive to render.</param>
		/// <param name="vertexStart">Starting vertex to render.</param>
		/// <param name="vertexCount">Number of vertices to render.</param>
		/// <param name="indexStart">Starting index to render.</param>
		/// <param name="indexCount">Number of indices to render.</param>
		protected override void OnRender(PrimitiveStyle primitiveStyle, int vertexStart, int vertexCount, int indexStart, int indexCount)
		{
			Gorgon.Renderer.DrawCachedTriangles(primitiveStyle, vertexStart, vertexCount, indexStart, indexCount);
		}

		/// <summary>
		/// Function called after the rendering ends with this shader.
		/// </summary>
		protected override void OnRenderEnd()
		{
			if (VertexShader != null)
				((IShaderRenderer)VertexShader).End();
			if (PixelShader != null)
				((IShaderRenderer)PixelShader).End();
		}

		/// <summary>
		/// Function called when the device is in a lost state.
		/// </summary>
		public override void DeviceLost()
		{
			if (VertexShader != null)
				VertexShader.DeviceLost();
			if (PixelShader != null)
				PixelShader.DeviceLost();
		}

		/// <summary>
		/// Function called when the device is reset.
		/// </summary>
		public override void DeviceReset()
		{
			if (VertexShader != null)
				VertexShader.DeviceReset();
			if (PixelShader != null)
				PixelShader.DeviceReset();
		}

		/// <summary>
		/// Function to reset this shaders vertex and pixel shaders to the default pixel and vertex shaders.
		/// </summary>
		/// <remarks>The default vertex and pixel shaders automatically set their target profiles to the highest supported by the video card and Direct X 9.  Please note that this does NOT mean that it will be set a SM 4.0 profile, that profile is for DirectX 10 and later.</remarks>
		public void Reset()
		{
			PixelShader = _defaultPS;
			VertexShader = _defaultVS;
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="DualShader"/> class.
		/// </summary>
		/// <param name="name">The name of the shader.</param>
		/// <param name="pixelShader">The pixel shader to use.</param>
		/// <param name="vertexShader">The vertex shader to use.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		public DualShader(string name, PixelShader pixelShader, VertexShader vertexShader)
			: base(name)
		{
			_defaultPS = new DefaultPixelShader(name + ".Default.PixelShader");
			_defaultVS = new DefaultVertexShader(name + ".Default.VertexShader");
			PixelShader = _defaultPS;
			VertexShader = _defaultVS;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DualShader"/> class.
		/// </summary>
		/// <param name="name">Name of the shader..</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		public DualShader(string name)
			: this(name, null, null)
		{
		}
		#endregion
	}
}
