// Gorgon
// Copyright (C) 2024 Michael Winsor
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
// Created: March 14, 2024 9:32:51 AM
//

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;
using System.Text;

namespace Gorgon.Editor.Native;

/// <summary>
/// A marshaller to convert a string to a native doubly null terminated unicode string
/// </summary>
[CustomMarshaller(typeof(string), MarshalMode.Default, typeof(PczzWStrMarshaller))]
[CustomMarshaller(typeof(string), MarshalMode.ManagedToUnmanagedIn, typeof(ManagedToUnmanagedIn))]
internal unsafe static class PczzWStrMarshaller
{
    /// <summary>
    /// Function to convert a managed string to an unmanaged version.
    /// </summary>
    /// <param name="managed">The managed string to convert.</param>
    /// <returns>An unmanaged string.</returns>
    public static byte* ConvertToUnmanaged(string managed)
    {
        if (managed is null)
        {
            return null;
        }

        int exactByteCount = checked(Encoding.UTF8.GetByteCount(managed) + 2); // + 2 for null terminator
        byte* mem = (byte*)Marshal.AllocCoTaskMem(exactByteCount);
        Span<byte> buffer = new(mem, exactByteCount);

        int byteCount = Encoding.UTF8.GetBytes(managed, buffer);
        buffer[byteCount] = 0; // null-terminate
        buffer[byteCount + 1] = 0; // null-terminate
        return mem;
    }

    /// <summary>
    /// Converts an unmanaged string to a managed version.
    /// </summary>
    /// <param name="unmanaged">The unmanaged string to convert.</param>
    /// <returns>A managed string.</returns>
    public static string ConvertToManaged(byte* unmanaged)
        => Marshal.PtrToStringUTF8((IntPtr)unmanaged);

    /// <summary>
    /// Free the memory for a specified unmanaged string.
    /// </summary>
    /// <param name="unmanaged">The memory allocated for the unmanaged string.</param>
    public static void Free(byte* unmanaged)
        => Marshal.FreeCoTaskMem((IntPtr)unmanaged);

    /// <summary>
    /// Custom marshaller to marshal a managed string as an unmanaged string.
    /// </summary>
    public ref struct ManagedToUnmanagedIn
    {
        private byte* _unmanagedValue;
        private bool _allocated;

        /// <summary>
        /// Property to return the requested size of the buffer, in bytes.
        /// </summary>
        public static int BufferSize => 256;

        /// <summary>
        /// Initializes the marshaller with a managed string and requested buffer.
        /// </summary>
        /// <param name="managed">The managed string with which to initialize the marshaller.</param>
        /// <param name="buffer">The request buffer whose size is at least <see cref="BufferSize"/>.</param>
        public void FromManaged(string managed, Span<byte> buffer)
        {
            _allocated = false;

            if (managed is null)
            {
                _unmanagedValue = null;
                return;
            }

            // 3 bytes per character.
            if ((long)3 * managed.Length >= buffer.Length)
            {
                // Calculate accurate byte count when the provided stack-allocated buffer is not sufficient
                int exactByteCount = checked(Encoding.UTF8.GetByteCount(managed) + 2); // + 2 for null terminator

                if (exactByteCount > buffer.Length)
                {
                    buffer = new Span<byte>((byte*)NativeMemory.Alloc((nuint)exactByteCount), exactByteCount);
                    _allocated = true;
                }
            }

            _unmanagedValue = (byte*)Unsafe.AsPointer(ref MemoryMarshal.GetReference(buffer));

            int byteCount = Encoding.UTF8.GetBytes(managed, buffer);
            buffer[byteCount] = 0; // null-terminate
            buffer[byteCount + 1] = 0; // null-terminate
        }

        /// <summary>
        /// Converts the current managed string to an unmanaged string.
        /// </summary>
        /// <returns>An unmanaged string.</returns>
        public readonly byte* ToUnmanaged() => _unmanagedValue;

        /// <summary>
        /// Frees any allocated unmanaged memory.
        /// </summary>
        public readonly void Free()
        {
            if (_allocated)
            {
                NativeMemory.Free(_unmanagedValue);
            }
        }
    }
}
