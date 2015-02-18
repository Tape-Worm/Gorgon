using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GorgonLibrary.Properties;

namespace GorgonLibrary.UI
{
	/// <summary>
	/// A theme to apply to flat forms.
	/// </summary>
	/// <remarks>A flat form theme is a means to apply multiple colour settings to a <see cref="FlatForm"/> window.  Themes can be loaded from an xml file or set manually via code.  
	/// By assigning a theme to a flat form, the colors for that window are automatically set by the colors in the window.
	/// <para>
	/// The theme object inherits from the ToolStrip renderer class and will be applied as a visual style for toolbars and menus as well.
	/// </para>
	/// </remarks>
	public class FlatFormTheme
		: ToolStripRenderer
	{
		#region Variables.

		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the background color for a toolstrip.
		/// </summary>
		public Color ToolStripBackColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the color used for the tool strip drop down arrow.
		/// </summary>
		public Color ToolStripArrowColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the background color used for checked menu items when hilighted.
		/// </summary>
		public Color CheckBoxBackColorHilight
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the background color used for checked menu items.
		/// </summary>
		public Color CheckBoxBackColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the border color for a drop down menu.
		/// </summary>
		public Color DropDownBorderColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the foreground color for the window and menus.
		/// </summary>
		public Color ForeColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the foreground color when the window is inactive.
		/// </summary>
		public Color ForeColorInactive
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the color to use for disabled items.
		/// </summary>
		public Color DisabledColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the hilight color used for items in a menu.
		/// </summary>
		public Color HilightForeColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the hilight background color for items in a menu.
		/// </summary>
		public Color HilightBackColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the color for the window background.
		/// </summary>
		public Color WindowBackground
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the color for the window border when the window is active.
		/// </summary>
		public Color WindowBorderActive
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the color for the window border when the window is inactive.
		/// </summary>
		public Color WindowBorderInactive
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the standard foreground color for the window sizing decoration icons (maximize and minimize/restore).
		/// </summary>
		public Color WindowSizeIconsForeColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the foreground color for the window sizing decoration icons (maximize and minimize/restore) when the mouse cursor is over the icon.
		/// </summary>
		public Color WindowSizeIconsForeColorHilight
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the background color for the window sizing decoration icons (maximize and minimize/restore) when the mouse cursor is over the icon.
		/// </summary>
		public Color WindowSizeIconsBackColorHilight
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the close button icon foreground color.
		/// </summary>
		public Color WindowCloseIconForeColor
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the close icon foreground color when the mouse cursor is over it.
		/// </summary>
		public Color WindowCloseIconForeColorHilight
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the close icon background color when the mouse cursor is over it.
		/// </summary>
		public Color WindowCloseIconBackColorHilight
		{
			get;
			set;
		}
		#endregion

		#region Methods.
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
				e.ArrowColor = !e.Item.Selected ? ForeColor : HilightForeColor;
			}
			else
			{
				e.ArrowColor = DisabledColor;
			}

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
					textColor = HilightForeColor;
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
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.ToolStripRenderer.RenderMenuItemBackground"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripItemRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
		{
			if (!(e.Item is ToolStripMenuItem))
			{
				OnRenderItemBackground(e);
				return;
			}

			var item = (ToolStripMenuItem)e.Item;

			if (item.Selected)
			{
				using (Brush backBrush = new SolidBrush(HilightBackColor))
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

			using (var pen = new Pen(DropDownBorderColor, 1.0f))
			{
				e.Graphics.DrawLine(pen, new Point(0, 0), new Point(e.Item.Width - 1, 0));
				e.Graphics.DrawLine(pen, new Point(0, 0), new Point(0, e.Item.Height));
				e.Graphics.DrawLine(pen, new Point(e.Item.Width - 1, 0), new Point(e.Item.Width - 1, e.Item.Height));
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.ToolStripRenderer.RenderButtonBackground"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderButtonBackground(ToolStripItemRenderEventArgs e)
		{
			var button = e.Item as ToolStripButton;

			if (e.Item.Selected)
			{
				using (Brush backBrush = new SolidBrush(((button == null) || (!button.Checked)) ? HilightBackColor : CheckBoxBackColorHilight))
					e.Graphics.FillRectangle(backBrush, new Rectangle(1, 1, e.Item.Width - 2, e.Item.Height - 2));
			}
			else
			{
				if (button != null)
				{
					if (!button.Checked)
					{
						return;
					}

					using (Brush backBrush = new SolidBrush(CheckBoxBackColor))
					{
						e.Graphics.FillRectangle(backBrush, new Rectangle(1, 1, e.Item.Width - 2, e.Item.Height - 2));
					}

					return;
				}

				base.OnRenderButtonBackground(e);
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.ToolStripRenderer.RenderItemCheck"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripItemImageRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
		{
			var newRect = new Rectangle(e.Item.Width - e.Image.Width - 5, e.ImageRectangle.Top, e.ImageRectangle.Width, e.ImageRectangle.Height);
			if ((e.Item.Enabled) && (!e.Item.Selected))
			{
				e.Graphics.DrawImage(Resources.Check_Enabled1, newRect);
			}
			else
			{
				e.Graphics.DrawImage(Resources.Check_Disabled1, newRect);
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.ToolStripRenderer.RenderToolStripBackground"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderToolStripBackground(ToolStripRenderEventArgs e)
		{
			using (Brush backBrush = new SolidBrush(ToolStripBackColor))
			{
				e.Graphics.FillRectangle(backBrush, e.AffectedBounds);

				if ((!e.ToolStrip.IsDropDown))
				{
					return;
				}

				using (var borderPen = new Pen(DropDownBorderColor))
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
		/// Raises the <see cref="E:System.Windows.Forms.ToolStripRenderer.RenderSeparator"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripSeparatorRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderSeparator(ToolStripSeparatorRenderEventArgs e)
		{
			using (var pen = new Pen(DropDownBorderColor, 1.0f))
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
		/// Raises the <see cref="M:System.Windows.Forms.ToolStripRenderer.OnRenderSplitButtonBackground(System.Windows.Forms.ToolStripItemRenderEventArgs)" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripItemRenderEventArgs" /> that contains the event data.</param>
		protected override void OnRenderSplitButtonBackground(ToolStripItemRenderEventArgs e)
		{
			var item = e.Item as ToolStripSplitButton;

			Debug.Assert(item != null, "Drop down button is NULL!");

			if (item.Selected)
			{
				using (Brush backBrush = new SolidBrush(HilightBackColor))
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

			using (var pen = new Pen(DropDownBorderColor, 1.0f))
			{
				var direction = ArrowDirection.Down;

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
															ToolStripArrowColor,
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
		/// Raises the <see cref="E:System.Windows.Forms.ToolStripRenderer.RenderDropDownButtonBackground"/> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.ToolStripItemRenderEventArgs"/> that contains the event data.</param>
		protected override void OnRenderDropDownButtonBackground(ToolStripItemRenderEventArgs e)
		{
			var item = e.Item as ToolStripDropDownButton;

			Debug.Assert(item != null, "Drop down button is NULL!");

			if (item.Selected)
			{
				using (Brush backBrush = new SolidBrush(HilightBackColor))
				{
					e.Graphics.FillRectangle(backBrush, new Rectangle(1, 1, e.Item.Width - 2, e.Item.Height - 2));
				}
			}

			if (!item.DropDown.Visible)
			{
				return;
			}

			using (var pen = new Pen(DropDownBorderColor, 1.0f))
			{
				e.Graphics.DrawLine(pen, new Point(0, 0), new Point(e.Item.Width - 1, 0));
				e.Graphics.DrawLine(pen, new Point(0, 0), new Point(0, e.Item.Height));
				e.Graphics.DrawLine(pen, new Point(e.Item.Width - 1, 0), new Point(e.Item.Width - 1, e.Item.Height));
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FlatFormTheme"/> class.
		/// </summary>
		public FlatFormTheme()
		{
			ForeColor = Color.FromKnownColor(KnownColor.WindowText);
			DisabledColor = Color.FromKnownColor(KnownColor.DimGray);
			ForeColorInactive = DisabledColor;
			HilightForeColor = Color.FromKnownColor(KnownColor.HighlightText);
			DropDownBorderColor = Color.FromKnownColor(KnownColor.ActiveBorder);
			CheckBoxBackColor = Color.FromKnownColor(KnownColor.SteelBlue);
			CheckBoxBackColorHilight = Color.FromKnownColor(KnownColor.CornflowerBlue);
			WindowBackground = Color.FromKnownColor(KnownColor.Control);
			WindowBorderActive = Color.FromKnownColor(KnownColor.ActiveBorder);
			WindowBorderInactive = Color.FromKnownColor(KnownColor.InactiveBorder);
			WindowSizeIconsForeColor = ForeColor;
			WindowSizeIconsForeColorHilight = HilightForeColor;
			WindowSizeIconsBackColorHilight = Color.FromKnownColor(KnownColor.Highlight);
			WindowCloseIconForeColor = WindowSizeIconsForeColor;
			WindowCloseIconForeColorHilight = WindowSizeIconsForeColorHilight;
			WindowCloseIconBackColorHilight = Color.DarkRed;
			ToolStripBackColor = WindowBackground;
			ToolStripArrowColor = ForeColor;
		}
		#endregion
	}
}
