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
// Created: June 29, 2016 8:13:58 PM
// 

using System.Collections;
using Gorgon.Core;
using Gorgon.Properties;

namespace Gorgon.Configuration;

/// <summary>
/// Provides a functionality for setting and reading various options from a predefined option bag
/// </summary>
public sealed class GorgonOptionBag
    : IGorgonOptionBag
{
    // The backing store for the options.
    private readonly List<IGorgonOption> _options = [];

    /// <inheritdoc/>
    public int Count => _options.Count;

    /// <inheritdoc/>
    public IGorgonOption this[int index] => _options[index];

    /// <inheritdoc/>
    public T? GetOptionValue<T>(string optionName) => _options.GetByName(optionName).GetValue<T>();

    /// <inheritdoc/>
    public void SetOptionValue<T>(string optionName, T? value)
    {
        if (!_options.ContainsName(optionName))
        {
            throw new KeyNotFoundException();
        }

        _options.GetByName(optionName).SetValue(value);
    }

    /// <inheritdoc/>
    public int IndexOf(IGorgonOption item) => _options.IndexOf(item);

    /// <inhertidoc/>
    public bool Contains(IGorgonOption item) => _options.Contains(item);

    /// <inheritdoc/>
    public IEnumerator<IGorgonOption> GetEnumerator() => _options.GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_options).GetEnumerator();

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonOptionBag"/> class.
    /// </summary>
    /// <param name="options">Values and types used to initialize the options.</param>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="options"/> contains a value that is already in this option bag.</exception>
    public GorgonOptionBag(IEnumerable<IGorgonOption> options)
    {
        foreach (IGorgonOption option in options)
        {
            // Don't allow duplicates.
            if ((this.ContainsName(option.Name))
                || (Contains(option)))
            {
                throw new ArgumentException(string.Format(Resources.GOR_ERR_OPTION_ALREADY_EXISTS, option.Name), nameof(options));
            }

            _options.Add(option);
        }
    }
}
