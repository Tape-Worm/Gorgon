#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: January 7, 2019 9:21:59 PM
// 
#endregion

using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Mm = SharpDX.Multimedia;
using Xa = SharpDX.XAudio2;

namespace Gorgon.Examples
{
    /// <summary>
    /// Provides audio playback for the example.
    /// </summary>
    internal class AudioPlayback
        : IDisposable
    {
        #region Variables.
        // The main XAudio interface.
        private readonly Xa.XAudio2 _audio;
        // The master voice for playback.
        private readonly Xa.MasteringVoice _masterVoice;
        // Flag to indicate that the audio is playing.
        private CancellationTokenSource _tokenSource;
        // The currently playing track.
        private Task _currentPlayback;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether or not the audio is playing.
        /// </summary>
        public bool IsPlaying => (_tokenSource != null) && (_tokenSource.Token.IsCancellationRequested);
        #endregion

        #region Methods.
        /// <summary>
        /// Function to stop the audio.
        /// </summary>
        public void Stop()
        {
            if (_tokenSource != null)
            {
                _tokenSource.Cancel();
            }

            _currentPlayback?.Wait();
            _currentPlayback = null;
        }

        /// <summary>
        /// Function to play an mp3.
        /// </summary>
        /// <param name="path">The path to the mp3 file.</param>
        public async Task PlayMp3Async(string path)
        {
            if (_currentPlayback != null)
            {
                await _currentPlayback;
                return;
            }
                        
            _tokenSource = new CancellationTokenSource();
            _currentPlayback = Task.Run(() =>
                               {
                                   var stream = new Mm.SoundStream(File.Open(path, FileMode.Open, FileAccess.Read, FileShare.None));
                                   Mm.WaveFormat format = stream.Format;
                                   var buffer = new Xa.AudioBuffer
                                   {
                                       Stream = stream.ToDataStream(),
                                       AudioBytes = (int)stream.Length,
                                       Flags = Xa.BufferFlags.EndOfStream
                                   };

                                   stream.Close();

                                   var source = new Xa.SourceVoice(_audio, format);
                                   source.SubmitSourceBuffer(buffer, stream.DecodedPacketsInfo);
                                   source.Start();

                                   try
                                   {
                                       var waiter = new SpinWait();
                                       while ((!_tokenSource.Token.IsCancellationRequested) && (!source.IsDisposed) && (source.State.BuffersQueued > 0))
                                       {
                                           waiter.SpinOnce();
                                       }

                                       source.Stop();
                                   }
                                   finally
                                   {                                       
                                       buffer.Stream?.Dispose();
                                       source.Dispose();
                                       stream?.Dispose();
                                   }
                               }, _tokenSource.Token);

            await _currentPlayback;
            _currentPlayback = null;
            CancellationTokenSource tokenSource = Interlocked.Exchange(ref _tokenSource, null);
            tokenSource?.Dispose();
            tokenSource = null;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _masterVoice?.Dispose();
            _audio?.Dispose();
            _tokenSource?.Cancel();
            _tokenSource?.Dispose();
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Examples.AudioPlayback"/> class.</summary>
        public AudioPlayback()
        {
            _audio = new Xa.XAudio2();
            _masterVoice = new Xa.MasteringVoice(_audio);
        }
        #endregion
    }
}
