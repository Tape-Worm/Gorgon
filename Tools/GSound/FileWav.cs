#region MIT.
// 
// GSound (Gorgon Sound)
// Copyright (C) 2012 Devin Argent
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
#endregion

using System;
using System.IO;


namespace GorgonLibrary.Sound
{
    /// <summary>
    /// This code was created by Magellan http://trinidad.delphigl.com/. Comments and stream loading were translated/created by ShadowDust702
    /// </summary>
    internal static class FileWAV
    {
        /// <summary>
        /// Header of the Wav file
        /// </summary>
        private class HeaderWAV
        {
            internal byte[] RIFFHeader = new byte[4];
            internal int FileSize;
            internal byte[] WAVEHeader = new byte[4];
            internal byte[] FormatHeader = new byte[4];
            internal int FormatHeaderSize;
            internal short FormatCode;
            internal short ChannelNumber;
            internal int SampleRate;
            internal int BytesPerSecond;
            internal short BytesPerSample;
            internal short BitsPerSample;
        }

        #region Variables

        private const int HeaderSize = 36;
        private const int ChunkHeaderSize = 4;

        private static Stream FStream;
        private static BinaryReader FReader;

        private static HeaderWAV FHeader = new HeaderWAV();

        private static byte[] FDataBuffer;

        #endregion

        #region Methods

        /// <summary>
        /// Reads a string a certain length.
        /// </summary>
        /// <param name="Count">The number of characters to be read.</param>
        /// <returns>Returns the string</returns>
        private static string ReadANSIString(int Count)
        {
            byte[] Buffer = new byte[Count];
            FStream.Read(Buffer, 0, Count);
            string Result = System.Text.Encoding.ASCII.GetString(Buffer);

            int Index = Result.IndexOf('\0');
            if (Index == -1)
                return Result;

            return Result.Substring(0, Index);
        }

        /// <summary>
        /// Throws an exception.
        /// </summary>
        private static void RaiseUnknownFormatException()
        {
            new Exception("The format is not supported");
        }

        /// <summary>
        /// Reads the header of the WAV file.
        /// </summary>
        private static void ReadHeader()
        {
            FReader.Read(FHeader.RIFFHeader, 0, 4);
            FHeader.FileSize = FReader.ReadInt32();
            FReader.Read(FHeader.WAVEHeader, 0, 4);
            FReader.Read(FHeader.FormatHeader, 0, 4);
            FHeader.FormatHeaderSize = FReader.ReadInt32();
            FHeader.FormatCode = FReader.ReadInt16();
            FHeader.ChannelNumber = FReader.ReadInt16();
            FHeader.SampleRate = FReader.ReadInt32();
            FHeader.BytesPerSecond = FReader.ReadInt32();
            FHeader.BytesPerSample = FReader.ReadInt16();
            FHeader.BitsPerSample = FReader.ReadInt16();

            FStream.Seek(FHeader.FormatHeaderSize - 16, SeekOrigin.Current);
        }

        /// <summary>
        /// Reads information from the chunk with the name "data".
        /// </summary>
        private static void ReadDataChunk()
        {
            int BufferSize;
            BufferSize = FReader.ReadInt32();

            FDataBuffer = new byte[BufferSize];
            FStream.Read(FDataBuffer, 0, BufferSize);
        }

        /// <summary>
        /// Reads all other information from the file.
        /// </summary>
        private static void ReadData()
        {
            do
            {
                switch (ReadANSIString(ChunkHeaderSize))
                {
                    case "data":
                        ReadDataChunk();
                        break;
                    default:
                        try
                        {
                            //ignore unknown chunks
                            FStream.Seek(FReader.ReadInt32(), SeekOrigin.Current);
                        }
                        catch
                        {
                            return;
                        }
                        break;
                }
            }
            while (FStream.Position < FStream.Length);
        }

        /// <summary>
        /// Creates the buffer that contains all the necessary information from the file.
        /// </summary>
        /// <returns>The sound buffer which contains the sound information.</returns>
        private static int GenerateSoundBuffer()
        {
            int Result = al.GenBuffer();

            
            if (FHeader.ChannelNumber == 1)
                switch (FHeader.BitsPerSample)
                {
                    case 8:
                        al.BufferData(Result, al.FORMAT_MONO8, FDataBuffer, FDataBuffer.Length, FHeader.SampleRate);
                        break;
                    case 16:
                        al.BufferData(Result, al.FORMAT_MONO16, FDataBuffer, FDataBuffer.Length, FHeader.SampleRate);
                        break;
                    default:
                        RaiseUnknownFormatException();
                        break;
                }
            else if (FHeader.ChannelNumber == 2)
                switch (FHeader.BitsPerSample)
                {
                    case 8:
                        al.BufferData(Result, al.FORMAT_STEREO8, FDataBuffer, FDataBuffer.Length, FHeader.SampleRate);
                        break;
                    case 16:
                        al.BufferData(Result, al.FORMAT_STEREO16, FDataBuffer, FDataBuffer.Length, FHeader.SampleRate);
                        break;
                    default:
                        RaiseUnknownFormatException();
                        break;
                }
            else
                RaiseUnknownFormatException();

            return Result;
        }

        #endregion

        #region Loading

        /// <summary>
        /// Loads a wav from a file
        /// </summary>
        /// <param name="FileName">Filename of the file to open.</param>
        /// <returns>Result code.</returns>
        public static int LoadFromFile(string FileName)
        {
            FStream = new FileStream(FileName, FileMode.Open);
            FReader = new BinaryReader(FStream);
            try
            {
                ReadHeader();
                ReadData();
            }
            finally
            {
                FReader.Close();
            }
            int Result = GenerateSoundBuffer();
            FDataBuffer = null;
            return Result;
        }

        /// <summary>
        /// Loads a wav from a stream
        /// </summary>
        /// <param name="In">Input stream.</param>
        /// <returns>Result code.</returns>
        public static int LoadFromStream(Stream In)
        {
            In.Position = 0;
            FStream = In;
            FReader = new BinaryReader(In);
            try
            {
                ReadHeader();
                ReadData();
            }
            finally
            {
                FReader.Close();
            }
            int Result = GenerateSoundBuffer();
            FDataBuffer = null;
            return Result;
        }

        #endregion
    }
}
