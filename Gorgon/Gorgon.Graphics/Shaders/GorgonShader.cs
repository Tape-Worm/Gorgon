#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Thursday, December 15, 2011 9:29:56 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using Gorgon.UI;
using SharpDX;
using D3D = SharpDX.Direct3D;
using D3DCompiler = SharpDX.D3DCompiler;

namespace Gorgon.Graphics
{
	/// <summary>
	/// The base shader object.
	/// </summary>
	public abstract class GorgonShader
		: GorgonNamedObject, IDisposable
	{
		#region Properties.
		/// <summary>
		/// Property to return the shader byte code.
		/// </summary>
		protected internal D3DCompiler.ShaderBytecode D3DByteCode
		{
			get;
		}

		/// <summary>
		/// Property to set or return whether to include debug information in the shader or not.
		/// </summary>
		public bool IsDebug
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the type of shader.
		/// </summary>
		public abstract ShaderType ShaderType
		{
			get;
		}
		
		/// <summary>
		/// Property to return the video device that created this shader.
		/// </summary>
		public IGorgonVideoDevice VideoDevice
		{
			get;
		}
		#endregion

		#region Methods.
        /*/// <summary>
        /// Function to save the shader to a stream.
        /// </summary>
        /// <param name="binary"><b>true</b> to save the binary version of the shader, <b>false</b> to save the source.</param>
        /// <param name="saveDebug"><b>true</b> to save the debug information, <b>false</b> to exclude it.</param>
        /// <returns>An array of bytes.</returns>
        /// <remarks>The <paramref name="saveDebug"/> parameter is only applicable when the <paramref name="binary"/> parameter is set to <b>true</b>.</remarks>
        /// <exception cref="System.ArgumentException">Thrown when the shader is being saved as source code and the <see cref="Gorgon.Graphics.GorgonShader.SourceCode">SourceCode</see> parameter is NULL (<i>Nothing</i> in VB.Net) or empty.</exception>
        /// <exception cref="GorgonException">Thrown when the shader fails to compile.</exception>
        public byte[] Save(bool binary, bool saveDebug)
        {
            using (var memoryStream = new MemoryStream())
            {
                Save(memoryStream, binary, saveDebug);
                memoryStream.Position = 0;

                return memoryStream.ToArray();
            }
        }

		/// <summary>
		/// Function to save the shader to a stream.
		/// </summary>
		/// <param name="stream">Stream to write into.</param>
		/// <param name="binary">[Optional] <b>true</b> to save the binary version of the shader, <b>false</b> to save the source.</param>
		/// <param name="saveDebug">[Optional] <b>true</b> to save the debug information, <b>false</b> to exclude it.</param>
		/// <remarks>The <paramref name="saveDebug"/> parameter is only applicable when the <paramref name="binary"/> parameter is set to <b>true</b>.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the shader is being saved as source code and the <see cref="Gorgon.Graphics.GorgonShader.SourceCode">SourceCode</see> parameter is NULL (<i>Nothing</i> in VB.Net) or empty.</exception>
		/// <exception cref="GorgonException">Thrown when the shader fails to compile.</exception>
		public void Save(Stream stream, bool binary = false, bool saveDebug = false)
		{
			Shaders.ShaderBytecode compiledShader = null;
			stream.ValidateObject("stream");

		    if ((!binary) && (string.IsNullOrEmpty(SourceCode)))
		    {
		        throw new ArgumentException(Resources.GORGFX_SHADER_NO_CODE, nameof(binary));
		    }

		    if (!binary)
			{
				byte[] shaderSource = Encoding.UTF8.GetBytes(SourceCode);
				stream.Write(shaderSource, 0, shaderSource.Length);

			    return;
			}

			try
			{
				compiledShader = CompileFromSource(saveDebug);
				byte[] header = Encoding.UTF8.GetBytes(GorgonShaderBinding.BinaryShaderHeader);
				stream.Write(header, 0, header.Length);
				compiledShader.Save(stream);
			}
			finally
			{
				if (compiledShader != null)
				{
				    compiledShader.Dispose();
				}
			}
		}

		/// <summary>
		/// Function to save the shader to a file.
		/// </summary>
		/// <param name="fileName">File name and path for the shader file.</param>
		/// <param name="binary">[Optional] <b>true</b> if saving as a binary version of the shader, <b>false</b> if not.</param>
		/// <param name="saveDebug">[Optional] <b>true</b> to save debug information with the shader, <b>false</b> to exclude it.</param>
		/// <remarks>The <paramref name="saveDebug"/> parameter is only applicable when the <paramref name="binary"/> parameter is set to <b>true</b>.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="fileName"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown is the fileName parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the shader is being saved as source code and the <see cref="Gorgon.Graphics.GorgonShader.SourceCode">SourceCode</see> parameter is NULL (<i>Nothing</i> in VB.Net) or empty.</para>
		/// </exception>
		/// <exception cref="GorgonException">Thrown when the shader fails to compile.</exception>
		public void Save(string fileName, bool binary = false, bool saveDebug = false)
		{
			FileStream stream = null;

			fileName.ValidateString("fileName");

			try
			{
				stream = File.Open(fileName, FileMode.Create, FileAccess.Write, FileShare.None);
				Save(stream, binary, saveDebug);
			}
			finally
			{
				stream?.Dispose();
			}
		}*/

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public virtual void Dispose()
		{
			D3DByteCode?.Dispose();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShader"/> class.
		/// </summary>
		/// <param name="videoDevice">The video device used to create the shader.</param>
		/// <param name="name">The name for this shader.</param>
		/// <param name="isDebug"><b>true</b> if debug information is included in the byte code, <b>false</b> if not.</param>
		/// <param name="byteCode">The byte code for the shader.</param>
		protected GorgonShader(IGorgonVideoDevice videoDevice, string name, bool isDebug, D3DCompiler.ShaderBytecode byteCode)
			: base(name)
		{
			VideoDevice = videoDevice;
			IsDebug = isDebug;
			D3DByteCode = byteCode;
		}
		#endregion
	}
}
