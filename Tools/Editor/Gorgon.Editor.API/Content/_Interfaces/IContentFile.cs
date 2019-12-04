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
// Created: October 30, 2018 7:11:23 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using Gorgon.Core;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.PlugIns;

namespace Gorgon.Editor.Content
{
    /// <summary>
    /// A data structure representing a file containing content.
    /// </summary>
    public interface IContentFile
        : IGorgonNamedObject
    {
        #region Events.
        /// <summary>
        /// Event triggered if this content file was deleted.
        /// </summary>
        event EventHandler Deleted;

        /// <summary>
        /// Event triggered if the content was closed with the <see cref="CloseContent"/> method.
        /// </summary>
        event EventHandler Closed;

        /// <summary>
        /// Event triggered if this content file was renamed.
        /// </summary>
        event EventHandler<ContentFileRenamedEventArgs> Renamed;

        /// <summary>
        /// Event triggered if the dependencies list for this file is updated.
        /// </summary>
        event EventHandler DependenciesUpdated;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the list of dependencies for this content.
        /// </summary>
        IReadOnlyList<IContentFile> Dependencies
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether the file has changes.
        /// </summary>
        bool IsChanged
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the path to the file.
        /// </summary>
        string Path
        {
            get;
        }

        /// <summary>
        /// Property to return the extension for the file.
        /// </summary>
        string Extension
        {
            get;
        }

        /// <summary>
        /// Property to return the plugin associated with the file.
        /// </summary>
        ContentPlugIn ContentPlugIn
        {
            get;
        }

        /// <summary>
        /// Property to return the metadata associated with the file.
        /// </summary>
        ProjectItemMetadata Metadata
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether the file is open for editing or not.
        /// </summary>
        bool IsOpen
        {
            get;
            set;
        }
        #endregion

        #region Methods.  
        /// <summary>
        /// Function to notify that the content should close if it's open.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method will close the content forcefully, that is, it will not prompt to save and any changes will be lost.
        /// </para>
        /// </remarks>
        void CloseContent();

        /// <summary>
        /// Function to link a content file to be dependant upon this content.
        /// </summary>
        /// <param name="child">The child content to link to this content.</param>
        void LinkContent(IContentFile child);

        /// <summary>
        /// Function to unlink a content file from being dependant upon this content.
        /// </summary>
        /// <param name="child">The child content to unlink from this content.</param>
        void UnlinkContent(IContentFile child);

        /// <summary>
        /// Function to remove all child dependency links from this content.
        /// </summary>
        void ClearLinks();

        /// <summary>
        /// Function to open the file for reading.
        /// </summary>
        /// <returns>A stream containing the file data.</returns>
        Stream OpenRead();

        /// <summary>
        /// Function to open the file for writing.
        /// </summary>
        /// <param name="append">[Optional] <b>true</b> to append data to the end of the file, or <b>false</b> to overwrite.</param>
        /// <returns>A stream to write the file data into.</returns>
        [Obsolete("This is on the file system writer, do not use.")]
        Stream OpenWrite(bool append = false);

        /// <summary>
        /// Function to notify that the metadata should be refreshed.
        /// </summary>
        void RefreshMetadata();

        /// <summary>
        /// Function to persist the metadata for content.
        /// </summary>
        void SaveMetadata();

        /// <summary>Function called to refresh the information about the file.</summary>
        void Refresh();
        #endregion
    }
}
