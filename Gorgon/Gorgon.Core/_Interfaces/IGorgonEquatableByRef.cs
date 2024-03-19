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
// Created: Sunday, January 27, 2013 9:41:28 AM
// 

namespace Gorgon.Core;

/// <summary>
/// A compliment for the <see cref="IEquatable{T}"/> type to allow passing a value type by reference for equality checks.
/// </summary>	
/// <typeparam name="T">The type to use for comparison.  Must be a value type.</typeparam>
/// <remarks>
/// <para>
/// This interface compliments the <see cref="IEquatable{T}"/> interface to use references to the value in the Equals parameter. Normally, the default of passing by value for a value type has performance 
/// implications when the type is larger than 16-24 bytes. By passing the value as a reference to a value type, we can use large value types when performing an equality check without hurting performance as 
/// much. 
/// </para>
/// <para>
/// This interface cannot be used on a reference type as it would give no advantage, and the standard <see cref="IEquatable{T}"/> would suffice.
/// </para>
/// </remarks>
public interface IGorgonEquatableByRef<T>
    : IEquatable<T>
    where T : struct    
{
    /// <summary>
    /// Function to compare this instance with another.
    /// </summary>
    /// <param name="other">The other instance to use for comparison.</param>
    /// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
    bool Equals(ref readonly T other);
}