#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Thursday, August 25, 2011 8:10:31 PM
// 
#endregion

using System.Globalization;
using Gorgon.Math;
using Gorgon.Properties;

namespace Gorgon.Core;

/// <summary>
/// Extension methods to provide formatting on numeric values for memory and hexadecimal values.
/// </summary>
public static class GorgonNumericFormattingExtension
{
    #region Methods.
    /// <summary>
    /// FUnction to return a string formatted to the current culture.
    /// </summary>
    /// <param name="value">Value to format.</param>
    /// <param name="sizeType">Size value to use.</param>
    /// <param name="requireDecimal">[Optional] <b>true</b> if the value should have a decimal place always appear, or <b>false</b> if not.</param>
    /// <returns>The string formatted to the current UI culture.</returns>
    private static string GetCultureString(double value, string sizeType, bool requireDecimal = true) =>
        string.Format(CultureInfo.CurrentCulture, requireDecimal ? "{0:0.0#} {1}" : "{0:0.#} {1}", value, sizeType);

    /// <summary>
    /// Function to return a formatted string containing the memory amount.
    /// </summary>
    /// <param name="amount">Amount of memory in bytes to format.</param>
    /// <returns>A string containing the formatted amount of memory in bytes.</returns>
    /// <remarks>
    /// This will produce a string value with the number of bytes, suffixed with the word 'bytes'.
    /// <code language="csharp">
    /// byte bytes = 128;
    /// 
    /// Console.WriteLine(bytes.FormatMemory());
    /// 
    /// // Produces: "128 bytes".
    /// </code>
    /// </remarks>
    public static string FormatMemory(this byte amount) => GetCultureString(amount, Resources.GOR_UNIT_MEM_BYTES, false);

    /// <summary>
    /// Function to return a formatted string containing the memory amount.
    /// </summary>
    /// <param name="amount">Amount of memory in bytes to format.</param>
    /// <returns>A string containing the formatted amount of memory in kilobytes.</returns>
    /// <remarks>
    /// This will produce a string value with the number of bytes suffixed with the word 'bytes' if less than 1023, or the number of kilobytes suffixed with the abbreviation 'KB' if the value is above 1023.
    /// <code language="csharp">
    /// short bytes = 999;
    /// short kilobytes = 2048;
    /// 
    /// Console.WriteLine(bytes.FormatMemory());
    /// Console.WriteLine(kilobytes.FormatMemory());
    /// 
    /// // Produces: "128 bytes" and "2.0 KB".
    /// </code>
    /// </remarks>
    public static string FormatMemory(this short amount)
    {
        double scale = amount.Abs() / 1024.0;

        return GetCultureString(scale >= 1.0 ? scale : amount, scale >= 1.0 ? Resources.GOR_UNIT_MEM_KB : Resources.GOR_UNIT_MEM_BYTES, false);
    }

    /// <summary>
    /// Function to return a formatted string containing the memory amount.
    /// </summary>
    /// <param name="amount">Amount of memory in bytes to format.</param>
    /// <returns>A string containing the formatted amount of memory.</returns>
    /// <remarks>
    /// This will produce a string value with the number of bytes suffixed with the word 'bytes' if less than 1023, or the number of kilobytes suffixed with the abbreviation 'KB' if the value is above 1023.
    /// <code language="csharp">
    /// ushort bytes = 999;
    /// ushort kilobytes = 2048;
    /// 
    /// Console.WriteLine(bytes.FormatMemory());
    /// Console.WriteLine(kilobytes.FormatMemory());
    /// 
    /// // Produces: "128 bytes" and "2.0 KB".
    /// </code>
    /// <para>
    /// If the value cannot be represented with a string suffix, then the number of bytes will be displayed as default.
    /// </para>
    /// </remarks>
    public static string FormatMemory(this ushort amount)
    {
        double scale = amount / 1024.0;

        return GetCultureString(scale >= 1.0 ? scale : amount, scale >= 1.0 ? Resources.GOR_UNIT_MEM_KB : Resources.GOR_UNIT_MEM_BYTES, false);
    }

