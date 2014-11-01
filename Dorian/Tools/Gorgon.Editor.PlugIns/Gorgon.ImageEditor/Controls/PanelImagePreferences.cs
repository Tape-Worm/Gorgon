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
// Created: Saturday, November 1, 2014 1:00:52 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GorgonLibrary.Editor.ImageEditorPlugIn.Properties;

namespace GorgonLibrary.Editor.ImageEditorPlugIn.Controls
{
	/// <summary>
	/// Panel for image editor preferences.
	/// </summary>
	public partial class PanelImagePreferences 
		: PreferencePanel
	{
		#region Variables.

		#endregion

		#region Properties.

		#endregion

		#region Methods.
		/// <summary>
		/// Function to localize the text on the controls for the panel.
		/// </summary>
		/// <remarks>
		/// Override this method to supply localized text for any controls on the panel.
		/// </remarks>
		protected override void LocalizeControls()
		{
			labelImageEditorSettings.Text = Resources.GORIMG_TEXT_IMAGE_EDITOR_SETTINGS;
			checkShowAnimations.Text = Resources.GORIMG_TEXT_SHOW_ANIMATIONS;
			checkShowActualSize.Text = Resources.GORIMG_TEXT_SHOW_ACTUALSIZE_ON_LOAD;
		}

		/// <summary>
		/// Function to commit any settings.
		/// </summary>
		public override void CommitSettings()
		{
			GorgonImageEditorPlugIn.Settings.UseAnimations = checkShowAnimations.Checked;
			GorgonImageEditorPlugIn.Settings.StartWithActualSize = checkShowActualSize.Checked;

			GorgonImageEditorPlugIn.Settings.Save();
		}

		/// <summary>
		/// Function to read the current settings into their respective controls.
		/// </summary>
		public override void InitializeSettings()
		{
			checkShowAnimations.Checked = GorgonImageEditorPlugIn.Settings.UseAnimations;
			checkShowActualSize.Checked = GorgonImageEditorPlugIn.Settings.StartWithActualSize;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="PanelImagePreferences"/> class.
		/// </summary>
		public PanelImagePreferences()
		{
			InitializeComponent();
		}
		#endregion
	}
}
