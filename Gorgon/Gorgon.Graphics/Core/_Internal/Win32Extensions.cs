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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: December 28, 2025 9:26:52 PM
//

using System.Runtime.CompilerServices;
using Gorgon.Core;
using TerraFX.Interop.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Extension methods for Win32 data structures and values.
/// </summary>
internal static class Win32Extensions
{
    extension(GorgonRectangle rect)
    {
        /// <summary>
        /// Function to covnert a <see cref="GorgonViewport"/> to a D3D12_VIEWPORT type.
        /// </summary>
        /// <returns>The D3D12 viewport.</returns>
        public RECT ToWin32Rect() => new(rect.Left, rect.Top, rect.Right, rect.Bottom);
    }

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
        /// <param name="resultCode">The Gorgon exception result code.</param>
        /// <param name="message">The message to pass in.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ThrowIfFailed(GorgonResult resultCode, string message)
        {
            if (hr.FAILED)
            {
                throw new GorgonException(resultCode, $"{message} Error code: 0x{hr.Value.FormatHex()}.");
            }
        }

        /// <summary>
        /// Function to throw a <see cref="GorgonException"/> if the HRESULT passed in is a failure.
        /// </summary>
        /// <typeparam name="TArg">The type of the argument to format into the message.</typeparam>
        /// <param name="resultCode">The Gorgon exception result code.</param>
        /// <param name="format">The composite format string for the message.</param>
        /// <param name="arg">The argument to format into the message.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ThrowIfFailed<TArg>(GorgonResult resultCode, string format, TArg arg)
        {
            if (hr.FAILED)
            {
                throw new GorgonException(resultCode, $"{string.Format(format, arg)} Error code: 0x{hr.Value.FormatHex()}.");
            }
        }

        /// <summary>
        /// Function to throw a <see cref="GorgonException"/> if the HRESULT passed in is a failure.
        /// </summary>
        /// <typeparam name="TArg0">The type of the first argument to format into the message.</typeparam>
        /// <typeparam name="TArg1">The type of the second argument to format into the message.</typeparam>
        /// <param name="resultCode">The Gorgon exception result code.</param>
        /// <param name="format">The composite format string for the message.</param>
        /// <param name="arg0">The first argument to format into the message.</param>
        /// <param name="arg1">The second argument to format into the message.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ThrowIfFailed<TArg0, TArg1>(GorgonResult resultCode, string format, TArg0 arg0, TArg1 arg1)
        {
            if (hr.FAILED)
            {
                throw new GorgonException(resultCode, $"{string.Format(format, arg0, arg1)} Error code: 0x{hr.Value.FormatHex()}.");
            }
        }
    }
}
