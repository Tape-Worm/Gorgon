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
// Created: July 12, 2025 12:00:09 PM
//
 
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;

namespace Gorgon.Native;

/// <summary>
/// Indices used for <see cref="User32Api.GetWindowAttribute"/> and <see cref="User32Api.SetWindowAttribute"/>.
/// </summary>
internal enum GwlIndex
{
    /// <summary>
    /// Get/set extended window style.
    /// </summary>
    GwlExStyle = -20,
    /// <summary>
    /// Get/set the application instance handle.
    /// </summary>
    GwlHinstance = -6,
    /// <summary>
    /// Get/set a new owner for a top level window.
    /// </summary>
    GwlHwndParent = -8,
    /// <summary>
    /// Get/set a new identifier of the child window. The window cannot be a top-level window. 
    /// </summary>
    GwlID = -12,
    /// <summary>
    /// Get/set a new window style. 
    /// </summary>
    GwlStyle = -16,
    /// <summary>
    /// Get/set the user data associated with the window. This data is intended for use by the application that created the window. Its value is initially zero. 
    /// </summary>
    GwlUserData = -21,
    /// <summary>
    /// Get/set a new address for the window procedure. 
    /// </summary>
    GwlWndProc = -4
}

/// <summary>
/// User 32 specific functionality.
/// </summary>
internal static partial class User32Api
{
    /// <summary>
    /// Changes an attribute of the specified window. The function also sets a value at the specified offset in the extra window memory. 
    /// </summary>
    /// <param name="hwnd">Window handle.</param>
    /// <param name="index">The index of the window attribute.</param>
    /// <param name="newLong">The new value.</param>
    /// <returns>The previous value.</returns>
    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true), SupportedOSPlatform("windows5.1.2600")]
    private static partial nint SetWindowLongPtrW(nint hwnd, GwlIndex index, nint newLong);

    /// <summary>
    /// Retrieves information about the specified window. The function also retrieves the value at a specified offset into the extra window memory.
    /// </summary>
    /// <param name="hwnd">Window handle.</param>
    /// <param name="index">The index of the window attribute.</param>
    /// <returns>The current value.</returns>
    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true), SupportedOSPlatform("windows5.1.2600")]
    private static partial nint GetWindowLongPtrW(nint hwnd, GwlIndex index);

    /// <summary>
    /// Changes an attribute of the specified window. The function also sets a value at the specified offset in the extra window memory. 
    /// </summary>
    /// <param name="hwnd">Window handle.</param>
    /// <param name="index">The index of the window attribute.</param>
    /// <param name="newLong">The new value.</param>
    /// <returns>The previous value.</returns>
    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true), SupportedOSPlatform("windows5.1.2600")]
    private static partial int SetWindowLongW(nint hwnd, GwlIndex index, int newLong);

    /// <summary>
    /// Retrieves information about the specified window. The function also retrieves the value at a specified offset into the extra window memory.
    /// </summary>
    /// <param name="hwnd">Window handle.</param>
    /// <param name="index">The index of the window attribute.</param>
    /// <returns>The current value.</returns>
    [LibraryImport("user32.dll", StringMarshalling = StringMarshalling.Utf16, SetLastError = true), SupportedOSPlatform("windows5.1.2600")]
    private static partial int GetWindowLongW(nint hwnd, GwlIndex index);

    /// <summary>
    /// Function to set a new window attribute to a window.
    /// </summary>
    /// <param name="hwnd">The handle to the window to update.</param>
    /// <param name="attribute">The attribute to modify.</param>
    /// <param name="newAttribute">The new value to assign to the attribute.</param>
    /// <returns>The previously assigned attribute.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint SetWindowAttribute(nint hwnd, GwlIndex attribute, nint newAttribute)
    {
        if (Environment.Is64BitProcess)
        {
            return SetWindowLongPtrW(hwnd, attribute, newAttribute);
        }

        return SetWindowLongW(hwnd, attribute, newAttribute.ToInt32());
    }

    /// <summary>
    /// Function to retrieve a window attribute.
    /// </summary>
    /// <param name="hwnd">The handle to the window to read the attribute from.</param>
    /// <param name="attribute">The attribute to read.</param>
    /// <returns>The currently assigned attribute value.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static nint GetWindowAttribute(nint hwnd, GwlIndex attribute)
    {
        if (Environment.Is64BitProcess)
        {
            return GetWindowLongPtrW(hwnd, attribute);
        }

        return GetWindowLongW(hwnd, attribute);
    }
}
