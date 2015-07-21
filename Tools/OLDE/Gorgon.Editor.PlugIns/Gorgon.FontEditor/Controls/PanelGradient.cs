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
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Editor.FontEditorPlugIn.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Fonts;
using Gorgon.Math;
using Gorgon.UI;

namespace Gorgon.Editor.FontEditorPlugIn.Controls
{
	/// <summary>
	/// A panel that will allow for editing of a linear gradient brush.
	/// </summary>
	partial class PanelGradient 
		: UserControl
	{
		#region Classes.
		/// <summary>
		/// Comparer class for sorting the weight nodes.
		/// </summary>
		private class WeightHandleComparer
			: IComparer<WeightHandle>
		{

			#region IComparer<WeightHandle> Members
			/// <summary>
			/// Compares two objects and returns a value indicating whether one is less than, equal to, or greater than the other.
			/// </summary>
			/// <param name="x">The first object to compare.</param>
			/// <param name="y">The second object to compare.</param>
			/// <returns>
			/// A signed integer that indicates the relative values of <paramref name="x" /> and <paramref name="y" />, as shown in the following table.Value Meaning Less than zero<paramref name="x" /> is less than <paramref name="y" />.Zero<paramref name="x" /> equals <paramref name="y" />.Greater than zero<paramref name="x" /> is greater than <paramref name="y" />.
			/// </returns>
			public int Compare(WeightHandle x, WeightHandle y)
			{
				if ((x == null)
				    || (y == null))
				{
					return 0;
				}

				if (x.Weight.EqualsEpsilon(y.Weight))
				{
					return 0;
				}

				return x.Weight < y.Weight ? -1 : 1;
			}
			#endregion
		}

		/// <summary>
		/// A handle for manipulating the weight/color for the gradient.
		/// </summary>
		private class WeightHandle
		{
			#region Variables.
			private readonly PointF[] _trianglePoints;					// Points to draw.
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
			/// <returns><b>true</b> if over the node, <b>false</b> if not.</returns>
			public bool HitTest(Point mouseCursor, Rectangle region)
			{
				float horizontalPosition = (region.Width - 1) * Weight;
				var hitBox = new RectangleF(horizontalPosition - 8, 0, 16, region.Height);

				return hitBox.Contains(mouseCursor);
			}

            /// <summary>
            /// Function to get the offset for dragging.
            /// </summary>
            /// <param name="mouseCursor">Cursor position.</param>
            /// <param name="region">Region containing the node.</param>
            /// <returns>The offset of the node from the cursor.</returns>
		    public int GetDragOffset(Point mouseCursor, Rectangle region)
            {
                return (int)((region.Width - 1) * Weight) - mouseCursor.X;
            }

			/// <summary>
			/// Function to draw the weight node.
			/// </summary>
			/// <param name="g">Graphics context.</param>
			/// <param name="region">Region containing the weight node.</param>
			/// <param name="isInteractive"><b>true</b> to draw as an interactive node, <b>false</b> to draw as a static node.</param>
			/// <param name="isSelected"><b>true</b> to draw the node as selected, <b>false</b> to draw as unselected.</param>
			public void Draw(System.Drawing.Graphics g, Rectangle region, bool isInteractive, bool isSelected)
			{
				float horizontalPosition = (region.Width - 1) * Weight;

				_trianglePoints[0] = new PointF(horizontalPosition - 8, region.Bottom - 1);
				_trianglePoints[1] = new PointF(horizontalPosition, region.Bottom - 9);
				_trianglePoints[2] = new PointF(horizontalPosition + 8, region.Bottom - 1);

				using (Brush brush = new SolidBrush(Color))
				{
					if (isInteractive)
					{
						g.FillPolygon(brush, _trianglePoints);
					}
					else
					{
						g.FillRectangle(brush, new RectangleF(horizontalPosition - 8, region.Bottom - 8, 16, 8));
					}
				}

				Color outlineColor = isSelected ? System.Drawing.Color.Blue : System.Drawing.Color.Black;

				using (var pen = new Pen(outlineColor))
				{
					if (isInteractive)
					{
						g.DrawPolygon(pen, _trianglePoints);
					}
					else
					{
						g.DrawRectangle(pen,
						                new Rectangle((int)horizontalPosition - 8,
						                              region.Bottom - 9,
						                              16,
						                              8));
					}
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
		private bool _scaleAngle;								// Flag to scale the angle.
		private float _angle;									// Angle for the gradient.
		private bool _useGammaCorrection;						// Flag to use gamma correction.
		private readonly WeightHandleComparer _sorter;			// Weight handle sorter.
		private List<WeightHandle> _handles;					// Weight handles.
		private WeightHandle _selectedNode;						// Selected node handle.
		private bool _nodeDrag;									// Flag to indicate that the node is being dragged.
	    private int _nodeDragOffset;							// Node dragging cursor offset.
		private Bitmap _controlPanelImage;						// Image used in the control panel.
		private Bitmap _gradDisplayImage;						// Gradient fill image for the editor panel.
		private Bitmap _gradientImage;							// Gradient fill image for the editor panel.
		private Bitmap _gradPreviewImage;						// Gradient fill image for the preview.
		private Bitmap _selectedColorImage;						// Selected color image.
		#endregion

		#region Events.
		/// <summary>
		/// Event fired when the brush has changed.
		/// </summary>
		[Description("Event fired when the brush has changed.")]
		public event EventHandler BrushChanged;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether to scale the angle for the gradient.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool ScaleAngle
		{
			get
			{
				return IsHandleCreated ? checkScaleAngle.Checked : _scaleAngle;
			}
			set
			{
				_scaleAngle = value;

				if (IsHandleCreated)
				{
					checkScaleAngle.Checked = value;
				}
			}
		}

		/// <summary>
		/// Property to set or return whether to use gamma correction for the gradient.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public bool UseGammaCorrection
		{
			get
			{
				return IsHandleCreated ? checkUseGamma.Checked : _useGammaCorrection;
			}
			set
			{
				_useGammaCorrection = value;

				if (IsHandleCreated)
				{
					checkUseGamma.Checked = value;
				}
			}
		}

		/// <summary>
		/// Property to set or return the angle for the gradient.
		/// </summary>
		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public float Angle
		{
			get
			{
				return IsHandleCreated ? (float)numericAngle.Value : _angle;
			}
			set
			{
				value = value.Min(359.9f).Max(0);

				_angle = value;

				if (IsHandleCreated)
				{
					numericAngle.Value = (decimal)value;
				}
			}
		}
		#endregion

		#region Methods.
        /// <summary>
        /// Function to provide localized control labels.
        /// </summary>
	    private void LocalizeControls()
        {
            itemAddNode.Text = buttonAddNode.Text = Resources.GORFNT_ACC_TEXT_ADD_NODE;
            itemRemoveNode.Text = buttonRemoveNode.Text = Resources.GORFNT_ACC_TEXT_REMOVE_NODE;
            buttonClearNodes.Text = Resources.GORFNT_ACC_TEXT_CLEAR_NODES;
            itemDuplicateNode.Text = buttonDuplicateNode.Text = Resources.GORFNT_ACC_TEXT_DUPLICATE_NODES;
            labelAngle.Text = $"{Resources.GORFNT_TEXT_ANGLE}:";
            labelSelectedWeight.Text = $"{Resources.GORFNT_TEXT_SELECTED_WEIGHT}:";
            labelSelectedColor.Text = $"{Resources.GORFNT_TEXT_SELECTED_COLOR}:";
            checkScaleAngle.Text = Resources.GORFNT_TEXT_SCALE_ANGLE;
            checkUseGamma.Text = Resources.GORFNT_TEXT_USE_GAMMA;
            labelPreview.Text = Resources.GORFNT_TEXT_PREVIEW;
        }

		/// <summary>
		/// Function called when the values on the brush have changed.
		/// </summary>
		private void OnChanged()
		{
			if (BrushChanged == null)
			{
				return;
			}

			BrushChanged(this, EventArgs.Empty);
		}

		/// <summary>
		/// Handles the Click event of the buttonClearNodes control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void buttonClearNodes_Click(object sender, EventArgs e)
		{
			try
			{
				if (GorgonDialogs.ConfirmBox(ParentForm, Resources.GORFNT_DLG_BRUSH_NODES_CLEAR) == ConfirmationResult.No)
				{
					return;
				}

				_handles = new List<WeightHandle>
			           {
				           new WeightHandle(0, Color.Black, 0),
				           new WeightHandle(1, Color.White, 1)
			           };

				_selectedNode = null;

				OnChanged();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				DrawGradientDisplay();
				DrawControls();
				DrawSelectedColor();
				DrawPreview();
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the PreviewKeyDown event of the panelSelectedColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PreviewKeyDownEventArgs"/> instance containing the event data.</param>
		private void panelSelectedColor_PreviewKeyDown(object sender, PreviewKeyDownEventArgs e)
		{
			if ((e.KeyCode != Keys.Enter)
				&& (e.KeyCode != Keys.Space))
			{
				return;
			}

			panelSelectedColor_DoubleClick(sender, EventArgs.Empty);
			e.IsInputKey = true;
		}

		/// <summary>
		/// Function to sort the weight handles.
		/// </summary>
		private void SortWeightHandles()
		{
			if (_handles.Count < 3)
			{
				return;
			}

			_handles.Sort(_sorter);

			for (int i = 0; i < _handles.Count; ++i)
			{
				_handles[i].Index = i;
			}
		}

		/// <summary>
		/// Function to validate the controls.
		/// </summary>
		private void ValidateControls()
		{
			itemRemoveNode.Enabled =
				buttonRemoveNode.Enabled =
				numericSelectedWeight.Enabled = (_selectedNode != null) && (_selectedNode.Index > 0) &&
				                                (_selectedNode.Index < _handles.Count - 1);

			itemDuplicateNode.Enabled = panelSelectedColor.Enabled = buttonDuplicateNode.Enabled = _selectedNode != null;

			buttonClearNodes.Enabled = _handles.Count > 2;

			if (_selectedNode != null)
			{
				numericSelectedWeight.Value = (decimal)_selectedNode.Weight;
			}
			else
			{
				numericSelectedWeight.Value = 0;
			}
		}

		/// <summary>
		/// Function to draw the selected color block.
		/// </summary>
		/// <param name="panelGraphics">Graphics interface for the panel.</param>
		private void DrawSelectedColor(System.Drawing.Graphics panelGraphics = null)
		{
			if (_selectedColorImage == null)
			{
				return;
			}

			using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(_selectedColorImage))
			{
				using (Brush backBrush = new TextureBrush(Resources.Pattern, WrapMode.Tile))
				{
					var region = new Rectangle(0, 0, panelSelectedColor.ClientSize.Width, panelSelectedColor.ClientSize.Height);

					g.CompositingMode = CompositingMode.SourceOver;
					g.FillRectangle(backBrush, region);

					if (_selectedNode != null)
					{
						using (Brush colorBrush = new SolidBrush(_selectedNode.Color))
						{
							g.FillRectangle(colorBrush, region);
						}
					}
				}
			}

			System.Drawing.Graphics graphicsSurface = panelGraphics;

			try
			{
				if (panelGraphics == null)
				{
					graphicsSurface = panelSelectedColor.CreateGraphics();
				}

				graphicsSurface.DrawImage(_selectedColorImage, new Point(0, 0));
			}
			finally
			{
				if ((graphicsSurface != null) && (panelGraphics == null))
				{
					graphicsSurface.Dispose();
				}
			}
		}

		/// <summary>
		/// Function to draw the controls on the control panel.
		/// </summary>
		/// <param name="panelGraphics">Graphics interface for the panel.</param>
		private void DrawControls(System.Drawing.Graphics panelGraphics = null)
		{
			if (_controlPanelImage == null)
			{
				return;
			}

			using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(_controlPanelImage))
			{
				g.Clear(panelGradControls.BackColor);

				_handles[0].Draw(g,
				                 new Rectangle(0, 0, _controlPanelImage.Width, _controlPanelImage.Height),
				                 false,
				                 _selectedNode == _handles[0]);
				_handles[_handles.Count - 1].Draw(g,
				                                  new Rectangle(0, 0, _controlPanelImage.Width, _controlPanelImage.Height),
				                                  false,
				                                  _selectedNode == _handles[_handles.Count - 1]);

				for (int i = 1; i < _handles.Count - 1; ++i)
				{
					_handles[i].Draw(g,
					                 new Rectangle(0, 0, _controlPanelImage.Width, _controlPanelImage.Height),
					                 true,
					                 _selectedNode == _handles[i]);
				}
			}

			System.Drawing.Graphics graphicsSurface = panelGraphics;

			try
			{
				if (panelGraphics == null)
				{
					graphicsSurface = panelGradControls.CreateGraphics();
				}
				
				graphicsSurface.DrawImage(_controlPanelImage, new Point(0, 0));
			}
			finally
			{
				if ((graphicsSurface != null) && (panelGraphics == null))
				{
					graphicsSurface.Dispose();
				}
			}
		}

		/// <summary>
		/// Function to draw the main gradient display.
		/// </summary>
		/// <param name="panelGraphics">Graphics interface for the panel.</param>
		private void DrawGradientDisplay(System.Drawing.Graphics panelGraphics = null)
		{
			if (_gradDisplayImage == null)
			{
				return;
			}

			var region = new Rectangle(0, 0, _gradDisplayImage.Width, _gradDisplayImage.Height);

			using (System.Drawing.Graphics gradLayer = System.Drawing.Graphics.FromImage(_gradientImage))
			{
				using (Brush brush = UpdateBrush(panelGradientDisplay.ClientRectangle, false))
				{
					gradLayer.InterpolationMode = InterpolationMode.High;
					gradLayer.CompositingMode = CompositingMode.SourceOver;
					gradLayer.CompositingQuality = CompositingQuality.HighQuality;
					gradLayer.Clear(Color.Transparent);
					gradLayer.FillRectangle(brush, region);
				}
			}
			
			using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(_gradDisplayImage))
			{
				g.InterpolationMode = InterpolationMode.High;
				g.CompositingMode = CompositingMode.SourceOver;
				g.CompositingQuality = CompositingQuality.HighQuality;

				using (Brush backBrush = new TextureBrush(Resources.Pattern, WrapMode.Tile))
				{
					g.FillRectangle(backBrush, region);
					g.DrawImage(_gradientImage, new Point(0, 0));
					g.DrawRectangle(Pens.Black, new Rectangle(0, 0, region.Width - 1, region.Height - 1));
				}
			}

			System.Drawing.Graphics graphicsSurface = panelGraphics;

			try
			{
				if (panelGraphics == null)
				{
					graphicsSurface = panelGradientDisplay.CreateGraphics();
				}

				graphicsSurface.DrawImage(_gradDisplayImage, new Point(0, 0));
			}
			finally
			{
				if ((graphicsSurface != null) && (panelGraphics == null))
				{
					graphicsSurface.Dispose();
				}
			}
		}

		/// <summary>
		/// Function to draw the preview gradient display.
		/// </summary>
		/// <param name="panelGraphics">Graphics interface for the panel.</param>
		private void DrawPreview(System.Drawing.Graphics panelGraphics = null)
		{
			if (_gradPreviewImage == null)
			{
				return;
			}

			var region = new Rectangle(0, 0, _gradPreviewImage.Width, _gradPreviewImage.Height);

			using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(_gradPreviewImage))
			{
				using (Brush backBrush = new TextureBrush(Resources.Pattern, WrapMode.Tile),
					brush = UpdateBrush(panelPreview.ClientRectangle, true))
				{
					g.FillRectangle(backBrush, region);
					g.FillRectangle(brush, region);
				}
			}

			System.Drawing.Graphics graphicsSurface = panelGraphics;

			try
			{
				if (panelGraphics == null)
				{
					graphicsSurface = panelPreview.CreateGraphics();
				}

				graphicsSurface.DrawImage(_gradPreviewImage, new Point(0, 0));
			}
			finally
			{
				if ((graphicsSurface != null) && (panelGraphics == null))
				{
					graphicsSurface.Dispose();
				}
			}
		}

		/// <summary>
		/// Function to allow for picking of the handle color.
		/// </summary>
		/// <param name="selectedHandle">Selected handle.</param>
		private void GetNodeColor(WeightHandle selectedHandle)
		{
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
				OnChanged();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				DrawControls();
				DrawGradientDisplay();
				DrawPreview();
				DrawSelectedColor();

				if (color != null)
				{
					color.Dispose();
				}
			}
		}

		/// <summary>
		/// Handles the DoubleClick event of the panelSelectedColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void panelSelectedColor_DoubleClick(object sender, EventArgs e)
		{
			if (_selectedNode == null)
			{
				return;
			}

			GetNodeColor(_selectedNode);
		}

		/// <summary>
		/// Handles the ValueChanged event of the numericSelectedWeight control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void numericSelectedWeight_ValueChanged(object sender, EventArgs e)
		{
			if ((_nodeDrag) || (_selectedNode == null))
			{
				return;
			}

			_selectedNode.Weight = (float)numericSelectedWeight.Value;
			SortWeightHandles();
			DrawGradientDisplay();
			DrawControls();
			DrawPreview();
			OnChanged();
		}

		/// <summary>
		/// Handles the CheckedChanged event of the checkUseGamma control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void checkUseGamma_CheckedChanged(object sender, EventArgs e)
		{
			DrawGradientDisplay();
			DrawPreview();
			OnChanged();
		}

		/// <summary>
		/// Handles the Click event of the checkScaleAngle control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void checkScaleAngle_Click(object sender, EventArgs e)
		{
			DrawPreview();
			OnChanged();
		}

		/// <summary>
		/// Handles the ValueChanged event of the numericAngle control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void numericAngle_ValueChanged(object sender, EventArgs e)
		{
			DrawPreview();
			OnChanged();
		}

		/// <summary>
		/// Handles the MouseDoubleClick event of the panelGradientDisplay control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelGradientDisplay_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			if (_gradDisplayImage == null)
			{
				return;
			}

			try
			{
				float weight = (float)e.X / (panelGradientDisplay.ClientSize.Width - 1);

				// If we're sharing a weight with another node, then don't add it.
				if (_handles.Any(item => item.Weight.EqualsEpsilon(weight)))
				{
					return;
				}

				// Get the color of the pixel at the selected point.
				GorgonColor color = _gradientImage.GetPixel(e.X, panelGradientDisplay.ClientSize.Height / 2);

				_selectedNode = new WeightHandle(weight, color, -1);
				_handles.Add(_selectedNode);
				SortWeightHandles();

				OnChanged();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				DrawControls();
				DrawGradientDisplay();
				DrawPreview();
				DrawSelectedColor();
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the Click event of the itemDuplicateNode control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemDuplicateNode_Click(object sender, EventArgs e)
		{
			try
			{
				if ((_selectedNode == null)
				    || (_controlPanelImage == null))
				{
					return;
				}

				float weightPosition = _selectedNode.Weight * _controlPanelImage.Width;

				if (weightPosition < _controlPanelImage.Width - 16)
				{
					weightPosition = (weightPosition + 16.0f) / _controlPanelImage.Width;
				}
				else
				{
					weightPosition = (weightPosition - 16.0f) / _controlPanelImage.Width;
				}

				_selectedNode = new WeightHandle(weightPosition, _selectedNode.Color, -1);
				_handles.Add(_selectedNode);
				SortWeightHandles();

				OnChanged();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				DrawControls();
				DrawGradientDisplay();
				DrawSelectedColor();
				DrawPreview();
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the MouseUp event of the panelGradControls control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelGradControls_MouseUp(object sender, MouseEventArgs e)
		{
			if (!_nodeDrag)
			{
				return;
			}

			if (e.Button != MouseButtons.Left)
			{
				return;
			}

			_nodeDrag = false;
			ValidateControls();
			OnChanged();
		}

		/// <summary>
		/// Handles the Click event of the itemRemoveNode control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemRemoveNode_Click(object sender, EventArgs e)
		{
			// If we right click, then drop the node.
			if (GorgonDialogs.ConfirmBox(ParentForm, Resources.GORFNT_DLG_BRUSH_GRAD_DROP_NODE) == ConfirmationResult.No)
			{
				return;
			}

			try
			{
				_handles.Remove(_selectedNode);
				SortWeightHandles();

				_selectedNode = null;

				OnChanged();
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				DrawControls();
				DrawGradientDisplay();
				DrawSelectedColor();
				DrawPreview();
				ValidateControls();
			}
		}

		/// <summary>
		/// Handles the MouseDown event of the panelBrushPreview control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelGradControls_MouseDown(object sender, MouseEventArgs e)
		{
			if (_nodeDrag)
			{
				return;
			}

			try
			{
				// Select nodes that are not the static nodes first.
				WeightHandle selectedNode = _handles.FirstOrDefault(item =>
				                                                    item.HitTest(e.Location, panelGradControls.ClientRectangle)
				                                                    && item.Index > 0 && item.Index < _handles.Count - 1);


				_selectedNode = selectedNode ?? _handles.FirstOrDefault(item =>
				                                                        item.HitTest(e.Location, panelGradControls.ClientRectangle)
				                                                        && (item.Index == 0 || item.Index <= _handles.Count - 1));

				switch (e.Button)
				{
					case MouseButtons.Left:
						if (_selectedNode == null)
						{
							return;
						}

				        _nodeDragOffset = _selectedNode.GetDragOffset(e.Location, panelGradControls.ClientRectangle);
						_nodeDrag = ((_selectedNode.Index > 0) && (_selectedNode.Index < _handles.Count - 1));
						break;
					case MouseButtons.Right:
						ValidateControls();
						popupNodeEdit.Show(panelGradControls, e.Location);
						break;
				}
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateControls();
				DrawSelectedColor();
				DrawControls();
			}
		}

		/// <summary>
		/// Handles the Click event of the itemAddNode control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
		private void itemAddNode_Click(object sender, EventArgs e)
		{
			float weightPosition = _controlPanelImage.Width / 2.0f;

			if (sender == itemAddNode)
			{
				weightPosition = panelGradControls.PointToClient(new Point(popupNodeEdit.Left, popupNodeEdit.Top)).X;
			}

			// Adjust position to be near the selected node if a node is selected.
			if ((_selectedNode != null)
				&& (_controlPanelImage != null))
			{
				weightPosition = _selectedNode.Weight * _controlPanelImage.Width;

				if (weightPosition < _controlPanelImage.Width - 16)
				{
					weightPosition += 16.0f;
				}
				else
				{
					weightPosition -= 16.0f;
				}
			}

			panelGradientDisplay_MouseDoubleClick(sender, new MouseEventArgs(MouseButtons.Left, 1, (int)weightPosition, 0, 0));
		}

		/// <summary>
		/// Handles the MouseDoubleClick event of the panelBrushPreview control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelGradControls_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			WeightHandle selectedHandle = null;

			foreach (WeightHandle handle in _handles)
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

			GetNodeColor(selectedHandle);
		}

		/// <summary>
		/// Handles the MouseMove event of the panelBrushPreview control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
		private void panelGradControls_MouseMove(object sender, MouseEventArgs e)
		{
			if ((_selectedNode == null) || (e.Button != MouseButtons.Left) || (!_nodeDrag))
			{
				return;
			}

			float newWeight = (float)(e.X + _nodeDragOffset) / (panelGradControls.ClientSize.Width - 1);

			if (newWeight < 0.0f)
			{
				newWeight = 0;
			}

			if (newWeight > 1.0f)
			{
				newWeight = 1.0f;
			}

			_selectedNode.Weight = newWeight;
			SortWeightHandles();

			DrawControls();
			DrawGradientDisplay();
			DrawPreview();

			ValidateControls();
		}

		/// <summary>
		/// Handles the Paint event of the panelBrushPreview control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
		private void panelGradControls_Paint(object sender, PaintEventArgs e)
		{
			DrawControls(e.Graphics);
		}

		/// <summary>
		/// Handles the Paint event of the panelPreview control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
		private void panelPreview_Paint(object sender, PaintEventArgs e)
		{
			DrawPreview(e.Graphics);
		}

		/// <summary>
		/// Handles the Paint event of the panelBrushPreview control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
		private void panelGradientDisplay_Paint(object sender, PaintEventArgs e)
		{
			DrawGradientDisplay(e.Graphics);
		}

		/// <summary>
		/// Function to convert a Gorgon glyph brush into a GDI+ linear gradient brush.
		/// </summary>
		/// <param name="destRect">Destination rectangle.</param>
		/// <param name="rotate"><b>true</b> to rotate the gradient, <b>false</b> to keep it oriented as is.</param>
		private Brush UpdateBrush(RectangleF destRect, bool rotate)
		{
			var linearBrush = new LinearGradientBrush(destRect,
			                                          _handles[0].Color,
			                                          _handles[_handles.Count - 1].Color,
			                                          rotate ? (float)numericAngle.Value : 0,
			                                          checkScaleAngle.Checked)
			                  {
				                  GammaCorrection = checkUseGamma.Checked,
				                  WrapMode = WrapMode.Tile
			                  };

			var interpColors = new ColorBlend(_handles.Count);

			int counter = 0;
			foreach(WeightHandle handle in _handles)
			{
				interpColors.Colors[counter] = handle.Color;
				interpColors.Positions[counter] = handle.Weight;
				++counter;
			}

			linearBrush.InterpolationColors = interpColors;

			return linearBrush;
		}

		/// <summary>
		/// Handles the Paint event of the panelSelectedColor control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
		private void panelSelectedColor_Paint(object sender, PaintEventArgs e)
		{
			DrawSelectedColor(e.Graphics);
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
                LocalizeControls();

				// Create bitmaps for flicker free painting.
				_controlPanelImage = new Bitmap(panelGradControls.ClientSize.Width,
				                                panelGradControls.ClientSize.Height,
				                                PixelFormat.Format32bppArgb);

				_gradDisplayImage = new Bitmap(panelGradientDisplay.ClientSize.Width,
				                               panelGradientDisplay.ClientSize.Height,
				                               PixelFormat.Format32bppArgb);

				_gradientImage = new Bitmap(panelGradientDisplay.ClientSize.Width,
				                            panelGradientDisplay.ClientSize.Height,
				                            PixelFormat.Format32bppArgb);

				_gradPreviewImage = new Bitmap(panelPreview.ClientSize.Width,
				                               panelPreview.ClientSize.Height,
				                               PixelFormat.Format32bppArgb);

				_selectedColorImage = new Bitmap(panelSelectedColor.ClientSize.Width,
				                                 panelSelectedColor.ClientSize.Height,
				                                 PixelFormat.Format32bppArgb);
			}
			catch (Exception ex)
			{
				GorgonDialogs.ErrorBox(ParentForm, ex);
			}
			finally
			{
				ValidateControls();
			}
		}

		/// <summary>
		/// Function to set the interpolation parameters for the brush.
		/// </summary>
		/// <param name="interpolators">The list of interpolation values.</param>
		public void SetInterpolation(ICollection<GorgonGlyphBrushInterpolator> interpolators)
		{
			try
			{
				if ((interpolators == null)
				    || (interpolators.Count == 0))
				{
					_handles = new List<WeightHandle>
					           {
						           new WeightHandle(0, Color.Black, 0),
						           new WeightHandle(1, Color.White, 1)
					           };

					return;
				}

				if (interpolators.Count == 1)
				{
					GorgonGlyphBrushInterpolator first = interpolators.First();

					_handles = new List<WeightHandle>
					           {
						           new WeightHandle(0, first.Color, 0),
						           new WeightHandle(1, Color.White, 1)
					           };
					return;
				}

				_handles.Clear();
				_handles.AddRange(interpolators.OrderBy(item => item.Weight)
				                               .Select((item, index) => new WeightHandle(item.Weight, item.Color, index)));
				SortWeightHandles();
			}
			finally
			{
				if (IsHandleCreated)
				{
					DrawGradientDisplay();
					DrawSelectedColor();
					DrawPreview();
					DrawControls();
					ValidateControls();
				}
			}

		}

		/// <summary>
		/// Function to return the interpolation values for the brush.
		/// </summary>
		/// <returns>A list of interpolators for the brush.</returns>
		public IEnumerable<GorgonGlyphBrushInterpolator> GetInterpolators()
		{
			return _handles.Select(item => new GorgonGlyphBrushInterpolator(item.Weight, item.Color));
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="PanelGradient"/> class.
		/// </summary>
		public PanelGradient()
		{
			InitializeComponent();
			_handles = new List<WeightHandle>
			           {
				           new WeightHandle(0, Color.Black, 0),
				           new WeightHandle(1, Color.White, 1)
			           };
			_sorter = new WeightHandleComparer();
		}
		#endregion
	}
}
