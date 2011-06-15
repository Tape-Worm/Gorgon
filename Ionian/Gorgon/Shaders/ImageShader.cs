#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Friday, December 07, 2007 4:17:18 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing an image shader interface.
	/// </summary>
	public class ImageShader
		: BaseShader<ImageShader>
	{
		#region Variables.
		private D3D9.TextureShader _shader = null;			// Texture shader.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the D3D shader.
		/// </summary>
		internal D3D9.TextureShader D3DShader
		{
			get
			{				
				return _shader;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the shader target profile.
		/// </summary>
		/// <param name="target">Version of the profile.</param>
		/// <returns>The shader target profile.</returns>
		/// <remarks>For a texture shader this is always tx_1_0.</remarks>
		protected override string ShaderProfile(Version target)
		{
			return "tx_1_0";
		}

		/// <summary>
		/// Function called before the rendering begins with this shader.
		/// </summary>
		protected override void OnRenderBegin()
		{
			throw new NotImplementedException();
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
			throw new NotImplementedException();
		}

		/// <summary>
		/// Function called after the rendering ends with this shader.
		/// </summary>
		protected override void OnRenderEnd()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Function called to create the actual shader object.
		/// </summary>
		protected override void CreateShader()
		{
			if (Function != null)
				_shader = new D3D9.TextureShader(Function.ByteCode.Data);
		}

		/// <summary>
		/// Function called to destroy the shader object.
		/// </summary>
		protected override void DestroyShader()
		{
			if (_shader != null)
				_shader.Dispose();

			_shader = null;
		}

		/// <summary>
		/// Function to compile the shader source code.
		/// </summary>
		/// <param name="functionName">Name of the function to compile.</param>
		/// <param name="flags">Options to use for compilation.</param>
		/// <remarks>See <see cref="GorgonLibrary.Graphics.ShaderCompileOptions"/> for more information about the compile time options.
		/// </remarks>
		public void CompileShader(string functionName, ShaderCompileOptions flags)
		{
			CompileShaderImplementation(functionName, new Version(1,0), flags);
		}

		/// <summary>
		/// Function to set the default values.
		/// </summary>
		public void SetDefaultValues()
		{
			_shader.SetDefaults();
		}		
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ImageShader"/> class.
		/// </summary>
		/// <param name="name">Name of the image shader.</param>		
		public ImageShader(string name)
			: base(name, null)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ImageShader"/> class.
		/// </summary>
		/// <param name="name">Name for this object.</param>
		/// <param name="function">Function to bind to the shader as an entry point.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		public ImageShader(string name, ShaderFunction function)
			: base(name, function)
		{
			if (function == null)
				throw new ArgumentNullException("function");
		}
		#endregion
	}
}
