// This control uses the color picker created by Ken Getz.  
// You can check out the article here: http://msdn.microsoft.com/msdnmag/issues/03/07/GDIColorPicker/default.aspx

using System;
using System.Drawing;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;

namespace GorgonLibrary.Graphics.Tools
{
  /// <summary>
  /// Summary description for ColorChooser1.
  /// </summary>
  internal partial class ColorChooser 	  
  {
		private enum ChangeStyle
		{
		  MouseMove,
		  RGB,
		  HSV,
		  None
		}

		private ChangeStyle changeType = ChangeStyle.None;
		private Point selectedPoint;

		private ColorWheel myColorWheel;
		private ColorHandler.RGB RGB;
		private ColorHandler.HSV HSV;
		private bool isInUpdate = false;
	
		private void ColorChooser1_Load(object sender, System.EventArgs e)
		{
		  // Turn on double-buffering, so the form looks better. 
		  this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
		  this.SetStyle(ControlStyles.UserPaint, true);
		  this.SetStyle(ControlStyles.DoubleBuffer, true);


		  // These properties are set in design view, as well, but they
		  // have to be set to false in order for the Paint
		  // event to be able to display their contents.
		  // Never hurts to make sure they're invisible.
		  pnlSelectedColor.Visible = false;
		  pnlBrightness.Visible = false;
		  pnlColor.Visible = false;

		  myColorWheel.ColorChanged += 
			new ColorWheel.ColorChangedEventHandler(
			this.myColorWheel_ColorChanged);

		  // Set the RGB and HSV values 
		  // of the NumericUpDown controls.
		  SetRGB(RGB);
		  SetHSV(HSV);		
		}

	  /// <summary>
	  /// Property to set or return whether to use the alpha component or not.
	  /// </summary>
		  public bool UseAlpha
		  {
			  get
			  {
				  return myColorWheel.UseAlpha;
			  }
			  set
			  {
				  if (value)
				  {
					  pnlSelectedColor.Width = 84;
					  Width = 366;
				  }
				  else
				  {
					  pnlSelectedColor.Width = 34;
					  Width = 317;
				  }
				  myColorWheel.SelectedColorRectangle = new Rectangle(pnlSelectedColor.Location, pnlSelectedColor.Size);
				  panelAlpha.Visible = value;
				  numericAlpha.Visible = value;
				  labelAlpha.Visible = value;
				  myColorWheel.UseAlpha = value;
				  Invalidate();
			  }
		  }

		private void HandleMouse(object sender,  MouseEventArgs e)
		{
		  // If you have the left mouse button down, 
		  // then update the selectedPoint value and 
		  // force a repaint of the color wheel.
		  if ( e.Button == MouseButtons.Left ) 
		  {
			changeType = ChangeStyle.MouseMove;
			selectedPoint = new Point(e.X, e.Y);
			this.Invalidate();
		  }
		}

		private void frmMain_MouseUp( object sender,  MouseEventArgs e)
		{
		  myColorWheel.SetMouseUp();
		  changeType = ChangeStyle.None;
		}

		private void HandleRGBChange(object sender,  System.EventArgs e)
		{

		  // If the R, G, or B values change, use this 
		  // code to update the HSV values and invalidate
		  // the color wheel (so it updates the pointers).
		  // Check the isInUpdate flag to avoid recursive events
		  // when you update the NumericUpdownControls.
		  if (!isInUpdate ) 
		  {
			changeType = ChangeStyle.RGB;
			RGB = new ColorHandler.RGB((int)nudRed.Value, (int)nudGreen.Value, (int)nudBlue.Value);
			SetHSV(ColorHandler.RGBtoHSV(RGB));
			this.Invalidate();
		  }
		}

		private void HandleHSVChange(  object sender,  EventArgs e)  
		{
		  // If the H, S, or V values change, use this 
		  // code to update the RGB values and invalidate
		  // the color wheel (so it updates the pointers).
		  // Check the isInUpdate flag to avoid recursive events
		  // when you update the NumericUpdownControls.
		  if (! isInUpdate ) 
		  {
			changeType = ChangeStyle.HSV;
			HSV = new ColorHandler.HSV((int)(nudHue.Value), (int)(nudSaturation.Value), (int)(nudBrightness.Value));
			SetRGB(ColorHandler.HSVtoRGB(HSV));
			this.Invalidate();
		  }
		}

