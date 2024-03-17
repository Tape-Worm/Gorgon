
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: February 8, 2020 9:27:54 PM
// 


using System.Diagnostics;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// A renderer used to display 2D texture content
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="Texture2DViewer"/> class.</remarks>
/// <param name="renderer">The main renderer for the content view.</param>
/// <param name="swapChain">The swap chain for the content view.</param>
/// <param name="dataContext">The view model to assign to the renderer.</param>
internal class Texture2DViewer(Gorgon2D renderer, GorgonSwapChain swapChain, IImageContent dataContext)
        : TextureViewer(ImageDataType.Image2D.ToString(), "Gorgon2DTextureArrayView", 0, renderer, swapChain, dataContext)
{

    // The texture view.
    private GorgonTexture2DView _textureView;
    // The texture resource.
    private GorgonTexture2D _texture;



    /// <summary>
    /// Function to destroy the texture.
    /// </summary>
    protected override void DestroyTexture()
    {
        _textureView?.Dispose();
        _texture?.Dispose();
        _textureView = null;
        _texture = null;
    }

    /// <summary>
    /// Function to create the texture for an image.
    /// </summary>
    protected override void CreateTexture()
    {
        if ((DataContext?.ImageData is null) || (DataContext.ImageType != ImageDataType.Image2D))
        {
            RenderRegion = DX.RectangleF.Empty;
            return;
        }

        RenderRegion = new DX.RectangleF(0, 0, DataContext.ImageData.Width, DataContext.ImageData.Height);

        _texture = DataContext.ImageData.ToTexture2D(Graphics, new GorgonTexture2DLoadOptions
        {
            Name = DataContext.File.Name,
            Binding = TextureBinding.ShaderResource,
            Usage = ResourceUsage.Immutable,
            IsTextureCube = false
        });

        ShaderView = _textureView = _texture.GetShaderResourceView();
        TextureUpdated();
    }

    /// <summary>Function to render the content.</summary>
    /// <remarks>This is the method that developers should override in order to draw their content to the view.</remarks>
    protected override void DrawTexture()
    {
        GorgonColor color = new(GorgonColor.White, Opacity);

        Debug.Assert(_textureView is not null, "The texture is null.  Why?");

        Renderer.Begin(BatchState, Camera);
        Renderer.DrawFilledRectangle(new DX.RectangleF(RenderRegion.Width * -0.5f,
                                                       RenderRegion.Height * -0.5f,
                                                       RenderRegion.Width,
                                                       RenderRegion.Height),
                                    color,
                                    _textureView,
                                    new DX.RectangleF(0, 0, 1, 1),
                                    DataContext.CurrentArrayIndex,
                                    textureSampler: GorgonSamplerState.PointFiltering);
        Renderer.End();
    }


}
