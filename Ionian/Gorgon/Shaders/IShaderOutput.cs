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
// Created: Wednesday, June 25, 2008 12:10:37 AM
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
	/// A list of predefined shader constants.
	/// </summary>
	/// <remarks>The shader constants presented here reflect these values in a given shader:
	/// <list type="table">
	/// <listheader>
	/// <term>Constant type and name</term>
	/// <description>Description</description>
	/// </listheader>
	/// <item>
	/// <term>float4x4 _projectionMatrix</term>
	/// <description>The current projection <see cref="GorgonLibrary.Matrix">matrix</see> defined by the current <see cref="GorgonLibrary.Graphics.Viewport">Viewport</see>.  Use this to transform vertices in a vertex shader into screen space.</description>
	/// </item>
	/// <item>
	/// <term>Texture _spriteImage</term>
	/// <description>This is the currently active <see cref="GorgonLibrary.Graphics.Image">image</see> (at index 0 in the texture stage) for a given <see cref="GorgonLibrary.Graphics.Sprite">Sprite</see> or series of sprites.</description>
	/// </item>
	/// </list>
	/// </remarks>
	public enum PredefinedShaderConstants
	{
		/// <summary>
		/// A projection matrix used to transform a vertex into screen space.
		/// </summary>
		ProjectionMatrix = 1,
		/// <summary>
		/// The currently active sprite image at stage 0.
		/// </summary>
		SpriteImage = 2
	}

	/// <summary>
	/// Defines a shader used in the rendering process.
	/// </summary>
	/// <remarks>This defines the component of a shader that can be applied to the rendering of a scene.</remarks>
	public interface IShaderRenderer
	{
		#region Properties.
		/// <summary>
		/// Property to return whether a projection matrix is available in the shader.
		/// </summary>
		/// <remarks>This shader constant should be represented in the shader as: <c>float4x4 _projectionMatrix</c>.  If this constant is found, this value will return TRUE.
		/// <para>If this is available the library will automatically assign a value to the constant for use in the shader.  This will contain a copy of the current projection <see cref="GorgonLibrary.Matrix">matrix</see> 
		/// used by the current <see cref="GorgonLibrary.Graphics.Viewport">viewport</see>.</para></remarks>
		bool HasProjectionMatrix
		{
			get;
		}

		/// <summary>
		/// Property to return whether a sprite image is available in the shader.
		/// </summary>
		/// <remarks>This shader constant should be represented in the shader as: <c>Texture _spriteImage</c>.  If this constant is found, this value will return TRUE.
		/// <para>If this is available the library will automatically assign a value to the constant for use in the shader.  This will typically be the current image used by a <see cref="GorgonLibrary.Graphics.Sprite">sprite</see> 
		/// or a series of sprites (if using the same image).</para></remarks>
		bool HasSpriteImage
		{
			get;
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
		/// <listheader>
		/// <term>Constant type and name</term>
		/// <description>Description</description>
		/// </listheader>
		/// <item>
		/// <term>float4x4 _projectionMatrix</term>
		/// <description>The current projection <see cref="GorgonLibrary.Matrix">matrix</see> defined by the current <see cref="GorgonLibrary.Graphics.Viewport">Viewport</see>.  Use this to transform vertices in a vertex shader into screen space.</description>
		/// </item>
		/// <item>
		/// <term>Texture _spriteImage</term>
		/// <description>This is the currently active <see cref="GorgonLibrary.Graphics.Image">image</see> (at index 0 in the texture stage) for a given <see cref="GorgonLibrary.Graphics.Sprite">Sprite</see> or series of sprites.</description>
		/// </item>
		/// </list>
		/// </para>
		/// <para>The constants are case sensitive and as such must be declared exactly as shown in the table above with the leading underscore character.  If the constant is not
		/// named properly it will not be used as a default.</para>
		/// <para>To test whether the default constants are available, check the return value of <see cref="GorgonLibrary.Graphics.IShaderRenderer.HasProjectionMatrix">HasProjectionMatrix</see> 
		/// or <see cref="GorgonLibrary.Graphics.IShaderRenderer.HasSpriteImage">HasSpriteImage</see> and if
		/// these properties return TRUE, then the constant has been implemented.</para>
		/// <para>This function is meant to be called internally by the shader.</para>
		/// </remarks>
		void GetDefinedConstants();

		/// <summary>
		/// Function to return the parameter associated with the predefiend constant.
		/// </summary>
		/// <param name="constant">Predefined constant representing the parameter.</param>
		/// <returns>The parameter interface for the <paramref name="constant"/>.</returns>
		/// <remarks>The function will return the associated parameter object for a given pre-defined constant.  The constant should be defined in the shader as one of:
		/// <list type="table">
		/// <listheader>
		/// <term>Constant type and name</term>
		/// <description>Description</description>
		/// </listheader>
		/// <item>
		/// <term>float4x4 _projectionMatrix</term>
		/// <description>The current projection <see cref="GorgonLibrary.Matrix">matrix</see> defined by the current <see cref="GorgonLibrary.Graphics.Viewport">Viewport</see>.  Use this to transform vertices in a vertex shader into screen space.</description>
		/// </item>
		/// <item>
		/// <term>Texture _spriteImage</term>
		/// <description>This is the currently active <see cref="GorgonLibrary.Graphics.Image">image</see> (at index 0 in the texture stage) for a given <see cref="GorgonLibrary.Graphics.Sprite">Sprite</see> or series of sprites.</description>
		/// </item>
		/// </list>
		/// </remarks>
		IShaderParameter GetDefinedParameter(PredefinedShaderConstants constant);

		/// <summary>
		/// Function to begin the rendering with the shader.
		/// </summary>
		void Begin();

		/// <summary>
		/// Function to render with the shader.
		/// </summary>
		/// <param name="primitiveStyle">Type of primitive to render.</param>
		/// <param name="vertexStart">Starting vertex to render.</param>
		/// <param name="vertexCount">Number of vertices to render.</param>
		/// <param name="indexStart">Starting index to render.</param>
		/// <param name="indexCount">Number of indices to render.</param>
		void Render(PrimitiveStyle primitiveStyle, int vertexStart, int vertexCount, int indexStart, int indexCount);

		/// <summary>
		/// Function to end rendering with the shader.
		/// </summary>
		void End();
		#endregion
	}
}
