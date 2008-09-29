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
// Created: Saturday, June 28, 2008 9:56:19 PM
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
	/// Base class for shaders.
	/// </summary>
	public abstract class Shader
		: NamedObject, IShaderRenderer, IDisposable, IDeviceStateObject
	{
		#region Variables.
		private bool _disposed = false;							// Flag to indicate that the object is disposed.
		private ShaderParameterList _parameters = null;			// List of parameters for the shader.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the compiled byte code for the shader.
		/// </summary>
		protected byte[] Compiled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return whether this shader is a binary (no source code) shader.
		/// </summary>
		public virtual bool IsBinary
		{
			get;
			protected set;
		}

		/// <summary>
		/// Property to return whether the shader has been compiled yet or not.
		/// </summary>
		public virtual bool IsCompiled
		{
			get
			{
				return Compiled != null;
			}
		}

		/// <summary>
		/// Property to set or return the source code for this shader.
		/// </summary>
		public virtual string ShaderSource
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the parameters for this shader.
		/// </summary>
		public ShaderParameterList Parameters
		{
			get
			{
				return _parameters;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the parameters for a shader.
		/// </summary>
		protected abstract void GetParameters();

		/// <summary>
		/// Function called before the rendering begins with this shader.
		/// </summary>
		protected abstract void OnRenderBegin();

		/// <summary>
		/// Function called when rendering with this shader.
		/// </summary>
		protected abstract void OnRender();

		/// <summary>
		/// Function called after the rendering ends with this shader.
		/// </summary>
		protected abstract void OnRenderEnd();
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Shader"/> class.
		/// </summary>
		/// <param name="name">Name for this object.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the name parameter is NULL or a zero length string.</exception>
		protected Shader(string name)
			: base(name)
		{
			if (ShaderCache.Shaders.Contains(name))
				throw new ArgumentException("'" + name + "' already exists.");
			
			_parameters = new ShaderParameterList();

			ShaderCache.Shaders.Add(this);
			DeviceStateList.Add(this);
		}
		#endregion

		#region IShaderRenderer Members
		/// <summary>
		/// Function to begin the rendering with the shader.
		/// </summary>
		void IShaderRenderer.Begin()
		{
			OnRenderBegin();
		}

		/// <summary>
		/// Function to render with the shader.
		/// </summary>
		void IShaderRenderer.Render()
		{
			OnRender();
		}
		
		/// <summary>
		/// Function to end rendering with the shader.
		/// </summary>
		void IShaderRenderer.End()
		{
			OnRenderEnd();
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					if (Gorgon.CurrentShader == this)
						Gorgon.CurrentShader = null;

					if (ShaderCache.Shaders.Contains(Name))
						ShaderCache.Shaders.Remove(Name);

					DeviceStateList.Remove(this);
				}
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

		#region IDeviceStateObject Members
		/// <summary>
		/// Function called when the device is in a lost state.
		/// </summary>
		public abstract void DeviceLost();

		/// <summary>
		/// Function called when the device is reset.
		/// </summary>
		public abstract void DeviceReset();

		/// <summary>
		/// Function to force the loss of the objects data.
		/// </summary>
		public virtual void ForceRelease()
		{
			DeviceLost();
		}
		#endregion
	}
}
