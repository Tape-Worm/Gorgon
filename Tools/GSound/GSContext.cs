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
using System.Reflection;
using System.IO;

namespace GorgonLibrary.Sound
{
    /// <summary>
    /// Distance model that a context can use
    /// </summary>
    public enum DistanceModel : int
    {
        /// <summary>
        /// Inverse Distance, unclamped.
        /// </summary>
        INVERSE_DISTANCE = 0xD001,
        /// <summary>
        /// Inverse distance, clamped.
        /// </summary>
        INVERSE_DISTANCE_CLAMPED = 0xD002
    }

    /// <summary>
    /// This is the sound context. Set listener information in here
    /// </summary>
    public class GSContext : object, IDisposable
    {
        #region Variables

        /// <summary>
        /// Pointer to the device
        /// </summary>
        private IntPtr FDevice = (IntPtr)0;
        /// <summary>
        /// Pointer to the OAL context
        /// </summary>
        private IntPtr FContext = (IntPtr)0;

        /// <summary>
        /// Is Disposed yet?
        /// </summary>
        private bool _disposed = false;

        /// <summary>
        /// The direction that is up.
        /// </summary>
        public Vector3D Up = new Vector3D(0f, 1f, 0f);

        /// <summary>
        /// How OpenAL behaves at different distances
        /// </summary>
        public DistanceModel DistanceModel
        {
            get
            {
                return (DistanceModel)al.GetInteger(al.DISTANCE_MODEL);
            }
            set
            {
                al.DistanceModel((int)value);
            }
        }

        #endregion

        #region Properties

        /// <summary>
        /// A pointer to OpenAL device
        /// </summary>
        private IntPtr Device
        {
            get
            {
                return FDevice;
            }
        }

        /// <summary>
        /// A pointer to the OpenAL context
        /// </summary>
        private IntPtr Context
        {
            get
            {
                return FContext;
            }
        }

        #endregion

        #region Constructors

        /// <summary>
        /// Creates the listener context object.
        /// </summary>
        public GSContext()
        {
            //Register Assembly resolve, so we can embed the OGG Dlls
            AppDomain.CurrentDomain.AssemblyResolve += (sender, args) =>
            {
                var resName = args.Name.Split(',')[0];
                var thisAssembly = Assembly.GetExecutingAssembly();
                switch (resName)
                {
                    case "csogg":
                        return Assembly.Load(Properties.Resources.csogg);
                    case "csvorbis":
                        return Assembly.Load(Properties.Resources.csvorbis);
                }

                return null;

            };

            //Open the 
            FDevice = alc.OpenDevice("");

            //create context
            FContext = alc.CreateContext(FDevice, null);
            //enable context
            alc.MakeContextCurrent(FContext);


        }



        #endregion

        #region Methods

        /// <summary>
        /// Set the Position, Velocity, and Direction of the listener
        /// </summary>
        /// <param name="Position">Where the listener is</param>
        /// <param name="Velocity">Velocity specifies the current velocity (speed and direction) of the object, in the world coordinate system. The object Velocity does not affect the source's position. OpenAL does not calculate the velocity from subsequent position updates, nor does it adjust the position over time based on the specified velocity. Any such calculation is left to the application. For the purposes of sound processing, position and velocity are independent parameters affecting different aspects of the sounds.</param>
        /// <param name="Direction">The direction the listener is looking</param>
        public void SetListener(Vector3D Position, Vector3D Velocity, Vector3D Direction)
        {
            al.Listenerfv(al.POSITION, new float[3] { Position.X, Position.Y, Position.Z });
            al.Listenerfv(al.VELOCITY, new float[3] { Velocity.X, Velocity.Y, Velocity.Z });
            al.Listenerfv(al.ORIENTATION, new float[6] { Direction.X, Direction.Y, Direction.Z, Up.X, Up.Y, Up.Z });
        }


        #endregion

        #region Destructors

        /// <summary>
        /// Finalization
        /// </summary>
        ~GSContext()
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

                alc.DestroyContext(FContext);
                alc.CloseDevice(FDevice);

                FContext = (IntPtr)0;
                FDevice = (IntPtr)0;
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
