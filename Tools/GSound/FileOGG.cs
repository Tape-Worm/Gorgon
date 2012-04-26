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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;


namespace GorgonLibrary.Sound
{
    /// <summary>
    /// Loads an OGG in to a Buffer
    /// </summary>
    internal static class FileOGG
    {
        

        /// <summary>
        /// Creates the buffer that contains all the necessary information from the file.
        /// </summary>
        /// <returns>The sound buffer which contains the sound information.</returns>
        private static int GenerateSoundBuffer(OggDecodeStream Decoded)
        {
            Decoded.Position = 0;
            byte[] Data = new byte[Decoded.Length];
            Decoded.Read(Data, 0, (int)Decoded.Length);

            int Result = al.GenBuffer();


            if (Decoded.Channels == 1)
                al.BufferData(Result, al.FORMAT_MONO16, Data, Data.Length, Decoded.Rate);
            else if (Decoded.Channels == 2)
                al.BufferData(Result, al.FORMAT_STEREO16, Data, Data.Length, Decoded.Rate);

            return Result;
        }
        
        /// <summary>
        /// Load an OGG vorbis buffer from a file
        /// </summary>
        /// <param name="Filename"></param>
        /// <returns></returns>
        public static int LoadFromFile(string Filename)
        {
            FileStream FS = new FileStream(System.IO.Directory.GetCurrentDirectory() + "\\" + Filename,FileMode.Open);


            OggDecodeStream Stream = new OggDecodeStream(FS,true);

            

            Stream.Position = 0;
            int Result = GenerateSoundBuffer(Stream);

            Stream.Close();
            return Result;
        }

        /// <summary>
        /// Load an OGG vorbis buffer from a stream.
        /// </summary>
        /// <param name="Filename"></param>
        /// <returns></returns>
        public static int LoadFromStream(Stream Stream)
        {
            //This is pretty much the same as "LoadFromFile" but it doesn't open the stream automagically
            OggDecodeStream OggStream = new OggDecodeStream(Stream, true);



            Stream.Position = 0;
            int Result = GenerateSoundBuffer(OggStream);

            OggStream.Close();
            return Result;
        }
    }
}
