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
// Created: January 5, 2019 1:44:44 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// Dialog serivce used for retrieving paths for importing image data.
    /// </summary>
    internal class ImportImageDialogService
        : FileOpenDialogService, IImportImageDialogService
    {
        #region Variables.
        // The settings for the image editor.
        private readonly ISettings _settings;
        // The codecs available to the importer.
        private readonly ICodecRegistry _codecs;
        #endregion

        #region Properties.
        /// <summary>Property to return the codec used for exporting.</summary>
        public IGorgonImageCodec SelectedCodec
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to retrieve the last directory path used for import/export.
        /// </summary>
        /// <returns>The directory last used for import/export.</returns>
        private DirectoryInfo GetLastImportExportPath()
        {
            DirectoryInfo result;

            string importExportPath = _settings.LastImportExportPath.FormatDirectory(Path.DirectorySeparatorChar);

            if (string.IsNullOrWhiteSpace(importExportPath))
            {
                result = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            }
            else
            {
                result = new DirectoryInfo(importExportPath);

                if (!result.Exists)
                {
                    result = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
                }
            }

            if (!result.Exists)
            {
                result = new DirectoryInfo(AppDomain.CurrentDomain.BaseDirectory);
            }

            return result;
        }

        /// <summary>
        /// Function to configure the file dialog.
        /// </summary>
        private void ConfigureDialog()
        {
            var fileFilter = new StringBuilder();
            foreach (IGorgonImageCodec codec in _codecs.Codecs)
            {
                IEnumerable<string> extensions = codec.CodecCommonExtensions.Distinct(StringComparer.CurrentCultureIgnoreCase).Select(item => $"*.{item}");

                if (fileFilter.Length > 0)
                {
                    fileFilter.Append("|");
                }

                fileFilter.AppendFormat("{0} ({1})|{2}", codec.CodecDescription, string.Join(", ", extensions), string.Join(";", extensions));
            }

            if (fileFilter.Length > 0)
            {
                fileFilter.Append("|");
            }

            fileFilter.Append(Resources.GORIMG_FILEMASK_ALL_FILES);

            FileFilter = fileFilter.ToString();
            DialogTitle = Resources.GORIMG_CAPTION_IMPORT_IMAGE;
            InitialDirectory = GetLastImportExportPath();                                    
        }

        /// <summary>Function to retrieve a single file name.</summary>
        /// <returns>The selected file path, or <b>null</b> if cancelled.</returns>
        public override string GetFilename()
        {            
            ConfigureDialog();
            string filePath = base.GetFilename();

            if (string.IsNullOrWhiteSpace(filePath))
            {
                return null;
            }

            var extension = new GorgonFileExtension(Path.GetExtension(filePath));

            SelectedCodec = null;

            // Check for an extension match on the codec list.
            foreach (IGorgonImageCodec codec in _codecs.Codecs.Where(item => (item.CodecCommonExtensions.Count > 0) && (item.CanDecode)))
            {
                if (codec.CodecCommonExtensions.Any(item => extension == new GorgonFileExtension(item)))
                {
                    SelectedCodec = codec;
                    break;
                }
            }

            return filePath;
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.ImageEditor.ImportImageDialogService"/> class.</summary>
        /// <param name="settings">The settings.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="settings" />, or the <paramref name="codecs"/> parameter is <strong>null</strong>.</exception>
        public ImportImageDialogService(ISettings settings, ICodecRegistry codecs)
        {
            _settings = settings ?? throw new ArgumentNullException(nameof(settings));
            _codecs = codecs ?? throw new ArgumentNullException(nameof(codecs));
        }
        #endregion
    }
}
