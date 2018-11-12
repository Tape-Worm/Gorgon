using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using ComponentFactory.Krypton.Toolkit;
using Gorgon.Editor.ImageEditor.ViewModels;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// Provides a ribbon interface for the plug in view.
    /// </summary>
    /// <remarks>
    /// We cannot provide a ribbon on the control directly. For some reason, the krypton components will only allow ribbons on forms.
    /// </remarks>
    internal partial class FormRibbon : KryptonForm
    {
        // The data context for the ribbon.
        private IImageContent _dataContext;

        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [PropertyChangedEventArgs] instance containing the event data.</param>
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e) => ValidateButtons();

        /// <summary>Handles the PropertyChanging event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [PropertyChangingEventArgs] instance containing the event data.</param>
        private void DataContext_PropertyChanging(object sender, PropertyChangingEventArgs e)
        {

        }

        /// <summary>
        /// Function to validate the state of the buttons.
        /// </summary>
        private void ValidateButtons()
        {
            if (DataContext == null)
            {
                TabImage.Visible = false;
                return;
            }
                        
            ButtonSaveImage.Enabled = DataContext.ContentState != UI.ContentState.Unmodified;
        }

        /// <summary>
        /// Property to set or return the data context for the ribbon on the form.
        /// </summary>
        public IImageContent DataContext
        {
            get => _dataContext;
            set
            {
                if (_dataContext != null)
                {
                    _dataContext.PropertyChanging -= DataContext_PropertyChanging;
                    _dataContext.PropertyChanged -= DataContext_PropertyChanged;
                }

                _dataContext = value;

                if (_dataContext != null)
                {
                    _dataContext.PropertyChanging += DataContext_PropertyChanging;
                    _dataContext.PropertyChanged += DataContext_PropertyChanged;
                }

                ValidateButtons();
            }
        }

        /// <summary>Handles the Click event of the ButtonCancelChanges control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [EventArgs] instance containing the event data.</param>
        private void ButtonCancelChanges_Click(object sender, EventArgs e)
        {
            if ((DataContext?.CloseContentCommand == null)
                || (!DataContext.CloseContentCommand.CanExecute(null)))
            {
                return;
            }

            DataContext.CloseContentCommand.Execute(null);
        }

        /// <summary>Initializes a new instance of the FormRibbon class.</summary>
        public FormRibbon() => InitializeComponent();
    }
}
