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
// Created: October 29, 2018 9:04:44 PM
// 
#endregion

using System;
using System.Threading.Tasks;
using System.Drawing;
using Gorgon.Editor.Content;
using Gorgon.Graphics.Imaging;
using System.IO;
using System.Threading;

namespace Gorgon.Editor.Plugins
{
    /// <summary>
    /// Provides metadata for a plugin.
    /// </summary>
    public interface IContentPluginMetadata
    {
        #region Properties.
        /// <summary>
        /// Property to return the name of the plug in.
        /// </summary>
        string PluginName
        {
            get;
        }

        /// <summary>
        /// Property to return the user friendly description of the plug in.
        /// </summary>
        string Description
        {
            get;
        }

        /// <summary>
        /// Property to return the ID of the small icon for this plug in.
        /// </summary>
        Guid SmallIconID
        {
            get;
        }

        /// <summary>
        /// Property to return the ID of the new icon for this plug in.
        /// </summary>
        Guid NewIconID
        {
            get;
        }

        /// <summary>
        /// Property to return whether content creation is possible.
        /// </summary>
        bool CanCreateContent
        {
            get;
        }

        /// <summary>
        /// Property to return the ID for the type of content produced by this plug in.
        /// </summary>
        string ContentTypeID
        {
            get;
        }

        /// <summary>
        /// Property to return the friendly (i.e shown on the UI) name for the type of content.
        /// </summary>
        string ContentType
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve a thumbnail for the content.
        /// </summary>
        /// <param name="contentFile">The content file used to retrieve the data to build the thumbnail with.</param>
        /// <param name="fileManager">The content file manager.</param>
        /// <param name="outputFile">The output file for the thumbnail data.</param>
        /// <param name="cancelToken">The token used to cancel the thumbnail generation.</param>
        /// <returns>A <see cref="IGorgonImage"/> containing the thumbnail image data.</returns>
        Task<IGorgonImage> GetThumbnailAsync(IContentFile contentFile, IContentFileManager fileManager, FileInfo outputFile, CancellationToken cancelToken);

        /// <summary>
        /// Function to determine if the content plugin can open the specified file.
        /// </summary>
        /// <param name="file">The content file to evaluate.</param>
        /// <param name="fileManager">The content file manager.</param>
        /// <returns><b>true</b> if the plugin can open the file, or <b>false</b> if not.</returns>
        bool CanOpenContent(IContentFile file, IContentFileManager fileManager);

        /// <summary>
        /// Function to retrieve the small icon for the content plug in.
        /// </summary>
        /// <returns>An image for the small icon.</returns>
        Image GetSmallIcon();

        /// <summary>
        /// Function to retrieve the icon used for new content creation.
        /// </summary>
        /// <returns>An image for the icon.</returns>
        /// <remarks>
        /// <para>
        /// This method is never called when <see cref="CanCreateContent"/> is <b>false</b>.
        /// </para>
        /// </remarks>
        Image GetNewIcon();
        #endregion
    }
}
