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

namespace GorgonLibrary.Editor
{
	public class DarkFormsRenderer
		: ToolStripRenderer
	{
        /// <summary>
        /// Dark background color for most items.
        /// </summary>
        public static readonly Color DarkBackground = Color.FromArgb(58, 58, 58);
        /// <summary>
        /// Hilighted menu item background color.
        /// </summary>
		public static readonly Color MenuHilightBackground = Color.FromArgb(189, 189, 189);
        /// <summary>
        /// Hilighted menu item foreground color.
        /// </summary>
		public static readonly Color MenuHilightForeground = Color.FromArgb(51, 51, 51);
        /// <summary>
        /// Border color.
        /// </summary>
		public static readonly Color BorderColor = Color.FromArgb(88, 85, 90);
        /// <summary>
        /// Disabled color.
        /// </summary>
		public static readonly Color DisabledColor = Color.FromArgb(102, 102, 102);
        /// <summary>
        /// Foreground color.
        /// </summary>
		public static readonly Color ForeColor = Color.White;
        
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
			if (e.Item.Enabled)
			{
				if (!e.Item.Selected)
				{
					e.ArrowColor = ForeColor;
				}
				else
				{
					e.ArrowColor = MenuHilightForeground;
				}
			}
			else
				e.ArrowColor = DisabledColor;
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
		/// Draws the item background.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
		{
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.ToolStripRenderer.RenderItemText"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripItemTextRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			Color textColor = ForeColor;

            if (!e.Item.Enabled)
            {
                textColor = DisabledColor;
            }
            else
            {				
                if (e.Item.Selected)
                {
                    textColor = MenuHilightForeground;
                }
            }

			ToolStripMenuItem item = e.Item as ToolStripMenuItem;
			if ((item != null) && (item.IsOnDropDown))
			{				
				e.TextRectangle = new Rectangle(e.TextRectangle.X - 12, e.TextRectangle.Y, e.TextRectangle.Width, e.TextRectangle.Height);
			}

			ToolStrip strip = e.Item.GetCurrentParent();

			if (!(strip is ToolStripDropDown))
			{
				if (e.Item.AutoSize)
					e.TextRectangle = new Rectangle(e.TextRectangle.Location, TextRenderer.MeasureText(e.Text, e.TextFont, e.TextRectangle.Size, e.TextFormat));
			}
                        
			TextRenderer.DrawText(e.Graphics, e.Text, e.TextFont, e.TextRectangle, textColor, e.TextFormat);
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

			if (item.Selected)
			{
				using (Brush backBrush = new SolidBrush(MenuHilightBackground))
				{
					e.Graphics.FillRectangle(backBrush, new Rectangle(1, 1, e.Item.Width - 2, e.Item.Height - 2));
				}
			}

			if (!item.IsOnDropDown)
			{
				if (item.DropDown.Visible)
				{
                    using (var pen = new Pen(BorderColor, 1.0f))
                    {
                        e.Graphics.DrawLine(pen, new Point(0, 0), new Point(e.Item.Width - 1, 0));
                        e.Graphics.DrawLine(pen, new Point(0, 0), new Point(0, e.Item.Height));
                        e.Graphics.DrawLine(pen, new Point(e.Item.Width - 1, 0), new Point(e.Item.Width - 1, e.Item.Height));
                    }
				}
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
				using (Brush backBrush = new SolidBrush(MenuHilightBackground))
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
			Rectangle newRect = new Rectangle(e.Item.Width - e.Image.Width - 5, e.ImageRectangle.Top, e.ImageRectangle.Width, e.ImageRectangle.Height);
			if ((e.Item.Enabled) && (!e.Item.Selected))
			{
				e.Graphics.DrawImage(Properties.Resources.Check_Enabled, newRect);
			}
			else
			{
				e.Graphics.DrawImage(Properties.Resources.Check_Disabled, newRect);
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.ToolStripRenderer.RenderToolStripBackground"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
		{
			using(Brush backBrush = new SolidBrush(DarkBackground))
			{				
				e.Graphics.FillRectangle(backBrush, e.AffectedBounds);

				if ((e.ToolStrip.IsDropDown))
				{					
					ToolStripDropDownMenu menu = e.ToolStrip as ToolStripDropDownMenu;
                    using (Pen borderPen = new Pen(BorderColor))
                    {
                        e.Graphics.DrawRectangle(borderPen, new Rectangle(e.AffectedBounds.Left, e.AffectedBounds.Top, e.AffectedBounds.Width - 1, e.AffectedBounds.Height - 1));
                    }
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
            using (var pen = new Pen(BorderColor, 1.0f))
            {
                if (!e.Vertical)
                    e.Graphics.DrawLine(pen, new Point(0, e.Item.ContentRectangle.Height / 2), new Point(e.Item.Width, e.Item.ContentRectangle.Height / 2));
                else
                    e.Graphics.DrawLine(pen, new Point(e.Item.ContentRectangle.Width / 2, e.Item.ContentRectangle.Top + 3), new Point(e.Item.ContentRectangle.Width / 2, e.Item.ContentRectangle.Bottom - 5));
            }
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.ToolStripRenderer.RenderDropDownButtonBackground"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripItemRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
		{
			ToolStripDropDownButton item = e.Item as ToolStripDropDownButton;

			if (item.Selected)
			{
				using (Brush backBrush = new SolidBrush(MenuHilightBackground))
					e.Graphics.FillRectangle(backBrush, new Rectangle(1, 1, e.Item.Width - 2, e.Item.Height - 2));
			}

			if (item.DropDown.Visible)
			{
                using (var pen = new Pen(BorderColor, 1.0f))
                {
                    e.Graphics.DrawLine(pen, new Point(0, 0), new Point(e.Item.Width - 1, 0));
                    e.Graphics.DrawLine(pen, new Point(0, 0), new Point(0, e.Item.Height));
                    e.Graphics.DrawLine(pen, new Point(e.Item.Width - 1, 0), new Point(e.Item.Width - 1, e.Item.Height));
                }
			}
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="DarkFormsRenderer"/> class.
		/// </summary>
		public DarkFormsRenderer()			
			: base()
		{			
		}
	}
}
