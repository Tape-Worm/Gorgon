#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: April 24, 2019 6:55:01 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Editor.ExtractSpriteTool.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Editor.ExtractSpriteTool
{
    /// <summary>
    /// The main view for the tool.
    /// </summary>
    internal partial class FormExtract
        : EditorToolBaseForm, IDataContext<IExtract>
    {
        #region Variables.
        // The renderer for this window.
        private IRenderer _renderer;
        // State flag for the closing operations.
        private int _closeState;
        #endregion

        #region Properties.
        /// <summary>Property to return the data context assigned to this view.</summary>
        public IExtract DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to validate the controls on the form.
        /// </summary>
        private void ValidateControls()
        {
            if (DataContext?.Texture == null)
            {
                foreach (Control ctl in Controls)
                {
                    ctl.Enabled = false;
                }
                return;
            }

            LabelArrayRange.Visible = LabelArrayCount.Visible = LabelArrayStart.Visible = NumericArrayIndex.Visible = NumericArrayCount.Visible = DataContext.HasArray;
            CheckPreviewSprites.Enabled = DataContext.Sprites != null;
            TableSkipMaskColor.Enabled = DataContext.SetEmptySpriteMaskColorCommand?.CanExecute(null) ?? false;
            ButtonOk.Enabled = DataContext.SaveSpritesCommand?.CanExecute(null) ?? false;
            TableSpritePreview.Visible = DataContext.InSpritePreview;
            ButtonNextSprite.Enabled = DataContext.NextPreviewSpriteCommand?.CanExecute(null) ?? false;
            ButtonPrevSprite.Enabled = DataContext.PrevPreviewSpriteCommand?.CanExecute(null) ?? false;
            LabelArray.Visible = ButtonSendToArrayStart.Visible = ButtonNextArray.Visible = ButtonPrevArray.Visible = DataContext.HasArray;
            ButtonNextArray.Enabled = DataContext.NextPreviewArrayCommand?.CanExecute(null) ?? false;
            ButtonPrevArray.Enabled = DataContext.PrevPreviewArrayCommand?.CanExecute(null) ?? false;
            ButtonSendToArrayStart.Enabled = DataContext.SendPreviewArrayToStartCommand?.CanExecute(null) ?? false;
        }

        /// <summary>Handles the Click event of the SkipColor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        private void SkipColor_Click(object sender, EventArgs e)
        {
            if ((DataContext?.SetEmptySpriteMaskColorCommand == null) || (!DataContext.SetEmptySpriteMaskColorCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.SetEmptySpriteMaskColorCommand.Execute(null);
        }

        /// <summary>Handles the ValueChanged event of the NumericCellWidth control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericCellWidth_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.CellSize = new DX.Size2((int)NumericCellWidth.Value, DataContext.CellSize.Height);
        }

        /// <summary>Handles the ValueChanged event of the NumericCellHeight control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericCellHeight_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.CellSize = new DX.Size2(DataContext.CellSize.Width, (int)NumericCellHeight.Value);
        }

        /// <summary>Handles the ValueChanged event of the NumericOffsetX control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericOffsetX_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.GridOffset = new DX.Point((int)NumericOffsetX.Value, DataContext.GridOffset.Y);
        }

        /// <summary>Handles the ValueChanged event of the NumericOffsetY control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericOffsetY_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.GridOffset = new DX.Point(DataContext.GridOffset.X, (int)NumericOffsetY.Value);
        }

        /// <summary>Handles the ValueChanged event of the NumericColumnCount control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericColumnCount_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.GridSize = new DX.Size2((int)NumericColumnCount.Value, DataContext.GridSize.Height);
        }

        /// <summary>Handles the ValueChanged event of the NumericRowCount control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericRowCount_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.GridSize = new DX.Size2(DataContext.GridSize.Width, (int)NumericRowCount.Value);
        }

        /// <summary>Handles the ValueChanged event of the NumericArrayIndex control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericArrayIndex_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.StartArrayIndex = (int)NumericArrayIndex.Value;
        }

        /// <summary>Handles the ValueChanged event of the NumericArrayCount control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void NumericArrayCount_ValueChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.ArrayCount = (int)NumericArrayCount.Value;
        }

        /// <summary>Handles the Click event of the ButtonGenerate control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonGenerate_Click(object sender, EventArgs e)
        {
            if ((DataContext?.GenerateSpritesCommand == null) || (!DataContext.GenerateSpritesCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.GenerateSpritesCommand.Execute(null);
        }

        /// <summary>Handles the Click event of the ButtonOk control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonOk_Click(object sender, EventArgs e)
        {
            var args = new SaveSpritesArgs();

            if ((DataContext?.SaveSpritesCommand == null) || (!DataContext.SaveSpritesCommand.CanExecute(args)))
            {
                return;
            }

            DataContext.SaveSpritesCommand.Execute(args);

            if (args.Cancel)
            {
                return;
            }

            DialogResult = DialogResult.OK;
        }


        /// <summary>Handles the Click event of the ButtonNextSprite control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonNextSprite_Click(object sender, EventArgs e)
        {
            if ((DataContext?.NextPreviewSpriteCommand == null) || (!DataContext.NextPreviewSpriteCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.NextPreviewSpriteCommand.Execute(null);
            ValidateControls();
        }

        /// <summary>Handles the Click event of the ButtonPrevSprite control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonPrevSprite_Click(object sender, EventArgs e)
        {
            if ((DataContext?.PrevPreviewSpriteCommand == null) || (!DataContext.PrevPreviewSpriteCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.PrevPreviewSpriteCommand.Execute(null);
            ValidateControls();
        }

        /// <summary>Handles the CheckedChanged event of the CheckPreviewSprites control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void CheckPreviewSprites_CheckedChanged(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.InSpritePreview = CheckPreviewSprites.Checked;
            ValidateControls();
        }

        /// <summary>Handles the Click event of the ButtonPrevArray control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonPrevArray_Click(object sender, EventArgs e)
        {
            if ((DataContext?.PrevPreviewArrayCommand == null) || (!DataContext.PrevPreviewArrayCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.PrevPreviewArrayCommand.Execute(null);
            ValidateControls();
        }

        /// <summary>Handles the Click event of the ButtonNextArray control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonNextArray_Click(object sender, EventArgs e)
        {
            if ((DataContext?.NextPreviewArrayCommand == null) || (!DataContext.NextPreviewArrayCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.NextPreviewArrayCommand.Execute(null);
            ValidateControls();
        }


        /// <summary>Handles the Click event of the ButtonSendToArrayStart control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSendToArrayStart_Click(object sender, EventArgs e)
        {
            if ((DataContext?.SendPreviewArrayToStartCommand == null) || (!DataContext.SendPreviewArrayToStartCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.SendPreviewArrayToStartCommand.Execute(null);
            ValidateControls();
        }

        /// <summary>Handles the Click event of the CheckSkipEmpty control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="T:System.EventArgs"/> instance containing the event data.</param>
        private void CheckSkipEmpty_Click(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.AllowSkipEmpty = CheckSkipEmpty.Checked;
            ValidateControls();
        }

        /// <summary>Handles the MouseWheel event of the PanelRender control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        private void PanelRender_MouseWheel(object sender, MouseEventArgs e)
        {
            if ((DataContext == null) || (DataContext.SpriteGenerationTask != null))
            {
                return;
            }

            if (DataContext.InSpritePreview)
            {
                if (e.Delta < 0)
                {
                    ButtonPrevSprite.PerformClick();
                }
                else if (e.Delta > 0)
                {
                    ButtonNextSprite.PerformClick();
                }
                return;
            }

            if (e.Delta < 0)
            {
                ButtonPrevArray.PerformClick();
            }
            else if (e.Delta > 0)
            {
                ButtonNextArray.PerformClick();
            }
        }

        /// <summary>
        /// Function to update the sprite preview label.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void UpdateSpritePreviewLabel(IExtract dataContext)
        {
            if (dataContext == null)
            {
                LabelSprite.Text = string.Format(Resources.GOREST_TEXT_SPRITE_PREVIEW, 0, 0);
                return;
            }

            LabelSprite.Text = string.Format(Resources.GOREST_TEXT_SPRITE_PREVIEW, dataContext.CurrentPreviewSprite + 1, dataContext.SpritePreviewCount);
        }

        /// <summary>
        /// Function to update the array label.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void UpdateArrayLabel(IExtract dataContext)
        {
            if (dataContext == null)
            {
                LabelArray.Text = string.Format(Resources.GOREST_TEXT_ARRAY, 0, 0);
                return;
            }

            LabelArray.Text = string.Format(Resources.GOREST_TEXT_ARRAY, dataContext.CurrentPreviewArrayIndex + 1, dataContext.PreviewArrayCount);
        }

        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IExtract.StartArrayIndex):
                    int currentIndex = (int)NumericArrayIndex.Value;
                    if (currentIndex != DataContext.StartArrayIndex)
                    {
                        NumericArrayIndex.Value = DataContext.StartArrayIndex;
                    }
                    break;
                case nameof(IExtract.CurrentPreviewArrayIndex):
                    UpdateArrayLabel(DataContext);
                    break;
                case nameof(IExtract.SpritePreviewCount):
                case nameof(IExtract.CurrentPreviewSprite):
                    UpdateSpritePreviewLabel(DataContext);
                    break;
                case nameof(IExtract.SpriteGenerationTask):
                    TableExtractControls.Enabled = DataContext.SpriteGenerationTask == null;
                    break;
                case nameof(IExtract.GridOffset):
                    NumericCellWidth.Maximum = DataContext.Texture.Width - DataContext.GridOffset.X;
                    NumericCellHeight.Maximum = DataContext.Texture.Height - DataContext.GridOffset.Y;
                    break;
                case nameof(IExtract.CellSize):
                    NumericOffsetX.Maximum = DataContext.Texture.Width - DataContext.CellSize.Width;
                    NumericOffsetY.Maximum = DataContext.Texture.Height - DataContext.CellSize.Height;
                    break;
                case nameof(IExtract.MaxGridSize):
                    NumericColumnCount.Maximum = DataContext.MaxGridSize.Width;
                    NumericRowCount.Maximum = DataContext.MaxGridSize.Height;
                    break;
                case nameof(IExtract.MaxArrayCount):
                    NumericArrayCount.Maximum = DataContext.MaxArrayCount;
                    break;
                case nameof(IExtract.MaxArrayIndex):
                    NumericArrayIndex.Maximum = DataContext.MaxArrayIndex;
                    break;
                case nameof(IExtract.SkipMaskColor):
                    SkipColor.Color = DataContext.SkipMaskColor;
                    break;
            }

            ValidateControls();
        }

        /// <summary>
        /// Function to unassign events from the data context.
        /// </summary>
        private void UnassignEvents()
        {
            if (DataContext == null)
            {
                return;
            }

            DataContext.PropertyChanged -= DataContext_PropertyChanged;
        }

        /// <summary>
        /// Function to initialize the form based on its data context.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void InitializeFromDataContext(IExtract dataContext)
        {
            if (dataContext == null)
            {
                UpdateSpritePreviewLabel(null);
                return;
            }

            SkipColor.Color = dataContext.SkipMaskColor;
            CheckSkipEmpty.Checked = dataContext.AllowSkipEmpty;

            NumericOffsetX.Maximum = (dataContext.Texture.Width - dataContext.CellSize.Width).Max(0);
            NumericOffsetY.Maximum = (dataContext.Texture.Height - dataContext.CellSize.Height).Max(0);
            NumericCellWidth.Maximum = (dataContext.Texture.Width - dataContext.GridOffset.X).Max(1);
            NumericCellHeight.Maximum = (dataContext.Texture.Height - dataContext.GridOffset.Y).Max(1);
            NumericArrayCount.Maximum = dataContext.MaxArrayCount.Max(1);
            NumericArrayIndex.Maximum = dataContext.MaxArrayIndex.Max(0);
            NumericColumnCount.Maximum = dataContext.MaxGridSize.Width;
            NumericRowCount.Maximum = dataContext.MaxGridSize.Height;

            NumericCellWidth.Value = dataContext.CellSize.Width.Min((int)NumericCellWidth.Maximum).Max(1);
            NumericCellHeight.Value = dataContext.CellSize.Height.Min((int)NumericCellHeight.Maximum).Max(1);
            NumericColumnCount.Value = dataContext.GridSize.Width.Min((int)NumericColumnCount.Maximum).Max(1);
            NumericRowCount.Value = dataContext.GridSize.Height.Min((int)NumericRowCount.Maximum).Max(1);
            NumericOffsetX.Value = dataContext.GridOffset.X.Min((int)NumericOffsetX.Maximum).Max(0);
            NumericOffsetY.Value = dataContext.GridOffset.Y.Min((int)NumericOffsetY.Maximum).Max(0);
            NumericArrayCount.Value = dataContext.ArrayCount.Min((int)NumericArrayCount.Maximum).Max(1);
            NumericArrayIndex.Value = dataContext.StartArrayIndex.Min((int)NumericArrayIndex.Maximum).Max(0);

            UpdateSpritePreviewLabel(dataContext);
            UpdateArrayLabel(dataContext);
        }

        /// <summary>Function to perform clean up when graphics operations are shutting down.</summary>
        /// <remarks>Any resources created in the <see cref="EditorToolBaseForm.OnSetupGraphics"/> method must be disposed of here.</remarks>
        protected override void OnShutdownGraphics()
        {
            IRenderer renderer = Interlocked.Exchange(ref _renderer, null);
            renderer?.SetDataContext(null);
            renderer?.Dispose();
        }

        /// <summary>Function to perform custom rendering using the graphics functionality.</summary>
        /// <remarks>
        ///   <para>
        /// This method is called once per frame to allow for custom graphics drawing on the <see cref="EditorToolBaseForm.RenderControl"/>.
        /// </para>
        ///   <para>
        /// Tool plug in implementors need to override this in order to perform custom rendering with the <see cref="EditorToolBaseForm.GraphicsContext"/>.
        /// </para>
        /// </remarks>
        protected override void OnRender() => _renderer.Render();

        /// <summary>Function to perform custom graphics set up.</summary>
        /// <remarks>
        ///   <para>
        /// This method allows tool plug in implementors to setup additional functionality for custom graphics rendering.
        /// </para>
        ///   <para>
        /// Resources created by this method should be cleaned up in the <see cref="EditorToolBaseForm.OnShutdownGraphics"/> method.
        /// </para>
        ///   <para>
        /// Implementors do not need to set up a <see cref="EditorToolBaseForm.SwapChain"/> since one is already provided.
        /// </para>
        /// </remarks>
        protected override void OnSetupGraphics()
        {
            _renderer = new Renderer(GraphicsContext, SwapChain);
            _renderer.Setup();
        }

        /// <summary>Raises the <see cref="Form.FormClosing"/> event.</summary>
        /// <param name="e">A <see cref="FormClosingEventArgs"/> that contains the event data.</param>
        protected override async void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            if (_closeState > 0)
            {
                e.Cancel = _closeState == 1;
                return;
            }

            // First state keeps us from causing chaos by hitting the close button when we're waiting for the view model task to complete.
            Interlocked.Increment(ref _closeState);

            e.Cancel = true;

            if (DataContext != null)
            {
                DataContext.IsMaximized = WindowState == FormWindowState.Maximized;

                if ((DataContext.ShutdownCommand != null) && (DataContext.ShutdownCommand.CanExecute(null)))
                {
                    await DataContext.ShutdownCommand.ExecuteAsync(null);
                }
            }

            await Task.Yield();

            Interlocked.Increment(ref _closeState);

            Close();
        }

        /// <summary>Raises the Shown event.</summary>
        /// <param name="e">An EventArgs containing event data.</param>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            if ((DataContext != null) && (DataContext.IsMaximized))
            {
                WindowState = FormWindowState.Maximized;
            }
        }

        /// <summary>Raises the Load event.</summary>
        /// <param name="e">An EventArgs containing event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            if (LicenseManager.UsageMode == LicenseUsageMode.Designtime)
            {
                return;
            }

            DataContext?.OnLoad();
            ValidateControls();

            PanelRender.Select();
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IExtract dataContext)
        {
            UnassignEvents();

            InitializeFromDataContext(dataContext);
            _renderer?.SetDataContext(dataContext);

            DataContext = dataContext;

            if (DataContext == null)
            {
                return;
            }

            DataContext.PropertyChanged += DataContext_PropertyChanged;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="FormExtract"/> class.</summary>
        public FormExtract()
        {
            InitializeComponent();
            PanelRender.MouseWheel += PanelRender_MouseWheel;
        }
        #endregion
    }
}
