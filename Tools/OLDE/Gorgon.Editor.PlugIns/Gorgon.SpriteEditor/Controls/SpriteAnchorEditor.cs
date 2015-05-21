#region MIT.
// 
// Gorgon.
// Copyright (C) 2014 Michael Winsor
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
// Created: Monday, November 17, 2014 9:56:44 PM
// 
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using System.Windows.Forms.Design;

namespace Gorgon.Editor.SpriteEditorPlugIn.Design
{
	/// <summary>
	/// Control used to axis-align a sprite anchor.
	/// </summary>
	sealed partial class SpriteAnchorEditor 
		: UserControl
	{
		#region Variables.
		// The size of the sprite.
		private Size _spriteSize;
		// Half way point for the sprite.
		private Point _spriteHalf;
		// Current anchor.
		private Point _currentAnchor;
		// The service for the drop down control.
		private readonly IWindowsFormsEditorService _service;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the anchor point for the sprite.
		/// </summary>
		public Point SpriteAnchor
		{
			get
			{
				if (radioTopLeft.Checked)
				{
					return new Point(0, 0);
				}

				if (radioTopCenter.Checked)
				{
					return new Point(_spriteHalf.X, 0);
				}

				if (radioTopRight.Checked)
				{
					return new Point(_spriteSize.Width - 1, 0);
				}

				if (radioMiddleLeft.Checked)
				{
					return new Point(0, _spriteHalf.Y);
				}

				if (radioCenter.Checked)
				{
					return _spriteHalf;
				}

				if (radioMiddleRight.Checked)
				{
					return new Point(_spriteSize.Width - 1, _spriteHalf.Y);
				}

				if (radioBottomLeft.Checked)
				{
					return new Point(0, _spriteSize.Height - 1);
				}

				if (radioBottomCenter.Checked)
				{
					return new Point(_spriteHalf.X, _spriteSize.Height - 1);
				}

				return radioBottomRight.Checked ? new Point(_spriteSize.Width - 1, _spriteSize.Height - 1) : _currentAnchor;
			}
			set
			{
				_currentAnchor = value;

				radioTopLeft.Checked = false;
				radioTopCenter.Checked = false;
				radioTopRight.Checked = false;
				radioMiddleLeft.Checked = false;
				radioCenter.Checked = false;
				radioMiddleRight.Checked = false;
				radioBottomLeft.Checked = false;
				radioBottomCenter.Checked = false;
				radioBottomRight.Checked = false;
				
				if ((value.X == 0) && (value.Y == 0))
				{
					radioTopLeft.Checked = true;
				}

				if ((value.X == _spriteHalf.X) && (value.Y == 0))
				{
					radioTopCenter.Checked = true;
				}

				if ((value.X == _spriteSize.Width - 1) && (value.Y == 0))
				{
					radioTopRight.Checked = true;
				}

				if ((value.X == 0) && (value.Y == _spriteHalf.Y))
				{
					radioMiddleLeft.Checked = true;
				}

				if ((value.X == _spriteHalf.X) && (value.Y == _spriteHalf.Y))
				{
					radioCenter.Checked = true;
				}

				if ((value.X == _spriteSize.Width - 1) && (value.Y == _spriteHalf.Y))
				{
					radioMiddleRight.Checked = true;
				}

				if ((value.X == 0) && (value.Y == _spriteSize.Height - 1))
				{
					radioBottomLeft.Checked = true;
				}

				if ((value.X == _spriteHalf.X) && (value.Y == _spriteSize.Height - 1))
				{
					radioBottomCenter.Checked = true;
				}

				if ((value.X == _spriteSize.Width - 1) && (value.Y == _spriteSize.Height - 1))
				{
					radioBottomRight.Checked = true;
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the CheckedChanged event of the radioTopLeft control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void radioTopLeft_CheckedChanged(object sender, EventArgs e)
		{
			if (_service != null)
			{
				_service.CloseDropDown();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="SpriteAnchorEditor"/> class.
		/// </summary>
		/// <param name="size">The size of the sprite.</param>
		/// <param name="service">The windows forms editor service tied to this control.</param>
		public SpriteAnchorEditor(Size size, IWindowsFormsEditorService service)
			: this()
		{
			_service = service;
			_spriteSize = size;
			_spriteHalf = new Point((int)(size.Width / 2.0f), (int)(size.Height / 2.0f));

			radioTopLeft.Focus();
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="SpriteAnchorEditor"/> class.
		/// </summary>
		public SpriteAnchorEditor()
		{
			DoubleBuffered = true;
			InitializeComponent();
		}
		#endregion
	}
}
