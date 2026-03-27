// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: December 28, 2025 9:26:52 PM
//

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Text;
using Gorgon.Core;
using TerraFX.Interop.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Extension methods for Win32 data structures and values.
/// </summary>
internal static class Win32Extensions
{
    extension<T>(ComPtr<T> ptr) where T : unmanaged, IUnknown.Interface
    {
        /// <summary>
        /// Property to return whether the COM pointer is <b>null</b> or not.
        /// </summary>
        public unsafe bool IsNull => ptr.Get() is null;
    }

    extension(HRESULT hr)
    {
        /// <summary>
        /// Function to throw a <see cref="GorgonException"/> if the HRESULT passed in is a failure.
        /// </summary>
        /// <param name="exception">A callback method that returns the exception to throw.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ThrowIfFailed(Func<HRESULT, Exception> exception)
        {
            if (hr.FAILED)
            {
                throw exception(hr);
            }
        }

        /// <summary>
        /// Function to throw a <see cref="GorgonException"/> if the HRESULT passed in is a failure.
        /// </summary>
        /// <param name="resultCode">The Gorgon exception result code.</param>
        /// <param name="message">The message to pass in.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ThrowIfFailed(GorgonResult resultCode, Func<string> message)
        {
            if (hr.FAILED)
            {
                throw new GorgonException(resultCode, $"{message()} Error code: 0x{hr.Value.FormatHex()}.");
            }
        }
    }
}
