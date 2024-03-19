// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Wednesday, January 23, 2013 8:30:13 AM
// 

namespace Gorgon.Core;

/// <summary>
/// A type safe version of the <see cref="ICloneable"/> interface.
/// </summary>
/// <typeparam name="T">The type to clone.</typeparam>
/// <remarks>
/// <para>
/// The .NET framework provides us with a <see cref="ICloneable"/> interface for objects that can be cloned. However, it returns the cloned object as a <see cref="object"/> type. This type returns a strongly 
/// typed clone of the object instead.
/// </para>
/// </remarks>
public interface IGorgonCloneable<out T>
{
    /// <summary>
    /// Function to clone an object.
    /// </summary>
    /// <returns>The cloned object.</returns>
    T Clone();
}