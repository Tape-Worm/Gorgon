// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Wednesday, August 12, 2015 11:29:45 PM
// 

using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Input.Devices;
using Gorgon.Input.Properties;
using Microsoft.Win32.SafeHandles;
using Windows.Win32.Foundation;
using Windows.Win32.Storage.FileSystem;
using Windows.Win32.UI.WindowsAndMessaging;

namespace Windows.Win32;

/// <summary>
/// Win32 native code.
/// </summary>
internal static partial class PInvoke
{
    /// <summary>
    /// Function to determine if the mouse cursor is visible or not.
    /// </summary>
    /// <returns>A <see cref="CURSORINFO_FLAGS"/> value indicating the state of the mouse cursor.</returns>
    /// <exception cref="Win32Exception">Thrown if the call failed.</exception>
    public static CURSORINFO_FLAGS IsCursorVisible()
    {
        CURSORINFO cursorInfo = new()
        {
            cbSize = (uint)Unsafe.SizeOf<CURSORINFO>(),
            flags = CURSORINFO_FLAGS.CURSOR_SHOWING,
            hCursor = HCURSOR.Null
        };

        if (GetCursorInfo(ref cursorInfo))
        {
            return cursorInfo.flags;
        }

        int win32Error = Marshal.GetLastWin32Error();
        throw new Win32Exception(string.Format(Resources.GORINP_ERR_WIN32_CURSOR_INFO, win32Error));
    }

    /// <summary>
    /// Function to convert a virtual key code to a Unicode character.
    /// </summary>
    /// <param name="key">The key to convert.</param>
    /// <param name="states">The current state of the keyboard.</param>
    /// <param name="buffer">The buffer that will receive the unicode character.</param>
    /// <returns>The character as a string, or an empty string if no character is applicable.</returns>
    public unsafe static string ToUnicodeChar(VirtualKeys key, ReadOnlySpan<byte> states, Span<char> buffer)
    {
        int result = ToUnicode((uint)key, 0, states, buffer, 0);

        return result switch
        {
            1 => new string(buffer),
            _ => string.Empty
        };
    }

