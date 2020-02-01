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
// Created: August 31, 2018 1:00:47 AM
// 
#endregion

using System;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Math;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// An implementation of <see cref="IBusyStateService"/> that shows a wait cursor when an application main thread is busy.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This implementation is reference counted. That is, a call to <see cref="SetBusy"/> increments an internal counter.
    /// </para>
    /// </remarks>
    public class WaitCursorBusyState
        : IBusyStateService
    {
        #region Variables.
        // Synchronization lock for the event.
        private readonly object _eventLock = new object();
        // The number of times the SetBusy method has been called.
        private int _busyStateCounter;
        #endregion

        #region Events.
        // Event triggered when the busy state changes.
        private event EventHandler _busyStateChanged;

        /// <summary>
        /// Event triggered when the busy state changes.
        /// </summary>
        public event EventHandler BusyStateChanged
        {
            add
            {
                lock (_eventLock)
                {
                    if (value == null)
                    {
                        _busyStateChanged = null;
                        return;
                    }

                    _busyStateChanged += value;
                }
            }
            remove
            {
                lock (_eventLock)
                {
                    if (value == null)
                    {
                        return;
                    }

                    _busyStateChanged -= value;
                }
            }
        }
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether or not there's a busy state.
        /// </summary>
        public bool IsBusy
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to trigger the busy state change event.
        /// </summary>
        private void OnBusyStateChanged()
        {
            EventHandler handler = null;

            lock (_eventLock)
            {
                handler = _busyStateChanged;
            }

            handler?.Invoke(this, EventArgs.Empty);
        }

        /// <summary>
        /// Function to forcefully reset the busy state back to an idle state.
        /// </summary>
        public void Reset()
        {
            if (Interlocked.Exchange(ref _busyStateCounter, 0) == 0)
            {
                return;
            }

            Cursor.Current = Cursors.Default;
            IsBusy = false;

            OnBusyStateChanged();
        }

        /// <summary>
        /// Function to set busy state.
        /// </summary>
        public void SetBusy()
        {
            if (Interlocked.Increment(ref _busyStateCounter) > 1)
            {
                return;
            }

            Cursor.Current = Cursors.WaitCursor;
            IsBusy = true;

            OnBusyStateChanged();
        }

        /// <summary>
        /// Function to hide the busy state.
        /// </summary>
        public void SetIdle()
        {
            if (Interlocked.Decrement(ref _busyStateCounter) > 0)
            {
                return;
            }

            // Ensure that we don't get a negative count.
            _busyStateCounter = _busyStateCounter.Max(0);

            Cursor.Current = Cursors.Default;
            IsBusy = false;

            OnBusyStateChanged();
        }
        #endregion
    }
}
