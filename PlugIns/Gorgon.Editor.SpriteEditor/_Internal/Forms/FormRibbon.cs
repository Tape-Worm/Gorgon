using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using ComponentFactory.Krypton.Ribbon;
using ComponentFactory.Krypton.Toolkit;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.SpriteEditor;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// Provides a ribbon interface for the plug in view.
    /// </summary>
    /// <remarks>
    /// We cannot provide a ribbon on the control directly. For some reason, the krypton components will only allow ribbons on forms.
    /// </remarks>
    internal partial class FormRibbon 
        : KryptonForm, IDataContext<ISpriteContent>
    {
        #region Events.
        /// <summary>
        /// Event triggered when the zoom menu is updated.
        /// </summary>
        public event EventHandler<ZoomEventArgs> ImageZoomed;
        #endregion

        #region Variables.
        // The list of menu items associated with the zoom level.
        private readonly Dictionary<ZoomLevels, ToolStripMenuItem> _menuItems = new Dictionary<ZoomLevels, ToolStripMenuItem>();
        // The buttons on the ribbon.
        private readonly List<WeakReference<KryptonRibbonGroupButton>> _ribbonButtons = new List<WeakReference<KryptonRibbonGroupButton>>();
        // The current zoom level.
        private ZoomLevels _zoomLevel = ZoomLevels.ToWindow;
        // A list of buttons mapped to the tool structure.
        private readonly Dictionary<SpriteEditTool, WeakReference<KryptonRibbonGroupButton>> _toolButtons = new Dictionary<SpriteEditTool, WeakReference<KryptonRibbonGroupButton>>();
        #endregion

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
        public ISpriteContent DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the zoom item menu to reflect the current selection.
        /// </summary>
        private void UpdateZoomMenu()
        {
            if (!_menuItems.TryGetValue(_zoomLevel, out ToolStripMenuItem currentItem))
            {
                return;
            }

            foreach (ToolStripMenuItem item in MenuZoom.Items.OfType<ToolStripMenuItem>().Where(item => item != currentItem))
            {
                item.Checked = false;
            }

            ButtonZoomSprite.TextLine1 = string.Format(Resources.GORSPR_TEXT_ZOOM_BUTTON, _zoomLevel.GetName());

            EventHandler<ZoomEventArgs> handler = ImageZoomed;
            ImageZoomed?.Invoke(this, new ZoomEventArgs(_zoomLevel));
        }

        /// <summary>
        /// Function to ensure that visual states match the current tool setting.
        /// </summary>
        /// <param name="dataContext">The current data context.</param>
        private void SetToolStates(ISpriteContent dataContext)
        {
            KryptonRibbonGroupButton button;

            if (dataContext.CurrentTool == SpriteEditTool.None)
            {
                foreach (KeyValuePair<SpriteEditTool, WeakReference<KryptonRibbonGroupButton>> buttonItem in _toolButtons)
                {
                    if (buttonItem.Value.TryGetTarget(out button))
                    {
                        button.Checked = false;
                    }
                }

                return;
            }

            if (!_toolButtons.TryGetValue(dataContext.CurrentTool, out WeakReference<KryptonRibbonGroupButton> buttonRef))
            {
                return;
            }

            if (!buttonRef.TryGetTarget(out button))
            {
                return;
            }

            button.Checked = true;
        }

        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [PropertyChangedEventArgs] instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(ISpriteContent.CurrentTool):
                    SetToolStates(DataContext);
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

        /// <summary>Handles the Click event of the ButtonSaveSprite control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private async void ButtonSaveSprite_Click(object sender, EventArgs e)
        {
            if ((DataContext?.SaveContentCommand == null) || (!DataContext.SaveContentCommand.CanExecute(SaveReason.UserSave)))
            {
                return;
            }

            await DataContext.SaveContentCommand.ExecuteAsync(SaveReason.UserSave);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonSpriteRedo control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteRedo_Click(object sender, EventArgs e)
        {
            if ((DataContext?.RedoCommand == null) || (!DataContext.RedoCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.RedoCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonSpriteUndo control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonSpriteUndo_Click(object sender, EventArgs e)
        {
            if ((DataContext?.UndoCommand == null) || (!DataContext.UndoCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.UndoCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonPickSprite control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonPickSprite_Click(object sender, EventArgs e)
        {
            if ((DataContext?.SpritePickCommand == null) || (!DataContext.SpritePickCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.SpritePickCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonClipSprite control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonClipSprite_Click(object sender, EventArgs e)
        {
            if (DataContext == null)
            {
                return;
            }
            
            DataContext.CurrentTool = ButtonClipSprite.Checked ? SpriteEditTool.SpriteClip : SpriteEditTool.None;
            ValidateButtons();
        }

        /// <summary>Handles the Click event of the ButtonNewSprite control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        private void ButtonNewSprite_Click(object sender, EventArgs e)
        {
            if ((DataContext?.NewSpriteCommand == null) || (!DataContext.NewSpriteCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.NewSpriteCommand.Execute(null);
            ValidateButtons();
        }

        /// <summary>
        /// Function to build the list of available ribbon buttons for ease of access.
        /// </summary>
        private void GetRibbonButtons()
        {
            foreach (KryptonRibbonTab tab in RibbonSpriteContent.RibbonTabs)
            {
                foreach (KryptonRibbonGroup grp in tab.Groups)
                {
                    // They really don't make it easy to get at the buttons do they?
                    foreach (KryptonRibbonGroupLines container in grp.Items.OfType<KryptonRibbonGroupLines>())
                    {
                        foreach (KryptonRibbonGroupButton item in container.Items.OfType<KryptonRibbonGroupButton>())
                        {
                            _ribbonButtons.Add(new WeakReference<KryptonRibbonGroupButton>(item));
                        }
                    }

                    foreach (KryptonRibbonGroupTriple container in grp.Items.OfType<KryptonRibbonGroupTriple>())
                    {
                        foreach (KryptonRibbonGroupButton item in container.Items.OfType<KryptonRibbonGroupButton>())
                        {
                            _ribbonButtons.Add(new WeakReference<KryptonRibbonGroupButton>(item));
                        }
                    }
                }
            }

            _toolButtons[SpriteEditTool.SpriteClip] = new WeakReference<KryptonRibbonGroupButton>(ButtonClipSprite);
            _toolButtons[SpriteEditTool.SpritePick] = new WeakReference<KryptonRibbonGroupButton>(ButtonPickSprite);
            _toolButtons[SpriteEditTool.SetAnchor] = new WeakReference<KryptonRibbonGroupButton>(ButtonSpriteAnchor);
            _toolButtons[SpriteEditTool.CornerResize] = new WeakReference<KryptonRibbonGroupButton>(ButtonSpriteVertexOffsets);
            _toolButtons[SpriteEditTool.CornerColor] = new WeakReference<KryptonRibbonGroupButton>(ButtonSpriteVertexColors);
        }

        /// <summary>
        /// Function to disable or enable all of the ribbon buttons.
        /// </summary>
        /// <param name="enable"><b>true</b> to enable all buttons, <b>false</b> to disable.</param>
        private void EnableRibbon(bool enable)
        {
            foreach (WeakReference<KryptonRibbonGroupButton> item in _ribbonButtons)
            {
                if (!item.TryGetTarget(out KryptonRibbonGroupButton button))
                {
                    continue;
                }

                button.Enabled = enable;
            }
        }

        /// <summary>
        /// Function to validate the state of the buttons.
        /// </summary>
        private void ValidateButtons()
        {
            if (DataContext?.Texture == null)
            {
                EnableRibbon(false);
                return;
            }

            // Temporary
            EnableRibbon(true);

            ButtonNewSprite.Enabled = DataContext.NewSpriteCommand?.CanExecute(null) ?? false;
            ButtonClipSprite.Enabled = DataContext.CurrentTool == SpriteEditTool.SpriteClip || DataContext.CurrentTool == SpriteEditTool.None;
            ButtonPickSprite.Enabled = DataContext?.SpritePickCommand?.CanExecute(null) ?? false;
            ButtonSpriteAnchor.Enabled = DataContext.CurrentTool == SpriteEditTool.SetAnchor || DataContext.CurrentTool == SpriteEditTool.None;
            ButtonSpriteVertexOffsets.Enabled = DataContext.CurrentTool == SpriteEditTool.CornerResize || DataContext.CurrentTool == SpriteEditTool.None;
            ButtonSpriteVertexColors.Enabled = DataContext.CurrentTool == SpriteEditTool.CornerColor || DataContext.CurrentTool == SpriteEditTool.None;
            ButtonSpriteColor.Enabled = DataContext.CurrentTool == SpriteEditTool.None;

            ButtonSpriteUndo.Enabled = DataContext.UndoCommand?.CanExecute(null) ?? false;
            ButtonSpriteRedo.Enabled = DataContext.RedoCommand?.CanExecute(null) ?? false;

            ButtonSaveSprite.Enabled = DataContext.SaveContentCommand?.CanExecute(SaveReason.UserSave) ?? false;
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

            DataContext.PropertyChanging -= DataContext_PropertyChanging;
            DataContext.PropertyChanged -= DataContext_PropertyChanged;
        }

        /// <summary>
        /// Function to reset the view when no data context is assigned.
        /// </summary>
        private void ResetDataContext()
        {
            RibbonSpriteContent.Enabled = false;
            UpdateZoomMenu();
            ItemZoomToWindow.Checked = true;
        }

        /// <summary>Handles the Click event of the ItemZoomToWindow control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [EventArgs] instance containing the event data.</param>
        private void ItemZoom_Click(object sender, EventArgs e)
        {
            var item = (ToolStripMenuItem)sender;

            if ((item.Tag == null) || (!Enum.TryParse(item.Tag.ToString(), out ZoomLevels zoom)))
            {
                item.Checked = false;
                return;
            }

            try
            {
                // Do not let us uncheck.
                if (_zoomLevel == zoom)
                {
                    item.Checked = true;
                    return;
                }

                _zoomLevel = zoom;
                UpdateZoomMenu();
            }
            finally
            {
                ValidateButtons();
            }
        }

        /// <summary>
        /// Function to initialize the view based on the data context.
        /// </summary>
        /// <param name="dataContext">The data context used to initialize.</param>
        private void InitializeFromDataContext(ISpriteContent dataContext)
        {
            if (dataContext == null)
            {
                ResetDataContext();
                return;
            }

            UpdateZoomMenu();
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(ISpriteContent dataContext)
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
        }

        /// <summary>
        /// Function to reset the zoom back to the default.
        /// </summary>
        public void ResetZoom()
        {
            _zoomLevel = ZoomLevels.ToWindow;
            UpdateZoomMenu();
            ValidateButtons();
        }

        /// <summary>
        /// Function to assign a specific zoom level.
        /// </summary>
        /// <param name="zoomLevel">The zoom level to assign.</param>
        public void SetZoom(ZoomLevels zoomLevel)
        {
            _zoomLevel = zoomLevel;
            UpdateZoomMenu();
            ValidateButtons();
        }

        /// <summary>
        /// Function to set the current tool.
        /// </summary>
        /// <param name="tool">The tool to set.</param>
        public void SetTool(SpriteEditTool tool)
        {
            if (DataContext == null)
            {
                return;
            }

            if (tool == DataContext.CurrentTool)
            {
                return;
            }

            switch (DataContext.CurrentTool)
            {
                case SpriteEditTool.CornerResize:
                    if (ButtonSpriteVertexOffsets.Checked)
                    {
                        ButtonSpriteVertexOffsets.PerformClick();
                    }
                    break;
                case SpriteEditTool.SpritePick:
                    if (ButtonPickSprite.Checked)
                    {
                        ButtonPickSprite.PerformClick();
                    }
                    break;
                case SpriteEditTool.SpriteClip:
                    if (ButtonClipSprite.Checked)
                    {
                        ButtonClipSprite.PerformClick();
                    }                    
                    break;
            }
            
            switch (tool)
            {
                case SpriteEditTool.CornerResize:
                    if (ButtonSpriteVertexOffsets.Checked)
                    {
                        ButtonSpriteVertexOffsets.PerformClick();
                    }
                    break;
                case SpriteEditTool.SpritePick:
                    if (ButtonPickSprite.Checked)
                    {
                        ButtonPickSprite.PerformClick();
                    }
                    break;
                case SpriteEditTool.SpriteClip:
                    if (!ButtonClipSprite.Checked)
                    {
                        ButtonClipSprite.PerformClick();
                    }
                    break;
            }            
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the FormRibbon class.</summary>
        public FormRibbon()
        {
            InitializeComponent();

            GetRibbonButtons();

            foreach (ToolStripMenuItem menuItem in MenuZoom.Items.OfType<ToolStripMenuItem>())
            {
                if (!Enum.TryParse(menuItem.Tag.ToString(), out ZoomLevels level))
                {
                    menuItem.Enabled = false;
                    continue;
                }

                menuItem.Text = level.GetName();
                _menuItems[level] = menuItem;
            }
        }
        #endregion
    }
}
