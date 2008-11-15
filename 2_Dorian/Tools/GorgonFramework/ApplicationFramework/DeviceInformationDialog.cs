#region MIT.
//
// Gorgon.
// Copyright (C) 2004 Michael Winsor
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
// Created: Saturday, May 08, 2004 11:59:38 AM
//
#endregion

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using GorgonLibrary;

namespace GorgonLibrary.Framework
{
	/// <summary>
	/// Form for displaying _driver information.
	/// </summary>
	internal partial class DeviceInformationDialog 
		: System.Windows.Forms.Form
	{
		#region Variables.
		private Driver _driver;						// Currently selected driver.
		private System.Drawing.Image _yesImage;		// Yes image.
		private System.Drawing.Image _noImage;		// No image.
		private Font _groupHeaderFont = null;		// Group header font.
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

			_groupHeaderFont = new Font("Tahoma", 8.25f);

			// Get resources.
			_yesImage = global::GorgonLibrary.Framework.Properties.Resources.YesImage2;
			_noImage = global::GorgonLibrary.Framework.Properties.Resources.NoImage2;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create a row with a boolean value.
		/// </summary>
		/// <param name="group">Group to use.</param>
		/// <param name="description">Description to display.</param>
		/// <param name="value">Value for the row.</param>
		private void CreateRow<T>(ListViewGroup group,string description,T value)
		{
			ListViewItem item = null;

			item = listCaps.Items.Add(description);
			item.SubItems.Add(value.ToString());

			group.Items.Add(item);
		}

		/// <summary>
		/// Function to create a group.
		/// </summary>
		/// <param name="caption">Caption for the group.</param>
		/// <returns>A new group object.</returns>
		private ListViewGroup CreateGroup(string caption)
		{
			return listCaps.Groups.Add(caption, caption);
		}

		/// <summary>
		/// Function to retrieve the driver capabilities for display.
		/// </summary>
		private void GetCaps()
		{
			ListViewGroup group = null;			// New group.

			listCaps.BeginUpdate();

			labelCardName.Text = _driver.Description + " capabilities.";

			// Acceleration support.
			group = CreateGroup("Hardware Capabilities.");
			CreateRow(group, "Has Hardware Acceleration?", _driver.HardwareAccelerated);
			CreateRow(group, "Has Hardware Transform and Lighting?", _driver.HardwareTransformAndLighting);
			CreateRow(group, "Has resource management support?", _driver.SupportResourceManagement);

			// Depth Buffer Support.
			group = CreateGroup("Depth Buffer Capabilities.");
			CreateRow(group, "Has support for Z testing?", _driver.SupportZBufferTesting);
			CreateRow(group, "Has support for alternate hidden surface removal?", _driver.SupportAlternateHSR);
			CreateRow(group, "Has support for W depth buffers?", _driver.SupportWBuffer);

			// Vertex/index stuff.
			group = CreateGroup("Vertex/Index Capabilities.");
			CreateRow(group, "Has support for 32bit indices?", _driver.SupportIndex32);
			CreateRow(group, "Maximum vertex buffer streams", _driver.MaximumStreamCount.ToString());
			CreateRow(group, "Maximum vertex point size", _driver.MaximumPointSize.ToString());
			CreateRow(group, "Has support for TnL system memory vertex buffers?", _driver.SupportVertexBufferInSystemMemory);
			CreateRow(group, "Has support for TnL video memory vertex buffers?", _driver.SupportVertexBufferInVideoMemory);
			CreateRow(group, "Has support for vertex shaders?", (_driver.VertexShaderVersion >= new Version(1, 0)) ? true : false);
			if (_driver.VertexShaderVersion >= new Version(1, 0))
				CreateRow(group, "Vertex shader version:", _driver.VertexShaderVersion.ToString());

			// Pixel operation support.
			group = CreateGroup("Pixel Capabilities.");
			CreateRow(group, "Has support for scissor testing?", _driver.SupportScissorTesting);
			CreateRow(group, "Has support for pixel shaders?", (_driver.PixelShaderVersion >= new Version(1, 0)) ? true : false);
			if (_driver.PixelShaderVersion >= new Version(1, 0))
				CreateRow(group, "Pixel shader version:", _driver.PixelShaderVersion.ToString());

			// Lighting support.
			group = CreateGroup("Light Capabilities.");
			if (_driver.MaximumLightCount != int.MaxValue)
				CreateRow(group, "Maximum light count", _driver.MaximumLightCount.ToString());
			else
				CreateRow(group, "Maximum light count", "Unlimited");

			// Texture support.
			group = CreateGroup("Texture Capabilities.");
			CreateRow(group, "Maximum texture width", _driver.MaximumTextureWidth.ToString());
			CreateRow(group, "Maximum texture height", _driver.MaximumTextureHeight.ToString());
			CreateRow(group, "Has support for dithering?", _driver.SupportDithering);
			CreateRow(group, "Has support for dynamic textures?", _driver.SupportDynamicTextures);
			CreateRow(group, "Has support for anisotropic filtering?", _driver.SupportAnisotropicFiltering);
			CreateRow(group, "Has support for automatic mip-map generation?", _driver.SupportAutoGeneratingMipMaps);
			CreateRow(group, "Has support for seperate texture memories?", _driver.SupportSeperateTextureMemories);
			CreateRow(group, "Has support for non-power of 2 sized textures?", _driver.SupportNonPowerOfTwoTexture);
			CreateRow(group, "Has support for non square textures?", _driver.SupportNonSquareTexture);
			if (!_driver.SupportNonPowerOfTwoTexture)
				CreateRow(group, "Has support for non-power of 2 sized textures with conditions?", _driver.SupportNonPowerOfTwoTextureConditional);
			CreateRow(group, "Has support for alpha textures?", _driver.SupportTextureAlpha);
			CreateRow(group, "Has support for texture wrapping?", _driver.SupportWrappingAddress);
			CreateRow(group, "Has support for texture clamping?", _driver.SupportClampingAddress);
			CreateRow(group, "Has support for texture mirroring?", _driver.SupportMirrorAddress);
			CreateRow(group, "Has support for texture mirroring once?", _driver.SupportMirrorOnceAddress);
			CreateRow(group, "Has support for texture border?", _driver.SupportBorderAddress);
			CreateRow(group, "Has support for texture independent UV addressing?", _driver.SupportIndependentUVAddressing);

			// Render target support.
			group = CreateGroup("Render Target Capabilities.");
			CreateRow(group, "Maximum supported simultaneous render targets", _driver.MaximumSimultaneousRenderTargets.ToString());

			// Texture support.
			group = CreateGroup("Blending Capabilities.");
			CreateRow(group, "Has support for blending source alpha?", _driver.SupportBlendingSourceAlpha);
			CreateRow(group, "Has support for blending source color?", _driver.SupportBlendingSourceColor);
			CreateRow(group, "Has support for blending destination alpha?", _driver.SupportBlendingDestinationAlpha);
			CreateRow(group, "Has support for blending destination color?", _driver.SupportBlendingDestinationColor);
			CreateRow(group, "Has support for blending opaque?", _driver.SupportBlendingOne);
			CreateRow(group, "Has support for blending transparent?", _driver.SupportBlendingZero);
			CreateRow(group, "Has support for blending inverse destination alpha?", _driver.SupportBlendingInverseDestinationAlpha);
			CreateRow(group, "Has support for blending inverse destination color?", _driver.SupportBlendingInverseDestinationColor);
			CreateRow(group, "Has support for blending inverse source alpha?", _driver.SupportBlendingInverseSourceAlpha);
			CreateRow(group, "Has support for blending both inverse source & dest alpha?", _driver.SupportBlendingBothInverseSourceAlpha);
			CreateRow(group, "Has support for blending inverse source color?", _driver.SupportBlendingInverseSourceColor);
			CreateRow(group, "Has support for blending inverse source alpha with saturation?", _driver.SupportBlendingSourceAlphaSaturation);
			CreateRow(group, "Has support for blending factor?", _driver.SupportBlendingFactor);

			// Stencil support.
			group = CreateGroup("Stencil Buffer Support.");
			CreateRow(group, "Has support for stencil buffers?", _driver.SupportStencil);
			if (_driver.SupportStencil)
			{
				CreateRow(group, "Has support for stencil keep?", _driver.SupportStencilKeep);
				CreateRow(group, "Has support for stencil replace?", _driver.SupportStencilReplace);
				CreateRow(group, "Has support for stencil zero?", _driver.SupportStencilZero);
				CreateRow(group, "Has support for stencil increment?", _driver.SupportStencilIncrement);
				CreateRow(group, "Has support for stencil decrement?", _driver.SupportStencilDecrement);
				CreateRow(group, "Has support for stencil increment w/saturation?", _driver.SupportStencilIncrementSaturate);
				CreateRow(group, "Has support for stencil decrement w/saturation?", _driver.SupportStencilDecrementSaturate);
				CreateRow(group, "Has support for stencil invert?", _driver.SupportStencilInvert);
				CreateRow(group, "Has support for stencil two sided stencil?", _driver.SupportTwoSidedStencil);
			}

			// Presentation interval support.
			group = CreateGroup("Presentation Interval Support.");
			CreateRow(group, "Immediate presentation", (_driver.PresentIntervalSupport & VSyncIntervals.IntervalNone) != 0 ? true : false);
			CreateRow(group, "Wait for 1 retrace.", (_driver.PresentIntervalSupport & VSyncIntervals.IntervalOne) != 0 ? true : false);
			CreateRow(group, "Wait for 2 retraces.", (_driver.PresentIntervalSupport & VSyncIntervals.IntervalTwo) != 0 ? true : false);
			CreateRow(group, "Wait for 3 retraces.", (_driver.PresentIntervalSupport & VSyncIntervals.IntervalThree) != 0 ? true : false);
			CreateRow(group, "Wait for 4 retraces.", (_driver.PresentIntervalSupport & VSyncIntervals.IntervalFour) != 0 ? true : false);
			listCaps.EndUpdate();
		}

		/// <summary>
		/// Handles the DrawSubItem event of the listCaps control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.DrawListViewSubItemEventArgs"/> instance containing the event data.</param>
		private void listCaps_DrawSubItem(object sender, DrawListViewSubItemEventArgs e)
		{
			Point position = Point.Empty;		// Position.
			Brush selectBrush = null;			// Selected brush.

			if (e.ColumnIndex == 0)
			{
				e.DrawDefault = true;
				return;
			}

			if ((e.ItemState & ListViewItemStates.Selected) == 0)
			{
				selectBrush = new SolidBrush(e.SubItem.BackColor);
				e.Graphics.FillRectangle(selectBrush, e.Bounds);
				selectBrush.Dispose();
			}
			else
			{
				// Draw a selected background.
				selectBrush = new SolidBrush(Color.FromKnownColor(KnownColor.Highlight));
				e.Graphics.FillRectangle(selectBrush, e.Bounds);
				selectBrush.Dispose();
			}

			position.X = (e.Bounds.Width / 2 - 8) + e.Bounds.Left;
			position.Y = (e.Bounds.Height / 2 - 8) + e.Bounds.Top;

			if (string.Compare(e.SubItem.Text, "true", true) == 0)
				e.Graphics.DrawImage(_yesImage, new Rectangle(position, new Size(16, 16)), new Rectangle(0, 0, _yesImage.Width, _yesImage.Height), GraphicsUnit.Pixel);
			else
			{
				if (string.Compare(e.SubItem.Text, "false", true) == 0)
					e.Graphics.DrawImage(_noImage, new Rectangle(position, new Size(16, 16)), new Rectangle(0, 0, _noImage.Width, _noImage.Height), GraphicsUnit.Pixel);
				else
					e.DrawText(TextFormatFlags.HorizontalCenter);
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"></see> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (Owner != null)
				Icon = Owner.Icon;

			GetCaps();
			listCaps.AutoResizeColumns(ColumnHeaderAutoResizeStyle.HeaderSize);
		}
		#endregion
	}
}
