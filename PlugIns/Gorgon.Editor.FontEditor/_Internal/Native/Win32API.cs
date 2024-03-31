// 
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
// Created: Sunday, March 10, 2013 11:07:01 PM
// 

using System.Buffers;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;
using Gorgon.Core;
using Gorgon.Editor.FontEditor.Properties;
using Gorgon.Native;

namespace Gorgon.Editor.FontEditor;

/// <summary>
/// Win32 functions
/// </summary>
[SuppressUnmanagedCodeSecurity]
internal static partial class Win32API
{
    // List of ranges.
    private static IDictionary<string, GorgonRange<int>> _ranges;
    // List of code point names.
    private static IDictionary<int, string> _codePointNames;

    /// <summary>
    /// Gets the ranges supported by a font.
    /// </summary>
    /// <param name="hdc">Device context.</param>
    /// <param name="lpgs">Font ranges.</param>
    /// <returns></returns>
    [LibraryImport("gdi32.dll")]
    private static partial uint GetFontUnicodeRanges(IntPtr hdc, IntPtr lpgs);

    /// <summary>
    /// Selects an object into the device context.
    /// </summary>
    /// <param name="hDC">Device context.</param>
    /// <param name="hObject">Object to select.</param>
    /// <returns>The previous object.</returns>
    [LibraryImport("gdi32.dll")]
    public static partial IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

    /// <summary>
    /// Function to retrieve indices for a glyph.
    /// </summary>
    /// <param name="hdc"></param>
    /// <param name="lpsz"></param>
    /// <param name="c"></param>
    /// <param name="pgi"></param>
    /// <param name="fl"></param>
    /// <returns></returns>
    [LibraryImport("gdi32.dll", EntryPoint = "GetGlyphIndicesW", StringMarshalling = StringMarshalling.Utf16)]
    private static partial uint GetGlyphIndices(IntPtr hdc, string lpsz, int c, [Out] ushort[] pgi, uint fl);

    /// <summary>
    /// Function to build a list of unicode ranges.
    /// </summary>
    private static void BuildUnicodeRangeList()
    {
        IList<string> rangeLines = Resources.UnicodeBlocks.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);
        _ranges = new SortedDictionary<string, GorgonRange<int>>();

        // Break out the lines.
        foreach (string line in rangeLines)
        {
            IList<string> items = line.Split([';']);

            // Get range.
            if ((string.IsNullOrEmpty(items[0])) || (string.IsNullOrEmpty(items[1])))
            {
                continue;
            }

            GorgonRange<int> range = new(int.Parse(items[0][..items[0].IndexOf('.')], NumberStyles.HexNumber),
                                        int.Parse(items[0][(items[0].LastIndexOf('.') + 1)..], NumberStyles.HexNumber));

            // Combine the first 2 latin categories into the one category.
            if (range.Maximum <= 0xff)
            {
                if (!_ranges.ContainsKey("Latin + Latin Supplement"))
                {
                    _ranges.Add("Latin + Latin Supplement", new GorgonRange<int>(0, 0xFF));
                }
            }
            else
            {
                _ranges.Add(items[1].Trim(), range);
            }
        }
    }

    /// <summary>
    /// Function to determine if a character is supported.
    /// </summary>
    /// <param name="c">The character to check.</param>
    /// <param name="hDc">Device context.</param>
    /// <returns>TRUE if supported, FALSE if not.</returns>
    public static bool IsGlyphSupported(char c, IntPtr hDc)
    {
        ushort[] indices = ArrayPool<ushort>.Shared.Rent(1);

        try
        {
            GetGlyphIndices(hDc, c.ToString(), 1, indices, 0x01);

            return indices[0] != 0xffff;
        }
        finally
        {
            ArrayPool<ushort>.Shared.Return(indices);
        }
    }

    /// <summary>
    /// Function to retrieve 
    /// </summary>
    /// <param name="character">Character for the code point.</param>
    /// <returns>The name of the code point if it exists.</returns>
    public static string GetCodePointName(char character)
    {
        if (_codePointNames == null)
        {
            IList<string> lines = Resources.UnicodeData.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries);

            _codePointNames = new Dictionary<int, string>();

            foreach (string[] fields in lines.Select(line => line.Split([';'])))
            {
                _codePointNames.Add(int.Parse(fields[0], NumberStyles.HexNumber),
                                    string.IsNullOrEmpty(fields[10]) ? fields[1] : fields[10]);
            }
        }

        return _codePointNames.ContainsKey(Convert.ToInt32(character))
                   ? _codePointNames[Convert.ToInt32(character)]
                   : string.Empty;
    }

    /// <summary>
    /// Function to retrieve the ranges available to a specific font.
    /// </summary>
    /// <param name="hDc">Device context.</param>
    /// <returns>A list of ranges.</returns>
    public static IDictionary<string, GorgonRange<int>> GetUnicodeRanges(IntPtr hDc)
    {
        Dictionary<string, GorgonRange<int>> result;

        if (_ranges == null)
        {
            BuildUnicodeRangeList();
        }

        uint size = GetFontUnicodeRanges(hDc, IntPtr.Zero);

        using GorgonNativeBuffer<byte> buffer = new((int)size);
        GetFontUnicodeRanges(hDc, (nint)((GorgonPtr<byte>)buffer));

        ref int bufferPtr = ref buffer.AsRef<int>(12);
        int itemCount = bufferPtr;

        result = new Dictionary<string, GorgonRange<int>>(itemCount);

        int ptrOffset = 16;

        for (int i = 0; i < itemCount; i++)
        {
            ref ushort shortPtr = ref buffer.AsRef<ushort>(ptrOffset);
            ref ushort shortPtr2 = ref Unsafe.Add(ref shortPtr, 1);
            ptrOffset += 4;

            ushort min = shortPtr;
            ushort max = shortPtr2;

            GorgonRange<int> value = new(min, max + min - 1);

            KeyValuePair<string, GorgonRange<int>> rangeName = (from unicodeRange in _ranges
                                                                where unicodeRange.Value.Contains(value.Minimum) && unicodeRange.Value.Contains(value.Maximum)
                                                                select unicodeRange).SingleOrDefault();

            if ((!string.IsNullOrEmpty(rangeName.Key)) && (!result.ContainsKey(rangeName.Key)))
            {
                result.Add(rangeName.Key, rangeName.Value);
            }
        }

        return result;
    }

    /// <summary>Initializes static members of the <see cref="Win32API" /> class.</summary>
    static Win32API() => Marshal.PrelinkAll(typeof(Win32API));
}
