﻿// 
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
// Created: June 29, 2016 8:16:09 PM
// 

namespace Gorgon.Configuration;

/// <summary>
/// Provides a functionality for setting and reading various options from a defined option bag
/// </summary>
public interface IGorgonOptionBag
    : IEnumerable<IGorgonOption>
{
    /// <summary>
    /// Property to return the number of options in the bag.
    /// </summary>
    int Count
    {
        get;
    }

    /// <summary>
    /// Property to return the option at the specified index.
    /// </summary>
    IGorgonOption this[int index]
    {
        get;
    }

    /// <summary>
    /// Function to retrieve the index of an option in the bag.
    /// </summary>
    /// <param name="option">The option to evaluate.</param>
    /// <returns>The index of the option, or -1 if not found.</returns>
    int IndexOf(IGorgonOption option);

    /// <summary>
    /// Function to determine if an <see cref="IGorgonOption"/> exists in this bag.
    /// </summary>
    /// <param name="option">The option to evaluate.</param>
    /// <returns><b>true</b> if the option exists, or <b>false</b> if not.</returns>
    bool Contains(IGorgonOption option);

    /// <summary>
    /// Function to retrieve the value for an option.
    /// </summary>
    /// <typeparam name="T">The type of data for the option.</typeparam>
    /// <param name="optionName">The name of the option.</param>
    /// <returns>The value stored with the option.</returns>
    /// <exception cref="KeyNotFoundException">Thrown when the option specified by the <paramref name="optionName"/> was not found.</exception>
    T? GetOptionValue<T>(string optionName);

    /// <summary>
    /// Function to assign a value for an option.
    /// </summary>
    /// <typeparam name="T">The type of data for the option.</typeparam>
    /// <param name="optionName">The name of the option.</param>
    /// <param name="value">The value to assign to the option.</param>
    /// <exception cref="KeyNotFoundException">Thrown when the option specified by the <paramref name="optionName"/> was not found.</exception>
    void SetOptionValue<T>(string optionName, T? value);
}
