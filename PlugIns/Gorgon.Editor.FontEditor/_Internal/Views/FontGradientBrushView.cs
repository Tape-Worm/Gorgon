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
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Editor.FontEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics;
using Gorgon.Graphics.Fonts;
using Gorgon.Math;
using Gorgon.Memory;
using Gorgon.UI;

namespace Gorgon.Editor.FontEditor
{
    /// <summary>
    /// A panel that will allow for editing of a linear gradient brush.
    /// </summary>
    internal partial class FontGradientBrushView 
        : EditorSubPanelCommon, IDataContext<IFontGradientBrush>
    {
        #region Variables.
        // Flag to indicate that the node is being dragged.
        private bool _nodeDrag;
        // Node dragging cursor offset.
        private int _nodeDragOffset;
        // Image used in the control panel.
        private Bitmap _controlPanelImage;
        // Gradient fill image for the editor panel.
        private Bitmap _gradDisplayImage;
        // Gradient fill image for the editor panel.
        private Bitmap _gradientImage;
        // Gradient fill image for the preview.
        private Bitmap _gradPreviewImage;
        // The lock used to prevent event reentrancy.
        private int _eventLock;
        // Points for drawing a triangle.
        private readonly PointF[] _trianglePoints = new PointF[3];
        #endregion

        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        public IFontGradientBrush DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs" /> instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            UnhookEvents();

            switch (e.PropertyName)
            {
                case nameof(IFontGradientBrush.UseGamma):
                    CheckUseGamma.Checked = DataContext.UseGamma;
                    break;
                case nameof(IFontGradientBrush.ScaleAngle):
                    CheckScaleAngle.Checked = DataContext.ScaleAngle;
                    break;
                case nameof(IFontGradientBrush.Angle):
                    NumericAngle.Value = (decimal)DataContext.Angle;
                    break;
                case nameof(IFontGradientBrush.SelectedNode):
                    if (DataContext.SelectedNode is not null)
                    {
                        PickerNodeColor.SelectedColor = PickerNodeColor.OriginalColor = DataContext.SelectedNode.Color;
                        NumericSelectedWeight.Value = (decimal)DataContext.SelectedNode.Weight;
                    }
                    else
                    {
                        PickerNodeColor.SelectedColor = PickerNodeColor.OriginalColor = GorgonColor.BlackTransparent;
                        NumericSelectedWeight.Value = 0M;
                    }
                    break;
            }

            DrawPreview();
            DrawGradientDisplay();
            DrawControls();
            ValidateControls();
            HookEvents();            
        }

        /// <summary>Handles the ColorChanged event of the PickerNodeColor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="ColorChangedEventArgs" /> instance containing the event data.</param>
        private void PickerNodeColor_ColorChanged(object sender, ColorChangedEventArgs e)
        {
            if ((DataContext?.SetColorCommand is null) || (!DataContext.SetColorCommand.CanExecute(e.Color)))
            {
                return;
            }

            DataContext.SetColorCommand.Execute(e.Color);

            DrawGradientDisplay();
            DrawControls();
            DrawPreview();
            ValidateControls();
        }

