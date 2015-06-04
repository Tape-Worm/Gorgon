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
// Created: Thursday, March 07, 2013 9:30:36 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Gorgon.Design;
using Gorgon.Editor.Design;
using Gorgon.Editor.FontEditorPlugIn.Properties;
using Gorgon.Graphics;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Renderers;
using SmoothingMode = System.Drawing.Drawing2D.SmoothingMode;

namespace Gorgon.Editor.FontEditorPlugIn
{
	/// <summary>
	/// Current state for drawing.
	/// </summary>
	enum DrawState
	{
		/// <summary>
		/// Draw the font textures.
		/// </summary>
		DrawFontTextures = 0,
		/// <summary>
		/// Draw the transition when a glyph is selected for editing.
		/// </summary>
		ToGlyphEdit = 1,
		/// <summary>
		/// Draw the glyph in edit mode.
		/// </summary>
		GlyphEdit = 2,
		/// <summary>
		/// Draw the transition when moving back to the font texture.
		/// </summary>
		FromGlyphEdit = 3,
        /// <summary>
        /// Draw the transition when moving between textures.
        /// </summary>
        NextTexture = 4,
        /// <summary>
        /// Draw the transition when moving between textures.
        /// </summary>
        PrevTexture = 5,
		/// <summary>
		/// Glyph clipper interface.
		/// </summary>
		ClipGlyph = 6,
        /// <summary>
        /// The kerning pair interface.
        /// </summary>
        KernPair = 7
	}

