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
// Created: Thursday, September 10, 2015 11:17:57 PM
// 
#endregion

using System;
using Gorgon.Native;

namespace Gorgon.Input
{
    /// <summary>
    /// Event arguments for an <see cref="GorgonRawHID.DataReceived"/> event.
    /// </summary>
    public class GorgonHIDEventArgs
        : EventArgs
    {
        #region Properties.
        /// <summary>
        /// Property to return the data buffer storing the HID data.
        /// </summary>
        public GorgonPtr<byte> Data
        {
            get;
        }

        /// <summary>
        /// Property to return the size of an individual HID input in the <see cref="Data"/>, in bytes.
        /// </summary>
        public int HIDSize
        {
            get;
        }

        /// <summary>
        /// Property to return the number of HID inputs within the <see cref="Data"/>
        /// </summary>
        public int Count
        {
            get;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonHIDEventArgs"/> class.
        /// </summary>
        /// <param name="data">The HID data.</param>
        /// <param name="size">The size of an individual HID input.</param>
        /// <param name="count">The number of HID inputs.</param>
        public GorgonHIDEventArgs(GorgonPtr<byte> data, int size, int count)
        {
            Data = data;
            HIDSize = size;
            Count = count;
        }
        #endregion
    }
}
