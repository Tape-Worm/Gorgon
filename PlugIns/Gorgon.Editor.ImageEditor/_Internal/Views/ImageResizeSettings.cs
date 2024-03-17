
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: January 15, 2019 9:04:41 PM
// 


using System.ComponentModel;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Controls;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// The panel used to provide settings for image import resizing
/// </summary>
internal partial class ImageResizeSettings
    : EditorSubPanelCommon, IDataContext<ICropResizeSettings>
{

    /// <summary>Property to return the data context assigned to this view.</summary>
    /// <value>The data context.</value>
    [Browsable(false)]
    public ICropResizeSettings ViewModel
    {
        get;
        private set;
    }



    /// <summary>
    /// Function to validate the state of the controls in the view.
    /// </summary>
    private void ValidateControls()
    {
        ValidateOk();

        if (ViewModel is null)
        {
            RadioCrop.Enabled = RadioResize.Enabled = LabelImageFilter.Enabled =
                ComboImageFilter.Enabled = CheckPreserveAspect.Enabled = LabelAnchor.Enabled =
                panelAnchor.Enabled = false;
            return;
        }

        LabelAnchor.Visible = AlignmentPicker.Visible = RadioCrop.Checked;

        bool radioEnabled = (ViewModel.AllowedModes & CropResizeMode.Resize) == CropResizeMode.Resize;
        RadioResize.Enabled = radioEnabled;
        LabelImageFilter.Visible = ComboImageFilter.Visible = CheckPreserveAspect.Visible = radioEnabled && RadioResize.Checked;
    }


    /// <summary>Handles the Click event of the CheckPreserveAspect control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void CheckPreserveAspect_Click(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.PreserveAspect = CheckPreserveAspect.Checked;
    }

    /// <summary>Handles the SelectedValueChanged event of the ComboImageFilter control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ComboImageFilter_SelectedValueChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.ImageFilter = (ImageFilter)ComboImageFilter.SelectedItem;
    }

    /// <summary>Handles the Click event of the RadioCrop control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void RadioCrop_Click(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        if ((ViewModel.AllowedModes & CropResizeMode.Crop) == CropResizeMode.Crop)
        {
            ViewModel.CurrentMode = CropResizeMode.Crop;
        }
        else
        {
            ViewModel.CurrentMode = CropResizeMode.None;
        }

        ValidateControls();
    }

    /// <summary>Handles the Click event of the RadioResize control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void RadioResize_Click(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.CurrentMode = CropResizeMode.Resize;

        ValidateControls();
    }

    /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ICropResizeSettings.CurrentMode):
                RadioCrop.Checked = ViewModel.CurrentMode is CropResizeMode.Crop or CropResizeMode.None;
                RadioResize.Checked = !RadioCrop.Checked;
                break;
        }
        UpdateLabels(ViewModel);
        ValidateControls();
    }

    /// <summary>Handles the AlignmentChanged event of the AlignmentPicker control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void AlignmentPicker_AlignmentChanged(object sender, EventArgs e)
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.CurrentAlignment = AlignmentPicker.Alignment;
    }

    /// <summary>
    /// Function to unassign the events assigned to the datacontext.
    /// </summary>
    private void UnassignEvents()
    {
        if (ViewModel is null)
        {
            return;
        }

        ViewModel.PropertyChanged -= DataContext_PropertyChanged;
    }


    /// <summary>
    /// Function called when the view should be reset by a <b>null</b> data context.
    /// </summary>
    private void ResetDataContext()
    {
        RadioCrop.Enabled = RadioResize.Enabled = RadioCrop.Checked = RadioResize.Checked = false;
        AlignmentPicker.Alignment = Gorgon.UI.Alignment.Center;
        AlignmentPicker.Enabled = false;
        CheckPreserveAspect.Checked = false;
        ComboImageFilter.Text = string.Empty;
        ComboImageFilter.Items.Clear();
        UpdateLabels(null);
    }

    /// <summary>
    /// Function to update the labels for the controls on the view.
    /// </summary>
    /// <param name="dataContext">The current data context.</param>
    private void UpdateLabels(ICropResizeSettings dataContext)
    {
        if (dataContext is null)
        {
            LabelDesc.Text = string.Format(Resources.GORIMG_RESIZE_CROP_DESC, string.Empty);
            LabelImportImageName.Text = string.Format(Resources.GORIMG_TEXT_RESIZE_CROP_IMPORT_NAME, string.Empty);
            LabelImportImageDimensions.Text = string.Format(Resources.GORIMG_TEXT_RESIZE_CROP_IMPORT_SIZE, 0, 0);
            LabelTargetImageDimensions.Text = string.Format(Resources.GORIMG_TEXT_RESIZE_CROP_IMAGE_SIZE, 0, 0);
            RadioCrop.Text = string.Format(Resources.GORIMG_TEXT_CROP_TO, 0, 0);
            RadioResize.Text = string.Format(Resources.GORIMG_TEXT_RESIZE_TO, 0, 0);
            return;
        }

        LabelDesc.Text = string.Format(Resources.GORIMG_RESIZE_CROP_DESC, dataContext.ImportFile);
        LabelImportImageName.Text = string.Format(Resources.GORIMG_TEXT_RESIZE_CROP_IMPORT_NAME, dataContext.ImportFile);
        LabelImportImageDimensions.Text = string.Format(Resources.GORIMG_TEXT_RESIZE_CROP_IMPORT_SIZE, dataContext.ImportImage?.Width ?? 0, dataContext.ImportImage?.Height ?? 0);
        LabelTargetImageDimensions.Text = string.Format(Resources.GORIMG_TEXT_RESIZE_CROP_IMAGE_SIZE, dataContext.TargetImageSize.Width, dataContext.TargetImageSize.Height);
        RadioCrop.Text = ((dataContext.AllowedModes & CropResizeMode.Crop) == CropResizeMode.Crop) ?
            string.Format(Resources.GORIMG_TEXT_CROP_TO, dataContext.TargetImageSize.Width, dataContext.TargetImageSize.Height)
            : Resources.GORIMG_TEXT_ALIGN_TO;
        RadioResize.Text = string.Format(Resources.GORIMG_TEXT_RESIZE_TO, dataContext.TargetImageSize.Width, dataContext.TargetImageSize.Height);
    }

    /// <summary>
    /// Function to initialize the view from the current data context.
    /// </summary>
    /// <param name="dataContext">The data context being assigned.</param>
    private void InitializeFromDataContext(ICropResizeSettings dataContext)
    {
        if (dataContext is null)
        {
            ResetDataContext();
            return;
        }

        UpdateLabels(dataContext);

        switch (dataContext.CurrentMode)
        {
            case CropResizeMode.Resize:
                RadioResize.Checked = true;
                break;
            default:
                RadioCrop.Checked = true;
                break;
        }

        AlignmentPicker.Alignment = dataContext.CurrentAlignment;
        ComboImageFilter.SelectedItem = dataContext.ImageFilter;
        CheckPreserveAspect.Checked = dataContext.PreserveAspect;
    }

    /// <summary>Function to cancel the change.</summary>
    protected override void OnCancel()
    {
        base.OnCancel();

        if ((ViewModel?.CancelCommand is null) || (!ViewModel.CancelCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.CancelCommand.Execute(null);
    }

    /// <summary>Function to submit the change.</summary>
    protected override void OnSubmit()
    {
        base.OnSubmit();

        if ((ViewModel?.OkCommand is null) || (!ViewModel.OkCommand.CanExecute(null)))
        {
            return;
        }

        ViewModel.OkCommand.Execute(null);
    }

    /// <summary>Function called to validate the OK button.</summary>
    /// <returns>
    ///   <b>true</b> if the OK button is valid, <b>false</b> if not.</returns>
    protected override bool OnValidateOk() => (ViewModel?.OkCommand is not null) && (ViewModel.OkCommand.CanExecute(null));

    /// <summary>Raises the <see cref="System.Windows.Forms.Control.VisibleChanged"/> event.</summary>
    /// <param name="e">An <see cref="EventArgs"/> object that contains the event data.</param>
    protected override void OnVisibleChanged(EventArgs e)
    {
        base.OnVisibleChanged(e);

        if (Visible)
        {
            ValidateControls();
        }
    }

    /// <summary>Raises the <see cref="System.Windows.Forms.UserControl.Load"/> event.</summary>
    /// <param name="e">An <see cref="EventArgs"/> that contains the event data.</param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        if (IsDesignTime)
        {
            return;
        }

        UpdateLabels(ViewModel);
        ValidateControls();
    }

    /// <summary>Function to assign a data context to the view as a view model.</summary>
    /// <param name="dataContext">The data context to assign.</param>
    /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
    public void SetDataContext(ICropResizeSettings dataContext)
    {
        UnassignEvents();

        InitializeFromDataContext(dataContext);
        ViewModel = dataContext;

        if (ViewModel is null)
        {
            return;
        }

        ViewModel.PropertyChanged += DataContext_PropertyChanged;
        if (IsHandleCreated)
        {
            ValidateControls();
        }
    }



    /// <summary>Initializes a new instance of the <see cref="Editor.Views.ImageResizeSettings"/> class.</summary>
    public ImageResizeSettings()
    {
        InitializeComponent();

        // Populate the image filter drop down.
        ImageFilter[] filters = (ImageFilter[])Enum.GetValues(typeof(ImageFilter));

        foreach (ImageFilter filter in filters)
        {
            ComboImageFilter.Items.Add(filter);
        }
    }

}