    /// <summary>
    /// The main content interface.
    /// </summary>
    class GorgonFontContent
        : ContentObject
	{
		#region Constants.
		/// <summary>
		/// The extension used for a Gorgon font file.
		/// </summary>
	    public const string FileExtension = ".gorFont";
		/// <summary>
		/// The size of the transformed texture.
		/// </summary>
		public const string GlyphTextureSizeProp = "TransformSize";
		/// <summary>
		/// The type of dependency for a texture brush texture.
		/// </summary>
	    public const string TextureBrushTextureType = "TextureBrush";
		/// <summary>
		/// The type of dependency for a glyph texture.
		/// </summary>
	    public const string GlyphTextureType = "Glyph";
		#endregion

		#region Variables.
		private GorgonFontContentPanel _panel;              // Interface for editing the font.
        private bool _disposed;                             // Flag to indicate that the object was disposed.
		private GorgonFontSettings _settings;				// Settings for the font.
		private GorgonSwapChain _swap;						// Swap chain for our display.
        private GorgonSwapChain _textDisplay;               // Swap chain for sample text display.
        private readonly bool _createFont;                  // Flag to indicate that the font should be created after initialization.
	    private GorgonTexture2D _badGlyphTexture;			// Texture used as a stand-in when a glyph is missing its texture.
        #endregion

        #region Properties.
	    /// <summary>
		/// Property to set or return the active state for the editor.
		/// </summary>
		[Browsable(false)]
	    public DrawState CurrentState
	    {
		    get;
		    set;
	    }

        /// <summary>
        /// Property to set or return the brush used to paint the glyphs.
        /// </summary>
        [LocalCategory(typeof(Resources), "CATEGORY_APPEARANCE"),
        LocalDescription(typeof(Resources), "PROP_BRUSH_DESC"),
        LocalDisplayName(typeof(Resources), "PROP_BRUSH_NAME"),
        Editor(typeof(FontBrushEditor), typeof(UITypeEditor)),
        TypeConverter(typeof(FontBrushTypeConverter)), DefaultValue(null)]
        public GorgonGlyphBrush Brush
        {
            get
            {
                return _settings.Brush;
            }
            set
            {
                _settings.Brush = value;
                OnContentUpdated();
				NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the outline size.
        /// </summary>
        [LocalCategory(typeof(Resources), "CATEGORY_APPEARANCE"), 
		LocalDescription(typeof(Resources), "PROP_OUTLINESIZE_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_OUTLINESIZE_NAME"),
		DefaultValue(0),
		RefreshProperties(RefreshProperties.All)]
        public int OutlineSize
        {
            get
            {
                return _settings.OutlineSize;
            }
            set
            {
	            if (value < 0)
	            {
		            value = 0;
	            }
	            if (value > 16)
	            {
		            value = 16;
	            }

	            if (_settings.OutlineSize == value)
	            {
		            return;
	            }

	            _settings.OutlineSize = value;
	            OnContentUpdated();
				NotifyPropertyChanged();
				ValidateProperties();
            }
        }

        /// <summary>
        /// Function to set the base color for the font glyphs.
        /// </summary>
		[LocalCategory(typeof(Resources), "CATEGORY_APPEARANCE"), 
		LocalDescription(typeof(Resources), "PROP_OUTLINECOLOR1_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_OUTLINECOLOR1_NAME"),
		DefaultValue(0xFF000000), Editor(typeof(RGBAEditor), typeof(UITypeEditor)), TypeConverter(typeof(RGBATypeConverter))]
        public Color OutlineColor1
        {
            get
            {
                return _settings.OutlineColor1.ToColor();
            }
            set
            {
	            if (_settings.OutlineColor1.ToColor() == value)
	            {
		            return;
	            }

	            _settings.OutlineColor1 = value;
	            OnContentUpdated();
				NotifyPropertyChanged();
            }
        }

		/// <summary>
		/// Function to set the base color for the font glyphs.
		/// </summary>
		[LocalCategory(typeof(Resources), "CATEGORY_APPEARANCE"),
		LocalDescription(typeof(Resources), "PROP_OUTLINECOLOR2_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_OUTLINECOLOR2_NAME"),
		DefaultValue(0xFF000000), Editor(typeof(RGBAEditor), typeof(UITypeEditor)), TypeConverter(typeof(RGBATypeConverter))]
		public Color OutlineColor2
		{
			get
			{
				return _settings.OutlineColor2.ToColor();
			}
			set
			{
				if (_settings.OutlineColor2.ToColor() == value)
				{
					return;
				}

				_settings.OutlineColor2 = value;
				OnContentUpdated();
				NotifyPropertyChanged();
			}
		}

        /// <summary>
        /// Property to set or return the font size.
        /// </summary>
        [LocalCategory(typeof(Resources), "CATEGORY_DESIGN"), 
		LocalDescription(typeof(Resources), "PROP_FONTSIZE_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_FONTSIZE_NAME"),
		DefaultValue(9.0f)]
        public float FontSize
        {
            get
            {
                return _settings.Size;
            }
            set
            {
	            if (value < 1.0f)
	            {
		            value = 1.0f;
	            }

	            if (_settings.Size.EqualsEpsilon(value))
	            {
		            return;
	            }

	            _settings.Size = value;
	            CheckTextureSize();
	            OnContentUpdated();
				NotifyPropertyChanged();
            }
        }

		/// <summary>
		/// Property to set or return the packing space between glyphs on the texture.
		/// </summary>
		[LocalCategory(typeof(Resources), "CATEGORY_DESIGN"), 
		LocalDescription(typeof(Resources), "PROP_PACKINGSIZE_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_PACKINGSIZE_NAME"),
		DefaultValue(1)]
		public int PackingSpace
		{
			get
			{
				return _settings.PackingSpacing;
			}
			set
			{
				if (value < 0)
				{
					value = 0;
				}

				if (_settings.PackingSpacing == value)
				{
					return;
				}

				_settings.PackingSpacing = value;
				CheckTextureSize();
				OnContentUpdated();
				NotifyPropertyChanged();
			}
		}

        /// <summary>
        /// Property to set or return the texture size for the font.
        /// </summary>
        [LocalCategory(typeof(Resources), "CATEGORY_DESIGN"), 
		LocalDescription(typeof(Resources), "PROP_TEXTURESIZE_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_TEXTURESIZE_NAME"),
		DefaultValue(typeof(Size), "256,256")]
        public Size FontTextureSize
        {
            get
            {
                return _settings.TextureSize;
            }
            set
            {
	            if (_settings.TextureSize == value)
	            {
		            return;
	            }

				if ((value.Width < 16)
					|| (value.Height < 16)
					|| (value.Width >= Graphics.Textures.MaxWidth)
					|| (value.Height >= Graphics.Textures.MaxHeight))
				{
					throw new NotSupportedException(string.Format(Resources.GORFNT_ERR_TEXTURE_SIZE_INVALID,
																  Graphics.Textures.MaxWidth,
																  Graphics.Textures.MaxHeight));
				}

	            _settings.TextureSize = value;

	            CheckTextureSize();
	            OnContentUpdated();
				NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the anti-alias mode.
        /// </summary>
        [LocalCategory(typeof(Resources), "CATEGORY_APPEARANCE"), 
		LocalDescription(typeof(Resources), "PROP_ANTIALIASMODE_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_ANTIALIASMODE_NAME"),
		DefaultValue(FontAntiAliasMode.AntiAlias)]
        public FontAntiAliasMode FontAntiAliasMode
        {
            get
            {
                return _settings.AntiAliasingMode;
            }
            set
            {
	            if (_settings.AntiAliasingMode == value)
	            {
		            return;
	            }

	            _settings.AntiAliasingMode = value;
	            OnContentUpdated();
				NotifyPropertyChanged();
            }
        }
		
		/// <summary>
        /// Property to set or return whether the font size should be in point size or pixel size.
        /// </summary>
        [LocalCategory(typeof(Resources), "CATEGORY_DESIGN"), 
		LocalDescription(typeof(Resources), "PROP_HEIGHTMODE_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_HEIGHTMODE_NAME"),
		DefaultValue(typeof(FontHeightMode), "Points")]
        public FontHeightMode FontHeightMode
        {
            get
            {
                return _settings.FontHeightMode;
            }
            set
            {
	            if (value == _settings.FontHeightMode)
	            {
		            return;
	            }

	            _settings.FontHeightMode = value;
	            CheckTextureSize();
	            OnContentUpdated();
				NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the name of the font family.
        /// </summary>
        [Editor(typeof(FontFamilyEditor), typeof(UITypeEditor)), 
		RefreshProperties(RefreshProperties.Repaint), 
		LocalCategory(typeof(Resources), "CATEGORY_DESIGN"),
		LocalDisplayName(typeof(Resources), "PROP_FONTFAMILY_NAME"),
		LocalDescription(typeof(Resources), "PROP_FONTFAMILY_DESC")]
        public string FontFamily
        {
            get
            {
                return _settings.FontFamilyName;
            }
            set
            {
                if (value == null)
                    value = string.Empty;

	            if (string.Equals(_settings.FontFamilyName, value, StringComparison.OrdinalIgnoreCase))
	            {
		            return;
	            }

	            _settings.FontFamilyName = value;
	            CheckTextureSize();
	            OnContentUpdated();
				NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the list of characters that need to be turned into glyphs.
        /// </summary>
        [LocalCategory(typeof(Resources), "CATEGORY_DESIGN"),
		LocalDisplayName(typeof(Resources), "PROP_CHARACTERS_NAME"),
		LocalDescription(typeof(Resources), "PROP_CHARACTERS_DESC"), 
		TypeConverter(typeof(CharacterSetTypeConverter)), 
		Editor(typeof(CharacterSetEditor), typeof(UITypeEditor))]
        public IEnumerable<char> Characters
        {
            get
            {
                return _settings.Characters;
            }
            set
            {
	            if ((value == null) || (!value.Any()))
	            {
		            value = new []
		                    {
			                    ' '
		                    };
	            }

	            _settings.Characters = value;
	            CheckTextureSize();
	            OnContentUpdated();
				NotifyPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the font style.
        /// </summary>
        [Editor(typeof(FontStyleEditor), typeof(UITypeEditor)), 
		LocalCategory(typeof(Resources), "CATEGORY_APPEARANCE"),
		LocalDisplayName(typeof(Resources), "PROP_STYLE_NAME"),
		LocalDescription(typeof(Resources), "PROP_STYLE_DESC"), 
		DefaultValue(FontStyle.Regular)]
        public FontStyle FontStyle
        {
            get
            {
                return _settings.FontStyle;
            }
            set
            {
	            if (_settings.FontStyle == value)
	            {
		            return;
	            }

	            _settings.FontStyle = value;
	            CheckTextureSize();
	            OnContentUpdated();
				NotifyPropertyChanged();
            }
        }

		/// <summary>
		/// Property to set or return whether to use kerning pairs when generating the font or not.
		/// </summary>
		[LocalCategory(typeof(Resources), "CATEGORY_DESIGN"),
		LocalDisplayName(typeof(Resources), "PROP_USEKERNING_NAME"),
		LocalDescription(typeof(Resources), "PROP_USEKERNING_DESC"),
		DefaultValue(true)]
		public bool UseKerningPairs
	    {
		    get
		    {
			    return _settings.UseKerningPairs;
		    }
		    set
		    {
			    if (_settings.UseKerningPairs == value)
			    {
				    return;
			    }

			    _settings.UseKerningPairs = value;
				OnContentUpdated();
				NotifyPropertyChanged();
		    }
	    }

        /// <summary>
        /// Property to return whether this content has properties that can be manipulated in the properties tab.
        /// </summary>
        public override bool HasProperties
        {
            get 
            {
                return true;
            }
        }

		/// <summary>
		/// Property to return the renderer.
		/// </summary>
        [Browsable(false)]
		public Gorgon2D Renderer
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the font content.
		/// </summary>
        [Browsable(false)]
		public GorgonFont Font
		{
			get;
			private set;
		}

        /// <summary>
        /// Property to return the type of content.
        /// </summary>
		[Browsable(false)]
        public override string ContentType
        {
            get 
            {
                return Resources.GORFNT_CONTENT_TYPE;
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
        #endregion

        #region Methods.
		/// <summary>
		/// Function to validate the properties for the font.
		/// </summary>
	    private void ValidateProperties()
	    {
			DisableProperty("OutlineColor2", _settings.OutlineSize < 3);
			DisableProperty("FontTextureSize", _settings.Glyphs.Count > 0);
	    }

        /// <summary>
        /// Function to limit the font texture size based on the font size.
        /// </summary>
        private void CheckTextureSize()
        {
            float fontSize = FontSize;
            if (FontHeightMode == FontHeightMode.Points)
            {
                fontSize = (float)System.Math.Ceiling(GorgonFontSettings.GetFontHeight(fontSize, OutlineSize));
            }
            else
            {
                fontSize += OutlineSize;
            }

            if ((fontSize > FontTextureSize.Width)
                || (fontSize > FontTextureSize.Height))
            {
                FontTextureSize = new Size((int)fontSize, (int)fontSize);
            }
        }

        /// <summary>
        /// Function called when the content is reverted back to its original state.
        /// </summary>
        /// <returns>
        /// <b>true</b> if reverted, <b>false</b> if not.
        /// </returns>
        protected override bool OnRevert()
        {
            if (!HasChanges)
            {
                return false;
            }
			
			Dependencies.Clear();
            Font.Dispose();
	        Font = null;

			Reload();
			
			ValidateProperties();

            return true;
        }

        /// <summary>
        /// Function to update the content.
        /// </summary>
        protected override void OnContentUpdated()
        {
			GorgonFont newFont;

            // If the cache does not contain the font, then find the font family that does.
            FontFamily family = System.Drawing.FontFamily.Families.FirstOrDefault(item =>
                                                                                  string.Equals(item.Name,
                                                                                                FontFamily,
                                                                                                StringComparison.OrdinalIgnoreCase));

            if (family == null)
            {
                throw new KeyNotFoundException(string.Format(Resources.GORFNT_ERR_FAMILY_NOT_FOUND, FontFamily));
            }

			var styles = Enum.GetValues(typeof(FontStyle)) as FontStyle[];

			Debug.Assert(styles != null, "No styles!");

			// Remove styles that won't work for this font family.
			foreach (FontStyle style in styles.Where(item => (_settings.FontStyle & item) == item 
															&& (!family.IsStyleAvailable(item))))
			{
				_settings.FontStyle &= ~style;
			}

	        // If we're left with regular and the font doesn't support it, then use the next available style.
            if ((!family.IsStyleAvailable(FontStyle.Regular)) && (_settings.FontStyle == FontStyle.Regular))
			{
                _settings.FontStyle = styles.First(family.IsStyleAvailable);
			}

			try
			{
				newFont = Graphics.Fonts.CreateFont(Name, _settings);
			}
			catch
			{
				// Restore the settings.
				if (Font != null)
				{
					_settings = Font.Settings.Clone();
				}
                
				throw;
			}

			// Wipe out the previous font.
			if (Font != null)
			{
				Font.Dispose();
				Font = null;
			}

			Font = newFont;
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><b>true</b> to release both managed and unmanaged resources; <b>false</b> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Save our settings.
                    GorgonFontEditorPlugIn.Settings.Save();

	                if (_badGlyphTexture != null)
	                {
		                _badGlyphTexture.Dispose();
	                }

					// Remove any externally linked dependencies.
	                EditorFile = null;

                    if (Font != null)
                    {
                        Font.Dispose();
                    }

					if (Renderer != null)
					{
						Renderer.Dispose();
					}

                    if (_textDisplay != null)
                    {
                        _textDisplay.Dispose();
                    }

                    if (_swap != null)
					{
						_swap.Dispose();
					}
                }

				_swap = null;
				Renderer = null;
                Font = null;
				_panel = null;
                _textDisplay = null;
                _disposed = true;
            }

            base.Dispose(disposing);
        }

		/// <summary>
		/// Function to persist the content data into a stream.
		/// </summary>
		/// <param name="stream">Stream that will receive the data for the content.</param>
		protected override void OnPersist(Stream stream)
		{
			Font.Save(stream);
			ValidateProperties();
		}

	    /// <summary>
	    /// Function to read the content data from a stream.
	    /// </summary>
	    /// <param name="stream">Stream containing the content data.</param>
	    protected override void OnRead(Stream stream)
	    {
		    Font = Graphics.Fonts.FromStream(Name,
		                                     stream,
		                                     (name, size) =>
		                                     _badGlyphTexture ??
		                                     (_badGlyphTexture =
		                                      Graphics.Textures.CreateTexture<GorgonTexture2D>(name,
		                                                                                       Resources.bad_glyph_256x256,
		                                                                                       new GorgonGDIOptions
		                                                                                       {
			                                                                                       Width = size.Width,
			                                                                                       Height = size.Height,
			                                                                                       Filter = ImageFilter.Point
		                                                                                       })));
		    _settings = Font.Settings.Clone();

			ValidateProperties();
	    }

	    /// <summary>
		/// Function to initialize the content editor.
		/// </summary>
		/// <returns>
		/// A control to place in the primary interface window.
		/// </returns>        
		protected override ContentPanel OnInitialize()
		{
		    _panel = new GorgonFontContentPanel(this, GetRawInput());

		    _swap = Graphics.Output.CreateSwapChain("FontEditor.SwapChain", new GorgonSwapChainSettings
			{
				Window = _panel.panelTextures,
				Format = BufferFormat.R8G8B8A8_UIntNormal
			});

			Renderer = Graphics.Output.Create2DRenderer(_swap);

			_textDisplay = Graphics.Output.CreateSwapChain("FontEditor.TextDisplay", new GorgonSwapChainSettings
			{
				Window = _panel.panelText,
				Format = BufferFormat.R8G8B8A8_UIntNormal
			});

            // If the font needs to be created, do so now.
		    if (_createFont)
		    {
		        Font = Graphics.Fonts.CreateFont(Name, _settings);
			    _settings = Font.Settings.Clone();
		    }

		    _panel.CreateResources();
			CurrentState = DrawState.DrawFontTextures;

			return _panel;
		}

		/// <summary>
		/// Function to load a dependency file.
		/// </summary>
		/// <param name="dependency">The dependency to load.</param>
		/// <param name="stream">Stream containing the dependency file.</param>
		/// <returns>
		/// The result of the load operation.  If the dependency loaded correctly, then the developer should return NoError.  If the dependency 
		/// is not vital to the content, then the developer can return CanContinue, otherwise the developer should return FatalError and the content will 
		/// not continue loading.
		/// </returns>
	    protected override DependencyLoadResult OnLoadDependencyFile(Dependency dependency, Stream stream)
	    {
			if (ImageEditor == null)
			{
				return new DependencyLoadResult(DependencyLoadState.ErrorContinue, Resources.GORFNT_ERR_EXTERN_IMAGE_EDITOR_MISSING);
			}

			if ((!string.Equals(dependency.Type, TextureBrushTextureType, StringComparison.OrdinalIgnoreCase))
			    && (!string.Equals(dependency.Type, GlyphTextureType, StringComparison.OrdinalIgnoreCase)))
			{
				return new DependencyLoadResult(DependencyLoadState.ErrorContinue,
				                                string.Format(Resources.GORFNT_ERR_DEPENDENCY_UNKNOWN_TYPE, dependency.Type));
			}

			IImageEditorContent imageContent = null;

			try
			{
				Size newSize = Size.Empty;

				// We need to load the image as transformed (either clipped or stretched).
				if (string.Equals(dependency.Type, GlyphTextureType, StringComparison.OrdinalIgnoreCase))
				{
					var converter = new SizeConverter();
					var sizeObject = converter.ConvertFromInvariantString(dependency.Properties[GlyphTextureSizeProp].Value);

					if (!(sizeObject is Size))
					{
						return new DependencyLoadResult(DependencyLoadState.ErrorContinue,
						                                Resources.GORFNT_ERR_DEPENDENCY_GLYPH_TEXTURE_BAD_TRANSFORM);
					}

					newSize = (Size)sizeObject;
				}

				imageContent = ImageEditor.ImportContent(dependency.EditorFile, stream);

				if (newSize.Width == 0)
				{
					newSize.Width = imageContent.Image.Settings.Width;
				}

				if (newSize.Height == 0)
				{
					newSize.Height = imageContent.Image.Settings.Height;
				}

				// Clip the image to a new size if necessary.
				if ((newSize.Width != imageContent.Image.Settings.Width)
				    || (newSize.Height != imageContent.Image.Settings.Height))
				{
					imageContent.Image.Resize(newSize.Width, newSize.Height, true);
				}

				if (imageContent.Image == null)
				{
					return new DependencyLoadResult(DependencyLoadState.ErrorContinue, Resources.GORFNT_ERR_EXTERN_IMAGE_MISSING);
				}

				if (imageContent.Image.Settings.ImageType != ImageType.Image2D)
				{
					return new DependencyLoadResult(DependencyLoadState.ErrorContinue,
					                                string.Format(Resources.GORFNT_ERR_IMAGE_NOT_2D, dependency.EditorFile.FilePath));
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
			finally
			{
				if (imageContent != null)
				{
					imageContent.Dispose();
				}
			}
	    }

	    /// <summary>
        /// Function to update the glyph regions/textures in the font object.
        /// </summary>
        public void UpdateFontGlyphs()
        {
            OnContentUpdated();
			NotifyPropertyChanged();
        }

		/// <summary>
		/// Function to update a font glyph advancement.
		/// </summary>
		/// <param name="advance">The advancement that was updated.</param>
		/// <param name="resetFont"><b>true</b> to rebuild the font, <b>false</b> to leave alone.</param>
	    public void UpdateFontGlyphAdvance(int advance, bool resetFont)
	    {
			if (resetFont)
			{
				OnContentUpdated();
				NotifyPropertyChanged("ResetGlyphAdvance", advance);
				return;
			}

			NotifyPropertyChanged("SelectedGlyphAdvance", advance);
	    }

		/// <summary>
		/// Function to update a font glyph offset.
		/// </summary>
		/// <param name="offset">Offset to update.</param>
		/// <param name="resetFont"><b>true</b> to rebuild the font, <b>false</b> to leave alone.</param>
	    public void UpdateFontGlyphOffset(Point offset, bool resetFont)
	    {
			if (resetFont)
			{
				OnContentUpdated();
				NotifyPropertyChanged("ResetGlyphOffset", offset);
				return;
			}

			NotifyPropertyChanged("SelectedGlyphOffset", offset);
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
		    var result = new Bitmap(128, 128, PixelFormat.Format32bppArgb);

			// Find the texture with the most characters.
			var texture = (from glyph in Font.Glyphs
			               group glyph by glyph.Texture
			               into textureGroup
			               orderby textureGroup.Count() descending
			               select textureGroup.Key).First();

			// Scale the image.
			using (Image sourceImage = texture.ToGDIImage()[0])
			{
				using (var graphics = System.Drawing.Graphics.FromImage(result))
				{
					graphics.Clear(Color.Black);

					float aspect = (float)sourceImage.Width / sourceImage.Height;
					var size = new SizeF(128, 128);
					var position = new PointF(0, 0);

					if (aspect > 1.0f)
					{
						position = new PointF(0, 64.0f - size.Height / 2.0f);
						size.Height /= aspect;
					}
					else if (aspect < 1.0f)
					{
						position = new PointF(64.0f - size.Width / 2.0f, 0);
						size.Width *= aspect;
					}

					graphics.SmoothingMode = SmoothingMode.HighQuality;
					graphics.DrawImage(sourceImage,
					                   new RectangleF(position, size),
					                   new RectangleF(0, 0, sourceImage.Width, sourceImage.Height),
					                   GraphicsUnit.Pixel);
				}
			}

			return result;
	    }

        /// <summary>
        /// Function to draw the preview text.
        /// </summary>
        public void DrawPreviewText()
        {
            Renderer.Target = (GorgonRenderTarget2D)_textDisplay;

            _panel.DrawText();

            Renderer.Render(2);
        }

	    /// <summary>
		/// Function to draw the interface for the content editor.
		/// </summary>
		public override void Draw()
		{
            Renderer.Target = _swap;
			Renderer.Clear(_panel.panelTextures.BackColor);

			switch (CurrentState)
			{
				case DrawState.FromGlyphEdit:
				case DrawState.ToGlyphEdit:
					_panel.DrawGlyphEditTransition();
					Renderer.Render(1);
					break;
                case DrawState.KernPair:
				case DrawState.GlyphEdit:
					_panel.DrawGlyphEdit();
					Renderer.Render(2);
					break;
                case DrawState.NextTexture:
                case DrawState.PrevTexture:
					_panel.DrawFontTexture();
			        Renderer.Render(1);
			        break;
				case DrawState.ClipGlyph:
					_panel.DrawGlyphClip();
					Renderer.Render(1);
					break;
				default:
					_panel.DrawFontTexture();
					Renderer.Render(2);
					break;
			}

            DrawPreviewText();
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFontContent"/> class.
        /// </summary>
		/// <param name="plugIn">The plug-in that creates this object.</param>
		/// <param name="initialSettings">The initial settings for the content.</param>
        public GorgonFontContent(ContentPlugIn plugIn, GorgonFontContentSettings initialSettings)
			: base(initialSettings)
        {
			PlugIn = plugIn;
			_settings = initialSettings.Settings;
            _createFont = initialSettings.CreateContent;
	        HasThumbnail = true;

			ValidateProperties();
        }
        #endregion
    }
}
