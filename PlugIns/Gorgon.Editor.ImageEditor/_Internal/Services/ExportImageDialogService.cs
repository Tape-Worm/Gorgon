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

using Gorgon.Editor.Content;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// Dialog serivce used for retrieving paths for exporting image data.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="ExportImageDialogService"/> class.</remarks>
/// <param name="settings">The settings.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="settings" /> parameter is <strong>null</strong>.</exception>
internal class ExportImageDialogService(ISettings settings)
        : FileSaveDialogService, IExportImageDialogService
{
    #region Variables.
    // The settings for the image editor.
    private readonly ISettings _settings = settings ?? throw new ArgumentNullException(nameof(settings));
    #endregion

    #region Properties.
    /// <summary>Property to set or return the codec used for exporting.</summary>
    public IGorgonImageCodec SelectedCodec
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the content image file being exported.
    /// </summary>
    public IContentFile ContentFile
    {
        get;
        set;
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
        if ((SelectedCodec is null) || (ContentFile is null))
        {
            FileFilter = string.Empty;
            DialogTitle = string.Empty;
            InitialDirectory = new DirectoryInfo(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments));
            InitialFilePath = string.Empty;
            return;
        }

        string exportFileName = ContentFile.Name;

        if (SelectedCodec.CodecCommonExtensions.Count > 0)
        {
            exportFileName = ContentFile.Name + "." + SelectedCodec.CodecCommonExtensions[0];
        }

        IEnumerable<string> extensions = SelectedCodec.CodecCommonExtensions.Distinct(StringComparer.CurrentCultureIgnoreCase).Select(item => $"*.{item}");
        FileFilter = $"{SelectedCodec.CodecDescription} ({string.Join(", ", extensions)})|{string.Join(";", extensions)}";
        DialogTitle = string.Format(Resources.GORIMG_CAPTION_EXPORT_IMAGE, SelectedCodec.Codec);
        InitialDirectory = GetLastImportExportPath();
        InitialFilePath = exportFileName;
    }

    /// <summary>Function to retrieve a single file name.</summary>
    /// <returns>The selected file path, or <b>null</b> if cancelled.</returns>
    public override string GetFilename()
    {
        ConfigureDialog();
        return base.GetFilename();
    }

    #endregion
    #region Constructor.
    #endregion
}
