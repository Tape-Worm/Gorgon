//
// Gorgon.
// Copyright (C) 2004 Michael Winsor
//
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
//
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
//
// Created: Saturday, May 08, 2004 11:59:38 AM
//

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using SharpUtilities.Controls;
using GorgonLibrary;

namespace GorgonLibrary.Framework
{
	/// <summary>
	/// Form for displaying _driver information.
	/// </summary>
	internal partial class DeviceInformationDialog : System.Windows.Forms.Form
	{
		#region Variables.
		private Driver _driver;						// Currently selected driver.
		private bool _fadeDirection;				// Fading direction.
		private System.Drawing.Image _yesImage;		// Yes image.
		private System.Drawing.Image _noImage;		// No image.		
		#endregion

		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="selectedDriver">Currently selected driver.</param>
		public DeviceInformationDialog(Driver selectedDriver)
		{
			this.DoubleBuffered = true;

			//
			// Required for Windows Form Designer support
			//
			InitializeComponent();

			_driver = selectedDriver;

			// Get resources.
			_yesImage = global::GorgonLibrary.Properties.Resources.YesImage;
			_noImage = global::GorgonLibrary.Properties.Resources.NoImage;
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Function to close the dialog.
        /// </summary>
        public void CloseDialog()
        {
            _fadeDirection = false;
            fadeTimer.Enabled = true;
        }

		/// <summary>
		/// Function to create a row with a boolean value.
		/// </summary>
		/// <param name="groupIndex">Index of the group to use.</param>
		/// <param name="description">Description to display.</param>
		/// <param name="value">Value for the row.</param>
		private void CreateBooleanRow(int groupIndex,string description,bool value)
		{
			int rowIndex = 0;		// Row index.

			cardInfo.Groups[groupIndex].Rows.Add();
			rowIndex = cardInfo.Groups[groupIndex].Rows.Count - 1;
			cardInfo.Groups[groupIndex].Rows[rowIndex].BackgroundGradient1 = Color.White;
			cardInfo.Groups[groupIndex].Rows[rowIndex].BorderStyle = InfoGridBorderStyles.FixedSingle;
			cardInfo.Groups[groupIndex].Rows[rowIndex].VerticalAlignment = InfoGridAlignment.Center;
			cardInfo.Groups[groupIndex].Rows[rowIndex].Description = description;
			cardInfo.Groups[groupIndex].Rows[rowIndex].DataType = InfoGridRowType.Boolean;
			cardInfo.Groups[groupIndex].Rows[rowIndex].DataValue = value.ToString();
			cardInfo.Groups[groupIndex].Rows[rowIndex].Height = 20;
			cardInfo.Groups[groupIndex].Rows[rowIndex].ImageTrue = _yesImage;
			cardInfo.Groups[groupIndex].Rows[rowIndex].ImageFalse = _noImage;
		}

		/// <summary>
		/// Function to create a row with a string value.
		/// </summary>
		/// <param name="groupIndex">Index of the group to use.</param>
		/// <param name="description">Description to display.</param>
		/// <param name="value">Value for the row.</param>
		private void CreateStringRow(int groupIndex, string description, string value)
		{
			int rowIndex = 0;		// Row index.

			cardInfo.Groups[groupIndex].Rows.Add();
			rowIndex = cardInfo.Groups[groupIndex].Rows.Count - 1;
			cardInfo.Groups[groupIndex].Rows[rowIndex].BackgroundGradient1 = Color.White;
			cardInfo.Groups[groupIndex].Rows[rowIndex].BorderStyle = InfoGridBorderStyles.FixedSingle;
			cardInfo.Groups[groupIndex].Rows[rowIndex].VerticalAlignment = InfoGridAlignment.Center;
			cardInfo.Groups[groupIndex].Rows[rowIndex].Description = description;
			cardInfo.Groups[groupIndex].Rows[rowIndex].DataType = InfoGridRowType.Text;
			cardInfo.Groups[groupIndex].Rows[rowIndex].DataValue = value;
			cardInfo.Groups[groupIndex].Rows[rowIndex].Height = 20;
		}

		/// <summary>
		/// Function to create a group.
		/// </summary>
		/// <param name="caption">Caption for the group.</param>
		private void CreateGroup(string caption)
		{
			int groupIndex = 0;		// Group index.

			cardInfo.Groups.Add();
			groupIndex = cardInfo.Groups.Count - 1;
			cardInfo.Groups[groupIndex].Text = caption;
			cardInfo.Groups[groupIndex].BackgroundStyle = InfoGridBackgroundStyles.LeftDiagonal;
			cardInfo.Groups[groupIndex].BackgroundGradient1 = Color.FromArgb(128, 128, 128); 
			cardInfo.Groups[groupIndex].BackgroundGradient2 = Color.FromArgb(64, 64, 64);
		}

		/// <summary>
		/// Ok button click event.
		/// </summary>
		/// <param name="sender">Sender of this event.</param>
		/// <param name="e">Event arguments</param>
		private void OK_Click(object sender, System.EventArgs e)
		{
			CloseDialog();
		}

		/// <summary>
		/// Handles the Tick event of the fadeTimer control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void fadeTimer_Tick(object sender, EventArgs e)
		{
			if (_fadeDirection)
			{
				Opacity += 0.025;
				if (Opacity >= 1.0)
					fadeTimer.Enabled = false;
			}
			else
			{
				Opacity -= 0.025;
				if (Opacity <= 0.1)
				{
					fadeTimer.Enabled = false;
					Hide();
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.FormClosing"></see> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.FormClosingEventArgs"></see> that contains the event data.</param>
		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			base.OnFormClosing(e);

			if (e.CloseReason == CloseReason.UserClosing)
			{
                CloseDialog();
				e.Cancel = true;
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);		

			driverDescription.Text = _driver.Description;

			cardInfo.Text = _driver.Description + " capabilities.";

			// Acceleration support.
			CreateGroup("Hardware Capabilities.");
			CreateBooleanRow(0,"Has Hardware Acceleration?",_driver.Hal);
			CreateBooleanRow(0, "Has Hardware Transform and Lighting?", _driver.TnL);
			CreateBooleanRow(0, "Has resource management support?", _driver.SupportResourceManagement);


			// Depth Buffer Support.
			CreateGroup("Depth Buffer Capabilities.");
			CreateBooleanRow(1, "Has support for Z testing?", _driver.SupportZBufferTesting);
			CreateBooleanRow(1, "Has support for alternate hidden surface removal?", _driver.SupportAlternateHSR);
			CreateBooleanRow(1, "Has support for W depth buffers?", _driver.SupportWBuffer);

			// Vertex/index stuff.
			CreateGroup("Vertex/Index Capabilities.");
			CreateBooleanRow(2, "Has support for 32bit indices?", _driver.SupportIndex32);
			CreateStringRow(2, "Maximum vertex buffer streams", _driver.MaximumStreamCount.ToString());
			CreateStringRow(2, "Maximum vertex point size", _driver.MaximumPointSize.ToString());
			CreateBooleanRow(2, "Has support for TnL system memory vertex buffers?", _driver.SupportVertexBufferInSystemMemory);
			CreateBooleanRow(2, "Has support for TnL video memory vertex buffers?", _driver.SupportVertexBufferInVideoMemory);
			CreateBooleanRow(2, "Has support for vertex shaders?", (_driver.VertexShaderVersion >= new Version(1, 0)) ? true : false);
			if (_driver.VertexShaderVersion >= new Version(1, 0))
				CreateStringRow(2, "Vertex shader version:", _driver.VertexShaderVersion.ToString());

			// Pixel operation support.
			CreateGroup("Pixel Capabilities.");
			CreateBooleanRow(3, "Has support for scissor testing?", _driver.SupportScissorTesting);
			CreateBooleanRow(3, "Has support for pixel shaders?", (_driver.PixelShaderVersion >= new Version(1, 0)) ? true : false);
			if (_driver.PixelShaderVersion >= new Version(1, 0))
				CreateStringRow(3, "Pixel shader version:", _driver.PixelShaderVersion.ToString());

			// Lighting support.
			CreateGroup("Light Capabilities.");
			if (_driver.MaximumLightCount != int.MaxValue) 
				CreateStringRow(4, "Maximum light count", _driver.MaximumLightCount.ToString());
			else
				CreateStringRow(4, "Maximum light count", "Unlimited");

			// Texture support.
			CreateGroup("Texture Capabilities.");
			CreateStringRow(5, "Maximum texture width", _driver.MaximumTextureWidth.ToString());
			CreateStringRow(5, "Maximum texture height", _driver.MaximumTextureHeight.ToString());
			CreateBooleanRow(5, "Has support for dithering?", _driver.SupportDithering);
			CreateBooleanRow(5, "Has support for dynamic textures?", _driver.SupportDynamicTextures);
			CreateBooleanRow(5, "Has support for anisotropic filtering?", _driver.SupportAnisotropicFiltering);
			CreateBooleanRow(5, "Has support for automatic mip-map generation?", _driver.SupportAutoGeneratingMipMaps);
			CreateBooleanRow(5, "Has support for seperate texture memories?", _driver.SupportSeperateTextureMemories);
			CreateBooleanRow(5, "Has support for non-power of 2 sized textures?", _driver.SupportNonPowerOfTwoTexture);
			CreateBooleanRow(5, "Has support for non square textures?", _driver.SupportNonSquareTexture);
			if (!_driver.SupportNonPowerOfTwoTexture)
				CreateBooleanRow(5, "Has support for non-power of 2 sized textures with conditions?", _driver.SupportNonPowerOfTwoTextureConditional);
			CreateBooleanRow(5, "Has support for alpha textures?", _driver.SupportTextureAlpha);
			CreateBooleanRow(5, "Has support for texture wrapping?", _driver.SupportWrappingAddress);
			CreateBooleanRow(5, "Has support for texture clamping?", _driver.SupportClampingAddress);
			CreateBooleanRow(5, "Has support for texture mirroring?", _driver.SupportMirrorAddress);
			CreateBooleanRow(5, "Has support for texture mirroring once?", _driver.SupportMirrorOnceAddress);
			CreateBooleanRow(5, "Has support for texture border?", _driver.SupportBorderAddress);
			CreateBooleanRow(5, "Has support for texture independent UV addressing?", _driver.SupportIndependentUVAddressing);

			// Texture support.
			CreateGroup("Blending Capabilities.");
			CreateBooleanRow(6, "Has support for blending source alpha?", _driver.SupportBlendingSourceAlpha);
			CreateBooleanRow(6, "Has support for blending source color?", _driver.SupportBlendingSourceColor);
			CreateBooleanRow(6, "Has support for blending destination alpha?", _driver.SupportBlendingDestinationAlpha);
			CreateBooleanRow(6, "Has support for blending destination color?", _driver.SupportBlendingDestinationColor);
			CreateBooleanRow(6, "Has support for blending opaque?", _driver.SupportBlendingOne);
			CreateBooleanRow(6, "Has support for blending transparent?", _driver.SupportBlendingZero);			
			CreateBooleanRow(6, "Has support for blending inverse destination alpha?", _driver.SupportBlendingInverseDestinationAlpha);
			CreateBooleanRow(6, "Has support for blending inverse destination color?", _driver.SupportBlendingInverseDestinationColor);
			CreateBooleanRow(6, "Has support for blending inverse source alpha?", _driver.SupportBlendingInverseSourceAlpha);
			CreateBooleanRow(6, "Has support for blending inverse source color?", _driver.SupportBlendingInverseSourceColor);
			CreateBooleanRow(6, "Has support for blending inverse source alpha with saturation?", _driver.SupportBlendingSourceAlphaSaturation);
			CreateBooleanRow(6, "Has support for blending factor?", _driver.SupportBlendingFactor);

			// Stencil support.
			CreateGroup("Stencil Buffer Support.");
			CreateBooleanRow(7, "Has support for stencil buffers?", _driver.SupportStencil);
			if (_driver.SupportStencil)
			{
				CreateBooleanRow(7, "Has support for stencil keep?", _driver.SupportStencilKeep);
				CreateBooleanRow(7, "Has support for stencil replace?", _driver.SupportStencilReplace);
				CreateBooleanRow(7, "Has support for stencil zero?", _driver.SupportStencilZero);
				CreateBooleanRow(7, "Has support for stencil increment?", _driver.SupportStencilIncrement);
				CreateBooleanRow(7, "Has support for stencil decrement?", _driver.SupportStencilDecrement);
				CreateBooleanRow(7, "Has support for stencil increment w/saturation?", _driver.SupportStencilIncrementSaturate);
				CreateBooleanRow(7, "Has support for stencil decrement w/saturation?", _driver.SupportStencilDecrementSaturate);
				CreateBooleanRow(7, "Has support for stencil invert?", _driver.SupportStencilInvert);
				CreateBooleanRow(7, "Has support for stencil two sided stencil?", _driver.SupportTwoSidedStencil);
			}

			// Presentation interval support.
			CreateGroup("Presentation Interval Support.");
			CreateBooleanRow(8, "Immediate presentation", (_driver.PresentIntervalSupport & VSyncIntervals.IntervalNone)!=0 ? true : false);
			CreateBooleanRow(8, "Wait for 1 retrace.", (_driver.PresentIntervalSupport & VSyncIntervals.IntervalOne)!=0 ? true : false);
			CreateBooleanRow(8, "Wait for 2 retraces.", (_driver.PresentIntervalSupport & VSyncIntervals.IntervalTwo)!=0 ? true : false);
			CreateBooleanRow(8, "Wait for 3 retraces.", (_driver.PresentIntervalSupport & VSyncIntervals.IntervalThree)!=0 ? true : false);
			CreateBooleanRow(8, "Wait for 4 retraces.", (_driver.PresentIntervalSupport & VSyncIntervals.IntervalFour)!=0 ? true : false);

			if (Owner != null)
				Icon = Owner.Icon;

			_fadeDirection = true;
			fadeTimer.Enabled = true;
		}
		#endregion
	}
}
