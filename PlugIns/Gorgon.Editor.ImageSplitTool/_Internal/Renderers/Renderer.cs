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
// Created: August 17, 2020 9:00:20 PM
// 
#endregion

using DX = SharpDX;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.ImageSplitTool.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using Gorgon.Editor.UI;
using Gorgon.Math;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.GdiPlus;
using Gorgon.Graphics.Fonts;

namespace Gorgon.Editor.ImageSplitTool
{
    /// <summary>
    /// The renderer used to draw the preview for the selected image.
    /// </summary>
    internal class Renderer
		: DefaultToolRenderer<ISplit>
	{
		#region Variables.
		// The image used for the preview.
		private GorgonTexture2DView _previewImage;
		// The background image for the preview.
		private GorgonTexture2DView _backgroundImage;
		// Flag to indicate that the texture is loading.
		private bool _loading;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the image to render for previewing.
		/// </summary>
		/// <param name="image">The image to update with.</param>
		private void UpdateRenderImage(IGorgonImage image)
		{
			_previewImage?.Dispose();

			if (image is null)
			{
				_previewImage = null;
				return;
			}

			_previewImage = GorgonTexture2DView.CreateTexture(Graphics, new GorgonTexture2DInfo("Atlas_Sprite_Preview")
			{
				ArrayCount = 1,
				Binding = TextureBinding.ShaderResource,
				Format = image.Format,
				Height = image.Height,
				Width = image.Width,
				Usage = ResourceUsage.Immutable,
				IsCubeMap = false,
				MipLevels = 1
			}, image);
		}

		/// <summary>
		/// Function to retrieve the rectangular region for rendering.
		/// </summary>
		/// <returns>The render area.</returns>
		private DX.Rectangle GetRenderRegion()
		{
			int size;

			if (MainRenderTarget.Width < MainRenderTarget.Height)
			{
				size = MainRenderTarget.Width;
			}
			else
			{
				size = MainRenderTarget.Height;
			}

			int top = (MainRenderTarget.Height / 2) - (size / 2);
			int left = (MainRenderTarget.Width / 2) - (size / 2);

			return new DX.Rectangle(left, top, size, size);
		}

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_backgroundImage?.Dispose();
				_previewImage?.Dispose();
			}

			base.Dispose(disposing);
		}

		/// <summary>Function called when a property on the <see cref="DefaultToolRenderer{T}.DataContext"/> is changing.</summary>
		/// <param name="propertyName">The name of the property that is changing.</param>
		/// <remarks>Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.</remarks>
		protected override void OnPropertyChanging(string propertyName)
		{
			switch (propertyName)
			{
				case nameof(ISplit.PreviewImage):
					_loading = true;
					break;
			}
		}

        /// <summary>Function called when a property on the <see cref="DefaultToolRenderer{T}.DataContext"/> has been changed.</summary>
        /// <param name="propertyName">The name of the property that was changed.</param>
        /// <remarks>Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.</remarks>
        protected override void OnPropertyChanged(string propertyName)
		{
			switch (propertyName)
			{
				case nameof(ISplit.PreviewImage):
					UpdateRenderImage(DataContext.PreviewImage);
					_loading = false;
					break;
			}
		}

        /// <summary>Function to render the background.</summary>
        /// <remarks>Developers can override this method to render a custom background.</remarks>
        protected override void OnRenderBackground()
		{
			MainRenderTarget.Clear(DarkFormsRenderer.WindowBackground);

			var renderRegion = GetRenderRegion().ToRectangleF();

			Renderer.Begin();
			Renderer.DrawFilledRectangle(renderRegion, 
										 ((DataContext.PreviewImage is null) || (_loading)) ? (GorgonColor)DarkFormsRenderer.DarkBackground : GorgonColor.White, 
										 _backgroundImage, 
										 new DX.RectangleF(0, 0, renderRegion.Width / _backgroundImage.Width, renderRegion.Height / _backgroundImage.Height));
			Renderer.End();
		}

		/// <summary>Function to render the content.</summary>
		/// <remarks>This is the method that developers should override in order to draw their content to the view.</remarks>
		protected override void OnRenderContent()
		{
			OnRenderBackground();

			var renderRegion = GetRenderRegion().ToRectangleF();
			var halfClient = new DX.Vector2(renderRegion.Width * 0.5f, renderRegion.Height * 0.5f);

			Renderer.Begin();

			// Render the image.
			if ((DataContext.PreviewImage != null) && (!_loading))
			{
				float scale = (renderRegion.Width / DataContext.PreviewImage.Width).Min(renderRegion.Height / DataContext.PreviewImage.Height);
				float width = DataContext.PreviewImage.Width * scale;
				float height = DataContext.PreviewImage.Height * scale;
				float x = renderRegion.X + halfClient.X - (width * 0.5f);
				float y = renderRegion.Y + halfClient.Y - (height * 0.5f);

				Renderer.DrawFilledRectangle(new DX.RectangleF(x, y, width, height), GorgonColor.White, _previewImage, new DX.RectangleF(0, 0, 1, 1));
			}
			else
			{
				if (!_loading)
				{
					DX.Size2F size = Resources.GORIST_TEXT_SELECT_IMAGE.MeasureText(Renderer.DefaultFont, false);
					Renderer.DrawString(Resources.GORIST_TEXT_SELECT_IMAGE,
															new DX.Vector2(renderRegion.X + halfClient.X - size.Width * 0.5f, renderRegion.Y + halfClient.Y - size.Height * 0.5f),
															color: GorgonColor.White);
				}
				else
				{
					DX.Size2F size = Resources.GORIST_TEXT_LOADING.MeasureText(Renderer.DefaultFont, false);
					Renderer.DrawString(Resources.GORIST_TEXT_LOADING,
															new DX.Vector2(renderRegion.X + halfClient.X - size.Width * 0.5f, renderRegion.Y + halfClient.Y - size.Height * 0.5f),
															color: GorgonColor.White);
				}
			}
			Renderer.End();
		}

		/// <summary>
        /// Function to initialize the renderer.
        /// </summary>
		public void Initialize()
		{
			using (IGorgonImage image = Resources.Transparency_Pattern.ToGorgonImage())
			{
				_backgroundImage = GorgonTexture2DView.CreateTexture(Graphics, new GorgonTexture2DInfo("Background")
				{
					Width = image.Width,
					Height = image.Height,
					Format = image.Format,
					Binding = TextureBinding.ShaderResource,
					Usage = ResourceUsage.Immutable
				}, image);
			}

			if (DataContext.PreviewImage != null)
			{
				UpdateRenderImage(DataContext.PreviewImage);
			}
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>Initializes a new instance of the <see cref="Renderer"/> class.</summary>
		/// <param name="renderer">The 2D renderer for the application.</param>
		/// <param name="swapChain">The swap chain bound to the window.</param>
		/// <param name="dataContext">The data context for the renderer.</param>
		public Renderer(Gorgon2D renderer, GorgonSwapChain swapChain, ISplit dataContext)
			: base("Preview Renderer", renderer, swapChain, dataContext)
		{
			
		}
		#endregion
	}
}
