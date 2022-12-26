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
// Created: Tuesday, June 14, 2011 8:58:43 PM
// 
#endregion

using System.Collections.Generic;

namespace Gorgon.Core;

/// <summary>
/// Gives an arbitrary object type a name to be used for lookup or another things that require an identifier.
/// </summary>
/// <remarks> 
/// <para>
/// Many objects require an ID to give uniqueness to that object. This could be necessary for lookup in a <see cref="IDictionary{TKey,TValue}"/>, or merely for logging purposes. This interface 
/// will ensure that items that need a textual representation of the object have a <see cref="Name"/> they can use for that purpose. 
/// </para>
/// </remarks>
public interface IGorgonNamedObject
{
    /// <summary>
    /// Property to return the name of this object.
    /// </summary>
    /// <remarks>
    /// For best practice, the name should only be set once during the lifetime of an object. Hence, this interface only provides a read-only implementation of this 
    /// property.
    /// </remarks>
    string Name
    {
        get;
    }
}
