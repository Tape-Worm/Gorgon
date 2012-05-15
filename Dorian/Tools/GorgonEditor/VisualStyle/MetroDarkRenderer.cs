#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Sunday, May 06, 2012 12:13:14 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Windows.Forms.VisualStyles;

namespace GorgonLibrary.GorgonEditor
{
	public class MetroDarkRenderer
		: ToolStripRenderer
	{
		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.ToolStripRenderer.RenderToolStripBorder"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
		{

		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.ToolStripRenderer.RenderArrow"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripArrowRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
		{
			e.ArrowColor = Color.White;
			base.OnRenderArrow(e);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.ToolStripRenderer.RenderGrip"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripGripRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderGrip(ToolStripGripRenderEventArgs e)
		{
			base.OnRenderGrip(e);

			if (e.GripStyle == ToolStripGripStyle.Hidden)
				return;

			if (e.GripDisplayStyle == ToolStripGripDisplayStyle.Horizontal)
			{
				e.Graphics.DrawLine(Pens.White, new Point(e.AffectedBounds.Left, e.AffectedBounds.Top), new Point(e.AffectedBounds.Right, e.AffectedBounds.Top));
				e.Graphics.DrawLine(Pens.White, new Point(e.AffectedBounds.Left, e.AffectedBounds.Top + 2), new Point(e.AffectedBounds.Right, e.AffectedBounds.Top + 2));
			}
			else
			{
				e.Graphics.DrawLine(Pens.White, new Point(e.AffectedBounds.Left, e.AffectedBounds.Top), new Point(e.AffectedBounds.Left, e.AffectedBounds.Bottom));
				e.Graphics.DrawLine(Pens.White, new Point(e.AffectedBounds.Left + 2, e.AffectedBounds.Top), new Point(e.AffectedBounds.Left + 2, e.AffectedBounds.Bottom));
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.ToolStripRenderer.RenderItemText"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripItemTextRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			Color textColor = Color.White;

			try
			{
				if (!e.Item.Enabled)
					textColor = Color.FromArgb(255, 0, 0, 0);
			
				using (Brush textBrush = new SolidBrush(textColor))
				{
					using (StringFormat format = new StringFormat(StringFormat.GenericDefault))
					{
						format.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.Show;
						if (e.Item.GetCurrentParent() is MenuStrip)
							e.Graphics.DrawString(e.Text, e.TextFont, textBrush, e.TextRectangle, format);
						else
							e.Graphics.DrawString(e.Text, e.TextFont, textBrush, new RectangleF(3, 1, e.TextRectangle.Width, e.TextRectangle.Height), format);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.Message);
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.ToolStripRenderer.RenderMenuItemBackground"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripItemRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
		{
			if (!(e.Item is ToolStripMenuItem))
			{
				base.OnRenderItemBackground(e);
				return;
			}

			ToolStripMenuItem item = e.Item as ToolStripMenuItem;

			if ((item.Selected) || (item.DropDown.Visible))
			{
				item.DropDown.DropShadowEnabled = false;
				using (Brush backBrush = new SolidBrush(Color.FromKnownColor(KnownColor.SteelBlue)))
					e.Graphics.FillRectangle(backBrush, new Rectangle(1, 1, e.Item.Width - 2, e.Item.Height - 2));
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.ToolStripRenderer.RenderButtonBackground"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
		{			
			if (e.Item.Selected)
			{
				Color color = Color.FromKnownColor(KnownColor.SteelBlue);

				if (Control.MouseButtons != MouseButtons.None)
					color = Color.FromKnownColor(KnownColor.DodgerBlue);

				using (Brush backBrush = new SolidBrush(color))
					e.Graphics.FillRectangle(backBrush, new Rectangle(1, 1, e.Item.Width - 2, e.Item.Height - 2));
			}
			else			
				base.OnRenderButtonBackground(e);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.ToolStripRenderer.RenderItemCheck"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripItemImageRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
		{
			//base.OnRenderItemCheck(e);
			Rectangle newRect = new Rectangle(e.Item.Width - e.Image.Width - 5, e.ImageRectangle.Top, e.ImageRectangle.Width, e.ImageRectangle.Height);
			if (e.Item.Enabled)
				e.Graphics.DrawImage(Properties.Resources.Check_Enabled, newRect.Location);
			else
				e.Graphics.DrawImage(Properties.Resources.Check_Disabled, newRect.Location);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.ToolStripRenderer.RenderToolStripBackground"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
		{
			using(Brush backBrush = new SolidBrush(Color.FromArgb(255, 160, 160, 160)))
			{				
				e.Graphics.FillRectangle(backBrush, e.AffectedBounds);

				if ((e.ToolStrip.IsDropDown))
				{
					ToolStripDropDownMenu menu = e.ToolStrip as ToolStripDropDownMenu;

					e.Graphics.DrawRectangle(Pens.Black, new Rectangle(e.AffectedBounds.Left, e.AffectedBounds.Top, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1));
					e.Graphics.FillRectangle(backBrush, e.ConnectedArea);
				}
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.ToolStripRenderer.RenderSeparator"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripSeparatorRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
		{
			e.Graphics.DrawLine(Pens.Black, new Point(0, 0), new Point(e.Item.Width, 0));
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="MetroDarkRenderer"/> class.
		/// </summary>
		public MetroDarkRenderer()			
			: base()
		{			
		}
	}
}
