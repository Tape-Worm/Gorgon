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
// Created: Monday, June 30, 2008 12:33:37 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using GorgonLibrary.Internal;
using D3D9 = SlimDX.Direct3D9;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a vertex shader.
	/// </summary>
	public class VertexShader
		: BaseShader<VertexShader>
	{
		#region Variables.
		private D3D9.VertexShader _shader = null;					// Vertex shader.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the shader target profile.
		/// </summary>
		/// <param name="target">Version of the profile.</param>
		/// <returns>The shader target profile.</returns>
		protected override string ShaderProfile(Version target)
		{
			if (target == null)
				throw new ArgumentNullException("target");

			switch (target.ToString())
			{
				case "1.1":
					return "vs_1_1";
				case "2.1":
					return "vs_2_a";
				case "2.2":
					return "vs_2_b";
				case "2.0.1":
					return "vs_2_sw";
				case "3":
				case "3.0":
					return "vs_3_0";
				case "3.0.1":
					return "vs_3_sw";
			}

			return "vs_2_0";
		}

		/// <summary>
		/// Function called to create the actual shader object.
		/// </summary>
		protected override void CreateShader()
		{
			if (Function != null)
			{
				D3D9.ShaderBytecode byteCode = null;

				try
				{
					byte[] streamData = new byte[Function.ByteCode.Data.Length];

					Function.ByteCode.Data.Position = 0;
					Function.ByteCode.Data.Read(streamData, 0, streamData.Length);
					byteCode = new SlimDX.Direct3D9.ShaderBytecode(streamData);
					_shader = new D3D9.VertexShader(Gorgon.Screen.Device, byteCode);
				}
				catch
				{
					if (byteCode != null)
						byteCode.Dispose();
					throw;
				}
			}
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
		/// Function called before the rendering begins with this shader.
		/// </summary>
		protected override void OnRenderBegin()
		{
			if (_shader != null)
				Gorgon.Screen.Device.VertexShader = _shader;
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
			Gorgon.Screen.Device.VertexShader = null;
		}

		/// <summary>
		/// Function called when the shader is deserialized.
		/// </summary>
		/// <param name="serializer">Deserializer used to read the data.</param>
		protected override void OnReadData(GorgonLibrary.Serialization.Serializer serializer)
		{
			
		}

		/// <summary>
		/// Function to compile the shader source code.
		/// </summary>
		/// <param name="functionName">Name of the function to compile.</param>
		/// <param name="target">Pixel shader target version.</param>
		/// <param name="flags">Options to use for compilation.</param>
		/// <remarks>See <see cref="GorgonLibrary.Graphics.ShaderCompileOptions"/> for more information about the compile time options.
		/// <para>The target parameter can be 1.1, 2.0, 2.1 (for 2_a), 2.2 (for 2_b), 2.0.1 (for 2_sw), 3.0 or 3.0.1 (for 3_sw).</para>
		/// </remarks>
		public void CompileShader(string functionName, Version target, ShaderCompileOptions flags)
		{
			CompileShaderImplementation(functionName, target, flags);			
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="VertexShader"/> class.
		/// </summary>
		/// <param name="name">Name for this object.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		public VertexShader(string name)
			: base(name, null)
		{
		}
		
		/// <summary>
		/// Initializes a new instance of the <see cref="VertexShader"/> class.
		/// </summary>
		/// <param name="name">Name for this object.</param>
		/// <param name="function">Function to bind to the shader as an entry point.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		public VertexShader(string name, ShaderFunction function)
			: base(name, function)
		{
		}
		#endregion

	}
}