    /// <summary>
    /// Function to return a formatted string containing the memory amount.
    /// </summary>
    /// <param name="amount">Amount of memory in bytes to format.</param>
    /// <returns>A string containing the formatted amount of memory.</returns>
    /// <remarks>
    /// This will produce a string value with the number of bytes suffixed with the word 'bytes' if less than 1023, the number of kilobytes suffixed with the abbreviation 'KB' if the value is 
    /// within the range of 1024 to 1048575, the number of megabytes suffixed with the abbreviation MB if the value is within the range of 1048576 to 1073741823, or the number of gigabytes 
    /// with the suffix of 'GB' if the value is greater than 1073741823.
    /// <code language="csharp">
    /// int bytes = 999;
    /// int kilobytes = 2048;
    /// int megabytes = 3145728;
    /// int gigabytes = int.MaxValue;
    /// 
    /// Console.WriteLine(bytes.FormatMemory());
    /// Console.WriteLine(kilobytes.FormatMemory());
    /// Console.WriteLine(megabytes.FormatMemory());
    /// Console.WriteLine(gigabytes.FormatMemory());
    /// 
    /// // Produces: "128 bytes", "2.0 KB", "3.0 MB", and "2 GB".
    /// </code>
    /// <para>
    /// If the value cannot be represented with a string suffix, then the number of bytes will be displayed as default.
    /// </para>
    /// </remarks>
    public static string FormatMemory(this int amount)
    {
        double scale = amount.Abs() / 1073741824.0;

        if (scale >= 1.0)
        {
            return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_GB);
        }

        scale = amount / 1048576.0;

