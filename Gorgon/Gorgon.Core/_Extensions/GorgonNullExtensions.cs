// 
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
// Created: September 4, 2018 9:49:20 AM
// 

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace Gorgon.Core;

/// <summary>
/// Extension methods for null checking on reference types and nullable value types.
/// </summary>
public static class GorgonNullExtensions
{
    extension([NotNullWhen(false)] object? value)
    {
        /// <summary>
        /// Determines whether the specified value is null.
        /// </summary>
        /// <returns><c>true</c> if the specified value is null; otherwise, <c>false</c>.</returns>
        /// <remarks>
        /// <para>
        /// This will check an object for <b>null</b> and <see cref="DBNull"/>.
        /// </para>
        /// </remarks>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool IsNull() => (value is null) || (value == DBNull.Value);
    }

    extension(object? value)
    {
        /// <summary>
        /// Function to check an object for <b>null</b> or <see cref="DBNull"/> and return a substitute value.
        /// </summary>
        /// <typeparam name="T">The type of value.</typeparam>
        /// <param name="substitutionValue">The value used to replace the return value if the original value is <b>null</b> or <see cref="DBNull"/>.</param>
        /// <returns>The original value if not <b>null</b> (or <see cref="DBNull"/>), or the <paramref name="substitutionValue"/> otherwise.</returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public T IfNull<T>(T substitutionValue) => !IsNull(value) ? (T)value : substitutionValue;

        /// <summary>
        /// Function to return the value as a nullable type.
        /// </summary>
        /// <typeparam name="T">The type of value to convert to, must be a value type.</typeparam>
        /// <returns>The value as a nullable value type.</returns>
        public T? AsNullable<T>()
            where T : struct
        {
            if ((value is null) || (value == DBNull.Value))
            {
                return null;
            }

            Type type = typeof(T);

            if ((type.IsEnum) && (type.IsEnumDefined(value)))
            {
                return (T)value;
            }

            return (T)Convert.ChangeType(value, type);
        }
    }
}