// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: April 28, 2025 10:12:23 PM
//

using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Input;
using Avalonia.Controls.Selection;
using Gorgon.IO;
using ReactiveUI;

namespace Gorgon.Examples;

/// <summary>
/// The view model for the chunk file.
/// </summary>
/// <remarks>
/// In here we will show how to use Gorgon's chunked file format and read a chunked file (like a Gorgon animation file, or sprite file). 
/// 
/// Chunked files are a file format in Gorgon that allow grouping of parts of data into sections called "Chunks". These chunks then can be written and read independently of one another. 
/// This allows an application to skip reading chunks if they're not necessary, and aids in forward and backward compatibility.
/// 
/// For more information, check the Gorgon documentation: https://www.tape-worm.net/GorgonDocs/articles/ChunkFileFormat.html
/// </remarks>
internal class ChunkFileViewModel
    : ReactiveObject
{
    private string _chunkFileData = string.Empty;
    private string _filePath = string.Empty;
    // These are the types of chunked files we're able to read.
    // We can load v3.0 sprite files, v3.1 animation files and v1.1 font files.
    private readonly IEnumerable<ulong> _chunkFileIDs = ["GORSPR30".ChunkID(), "GORANM31".ChunkID(), "GORFNT11".ChunkID()];

    /// <summary>
    /// Property to return the chunk file data.
    /// </summary>
    public string ChunkFileData
    {
        get => _chunkFileData;
        private set => this.RaiseAndSetIfChanged(ref _chunkFileData, value);
    }

    private string _errorMessage = string.Empty;

    /// <summary>
    /// Property to return any error message.
    /// </summary>
    public string ErrorMessage
    {
        get => _errorMessage;
        private set => this.RaiseAndSetIfChanged(ref _errorMessage, value);
    }

    /// <summary>
    /// Property to return the title for the application.
    /// </summary>
    public string Title => string.IsNullOrWhiteSpace(_filePath) ? "Chunk File" : $"Chunk File - {_filePath}";

    /// <summary>
    /// Property to return the command to execute to open a chunk file.
    /// </summary>
    public ICommand LoadChunkFileCommand
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the command to close the error display.
    /// </summary>
    public ICommand CloseErrorCommand
    {
        get;
        private set;
    }

    private ObservableCollection<string> _chunks = [];

    /// <summary>
    /// Property to return the list of chunks in the file.
    /// </summary>
    public ObservableCollection<string> Chunks
    {
        get => _chunks;
        private set => this.RaiseAndSetIfChanged(ref _chunks, value);
    }

    /// <summary>
    /// Property to return the selection model for our chunk list.
    /// </summary>
    public SelectionModel<string> Selection
    {
        get;
    } = new();

    /// <summary>
    /// Event handler to handle when selections change in the chunk list.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameter.</param>
    private void Selection_SelectionChanged(object? sender, SelectionModelSelectionChangedEventArgs<string> e)
    {
        try
        {
            ErrorMessage = string.Empty;

            if ((e.SelectedItems.Count == 0) || (string.IsNullOrWhiteSpace(e.SelectedItems[0])))
            {
                ChunkFileData = string.Empty;
                return;
            }

            // We will retrieve the ID of the chunk that holds the data we're interested in.
            // The chunk ID is an 8 byte unsigned long value, and can be any value dictated by the file format.
            // Typically, it's easiest to use an 8 character string value and call the .ChunkID() extension 
            // method to convert the string to 8 byte ulong value, which is what we're doing with the list box 
            // items:
            ulong id = e.SelectedItems[0]?.ChunkID() ?? 0;

            // Once we have the ID that we want, open the chunk file via a stream.
            using Stream stream = File.Open(_filePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            using GorgonChunkFileReader file = new(stream, _chunkFileIDs);

            // Now we open the chunk file. This will validate the stream data and determine if this a proper 
            // Gorgon chunk file or not.
            file.Open();

            // This will open a specific chunk based on the chunk ID that we gathered above. If a chunk is not 
            // found then an exception is thrown, otherwise it will return an instance of a chunk reader which 
            // allows the user to read the data contained within that chunk.
            // This reader works similarly to a .NET BinaryReader.
            using IGorgonChunkReader chunk = file.OpenChunk(id);

            // Each chunk stores a information about itself, including offset, and size. In this case we'll 
            // read the entire chunk into a byte array.
            byte[] chunkData = new byte[chunk.ChunkInfo.Size];

            // Now read the data.
            if (chunkData.Length == 0)
            {
                ChunkFileData = "No data in chunk.";
            }
            else
            {
                StringBuilder chunkString = new();
                chunk.ReadArray(chunkData);

                chunkString.AppendFormat("Chunk {0}\nFile offset: 0x{1:x} ({1} bytes)\nSize: {2} bytes.\n\nData:\n", e.SelectedItems[0], chunk.ChunkInfo.FileOffset, chunkData.Length);

                for (int i = 0; i < chunkData.Length; ++i)
                {
                    if (i > 0)
                    {
                        chunkString.Append(" ");
                    }

                    chunkString.AppendFormat("0x{0:x2}", chunkData[i]);
                }

                ChunkFileData = chunkString.ToString();
            }

            // We can explicitly close the chunk file at this point. Or it will be closed when the object is
            // disposed. It's best practice to close as soon as you are done with the file however.
            // This does not close the stream, and in fact will advance the stream to the end of the chunk
            // file.
            file.Close();
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    /// <summary>
    /// Function to load a chunk file from the file system.
    /// </summary>
    private async Task LoadChunkFileAsync()
    {
        try
        {
            ErrorMessage = string.Empty;

            string path = await LoadFilePickerInteraction.GetFilePathAsync();

            if (path == string.Empty)
            {
                return;
            }

            using Stream stream = File.Open(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            using GorgonChunkFile file = new GorgonChunkFileReader(stream, _chunkFileIDs);

            file.Open();

            ObservableCollection<string> chunkValues = [];

            foreach (GorgonChunk chunkID in file.Chunks)
            {
                chunkValues.Add(chunkID.ID.ToChunkString());
            }

            file.Close();

            Selection.Clear();
            Chunks = chunkValues;
            _filePath = path;

            this.RaisePropertyChanged(nameof(Title));

            if (chunkValues.Count > 0)
            {
                Selection.Select(0);
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    /// <summary>
    /// Initializes a new instance of the chunk file view model.
    /// </summary>
    public ChunkFileViewModel()
    {
        LoadChunkFileCommand = ReactiveCommand.CreateFromTask(LoadChunkFileAsync);
        CloseErrorCommand = ReactiveCommand.Create(() => ErrorMessage = string.Empty);
        Selection.SelectionChanged += Selection_SelectionChanged;
    }
}
