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
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using Gorgon.Design;
using Gorgon.Editor.Design;
using Gorgon.Editor.SpriteEditorPlugIn.Controls;
using Gorgon.Editor.SpriteEditorPlugIn.Design;
using Gorgon.Editor.SpriteEditorPlugIn.Properties;
using Gorgon.Graphics;
using Gorgon.Math;
using Gorgon.Renderers;

namespace Gorgon.Editor.SpriteEditorPlugIn
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
		// The anchor point for the sprite.
		private Point _spriteAnchor;
		// The current blending mode.
		private BlendingMode _blendMode = BlendingMode.Modulate;
		// The texture rectangle, in pixel space.
		private Rectangle _textureRect;
		// The settings for the sprite.
		private GorgonSpriteContentSettings _settings;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the background pattern texture.
		/// </summary>
		[Browsable(false)]
		public GorgonTexture2D BackgroundTexture
		{
			get;
			private set;
		}

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
		/// Property to return the sprite used to display the texture.
		/// </summary>
		[Browsable(false)]
		public GorgonSprite TextureSprite
		{
			get;
			private set;
		}

        /// <summary>
        /// Property to return the vertices for the sprite.
        /// </summary>
        [TypeConverter(typeof(SpriteVerticesTypeConverter)),
        LocalCategory(typeof(Resources), "CATEGORY_DESIGN"),
		LocalDisplayName(typeof(Resources), "PROP_SPRITE_VERTICES_NAME"),
		LocalDescription(typeof(Resources), "PROP_SPRITE_VERTICES_DESC")]
	    public SpriteVertices Vertices
	    {
	        get;
            private set;
	    }

        /// <summary>
        /// Property to set or return whether the sprite is flipped horizontally or not.
        /// </summary>
        [DefaultValue(false),
        LocalCategory(typeof(Resources), "CATEGORY_APPEARANCE"),
        LocalDisplayName(typeof(Resources), "PROP_SPRITE_FLIPHORZ_NAME"),
        LocalDescription(typeof(Resources), "PROP_SPRITE_FLIPHORZ_DESC")]
	    public bool FlipHorizontal
	    {
	        get
	        {
	            return Sprite != null && Sprite.HorizontalFlip;
	        }
            set
            {
                if ((Sprite == null)
                    || (Sprite.HorizontalFlip == value))
                {
                    return;
                }

                Sprite.HorizontalFlip = value;

                NotifyPropertyChanged();
            }
	    }

        /// <summary>
        /// Property to set or return whether the sprite is flipped vertically or not.
        /// </summary>
        [DefaultValue(false),
        LocalCategory(typeof(Resources), "CATEGORY_APPEARANCE"),
        LocalDisplayName(typeof(Resources), "PROP_SPRITE_FLIPVERT_NAME"),
        LocalDescription(typeof(Resources), "PROP_SPRITE_FLIPVERT_DESC")]
	    public bool FlipVertical
	    {
	        get
	        {
	            return Sprite != null && Sprite.VerticalFlip;
	        }
            set
            {
                if ((Sprite == null)
                    || (Sprite.VerticalFlip == value))
                {
                    return;
                }

                Sprite.VerticalFlip = value;

                NotifyPropertyChanged();
            }
        }

		/// <summary>
		/// Property to set or return the minimum value for alpha testing.
		/// </summary>
		[DefaultValue(0),
		LocalCategory(typeof(Resources), "CATEGORY_APPEARANCE"),
		LocalDisplayName(typeof(Resources), "PROP_SPRITE_MINALPHA_NAME"),
		LocalDescription(typeof(Resources), "PROP_SPRITE_ALPHATEST_DESC")]
		public byte MinAlphaTest
		{
			get
			{
				return Sprite == null ? (byte)0 : (byte)(Sprite.AlphaTestValues.Minimum * 255);
			}
			set
			{
				float realValue = value / 255.0f;

				if ((Sprite.AlphaTestValues.Maximum.EqualsEpsilon(realValue))
					|| (Sprite == null))
				{
					return;
				}

				if (realValue > Sprite.AlphaTestValues.Maximum)
				{
					realValue = Sprite.AlphaTestValues.Maximum;
				}

				Sprite.AlphaTestValues = new GorgonRangeF(realValue, Sprite.AlphaTestValues.Maximum);

				NotifyPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the minimum value for alpha testing.
		/// </summary>
		[DefaultValue(0),
		LocalCategory(typeof(Resources), "CATEGORY_APPEARANCE"),
		LocalDisplayName(typeof(Resources), "PROP_SPRITE_MAXALPHA_NAME"),
		LocalDescription(typeof(Resources), "PROP_SPRITE_ALPHATEST_DESC")]
		public byte MaxAlphaTest
		{
			get
			{
				return Sprite == null ? (byte)0 : (byte)(Sprite.AlphaTestValues.Maximum * 255);
			}
			set
			{
				float realValue = value / 255.0f;

				if ((Sprite.AlphaTestValues.Maximum.EqualsEpsilon(realValue))
					|| (Sprite == null))
				{
					return;
				}

				if (realValue < Sprite.AlphaTestValues.Minimum)
				{
					realValue = Sprite.AlphaTestValues.Minimum;
				}

				Sprite.AlphaTestValues = new GorgonRangeF(Sprite.AlphaTestValues.Minimum, realValue);

				NotifyPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return a preset blending mode.
		/// </summary>
		[RefreshProperties(RefreshProperties.All),
		DefaultValue(typeof(BlendingMode), "Modulate"),
		LocalCategory(typeof(Resources), "CATEGORY_APPEARANCE"),
		LocalDisplayName(typeof(Resources), "PROP_SPRITE_BLEND_PRESET_NAME"),
		LocalDescription(typeof(Resources), "PROP_SPRITE_BLEND_PRESET_DESC")]
		public BlendingMode BlendPreset
		{
			get
			{
				return _blendMode;
			}
			set
			{
				if (_blendMode == value)
				{
					return;
				}

				_blendMode = Sprite.BlendingMode = value;
				
				NotifyPropertyChanged();
				System.ComponentModel.TypeDescriptor.Refresh(Sprite.Blending);
				ValidateSpriteProperties();
			}
		}

		/// <summary>
		/// Property to return the texture sampling states for the sprite.
		/// </summary>
		[
		LocalCategory(typeof(Resources), "CATEGORY_TEXTURE"),
		LocalDisplayName(typeof(Resources), "PROP_TEXTURE_SAMPLER_NAME"),
		LocalDescription(typeof(Resources), "PROP_TEXTURE_SAMPLER_DESC"),
		TypeConverter(typeof(TextureSamplerTypeConverter))]
		public GorgonRenderable.TextureSamplerState TextureSampler
		{
			get
			{
				return Sprite == null ? null : Sprite.TextureSampler;
			}
		}

		/// <summary>
		/// Property to return the depth states for the sprite.
		/// </summary>
		[TypeConverter(typeof(DepthTypeConverter)),
		LocalCategory(typeof(Resources), "CATEGORY_BEHAVIOR"),
		LocalDisplayName(typeof(Resources), "PROP_SPRITE_DEPTH_NAME"),
		LocalDescription(typeof(Resources), "PROP_SPRITE_DEPTH_DESC")]
		public GorgonRenderable.DepthStencilStates Depth
		{
			get
			{
				return Sprite == null ? null : Sprite.DepthStencil;
			}
		}

		/// <summary>
		/// Property to return the stencil states for the sprite.
		/// </summary>
		[TypeConverter(typeof(StencilTypeConverter)),
		LocalCategory(typeof(Resources), "CATEGORY_BEHAVIOR"),
		LocalDisplayName(typeof(Resources), "PROP_SPRITE_STENCIL_NAME"),
		LocalDescription(typeof(Resources), "PROP_SPRITE_STENCIL_DESC")]
		public GorgonRenderable.DepthStencilStates Stencil
		{
			get
			{
				return Sprite == null ? null : Sprite.DepthStencil;
			}
		}

		/// <summary>
		/// Property to set or return the blending state(s) for the sprite.
		/// </summary>
		[TypeConverter(typeof(BlendingTypeConverter)),
		LocalCategory(typeof(Resources), "CATEGORY_APPEARANCE"),
		LocalDisplayName(typeof(Resources), "PROP_SPRITE_BLENDING_NAME"),
		LocalDescription(typeof(Resources), "PROP_SPRITE_BLENDING_DESC")]
		public GorgonRenderable.BlendState Blending
		{
			get
			{
				return Sprite == null ? null : Sprite.Blending;
			}
		}

		/// <summary>
		/// Property to set or return the size of the sprite.
		/// </summary>
		[LocalCategory(typeof(Resources), "CATEGORY_DESIGN"),
		LocalDescription(typeof(Resources), "PROP_SPRITE_SIZE_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_SPRITE_SIZE_NAME"),
		TypeConverter(typeof(SizeConverter)),
		DefaultValue(typeof(Size), "0, 0")]
		public Size Size
		{
			get
			{
				if (Sprite == null)
				{
					return Size.Empty;
				}

				return (Size)Sprite.Size;
			}
			set
			{
				if ((Sprite == null)
				    || (Sprite.Size == value))
				{
					return;
				}

				Sprite.Size = value;
				RefreshProperty("Size");

				NotifyPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the sprite offset into the bound texture.
		/// </summary>
		[LocalCategory(typeof(Resources), "CATEGORY_TEXTURE"),
		LocalDescription(typeof(Resources), "PROP_TEXTURE_REGION_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_TEXTURE_REGION_NAME"),
		RefreshProperties(RefreshProperties.All),
		TypeConverter(typeof(RectangleConverter)),
		DefaultValue(typeof(Rectangle), "0, 0, 0, 0")]
		public Rectangle TextureRegion
		{
			get
			{
				return _textureRect;
			}
			set
			{
				if (_textureRect == value)
				{
					return;
				}

				_textureRect = value;
				RefreshProperty("TextureRegion");

				if ((Sprite == null)
					|| (Sprite.Texture == null))
				{
					return;
				}

				RectangleF texelRect = Sprite.Texture.ToTexel(value);

				Sprite.TextureRegion = texelRect;

				NotifyPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return anchor for the sprite.
		/// </summary>
		[LocalCategory(typeof(Resources), "CATEGORY_DESIGN"),
		LocalDescription(typeof(Resources), "PROP_SPRITE_ANCHOR_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_SPRITE_ANCHOR_NAME"),
		TypeConverter(typeof(PointConverter)), 
		DefaultValue(typeof(Point), "0, 0"),
		Editor(typeof(AnchorTypeEditor), typeof(UITypeEditor))]
		public Point Anchor
		{
			get
			{
				return _spriteAnchor;
			}
			set
			{
				if (_spriteAnchor == value)
				{
					return;
				}
				
				_spriteAnchor = value;

				NotifyPropertyChanged();
			}
		}

		/// <summary>
		/// Property to return the texture bound to the sprite.
		/// </summary>
		[LocalCategory(typeof(Resources), "CATEGORY_TEXTURE"),
		LocalDescription(typeof(Resources), "PROP_TEXTURE_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_TEXTURE_NAME"),
		TypeConverter(typeof(TextureTypeConverter)),
		Editor(typeof(TextureTypeEditor), typeof(UITypeEditor))]
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

				// Update the texture region on the sprite so that the pixel coordinates stay the same
				// relative to the new texture size.
				if (Sprite.Texture != null)
				{
					Sprite.TextureRegion = Sprite.Texture.ToTexel(_textureRect);
					TextureSprite.Texture = Sprite.Texture;
					TextureSprite.Size = Sprite.Texture.Settings.Size;
					TextureSprite.Color = new GorgonColor(1, 1, 1, 0.35f);
				}
				else
				{
					TextureSprite.Texture = null;
					TextureSprite.Color = GorgonColor.Transparent;
				}

				NotifyPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the color for the sprite.
		/// </summary>
		[LocalCategory(typeof(Resources), "CATEGORY_APPEARANCE"),
		LocalDescription(typeof(Resources), "PROP_SPRITE_COLOR_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_SPRITE_COLOR_NAME"),
		TypeConverter(typeof(RGBATypeConverter)),
		Editor(typeof(RGBAEditor), typeof(UITypeEditor)),
		DefaultValue(typeof(Color), "#FFFFFFFF")]
		public Color Color
		{
			get
			{
				return (Sprite == null || Sprite.Color == GorgonColor.White) ? Color.White : Sprite.Color.ToColor();
			}
			set
			{
				if ((Sprite == null)
				    || (Sprite.Color.ToColor() == value))
				{
					return;
				}

				Sprite.Color = value;

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
		/// Function to validate the properties on the property grid for this sprite.
		/// </summary>
		private void ValidateSpriteProperties()
		{
			DisableProperty("TextureRegion", Sprite.Texture == null);
			DisableProperty("Blending", _blendMode != BlendingMode.Custom);
			DisableProperty("TextureSampler", Texture == null);
		}

		/// <summary>
		/// Function to initialize the sprite object.
		/// </summary>
		private void InitSprite()
		{
			// Convert the anchor to integer values and store separately.
			// We turn off anchoring for our display sprite so that we can edit the anchor
			// without doing all kinds of weird stuff to display the sprite correctly.
			_spriteAnchor = (Point)Sprite.Anchor;
			_blendMode = Sprite.BlendingMode;

			// Disable the sprite anchor.
			Sprite.Anchor = Vector2.Zero;
			Sprite.Scale = new Vector2(1);
			Sprite.Position = Vector2.Zero;

			TextureSprite = Renderer.Renderables.CreateSprite("TextureSprite", new Vector2(1), GorgonColor.Transparent);
			TextureSprite.TextureRegion = new RectangleF(0, 0, 1, 1);

			if (Sprite.Texture != null)
			{
				TextureSprite.Texture = Sprite.Texture;
				TextureSprite.Size = Sprite.Texture.Settings.Size;
				TextureSprite.Color = new GorgonColor(1, 1, 1, 0.35f);
			}

			_textureRect = Rectangle.Truncate(Sprite.Texture != null ? Sprite.Texture.ToPixel(Sprite.TextureRegion) : Sprite.TextureRegion);

			Vertices = new SpriteVertices(this);

			ValidateSpriteProperties();
		}

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

					if (BackgroundTexture != null)
					{
						BackgroundTexture.Dispose();
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
			BackgroundTexture = null;
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
			// Re-assign the anchor so it'll get persisted.
			Sprite.Anchor = _spriteAnchor;

			Sprite.Save(stream);

			Sprite.Anchor = Vector2.Zero;

			ValidateSpriteProperties();
		}

		/// <summary>
		/// Function called when the content is fully loaded and ready for editing.
		/// </summary>
		public override void OnContentReady()
		{
			if ((_settings == null)
				|| (!_settings.CreateContent))
			{
				return;
			}

			// Since this is the last step in the creation of a new content object, auto-flip
			// into edit mode.
			_panel.StartEditMode();
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

			InitSprite();
		}

		/// <summary>
		/// Function called when the content is reverted back to its original state.
		/// </summary>
		/// <returns>
		/// <c>true</c> if reverted, <c>false</c> if not.
		/// </returns>
		protected override bool OnRevert()
		{
			if (!HasChanges)
			{
				return false;
			}

			Dependencies.Clear();
			Reload();

			ValidateSpriteProperties();

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
			_panel = new PanelSpriteEditor(this);

			_swap = Graphics.Output.CreateSwapChain("SpriteEditorSwap",
			                                        new GorgonSwapChainSettings
			                                        {
														Window = _panel.panelSprite,
														Format = BufferFormat.R8G8B8A8_UIntNormal
			                                        });

			Renderer = Graphics.Output.Create2DRenderer(_swap);

			BackgroundTexture = Graphics.Textures.CreateTexture<GorgonTexture2D>("BackgroundTexture", Resources.Pattern);

			if ((_settings == null) || (!_settings.CreateContent))
			{
				return _panel;
			}

			Sprite = Renderer.Renderables.CreateSprite(Name, _settings.Settings);
			var dependency = new Dependency(_settings.TextureDependency.EditorFile, TextureDependencyType);

			// Build the texture dependency.
			Dependencies.Add(dependency);
			Dependencies.CacheDependencyObject(dependency.EditorFile, dependency.Type, Sprite.Texture);

			InitSprite();

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

			Renderer.Drawing.Blit(BackgroundTexture, _panel.ClientRectangle, BackgroundTexture.ToTexel(_panel.ClientRectangle));

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

        /// <summary>
        /// Function called when a vertex for the sprite is updated.
        /// </summary>
        /// <param name="vertex">Vertex that was updated.</param>
	    public void OnVertexUpdated(SpriteVertex vertex)
	    {
	        NotifyPropertyChanged("Vertex", vertex);
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
			_settings = settings;
			PlugIn = _plugIn = plugIn;
			HasThumbnail = true;
		}
		#endregion
	}
}
