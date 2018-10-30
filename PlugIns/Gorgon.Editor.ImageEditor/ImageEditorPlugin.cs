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
// Created: October 29, 2018 2:52:09 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Drawing;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.Plugins;
using Gorgon.Editor.Services;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// Gorgon image editor content plug-in interface.
    /// </summary>
    internal class ImageEditorPlugin
        : ContentPlugin, IContentPluginMetadata
    {
        #region Variables.
        // The loaded image codecs.
        private List<IGorgonImageCodec> _codecList = new List<IGorgonImageCodec>();

        // The list of available codecs matched by extension.
        private readonly List<(GorgonFileExtension extension, IGorgonImageCodec codec)> _codecs = new List<(GorgonFileExtension extension, IGorgonImageCodec codec)>();
        #endregion

        #region Properties.
        /// <summary>Property to return the ID of the small icon for this plug in.</summary>
        public Guid SmallIconID 
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>Function called to create a content object from this plugin.</summary>
        /// <returns>A new IEditorContent object.</returns>
        protected override IEditorContent OnCreateContent(IGorgonLog log)
        {
            return new TestContent();
        }

        /// <summary>Function to provide initialization for the plugin.</summary>
        /// <param name="pluginService">The plugin service used to access other plugins.</param>
        /// <remarks>This method is only called when the plugin is loaded at startup.</remarks>
        protected override void OnInitialize(IContentPluginService pluginService)
        {
            // Get built-in codec list.
            _codecList.Add(new GorgonCodecPng());
            _codecList.Add(new GorgonCodecJpeg());
            _codecList.Add(new GorgonCodecDds());
            _codecList.Add(new GorgonCodecTga());
            _codecList.Add(new GorgonCodecBmp());
            _codecList.Add(new GorgonCodecGif());

            foreach (IGorgonImageCodec codec in _codecList)
            {
                foreach (string extension in codec.CodecCommonExtensions)
                {
                    _codecs.Add((new GorgonFileExtension(extension), codec));
                }
            }

            // TODO: We need to load configuration settings for the plugin.
        }

        /// <summary>Function to determine if the content plugin can open the specified file.</summary>
        /// <param name="filePath">The path to the file.</param>
        /// <returns><b>true</b> if the plugin can open the file, or <b>false</b> if not.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="filePath"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="filePath"/> parameter is empty.</exception>
        public bool CanOpenContent(string filePath)
        {
            if (filePath == null)
            {
                throw new ArgumentNullException(nameof(filePath));
            }

            if (string.IsNullOrWhiteSpace(filePath))
            {
                throw new ArgumentEmptyException(nameof(filePath));
            }

            var file = new FileInfo(filePath);

            if (!file.Exists)
            {
                return false;
            }

            // First, locate the extension.
            if (!string.IsNullOrWhiteSpace(file.Extension))
            {
                var extension = new GorgonFileExtension(file.Extension);

                if (_codecs.Any(item => item.extension == extension))
                {
                    return true;
                }
            }

            // If that failed, then test to see if the file can be opened by the codec, this is more intensive because we have file access.
            foreach (IGorgonImageCodec codec in _codecList)
            {
                using (Stream stream = file.OpenRead())
                {
                    if (codec.IsReadable(stream))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        /// <summary>
        /// Function to retrieve the small icon for the content plug in.
        /// </summary>
        /// <returns>An image for the small icon.</returns>
        public Image GetSmallIcon() => Resources.image_16x16;
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the ImageEditorPlugin class.</summary>
        public ImageEditorPlugin()
            : base(Resources.GORIMG_DESC)
        {
            SmallIconID = Guid.NewGuid();
        }
        #endregion
    }
}
