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
// Created: Saturday, November 15, 2014 10:37:49 PM
// 
#endregion

using System;
using GorgonLibrary.Configuration;

namespace GorgonLibrary.Editor.SpriteEditorPlugIn
{
	/// <summary>
	/// Settings for the gorgon sprite plug-in.
	/// </summary>
	class GorgonSpritePlugInSettings
		: EditorPlugInSettings
	{
		#region Properties.
        /// <summary>
        /// Property to set or return the last path used to find a texture.
        /// </summary>
        [ApplicationSetting("LastTexturePath", typeof(string), "Paths")]
        public string LastTexturePath
        {
            get;
            set;
        }

		/// <summary>
		/// Property to set or return the size of the zoom window when editing glyphs.
		/// </summary>
		[ApplicationSetting("ZoomWindowSize", 256, typeof(int), "SpriteEditor")]
		public int ZoomWindowSize
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the scaling factor for the zoom window.
		/// </summary>
		[ApplicationSetting("ZoomWindowScaleFactor", 2.0f, typeof(float), "SpriteEditor")]
		public float ZoomWindowScaleFactor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to snap the zoom window to the corners of the editor window, or to follow the cursor.
		/// </summary>
		[ApplicationSetting("ZoomWindowSnap", false, typeof(bool), "SpriteEditor")]
		public bool ZoomWindowSnap
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonSpritePlugInSettings"/> class.
		/// </summary>
		public GorgonSpritePlugInSettings()
			: base("SpriteEditor.PlugIn", new Version(1, 0, 0, 0))
		{
			
		}
		#endregion
	}
}
