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
// Created: July 12, 2025 7:43:02 PM
//

namespace Gorgon.UI.Win32;

/// <summary>
/// A result value for the <see cref="GorgonWindowProcedure"/> delegate.
/// </summary>
/// <param name="handled"><b>true</b> if the user handled the message, <b>false</b> if not.</param>
/// <seealso cref="GorgonWindowProcedure"/>
/// <remarks>
/// <para>
/// When applications process a <see cref="GorgonWindowMessages">window message</see>, this value should be returned to notify the system that the message has been handled by passing <b>true</b> to the 
/// constructor, or <b>false</b> if the message should be further processed.
/// </para>
/// <para>
/// If a window message requires a specific value returned, users can set the <see cref="Result"/> value. Users should always check the Microsoft documentation for the window message being processed to 
/// determine if the message needs a specific value. 
/// </para>
/// <para>
/// <note type="warning">
/// <para>
/// The <see cref="Result"/> value is only used when the <see cref="Handled"/> is <b>true</b>.
/// </para>
/// </note>
/// </para>
/// </remarks>
/// <seealso cref="GorgonWindowProcedure"/>
/// <seealso cref="GorgonWindowMessages"/>
public ref struct GorgonWindowMessageResult(bool handled)
{
    /// <summary>
    /// <b>true</b> if the user has handled the message, or <b>false</b> if the system can continue procesing the message.
    /// </summary>
    /// <remarks>
    /// If this value is <b>true</b>, then the <see cref="Result"/> value will be passed back to the window procedure return value.
    /// </remarks>
    public readonly bool Handled = handled;
    /// <summary>
    /// A result value to return, if needed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value is only used if <see cref="Handled"/> is set to <b>true</b>.
    /// </para>
    /// <para>
    /// Users should check with the official Microsoft documentation to determine which value should be returned for the message. See the <see cref="GorgonWindowProcedure"/> deletgate type for futher 
    /// information.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonWindowProcedure"/>
    public nint Result = IntPtr.Zero;
}
