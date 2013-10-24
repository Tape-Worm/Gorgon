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
using GorgonLibrary.Design;
using GorgonLibrary.Editor.FontEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.Math;
using GorgonLibrary.Renderers;
using SmoothingMode = System.Drawing.Drawing2D.SmoothingMode;

namespace GorgonLibrary.Editor.FontEditorPlugIn
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
		FromGlyphEdit = 3
	}

    /// <summary>
    /// The main content interface.
    /// </summary>
    class GorgonFontContent
        : ContentObject
	{
		#region Variables.
		private GorgonFontContentPanel _panel;              // Interface for editing the font.
        private bool _disposed;                             // Flag to indicate that the object was disposed.
		private GorgonFontSettings _settings;				// Settings for the font.
		private GorgonSwapChain _swap;						// Swap chain for our display.
        private GorgonSwapChain _textDisplay;               // Swap chain for sample text display.
        private readonly bool _createFont;                  // Flag to indicate that the font should be created after initialization.
        #endregion

        #region Properties.
		/// <summary>
		/// Property to return the image editor plug-in.
		/// </summary>
		[Browsable(false)]
	    public IImageEditorPlugIn ImageEditor
		{
			get;
			private set;
		}

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
        /// Function to set the base color for the font glyphs.
        /// </summary>
		[LocalCategory(typeof(Resources), "CATEGORY_APPEARANCE"), 
		LocalDescription(typeof(Resources), "PROP_BASECOLOR_DESC"), 
		LocalDisplayName(typeof(Resources), "PROP_BASECOLOR_NAME"),
		Editor(typeof(RGBAEditor), typeof(UITypeEditor)),
		TypeConverter(typeof(RGBATypeConverter)), DefaultValue(0xFFFFFFFF)]
        public Color BaseColor
        {
            get
            {
                return _settings.BaseColor;
            }
            set
            {
	            _settings.BaseColor = value;
	            OnContentUpdated();
	            OnContentPropertyChanged("BaseColor", value);
            }
        }

        /// <summary>
        /// Property to set or return the outline size.
        /// </summary>
        [LocalCategory(typeof(Resources), "CATEGORY_APPEARANCE"), 
		LocalDescription(typeof(Resources), "PROP_OUTLINESIZE_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_OUTLINESIZE_NAME"),
		DefaultValue(0)]
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
	            OnContentPropertyChanged("OutlineSize", value);
            }
        }

        /// <summary>
        /// Function to set the base color for the font glyphs.
        /// </summary>
		[LocalCategory(typeof(Resources), "CATEGORY_APPEARANCE"), 
		LocalDescription(typeof(Resources), "PROP_OUTLINECOLOR_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_OUTLINECOLOR_NAME"),
		DefaultValue(0xFF000000), Editor(typeof(RGBAEditor), typeof(UITypeEditor)), TypeConverter(typeof(RGBATypeConverter))]
        public Color OutlineColor
        {
            get
            {
                return _settings.OutlineColor.ToColor();
            }
            set
            {
	            if (_settings.OutlineColor.ToColor() == value)
	            {
		            return;
	            }

	            _settings.OutlineColor = value;
	            OnContentUpdated();
	            OnContentPropertyChanged("OutlineColor", value);
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
	            OnContentPropertyChanged("FontSize", value);
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
				OnContentPropertyChanged("PackingSpace", value);
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
                if (value.Width < 16)
                    value.Width = 16;
                if (value.Height < 16)
                    value.Height = 16;
                if (value.Width >= Graphics.Textures.MaxWidth)
                    value.Width = Graphics.Textures.MaxWidth - 1;
                if (value.Height >= Graphics.Textures.MaxHeight)
                    value.Height = Graphics.Textures.MaxHeight - 1;

	            if (_settings.TextureSize == value)
	            {
		            return;
	            }

	            _settings.TextureSize = value;
	            CheckTextureSize();
	            OnContentUpdated();
	            OnContentPropertyChanged("FontTextureSize", value);
            }
        }

        /// <summary>
        /// Property to set or return the anti-alias mode.
        /// </summary>
        [LocalCategory(typeof(Resources), "CATEGORY_APPEARANCE"), 
		LocalDescription(typeof(Resources), "PROP_ANTIALIASMODE_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_ANTIALIASMODE_NAME"),
		DefaultValue(FontAntiAliasMode.AntiAliasHQ)]
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
	            OnContentPropertyChanged("FontAntiAliasMode", value);
            }
        }

		/// <summary>
		/// Property to set or return the contrast of the glyphs.
		/// </summary>
		[LocalCategory(typeof(Resources), "CATEGORY_APPEARANCE"), 
		LocalDescription(typeof(Resources), "PROP_TEXTCONTRAST_DESC"),
		LocalDisplayName(typeof(Resources), "PROP_TEXTCONTRAST_NAME"),
		DefaultValue(4)]
		public int TextContrast
		{
			get
			{
				return _settings.TextContrast;
			}
			set
			{
				if (value < 0)
				{
					value = 0;
				}
				if (value > 12)
				{
					value = 12;
				}

				if (value == _settings.TextContrast)
				{
					return;
				}

				_settings.TextContrast = value;
				OnContentUpdated();
				OnContentPropertyChanged("TextContrast", value);
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
	            OnContentPropertyChanged("UsePointSize", value);
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
	            OnContentPropertyChanged("FontFamily", value);
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
	            OnContentPropertyChanged("Characters", value);
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
	            OnContentPropertyChanged("FontStyle", value);
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
        public override string ContentType
        {
            get 
            {
                return "Gorgon Font";
            }
        }		

        /// <summary>
        /// Property to return whether the content object supports a renderer interface.
        /// </summary>
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
        /// Function to limit the font texture size based on the font size.
        /// </summary>
        private void CheckTextureSize()
        {
            float fontSize = FontSize;
            if (FontHeightMode == FontHeightMode.Points)
                fontSize = (float)System.Math.Ceiling(GorgonFontSettings.GetFontHeight(fontSize, OutlineSize));
            else
                fontSize += OutlineSize;

            if ((fontSize > FontTextureSize.Width) || (fontSize > FontTextureSize.Height))
                FontTextureSize = new Size((int)fontSize, (int)fontSize);
        }

        /// <summary>
        /// Function to update the content.
        /// </summary>
        protected override void OnContentUpdated()
        {
			GorgonFont newFont;

			if (!GorgonFontEditorPlugIn.CachedFonts.ContainsKey(FontFamily.ToLower()))
			{
				throw new KeyNotFoundException(string.Format(Resources.GORFNT_FAMILY_NOT_FOUND, FontFamily));
			}

			Font cachedFont = GorgonFontEditorPlugIn.CachedFonts[FontFamily.ToLower()];
			var styles = Enum.GetValues(typeof(FontStyle)) as FontStyle[];

			Debug.Assert(styles != null, "No styles!");

			// Remove styles that won't work for this font family.
			foreach (FontStyle style in styles.Where(item => (_settings.FontStyle & item) == item 
															&& (!cachedFont.FontFamily.IsStyleAvailable(item))))
			{
				_settings.FontStyle &= ~style;
			}

	        // If we're left with regular and the font doesn't support it, then use the next available style.
			if ((!cachedFont.FontFamily.IsStyleAvailable(FontStyle.Regular)) && (_settings.FontStyle == FontStyle.Regular))
			{
				_settings.FontStyle = styles.First(item => cachedFont.FontFamily.IsStyleAvailable(item));
			}

			try
			{
                // TODO: Remove, I think this may be redundant.
                //var fontSettings = new GorgonFontSettings
                //    {
                //        AntiAliasingMode = _settings.AntiAliasingMode,
                //        BaseColor = _settings.BaseColor,
                //        Brush = _settings.Brush,
                //        Characters = _settings.Characters,
                //        DefaultCharacter = _settings.DefaultCharacter,
                //        FontFamilyName = _settings.FontFamilyName,
                //        FontHeightMode = _settings.FontHeightMode,
                //        FontStyle = _settings.FontStyle,
                //        OutlineColor = _settings.OutlineColor,
                //        OutlineSize = _settings.OutlineSize,
                //        PackingSpacing = _settings.PackingSpacing,
                //        Size = _settings.Size,
                //        TextContrast = _settings.TextContrast,
                //        TextureSize = _settings.TextureSize
                //    };

                //if (_settings.Glyphs.Count > 0)
                //{
                //    fontSettings.Glyphs.AddRange(_settings.Glyphs);
                //}

                //if (_settings.KerningPairs.Count > 0)
                //{
                //    foreach (var pair in _settings.KerningPairs)
                //    {
                //        fontSettings.KerningPairs.Add(pair.Key, pair.Value);
                //    }
                //}

				newFont = Graphics.Fonts.CreateFont(Name, _settings);
			}
			catch
			{
				// Restore the settings.
				if (Font != null)
				{
					_settings = Font.Settings;
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
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // Save our settings.
                    GorgonFontEditorPlugIn.Settings.Save();

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

					if (_panel != null)
					{
						_panel.Dispose();						
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
	    }

	    /// <summary>
	    /// Function to read the content data from a stream.
	    /// </summary>
	    /// <param name="stream">Stream containing the content data.</param>
	    protected override void OnRead(Stream stream)
	    {
		    Font = Graphics.Fonts.FromStream(Path.GetFileNameWithoutExtension(Name), stream);
		    _settings = Font.Settings;
	    }

        /// <summary>
        /// Function called when the name is about to be changed.
        /// </summary>
        /// <param name="proposedName">The proposed name for the content.</param>
        /// <returns>
        /// A valid name for the content.
        /// </returns>
        protected override string ValidateName(string proposedName)
        {
	        if (string.IsNullOrWhiteSpace(proposedName))
	        {
		        return string.Empty;
	        }

            if (!proposedName.EndsWith(".gorFont", StringComparison.OrdinalIgnoreCase))
            {
                return proposedName + ".gorFont";
            }

            return proposedName;
        }

		/// <summary>
		/// Function to initialize the content editor.
		/// </summary>
		/// <returns>
		/// A control to place in the primary interface window.
		/// </returns>        
		protected override ContentPanel OnInitialize()
		{
			_panel = new GorgonFontContentPanel
			         {
			             Content = this
			         };

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
		    }

		    _panel.CreateResources();
			CurrentState = DrawState.DrawFontTextures;

			// Find out if the image editor is loaded.
			ImageEditor = Gorgon.PlugIns.FirstOrDefault(
				                              item => string.Equals(item.Name,
				                                            "GorgonLibrary.Editor.ImageEditorPlugIn.GorgonImageEditorPlugIn",
				                                            StringComparison.OrdinalIgnoreCase)) as IImageEditorPlugIn;

			return _panel;
		}

        /// <summary>
        /// Function to update the font object.
        /// </summary>
        public void UpdateFont()
        {
            OnContentUpdated();
            OnContentPropertyChanged("Glyphs", Font.Settings.Glyphs);
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
		/// Function to draw the interface for the content editor.
		/// </summary>
		public override void Draw()
		{
            Renderer.Target = (GorgonRenderTarget2D)_swap;
			Renderer.Clear(_panel.panelTextures.BackColor);

			switch (CurrentState)
			{
				case DrawState.FromGlyphEdit:
				case DrawState.ToGlyphEdit:
					_panel.DrawGlyphEditTransition();
					Renderer.Render(1);
					break;
				case DrawState.GlyphEdit:
					_panel.DrawGlyphEdit();
					Renderer.Render(2);
					break;
				default:
					_panel.DrawFontTexture();
					Renderer.Render(2);
					break;
			}

            Renderer.Target = (GorgonRenderTarget2D)_textDisplay;

			_panel.DrawText();
            
			Renderer.Render(2);
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFontContent"/> class.
        /// </summary>
		/// <param name="plugIn">The plug-in that creates this object.</param>
		/// <param name="initialSettings">The initial settings for the content.</param>
        public GorgonFontContent(ContentPlugIn plugIn, GorgonFontContentSettings initialSettings)
			: base(initialSettings.Name)
        {
			PlugIn = plugIn;
			_settings = initialSettings.Settings;
            _createFont = initialSettings.CreateContent;
	        HasThumbnail = true;
        }
        #endregion
    }
}
