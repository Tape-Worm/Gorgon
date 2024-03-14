using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Editor;
using Gorgon.Editor.Content;
using Gorgon.Editor.UI.Controls;
using Gorgon.UI;

namespace Gorgon.Editor.Services;

/// <summary>
/// A service used to present a save as dialog for the project file system.
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="SaveAsService" /> class.</remarks>
/// <param name="fileManager">The file manager for the project.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="fileManager"/> parameter is <b>null</b>.</exception>
public class SaveAsService(IContentFileManager fileManager)
        : ISaveAsService
{
    #region Variables.
    // The file manager for the project.
    private readonly IContentFileManager _fileManager = fileManager ?? throw new ArgumentNullException(nameof(fileManager));
    #endregion

    #region Methods.
    /// <summary>
    /// Function to present a means of providing a path for a save as operation.
    /// </summary>
    /// <param name="currentFileName">The current file name.</param>
    /// <param name="filesOfType">The file type to search for.</param>
    /// <param name="typeKey">[Optional] The key to check in the file metadata for the file type.</param>
    /// <returns>The selected file path to save the file as.</returns>
    public string SaveAs(string currentFileName, string filesOfType, string typeKey = CommonEditorConstants.ContentTypeAttr)
    {
        FormSaveDialog saveDialog = new();
        try
        {
            string directoryName = Path.GetDirectoryName(currentFileName);
            string fileName = Path.GetFileName(currentFileName);

            saveDialog.CurrentDirectory = !string.IsNullOrEmpty(directoryName) ? directoryName : "/";
            saveDialog.CurrentFileName = fileName;

            saveDialog.FileManager = _fileManager;
            saveDialog.FileTypeFilter = (typeKey, filesOfType);

            if (saveDialog.ShowDialog(GorgonApplication.MainForm) != DialogResult.OK)
            {
                return null;
            }

            return saveDialog.SelectedFilePath;
        }
        finally
        {
            saveDialog.Dispose();
        }
    }

    #endregion
}
