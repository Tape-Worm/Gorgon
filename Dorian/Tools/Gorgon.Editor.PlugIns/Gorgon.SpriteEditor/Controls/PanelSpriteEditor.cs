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
// Created: Wednesday, November 12, 2014 12:02:02 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using GorgonLibrary.Editor.SpriteEditorPlugIn.Properties;
using GorgonLibrary.Input;

namespace GorgonLibrary.Editor.SpriteEditorPlugIn.Controls
{
	/// <summary>
	/// Main UI for sprite editing.
	/// </summary>
	partial class PanelSpriteEditor 
		: ContentPanel
	{
		#region Variables.
		// The sprite being edited.
		private GorgonSpriteContent _content;
		// The plug-in that created the content.
		private GorgonSpriteEditorPlugIn _plugIn;
		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform localization on the control text properties.
		/// </summary>
		protected override void LocalizeControls()
		{
			Text = Resources.GORSPR_CONTENT_TYPE;
		}

		/// <summary>
		/// Function called when the content has changed.
		/// </summary>
		public override void RefreshContent()
		{
			base.RefreshContent();

			if (_content == null)
			{
				throw new InvalidCastException(string.Format(Resources.GORSPR_ERR_CONTENT_NOT_SPRITE, Content.Name));
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="PanelSpriteEditor"/> class.
		/// </summary>
		public PanelSpriteEditor(GorgonSpriteContent spriteContent, GorgonInputFactory input)
			: base(spriteContent, input)
		{
			_content = spriteContent;
			_plugIn = (GorgonSpriteEditorPlugIn)_content.PlugIn;

			InitializeComponent();
		}
		#endregion
	}
}
