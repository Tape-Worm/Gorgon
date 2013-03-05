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
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using GorgonLibrary.Native;

namespace GorgonLibrary.UI
{
	/// <summary>
	/// Mimics the Zune (and Visual Studio 2012) UI window.
	/// </summary>
	public partial class ZuneForm : Form
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
		private MARGINS _dwmMargins = default(MARGINS);
		private bool _marginOk = false;
		private ResizeDirection _resizeDirection = ResizeDirection.None;
		private Image _iconImage = null;
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
		/// Property to set or return the width of the border, in pixels.
		/// </summary>
		[Browsable(true), Description("The width of the resize border in pixels."), Category("Design")]
		public int BorderWidth
		{
			get;
			set;
		}

		/// <summary>
		/// </summary>
		/// <returns>The text associated with this control.</returns>
		[Browsable(true)]
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
			}
		}

		/// <summary>
		/// Property to set or return the icon for this form.
		/// </summary>
		public new Icon Icon
		{
			get
			{
				return base.Icon;
			}
			set
			{
				base.Icon = value;
				if (!DesignMode)
				{
					ExtractIcon();
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the DoubleClick event of the pictureIcon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void pictureIcon_DoubleClick(object sender, EventArgs e)
		{
			Close();
		}

		/// <summary>
		/// Handles the Click event of the pictureIcon control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void pictureIcon_Click(object sender, EventArgs e)
		{
			SendKeys.Send("% ");
		}

		/// <summary>
        /// Handles the MouseEnter event of the labelClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void labelClose_MouseEnter(object sender, EventArgs e)
        {
            Label label = sender as Label;

            if (label != null)
            {
                label.ForeColor = Color.FromKnownColor(KnownColor.HighlightText);
            }
        }

        /// <summary>
        /// Handles the MouseLeave event of the labelClose control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void labelClose_MouseLeave(object sender, EventArgs e)
        {
            Label label = sender as Label;

            if (label != null)
            {
                label.ForeColor = ForeColor;
            }
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
            if (WindowState != FormWindowState.Maximized)
            {
                WindowState = FormWindowState.Maximized;
            }
            else
            {
                WindowState = FormWindowState.Normal;
            }
            ValidateWindowControls();
        }

        /// <summary>
        /// Handles the MouseDown event of the labelCaption control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void labelCaption_MouseDown(object sender, MouseEventArgs e)
        {
            OnMouseDown(e);
        }

        /// <summary>
        /// Handles the MouseMove event of the labelCaption control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void labelCaption_MouseMove(object sender, MouseEventArgs e)
        {
            OnMouseMove(e);
        }
        
        /// <summary>
		/// Function to validate all the window controls.
		/// </summary>
		private void ValidateWindowControls()
		{
			if (ControlBox)
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
				}
			}
			else
			{
				panelCaptionArea.Visible = false;
			}

			if (WindowState == FormWindowState.Maximized)
			{
				labelMaxRestore.Text = "2";
			}
			else
			{
				labelMaxRestore.Text = "1";
			}
		}

		/// <summary>
		/// Function to perform hit testing on the non-client area.
		/// </summary>
		/// <param name="hwnd">Window handle.</param>
		/// <param name="wParam">Message parameter.</param>
		/// <param name="lParam">Message parameter.</param>
		/// <returns>A pointer.</returns>
		private HitTests HitTestNonClient(IntPtr hwnd, IntPtr wParam, IntPtr lParam)
		{			
			Point point = new Point(lParam.ToInt32() & 0xFFFF, (lParam.ToInt32() >> 16) & 0xFFFF);
			Rectangle caption = RectangleToScreen(new Rectangle(0, _dwmMargins.cxLeftWidth, Width, _dwmMargins.cyTopHeight - _dwmMargins.cxLeftWidth));
			Rectangle top = RectangleToScreen(new Rectangle(0, 0, Width, _dwmMargins.cxLeftWidth));
			Rectangle left = RectangleToScreen(new Rectangle(0, 0, _dwmMargins.cxLeftWidth, Height));
			Rectangle right = RectangleToScreen(new Rectangle(Width - _dwmMargins.cxRightWidth, 0, _dwmMargins.cxRightWidth, Height));
			Rectangle bottom = RectangleToScreen(new Rectangle(0, Height - _dwmMargins.cyBottomHeight, Width, _dwmMargins.cyBottomHeight));
			Rectangle topLeft = RectangleToScreen(new Rectangle(0, 0, _dwmMargins.cxLeftWidth, _dwmMargins.cxLeftWidth));
			Rectangle topRight = RectangleToScreen(new Rectangle(Width - _dwmMargins.cxRightWidth, 0, _dwmMargins.cxRightWidth, _dwmMargins.cxRightWidth));
			Rectangle bottomLeft = RectangleToScreen(new Rectangle(0, Height - _dwmMargins.cyBottomHeight, _dwmMargins.cxLeftWidth, _dwmMargins.cyBottomHeight));
			Rectangle bottomRight = RectangleToScreen(new Rectangle(Width - _dwmMargins.cxRightWidth, Height - _dwmMargins.cyBottomHeight, _dwmMargins.cxRightWidth, _dwmMargins.cyBottomHeight));

			if (topLeft.Contains(point))
			{
				return HitTests.TopLeft;
			}

			if (topRight.Contains(point))
			{
				return HitTests.TopRight;
			}

			if (top.Contains(point))
			{
				return HitTests.Top;
			}

			if (left.Contains(point))
			{
				return HitTests.Left;
			}

			if (right.Contains(point))
			{
				return HitTests.Right;
			}

			if (bottom.Contains(point))
			{
				return HitTests.Bottom;
			}

			if (bottomLeft.Contains(point))
			{
				return HitTests.BottomLeft;
			}

			if (bottomRight.Contains(point))
			{
				return HitTests.BottomRight;
			}

			if (caption.Contains(point))
			{
				return HitTests.Caption;
			}

			return HitTests.Client;
		}

        /// <summary>
        /// </summary>
        /// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            ValidateWindowControls();
        }

		/// <summary>
		/// Window procedure.
		/// </summary>
		/// <param name="m">The Windows <see cref="T:System.Windows.Forms.Message" /> to process.</param>
		protected override void WndProc(ref Message m)
		{
			IntPtr result = IntPtr.Zero;
			WindowMessages message = (WindowMessages)m.Msg;

			if (DesignMode)
			{
				base.WndProc(ref m);
				return;
			}

			if (Win32API.DwmDefWindowProc(m.HWnd, message, m.WParam, m.LParam, ref result) == 1)
			{
				m.Result = result;
				return;
			}

			if ((message == WindowMessages.NCCalcSize) && (m.WParam.ToInt32() == 1))
			{
				unsafe
				{
					_dwmMargins = default(MARGINS);
					//NCCALCSIZE_PARAMS sizeParameters = default(NCCALCSIZE_PARAMS);
					//m.LParam.Read<NCCALCSIZE_PARAMS>(out sizeParameters);

					if (!_marginOk)
					{
						_dwmMargins.cyBottomHeight = 3;
						_marginOk = true;
					}

					//m.LParam.Write<NCCALCSIZE_PARAMS>(ref sizeParameters);

					m.Result = IntPtr.Zero;
					return;
				}
			}

			if ((message == WindowMessages.NCHitTest) && (m.WParam.ToInt32() == 0))
			{
				m.Result = new IntPtr((int)HitTestNonClient(m.HWnd, m.WParam, m.LParam));
				return;
			}

			base.WndProc(ref m);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.MouseMove" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);

			if ((DesignMode) || (WindowState != FormWindowState.Normal))
			{
				return;
			}

			if ((e.Location.X < BorderWidth) && (e.Location.Y < BorderWidth))
			{
				ResizeDir = ResizeDirection.TopLeft;
			}
			else if ((e.Location.X < BorderWidth) && (e.Location.Y > this.Height - BorderWidth))
			{
				ResizeDir = ResizeDirection.BottomLeft;
			}
			else if ((e.Location.X > this.Width - BorderWidth) && (e.Location.Y > this.Height - BorderWidth))
			{
				ResizeDir = ResizeDirection.BottomRight;
			}
			else if ((e.Location.X > this.Width - BorderWidth) && (e.Location.Y < BorderWidth))
			{
				ResizeDir = ResizeDirection.TopRight;
			}
			else if ((e.Location.X < BorderWidth))
			{
				ResizeDir = ResizeDirection.Left;
			}
			else if ((e.Location.X > this.Width - BorderWidth))
			{
				ResizeDir = ResizeDirection.Right;
			}
			else if ((e.Location.Y < BorderWidth))
			{
				ResizeDir = ResizeDirection.Top;
			}
			else if ((e.Location.Y > this.Height - BorderWidth))
			{
				ResizeDir = ResizeDirection.Bottom;
			}
			else
			{
				ResizeDir = ResizeDirection.None;
			}
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Control.MouseDown" /> event.
		/// </summary>
		/// <param name="e">A <see cref="T:System.Windows.Forms.MouseEventArgs" /> that contains the event data.</param>
		protected override void OnMouseDown(MouseEventArgs e)
		{
			base.OnMouseDown(e);

			if (DesignMode)
			{
				return;
			}

			if (e.Button == System.Windows.Forms.MouseButtons.Left)
			{
				if ((Width - BorderWidth > e.X) && (e.X > BorderWidth) && (e.Y > BorderWidth) && (e.Y < Height - BorderWidth))
				{
			        Win32API.ReleaseCapture();
					Win32API.SendMessage(Handle, (uint)WindowMessages.NCLeftButtonDown, new IntPtr((int)HitTests.Caption), IntPtr.Zero);
				}
				else
				{
					if ((WindowState == FormWindowState.Normal) && (this.FormBorderStyle == System.Windows.Forms.FormBorderStyle.Sizable))
					{
						switch(ResizeDir)
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

			if ((!ControlBox) || (Icon == null))
			{
				return;
			}

			_iconImage = new Bitmap(24, 24, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
			using (Graphics g = Graphics.FromImage(_iconImage))
			{
				g.DrawIcon(Icon, new Rectangle(0, 0, 24, 24));
			}

			pictureIcon.Image = _iconImage;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			ValidateWindowControls();
		}

		/// <summary>
		/// Disable this function as it messes up our sizing.
		/// </summary>
		/// <param name="x">The x-coordinate.</param>
		/// <param name="y">The y-coordinate.</param>
		/// <param name="width">The bounds width.</param>
		/// <param name="height">The bounds height.</param>
		/// <param name="specified">A value from the BoundsSpecified enumeration.</param>
		protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
		{
			if (DesignMode)
			{
				base.SetBoundsCore(x, y, width, height, specified);
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="ZuneForm"/> class.
		/// </summary>
		public ZuneForm()
		{
			SetStyle(ControlStyles.ResizeRedraw, true);

			InitializeComponent();

			BorderWidth = 6;
			ValidateWindowControls();
		}
		#endregion
	}
}
