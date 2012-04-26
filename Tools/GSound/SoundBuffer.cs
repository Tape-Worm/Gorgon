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
    /// Currently only supports Wave,OGG Vorbis files
    /// </summary>
    public enum SoundType : byte
    {
		/// <summary>
		/// Wave file.
		/// </summary>
        Wav = 0,
		/// <summary>
		/// OGG file.
		/// </summary>
        OGG = 1
    }

    /// <summary>
    /// A sound object e.g a wave file, use a SoundSource to play this
    /// </summary>
    public class SoundBuffer : IDisposable
    {
        #region Variables

        /// <summary>
        /// Internal ID for the sound buffer
        /// </summary>
        private int BufferID;
        /// <summary>
        /// Is Disposed yet?
        /// </summary>
        private bool _disposed;

        #endregion

        #region Methods

        /// <summary>
        /// Returns the internal OpenAL ID for this object
        /// </summary>
        /// <returns></returns>
        public int GetBufferID()
        {
            return BufferID;
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Load the sound from a wav file
        /// </summary>
        /// <param name="Filename"></param>
        public SoundBuffer(string Filename)
        {
            BufferID = FileWAV.LoadFromFile(Filename);
        }

        /// <summary>
        /// Load the sound from a wav stream
        /// </summary>
        /// <param name="Stream"></param>
        public SoundBuffer(Stream Stream)
        {
            BufferID = FileWAV.LoadFromStream(Stream);
        }

        /// <summary>
        /// Load the sound from a file
        /// </summary>
        /// <param name="Filename">File name of the file to load.</param>
		/// <param name="Type">Type of file to load.</param>
        public SoundBuffer(string Filename, SoundType Type)
        {
            switch (Type)
            {
                case SoundType.Wav:
                    BufferID = FileWAV.LoadFromFile(Filename);
                    break;
                case SoundType.OGG:
                    BufferID = FileOGG.LoadFromFile(Filename);
                    break;
            }
        }

        /// <summary>
        /// Load the sound from a stream
        /// </summary>
        /// <param name="Stream">Stream containing the audio data.</param>
		/// <param name="Type">Type of file to load.</param>
        public SoundBuffer(Stream Stream, SoundType Type)
        {
            switch (Type)
            {
                case SoundType.Wav:
                    BufferID = FileWAV.LoadFromStream(Stream);
                    break;
                case SoundType.OGG:
                    BufferID = FileOGG.LoadFromStream(Stream);
                    break;
            }
        }

        #endregion

        #region Destructors

        /// <summary>
        /// Finalization
        /// </summary>
        ~SoundBuffer()
        {
            Dispose(false);
        }

        /// <summary>
        /// Clean up the memory for this object
        /// </summary>
        /// <param name="disposing">true if the Dispose method was called outside of a the finalize method</param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {

                    // Do managed dispose here.
                }

                al.DeleteBuffer(BufferID);
                _disposed = true;
            }
        }

        /// <summary>
        /// Clean up the memory for this object
        /// </summary>
        public void Dispose()
        {
            GC.SuppressFinalize(this);
            Dispose(true);
        }

        #endregion
    }
}
