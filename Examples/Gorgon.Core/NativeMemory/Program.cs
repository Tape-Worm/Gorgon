
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
// Created: Tuesday, September 18, 2012 8:00:02 PM
// 

using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Native;

namespace Gorgon.Examples;

/// <summary>
/// Native memory functionality example.
/// </summary>
/// <remarks>
/// Gorgon has several objects and extension methods for handling native memory in a safe manner. These are useful in cases where interop with native functionality is required. 
/// All of the functionality provided is compatible with the built-in Span{T} functionality.
/// </remarks>
internal static class Program
{
    /// <summary>
    /// Some data for our example.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private readonly struct Data
    {
        public readonly int ID;
        public readonly Vector2 Position;
        public readonly Vector2 Scale;
        public readonly float Angle;

        public Data()
        {
            ID = GorgonRandom.RandomInt32(1, 256);
            Position = new Vector2(GorgonRandom.RandomSingle(-1, 1), GorgonRandom.RandomSingle(-1, 1));
            Scale = new Vector2(GorgonRandom.RandomSingle(0.25f, 1));
            Angle = GorgonRandom.RandomSingle(0, 359.9f);
        }
    }

    /// <summary>
    /// Functionality related to the GorgonNativeBuffer type.
    /// </summary>
    private static void GorgonNativeBufferStuff()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Using GorgonNativeBuffer<T>.");
        Console.ResetColor();
        Console.WriteLine("A GorgonNativeBuffer<T> is a buffer type, backed by native memory.");
        Console.WriteLine("Memory allocated with this type can also choose an alignment for best performance.");
        Console.WriteLine("These buffers can be allocated with an arbitrary amount, or can use a pinned object");
        Console.WriteLine("like a .NET array.");
        Console.WriteLine();
        Console.WriteLine("Buffers can also be casted to a GorgonPtr<T> for more flexibility.");
        Console.WriteLine();

        Console.WriteLine("Allocating 64 KB of data on a 4 byte alignment...");
        // Always dispose of the buffer, it contains native memory which is untracked by the GC, so leaving it alive 
        // may leave the memory allocated for longer than we might want.
        using GorgonNativeBuffer<byte> buffer = new(65536, 4);

        Console.WriteLine($"Buffer allocated at 0x{((ulong)buffer.Location):x} with {buffer.SizeInBytes} bytes.");

