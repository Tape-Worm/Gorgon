#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: March 21, 2021 10:22:33 PM
// 
#endregion

namespace Gorgon.Graphics.Core;

/// <summary>
/// Information for creating temporary render targets.
/// </summary>
internal class TempTargetTextureInfo
    : IGorgonTexture2DInfo
{
    #region Properties.
    /// <summary>Property to return the width of the texture, in pixels.</summary>
    public int Width
    {
        get;
        set;
    }

    /// <summary>Property to return the height of the texture, in pixels.</summary>
    public int Height
    {
        get;
        set;
    }

    /// <summary>Gets or sets the array count.</summary>
    public int ArrayCount
    {
        get;
        set;
    }

    /// <summary>Property to return whether this 2D texture is a cube map.</summary>
    /// <remarks>
    ///   <para>
    /// When this value is set to <b>true</b>, then the texture is defined as a cube map using the <see cref="ArrayCount" /> as the number of faces. Because of this, the <see cref="P:Gorgon.Graphics.Core.TempTargetTextureInfo.ArrayCount" /> value
    /// must be a multiple of 6. If it is not, then the array count will be adjusted to meet the requirement.
    /// </para>
    ///   <para>
    /// This value is defaulted to <b>false</b>.
    /// </para>
    /// </remarks>
    public bool IsCubeMap
    {
        get;
        set;
    }

    /// <summary>Property to return the format of the texture.</summary>
    public BufferFormat Format
    {
        get;
        set;
    }

    /// <summary>Property to return the number of mip-map levels for the texture.</summary>
    /// <remarks>
    ///   <para>
    /// If the texture is multisampled, this value must be set to 1.
    /// </para>
    ///   <para>
    /// This value is defaulted to 1.
    /// </para>
    /// </remarks>
    public int MipLevels
    {
        get;
        set;
    }

    /// <summary>Property to return the multisample quality and count for this texture.</summary>
    /// <remarks>This value is defaulted to <see cref="GorgonMultisampleInfo.NoMultiSampling" />.</remarks>
    public GorgonMultisampleInfo MultisampleInfo
    {
        get;
        set;
    }

    /// <summary>Property to return the intended usage flags for this texture.</summary>
    /// <remarks>This value is defaulted to <see cref="ResourceUsage.Default" />.</remarks>
    public ResourceUsage Usage => ResourceUsage.Default;

    /// <summary>Property to return the flags to determine how the texture will be bound with the pipeline when rendering.</summary>
    /// <remarks>
    ///   <para>
    /// If the <see cref="Usage" /> property is set to <see cref="ResourceUsage.Staging" />, then the texture must be created with a value of <see cref="TextureBinding.None" /> as staging textures do not
    /// support bindings of any kind. If this value is set to anything other than <see cref="TextureBinding.None" />, an exception will be thrown.
    /// </para>
    ///   <para>
    /// This value is defaulted to <see cref="TextureBinding.ShaderResource" />.
    /// </para>
    /// </remarks>
    public TextureBinding Binding
    {
        get;
        private set;
    } = TextureBinding.ShaderResource | TextureBinding.RenderTarget;

    /// <summary>Property to return whether this texture can be shared with other graphics interfaces.</summary>
    /// <remarks>
    /// Settings this flag to <b>true</b> allows the texture to be used with external graphics interfaces such as a Direct3D device. This is useful for providing interoperation between systems.
    /// </remarks>
    public TextureSharingOptions Shared => TextureSharingOptions.None;

    /// <summary>Property to return the name of this object.</summary>
    public string Name
    {
        get;
        set;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to initialize the information data.
    /// </summary>
    /// <param name="info">The info to copy.</param>
    public void Initialize(IGorgonTexture2DInfo info)
    {
        Name = info.Name;
        Binding |= info.Binding;
        MultisampleInfo = info.MultisampleInfo;
        MipLevels = info.MipLevels;
        Format = info.Format;
        IsCubeMap = info.IsCubeMap;
        ArrayCount = info.ArrayCount;
        Width = info.Width;
        Height = info.Height;
    }
    #endregion
}
