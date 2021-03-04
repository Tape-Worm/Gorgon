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
// Created: May 6, 2019 10:14:55 AM
// 
#endregion

using System;
using System.Drawing;
using Gorgon.Editor.Content;

namespace Gorgon.Editor.UI.Controls
{
    /// <summary>
    /// An file system directory entry for the <see cref="ContentFileExplorer"/>.
    /// </summary>
    public class ContentFileExplorerFileEntry
        : PropertyMonitor, IContentFileExplorerSearchEntry
    {
        #region Variables.
        // Flag to indicate that this entry is visible.
        private bool _visible = true;
        // Flag to indicate that the entry is selected.
        private bool _isSelected;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the parent of this entry.
        /// </summary>
        public ContentFileExplorerDirectoryEntry Parent
        {
            get;
        }

        /// <summary>
        /// Property to return the content file associated with this entry.
        /// </summary>
        public IContentFile File
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether this entry is visible in the list or not.
        /// </summary>
        public bool IsVisible
        {
            get => ((Parent is null) || ((Parent.IsVisible) && (Parent.IsExpanded))) && _visible;
            set
            {
                if (_visible == value)
                {
                    return;
                }

                OnPropertyChanging();
                _visible = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return whether the entry is selected or not.
        /// </summary>
        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected == value)
                {
                    return;
                }

                OnPropertyChanging();
                _isSelected = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the name of the entry.
        /// </summary>
        public string Name => File.Name;

        /// <summary>
        /// Property to return the full path to the entry.
        /// </summary>
        public string FullPath => File.Path;

        /// <summary>
        /// Property to return the icon for this image.
        /// </summary>
        public Image FileIcon => File.Metadata?.ContentMetadata?.GetSmallIcon();

        /// <summary>
        /// Property to return the file association type.
        /// </summary>
        public string AssociationType
        {
            get;
        }

        /// <summary>Property to return whether or not this entry is a directory.</summary>
        bool IContentFileExplorerSearchEntry.IsDirectory => false;
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="ContentFileExplorerFileEntry"/> class.</summary>
        /// <param name="contentFile">The content file associated with this entry.</param>
        /// <param name="parent">The parent directory for this file.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="contentFile"/>, or the <paramref name="parent"/> parameter is <b>null</b>.</exception>
        public ContentFileExplorerFileEntry(IContentFile contentFile, ContentFileExplorerDirectoryEntry parent)
        {
            File = contentFile ?? throw new ArgumentNullException(nameof(contentFile));
            Parent = parent ?? throw new ArgumentNullException(nameof(parent));

            AssociationType = string.Empty;

            string contentType = string.Empty;
            if (File.Metadata?.Attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out contentType) ?? false)
            {
                AssociationType = contentType;
            }
        }
        #endregion
    }
}
