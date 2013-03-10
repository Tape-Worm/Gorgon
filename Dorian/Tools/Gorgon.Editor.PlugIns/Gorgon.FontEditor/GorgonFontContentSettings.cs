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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using GorgonLibrary.Configuration;

namespace GorgonLibrary.Editor.FontEditorPlugIn
{
	/// <summary>
	/// Plug-in specific settings.
	/// </summary>
	class GorgonFontContentSettings
		: ContentSettings
	{
		#region Variables.

		#endregion

		#region Properties.
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

		#region Methods.

		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFontContentSettings"/> class.
		/// </summary>
		public GorgonFontContentSettings()
			: base("Gorgon.FontEditor.PlugIn", new Version(1, 0, 0, 0))
		{
			FontTextureSize = new Size(256, 256);
		}
		#endregion
	}
}