		private void SetRGB( ColorHandler.RGB RGB) 
		{
		  // Update the RGB values on the form, but don't trigger
		  // the ValueChanged event of the form. The isInUpdate
		  // variable ensures that the event procedures
		  // exit without doing anything.
		  isInUpdate = true;
		  RefreshValue(nudRed, RGB.Red);
		  RefreshValue(nudBlue, RGB.Blue);
		  RefreshValue(nudGreen, RGB.Green);
		  RefreshValue(numericAlpha, myColorWheel.Alpha);
		  isInUpdate = false;
		}

		private void SetHSV(ColorHandler.HSV HSV) 
		{
		  // Update the HSV values on the form, but don't trigger
		  // the ValueChanged event of the form. The isInUpdate
		  // variable ensures that the event procedures
		  // exit without doing anything.
		  isInUpdate = true;
		  RefreshValue(nudHue, HSV.Hue);
		  RefreshValue(nudSaturation, HSV.Saturation);
		  RefreshValue(nudBrightness, HSV.value);
		  RefreshValue(numericAlpha, myColorWheel.Alpha);
		  isInUpdate = false;
		}

		private void HandleTextChanged(object sender, System.EventArgs e)
		{
		  // This step works around a bug -- unless you actively
		  // retrieve the value, the min and max settings for the 
		  // control aren't honored when you type text. This may
		  // be fixed in the 1.1 version, but in VS.NET 1.0, this 
		  // step is required.
		  Decimal x = ((NumericUpDown)sender).Value;
		}

		private void RefreshValue(  NumericUpDown nud,  int value) 
		{
		  // Update the value of the NumericUpDown control, 
		  // if the value is different than the current value.
		  // Refresh the control, causing an immediate repaint.
		  if ( nud.Value != value ) 
		  {
			nud.Value = value;
			nud.Refresh();
		  }
		}

		public Color Color  
		{
			// Get or set the color to be
			// displayed in the color wheel.
			get
			{
				return Color.FromArgb((byte)numericAlpha.Value, (byte)nudRed.Value, (byte)nudGreen.Value, (byte)nudBlue.Value);
			}
			set 
			{
				// Indicate the color change type. Either RGB or HSV
				// will cause the color wheel to update the position
				// of the pointer.
				changeType = ChangeStyle.RGB;
				RGB = new ColorHandler.RGB(value.R, value.G, value.B);
				HSV = ColorHandler.RGBtoHSV(RGB);
				myColorWheel.Alpha = value.A;
			}
		}

		private void myColorWheel_ColorChanged(object sender,  ColorChangedEventArgs e)  
		{
		  SetRGB(e.RGB);
		  SetHSV(e.HSV);
		}

		private void ColorChooser1_Paint(object sender, System.Windows.Forms.PaintEventArgs e)
		{
		  // Depending on the circumstances, force a repaint
		  // of the color wheel passing different information.
		  switch (changeType)
		  {
			case ChangeStyle.HSV:
			  myColorWheel.Draw(e.Graphics, HSV);
			  break;
			case ChangeStyle.MouseMove:
			case ChangeStyle.None:
			  myColorWheel.Draw(e.Graphics, selectedPoint);
			  break;
			case ChangeStyle.RGB:
			  myColorWheel.Draw(e.Graphics, RGB);
			  break;
		  }
	  }

	  /// <summary>
	  /// Handles the ValueChanged event of the numericAlpha control.
	  /// </summary>
	  /// <param name="sender">The source of the event.</param>
	  /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
	  private void numericAlpha_ValueChanged(object sender, EventArgs e)
	  {
		  if (!isInUpdate)
		  {
			  changeType = ChangeStyle.RGB;
			  myColorWheel.Alpha = (byte)numericAlpha.Value;
			  RGB = new ColorHandler.RGB((int)nudRed.Value, (int)nudGreen.Value, (int)nudBlue.Value);
			  SetHSV(ColorHandler.RGBtoHSV(RGB));
			  this.Invalidate();
		  }
	  }
  }
}
