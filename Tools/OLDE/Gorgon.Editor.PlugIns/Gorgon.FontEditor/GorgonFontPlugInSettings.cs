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
// Created: Saturday, March 9, 2013 6:42:06 PM
// 
#endregion

using System;
using System.Drawing;
using Gorgon.Configuration;
using Gorgon.Editor.FontEditorPlugIn.Properties;
using Gorgon.Graphics;

namespace Gorgon.Editor.FontEditorPlugIn
{
	/// <summary>
	/// Plug-in specific settings.
	/// </summary>
	class GorgonFontPlugInSettings
		: EditorPlugInSettings
	{
		#region Properties.
        /// <summary>
        /// Property to set or return the sample text.
        /// </summary>
        [GorgonApplicationSetting("FontSampleText", typeof(string), "FontEditor")]
        public string SampleText
        {
            get;
            set;
        }

		/// <summary>
		/// Property to set or return the color of the preview text in the editor.
		/// </summary>
		[GorgonApplicationSetting("PreviewTextColor", typeof(int), "PreviewText")]
		public int TextColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the background color of the preview text in the editor.
		/// </summary>
		[GorgonApplicationSetting("PreviewBackgroundColor", typeof(int), "PreviewText")]
		public int BackgroundColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the preview text should have a shadow or not.
		/// </summary>
		[GorgonApplicationSetting("PreviewShadowEnabled", typeof(bool), "PreviewText")]
		public bool ShadowEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the offset of the shadow on the preview text.
		/// </summary>
		[GorgonApplicationSetting("PreviewShadowOffset", typeof(Point), "PreviewText")]
		public Point ShadowOffset
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the opacity of the shadow on the preview text.
		/// </summary>
		[GorgonApplicationSetting("PreviewShadowOpacity", typeof(float), "PreviewText")]
		public float ShadowOpacity
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the blending mode for the preview text.
		/// </summary>
		[GorgonApplicationSetting("PreviewBlendMode", typeof(string), "PreviewText")]
		public string BlendMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether transition animations are shown for the editor.
		/// </summary>
		[GorgonApplicationSetting("ShowAnimations", typeof(bool), "FontEditor")]
		public bool ShowAnimations
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the default size type.
		/// </summary>
		[GorgonApplicationSetting("FontSizeType", FontHeightMode.Points, typeof(FontHeightMode), "FontEditor")]
		public FontHeightMode FontSizeType
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the default font anti-alias mode.
		/// </summary>
		[GorgonApplicationSetting("FontAntiAliasMode", FontAntiAliasMode.AntiAlias, typeof(FontAntiAliasMode), "FontEditor")]
		public FontAntiAliasMode FontAntiAliasMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the default font texture size.
		/// </summary>
		[GorgonApplicationSetting("FontTextureSize", typeof(Size), "FontEditor")]
		public Size FontTextureSize
		{
			get;
			set;
		}

        /// <summary>
        /// Property to set or return the last texture import path.
        /// </summary>
        [GorgonApplicationSetting("LastTextureImportPath", typeof(string), "FontEditor")]
	    public string LastTextureImportPath
	    {
	        get;
	        set;
	    }

        /// <summary>
        /// Property to set or return the last view used with the texture import dialog.
        /// </summary>
        [GorgonApplicationSetting("LastTextureImportDialogview", typeof(FileViews), "FontEditor")]
	    public FileViews LastTextureImportDialogView
	    {
	        get;
	        set;
	    }

		/// <summary>
		/// Property to set or return the last view used with the texture import dialog.
		/// </summary>
		[GorgonApplicationSetting("LastTextureExtension", "png", typeof(string), "FontEditor")]
		public string LastTextureExtension
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the size of the zoom window when editing glyphs.
		/// </summary>
		[GorgonApplicationSetting("ZoomWindowSize", 256, typeof(int), "FontEditor")]
		public int ZoomWindowSize
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the scaling factor for the zoom window.
		/// </summary>
		[GorgonApplicationSetting("ZoomWindowScaleFactor", 2.0f, typeof(float), "FontEditor")]
		public float ZoomWindowScaleFactor
		{
			get;
			set;
		}

        /// <summary>
        /// Property to set or return whether to snap the zoom window to the corners of the editor window, or to follow the cursor.
        /// </summary>
		[GorgonApplicationSetting("ZoomWindowSnap", false, typeof(bool), "FontEditor")]
		public bool ZoomWindowSnap
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFontPlugInSettings"/> class.
		/// </summary>
		public GorgonFontPlugInSettings()
			: base("FontEditor.PlugIn", new Version(1, 0, 0, 0))
		{
			ShowAnimations = true;
			ZoomWindowSnap = false;
			ZoomWindowScaleFactor = 2.0f;
			ZoomWindowSize = 256;
			FontTextureSize = new Size(256, 256);
			TextColor = Color.White.ToArgb();            
			BackgroundColor = Color.FromArgb(255, DarkFormsRenderer.WindowBackground).ToArgb();
			ShadowEnabled = false;
			ShadowOffset = new Point(1, 1);
			ShadowOpacity = 0.5f;
            SampleText = Resources.GORFNT_DEFAULT_PREVIEW_TEXT;
            LastTextureImportDialogView = FileViews.Large;
			BlendMode = Resources.GORFNT_TEXT_BLEND_MOD;
		}
		#endregion
	}
}
