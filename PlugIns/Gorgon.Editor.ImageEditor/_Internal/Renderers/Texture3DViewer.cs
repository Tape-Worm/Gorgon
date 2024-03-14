#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: January 9, 2019 1:43:36 PM
// 
#endregion

using System.Numerics;
using System.Threading;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Graphics.Imaging;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// A viewer for a 3D texture.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="Texture3DViewer"/> class.</remarks>
/// <param name="renderer">The main renderer for the content view.</param>
/// <param name="swapChain">The swap chain for the content view.</param>
/// <param name="dataContext">The view model to assign to the renderer.</param>
internal class Texture3DViewer(Gorgon2D renderer, GorgonSwapChain swapChain, IImageContent dataContext)
        : TextureViewer(ImageType.Image3D.ToString(), "Gorgon3DTextureView", 1, renderer, swapChain, dataContext)
{
    #region Variables.
    // The texture to display.
    private GorgonTexture3D _texture;
    // The view for the texture.
    private GorgonTexture3DView _textureView;
    // The volume renderer.
    private VolumeRenderer _volRenderer;
    #endregion

    #region Methods.
    /// <summary>Function to dispose any texture resources.</summary>
    protected override void DestroyTexture()
    {
        _texture?.Dispose();
        _textureView?.Dispose();

        _texture = null;
        _textureView = null;
    }

    /// <summary>Function to create the texture for the view.</summary>
    protected override void CreateTexture()
    {
        if ((DataContext?.ImageData is null) || (DataContext.ImageType != ImageType.Image3D))
        {
            RenderRegion = DX.RectangleF.Empty;
            return;
        }

        RenderRegion = new DX.RectangleF(0, 0, DataContext.ImageData.Width, DataContext.ImageData.Height);

        _texture = DataContext.ImageData.ToTexture3D(Graphics, new GorgonTextureLoadOptions
        {
            Name = DataContext.File.Name,
            Binding = TextureBinding.ShaderResource,
            Usage = ResourceUsage.Immutable
        });

        ShaderView = _textureView = _texture.GetShaderResourceView();
        _volRenderer.AssignTexture(_textureView);
        TextureUpdated();
    }

    /// <summary>Function to render the content.</summary>
    /// <remarks>This is the method that developers should override in order to draw their content to the view.</remarks>
    protected override void DrawTexture()
    {
        Renderer.Begin(BatchState, Camera);
        // We can use this for 3D textures because the texture is in slot 1, and slot 0, where the 2D texture is usually located is vacant and not used by the pixel shader.
        Renderer.DrawFilledRectangle(new DX.RectangleF(RenderRegion.Width * -0.5f,
                                                       RenderRegion.Height * -0.5f,
                                                       RenderRegion.Width,
                                                       RenderRegion.Height),
                                    new GorgonColor(GorgonColor.White, Opacity),
                                    null,
                                    new DX.RectangleF(0, 0, 1, 1),
                                    0,
                                    textureSampler: GorgonSamplerState.PointFiltering);
        Renderer.End();

        // Draw a frame around the volume rendering area.
        DX.RectangleF volRegion = _volRenderer.VolumeRegion;
        Renderer.Begin();

        DX.Size2F textArea = Resources.GORIMG_TEXT_3DVIEW.MeasureLine(Renderer.DefaultFont, false);
        Renderer.DrawFilledRectangle(volRegion, new GorgonColor(GorgonColor.Black, 0.5f));
        Renderer.DrawFilledRectangle(new DX.RectangleF(volRegion.Left - 1, volRegion.Bottom, volRegion.Width + 2, textArea.Height + 6), GorgonColor.White);
        Renderer.DrawRectangle(new DX.RectangleF(volRegion.X - 1, volRegion.Y - 1, volRegion.Width + 2, volRegion.Height + 2), GorgonColor.White);
        Renderer.DrawString("3D View", new Vector2((volRegion.Right + volRegion.Left) / 2.0f - (textArea.Width / 2.0f), volRegion.Bottom + 3), color: GorgonColor.Black);

        Renderer.End();

        _volRenderer.Render();
    }

    /// <summary>Function called when the view has been resized.</summary>
    /// <remarks>Developers can override this method to handle cases where the view window is resized and the content has size dependent data (e.g. render targets).</remarks>
    protected override void OnResizeEnd()
    {
        base.OnResizeEnd();

        _volRenderer.ResizeRenderRegion(ClientSize);
    }

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing">
    ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            VolumeRenderer volRender = Interlocked.Exchange(ref _volRenderer, null);
            volRender?.Dispose();
        }
        base.Dispose(disposing);
    }

    /// <summary>Function called during resource creation.</summary>
    protected override void OnCreateResources() 
    {
        base.OnCreateResources();

        _volRenderer = new VolumeRenderer(Graphics);
        _volRenderer.CreateResources(ClientSize);            
    }

    #endregion
}
