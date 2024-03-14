
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: January 15, 2020 10:24:37 AM
// 


using Gorgon.Diagnostics;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ViewModels;

/// <summary>
/// The clip board handler for file system objects
/// </summary>
internal class FileSystemClipboardHandler
    : PropertyMonitor, IClipboardHandler
{

    // Flag to indicate whether clipboard data is available.
    private bool _hasClipboardData;
    // The service used to access the clipboard.
    private readonly IClipboardService _clipboardService;
    // The message display service.
    private readonly IGorgonLog _log;
    // The file explorer view model that owns this handler.
    private readonly IFileExplorer _fileExplorer;
    // Supported data types for the clipboard.
    private readonly Type _supportedDataTypeDirectory = typeof(IDirectoryCopyMoveData);
    private readonly Type _supportedDataTypeFile = typeof(IFileCopyMoveData);



    /// <summary>Property to return whether the clipboard has data or not.</summary>
    public bool HasData
    {
        get => _hasClipboardData;
        private set
        {
            if (_hasClipboardData == value)
            {
                return;
            }

            OnPropertyChanging();
            _hasClipboardData = value;
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return the command that returns the type of data present on the clipboard (if any).</summary>
    public IEditorCommand<GetClipboardDataTypeArgs> GetClipboardDataTypeCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to clear the clipboard.
    /// </summary>
    public IEditorCommand<object> ClearCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command used to copy data into the clipboard.
    /// </summary>
    public IEditorCommand<object> CopyDataCommand
    {
        get;
    }

    /// <summary>Property to return the command used to paste data from the clipboard.</summary>
    public IEditorAsyncCommand<object> PasteDataCommand
    {
        get;
    }




    /// <summary>
    /// Function to determine if data can be pasted into the file system.
    /// </summary>
    /// <returns><b>true</b> if data can be pasted, <b>false</b> if not.</returns>
    private bool CanPasteData()
    {
        if ((!HasData)
            || (_fileExplorer?.CopyDirectoryCommand is null)
            || (_fileExplorer.CopyFileCommand is null)
            || (_fileExplorer.MoveDirectoryCommand is null)
            || (_fileExplorer.MoveFileCommand is null))
        {
            return false;
        }

        if (_clipboardService.IsType<FileCopyMoveData>())
        {
            FileCopyMoveData data = _clipboardService.GetData<FileCopyMoveData>();
            data.DestinationDirectory = _fileExplorer.SelectedDirectory?.ID ?? string.Empty;

            return data.Operation switch
            {
                CopyMoveOperation.Copy => _fileExplorer.CopyFileCommand.CanExecute(data),
                CopyMoveOperation.Move => _fileExplorer.MoveFileCommand.CanExecute(data),
                _ => false,
            };
        }
        else if (_clipboardService.IsType<DirectoryCopyMoveData>())
        {
            DirectoryCopyMoveData data = _clipboardService.GetData<DirectoryCopyMoveData>();
            data.DestinationDirectory = _fileExplorer.SelectedDirectory?.ID ?? string.Empty;

            return data.Operation switch
            {
                CopyMoveOperation.Copy => _fileExplorer.CopyDirectoryCommand.CanExecute(data),
                CopyMoveOperation.Move => _fileExplorer.MoveDirectoryCommand.CanExecute(data),
                _ => false,
            };
        }

        return false;
    }

    /// <summary>
    /// Function to paste data into the file system from the clipboard.
    /// </summary>
    private async Task DoPasteDataAsync()
    {
        try
        {
            if (_clipboardService.IsType<DirectoryCopyMoveData>())
            {
                DirectoryCopyMoveData directoryData = _clipboardService.GetData<DirectoryCopyMoveData>();

                if (directoryData is null)
                {
                    return;
                }

                directoryData.DestinationDirectory = _fileExplorer.SelectedDirectory?.ID ?? string.Empty;

                if (string.IsNullOrWhiteSpace(directoryData.DestinationDirectory))
                {
                    return;
                }

                switch (directoryData.Operation)
                {
                    case CopyMoveOperation.Copy:
                        await _fileExplorer.CopyDirectoryCommand.ExecuteAsync(directoryData);
                        break;
                    case CopyMoveOperation.Move:
                        await _fileExplorer.MoveDirectoryCommand.ExecuteAsync(directoryData);
                        _clipboardService.Clear();
                        HasData = false;
                        break;
                }

                return;
            }

            if (!_clipboardService.IsType<FileCopyMoveData>())
            {
                return;
            }

            FileCopyMoveData fileData = _clipboardService.GetData<FileCopyMoveData>();

            if (fileData is null)
            {
                return;
            }

            switch (fileData.Operation)
            {
                case CopyMoveOperation.Copy:
                    await _fileExplorer.CopyFileCommand.ExecuteAsync(fileData);
                    break;
                case CopyMoveOperation.Move:
                    await _fileExplorer.MoveFileCommand.ExecuteAsync(fileData);
                    _clipboardService.Clear();
                    HasData = false;
                    break;
            }
        }
        catch (Exception ex)
        {
            _log.Print("Error while pasting data from the clipboard into the file system.", LoggingLevel.Simple);
            _log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to retrieve the type of data stored on the clipboard.
    /// </summary>
    /// <param name="args">The arguments for the command.</param>
    private void DoGetClipboardDataType(GetClipboardDataTypeArgs args)
    {
        try
        {
            if (_clipboardService.HasData)
            {
                if (_clipboardService.IsType<FileCopyMoveData>())
                {
                    args.DataType = typeof(IFileCopyMoveData);
                }
                else if (_clipboardService.IsType<DirectoryCopyMoveData>())
                {
                    args.DataType = typeof(IDirectoryCopyMoveData);
                }
            }

            HasData = args.HasData;
        }
        catch (Exception ex)
        {
            _log.Print("Error retrieving clipboard data type information.", LoggingLevel.All);
            _log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to determine if the data can be copied or cut.
    /// </summary>
    /// <param name="args">The data to copy or cut.</param>
    /// <returns><b>true</b> if the data can be copied or cut, <b>false</b> if not.</returns>
    private bool CanCopyData(object args)
    {
        Type argsType = args?.GetType();

        if (argsType is null)
        {
            return true;
        }

        if (_supportedDataTypeFile.IsAssignableFrom(argsType))
        {
            var fileData = (FileCopyMoveData)args;
            return (fileData.SourceFiles is not null) && (fileData.SourceFiles.Count > 0);
        }

        if (!_supportedDataTypeDirectory.IsAssignableFrom(argsType))
        {
            return false;
        }

        var dirData = (DirectoryCopyMoveData)args;

        return ((dirData is not null) && (!string.IsNullOrWhiteSpace(dirData.SourceDirectory))
            && ((dirData.Operation != CopyMoveOperation.Move) || (!string.Equals(dirData.SourceDirectory, _fileExplorer.Root.ID, StringComparison.OrdinalIgnoreCase))));
    }

    /// <summary>
    /// Function to copy data into the clipboard.
    /// </summary>
    /// <param name="args">The data to copy.</param>
    private void DoCopyData(object args)
    {
        try
        {
            DoClearClipboard();

            if (args is null)
            {
                return;
            }

            _clipboardService.CopyItem(args);
            HasData = true;
        }
        catch (Exception ex)
        {
            _log.Print("Error copying data into the clipboard.", LoggingLevel.All);
            _log.LogException(ex);
        }
    }

    /// <summary>
    /// Function to clear the clipboard.
    /// </summary>
    private void DoClearClipboard()
    {
        try
        {
            _clipboardService.Clear();
            HasData = false;
        }
        catch (Exception ex)
        {
            _log.Print("Error clearing the clipboard.", LoggingLevel.All);
            _log.LogException(ex);
        }
    }



    /// <summary>Initializes a new instance of the <see cref="FileSystemClipboardHandler"/> class.</summary>
    /// <param name="fileExplorer">The file explorer view model that owns this handler.</param>
    /// <param name="clipboardService">The clipboard service used to access clipboard data in Windows.</param>
    /// <param name="log">The log for debug messages.</param>
    public FileSystemClipboardHandler(IFileExplorer fileExplorer, IClipboardService clipboardService, IGorgonLog log)
    {
        _fileExplorer = fileExplorer;
        _clipboardService = clipboardService;
        _log = log;

        ClearCommand = new EditorCommand<object>(DoClearClipboard);
        GetClipboardDataTypeCommand = new EditorCommand<GetClipboardDataTypeArgs>(DoGetClipboardDataType);
        CopyDataCommand = new EditorCommand<object>(DoCopyData, CanCopyData);
        PasteDataCommand = new EditorAsyncCommand<object>(DoPasteDataAsync, CanPasteData);
    }

}
