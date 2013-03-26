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
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Drawing.Design;
using System.ComponentModel;
using System.Windows.Forms;
using SlimMath;
using GorgonLibrary;
using GorgonLibrary.UI;
using GorgonLibrary.IO;
using GorgonLibrary.FileSystem;
using GorgonLibrary.Math;
using GorgonLibrary.Graphics;
using GorgonLibrary.Renderers;

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
    /// <summary>
    /// The main content interface.
    /// </summary>
    class GorgonFontContent
        : ContentObject
    {
        #region Variables.
		private ContentPlugIn _plugIn = null;						// The plug-in that interfaces the content with the main system.
        private GorgonFontContentPanel _panel = null;               // Interface for editing the font.
        private bool _disposed = false;                             // Flag to indicate that the object was disposed.
		private GorgonFontSettings _settings = null;				// Settings for the font.
		private GorgonSwapChain _swap = null;						// Swap chain for our display.
        private GorgonSwapChain _textDisplay = null;                // Swap chain for sample text display.
        private GorgonText _text = null;                            // Text object.
        private StringBuilder _sampleText = new StringBuilder(256); // Sample text.
        #endregion

        #region Properties.
        /// <summary>
        /// Function to set the base color for the font glyphs.
        /// </summary>
        [Category("Appearance"), Description("Sets a base color for the glyphs on the texture."), Editor(typeof(RGBAEditor), typeof(UITypeEditor)), TypeConverter(typeof(RGBATypeConverter))]
        public Color BaseColor
        {
            get
            {
                return _settings.BaseColors[0];
            }
            set
            {
                if (_settings.BaseColors[0].ToColor() != value)
                {
                    _settings.BaseColors = new GorgonColor[] { value };
                    OnContentPropertyChanged("BaseColor", value);
                    UpdateContent();
                }
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
                    OnContentPropertyChanged("OutlineSize", value);
                    UpdateContent();
                }
            }
        }

        /// <summary>
        /// Function to set the base color for the font glyphs.
        /// </summary>
        [Category("Appearance"), Description("Sets the outline color for the glyphs with outlines.  Note that the outline size must be greater than 0 before it is applied to the glyph."), DefaultValue(0), Editor(typeof(RGBAEditor), typeof(UITypeEditor)), TypeConverter(typeof(RGBATypeConverter))]
        public Color OutlineColor
        {
            get
            {
                return _settings.OutlineColor;
            }
            set
            {
                if (_settings.OutlineColor.ToColor() != value)
                {
                    _settings.OutlineColor = value;
                    OnContentPropertyChanged("OutlineColor", value);
                    UpdateContent();
                }
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
                    OnContentPropertyChanged("FontSize", value);
                    UpdateContent();
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
                    OnContentPropertyChanged("FontTextureSize", value);
                    UpdateContent();
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
                    OnContentPropertyChanged("FontAntiAliasMode", value);
                    UpdateContent();
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
                    OnContentPropertyChanged("UsePointSize", value);
                    UpdateContent();
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

                if (string.Compare(_settings.FontFamilyName, value, true) != 0)
                {
                    _settings.FontFamilyName = value;
                    CheckTextureSize();
                    OnContentPropertyChanged("FontFamily", value);
                    UpdateContent();
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
                    OnContentPropertyChanged("Characters", value);
                    UpdateContent();
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
                if (_settings.FontStyle != value)
                {
                    _settings.FontStyle = value;
                    CheckTextureSize();
                    OnContentPropertyChanged("FontStyle", value);
                    UpdateContent();
                }
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
        protected override void UpdateContent()
        {
            // TODO: Add some error checking and such.
            Font.Update(_settings);            

            base.UpdateContent();
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
        /// Function called when the HasChanges property is updated.
        /// </summary>
        protected override void OnHasChangesUpdated()
        {
            if (_panel != null)
            {
				_panel.Text = "Gorgon Font - " + Name;
                _panel.OnContentChanged();
            }
        }

        /// <summary>
        /// Function to persist the content to the file system.
        /// </summary>
        protected override void OnPersist()
        {
            if ((Font != null) && (File != null) && (HasChanges))
            {
                using (var stream = File.OpenStream(true))
                {
                    Font.Save(stream);
                }
            }            
        }

        /// <summary>
        /// Function called when the content window is closed.
        /// </summary>
        /// <returns>
        /// TRUE to continue closing the window, FALSE to cancel the close.
        /// </returns>
        protected override bool OnClose()
        {
            if (HasChanges)
            {
                // Write the changes to the file.
                OnPersist();
            }

            return true;
        }

		/// <summary>
		/// Function to open the content from the file system.
		/// </summary>
		protected override void OnOpenContent()
		{
			if (File == null)
			{
				return;
			}

			// Open the file.
			using (var stream = File.OpenStream(false))
			{
				Font = Graphics.Fonts.FromStream(File.Name, stream);
                //_text.Font = Font;
			}            

			// Get the settings.
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
            if (!proposedName.EndsWith(".gorFont", StringComparison.CurrentCultureIgnoreCase))
            {
                return proposedName + ".gorFont";
            }

            return proposedName;
        }

		/// <summary>
		/// Function to create new content.
		/// </summary>
		/// <returns>
		/// TRUE if successful, FALSE if not or canceled.
		/// </returns>
		protected override bool CreateNew()
		{
			formNewFont newFont = null;

			try
			{
				newFont = new formNewFont();
				newFont.Content = this;
				newFont.FontCharacters = _settings.Characters;

				if (newFont.ShowDialog() == DialogResult.OK)
				{
					Cursor.Current = Cursors.WaitCursor;

					Name = newFont.FontName.FormatFileName();
					if (!Name.EndsWith(".gorFont", StringComparison.CurrentCultureIgnoreCase))
					{
						Name = Name + ".gorFont";
					}

					_settings.FontFamilyName = newFont.FontFamilyName;
					_settings.Size = newFont.FontSize;
					_settings.FontHeightMode = newFont.FontHeightMode;
					_settings.AntiAliasingMode = newFont.FontAntiAliasMode;
					_settings.FontStyle = newFont.FontStyle;
					_settings.TextureSize = newFont.FontTextureSize;
					_settings.Characters = newFont.FontCharacters;

                    Font = Graphics.Fonts.CreateFont(this.Name, _settings);
                    //_text.Font = Font;

					return true;
				}

				return false;
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(null, ex);
				return false;
			}
			finally
			{
				Cursor.Current = Cursors.Default;

				if (newFont != null)
				{
					newFont.Dispose();
					newFont = null;
				}
			}
		}

		/// <summary>
		/// Function to draw the interface for the content editor.
		/// </summary>
		public override void Draw()
		{
            Renderer.Target = _swap;
			Renderer.Clear(_panel.panelTextures.BackColor);           

			Renderer.Drawing.SmoothingMode = SmoothingMode.None;

			_panel.DrawFontTexture();

            Renderer.Render(2);

            Renderer.Target = _textDisplay;
            Renderer.Clear(_panel.panelText.BackColor);
            Renderer.Drawing.SmoothingMode = SmoothingMode.Smooth;
            
            _text.Text = _sampleText.ToString();
            _text.Draw();
            
			Renderer.Render(2);
		}

        /// <summary>
        /// Function to initialize the content editor.
        /// </summary>
        /// <returns>
        /// A control to place in the primary interface window.
        /// </returns>        
        protected override Control OnInitialize()
        {
            _panel = new GorgonFontContentPanel();
			_panel.Content = this;
			_panel.Text = "Gorgon Font - " + Name;

			_swap = Graphics.Output.CreateSwapChain("FontEditor.SwapChain", new GorgonSwapChainSettings()
			{
				Window = _panel.panelTextures,
				Format = BufferFormat.R8G8B8A8_UIntNormal
			});

			Renderer = Graphics.Output.Create2DRenderer(_swap);

            // TODO:
            // If a previous renderer is active at this point, our system fucks up.
            // Need to find a way to create a new renderer without breaking the current one.
            Renderer.End2D();

            _textDisplay = Graphics.Output.CreateSwapChain("FontEditor.TextDisplay", new GorgonSwapChainSettings()
            {
                Window = _panel.panelText,
                Format = BufferFormat.R8G8B8A8_UIntNormal
            });

            _sampleText.Length = 0;
            _sampleText.Append(GorgonFontEditorPlugIn.Settings.SampleText);

            _text = Renderer.Renderables.CreateText("Sample.Text", Font, _sampleText.ToString());
            _text.Color = Color.Black;

            return _panel;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFontContent"/> class.
        /// </summary>
		/// <param name="plugIn">The plug-in that creates this object.</param>
        public GorgonFontContent(ContentPlugIn plugIn)
            : base()
        {
			_plugIn = plugIn;
			_settings = new GorgonFontSettings();
        }
        #endregion
    }
}
