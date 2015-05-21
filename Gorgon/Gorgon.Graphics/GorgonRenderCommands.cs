#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Monday, July 08, 2013 9:33:52 PM
// 
#endregion

using System;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
    /// <summary>
    /// An object to encapsulate the rendering commands from a deferred context.
    /// </summary>
    public class GorgonRenderCommands
        : IDisposable
    {
        #region Variables.
        private bool _disposed;                         // Flag to indicate that the object was disposed.
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the D3D command list object.
        /// </summary>
        internal D3D.CommandList D3DCommands
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the graphics context that owns this command list.
        /// </summary>
        public GorgonGraphics Graphics
        {
            get;
            private set;
        }
        #endregion

        #region Methods.

        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonRenderCommands"/> class.
        /// </summary>
        /// <param name="graphics">The graphics context.</param>
        internal GorgonRenderCommands(GorgonGraphics graphics)
        {
            Graphics = graphics;
            
            D3DCommands = graphics.Context.FinishCommandList(true);
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            if (disposing)
            {
                Graphics.ReleaseCommands(this);

                if (D3DCommands != null)
                {
                    D3DCommands.Dispose();
                }
                D3DCommands = null;
            }

            _disposed = true;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);   
        }
        #endregion
    }
}