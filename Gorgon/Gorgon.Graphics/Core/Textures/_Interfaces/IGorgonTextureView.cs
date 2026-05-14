// Gorgon.
// Copyright (C) 2026 Michael Winsor
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
// Created: April 20, 2026 7:53:23 PM
//

using Gorgon.Core;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A view for texture resources.
/// </summary>
/// <typeparam name="T">The type of texture for the view. Must inherit from the <see cref="GorgonTextureCommon"/> type.</typeparam>
/// <remarks>
/// <para>
/// A texture view allows a shader to interpret a texture resource in different ways. For example, the view could allow the shader to only use array indices 2-7 in a texture with 16 array indices. It can 
/// allow access to a specific mip level, and even limited format reinterpretation.
/// </para>
/// <para type="bindless_texture_view_doc">
/// In Gorgon's bindless model, passing the view is done by writing a constant value via the <see cref="GorgonCommandList.WriteConstant{T}(int, in T)"/> method. The value is the handle of the view, which 
/// is retrieved by the <see cref="GetViewHandle()"/> method on the view. Then, in the shader, the resource can be indexed easily by the <c>ResourceDescriptorHeap[srv_handle]</c> intrinsic function (where 
/// <c>srv_handle</c> is the name of the constant that was updated).
/// </para>
/// <para type="bindless_texture_view_doc">
/// Once the resource is available (e.g. <c>Texture2D texture = ResourceDescriptorHeap[srv_handle]</c>), then it can be used like any texture and have its content sampled or loaded.
/// </para>
/// </remarks>
/// <seealso cref="GetViewHandle"/>
public interface IGorgonTextureView<out T>
    : IGorgonNamedObject, IDisposable
    where T : GorgonTextureCommon
{
    /// <summary>
    /// Property to return whether the view resource is disposed or not.
    /// </summary>
    bool IsResourceDisposed
    {
        get;
    }

    /// <summary>
    /// Property to return the graphics interface associated with this view.
    /// </summary>
    GorgonGraphics Graphics
    {
        get;
    }

    /// <summary>
    /// Property to return the resource for this view.
    /// </summary>
    GorgonGpuResource Resource
    {
        get;
    }

    /// <summary>
    /// Property to return the resource type that is backing this view.
    /// </summary>
    GraphicsResourceType ResourceType
    {
        get;
    }

    /// <summary>
    /// Property to return the texture used by this view.
    /// </summary>
    /// <remarks>
    /// This value is a strongly typed version of the <see cref="GorgonResourceView.Resource"/> property and point to the same object.
    /// </remarks>
    T Texture
    {
        get;
    }

    /// <summary>
    /// Property to return the format information for the <see cref="Format"/>.
    /// </summary>
    GorgonFormatInfo FormatInfo
    {
        get;
    }

    /// <summary>
    /// Property to return the format for the view.
    /// </summary>
    BufferFormat Format
    {
        get;
    }

    /// <summary>
    /// Property to return the first mip level in the view.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The <see cref="MinimumLodClamp"/> value should be set to zero if this value is non-zero.
    /// </para>
    /// </remarks>
    short MipLevel
    {
        get;
    }

    /// <summary>
    /// Property to return the number of mip levels in the view.
    /// </summary>
    short MipCount
    {
        get;
    }

    /// <summary>
    /// Property to return the first array index within the buffer to start the view at.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value only applies to textures that have a <see cref="GorgonTextureCommon.Type"/> of <see cref="TextureType.Texture1D"/>, or <see cref="TextureType.Texture2D"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="TextureType"/>
    short ArrayIndex
    {
        get;
    }

    /// <summary>
    /// Property to return the number of array indices in the view.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value only applies to textures that have a <see cref="GorgonTextureCommon.Type"/> of <see cref="TextureType.Texture1D"/>, or <see cref="TextureType.Texture2D"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="TextureType"/>
    short ArrayCount
    {
        get;
    }

    /// <summary>
    /// Property to return the minimum LOD clamp that can be accessed by the view.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A value of 0 indicates that the entire mip chain is accessible, specifying 3.0f means that mip map levels from 3.0 to <see cref="GorgonTextureCommon.MipCount"/><c>-1</c> are accessible.
    /// </para>
    /// <para>
    /// The <see cref="MipLevel"/> value should be set to zero if this value is non-zero.
    /// </para>
    /// </remarks>
    float MinimumLodClamp
    {
        get;
    }

    /// <summary>
    /// Property to return the the index of the plane in a planar format to use in the view.
    /// </summary>
    byte PlaneIndex
    {
        get;
    }

    /// <summary>
    /// Function to retrieve the handle of the view, which is used to pass to a shader for resource heap indexing.
    /// </summary>
    /// <returns>The handle of the view.</returns>
    int GetViewHandle();
}
