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
        #endregion

        #region Properties.
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
				using (var buffer = new GorgonDataStream((int)stream.Length))
				{
					stream.CopyTo(buffer);
					buffer.Position = 0;
					Font = Graphics.Fonts.FromStream(File.BaseFileName, buffer);
				}				
			}

			// Get the settings.
			this.Name = File.Name;
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
			Renderer.Clear(_panel.panelTextures.BackColor);

			Renderer.Drawing.SmoothingMode = SmoothingMode.None;

			_panel.DrawFontTexture();			

			Renderer.Render(2);
		}

        /// <summary>
        /// Function to initialize the content editor.
        /// </summary>
        /// <returns>
        /// A control to place in the primary interface window.
        /// </returns>        
        public override System.Windows.Forms.Control InitializeContent()
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
