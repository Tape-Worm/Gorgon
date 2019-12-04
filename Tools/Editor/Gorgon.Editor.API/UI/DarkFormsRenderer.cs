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

using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace Gorgon.Editor.UI
{
	/// <summary>
	/// A renderer to draw the visual styles for UI elements.
	/// </summary>
	public class DarkFormsRenderer
		: ToolStripRenderer
	{
		/// <summary>
        /// Focused/mouse over foreground color.
        /// </summary>
		public static readonly Color FocusedForeground = Color.Black;
		/// <summary>
        /// Focused/mouse over background color.
        /// </summary>
		public static readonly Color FocusedBackground = Color.Orange;
        /// <summary>
        /// Normal window background.
        /// </summary>
        public static readonly Color WindowBackground = Color.FromArgb(35, 35, 37);
        /// <summary>
        /// Dark background color for most items.
        /// </summary>
        public static readonly Color DarkBackground = Color.FromArgb(28, 28, 28);
        /// <summary>
        /// Hilighted menu item background color.
        /// </summary>
		public static readonly Color MenuHilightBackground = Color.SteelBlue;
        /// <summary>
        /// Hilighted menu item foreground color.
        /// </summary>
		public static readonly Color MenuHilightForeground = Color.White;
        /// <summary>
        /// Border color.
        /// </summary>
		public static readonly Color BorderColor = Color.FromArgb(47, 47, 48);
        /// <summary>
        /// Disabled color.
        /// </summary>
		public static readonly Color DisabledColor = Color.FromArgb(64, 64, 64);
        /// <summary>
        /// Foreground color.
        /// </summary>
		public static readonly Color ForeColor = Color.White;
		/// <summary>
        /// Foreground color for a cut item.
        /// </summary>
		public static readonly Color CutForeground = Color.FromArgb(128, 128, 100);
		/// <summary>
        /// Foreground color for an open item.
        /// </summary>
		public static readonly Color OpenFileForeground = Color.LimeGreen;
        
		/// <summary>
		/// Raises the <see cref="ToolStripRenderer.RenderToolStripBorder"/> event.
		/// </summary>
		/// <param name="e">A <see cref="ToolStripRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderToolStripBorder(ToolStripRenderEventArgs e)
		{
		}        

		/// <summary>
		/// Raises the <see cref="ToolStripRenderer.RenderArrow"/> event.
		/// </summary>
		/// <param name="e">A <see cref="ToolStripArrowRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderArrow(ToolStripArrowRenderEventArgs e)
		{
			if (e.Item.Enabled)
			{
				e.ArrowColor = !e.Item.Selected ? ForeColor : MenuHilightForeground;
			}
			else
			{
				e.ArrowColor = DisabledColor;
			}

			base.OnRenderArrow(e);
		}

		/// <summary>
		/// Raises the <see cref="ToolStripRenderer.RenderGrip"/> event.
		/// </summary>
		/// <param name="e">A <see cref="ToolStripGripRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderGrip(ToolStripGripRenderEventArgs e)
		{
			base.OnRenderGrip(e);

			if (e.GripStyle == ToolStripGripStyle.Hidden)
			{
				return;
			}

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
		/// <param name="e">A <see cref="ToolStripRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderImageMargin(ToolStripRenderEventArgs e)
		{
		}

		/// <summary>
		/// Raises the <see cref="ToolStripRenderer.RenderItemText"/> event.
		/// </summary>
		/// <param name="e">A <see cref="ToolStripItemTextRenderEventArgs"/> that contains the event data.</param>
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

			var item = e.Item as ToolStripMenuItem;
			if ((item != null) && (item.IsOnDropDown))
			{
				e.TextRectangle = new Rectangle(e.TextRectangle.X - 12,
				                                e.TextRectangle.Y,
				                                e.TextRectangle.Width,
				                                e.TextRectangle.Height);
			}

			ToolStrip strip = e.Item.GetCurrentParent();

			if (!(strip is ToolStripDropDown))
			{
				if (e.Item.AutoSize)
				{
					e.TextRectangle = new Rectangle(e.TextRectangle.Location,
					                                TextRenderer.MeasureText(e.Text, e.TextFont, e.TextRectangle.Size, e.TextFormat));
				}
			}
                        
			TextRenderer.DrawText(e.Graphics, e.Text, e.TextFont, e.TextRectangle, textColor, e.TextFormat);

            if ((item == null) || (!item.CheckOnClick) || (item.Checked))
            {
                return;
            }

            var checkPos = new Point(item.Width - 34, (int)(item.Height * 0.5f - 6));
            Color color = item.Selected ? MenuHilightForeground : ForeColor;

            if (!item.Enabled)
            {
                color = DisabledColor;
            }

            using (var pen = new Pen(color))
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                e.Graphics.DrawRectangle(pen, new Rectangle(checkPos.X, checkPos.Y, 12, 12));
            }
        }

        /// <summary>
        /// Raises the <see cref="ToolStripRenderer.RenderMenuItemBackground"/> event.
        /// </summary>
        /// <param name="e">A <see cref="ToolStripItemRenderEventArgs"/> that contains the event data.</param>
        protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
		{
			if (!(e.Item is ToolStripMenuItem))
			{
				base.OnRenderItemBackground(e);
				return;
			}

			var item = e.Item as ToolStripMenuItem;

			if (item.Selected)
			{
				using (Brush backBrush = new SolidBrush(MenuHilightBackground))
				{
					e.Graphics.FillRectangle(backBrush, new Rectangle(1, 1, e.Item.Width - 2, e.Item.Height - 2));
				}
			}

			if (item.IsOnDropDown)
			{
				return;
			}

			if (!item.DropDown.Visible)
			{
				return;
			}

			using (var pen = new Pen(BorderColor, 1.0f))
			{
				e.Graphics.DrawLine(pen, new Point(0, 0), new Point(e.Item.Width - 1, 0));
				e.Graphics.DrawLine(pen, new Point(0, 0), new Point(0, e.Item.Height));
				e.Graphics.DrawLine(pen, new Point(e.Item.Width - 1, 0), new Point(e.Item.Width - 1, e.Item.Height));
			}
		}

		/// <summary>
		/// Raises the <see cref="ToolStripRenderer.RenderButtonBackground"/> event.
		/// </summary>
		/// <param name="e">A <see cref="ToolStripRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
		{
			var button = e.Item as ToolStripButton;

			if (e.Item.Selected)
			{
                using (Brush backBrush = new SolidBrush(MenuHilightBackground))
                {
                    e.Graphics.FillRectangle(backBrush, new Rectangle(1, 1, e.Item.Width - 2, e.Item.Height - 2));
                }
            }
			else
			{
				if (button != null)
				{
					if (!button.Checked)
					{
						return;
					}

					using (Brush backBrush = new SolidBrush(MenuHilightBackground))
					{
						e.Graphics.FillRectangle(backBrush, new Rectangle(1, 1, e.Item.Width - 2, e.Item.Height - 2));
					}

					return;
				}

				base.OnRenderButtonBackground(e);
			}
		}

		/// <summary>
		/// Raises the <see cref="ToolStripRenderer.RenderItemCheck"/> event.
		/// </summary>
		/// <param name="e">A <see cref="ToolStripItemImageRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
		{
            var item = (ToolStripMenuItem)e.Item;

            if (!item.CheckOnClick)
            {
                return;
            }

            var checkPos = new Point(item.Width - 34, (int)(item.Height * 0.5f - 6));
            Color color = item.Selected ? MenuHilightForeground : ForeColor;

            if (!item.Enabled)
            {
                color = DisabledColor;
            }

            using (var pen = new Pen(color))
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
                e.Graphics.DrawRectangle(pen, new Rectangle(checkPos.X, checkPos.Y, 12, 12));

                checkPos = new Point(item.Width - 32, (int)(item.Height * 0.5f - 4));

                using (var brush = new SolidBrush(color))
                {
                    e.Graphics.FillRectangle(brush, new Rectangle(checkPos.X, checkPos.Y, 8, 8));
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="ToolStripRenderer.RenderToolStripBackground"/> event.
        /// </summary>
        /// <param name="e">A <see cref="ToolStripRenderEventArgs"/> that contains the event data.</param>
        protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
		{
			using(Brush backBrush = new SolidBrush(DarkBackground))
			{				
				e.Graphics.FillRectangle(backBrush, e.AffectedBounds);

				if ((!e.ToolStrip.IsDropDown))
				{
					return;
				}

				using (var borderPen = new Pen(BorderColor))
				{
					e.Graphics.DrawRectangle(borderPen,
					                         new Rectangle(e.AffectedBounds.Left,
					                                       e.AffectedBounds.Top,
					                                       e.AffectedBounds.Width - 1,
					                                       e.AffectedBounds.Height - 1));
				}

				e.Graphics.FillRectangle(backBrush, e.ConnectedArea);
			}
		}

		/// <summary>
		/// Raises the <see cref="ToolStripRenderer.RenderSeparator"/> event.
		/// </summary>
		/// <param name="e">A <see cref="ToolStripSeparatorRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
		{
            using (var pen = new Pen(BorderColor, 1.0f))
            {
	            if (!e.Vertical)
	            {
		            e.Graphics.DrawLine(pen,
		                                new Point(0, e.Item.ContentRectangle.Height / 2),
		                                new Point(e.Item.Width, e.Item.ContentRectangle.Height / 2));
	            }
	            else
	            {
		            e.Graphics.DrawLine(pen,
		                                new Point(e.Item.ContentRectangle.Width / 2, e.Item.ContentRectangle.Top + 3),
		                                new Point(e.Item.ContentRectangle.Width / 2, e.Item.ContentRectangle.Bottom - 5));
	            }
            }
		}

		/// <summary>
		/// Raises the <see cref="ToolStripRenderer.OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs)" /> event.
		/// </summary>
		/// <param name="e">A <see cref="ToolStripItemRenderEventArgs" /> that contains the event data.</param>
		protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
		{
			var item = e.Item as ToolStripSplitButton;

			Debug.Assert(item != null, "Drop down button is NULL!");

			if (item.Selected)
			{
				using (Brush backBrush = new SolidBrush(MenuHilightBackground))
				{
					if (item.ButtonSelected)
					{
						e.Graphics.FillRectangle(backBrush, item.ButtonBounds);

					}

					if (item.DropDownButtonSelected)
					{
						e.Graphics.FillRectangle(backBrush, item.DropDownButtonBounds);
					}
				}
			}

			using (var pen = new Pen(BorderColor, 1.0f))
			{
                ArrowDirection direction = ArrowDirection.Down;

				switch (item.DropDownDirection)
				{
					case ToolStripDropDownDirection.AboveLeft:
					case ToolStripDropDownDirection.AboveRight:
						direction = ArrowDirection.Up;
						break;
					case ToolStripDropDownDirection.Left:
						direction = ArrowDirection.Left;
						break;
					case ToolStripDropDownDirection.Right:
						direction = ArrowDirection.Right;
						break;
				}
				DrawArrow(new ToolStripArrowRenderEventArgs(e.Graphics,
				                                            item,
															item.DropDownButtonBounds, 
				                                            Color.White,
				                                            direction));

				if (!item.DropDown.Visible)
				{
					e.Graphics.DrawLine(pen, item.SplitterBounds.Location, new Point(item.SplitterBounds.X, item.SplitterBounds.Height));
					return;
				}

				e.Graphics.DrawLine(pen, new Point(0, 0), new Point(e.Item.Width - 1, 0));
				e.Graphics.DrawLine(pen, new Point(0, 0), new Point(0, e.Item.Height));
				e.Graphics.DrawLine(pen, new Point(e.Item.Width - 1, 0), new Point(e.Item.Width - 1, e.Item.Height));
			}
		}

		/// <summary>
		/// Raises the <see cref="ToolStripRenderer.RenderDropDownButtonBackground"/> event.
		/// </summary>
		/// <param name="e">A <see cref="ToolStripItemRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
		{
			var item = e.Item as ToolStripDropDownButton;

			Debug.Assert(item != null, "Drop down button is NULL!");

			if (item.Selected)
			{
				using (Brush backBrush = new SolidBrush(MenuHilightBackground))
				{
					e.Graphics.FillRectangle(backBrush, new Rectangle(1, 1, e.Item.Width - 2, e.Item.Height - 2));
				}
			}

			if (!item.DropDown.Visible)
			{
				return;
			}

			using (var pen = new Pen(BorderColor, 1.0f))
			{
				e.Graphics.DrawLine(pen, new Point(0, 0), new Point(e.Item.Width - 1, 0));
				e.Graphics.DrawLine(pen, new Point(0, 0), new Point(0, e.Item.Height));
				e.Graphics.DrawLine(pen, new Point(e.Item.Width - 1, 0), new Point(e.Item.Width - 1, e.Item.Height));
			}
		}        
	}
}
