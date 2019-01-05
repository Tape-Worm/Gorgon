using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Imaging.Codecs;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// Provides a ribbon interface for the plug in view.
    /// </summary>
    /// <remarks>
    /// We cannot provide a ribbon on the control directly. For some reason, the krypton components will only allow ribbons on forms.
    /// </remarks>
    internal partial class FormRibbon 
        : KryptonForm, IDataContext<IImageContent>
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the current graphics context.
        /// </summary>        
        public IGraphicsContext GraphicsContext
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the data context for the ribbon on the form.
        /// </summary>
        public IImageContent DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [PropertyChangedEventArgs] instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IImageContent.PixelFormats):
                    RefreshPixelFormats(DataContext);
                    break;
                case nameof(IImageContent.CurrentPixelFormat):
                    ButtonImageFormat.TextLine1 = $"{Resources.GORIMG_TEXT_IMAGE_FORMAT}: {DataContext.CurrentPixelFormat}";
                    UpdatePixelFormatMenuSelection(DataContext);
                    break;
            }

            ValidateButtons();
        }

        /// <summary>Handles the PropertyChanging event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [PropertyChangingEventArgs] instance containing the event data.</param>
        private void DataContext_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {
        }

        /// <summary>Handles the CollectionChanged event of the Codecs control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [NotifyCollectionChangedEventArgs] instance containing the event data.</param>
        private void Codecs_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (IGorgonImageCodec codec in e.NewItems.OfType<IGorgonImageCodec>())
                    {
                        if (MenuCodecs.Items.OfType<ToolStripMenuItem>().Any(item => item.Tag == codec))
                        {
                            continue;
                        }

                        AddCodecItem(DataContext, codec);
                    }
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (IGorgonImageCodec codec in e.OldItems.OfType<IGorgonImageCodec>())
                    {
                        RemoveCodecItem(codec);
                    }
                    break;
                case NotifyCollectionChangedAction.Reset:
                    ClearCodecs();
                    break;
            }
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the Item control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [EventArgs] instance containing the event data.</param>
        private void PixelFormatItem_Click(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }

            var item = (ToolStripMenuItem)sender;
            var format = (BufferFormat)item.Tag;

            if ((DataContext.ConvertFormatCommand != null) && (DataContext.ConvertFormatCommand.CanExecute(format)))
            {
                DataContext.ConvertFormatCommand.Execute(format);
            }

            // Ensure only this item is checked.
            UpdatePixelFormatMenuSelection(DataContext);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the Item control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [EventArgs] instance containing the event data.</param>
        private void CodecItem_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;
            var codec = item.Tag as IGorgonImageCodec;

            if ((DataContext?.ExportImageCommand != null) && (DataContext.ExportImageCommand.CanExecute(codec)))
            {
                DataContext.ExportImageCommand.Execute(codec);
            }
        }

        /// <summary>Handles the Click event of the ButtonSaveImage control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ButtonSaveImage_Click(object sender, EventArgs e)
        {
            if ((DataContext?.SaveContentCommand == null) || (!DataContext.SaveContentCommand.CanExecute(null)))
            {
                return;
            }

            await DataContext.SaveContentCommand.ExecuteAsync(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonImageRedo control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonImageRedo_Click(object sender, EventArgs e)
        {
            if ((DataContext?.RedoCommand == null) || (!DataContext.RedoCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.RedoCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonImageUndo control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonImageUndo_Click(object sender, EventArgs e)
        {
            if ((DataContext?.UndoCommand == null) || (!DataContext.UndoCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.UndoCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>
        /// Function to validate the state of the buttons.
        /// </summary>
        private void ValidateButtons()
        {
            if (DataContext == null)
            {
                return;
            }

            ButtonImageUndo.Enabled = DataContext.UndoCommand?.CanExecute(null) ?? false;
            ButtonImageRedo.Enabled = DataContext.RedoCommand?.CanExecute(null) ?? false;
            ButtonExport.Enabled = MenuCodecs.Items.Count > 0;            
            ButtonSaveImage.Enabled = DataContext.SaveContentCommand?.CanExecute(null) ?? false;
        }

        /// <summary>
        /// Function to ensure the menu is single selection only.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void UpdatePixelFormatMenuSelection(IImageContent dataContext)
        {
            if (dataContext == null)
            {
                return;
            }

            foreach (ToolStripMenuItem item in MenuImageFormats.Items.OfType<ToolStripMenuItem>())
            {
                item.Checked = ((BufferFormat)item.Tag) == dataContext.CurrentPixelFormat;
            }
        }

        /// <summary>
        /// Function to add an image pixel format to the list.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void RefreshPixelFormats(IImageContent dataContext)
        {
            foreach (ToolStripMenuItem item in MenuImageFormats.Items.OfType<ToolStripMenuItem>())
            {
                item.Click -= PixelFormatItem_Click;
            }

            MenuImageFormats.Items.Clear();

            if (dataContext == null)
            {
                return;
            }

            foreach (BufferFormat format in dataContext.PixelFormats)
            {
                var item = new ToolStripMenuItem(format.ToString())
                {
                    Name = format.ToString(),
                    Checked = dataContext.CurrentPixelFormat == format,
                    CheckOnClick = true,
                    Tag = format
                };

                item.Click += PixelFormatItem_Click;

                MenuImageFormats.Items.Add(item);

                if (item.Checked)
                {
                    ButtonImageFormat.TextLine1 = $"{Resources.GORIMG_TEXT_IMAGE_FORMAT}: {format}";
                }
            }
        }


        /// <summary>
        /// Function to clear the codec list.
        /// </summary>
        private void ClearCodecs()
        {
            foreach (ToolStripMenuItem item in MenuCodecs.Items.OfType<ToolStripMenuItem>())
            {
                item.Click -= CodecItem_Click;
            }
            MenuCodecs.Items.Clear();
        }

        /// <summary>
        /// Function to add an image codec to the list.
        /// </summary>
        /// <param name="dataContext">The data context to use.</param>
        /// <param name="codec">The codec to add.</param>
        private void AddCodecItem(IImageContent dataContext, IGorgonImageCodec codec)
        {
            var item = new ToolStripMenuItem($"{codec.CodecDescription} ({codec.Codec})")
            {
                Name = codec.Name,                
                CheckOnClick = false,
                Tag = codec
            };

            item.Click += CodecItem_Click;

            MenuCodecs.Items.Add(item);
        }

        /// <summary>
        /// Function to remove a codec item from the list.
        /// </summary>
        /// <param name="codec">The codec to remove.</param>
        private void RemoveCodecItem(IGorgonImageCodec codec)
        {
            ToolStripMenuItem menuItem = MenuCodecs.Items.OfType<ToolStripMenuItem>().FirstOrDefault(item => item.Tag == codec);

            if (menuItem == null)
            {
                return;
            }

            menuItem.Click -= CodecItem_Click;
            MenuCodecs.Items.Remove(menuItem);
        }

        /// <summary>
        /// Function to unassign the events for the data context.
        /// </summary>
        private void UnassignEvents()
        {
            if (DataContext == null)
            {
                return;
            }

            if (DataContext.Codecs != null)
            {
                DataContext.Codecs.CollectionChanged -= Codecs_CollectionChanged;
            }
            DataContext.PropertyChanging -= DataContext_PropertyChanging;
            DataContext.PropertyChanged -= DataContext_PropertyChanged;
        }

        /// <summary>
        /// Function to reset the view when no data context is assigned.
        /// </summary>
        private void ResetDataContext()
        {
            RibbonImageContent.Enabled = false;
            ClearCodecs();
        }

        /// <summary>
        /// Function to initialize the view based on the data context.
        /// </summary>
        /// <param name="dataContext">The data context used to initialize.</param>
        private void InitializeFromDataContext(IImageContent dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            ClearCodecs();
            foreach (IGorgonImageCodec codec in dataContext.Codecs)
            {
                AddCodecItem(dataContext, codec);
            }

            RefreshPixelFormats(dataContext);

            UpdatePixelFormatMenuSelection(dataContext);
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IImageContent dataContext)
        {
            UnassignEvents();

            InitializeFromDataContext(dataContext);

            DataContext = dataContext;
            ValidateButtons();

            if (DataContext == null)
            {
                return;
            }

            DataContext.PropertyChanged += DataContext_PropertyChanged;
            DataContext.PropertyChanging += DataContext_PropertyChanging;

            if (DataContext.Codecs != null)
            {
                DataContext.Codecs.CollectionChanged += Codecs_CollectionChanged;
            }
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the FormRibbon class.</summary>
        public FormRibbon() => InitializeComponent();
        #endregion
    }
}
