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
// Created: Saturday, September 23, 2006 1:27:12 AM
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
	/// Object representing a pass within the shader.
	/// </summary>
	public class ShaderPass
		: NamedObject, IShaderRenderer
	{
		#region Variables.
		private D3D9.EffectHandle _effectHandle;				// Handle to the pass.
		private ShaderTechnique _owner;							// Technique that owns this pass.
		private int _passIndex = 0;								// Index of this pass.
		private IShaderParameter _projectionMatrix = null;		// Projection matrix parameter.
		private IShaderParameter _spriteImage = null;			// Sprite image parameter.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the owning technique for this pass.
		/// </summary>
		public ShaderTechnique Owner
		{
			get
			{
				return _owner;
			}
		}

		/// <summary>
		/// Property to return the index of the shader pass.
		/// </summary>
		public int PassIndex
		{
			get
			{
				return _passIndex;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to get pass information.
		/// </summary>
		/// <param name="index">Index of the pass.</param>
		private void GetPass(int index)
		{
			try
			{
				// Get handle and name.
				_passIndex = index;
				_effectHandle = _owner.Owner.D3DEffect.GetPass(_owner.D3DEffectHandle, index);
				Name = _owner.Owner.D3DEffect.GetPassDescription(_effectHandle).Name;
			}
			catch (Exception ex)
			{
				throw GorgonException.Repackage(GorgonErrors.CannotReadData, "Could not retrieve the shader passes.", ex);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="technique">Owning technique.</param>
		/// <param name="index">Index of the pass.</param>
		internal ShaderPass(ShaderTechnique technique, int index)
			: base("NO_NAME")
		{
			_owner = technique;
			_effectHandle = null;
			GetPass(index);
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

			if (Owner.Owner.Parameters.Contains("_projectionMatrix"))
				_projectionMatrix = Owner.Owner.Parameters["_projectionMatrix"];
			if (Owner.Owner.Parameters.Contains("_spriteImage"))
				_spriteImage = Owner.Owner.Parameters["_spriteImage"];
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

			_owner.Owner.D3DEffect.Technique = _owner.D3DEffectHandle;
			_owner.Owner.D3DEffect.Begin(D3D9.FX.None);
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
			_owner.Owner.D3DEffect.BeginPass(_passIndex);
			Gorgon.Renderer.DrawCachedTriangles(primitiveStyle, vertexStart, vertexCount, indexStart, indexCount);
			_owner.Owner.D3DEffect.EndPass();
		}

		/// <summary>
		/// Function to end rendering with the shader.
		/// </summary>
		void IShaderRenderer.End()
		{
			
			_owner.Owner.D3DEffect.End();
		}
		#endregion
		#endregion
	}
}
