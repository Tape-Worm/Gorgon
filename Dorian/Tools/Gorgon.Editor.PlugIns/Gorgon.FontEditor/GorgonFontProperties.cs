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
using GorgonLibrary.Configuration;
using GorgonLibrary.Editor.FontEditorPlugIn.Properties;

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
	/// <summary>
	/// Plug-in specific settings.
	/// </summary>
	class GorgonFontProperties
		: EditorPlugInSettings
	{
		#region Properties.
        /// <summary>
        /// Property to set or return the sample text.
        /// </summary>
        [ApplicationSetting("FontSampleText", typeof(string), "FontEditor")]
        public string SampleText
        {
            get;
            set;
        }

		/// <summary>
		/// Property to set or return the color of the preview text in the editor.
		/// </summary>
		[ApplicationSetting("PreviewTextColor", typeof(int), "PreviewText")]
		public int TextColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the background color of the preview text in the editor.
		/// </summary>
		[ApplicationSetting("PreviewBackgroundColor", typeof(int), "PreviewText")]
		public int BackgroundColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the preview text should have a shadow or not.
		/// </summary>
		[ApplicationSetting("PreviewShadowEnabled", typeof(bool), "PreviewText")]
		public bool ShadowEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the offset of the shadow on the preview text.
		/// </summary>
		[ApplicationSetting("PreviewShadowOffset", typeof(Point), "PreviewText")]
		public Point ShadowOffset
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the opacity of the shadow on the preview text.
		/// </summary>
		[ApplicationSetting("PreviewShadowOpacity", typeof(float), "PreviewText")]
		public float ShadowOpacity
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the default size type.
		/// </summary>
		[ApplicationSetting("FontSizeType", Graphics.FontHeightMode.Points, typeof(Graphics.FontHeightMode), "FontEditor")]
		public Graphics.FontHeightMode FontSizeType
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the default font anti-alias mode.
		/// </summary>
		[ApplicationSetting("FontAntiAliasMode", Graphics.FontAntiAliasMode.AntiAliasHQ, typeof(Graphics.FontAntiAliasMode), "FontEditor")]
		public Graphics.FontAntiAliasMode FontAntiAliasMode
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the default font texture size.
		/// </summary>
		[ApplicationSetting("FontTextureSize", typeof(Size), "FontEditor")]
		public Size FontTextureSize
		{
			get;
			set;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFontProperties"/> class.
		/// </summary>
		public GorgonFontProperties()
			: base("FontEditor.PlugIn", new Version(1, 0, 0, 0))
		{
			FontTextureSize = new Size(256, 256);
			TextColor = Color.White.ToArgb();            
			BackgroundColor = Color.FromArgb(255, DarkFormsRenderer.WindowBackground).ToArgb();
			ShadowEnabled = false;
			ShadowOffset = new Point(1, 1);
			ShadowOpacity = 0.5f;
            SampleText = Resources.GORFNT_DEFAULT_TEXT;
		}
		#endregion
	}
}
