#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: October 29, 2018 4:15:09 PM
// 
#endregion

using System;
using System.IO;
using Gorgon.Editor.Content;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.IO;

namespace Gorgon.Editor.UI
{
    /// <summary>
    /// Common functionality for the editor content view model.
    /// </summary>
    public abstract class EditorContentCommon<T>
        : ViewModelBase<T>, IEditorContent
        where T : class, IContentViewModelInjection
    {
        #region Constants.
        /// <summary>
        /// The attribute name for the content type attribute.
        /// </summary>
        public const string ContentTypeAttr = "Type";
        #endregion

        #region Events.
        /// <summary>Event to notify the view that the content should close.</summary>
        public event EventHandler CloseContent;
        #endregion

        #region Variables.
        // The command used to close the content.
        private IEditorCommand<object> _closeCommand;

        // The file for the content.
        private IContentFile _file;

        // The current content state.
        private ContentState _state = ContentState.New;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the scratch area file system.
        /// </summary>
        protected IGorgonFileSystem ScratchArea
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the scratch area file system writer.
        /// </summary>
        protected IGorgonFileSystemWriter<Stream> ScratchWriter
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the message display service.
        /// </summary>
        protected IMessageDisplayService MessageDisplay
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the busy state service.
        /// </summary>
        protected IBusyStateService BusyState
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the current content state.
        /// </summary>
        public ContentState ContentState
        {
            get => _state;
            set
            {
                if ((_state == value) || ((_state == ContentState.New) && (value != ContentState.Unmodified)))
                {
                    return;
                }

                OnPropertyChanging();
                _state = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the file for the content.
        /// </summary>
        public IContentFile File
        {
            get => _file;
            private set
            {
                if (_file == value)
                {
                    return;
                }

                OnPropertyChanging();
                _file = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the type of content.
        /// </summary>
        public abstract string ContentType
        {
            get;
        }

        /// <summary>Property to set or return the command used to close the content.</summary>
        public IEditorCommand<object> CloseContentCommand
        {
            get => _closeCommand;
            set
            {
                if (_closeCommand == value)
                {
                    return;
                }

                OnPropertyChanging();
                _closeCommand = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Methods.
        // TODO: Maybe change this to a cancel event.
        /// <summary>Handles the Excluded event of the File control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [EventArgs] instance containing the event data.</param>
        private void File_Excluded(object sender, EventArgs e) => DoCloseContent();

        /// <summary>Handles the Deleted event of the File control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The [EventArgs] instance containing the event data.</param>
        private void File_Deleted(object sender, EventArgs e) => DoCloseContent();

        /// <summary>Handles the Moved event of the File control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The ContentFileMovedEventArgs instance containing the event data.</param>
        private void File_Moved(object sender, ContentFileMovedEventArgs e)
        {
            try
            {
                _file.Excluded -= File_Excluded;
                _file.Moved -= File_Moved;
                _file.Deleted -= File_Deleted;

                File = e.NewFile;

                _file.Moved += File_Moved;
                _file.Deleted += File_Deleted;
                _file.Excluded += File_Excluded;
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GOREDIT_ERR_AFTER_MOVE_UPDATE);
                DoCloseContent();
            }
        }

        /// <summary>
        /// Function called to close the content.
        /// </summary>
        private void DoCloseContent()
        {
            BusyState.SetBusy();

            try
            {
                EventHandler handler = CloseContent;
                CloseContent?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                MessageDisplay.ShowError(ex, Resources.GOREDIT_ERR_CLOSE_CONTENT);
            }
            finally
            {
                BusyState.SetIdle();
            }
        }               

        /// <summary>
        /// Function to initialize the content.
        /// </summary>
        /// <param name="injectionParameters">Common view model dependency injection parameters from the application.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="injectionParameters"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentMissingException">Thrown when a required property on the <paramref name="injectionParameters"/> is <b>null</b>.</exception>
        protected override void OnInitialize(T injectionParameters)
        {
            if (injectionParameters == null)
            {
                throw new ArgumentNullException(nameof(injectionParameters));
            }

            _file = injectionParameters.File ?? throw new ArgumentMissingException(nameof(IContentViewModelInjection.File), nameof(injectionParameters));
            MessageDisplay = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(IViewModelInjection.MessageDisplay), nameof(injectionParameters));
            BusyState = injectionParameters.BusyService ?? throw new ArgumentMissingException(nameof(IViewModelInjection.BusyService), nameof(injectionParameters));

            if (!string.IsNullOrWhiteSpace(ContentType))
            {
                _file.Metadata.Attributes[ContentTypeAttr] = ContentType;
            }

            _file.Moved += File_Moved;
            _file.Deleted += File_Deleted;
            _file.Excluded += File_Excluded;

            ScratchArea = injectionParameters.ScratchArea ?? throw new ArgumentMissingException(nameof(IContentViewModelInjection.ScratchArea), nameof(injectionParameters));
            ScratchWriter = injectionParameters.ScratchWriter ?? throw new ArgumentMissingException(nameof(IContentViewModelInjection.ScratchWriter), nameof(injectionParameters));            
        }

        /// <summary>Function called when the associated view is unloaded.</summary>
        public override void OnUnload()
        {
            // TODO: This should get marked when we commit the file data back to the file system.
            // _file.IsChanged = true;
            _file.Moved -= File_Moved;
            _file.Deleted -= File_Deleted;
            _file.Excluded -= File_Excluded;
            _file.IsOpen = false;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the EditorContentCommon class.</summary>
        protected EditorContentCommon()
        {
            CloseContentCommand = new EditorCommand<object>(DoCloseContent);
        }
        #endregion
    }
}
