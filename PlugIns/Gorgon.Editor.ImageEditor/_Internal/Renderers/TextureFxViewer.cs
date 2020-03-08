#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: February 8, 2020 9:27:54 PM
// 
#endregion

using DX = SharpDX;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using System.Diagnostics;
using Gorgon.Graphics.Imaging;
using Gorgon.Core;
using Gorgon.Math;
using Gorgon.Editor.Rendering;
using System.Windows.Forms;
using System.ComponentModel;
using Gorgon.Editor.ImageEditor.Fx;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// A renderer used to display texture content with effects.
    /// </summary>
    internal class TextureFxViewer
        : TextureViewer
    {
        #region Constants.
        /// <summary>
        /// The effects renderer.
        /// </summary>
        public const string ViewerName = "ContextFx";
        #endregion

        #region Variables.
        // The number of blur passes.
        private int _passes = 1;
        // The amount to sharpen.
        private int _sharpAmount = 50;
        // The amount to sharpen.
        private int _embossAmount = 50;
        // The threshold for the edge detect effect.
        private int _edgeThreshold = 50;
        // The offset for the edge detect lines.
        private float _edgeOffset = 1.0f;
        // The color for the lines in the edge detect effect.
        private GorgonColor _edgeColor = GorgonColor.Black;
        // Flag to indicate that the edge detection effect is overlaid on top of the original image.
        private bool _edgeOverlay = true;
        // The service used to apply effects.
        private readonly IFxPreviewer _fxPreviewer;
        #endregion

        #region Methods.
        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                DataContext.FxContext.BlurSettings.PropertyChanged -= FxBlurSettings_PropertyChanged;
                DataContext.FxContext.SharpenSettings.PropertyChanged -= SharpenSettings_PropertyChanged;
                DataContext.FxContext.EmbossSettings.PropertyChanged -= EmbossSettings_PropertyChanged;
                DataContext.FxContext.EdgeDetectSettings.PropertyChanged -= EdgeDetectSettings_PropertyChanged;
            }
            base.Dispose(disposing);
        }

        /// <summary>Handles the PropertyChanged event of the EdgeDetectSettings control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void EdgeDetectSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IFxEdgeDetect.Threshold):
                    _edgeThreshold = DataContext.FxContext.EdgeDetectSettings.Threshold;
                    break;
                case nameof(IFxEdgeDetect.Offset):
                    _edgeOffset = DataContext.FxContext.EdgeDetectSettings.Offset;
                    break;
                case nameof(IFxEdgeDetect.LineColor):
                    _edgeColor = DataContext.FxContext.EdgeDetectSettings.LineColor;
                    break;
                case nameof(IFxEdgeDetect.Overlay):
                    _edgeOverlay = DataContext.FxContext.EdgeDetectSettings.Overlay;
                    break;
            }
            RenderFx();
        }

        /// <summary>Handles the PropertyChanged event of the SharpenSettings control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void SharpenSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IFxSharpen.Amount):
                    _sharpAmount = DataContext.FxContext.SharpenSettings.Amount;
                    RenderFx();
                    break;
            }
        }

        /// <summary>Handles the PropertyChanged event of the EmbossSettings control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void EmbossSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IFxEmboss.Amount):
                    _embossAmount = DataContext.FxContext.EmbossSettings.Amount;
                    RenderFx();
                    break;
            }
        }

        /// <summary>Handles the PropertyChanged event of the FxBlurSettings control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void FxBlurSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IFxBlur.BlurAmount):
                    _passes = DataContext.FxContext.BlurSettings.BlurAmount;
                    RenderFx();
                    break;
            }            
        }

        /// <summary>
        /// Function to render the effects to the preview.
        /// </summary>
        private void RenderFx()
        {
            if (DataContext.CurrentHostedPanel == DataContext.FxContext.BlurSettings)
            {
                _fxPreviewer.GenerateBlurPreview(_passes);
            }
            else if (DataContext.CurrentHostedPanel == DataContext.FxContext.SharpenSettings)
            {
                _fxPreviewer.GenerateSharpenEmbossPreview(_sharpAmount, false);
            }
            else if (DataContext.CurrentHostedPanel == DataContext.FxContext.EmbossSettings)
            {
                _fxPreviewer.GenerateSharpenEmbossPreview(_embossAmount, true);
            }
            else if (DataContext.CurrentHostedPanel == DataContext.FxContext.EdgeDetectSettings)
            {
                _fxPreviewer.GenerateEdgeDetectPreview(_edgeThreshold, _edgeOffset, _edgeColor, _edgeOverlay);
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
                case nameof(IImageContent.CurrentHostedPanel):
                    RenderFx();
                    break;
            }
        }

        /// <summary>Function to render the content.</summary>
        /// <remarks>This is the method that developers should override in order to draw their content to the view.</remarks>
        protected override void DrawTexture()
        {
            Graphics.SetRenderTarget(MainRenderTarget);

            var color = new GorgonColor(GorgonColor.White, Opacity);            

            Renderer.Begin(BatchState, Camera);
            Renderer.DrawFilledRectangle(new DX.RectangleF(RenderRegion.Width * -0.5f,
                                                           RenderRegion.Height * -0.5f,
                                                           RenderRegion.Width,
                                                           RenderRegion.Height),
                                        color,
                                        DataContext.CurrentHostedPanel == null ? _fxPreviewer.OriginalTexture : _fxPreviewer.PreviewTexture,
                                        new DX.RectangleF(0, 0, 1, 1),
                                        DataContext.CurrentArrayIndex,
                                        textureSampler: GorgonSamplerState.PointFiltering);
            Renderer.End();
        }

        /// <summary>Function to create the texture for display.</summary>
        protected override void CreateTexture()
        {
            if (_fxPreviewer.PreviewTexture == null)
            {
                return;
            }

            RenderRegion = new DX.RectangleF(0, 0, _fxPreviewer.PreviewTexture.Width, _fxPreviewer.PreviewTexture.Height);

            // Render previews.
            RenderFx();
        }

        /// <summary>Function to destroy the texture when done with it.</summary>
        protected override void DestroyTexture()
        {
            // Textures are handled in the previewer.
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="TextureFxViewer"/> class.</summary>
        /// <param name="renderer">The main renderer for the content view.</param>
        /// <param name="swapChain">The swap chain for the content view.</param>
        /// <param name="dataContext">The view model to assign to the renderer.</param>
        public TextureFxViewer(Gorgon2D renderer, GorgonSwapChain swapChain, IImageContent dataContext)
            : base(ViewerName, "Gorgon2DTextureArrayView", 0, renderer, swapChain, dataContext)
        {
            _fxPreviewer = (IFxPreviewer)dataContext.FxContext.FxService;
            dataContext.FxContext.BlurSettings.PropertyChanged += FxBlurSettings_PropertyChanged;
            dataContext.FxContext.SharpenSettings.PropertyChanged += SharpenSettings_PropertyChanged;
            dataContext.FxContext.EmbossSettings.PropertyChanged += EmbossSettings_PropertyChanged;
            dataContext.FxContext.EdgeDetectSettings.PropertyChanged += EdgeDetectSettings_PropertyChanged;
            AllowArrayDepthChange = false;
            AllowMipChange = false;
        }        
        #endregion
    }
}
