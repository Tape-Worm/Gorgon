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
// Created: November 12, 2018 10:23:07 PM
// 
#endregion

using System.Diagnostics;
using Gorgon.Core;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;

namespace Gorgon.Editor.ImageEditor;

/// <summary>
/// The texture conversion functionality for the external texconv process.
/// </summary>
/// <remarks>Initializes a new instance of the TexConvCompressor class.</remarks>
/// <param name="texConvFile">The tex conv file.</param>
/// <param name="scratchWriter">The scratch writer.</param>
/// <param name="codec">The codec used to read/write the file.</param>
internal class TexConvCompressor(FileInfo texConvFile, IGorgonFileSystemWriter<Stream> scratchWriter, IGorgonImageCodec codec)
{
    #region Conversion Formats.
    // Formats used to convert from a compressed format to an expanded format.
    private readonly Dictionary<BufferFormat, string> _d3dFormats = new()
    {
        {
            BufferFormat.BC1_UNorm,
               "B8G8R8A8_UNORM"
        },
        {
            BufferFormat.BC1_UNorm_SRgb,
               "B8G8R8A8_UNORM_SRGB"
        },
        {
            BufferFormat.BC2_UNorm,
               "B8G8R8A8_UNORM"
        },
        {
            BufferFormat.BC2_UNorm_SRgb,
               "B8G8R8A8_UNORM_SRGB"
        },
        {
            BufferFormat.BC3_UNorm,
               "R8G8B8A8_UNORM"
        },
        {
            BufferFormat.BC3_UNorm_SRgb,
               "R8G8B8A8_UNORM_SRGB"
        },
        {
            BufferFormat.BC4_SNorm,
               "R8_SNORM"
        },
        {
            BufferFormat.BC4_UNorm,
               "R8_UNORM"
        },
        {
            BufferFormat.BC5_SNorm,
               "R8G8_SNORM"
        },
        {
            BufferFormat.BC5_UNorm,
               "R8G8_UNORM"
        },
        {
            BufferFormat.BC6H_Sf16,
               "R16G16B16A16_FLOAT"
        },
        {
            BufferFormat.BC6H_Uf16,
               "R16G16B16A16_FLOAT"
        },
        {
            BufferFormat.BC7_UNorm,
               "R8G8B8A8_UNORM"
        },
        {
            BufferFormat.BC7_UNorm_SRgb,
               "R8G8B8A8_UNORM_SRGB"
        }
    };
    #endregion

    #region Variables.
    // The path to the texture converter process.
    private readonly FileInfo _texConv = texConvFile;
    // The file system writer to use.
    private readonly IGorgonFileSystemWriter<Stream> _writer = scratchWriter;
    // PlugIn image file codec.
    private readonly IGorgonImageCodec _codec = codec;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to compress an image using block compression.
    /// </summary>
    /// <param name="imageFile">The file to compress.</param>
    /// <param name="format">The block compression format to use.</param>
    /// <param name="mipCount">The number of mip levels.</param>
    /// <returns>The virtual file with the compressed data.</returns>
    public IGorgonVirtualFile Compress(IGorgonVirtualFile imageFile, BufferFormat format, int mipCount)
    {
        string directory = Path.GetDirectoryName(imageFile.PhysicalFile.FullPath);
        Process texConvProcess = null;
        IGorgonVirtualFile encodedFile = null;

        var info = new ProcessStartInfo
        {
            Arguments = $"-f {format.ToString().ToUpper()} -y -m {mipCount} -fl 12.0 -ft DDS -o \"{directory}\" -nologo \"{imageFile.PhysicalFile.FullPath}\"",
            ErrorDialog = true,
            FileName = _texConv.FullName,
            WorkingDirectory = directory,
            UseShellExecute = false,
#if DEBUG
            CreateNoWindow = false,
#else
            CreateNoWindow = true,
#endif
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };

        try
        {
            texConvProcess = Process.Start(info);

            if (texConvProcess is null)
            {
                return null;
            }

            texConvProcess.WaitForExit();

            string errorData = texConvProcess.StandardError.ReadToEnd();

            if (errorData.StartsWith("Invalid value", StringComparison.OrdinalIgnoreCase))
            {
                errorData = errorData[..errorData.IndexOf("\n", StringComparison.OrdinalIgnoreCase)];
                throw new GorgonException(GorgonResult.CannotWrite, Resources.GORIMG_ERR_CANNOT_COMPRESS, new IOException(errorData));
            }

            errorData = texConvProcess.StandardOutput.ReadToEnd();

            if (errorData.Contains("FAILED"))
            {
                throw new GorgonException(GorgonResult.CannotWrite, Resources.GORIMG_ERR_CANNOT_COMPRESS, new IOException(errorData));
            }

            _writer.FileSystem.Refresh();

            encodedFile = _writer.FileSystem.GetFile(imageFile.FullPath);
            return encodedFile;
        }
        finally
        {
            texConvProcess?.Dispose();
        }
    }

    /// <summary>
    /// Function to decompress a block compressed file into a standard pixel format.
    /// </summary>
    /// <param name="imageFile">The file to decompress</param>
    /// <param name="metadata">The metadata for the file.</param>
    /// <returns>The decompressed image.</returns>
    public IGorgonImage Decompress(ref IGorgonVirtualFile imageFile, IGorgonImageInfo metadata)
    {
        string directory = Path.GetDirectoryName(imageFile.PhysicalFile.FullPath);
        IGorgonImage result = null;
        Process texConvProcess = null;
        Stream inStream = null;
        IGorgonVirtualFile decodedFile = null;

        var info = new ProcessStartInfo
        {
            Arguments = $"-f {_d3dFormats[metadata.Format]} -y -m {metadata.MipCount} -fl 12.0 -ft DDS -o \"{directory}\" -nologo -px decoded \"{imageFile.PhysicalFile.FullPath}\"",
            ErrorDialog = true,
            FileName = _texConv.FullName,
            WorkingDirectory = directory,
            UseShellExecute = false,
#if DEBUG
            CreateNoWindow = false,
#else
            CreateNoWindow = true,
#endif
            RedirectStandardError = true,
            RedirectStandardOutput = true
        };

        try
        {
            texConvProcess = Process.Start(info);

            if (texConvProcess is null)
            {
                return null;
            }

            texConvProcess.WaitForExit();

            _writer.FileSystem.Refresh();
            imageFile = _writer.FileSystem.GetFile(imageFile.FullPath);

            decodedFile = _writer.FileSystem.GetFile(imageFile.Directory.FullPath + "decoded" + imageFile.Name);
            inStream = decodedFile.OpenStream();
            result = _codec.FromStream(inStream);

            return result;
        }
        finally
        {
            texConvProcess?.Dispose();
            inStream?.Dispose();

            if (decodedFile is not null)
            {
                _writer.DeleteFile(decodedFile.FullPath);
            }
        }
    }

    #endregion
}
