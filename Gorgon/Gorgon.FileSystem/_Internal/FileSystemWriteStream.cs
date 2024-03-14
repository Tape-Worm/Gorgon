
// 
// Gorgon
// Copyright (C) 2015 Michael Winsor
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
// Created: Thursday, September 24, 2015 8:33:14 PM
// 


namespace Gorgon.IO;

/// <summary>
/// A file stream writer that will update the virtual file information when it closes
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="FileSystemWriteStream"/> class
/// </remarks>
/// <param name="writePath">The path to the writable file.</param>
/// <param name="fileMode">The file mode to use to open the file.</param>
internal class FileSystemWriteStream(string writePath, FileMode fileMode)
        : FileStream(writePath,
           fileMode,
           fileMode == FileMode.Open ? FileAccess.Read : fileMode == FileMode.OpenOrCreate ? FileAccess.ReadWrite : FileAccess.Write,
           fileMode == FileMode.Open ? FileShare.Read : FileShare.None)
{

    // The length of the stream, in bytes.
    private long _length;
    // Flag to indicate that the stream is disposed.
    private int _disposed;



    /// <summary>Gets the length in bytes of the stream.</summary>
    public override long Length => (_disposed == 0) ? base.Length : _length;

    /// <summary>
    /// Property to set or return the callback to execute once the stream is closed.
    /// </summary>
    public Action<FileStream> OnCloseCallback
    {
        get;
        set;
    }



    /// <summary>
    /// Releases the unmanaged resources used by the <see cref="FileStream"/> and optionally releases the managed resources.
    /// </summary>
    /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources. </param>
    protected override void Dispose(bool disposing)
    {
        try
        {
            if (disposing)
            {
                if (Interlocked.Exchange(ref _disposed, 1) == 1)
                {
                    base.Dispose(disposing);
                    return;
                }

                Interlocked.Exchange(ref _length, Length);
            }

            base.Dispose(disposing);

            if (disposing)
            {
                OnCloseCallback?.Invoke(this);
            }
        }
        finally
        {
            // Always unhook the callback to avoid leakage.
            OnCloseCallback = null;
        }
    }


}
