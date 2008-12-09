#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: Saturday, December 06, 2008 2:01:57 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using Drawing = System.Drawing;
using System.Data;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Track display control.
	/// </summary>
	public partial class TrackKeyDisplay : UserControl
	{
		#region Constants.
		/// <summary>
		/// Maximum frames/row.
		/// </summary>
		public const int MaxFramesPerRow = 1024;		
		#endregion

		#region Value Types.
		private struct KeyBox
		{
			#region Variables.			
			private Drawing.Rectangle _box;			// Box for hittest.			
			private float _key;						// Keyframe time for box.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the hit test box for the key.
			/// </summary>
			public Drawing.Rectangle Box
			{
				get
				{
					return _box;
				}
			}

			/// <summary>
			/// Property to return the key for the box.
			/// </summary>
			public float KeyTime
			{
				get
				{
					return _key;
				}
			}			
			#endregion

			#region Methods.
			/// <summary>
			/// Returns the hash code for this instance.
			/// </summary>
			/// <returns>
			/// A 32-bit signed integer that is the hash code for this instance.
			/// </returns>
			public override int GetHashCode()
			{
				return _box.GetHashCode() ^ _key.GetHashCode();
			}

			/// <summary>
			/// Indicates whether this instance and a specified object are equal.
			/// </summary>
			/// <param name="obj">Another object to compare to.</param>
			/// <returns>
			/// true if <paramref name="obj"/> and this instance are the same type and represent the same value; otherwise, false.
			/// </returns>
			public override bool Equals(object obj)
			{
				if (obj is KeyBox)
					return (((KeyBox)obj).Box == _box);

				return false;
			}

			/// <summary>
			/// Implements the operator ==.
			/// </summary>
			/// <param name="left">The left.</param>
			/// <param name="right">The right.</param>
			/// <returns>The result of the operator.</returns>
			public static bool operator ==(KeyBox left, KeyBox right)
			{
				return left.Box == right.Box;
			}

			/// <summary>
			/// Implements the operator !=.
			/// </summary>
			/// <param name="left">The left.</param>
			/// <param name="right">The right.</param>
			/// <returns>The result of the operator.</returns>
			public static bool operator !=(KeyBox left, KeyBox right)
			{
				return !(left == right);
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="KeyBox"/> struct.
			/// </summary>
			/// <param name="box">The box containing the key.</param>
			/// <param name="keyTime">The key time used by the box.</param>
			public KeyBox(Drawing.Rectangle box, float keyTime)
			{
				_box = box;
				_key = keyTime;
			}
			#endregion
		}
		#endregion

		#region Variables.
		private Track _track = null;											// Track to display.
		private Drawing.Font _numberFont = null;								// Number font.
		private List<KeyBox> _keyBoxes = null;									// Key boxes.
		private Drawing.Drawing2D.LinearGradientBrush _backBrush = null;		// Background cell brush.
		private Drawing.Drawing2D.LinearGradientBrush _hiliteBrush = null;		// Hilite cell brush.
		private Drawing.Drawing2D.LinearGradientBrush _backSelBrush = null;		// Background selected cell brush.
		private Drawing.Drawing2D.LinearGradientBrush _hiliteSelBrush = null;	// Hilite selected cell brush.
		private Drawing.SolidBrush _textBrush = null;							// Text brush.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the track this control represents.
		/// </summary>
		public Track Track
		{
			get
			{
				return _track;
			}
			set
			{
				_track = value;
				UpdateKeyPanelSize();
			}
		}

		/// <summary>
		/// Property to set or return the animation window that owns this.
		/// </summary>
		public formAnimationEditor OwnerForm
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Paint event of the panelKeyList control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.PaintEventArgs"/> instance containing the event data.</param>
		private void panelKeyList_Paint(object sender, PaintEventArgs e)
		{
			if (!Visible)
				return;

			Drawing.Pen pen = new Drawing.Pen(Drawing.Color.Black);			
			Drawing.Brush blackBrush = new Drawing.SolidBrush(panelKeyList.ForeColor);
			Drawing.Rectangle drawArea = panelKeyList.DisplayRectangle;
			Drawing.SizeF textSize = Drawing.SizeF.Empty;
			int keyCount = 0;
			int rows = 1;
			int position = 0;
			int roundedKeyCount = 0;
			
			try
			{
				if (Track == null)
					return;

				keyCount = (int)((Track.Owner.Length / 1000.0f) * Track.Owner.FrameRate);

				if (keyCount < 1)
					keyCount = 1;

				drawArea.Width -= 1;
				drawArea.Height -= 1;

				roundedKeyCount = keyCount;
				while ((roundedKeyCount % 1024) != 0)
					roundedKeyCount++;
								
				rows = roundedKeyCount / MaxFramesPerRow;

				if (rows == 0)
					rows = 1;

				for (int i = 0; i < rows; i++)
				{
					e.Graphics.FillRectangle(_backBrush, new Drawing.Rectangle(e.ClipRectangle.X, i * 32, e.ClipRectangle.Width, 32));
					e.Graphics.FillRectangle(_hiliteBrush, new Drawing.Rectangle(e.ClipRectangle.X, i * 32, e.ClipRectangle.Width, (int)_hiliteBrush.Rectangle.Height));
				}

				_keyBoxes.Clear();
				foreach (KeyFrame key in Track)
				{
					int keyIndex = MathUtility.RoundInt((key.Time / 1000.0f) * Track.Owner.FrameRate);
					int tempIndex = keyIndex;
					int row = 0;

					while ((tempIndex - MaxFramesPerRow) >= 0)
					{
						row++;
						tempIndex -= MaxFramesPerRow;
					}

					position = tempIndex * 32;
				
					KeyBox keyBox = new KeyBox(new Drawing.Rectangle(position, row * 32, 32, 32), key.Time);
					if (e.ClipRectangle.IntersectsWith(keyBox.Box))
					{
						_keyBoxes.Add(keyBox);
						e.Graphics.FillRectangle(_backSelBrush, keyBox.Box);
						e.Graphics.FillRectangle(_hiliteSelBrush, new Drawing.Rectangle(keyBox.Box.X, keyBox.Box.Y, keyBox.Box.Width, (int)_hiliteSelBrush.Rectangle.Height));
					}
				}

				position = 0;
				for (int key = 0; key < keyCount; key++)
				{
					int row = 0;
					int tempIndex = key;

					while ((tempIndex - MaxFramesPerRow) >= 0)
					{
						row++;
						tempIndex -= MaxFramesPerRow;
					}

					if (position >= 32 * MaxFramesPerRow)
						position = 0;

					KeyBox keyBox = new KeyBox(new Drawing.Rectangle(position, row * 32, 32, 32), ((float)key / Track.Owner.FrameRate) * 1000.0f);					
										
					if (e.ClipRectangle.IntersectsWith(keyBox.Box))
					{
						if (!_keyBoxes.Contains(keyBox))
							_keyBoxes.Add(keyBox);	

						if ((OwnerForm.CurrentFrame == key) && (Track == OwnerForm.DropIn.CurrentTrack))
							e.Graphics.DrawImage(Properties.Resources.Indicator, new Drawing.Point(keyBox.Box.Right - Properties.Resources.Indicator.Width, keyBox.Box.Bottom - Properties.Resources.Indicator.Height));

						e.Graphics.DrawRectangle(pen, keyBox.Box);
						textSize = e.Graphics.MeasureString(key.ToString(), _numberFont, row * 32);
						e.Graphics.DrawString(key.ToString(), _numberFont, blackBrush, keyBox.Box);
					}
					position += 32;
				}

				e.Graphics.DrawRectangle(pen, drawArea);
			}
			finally
			{
				if (blackBrush != null)
					blackBrush.Dispose();
				if (pen == null)
					pen.Dispose();
				blackBrush = null;
				pen = null;
			}
		}

		/// <summary>
		/// Handles the MouseClick event of the panelKeyList control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void panelKeyList_MouseClick(object sender, MouseEventArgs e)
		{
			OnMouseClick(e);
		}

		/// <summary>
		/// Handles the MouseDown event of the panelKeyList control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.Windows.Forms.MouseEventArgs"/> instance containing the event data.</param>
		private void panelKeyList_MouseDown(object sender, MouseEventArgs e)
		{
			OnMouseDown(e);

			if (_keyBoxes.Count == 0)
			{
				panelKeyList.Invalidate();
				Application.DoEvents();
			}

			for (int i = 0; i < _keyBoxes.Count; i++)
			{
				if (_keyBoxes[i].Box.Contains(e.Location))
				{
					OwnerForm.SetTime(_keyBoxes[i].KeyTime);
					OwnerForm.DropIn.CurrentTime = _keyBoxes[i].KeyTime;
					break;
				}
			}
		}

		/// <summary>
		/// Function to update the key panel size.
		/// </summary>
		private void UpdateKeyPanelSize()
		{
			if (Track == null)
			{
				panelKeyList.Width = 2;
				return;
			}

			_keyBoxes = new List<KeyBox>((int)((Track.Owner.Length / 1000.0f) * Track.Owner.FrameRate));
			panelKeyList.Width = (int)((Track.Owner.Length / 1000.0f) * Track.Owner.FrameRate) * 32 + 1;
		}

		/// <summary>
		/// Forces the control to invalidate its client area and immediately redraw itself and any child controls.
		/// </summary>
		/// <PermissionSet>
		/// 	<IPermission class="System.Security.Permissions.EnvironmentPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
		/// 	<IPermission class="System.Security.Permissions.FileIOPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
		/// 	<IPermission class="System.Security.Permissions.SecurityPermission, mscorlib, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Flags="UnmanagedCode, ControlEvidence"/>
		/// 	<IPermission class="System.Diagnostics.PerformanceCounterPermission, System, Version=2.0.3600.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" version="1" Unrestricted="true"/>
		/// </PermissionSet>
		public override void Refresh()
		{
			base.Refresh();
			panelKeyList.Invalidate(true);
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs"/> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			_backBrush = new Drawing.Drawing2D.LinearGradientBrush(new Drawing.Rectangle(0, 0, 150, 32), Drawing.Color.Black, Drawing.Color.DarkGray, 90.0f);
			_hiliteBrush = new Drawing.Drawing2D.LinearGradientBrush(new Drawing.Rectangle(0, 0, 150, 16), Drawing.Color.White, Drawing.Color.Gray, 90.0f);
			_backSelBrush = new Drawing.Drawing2D.LinearGradientBrush(new Drawing.Rectangle(0, 0, 150, 32), Drawing.Color.DarkBlue, Drawing.Color.LightBlue, 90.0f);
			_hiliteSelBrush = new Drawing.Drawing2D.LinearGradientBrush(new Drawing.Rectangle(0, 0, 150, 16), Drawing.Color.White, Drawing.Color.LightBlue, 90.0f);
			_textBrush = new Drawing.SolidBrush(Drawing.Color.White);
			
			_numberFont = new Drawing.Font("MS San Serif", 8.5f, Drawing.FontStyle.Bold);
			UpdateKeyPanelSize();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="TrackKeyDisplay"/> class.
		/// </summary>
		public TrackKeyDisplay()
		{
			InitializeComponent();
		}
		#endregion
	}
}
