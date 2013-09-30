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
using System.Drawing;
using System.Drawing.Design;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using GorgonLibrary.Editor.FontEditorPlugIn.Properties;
using GorgonLibrary.Graphics;
using GorgonLibrary.IO;
using GorgonLibrary.Renderers;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
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
        /// Function to set the base color for the font glyphs.
        /// </summary>
        [Category("Appearance"), Description("Sets a base color for the glyphs on the texture."), Editor(typeof(RGBAEditor), typeof(UITypeEditor)), 
		TypeConverter(typeof(RGBATypeConverter)), DefaultValue(0xFFFFFFFF)]
        public Color BaseColor
        {
            get
            {
                return _settings.BaseColors[0];
            }
            set
            {
				if (_settings.BaseColors.Count == 0)
				{
					_settings.BaseColors.Add(value);
					return;
				}

	            if (_settings.BaseColors[0].ToColor() == value)
	            {
		            return;
	            }

	            _settings.BaseColors[0] = value;
	            OnContentUpdated();
	            OnContentPropertyChanged("BaseColor", value);
            }
        }

        /// <summary>
        /// Property to set or return the outline size.
        /// </summary>
        [Category("Appearance"), Description("Sets the size, in pixels, of an outline to apply to each glyph.  Please note that the outline color must have an alpha greater than 0 before the outline is applied."), DefaultValue(0)]
        public int OutlineSize
        {
            get
            {
                return _settings.OutlineSize;
            }
            set
            {
                if (value < 0)
                    value = 0;
                if (value > 16)
                    value = 16;

                if (_settings.OutlineSize != value)
                {
                    _settings.OutlineSize = value;
                    OnContentUpdated();
					OnContentPropertyChanged("OutlineSize", value);
				}
            }
        }

        /// <summary>
        /// Function to set the base color for the font glyphs.
        /// </summary>
		[Category("Appearance"), Description("Sets the outline color for the glyphs with outlines.  Note that the outline size must be greater than 0 before it is applied to the glyph."), 
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
        [Category("Design"), Description("The size of the font.  This value may be in Points or in Pixels depending on the font height mode."), DefaultValue(9.0f)]
        public float FontSize
        {
            get
            {
                return _settings.Size;
            }
            set
            {
                if (value < 1.0f)
                    value = 1.0f;

                if (_settings.Size != value)
                {
                    _settings.Size = value;
                    CheckTextureSize();
                    OnContentUpdated();
					OnContentPropertyChanged("FontSize", value);
				}
            }
        }

		/// <summary>
		/// Property to set or return the packing space between glyphs on the texture.
		/// </summary>
		[Category("Design"), Description("Specifies the number of pixels between each glyph on the texture."), DefaultValue(1)]
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

				if (_settings.PackingSpacing != value)
				{
					_settings.PackingSpacing = value;
					CheckTextureSize();
					OnContentUpdated();
					OnContentPropertyChanged("PackingSpace", value);
				}
			}
		}

        /// <summary>
        /// Property to set or return the texture size for the font.
        /// </summary>
        [Category("Design"), Description("The size of the textures used to hold the sprite glyphs."), DefaultValue(typeof(Size), "256,256")]
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

                if (_settings.TextureSize != value)
                {
                    _settings.TextureSize = value;
                    CheckTextureSize();
                    OnContentUpdated();
					OnContentPropertyChanged("FontTextureSize", value);
				}
            }
        }

        /// <summary>
        /// Property to set or return the anti-alias mode.
        /// </summary>
        [Category("Appearance"), Description("Sets the type of anti-aliasing to use for the font.  High Quality (HQ) produces the best output, but doesn't get applied to fonts at certain sizes.  Non HQ, will attempt to anti-alias regardless, but the result will be worse."), DefaultValue(FontAntiAliasMode.AntiAliasHQ)]
        public FontAntiAliasMode FontAntiAliasMode
        {
            get
            {
                return _settings.AntiAliasingMode;
            }
            set
            {
                if (_settings.AntiAliasingMode != value)
                {
                    _settings.AntiAliasingMode = value;
                    OnContentUpdated();
					OnContentPropertyChanged("FontAntiAliasMode", value);
				}
            }
        }

		/// <summary>
		/// Property to set or return the contrast of the glyphs.
		/// </summary>
		[Category("Appearance"), Description("Sets the contrast for anti-aliased glyphs."), DefaultValue(4)]
		public int TextContrast
		{
			get
			{
				return _settings.TextContrast;
			}
			set
			{
				if (value < 0)
					value = 0;
				if (value > 12)
					value = 12;

				if (value != _settings.TextContrast)
				{
					_settings.TextContrast = value;
					OnContentUpdated();
					OnContentPropertyChanged("TextContrast", value);
				}
			}
		}
		
		/// <summary>
        /// Property to set or return whether the font size should be in point size or pixel size.
        /// </summary>
        [DisplayName("FontHeightMode"), Category("Design"), Description("Sets whether to use points or pixels to determine the size of the font."), DefaultValue(typeof(FontHeightMode), "Points")]
        public FontHeightMode UsePointSize
        {
            get
            {
                return _settings.FontHeightMode;
            }
            set
            {
                if (value != _settings.FontHeightMode)
                {
                    _settings.FontHeightMode = value;
                    CheckTextureSize();
                    OnContentUpdated();
					OnContentPropertyChanged("UsePointSize", value);
				}
            }
        }

        /// <summary>
        /// Property to set or return the name of the font family.
        /// </summary>
        [Editor(typeof(FontFamilyEditor), typeof(UITypeEditor)), RefreshProperties(System.ComponentModel.RefreshProperties.Repaint), Category("Design"), Description("The font family to use for the font.")]
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

                if (!string.Equals(_settings.FontFamilyName, value, StringComparison.OrdinalIgnoreCase))
                {
                    _settings.FontFamilyName = value;
                    CheckTextureSize();
                    OnContentUpdated();
					OnContentPropertyChanged("FontFamily", value);
				}
            }
        }

        /// <summary>
        /// Property to set or return the list of characters that need to be turned into glyphs.
        /// </summary>
        [Category("Design"), Description("Sets the list of characters to convert into glyphs."), TypeConverter(typeof(CharacterSetTypeConverter)), Editor(typeof(CharacterSetEditor), typeof(UITypeEditor))]
        public IEnumerable<char> Characters
        {
            get
            {
                return _settings.Characters;
            }
            set
            {
                if ((value == null) || (value.Count() == 0))
                    value = new char[] { ' ' };

                if (_settings.Characters != value)
                {
                    _settings.Characters = value;
                    CheckTextureSize();
                    OnContentUpdated();
					OnContentPropertyChanged("Characters", value);
				}
            }
        }

        /// <summary>
        /// Property to set or return the font style.
        /// </summary>
        [Editor(typeof(FontStyleEditor), typeof(UITypeEditor)), Category("Appearance"), Description("Sets whether the font should be bolded, underlined, italicized, or strike through"), DefaultValue(FontStyle.Regular)]
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
        /// Property to return whether the content can be exported.
        /// </summary>
        public override bool CanExport
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
            if (UsePointSize == FontHeightMode.Points)
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
			GorgonFont newFont = null;

			if (!GorgonFontEditorPlugIn.CachedFonts.ContainsKey(FontFamily.ToLower()))
			{
				throw new KeyNotFoundException("The font family '" + FontFamily + "' could not be found.");
			}

			Font cachedFont = GorgonFontEditorPlugIn.CachedFonts[FontFamily.ToLower()];
			var styles = Enum.GetValues(typeof(FontStyle)) as FontStyle[];

			// Remove styles that won't work.
			for (int i = 0; i < styles.Length; i++)
			{
				if ((!cachedFont.FontFamily.IsStyleAvailable(styles[i])) && ((_settings.FontStyle & styles[i]) == styles[i]))
					_settings.FontStyle &= ~styles[i];
			}

			// If we're left with regular and the font doesn't support it, then use the next available style.
			if ((!cachedFont.FontFamily.IsStyleAvailable(FontStyle.Regular)) && (_settings.FontStyle == FontStyle.Regular))
			{
				foreach (FontStyle style in styles.Where(style => cachedFont.FontFamily.IsStyleAvailable(style)))
				{
					_settings.FontStyle |= style;
					break;
				}
			}

			try
			{
				var fontSettings = new GorgonFontSettings
					{
						AntiAliasingMode = _settings.AntiAliasingMode,
						Brush = _settings.Brush,
						Characters = _settings.Characters,
						DefaultCharacter = _settings.DefaultCharacter,
						FontFamilyName = _settings.FontFamilyName,
						FontHeightMode = _settings.FontHeightMode,
						FontStyle = _settings.FontStyle,
						OutlineColor = _settings.OutlineColor,
						OutlineSize = _settings.OutlineSize,
						PackingSpacing = _settings.PackingSpacing,
						Size = _settings.Size,
						TextContrast = _settings.TextContrast,
						TextureSize = _settings.TextureSize
					};

				fontSettings.BaseColors.Clear();
				for (int i = 0; i < _settings.BaseColors.Count; i++)
				{
					fontSettings.BaseColors.Add(_settings.BaseColors[i]);
				}

				newFont = Graphics.Fonts.CreateFont(Name, fontSettings);
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

			// Wipe out the font.
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

		    _swap = Graphics.Output.CreateSwapChain("FontEditor.SwapChain", new GorgonSwapChainSettings()
			{
				Window = _panel.panelTextures,
				Format = BufferFormat.R8G8B8A8_UIntNormal
			});

			Renderer = Graphics.Output.Create2DRenderer(_swap);

			_textDisplay = Graphics.Output.CreateSwapChain("FontEditor.TextDisplay", new GorgonSwapChainSettings()
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

			return _panel;
		}

		/// <summary>
		/// Function called when the content is shown for the first time.
		/// </summary>
		public override void Activate()
		{
			if (Renderer != null)
			{
				Renderer.Begin2D();
			}
		}

		/// <summary>
		/// Function to draw the interface for the content editor.
		/// </summary>
		public override void Draw()
		{
            Renderer.Target = (GorgonRenderTarget2D)_swap;
			Renderer.Clear(_panel.panelTextures.BackColor);           

			_panel.DrawFontTexture();

            Renderer.Render(2);

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
        }
        #endregion
    }
}
