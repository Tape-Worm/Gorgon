﻿
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
// Created: September 4, 2018 10:43:51 PM
// 

using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using Gorgon.Collections;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Content;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;
using Gorgon.IO;
using Gorgon.Timing;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// The file explorer view model
/// </summary>
internal class FileExplorer
    : ViewModelBase<FileExplorerParameters, IHostContentServices>, IFileExplorer, IContentFileManager
{

    // The amount of time, in milliseconds, to pause an operation so the user can cancel the operation.
    private const int MaxUserInteractionTimeMilliseconds = 50;
    private const int MinUserInteractionTimeMilliseconds = 5;

    // Internal event for the file system updated event.
    private event EventHandler FileSystemUpdatedEvent;
    // Event triggered when the SelectedFileCount changes.
    private event EventHandler SelectedFilesChangedEvent;

    /// <summary>Event triggered when the selected files change.</summary>
    event EventHandler IContentFileManager.SelectedFilesChanged
    {
        add
        {
            lock (_selectedChangedEventLock)
            {
                if (value is null)
                {
                    SelectedFilesChangedEvent = null;
                    return;
                }

                SelectedFilesChangedEvent += value;
            }
        }
        remove
        {
            lock (_selectedChangedEventLock)
            {
                if (value is null)
                {
                    return;
                }

                SelectedFilesChangedEvent -= value;
            }
        }
    }

    /// <summary>
    /// Event triggered when the file system has been updated.
    /// </summary>
    public event EventHandler FileSystemUpdated
    {
        add
        {
            lock (_fsUpdatedEventLock)
            {
                if (value is null)
                {
                    return;
                }

                FileSystemUpdatedEvent += value;
            }
        }
        remove
        {
            lock (_fsUpdatedEventLock)
            {
                if (value is null)
                {
                    FileSystemUpdatedEvent = null;
                    return;
                }

                FileSystemUpdatedEvent -= value;
            }
        }
    }

    // The synchronization locks for the file system events.
    private readonly object _fsUpdatedEventLock = new();
    private readonly object _selectedChangedEventLock = new();
    // The project file system and writer.
    private IGorgonFileSystemWriter<FileStream> _fileSystemWriter;
    // The directory locator dialog service.
    private IDirectoryLocateService _directoryLocator;
    // The factory used to build view models.
    private ViewModelFactory _factory;
    // The root directory.
    private IDirectory _root;
    // The selected directory.
    private IDirectory _selectedDir;
    // Cached list of directories and files (referenced by full path).
    private readonly Dictionary<string, IDirectory> _directories = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, IFile> _files = new(StringComparer.OrdinalIgnoreCase);
    // The main thread synchronization context.
    private SynchronizationContext _syncContext;
    // The service used to search through the file system.
    private ISearchService<IFile> _searchService;
    // The list of files returned by the search functionality.
    private List<IFile> _searchFiles;
    // The application settings.
    private Editor.EditorSettings _settings;
    // The list of selected files.
    private ObservableCollection<IFile> _selectedFiles = [];
    // The clipboard handler.
    private IClipboardHandler _clipboardHandler;
    // Timer used to determine how long it takes to update UI.
    private int _userInteractionTimeMilliseconds = MaxUserInteractionTimeMilliseconds;
    private readonly IGorgonTimer _uiTimer = GorgonTimerQpc.SupportsQpc() ? new GorgonTimerQpc() : new GorgonTimerMultimedia();
    private double _lastTime = -1;

    /// <summary>Property to return the current directory.</summary>
    string IContentFileManager.CurrentDirectory => SelectedDirectory?.FullPath ?? Root.FullPath;

    /// <summary>
    /// Property to return the clipboard handler for this view model.
    /// </summary>
    public IClipboardHandler Clipboard
    {
        get => _clipboardHandler;
        private set
        {
            if (_clipboardHandler == value)
            {
                return;
            }

            OnPropertyChanging();
            _clipboardHandler = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return the currently selected directory.</summary>
    public IDirectory SelectedDirectory
    {
        get => _selectedDir;
        private set
        {
            if (_selectedDir == value)
            {
                return;
            }

            OnPropertyChanging();
            _selectedDir = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return the currently selected file.</summary>
    public ObservableCollection<IFile> SelectedFiles
    {
        get => _selectedFiles;
        private set
        {
            if (value is null)
            {
                _selectedFiles.Clear();
                return;
            }

            if (value == _selectedFiles)
            {
                return;
            }

            OnPropertyChanging();
            _selectedFiles = value;
            OnPropertyChanged();
            OnSelectedFileCountChanged();
        }
    }

    /// <summary>Property to return the root node for the file system.</summary>
    public IDirectory Root
    {
        get => _root;
        private set
        {
            if (_root == value)
            {
                return;
            }

            OnPropertyChanging();
            _root = value;
            OnPropertyChanging();
        }
    }

    /// <summary>Property to return the command to execute when a directory is selected.</summary>
    public IEditorCommand<string> SelectDirectoryCommand
    {
        get;
    }

    /// <summary>Property to return the command to execute when a file is selected.</summary>
    public IEditorCommand<IReadOnlyList<string>> SelectFileCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to rename a directory.
    /// </summary>
    public IEditorCommand<RenameArgs> RenameDirectoryCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to rename a file.
    /// </summary>
    public IEditorCommand<RenameArgs> RenameFileCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to create a new directory.
    /// </summary>
    public IEditorCommand<CreateDirectoryArgs> CreateDirectoryCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to delete a directory.
    /// </summary>
    public IEditorAsyncCommand<DeleteArgs> DeleteDirectoryCommand
    {
        get;
    }

    /// <summary>Property to return the command used to copy a file (or files).</summary>
    public IEditorAsyncCommand<IFileCopyMoveData> CopyFileCommand
    {
        get;
    }

    /// <summary>Property to return the command used to move a file (or files).</summary>
    public IEditorAsyncCommand<IFileCopyMoveData> MoveFileCommand
    {
        get;
    }

    /// <summary>Property to return the command used to copy a directory.</summary>
    public IEditorAsyncCommand<IDirectoryCopyMoveData> CopyDirectoryCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to move a directory.
    /// </summary>
    public IEditorAsyncCommand<IDirectoryCopyMoveData> MoveDirectoryCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to delete a file, or multiple files.
    /// </summary>
    public IEditorAsyncCommand<DeleteArgs> DeleteFileCommand
    {
        get;
    }

    /// <summary>
    /// Property to export the selected directory, and its contents to the physical filesystem.
    /// </summary>
    public IEditorAsyncCommand<object> ExportDirectoryCommand
    {
        get;
    }

    /// <summary>
    /// Property to export the selected file(s) to the physical filesystem.
    /// </summary>
    public IEditorAsyncCommand<object> ExportFilesCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to perform a search for files.
    /// </summary>
    public IEditorCommand<string> SearchCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the list of search results for a filtered node list.
    /// </summary>
    public IReadOnlyList<IFile> SearchResults
    {
        get => _searchFiles;
        private set
        {
            if (_searchFiles == value)
            {
                return;
            }

            OnPropertyChanging();

            if (value is not null)
            {
                if (_searchFiles is null)
                {
                    _searchFiles = new List<IFile>(value);
                }
                else
                {
                    _searchFiles.Clear();
                    _searchFiles.AddRange(value);
                }
            }
            else
            {
                _searchFiles = null;
            }

            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Property to return the command used to import files and directories from the physical file system.
    /// </summary>
    public IEditorAsyncCommand<IImportData> ImportCommand
    {
        get;
    }

    /// <summary>
    /// Property to set or return the command to execute when a content node is opened.
    /// </summary>
    public IEditorAsyncCommand<object> OpenContentFileCommand
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the metadata for the content plug-ins.
    /// </summary>
    public IReadOnlyList<IContentPlugInMetadata> PlugInMetadata
    {
        get;
        private set;
    }

    /// <summary>Property to return the command to refresh the file system.</summary>
    public IEditorAsyncCommand<object> RefreshCommand
    {
        get;
    }

    /// <summary>Property to return the command to retrieve a directory object by path.</summary>
    public IEditorCommand<GetDirectoryArgs> GetDirectoryCommand
    {
        get;
    }

    /// <summary>
    /// Function to call the <see cref="SelectedFilesChangedEvent"/>
    /// </summary>
    private void OnSelectedFileCountChanged()
    {
        EventHandler handler;

        lock (_selectedChangedEventLock)
        {
            handler = SelectedFilesChangedEvent;
        }

        handler?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Function to call the <see cref="FileSystemUpdated"/> event.
    /// </summary>
    private void OnFileSystemUpdated()
    {
        EventHandler handler;

        lock (_fsUpdatedEventLock)
        {
            handler = FileSystemUpdatedEvent;
        }

        handler?.Invoke(this, EventArgs.Empty);
    }

    /// <summary>
    /// Function to retrieve the mask and search pattern.
    /// </summary>
    /// <param name="directoryPath">The path to the directory to search in.</param>
    /// <param name="searchMask">The mask to filter on.</param>
    /// <returns>An updated file mask, and a pattern state ID.</returns>
    private (string mask, int patternState, IDirectory parentDir, bool usePattern) GetSearchState(string directoryPath, string searchMask)
    {
        if (!directoryPath.StartsWith("/", StringComparison.OrdinalIgnoreCase))
        {
            directoryPath = "/" + directoryPath;
        }

        directoryPath = directoryPath.FormatDirectory('/');

        IDirectory directory = null;

        if (directoryPath == "/")
        {
            directory = Root;
        }
        else
        {
            directory = _directories.Values.FirstOrDefault(item => string.Equals(item.FullPath, directoryPath, StringComparison.OrdinalIgnoreCase));

            if (directory is null)
            {
                throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, directoryPath));
            }
        }

        int searchPatternState = 0;
        bool searchFilePattern = (!string.Equals(searchMask, "*", StringComparison.OrdinalIgnoreCase)) && (!string.Equals(searchMask, "*.*", StringComparison.OrdinalIgnoreCase));

        if (searchFilePattern)
        {
            if ((searchMask.StartsWith("*", StringComparison.OrdinalIgnoreCase)) && (searchMask.EndsWith("*", StringComparison.OrdinalIgnoreCase)))
            {
                searchMask = searchMask[1..];
                searchMask = searchMask[0..^1];
                searchPatternState = 3;
            }
            else if (searchMask.StartsWith("*", StringComparison.OrdinalIgnoreCase))
            {
                searchMask = searchMask[1..];
                searchPatternState = 1;
            }
            else if (searchMask.EndsWith("*", StringComparison.OrdinalIgnoreCase))
            {
                searchMask = searchMask[0..^1];
                searchPatternState = 2;
            }
        }

        return (searchMask, searchPatternState, directory, searchFilePattern);
    }

    /// <summary>
    /// Function to set up the content plug-in association for a content file.
    /// </summary>
    /// <param name="filePath">The path to the content file.</param>
    /// <param name="metadata">The metadata to evaluate.</param>
    /// <param name="metadataOnly"><b>true</b> to indicate that only metadata should be used to scan the content file, <b>false</b> to scan, in depth, per plugin (slow).</param>
    /// <returns><b>true</b> if a content plug-in was associated, <b>false</b> if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="metadata"/> parameter is <b>null</b>.</exception>
    private bool AssignContentPlugIn(string filePath, ProjectItemMetadata metadata, bool metadataOnly)
    {
        if (metadata is null)
        {
            throw new ArgumentNullException(nameof(metadata));
        }

        // This node is already associated.
        if (metadata?.ContentMetadata is not null)
        {
            return false;
        }

        // Check the metadata for the plugin type associated with the node.            
        (ContentPlugIn plugin, MetadataPlugInState state) = HostServices.ContentPlugInService.GetContentPlugIn(metadata);

        switch (state)
        {
            case MetadataPlugInState.NotFound:
                metadata.ContentMetadata = null;
                metadata.PlugInName = string.Empty;
                return true;
            case MetadataPlugInState.Assigned:
                metadata.ContentMetadata = plugin as IContentPlugInMetadata;
                return true;
        }

        if (metadataOnly)
        {
            return true;
        }

        // Assume that no plugin is available for the node.
        metadata.PlugInName = string.Empty;

        // Attempt to associate a content plug-in with the node.            
        foreach (KeyValuePair<string, ContentPlugIn> servicePlugIn in HostServices.ContentPlugInService.PlugIns)
        {
            if ((servicePlugIn.Value is not IContentPlugInMetadata pluginMetadata)
                || (!pluginMetadata.CanOpenContent(filePath)))
            {
                continue;
            }

            metadata.ContentMetadata = pluginMetadata;
            return true;
        }

        return false;
    }

    /// <summary>
    /// Function to set up the content plug-in association for a content file.
    /// </summary>
    /// <param name="contentFile">The content file to evaluate.</param>
    /// <param name="metadataOnly"><b>true</b> to indicate that only metadata should be used to scan the content file, <b>false</b> to scan, in depth, per plugin (slow).</param>
    /// <returns><b>true</b> if a content plug-in was associated, <b>false</b> if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="contentFile"/> parameter is <b>null</b>.</exception>
    private bool AssignContentPlugIn(IFile contentFile, bool metadataOnly)
    {
        if (contentFile is null)
        {
            throw new ArgumentNullException(nameof(contentFile));
        }

        bool result = AssignContentPlugIn(contentFile.FullPath, contentFile.Metadata, metadataOnly);

        if ((result) && (contentFile.RefreshCommand is not null) && (contentFile.RefreshCommand.CanExecute(null)))
        {
            contentFile.RefreshCommand.Execute(null);
        }

        return result;
    }

    /// <summary>
    /// Function to determine if a file in the directory, or any sub directory is opened.
    /// </summary>
    /// <param name="directory">The directory to start searching from.</param>
    /// <returns>The <see cref="IFile"/> that is open, or <b>null</b> if not file is open.</returns>
    private IFile CheckForOpenFile(IDirectory directory) => directory.Files
                                                                     .Concat(directory.Directories.TraverseBreadthFirst(d => d.Directories)
                                                                                                  .SelectMany(item => item.Files))
                                                                     .FirstOrDefault(item => item.IsOpen);

    /// <summary>
    /// Function called when there is a conflict when copying files.
    /// </summary>
    /// <param name="sourceItem">The file being copied.</param>
    /// <param name="destItem">The destination file that is conflicting.</param>
    /// <returns>A resolution for the conflict.</returns>
    private FileConflictResolution CopyFileSystemConflictHandler(string sourceItem, string destItem)
    {
        FileConflictResolution result = FileConflictResolution.Overwrite;

        // Synchronize to the main thread.
        // Since the copy call is asynchronous, we'll need to set up a synchronization so the dialogs and whatnot can be displayed.
        _syncContext.Send(ctx =>
        {
            bool isBusy = HostServices.BusyService.IsBusy;

            // Reset the busy state.  The dialog will disrupt it anyway.
            HostServices.BusyService.SetIdle();
            MessageResponse response = MessageResponse.None;

            try
            {
                string destDirectoryPath = destItem.FormatDirectory('/');
                IDirectory destDirectory = _directories.FirstOrDefault(item => string.Equals(item.Value.FullPath, destDirectoryPath, StringComparison.OrdinalIgnoreCase)).Value;
                IFile destFile = _files.FirstOrDefault(item => string.Equals(item.Value.FullPath, destItem, StringComparison.OrdinalIgnoreCase)).Value;

                if (destDirectory is null)
                {
                    if ((destFile is not null) && (destFile.IsOpen))
                    {
                        response = HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_OPEN_CONTENT_CANT_OVERWRITE, destFile.Name), allowCancel: true);
                    }
                }
                else
                {
                    response = HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_DEST_IS_DIRECTORY, destItem.Ellipses(40, true)), allowCancel: true);
                }

                if (response != MessageResponse.None)
                {
                    switch (response)
                    {
                        case MessageResponse.Yes:
                            result = FileConflictResolution.Rename;
                            return;
                        case MessageResponse.No:
                            result = FileConflictResolution.Skip;
                            return;
                        default:
                            result = FileConflictResolution.Cancel;
                            return;
                    }
                }

                string fileName;
                string dirName;

                if (destFile is null)
                {
                    fileName = Path.GetFileName(destItem);
                    dirName = Path.GetDirectoryName(destItem).FormatDirectory('/').Ellipses(40, true);
                }
                else
                {
                    fileName = destFile.Name;
                    dirName = destFile.Parent.FullPath.Ellipses(40, true);
                }
                response = HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_FILE_EXISTS, fileName, dirName),
                                                            toAll: true, allowCancel: true);

                switch (response)
                {
                    case MessageResponse.Yes:
                        result = FileConflictResolution.Overwrite;
                        return;
                    case MessageResponse.YesToAll:
                        result = FileConflictResolution.OverwriteAll;
                        return;
                    case MessageResponse.No:
                        result = FileConflictResolution.Rename;
                        return;
                    case MessageResponse.NoToAll:
                        result = FileConflictResolution.RenameAll;
                        return;
                    default:
                        result = FileConflictResolution.Cancel;
                        return;
                }
            }
            finally
            {
                // Restore the busy state if we originally had it active.
                if (isBusy)
                {
                    HostServices.BusyService.SetBusy();
                }
            }
        }, null);

        return result;
    }

    /// <summary>
    /// Function called when there is a conflict when exporting files.
    /// </summary>
    /// <param name="sourceItem">The file being copied from the virtual file system.</param>
    /// <param name="destItem">The destination file on the physical file system that is conflicting.</param>
    /// <returns>A resolution for the conflict.</returns>
    private FileConflictResolution ExportConflictHandler(string sourceItem, string destItem)
    {
        FileConflictResolution result = FileConflictResolution.Overwrite;

        // Synchronize to the main thread.
        // Since the copy call is asynchronous, we'll need to set up a synchronization so the dialogs and whatnot can be displayed.
        _syncContext.Send(ctx =>
        {
            bool isBusy = HostServices.BusyService.IsBusy;

            // Reset the busy state.  The dialog will disrupt it anyway.
            HostServices.BusyService.SetIdle();
            MessageResponse response = MessageResponse.None;

            try
            {
                string destDirectoryPath = destItem.FormatDirectory(Path.DirectorySeparatorChar);

                if (System.IO.Directory.Exists(destDirectoryPath))
                {
                    response = HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_DEST_IS_DIRECTORY, destItem.Ellipses(40, true)), allowCancel: true);
                }

                if (response != MessageResponse.None)
                {
                    switch (response)
                    {
                        case MessageResponse.Yes:
                            result = FileConflictResolution.Rename;
                            return;
                        case MessageResponse.No:
                            result = FileConflictResolution.Skip;
                            return;
                        default:
                            result = FileConflictResolution.Cancel;
                            return;
                    }
                }

                response = HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_FILE_EXISTS, Path.GetFileName(sourceItem), destItem.Ellipses(40, true)),
                                                            toAll: true, allowCancel: true);

                switch (response)
                {
                    case MessageResponse.Yes:
                        result = FileConflictResolution.Overwrite;
                        return;
                    case MessageResponse.YesToAll:
                        result = FileConflictResolution.OverwriteAll;
                        return;
                    case MessageResponse.No:
                        result = FileConflictResolution.Rename;
                        return;
                    case MessageResponse.NoToAll:
                        result = FileConflictResolution.RenameAll;
                        return;
                    default:
                        result = FileConflictResolution.Cancel;
                        return;
                }
            }
            finally
            {
                // Restore the busy state if we originally had it active.
                if (isBusy)
                {
                    HostServices.BusyService.SetBusy();
                }
            }
        }, null);

        return result;
    }

    /// <summary>
    /// Function called when there is a conflict when moving files.
    /// </summary>
    /// <param name="sourceItem">The file being moved.</param>
    /// <param name="destItem">The destination file that is conflicting.</param>
    /// <returns>A resolution for the conflict.</returns>
    private FileConflictResolution MoveFileSystemConflictHandler(string sourceItem, string destItem)
    {
        FileConflictResolution result = FileConflictResolution.Overwrite;

        // Synchronize to the main thread.
        // Since the copy call is asynchronous, we'll need to set up a synchronization so the dialogs and whatnot can be displayed.
        _syncContext.Send(ctx =>
        {
            bool isBusy = HostServices.BusyService.IsBusy;

            // Reset the busy state.  The dialog will disrupt it anyway.
            HostServices.BusyService.SetIdle();
            MessageResponse response = MessageResponse.None;

            try
            {
                string destDirectoryPath = destItem.FormatDirectory('/');
                IDirectory destDirectory = _directories.FirstOrDefault(item => string.Equals(item.Value.FullPath, destDirectoryPath, StringComparison.OrdinalIgnoreCase)).Value;
                IFile destFile = _files.FirstOrDefault(item => string.Equals(item.Value.FullPath, destItem, StringComparison.OrdinalIgnoreCase)).Value;

                if (destDirectory is null)
                {
                    if ((destFile is not null) && (destFile.IsOpen))
                    {
                        HostServices.MessageDisplay.ShowWarning(string.Format(Resources.GOREDIT_MSG_OPEN_CONTENT_CANT_OVERWRITE_SKIP, destFile.Name));
                        result = FileConflictResolution.Skip;
                        return;
                    }
                }
                else
                {
                    HostServices.MessageDisplay.ShowWarning(string.Format(Resources.GOREDIT_MSG_DEST_IS_DIRECTORY_SKIP, destFile.Name));
                    result = FileConflictResolution.Skip;
                    return;
                }

                response = HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_FILE_EXISTS_MOVE, destFile.Name, destFile.Parent.FullPath.Ellipses(40, true)),
                                                            toAll: true, allowCancel: true);

                switch (response)
                {
                    case MessageResponse.Yes:
                        result = FileConflictResolution.Overwrite;
                        return;
                    case MessageResponse.YesToAll:
                        result = FileConflictResolution.OverwriteAll;
                        return;
                    case MessageResponse.No:
                        result = FileConflictResolution.Skip;
                        return;
                    case MessageResponse.NoToAll:
                        result = FileConflictResolution.SkipAll;
                        return;
                    default:
                        result = FileConflictResolution.Cancel;
                        return;
                }
            }
            finally
            {
                // Restore the busy state if we originally had it active.
                if (isBusy)
                {
                    HostServices.BusyService.SetBusy();
                }
            }
        }, null);

        return result;
    }

    /// <summary>
    /// Function to remove the directory from the cache.
    /// </summary>
    /// <param name="directory">The directory to remove from the cache.</param>
    private void RemoveDirectoryFromCache(IDirectory directory)
    {
        List<IDirectory> dirIDList = [];
        List<IFile> fileIDList = [];

        if (directory != Root)
        {
            dirIDList.Add(directory);
        }

        fileIDList.AddRange(directory.Files);
        IEnumerable<IDirectory> directories = _directories.Values.TraverseBreadthFirst(d => d.Directories);

        dirIDList.AddRange(directories);
        fileIDList.AddRange(directories.SelectMany(d => d.Files));

        foreach (IFile file in fileIDList)
        {
            _files.Remove(file.ID);
        }

        foreach (IDirectory dir in dirIDList)
        {
            dir.Directories.CollectionChanged -= Directories_CollectionChanged;
            dir.Files.CollectionChanged -= Files_CollectionChanged;
            _directories.Remove(dir.ID);
        }

    }

    /// <summary>
    /// Function to remove the files for a directory from the cache.
    /// </summary>
    /// <param name="directory">The directory containing the files to remove from the cache.</param>
    private void RemoveFilesFromCache(IDirectory directory)
    {
        List<IFile> fileIDs = [.. directory.Files];

        foreach (IFile file in fileIDs)
        {
            if (SelectedFiles.Contains(file))
            {
                SelectedFiles.Remove(file);
                OnSelectedFileCountChanged();
            }
            _files.Remove(file.ID);
        }
    }

    /// <summary>Handles the CollectionChanged event of the Directories control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
    private void Directories_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        IReadOnlyList<IDirectory> directories = (IReadOnlyList<IDirectory>)sender;
        IDirectory dir;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                dir = (IDirectory)e.NewItems[0];
                _directories[dir.ID] = dir;

                dir.Directories.CollectionChanged += Directories_CollectionChanged;
                dir.Files.CollectionChanged += Files_CollectionChanged;
                break;
            case NotifyCollectionChangedAction.Remove:
                dir = (IDirectory)e.OldItems[0];
                _directories.Remove(dir.ID);

                dir.Files.CollectionChanged -= Files_CollectionChanged;
                dir.Directories.CollectionChanged -= Directories_CollectionChanged;
                break;
            case NotifyCollectionChangedAction.Reset:
                IDirectory parent = _directories.FirstOrDefault(item => item.Value.Directories == directories).Value;
                if (parent is not null)
                {
                    RemoveDirectoryFromCache(parent);
                }
                break;
        }
    }

    /// <summary>Handles the CollectionChanged event of the Files control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
    private void Files_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
    {
        IReadOnlyList<IFile> files = (IReadOnlyList<IFile>)sender;
        IFile file;

        switch (e.Action)
        {
            case NotifyCollectionChangedAction.Add:
                file = (IFile)e.NewItems[0];
                _files[file.ID] = file;

                // Force a refresh on the file so we've got the most up to date.
                if ((file.RefreshCommand is not null) && (file.RefreshCommand.CanExecute(null)))
                {
                    file.RefreshCommand.Execute(null);
                }
                break;
            case NotifyCollectionChangedAction.Remove:
                file = (IFile)e.OldItems[0];
                _files.Remove(file.ID);

                SelectedFiles.Remove(file);
                OnSelectedFileCountChanged();
                break;
            case NotifyCollectionChangedAction.Reset:
                IDirectory parent = _directories.FirstOrDefault(item => item.Value.Files == files).Value;
                if (parent is not null)
                {
                    RemoveFilesFromCache(parent);
                }
                break;
        }
    }

    /// <summary>Handles the PropertyChanged event of the Clipboard control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="PropertyChangingEventArgs"/> instance containing the event data.</param>
    private void Clipboard_PropertyChanging(object sender, PropertyChangingEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IClipboardHandler.HasData):
                if (Clipboard.HasData)
                {
                    break;
                }

                // Reset all cut flags on all file system items if we have no clipboard data.
                foreach (IFile file in Root.Files.Where(item => item.IsCut))
                {
                    file.IsCut = false;
                }

                foreach (IDirectory dir in _directories.Values.Where(item => item.IsCut))
                {
                    dir.IsCut = false;
                    foreach (IFile file in dir.Files.Where(item => item.IsCut))
                    {
                        file.IsCut = false;
                    }
                }
                break;
        }
    }

    /// <summary>
    /// Function to determine if a file is linked to another file.
    /// </summary>
    /// <param name="file">The file to evaluate.</param>
    /// <returns><b>true</b> if linked, <b>false</b> if not.</returns>
    private bool IsFileLinked(IFile file)
    {
        foreach (string filePath in _files.Values.Where(item => item != file)
                                                 .SelectMany(item => item.Metadata.DependsOn)
                                                 .SelectMany(item => item.Value))
        {
            if (string.Equals(file.FullPath, filePath, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Function to repair file dependency metadata when a file is renamed, or moved (or its parent directory is renamed or moved).
    /// </summary>
    /// <param name="originalPath">The original path to the file.</param>
    /// <param name="newPath">The new path to the file.</param>
    private void RepairFileLinkage(string originalPath, string newPath)
    {
        foreach (List<string> dependencies in _files.Where(item => (!string.Equals(item.Value.FullPath, originalPath, StringComparison.OrdinalIgnoreCase)
                                                                && (!string.Equals(item.Value.FullPath, newPath, StringComparison.OrdinalIgnoreCase))))
                                                    .Select(item => item.Value.Metadata.DependsOn)
                                                    .Where(item => item.Count > 0)
                                                    .SelectMany(item => item.Values))
        {
            for (int i = 0; i < dependencies.Count; ++i)
            {
                if (string.Equals(originalPath, dependencies[i], StringComparison.OrdinalIgnoreCase))
                {
                    dependencies[i] = newPath;
                }
            }
        }
    }

    /// <summary>
    /// Function to enumerate the child directories/files for the file system.
    /// </summary>
    /// <param name="directory">The parent directory that is being enumerated.</param>
    private void EnumerateChildren(IDirectory directory)
    {
        _directories[directory.ID] = directory;

        foreach (IFile file in directory.Files)
        {
            AssignContentPlugIn(file, true);
            _files[file.ID] = file;
        }

        foreach (IDirectory subDir in directory.Directories.TraverseBreadthFirst(d => d.Directories))
        {
            _directories[subDir.ID] = subDir;

            foreach (IFile file in subDir.Files)
            {
                AssignContentPlugIn(file, true);
                _files[file.ID] = file;
            }
        }
    }

    /// <summary>
    /// Function to update directory view models after a copy or move operation.
    /// </summary>
    /// <param name="directories">The list of directories that were copied/moved.</param>
    /// <param name="destDirectory">The directory that received the copied files.</param>
    private void UpdateDirectoryViewModels(IEnumerable<IGorgonVirtualDirectory> directories, IDirectory destDirectory)
    {
        IReadOnlyList<IDirectory> newDirectories = _factory.CreateDirectories(directories, destDirectory);

        foreach (IDirectory dir in newDirectories.OrderBy(item => item.FullPath.Length))
        {
            if (!_directories.TryGetValue(dir.Parent.ID, out IDirectory parentDir))
            {
                continue;
            }

            parentDir.Directories.Add(dir);
        }
    }

    /// <summary>
    /// Function to update file view models after a copy or move operation.
    /// </summary>
    /// <param name="files">The list of source/destination files that were copied/moved.</param>
    /// <param name="destDirectory">The directory that received the copied files.</param>
    /// <param name="updateSelections"><b>true</b> to update file selections, <b>false</b> to leave as-is.</param>
    private void UpdateFileViewModels(IReadOnlyList<(IGorgonVirtualFile src, IGorgonVirtualFile dest)> files, IDirectory destDirectory, bool updateSelections)
    {
        ObservableCollection<IFile> selected = [];

        foreach ((IGorgonVirtualFile src, IGorgonVirtualFile dest) in files)
        {
            IFile sourceFile = _files.FirstOrDefault(item => string.Equals(item.Value.FullPath, src.FullPath, StringComparison.OrdinalIgnoreCase)).Value;
            if (sourceFile is null)
            {
                continue;
            }

            IFile newFile = _factory.DuplicateFile(sourceFile, dest, destDirectory);

            if (!_directories.TryGetValue(newFile.Parent.ID, out IDirectory parentDir))
            {
                continue;
            }

            // If we've got the same file name in here, then we'll need to remove it prior to adding.
            IFile existingFile = parentDir.Files.FirstOrDefault(item => string.Equals(item.FullPath, dest.FullPath, StringComparison.OrdinalIgnoreCase));

            if (existingFile is null)
            {
                parentDir.Files.Add(newFile);
                existingFile = newFile;
            }
            else
            {
                // Update the cache with the new ID.                    
                _files.Remove(existingFile.ID);
                existingFile.Metadata = newFile.Metadata;
                _files.Add(existingFile.ID, existingFile);
            }

            if ((updateSelections) || (sourceFile.Parent != existingFile.Parent))
            {
                selected.Add(existingFile);
            }
        }

        SelectedFiles = selected;
    }

    /// <summary>
    /// Function to return whether a directory can be created or not.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    /// <returns><b>true</b> if the directory can be created, <b>false</b> if not.</returns>
    private bool CanCreateDirectory(CreateDirectoryArgs args) => (args?.ParentDirectory ?? SelectedDirectory) is not null;

    /// <summary>Function to create a new directory.</summary>
    /// <param name="args">The arguments for the command.</param>
    private void DoCreateDirectory(CreateDirectoryArgs args)
    {
        IDirectory parent = args.ParentDirectory ?? SelectedDirectory;

        // Gets a name for the directory.
        string GetName(string originalName)
        {
            string result = originalName;
            int count = 0;

            while ((parent.Directories.Any(item => string.Equals(item.Name, result, StringComparison.OrdinalIgnoreCase)))
                || (parent.Files.Any(item => string.Equals(item.Name, result, StringComparison.OrdinalIgnoreCase))))
            {
                result = $"{result} ({++count})";
            }

            return result;
        }

        try
        {
            string name = GetName(args.Name);
            IGorgonVirtualDirectory directory = _fileSystemWriter.CreateDirectory(parent.FullPath + name);
            args.Directory = _factory.CreateDirectory(directory, parent);

            OnFileSystemUpdated();
        }
        catch (Exception ex)
        {
            args.Directory = null;
            HostServices.MessageDisplay.ShowError(ex, Resources.GOREDIT_ERR_CANNOT_CREATE_DIR);
        }
    }

    /// <summary>
    /// Function to return whether a directory can be deleted or not.
    /// </summary>
    /// <param name="args">The command arguments.</param>
    /// <returns><b>true</b> if the directory can be deleted, <b>false</b> if not.</returns>
    private bool CanDeleteDirectory(DeleteArgs args)
    {
        IDirectory dir;

        if ((args is not null) && (string.Equals(args.DeleteID, Root.ID, StringComparison.OrdinalIgnoreCase)))
        {
            return (Root.Directories.Count > 0) || (Root.Files.Count > 0);
        }

        if (string.IsNullOrWhiteSpace(args?.DeleteID))
        {
            dir = SelectedDirectory;
        }
        else
        {
            _directories.TryGetValue(args.DeleteID, out dir);
        }

        return (dir is not null) && (dir.AvailableActions & DirectoryActions.Delete) == DirectoryActions.Delete;
    }

    /// <summary>
    /// Function to delete the directory specified.
    /// </summary>
    /// <param name="directory">The directory to delete.</param>
    /// <param name="progressCallback">The callback used to report progress.</param>
    /// <param name="cancelToken">The token used to cancel the operation.</param>
    private void DeleteDirectory(IDirectory directory, Action<string> progressCallback, CancellationToken cancelToken)
    {
        _fileSystemWriter.DeleteDirectory(directory.FullPath, progressCallback, cancelToken);

        // If we've provided a progress callback, then we don't need to to use the code below.
        // Otherwise we'd end up with a cross thread error as the UI would be updated on a separate thread.
        if (progressCallback is not null)
        {
            return;
        }

        // Remove from the cache.
        if (directory == Root)
        {
            Root.Files.Clear();
            Root.Directories.Clear();
            return;
        }

        directory.Parent.Directories.Remove(directory);
    }

    /// <summary>
    /// Function to pause an operation so the user has time to react.
    /// </summary>
    private void SleepUI()
    {
        double time = _uiTimer.Seconds;

        if (_lastTime == -1)
        {
            _uiTimer.Reset();
            time = 0;
            _lastTime = 0;
            _userInteractionTimeMilliseconds = MaxUserInteractionTimeMilliseconds;
        }

        if (_userInteractionTimeMilliseconds > MinUserInteractionTimeMilliseconds)
        {
            double currentTime = time - _lastTime;

            if (currentTime > 5)
            {
                _userInteractionTimeMilliseconds = MinUserInteractionTimeMilliseconds;
                _lastTime = time;
            }
            else if (currentTime > 3)
            {
                _userInteractionTimeMilliseconds = MaxUserInteractionTimeMilliseconds / 5;
            }
        }

        Thread.Sleep(_userInteractionTimeMilliseconds);
    }

    /// <summary>
    /// Function to delete a directory.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    private async Task DoDeleteDirectoryAsync(DeleteArgs args)
    {
        CancellationTokenSource cancelSource = new();
        List<IDirectory> deletedDirs = [];
        List<IFile> deletedFiles = [];

        // Event handlers to track which directories and files were successfully deleted.
        void DirectoriesDeleted(object sender, VirtualDirectoryDeletedEventArgs e)
        {
            foreach (IGorgonVirtualDirectory dir in e.VirtualDirectories)
            {
                if (dir.Parent is null)
                {
                    continue;
                }

                IDirectory dirViewModel = _directories.Values.FirstOrDefault(item => string.Equals(item.FullPath, dir.FullPath, StringComparison.OrdinalIgnoreCase));
                if (dirViewModel is not null)
                {
                    deletedDirs.Add(dirViewModel);
                }
            }
        }

        void FilesDeleted(object sender, VirtualFileDeletedEventArgs e)
        {
            foreach (IGorgonVirtualFile file in e.VirtualFiles)
            {
                IFile fileViewModel = _files.Values.FirstOrDefault(item => string.Equals(item.FullPath, file.FullPath, StringComparison.OrdinalIgnoreCase));
                if (fileViewModel is not null)
                {
                    deletedFiles.Add(fileViewModel);
                }
            }
        }

        // Progress reporting.
        void ProgressCallback(string path)
        {
            UpdateMarequeeProgress($"{path}", Resources.GOREDIT_TEXT_DELETING, cancelSource.Cancel);

            if (cancelSource.IsCancellationRequested)
            {
                return;
            }

            // Give our UI time to update.  
            // We do this here so the user is able to click the Cancel button should they need it.
            SleepUI();
        }

        IDirectory directory = null;

        try
        {
            if (string.IsNullOrWhiteSpace(args.DeleteID))
            {
                directory = _directories[SelectedDirectory.ID];
            }
            else
            {
                _directories.TryGetValue(args.DeleteID, out directory);
            }

            IFile openFile = CheckForOpenFile(directory);
            args.ItemsDeleted = false;

            if (openFile is not null)
            {
                HostServices.MessageDisplay.ShowError(string.Format(Resources.GOREDIT_ERR_DIRECTORY_LOCKED, directory.FullPath, openFile.Name));
                return;
            }

            // Ensure file linkages are noticed.
            foreach (IFile selectedFile in directory.Files.Concat(directory.Directories.TraverseBreadthFirst(d => d.Directories)
                                                          .SelectMany(item => item.Files)))
            {
                if (!IsFileLinked(selectedFile))
                {
                    continue;
                }

                if (HostServices.MessageDisplay.ShowConfirmation(Resources.GOREDIT_CONFIRM_FILE_LINKED) == MessageResponse.No)
                {
                    return;
                }
                else
                {
                    break;
                }
            }

            if (directory == Root)
            {
                // If there's nothing to delete, then just get out. No sense in wasting effort.
                if (((directory.Directories.Count == 0) && (directory.Files.Count == 0))
                    || (HostServices.MessageDisplay.ShowConfirmation(Resources.GOREDIT_CONFIRM_DELETE_ALL) != MessageResponse.Yes))
                {
                    return;
                }
            }
            else
            {
                if (HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_DELETE_CHILDREN, directory.FullPath)) != MessageResponse.Yes)
                {
                    return;
                }
            }

            UpdateMarequeeProgress(string.Empty, Resources.GOREDIT_TEXT_DELETING, cancelSource.Cancel);

            SelectedFiles.Clear();
            OnSelectedFileCountChanged();

            _fileSystemWriter.VirtualDirectoryDeleted += DirectoriesDeleted;
            _fileSystemWriter.VirtualFileDeleted += FilesDeleted;
            await Task.Run(() => DeleteDirectory(directory, ProgressCallback, cancelSource.Token));
            _fileSystemWriter.VirtualDirectoryDeleted -= DirectoriesDeleted;
            _fileSystemWriter.VirtualFileDeleted -= FilesDeleted;

            args.ItemsDeleted = (deletedFiles.Count > 0) || (deletedDirs.Count > 0);

            if (!args.ItemsDeleted)
            {
                return;
            }

            foreach (IFile file in deletedFiles)
            {
                file.Parent.Files.Remove(file);
            }

            foreach (IDirectory dir in deletedDirs)
            {
                dir.Parent.Directories.Remove(dir);
            }
        }
        catch (OperationCanceledException)
        {
            // Intentionally left empty.
        }
        catch (Exception ex)
        {
            args.ItemsDeleted = (deletedFiles.Count > 0) || (deletedDirs.Count > 0);
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GOREDIT_ERR_DELETE, directory?.FullPath ?? string.Empty));
        }
        finally
        {
            if (args.ItemsDeleted)
            {
                OnFileSystemUpdated();
            }

            _fileSystemWriter.VirtualDirectoryDeleted -= DirectoriesDeleted;
            _fileSystemWriter.VirtualFileDeleted -= FilesDeleted;

            HideProgress();
            HideWaitPanel();
            cancelSource.Dispose();
            _lastTime = -1;
        }
    }

    /// <summary>
    /// Function to determine if a file (or files) can be deleted.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    /// <returns><b>true</b> if the file/files can be deleted, <b>false</b> if not.</returns>
    private bool CanDeleteFile(DeleteArgs args) => (SelectedFiles.Count > 0) && (!SelectedFiles[0].IsOpen);

    /// <summary>
    /// Function to delete a file (or files) from the file system.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoDeleteFileAsync(DeleteArgs args)
    {
        CancellationTokenSource cancelSource = null;
        string currentFilePath = string.Empty;
        List<IFile> deletedFiles = [];

        // Update the progress of the delete operation.
        void ProgressUpdate(string filePath)
        {
            currentFilePath = filePath;
            UpdateMarequeeProgress($"{filePath}", Resources.GOREDIT_TEXT_DELETING, cancelSource.Cancel);

            if (cancelSource.IsCancellationRequested)
            {
                return;
            }

            // Wait for a bit in order to give users time to react.
            SleepUI();
        }

        // Function called to locate the file view model.
        void OnDeleted(object sender, VirtualFileDeletedEventArgs e)
        {
            for (int i = 0; i < e.VirtualFiles.Count; ++i)
            {
                IFile deletedFile = _files.Values.FirstOrDefault(item => string.Equals(item.FullPath, e.VirtualFiles[i].FullPath, StringComparison.OrdinalIgnoreCase));

                if (deletedFile is not null)
                {
                    deletedFiles.Add(deletedFile);
                }
            }
        }

        try
        {
            IFile lockedFile = SelectedFiles.FirstOrDefault(item => item.IsOpen);
            if (lockedFile is not null)
            {
                HostServices.MessageDisplay.ShowError(string.Format(Resources.GOREDIT_ERR_FILE_LOCKED, lockedFile.FullPath));
                return;
            }

            // Ensure file linkages are noticed.
            foreach (IFile selectedFile in SelectedFiles)
            {
                if (!IsFileLinked(selectedFile))
                {
                    continue;
                }

                if (HostServices.MessageDisplay.ShowConfirmation(Resources.GOREDIT_CONFIRM_FILE_LINKED) == MessageResponse.No)
                {
                    args.ItemsDeleted = false;
                    return;
                }
                else
                {
                    break;
                }
            }

            MessageResponse response = MessageResponse.None;
            if (SelectedFiles.Count == 1)
            {
                response = HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_SINGLE_FILE_DELETE, SelectedFiles[0].FullPath));
            }
            else
            {
                response = HostServices.MessageDisplay.ShowConfirmation(string.Format(Resources.GOREDIT_CONFIRM_MULTIPLE_FILE_DELETE, SelectedFiles.Count));
            }

            if (response == MessageResponse.No)
            {
                return;
            }

            _fileSystemWriter.VirtualFileDeleted += OnDeleted;
            if (SelectedFiles.Count > 1)
            {
                cancelSource = new CancellationTokenSource();
                UpdateMarequeeProgress(Resources.GOREDIT_TEXT_PLEASE_WAIT);
                await Task.Run(() => _fileSystemWriter.DeleteFiles(SelectedFiles.Select(item => item.FullPath), ProgressUpdate, cancelSource.Token));
            }
            else
            {
                HostServices.BusyService.SetBusy();
                _fileSystemWriter.DeleteFile(SelectedFiles[0].FullPath);
            }
            _fileSystemWriter.VirtualFileDeleted -= OnDeleted;

            args.ItemsDeleted = (deletedFiles is not null) && (deletedFiles.Count > 0);
            if ((deletedFiles is null) || (deletedFiles.Count == 0))
            {
                return;
            }

            bool searchUpdate = false;
            for (int i = 0; i < deletedFiles.Count; ++i)
            {
                IFile file = deletedFiles[i];
                file.Parent.Files.Remove(file);

                if (_searchFiles is not null)
                {
                    if (_searchFiles.Remove(file))
                    {
                        searchUpdate = true;
                    }
                }
            }

            SelectedFiles.Clear();
            OnSelectedFileCountChanged();

            if (searchUpdate)
            {
                NotifyPropertyChanged(nameof(SearchResults));
            }
        }
        catch (OperationCanceledException)
        {
            // Intentionally left empty.
        }
        catch (Exception ex)
        {
            args.ItemsDeleted = (deletedFiles is not null) && (deletedFiles.Count > 0);
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GOREDIT_ERR_DELETE, currentFilePath));
        }
        finally
        {
            if (args.ItemsDeleted)
            {
                OnFileSystemUpdated();
            }

            cancelSource?.Dispose();
            _fileSystemWriter.VirtualFileDeleted -= OnDeleted;
            HostServices.BusyService.SetIdle();
            HideProgress();
            _lastTime = -1;
        }
    }

    /// <summary>
    /// Function to determine if a directory can be selected or not.
    /// </summary>
    /// <param name="id">The ID of the directory to select.</param>
    /// <returns><b>true</b> if the directory can be selected, or <b>false</b> if not.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "<Pending>")]
    private bool CanSelectDirectory(string id)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            return SelectedDirectory is not null;
        }

        if (!_directories.ContainsKey(id))
        {
            return false;
        }

        return (SelectedDirectory is null) || (!string.Equals(id, SelectedDirectory.ID, StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Function to set the currently selected directory.
    /// </summary>
    /// <param name="id">The ID for the directory.</param>
    private void DoSelectDirectory(string id)
    {
        IDirectory directory = null;

        try
        {
            if ((string.IsNullOrWhiteSpace(id))
                || (!_directories.TryGetValue(id, out directory)))
            {
                SelectedDirectory = null;
                SelectedFiles.Clear();
                OnSelectedFileCountChanged();
                return;
            }

            if (directory == SelectedDirectory)
            {
                return;
            }

            SelectedDirectory = directory;
            SelectedFiles.Clear();

            if (directory.Files.Count > 0)
            {
                SelectedFiles.Add(directory.Files[0]);
            }
            OnSelectedFileCountChanged();
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GOREDIT_ERR_NODE_SELECTION, directory?.FullPath ?? string.Empty));
        }
    }

    /// <summary>
    /// Function to determine if files can be selected.
    /// </summary>
    /// <param name="ids">The list of file IDs to select.</param>
    /// <returns><b>true</b> if the paths can be selected, or <b>false</b> if not.</returns>
    private bool CanSelectFile(IReadOnlyList<string> ids) => (ids is not null) && (SelectedDirectory is not null) && (SelectedDirectory.Files.Count > 0);

    /// <summary>
    /// Function to set the currently selected file(s).
    /// </summary>
    /// <param name="ids">The list of file IDs to select.</param>
    private void DoSelectFile(IReadOnlyList<string> ids)
    {
        string errorPath = string.Empty;

        try
        {
            ObservableCollection<IFile> selected = [];

            foreach (string id in ids)
            {
                if (!_files.TryGetValue(id, out IFile file))
                {
#if DEBUG
                    HostServices.MessageDisplay.ShowError($"The file ID {id} is not cached within the file cache list.");
#endif
                    continue;
                }

                errorPath = file.FullPath;
                selected.Add(file);
            }

            SelectedFiles = selected;
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GOREDIT_ERR_NODE_SELECTION, errorPath));
        }
    }

    /// <summary>
    /// Function to determine whether a selected file can be renamed or not.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    /// <returns><b>true</b> if the file can be deleted, or <b>false</b> is not.</returns>
    private bool CanRenameFile(RenameArgs args) => (SelectedFiles.Count == 1) && (!SelectedFiles[0].IsOpen);

    /// <summary>
    /// Function to rename a file.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    private void DoRenameFile(RenameArgs args)
    {
        IFile selected = SelectedFiles[0];

        try
        {
            // If we didn't supply an actual name, then just reset back to the old name.
            if ((string.IsNullOrWhiteSpace(args.NewName))
                    || (string.Equals(args.OldName, args.NewName, StringComparison.CurrentCulture)))
            {
                args.Cancel = true;
                return;
            }

            if (((selected.Parent.Directories.Any(item => (item != selected) && (string.Equals(item.Name, args.NewName, StringComparison.CurrentCultureIgnoreCase)))))
                || (selected.Parent.Files.Any(item => (item != selected) && (string.Equals(item.Name, args.NewName, StringComparison.CurrentCultureIgnoreCase)))))
            {
                HostServices.MessageDisplay.ShowError(string.Format(Resources.GOREDIT_ERR_NODE_EXISTS, args.NewName));
                args.Cancel = true;
                return;
            }

            if (selected.IsOpen)
            {
                HostServices.MessageDisplay.ShowError(string.Format(Resources.GOREDIT_ERR_CANNOT_RENAME_FILE_OPEN_FILE, selected.Name));
                return;
            }

            // Ensure that we can actually update the virtual directory object.
            if ((selected.RenameCommand is null) || (!selected.RenameCommand.CanExecute(args)))
            {
                args.Cancel = true;
                return;
            }

            // Update the directory in our file system.
            string originalName = selected.FullPath;
            _fileSystemWriter.RenameFile(originalName, args.NewName);

            selected.RenameCommand.Execute(args);

            RepairFileLinkage(originalName, selected.FullPath);

            OnFileSystemUpdated();
        }
        catch (Exception ex)
        {
            args.Cancel = true;
            HostServices.MessageDisplay.ShowError(ex);
        }
    }

    /// <summary>
    /// Function to determine whether a selected directory can be renamed or not.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    /// <returns><b>true</b> if the directory can be deleted, or <b>false</b> is not.</returns>
    private bool CanRenameDirectory(RenameArgs args)
    {
        IDirectory dir = SelectedDirectory;

        if (!string.IsNullOrWhiteSpace(args?.ID))
        {
            _directories.TryGetValue(args.ID, out dir);
        }

        return (dir is not null) && ((dir.AvailableActions & DirectoryActions.Rename) == DirectoryActions.Rename);
    }

    /// <summary>
    /// Function to rename a directory.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    private void DoRenameDirectory(RenameArgs args)
    {
        IDirectory selected = string.IsNullOrWhiteSpace(args.ID) ? SelectedDirectory : _directories[args.ID];

        try
        {
            // If we didn't supply an actual name, then just reset back to the old name.
            if ((string.IsNullOrWhiteSpace(args.NewName))
                    || (string.Equals(args.OldName, args.NewName, StringComparison.CurrentCulture)))
            {
                args.Cancel = true;
                return;
            }

            if (((selected.Parent.Directories.Any(item => (item != selected) && (string.Equals(item.Name, args.NewName, StringComparison.CurrentCultureIgnoreCase)))))
                || (selected.Parent.Files.Any(item => (item != selected) && (string.Equals(item.Name, args.NewName, StringComparison.CurrentCultureIgnoreCase)))))
            {
                HostServices.MessageDisplay.ShowError(string.Format(Resources.GOREDIT_ERR_NODE_EXISTS, args.NewName));
                args.Cancel = true;
                return;
            }

            IFile openFile = CheckForOpenFile(selected);

            if (openFile is not null)
            {
                args.Cancel = true;
                HostServices.MessageDisplay.ShowError(string.Format(Resources.GOREDIT_ERR_CANNOT_RENAME_DIR_OPEN_FILE, selected.Name, openFile.Name));
                return;
            }

            // Ensure that we can actually update the virtual directory object.
            if ((selected.RenameCommand is null) || (!selected.RenameCommand.CanExecute(args)))
            {
                args.Cancel = true;
                return;
            }

            // Update the directory in our file system.
            (string originalPath, IFile file)[] originalPaths = selected.Files.Concat(selected.Directories.TraverseBreadthFirst(d => d.Directories)
                                                                                                          .SelectMany(d => d.Files))
                                                                                                          .Select(item => (item.FullPath, item))
                                                                              .ToArray();
            _fileSystemWriter.RenameDirectory(selected.FullPath, args.NewName);

            selected.RenameCommand.Execute(args);

            foreach ((string originalPath, IFile file) in originalPaths)
            {
                RepairFileLinkage(originalPath, file.FullPath);
            }

            OnFileSystemUpdated();
        }
        catch (Exception ex)
        {
            args.Cancel = true;
            HostServices.MessageDisplay.ShowError(ex);
        }
    }

    /// <summary>
    /// Function to determine if the directory can be moved.
    /// </summary>
    /// <param name="copyData">The source and destination directory.</param>
    /// <returns><b>true</b> if the directory can be dropped, <b>false</b> if not.</returns>
    private bool CanMoveDirectory(IDirectoryCopyMoveData copyData)
    {
        if ((!_directories.TryGetValue(copyData.SourceDirectory, out IDirectory srcDirectory))
            || (!_directories.TryGetValue(copyData.DestinationDirectory, out IDirectory destDirectory)))
        {
            return false;
        }

        // Ensure that:
        // 1. We can move.
        // 2. That our source is not the root of the file system.
        // 3. That the src and dest are not the same.
        // 4. That the immediate parent of the src directory is not our destination.
        if (((srcDirectory.AvailableActions & DirectoryActions.Move) != DirectoryActions.Move)
            || (srcDirectory == Root)
            || (string.Equals(copyData.SourceDirectory, copyData.DestinationDirectory, StringComparison.OrdinalIgnoreCase))
            || (srcDirectory == destDirectory)
            || (srcDirectory.Parent == destDirectory))
        {
            return false;
        }

        IDirectory parent = destDirectory.Parent;

        // We can move any directory into the root (except itself, which is handled above).
        if (parent is null)
        {
            return true;
        }

        // Ensure that the directory being moved is not an ancestor of the destination.
        while (parent is not null)
        {
            if (parent == srcDirectory)
            {
                return false;
            }

            parent = parent.Parent;
        }

        return true;
    }

    /// <summary>
    /// Function to perform a move operation on a directory.
    /// </summary>
    /// <param name="copyData">The source and destination directory.</param>
    private async Task DoMoveDirectoryAsync(IDirectoryCopyMoveData copyData)
    {
        CancellationTokenSource cancelSource = new();
        IReadOnlyList<(IGorgonVirtualDirectory src, IGorgonVirtualDirectory dest)> movedDirs = null;
        IReadOnlyList<(IGorgonVirtualFile src, IGorgonVirtualFile dest)> movedFiles = null;

        // Progress reporting.
        void ProgressCallback(string path, double percent)
        {
            if (cancelSource.IsCancellationRequested)
            {
                return;
            }

            IDirectory dir = _directories.Values.FirstOrDefault(item => string.Equals(item.FullPath, path, StringComparison.OrdinalIgnoreCase));
            IFile file = null;

            if (dir is null)
            {
                file = _files.Values.FirstOrDefault(item => string.Equals(item.FullPath, path, StringComparison.OrdinalIgnoreCase));
            }

            string virtualPath = dir?.FullPath ?? file?.FullPath;

            if (string.IsNullOrWhiteSpace(virtualPath))
            {
                HostServices.Log.PrintError($"The current path is empty/null. Cannot copy.", LoggingLevel.All);
                return;
            }

            float totalPercentage = (float)percent / 1.0f;
            UpdateProgress($"{virtualPath} {(totalPercentage * 100):0.0}%", totalPercentage, Resources.GOREDIT_TEXT_MOVING, cancelSource.Cancel);

            if (cancelSource.IsCancellationRequested)
            {
                return;
            }

            // Give our UI time to update.  
            // We do this here so the user is able to click the Cancel button should they need it.
            SleepUI();
        }

        // Event handler to retrieve the list of source and destination directories, and source and destination files that were moved.
        void DirectoryMoved(object sender, VirtualDirectoryCopiedMovedEventArgs e)
        {
            movedDirs = e.VirtualDirectories;
            movedFiles = e.VirtualFiles;
        }

        IDirectory srcDirectory = null;
        IDirectory destDirectory = null;

        try
        {
            srcDirectory = _directories[copyData.SourceDirectory];
            destDirectory = _directories[copyData.DestinationDirectory];

            IFile openFile = CheckForOpenFile(srcDirectory);

            if (openFile is not null)
            {
                HostServices.MessageDisplay.ShowError(string.Format(Resources.GOREDIT_ERR_CANNOT_MOVE_DIR_OPEN_FILE, srcDirectory.Name, openFile.Name));
                return;
            }

            UpdateProgress(srcDirectory.FullPath, 0, Resources.GOREDIT_TEXT_MOVING, cancelSource.Cancel);

            _fileSystemWriter.VirtualDirectoryMoved += DirectoryMoved;
            await Task.Run(() => _fileSystemWriter.MoveDirectory(srcDirectory.FullPath, destDirectory.FullPath,
            new GorgonCopyCallbackOptions
            {
                CancelToken = cancelSource.Token,
                ProgressCallback = ProgressCallback,
                ConflictResolutionCallback = MoveFileSystemConflictHandler
            }));
            _fileSystemWriter.VirtualDirectoryMoved -= DirectoryMoved;
            HideProgress();

            copyData.DirectoriesCopied = ((movedDirs is not null) && (movedDirs.Count > 0)) || ((movedFiles is not null) && (movedFiles.Count > 0));
            if (!copyData.DirectoriesCopied)
            {
                return;
            }

            // Change to a different progress screen for our enumeration.
            UpdateMarequeeProgress(Resources.GOREDIT_TEXT_MOVING);

            UpdateDirectoryViewModels(movedDirs.Where(item => item.dest is not null).Select(item => item.dest), destDirectory);
            UpdateFileViewModels(movedFiles, destDirectory, false);

            // Remove the source files/directories from the view if it's subscribed.
            foreach ((IGorgonVirtualFile movedFile, IGorgonVirtualFile newFile) in movedFiles)
            {
                // Since the file system service uses absolute physical paths, we'll have to associate by physical path names.
                IFile file = _files.Values.FirstOrDefault(item => string.Equals(item.FullPath, movedFile.FullPath, StringComparison.OrdinalIgnoreCase));

                if (file is null)
                {
                    continue;
                }

                file.Parent.Files.Remove(file);
                RepairFileLinkage(file.FullPath, newFile.FullPath);
            }

            foreach (IGorgonVirtualDirectory srcDir in movedDirs.Select(item => item.src).OrderByDescending(item => item.FullPath.Length))
            {
                IDirectory src = _directories.Values.FirstOrDefault(item => string.Equals(item.FullPath, srcDir.FullPath, StringComparison.OrdinalIgnoreCase));

                if (src is null)
                {
                    continue;
                }

                src.Parent.Directories.Remove(src);
            }
        }
        catch (OperationCanceledException)
        {
            // Intentionally left empty.
        }
        catch (Exception ex)
        {
            copyData.DirectoriesCopied = ((movedDirs is not null) && (movedDirs.Count > 0)) || ((movedFiles is not null) && (movedFiles.Count > 0));
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GOREDIT_ERR_CANNOT_MOVE, srcDirectory?.FullPath, destDirectory?.FullPath));
        }
        finally
        {
            if (copyData.DirectoriesCopied)
            {
                OnFileSystemUpdated();
            }

            _fileSystemWriter.VirtualDirectoryMoved -= DirectoryMoved;
            HideProgress();
            cancelSource?.Dispose();
            _lastTime = -1;
        }
    }

    /// <summary>
    /// Function to determine if the file(s) can be copied or moved.
    /// </summary>
    /// <param name="copyData">The files and destination directory data.</param>
    /// <returns><b>true</b> if the file(s) can be copied or moved, <b>false</b> if not.</returns>
    private bool CanCopyOrMoveFiles(IFileCopyMoveData copyData)
    {
        IDirectory destDirectory = null;

        if ((copyData.DestinationDirectory is not null) && (!_directories.TryGetValue(copyData.DestinationDirectory ?? string.Empty, out destDirectory)))
        {
            return false;
        }

        foreach (string filePath in copyData.SourceFiles)
        {
            if ((!_files.TryGetValue(filePath, out IFile file))
                || ((file.Parent == destDirectory) && (copyData.Operation == CopyMoveOperation.Move))
                || ((file.IsOpen) && (copyData.Operation == CopyMoveOperation.Move)))
            {
                return false;
            }
        }

        return true;
    }

    /// <summary>
    /// Function to move files from one location to another.
    /// </summary>
    /// <param name="args">The arguments for the operation.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoMoveFilesAsync(IFileCopyMoveData args)
    {
        string currentFile = string.Empty;

        CancellationTokenSource cancelSource = new();
        IReadOnlyList<(IGorgonVirtualFile src, IGorgonVirtualFile dest)> movedFiles = null;

        // Progress reporting.
        void ProgressCallback(string path, double percent)
        {
            if (cancelSource.IsCancellationRequested)
            {
                return;
            }

            IFile file = _files.Values.FirstOrDefault(item => string.Equals(item.FullPath, path, StringComparison.OrdinalIgnoreCase));
            string virtualPath = currentFile = file?.FullPath ?? string.Empty;

            if (string.IsNullOrWhiteSpace(virtualPath))
            {
                HostServices.Log.PrintError($"The current path is empty/null. Cannot copy.", LoggingLevel.All);
                return;
            }

            float totalPercentage = (float)percent / 1.0f;
            UpdateProgress($"{virtualPath} {(totalPercentage * 100):0.0}%", totalPercentage, Resources.GOREDIT_TEXT_MOVING, cancelSource.Cancel);

            if (cancelSource.IsCancellationRequested)
            {
                return;
            }

            // Give our UI time to update.  
            // We do this here so the user is able to click the Cancel button should they need it.
            SleepUI();
        }

        // Event handler to retrieve the list of source and destination directories, and source and destination files that were moved.
        void FilesMoved(object sender, VirtualFileCopiedMovedEventArgs e) => movedFiles = e.VirtualFiles;

        IDirectory destDirectory = null;

        List<IFile> srcFiles = [];

        try
        {
            foreach (string id in args.SourceFiles)
            {
                if (!_files.TryGetValue(id, out IFile file))
                {
                    continue;
                }

                srcFiles.Add(file);
            }

            IFile openFile = srcFiles.FirstOrDefault(item => item.IsOpen);

            if (openFile is not null)
            {
                HostServices.MessageDisplay.ShowError(string.Format(Resources.GOREDIT_ERR_CANNOT_MOVE_FILE_OPEN_FILE, openFile.Name));
                return;
            }

            destDirectory = _directories[args.DestinationDirectory];

            UpdateProgress(srcFiles[0].FullPath, 0, Resources.GOREDIT_TEXT_MOVING, cancelSource.Cancel);

            _fileSystemWriter.VirtualFileMoved += FilesMoved;
            await Task.Run(() => _fileSystemWriter.MoveFiles(srcFiles.Select(item => item.FullPath), destDirectory.FullPath,
            new GorgonCopyCallbackOptions
            {
                CancelToken = cancelSource.Token,
                ProgressCallback = ProgressCallback,
                ConflictResolutionCallback = MoveFileSystemConflictHandler
            }));
            _fileSystemWriter.VirtualFileMoved -= FilesMoved;
            HideProgress();

            args.FilesCopied = (movedFiles is not null) && (movedFiles.Count > 0);

            if (!args.FilesCopied)
            {
                return;
            }

            // Change to a different progress screen for our enumeration.
            UpdateMarequeeProgress(Resources.GOREDIT_TEXT_MOVING);
            UpdateFileViewModels(movedFiles, destDirectory, true);

            // Remove the source files/directories from the view if it's subscribed.
            foreach ((IGorgonVirtualFile movedFile, IGorgonVirtualFile newFile) in movedFiles)
            {
                // Since the file system service uses absolute physical paths, we'll have to associate by physical path names.
                IFile sourceFile = _files.Values.FirstOrDefault(item => string.Equals(item.FullPath, movedFile.FullPath, StringComparison.OrdinalIgnoreCase));

                if (sourceFile is null)
                {
                    continue;
                }

                RepairFileLinkage(movedFile.FullPath, newFile.FullPath);
                sourceFile.Parent.Files.Remove(sourceFile);
            }
        }
        catch (OperationCanceledException)
        {
            // Intentionally left empty.
        }
        catch (Exception ex)
        {
            args.FilesCopied = (movedFiles is not null) && (movedFiles.Count > 0);
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GOREDIT_ERR_CANNOT_MOVE, currentFile, destDirectory?.FullPath));
        }
        finally
        {
            if (args.FilesCopied)
            {
                OnFileSystemUpdated();
            }

            _fileSystemWriter.VirtualFileMoved -= FilesMoved;
            HideProgress();
            cancelSource?.Dispose();
            _lastTime = -1;
        }
    }

    /// <summary>
    /// Function to copy files from one location to another.
    /// </summary>
    /// <param name="args">The arguments for the operation.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoCopyFilesAsync(IFileCopyMoveData args)
    {
        string currentFile = string.Empty;

        CancellationTokenSource cancelSource = new();
        IReadOnlyList<(IGorgonVirtualFile src, IGorgonVirtualFile dest)> copiedFiles = [];

        // Progress reporting.
        void ProgressCallback(string path, double percent)
        {
            if (cancelSource.IsCancellationRequested)
            {
                return;
            }

            IFile file = _files.Values.FirstOrDefault(item => string.Equals(item.FullPath, path, StringComparison.OrdinalIgnoreCase));

            string virtualPath = currentFile = file?.FullPath;

            if (string.IsNullOrWhiteSpace(virtualPath))
            {
                HostServices.Log.PrintError($"The current path is empty/null. Cannot copy.", LoggingLevel.All);
                return;
            }

            float totalPercentage = (float)percent / 1.0f;
            UpdateProgress($"{virtualPath} {(totalPercentage * 100):0.0}%", totalPercentage, Resources.GOREDIT_TEXT_COPYING, cancelSource.Cancel);

            if (cancelSource.IsCancellationRequested)
            {
                return;
            }

            // Give our UI time to update.  
            // We do this here so the user is able to click the Cancel button should they need it.
            SleepUI();
        }

        // Event handler to retrieve the list of files that were copied.
        void FilesCopied(object sender, VirtualFileCopiedMovedEventArgs e) => copiedFiles = e.VirtualFiles;

        IDirectory destDirectory = null;
        List<IFile> srcFiles = [];

        try
        {
            foreach (string id in args.SourceFiles)
            {
                if (!_files.TryGetValue(id, out IFile file))
                {
                    continue;
                }

                srcFiles.Add(file);
            }

            destDirectory = _directories[args.DestinationDirectory];

            UpdateProgress(srcFiles[0].FullPath, 0, Resources.GOREDIT_TEXT_COPYING, cancelSource.Cancel);

            _fileSystemWriter.VirtualFileCopied += FilesCopied;
            await Task.Run(() => _fileSystemWriter.CopyFiles(srcFiles.Select(item => item.FullPath), destDirectory.FullPath,
            new GorgonCopyCallbackOptions
            {
                CancelToken = cancelSource.Token,
                ProgressCallback = ProgressCallback,
                ConflictResolutionCallback = CopyFileSystemConflictHandler
            }));
            _fileSystemWriter.VirtualFileCopied -= FilesCopied;
            HideProgress();

            // Change to a different progress screen for our enumeration.
            UpdateMarequeeProgress(Resources.GOREDIT_TEXT_COPYING);

            args.FilesCopied = (copiedFiles is not null) && (copiedFiles.Count > 0);
            if (!args.FilesCopied)
            {
                return;
            }

            UpdateFileViewModels(copiedFiles, destDirectory, false);
        }
        catch (OperationCanceledException)
        {
            // Intentionally left empty.
        }
        catch (Exception ex)
        {
            args.FilesCopied = (copiedFiles is not null) && (copiedFiles.Count > 0);
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GOREDIT_ERR_CANNOT_COPY, currentFile, args.DestinationDirectory));
        }
        finally
        {
            if (args.FilesCopied)
            {
                OnFileSystemUpdated();
            }
            _fileSystemWriter.VirtualFileCopied -= FilesCopied;
            HideProgress();
            cancelSource?.Dispose();
            _lastTime = -1;
        }
    }

    /// <summary>
    /// Function to determine if the directory can be copied.
    /// </summary>
    /// <param name="copyData">The source and destination directory.</param>
    /// <returns><b>true</b> if the directory can be dropped, <b>false</b> if not.</returns>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0046:Convert to conditional expression", Justification = "<Pending>")]
    private bool CanCopyDirectory(IDirectoryCopyMoveData copyData)
    {
        if ((!_directories.TryGetValue(copyData.SourceDirectory, out IDirectory srcDirectory))
            || (!_directories.TryGetValue(copyData.DestinationDirectory, out IDirectory destDirectory))
            || (srcDirectory.Parent == destDirectory))
        {
            return false;
        }

        return ((srcDirectory.AvailableActions & DirectoryActions.Copy) == DirectoryActions.Copy);
    }

    /// <summary>
    /// Function to perform a copy operation on a directory.
    /// </summary>
    /// <param name="copyData">The source and destination directory.</param>
    private async Task DoCopyDirectoryAsync(IDirectoryCopyMoveData copyData)
    {
        CancellationTokenSource cancelSource = new();
        IReadOnlyList<(IGorgonVirtualDirectory src, IGorgonVirtualDirectory dest)> copiedDirs = [];
        IReadOnlyList<(IGorgonVirtualFile src, IGorgonVirtualFile dest)> copiedFiles = [];

        // Progress reporting.
        void ProgressCallback(string path, double percent)
        {
            if (cancelSource.IsCancellationRequested)
            {
                return;
            }

            IDirectory dir = _directories.Values.FirstOrDefault(item => string.Equals(item.FullPath, path, StringComparison.OrdinalIgnoreCase));
            IFile file = null;

            if (dir is null)
            {
                file = _files.Values.FirstOrDefault(item => string.Equals(item.FullPath, path, StringComparison.OrdinalIgnoreCase));
            }

            string virtualPath = dir?.FullPath ?? file?.FullPath;

            if (string.IsNullOrWhiteSpace(virtualPath))
            {
                HostServices.Log.PrintError($"The current path is empty/null. Cannot copy.", LoggingLevel.All);
                return;
            }

            float totalPercentage = (float)percent / 1.0f;
            UpdateProgress($"{virtualPath} {(totalPercentage * 100):0.0}%", totalPercentage, Resources.GOREDIT_TEXT_COPYING, cancelSource.Cancel);

            if (cancelSource.IsCancellationRequested)
            {
                return;
            }

            // Give our UI time to update.  
            // We do this here so the user is able to click the Cancel button should they need it.
            SleepUI();
        }

        // Event handler to retrieve the list of directories, and files that were copied.
        void DirectoryCopied(object sender, VirtualDirectoryCopiedMovedEventArgs e)
        {
            copiedDirs = e.VirtualDirectories;
            copiedFiles = e.VirtualFiles;
        }

        IDirectory srcDirectory = null;
        IDirectory destDirectory = null;

        try
        {
            srcDirectory = _directories[copyData.SourceDirectory];
            destDirectory = _directories[copyData.DestinationDirectory];

            UpdateProgress(srcDirectory.FullPath, 0, Resources.GOREDIT_TEXT_COPYING, cancelSource.Cancel);

            _fileSystemWriter.VirtualDirectoryCopied += DirectoryCopied;
            await Task.Run(() => _fileSystemWriter.CopyDirectory(srcDirectory.FullPath, destDirectory.FullPath,
            new GorgonCopyCallbackOptions
            {
                CancelToken = cancelSource.Token,
                ProgressCallback = ProgressCallback,
                ConflictResolutionCallback = CopyFileSystemConflictHandler
            }));
            _fileSystemWriter.VirtualDirectoryCopied -= DirectoryCopied;
            HideProgress();

            // Change to a different progress screen for our enumeration.
            UpdateMarequeeProgress(Resources.GOREDIT_TEXT_COPYING);

            copyData.DirectoriesCopied = ((copiedDirs is not null) && (copiedDirs.Count > 0)) || ((copiedFiles is not null) && (copiedFiles.Count > 0));
            if (!copyData.DirectoriesCopied)
            {
                return;
            }

            UpdateDirectoryViewModels(copiedDirs.Select(item => item.dest), destDirectory);
            UpdateFileViewModels(copiedFiles, destDirectory, false);
        }
        catch (OperationCanceledException)
        {
            // Intentionally left empty.
        }
        catch (Exception ex)
        {
            copyData.DirectoriesCopied = ((copiedDirs is not null) && (copiedDirs.Count > 0)) || ((copiedFiles is not null) && (copiedFiles.Count > 0));
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GOREDIT_ERR_CANNOT_COPY, srcDirectory?.Name ?? string.Empty, destDirectory?.Name ?? string.Empty));
        }
        finally
        {
            if (copyData.DirectoriesCopied)
            {
                OnFileSystemUpdated();
            }

            _fileSystemWriter.VirtualDirectoryCopied -= DirectoryCopied;
            HideProgress();
            cancelSource?.Dispose();
            _lastTime = -1;
        }
    }

    /// <summary>
    /// Function to perform a basic search for files.
    /// </summary>
    /// <param name="searchText">The text to search for.</param>
    private void DoSearch(string searchText)
    {
        HostServices.BusyService.SetBusy();

        try
        {
            if (string.IsNullOrWhiteSpace(searchText))
            {
                SearchResults = null;
            }
            else
            {
                SearchResults = _searchService.Search(searchText)?.ToArray();
            }
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GOREDIT_ERR_SEARCH, searchText));
        }
        finally
        {
            HostServices.BusyService.SetIdle();
        }
    }

    /// <summary>
    /// Function to determine if the selected files can be exported.
    /// </summary>
    /// <returns><b>true</b> if the selected files can be exported, <b>false</b> if not.</returns>
    private bool CanExportFiles() => (SelectedDirectory is not null)
                                    && (_directories.ContainsKey(SelectedDirectory.ID))
                                    && (SelectedFiles.Count != 0)
                                    && (SelectedFiles.All(item => _files.ContainsKey(item.ID)));

    /// <summary>
    /// Function to export the selected files to the physical file system.
    /// </summary>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoExportFilesAsync()
    {
        string currentFile = string.Empty;

        CancellationTokenSource cancelSource = new();

        // Progress reporting.
        void ProgressCallback(string path, double percent)
        {
            if (cancelSource.IsCancellationRequested)
            {
                return;
            }

            IFile file = _files.Values.FirstOrDefault(item => string.Equals(item.FullPath, path, StringComparison.OrdinalIgnoreCase));

            string virtualPath = currentFile = file?.FullPath;

            if (string.IsNullOrWhiteSpace(virtualPath))
            {
                HostServices.Log.PrintError($"The current path is empty/null. Cannot copy.", LoggingLevel.All);
                return;
            }

            float totalPercentage = (float)percent / 1.0f;
            UpdateProgress($"{virtualPath} {(totalPercentage * 100):0.0}%", totalPercentage, Resources.GOREDIT_TEXT_EXPORTING, cancelSource.Cancel);

            if (cancelSource.IsCancellationRequested)
            {
                return;
            }

            // Give our UI time to update.  
            // We do this here so the user is able to click the Cancel button should they need it.
            SleepUI();
        }

        try
        {
            string initialPathTemp = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments).FormatDirectory(Path.DirectorySeparatorChar);

            DirectoryInfo destDirectory = _directoryLocator.GetDirectory(new DirectoryInfo(initialPathTemp), Resources.GOREDIT_TEXT_EXPORT_TO);

            if (destDirectory is null)
            {
                return;
            }

            UpdateProgress(SelectedFiles[0].FullPath, 0, Resources.GOREDIT_TEXT_COPYING, cancelSource.Cancel);

            await Task.Run(() => _fileSystemWriter.ExportFiles(SelectedFiles.Select(item => item.FullPath), destDirectory.FullName,
            new GorgonCopyCallbackOptions
            {
                CancelToken = cancelSource.Token,
                ProgressCallback = ProgressCallback,
                ConflictResolutionCallback = ExportConflictHandler
            }));
            HideProgress();
        }
        catch (OperationCanceledException)
        {
            // Intentionally left empty.
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GOREDIT_ERR_CANNOT_COPY, currentFile, SelectedDirectory.FullPath));
        }
        finally
        {
            HideProgress();
            cancelSource?.Dispose();
            _lastTime = -1;
        }
    }

    /// <summary>
    /// Function to determine if a directory can be exported.
    /// </summary>
    /// <returns><b>true</b> if the directory can be exported, <b>false</b> if not.</returns>
    private bool CanExportDirectory() => (SelectedDirectory is not null) && (_directories.ContainsKey(SelectedDirectory.ID))
                                    && ((SelectedDirectory.Directories.Count > 0) || (SelectedDirectory.Files.Count > 0));

    /// <summary>
    /// Function to export a node and its contents to the physical file system.
    /// </summary>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoExportDirectoryAsync()
    {
        CancellationTokenSource cancelSource = new();

        IDirectory srcDir = SelectedDirectory;
        DirectoryInfo destDir = null;

        // Progress reporting.
        void ProgressCallback(string path, double percent)
        {
            if (cancelSource.IsCancellationRequested)
            {
                return;
            }

            IDirectory dir = _directories.Values.FirstOrDefault(item => string.Equals(item.FullPath, path, StringComparison.OrdinalIgnoreCase));
            IFile file = null;

            if (dir is null)
            {
                file = _files.Values.FirstOrDefault(item => string.Equals(item.FullPath, path, StringComparison.OrdinalIgnoreCase));
            }

            string virtualPath = dir?.FullPath ?? file?.FullPath;

            if (string.IsNullOrWhiteSpace(virtualPath))
            {
                HostServices.Log.PrintError($"The current path is empty/null. Cannot export.", LoggingLevel.All);
                return;
            }

            float totalPercentage = (float)percent / 1.0f;
            UpdateProgress($"{virtualPath} {(totalPercentage * 100):0.0}%", totalPercentage, Resources.GOREDIT_TEXT_EXPORTING, cancelSource.Cancel);

            if (cancelSource.IsCancellationRequested)
            {
                return;
            }

            // Give our UI time to update.  
            // We do this here so the user is able to click the Cancel button should they need it.
            SleepUI();
        }

        try
        {
            destDir = _directoryLocator.GetDirectory(new DirectoryInfo(_settings.LastOpenSavePath.FormatDirectory(Path.DirectorySeparatorChar)), Resources.GOREDIT_TEXT_EXPORT_TO);

            if (destDir is null)
            {
                return;
            }

            UpdateProgress(srcDir.FullPath, 0, Resources.GOREDIT_TEXT_EXPORTING, cancelSource.Cancel);

            await Task.Run(() => _fileSystemWriter.ExportDirectory(srcDir.FullPath, destDir.FullName,
            new GorgonCopyCallbackOptions
            {
                CancelToken = cancelSource.Token,
                ProgressCallback = ProgressCallback,
                ConflictResolutionCallback = ExportConflictHandler
            }));

            _settings.LastOpenSavePath = destDir.FullName.FormatDirectory(Path.DirectorySeparatorChar);
        }
        catch (OperationCanceledException)
        {
            // Intentionally empty.
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GOREDIT_ERR_EXPORT, destDir?.FullName ?? Resources.GOREDIT_TEXT_UNKNOWN));
        }
        finally
        {
            cancelSource.Dispose();
            HideProgress();
            _lastTime = -1;
        }
    }

    /// <summary>
    /// Function to determine if an import can be performed.
    /// </summary>
    /// <param name="args">The command arguments.</param>
    /// <returns><b>true</b> if the import can proceed, <b>false</b> if not.</returns>
    private bool CanImport(IImportData args) => (SearchResults is null)
                                                     && (_directories.ContainsKey(args.DestinationDirectory))
                                                     && (!string.IsNullOrWhiteSpace(args.DestinationDirectory));

    /// <summary>
    /// Function import files/directories from Windows Explorer into the virtual file system.
    /// </summary>
    /// <param name="args">The command arguments.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoImportAsync(IImportData args)
    {
        CancellationTokenSource cancelSource = new();
        IReadOnlyList<IGorgonVirtualDirectory> copiedDirs = [];
        IReadOnlyList<IGorgonVirtualFile> copiedFiles = [];
        HashSet<IEditorContentImporter> importers = [];
        Dictionary<string, string> importedFilePaths = new(StringComparer.OrdinalIgnoreCase);

        // Progress reporting.
        void ProgressCallback(string path, double percent)
        {
            if (cancelSource.IsCancellationRequested)
            {
                return;
            }

            if (!importedFilePaths.TryGetValue(path, out string originalPath))
            {
                originalPath = path;
            }

            float totalPercentage = (float)percent / 1.0f;
            UpdateProgress($"{originalPath.Ellipses(40, true)} {(totalPercentage * 100):0.0}%", totalPercentage, Resources.GOREDIT_TEXT_IMPORTING, cancelSource.Cancel);

            if (cancelSource.IsCancellationRequested)
            {
                return;
            }

            // Give our UI time to update.  
            // We do this here so the user is able to click the Cancel button should they need it.
            SleepUI();
        }

        // Event handler to retrieve the list of directories, and files that were imported.
        void Imported(object sender, ImportedEventArgs e)
        {
            copiedDirs = e.VirtualDirectories;
            copiedFiles = e.VirtualFiles;
        }

        // Event handler to perform a file conversion on import (if applicable).
        void BeforeFileImport(object sender, FileImportingArgs e)
        {
            if (string.IsNullOrWhiteSpace(e.PhysicalFilePath))
            {
                return;
            }

            string originalPath = e.PhysicalFilePath;
            IEditorContentImporter importer = HostServices.ContentPlugInService.GetContentImporter(e.PhysicalFilePath);

            // No importer, no conversion possible.
            if (importer is null)
            {
                return;
            }

            if (!importers.Contains(importer))
            {
                importers.Add(importer);
            }

            // If we have no importer plug-in for the current file, then leave.
            if (importer is null)
            {
                importedFilePaths[originalPath] = originalPath;
                return;
            }

            try
            {
                IGorgonVirtualFile file = importer.ImportData(e.PhysicalFilePath, CancellationToken.None);

                if (file is null)
                {
                    importedFilePaths[originalPath] = originalPath;
                    return;
                }

                // Update the source path so we copy the correct file.
                e.PhysicalFilePath = file.PhysicalFile.FullPath;
                importedFilePaths[e.PhysicalFilePath] = originalPath;
            }
            catch (Exception ex)
            {
                if ((_syncContext != SynchronizationContext.Current) && (!string.IsNullOrWhiteSpace(originalPath)))
                {
                    _syncContext.Send(d => HostServices.MessageDisplay.ShowWarning(string.Format(Resources.GOREDIT_WRN_IMPORT_FILE, Path.GetFileName(originalPath)), details: $"{Resources.GOREDIT_ERR_ERROR}:\n\n{ex.Message}"), ex);
                }
            }
        }

        IDirectory destDirectory = null;
        DirectoryInfo sourceDir = null;

        try
        {
            if (args.PhysicalPaths.Count == 0)
            {
                sourceDir = _directoryLocator.GetDirectory(new DirectoryInfo(_settings.LastOpenSavePath.FormatDirectory(Path.DirectorySeparatorChar)), Resources.GOREDIT_TEXT_IMPORT_FROM);

                if (sourceDir is null)
                {
                    return;
                }

                args.PhysicalPaths.AddRange(sourceDir.EnumerateFileSystemInfos().Select(item => item.FullName));

                if (args.PhysicalPaths.Count == 0)
                {
                    return;
                }
            }

            destDirectory = _directories[args.DestinationDirectory];

            UpdateProgress(args.PhysicalPaths[0], 0, Resources.GOREDIT_TEXT_IMPORTING, cancelSource.Cancel);

            _fileSystemWriter.Imported += Imported;
            _fileSystemWriter.FileImporting += BeforeFileImport;
            await Task.Run(() => _fileSystemWriter.Import(args.PhysicalPaths, destDirectory.FullPath,
            new GorgonCopyCallbackOptions
            {
                CancelToken = cancelSource.Token,
                ProgressCallback = ProgressCallback,
                ConflictResolutionCallback = CopyFileSystemConflictHandler
            }));
            _fileSystemWriter.FileImporting -= BeforeFileImport;
            _fileSystemWriter.Imported -= Imported;
            HideProgress();

            // Clean up after the importers.
            foreach (IEditorContentImporter importer in importers)
            {
                importer.CleanUp();
            }

            // Change to a different progress screen for our enumeration.
            UpdateMarequeeProgress(Resources.GOREDIT_TEXT_IMPORTING);

            args.ItemsImported = ((copiedDirs is not null) && (copiedDirs.Count > 0)) || ((copiedFiles is not null) && (copiedFiles.Count > 0));
            if (!args.ItemsImported)
            {
                return;
            }

            UpdateDirectoryViewModels(copiedDirs, destDirectory);

            ObservableCollection<IFile> selected = [];

            IReadOnlyList<IFile> files = _factory.CreateFiles(copiedFiles, destDirectory);

            foreach (IFile file in files)
            {
                if (!_directories.TryGetValue(file.Parent.ID, out IDirectory parentDir))
                {
                    continue;
                }

                // If we've got the same file name in here, then we need to refresh it. We won't need to add it to the list since it's already there, but the information about the file will 
                // probably have changed.
                IFile existingFile = parentDir.Files.FirstOrDefault(item => string.Equals(item.FullPath, file.FullPath, StringComparison.OrdinalIgnoreCase));

                if (existingFile is null)
                {
                    parentDir.Files.Add(file);
                    existingFile = file;
                }

                AssignContentPlugIn(existingFile, false);

                selected.Add(existingFile);
            }

            SelectedFiles = selected;
        }
        catch (OperationCanceledException)
        {
            // Intentionally left empty.
        }
        catch (Exception ex)
        {
            args.ItemsImported = ((copiedDirs is not null) && (copiedDirs.Count > 0)) || ((copiedFiles is not null) && (copiedFiles.Count > 0));
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GOREDIT_ERR_IMPORT, destDirectory?.Name ?? string.Empty));
        }
        finally
        {
            if (args.ItemsImported)
            {
                OnFileSystemUpdated();
            }

            _fileSystemWriter.FileImporting -= BeforeFileImport;
            _fileSystemWriter.Imported -= Imported;
            HideProgress();
            cancelSource?.Dispose();
            _lastTime = -1;
        }
    }

    /// <summary>
    /// Function to refresh the file system.
    /// </summary>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task DoRefreshAsync()
    {
        try
        {
            ShowWaitPanel(Resources.GOREDIT_TEXT_PLEASE_WAIT);

            IReadOnlyList<IFile> files = _files.Values.Where(item => (item.RefreshCommand is not null) && (item.RefreshCommand.CanExecute(null)))
                                                      .ToArray();

            await Task.Run(() =>
            {
                foreach (IFile file in files)
                {
                    // Reset so we can get the plug-in.
                    file.Metadata.ContentMetadata = null;
                    file.Metadata.PlugInName = null;
                    AssignContentPlugIn(file.FullPath, file.Metadata, false);
                }
            });

            // This has to be executed on the main thread.
            foreach (IFile file in files)
            {
                file.RefreshCommand.Execute(null);
            }

            SelectedFiles.Clear();
            DoSelectDirectory(Root.ID);
            if ((SelectedDirectory is null) || (SelectedDirectory.Files.Count == 0))
            {
                OnFileSystemUpdated();
                return;
            }

            SelectedFiles.Add(SelectedDirectory.Files[0]);
            OnSelectedFileCountChanged();
            OnFileSystemUpdated();
        }
        catch (Exception ex)
        {
            HostServices.Log.PrintError("Failed to refresh the file system.", LoggingLevel.Simple);
            HostServices.Log.LogException(ex);
        }
        finally
        {
            HideWaitPanel();
        }
    }

    /// <summary>
    /// Function to determine if a directory can be retrieved.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    /// <returns><b>true</b> if the directory can be retrieved, <b>false</b> if not.</returns>
    private bool CanGetDirectory(GetDirectoryArgs args) => (args is not null) && (!string.IsNullOrWhiteSpace(args.Path));

    /// <summary>
    /// Function to retrieve a directory object by its path.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    private void DoGetDirectory(GetDirectoryArgs args)
    {
        try
        {
            string path = args.Path;

            if (!path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
            {
                path = "/" + path;
            }

            string actualPath = path.FormatDirectory('/');

            args.Directory = _directories.Values.FirstOrDefault(item => string.Equals(item.FullPath, actualPath, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, args.Path));
        }
    }

    /// <summary>
    /// Function to inject dependencies for the view model.
    /// </summary>
    /// <param name="injectionParameters">The parameters to inject.</param>
    /// <remarks>Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.</remarks>
    protected override void OnInitialize(FileExplorerParameters injectionParameters)
    {
        _factory = injectionParameters.ViewModelFactory;
        _fileSystemWriter = injectionParameters.FileSystem;
        _searchService = injectionParameters.SearchService;
        _directoryLocator = injectionParameters.DirectoryLocator;
        _settings = injectionParameters.EditorSettings;
        Root = injectionParameters.Root;
        Clipboard = injectionParameters.Clipboard;

        // We should be on the main thread here.
        _syncContext = injectionParameters.SyncContext;

        EnumerateChildren(Root);

        _selectedDir = Root;
        if (Root.Files.Count > 0)
        {
            SelectedFiles.Add(Root.Files[0]);
            OnSelectedFileCountChanged();
        }

        PlugInMetadata = new List<IContentPlugInMetadata>(HostServices.ContentPlugInService.PlugIns.Values.OfType<IContentPlugInMetadata>());
    }

    /// <summary>Function called when the associated view is loaded.</summary>
    protected override void OnLoad()
    {
        base.OnLoad();

        Root.Directories.CollectionChanged += Directories_CollectionChanged;
        Root.Files.CollectionChanged += Files_CollectionChanged;

        // Hook the sub directory and file collections.
        foreach (IDirectory subDir in Root.Directories.TraverseBreadthFirst(d => d.Directories))
        {
            subDir.Files.CollectionChanged += Files_CollectionChanged;
            subDir.Directories.CollectionChanged += Directories_CollectionChanged;
        }

        Clipboard.PropertyChanging += Clipboard_PropertyChanging;
    }

    /// <summary>Function called when the associated view is unloaded.</summary>
    protected override void OnUnload()
    {
        // Unsubscribe everyone from the event.
        FileSystemUpdatedEvent = null;

        Clipboard.PropertyChanging -= Clipboard_PropertyChanging;

        // Unhook the sub directories/files.
        foreach (IDirectory subDir in Root.Directories.TraverseBreadthFirst(d => d.Directories))
        {
            subDir.Directories.CollectionChanged -= Directories_CollectionChanged;
            subDir.Files.CollectionChanged -= Files_CollectionChanged;
        }

        Root.Directories.CollectionChanged -= Directories_CollectionChanged;
        Root.Files.CollectionChanged -= Files_CollectionChanged;

        base.OnUnload();
    }

    /// <summary>Function to create a new directory</summary>
    /// <param name="directory">The path to the new directory.</param>
    /// <returns><b>true</b> if the directory was created, <b>false</b> if it already existed.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="directory"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="directory"/> parameter is empty.</exception>
    /// <exception cref="ArgumentException">Thrown if the <paramref name="directory"/> contains illegal characters.</exception>
    /// <exception cref="IOException">Thrown if the <paramref name="directory"/> points to an existing file on the file system.</exception>
    bool IContentFileManager.CreateDirectory(string directory)
    {
        if (directory is null)
        {
            throw new ArgumentNullException(nameof(directory));
        }

        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentEmptyException(nameof(directory));
        }

        if (!directory.StartsWith("/", StringComparison.OrdinalIgnoreCase))
        {
            directory = "/" + directory;
        }

        directory = directory.FormatDirectory('/');

        if (directory == "/")
        {
            return false;
        }

        string filePath = directory[0..^1];
        if (_fileSystemWriter.FileSystem.GetFile(filePath) is not null)
        {
            throw new IOException(string.Format(Resources.GOREDIT_ERR_PATH_IS_FILE, filePath));
        }

        IGorgonVirtualDirectory virtDirectory = _fileSystemWriter.FileSystem.GetDirectory(directory);

        if (virtDirectory is not null)
        {
            return true;
        }

        virtDirectory = _fileSystemWriter.CreateDirectory(directory);

        // Function to update the UI.
        void UpdateUI(object context)
        {
            IGorgonVirtualDirectory newDir = (IGorgonVirtualDirectory)context;

            IDirectory parentDir = _directories.Values.FirstOrDefault(item => string.Equals(item.FullPath, newDir.Parent.FullPath, StringComparison.OrdinalIgnoreCase));
            _factory.CreateDirectory((IGorgonVirtualDirectory)context, parentDir);
            OnFileSystemUpdated();
        }

        // We need to invoke this back on the main thread.
        if (_syncContext != SynchronizationContext.Current)
        {
            _syncContext.Send(UpdateUI, directory);
        }
        else
        {
            UpdateUI(directory);
        }

        return true;
    }

    /// <summary>Function to delete a directory.</summary>
    /// <param name="path">The path to the directory.</param>
    /// <returns>
    ///   <b>true</b> if the directory was deleted, <b>false</b> if it wasn't found.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
    bool IContentFileManager.DeleteDirectory(string path)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentEmptyException(nameof(path));
        }

        if (!path.StartsWith("/", StringComparison.OrdinalIgnoreCase))
        {
            path = "/" + path;
        }

        string actualPath = path.FormatDirectory('/');

        IDirectory dir = _directories.Values.FirstOrDefault(item => string.Equals(item.FullPath, actualPath, StringComparison.OrdinalIgnoreCase));

        if (dir is null)
        {
            return false;
        }

        _fileSystemWriter.DeleteDirectory(dir.FullPath);

        // Update the UI.
        void UpdateUI(object context)
        {
            dir.Parent.Directories.Remove(dir);
            OnFileSystemUpdated();
        }

        if (_syncContext != SynchronizationContext.Current)
        {
            _syncContext.Send(UpdateUI, null);
        }
        else
        {
            UpdateUI(null);
        }

        return true;
    }

    /// <summary>Function to determine if a directory exists or not.</summary>
    /// <param name="path">The path to the directory.</param>
    /// <returns>
    ///   <b>true</b> if the directory exists, <b>false</b> if not.</returns>
    bool IContentFileManager.DirectoryExists(string path) => _fileSystemWriter.FileSystem.GetDirectory(path) is not null;

    /// <summary>
    /// Function to determine if a file exists or not.
    /// </summary>
    /// <param name="path">The path to the file.</param>
    /// <returns><b>true</b> if the file exists, <b>false</b> if not.</returns>
    bool IContentFileManager.FileExists(string path) => _fileSystemWriter.FileSystem.GetFile(path) is not null;

    /// <summary>Function to retrieve a file based on the path specified.</summary>
    /// <param name="path">The path to the file.</param>
    /// <returns>A <see cref="IContentFile"/> if found, <b>null</b> if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
    IContentFile IContentFileManager.GetFile(string path)
    {

        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        return string.IsNullOrWhiteSpace(path)
            ? throw new ArgumentEmptyException(nameof(path))
            : _files.Values.FirstOrDefault(item => string.Equals(item.FullPath, path, StringComparison.OrdinalIgnoreCase)) as IContentFile;

    }

    /// <summary>
    /// Function to open a file stream for the specified virtual file.
    /// </summary>
    /// <param name="path">The path to the virtual file to open.</param>
    /// <param name="mode">The operating mode for the file stream.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="path"/> parameter is empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file specified by the <paramref name="path"/> was not found and the mode was set to <see cref="FileMode.Open"/>.</exception>
    /// <exception cref="GorgonException">Thrown if the file specified in the <paramref name="path"/> is open for editing.</exception>
    /// <returns>A file stream for the virtual file.</returns>
    Stream IContentFileManager.OpenStream(string path, FileMode mode)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentEmptyException(nameof(path));
        }

        if (mode is FileMode.Open or FileMode.OpenOrCreate)
        {
            IGorgonVirtualFile file = _fileSystemWriter.FileSystem.GetFile(path);

            return file is null ? throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, path)) : file.OpenStream();
        }

        // We cannot write to a file that's already open for editing.
        IFile fileViewModel = _files.Values.FirstOrDefault(item => string.Equals(item.FullPath, path, StringComparison.OrdinalIgnoreCase));

        if ((fileViewModel is not null) && (fileViewModel.IsOpen))
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_CANNOT_OPEN_CONTENT, path));
        }

        // Updates the UI.
        void UpdateUI(object ctx)
        {
            VirtualFileClosedEventArgs e = (VirtualFileClosedEventArgs)ctx;
            IDirectory parent = _directories.Values.FirstOrDefault(item => string.Equals(item.FullPath, e.VirtualFile.Directory.FullPath, StringComparison.OrdinalIgnoreCase));

            if (fileViewModel is null)
            {
                if ((parent is not null) && (e.Created))
                {
                    fileViewModel = _factory.CreateFile(e.VirtualFile, parent);

                    if (fileViewModel is null)
                    {
                        return;
                    }

                    parent.Files.Add(fileViewModel);

                    AssignContentPlugIn(fileViewModel, false);
                }
                else
                {
                    return;
                }
            }

            if ((fileViewModel.RefreshCommand is not null) && (fileViewModel.RefreshCommand.CanExecute(null)))
            {
                fileViewModel.RefreshCommand.Execute(null);
            }

            OnFileSystemUpdated();
        }

        // When the file is closed, update the UI.
        void FileClosed(object sender, VirtualFileClosedEventArgs e)
        {
            _fileSystemWriter.VirtualFileClosed -= FileClosed;

            // Ensure we execute the UI update on the main thread.
            if (_syncContext != SynchronizationContext.Current)
            {
                _syncContext.Send(UpdateUI, e);
            }
            else
            {
                UpdateUI(e);
            }
        }

        _fileSystemWriter.VirtualFileClosed += FileClosed;
        return _fileSystemWriter.OpenStream(path, mode);
    }

    /// <summary>Function to retrieve the content files for a given directory path.</summary>
    /// <param name="directoryPath">The directory path to search under.</param>
    /// <param name="searchMask">The search mask to use.</param>
    /// <param name="recursive">[Optional] <b>true</b> to retrieve all files under the path, including those in sub directories, or <b>false</b> to retrieve files in the immediate path.</param>
    /// <returns>An <c>IEnumerable</c> containing the content files found on the path.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="directoryPath"/> is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="directoryPath"/> is empty.</exception>
    /// <remarks>
    ///   <para>
    /// This will search on the specified <paramref name="directoryPath" /> for all content files that match the <paramref name="searchMask" />.
    /// </para>
    ///   <para>
    /// The <paramref name="searchMask" /> parameter can be a full file name, or can contain a wildcard character (<b>*</b>) to filter the search. If the <paramref name="searchMask" /> is set to <b>*</b>, then
    /// all content files under the directory will be returned.
    /// </para>
    /// </remarks>
    IEnumerable<IContentFile> IContentFileManager.EnumerateContentFiles(string directoryPath, string searchMask, bool recursive)
    {
        if (directoryPath is null)
        {
            throw new ArgumentNullException(nameof(directoryPath));
        }

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentEmptyException(nameof(directoryPath));
        }

        if (string.IsNullOrWhiteSpace(searchMask))
        {
            searchMask = "*";
        }

        (string mask, int searchPatternState, IDirectory parentDir, bool usePattern) = GetSearchState(directoryPath, searchMask);

        IEnumerable<IContentFile> paths = parentDir.Files.Cast<IContentFile>();

        if (recursive)
        {
            paths = paths.Concat(parentDir.Directories
                                          .TraverseBreadthFirst(d => d.Directories)
                                          .SelectMany(item => item.Files)
                                          .Cast<IContentFile>());
        }

        return searchPatternState switch
        {
            1 => paths.Where(item => item.Name.EndsWith(mask, StringComparison.OrdinalIgnoreCase)),
            2 => paths.Where(item => item.Name.StartsWith(mask, StringComparison.OrdinalIgnoreCase)),
            3 => paths.Where(item => item.Name.IndexOf(mask, StringComparison.OrdinalIgnoreCase) != -1),
            _ => usePattern ? paths.Where(item => string.Equals(item.Name, mask, StringComparison.OrdinalIgnoreCase)) : paths,
        };
    }

    /// <summary>Function to retrieve the content sub directories for a given directory path.</summary>
    /// <param name="directoryPath">The directory path to search under.</param>
    /// <param name="searchMask">The search mask to use.</param>
    /// <param name="recursive">[Optional] <b>true</b> to retrieve all files under the path, including those in sub directories, or <b>false</b> to retrieve files in the immediate path.</param>
    /// <returns>An <c>IEnumerable</c> containing the directory paths found on the path.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="directoryPath"/> is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="directoryPath"/> is empty.</exception>
    /// <remarks>
    ///   <para>
    /// This will search on the specified <paramref name="directoryPath" /> for directories that match the <paramref name="searchMask" />.
    /// </para>
    ///   <para>
    /// The <paramref name="searchMask" /> parameter can be a full directory name, or can contain a wildcard character (<b>*</b>) to filter the search. If the <paramref name="searchMask" /> is set to <b>*</b>, then
    /// all sub directories under the directory will be returned.
    /// </para>
    /// </remarks>
    IEnumerable<string> IContentFileManager.EnumerateDirectories(string directoryPath, string searchMask, bool recursive)
    {
        if (directoryPath is null)
        {
            throw new ArgumentNullException(nameof(directoryPath));
        }

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentEmptyException(nameof(directoryPath));
        }

        if (string.IsNullOrWhiteSpace(searchMask))
        {
            searchMask = "*";
        }

        (string mask, int searchPatternState, IDirectory parentDir, bool usePattern) = GetSearchState(directoryPath, searchMask);

        IEnumerable<IDirectory> paths = recursive ? parentDir.Directories
                                                             .TraverseBreadthFirst(d => d.Directories)
                                                  : parentDir.Directories;

        return searchPatternState switch
        {
            1 => paths.Where(item => item.Name.EndsWith(mask, StringComparison.OrdinalIgnoreCase)).Select(item => item.FullPath),
            2 => paths.Where(item => item.Name.StartsWith(mask, StringComparison.OrdinalIgnoreCase)).Select(item => item.FullPath),
            3 => paths.Where(item => item.Name.IndexOf(mask, StringComparison.OrdinalIgnoreCase) != -1).Select(item => item.FullPath),
            _ => usePattern ? paths.Where(item => string.Equals(item.Name, mask, StringComparison.OrdinalIgnoreCase)).Select(item => item.FullPath)
                            : paths.Select(item => item.FullPath),
        };
    }

    /// <summary>Function to retrieve the paths under a given directory.</summary>
    /// <param name="directoryPath">The directory path to search under.</param>
    /// <param name="searchMask">The search mask to use.</param>
    /// <param name="recursive">[Optional] <b>true</b> to retrieve all paths under the directory, including those in sub directories, or <b>false</b> to retrieve paths in the immediate directory path.</param>
    /// <returns>An <c>IEnumerable</c> containing the paths found on the directory.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="directoryPath"/> is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="directoryPath"/> is empty.</exception>
    /// <remarks>
    ///   <para>
    /// This will search on the specified <paramref name="directoryPath" /> for all paths (i.e. both directories and files) that match the <paramref name="searchMask" />.
    /// </para>
    ///   <para>
    /// The <paramref name="searchMask" /> parameter can be a full path part, or can contain a wildcard character (<b>*</b>) to filter the search. If the <paramref name="searchMask" /> is set to <b>*</b>, then
    /// all paths under the directory will be returned.
    /// </para>
    /// </remarks>
    IEnumerable<string> IContentFileManager.EnumeratePaths(string directoryPath, string searchMask, bool recursive)
    {
        if (directoryPath is null)
        {
            throw new ArgumentNullException(nameof(directoryPath));
        }

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new ArgumentEmptyException(nameof(directoryPath));
        }

        if (string.IsNullOrWhiteSpace(searchMask))
        {
            searchMask = "*";
        }

        (string mask, int searchPatternState, IDirectory parentDir, bool usePattern) = GetSearchState(directoryPath, searchMask);

        IEnumerable<IDirectory> paths = recursive ? parentDir.Directories
                                                             .TraverseBreadthFirst(d => d.Directories)
                                                  : parentDir.Directories;

        var allPaths = parentDir.Files.Select(item => new { item.FullPath, item.Name })
                                                      .Concat(paths.Select(item => new { item.FullPath, item.Name })
                                                                   .Concat(paths.SelectMany(item => item.Files).Select(item => new { item.FullPath, item.Name })));

        return searchPatternState switch
        {
            1 => allPaths.Where(item => item.Name.EndsWith(mask, StringComparison.OrdinalIgnoreCase)).Select(item => item.FullPath),
            2 => allPaths.Where(item => item.Name.StartsWith(mask, StringComparison.OrdinalIgnoreCase)).Select(item => item.FullPath),
            3 => allPaths.Where(item => item.Name.IndexOf(mask, StringComparison.OrdinalIgnoreCase) != -1).Select(item => item.FullPath),
            _ => usePattern ? allPaths.Where(item => string.Equals(item.Name, mask, StringComparison.OrdinalIgnoreCase)).Select(item => item.FullPath)
                            : allPaths.Select(item => item.FullPath),
        };
    }

    /// <summary>Function to delete a file.</summary>
    /// <param name="path">The path to the file to delete.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="path"/> is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown whe the <paramref name="path"/> is empty.</exception>
    /// <exception cref="FileNotFoundException">Thrown when the file specified in the <paramref name="path"/> was not found in the file system.</exception>
    /// <exception cref="GorgonException">Thrown if the file specified in the <paramref name="path"/> is open in the editor.</exception>
    void IContentFileManager.DeleteFile(string path)
    {
        if (path is null)
        {
            throw new ArgumentNullException(nameof(path));
        }

        if (string.IsNullOrWhiteSpace(path))
        {
            throw new ArgumentEmptyException(nameof(path));
        }

        IFile file = _files.Values.FirstOrDefault(item => string.Equals(item.FullPath, path, StringComparison.OrdinalIgnoreCase)) ?? throw new FileNotFoundException(string.Format(Resources.GOREDIT_ERR_FILE_NOT_FOUND, path));

        if (file.IsOpen)
        {
            throw new GorgonException(GorgonResult.AccessDenied, string.Format(Resources.GOREDIT_ERR_FILE_LOCKED, path));
        }

        _fileSystemWriter.DeleteFile(file.FullPath);

        // Update UI.
        void UpdateUI(object context)
        {
            OnFileSystemUpdated();
            file.Parent.Files.Remove(file);

            if ((_searchFiles is null) || (!_searchFiles.Remove(file)))
            {
                return;
            }

            NotifyPropertyChanged(nameof(SearchResults));
        }

        if (_syncContext != SynchronizationContext.Current)
        {
            _syncContext.Send(UpdateUI, null);
        }
        else
        {
            UpdateUI(null);
        }
    }

    /// <summary>
    /// Function to retrieve a list of the file paths that are selected on the file system.
    /// </summary>
    /// <returns>The list of selected file paths.</returns>
    IReadOnlyList<string> IContentFileManager.GetSelectedFiles() => SelectedFiles?.Select(item => item.FullPath).ToArray() ?? [];

    /// <summary>
    /// Function to notify the application that the metadata for the file system should be flushed back to the disk.
    /// </summary>
    void IContentFileManager.FlushMetadata() => OnFileSystemUpdated();

    /// <summary>
    /// Function to create a content loader for loading in content information.
    /// </summary>
    /// <param name="textureCache">The cache used to hold texture data.</param>
    /// <returns>A new content loader interface.</returns>
    IGorgonContentLoader IContentFileManager.GetContentLoader(GorgonTextureCache<GorgonTexture2D> textureCache) => _fileSystemWriter.FileSystem.CreateContentLoader(HostServices.GraphicsContext.Renderer2D, textureCache);

    /// <summary>
    /// Function to convert the content file manager to a standard read-only Gorgon virtual file system.
    /// </summary>
    /// <returns>The <see cref="IGorgonFileSystem"/> for this content manager.</returns>
    IGorgonFileSystem IContentFileManager.ToGorgonFileSystem() => _fileSystemWriter.FileSystem;

    /// <summary>
    /// Function to determine if a directory is excluded from a packed file.
    /// </summary>
    /// <param name="directory">Path to the directory to evaluate.</param>
    /// <returns><b>true</b> if excluded, <b>false</b> if not.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="directory"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="directory"/> parameter is empty.</exception>
    /// <exception cref="DirectoryNotFoundException">Thrown if the directory does not exist.</exception>
    bool IContentFileManager.IsDirectoryExcluded(string directory)
    {
        if (directory is null)
        {
            throw new ArgumentNullException(nameof(directory));
        }

        if (string.IsNullOrWhiteSpace(directory))
        {
            throw new ArgumentEmptyException(nameof(directory));
        }

        directory = directory.FormatDirectory('/');

        if (!directory.StartsWith("/", StringComparison.OrdinalIgnoreCase))
        {

            directory = "/" + directory;
        }

        IDirectory dir = _directories.Values.FirstOrDefault(item => string.Equals(item.FullPath, directory, StringComparison.OrdinalIgnoreCase)) ?? throw new DirectoryNotFoundException(string.Format(Resources.GOREDIT_ERR_DIRECTORY_NOT_FOUND, directory));

        if (dir is not IExcludable excluder)
        {
            return false;
        }

        return excluder.IsExcluded;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FileExplorer"/> class.
    /// </summary>
    public FileExplorer()
    {
        SelectDirectoryCommand = new EditorCommand<string>(DoSelectDirectory, CanSelectDirectory);
        SelectFileCommand = new EditorCommand<IReadOnlyList<string>>(DoSelectFile, CanSelectFile);
        RenameDirectoryCommand = new EditorCommand<RenameArgs>(DoRenameDirectory, CanRenameDirectory);
        CreateDirectoryCommand = new EditorCommand<CreateDirectoryArgs>(DoCreateDirectory, CanCreateDirectory);
        DeleteDirectoryCommand = new EditorAsyncCommand<DeleteArgs>(DoDeleteDirectoryAsync, CanDeleteDirectory);
        CopyDirectoryCommand = new EditorAsyncCommand<IDirectoryCopyMoveData>(DoCopyDirectoryAsync, CanCopyDirectory);
        MoveDirectoryCommand = new EditorAsyncCommand<IDirectoryCopyMoveData>(DoMoveDirectoryAsync, CanMoveDirectory);
        CopyFileCommand = new EditorAsyncCommand<IFileCopyMoveData>(DoCopyFilesAsync, CanCopyOrMoveFiles);
        MoveFileCommand = new EditorAsyncCommand<IFileCopyMoveData>(DoMoveFilesAsync, CanCopyOrMoveFiles);
        DeleteFileCommand = new EditorAsyncCommand<DeleteArgs>(DoDeleteFileAsync, CanDeleteFile);
        RenameFileCommand = new EditorCommand<RenameArgs>(DoRenameFile, CanRenameFile);
        SearchCommand = new EditorCommand<string>(DoSearch);
        ExportDirectoryCommand = new EditorAsyncCommand<object>(DoExportDirectoryAsync, CanExportDirectory);
        ExportFilesCommand = new EditorAsyncCommand<object>(DoExportFilesAsync, CanExportFiles);
        ImportCommand = new EditorAsyncCommand<IImportData>(DoImportAsync, CanImport);
        RefreshCommand = new EditorAsyncCommand<object>(DoRefreshAsync);
        GetDirectoryCommand = new EditorCommand<GetDirectoryArgs>(DoGetDirectory, CanGetDirectory);
    }
}
