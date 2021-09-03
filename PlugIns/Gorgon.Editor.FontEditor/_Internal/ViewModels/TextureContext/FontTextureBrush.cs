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
// Created: September 10, 2021 12:45:27 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DX = SharpDX;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Graphics.Imaging;
using Gorgon.Editor.Content;
using Gorgon.Editor.FontEditor.Properties;
using Gorgon.Math;

namespace Gorgon.Editor.FontEditor
{
    /// <summary>
    /// The view model for the texture brush editor.
    /// </summary>
    internal class FontTextureBrush
        : HostedPanelViewModelBase<FontTextureBrushParameters>, IFontTextureBrush, IFontBrush
    {
        #region Variables.
        // The brush used to paint the glyphs.
        private GorgonGlyphTextureBrush _brush;
        // The texture for the brush.
        private IGorgonImage _texture;
        // The area on the texture to clip.
        private DX.RectangleF _region;
        // The wrapping mode for the edges of the texture.
        private GlyphBrushWrapMode _wrapMode = GlyphBrushWrapMode.Tile;
        // The file manager for the application.
        private ImageLoadService _imageLoader;

        /// <summary>
        /// The default brush.
        /// </summary>
        public static readonly GorgonGlyphTextureBrush DefaultBrush = new(null);
        #endregion

        #region Properties.
        /// <summary>Property to return whether the panel is modal.</summary>
        public override bool IsModal => true;

        /// <summary>Property to set or return the brush used to render the glyphs.</summary>
        public GorgonGlyphTextureBrush Brush
        {
            get => _brush;
            set
            {
                if (value is null)
                {
                    value = DefaultBrush;
                }

                if ((_brush == value) || (value.Equals(_brush)))
                {
                    return;
                }

                OnPropertyChanging();
                _brush = value;
                ExtractBrushData(value);
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the texture rendered into the glyphs.</summary>
        public IGorgonImage Texture => _texture;

        /// <summary>Property to return the region on the texture.</summary>
        public DX.RectangleF Region
        {
            get => _region;
            private set
            {
                if (_region.Equals(ref value))
                {
                    return;
                }

                OnPropertyChanging();
                _region = value;                
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the wrapping mode for the glyph brush.</summary>
        public GlyphBrushWrapMode WrapMode
        {
            get => _wrapMode;
            private set
            {
                if (_wrapMode == value)
                {
                    return;
                }

                OnPropertyChanging();
                _wrapMode = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the command used to assign the boundaries for the texture.</summary>
        public IEditorCommand<DX.RectangleF> SetRegionCommand
        {
            get;
            private set;
        }

        /// <summary>Property to return the command used to assign the wrapping mode.</summary>
        public IEditorCommand<GlyphBrushWrapMode> SetWrappingModeCommand
        {
            get;
        }

        /// <summary>Property to return the command used to load the texture to use for the brush.</summary>
        public IEditorAsyncCommand<SetTextureArgs> LoadTextureCommand
        {
            get;
        }

        /// <summary>
        /// Property to set or return the brush used to render the glyphs.
        /// </summary>
        GorgonGlyphBrush IFontBrush.Brush => Brush;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to extract the information from the current brush.
        /// </summary>
        /// <param name="brush"></param>
        private void ExtractBrushData(GorgonGlyphTextureBrush brush)
        {
            if (brush.Image.Length != 0)
            {
                _texture = brush.ToGorgonImage();
            }

            if (_texture is not null)
            {
                Region = new DX.RectangleF(brush.TextureRegion.X * _texture.Width, brush.TextureRegion.Y * _texture.Height, brush.TextureRegion.Width * _texture.Width, brush.TextureRegion.Height * _texture.Height);
            }
            else
            {
                Region = new DX.RectangleF(0, 0, 1, 1);
            }
            
            WrapMode = brush.WrapMode;   
        }

        /// <summary>
        /// Function to build the texture brush.
        /// </summary>
        private void BuildBrush()
        {
            if (_texture is null)
            {
                return;
            }
            
            GorgonGlyphTextureBrush brush = new(_texture)
            {
                TextureRegion = new DX.RectangleF(_region.X / _texture.Width, _region.Y / _texture.Height, _region.Width / _texture.Width, _region.Height / _texture.Height),
                WrapMode = _wrapMode
            };

            Brush = brush;
        }

        /// <summary>
        /// Function to determine if a texture for the brush can be loaded.
        /// </summary>
        /// <param name="args">The command arguments.</param>
        /// <returns><b>true</b> if the texture can be loaded, <b>false</b> if not.</returns>
        private bool CanLoadTexture(SetTextureArgs args)
        {
            if (!IsActive)
            {
                return false;
            }

            args.Cancel = true;

            if ((string.IsNullOrWhiteSpace(args.TextureFilePath))
                || (!_imageLoader.IsImageFile(args.TextureFilePath)))
            {
                return false;
            }

            return _imageLoader.CanBeUsedForGlyphBrush(args.TextureFilePath);
        }

        /// <summary>
        /// Function to load a texture image for the brush.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        /// <returns>A task for asynchronous operation.</returns>
        private async Task DoLoadTextureAsync(SetTextureArgs args)
        {
            ShowWaitPanel(Resources.GORFNT_TEXT_LOADING);

            try
            {
                NotifyPropertyChanging(nameof(Texture));
                _texture?.Dispose();
                _texture = null;

                _texture = await _imageLoader.LoadImageAsync(args.TextureFilePath);

                NotifyPropertyChanged(nameof(Texture));

                BuildBrush();
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_LOAD_TEXTURE);
            }
            finally
            {
                HideWaitPanel();
            }
        }

        /// <summary>
        /// Function to determine if a region can be set.
        /// </summary>
        /// <param name="region">The region to set.</param>
        /// <returns><b>true</b> if the region can be set, <b>false</b> if not.</returns>
        private bool CanSetRegion(DX.RectangleF region) => (!region.Width.EqualsEpsilon(0)) && (!region.Height.EqualsEpsilon(0));

        /// <summary>
        /// Function to set the region for the texture.
        /// </summary>
        /// <param name="region">The region to set.</param>
        private void DoSetRegion(DX.RectangleF region)
        {
            try
            {
                Region = region;
                BuildBrush();
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_SET_REGION);
            }
        }

        /// <summary>
        /// Function to set the wrapping mode.
        /// </summary>
        /// <param name="wrapMode">The wrapping mode to set.</param>
        private void DoSetWrapMode(GlyphBrushWrapMode wrapMode)
        {
            try
            {
                WrapMode = wrapMode;
                BuildBrush();
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORFNT_ERR_SET_REGION);
            }
        }

        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        protected override void OnInitialize(FontTextureBrushParameters injectionParameters)
        {
            _imageLoader = injectionParameters.ImageLoader;

            ExtractBrushData(DefaultBrush);
        }

        /// <summary>Function called when the associated view is loaded.</summary>
        protected override void OnLoad()
        {
            base.OnLoad();

            ExtractBrushData(_brush ?? DefaultBrush);
        }

        /// <summary>Function called when the associated view is unloaded.</summary>
        /// <remarks>This method is used to perform tear down and clean up of resources.</remarks>
        protected override void OnUnload()
        {
            _texture?.Dispose();
            _brush = DefaultBrush;

            base.OnUnload();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="FontTextureBrush" /> class.</summary>
        public FontTextureBrush()
        {
            LoadTextureCommand = new EditorAsyncCommand<SetTextureArgs>(DoLoadTextureAsync, CanLoadTexture);
            SetRegionCommand = new EditorCommand<DX.RectangleF>(DoSetRegion, CanSetRegion);
            SetWrappingModeCommand = new EditorCommand<GlyphBrushWrapMode>(DoSetWrapMode);
        }
        #endregion
    }
}