        /// <summary>
        /// Handles the Click event of the buttonClearNodes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonClearNodes_Click(object sender, EventArgs e)
        {
            if ((DataContext?.ClearNodesCommand is null) || (!DataContext.ClearNodesCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.ClearNodesCommand.Execute(null);

            DrawGradientDisplay();
            DrawControls();
            DrawPreview();
            ValidateControls();
        }

        /// <summary>
        /// Function to validate the controls.
        /// </summary>
        private void ValidateControls()
        {
            if (DataContext is null)
            {
                TableControls.Enabled = false;
                return;
            }
            else
            {
                TableControls.Enabled = true;
            }

            ItemRemoveNode.Enabled = ButtonRemoveNode.Enabled = DataContext.DeleteNodeCommand?.CanExecute(null) ?? false;
            NumericSelectedWeight.Enabled = DataContext.SetWeightCommand?.CanExecute((float)NumericSelectedWeight.Value) ?? false;
            ItemDuplicateNode.Enabled = ButtonDuplicateNode.Enabled = DataContext.DuplicateNodeCommand?.CanExecute(null) ?? false;
            ButtonClearNodes.Enabled = DataContext.ClearNodesCommand?.CanExecute(null) ?? false;
            LabelNodeColor.Visible = PickerNodeColor.Visible = DataContext.SetColorCommand?.CanExecute(PickerNodeColor.SelectedColor) ?? false;

            ValidateOk();
        }


        /// <summary>
        /// Function to draw the weight node.
        /// </summary>
        /// <param name="handle">The handle to draw.</param>
        /// <param name="g">Graphics context.</param>
        /// <param name="region">Region containing the weight node.</param>
        /// <param name="isInteractive">TRUE to draw as an interactive node, FALSE to draw as a static node.</param>
        private void DrawHandle(WeightHandle handle, System.Drawing.Graphics g, Rectangle region, bool isInteractive)
        {
            Debug.Assert(DataContext is not null, "No data context. Should have one here");

            float horizontalPosition = (region.Width - 1) * handle.Weight;

            _trianglePoints[0] = new PointF(horizontalPosition - 8, region.Bottom - 1);
            _trianglePoints[1] = new PointF(horizontalPosition, region.Bottom - 9);
            _trianglePoints[2] = new PointF(horizontalPosition + 8, region.Bottom - 1);

            using (Brush brush = new SolidBrush(handle.Color))
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

            Color outlineColor = DataContext.SelectedNode == handle ? Color.Blue : Color.Black;

            using var pen = new Pen(outlineColor);
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

        /// <summary>
        /// Function to draw the controls on the control panel.
        /// </summary>
        /// <param name="panelGraphics">Graphics interface for the panel.</param>
        private void DrawControls(System.Drawing.Graphics panelGraphics = null)
        {
            if ((_controlPanelImage is null) || (DataContext is null) || (DataContext.Nodes.Count == 0))
            {
                return;
            }
            
            using (var g = System.Drawing.Graphics.FromImage(_controlPanelImage))
            {
                g.Clear(PanelGradControls.BackColor);

                DrawHandle(DataContext.Nodes[0], g, new Rectangle(0, 0, _controlPanelImage.Width, _controlPanelImage.Height), false);
                DrawHandle(DataContext.Nodes[^1], g, new Rectangle(0, 0, _controlPanelImage.Width, _controlPanelImage.Height), false);

                for (int i = 1; i < DataContext.Nodes.Count - 1; ++i)
                {
                    DrawHandle(DataContext.Nodes[i], g, new Rectangle(0, 0, _controlPanelImage.Width, _controlPanelImage.Height), true);
                }
            }

            System.Drawing.Graphics graphicsSurface = panelGraphics;

            try
            {
                if (panelGraphics is null)
                {
                    graphicsSurface = PanelGradControls.CreateGraphics();
                }
                
                graphicsSurface.DrawImage(_controlPanelImage, new Point(0, 0));
            }
            finally
            {
                if ((graphicsSurface is not null) && (panelGraphics is null))
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
            if ((_gradDisplayImage is null) || (DataContext is null))
            {
                return;
            }

            var region = new Rectangle(0, 0, _gradDisplayImage.Width, _gradDisplayImage.Height);

            using (var gradLayer = System.Drawing.Graphics.FromImage(_gradientImage))
            {
                using Brush brush = UpdateBrush(PanelGradientDisplay.ClientRectangle, false);

                if (brush is null)
                {
                    gradLayer.FillRectangle(Brushes.Red, region);
                }
                else
                {
                    gradLayer.InterpolationMode = InterpolationMode.High;
                    gradLayer.CompositingMode = CompositingMode.SourceOver;
                    gradLayer.CompositingQuality = CompositingQuality.HighQuality;
                    gradLayer.Clear(Color.Transparent);
                    gradLayer.FillRectangle(brush, region);
                }
            }
            
            using (var g = System.Drawing.Graphics.FromImage(_gradDisplayImage))
            {
                g.InterpolationMode = InterpolationMode.High;
                g.CompositingMode = CompositingMode.SourceOver;
                g.CompositingQuality = CompositingQuality.HighQuality;

                using Brush backBrush = new TextureBrush(Resources.Transparency_Pattern, WrapMode.Tile);
                g.FillRectangle(backBrush, region);
                g.DrawImage(_gradientImage, region);
                g.DrawRectangle(Pens.Black, new Rectangle(0, 0, region.Width - 1, region.Height - 1));
            }

            System.Drawing.Graphics graphicsSurface = panelGraphics;

            try
            {
                if (panelGraphics is null)
                {
                    graphicsSurface = PanelGradientDisplay.CreateGraphics();
                }

                graphicsSurface.DrawImage(_gradDisplayImage, new Point(0, 0));
            }
            finally
            {
                if ((graphicsSurface is not null) && (panelGraphics is null))
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
            if ((_gradPreviewImage is null) || (DataContext is null))
            {
                return;
            }

            Rectangle region = new(0, 0, _gradPreviewImage.Width, _gradPreviewImage.Height);

            using (var g = System.Drawing.Graphics.FromImage(_gradPreviewImage))
            {
                using Brush backBrush = new TextureBrush(Resources.Transparency_Pattern, WrapMode.Tile);
                using Brush brush = UpdateBrush(PanelPreview.ClientRectangle, true);

                if (brush is null)
                {
                    g.FillRectangle(Brushes.Red, region);
                }
                else
                {
                    g.FillRectangle(backBrush, region);
                    g.FillRectangle(brush, region);
                }
            }

            System.Drawing.Graphics graphicsSurface = panelGraphics;

            try
            {
                if (panelGraphics is null)
                {
                    graphicsSurface = PanelPreview.CreateGraphics();
                }

                graphicsSurface.DrawImage(_gradPreviewImage, new Point(0, 0));
            }
            finally
            {
                if ((graphicsSurface is not null) && (panelGraphics is null))
                {
                    graphicsSurface.Dispose();
                }
            }
        }

        /// <summary>
        /// Handles the ValueChanged event of the numericSelectedWeight control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericSelectedWeight_ValueChanged(object sender, EventArgs e)
        {
            if ((_nodeDrag) || (DataContext?.SetWeightCommand is null) || (!DataContext.SetWeightCommand.CanExecute((float)NumericSelectedWeight.Value)))
            {
                return;
            }

            DataContext.SetWeightCommand.Execute((float)NumericSelectedWeight.Value);

            DrawGradientDisplay();
            DrawControls();
            DrawPreview();
            ValidateControls();
        }

        /// <summary>
        /// Handles the CheckedChanged event of the checkUseGamma control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CheckUseGamma_CheckedChanged(object sender, EventArgs e)
        {
            if (DataContext is null)
            {
                return;
            }

            DataContext.UseGamma = CheckUseGamma.Checked;
            
            DrawGradientDisplay();
            DrawPreview();
            ValidateControls();
        }

        /// <summary>
        /// Handles the Click event of the checkScaleAngle control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CheckScaleAngle_Click(object sender, EventArgs e)
        {
            if (DataContext is null)
            {
                return;
            }

            DataContext.ScaleAngle = CheckScaleAngle.Checked;
            
            DrawPreview();            
            ValidateControls();
        }

        /// <summary>
        /// Handles the ValueChanged event of the numericAngle control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericAngle_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext is null)
            {
                return;
            }

            DataContext.Angle = (float)NumericAngle.Value;
            DrawPreview();            
        }

        /// <summary>
        /// Handles the MouseDoubleClick event of the panelGradientDisplay control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void PanelGradientDisplay_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if ((DataContext is null) || (_gradDisplayImage is null))
            {
                return;
            }

            float weight = (float)e.X / (PanelGradientDisplay.ClientSize.Width - 1);
            // Get the color of the pixel at the selected point.
            GorgonColor color = _gradientImage.GetPixel(e.X, PanelGradientDisplay.ClientSize.Height / 2);

            if ((DataContext.AddNodeCommand is not null) && (DataContext.AddNodeCommand.CanExecute((weight, color))))
            {
                DataContext.AddNodeCommand.Execute((weight, color));
            }

            DrawControls();
            DrawGradientDisplay();
            DrawPreview();
            ValidateControls();
        }

        /// <summary>
        /// Handles the Click event of the itemDuplicateNode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ItemDuplicateNode_Click(object sender, EventArgs e)
        {
            if ((DataContext?.DuplicateNodeCommand is null) || (!DataContext.DuplicateNodeCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.DuplicateNodeCommand.Execute(null);

            DrawControls();
            DrawGradientDisplay();
            DrawPreview();
            ValidateControls();
        }

        /// <summary>
        /// Handles the MouseUp event of the panelGradControls control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void PanelGradControls_MouseUp(object sender, MouseEventArgs e)
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
        }

        /// <summary>
        /// Handles the Click event of the itemRemoveNode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ItemRemoveNode_Click(object sender, EventArgs e)
        {
            if ((DataContext?.DeleteNodeCommand is null) || (!DataContext.DeleteNodeCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.DeleteNodeCommand.Execute(null);
            DrawControls();
            DrawGradientDisplay();
            DrawPreview();
            ValidateControls();
        }

        /// <summary>
        /// Handles the MouseDown event of the panelBrushPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void PanelGradControls_MouseDown(object sender, MouseEventArgs e)
        {
            if ((_nodeDrag) || (DataContext is null))
            {
                return;
            }

            // Function to determine if the mouse is over the node.
            static bool HitTest(WeightHandle node, Point mouseCursor, Rectangle region)
            {
                float horizontalPosition = (region.Width - 1) * node.Weight;

                var hitBox = new RectangleF(horizontalPosition - 8, 0, 16, region.Height);

                return hitBox.Contains(mouseCursor);
            }

            // Select nodes that are not the static nodes first.
            WeightHandle selectedNode = DataContext.Nodes.FirstOrDefault(item => HitTest(item, e.Location, PanelGradControls.ClientRectangle) && (item.Index > 0) && (item.Index < DataContext.Nodes.Count - 1));

            // If we didn't select a node that can be dragged, then check the first/last nodes for a hit.
            if (selectedNode is null)
            {
                selectedNode = HitTest(DataContext.Nodes[0], e.Location, PanelGradControls.ClientRectangle) ? DataContext.Nodes[0] : null;

                if (selectedNode is null)
                {
                    selectedNode = HitTest(DataContext.Nodes[^1], e.Location, PanelGradControls.ClientRectangle) ? DataContext.Nodes[^1] : null;
                }
            }

            switch (e.Button)
            {
                case MouseButtons.Left:
                    if ((DataContext.SelectNodeCommand is not null) && (DataContext.SelectNodeCommand.CanExecute(selectedNode)))
                    {
                        DataContext.SelectNodeCommand.Execute(selectedNode);
                    }

                    if (DataContext.SelectedNode is not null)
                    {
                        _nodeDragOffset = (int)((PanelGradControls.Width - 1) * DataContext.SelectedNode.Weight) - e.Location.X;
                        _nodeDrag = ((DataContext.SelectedNode.Index > 0) && (DataContext.SelectedNode.Index < DataContext.Nodes.Count - 1));
                    }
                    else
                    {
                        _nodeDragOffset = 0;
                        _nodeDrag = false;
                    }
                    break;
                case MouseButtons.Right:
                    ValidateControls();
                    PopupNodeEdit.Show(PanelGradControls, e.Location);
                    break;
            }

            ValidateControls();
            DrawControls();
        }

        /// <summary>
        /// Handles the Click event of the itemAddNode control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ItemAddNode_Click(object sender, EventArgs e)
        {
            if (DataContext is null)
            {
                return;
            }

            float weightPosition = _controlPanelImage.Width / 2.0f;

            if (sender == ItemAddNode)
            {
                weightPosition = PanelGradControls.PointToClient(new Point(PopupNodeEdit.Left, PopupNodeEdit.Top)).X;
            }

            // Adjust position to be near the selected node if a node is selected.
            if ((DataContext.SelectedNode is not null) && (_controlPanelImage is not null))
            {
                weightPosition = DataContext.SelectedNode.Weight * _controlPanelImage.Width;

                if (weightPosition < _controlPanelImage.Width - 16)
                {
                    weightPosition += 16.0f;
                }
                else
                {
                    weightPosition -= 16.0f;
                }
            }

            PanelGradientDisplay_MouseDoubleClick(sender, new MouseEventArgs(MouseButtons.Left, 1, (int)weightPosition, 0, 0));
        }

        /// <summary>
        /// Handles the MouseMove event of the panelBrushPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void PanelGradControls_MouseMove(object sender, MouseEventArgs e)
        {
            if ((e.Button != MouseButtons.Left) || (!_nodeDrag))
            {
                return;
            }

            float newWeight = (float)(e.X + _nodeDragOffset) / (PanelGradControls.ClientSize.Width - 1);

            if (newWeight < 0.0f)
            {
                newWeight = 0;
            }

            if (newWeight > 1.0f)
            {
                newWeight = 1.0f;
            }

            if ((DataContext?.SetWeightCommand is not null) && (DataContext.SetWeightCommand.CanExecute(newWeight)))
            {
                DataContext.SetWeightCommand.Execute(newWeight);
            }

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
        private void PanelGradControls_Paint(object sender, PaintEventArgs e) => DrawControls(e.Graphics);

        /// <summary>
        /// Handles the Paint event of the panelPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        private void PanelPreview_Paint(object sender, PaintEventArgs e) => DrawPreview(e.Graphics);

        /// <summary>
        /// Handles the Paint event of the panelBrushPreview control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PaintEventArgs"/> instance containing the event data.</param>
        private void PanelGradientDisplay_Paint(object sender, PaintEventArgs e) => DrawGradientDisplay(e.Graphics);

        /// <summary>
        /// Function to convert a Gorgon glyph brush into a GDI+ linear gradient brush.
        /// </summary>
        /// <param name="destRect">Destination rectangle.</param>
        /// <param name="rotate">TRUE to rotate the gradient, FALSE to keep it oriented as is.</param>
        private Brush UpdateBrush(RectangleF destRect, bool rotate)
        {
            if ((DataContext is null) || (DataContext.Nodes.Count == 0))
            {
                return null;
            }

            var linearBrush = new LinearGradientBrush(destRect,
                                                      DataContext.Nodes[0].Color,
                                                      DataContext.Nodes[^1].Color,
                                                      rotate ? DataContext.Angle : 0,
                                                      DataContext.ScaleAngle)
            {
                GammaCorrection = DataContext.UseGamma,
                WrapMode = WrapMode.Tile
            };

            var interpColors = new ColorBlend(DataContext.Nodes.Count);

            int counter = 0;
            foreach(WeightHandle handle in DataContext.Nodes)
            {
                interpColors.Colors[counter] = handle.Color;
                interpColors.Positions[counter] = handle.Weight;
                ++counter;
            }

            linearBrush.InterpolationColors = interpColors;

            return linearBrush;
        }

        /// <summary>
        /// Function to unhook control events to prevent reentrancy.
        /// </summary>
        private void UnhookEvents()
        {
            if (Interlocked.Exchange(ref _eventLock, 0) == 0)
            {
                return;
            }

            CheckScaleAngle.Click -= CheckScaleAngle_Click;
            CheckUseGamma.Click -= CheckUseGamma_CheckedChanged;
            PickerNodeColor.ColorChanged -= PickerNodeColor_ColorChanged;
            NumericSelectedWeight.ValueChanged -= NumericSelectedWeight_ValueChanged;
            NumericAngle.ValueChanged -= NumericAngle_ValueChanged;
        }

        /// <summary>
        /// Function to hook control events to prevern reentrancy.
        /// </summary>
        private void HookEvents()
        {
            if (Interlocked.Exchange(ref _eventLock, 1) == 1)
            {
                return;
            }

            NumericSelectedWeight.ValueChanged += NumericSelectedWeight_ValueChanged;
            PickerNodeColor.ColorChanged += PickerNodeColor_ColorChanged;
            NumericAngle.ValueChanged += NumericAngle_ValueChanged;
            CheckUseGamma.Click += CheckUseGamma_CheckedChanged;
            CheckScaleAngle.Click += CheckScaleAngle_Click;
        }

        /// <summary>
        /// Function to unhook events for controls and the data context.
        /// </summary>
        private void UnassignEvents()
        {
            UnhookEvents();

            if (DataContext is null)
            {
                return;
            }

            DataContext.PropertyChanged -= DataContext_PropertyChanged;
        }

        /// <summary>
        /// Function to reset the control back to its original state.
        /// </summary>
        private void ResetDataContext()
        {
            CheckUseGamma.Checked = 
            CheckScaleAngle.Checked = false;
            NumericSelectedWeight.Value =
            NumericAngle.Value = 0M;            
        }

        /// <summary>
        /// Function called to initialize the control from its data context.
        /// </summary>
        /// <param name="dataContext">The data context to use.</param>
        private void InitializeDataContext(IFontGradientBrush dataContext)
        {
            if (dataContext is null)
            {
                ResetDataContext();
                return;
            }

            CheckUseGamma.Checked = dataContext.UseGamma;
            CheckScaleAngle.Checked = dataContext.ScaleAngle;
            NumericSelectedWeight.Value = (decimal)(dataContext.SelectedNode?.Weight ?? 0);
            NumericAngle.Value = (decimal)dataContext.Angle;
        }

        /// <summary>Function called to validate the OK button.</summary>
        /// <returns>
        ///   <b>true</b> if the OK button is valid, <b>false</b> if not.</returns>
        protected override bool OnValidateOk()
        {
            if (DataContext?.OkCommand is not null)
            {
                return DataContext.OkCommand.CanExecute(null);
            }

            return base.OnValidateOk();
        }

        /// <summary>Function to cancel the change.</summary>
        protected override void OnCancel()
        {
            if (DataContext is not null)
            {
                DataContext.IsActive = false;
            }
        }

        /// <summary>Function to submit the change.</summary>
        protected override void OnSubmit()
        {
            base.OnSubmit();

            if ((DataContext?.OkCommand is null) || (!DataContext.OkCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.OkCommand.Execute(null);
        }

        /// <summary>
        /// Raises the <see cref="UserControl.Load" /> event.
        /// </summary>
        /// <param name="e">An <see cref="EventArgs" /> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);


            _controlPanelImage = new Bitmap(PanelGradControls.ClientSize.Width,
                                            PanelGradControls.ClientSize.Height,
                                            PixelFormat.Format32bppArgb);

            _gradDisplayImage = new Bitmap(PanelGradientDisplay.ClientSize.Width,
                                            PanelGradientDisplay.ClientSize.Height,
                                            PixelFormat.Format32bppArgb);

            _gradientImage = new Bitmap(PanelGradientDisplay.ClientSize.Width,
                                        PanelGradientDisplay.ClientSize.Height,
                                        PixelFormat.Format32bppArgb);

            _gradPreviewImage = new Bitmap(PanelPreview.ClientSize.Width,
                                            PanelPreview.ClientSize.Height,
                                            PixelFormat.Format32bppArgb);
            ValidateControls();
        }

        /// <summary>
        /// Function to assign a data context to the view as a view model.
        /// </summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IFontGradientBrush dataContext)
        {
            UnassignEvents();

            InitializeDataContext(dataContext);

            DataContext = dataContext;

            DrawGradientDisplay();
            DrawControls();
            DrawPreview();

            if (DataContext is null)
            {
                return;
            }

            DataContext.PropertyChanged += DataContext_PropertyChanged;
            HookEvents();
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="FontGradientBrushView"/> class.
        /// </summary>
        public FontGradientBrushView() => InitializeComponent();
        #endregion
    }
}
