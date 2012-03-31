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
using GorgonLibrary;


namespace GorgonLibrary.Sound
{
    /// <summary>
    /// A Sound source with 3D positioning
    /// </summary>
    public class SoundSource : IDisposable
    {
        /// <summary>
        /// Shows the state a sound could be in
        /// </summary>
        public enum SoundStates : int
        {
			/// <summary>
			/// Initial state.
			/// </summary>
            INITIAL = al.INITIAL,
			/// <summary>
			/// Stopped playing.
			/// </summary>
            STOPPED = al.STOPPED,
			/// <summary>
			/// Currently playing.
			/// </summary>
            PLAYING = al.PLAYING,
			/// <summary>
			/// Paused.
			/// </summary>
            PAUSED = al.PAUSED
        }

        #region Variables

        /// <summary>
        /// The OpenAL Sound source object
        /// </summary>
        private int Source;
        /// <summary>
        /// Pitch
        /// </summary>
        private float _Pitch;
        /// <summary>
        /// Gain
        /// </summary>
        private float _Gain;
        /// <summary>
        /// World coordinates
        /// </summary>
        private Vector3D _Position;
        /// <summary>
        /// Velocity
        /// </summary>
        private Vector3D _Velocity;
        /// <summary>
        /// If true this sound source will loop
        /// </summary>
        private bool _Loop;
        private float _ReferenceDistance;
        private float _MaxRange;
        private bool _Paused = false;
        /// <summary>
        /// Reference to the sound file/buffer
        /// </summary>
        private SoundBuffer _Buffer;
        private bool _disposed;


        #endregion

        #region Properties

        /// <summary>
        /// The pitch of the sound, can be useful for things like cars, that require varying pitches
        /// </summary>
        public float Pitch
        {
            get
            {
                return _Pitch;
            }
            set
            {
                al.Sourcef(Source, al.PITCH, value);
                _Pitch = value;
            }
        }
        
        /// <summary>
        /// If someone notices a difference when they change this, let me know...
        /// </summary>
        public float Gain
        {
            get
            {
                return _Gain;
            }
            set
            {
                al.Sourcef(Source, al.GAIN, value);
                _Gain = value;
            }
        }
        
        /// <summary>
        /// The 3 Dimensional position of this sound object
        /// </summary>
        public Vector3D Position
        {
            get
            {
                return _Position;
            }
            set
            {
                al.Sourcefv(Source, al.POSITION, new float[3] { value.X, value.Y, value.Z });
                _Position = value;
            }
        }
        
        /// <summary>
        /// The 3 Dimensional velocity of this sound object
        /// </summary>
        public Vector3D Velocity
        {
            get
            {
                return _Velocity;
            }
            set
            {
                if (value.X < 0)
                    value.X = -value.X;
                if (value.Y < 0)
                    value.Y = -value.Y;
                if (value.Z < 0)
                    value.Z = -value.Z;
                value = value * 0.1f;
                al.Sourcefv(Source, al.VELOCITY, new float[3] { value.X , value.Y, value.Z});
                _Velocity = value;
            }
        }
        
        /// <summary>
        /// If true this sound will loop while it's being played
        /// </summary>
        public bool Loop
        {
            get
            {
                return _Loop;
            }
            set
            {
                if (value)
                {
                    al.Sourcei(Source, al.LOOPING, al.TRUE);
                }
                else
                {
                    al.Sourcei(Source, al.LOOPING, al.FALSE);
                }
                _Loop = value;
            }
        }
        
        /// <summary>
        /// The distance that the source will be the loudest (if the listener is
        /// closer, it won't be any louder than if they were at this distance)
        /// </summary>
        public float ReferenceDistance
        {
            get
            {
                return _ReferenceDistance;
            }
            set
            {
                al.Sourcef(Source, al.REFERENCE_DISTANCE, value);
                _ReferenceDistance = value;
            }
        }

