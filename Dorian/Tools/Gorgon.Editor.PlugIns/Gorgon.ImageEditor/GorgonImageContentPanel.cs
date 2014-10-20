#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Thursday, March 07, 2013 9:51:37 PM
// 
#endregion

using System;
using System.Diagnostics;
using System.Drawing;
using System.Security.Cryptography;
using System.Windows.Forms;
using GorgonLibrary.Editor.ImageEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;
using GorgonLibrary.UI;
using SlimMath;

namespace GorgonLibrary.Editor.ImageEditorPlugIn
{
    /// <summary>
    /// Control for displaying image data.
    /// </summary>
    partial class GorgonImageContentPanel 
		: ContentPanel
	{
		#region Variables.
		// Image content.
        private GorgonImageContent _content;
		// Texture to display.
	    private GorgonTexture _texture;
		// Background texture.
	    private GorgonTexture2D _backgroundTexture;
		// The texture region in screen space.
	    private RectangleF _textureRegion;
        // The current shader view.
        private GorgonTextureShaderView _currentView;
		// The current array index.
	    private int _arrayIndex;
		// The current mip level.
	    private int _mipLevel;
		// The current depth slice.
	    private int _depthSlice;
        #endregion

        #region Methods.
		/// <summary>
		/// Handles the Click event of the buttonEditFileExternal control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonEditFileExternal_Click(object sender, EventArgs e)
		{
			Cursor.Current = Cursors.WaitCursor;

			try
			{
				_content.EditExternal();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				Cursor.Current = Cursors.Default;
			}
		}

		/// <summary>
		/// Handles the Click event of the buttonPrevDepthSlice control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonPrevDepthSlice_Click(object sender, EventArgs e)
		{
			_depthSlice--;

			if (_depthSlice < 0)
			{
				_depthSlice = 0;
			}

			SetDepthSlice();
			ValidateControls();
		}

		/// <summary>
		/// Handles the Click event of the buttonNextDepthSlice control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonNextDepthSlice_Click(object sender, EventArgs e)
		{
			_depthSlice++;

			if (_depthSlice >= _content.Depth)
			{
				_depthSlice = _content.Depth - 1;
			}

			SetDepthSlice();
			ValidateControls();
		}

		/// <summary>
        /// Handles the Click event of the buttonPrevMipLevel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonPrevMipLevel_Click(object sender, EventArgs e)
        {
	        _mipLevel--;

			if (_mipLevel < 0)
            {
				_mipLevel = 0;
            }

            GetCurrentShaderView(_mipLevel, _arrayIndex);
            ValidateControls();
        }

		/// <summary>
		/// Handles the Click event of the buttonNextArrayIndex control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonNextArrayIndex_Click(object sender, EventArgs e)
		{
			_arrayIndex++;

			if (_arrayIndex >= _content.ArrayCount)
			{
				_arrayIndex = _content.ArrayCount - 1;
			}

			GetCurrentShaderView(_mipLevel, _arrayIndex);
			ValidateControls();
		}

		/// <summary>
		/// Handles the Click event of the buttonPrevArrayIndex control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonPrevArrayIndex_Click(object sender, EventArgs e)
		{
			_arrayIndex--;

			if (_arrayIndex < 0)
			{
				_arrayIndex = 0;
			}

			GetCurrentShaderView(_mipLevel, _arrayIndex);
			ValidateControls();
		}

        /// <summary>
        /// Handles the Click event of the buttonNextMipLevel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonNextMipLevel_Click(object sender, EventArgs e)
        {
	        _mipLevel++;

            if (_mipLevel >= _content.MipCount)
            {
				_mipLevel = _content.MipCount - 1;
            }

            GetCurrentShaderView(_mipLevel, _arrayIndex);
            ValidateControls();
        }

        /// <summary>
        /// Function to validate control state.
        /// </summary>
        private void ValidateControls()
        {
	        buttonEditFileExternal.Enabled = !string.IsNullOrWhiteSpace(_content.ExePath);

            buttonPrevMipLevel.Enabled = (_currentView != null) && (_content.MipCount > 1) && (_mipLevel > 0);
            buttonNextMipLevel.Enabled = (_currentView != null) && (_content.MipCount > 1) && (_mipLevel < _content.MipCount - 1);
            buttonPrevArrayIndex.Enabled = (_currentView != null) && (_content.ArrayCount > 1)
                                           && (_arrayIndex > 0);
            buttonNextArrayIndex.Enabled = (_currentView != null) && (_content.ArrayCount > 1)
                                           && (_arrayIndex < _content.ArrayCount - 1);

	        buttonPrevDepthSlice.Enabled = _content.Depth > 1 && _depthSlice > 0;
	        buttonNextDepthSlice.Enabled = _content.Depth > 1 && _depthSlice < _content.Depth - 1;

            // Volume textures don't have array indices.
	        buttonPrevArrayIndex.Visible =
		        buttonNextArrayIndex.Visible =
		        labelArrayIndex.Visible = _content.ImageType != ImageType.Image3D && _content.Codec != null && _content.Codec.SupportsArray;

			buttonPrevMipLevel.Visible = buttonNextMipLevel.Visible = sepMip.Visible = labelMipLevel.Visible = _content.Codec != null && _content.Codec.SupportsMipMaps;

	        buttonPrevDepthSlice.Visible = buttonNextDepthSlice.Visible = labelDepthSlice.Visible = _content.Codec != null && _content.Codec.SupportsDepth && _content.ImageType == ImageType.Image3D;

	        sepArray.Visible = labelArrayIndex.Visible || labelDepthSlice.Visible;
        }

		/// <summary>
		/// Function to update the image information text.
		/// </summary>
	    private void UpdateImageInfo()
	    {
			buttonEditFileExternal.Text = string.IsNullOrWhiteSpace(_content.ExeName)
											  ? Resources.GORIMG_TEXT_EDIT_EXTERNAL
											  : string.Format(Resources.GORIMG_TEXT_EDIT_EXTERNAL_APPNAME, _content.ExeName);

			switch (_content.ImageType)
			{
				case ImageType.Image1D:
					labelImageInfo.Text = string.Format(Resources.GORIMG_TEXT_IMAGE_INFO_1D,
														_texture.Settings.Width,
														_texture.Settings.Format +
														(_content.BlockCompression != BufferFormat.Unknown ? " (" + _content.BlockCompression + ") " : string.Empty));
					break;
				case ImageType.Image2D:
				case ImageType.ImageCube:
					labelImageInfo.Text = string.Format(Resources.GORIMG_TEXT_IMAGE_INFO_2D,
														_texture.Settings.Width,
														_texture.Settings.Height,
														_texture.Settings.Format +
														(_content.BlockCompression != BufferFormat.Unknown ? " (" + _content.BlockCompression + ") " : string.Empty));
					break;
				case ImageType.Image3D:
					labelImageInfo.Text = string.Format(Resources.GORIMG_TEXT_IMAGE_INFO_3D,
														_texture.Settings.Width,
														_texture.Settings.Height,
														_texture.Settings.Depth,
														_texture.Settings.Format +
														(_content.BlockCompression != BufferFormat.Unknown ? " (" + _content.BlockCompression + ") " : string.Empty));
					break;
			}
	    }

		/// <summary>
		/// Function to create a new texture for display.
		/// </summary>
	    private void CreateTexture()
	    {
			if (_texture != null)
			{
				_texture.Dispose();
			}

			switch (_content.ImageType)
			{
				case ImageType.Image1D:
					_texture = ContentObject.Graphics.Textures.CreateTexture<GorgonTexture1D>("DisplayTexture", _content.Image);
					break;
				case ImageType.Image2D:
				case ImageType.ImageCube:
					_texture = ContentObject.Graphics.Textures.CreateTexture<GorgonTexture2D>("DisplayTexture", _content.Image);
					break;
				case ImageType.Image3D:
					_texture = ContentObject.Graphics.Textures.CreateTexture<GorgonTexture3D>("DisplayTexture", _content.Image);
					SetDepthSlice();
					break;
			}

		    if (_currentView != null)
		    {
		        GetCurrentShaderView(_mipLevel < _content.MipCount
		                                 ? _mipLevel
		                                 : _content.MipCount - 1,
		                             _arrayIndex < _content.ArrayCount
		                                 ? _arrayIndex
		                                 : _content.ArrayCount - 1);
		    }
		    else
		    {
                GetCurrentShaderView(0, 0);
		    }

			UpdateImageInfo();
	    }

        /// <summary>
        /// Function to scale the image to the window.
        /// </summary>
        /// <returns>A new rectangle containing the size and location of the scaled image.</returns>
        private RectangleF ScaleImage()
        {
            Vector2 windowSize = panelTextureDisplay.ClientSize;
            var imageSize = new Vector2(_content.Width, _content.Height);
            var location = new Vector2(panelTextureDisplay.ClientSize.Width / 2.0f,
                                       panelTextureDisplay.ClientSize.Height / 2.0f);
            var scale = new Vector2(windowSize.X / imageSize.X, windowSize.Y / imageSize.Y); ;

            if (scale.Y > scale.X)
            {
                scale.Y = scale.X;
            }
            else
            {
                scale.X = scale.Y;
            }

            Vector2.Modulate(ref imageSize, ref scale, out imageSize);
            location.X = location.X - imageSize.X / 2.0f;
            location.Y = location.Y - imageSize.Y / 2.0f;

            return new RectangleF(location, imageSize);
        }

		/// <summary>
		/// Function to set the current depth slice.
		/// </summary>
	    private void SetDepthSlice()
		{
			_content.DepthSlice = _depthSlice;

			labelDepthSlice.Text = string.Format(Resources.GORIMG_TEXT_DEPTH_SLICE, _depthSlice + 1, _content.Depth);
		}

        /// <summary>
        /// Function to retrieve the shader view for the texture.
        /// </summary>
        /// <param name="mipIndex">Index of the mip map level.</param>
        /// <param name="arrayIndex">Index of the array level.</param>
        private void GetCurrentShaderView(int mipIndex, int arrayIndex)
        {
            switch (_content.ImageType)
            {
                case ImageType.Image1D:
                    _currentView = ((GorgonTexture1D)_texture).GetShaderView(_texture.Settings.Format,
                                                                             mipIndex,
                                                                             1,
                                                                             arrayIndex);
					labelArrayIndex.Text = string.Format(Resources.GORIMG_TEXT_ARRAY_INDEX, arrayIndex + 1, _content.ArrayCount);
                    break;
                case ImageType.Image2D:
                    _currentView = ((GorgonTexture2D)_texture).GetShaderView(_texture.Settings.Format,
                                                                             mipIndex,
                                                                             1,
                                                                             arrayIndex);
					labelArrayIndex.Text = string.Format(Resources.GORIMG_TEXT_ARRAY_INDEX, arrayIndex + 1, _content.ArrayCount);
                    break;
                case ImageType.Image3D:
                    _currentView = ((GorgonTexture3D)_texture).GetShaderView(_texture.Settings.Format,
                                                                             mipIndex);
		            arrayIndex = 0;
                    break;
            }

	        labelMipLevel.Text = string.Format(Resources.GORIMG_TEXT_MIP_LEVEL,
	                                           mipIndex + 1, _content.MipCount,
	                                           _content.Image.Buffers[mipIndex, arrayIndex].Width,
	                                           _content.Image.Buffers[mipIndex, arrayIndex].Height);
        }

		/// <summary>
		/// Function to localize the text of the controls on the form.
		/// </summary>
		protected override void LocalizeControls()
		{
			Text = Resources.GORIMG_DESC;
			labelImageInfo.Text = string.Format(Resources.GORIMG_TEXT_IMAGE_INFO_2D, 0, 0, BufferFormat.Unknown);
		    labelMipLevel.Text = string.Format(Resources.GORIMG_TEXT_MIP_LEVEL, 0, 0, 0, 0);
		    labelArrayIndex.Text = string.Format(Resources.GORIMG_TEXT_ARRAY_INDEX, 0, 0);
			labelDepthSlice.Text = string.Format(Resources.GORIMG_TEXT_DEPTH_SLICE, 0, 0);

			buttonEditFileExternal.Text = string.IsNullOrWhiteSpace(_content.ExeName)
				                              ? Resources.GORIMG_TEXT_EDIT_EXTERNAL
				                              : string.Format(Resources.GORIMG_TEXT_EDIT_EXTERNAL_APPNAME, _content.ExeName);
			buttonNextDepthSlice.Text = Resources.GORIMG_TEXT_NEXT_DEPTH;
			buttonPrevDepthSlice.Text = Resources.GORIMG_TEXT_PREV_DEPTH;
			buttonNextMipLevel.Text = Resources.GORIMG_TEXT_NEXT_MIP;
			buttonPrevMipLevel.Text = Resources.GORIMG_TEXT_PREV_MIP;
			buttonPrevArrayIndex.Text = Resources.GORIMG_TEXT_PREV_ARRAY;
			buttonNextArrayIndex.Text = Resources.GORIMG_TEXT_NEXT_ARRAY;
		}

		/// <summary>
		/// Function called when a property is changed on the related content.
		/// </summary>
		/// <param name="propertyName">Name of the property.</param>
		/// <param name="value">New value assigned to the property.</param>
	    protected override void OnContentPropertyChanged(string propertyName, object value)
	    {
			switch (propertyName)
			{
				case "Width":
				case "Height":
				case "Depth":
				case "ImageFormat":
				case "ImageType":
				case "MipCount":
				case "ArrayCount":
				case "ImageEditExternal":
					CreateTexture();
					break;
				case "Codec":
					UpdateImageInfo();
					break;
			}

            ValidateControls();

			base.OnContentPropertyChanged(propertyName, value);
	    }

	    /// <summary>
		/// Function called when the content has changed.
		/// </summary>
		public override void RefreshContent()
		{
			base.RefreshContent();

			_content = Content as GorgonImageContent;

			if (_content == null)
			{
				throw new InvalidCastException(string.Format(Resources.GORIMG_CONTENT_NOT_IMAGE, Content.Name));
			}

		    if ((_content.Image != null) && (_texture == null))
		    {
				CreateTexture();
		    }

			ValidateControls();
		}

		/// <summary>
		/// Function to draw the texture.
		/// </summary>
	    public void Draw()
	    {
		    _content.Renderer.Drawing.BlendingMode = BlendingMode.Modulate;
			_content.Renderer.Drawing.TextureSampler.HorizontalWrapping = TextureAddressing.Wrap;
			_content.Renderer.Drawing.TextureSampler.VerticalWrapping = TextureAddressing.Wrap;

			_content.Renderer.Drawing.Blit(_backgroundTexture,
			                               panelTextureDisplay.ClientRectangle,
			                               _backgroundTexture.ToTexel(panelTextureDisplay.ClientRectangle));

			_content.Renderer.Drawing.TextureSampler.HorizontalWrapping = TextureAddressing.Clamp;
			_content.Renderer.Drawing.TextureSampler.VerticalWrapping = TextureAddressing.Clamp;

			if (_texture == null)
			{
				return;
			}

			switch (_content.ImageType)
			{
				case ImageType.Image1D:
					_content.Renderer.PixelShader.Current = _content.Draw1D;
					break;
				case ImageType.Image2D:
				case ImageType.ImageCube:
					_content.Renderer.PixelShader.Current = _content.Draw2D;
					break;
				case ImageType.Image3D:
					_content.Renderer.PixelShader.Current = _content.Draw3D;
					break;
			}

			// Send some data to render to the GPU.  We'll set the texture -after- this so that we can trick the 2D renderer into using
			// a 1D/3D texture.  This works because the blit does not occur until after we have a state change or we force rendering with "Render".
			var texture2D = _texture as GorgonTexture2D;
            
			_content.Renderer.Drawing.Blit(texture2D ?? _backgroundTexture, ScaleImage());

		    _content.Renderer.PixelShader.Resources[0] = _currentView;

			_content.Renderer.PixelShader.Current = null;
			_content.Renderer.PixelShader.Resources[0] = null;
	    }

		/// <summary>
		/// Function to create the resources used by the content.
		/// </summary>
	    public void CreateResources()
		{
			_backgroundTexture = ContentObject.Graphics.Textures.CreateTexture<GorgonTexture2D>("BackgroundTexture", Resources.Pattern);
		}
		#endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonImageContentPanel"/> class.
        /// </summary>
        public GorgonImageContentPanel()
        {
            InitializeComponent();
        }
        #endregion
	}
}
