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
// This object was adapted from the CodeProject article: Metro UI (Zune like) Interface (form).
// This article can be found at http://www.codeproject.com/Articles/138661/Metro-UI-Zune-like-Interface-form#xx3740425xx
//
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using GorgonLibrary.Native;
using GorgonLibrary.Design;
using GorgonLibrary.Math;
using GorgonLibrary.Properties;

namespace GorgonLibrary.UI
{
	/// <summary>
	/// A form that provides a flattened interface look.
	/// </summary>
	/// <remarks>
	/// This is an overload to the standard windows form that mimics the flat style of the old Microsoft Zune application, and, to some degree, Windows 8 windows on 
	/// operating systems that don't use flat style forms.
	/// <para>
    /// To use the form, merely create a new Windows Form in Visual Studio and then change the inheritance from <c>System.Windows.Forms.Form</c> to <c>GorgonLibrary.UI.FlatForm</c>. 
    /// Once this is done the form will change to the flat style.
	/// </para>
	/// <para>
	/// This form uses the client area of the window to handle the caption, and system buttons (close, minimize, etc...), so it is possible to add new buttons, images, etc.. into 
	/// the caption of the window for a truly customized look.
	/// </para>
	/// </remarks>
	public partial class FlatForm : Form
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
		private Rectangle _restoreRect;
		private FlatFormTheme _theme = new FlatFormTheme();
		private int _captionHeight;
		[AccessedThroughProperty("ContentArea")]
		private Panel _panelContent;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the resize direction.
		/// </summary>
		private ResizeDirection ResizeDir
		{
			get
			{
				return _resizeDirection;
			}
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
		/// </summary>
		/// <PermissionSet>
		///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
		/// </PermissionSet>
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Color BackColor
		{
			get
			{
				return _theme.WindowBackground;
			}
			set
			{
				_theme.WindowBackground = value;
				Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the foreground color of the control.
		/// </summary>
		/// <PermissionSet>
		///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
		/// </PermissionSet>
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new Color ForeColor
		{
			get
			{
				return _theme.ForeColor;
			}
			set
			{
				_theme.ForeColor = value;
				Refresh();
			}
		}

		/// <summary>
		/// Property to return the content area.
		/// </summary>
		[Browsable(false)]
		public Panel ContentArea
		{
			get
			{
				return _panelContent;
			}
		}

		/// <summary>
		/// Property to set or return the current theme for the window.
		/// </summary>
		[Browsable(true), LocalCategory(typeof(Resources), "PROP_CATEGORY_APPEARANCE"), RefreshProperties(RefreshProperties.All)]
		public FlatFormTheme Theme
		{
			get
			{
				return _theme;
			}
			set
			{
				if (value == null)
				{
					value = new FlatFormTheme();
				}

				_theme = value;

				ToolStripManager.RenderMode = ToolStripManagerRenderMode.Custom;
				ToolStripManager.Renderer = _theme;

				ApplyTheme();
				
				Refresh();
			}
		}

		/// <summary>
		/// Gets or sets the border style of the form.
		/// </summary>
		/// <returns>A <see cref="T:System.Windows.Forms.FormBorderStyle" /> that represents the style of border to display for the form. The default is FormBorderStyle.Sizable.</returns>
		///   <PermissionSet>
		///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
		///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
		///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
		///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
		///   </PermissionSet>
		[Browsable(false), EditorBrowsable(EditorBrowsableState.Never), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public new FormBorderStyle FormBorderStyle
		{
			get
			{
				return FormBorderStyle.None;
			}
			// ReSharper disable once ValueParameterNotUsed
			set
			{
				base.FormBorderStyle = FormBorderStyle.None;
			}
		}

		/// <summary>
		/// Property to set or return whether a single pixel width border will be drawn on the form.
		/// </summary>
		[Browsable(true), LocalCategory(typeof(Resources), "PROP_CATEGORY_APPEARANCE"), LocalDescription(typeof(Resources), "PROP_BORDER_DESC"), DefaultValue(false), 
		RefreshProperties(RefreshProperties.All)]
		public bool Border
		{
			get
			{
				return _border;
			}
			set
			{
				_border = value;

				Refresh();
			}
		}

		/// <summary>
		/// Property to set or return the size of the border, in pixels.
		/// </summary>
		/// <remarks>This is only valid when <see cref="GorgonLibrary.UI.FlatForm.Resizable">Resizable</see> is set to TRUE.</remarks>
		[Browsable(true), LocalDescription(typeof(Resources), "PROP_BORDERSIZE_DESC"), LocalCategory(typeof(Resources), "PROP_CATEGORY_APPEARANCE"),
		RefreshProperties(RefreshProperties.All), DefaultValue(1)]
		public int BorderSize
		{
			get
			{
				return _borderWidth;
			}
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
		/// <remarks>This is only valid when <see cref="GorgonLibrary.UI.FlatForm.Resizable">Resizable</see> is set to TRUE.</remarks>
		[Browsable(true), LocalDescription(typeof(Resources), "PROP_RESIZEHANDLE_DESC"), LocalCategory(typeof(Resources), "PROP_CATEGORY_DESIGN"),
		RefreshProperties(RefreshProperties.All), DefaultValue(6)]
		public int ResizeHandleSize
		{
			get;
			set;
		}

		/// <summary>
		/// Hidden control box property.
		/// </summary>
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
		/// Hidden help button box property.
		/// </summary>
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
		/// Property to set or return whether to show the icon for the form.
		/// </summary>
		[Browsable(true), LocalCategory(typeof(Resources), "PROP_CATEGORY_DESIGN"), LocalDescription(typeof(Resources), "PROP_SHOWICON_DESC")]
		public new bool ShowIcon
		{
			get
			{
				return base.ShowIcon;
			}
			set
			{
				base.ShowIcon = value;
				pictureIcon.Visible = base.ShowIcon;
			}
		}

		/// <summary>
		/// Property to set or return whether the window caption, including the system menu, max/min buttons and the close button are visible.
		/// </summary>
		[Browsable(true), LocalCategory(typeof(Resources), "PROP_CATEGORY_WINDOWSTYLE"), LocalDescription(typeof(Resources), "PROP_WINDOWCAPTION_DESC")]
		public bool ShowWindowCaption
		{
			get
			{
				return _showWindowCaption;
			}
			set
			{
				_showWindowCaption = value;
				ValidateWindowControls();
			}
		}

		/// <summary>
		/// </summary>
		/// <returns>The text associated with this control.</returns>
		[Browsable(true), LocalCategory(typeof(Resources), "PROP_CATEGORY_APPEARANCE"), LocalDescription(typeof(Resources), "PROP_TEXT_DESC")]
		public override string Text
		{
			get
			{
				return base.Text;
			}
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
			get
			{
				return base.Icon;
			}
			set
			{
				base.Icon = value;
				ExtractIcon();					
				ValidateWindowControls();
			}
		}

		/// <summary>
		/// Property to return the sizing grip style.
		/// </summary>
		/// <remarks>This is not used for the Zune style window.</remarks>
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
		public new Rectangle RestoreBounds
		{
			get
			{
				return _restoreRect;
			}
		}

		/// <summary>
		/// Gets or sets a value that indicates whether form is minimized, maximized, or normal.
		/// </summary>
		/// <returns>A <see cref="T:System.Windows.Forms.FormWindowState" /> that represents whether form is minimized, maximized, or normal. The default is FormWindowState.Normal.</returns>
		/// <PermissionSet>
		///   <IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
		///   <IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
		///   <IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence" />
		///   <IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true" />
		/// </PermissionSet>
		public new FormWindowState WindowState
		{
			get
			{
				return _windowState;
			}
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
					_restoreRect = Bounds;
				}

				if ((!IsHandleCreated) || (DesignMode))
				{
					return;
				}

				if ((value == FormWindowState.Normal) && (_prevMinState == FormWindowState.Maximized))
				{
					SetBounds(_restoreRect.Left, _restoreRect.Top, _restoreRect.Width, _restoreRect.Height, BoundsSpecified.All);
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

			using (Graphics g = CreateGraphics())
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
			Win32API.ReleaseCapture();										// SC_MOVE
			Win32API.SendMessage(Handle, (int)WindowMessages.SysCommand, new IntPtr(0xF010), IntPtr.Zero);
		}

		/// <summary>
		/// Handles the Click event of the itemSize control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemSize_Click(object sender, EventArgs e)
		{
			Win32API.ReleaseCapture();										// SC_SIZE
			Win32API.SendMessage(Handle, (int)WindowMessages.SysCommand, new IntPtr(0xF000), IntPtr.Zero);
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
            var label = sender as Label;

			if (label == null)
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
            var label = sender as Label;

	        if (label == null)
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
			var label = sender as Label;

			if (label == null)
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
			var label = sender as Label;

			if (label == null)
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
		/// <param name="control">Control with the local mouse coorindates.</param>
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

			int maxSizeForContainer = (panelWinIcons.Height.Max(_iconImage != null ? _iconImage.Height + 8 : 8).Max(_captionHeight));

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
					labelMaxRestore.Text = Resources.GOR_ZUNE_MAX_ICON;
					break;
				case FormWindowState.Minimized:
					itemRestore.Enabled = true;
					itemMove.Enabled = false;
					itemSize.Enabled = false;
					itemMaximize.Enabled = true;
					itemMinimize.Enabled = false;
					break;
				case FormWindowState.Normal:
					labelMaxRestore.Text = Resources.GOR_ZUNE_RESTORE_ICON;
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
		/// Processes a command key.
		/// </summary>
		/// <param name="msg">A <see cref="T:System.Windows.Forms.Message" />, passed by reference, that represents the Win32 message to process.</param>
		/// <param name="keyData">One of the <see cref="T:System.Windows.Forms.Keys" /> values that represents the key to process.</param>
		/// <returns>
		/// true if the keystroke was processed and consumed by the control; otherwise, false to allow further processing.
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
		/// Handles the MouseMove event of the ZuneForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void ZuneForm_MouseMove(object sender, MouseEventArgs e)
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
		/// Handles the Activated event of the ZuneForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ZuneForm_Activated(object sender, EventArgs e)
		{
			if (base.WindowState == FormWindowState.Minimized)
			{
				_windowState = _prevMinState;	
			}

			panelCaptionArea.ForeColor = _theme.ForeColor;
			labelMinimize.ForeColor = _theme.ForeColor;
			labelMaxRestore.ForeColor = _theme.ForeColor;
			labelClose.ForeColor = _theme.ForeColor;

			using (var graphics = CreateGraphics())
			{
				DrawBorder(graphics);
			}
		}

		/// <summary>
		/// Handles the Deactivate event of the ZuneForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ZuneForm_Deactivate(object sender, EventArgs e)
		{
			panelCaptionArea.ForeColor = _theme.ForeColorInactive;
			labelMinimize.ForeColor = _theme.ForeColorInactive;
			labelMaxRestore.ForeColor = _theme.ForeColorInactive;
			labelClose.ForeColor = _theme.ForeColorInactive;
			using (var graphics = CreateGraphics())
			{
				DrawBorder(graphics);
			}
		}

		/// <summary>
		/// Window procedure override.
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
		/// Handles the MouseDown event of the ZuneForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void ZuneForm_MouseDown(object sender, MouseEventArgs e)
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
					Win32API.ReleaseCapture();
					Win32API.SendMessage(Handle, (uint)WindowMessages.NCLeftButtonDown, new IntPtr((int)HitTests.Caption), IntPtr.Zero);
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
							Win32API.ReleaseCapture();
							Cursor.Current = Cursors.SizeWE;
							Win32API.SendMessage(Handle, (uint)WindowMessages.NCLeftButtonDown, new IntPtr((int)HitTests.Left), IntPtr.Zero);
							break;
						case ResizeDirection.TopLeft:
							Win32API.ReleaseCapture();
							Cursor.Current = Cursors.SizeNWSE;
							Win32API.SendMessage(Handle, (uint)WindowMessages.NCLeftButtonDown, new IntPtr((int)HitTests.TopLeft), IntPtr.Zero);
							break;
						case ResizeDirection.Top:
							Win32API.ReleaseCapture();
							Cursor.Current = Cursors.SizeNS;
							Win32API.SendMessage(Handle, (uint)WindowMessages.NCLeftButtonDown, new IntPtr((int)HitTests.Top), IntPtr.Zero);
							break;
						case ResizeDirection.TopRight:
							Win32API.ReleaseCapture();
							Cursor.Current = Cursors.SizeNESW;
							Win32API.SendMessage(Handle, (uint)WindowMessages.NCLeftButtonDown, new IntPtr((int)HitTests.TopRight), IntPtr.Zero);
							break;
						case ResizeDirection.Right:
							Win32API.ReleaseCapture();
							Cursor.Current = Cursors.SizeWE;
							Win32API.SendMessage(Handle, (uint)WindowMessages.NCLeftButtonDown, new IntPtr((int)HitTests.Right), IntPtr.Zero);
							break;
						case ResizeDirection.BottomRight:
							Win32API.ReleaseCapture();
							Cursor.Current = Cursors.SizeNWSE;
							Win32API.SendMessage(Handle, (uint)WindowMessages.NCLeftButtonDown, new IntPtr((int)HitTests.BottomRight), IntPtr.Zero);
							break;
						case ResizeDirection.Bottom:
							Win32API.ReleaseCapture();
							Cursor.Current = Cursors.SizeNS;
							Win32API.SendMessage(Handle, (uint)WindowMessages.NCLeftButtonDown, new IntPtr((int)HitTests.Bottom), IntPtr.Zero);
							break;
						case ResizeDirection.BottomLeft:
							Win32API.ReleaseCapture();
							Cursor.Current = Cursors.SizeNESW;
							Win32API.SendMessage(Handle, (uint)WindowMessages.NCLeftButtonDown, new IntPtr((int)HitTests.BottomLeft), IntPtr.Zero);
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
		/// Handles the Load event of the ZuneForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ZuneForm_Load(object sender, EventArgs e)
		{
			try
			{
				ToolStripManager.Renderer = _theme;

				labelCaption_TextChanged(this, EventArgs.Empty);
				ValidateWindowControls();
				ApplyTheme();
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
			using (Graphics g = Graphics.FromImage(_iconImage))
			{
				g.DrawIcon(Icon, new Rectangle(0, 0, 24, 24));
			}

			pictureIcon.Image = _iconImage;
		}

		/// <summary>
		/// Handles the Resize event of the ZuneForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ZuneForm_Resize(object sender, EventArgs e)
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

						_restoreRect = DesktopBounds;
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
		private void DrawBorder(Graphics graphics)
		{
			if ((WindowState != FormWindowState.Normal) || (!Border))
			{
				return;
			}

			using (var pen = new Pen(ActiveForm == this ? _theme.WindowBorderActive : _theme.WindowBorderInactive, _borderWidth))
			{
				graphics.DrawRectangle(pen, new Rectangle(0, 0, Width - _borderWidth, Height - _borderWidth));
			}
		}

		/// <summary>
		/// Handles the Paint event of the ZuneForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
		private void ZuneForm_Paint(object sender, PaintEventArgs e)
		{
			DrawBorder(e.Graphics);
		}

		/// <summary>
		/// Handles the PaddingChanged event of the ZuneForm control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void ZuneForm_PaddingChanged(object sender, EventArgs e)
		{
			// Only store the current padding for the first time.
			if (_currentPadding == null)
			{
				_currentPadding = Padding;
			}

			ValidateWindowControls();
		}

		/// <summary>
		/// Function called to allow sub-classed windows to apply the theme to controls that are not necessarily themeable.
		/// </summary>
		protected virtual void ApplyTheme()
		{
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="FlatForm"/> class.
		/// </summary>
		public FlatForm()
		{
			ResizeHandleSize = 6;
			Resizable = true;

			SetStyle(ControlStyles.ResizeRedraw, true);

			InitializeComponent();

			_restoreRect = Bounds;

			ValidateWindowControls();
		}
		#endregion
	}
}
