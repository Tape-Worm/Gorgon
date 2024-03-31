
// 
// Gorgon
// Copyright (C) 2012 Michael Winsor
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

using System.Runtime.InteropServices;
using System.Text;
using Gorgon.IO;
using Gorgon.Math;

namespace Gorgon.Examples;

/// <summary>
/// Entry point class
/// </summary>
/// <remarks>
/// This example showcases the enhancements brought by the new BinaryReader and BinaryWriter objects
/// 
/// These objects provide extended functionality for the standard .NET BinaryReader/Writer classes so that generic types can be read and/or written, and raw memory (unsafe and ref managed pointers) can be 
/// also be read and/or written
/// </remarks>
internal static class Program
{
    /// <summary>
    /// Test data that we can use to read/write to and from the stream.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    private struct SomeTestData
    {
        /// <summary>
        /// Value 1
        /// </summary>
        public int Value1;
        /// <summary>
        /// Value 2
        /// </summary>
        public double Value2;
        /// <summary>
        /// Value 3
        /// </summary>
        public short Value3;
    }

    /// <summary>
    /// Function to write the contents of a span into a stream using the BinaryWriter and reading it back again using the BinaryReader.
    /// </summary>
    /// <param name="stream">The stream that will receive the data.</param>
    /// <remarks>
    /// <para>
    /// This is the lowest level of I/O where raw byte data is sent to and read back from the stream. This requires a slightly more complicated syntax for writing and reading data. Users 
    /// should rarely, if ever, need to use these methods.
    /// </para>
    /// </remarks>
    private static void WriteSpan(MemoryStream stream)
    {
        stream.Position = 0;
        BinaryWriter writer = new(stream, Encoding.UTF8, true);
        BinaryReader reader = new(stream, Encoding.UTF8, true);

        try
        {
            SomeTestData data = new()
            {
                Value1 = 1,
                Value2 = 2.1,
                Value3 = 3
            };

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Writing/Reading span value to memory into a memory stream.");
            Console.ForegroundColor = ConsoleColor.White;

            ReadOnlySpan<SomeTestData> span = new(ref data);

            writer.WriteRange(span);

            stream.Position = 0;

            SomeTestData readData = default;
            Span<SomeTestData> readSpan = new(ref readData);
            reader.ReadRange(readSpan);

            Console.WriteLine($"int32 Value1 = 1: {readData.Value1 == 1}");
            Console.WriteLine($"double Value2 = 2.1: {readData.Value2.EqualsEpsilon(2.1)}");
            Console.WriteLine($"int16 Value3 = 3: {readData.Value3 == 3}");

            stream.Position = 0;
        }
        finally
        {
            writer.Dispose();
            reader.Dispose();
        }
    }

    /// <summary>
    /// Function to write the contents of a value type to a stream using the BinaryWriter and reading it back again using the BinaryReader.
    /// </summary>
    /// <param name="stream">The stream that will receive the data.</param>
    private static void WriteByRefValueType(MemoryStream stream)
    {
        stream.Position = 0;
        BinaryWriter writer = new(stream, Encoding.UTF8, true);
        BinaryReader reader = new(stream, Encoding.UTF8, true);

        try
        {
            SomeTestData data = new()
            {
                Value1 = 1234,
                Value2 = 3.1459,
                Value3 = 42
            };

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Writing/Reading a value type to a memory stream.");
            Console.ForegroundColor = ConsoleColor.White;

            writer.WriteValue(in data);

            stream.Position = 0;

            reader.ReadValue(out SomeTestData readData);

            Console.WriteLine($"int32 Value1 = 1234: {readData.Value1 == 1234}");
            Console.WriteLine($"double Value2 = 3.1459: {readData.Value2.EqualsEpsilon(3.1459)}");
            Console.WriteLine($"int16 Value3 = 42: {readData.Value3 == 42}");

            stream.Position = 0;
        }
        finally
        {
            writer.Dispose();
            reader.Dispose();
        }
    }

    /// <summary>
    /// Function to write an array of value types to a stream using the BinaryWriter and reading it back again using the BinaryReader.
    /// </summary>
    /// <param name="stream">The stream that will receive the data.</param>
    private static void WriteArrayValues(MemoryStream stream)
    {
        stream.Position = 0;

        BinaryWriter writer = new(stream, Encoding.UTF8, true);
        BinaryReader reader = new(stream, Encoding.UTF8, true);

        try
        {
            SomeTestData[] expected = new SomeTestData[3];

            for (int i = 1; i < 4; ++i)
            {
                expected[i - 1] = new SomeTestData
                {
                    Value1 = i,
                    Value2 = System.Math.PI * i,
                    Value3 = (short)(i & 2)
                };
            }

            Console.ForegroundColor = ConsoleColor.Cyan;
            Console.WriteLine("Writing/Reading an array of value types to a memory stream.");
            Console.ForegroundColor = ConsoleColor.White;

            writer.WriteRange<SomeTestData>(expected);

            stream.Position = 0;

            SomeTestData[] actual = new SomeTestData[4];
            reader.ReadRange(actual.AsSpan(1));

            for (int i = 1; i < 4; ++i)
            {
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"[{i - 1}] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"int32 Value1 = {expected[i - 1].Value1}: {actual[i].Value1 == expected[i - 1].Value1}");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"[{i - 1}] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"double Value2 = {expected[i - 1].Value2:0.00000}: {actual[i].Value2.EqualsEpsilon(expected[i - 1].Value2)}");
                Console.ForegroundColor = ConsoleColor.Yellow;
                Console.Write($"[{i - 1}] ");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"int16 Value3 = {expected[i - 1].Value3}: {actual[i].Value3 == expected[i - 1].Value3}");
            }

            stream.Position = 0;
        }
        finally
        {
            writer.Dispose();
            reader.Dispose();
        }
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        MemoryStream stream = new();

        try
        {
            Console.Title = "Gorgon Example #5 - Binary reader/write enhancements.";
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("This example showcases the enhancements brought by the extensions to the BinaryReader and BinaryWriter objects.");
            Console.WriteLine();
            Console.WriteLine("These methods provide extended functionality to the standard .NET BinaryReader/Writer classes so that generic types ");
            Console.WriteLine("can be read and/or written, and raw memory (GorgonPtr types/Spans) can be also be read and/or written.");
            Console.WriteLine();

            WriteByRefValueType(stream);
            WriteSpan(stream);
            WriteArrayValues(stream);

            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"There was an error:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
            Console.ResetColor();
        }
        finally
        {
            stream.Dispose();

            Console.ResetColor();
            Console.WriteLine();
            Console.WriteLine("Press any key.");

            Console.ReadKey();
        }
    }
}
