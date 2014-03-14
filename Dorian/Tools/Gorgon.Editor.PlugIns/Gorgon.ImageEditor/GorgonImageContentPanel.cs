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
	    private GorgonTexture2D _texture;
		// Background texture.
	    private GorgonTexture2D _backgroundTexture;
		// The texture region in screen space.
	    private RectangleF _textureRegion;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to validate control state.
        /// </summary>
        private void ValidateControls()
        {
            
        }

		/// <summary>
		/// Function to localize the text of the controls on the form.
		/// </summary>
		protected override void LocalizeControls()
		{
			Text = Resources.GORIMG_DESC;
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
				_texture = _content.Graphics.Textures.CreateTexture<GorgonTexture2D>("DisplayTexture", _content.Image);
				_textureRegion = new RectangleF(Vector2.Zero, _texture.Settings.Size);
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

			_content.Renderer.Drawing.Blit(_texture, _textureRegion);
	    }

		/// <summary>
		/// Function to create the resources used by the content.
		/// </summary>
	    public void CreateResources()
		{
			_backgroundTexture = _content.Graphics.Textures.CreateTexture<GorgonTexture2D>("BackgroundTexture", Resources.Pattern);
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
