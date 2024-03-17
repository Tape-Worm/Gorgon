
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 27, 2018 8:51:39 PM
// 


using System.Security;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.ProjectData;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.IO;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// A view model for the new project interface
/// </summary>
internal class NewProject
    : ViewModelBase<NewProjectParameters, IHostContentServices>, INewProject
{

    // The list of invalid characters for a directory.
    private readonly IEnumerable<char> _invalidDirCharacters;

    // The title for the project.
    private string _title = Resources.GOREDIT_NEW_PROJECT;
    // The project manager used to build up project info.
    private ProjectManager _projectManager;
    // The computer information object.
    private IGorgonComputerInfo _computerInfo;
    // The currently active project.
    private IProjectEditor _current;
    // The active GPU name.
    private string _gpuName = Resources.GOREDIT_TEXT_UNKNOWN;
    // The path where the project will be stored.
    private DirectoryInfo _workspace;
    // The settings for the application.
    private Editor.EditorSettings _settings;
    // The directory locator service.
    private IDirectoryLocateService _directoryLocator;
    // Available drive space, in bytes.
    private ulong _availableSpace;
    // The reason that the entered path is invalid.
    private string _invalidPathReason;
    // The default path for projects.
    private readonly string _defaultProjectPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "Gorgon", "Projects");



    /// <summary>Property to return the reason that a workspace path may be invalid.</summary>
    public string InvalidPathReason
    {
        get => _invalidPathReason;
        set
        {
            if (string.Equals(_invalidPathReason, value, StringComparison.CurrentCultureIgnoreCase))
            {
                return;
            }

            OnPropertyChanging();
            _invalidPathReason = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the title for the project.
    /// </summary>
    public string Title
    {
        get => string.IsNullOrWhiteSpace(_title) ? Resources.GOREDIT_NEW_PROJECT : _title;
        private set
        {
            if (string.Equals(_title, value, StringComparison.CurrentCulture))
            {
                return;
            }

            OnPropertyChanging();
            _title = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the workspace path.
    /// </summary>
    public DirectoryInfo WorkspacePath
    {
        get => _workspace;
        private set
        {
            if (string.Equals(_workspace?.FullName, value?.FullName, StringComparison.OrdinalIgnoreCase))
            {
                return;
            }

            OnPropertyChanging();
            _workspace = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the command to execute when the project should be created.
    /// </summary>
    public IEditorCommand<object> CreateProjectCommand
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the available RAM, in bytes.
    /// </summary>
    public ulong AvailableRam => (ulong)_computerInfo.AvailablePhysicalRAM;

    /// <summary>
    /// Property to return the available drive space on the drive that hosts the working folder.
    /// </summary>
    public ulong AvailableDriveSpace
    {
        get => _availableSpace;
        private set
        {
            if (_availableSpace == value)
            {
                return;
            }

            OnPropertyChanging();
            _availableSpace = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the active GPU name.
    /// </summary>
    public string GPUName
    {
        get => _gpuName;
        set
        {
            if (string.Equals(_gpuName, value, StringComparison.CurrentCulture))
            {
                return;
            }

            OnPropertyChanging();
            _gpuName = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to set or return the currently active project.
    /// </summary>
    public IProjectEditor CurrentProject
    {
        get => _current;
        set
        {
            if (_current == value)
            {
                return;
            }

            OnPropertyChanging();
            _current = value;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the command to execute when the project workspace is set.
    /// </summary>
    public IEditorCommand<SetProjectWorkspaceArgs> SetProjectWorkspaceCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to execute when the project workspace should be selected.
    /// </summary>
    public IEditorCommand<object> SelectProjectWorkspaceCommand
    {
        get;
    }



    /// <summary>
    /// Function to validate the selected workspace directory.
    /// </summary>
    /// <param name="directory">The directory to validate.</param>
    /// <returns><b>true</b> if the directory is valid, <b>false</b> if not.</returns>
    private bool ValidateDirectory(string directory)
    {
        string[] forbiddenPaths = ((Environment.SpecialFolder[])Enum.GetValues(typeof(Environment.SpecialFolder)))
                                    .Select(item => Environment.GetFolderPath(item).FormatDirectory(Path.DirectorySeparatorChar))
                                    .Where(item => !string.IsNullOrWhiteSpace(item))
                                    .Distinct(StringComparer.OrdinalIgnoreCase)
                                    .ToArray();
        directory = Path.GetFullPath(directory);

        if (!System.IO.Directory.Exists(directory))
        {
            return true;
        }

        // Do not allow us to write to the main windows or system folders, that'd be bad.
        if ((forbiddenPaths.Any(item => string.Equals(directory, item, StringComparison.OrdinalIgnoreCase)))
            || (System.IO.Directory.GetParent(directory) is null))
        {
            InvalidPathReason = string.Format(Resources.GOREDIT_ERR_NOT_AUTHORIZED, directory.Ellipses(45, true));
            return false;
        }

        Stream lockStream = null;

        try
        {
            string[] itemsInDir = System.IO.Directory.GetFileSystemEntries(directory, "*");

            if ((itemsInDir.Length > 0) && (HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_WORKSPACE_PATH_EXISTS, directory)) != MessageResponse.Yes))
            {
                InvalidPathReason = string.Format(Resources.GOREDIT_ERR_WORKSPACE_EXISTS, directory.Ellipses(45, true));
                return false;
            }

            string tempDir = Path.Combine(directory, Guid.NewGuid().ToString("N"));
            System.IO.Directory.CreateDirectory(tempDir);
            System.IO.Directory.Delete(tempDir);
        }
        catch (UnauthorizedAccessException)
        {
            InvalidPathReason = string.Format(Resources.GOREDIT_ERR_NOT_AUTHORIZED, directory.Ellipses(45, true));
            return false;
        }
        catch (SecurityException)
        {
            InvalidPathReason = string.Format(Resources.GOREDIT_ERR_NOT_AUTHORIZED, directory.Ellipses(45, true));
            return false;
        }
        catch (IOException ioEx)
        {
            InvalidPathReason = string.Format(ioEx.Message, directory.Ellipses(45, true));
            return false;
        }
        finally
        {
            lockStream?.Dispose();
        }

        if (_projectManager.IsDirectoryLocked(directory))
        {
            InvalidPathReason = string.Format(Resources.GOREDIT_ERR_PROJECT_OPEN_LOCKED, directory.Ellipses(45, true));
            return false;
        }

        return true;
    }

    /// <summary>
    /// Function called when a workspace directory is selected by the directory picker.
    /// </summary>
    private void DoSelectProjectWorkspace()
    {
        string lastPath = string.Empty;

        try
        {
            lastPath = string.IsNullOrWhiteSpace(_settings.LastProjectWorkingDirectory) ? null : Path.GetFullPath(_settings.LastProjectWorkingDirectory);

            if ((lastPath is null) || (!System.IO.Directory.Exists(lastPath)))
            {
                if (!string.IsNullOrWhiteSpace(lastPath))
                {
                    DirectoryInfo parentDir = new(lastPath);

                    if (parentDir.Parent is not null)
                    {
                        lastPath = parentDir.FullName;
                    }
                    else
                    {
                        lastPath = null;
                    }
                }

                if (lastPath is null)
                {
                    lastPath = _defaultProjectPath;
                    System.IO.Directory.CreateDirectory(lastPath);
                }
            }

            lastPath = Path.GetFullPath(lastPath);

            // Check for read access.                
            while (true)
            {
                try
                {
                    System.IO.Directory.GetFileSystemEntries(lastPath, "*");
                    break;
                }
                catch (UnauthorizedAccessException)
                {
                    lastPath = System.IO.Directory.GetParent(lastPath)?.FullName;

                    if (string.IsNullOrWhiteSpace(lastPath))
                    {
                        lastPath = _defaultProjectPath;
                        break;
                    }
                }
            }

            InvalidPathReason = null;

            DirectoryInfo dir = _directoryLocator.GetDirectory(new DirectoryInfo(lastPath), Resources.GOREDIT_CAPTION_SELECT_PROJECT_DIR);

            if (dir is null)
            {
                return;
            }

            WorkspacePath = dir;

            if (!ValidateDirectory(dir.FullName))
            {
                return;
            }

            Title = dir.Name;

            InvalidPathReason = null;
            AvailableDriveSpace = (ulong)(new DriveInfo(Path.GetPathRoot(dir.FullName))).AvailableFreeSpace;
            _settings.LastProjectWorkingDirectory = dir.Parent.FullName.FormatDirectory(Path.DirectorySeparatorChar);
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GOREDIT_ERR_CANNOT_SET_WORKSPACE, lastPath ?? string.Empty));
            InvalidPathReason = ex.Message;
        }
    }

    /// <summary>
    /// Function called to determine if the project workspace can be set.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    /// <returns><b>true</b> if the project workspace can be set, or <b>false</b> if not.</returns>
    private bool CanSetProjectWorkspace(SetProjectWorkspaceArgs args) => args is not null;

    /// <summary>
    /// Function to set the project workspace.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    private void DoSetProjectWorkspace(SetProjectWorkspaceArgs args)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(args.WorkspacePath))
            {
                WorkspacePath = null;
                Title = null;
                InvalidPathReason = null;
                return;
            }

            string doubledPathSeparator = Path.DirectorySeparatorChar.ToString() + Path.DirectorySeparatorChar.ToString();
            string doubleVolumeSeparator = Path.VolumeSeparatorChar.ToString() + Path.VolumeSeparatorChar.ToString();

            string path = args.WorkspacePath.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);

            // Extract out doubled up separators.
            while ((path.Contains(doubledPathSeparator)) || (path.Contains(doubleVolumeSeparator)))
            {
                path = path.Replace(doubledPathSeparator, Path.DirectorySeparatorChar.ToString());
                path = path.Replace(doubleVolumeSeparator, Path.VolumeSeparatorChar.ToString());
            }

            // Remove trailing separator if we've supplied one.
            path = path.TrimEnd(Path.DirectorySeparatorChar);

            // For a path, we can skip the directory separator.
            foreach (char c in path)
            {
                // Skip the separators, they're ok.
                if ((c == Path.DirectorySeparatorChar) || (c == Path.VolumeSeparatorChar))
                {
                    continue;
                }

                if (_invalidDirCharacters.Any(item => item == c))
                {
                    InvalidPathReason = string.Format(Resources.GOREDIT_ERR_INVALID_PATH, args.WorkspacePath, string.Join(", ", _invalidDirCharacters
                        .Where(item => (item != Path.VolumeSeparatorChar) && (item != Path.DirectorySeparatorChar) && (item != Path.AltDirectorySeparatorChar))
                        .Select(item => item.ToString())));
                    return;
                }
            }

            // If we just a drive letter (e.g. "C:"), then restore the path separator so we can treat it as the root of the drive.
            if (path[^1] == Path.VolumeSeparatorChar)
            {
                path += Path.DirectorySeparatorChar;
            }

            path = Path.GetFullPath(path);
            int lastSep = path.LastIndexOf(Path.DirectorySeparatorChar);
            int volSep = path.IndexOf(Path.VolumeSeparatorChar);

            // If we just entered a name, then get the current directory, and use that with the path entered.
            if ((lastSep == -1) || (volSep == -1))
            {
                path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), path);
            }

            DirectoryInfo dir = new(path);

            if (!ValidateDirectory(dir.FullName))
            {
                return;
            }

            // Get the title.
            if (lastSep == -1)
            {
                Title = path;
            }
            else
            {
                Title = path[(lastSep + 1)..];
            }

            InvalidPathReason = null;
            WorkspacePath = dir;
            AvailableDriveSpace = (ulong)(new DriveInfo(Path.GetPathRoot(dir.FullName))).AvailableFreeSpace;
            _settings.LastProjectWorkingDirectory = dir.Parent.FullName.FormatDirectory(Path.DirectorySeparatorChar);
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GOREDIT_ERR_CANNOT_SET_WORKSPACE, args.WorkspacePath));
            InvalidPathReason = ex.Message;
        }
    }

    /// <summary>
    /// Function to inject dependencies for the view model.
    /// </summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
    /// <remarks>Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.</remarks>
    protected override void OnInitialize(NewProjectParameters injectionParameters)
    {
        _settings = injectionParameters.EditorSettings;
        _projectManager = injectionParameters.ProjectManager;
        _directoryLocator = injectionParameters.DirectoryLocator;

        string lastWorkspace = Path.GetFullPath(string.IsNullOrWhiteSpace(_settings.LastProjectWorkingDirectory) ? _defaultProjectPath : _settings.LastProjectWorkingDirectory);

        if (!System.IO.Directory.Exists(lastWorkspace))
        {
            lastWorkspace = _defaultProjectPath;

            if (!System.IO.Directory.Exists(lastWorkspace))
            {
                System.IO.Directory.CreateDirectory(lastWorkspace);
            }
        }

        _availableSpace = (ulong)(new DriveInfo(Path.GetPathRoot(lastWorkspace))).AvailableFreeSpace;
        _computerInfo = new GorgonComputerInfo();
    }



    /// <summary>Initializes a new instance of the <see cref="NewProject"/> class.</summary>
    public NewProject()
    {
        _invalidDirCharacters = Path.GetInvalidFileNameChars();
        SetProjectWorkspaceCommand = new EditorCommand<SetProjectWorkspaceArgs>(DoSetProjectWorkspace, CanSetProjectWorkspace);
        SelectProjectWorkspaceCommand = new EditorCommand<object>(DoSelectProjectWorkspace);
    }

}
