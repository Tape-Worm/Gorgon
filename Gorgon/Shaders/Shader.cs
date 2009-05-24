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
		private IShaderParameter _projectionMatrix = null;		// Projection matrix parameter.
		private IShaderParameter _spriteImage = null;			// Sprite image parameter.
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
		/// <param name="primitiveStyle">Type of primitive to render.</param>
		/// <param name="vertexStart">Starting vertex to render.</param>
		/// <param name="vertexCount">Number of vertices to render.</param>
		/// <param name="indexStart">Starting index to render.</param>
		/// <param name="indexCount">Number of indices to render.</param>
		protected abstract void OnRender(PrimitiveStyle primitiveStyle, int vertexStart, int vertexCount, int indexStart, int indexCount);

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
		#region Properties.
		/// <summary>
		/// Property to return whether a projection matrix is available in the shader.
		/// </summary>
		/// <remarks>This shader constant should be represented in the shader as: <c>float4x4 _projectionMatrix</c>.  If this constant is found, this value will return TRUE.
		/// <para>If this is available the library will automatically assign a value to the constant for use in the shader.  This will contain a copy of the current projection <see cref="GorgonLibrary.Matrix">matrix</see> 
		/// used by the current <see cref="GorgonLibrary.Graphics.Viewport">viewport</see>.</para></remarks>
		public bool HasProjectionMatrix
		{
			get
			{
				return _projectionMatrix != null;
			}			
		}

		/// <summary>
		/// Property to return whether a sprite image is available in the shader.
		/// </summary>
		/// <remarks>This shader constant should be represented in the shader as: <c>Texture _spriteImage</c>.  If this constant is found, this value will return TRUE.
		/// <para>If this is available the library will automatically assign a value to the constant for use in the shader.  This will typically be the current image used by a <see cref="GorgonLibrary.Graphics.Sprite">sprite</see> 
		/// or a series of sprites (if using the same image).</para></remarks>
		public bool HasSpriteImage
		{
			get
			{
				return _spriteImage != null;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the pre-defined constant parameters from a shader.
		/// </summary>
		/// <remarks>
		/// A shader can contain pre-defined constant values that the library will update automatically.  This is done to cut down on some redundant code that the user will
		/// typically always have to perform when calling a shader.  For example, the user may have 3 shaders and each time a shader is used the current sprite image would have to
		/// be set by the user each time the image changed for each sprite.  Additionally, passing the transformations (i.e. rotation, scaling and translation) to the shader
		/// is also more work than it needs to be.
		/// <para>To use this "default" functionality your shader will have to implement one of these constants:
		/// <list type="table">
		/// 			<listheader>
		/// 				<term>Constant type and name</term>
		/// 				<description>Description</description>
		/// 			</listheader>
		/// 			<item>
		/// 				<term>float4x4 _projectionMatrix</term>
		/// 				<description>The current projection <see cref="GorgonLibrary.Matrix">matrix</see> defined by the current <see cref="GorgonLibrary.Graphics.Viewport">Viewport</see>.  Use this to transform vertices in a vertex shader into screen space.</description>
		/// 			</item>
		/// 			<item>
		/// 				<term>Texture _spriteImage</term>
		/// 				<description>This is the currently active <see cref="GorgonLibrary.Graphics.Image">image</see> (at index 0 in the texture stage) for a given <see cref="GorgonLibrary.Graphics.Sprite">Sprite</see> or series of sprites.</description>
		/// 			</item>
		/// 		</list>
		/// 	</para>
		/// 	<para>The constants are case sensitive and as such must be declared exactly as shown in the table above with the leading underscore character.  If the constant is not
		/// named properly it will not be used as a default.</para>
		/// 	<para>To test whether the default constants are available, check the return value of <see cref="GorgonLibrary.Graphics.IShaderRenderer.HasProjectionMatrix">HasProjectionMatrix</see>
		/// or <see cref="GorgonLibrary.Graphics.IShaderRenderer.HasSpriteImage">HasSpriteImage</see> and if
		/// these properties return TRUE, then the constant has been implemented.</para>
		/// 	<para>This function is meant to be called internally by the shader.</para>
		/// </remarks>
		void IShaderRenderer.GetDefinedConstants()
		{
			_projectionMatrix = null;
			_spriteImage = null;

			if (_parameters.Contains("_projectionMatrix"))
				_projectionMatrix = _parameters["_projectionMatrix"];
			if (_parameters.Contains("_spriteImage"))
				_spriteImage = _parameters["_spriteImage"];
		}

		/// <summary>
		/// Function to return the parameter associated with the predefiend constant.
		/// </summary>
		/// <param name="constant">Predefined constant representing the parameter.</param>
		/// <returns>
		/// The parameter interface for the <paramref name="constant"/>.
		/// </returns>
		/// <remarks>The function will return the associated parameter object for a given pre-defined constant.  The constant should be defined in the shader as one of:
		/// <list type="table">
		/// 		<listheader>
		/// 			<term>Constant type and name</term>
		/// 			<description>Description</description>
		/// 		</listheader>
		/// 		<item>
		/// 			<term>float4x4 _projectionMatrix</term>
		/// 			<description>The current projection <see cref="GorgonLibrary.Matrix">matrix</see> defined by the current <see cref="GorgonLibrary.Graphics.Viewport">Viewport</see>.  Use this to transform vertices in a vertex shader into screen space.</description>
		/// 		</item>
		/// 		<item>
		/// 			<term>Texture _spriteImage</term>
		/// 			<description>This is the currently active <see cref="GorgonLibrary.Graphics.Image">image</see> (at index 0 in the texture stage) for a given <see cref="GorgonLibrary.Graphics.Sprite">Sprite</see> or series of sprites.</description>
		/// 		</item>
		/// 	</list>
		/// </remarks>
		public IShaderParameter GetDefinedParameter(PredefinedShaderConstants constant)
		{
			switch (constant)
			{
				case PredefinedShaderConstants.ProjectionMatrix:
					return _projectionMatrix;
				case PredefinedShaderConstants.SpriteImage:
					return _spriteImage;
				default:
					return null;
			}
		}

		/// <summary>
		/// Function to begin the rendering with the shader.
		/// </summary>
		void IShaderRenderer.Begin()
		{
			if (HasProjectionMatrix)
				_projectionMatrix.SetValue(Gorgon.CurrentClippingViewport.ProjectionMatrix);
			if (HasSpriteImage)
				_spriteImage.SetValue(Gorgon.Renderer.GetImage(0));

			OnRenderBegin();
		}

		/// <summary>
		/// Function to render with the shader.
		/// </summary>
		/// <param name="primitiveStyle">Type of primitive to render.</param>
		/// <param name="vertexStart">Starting vertex to render.</param>
		/// <param name="vertexCount">Number of vertices to render.</param>
		/// <param name="indexStart">Starting index to render.</param>
		/// <param name="indexCount">Number of indices to render.</param>
		void IShaderRenderer.Render(PrimitiveStyle primitiveStyle, int vertexStart, int vertexCount, int indexStart, int indexCount)
		{
			OnRender(primitiveStyle, vertexStart, vertexCount, indexStart, indexCount);
		}
		
		/// <summary>
		/// Function to end rendering with the shader.
		/// </summary>
		void IShaderRenderer.End()
		{
			OnRenderEnd();
		}
		#endregion
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
