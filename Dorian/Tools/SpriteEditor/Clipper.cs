#region MIT.
// 
// Gorgon.
// Copyright (C) 2007 Michael Winsor
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
// Created: Friday, May 18, 2007 12:38:11 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using Drawing = System.Drawing;
using Dialogs;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Object representing the clipper.
	/// </summary>
	public class Clipper
	{		
		#region Variables.
		private formMain _owner = null;											// Owner form.
		private bool _fixedDimensions;											// Flag to indicate that we're using fixed dimensions.
		private Vector2D _fixedSize = Vector2D.Unit;							// Fixed width and height.
		private bool _keyboardMode;												// Flag to indicate whether we clip using the mouse or keyboard.
		private Drawing.RectangleF _clipRectangle = Drawing.RectangleF.Empty;	// Clipping rectangle.
		private bool _clipStarted;												// Clipping has started.
		private bool _inSelection;												// Flag to indicate that we're in a selection rectangle.
		private Image _patternImage = null;										// Image pattern to use for selection rectangle.
		private TextSprite _helpText = null;									// Help text.
		private TextSprite _helpCaption = null;									// Help text caption.
		private bool _showHelp;													// Flag to show keyboard mode help.
		private Drawing.RectangleF _updateRectangle = Drawing.RectangleF.Empty; // Update rectangle.
		private Vector2D _selectPatternOffset = Vector2D.Zero;					// Selection pattern offset.
		private Vector2D _updatePatternOffset = Vector2D.Zero;					// Update area pattern offset.
		private float _updateAlpha = 96.0f;										// Update color alpha.
		private bool _updateAlphaFlip = false;									// Update color alpha flip flag.
		#endregion

		#region Events.
		/// <summary>
		/// Event fired when an area is selected.
		/// </summary>
		public event EventHandler AreaSelected;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the original rectangle being updated.
		/// </summary>
		public Drawing.RectangleF UpdateRectangle
		{
			get
			{
				return _updateRectangle;
			}
			set
			{
				_updateRectangle = value;
			}
		}

		/// <summary>
		/// Property to set or return whether keyboard mode help is showing.
		/// </summary>
		public bool IsHelpShowing
		{
			get
			{
				return _showHelp;
			}
			set
			{
				_showHelp = value;
			}
		}

		/// <summary>
		/// Property to return the clipping rectangle.
		/// </summary>
		public Drawing.RectangleF ClipRectangle
		{
			get
			{
				return _clipRectangle;
			}
		}

		/// <summary>
		/// Property to set or return whether we're using keyboard or mouse mode.
		/// </summary>
		public bool KeyboardMode
		{
			get
			{
				return _keyboardMode;
			}
			set
			{
				_keyboardMode = value;
			}
		}

		/// <summary>
		/// Property to set or return whether we're using fixed dimensions or not.
		/// </summary>
		public bool IsFixedDimensions
		{
			get
			{
				return _fixedDimensions;
			}
			set
			{
				_fixedDimensions = value;

				Settings.Root = "Clipping";
				Settings.SetSetting("FixedSize", _fixedDimensions.ToString());
				Settings.Root = null;
			}
		}

		/// <summary>
		/// Property to set or return the fixed width and height.
		/// </summary>
		public Vector2D FixedSize
		{
			get
			{
				return _fixedSize;
			}
			set
			{
				_fixedSize = value;

				if (_fixedSize.X < 1.0f)
					_fixedSize.X = 1.0f;
				if (_fixedSize.Y < 1.0f)
					_fixedSize.Y = 1.0f;

				Settings.Root = "Clipping";
				Settings.SetSetting("FixedWidth", _fixedSize.X.ToString("0.0", System.Globalization.CultureInfo.CurrentUICulture));
				Settings.SetSetting("FixedHeight", _fixedSize.Y.ToString("0.0", System.Globalization.CultureInfo.CurrentUICulture));
				Settings.Root = null;
			}
		}

		/// <summary>
		/// Property to return whether clipping has started or not.
		/// </summary>
		public bool IsClippingStarted
		{
			get
			{
				return _clipStarted;
			}
		}

		/// <summary>
		/// Property to return whether we're in the middle of a selection.
		/// </summary>
		public bool IsInSelection
		{
			get
			{
				return _inSelection;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to draw the help for keyboard mode.
		/// </summary>
		public void DrawHelp()
		{
			Gorgon.Screen.FilledRectangle(0, 0, 320, 20, Drawing.Color.Black);
			Gorgon.Screen.FilledRectangle(0, 20, 320, 120, Drawing.Color.FromArgb(200, 0, 0, 128));
			Gorgon.Screen.Rectangle(0, 0, 320, 20, Drawing.Color.White);
			Gorgon.Screen.Rectangle(0, 0, 320, 140, Drawing.Color.White);

			// Draw text.
			Gorgon.CurrentClippingViewport = _helpText.Bounds;
			_helpCaption.SetPosition(2, 2);
			_helpCaption.Draw();
			_helpText.SetPosition(4, 26);
			_helpText.Draw();
			Gorgon.CurrentClippingViewport = null;
		}

		/// <summary>
		/// Function to retrieve the settings for the clipper.
		/// </summary>
		public void GetSettings()
		{
			try
			{
				Settings.Root = "Clipping";
				_fixedDimensions = string.Compare(Settings.GetSetting("FixedSize", "false"), "true", true) == 0;
				_fixedSize = new Vector2D(Convert.ToSingle(Settings.GetSetting("FixedWidth", "16"), System.Globalization.CultureInfo.CurrentUICulture), Convert.ToSingle(Settings.GetSetting("FixedHeight", "16"), System.Globalization.CultureInfo.CurrentUICulture));

				if (_fixedSize.X < 1)
					_fixedSize.X = 1;

				if (_fixedSize.Y < 1)
					_fixedSize.Y = 1;
			}
			catch (Exception ex)
			{
				UI.ErrorBox(_owner, "Unable to retrieve clipper settings.", ex);
			}
			finally
			{
				Settings.Root = null;
				_owner.ValidateForm();
			}
		}
		
		/// <summary>
		/// Function to return the clipping rectangle based on the current cursor position.
		/// </summary>
		/// <param name="cursorPosition">Cursor position.</param>
		/// <returns>The clipping rectangle.</returns>
		public Drawing.RectangleF GetClipDimensions(Vector2D cursorPosition)
		{
			Drawing.RectangleF result = Drawing.RectangleF.Empty;		// Result.

			if ((_inSelection) && (!_fixedDimensions))
			{
				result.Location = _clipRectangle.Location;

				// Flip around if we have negative numbers.
				if (cursorPosition.X < result.X)
				{
					result.Width = result.X - cursorPosition.X;
					result.X = cursorPosition.X;
				}
				else
					result.Width = cursorPosition.X - result.X;

				if (cursorPosition.Y < result.Y)
				{
					result.Height = result.Y - cursorPosition.Y;
					result.Y = cursorPosition.Y;
				}
				else
					result.Height = cursorPosition.Y - result.Y;

				// Make it inclusive.
				result.Width += 1.0f;
				result.Height += 1.0f;

				// Don't allow an empty rectangle.
				if (MathUtility.EqualFloat(result.Width, 0.0f, 0.0999f))
					result.Width = 1.0f;
				if (MathUtility.EqualFloat(result.Height, 0.0f, 0.0999f))
					result.Height = 1.0f;
			}
			else
			{
				result.Location = cursorPosition;				
				if (!_fixedDimensions)
				{
					result.Width = 0;
					result.Height = 0;
				}
				else
					result.Size = new Drawing.SizeF(_fixedSize.X, _fixedSize.Y);
			}

			return result;
		}
		
		/// <summary>
		/// Function to start clipping.
		/// </summary>
		public void StartClipping()
		{
			_clipStarted = true;
			_clipRectangle = Drawing.RectangleF.Empty;
			_showHelp = false;
			_owner.ValidateForm();
			_updateAlpha = 96.0f;
			_updateAlphaFlip = false;
		}

		/// <summary>
		/// Function to cancel the selection rectangle.
		/// </summary>
		public void CancelSelection()
		{
			if (!_fixedDimensions)
			{
				_clipRectangle.Width = 0;
				_clipRectangle.Height = 0;
				_inSelection = false;
			}

			_owner.ValidateForm();
		}

		/// <summary>
		/// Function to end clipping.
		/// </summary>
		public void EndClipping()
		{
			_clipStarted = false;
			_inSelection = false;
		}

		/// <summary>
		/// Function to set a clipping point.
		/// </summary>
		/// <param name="cursorPosition">Cursor position.</param>
		public void SetClipPoint(Vector2D cursorPosition)
		{
			if (!_fixedDimensions)
			{
				if (!_inSelection)
				{
					_clipRectangle.Location = cursorPosition;
					_inSelection = true;
				}
				else
				{
					_clipRectangle = GetClipDimensions(cursorPosition);

					if (AreaSelected != null)
						AreaSelected(this, EventArgs.Empty);
				}
			}
			else
			{
				// Compute the fixed rectangle.
				_clipRectangle.Location = cursorPosition;
				_clipRectangle.Size = new Drawing.SizeF(_fixedSize.X, _fixedSize.Y);

				if (AreaSelected != null)
					AreaSelected(this, EventArgs.Empty);
			}

			_owner.ValidateForm();
		}

		/// <summary>
		/// Function to draw the previous selection rectangle if one exists.
		/// </summary>
		/// <param name="renderTime">Frame delta.</param>
		private void DrawPreviousSelectionRectangle(float renderTime)
		{
			Drawing.RectangleF clipRect = Drawing.RectangleF.Empty;		// Rectangle to draw.
            float scrollRate = -8 * renderTime;                         // Scrolling rate.

			if (_updateRectangle == Drawing.RectangleF.Empty)
				return;
			
			clipRect = _updateRectangle;
			clipRect.Offset(Vector2D.Negate(_owner.ScrollOffset));
			clipRect.Offset(8.0f, 8.0f);

			Gorgon.Screen.BlendingMode = BlendingModes.Modulated;
			Gorgon.Screen.DrawingPattern = _patternImage;
			Gorgon.Screen.WrapMode = ImageAddressing.Wrapping;
            _updatePatternOffset = Vector2D.Add(_updatePatternOffset, new Vector2D(scrollRate, scrollRate));
			if (_updatePatternOffset.X < -1024.0f)
				_updatePatternOffset.X = 0;
			if (_updatePatternOffset.Y < -1024.0f)
				_updatePatternOffset.Y = 0;
			Gorgon.Screen.DrawingPatternOffset = _updatePatternOffset;
			if (!_updateAlphaFlip)
				_updateAlpha = (_updateAlpha + (110.0f * renderTime));
			else
				_updateAlpha = (_updateAlpha - (110.0f * renderTime));
			if ((_updateAlpha > 128.0f) || (_updateAlpha < 96.0f))
			{
				_updateAlphaFlip = !_updateAlphaFlip;
				if (_updateAlpha >= 128.0f)
					_updateAlpha = 128.0f;
				if (_updateAlpha <= 96.0f)
					_updateAlpha = 96.0f;
			}
			Gorgon.Screen.Rectangle(clipRect.X, clipRect.Y, clipRect.Width, clipRect.Height, Drawing.Color.FromArgb((byte)_updateAlpha, Drawing.Color.LightBlue));
			Gorgon.Screen.FilledRectangle(clipRect.X, clipRect.Y, clipRect.Width, clipRect.Height, Drawing.Color.FromArgb((byte)_updateAlpha, 0, 64, 192));
			Gorgon.Screen.DrawingPattern = null;
		}

		/// <summary>
		/// Function to draw the selection rectangle.
		/// </summary>
		/// <param name="cursorPosition">Cursor position of the selection rectangle.</param>
		/// <param name="renderTime">Frame delta.</param>
		private void DrawSelectionRectangle(Vector2D cursorPosition, float renderTime)
		{
			Drawing.RectangleF clipRect = Drawing.RectangleF.Empty;		// Rectangle to draw.
            float scrollRate = 16 * renderTime;                         // Scrolling rate.

			if ((!_inSelection) && (!_fixedDimensions))
				return;

			// Get the visible clipping region.
			clipRect = GetClipDimensions(Vector2D.Add(cursorPosition, _owner.ScrollOffset));

			// Convert to screen coordinates.
			clipRect.Offset(Vector2D.Negate(_owner.ScrollOffset));

			Gorgon.Screen.BlendingMode = BlendingModes.Modulated;
			Gorgon.Screen.DrawingPattern = _patternImage;
			Gorgon.Screen.WrapMode = ImageAddressing.Wrapping;
            _selectPatternOffset = Vector2D.Add(_selectPatternOffset, new Vector2D(scrollRate, scrollRate));
			if (_selectPatternOffset.X > 1024.0f)
				_selectPatternOffset.X = 0;
			if (_selectPatternOffset.Y > 1024.0f)
				_selectPatternOffset.Y = 0;
			Gorgon.Screen.DrawingPatternOffset = _selectPatternOffset; 
			Gorgon.Screen.Rectangle(clipRect.X, clipRect.Y, clipRect.Width, clipRect.Height, Drawing.Color.Red);
			Gorgon.Screen.FilledRectangle(clipRect.X, clipRect.Y, clipRect.Width, clipRect.Height, Drawing.Color.FromArgb(128, 128, 0, 0));
			Gorgon.Screen.DrawingPattern = null;
		}

		/// <summary>
		/// Function to draw the clipping rectangle cursor.
		/// </summary>
		/// <param name="cursorPosition">Position of the clip cursor.</param>
		/// <param name="renderTime">Frame delta.</param>
		public void DrawCursor(Vector2D cursorPosition, float renderTime)
		{
			if (!_clipStarted)
				return;

			try
			{
				Drawing.Color lineColor;		// Line color.

				lineColor = Drawing.Color.FromArgb(192, Drawing.Color.White);

				// Draw the cursor lines.
				Gorgon.Screen.BlendingMode = BlendingModes.Inverted;

				// Don't draw the upper portion if we're clipping.
				if ((!_inSelection) && (!_fixedDimensions))
				{
					Gorgon.Screen.VerticalLine(cursorPosition.X, 0, cursorPosition.Y, lineColor);
					Gorgon.Screen.HorizontalLine(0, cursorPosition.Y, cursorPosition.X, lineColor);
				}
				Gorgon.Screen.VerticalLine(cursorPosition.X, cursorPosition.Y + 1, (_owner.BackBuffer.Height) - cursorPosition.Y + 1, lineColor);
				Gorgon.Screen.HorizontalLine(cursorPosition.X + 1, cursorPosition.Y, (_owner.BackBuffer.Width) - cursorPosition.X + 1, lineColor);
				Gorgon.Screen.BlendingMode = BlendingModes.Modulated;

				// Draw the selection rectangle.
				DrawPreviousSelectionRectangle(renderTime);
				DrawSelectionRectangle(cursorPosition, renderTime);
			}
			catch (Exception ex)
			{
				UI.ErrorBox(_owner, "Unable to draw the cursor.", ex);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Form that owns this object.</param>
		public Clipper(formMain owner)
		{
			_owner = owner;
			_helpCaption = new TextSprite("Caption", string.Empty, formMain.MainFont);
			_helpCaption.Color = Drawing.Color.White;
			_helpCaption.Text = "Keyboard mode key list...";

			_helpText = new TextSprite("HelpText", string.Empty, formMain.MainFont);
			_helpText.Color = Drawing.Color.White;
			_helpText.Bounds = new Viewport(1, 1, 318, 138);
			_helpText.WordWrap = true;

			_helpText.Text = "SPACE - Toggle keyboard/mouse mode.\nENTER - Start/finish selection.\nESC - Stop selecting if selection box is active, stop clipping if no selection box.\nArrow keys - Move cursor by 1 pixel.\nHold shift to move by 100 pixels.\nHold CTRL to move by 10 pixels.";

			// Get the pattern image.
			_patternImage = ImageCache.Images["Pattern"];
		}
		#endregion
	}
}
