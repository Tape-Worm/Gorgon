#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Tuesday, November 11, 2014 11:48:20 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GorgonLibrary.Design;
using GorgonLibrary.Editor.SpriteEditorPlugIn.Controls;
using GorgonLibrary.Editor.SpriteEditorPlugIn.Design;
using GorgonLibrary.Editor.SpriteEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using GorgonLibrary.Renderers;
using GorgonLibrary.UI;
using SlimMath;

namespace GorgonLibrary.Editor.SpriteEditorPlugIn
{
	/// <summary>
	/// Content for a sprite object.
	/// </summary>
	class GorgonSpriteContent
		: ContentObject
	{
		#region Constants.
		/// <summary>
		/// Dependency type name for the sprite texture. 
		/// </summary>
		public const string TextureDependencyType = "SpriteTexture";
		#endregion

		#region Variables.
		// Flag to indicate that the object was disposed.
		private bool _disposed;
		// The plug-in for this content.
		private GorgonSpriteEditorPlugIn _plugIn;
		// The main UI panel used to edit the sprite.
		private PanelSpriteEditor _panel;
		// The swap chain to display graphics data in the panel.
		private GorgonSwapChain _swap;
		// Background pattern texture.
		private GorgonTexture2D _background;
		// The anchor point for the sprite.
		private Point _spriteAnchor;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the working copy of the sprite.
		/// </summary>
		[Browsable(false)]
		public GorgonSprite Sprite
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the texture bound to the sprite.
		/// </summary>
		[LocalCategory(typeof(Resources), "CATEGORY_APPEARANCE"),
		LocalDescription(typeof(Resources), "PROP_TEXTURE_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_TEXTURE_NAME"),
		TypeConverter(typeof(TextureTypeConverter)),
		Editor(typeof(TextureEditor), typeof(UITypeEditor))]
		public GorgonTexture2D Texture
		{
			get
			{
				return Sprite == null ? null : Sprite.Texture;
			}
			set
			{
				if (Sprite == null)
				{
					return;
				}

				if ((value == null) && (Sprite.Texture == null))
				{
					return;
				}

				if ((value != null) && (Sprite.Texture != null) && (string.Equals(value.Name, Sprite.Texture.Name, StringComparison.OrdinalIgnoreCase)))
				{
					return;
				}

				// Clear out the old sprite texture.
				if (Sprite.Texture != null)
				{
					Sprite.Texture.Dispose();
					Sprite.Texture = null;
				}

				Sprite.Texture = value;
				Sprite.GetDeferredTexture();

				NotifyPropertyChanged();
			}
		}

		/// <summary>
		/// Property to return whether this content has properties that can be manipulated in the properties tab.
		/// </summary>
		[Browsable(false)]
		public override bool HasProperties
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Property to return the type of content.
		/// </summary>
		[Browsable(false)]
		public override string ContentType
		{
			get
			{
				return Resources.GORSPR_CONTENT_TYPE;
			}
		}

		/// <summary>
		/// Property to return whether the content object supports a renderer interface.
		/// </summary>
		[Browsable(false)]
		public override bool HasRenderer
		{
			get
			{
				return true;
			}
		}

		/// <summary>
		/// Property to return the renderer interface.
		/// </summary>
		[Browsable(false)]
		public Gorgon2D Renderer
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources.
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					GorgonSpriteEditorPlugIn.Settings.Save();

					// Clear any external dependencies.
					EditorFile = null;

					if (_background != null)
					{
						_background.Dispose();
					}

					if (_swap != null)
					{
						_swap.Dispose();
					}

					if (Renderer != null)
					{
						Renderer.Dispose();
					}
				}
			}

			_disposed = true;
			Texture = null;
			_background = null;
			Renderer = null;
			_swap = null;

			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to persist the content data to a stream.
		/// </summary>
		/// <param name="stream">Stream that will receive the data.</param>
		protected override void OnPersist(Stream stream)
		{
			Sprite.Save(stream);
		}

		/// <summary>
		/// Function to read the content data from a stream.
		/// </summary>
		/// <param name="stream">Stream containing the content data.</param>
		protected override void OnRead(Stream stream)
		{
			// Load the sprite.
			Sprite = Renderer.Renderables.FromStream<GorgonSprite>(Name, stream);

			// If the texture is deferred, attempt to load.
			Sprite.GetDeferredTexture();

			// Convert the anchor to integer values and store separately.
			// We turn off anchoring for our display sprite so that we can edit the anchor
			// without doing all kinds of weird stuff to display the sprite correctly.
			_spriteAnchor = (Point)Sprite.Anchor;

			// Disable the sprite anchor.
			Sprite.Anchor = Vector2.Zero;
			Sprite.Scale = new Vector2(1);
			Sprite.Position = Vector2.Zero;
		}

		/// <summary>
		/// Function called when the content is reverted back to its original state.
		/// </summary>
		/// <returns>
		/// TRUE if reverted, FALSE if not.
		/// </returns>
		protected override bool OnRevert()
		{
			if (!HasChanges)
			{
				return false;
			}

			Dependencies.Clear();
			Reload();

			return true;
		}

		/// <summary>
		/// Function to load a dependency file.
		/// </summary>
		/// <param name="dependency">The dependency to load.</param>
		/// <param name="stream">Stream containing the dependency file.</param>
		/// <returns>
		/// The result of the load operation.  If the dependency loaded correctly, then the developer should return a successful result.  If the dependency
		/// is not vital to the content, then the developer can return a continue result, otherwise the developer should return fatal result and the content will
		/// not continue loading.
		/// </returns>
		protected override DependencyLoadResult OnLoadDependencyFile(Dependency dependency, Stream stream)
		{
			if (!string.Equals(dependency.Type, TextureDependencyType, StringComparison.OrdinalIgnoreCase))
			{
				return new DependencyLoadResult(DependencyLoadState.ErrorContinue,
												string.Format(Resources.GORSPR_ERR_UNKNOWN_DEPENDENCY, dependency.Type));
			}

			using (IImageEditorContent imageContent = ImageEditor.ImportContent(dependency.EditorFile, stream))
			{
				if (imageContent.Image.Settings.ImageType != ImageType.Image2D)
				{
					return new DependencyLoadResult(DependencyLoadState.ErrorContinue, string.Format(Resources.GORSPR_ERR_2D_TEXTURE_ONLY));
				}

				if (imageContent.Image == null)
				{
					return new DependencyLoadResult(DependencyLoadState.ErrorContinue, string.Format(Resources.GORSPR_WARN_NO_TEXTURE, Name, dependency.EditorFile.FilePath));
				}

				if (!Dependencies.Contains(dependency.EditorFile, dependency.Type))
				{
					Dependencies[dependency.EditorFile, dependency.Type] = dependency.Clone();
				}
				
				Dependencies.CacheDependencyObject(dependency.EditorFile,
				                                   dependency.Type,
				                                   Graphics.Textures.CreateTexture<GorgonTexture2D>(dependency.EditorFile.FilePath, imageContent.Image));

				return new DependencyLoadResult(DependencyLoadState.Successful, null);
			}
		}

		/// <summary>
		/// Function called when the content is being initialized.
		/// </summary>
		/// <returns>
		/// A control to place in the primary interface window.
		/// </returns>
		protected override ContentPanel OnInitialize()
		{
			_panel = new PanelSpriteEditor(this, GetRawInput());

			_swap = Graphics.Output.CreateSwapChain("SpriteEditorSwap",
			                                        new GorgonSwapChainSettings
			                                        {
														Window = _panel.panelSprite,
														Format = BufferFormat.R8G8B8A8_UIntNormal
			                                        });

			Renderer = Graphics.Output.Create2DRenderer(_swap);

			_background = Graphics.Textures.CreateTexture<GorgonTexture2D>("BackgroundTexture", Resources.Pattern);

			return _panel;
		}

		/// <summary>
		/// Function to draw the interface for the content editor.
		/// </summary>
		public override void Draw()
		{
			Renderer.Target = _swap;

			Renderer.Drawing.TextureSampler.HorizontalWrapping = TextureAddressing.Wrap;
			Renderer.Drawing.TextureSampler.VerticalWrapping = TextureAddressing.Wrap;

			Renderer.Drawing.Blit(_background, _panel.ClientRectangle, _background.ToTexel(_panel.ClientRectangle));

			_panel.Draw();

			Renderer.Render(1);
		}

		/// <summary>
		/// Function to retrieve a thumbnail image for the content plug-in.
		/// </summary>
		/// <returns>
		/// The image for the thumbnail of the content.
		/// </returns>
		/// <remarks>
		/// The size of the thumbnail should be set to 128x128.
		/// </remarks>
		public override Image GetThumbNailImage()
		{
			return (Image)Resources.sprite_128x128.Clone();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSpriteContent"/> class.
		/// </summary>
		/// <param name="plugIn">The plug in for the content.</param>
		/// <param name="settings">The settings used to create the content.</param>
		public GorgonSpriteContent(GorgonSpriteEditorPlugIn plugIn, GorgonSpriteContentSettings settings)
			: base(settings)
		{
			PlugIn = _plugIn = plugIn;
			HasThumbnail = true;
		}
		#endregion
	}
}
