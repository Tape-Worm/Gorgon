
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
// Created: February 10, 2020 10:19:46 PM
// 


using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Animation;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// A common class for the texture viewers
/// </summary>
internal abstract class TextureViewer
    : DefaultContentRenderer<IImageContent>, ITextureViewer
{

    /// <summary>
    /// Parameters to pass to the texture shader(s).
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 16, Pack = 16)]
    internal struct TextureParams
    {
        /// <summary>
        /// Size of the structure, in bytes.
        /// </summary>
        public readonly static int Size = Unsafe.SizeOf<TextureParams>();

        /// <summary>
        /// Depth slice index.
        /// </summary>
        public float DepthSlice;
        /// <summary>
        /// Mip map level.
        /// </summary>
        public float MipLevel;
    }



    // The pixel shader for rendering an image.
    private GorgonPixelShader _imageShader;
    // The parameters for the texture viewer shader.
    private GorgonConstantBufferView _textureParameters;
    // Pixel shader used to render the image.
    private Gorgon2DShaderState<GorgonPixelShader> _batchShaderState;
    // The name of the pixel shader to use for rendering a texture.
    private readonly string _shaderName;
    // The texture slot for our texture.
    private readonly int _textureSlot;
    // The slots available for textures on the shader.
    private readonly GorgonShaderResourceView[] _slots = new GorgonShaderResourceView[2];
    // The controller for animating the content.
    private readonly ImageAnimationController _animController = new();
    private IGorgonAnimation _opacityAnimation;
    private readonly GorgonAnimationBuilder _animationBuilder = new();
    // Flag to indicate that the texture needs refreshing.
    private bool _textureNeedsUpdate = true;



    /// <summary>
    /// Property to return the batch state for rendering the texture.
    /// </summary>
    protected Gorgon2DBatchState BatchState
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the view for the texture as a shader resource.
    /// </summary>
    protected GorgonShaderResourceView ShaderView
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether changing mip levels is allowed.
    /// </summary>
    public bool AllowMipChange
    {
        get;
        protected set;
    } = true;

    /// <summary>
    /// Property to set or return whether changing array indices/depth slices is allowed.
    /// </summary>
    public bool AllowArrayDepthChange
    {
        get;
        protected set;
    } = true;

    /// <summary>Property to set or return the opacity of the content in the view.</summary>
    public float Opacity
    {
        get;
        set;
    }



    /// <summary>
    /// Function to update the texture parameters for rendering.
    /// </summary>
    private void UpdateTextureParameters()
    {
        if (DataContext is null)
        {
            return;
        }

        TextureParams p = new()
        {
            DepthSlice = DataContext.DepthCount <= 1 ? 0 : ((float)(DataContext.CurrentDepthSlice) / (DataContext.DepthCount - 1)),
            MipLevel = DataContext.CurrentMipLevel
        };

        _textureParameters.Buffer.SetData(in p);
    }

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing">
    ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            GorgonPixelShader shader = Interlocked.Exchange(ref _imageShader, null);
            GorgonConstantBufferView cBuffer = Interlocked.Exchange(ref _textureParameters, null);

            if (shader is null)
            {
                return;
            }

            shader.Dispose();
            cBuffer?.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>
    /// Function called during resource creation.
    /// </summary>
    protected virtual void OnCreateResources()
    {
    }

    /// <summary>
    /// Function to draw the texture.
    /// </summary>
    protected abstract void DrawTexture();

    /// <summary>
    /// Function to create the texture for display.
    /// </summary>
    protected abstract void CreateTexture();

    /// <summary>
    /// Function to destroy the texture when done with it.
    /// </summary>
    protected abstract void DestroyTexture();

    /// <summary>Function to render the content.</summary>
    /// <remarks>This is the method that developers should override in order to draw their content to the view.</remarks>
    protected sealed override void OnRenderContent()
    {
        base.OnRenderContent();

        DrawTexture();

        if (_animController.State == AnimationState.Playing)
        {
            _animController.Update();
        }
    }

    /// <summary>Function called when the renderer needs to load any resource data.</summary>
    /// <remarks>
    /// Developers can override this method to set up their own resources specific to their renderer. Any resources set up in this method should be cleaned up in the associated
    /// <see cref="DefaultContentRenderer{T}.OnUnload"/> method.
    /// </remarks>
    protected override void OnLoad()
    {
        base.OnLoad();

        Opacity = 1.0f;
        CreateTexture();
        AnimateTexture();

        _textureNeedsUpdate = false;
    }

    /// <summary>Function called when the renderer needs to clean up any resource data.</summary>
    /// <remarks>
    /// Developers should always override this method if they've overridden the <see cref="DefaultContentRenderer{T}.OnLoad"/> method. Failure to do so can cause memory leakage.
    /// </remarks>
    protected override void OnUnload()
    {
        base.OnUnload();

        DestroyTexture();
    }

    /// <summary>
    /// Function to animate the texture opacity.
    /// </summary>
    protected void AnimateTexture()
    {
        _animController.Stop();

        _opacityAnimation = _animationBuilder.Clear()
                                             .EditSingle(nameof(ITextureViewer.Opacity))
                                             .SetInterpolationMode(TrackInterpolationMode.Spline)
                                             .SetKey(new GorgonKeySingle(0, 0))
                                             .SetKey(new GorgonKeySingle(0.35f, 1.0f))
                                             .EndEdit()
                                             .Build("OpacityAnimation");

        _animController.Play(this, _opacityAnimation);
    }

    /// <summary>
    /// Function to notify that the texture has been updated.
    /// </summary>
    protected void TextureUpdated()
    {
        Array.Clear(_slots, 0, _slots.Length);
        _slots[_textureSlot] = ShaderView;

        Gorgon2DShaderStateBuilder<GorgonPixelShader> shaderBuilder = new();
        shaderBuilder
            .ResetTo(_batchShaderState)
            .ConstantBuffer(_textureParameters, 1)
            .Shader(_imageShader)
            .SamplerState(GorgonSamplerState.PointFiltering, 0)
            .SamplerState(GorgonSamplerState.PointFiltering, 1)
            .ShaderResources(_slots);

        _batchShaderState = shaderBuilder.Build();

        Gorgon2DBatchStateBuilder batchBuilder = new();

        BatchState = batchBuilder.ResetTo(BatchState)
                                  .PixelShaderState(_batchShaderState)
                                  .Build();

        UpdateTextureParameters();
    }

    /// <summary>Function called when a property on the <see cref="DefaultContentRenderer{T}.DataContext"/> is changing.</summary>
    /// <param name="propertyName">The name of the property that is changing.</param>
    /// <remarks>Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.</remarks>
    protected override void OnPropertyChanging(string propertyName)
    {
        base.OnPropertyChanging(propertyName);

        switch (propertyName)
        {
            case nameof(IImageContent.ImageData):
                DestroyTexture();
                _textureNeedsUpdate = true;
                break;
        }
    }

    /// <summary>Function called when a property on the <see cref="DefaultContentRenderer{T}.DataContext"/> has been changed.</summary>
    /// <param name="propertyName">The name of the property that was changed.</param>
    /// <remarks>Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.</remarks>
    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);

        switch (propertyName)
        {
            case nameof(IImageContent.CurrentMipLevel):
            case nameof(IImageContent.CurrentDepthSlice):
                UpdateTextureParameters();
                break;
            case nameof(IImageContent.ImageData):
                // We use this flag to indicate that the texture needs rebuilding.
                // If we fail to do this, repeated hits on the property change will 
                // just rebuild the texture without destroying it causing a leak. 
                if (_textureNeedsUpdate)
                {
                    CreateTexture();
                    _textureNeedsUpdate = false;
                }
                break;
        }
    }

    /// <summary>Handles the PreviewKeyDown event.</summary>
    /// <param name="args">The <see cref="PreviewKeyDownEventArgs"/> instance containing the event data.</param>
    protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs args)
    {
        base.OnPreviewKeyDown(args);

        switch (args.KeyCode)
        {
            case Keys.Left:
                if (!AllowArrayDepthChange)
                {
                    return;
                }

                if (args.Shift)
                {
                    DataContext.CurrentArrayIndex -= (DataContext.ArrayCount / 2).Max(1).Min(6);
                    DataContext.CurrentDepthSlice -= (DataContext.DepthCount / 2).Max(1).Min(6);
                }
                else
                {
                    --DataContext.CurrentArrayIndex;
                    --DataContext.CurrentDepthSlice;
                }
                args.IsInputKey = true;
                return;
            case Keys.Right:
                if (!AllowArrayDepthChange)
                {
                    return;
                }

                if (args.Shift)
                {
                    DataContext.CurrentArrayIndex += (DataContext.ArrayCount / 2).Max(1).Min(6);
                    DataContext.CurrentDepthSlice += (DataContext.DepthCount / 2).Max(1).Min(6);
                }
                else
                {
                    ++DataContext.CurrentArrayIndex;
                    ++DataContext.CurrentDepthSlice;
                }
                args.IsInputKey = true;
                return;
            case Keys.Up:
                if (!AllowMipChange)
                {
                    return;
                }

                if (args.Shift)
                {
                    DataContext.CurrentMipLevel -= 2;
                }
                else
                {
                    --DataContext.CurrentMipLevel;
                }
                args.IsInputKey = true;
                return;
            case Keys.Down:
                if (!AllowMipChange)
                {
                    return;
                }

                if (args.Shift)
                {
                    DataContext.CurrentMipLevel += 2;
                }
                else
                {
                    ++DataContext.CurrentMipLevel;
                }
                args.IsInputKey = true;
                return;
        }
    }

    /// <summary>Function to create resources required for the lifetime of the viewer.</summary>
    public void CreateResources()
    {
        _textureParameters = GorgonConstantBufferView.CreateConstantBuffer(Graphics, new GorgonConstantBufferInfo(TextureParams.Size));

        _imageShader = GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics, Resources.ImageViewShaders, _shaderName);

        OnCreateResources();
    }



    /// <summary>Initializes a new instance of the <see cref="TextureViewer"/> class.</summary>
    /// <param name="name">The name of the renderer.</param>
    /// <param name="shaderName">The name of the shader to use for rendering the texture.</param>
    /// <param name="textureSlot">The slot for our texture in the shader.</param>
    /// <param name="renderer">The main renderer for the content view.</param>
    /// <param name="swapChain">The swap chain for the content view.</param>
    /// <param name="dataContext">The view model to assign to the renderer.</param>        
    protected TextureViewer(string name, string shaderName, int textureSlot, Gorgon2D renderer, GorgonSwapChain swapChain, IImageContent dataContext)
        : base(name, renderer, swapChain, dataContext)
    {
        _shaderName = shaderName;
        _textureSlot = textureSlot.Max(0).Min(_slots.Length - 1);
    }

}