        if (scale >= 1.0)
        {
            return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_MB);
        }

        scale = amount / 1024.0;

        return GetCultureString(scale >= 1.0 ? scale : amount, scale >= 1.0 ? Resources.GOR_UNIT_MEM_KB : Resources.GOR_UNIT_MEM_BYTES, false);
    }

    /// <summary>
    /// Function to return a formatted string containing the memory amount.
    /// </summary>
    /// <param name="amount">Amount of memory in bytes to format.</param>
    /// <returns>A string containing the formatted amount of memory.</returns>
    /// <remarks>
    /// This will produce a string value with the number of bytes suffixed with the word 'bytes' if less than 1023, the number of kilobytes suffixed with the abbreviation 'KB' if the value is 
    /// within the range of 1024 to 1048575, the number of megabytes suffixed with the abbreviation MB if the value is within the range of 1048576 to 1073741823, or the number of gigabytes 
    /// with the suffix of 'GB' if the value is greater than 1073741823.
    /// <code language="csharp">
    /// uint bytes = 999;
    /// uint kilobytes = 2048;
    /// uint megabytes = 3145728;
    /// uint gigabytes = 3221225472;
    /// 
    /// Console.WriteLine(bytes.FormatMemory());
    /// Console.WriteLine(kilobytes.FormatMemory());
    /// Console.WriteLine(megabytes.FormatMemory());
    /// Console.WriteLine(gigabytes.FormatMemory());
    /// 
    /// // Produces: "128 bytes", "2.0 KB", "3.0 MB", and "3 GB".
    /// </code>
    /// <para>
    /// If the value cannot be represented with a string suffix, then the number of bytes will be displayed as default.
    /// </para>
    /// </remarks>
    public static string FormatMemory(this uint amount)
    {
        double scale = amount / 1073741824.0;

        if (scale >= 1.0)
        {
            return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_GB);
        }

        scale = amount / 1048576.0;

        if (scale >= 1.0)
        {
            return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_MB);
        }

        scale = amount / 1024.0;

        return GetCultureString(scale >= 1.0 ? scale : amount, scale >= 1.0 ? Resources.GOR_UNIT_MEM_KB : Resources.GOR_UNIT_MEM_BYTES, false);
    }

    /// <summary>
    /// Function to return a formatted string containing the memory amount.
    /// </summary>
    /// <param name="amount">Amount of memory in bytes to format.</param>
    /// <returns>A string containing the formatted amount of memory.</returns>
    /// <remarks>
    /// This will produce a string value with the number of bytes suffixed with the word 'bytes' if less than 1023, the number of kilobytes suffixed with the abbreviation 'KB' if the value is 
    /// within the range of 1024 to 1048575, the number of megabytes suffixed with the abbreviation MB if the value is within the range of 1048576 to 1073741823, the number of gigabytes 
    /// with the suffix of 'GB' if the value is within the range of 1073741824 to 1099511627775, the number of terabytes with the suffix 'TB' if the value is within the range of 1099511627776 
    /// to 1125899906842623, of the number of petabytes with the suffix 'PB' if the value is greater than 1125899906842623.
    /// <code language="csharp">
    /// long bytes = 999;
    /// long kilobytes = 2048;
    /// long megabytes = 3145728;
    /// long gigabytes = 4294967296;
    /// long terabytes = 5497558138880;
    /// long petabytes = 6755399441055744;
    /// 
    /// Console.WriteLine(bytes.FormatMemory());
    /// Console.WriteLine(kilobytes.FormatMemory());
    /// Console.WriteLine(megabytes.FormatMemory());
    /// Console.WriteLine(gigabytes.FormatMemory());
    /// Console.WriteLine(terabytes.FormatMemory());
    /// Console.WriteLine(petabytes.FormatMemory());
    /// 
    /// // Produces: "128 bytes", "2.0 KB", "3.0 MB", "4.0 GB", "5.0 TB", and "6.0 PB".
    /// </code>
    /// <para>
    /// If the value cannot be represented with a string suffix, then the number of bytes will be displayed as default.
    /// </para>
    /// </remarks>
    public static string FormatMemory(this long amount)
    {
        double scale = amount.Abs() / 1125899906842624.0;

        if (scale >= 1.0)
        {
            return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_PB);
        }

        scale = amount / 1099511627776.0;

        if (scale >= 1.0)
        {
            return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_TB);
        }

        scale = amount / 1073741824.0;

        if (scale >= 1.0)
        {
            return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_GB);
        }

        scale = amount / 1048576.0;

        if (scale >= 1.0)
        {
            return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_MB);
        }

        scale = amount / 1024.0;

        return GetCultureString(scale >= 1.0 ? scale : amount, scale >= 1.0 ? Resources.GOR_UNIT_MEM_KB : Resources.GOR_UNIT_MEM_BYTES, false);
    }

    /// <summary>
    /// Function to return a formatted string containing the memory amount.
    /// </summary>
    /// <param name="amount">Amount of memory in bytes to format.</param>
    /// <returns>A string containing the formatted amount of memory.</returns>
    /// <remarks>
    /// This will produce a string value with the number of bytes suffixed with the word 'bytes' if less than 1023, the number of kilobytes suffixed with the abbreviation 'KB' if the value is 
    /// within the range of 1024 to 1048575, the number of megabytes suffixed with the abbreviation MB if the value is within the range of 1048576 to 1073741823, the number of gigabytes 
    /// with the suffix of 'GB' if the value is within the range of 1073741824 to 1099511627775, the number of terabytes with the suffix 'TB' if the value is within the range of 1099511627776 
    /// to 1125899906842623, of the number of petabytes with the suffix 'PB' if the value is greater than 1125899906842623.
    /// <code language="csharp">
    /// ulong bytes = 999;
    /// ulong kilobytes = 2048;
    /// ulong megabytes = 3145728;
    /// ulong gigabytes = 4294967296;
    /// ulong terabytes = 5497558138880;
    /// ulong petabytes = 6755399441055744;
    /// 
    /// Console.WriteLine(bytes.FormatMemory());
    /// Console.WriteLine(kilobytes.FormatMemory());
    /// Console.WriteLine(megabytes.FormatMemory());
    /// Console.WriteLine(gigabytes.FormatMemory());
    /// Console.WriteLine(terabytes.FormatMemory());
    /// Console.WriteLine(petabytes.FormatMemory());
    /// 
    /// // Produces: "128 bytes", "2.0 KB", "3.0 MB", "4.0 GB", "5.0 TB", and "6.0 PB".
    /// </code>
    /// <para>
    /// If the value cannot be represented with a string suffix, then the number of bytes will be displayed as default.
    /// </para>
    /// </remarks>
    public static string FormatMemory(this ulong amount)
    {
        double scale = amount / 1125899906842624.0;

        if (scale >= 1.0)
        {
            return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_PB);
        }

        scale = amount / 1099511627776.0;

        if (scale >= 1.0)
        {
            return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_TB);
        }

        scale = amount / 1073741824.0;

        if (scale >= 1.0)
        {
            return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_GB);
        }

        scale = amount / 1048576.0;

        if (scale >= 1.0)
        {
            return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_MB);
        }

        scale = amount / 1024.0;

        return GetCultureString(scale >= 1.0 ? scale : amount, scale >= 1.0 ? Resources.GOR_UNIT_MEM_KB : Resources.GOR_UNIT_MEM_BYTES, false);
    }

    /// <summary>
    /// Function to return a formatted string containing the memory amount.
    /// </summary>
    /// <param name="amount">Amount of memory in bytes to format.</param>
    /// <returns>A string containing the formatted amount of memory.</returns>
    /// <remarks>
    /// This overload is like the <see cref="FormatMemory(int)"/> method, only it will return the value with floating point formatting. e.g: "3.25 MB" instead of "3.0 MB"
    /// </remarks>
    public static string FormatMemory(this float amount)
    {
        double scale = amount.Abs() / 1125899906842624.0;

        if (scale >= 1.0)
        {
            return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_PB);
        }

        scale = amount / 1099511627776.0;

        if (scale >= 1.0)
        {
            return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_TB);
        }

        scale = amount / 1073741824.0;

        if (scale >= 1.0)
        {
            return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_GB);
        }

        scale = amount / 1048576.0;

        if (scale >= 1.0)
        {
            return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_MB);
        }

        scale = amount / 1024.0;

        return GetCultureString(scale >= 1.0 ? scale : amount, scale >= 1.0 ? Resources.GOR_UNIT_MEM_KB : Resources.GOR_UNIT_MEM_BYTES, false);
    }

    /// <summary>
    /// Function to return a formatted string containing the memory amount.
    /// </summary>
    /// <param name="amount">Amount of memory in bytes to format.</param>
    /// <returns>A string containing the formatted amount of memory.</returns>
    /// <remarks>
    /// This overload is like the <see cref="FormatMemory(int)"/> method, only it will return the value with floating point formatting. e.g: "3.25 MB" instead of "3.0 MB"
    /// </remarks>
    public static string FormatMemory(this double amount)
    {
        double scale = amount.Abs() / 1125899906842624.0;

        if (scale >= 1.0)
        {
            return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_PB);
        }

        scale = amount / 1099511627776.0;

        if (scale >= 1.0)
        {
            return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_TB);
        }

        scale = amount / 1073741824.0;

        if (scale >= 1.0)
        {
            return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_GB);
        }

        scale = amount / 1048576.0;

        if (scale >= 1.0)
        {
            return GetCultureString(scale >= 1.0 ? scale : amount, Resources.GOR_UNIT_MEM_MB);
        }

        scale = amount / 1024.0;

        return GetCultureString(scale >= 1.0 ? scale : amount, scale >= 1.0 ? Resources.GOR_UNIT_MEM_KB : Resources.GOR_UNIT_MEM_BYTES, false);
    }

    /// <summary>
    /// Function to format a byte value into a hexadecimal string.
    /// </summary>
    /// <param name="value">Byte value to format.</param>
    /// <returns>The formatted byte value.</returns>
    /// <remarks>
    /// This will return a string with the number formatted as a hexadecimal number. This number will be padded with zeroes that represent the size of the type. For example:
    /// <code language="csharp">
    /// byte hexValue = 15;
    /// 
    /// Console.WriteLine(hexValue.FormatHex()); // Produces "0F"
    /// 
    /// int hexValueInt = 127;
    /// 
    /// Console.WriteLine(hexValueInt.FormatHex()); // Produces "007F".
    /// 
    /// short hexValueShort = 1023;
    /// 
    /// Console.WriteLine(hexValueShort.FormatHex()); // Produces "03FF".
    /// </code>
    /// </remarks>
    public static string FormatHex(this byte value) => value.ToString("x").PadLeft(2, '0');

    /// <summary>
    /// Function to format a short value into a hexadecimal string.
    /// </summary>
    /// <param name="value">Short value to format.</param>
    /// <returns>The formatted short value.</returns>
    /// <remarks>
    /// This will return a string with the number formatted as a hexadecimal number. This number will be padded with zeroes that represent the size of the type. For example:
    /// <code language="csharp">
    /// byte hexValue = 15;
    /// 
    /// Console.WriteLine(hexValue.FormatHex()); // Produces "0F"
    /// 
    /// int hexValueInt = 127;
    /// 
    /// Console.WriteLine(hexValueInt.FormatHex()); // Produces "007F".
    /// 
    /// short hexValueShort = 1023;
    /// 
    /// Console.WriteLine(hexValueShort.FormatHex()); // Produces "03FF".
    /// </code>
    /// </remarks>
    public static string FormatHex(this short value) => value.ToString("x").PadLeft(4, '0');

    /// <summary>
    /// Function to format an unsigned short value into a hexadecimal string.
    /// </summary>
    /// <param name="value">Unsigned short value to format.</param>
    /// <returns>The formatted unsigned short value.</returns>
    /// <remarks>
    /// This will return a string with the number formatted as a hexadecimal number. This number will be padded with zeroes that represent the size of the type. For example:
    /// <code language="csharp">
    /// byte hexValue = 15;
    /// 
    /// Console.WriteLine(hexValue.FormatHex()); // Produces "0F"
    /// 
    /// int hexValueInt = 127;
    /// 
    /// Console.WriteLine(hexValueInt.FormatHex()); // Produces "007F".
    /// 
    /// short hexValueShort = 1023;
    /// 
    /// Console.WriteLine(hexValueShort.FormatHex()); // Produces "03FF".
    /// </code>
    /// </remarks>
    public static string FormatHex(this ushort value) => value.ToString("x").PadLeft(4, '0');

    /// <summary>
    /// Function to format an integer value into a hexadecimal string.
    /// </summary>
    /// <param name="value">Integer value to format.</param>
    /// <returns>The formatted integer value.</returns>
    /// <remarks>
    /// This will return a string with the number formatted as a hexadecimal number. This number will be padded with zeroes that represent the size of the type. For example:
    /// <code language="csharp">
    /// byte hexValue = 15;
    /// 
    /// Console.WriteLine(hexValue.FormatHex()); // Produces "0F"
    /// 
    /// int hexValueInt = 127;
    /// 
    /// Console.WriteLine(hexValueInt.FormatHex()); // Produces "007F".
    /// 
    /// short hexValueShort = 1023;
    /// 
    /// Console.WriteLine(hexValueShort.FormatHex()); // Produces "03FF".
    /// </code>
    /// </remarks>
    public static string FormatHex(this int value) => value.ToString("x").PadLeft(8, '0');

    /// <summary>
    /// Function to format an unsigned integer value into a hexadecimal string.
    /// </summary>
    /// <param name="value">Unsigned integer value to format.</param>
    /// <returns>The formatted unsigned integer value.</returns>
    /// <remarks>
    /// This will return a string with the number formatted as a hexadecimal number. This number will be padded with zeroes that represent the size of the type. For example:
    /// <code language="csharp">
    /// byte hexValue = 15;
    /// 
    /// Console.WriteLine(hexValue.FormatHex()); // Produces "0F"
    /// 
    /// int hexValueInt = 127;
    /// 
    /// Console.WriteLine(hexValueInt.FormatHex()); // Produces "007F".
    /// 
    /// short hexValueShort = 1023;
    /// 
    /// Console.WriteLine(hexValueShort.FormatHex()); // Produces "03FF".
    /// </code>
    /// </remarks>
    public static string FormatHex(this uint value) => value.ToString("x").PadLeft(8, '0');

    /// <summary>
    /// Function to format a long value into a hexadecimal string.
    /// </summary>
    /// <param name="value">Long value to format.</param>
    /// <returns>The formatted long value.</returns>
    /// <remarks>
    /// This will return a string with the number formatted as a hexadecimal number. This number will be padded with zeroes that represent the size of the type. For example:
    /// <code language="csharp">
    /// byte hexValue = 15;
    /// 
    /// Console.WriteLine(hexValue.FormatHex()); // Produces "0F"
    /// 
    /// int hexValueInt = 127;
    /// 
    /// Console.WriteLine(hexValueInt.FormatHex()); // Produces "007F".
    /// 
    /// short hexValueShort = 1023;
    /// 
    /// Console.WriteLine(hexValueShort.FormatHex()); // Produces "03FF".
    /// </code>
    /// </remarks>
    public static string FormatHex(this long value) => value.ToString("x").PadLeft(16, '0');

    /// <summary>
    /// Function to format an unsigned long value into a hexadecimal string.
    /// </summary>
    /// <param name="value">Unsigned long value to format.</param>
    /// <returns>The formatted unsigned long value.</returns>
    /// <remarks>
    /// This will return a string with the number formatted as a hexadecimal number. This number will be padded with zeroes that represent the size of the type. For example:
    /// <code language="csharp">
    /// byte hexValue = 15;
    /// 
    /// Console.WriteLine(hexValue.FormatHex()); // Produces "0F"
    /// 
    /// int hexValueInt = 127;
    /// 
    /// Console.WriteLine(hexValueInt.FormatHex()); // Produces "007F".
    /// 
    /// short hexValueShort = 1023;
    /// 
    /// Console.WriteLine(hexValueShort.FormatHex()); // Produces "03FF".
    /// </code>
    /// </remarks>
    public static string FormatHex(this ulong value) => value.ToString("x").PadLeft(16, '0');

    /// <summary>
    /// Function to format a pointer (nint) value into a hexadecimal string.
    /// </summary>
    /// <param name="pointer">Pointer to format.</param>
    /// <returns>The formatted address of the pointer.</returns>
    /// <remarks>
    /// This will return a string with the number formatted as a hexadecimal number. Like other overloads of this method, this will pad zeroes to the left of the value based on the size of the type, but unlike the 
    /// other overloads, it will use the correct number of zeroes based on the platform (x64, x86).  For example:
    /// <code language="csharp">
    /// nint hexValue = 122388812;
    /// 
    /// Console.WriteLine(hexValue.FormatHex()); // Produces "074B814C" for x86, and "00000000074B814C" for x64
    /// </code>
    /// </remarks>
    public static string FormatHex(this nint pointer) => Environment.Is64BitProcess
                   ? ((long)pointer).ToString("x").PadLeft(16, '0')
                   : ((int)pointer).ToString("x").PadLeft(8, '0');

    /// <summary>
    /// Function to format a pointer (nuint) value into a hexadecimal string.
    /// </summary>
    /// <param name="pointer">Pointer to format.</param>
    /// <returns>The formatted address of the pointer.</returns>
    /// <remarks>
    /// This will return a string with the number formatted as a hexadecimal number. Like other overloads of this method, this will pad zeroes to the left of the value based on the size of the type, but unlike the 
    /// other overloads, it will use the correct number of zeroes based on the platform (x64, x86).  For example:
    /// <code language="csharp">
    /// nint hexValue = 122388812;
    /// 
    /// Console.WriteLine(hexValue.FormatHex()); // Produces "074B814C" for x86, and "00000000074B814C" for x64
    /// </code>
    /// </remarks>
    public static string FormatHex(this nuint pointer) => Environment.Is64BitProcess
                   ? ((long)pointer).ToString("x").PadLeft(16, '0')
                   : ((int)pointer).ToString("x").PadLeft(8, '0');
    #endregion
}
