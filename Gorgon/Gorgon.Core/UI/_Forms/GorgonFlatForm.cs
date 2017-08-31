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
// Created: Sunday, March 3, 2013 9:50:51 PM
//
#endregion

using System;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using Gorgon.Core.Properties;
using Gorgon.Design;
using Gorgon.Math;
using Gorgon.Native;
using Gorgon.UI.Design;
using DrawingGraphics = System.Drawing.Graphics;

namespace Gorgon.UI
{
	/// <summary>
	/// A form that provides a flattened interface look similar to Windows 8/Windows X.
	/// </summary>
	/// <remarks>
	/// <para>
	/// This is an overload to the standard windows form that mimics the flat style of Windows 8 windows on operating systems that don't use flat style forms natively.
	/// </para>
	/// <para>
    /// To use the form, merely create a new Windows <see cref="Form"/> in Visual Studio and then change the inheritance from <c>System.Windows.Forms.Form</c> to <c>Gorgon.UI.GorgonFlatForm</c>. 
    /// Once this is done the form will change to the flat style.
	/// </para>
	/// <para>
	/// This form uses the client area of the window to handle the caption, and system buttons (close, minimize, etc...), so it is possible to add new buttons, images, etc.. into 
	/// the caption of the window for a truly customized look.
	/// </para>
	/// <para>
	/// To add controls to the form, just add your controls to the <see cref="ContentArea"/> panel on the form.
	/// </para>
	/// <para>
	/// Users may also customize the colors of the flat form by adding a <see cref="GorgonFlatFormTheme"/> to the <see cref="Theme"/> property.
	/// </para>
	/// </remarks>
	public partial class GorgonFlatForm
		: Form
	{
		#region Enums.
		/// <summary>
		/// Resize directions.
		/// </summary>
		private enum ResizeDirection
		{
			/// <summary>
			/// None.
			/// </summary>
			None = 0,
			/// <summary>
			/// Left.
			/// </summary>
			Left = 1,
			/// <summary>
			/// Top left corner.
			/// </summary>
			TopLeft = 2,
			/// <summary>
			/// Top.
			/// </summary>
			Top = 4,
			/// <summary>
			/// Top right corner.
			/// </summary>
			TopRight = 8,
			/// <summary>
			/// Right.
			/// </summary>
			Right = 16,
			/// <summary>
			/// Bottom right corner.
			/// </summary>
			BottomRight = 32,
			/// <summary>
			/// Bottom.
			/// </summary>
			Bottom = 64,
			/// <summary>
			/// Bottom left corner.
			/// </summary>
			BottomLeft = 128
		}

		/// <summary>
		/// Types of messages that passed to a window.
		/// </summary>
		/// <remarks>See the MSDN documentation for more detail.</remarks>
		private enum WindowMessages
		{
			SysCommand = 0x0112,
			// ReSharper disable once InconsistentNaming
			NCLeftButtonDown = 0x00A1,
			EnterSizeMove = 0x0231,
			ExitSizeMove = 0x0232
		}

		/// <summary>
		/// Hit tests for non-client areas.
		/// </summary>
		private enum HitTests
		{
			/// <summary>
			/// Bottom border
			/// </summary>
			Bottom = 15,
			/// <summary>
			/// Bottom left corner.
			/// </summary>
			BottomLeft = 16,
			/// <summary>
			/// Bottom right corner.
			/// </summary>
			BottomRight = 17,
			/// <summary>
			/// Caption area.
			/// </summary>
			Caption = 2,
			/// <summary>
			/// Left border.
			/// </summary>
			Left = 10,
			/// <summary>
			/// Right border.
			/// </summary>
			Right = 11,
			/// <summary>
			/// Top border.
			/// </summary>
			Top = 12,
			/// <summary>
			/// Top left corner.
			/// </summary>
			TopLeft = 13,
			/// <summary>
			/// Top right corner.
			/// </summary>
			TopRight = 14
		}
		#endregion

		#region Variables.
		private Padding? _currentPadding;
	    private ResizeDirection _resizeDirection = ResizeDirection.None;
		private Image _iconImage;
		private bool _showWindowCaption = true;
		private int _borderWidth = 1;
		private bool _border;
		private FormWindowState _windowState = FormWindowState.Normal;
		private FormWindowState _prevMinState = FormWindowState.Normal;
		private GorgonFlatFormTheme _theme = new GorgonFlatFormTheme();
		private int _captionHeight;
		[AccessedThroughProperty("ContentArea")]
		private Panel _panelContent;
		private PropertyDescriptor _themeProperty;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the resize direction.
		/// </summary>
		private ResizeDirection ResizeDir
		{
			get => _resizeDirection;
			set
			{
				_resizeDirection = value;

				switch (_resizeDirection)
				{
					case ResizeDirection.Bottom:
					case ResizeDirection.Top:
						Cursor.Current = Cursors.SizeNS;
						break;
					case ResizeDirection.Right:
					case ResizeDirection.Left:
						Cursor.Current = Cursors.SizeWE;
						break;
					case ResizeDirection.BottomRight:
					case ResizeDirection.TopLeft:
						Cursor.Current = Cursors.SizeNWSE;
						break;
					case ResizeDirection.TopRight:
					case ResizeDirection.BottomLeft:
						Cursor.Current = Cursors.SizeNESW;
						break;
					default:
						Cursor.Current = Cursors.Default;
						break;						
				}
			}
		}

		/// <summary>
		/// Property to set or return the background color for the <see cref="GorgonFlatForm"/>.
		/// </summary>
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Color BackColor
		{
			get => _theme.WindowBackground;
			set
			{
				_theme.WindowBackground = value;
				Refresh();
			}
		}

		/// <summary>
		/// Property to set or return the foreground color of the <see cref="GorgonFlatForm"/>.
		/// </summary>
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Color ForeColor
		{
			get => _theme.ForeColor;
			set
			{
				_theme.ForeColor = value;
				Refresh();
			}
		}

		/// <summary>
		/// Property to return the <see cref="Panel"/> that contains the controls in the form client area.
		/// </summary>
		/// <remarks>
		/// When designing a <see cref="GorgonFlatForm"/>, this <see cref="Panel"/> is where controls that occupy the client area will live. This keeps a separation between the non-client area (i.e. 
		/// the caption, icons, etc...) and the controls the user is meant to interact with.
		/// </remarks>
		[Browsable(false)]
		public Panel ContentArea => _panelContent;

		/// <summary>
		/// Property to set or return the current <see cref="GorgonFlatFormTheme"/> for the window.
		/// </summary>
		/// <remarks>
		/// Setting a theme will set all the colors on the <see cref="GorgonFlatForm"/> at once. All toolbars and menus will automatically be updated to the new theme as well.
		/// </remarks>
		[Browsable(true), 
		LocalCategory(typeof(Resources), "PROP_CATEGORY_APPEARANCE"), 
		TypeConverter(typeof(GorgonFlatFormThemeConverter)),
		DesignerSerializationVisibility(DesignerSerializationVisibility.Content),
		RefreshProperties(RefreshProperties.All)]
		public GorgonFlatFormTheme Theme
		{
			get => _theme;
			set
			{
				if (value == null)
				{
					value = new GorgonFlatFormTheme();
				}
				
				if (_theme != null)
				{
					_theme.PropertyChanged -= Theme_PropertyChangedEvent;
				}

				_theme = value;

				ToolStripManager.Renderer = _theme;

				// Fire the event to signal that the theme has changed.
				Theme_PropertyChangedEvent(this, EventArgs.Empty);

				_theme.PropertyChanged += Theme_PropertyChangedEvent;
			}
		}

		/// <summary>
		/// Property to set or return the border style of the form.
		/// </summary>
		/// <remarks>
		/// This property is disabled since the flat form uses its own border drawing. Setting this value will do nothing.
		/// </remarks>
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new FormBorderStyle FormBorderStyle
		{
			get => FormBorderStyle.None;
			// ReSharper disable once ValueParameterNotUsed
			set => base.FormBorderStyle = FormBorderStyle.None;
		}

		/// <summary>
		/// Property to set or return whether a single pixel width border will be drawn on the form.
		/// </summary>
		[Browsable(true), LocalCategory(typeof(Resources), "PROP_CATEGORY_APPEARANCE"), LocalDescription(typeof(Resources), "PROP_BORDER_DESC"), DefaultValue(false), 
		RefreshProperties(RefreshProperties.All)]
		public bool ShowBorder
		{
			get => _border;
			set
			{
				_border = value;

				Refresh();
			}
		}

		/// <summary>
		/// Property to set or return the size of the border, in pixels.
		/// </summary>
		/// <remarks>
		/// This is only valid when the <see cref="Resizable"/> property is set to <b>true</b>.
		/// </remarks>
		[Browsable(true), LocalDescription(typeof(Resources), "PROP_BORDERSIZE_DESC"), LocalCategory(typeof(Resources), "PROP_CATEGORY_APPEARANCE"),
		RefreshProperties(RefreshProperties.All), DefaultValue(1)]
		public int BorderSize
		{
			get => _borderWidth;
			set
			{
				if (value < 0)
				{
					value = 0;
				}

				_borderWidth = value;
				Refresh();
			}
		}


		/// <summary>
		/// Property to set or return whether the form can be resized by a user or not.
		/// </summary>
		[Browsable(true), LocalCategory(typeof(Resources), "PROP_CATEGORY_BEHAVIOR"), LocalDescription(typeof(Resources), "PROP_RESIZABLE_DESC"), DefaultValue(true)]
		public bool Resizable
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the size of the resize handle border, in pixels.
		/// </summary>
		/// <remarks>
		/// This is only valid when the <see cref="Resizable"/> property is set to <b>true</b>.
		/// </remarks>
		[Browsable(true), LocalDescription(typeof(Resources), "PROP_RESIZEHANDLE_DESC"), LocalCategory(typeof(Resources), "PROP_CATEGORY_DESIGN"),
		RefreshProperties(RefreshProperties.All), DefaultValue(6)]
		public int ResizeHandleSize
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the control box on the form is visible or not.
		/// </summary>
		/// <remarks>
		/// This property is disabled for the <see cref="GorgonFlatForm"/>, setting it will do nothing and it will always return <b>true</b>.
		/// </remarks>
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new bool ControlBox
		{
			get
			{
				return true;
			}
			// ReSharper disable once ValueParameterNotUsed
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return whether the help icon on the form is visible or not.
		/// </summary>
		/// <remarks>
		/// This property is disabled for the <see cref="GorgonFlatForm"/>, setting it will do nothing and it will always return <b>false</b>.
		/// </remarks>
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new bool HelpButton
		{
			get
			{
				return false;
			}
			// ReSharper disable once ValueParameterNotUsed
			set
			{
			}
		}

		/// <summary>
		/// Property to set or return whether to show the icon for the <see cref="GorgonFlatForm"/>.
		/// </summary>
		/// <remarks>
		/// This is the window icon on the upper-left corner where the system menu for the window is displayed.
		/// </remarks>
		[Browsable(true), LocalCategory(typeof(Resources), "PROP_CATEGORY_DESIGN"), LocalDescription(typeof(Resources), "PROP_SHOWICON_DESC")]
		public new bool ShowIcon
		{
			get => base.ShowIcon;
			set
			{
				base.ShowIcon = value;
				pictureIcon.Visible = base.ShowIcon;
			}
		}

		/// <summary>
		/// Property to set or return whether the window caption, system menu, max/min buttons and the close button are visible.
		/// </summary>
		[Browsable(true), LocalCategory(typeof(Resources), "PROP_CATEGORY_WINDOWSTYLE"), LocalDescription(typeof(Resources), "PROP_WINDOWCAPTION_DESC")]
		public bool ShowWindowCaption
		{
			get => _showWindowCaption;
			set
			{
				_showWindowCaption = value;
				ValidateWindowControls();
			}
		}

		/// <summary>
		/// Property to set or return the caption for the <see cref="GorgonFlatForm"/>.
		/// </summary>
		[Browsable(true), LocalCategory(typeof(Resources), "PROP_CATEGORY_APPEARANCE"), LocalDescription(typeof(Resources), "PROP_TEXT_DESC")]
		public override string Text
		{
			get => base.Text;
			set
			{
				base.Text = value;
				labelCaption.Text = value;
				ValidateWindowControls();
			}
		}

		/// <summary>
		/// Property to set or return the icon for this form.
		/// </summary>
		[Browsable(true), LocalCategory(typeof(Resources), "PROP_CATEGORY_APPEARANCE"), LocalDescription(typeof(Resources), "PROP_ICON_DESC")]
		public new Icon Icon
		{
			get => base.Icon;
			set
			{
				base.Icon = value;
				ExtractIcon();					
				ValidateWindowControls();
			}
		}

		/// <summary>
		/// Property to set or return the sizing grip style.
		/// </summary>
		/// <remarks>
		/// This property is disabled for the <see cref="GorgonFlatForm"/>, setting it will do nothing and it will always return <see cref="System.Windows.Forms.SizeGripStyle.Hide"/>.
		/// </remarks>
		[Browsable(false)]
		public new SizeGripStyle SizeGripStyle
		{
			get
			{
				return SizeGripStyle.Hide;
			}
			// ReSharper disable once ValueParameterNotUsed
			set
			{
				// Do nothing.
			}
		}

		/// <summary>
		/// Gets the location and size of the form in its normal window state.
		/// </summary>
		/// <returns>A <see cref="T:System.Drawing.Rectangle" /> that contains the location and size of the form in the normal window state.</returns>
		[Browsable(false)]
		public new Rectangle RestoreBounds
		{
			get;
			private set;
		}

		/// <summary>
		/// Gets or sets a value that indicates whether form is minimized, maximized, or normal.
		/// </summary>
		/// <returns>A <see cref="T:System.Windows.Forms.FormWindowState" /> that represents whether form is minimized, maximized, or normal. The default is <see cref="System.Windows.Forms.FormWindowState.Normal"/>.</returns>
		public new FormWindowState WindowState
		{
			get => _windowState;
			set
			{
				if (_windowState == value)
				{
					return;
				}

				_prevMinState = _windowState;
				_windowState = value;

				if ((IsHandleCreated) && (value == FormWindowState.Maximized))
				{
					RestoreBounds = Bounds;
				}

				if ((!IsHandleCreated) || (DesignMode))
				{
					return;
				}

				if ((value == FormWindowState.Normal) && (_prevMinState == FormWindowState.Maximized))
				{
					SetBounds(RestoreBounds.Left, RestoreBounds.Top, RestoreBounds.Width, RestoreBounds.Height, BoundsSpecified.All);
				}

				if (value != FormWindowState.Maximized)
				{
					base.WindowState = value;
					return;
				}

				// Security issue.
				if ((TopLevel) && (IsRestrictedWindow))
				{
					_prevMinState = _windowState = base.WindowState = FormWindowState.Normal;
					return;
				}

				Screen currentScreen = Screen.FromControl(this);

				SetBounds(currentScreen.Bounds.Left,
				          currentScreen.Bounds.Top,
				          currentScreen.WorkingArea.Width,
				          currentScreen.WorkingArea.Height,
						  BoundsSpecified.All);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function called when the Save Theme menu item is clicked.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event parameters.</param>
		private void OnSaveTheme(object sender, EventArgs e)
		{
			SaveFileDialog dialog = null;
			Stream stream = null;

			try
			{
				dialog = new SaveFileDialog
				{
					Title = Resources.GOR_TEXT_SAVE_THEME,
					DefaultExt = "xml",
					Filter = Resources.GOR_TEXT_XML_FILTER
				};

				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				stream = dialog.OpenFile();
				GorgonFlatFormTheme currentTheme = (GorgonFlatFormTheme)_themeProperty.GetValue(this);

				if (currentTheme == null)
				{
					return;
				}

				currentTheme.Save(stream);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				stream?.Dispose();
				dialog?.Dispose();
			}
		}

		/// <summary>
		/// Function called when the Load Theme menu item is clicked.
		/// </summary>
		/// <param name="sender">Sender of the event.</param>
		/// <param name="e">Event parameters.</param>
		private void OnLoadTheme(object sender, EventArgs e)
		{
			OpenFileDialog dialog = null;
			Stream stream = null;

			try
			{
				dialog = new OpenFileDialog
				{
					Title = Resources.GOR_TEXT_LOAD_THEME,
					DefaultExt = "xml",
					Filter = Resources.GOR_TEXT_XML_FILTER
				};

				if (dialog.ShowDialog(this) != DialogResult.OK)
				{
					return;
				}

				GorgonFlatFormTheme currentTheme = (GorgonFlatFormTheme)_themeProperty.GetValue(this);

				if (currentTheme == null)
				{
					return;
				}

				// Back up the selected images since our theme file does not contain image data.
				Image checkEnabled = currentTheme.MenuCheckEnabledImage;
				Image checkDisabled = currentTheme.MenuCheckDisabledImage;

				stream = dialog.OpenFile();
				GorgonFlatFormTheme newTheme = GorgonFlatFormTheme.Load(stream);

				// Restore the previous images.
				newTheme.MenuCheckEnabledImage = checkEnabled;
				newTheme.MenuCheckDisabledImage = checkDisabled;

				_themeProperty.SetValue(this, newTheme);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
			}
			finally
			{
				stream?.Dispose();
				dialog?.Dispose();
			}
		}

		/// <summary>
		/// Function to build the designer verbs for load/saving themes.
		/// </summary>
		private void BuildDesignerVerbs()
		{
			if ((!DesignMode)
				|| (Site == null))
			{
				return;
			}

			_themeProperty = TypeDescriptor.GetProperties(typeof(GorgonFlatForm))
			                               .Cast<PropertyDescriptor>()
			                               .First(item => string.Equals(item.Name, "Theme", StringComparison.OrdinalIgnoreCase));

			IDesignerHost host = (IDesignerHost)Site.GetService(typeof(IDesignerHost));
			IMenuCommandService menuDesigner = (IMenuCommandService)host.GetService(typeof(IMenuCommandService));

			DesignerVerb loadVerb = new DesignerVerb(Resources.VERB_LOAD_THEME, OnLoadTheme);
			DesignerVerb saveVerb = new DesignerVerb(Resources.VERB_SAVE_THEME, OnSaveTheme);

			loadVerb.Properties.Add("Theme", _themeProperty);
			saveVerb.Properties.Add("Theme", _themeProperty);

			menuDesigner.AddVerb(loadVerb);
			menuDesigner.AddVerb(saveVerb);
		}

		/// <summary>
		/// Handles the TextChanged event of the labelCaption control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void labelCaption_TextChanged(object sender, EventArgs e)
		{
			if ((IsDisposed)
			    || (!IsHandleCreated))
			{
				return;
			}

			using (DrawingGraphics g = CreateGraphics())
			{
				_captionHeight = TextRenderer.MeasureText(g, labelCaption.Text, Font).Height + 8;
			}
		}

		/// <summary>
		/// Handles the MouseDown event of the panelWinIcons control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelWinIcons_MouseDown(object sender, MouseEventArgs e)
		{
			e = TransformMouseArgs(panelWinIcons, e);
			OnMouseDown(e);
		}

		/// <summary>
		/// Handles the MouseMove event of the panelWinIcons control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelWinIcons_MouseMove(object sender, MouseEventArgs e)
		{
			e = TransformMouseArgs(panelWinIcons, e);
			OnMouseMove(e);
		}

		/// <summary>
		/// Handles the MouseDown event of the _panelContent control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void _panelContent_MouseDown(object sender, MouseEventArgs e)
		{
			e = TransformMouseArgs(_panelContent, e);
			OnMouseDown(e);
		}

		/// <summary>
		/// Handles the MouseMove event of the _panelContent control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void _panelContent_MouseMove(object sender, MouseEventArgs e)
		{
			e = TransformMouseArgs(_panelContent, e);
			OnMouseMove(e);
		}

		/// <summary>
		/// Handles the DoubleClick event of the panelCaptionArea control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void panelCaptionArea_DoubleClick(object sender, EventArgs e)
		{
			if (!MaximizeBox)
			{
				return;
			}

			WindowState = WindowState == FormWindowState.Maximized ? FormWindowState.Normal : FormWindowState.Maximized;
		}

		/// <summary>
		/// Handles the Click event of the itemMove control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemMove_Click(object sender, EventArgs e)
		{
			UserApi.ReleaseCapture();										// SC_MOVE
			UserApi.SendMessage(Handle, (int)WindowMessages.SysCommand, new IntPtr(0xF010), IntPtr.Zero);
		}

		/// <summary>
		/// Handles the Click event of the itemSize control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemSize_Click(object sender, EventArgs e)
		{
			UserApi.ReleaseCapture();										// SC_SIZE
			UserApi.SendMessage(Handle, (int)WindowMessages.SysCommand, new IntPtr(0xF000), IntPtr.Zero);
		}

		/// <summary>
		/// Handles the Click event of the itemMinimize control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemMinimize_Click(object sender, EventArgs e)
		{
			WindowState = FormWindowState.Minimized;
		}

		/// <summary>
		/// Handles the Click event of the itemRestore control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemRestore_Click(object sender, EventArgs e)
		{
			WindowState = FormWindowState.Normal;
		}

		/// <summary>
		/// Handles the Click event of the itemMaximize control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemMaximize_Click(object sender, EventArgs e)
		{
			WindowState = FormWindowState.Maximized;
		}

		/// <summary>
		/// Handles the Click event of the itemClose control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemClose_Click(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Function to show the system menu.
		/// </summary>
		private void ShowSysMenu()
		{
			if (!ShowWindowCaption)
			{
				return;
			}

			popupSysMenu.Show(PointToScreen(new Point(panelCaptionArea.Left, panelCaptionArea.Height + panelCaptionArea.Top)));
		}

		/// <summary>
		/// Handles the MouseDown event of the pictureIcon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void pictureIcon_MouseDown(object sender, MouseEventArgs e)
		{
			if ((e.Clicks > 1) && (e.Button == MouseButtons.Left))
			{
				Close();
				return;
			}

			ValidateWindowControls();
			ShowSysMenu();
		}

		/// <summary>
        /// Handles the MouseEnter event of the labelClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void labelClose_MouseEnter(object sender, EventArgs e)
        {
            if (!(sender is Label label))
			{
				return;
			}

			label.ForeColor = _theme.WindowCloseIconForeColorHilight;
			label.BackColor = _theme.WindowCloseIconBackColorHilight;
        }

        /// <summary>
        /// Handles the MouseLeave event of the labelClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void labelClose_MouseLeave(object sender, EventArgs e)
        {
            if (!(sender is Label label))
	        {
		        return;
	        }

	        label.ForeColor = (ActiveForm == this) ? _theme.WindowCloseIconForeColor : _theme.ForeColorInactive;
	        label.BackColor = _theme.WindowBackground;
        }

		/// <summary>
		/// Handles the MouseEnter event of the labelMinimize control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void labelMinimize_MouseEnter(object sender, EventArgs e)
		{
		    if (!(sender is Label label))
			{
				return;
			}

			label.ForeColor = _theme.WindowSizeIconsForeColorHilight;
			label.BackColor = _theme.WindowSizeIconsBackColorHilight;
		}

		/// <summary>
		/// Handles the MouseLeave event of the labelMinimize control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void labelMinimize_MouseLeave(object sender, EventArgs e)
		{
		    if (!(sender is Label label))
			{
				return;
			}

			label.ForeColor = (ActiveForm == this) ? _theme.WindowSizeIconsForeColor : _theme.ForeColorInactive;
			label.BackColor = _theme.WindowBackground;
		}

        /// <summary>
        /// Handles the MouseDown event of the labelClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void labelClose_MouseDown(object sender, MouseEventArgs e)
        {
            Close();
        }

        /// <summary>
        /// Handles the Click event of the labelMinimize control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void labelMinimize_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
            ValidateWindowControls();
        }

        /// <summary>
        /// Handles the Click event of the labelMaxRestore control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void labelMaxRestore_Click(object sender, EventArgs e)
        {
	        WindowState = WindowState != FormWindowState.Maximized ? FormWindowState.Maximized : FormWindowState.Normal;

	        ValidateWindowControls();
        }

		/// <summary>
		/// Function to transform mouse event arguments into the client space of the form.
		/// </summary>
		/// <param name="control">Control with the local mouse coordinates.</param>
		/// <param name="e">Event arguments to transform.</param>
		/// <returns>The transformed event arguments.</returns>
		private MouseEventArgs TransformMouseArgs(Control control, MouseEventArgs e)
		{
			Point newCoordinates = PointToClient(control.PointToScreen(e.Location));
			return new MouseEventArgs(e.Button, e.Clicks, newCoordinates.X, newCoordinates.Y, e.Delta);
		}

		/// <summary>
        /// Handles the MouseDown event of the labelCaption control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void labelCaption_MouseDown(object sender, MouseEventArgs e)
		{
			e = TransformMouseArgs(labelCaption, e);
            OnMouseDown(e);
        }

        /// <summary>
        /// Handles the MouseMove event of the labelCaption control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void labelCaption_MouseMove(object sender, MouseEventArgs e)
        {
			e = TransformMouseArgs(labelCaption, e);
            OnMouseMove(e);
        }

		/// <summary>
		/// Function to perform layout of the window controls.
		/// </summary>
		private void LayoutWindowControls()
		{
			if ((!ShowWindowCaption)
				|| (!IsHandleCreated))
			{
				return;
			}

			int maxSizeForContainer = (panelWinIcons.Height.Max(_iconImage?.Height + 8 ?? 8).Max(_captionHeight));

			panelCaptionArea.Height = maxSizeForContainer;

			int iconsVertOffset = (maxSizeForContainer / 2) - (panelWinIcons.Height / 2);
			int iconOffset = (maxSizeForContainer / 2) - (pictureIcon.Height / 2);

			panelWinIcons.Top = iconsVertOffset;
			pictureIcon.Top = iconOffset;
		}
        
        /// <summary>
		/// Function to validate all the window controls.
		/// </summary>
		private void ValidateWindowControls()
		{
			if (ShowWindowCaption)
			{
				panelCaptionArea.Visible = true;

				if ((MaximizeBox) || (MinimizeBox))
				{
					labelMaxRestore.Visible = labelMinimize.Visible = true;
					labelMaxRestore.Enabled = MaximizeBox;
					labelMinimize.Enabled = MinimizeBox;
				}
				else
				{					
					labelMaxRestore.Visible = labelMinimize.Visible = false;
					itemRestore.Visible = false;
				}

				itemMaximize.Visible = MaximizeBox;
				itemMinimize.Visible = MinimizeBox;
			}
			else
			{
				panelCaptionArea.Visible = false;
			}

			itemSize.Visible = Resizable;

			switch (WindowState)
			{
				case FormWindowState.Maximized:
					itemRestore.Enabled = true;
					itemMaximize.Enabled = false;
					itemMinimize.Enabled = true;
					itemMove.Enabled = false;
					itemSize.Enabled = false;
					labelMaxRestore.Text = Resources.GOR_TEXT_FLATFORM_MAX_ICON;
					toolTip.SetToolTip(labelMaxRestore, Resources.GOR_TEXT_FLATFORM_MAXIMIZE_TIP);
					break;
				case FormWindowState.Minimized:
					itemRestore.Enabled = true;
					itemMove.Enabled = false;
					itemSize.Enabled = false;
					itemMaximize.Enabled = true;
					itemMinimize.Enabled = false;
					break;
				case FormWindowState.Normal:
					labelMaxRestore.Text = Resources.GOR_TEXT_FLATFORM_RESTORE_ICON;
					toolTip.SetToolTip(labelMaxRestore, Resources.GOR_TEXT_FLATFORM_RESTORE_TIP);
					itemMove.Enabled = true;
					itemSize.Enabled = true;
					itemRestore.Enabled = false;
					itemMaximize.Enabled = true;
					itemMinimize.Enabled = true;
					break;
			}

	        LayoutWindowControls();
		}

		/// <summary>
		/// Handles the MouseMove event of the <see cref="GorgonFlatForm"/> control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void Form_MouseMove(object sender, MouseEventArgs e)
		{
			try
			{
				if ((DesignMode) || (WindowState != FormWindowState.Normal) || (!Resizable))
				{
					return;
				}

				if ((e.Location.X < ResizeHandleSize * 2) && (e.Location.Y < ResizeHandleSize * 2))
				{
					ResizeDir = ResizeDirection.TopLeft;
				}
				else if ((e.Location.X < ResizeHandleSize * 2) && (e.Location.Y > Height - ResizeHandleSize * 2))
				{
					ResizeDir = ResizeDirection.BottomLeft;
				}
				else if ((e.Location.X > Width - ResizeHandleSize * 2) && (e.Location.Y > Height - ResizeHandleSize * 2))
				{
					ResizeDir = ResizeDirection.BottomRight;
				}
				else if ((e.Location.X > Width - ResizeHandleSize * 2) && (e.Location.Y < ResizeHandleSize * 2))
				{
					ResizeDir = ResizeDirection.TopRight;
				}
				else if ((e.Location.X < ResizeHandleSize))
				{
					ResizeDir = ResizeDirection.Left;
				}
				else if ((e.Location.X > Width - ResizeHandleSize))
				{
					ResizeDir = ResizeDirection.Right;
				}
				else if ((e.Location.Y < ResizeHandleSize))
				{
					ResizeDir = ResizeDirection.Top;
				}
				else if ((e.Location.Y > Height - ResizeHandleSize))
				{
					ResizeDir = ResizeDirection.Bottom;
				}
				else
				{
					ResizeDir = ResizeDirection.None;
				}
			}
			finally
			{
				ValidateWindowControls();
			}
		}

		/// <summary>
		/// Handles the Activated event of the <see cref="GorgonFlatForm"/> control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Form_Activated(object sender, EventArgs e)
		{
			if (base.WindowState == FormWindowState.Minimized)
			{
				_windowState = _prevMinState;
			}

			Theme_PropertyChangedEvent(this, EventArgs.Empty);

			using (DrawingGraphics graphics = CreateGraphics())
			{
				DrawBorder(graphics);
			}
		}

		/// <summary>
		/// Handles the Deactivate event of the <see cref="GorgonFlatForm"/> control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Form_Deactivate(object sender, EventArgs e)
		{
			Theme_PropertyChangedEvent(this, EventArgs.Empty);

			using (DrawingGraphics graphics = CreateGraphics())
			{
				DrawBorder(graphics);
			}
		}

		/// <summary>
		/// Handles the MouseDown event of the <see cref="GorgonFlatForm"/> control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void Form_MouseDown(object sender, MouseEventArgs e)
		{
			if (DesignMode)
			{
				return;
			}

			try
			{
				if ((e.Button == MouseButtons.Left) && (e.Clicks > 1))
				{
					if (panelCaptionArea.ClientRectangle.Contains(panelCaptionArea.PointToClient(e.Location)))
					{
						panelCaptionArea_DoubleClick(this, EventArgs.Empty);
					}
					return;
				}

				if (e.Button != MouseButtons.Left)
				{
					return;
				}

				if ((Width - ResizeHandleSize > e.X) && (e.X > ResizeHandleSize) && (e.Y > ResizeHandleSize) && (e.Y < Height - ResizeHandleSize) && (WindowState == FormWindowState.Normal))
				{
					UserApi.ReleaseCapture();
					UserApi.SendMessage(Handle, (uint)WindowMessages.NCLeftButtonDown, new IntPtr((int)HitTests.Caption), IntPtr.Zero);
				}
				else
				{
					if ((WindowState != FormWindowState.Normal) || (!Resizable))
					{
						return;
					}

					switch (ResizeDir)
					{
						case ResizeDirection.Left:
							UserApi.ReleaseCapture();
							Cursor.Current = Cursors.SizeWE;
							UserApi.SendMessage(Handle, (uint)WindowMessages.NCLeftButtonDown, new IntPtr((int)HitTests.Left), IntPtr.Zero);
							break;
						case ResizeDirection.TopLeft:
							UserApi.ReleaseCapture();
							Cursor.Current = Cursors.SizeNWSE;
							UserApi.SendMessage(Handle, (uint)WindowMessages.NCLeftButtonDown, new IntPtr((int)HitTests.TopLeft), IntPtr.Zero);
							break;
						case ResizeDirection.Top:
							UserApi.ReleaseCapture();
							Cursor.Current = Cursors.SizeNS;
							UserApi.SendMessage(Handle, (uint)WindowMessages.NCLeftButtonDown, new IntPtr((int)HitTests.Top), IntPtr.Zero);
							break;
						case ResizeDirection.TopRight:
							UserApi.ReleaseCapture();
							Cursor.Current = Cursors.SizeNESW;
							UserApi.SendMessage(Handle, (uint)WindowMessages.NCLeftButtonDown, new IntPtr((int)HitTests.TopRight), IntPtr.Zero);
							break;
						case ResizeDirection.Right:
							UserApi.ReleaseCapture();
							Cursor.Current = Cursors.SizeWE;
							UserApi.SendMessage(Handle, (uint)WindowMessages.NCLeftButtonDown, new IntPtr((int)HitTests.Right), IntPtr.Zero);
							break;
						case ResizeDirection.BottomRight:
							UserApi.ReleaseCapture();
							Cursor.Current = Cursors.SizeNWSE;
							UserApi.SendMessage(Handle, (uint)WindowMessages.NCLeftButtonDown, new IntPtr((int)HitTests.BottomRight), IntPtr.Zero);
							break;
						case ResizeDirection.Bottom:
							UserApi.ReleaseCapture();
							Cursor.Current = Cursors.SizeNS;
							UserApi.SendMessage(Handle, (uint)WindowMessages.NCLeftButtonDown, new IntPtr((int)HitTests.Bottom), IntPtr.Zero);
							break;
						case ResizeDirection.BottomLeft:
							UserApi.ReleaseCapture();
							Cursor.Current = Cursors.SizeNESW;
							UserApi.SendMessage(Handle, (uint)WindowMessages.NCLeftButtonDown, new IntPtr((int)HitTests.BottomLeft), IntPtr.Zero);
							break;
					}
				}
			}
			finally
			{
				ValidateWindowControls();
			}
		}

		/// <summary>
		/// Handles the Load event of the <see cref="GorgonFlatForm"/> control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Form_Load(object sender, EventArgs e)
		{
			try
			{
				BuildDesignerVerbs();

				ToolStripManager.Renderer = _theme;

				labelCaption_TextChanged(this, EventArgs.Empty);
				ValidateWindowControls();

				Theme_PropertyChangedEvent(this, EventArgs.Empty);

				ApplyTheme();

				_theme.PropertyChanged += Theme_PropertyChangedEvent;
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(this, ex);
				Close();
			}
		}

		/// <summary>
		/// Function to extract the icon image from the embedded image.
		/// </summary>
		private void ExtractIcon()
		{
			if (_iconImage != null)
			{
				pictureIcon.Image = null;
				_iconImage.Dispose();
				_iconImage = null;
			}

			if ((!ShowWindowCaption) || (Icon == null))
			{
				return;
			}

			_iconImage = new Bitmap(24, 24, PixelFormat.Format32bppArgb);
			using (DrawingGraphics g = DrawingGraphics.FromImage(_iconImage))
			{
				g.DrawIcon(Icon, new Rectangle(0, 0, 24, 24));
			}

			pictureIcon.Image = _iconImage;
		}

		/// <summary>
		/// Handles the EnabledChanged event of the FlatForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void FlatForm_EnabledChanged(object sender, EventArgs e)
		{
			Theme_PropertyChangedEvent(this, EventArgs.Empty);
		}

		/// <summary>
		/// Handles the Resize event of the <see cref="GorgonFlatForm"/> control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Form_Resize(object sender, EventArgs e)
		{
			try
			{
				if ((DesignMode) || (_currentPadding == null))
				{
					return;
				}

				switch (WindowState)
				{
					case FormWindowState.Maximized:
						if (base.WindowState == FormWindowState.Minimized)
						{
							_windowState = FormWindowState.Maximized;
						}

						_currentPadding = Padding;
						Padding = new Padding(0);
						break;
					case FormWindowState.Normal:
						if (base.WindowState == FormWindowState.Minimized)
						{
							_windowState = FormWindowState.Normal;
						}

						RestoreBounds = DesktopBounds;
						Padding = _currentPadding.Value;
						break;
					default:
						Padding = _currentPadding.Value;
						break;
				}
			}
			finally
			{
				ValidateWindowControls();
			}
		}

		/// <summary>
		/// Function to draw the border for the window.
		/// </summary>
		/// <param name="graphics">Graphics interface to use.</param>
		private void DrawBorder(DrawingGraphics graphics)
		{
			if ((WindowState != FormWindowState.Normal) || (!ShowBorder) || (_borderWidth <= 0))
			{
				return;
			}

			using (Pen pen = new Pen(ActiveForm == this ? _theme.WindowBorderActive : _theme.WindowBorderInactive, _borderWidth))
			{
				float halfBorder = _borderWidth / 2.0f;
				graphics.DrawRectangle(pen, halfBorder, halfBorder, ClientSize.Width - _borderWidth, ClientSize.Height - _borderWidth);
			}
		}

		/// <summary>
		/// Handles the Paint event of the <see cref="GorgonFlatForm"/> control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
		private void Form_Paint(object sender, PaintEventArgs e)
		{
			DrawBorder(e.Graphics);
		}

		/// <summary>
		/// Handles the PaddingChanged event of the <see cref="GorgonFlatForm"/> control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void Form_PaddingChanged(object sender, EventArgs e)
		{
			// Only store the current padding for the first time.
			if (_currentPadding == null)
			{
				_currentPadding = Padding;
			}

			ValidateWindowControls();
		}

		/// <summary>
		/// Handles the PropertyChangedEvent event of the Theme control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="eventArgs">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void Theme_PropertyChangedEvent(object sender, EventArgs eventArgs)
		{
			_panelContent.BackColor = _theme.ContentPanelBackground;
			labelCaption.BackColor = _theme.WindowBackground;
			labelMinimize.BackColor = _theme.WindowBackground;
			labelMaxRestore.BackColor = _theme.WindowBackground;
			labelClose.BackColor = _theme.WindowBackground;
			base.BackColor = _theme.WindowBackground;
			pictureIcon.BackColor = _theme.WindowBackground;

			// If this window is not enabled, then we need to set to disabled.
			if (!Enabled)
			{
				labelCaption.ForeColor = _theme.DisabledColor;
				labelMinimize.ForeColor = _theme.DisabledColor;
				labelMaxRestore.ForeColor = _theme.DisabledColor;
				labelClose.ForeColor = _theme.DisabledColor;
				_panelContent.ForeColor = _theme.DisabledColor;

				ApplyTheme();

				Invalidate(true);
				return;
			}

			if ((ActiveForm != this) && (!DesignMode))
			{
				labelCaption.ForeColor = _theme.ForeColorInactive;
				labelMinimize.ForeColor = _theme.ForeColorInactive;
				labelMaxRestore.ForeColor = _theme.ForeColorInactive;
				labelClose.ForeColor = _theme.ForeColorInactive;
				_panelContent.ForeColor = _theme.ForeColorInactive;

				ApplyTheme();

				Invalidate(true);
				return;
			}

			_panelContent.ForeColor = _theme.ForeColor;
			labelCaption.ForeColor = _theme.ForeColor;
			labelMinimize.ForeColor = _theme.WindowSizeIconsForeColor;
			labelMaxRestore.ForeColor = _theme.WindowSizeIconsForeColor;
			labelClose.ForeColor = _theme.WindowCloseIconForeColor;

			ApplyTheme();

			Invalidate(true);
		}

		/// <summary>
		/// Function to inform the designer that the theme property needs to be serialized.
		/// </summary>
		/// <returns><b>true</b>.</returns>
		private bool ShouldSerializeTheme()
		{
			return true;
		}

		/// <summary>
		/// Function to process a command key.
		/// </summary>
		/// <param name="msg">A <see cref="T:System.Windows.Forms.Message" />, passed by reference, that represents the Win32 message to process.</param>
		/// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys" /> values that represents the key to process.</param>
		/// <returns>
		/// <b>true</b> if the keystroke was processed and consumed by the control; otherwise, <b>false</b> to allow further processing.
		/// </returns>
		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
		    if (keyData != (Keys.Alt | Keys.Space))
		    {
			    return base.ProcessCmdKey(ref msg, keyData);
		    }

		    ShowSysMenu();
		    return true;
		}

		/// <summary>
		/// Function to process window messages.
		/// </summary>
		/// <param name="m">The Windows <see cref="T:System.Windows.Forms.Message" /> to process.</param>
		protected override void WndProc(ref Message m)
		{
			switch ((WindowMessages)m.Msg)
			{
				case WindowMessages.EnterSizeMove:
					if (DesignMode)
					{
						return;
					}

					// If we're resizing, then begin resizing.
					OnResizeBegin(EventArgs.Empty);
					break;
				case WindowMessages.ExitSizeMove:
					if (DesignMode)
					{
						return;
					}

					// If we're resizing, then stop.
					OnResizeEnd(EventArgs.Empty);
					break;
			}
			base.WndProc(ref m);
		}

		/// <summary>
		/// Function to allow themes to be applied to child controls/windows that do not support <see cref="GorgonFlatFormTheme"/>.
		/// </summary>
		/// <remarks>
		/// <para>
		/// While the <see cref="GorgonFlatForm"/> is themeable, most controls are not. And while many controls will inherit the color scheme of their parent window, some will use their own. This method, when 
		/// overridden in your application will allow you to change the color scheme of those child controls (or windows) that don't support theming.
		/// </para>
		/// </remarks>
		protected virtual void ApplyTheme()
		{
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonFlatForm"/> class.
		/// </summary>
		public GorgonFlatForm()
		{
			ResizeHandleSize = 6;
			Resizable = true;

			SetStyle(ControlStyles.ResizeRedraw, true);

			InitializeComponent();

			RestoreBounds = Bounds;

			ValidateWindowControls();
		}
		#endregion
	}
}
