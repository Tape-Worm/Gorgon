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
using GorgonLibrary.Editor.ImageEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;
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
        #endregion

        #region Methods.
        /// <summary>
        /// Handles the Click event of the buttonPrevMipLevel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonPrevMipLevel_Click(object sender, EventArgs e)
        {
            int currentMip = (_currentView.MipStart - 1);

            if (currentMip < 0)
            {
                currentMip = 0;
            }

            GetCurrentShaderView(currentMip, _currentView.ArrayStart);

            labelMipLevel.Text = string.Format(Resources.GORIMG_TEXT_MIP_LEVEL, currentMip);

            ValidateControls();
        }

        /// <summary>
        /// Handles the Click event of the buttonNextMipLevel control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void buttonNextMipLevel_Click(object sender, EventArgs e)
        {
            int currentMip = (_currentView.MipStart + 1);

            if (currentMip >= _content.MipCount)
            {
                currentMip = _content.MipCount - 1;
            }

            GetCurrentShaderView(currentMip, _currentView.ArrayStart);

            labelMipLevel.Text = string.Format(Resources.GORIMG_TEXT_MIP_LEVEL, currentMip);

            ValidateControls();
        }

        /// <summary>
        /// Function to validate control state.
        /// </summary>
        private void ValidateControls()
        {
            buttonPrevMipLevel.Enabled = (_currentView != null) && (_content.MipCount > 1) && (_currentView.MipStart > 0);
            buttonNextMipLevel.Enabled = (_currentView != null) && (_content.MipCount > 1) && (_currentView.MipStart < _content.MipCount);
            buttonPrevArrayIndex.Enabled = (_currentView != null) && (_content.ArrayCount > 1)
                                           && (_currentView.ArrayStart > 0);
            buttonNextArrayIndex.Enabled = (_currentView != null) && (_content.ArrayCount > 1)
                                           && (_currentView.ArrayStart < _content.ArrayCount);

            // Volume textures don't have array indices.
            buttonPrevArrayIndex.Visible =
                buttonNextArrayIndex.Visible =
                sepArray.Visible = labelArrayIndex.Visible = _content.ImageType != ImageType.Image3D;
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
					break;
			}

			Debug.Assert(_texture != null, "Texture is NULL");

		    _textureRegion = new RectangleF(0, 0, _texture.Settings.Width, _texture.Settings.Height);

		    if (_currentView != null)
		    {
		        GetCurrentShaderView(_currentView.MipStart < _content.MipCount
		                                 ? _currentView.MipStart
		                                 : _content.MipCount - 1,
		                             _currentView.ArrayStart < _content.ArrayCount
		                                 ? _currentView.ArrayStart
		                                 : _content.ArrayCount - 1);
		    }
		    else
		    {
                GetCurrentShaderView(0, 0);
		    }
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
                    break;
                case ImageType.Image2D:
                    _currentView = ((GorgonTexture2D)_texture).GetShaderView(_texture.Settings.Format,
                                                                             mipIndex,
                                                                             1,
                                                                             arrayIndex);
                    break;
                case ImageType.Image3D:
                    _currentView = ((GorgonTexture3D)_texture).GetShaderView(_texture.Settings.Format,
                                                                             mipIndex);
                    break;
            }
        }

		/// <summary>
		/// Function to localize the text of the controls on the form.
		/// </summary>
		protected override void LocalizeControls()
		{
			Text = Resources.GORIMG_DESC;
		    labelMipLevel.Text = string.Format(Resources.GORIMG_TEXT_MIP_LEVEL, 0);
		    labelArrayIndex.Text = string.Format(Resources.GORIMG_TEXT_ARRAY_INDEX, 0);
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
				case "ImageType":
					CreateTexture();
					break;
			}

            ValidateControls();
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

		    if (_content.Image != null)
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
