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
// Created: Thursday, September 26, 2013 9:22:57 PM
// 
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using Gorgon.Graphics;
using Gorgon.IO;
using Gorgon.UI;

namespace Gorgon.Editor.FontEditorPlugIn
{
    /// <summary>
    /// Settings for font content.
    /// </summary>
    class GorgonFontContentSettings
        : ContentSettings
    {
        #region Variables.
        private readonly Size _maxTextureSize;               // Maximum texture size.
        #endregion

        #region Properties.
	    /// <summary>
        /// Property to return the settings for the font.
        /// </summary>
        public GorgonFontSettings Settings
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
	    /// <summary>
        /// Function to initialize the settings for the content.
        /// </summary>
        /// <returns>
        /// <c>true</c> if the object was set up, <c>false</c> if not.
        /// </returns>
        public override bool PerformSetup()
        {
            FormNewFont newFont = null;

            try
            {
                newFont = new FormNewFont
                {
                    MaxTextureSize = _maxTextureSize,
                    FontCharacters = Settings.Characters
                };

                if (newFont.ShowDialog() != DialogResult.OK)
                {
                    return false;
                }

                Cursor.Current = Cursors.WaitCursor;

                Name = newFont.FontName.FormatFileName();

	            Settings.FontFamilyName = newFont.FontFamilyName;
                Settings.Size = newFont.FontSize;
                Settings.FontHeightMode = newFont.FontHeightMode;
                Settings.AntiAliasingMode = newFont.FontAntiAliasMode;
                Settings.FontStyle = newFont.FontStyle;
                Settings.TextureSize = newFont.FontTextureSize;
                Settings.Characters = newFont.FontCharacters;

                return true;
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
                }
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFontContentSettings"/> struct.
        /// </summary>
        /// <param name="maxTextureSize">The maximum size of a texture.</param>
        public GorgonFontContentSettings(Size maxTextureSize)
        {
            Settings = new GorgonFontSettings();
            _maxTextureSize = maxTextureSize;
        }
        #endregion
    }
}
