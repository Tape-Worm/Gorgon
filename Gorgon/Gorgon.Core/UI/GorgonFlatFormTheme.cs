#region MIT.
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Friday, February 20, 2015 12:28:54 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Gorgon.Core.Properties;
using Gorgon.Design;
using Gorgon.UI.Design;

namespace Gorgon.UI
{
	/// <summary>
	/// Color modification values for the <see cref="GorgonFlatForm"/>.
	/// </summary>
	/// <remarks>A flat form theme is a means to apply multiple color settings to a <see cref="GorgonFlatForm"/> window.  Themes can be loaded from an xml file or set manually via code.  
	/// By assigning a theme to a flat form, the colors for that window are automatically set by the colors in the window.
	/// <para>
	/// The theme object inherits from the ToolStrip renderer class and will be applied as a visual style for tool bars and menus as well.
	/// </para>
	/// </remarks>
	[TypeConverter(typeof(GorgonFlatFormThemeConverter)), Serializable]
	public class GorgonFlatFormTheme
		: ToolStripRenderer, IXmlSerializable
	{
		#region Variables.
		private Color _foreColor;
		private Color _toolStripBackColor;
		private Color _toolStripArrowColor;
		private Color _checkBoxBackColorHilight;
		private Color _checkBoxBackColor;
		private Color _dropDownBorderColor;
		private Color _foreColorInactive;
		private Color _disabledColor;
		private Color _hilightForeColor;
		private Color _hilightBackColor;
		private Color _windowBackground;
		private Color _contentBackground;
		private Color _windowBorderActive;
		private Color _windowBorderInactive;
		private Color _windowSizeIconsForeColor;
		private Color _windowSizeIconsForeColorHilight;
		private Color _windowSizeIconsBackColorHilight;
		private Color _windowCloseIconForeColor;
		private Color _windowCloseIconForeColorHilight;
		private Color _windowCloseIconBackColorHilight;
		private Image _checkEnabledImage;
		private Image _checkDisabledImage;
		#endregion

		#region Events.
		/// <summary>
		/// Event triggered when a color is changed in the theme.
		/// </summary>
		[Browsable(false)]
		public EventHandler PropertyChanged;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the background color for a tool strip.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Color), "Control")]
		[LocalDescription(typeof(Resources), "PROP_THEME_TOOLSTRIPBACKCOLOR_DESC")]
		public Color ToolStripBackColor
		{
			get
			{
				return _toolStripBackColor;
			}
			set
			{
				_toolStripBackColor = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the color used for the tool strip drop down arrow.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Color), "WindowText")]
		[LocalDescription(typeof(Resources), "PROP_THEME_TOOLSTRIPARROWCOLOR_DESC")]
		public Color ToolStripArrowColor
		{
			get
			{
				return _toolStripArrowColor;
			}
			set
			{
				_toolStripArrowColor = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the background color used for checked tool strip buttons when hilighted.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Color), "Highlight")]
		[LocalDescription(typeof(Resources), "PROP_THEME_MENUCHECKBACKCOLORHILIGHT_DESC")]
		public Color CheckBoxBackColorHilight
		{
			get
			{
				return _checkBoxBackColorHilight;
			}
			set
			{
				_checkBoxBackColorHilight = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the background color used for checked tool strip buttons.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Color), "Control")]
		[LocalDescription(typeof(Resources), "PROP_THEME_MENUCHECKBACKCOLOR_DESC")]
		public Color CheckBoxBackColor
		{
			get
			{
				return _checkBoxBackColor;
			}
			set
			{
				_checkBoxBackColor = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the border color for a drop down menu.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Color), "ActiveBorder")]
		[LocalDescription(typeof(Resources), "PROP_THEME_DROPDOWNBORDERCOLOR_DESC")]
		public Color DropDownBorderColor
		{
			get
			{
				return _dropDownBorderColor;
			}
			set
			{
				_dropDownBorderColor = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the foreground color for the window and menus.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Color), "WindowText")]
		[LocalDescription(typeof(Resources), "PROP_THEME_FORECOLOR_DESC")]
		[NotifyParentProperty(true)]
		public Color ForeColor
		{
			get
			{
				return _foreColor;
			}
			set
			{
				_foreColor = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the foreground color when the window is inactive.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Color), "DimGray")]
		[LocalDescription(typeof(Resources), "PROP_THEME_FORECOLOR_INACTIVE_DESC")]
		public Color ForeColorInactive
		{
			get
			{
				return _foreColorInactive;
			}
			set
			{
				_foreColorInactive = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the color to use for disabled items.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Color), "DimGray")]
		[LocalDescription(typeof(Resources), "PROP_THEME_DISABLECOLOR_DESC")]
		public Color DisabledColor
		{
			get
			{
				return _disabledColor;
			}
			set
			{
				_disabledColor = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the hilight color used for items in a menu.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Color), "HighlightText")]
		[LocalDescription(typeof(Resources), "PROP_THEME_HILIGHTFORECOLOR_DESC")]
		public Color HilightForeColor
		{
			get
			{
				return _hilightForeColor;
			}
			set
			{
				_hilightForeColor = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the hilight background color for items in a menu.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Color), "Highlight")]
		[LocalDescription(typeof(Resources), "PROP_THEME_HILIGHTBACKCOLOR_DESC")]
		public Color HilightBackColor
		{
			get
			{
				return _hilightBackColor;
			}
			set
			{
				_hilightBackColor = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the color for the window background.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Color), "Control")]
		[LocalDescription(typeof(Resources), "PROP_THEME_WINBACKCOLOR_DESC")]
		public Color WindowBackground
		{
			get
			{
				return _windowBackground;
			}
			set
			{
				_windowBackground = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the color for the window background.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Color), "Window")]
		[LocalDescription(typeof(Resources), "PROP_THEME_WINBACKCOLOR_DESC")]
		public Color ContentPanelBackground
		{
			get
			{
				return _contentBackground;
			}
			set
			{
				_contentBackground = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the color for the window border when the window is active.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Color), "ActiveBorder")]
		[LocalDescription(typeof(Resources), "PROP_THEME_WINBORDERACTIVE_DESC")]
		public Color WindowBorderActive
		{
			get
			{
				return _windowBorderActive;
			}
			set
			{
				_windowBorderActive = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the color for the window border when the window is inactive.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Color), "InactiveBorder")]
		[LocalDescription(typeof(Resources), "PROP_THEME_WINBORDERINACTIVE_DESC")]
		public Color WindowBorderInactive
		{
			get
			{
				return _windowBorderInactive;
			}
			set
			{
				_windowBorderInactive = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the standard foreground color for the window sizing decoration icons (maximize and minimize/restore).
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Color), "WindowText")]
		[LocalDescription(typeof(Resources), "PROP_THEME_WINICONHILIGHT_DESC")]
		public Color WindowSizeIconsForeColor
		{
			get
			{
				return _windowSizeIconsForeColor;
			}
			set
			{
				_windowSizeIconsForeColor = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the foreground color for the window sizing decoration icons (maximize and minimize/restore) when the mouse cursor is over the icon.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Color), "HighlightText")]
		[LocalDescription(typeof(Resources), "PROP_THEME_WINICONHILIGHT_DESC")]
		public Color WindowSizeIconsForeColorHilight
		{
			get
			{
				return _windowSizeIconsForeColorHilight;
			}
			set
			{
				_windowSizeIconsForeColorHilight = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the background color for the window sizing decoration icons (maximize and minimize/restore) when the mouse cursor is over the icon.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Color), "Highlight")]
		[LocalDescription(typeof(Resources), "PROP_THEME_WINICONBACKHILIGHT_DESC")]
		public Color WindowSizeIconsBackColorHilight
		{
			get
			{
				return _windowSizeIconsBackColorHilight;
			}
			set
			{
				_windowSizeIconsBackColorHilight = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the close button icon foreground color.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Color), "WindowText")]
		[LocalDescription(typeof(Resources), "PROP_THEME_WINCLOSEFORECOLOR_DESC")]
		public Color WindowCloseIconForeColor
		{
			get
			{
				return _windowCloseIconForeColor;
			}
			set
			{
				_windowCloseIconForeColor = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the close icon foreground color when the mouse cursor is over it.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Color), "HighlightText")]
		[LocalDescription(typeof(Resources), "PROP_THEME_WINCLOSEFORECOLORHILIGHT_DESC")]
		public Color WindowCloseIconForeColorHilight
		{
			get
			{
				return _windowCloseIconForeColorHilight;
			}
			set
			{
				_windowCloseIconForeColorHilight = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the close icon background color when the mouse cursor is over it.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Color), "Red")]
		[LocalDescription(typeof(Resources), "PROP_THEME_WINCLOSEBACKCOLORHILIGHT_DESC")]
		public Color WindowCloseIconBackColorHilight
		{
			get
			{
				return _windowCloseIconBackColorHilight;
			}
			set
			{
				_windowCloseIconBackColorHilight = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the image used for menu items that use a check mark.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Image), null)]
		[LocalDescription(typeof(Resources), "PROP_THEME_CHECKENABLED_IMAGE")]
		public Image MenuCheckEnabledImage
		{
			get
			{
				return _checkEnabledImage;
			}
			set
			{
				_checkEnabledImage = value;
				OnPropertyChanged();
			}
		}

		/// <summary>
		/// Property to set or return the image used for disabled menu items that use a check mark.
		/// </summary>
		[Browsable(true)]
		[DefaultValue(typeof(Image), null)]
		[LocalDescription(typeof(Resources), "PROP_THEME_CHECKDISABLED_IMAGE")]
		public Image MenuCheckDisabledImage
		{
			get
			{
				return _checkDisabledImage;
			}
			set
			{
				_checkDisabledImage = value;
				OnPropertyChanged();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function called when a color property is changed.
		/// </summary>
		protected void OnPropertyChanged()
		{
			PropertyChanged?.Invoke(this, EventArgs.Empty);
		}

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
				e.Graphics.DrawImage(_checkEnabledImage ?? Resources.Check_Enabled1, newRect);
			}
			else
			{
				e.Graphics.DrawImage(_checkDisabledImage ?? Resources.Check_Disabled1, newRect);
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

			Debug.Assert(item != null, "Drop down button is <b>null</b>!");

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

			Debug.Assert(item != null, "Drop down button is <b>null</b>!");

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

		/// <summary>
		/// Function to load this theme from a stream containing XML data.
		/// </summary>
		/// <param name="stream">Stream to load from.</param>
		/// <returns>The <see cref="GorgonFlatForm"/> theme deserialized from XML.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> parameter is <b>null</b> (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="ArgumentException">Thrown when the stream is write-only.</exception>
		/// <exception cref="EndOfStreamException">Thrown when the stream position cannot read beyond the stream length.</exception>
		public static GorgonFlatFormTheme Load(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			if (!stream.CanRead)
			{
				throw new ArgumentException(Resources.GOR_ERR_STREAM_IS_WRITEONLY, nameof(stream));
			}

			if (stream.Position >= stream.Length)
			{
				throw new EndOfStreamException(Resources.GOR_ERR_STREAM_EOS);
			}

			var serializer = new XmlSerializer(typeof(GorgonFlatFormTheme));
			return (GorgonFlatFormTheme)serializer.Deserialize(stream);
		}

		/// <summary>
		/// Function to save this theme as XML data into a stream.
		/// </summary>
		/// <param name="stream">Stream to fill with the data.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="stream"/> is <b>null</b> (<i>Nothing</i> in VB.Net)</exception>
		/// <exception cref="ArgumentException">Thrown when the stream is read-only.</exception>
		public void Save(Stream stream)
		{
			if (stream == null)
			{
				throw new ArgumentNullException(nameof(stream));
			}

			if (!stream.CanWrite)
			{
				throw new ArgumentException(Resources.GOR_ERR_STREAM_IS_READONLY, nameof(stream));
			}

			var serializer = new XmlSerializer(typeof(GorgonFlatFormTheme));
			serializer.Serialize(stream, this);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFlatFormTheme"/> class.
		/// </summary>
		public GorgonFlatFormTheme()
		{
			ForeColor = Color.FromKnownColor(KnownColor.WindowText);
			DisabledColor = Color.FromKnownColor(KnownColor.DimGray);
			ForeColorInactive = DisabledColor;
			HilightForeColor = Color.FromKnownColor(KnownColor.HighlightText);
			HilightBackColor = Color.FromKnownColor(KnownColor.Highlight);
			DropDownBorderColor = Color.FromKnownColor(KnownColor.ActiveBorder);
			CheckBoxBackColor = Color.FromKnownColor(KnownColor.Control);
			CheckBoxBackColorHilight = Color.FromKnownColor(KnownColor.Highlight);
			WindowBackground = Color.FromKnownColor(KnownColor.Control);
			ContentPanelBackground = Color.FromKnownColor(KnownColor.Window);
			WindowBorderActive = Color.FromKnownColor(KnownColor.ActiveBorder);
			WindowBorderInactive = Color.FromKnownColor(KnownColor.InactiveBorder);
			WindowSizeIconsForeColor = ForeColor;
			WindowSizeIconsForeColorHilight = HilightForeColor;
			WindowSizeIconsBackColorHilight = Color.FromKnownColor(KnownColor.Highlight);
			WindowCloseIconForeColor = WindowSizeIconsForeColor;
			WindowCloseIconForeColorHilight = WindowSizeIconsForeColorHilight;
			WindowCloseIconBackColorHilight = Color.Red;
			ToolStripBackColor = WindowBackground;
			ToolStripArrowColor = ForeColor;
		}
		#endregion

		#region IXmlSerializable Members
		/// <summary>
		/// This method is reserved and should not be used. When implementing the IXmlSerializable interface, you should return null (<i>Nothing</i> in Visual Basic) from this method, and instead, if specifying a custom schema is required, apply the <see cref="T:System.Xml.Serialization.XmlSchemaProviderAttribute" /> to the class.
		/// </summary>
		/// <returns>
		/// An <see cref="T:System.Xml.Schema.XmlSchema" /> that describes the XML representation of the object that is produced by the <see cref="M:System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter)" /> method and consumed by the <see cref="M:System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader)" /> method.
		/// </returns>
		XmlSchema IXmlSerializable.GetSchema()
		{
			return null;
		}

		/// <summary>
		/// Generates an object from its XML representation.
		/// </summary>
		/// <param name="reader">The <see cref="T:System.Xml.XmlReader" /> stream from which the object is deserialized.</param>
		void IXmlSerializable.ReadXml(XmlReader reader)
		{
			if (reader.MoveToContent() != XmlNodeType.Element)
			{
				throw new IOException(Resources.GOR_ERR_FLATFORM_CANNOT_READ_THEME);
			}

			if (!string.Equals(reader.LocalName, GetType().Name))
			{
				throw new IOException(Resources.GOR_ERR_FLATFORM_CANNOT_READ_THEME);
			}

			// Get our properties so we can set them.
			IDictionary<string, PropertyInfo> properties = (from property in GetType().GetProperties()
			                                                where property.PropertyType == typeof(Color)
			                                                      && property.GetCustomAttribute<BrowsableAttribute>() != null
			                                                select property).ToDictionary(key => key.Name, value => value, StringComparer.OrdinalIgnoreCase);

			if (!reader.Read())
			{
				return;
			}

			var converter = new ColorConverter();

			while (reader.MoveToContent() == XmlNodeType.Element)
			{
				PropertyInfo property;

				if (!properties.TryGetValue(reader.LocalName, out property))
				{
					reader.Read();
					continue;
				}

				property.SetValue(this, converter.ConvertFromInvariantString(reader.GetAttribute("Color")));

				reader.Read();
			}
		}

		/// <summary>
		/// Converts an object into its XML representation.
		/// </summary>
		/// <param name="writer">The <see cref="T:System.Xml.XmlWriter" /> stream to which the object is serialized.</param>
		void IXmlSerializable.WriteXml(XmlWriter writer)
		{
			var converter = new ColorConverter();
			IEnumerable<PropertyInfo> colorProperties = from property in GetType().GetProperties()
			                                            where property.PropertyType == typeof(Color)
			                                                  && property.GetCustomAttribute<BrowsableAttribute>() != null
			                                            select property;

			foreach (PropertyInfo property in colorProperties)
			{
				var currentColor = (Color)property.GetValue(this);

				writer.WriteStartElement(property.Name);
				// ReSharper disable PossibleNullReferenceException
				// ReSharper disable once AssignNullToNotNullAttribute
				writer.WriteAttributeString("Color", converter.ConvertToInvariantString(currentColor));
				// ReSharper restore PossibleNullReferenceException
				writer.WriteEndElement();
			}
		}
		#endregion
	}
}
