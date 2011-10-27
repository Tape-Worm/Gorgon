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
// Created: Thursday, June 26, 2008 12:13:41 AM
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
	/// Object that ecapsulates a shader function.
	/// </summary>
	public class ShaderFunction
		: NamedObject, IDisposable
	{
		#region Variables.
		private bool _disposed = false;			// Flag to indicate if the object has been disposed or not.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the byte code for the function.
		/// </summary>
		internal D3D9.ShaderBytecode ByteCode
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the shader target profile name.
		/// </summary>
		public string Target
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the shader that owns this function.
		/// </summary>
		public Shader Shader
		{
			get;
			internal set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ShaderFunction"/> class.
		/// </summary>
		/// <param name="functionName">Name of the function ecapsulated by this object.</param>
		/// <param name="owner">Shader that owns this function.</param>
		/// <param name="byteCode">Byte code for the function.</param>
		/// <param name="shaderTarget">Shader target profile name.</param>
		internal ShaderFunction(string functionName, Shader owner, D3D9.ShaderBytecode byteCode, string shaderTarget)
			: base(functionName)
		{
			if (owner == null)
				throw new ArgumentNullException("owner");
			if (string.IsNullOrEmpty(shaderTarget))
				throw new ArgumentNullException("shaderTarget");
			if (byteCode == null)
				throw new ArgumentNullException("byteCode");
			ByteCode = byteCode;
			Target = shaderTarget;
			Shader = owner;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (ByteCode != null)
						ByteCode.Dispose();
				}

				ByteCode = null;
				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
