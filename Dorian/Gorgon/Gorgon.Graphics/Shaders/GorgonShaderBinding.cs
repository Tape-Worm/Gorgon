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
// Created: Tuesday, January 31, 2012 8:21:21 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Diagnostics;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Used to manage shader bindings and shader buffers.
	/// </summary>
	public sealed class GorgonShaderBinding
	{
		#region Variables.
		private GorgonGraphics _graphics = null;		// Graphics interface.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the current vertex shader states.
		/// </summary>
		public GorgonVertexShaderState VertexShader
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the current vertex shader states.
		/// </summary>
		public GorgonPixelShaderState PixelShader
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function clean up any resources within this interface.
		/// </summary>
		internal void CleanUp()
		{
			if (PixelShader != null)
				PixelShader.Dispose();
			if (VertexShader != null)
				VertexShader.Dispose();

			PixelShader = null;
			VertexShader = null;
		}

		/// <summary>
		/// Function to re-seat a texture after it's been altered.
		/// </summary>
		/// <param name="texture">Texture to re-seat.</param>
		internal void Reseat(GorgonTexture texture)
		{
			PixelShader.Textures.ReSeat(texture);
			VertexShader.Textures.ReSeat(texture);
		}

		/// <summary>
		/// Function to unbind a texture from all shaders.
		/// </summary>
		/// <param name="texture">Texture to unbind.</param>
		internal void Unbind(GorgonTexture texture)
		{
			PixelShader.Textures.Unbind(texture);
			VertexShader.Textures.Unbind(texture);
		}

		/// <summary>
		/// Function to create a constant buffer.
		/// </summary>
		/// <param name="size">Size of the buffer, in bytes.</param>
		/// <param name="allowCPUWrite">TRUE to allow the CPU to write to the buffer, FALSE to disallow.</param>
		/// <returns>A new constant buffer.</returns>
		public GorgonConstantBuffer CreateConstantBuffer(int size, bool allowCPUWrite)
		{
			return CreateConstantBuffer(size, allowCPUWrite, null);
		}

		/// <summary>
		/// Function to create a constant buffer.
		/// </summary>
		/// <param name="size">Size of the buffer, in bytes.</param>
		/// <param name="allowCPUWrite">TRUE to allow the CPU to write to the buffer, FALSE to disallow.</param>
		/// <param name="stream">Stream used to initialize the buffer.</param>
		/// <returns>A new constant buffer.</returns>
		public GorgonConstantBuffer CreateConstantBuffer(int size, bool allowCPUWrite, GorgonDataStream stream)
		{			
			GorgonConstantBuffer buffer = new GorgonConstantBuffer(_graphics, size, allowCPUWrite);
			buffer.Initialize(stream);
			_graphics.AddTrackedObject(buffer);
			return buffer;
		}

		/// <summary>
		/// Function to create a constant buffer and initializes it with data.
		/// </summary>
		/// <typeparam name="T">Type of data to pass to the constant buffer.</typeparam>
		/// <param name="value">Value to write to the buffer</param>
		/// <param name="allowCPUWrite">TRUE to allow the CPU to write to the buffer, FALSE to disallow.</param>
		/// <returns>A new constant buffer.</returns>
		public GorgonConstantBuffer CreateConstantBuffer<T>(T value, bool allowCPUWrite)
			where T : struct
		{
			using (GorgonDataStream stream = GorgonDataStream.ValueTypeToStream<T>(value))
			{
				GorgonConstantBuffer buffer = new GorgonConstantBuffer(_graphics, (int)stream.Length, allowCPUWrite);
				buffer.Initialize(stream);

				_graphics.AddTrackedObject(buffer);
				return buffer;
			}
		}

		/// <summary>
		/// Function to create a vertex shader.
		/// </summary>
		/// <param name="name">Name of the vertex shader.</param>
		/// <param name="entryPoint">Entry point for the shader.</param>
		/// <param name="sourceCode">Source code for the shader.</param>
		/// <param name="debug">TRUE to include debug information, FALSE to exclude.</param>
		/// <returns>A new vertex shader.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> or <paramref name="entryPoint"/> parameters are empty strings.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the name or entryPoint parameters are NULL (Nothing in VB.Net).</exception>
		public GorgonVertexShader CreateVertexShader(string name, string entryPoint, string sourceCode, bool debug)
		{
			GorgonVertexShader shader = null;

			GorgonDebug.AssertParamString(name, "name");
			GorgonDebug.AssertParamString(entryPoint, "entryPoint");

			shader = new GorgonVertexShader(_graphics, name, entryPoint);
			shader.SourceCode = sourceCode;
			shader.Compile(debug);
			_graphics.AddTrackedObject(shader);

			return shader;
		}

		/// <summary>
		/// Function to create a pixel shader.
		/// </summary>
		/// <param name="name">Name of the pixel shader.</param>
		/// <param name="entryPoint">Entry point for the shader.</param>
		/// <param name="sourceCode">Source code for the shader.</param>
		/// <param name="debug">TRUE to include debug information, FALSE to exclude it.</param>
		/// <returns>A new pixel shader.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> or <paramref name="entryPoint"/> parameters are empty strings.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the name or entryPoint parameters are NULL (Nothing in VB.Net).</exception>
		public GorgonPixelShader CreatePixelShader(string name, string entryPoint, string sourceCode, bool debug)
		{
			GorgonPixelShader shader = null;

			GorgonDebug.AssertParamString(name, "name");
			GorgonDebug.AssertParamString(entryPoint, "entryPoint");

			shader = new GorgonPixelShader(_graphics, name, entryPoint);
			shader.SourceCode = sourceCode;
			shader.Compile(debug);
			_graphics.AddTrackedObject(shader);

			return shader;
		}

		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderBinding"/> class.
		/// </summary>
		/// <param name="graphics">The graphics.</param>
		internal GorgonShaderBinding(GorgonGraphics graphics)
		{
			VertexShader = new GorgonVertexShaderState(graphics);
			PixelShader = new GorgonPixelShaderState(graphics);
			_graphics = graphics;
		}
		#endregion
	}
}
