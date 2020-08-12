#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: August 4, 2020 11:21:27 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;

namespace Gorgon.Examples
{
    /// <summary>
    /// Parameters to pass in to the <see cref="ITextContent"/> view model.
    /// </summary>
    /// <remarks>
    /// These are default values and configuration information for our view model. These are used when we initialize the view model 
    /// after we create an instance of the view model. 
    /// 
    /// Basically this provides us with Dependency Injection for the view model.
    /// </remarks>
    internal class TextContentParameters
        : ContentViewModelInjection
    {
        /// <summary>
        /// Property to return the text color interface.
        /// </summary>
        public ITextColor TextColor
        {
            // This is the view model for the text color sub panel. We make it available on the main view model 
            // so that we can set it up to activate its associated sub panel on the UI.
            get;
        }

        /// <summary>
        /// Property to return the editor for text.
        /// </summary>
        public TextEditorService TextEditor
        {
            // This is the service used to modify the text for the text content. We send this to the view model 
            // so that we can activate the service when we request a text change on the content.
            get;
        }
        
        /// <summary>
        /// Property to return the undo/redo service.
        /// </summary>
        public IUndoService UndoService
        {
            // This is the service used to provide undo/redo functionality. Like the text editor service, we 
            // pass this in to the view model so we can track text content changes (specifically the text 
            // itself).
            get;
        }

        /// <summary>
        /// Property to return the settings view model.
        /// </summary>
        public ISettings Settings
        {
            // The view model for the settings for the settings panel. We pass this to our view model so 
            // that we can detect changes to the plug in settings.
            get;
        }

        /// <summary>
        /// Property to return the initial text for the view model.
        /// </summary>
        public string Text
        {
            get;
        }

        /// <summary>Initializes a new instance of the <see cref="TextContentParameters"/> class.</summary>
        /// <param name="text">The initial text for the view model.</param>
        /// <param name="textColor">The text color panel view model.</param>
        /// <param name="settings">The settings panel view model.</param>
        /// <param name="editor">The text editor.</param>
        /// <param name="undoService">The service used to apply undo/redo operations.</param>
        /// <param name="fileManager">The file manager for content files.</param>
        /// <param name="file">The file that contains the content.</param>
        /// <param name="commonServices">The common services for the application.</param>
        /// <exception cref="ArgumentNullException">Thrown when any of the parameters are <b>null</b>.</exception>
        public TextContentParameters(string text, ITextColor textColor, ISettings settings, TextEditorService editor, IUndoService undoService, IContentFileManager fileManager, IContentFile file, IHostContentServices commonServices)
            : base(fileManager, file, commonServices)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            TextColor = textColor ?? throw new ArgumentNullException(nameof(textColor));
            TextEditor = editor ?? throw new ArgumentNullException(nameof(editor));
            UndoService = undoService ?? throw new ArgumentNullException(nameof(undoService));
            Settings = settings ?? throw new ArgumentNullException(nameof(settings));
        }
    }
}
