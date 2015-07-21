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
// Created: Thursday, March 07, 2013 8:17:32 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Editor.FontEditorPlugIn.Properties;
using Gorgon.IO;

namespace Gorgon.Editor.FontEditorPlugIn
{
    /// <summary>
    /// Font editor plug-in interface.
    /// </summary>
    public class GorgonFontEditorPlugIn
        : ContentPlugIn, IPlugInSettingsUI
	{
		#region Variables.
		private ToolStripMenuItem _createItem;
		private bool _disposed;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the cached fonts for the font editor.
		/// </summary>
		internal static SortedDictionary<string, Font> CachedFonts
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the settings for the plug-in.
		/// </summary>
		internal static GorgonFontPlugInSettings Settings
		{
			get;
			private set;
		}
        #endregion

		#region Methods.
		/// <summary>
		/// Function to update the font cache.
		/// </summary>
		private static void UpdateCachedFonts()
		{
			// Clear the cached fonts.
			if (CachedFonts != null)
			{
			    foreach (var font in CachedFonts)
			    {
			        font.Value.Dispose();
			    }
			}

			var fonts = new SortedDictionary<string, Font>(StringComparer.OrdinalIgnoreCase);

			// Get font families for previewing - Only up to 1500, after that we have to use the default font for whatever control is making use of this.
			foreach (var family in FontFamily.Families.Take(1500))
			{
				Font newFont = null;

				if (fonts.ContainsKey(family.Name))
				{
					continue;
				}

				if (family.IsStyleAvailable(FontStyle.Regular))
				{
					newFont = new Font(family, 16.0f, FontStyle.Regular, GraphicsUnit.Pixel);
				}
				else if (family.IsStyleAvailable(FontStyle.Bold))
				{
					newFont = new Font(family, 16.0f, FontStyle.Bold, GraphicsUnit.Pixel);
				}
				else if (family.IsStyleAvailable(FontStyle.Italic))
				{
					newFont = new Font(family, 16.0f, FontStyle.Italic, GraphicsUnit.Pixel);
				}

				// Only add if we could use the regular, bold or italic style.
				if (newFont != null)
				{
					fonts.Add(family.Name, newFont);
				}
			}

			CachedFonts = fonts;
		}

        /// <summary>
        /// Function to determine if a plug-in can be used.
        /// </summary>
        /// <returns>
        /// A string containing a list of reasons why the plug-in is not valid for use, or an empty string if the control is valid.
        /// </returns>        
        protected override string ValidatePlugIn()
        {
	        return string.Empty;
        }

        /// <summary>
        /// Function to create a content object interface.
        /// </summary>
        /// <param name="settings">The initial settings for the content.</param>
        /// <returns>
        /// A new content object interface.
        /// </returns>
        protected override ContentObject OnCreateContentObject(ContentSettings settings)
        {
            return new GorgonFontContent(this, (GorgonFontContentSettings)settings);
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
					if (_createItem != null)
					{
						_createItem.Dispose();
					}

					if (CachedFonts != null)
					{
						foreach (var font in CachedFonts)
						{
							font.Value.Dispose();							
						}
						CachedFonts.Clear();						
					}
				}

				CachedFonts = null;
				_createItem = null;
				_disposed = true;
			}

			base.Dispose(disposing);
		}

        /// <summary>
        /// Function to determine if right-to-left rendering is required for text.
        /// </summary>
        /// <param name="control">Control to examine.</param>
        /// <returns><b>true</b> if RTL text is required, <b>false</b> if not.</returns>
        internal static bool IsRightToLeft(Control control)
        {
            switch (control.RightToLeft)
            {
                case RightToLeft.Yes:
                    return true;
                case RightToLeft.Inherit:
                    return control.Parent != null && IsRightToLeft(control.Parent);
                default:
                    return false;
            }
        }

		/// <summary>
		/// Function to populate file editor attributes for imported content.
		/// </summary>
		/// <param name="stream">Stream to the file.</param>
		/// <param name="attributes">Attributes to populate.</param>
	    public override void GetEditorFileAttributes(Stream stream, IDictionary<string, string> attributes)
		{
			attributes["Type"] = Resources.GORFNT_CONTENT_TYPE;
		}

	    /// <summary>
        /// Function to retrieve the settings for the content.
        /// </summary>
        /// <returns>
        /// The settings interface for the content.
        /// </returns>
        protected override ContentSettings OnGetContentSettings()
        {
            return new GorgonFontContentSettings(new Size(ContentObject.Graphics.Textures.MaxWidth, ContentObject.Graphics.Textures.MaxHeight));
        }

		/// <summary>
		/// Function to retrieve the create menu item for this content.
		/// </summary>
		/// <returns>The menu item for this </returns>
		public override ToolStripMenuItem GetCreateMenuItem()
		{
		    return _createItem
		           ?? (_createItem = CreateMenuItem("itemCreateGorgonFont", Resources.GORFNT_TEXT_CREATE_NEW_FONT, GetContentIcon()));
		}

        /// <summary>
		/// Function to return the icon for the content.
		/// </summary>
		/// <returns>
		/// The 16x16 image for the content.
		/// </returns>
		public override Image GetContentIcon()
		{
			return Resources.font_document_16x16;
		}
	    #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFontEditorPlugIn"/> class.
        /// </summary>
        public GorgonFontEditorPlugIn()
            : base(Resources.GORFNT_DESC)
        {
            FileExtensions.Add(new GorgonFileExtension("gorFont", $"{Resources.GORFNT_CONTENT_EXTENSION_DESC} (*.gorFont)"));
			UpdateCachedFonts();
			
			Settings = new GorgonFontPlugInSettings();
			Settings.Load();
        }
        #endregion

        #region IPlugInSettingsUI Members
        /// <summary>
        /// Function to return the UI object for the settings.
        /// </summary>
        /// <returns>
        /// The UI object for the settings.
        /// </returns>
        public PreferencePanel GetSettingsUI()
        {
            return new PanelFontPreferences();
        }
        #endregion
    }
}