        Console.WriteLine("Filling buffer with the value 0x7f...");
        buffer.Fill(0x7f);

        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i] != 0x7f)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failure at byte {i}. Expected 0x7F, got 0x{buffer[i]:x}.");
                Console.ResetColor();
                return;
            }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("OK.");
        Console.ResetColor();

        Console.WriteLine("Resizing the buffer to 32 KB.");
        // Resize the buffer to 32KB.
        buffer.Resize(32767, 4, true);
        Console.WriteLine($"Buffer allocated at 0x{((ulong)buffer.Location):x} with {buffer.SizeInBytes} bytes.");

        Console.WriteLine($"Checking the values to see if they're preserved after resize...");
        for (int i = 0; i < buffer.Length; i++)
        {
            if (buffer[i] != 0x7f)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failure at byte {i}. Expected 0x7F, got 0x{buffer[i]:x}.");
                Console.ResetColor();
                return;
            }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("OK.");
        Console.ResetColor();

        Console.WriteLine("Allocating 128 KB of data in a .NET byte array...");
        byte[] dotNetArray = new byte[131072];

        using GorgonNativeBuffer<byte> pinBuffer = dotNetArray.PinAsNativeBuffer();
        Console.WriteLine($"Buffer allocated at 0x{((ulong)pinBuffer.Location):x} with {pinBuffer.SizeInBytes} bytes.");

        Console.WriteLine("Writing 256 bytes to pinned native buffer at a 32 KB offset...");
        for (int i = 0; i < 256; ++i)
        {
            pinBuffer[i + 32768] = (byte)i;
        }

        Console.WriteLine("Reading 256 bytes from index 32768 in the .NET byte array...");
        for (int i = 0; i < 256; i++)
        {
            if (dotNetArray[i + 32768] != i)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failure at byte {i + 32768}. Expected {i}, got 0x{dotNetArray[i + 32768]:x}.");
                Console.ResetColor();
                return;
            }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("OK.");
        Console.ResetColor();

        Console.WriteLine("Writing 4 byte int32 value at byte offset 1 using AsRef...");
        ref int intValue = ref pinBuffer.AsRef<int>(1);
        unchecked
        {
            intValue = (int)0x807FB8A0;
        }

        Console.WriteLine($"Reading integer value 0x{intValue:x} as bytes...");

        if (BitConverter.IsLittleEndian)
        {
            if ((dotNetArray[1] != 0xA0) || (dotNetArray[2] != 0xB8) || (dotNetArray[3] != 0x7F) || (dotNetArray[4] != 0x80))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failure reading integer bytes.");
                Console.ResetColor();
                return;
            }
        }
        else
        {
            if ((dotNetArray[4] != 0xA0) || (dotNetArray[3] != 0xB8) || (dotNetArray[2] != 0x7F) || (dotNetArray[1] != 0x80))
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failure reading integer bytes.");
                Console.ResetColor();
                return;
            }
        }

        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("OK.");
        Console.ResetColor();

        Data data = new();

        using GorgonNativeBuffer<byte> byteBuffer = data.PinAsNativeByteBuffer();

        Console.Write($"Data: ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"ID ({data.ID}), Pos ({data.Position.X:0.0##}x{data.Position.Y:0.0##}), Scale ({data.Scale.X:0.0##}x{data.Scale.Y:0.0##}), Angle ({data.Angle:0.0})");
        Console.ResetColor();
        Console.Write("Data as byte buffer: ");
        Console.ForegroundColor = ConsoleColor.Green;

        for (int i = 0; i < byteBuffer.SizeInBytes; ++i)
        {
            if (i > 0)
            {
                Console.Write(", ");
            }

            Console.Write($"0x{byteBuffer[i]:x}");
        }

        Console.ResetColor();
        Console.WriteLine();

        GorgonPtr<Data> dataPtr = GorgonPtr<byte>.To<Data>(byteBuffer);
        ref Data newData = ref dataPtr.Value;

        Console.Write($"Reading data (GorgonPtr<Data>): ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine($"ID ({newData.ID}), Pos ({newData.Position.X:0.0##}x{newData.Position.Y:0.0##}), Scale ({newData.Scale.X:0.0##}x{newData.Scale.Y:0.0##}), Angle ({newData.Angle:0.0})");
        Console.ResetColor();

        Console.WriteLine("Press any key...");
        Console.ReadKey();
    }

    /// <summary>
    /// Functionality related to the GorgonPtr type.
    /// </summary>
    private static void GorgonPtrStuff()
    {
        Console.Clear();
        Console.ForegroundColor = ConsoleColor.Green;
        Console.WriteLine("Using GorgonPtr<T>.");
        Console.ResetColor();
        Console.WriteLine("A GorgonPtr<T> is a safe pointer type that allows access to native memory.");
        Console.WriteLine("They can be used in a similar way like a pointer with pointer arithmetic, and array indexing.");
        Console.WriteLine("These types can also be cast to native pointer types (e.g. byte *), and Span<T> types.");
        Console.WriteLine();

        unsafe
        {
            byte* bytes = stackalloc byte[1024];

            // This will allow us to read/write from/into the array created above.
            // The advantage here is that memory will be contained within the range we specify, and we cannot exceed the bounds of the
            // buffer.
            // In this case, we're taking a slice of the buffer above that starts at the 256th byte, and covers 256 bytes of data.
            GorgonPtr<byte> ptr = new(bytes + 256, 256);

            Console.WriteLine("Writing indexed bytes...");

            for (int i = 0; i < ptr.Length; ++i)
            {
                ptr[i] = (byte)i;
            }

            Console.WriteLine("Reading indexed bytes...");

            for (int i = 0; i < ptr.Length; ++i)
            {
                if (ptr[i] != (byte)i)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failure at byte {i}. Expected {i}, got {ptr[i]}.");
                    Console.ResetColor();
                    return;
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK.");
            Console.ResetColor();

            // We can also treat a gorgon ptr like a pointer by using ++ and -- to advance or whatever the opposite is called the pointer.
            GorgonPtr<byte> newPtr = ptr;

            Console.WriteLine("Writing bytes using pointer arithmetic...");
            for (int i = 0; i < ptr.Length; ++i)
            {
                newPtr.Value = (byte)i;
                newPtr++;
            }

            Console.WriteLine("Reading bytes using pointer arithmetic...");
            newPtr = ptr;
            for (int i = 0; i < ptr.Length; ++i)
            {
                if (newPtr.Value != (byte)i)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failure at byte {i}. Expected {i}, got {newPtr.Value}");
                    Console.ResetColor();
                    return;
                }

                newPtr++;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK.");
            Console.ResetColor();

            Span<byte> span = ptr;
            Console.WriteLine("Writing Span<byte> bytes...");

            for (int i = 0; i < span.Length; ++i)
            {
                span[i] = (byte)i;
            }

            Console.WriteLine("Reading Span<byte> bytes...");

            for (int i = 0; i < span.Length; ++i)
            {
                if (span[i] != (byte)i)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failure at byte {i}. Expected {i}, got {ptr[i]}.");
                    Console.ResetColor();
                    return;
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK.");
            Console.ResetColor();

            byte* unsafeBytes = (byte*)ptr;

            Console.WriteLine("Writing byte* data...");
            for (int i = 0; i < 256; ++i)
            {
                *unsafeBytes = (byte)i;
                unsafeBytes++;
            }

            Console.WriteLine("Reading byte* data...");

            unsafeBytes = (byte*)ptr;
            for (int i = 0; i < 256; ++i)
            {
                if (*unsafeBytes != (byte)i)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failure at byte {i}. Expected {i}, got {newPtr.Value}");
                    Console.ResetColor();
                    return;
                }

                unsafeBytes++;
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK.");
            Console.ResetColor();

            // We can also treat a gorgon ptr as a slice, similar to a span.
            // This now creates a pointer that references the 8th byte, up to the 16th byte of the source pointer.
            GorgonPtr<byte> slicePtr = ptr[8..16];

            Console.WriteLine("Writing slice [8..16] bytes...");
            for (int i = 0; i < slicePtr.Length; ++i)
            {
                slicePtr[i] = (byte)i;
            }

            Console.WriteLine("Reading slice [8..16] bytes...");
            newPtr = ptr;
            for (int i = 0; i < slicePtr.Length; ++i)
            {
                if (slicePtr[i] != (byte)i)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failure at byte {i}. Expected {i}, got {newPtr.Value}");
                    Console.ResetColor();
                    return;
                }

                if (ptr[i + 8] != (byte)i)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failure at byte {i + 8}. Expected {i}, got {newPtr.Value}");
                    Console.ResetColor();
                    return;
                }
            }

            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine("OK.");
            Console.ResetColor();

            Data data = new();

            Console.Write($"Writing byte data: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"ID ({data.ID}), Pos ({data.Position.X:0.0##}x{data.Position.Y:0.0##}), Scale ({data.Scale.X:0.0##}x{data.Scale.Y:0.0##}), Angle ({data.Angle:0.0})");
            Console.ResetColor();

            ptr = new GorgonPtr<byte>(bytes, 1024);
            ptr.AsRef<Data>() = data;

            Console.ResetColor();
            Console.Write("Byte data: ");
            Console.ForegroundColor = ConsoleColor.Green;

            for (int i = 0; i < Unsafe.SizeOf<Data>(); ++i)
            {
                if (i > 0)
                {
                    Console.Write(", ");
                }

                Console.Write($"0x{ptr[i]:x}");
            }

            Console.ResetColor();
            Console.WriteLine();

            GorgonPtr<Data> dataPtr = GorgonPtr<byte>.To<Data>(ptr);
            ref Data newData = ref dataPtr.Value;

            Console.Write($"Reading casted data: ");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"ID ({newData.ID}), Pos ({newData.Position.X:0.0##}x{newData.Position.Y:0.0##}), Scale ({newData.Scale.X:0.0##}x{newData.Scale.Y:0.0##}), Angle ({newData.Angle:0.0})");
            Console.ResetColor();
        }

        Console.WriteLine("Press any key...");
        Console.ReadKey();
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        try
        {
            Console.Title = "Native Memory";
            Console.CursorVisible = false;
            Console.ForegroundColor = ConsoleColor.White;

            GorgonPtrStuff();
            GorgonNativeBufferStuff();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"There was an error:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
        }
        finally
        {
            Console.ResetColor();
            Console.CursorVisible = true;
        }
    }
}
