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
// Created: July 12, 2025 7:33:51 PM
//

using System.Runtime.CompilerServices;
using Gorgon.Graphics;

namespace Gorgon.UI.Win32;

/// <summary>
/// A data structure containing information for a Windows message.
/// </summary>
/// <param name="window">The window receiving the message.</param>    
/// <param name="message">The messaging being transmitted.</param>
/// <param name="wParam">A parameter for the message.</param>
/// <param name="lParam">Another parameter for the message.</param>
public readonly struct GorgonWindowMessage(GorgonWindow window, int message, nuint wParam, nint lParam)
{
    /// <summary>
    /// The window receiving the message.
    /// </summary>
    public readonly GorgonWindow Window = window;

    /// <summary>
    /// The window message being transmitted.
    /// </summary>
    /// <remarks>
    /// Users can compare this value with the <see cref="GorgonWindowMessage"/> class constants.
    /// </remarks>
    public readonly int Message = message;

    /// <summary>
    /// A parameter for the message.
    /// </summary>
    public readonly nuint WParam = wParam;

    /// <summary>
    /// Another parameter for the message.
    /// </summary>
    public readonly nint LParam = lParam;

    /// <summary>
    /// Function to retrieve a <see cref="GorgonPoint"/> coordinate from the <see cref="LParam"/> value.
    /// </summary>
    /// <returns>The <see cref="GorgonPoint"/> contained within <see cref="LParam"/>.</returns>
    /// <seealso cref="GorgonPoint"/>
    /// <remarks>
    /// <para>
    /// Use this method when handling coordinates when the X and Y coordinates can be negative values (e.g. a monitor placed to the left of the primary monitor).
    /// </para>
    /// </remarks>
    public GorgonPoint GetPointFromLParam()
    {
        int value = LParam.ToInt32();

        return new GorgonPoint((short)(value & 0xffff), (short)((value >> 16) & 0xffff));
    }

    /// <summary>
    /// Function to retrieve a <see cref="GorgonPoint"/> coordinate from the <see cref="WParam"/> value.
    /// </summary>
    /// <returns>The <see cref="GorgonPoint"/> contained within <see cref="WParam"/>.</returns>
    /// <seealso cref="GorgonPoint"/>
    /// <remarks>
    /// <para>
    /// Use this method when handling coordinates when the X and Y coordinates can be negative values (e.g. a monitor placed to the left of the primary monitor).
    /// </para>
    /// </remarks>
    public GorgonPoint GetPointFromWParam()
    {
        uint value = WParam.ToUInt32();

        return new GorgonPoint((short)(value & 0xffff), (short)((value >> 16) & 0xffff));
    }

    /// <summary>
    /// Function to retrieve the high word value from the <see cref="LParam"/> value.
    /// </summary>
    /// <returns>The high word value.</returns>
    public int GetHighWordFromLParam() => (LParam.ToInt32() >> 16) & 0xffff;

    /// <summary>
    /// Function to retrieve the low word value from the <see cref="LParam"/> value.
    /// </summary>
    /// <returns>The low word value.</returns>
    public int GetLowWordFromLParam() => LParam.ToInt32() & 0xffff;

    /// <summary>
    /// Function to retrieve the high word value from the <see cref="WParam"/> value.
    /// </summary>
    /// <returns>The high word value.</returns>
    public uint GetHighWordFromWParam() => (WParam.ToUInt32() >> 16) & 0xffff;

    /// <summary>
    /// Function to retrieve the high word value from the <see cref="WParam"/> value.
    /// </summary>
    /// <returns>The high word value.</returns>
    public uint GetLowWordFromWParam() => WParam.ToUInt32() & 0xffff;

    /// <summary>
    /// Function to retrieve the contents of the data pointed at by <see cref="LParam"/> as the specified type.
    /// </summary>
    /// <typeparam name="T">The type used to interpret the data.</typeparam>
    /// <returns>A read only reference to the data pointed at by <see cref="LParam"/>.</returns>
    /// <exception cref="NullReferenceException">Thrown if the <see cref="LParam"/> parameter is <b>null</b>.</exception>
    public unsafe ref readonly T LParamAs<T>() where T : unmanaged
    {
        if (LParam == IntPtr.Zero)
        {
            throw new NullReferenceException();
        }

        return ref Unsafe.AsRef<T>((void*)LParam);
    }

    /// <summary>
    /// Function to retrieve the contents of the data pointed at by <see cref="WParam"/> as the specified type.
    /// </summary>
    /// <typeparam name="T">The type used to interpret the data.</typeparam>
    /// <returns>A read only reference to the data pointed at by <see cref="WParam"/>.</returns>
    /// <exception cref="NullReferenceException">Thrown if the <see cref="WParam"/> parameter is <b>null</b>.</exception>
    public unsafe ref readonly T WParamAs<T>() where T : unmanaged
    {
        if (WParam == UIntPtr.Zero)
        {
            throw new NullReferenceException();
        }

        return ref Unsafe.AsRef<T>((void*)WParam);
    }
}
