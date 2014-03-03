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
// Created: Sunday, March 2, 2014 8:52:44 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fetze.WinFormsColor;
using GorgonLibrary.Graphics;
using GorgonLibrary.Graphics.Fonts;
using GorgonLibrary.Math;
using GorgonLibrary.UI;

namespace GorgonLibrary.Editor.FontEditorPlugIn.Controls
{
	/// <summary>
	/// A panel that will allow for editing of a linear gradient brush.
	/// </summary>
	// ReSharper disable once InconsistentNaming
	partial class panelGradient 
		: UserControl
	{
		#region Classes.
		/// <summary>
		/// A handle for manipulating the weight/color for the gradient.
		/// </summary>
		private class WeightHandle
		{
			#region Variables.
			private readonly PointF[] _trianglePoints;					// Points to draw.
			private Color _outlineColor = System.Drawing.Color.Black;	// Outline color.
			#endregion

			#region Properties.
			/// <summary>
			/// Property to return the index of the interpolation value.
			/// </summary>
			public int Index
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the color for the weight node.
			/// </summary>
			public GorgonColor Color
			{
				get;
				set;
			}

			/// <summary>
			/// Property to set or return the weight for the node.
			/// </summary>
			public float Weight
			{
				get;
				set;
			}
			#endregion

			#region Methods.
			/// <summary>
			/// Function to determine if the mouse is over the node.
			/// </summary>
			/// <param name="mouseCursor">Cursor position in client space.</param>
			/// <param name="region">The region containing the weight node.</param>
			/// <returns>TRUE if over the node, FALSE if not.</returns>
			public bool HitTest(Point mouseCursor, Rectangle region)
			{
				float horizontalPosition = (region.Width - 1) * Weight;
				var hitBox = new RectangleF(horizontalPosition - 8, 0, 16, region.Height);

				if (hitBox.Contains(mouseCursor))
				{
					_outlineColor = System.Drawing.Color.Blue;
					return true;
				}

				_outlineColor = System.Drawing.Color.Black;
				return false;
			}

			/// <summary>
			/// Function to draw the weight node.
			/// </summary>
			/// <param name="g">Graphics context.</param>
			/// <param name="region">Region containing the weight node.</param>
			public void Draw(System.Drawing.Graphics g, Rectangle region)
			{
				float horizontalPosition = (region.Width - 1) * Weight;

				horizontalPosition = horizontalPosition.Max(1).Min(region.Right - 2);

				_trianglePoints[0] = new PointF(horizontalPosition - 8, region.Bottom - 1);
				_trianglePoints[1] = new PointF(horizontalPosition, region.Bottom - 9);
				_trianglePoints[2] = new PointF(horizontalPosition + 8, region.Bottom - 1);

				using (Brush brush = new SolidBrush(Color))
				{
					g.FillPolygon(brush, _trianglePoints);
				}

				using (var pen = new Pen(_outlineColor))
				{
					g.DrawPolygon(pen, _trianglePoints);
					g.DrawLine(pen, horizontalPosition, 0, horizontalPosition, region.Height - 9);
				}
			}
			#endregion

			#region Constructor/Destructor.
			/// <summary>
			/// Initializes a new instance of the <see cref="WeightHandle"/> class.
			/// </summary>
			/// <param name="weight">The weight of the node.</param>
			/// <param name="color">Color for the node.</param>
			/// <param name="index">The index of the corresponding interpolation value.</param>
			public WeightHandle(float weight, GorgonColor color, int index)
			{
				Weight = weight;
				Color = color;
				Index = index;

				_trianglePoints = new PointF[3];
			}
			#endregion
		}
		#endregion

		#region Variables.
		private GorgonGlyphLinearGradientBrush _glyphBrush;		// Glyph brush.
		private List<WeightHandle> _handles;					// Weight handles.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the brush to use when drawing the gradient.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public GorgonGlyphLinearGradientBrush Brush
		{
			get
			{
				return _glyphBrush;
			}
			set
			{
				// Do not set this value at design time.
				if (DesignMode)
				{
					return;
				}

				if (value == null)
				{
					// If we don't have a brush set up, then use a default black -> white gradient.
					GetDefaultBrush();
				}
				else
				{
					_glyphBrush = value;

					if (_glyphBrush.Interpolation.Count > 1)
					{
						_handles.AddRange(_glyphBrush.Interpolation.Select((item, index) => new WeightHandle(item.Weight, item.Color, index)));
					}
					else
					{
						_handles = new List<WeightHandle>
						           {
									   new WeightHandle(0, _glyphBrush.StartColor, 0), 
									   new WeightHandle(1, _glyphBrush.EndColor, 1)
						           };
					}
				}

				panelBrushGradient.Invalidate();
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the Click event of the checkScaleAngle control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void checkScaleAngle_Click(object sender, EventArgs e)
		{
			Brush.ScaleAngle = checkScaleAngle.Checked;
			panelPreview.Invalidate();
		}

		/// <summary>
		/// Handles the ValueChanged event of the numericAngle control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void numericAngle_ValueChanged(object sender, EventArgs e)
		{
			Brush.Angle = (float)numericAngle.Value;
			panelPreview.Invalidate();
		}

		/// <summary>
		/// Handles the MouseDoubleClick event of the panelGradientDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelGradientDisplay_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			float weight = (float)e.X / panelGradientDisplay.ClientSize.Width;
			GorgonColor color = weight <= 0.5f ? _handles[0].Color : _handles[_handles.Count - 1].Color;

			// If we're at the end or beginning, do nothing.
			if (_handles.Any(item => item.Weight.EqualsEpsilon(weight)))
			{
				return;
			}

			_handles.Add(new WeightHandle(weight, color, -1));

			Brush.Interpolation.Clear();
			
			foreach(WeightHandle handle in _handles.OrderBy(item => item.Weight))
			{
				handle.Index = Brush.Interpolation.Count;
				Brush.Interpolation.Add(new GorgonGlyphBrushInterpolator(handle.Weight, handle.Color));
			}

			panelBrushGradient.Invalidate(true);
			panelPreview.Invalidate();
			panelSelectedColor.Invalidate();
		}

		/// <summary>
		/// Handles the MouseDown event of the panelBrushPreview control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelBrushPreview_MouseDown(object sender, MouseEventArgs e)
		{

		}

		/// <summary>
		/// Handles the MouseDoubleClick event of the panelBrushPreview control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelBrushPreview_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			WeightHandle selectedHandle = null;

			foreach (WeightHandle handle in _handles.OrderBy(item => item.Weight))
			{
				if (handle.HitTest(e.Location, panelGradControls.ClientRectangle))
				{
					selectedHandle = handle;
				}
			}

			if (selectedHandle == null)
			{
				return;
			}

			ColorPickerDialog color = null;

			try
			{
				color = new ColorPickerDialog
				        {
							SelectedColor = selectedHandle.Color,
							OldColor = selectedHandle.Color,
							AlphaEnabled = true
				        };

				if (color.ShowDialog(ParentForm) != DialogResult.OK)
				{
					return;
				}

				selectedHandle.Color = color.SelectedColor;

				if (Brush.Interpolation.Count > 1)
				{
					Brush.Interpolation[selectedHandle.Index] = new GorgonGlyphBrushInterpolator(selectedHandle.Weight, color.SelectedColor);
				}
				else
				{
					if (selectedHandle.Index == 0)
					{
						Brush.StartColor = color.SelectedColor;
					}
					else
					{
						Brush.EndColor = color.SelectedColor;
					}
				}

				panelBrushGradient.Invalidate(true);
				panelPreview.Invalidate();
				panelSelectedColor.Invalidate();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				if (color != null)
				{
					color.Dispose();
				}
			}
		}

		/// <summary>
		/// Handles the MouseMove event of the panelBrushPreview control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelBrushPreview_MouseMove(object sender, MouseEventArgs e)
		{
			foreach (WeightHandle handle in _handles)
			{
				handle.HitTest(e.Location, panelGradControls.ClientRectangle);
			}
		}

		/// <summary>
		/// Function to retrieve a default linear gradient brush.
		/// </summary>
		/// <returns>The default linear gradient brush.</returns>
		private void GetDefaultBrush()
		{
			_glyphBrush = new GorgonGlyphLinearGradientBrush
			              {
				              StartColor = Color.Black,
				              EndColor = Color.White
			              };
			_handles = new List<WeightHandle>
			           {
						   new WeightHandle(0, Color.Black, 0), 
						   new WeightHandle(1, Color.White, 1)
			           };
		}

		/// <summary>
		/// Handles the Paint event of the panelBrushPreview control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
		private void panelBrushPreview_Paint(object sender, PaintEventArgs e)
		{
			foreach (WeightHandle handle in _handles.OrderBy(item => item.Weight))
			{
				handle.Draw(e.Graphics, panelGradControls.ClientRectangle);
			}
		}

		/// <summary>
		/// Handles the Paint event of the panelPreview control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
		private void panelPreview_Paint(object sender, PaintEventArgs e)
		{
			if (Brush == null)
			{
				return;
			}

			using (Brush brush = ConvertBrush(panelPreview.ClientRectangle, true))
			{
				e.Graphics.FillRectangle(brush, panelPreview.ClientRectangle);
			}
		}


		/// <summary>
		/// Handles the Paint event of the panelBrushPreview control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
		private void panelGradientDisplay_Paint(object sender, PaintEventArgs e)
		{
			// Don't paint if we haven't converted our brush yet.
			if (Brush == null)
			{
				return;
			}

			using (Brush brush = ConvertBrush(panelGradientDisplay.ClientRectangle, false))
			{
				e.Graphics.FillRectangle(brush, panelGradientDisplay.ClientRectangle);
				e.Graphics.DrawRectangle(Pens.Black, new Rectangle(0, 0, panelGradientDisplay.ClientRectangle.Width - 1, panelGradientDisplay.ClientRectangle.Height - 1));
			}
		}

		/// <summary>
		/// Function to convert a Gorgon glyph brush into a GDI+ linear gradient brush.
		/// </summary>
		/// <param name="destRect">Destination rectangle.</param>
		/// <param name="rotate">TRUE to rotate the gradient, FALSE to keep it oriented as is.</param>
		private Brush ConvertBrush(Rectangle destRect, bool rotate)
		{
			var panelRect = new Rectangle(0, 0,destRect.Width, destRect.Height - 16);
			var linearBrush = new LinearGradientBrush(panelRect, Brush.StartColor, Brush.EndColor, rotate ? Brush.Angle : 0, rotate)
			{
				GammaCorrection = Brush.GammaCorrection,
				WrapMode = Brush.WrapMode
			};

			if (Brush.LinearColors.Count > 0)
			{
				linearBrush.LinearColors = Brush.LinearColors.Select(item => item.ToColor()).ToArray();
			}

			if (Brush.Interpolation.Count == 0)
			{
				return linearBrush;
			}

			var interpColors = new ColorBlend(Brush.Interpolation.Count);

			for (int i = 0; i < Brush.Interpolation.Count; i++)
			{
				interpColors.Colors[i] = Brush.Interpolation[i].Color;
				interpColors.Positions[i] = Brush.Interpolation[i].Weight;
			}

			linearBrush.InterpolationColors = interpColors;

			return linearBrush;
		}

		/// <summary>
		/// Raises the <see cref="E:System.Windows.Forms.UserControl.Load" /> event.
		/// </summary>
		/// <param name="e">An <see cref="T:System.EventArgs" /> that contains the event data.</param>
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			try
			{
				if (Brush == null)
				{
					GetDefaultBrush();
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="panelGradient"/> class.
		/// </summary>
		public panelGradient()
		{
			InitializeComponent();
			_handles = new List<WeightHandle>();
		}
		#endregion
	}
}
