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
// Created: Wednesday, September 9, 2015 12:37:29 AM
// 
#endregion

using Gorgon.Native;

namespace Gorgon.Input
{
    /// <summary>
    /// Settings to pass to the <see cref="GorgonRawInput"/> interface.
    /// </summary>
    public struct GorgonRawInputSettings
    {
        /// <summary>
        /// Flag to indicate whether to allow the application to receive raw input while in the background.
        /// </summary>
        /// <remarks>
        /// <para>
        /// When set to <b>true</b>, raw input messages will be sent to the application regardless of whether it is in the foreground or not, otherwise it will only receive messages 
        /// while in the foreground.
        /// </para>
        /// <para>
        /// This setting requires that the <see cref="TargetWindow"/> be set, if it is not, then the primary application window will be used.
        /// </para>
        /// </remarks>
        public bool AllowBackground;

        /// <summary>
        /// The target window handle for raw input messages.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is used to target Raw Input messages to a specific window in the application. If this value is left at <b>null</b>, then the messages will be sent to which ever 
        /// window has keyboard focus in the application.
        /// </para>
        /// <para>
        /// If the <see cref="AllowBackground"/> is set to <b>true</b>, then this value must be set.
        /// </para>
        /// </remarks>
        public GorgonReadOnlyPointer TargetWindow;
    }
}
