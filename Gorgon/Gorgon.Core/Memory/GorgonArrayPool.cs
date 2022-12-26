#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: January 25, 2021 12:55:54 PM
// 
#endregion

using System.Buffers;

namespace Gorgon.Memory;

/// <summary>
/// Class to return array pools with a specific maximum size for the arrays that are pooled.
/// </summary>
public static class GorgonArrayPool<T>
{
    #region Properties.
    /// <summary>
    /// Property to return an array pool with a maximum size of 1,048,576 items per array.
    /// </summary>
    /// <remarks>
    /// This is the same as using <see cref="ArrayPool{T}.Shared"/>.
    /// </remarks>
    public static ArrayPool<T> SharedTiny => ArrayPool<T>.Shared;

    /// <summary>
    /// Property to return an array pool with a maximum size of 16,777,216 items per array.
    /// </summary>
    public static ArrayPool<T> SharedSmall { get; } = ArrayPool<T>.Create(16_777_216, 50);

    /// <summary>
    /// Property to return an array pool with a maximum size of 67,108,864 items per array.
    /// </summary>
    public static ArrayPool<T> SharedMedium { get; } = ArrayPool<T>.Create(67_108_864, 25);

    /// <summary>
    /// Property to return an array pool with a maximum size of 134,217,728 items per array.
    /// </summary>
    public static ArrayPool<T> SharedLarge { get; } = ArrayPool<T>.Create(134_217_728, 11);

    /// <summary>
    /// Property to return an array pool with a maximum size of 1,073,741,824 items per array.
    /// </summary>
    public static ArrayPool<T> SharedHuge { get; } = ArrayPool<T>.Create(1_073_741_824, 5);
    #endregion

    #region Methods.
    /// <summary>
    /// Function to return the best suited pool based on the requested array size.
    /// </summary>
    /// <param name="size">The size of the requested array.</param>
    /// <returns>The best suited array pool.</returns>
    public static ArrayPool<T> GetBestPool(int size)
    {
#pragma warning disable IDE0046 // Convert to conditional expression
        if (size <= 1_048_576)
        {
            return SharedTiny;
        }

        if (size <= 16_777_216)
        {
            return SharedSmall;
        }

        if (size <= 67_108_864)
        {
            return SharedMedium;
        }

        return size <= 131_217_728 ? SharedLarge : SharedHuge;
#pragma warning restore IDE0046 // Convert to conditional expression

    }
    #endregion
}
