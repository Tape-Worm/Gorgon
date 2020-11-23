#region MIT
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Wednesday, September 16, 2015 8:48:16 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Gorgon.Native;

namespace Gorgon.Input
{
    /// <summary>
    /// A message filter hook to allow for intercepting a message to a window.
    /// </summary>
    /// <remarks>
    /// This was adapted from the source code for SharpDX by Alexandre Mutel at http://sharpdx.org
    /// </remarks>
    internal class MessageFilterHook
    {
        #region Delegates.
        /// <summary>
        /// Delegate for the callback into the window procedure.
        /// </summary>
        /// <param name="hwnd">Window handle receiving the message.</param>
        /// <param name="msg">Message received.</param>
        /// <param name="wParam">Window parameter 1.</param>
        /// <param name="lParam">Window parameter 2.</param>
        /// <returns>The result of processing the message.</returns>
        private delegate IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam);
        #endregion

        #region Variables.
        // A list of message hooks registered to varying windows.
        private static readonly Dictionary<IntPtr, MessageFilterHook> _registeredHooks = new Dictionary<IntPtr, MessageFilterHook>(new GorgonIntPtrEqualityComparer());
        // Default window procedure.
        private IntPtr _defaultWndProc;
        // Window to hook.
        private readonly IntPtr _hwnd;

        // New window procedure.
        private IntPtr _newWndProcPtr;
        // The new window procedure method.
        private WndProc _newWndProc;
        // The list of message filters.
        private List<IMessageFilter> _messageFilters = new List<IMessageFilter>();
        // Flag to indicate whether the hook is installed.
        private bool _hooked;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to process the messages through the installed message filters.
        /// </summary>
        /// <param name="hwnd">Window handle receiving the message.</param>
        /// <param name="msg">Message being received.</param>
        /// <param name="wParam">Window parameter 1.</param>
        /// <param name="lParam">Window parameter 2.</param>
        /// <returns>The result of a call to the previous window procedure if the message was not processed, or <see cref="IntPtr.Zero"/> if the message was processed.</returns>
        private IntPtr NewWndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam)
        {
            if (!_hooked)
            {
                UninstallWindowProcedure();
                return UserApi.CallWindowProc(_defaultWndProc, hwnd, msg, wParam, lParam);
            }

            var windowMessage = new Message
            {
                HWnd = hwnd,
                Msg = msg,
                WParam = wParam,
                LParam = lParam
            };

            // Send a copy of the message to any installed filters.
            // ReSharper disable once LoopCanBeConvertedToQuery
            // ReSharper disable once ForCanBeConvertedToForeach
            for (int i = 0; i < _messageFilters.Count; i++)
            {
                // ReSharper disable once InconsistentlySynchronizedField
                IMessageFilter filter = _messageFilters[i];

                if (filter.PreFilterMessage(ref windowMessage))
                {
                    return windowMessage.Result;
                }
            }

            return UserApi.CallWindowProc(_defaultWndProc, hwnd, msg, wParam, lParam);
        }

        /// <summary>
        /// Function to uninstall the window procedure hook.
        /// </summary>
        private void UninstallWindowProcedure()
        {
            _hooked = false;

            IntPtr currentProc = UserApi.GetWindowLong(new HandleRef(this, _hwnd), UserApi.WindowLongWndProc);

            if (currentProc == _newWndProcPtr)
            {
                // Only set this if the current window procedure is the one owned by this hook.
                UserApi.SetWindowLong(new HandleRef(this, _hwnd), UserApi.WindowLongWndProc, _defaultWndProc);
            }
        }

        /// <summary>
        /// Function to install the window procedure hook.
        /// </summary>
        private void InstallWindowProcedure()
        {
            // Get the previous window procedure associated with this handle.
            _defaultWndProc = UserApi.GetWindowLong(new HandleRef(this, _hwnd), UserApi.WindowLongWndProc);

            // Create the new window proc. Hold the reference to the delegate so it doesn't get reclaimed by the GC 
            // while we're in native land.
            _newWndProc = NewWndProc;
            _newWndProcPtr = Marshal.GetFunctionPointerForDelegate(_newWndProc);

            // Install our window hook.
            UserApi.SetWindowLong(new HandleRef(this, _hwnd), UserApi.WindowLongWndProc, _newWndProcPtr);

            _hooked = true;
        }

        /// <summary>
        /// Function to add a filter to the window procedure hook.
        /// </summary>
        /// <param name="filter">The filter to add to the hook.</param>
        private void AddFilter(IMessageFilter filter)
        {
            var filters = new List<IMessageFilter>(_messageFilters);

            if (!_messageFilters.Contains(filter))
            {
                filters.Add(filter);
            }

            _messageFilters = filters;
        }

        /// <summary>
        /// Function to remove a filter from the window procedure hook.
        /// </summary>
        /// <param name="filter">The filter to remove from the hook.</param>
        private void RemoveFilter(IMessageFilter filter)
        {
            var filters = new List<IMessageFilter>(_messageFilters);

            if (!_messageFilters.Contains(filter))
            {
                return;
            }

            filters.Remove(filter);

            if (filters.Count == 0)
            {
                UninstallWindowProcedure();
            }

            _messageFilters = filters;
        }

        /// <summary>
        /// Function to add a new filter to the window hook.
        /// </summary>
        /// <param name="hwnd">Window handle to hook.</param>
        /// <param name="filter">Filter to install into the hook</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="hwnd"/> parameter is <see cref="IntPtr.Zero"/>, or the <paramref name="filter"/> parameter is <b>null</b>.</exception>
        public static void AddFilter(IntPtr hwnd, IMessageFilter filter)
        {
            if (hwnd == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(hwnd));
            }

            if (filter == null)
            {
                throw new ArgumentNullException(nameof(filter));
            }

            lock (_registeredHooks)
            {

                if (!_registeredHooks.TryGetValue(hwnd, out MessageFilterHook hook))
                {
                    hook = new MessageFilterHook(hwnd);
                    _registeredHooks[hwnd] = hook;
                    hook.InstallWindowProcedure();
                }

                hook.AddFilter(filter);
            }
        }

        /// <summary>
        /// Function to add a new filter to the window hook.
        /// </summary>
        /// <param name="hwnd">Window handle to hook.</param>
        /// <param name="filter">Filter to uninstall from the hook</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="hwnd"/> parameter is <see cref="IntPtr.Zero"/>, or the <paramref name="filter"/> parameter is <b>null</b>.</exception>
        public static void RemoveFilter(IntPtr hwnd, IMessageFilter filter)
        {
            if (hwnd == IntPtr.Zero)
            {
                throw new ArgumentNullException(nameof(hwnd));
            }

            lock (_registeredHooks)
            {

                if (!_registeredHooks.TryGetValue(hwnd, out MessageFilterHook hook))
                {
                    return;
                }

                hook.RemoveFilter(filter);

                if (hook._hooked)
                {
                    return;
                }

                _registeredHooks.Remove(hwnd);
                hook.UninstallWindowProcedure();
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="MessageFilterHook"/> class.
        /// </summary>
        /// <param name="hwnd">The window handle to hook.</param>
        private MessageFilterHook(IntPtr hwnd) => _hwnd = hwnd;
        #endregion
    }
}