        /// <summary>
        /// Gets the current state of the sound
        /// </summary>
        public SoundStates State
        {
            get
            {
                int[] Out = new int[1];
                al.GetSourcei(Source, al.SOURCE_STATE, Out);
                return (SoundStates)Out[0];
            }
        }

        
        /// <summary>
        /// The distance that the source will be the quietest (if the listener is
        /// farther, it won't be any quieter than if they were at this distance)
        /// </summary>
        public float MaxRange
        {
            get
            {
                return _MaxRange;
            }
            set
            {
                al.Sourcef(Source, al.MAX_DISTANCE, value);
                _MaxRange = value;
            }
        }

        
        /// <summary>
        /// If true, the sound will be paused
        /// </summary>
        public bool Paused
        {
            get
            {
                return _Paused;
            }
            set
            {
                if (value)
                    al.SourcePause(Source);
                if (!value && _Paused)
                    al.SourcePlay(Source);
                _Paused = value;
            }
        }


        /// <summary>
        /// The sound buffer for this sound source
        /// </summary>
        public SoundBuffer Buffer
        {
            get
            {
                return _Buffer;
            }
            set
            {
                _Buffer = value;
                al.Sourcei(Source, al.BUFFER, value.GetBufferID());
            }
        }

        /// <summary>
        /// How quiet it is at max distance
        /// </summary>
        public float MinGain
        {
            get
            {
                float[] Result = new float[1];
                al.GetSourcef(Source, al.MIN_GAIN, Result);
                return Result[0];
            }
            set
            {
                al.Sourcef(Source, al.MIN_GAIN, value);
            }
        }

        /// <summary>
        /// How loud it is at closest distance
        /// </summary>
        public float MaxGain
        {
            get
            {
                float[] Result = new float[1];
                al.GetSourcef(Source, al.MAX_GAIN, Result);
                return Result[0];
            }
            set
            {
                al.Sourcef(Source, al.MAX_GAIN, value);
            }
        }

        /// <summary>
        /// rolloffFactor is used for distance attenuation calculations based on inverse distance with rolloff. For distances smaller than maxDistance (and, depending on the distance model, larger than referenceDistance), this will scale the distance attenuation over the applicable range. See Sound.OpenAL.AL.Attenuation for details how the attenuation is computed as a function of the distance. The initial value is 1.
        /// </summary>
        public float RolloffFactor
        {
            get
            {
                float[] Result = new float[1];
                al.GetSourcef(Source, al.ROLLOFF_FACTOR, Result);
                return Result[0];
            }
            set
            {
                al.Sourcef(Source, al.ROLLOFF_FACTOR, value);
            }
        }

        #endregion

        #region Constructors
		/// <summary>
		/// Initializes a new instance of the <see cref="SoundSource"/> class.
		/// </summary>
		/// <param name="Buffer">The buffer to get sound data from.</param>
        public SoundSource(SoundBuffer Buffer)
        {
            //Generate the sound source
            int[] Buf = new int[1];

            al.GenSources(1, Buf);
            Source = Buf[0];

            this.Buffer = Buffer;

            Pitch = 1f;
            Gain = 1f;
            Position = Vector3D.Zero;
            Velocity = Vector3D.Zero;
            Loop = false;
            ReferenceDistance = 10f;
            MaxRange = 3000f;
        }

        #endregion

        #region Methods

        /// <summary>
        /// Play the sound
        /// </summary>
        public void Play()
        {
            al.SourcePlay(Source);
        }

        /// <summary>
        /// Pause the sound
        /// </summary>
        public void Pause()
        {
            Paused = true;
        }

        /// <summary>
        /// Stop the sound
        /// </summary>
        public void Stop()
        {
            al.SourceStop(Source);
        }

        #endregion
        
        #region Destructors

        /// <summary>
        /// Finalization
        /// </summary>
        ~SoundSource()
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

                al.DeleteSource(Source);
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
