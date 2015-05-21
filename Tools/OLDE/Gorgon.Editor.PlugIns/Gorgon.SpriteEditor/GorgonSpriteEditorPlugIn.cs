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
// Created: Tuesday, November 11, 2014 11:21:47 PM
// 
#endregion

using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Gorgon.Editor.SpriteEditorPlugIn.Properties;
using Gorgon.IO;

namespace Gorgon.Editor.SpriteEditorPlugIn
{
	/// <summary>
	/// Sprite editor plug-in interface.
	/// </summary>
    public class GorgonSpriteEditorPlugIn
		: ContentPlugIn, IPlugInSettingsUI
	{
		#region Variables.
		// Flag to indicate that the object was disposed.
		private bool _disposed;
		// Menu item to create a new sprite.
		private ToolStripMenuItem _createItem;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the settings for the sprite editor plug-in.
		/// </summary>
		internal static GorgonSpritePlugInSettings Settings
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
					if (_createItem != null)
					{
						_createItem.Dispose();
					}
				}
			}

			_disposed = true;

			base.Dispose(disposing);
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
			return new GorgonSpriteContent(this, (GorgonSpriteContentSettings)settings);
		}

		/// <summary>
		/// Function to populate file editor attributes for imported content.
		/// </summary>
		/// <param name="stream">Stream to the file.</param>
		/// <param name="attributes">Attributes to populate.</param>
		public override void GetEditorFileAttributes(Stream stream, IDictionary<string, string> attributes)
		{
			attributes["Type"] = Resources.GORSPR_CONTENT_TYPE;	
		}

		/// <summary>
		/// Function to retrieve the settings for the content.
		/// </summary>
		/// <returns>
		/// The settings for the content.
		/// </returns>
		protected override ContentSettings OnGetContentSettings()
		{
			return new GorgonSpriteContentSettings();
		}

		/// <summary>
		/// Function to return the icon for the content.
		/// </summary>
		/// <returns>
		/// The 16x16 image for the content.
		/// </returns>
		public override Image GetContentIcon()
		{
			return Resources.sprite_16x16;
		}

		/// <summary>
		/// Function to retrieve the create menu item for this content.
		/// </summary>
		/// <returns>The menu item for this </returns>
		public override ToolStripMenuItem GetCreateMenuItem()
		{
			return _createItem
				   ?? (_createItem = CreateMenuItem("itemCreateGorgonFont", Resources.GORSPR_TEXT_CREATE_NEW_SPRITE, GetContentIcon()));
		}

		/// <summary>
		/// Function to determine if a plug-in can be used.
		/// </summary>
		/// <returns>
		/// A string containing a list of reasons why the plug-in is not valid for use, or an empty string if the control is not valid for use.
		/// </returns>
		protected override string ValidatePlugIn()
		{
			return !HasImageEditor() ? Resources.GORSPR_ERR_NEED_IMAGE_EDITOR : string.Empty;
		}

		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSpriteEditorPlugIn"/> class.
		/// </summary>
		public GorgonSpriteEditorPlugIn()
			: base(Resources.GORSPR_DESC)
		{
			Settings = new GorgonSpritePlugInSettings();
			Settings.Load();

			FileExtensions.Add(new GorgonFileExtension("gorSprite", string.Format("{0} (*.gorSprite)", Resources.GORSPR_CONTENT_EXTENSION_DESC)));
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
			// TODO: Return a preference panel for the sprite editor.
			return null;
		}
		#endregion
	}
}
