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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI.Views;

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
        #endregion

        #region Properties.
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
        /*/// <summary>Function to retrieve the view for the content.</summary>
        /// <returns>A UI for the content, must not be <b>null</b>.</returns>
        protected abstract ContentBaseControl OnGetView();

        /// <summary>Function to close the content.</summary>
        public abstract void Close();

        /// <summary>Function to retrieve the view for the content.</summary>
        /// <returns>A UI for the content, must not be <b>null</b>.</returns>
        /// <exception cref="GorgonException">Thrown if no view was found for the content.</exception>
        public ContentBaseControl GetView()
        {
            ContentBaseControl control = OnGetView();

            if (control == null)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GOREDIT_ERR_NO_CONTENT_VIEW);
            }

            return control;
        }*/

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
