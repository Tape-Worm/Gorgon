#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Saturday, September 23, 2006 1:27:55 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a technique within a shader.
	/// </summary>
	public class ShaderTechnique
		: NamedObject, IShaderRenderer
	{
		#region Variables.
		private D3D9.EffectHandle _effectHandle;				// Handle to the technique.
		private ShaderPassList _passes;							// List of passes.
		private FXShader _owner;								// Shader that owns the technique.
		private ShaderParameterList _parameters;				// Shader parameters.
		private IShaderParameter _projectionMatrix = null;		// Projection matrix parameter.
		private IShaderParameter _spriteImage = null;			// Sprite image parameter.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the effect handle for the technique.
		/// </summary>
		internal D3D9.EffectHandle D3DEffectHandle
		{
			get
			{
				return _effectHandle;
			}
		}

		/// <summary>
		/// Property to return the parameters for the technique.
		/// </summary>
		public ShaderParameterList Parameters
		{
			get
			{
				return _parameters;
			}
		}

		/// <summary>
		/// Property to return the shader that owns this technique.
		/// </summary>
		public FXShader Owner
		{
			get
			{
				return _owner;
			}
		}

		/// <summary>
		/// Property to return the list of passes for the technique.
		/// </summary>
		public ShaderPassList Passes
		{
			get
			{
				return _passes;
			}
		}

		/// <summary>
		/// Property to return whether this technique is valid or not.
		/// </summary>
		public bool Valid
		{
			get
			{
				return _owner.D3DEffect.ValidateTechnique(_effectHandle);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the D3D technique information.
		/// </summary>
		/// <param name="index">Index of the technique.</param>
		private void GetTechnique(int index)
		{
			try
			{
				// Get the handle.
				_effectHandle = _owner.D3DEffect.GetTechnique(index);
				Name = _owner.D3DEffect.GetTechniqueDescription(_effectHandle).Name;
				_passes.Add(this);
			}
			catch (Exception ex)
			{
				throw GorgonException.Repackage(GorgonErrors.CannotReadData, "Error trying to retrieve the technique information.", ex);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="shader">Shader that owns this technique.</param>
		/// <param name="index">Index of the technique.</param>
		internal ShaderTechnique(FXShader shader, int index)
			: base("NO_NAME")
		{
			_owner = shader;
			_effectHandle = null;
			_passes = new ShaderPassList();
			_parameters = new ShaderParameterList();
			GetTechnique(index);
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

			if (Owner.Parameters.Contains("_projectionMatrix"))
				_projectionMatrix = Owner.Parameters["_projectionMatrix"];
			if (Owner.Parameters.Contains("_spriteImage"))
				_spriteImage = Owner.Parameters["_spriteImage"];
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

			_owner.D3DEffect.Technique = _effectHandle; 
			_owner.D3DEffect.Begin(D3D9.FX.None);
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
			// Begin rendering each pass.
			for (int i = 0; i < _passes.Count; i++)
			{
				_owner.D3DEffect.BeginPass(i);
				Gorgon.Renderer.DrawCachedTriangles(primitiveStyle, vertexStart, vertexCount, indexStart, indexCount);
				_owner.D3DEffect.EndPass();
			}
		}

		/// <summary>
		/// Function to end rendering with the shader.
		/// </summary>
		void IShaderRenderer.End()
		{
			_owner.D3DEffect.End();
		}
		#endregion
		#endregion
	}
}

