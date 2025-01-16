// 
// Gorgon
// Copyright (C) 2016 Michael Winsor
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
// Created: August 16, 2016 4:40:09 PM
// 

using Gorgon.Core;

namespace Gorgon.Configuration;

/// <summary>
/// An option to be stored in a <see cref="IGorgonOptionBag"/>
/// </summary>
public interface IGorgonOption
    : IGorgonNamedObject
{
    /// <summary>
    /// Property to return the type of data stored in the option.
    /// </summary>
    Type Type
    {
        get;
    }

    /// <summary>
    /// Property to return text to display regarding this option.
    /// </summary>
    /// <remarks>
    /// This is pulled from the first line of the <see cref="Description"/>.
    /// </remarks>
    public string Text
    {
        get;
    }

    /// <summary>
    /// Property to return the friendly description of this option.
    /// </summary>
    string Description
    {
        get;
    }

    /// <summary>
    /// Function to retrieve the value stored in this option.
    /// </summary>
    /// <typeparam name="T">The type for the value.</typeparam>
    /// <returns>The value, strongly typed.</returns>
    T? GetValue<T>();

    /// <summary>
    /// Function to assign a value for the option.
    /// </summary>
    /// <typeparam name="T">The type parmeter for the value.</typeparam>
    /// <param name="value">The value to assign.</param>
    void SetValue<T>(T? value);

    /// <summary>
    /// Function to retrieve the default value for this option.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <returns>The value, strongly typed.</returns>
    T? GetDefaultValue<T>();

    /// <summary>
    /// Function to retrieve the minimum allowed value for this option.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <returns>The value, strongly typed.</returns>
    T? GetMinValue<T>();

    /// <summary>
    /// Function to retrieve the maximum allowed value for this option.
    /// </summary>
    /// <typeparam name="T">The type of the value.</typeparam>
    /// <returns>The value, strongly typed.</returns>
    T? GetMaxValue<T>();
}