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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows.Forms;

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
    /// <summary>
    /// Font editor plug-in interface.
    /// </summary>
    public class GorgonFontEditorPlugIn
        : ContentPlugIn
	{
		#region Variables.
		private ToolStripMenuItem _createItem = null;
		private bool _disposed = false;
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
		internal static GorgonFontContentSettings Settings
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether the plug-in supports importing of external data.
		/// </summary>
		public override bool SupportsImport
		{
			get 
			{
				return true;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the font cache.
		/// </summary>
		private void UpdateCachedFonts()
		{
			SortedDictionary<string, Font> fonts = null;

			// Clear the cached fonts.
			if (CachedFonts != null)
			{
				foreach (var font in CachedFonts)
					font.Value.Dispose();
			}

			fonts = new SortedDictionary<string, Font>();

			// Get font families.
			foreach (var family in FontFamily.Families)
			{
				Font newFont = null;

				if (!fonts.ContainsKey(family.Name))
				{
					if (family.IsStyleAvailable(FontStyle.Regular))
						newFont = new Font(family, 16.0f, FontStyle.Regular, GraphicsUnit.Pixel);
					else
					{
						if (family.IsStyleAvailable(FontStyle.Bold))
							newFont = new Font(family, 16.0f, FontStyle.Bold, GraphicsUnit.Pixel);
						else
						{
							if (family.IsStyleAvailable(FontStyle.Italic))
								newFont = new Font(family, 16.0f, FontStyle.Italic, GraphicsUnit.Pixel);
						}
					}

					// Only add if we could use the regular, bold or italic style.
					if (newFont != null)
						fonts.Add(family.Name, newFont);
				}
			}

			CachedFonts = fonts;
		}

        /// <summary>
        /// Function to determine if a plug-in can be used.
        /// </summary>
        /// <returns>
        /// A string containing a list of reasons why the plug-in is not valid for use, or an empty string if the control is not valid for use.
        /// </returns>        
        protected override string ValidatePlugIn()
        {
            StringBuilder invalidReasons = new StringBuilder(512);

            // Currently we won't work on Direct 3D 9 video devices because of issues when saving texture data.
            if (Graphics.VideoDevice.SupportedFeatureLevel == GorgonLibrary.Graphics.DeviceFeatureLevel.SM2_a_b)
            {
                invalidReasons.AppendFormat("This plug-in requires a video device with feature level SM4 or above, the current video device has a feature level of {0}.", Graphics.VideoDevice.SupportedFeatureLevel);
            }

            return invalidReasons.ToString();
        }

		/// <summary>
        /// Function to create a content object interface.
        /// </summary>
        /// <param name="editorObjects">Editor information to pass to the interface.</param>
        /// <returns>
        /// A new content object interface.
        /// </returns>
        protected override ContentObject OnCreateContentObject()
        {
            return new GorgonFontContent(this);
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
		/// Function to retrieve the create menu item for this content.
		/// </summary>
		/// <returns>The menu item for this </returns>
		public override ToolStripMenuItem GetCreateMenuItem()
		{
			if (_createItem == null)
			{
				_createItem = CreateMenuItem("itemCreateGorgonFont", "Create new font...", GetContentIcon());
			}

			return _createItem;
		}

		/// <summary>
		/// Function to return the icon for the content.
		/// </summary>
		/// <returns>
		/// The 16x16 image for the content.
		/// </returns>
		public override System.Drawing.Image GetContentIcon()
		{
			return Properties.Resources.font_document_16x16;
		}
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFontEditorPlugIn"/> class.
        /// </summary>
        public GorgonFontEditorPlugIn()
            : base("Gorgon Font Editor")
        {
			FileExtensions.Add(".gorfont", new Tuple<string, string>("gorFont", "Gorgon Font File (*.gorFont)"));
			UpdateCachedFonts();
			
			Settings = new GorgonFontContentSettings();
			Settings.Load();
        }
        #endregion
    }
}