    /// <summary>
    /// Function to wait until one or all of the specified objects are in the signaled state or the time-out interval elapses. The objects can include input event objects, which you specify using the dwWakeMask parameter.
    /// </summary>
    /// <param name="handles">An array of object handles. For a list of the object types whose handles can be specified, see the following Remarks section. The array can contain handles of objects of different types. It may not contain multiple copies of the same handle. If one of these handles is closed while the wait is still pending, the function's behavior is undefined. The handles must have the SYNCHRONIZE access right.For more information, see Standard Access Rights. This value is allowed to be empty.</param>
    /// <param name="flags">The time-out interval, in milliseconds. If a nonzero value is specified, the function waits until the specified objects are signaled or the interval elapses. If dwMilliseconds is zero, the function does not enter a wait state if the specified objects are not signaled; it always returns immediately. If dwMilliseconds is <see cref="INFINITE"/>, the function will return only when the specified objects are signaled. Windows XP, Windows Server 2003, Windows Vista, Windows 7, Windows Server 2008, and Windows Server 2008 R2: The dwMilliseconds value does include time spent in low-power states. For example, the timeout does keep counting down while the computer is asleep. Windows 8 and newer, Windows Server 2012 and newer: The dwMilliseconds value does not include time spent in low-power states. For example, the timeout does not keep counting down while the computer is asleep.</param>
    /// <param name="cancelToken">The input types for which an input event object handle will be added to the array of object handles. This parameter can be any combination of the values listed in <see cref="GetQueueStatus"/> flags parameter.</param>
    /// <returns>If the function succeeds, the return value indicates the event index that caused the function to return. It can be one of <see cref="WAIT_EVENT.WAIT_OBJECT_0"/> (or <c>(int)WAIT_EVENT.WAIT_OBJECT_0 + 1, (int)WAIT_EVENT.WAIT_OBJECT_0 + 2, etc...</c>) for each one of the handles, or <see cref="WAIT_EVENT.WAIT_ABANDONED_0"/> (or <c>(int)WAIT_EVENT.WAIT_ABANDONED_0 + 1, (int)WAIT_EVENT.WAIT_ABANDONED_0 + 2, etc...)</c>. If a cancellation token is specified, then that wait handle is the last handle in the handle list and will be equal to <see cref="WAIT_EVENT.WAIT_OBJECT_0"/> + <paramref name="handles"/> length.</returns>
    /// <exception cref="Win32Exception">Thrown if the call failed. The exception will contain the last win32 error.</exception>
    /// <exception cref="OperationCanceledException">Thrown if the cancel token is singaled to cancel the operation..</exception>
    public static int MessageWaitForMultipleObjects(ReadOnlySpan<SafeHandle> handles, QUEUE_STATUS_FLAGS flags, CancellationToken cancelToken)
    {
        SafeWaitHandle? cancelHandle = cancelToken != CancellationToken.None ? cancelToken.WaitHandle.GetSafeWaitHandle() : null;
        Span<HANDLE> handlePtrs = cancelHandle is not null ? stackalloc HANDLE[handles.Length + 1] : (!handles.IsEmpty ? stackalloc HANDLE[handles.Length] : []);

        if (cancelHandle is not null)
        {
            bool success = false;
            cancelHandle.DangerousAddRef(ref success);

            if (!success)
            {
                throw new Win32Exception(Resources.GORINP_ERR_WIN32_CANNOT_GET_WAIT_HANDLE);
            }

            handlePtrs[handles.Length] = new HANDLE(cancelHandle.DangerousGetHandle());
        }

        try
        {
            if (handles.Length > 0)
            {
                for (int i = 0; i < handles.Length; ++i)
                {
                    handlePtrs[i] = new HANDLE(handles[i].DangerousGetHandle());
                }
            }

            WAIT_EVENT result = MsgWaitForMultipleObjectsEx(handlePtrs, INFINITE, flags, MSG_WAIT_FOR_MULTIPLE_OBJECTS_EX_FLAGS.MWMO_NONE);

            if (result == WAIT_EVENT.WAIT_FAILED)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            int value = (int)result;
            int wait0 = (int)WAIT_EVENT.WAIT_OBJECT_0;

            if ((value == wait0 + handles.Length) && (cancelHandle is not null))
            {
                cancelToken.ThrowIfCancellationRequested();
                return handlePtrs.Length - 1;
            }

            return value;
        }
        finally
        {
            cancelHandle?.DangerousRelease();
        }
    }

    /// <summary>
    /// Function to create a file handle.
    /// </summary>
    /// <param name="fileName">The path to the file, or device name.</param>
    /// <param name="desiredAccess">The desired access level to the handle.</param>
    /// <param name="shareMode">The allowed sharing modes.</param>
    /// <param name="creationDisposition">An action to take on a file or device that exists or does not exist.</param>
    /// <param name="flagsAndAttributes">The file or device attributes and flags, <see cref="FILE_FLAGS_AND_ATTRIBUTES.FILE_ATTRIBUTE_NORMAL"/> being the most common default value for files.</param>
    /// <returns>If the function succeeds, the return value is an open handle to the specified file, device, named pipe, or mail slot.</returns>
    /// <exception cref="Win32Exception">Thrown if the file handle could not be opened.</exception>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static HANDLE CreateFile(string fileName, GENERIC_ACCESS_RIGHTS desiredAccess, FILE_SHARE_MODE shareMode, FILE_CREATION_DISPOSITION creationDisposition, FILE_FLAGS_AND_ATTRIBUTES flagsAndAttributes)
    {
        HANDLE result = CreateFile(fileName, (uint)desiredAccess, shareMode, null, creationDisposition, flagsAndAttributes, HANDLE.Null);

        if (result == HANDLE.INVALID_HANDLE_VALUE)
        {
            int err = Marshal.GetLastWin32Error();
            throw new Win32Exception(err);
        }

        return result;
    }
}