
// 
// Gorgon
// Copyright (C) 2018 Michael Winsor
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
// Created: April 8, 2018 1:27:04 PM
// 

using System.Collections;
using Gorgon.Native;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A list of outputs on a video adapter
/// </summary>
public sealed class GorgonVideoAdapterOutputList
    : IReadOnlyDictionary<string, IGorgonVideoOutputInfo>
{
    // The backing store for the list.
    private readonly IReadOnlyDictionary<string, IGorgonVideoOutputInfo> _outputs;

    /// <summary>
    /// Property to return whether the keys are case sensitive.
    /// </summary>
    public bool KeysAreCaseSensitive => false;

    /// <summary>Gets the number of elements in the collection.</summary>
    /// <returns>The number of elements in the collection. </returns>
    public int Count => _outputs.Count;

    /// <inheritdoc/>
    IEnumerable<string> IReadOnlyDictionary<string, IGorgonVideoOutputInfo>.Keys => _outputs.Keys;

    /// <inheritdoc/>
    IEnumerable<IGorgonVideoOutputInfo> IReadOnlyDictionary<string, IGorgonVideoOutputInfo>.Values => _outputs.Values;

    /// <summary>
    /// Property to return an item in the dictionary by its name.
    /// </summary>
    public IGorgonVideoOutputInfo this[string name] => _outputs[name];

    /// <summary>
    /// Function to return the correct output where the majority of a window resides.
    /// </summary>
    /// <param name="windowHandle">The handle to the window to locate.</param>
    /// <returns>A <see cref="IGorgonVideoOutputInfo"/> that contains the majority of the window, or <b>null</b> if no output could be determined.</returns>
    public IGorgonVideoOutputInfo GetOutputFromWindowHandle(nint windowHandle)
    {
        if ((windowHandle == IntPtr.Zero)
            || (Count == 0))
        {
            return null;
        }

        nint monitor = Win32API.MonitorFromWindow(windowHandle, MonitorFlags.MONITOR_DEFAULTTONEAREST);

        return monitor == IntPtr.Zero ? null : _outputs.FirstOrDefault(item => item.Value.MonitorHandle == monitor).Value;
    }

    /// <summary>
    /// Function to return whether an item with the specified name exists in this collection.
    /// </summary>
    /// <param name="name">Name of the item to find.</param>
    /// <returns><b>true</b>if found, <b>false</b> if not.</returns>
    public bool ContainsKey(string name) => _outputs.ContainsKey(name);

    /// <summary>
    /// Function to return an item from the collection.
    /// </summary>
    /// <param name="name">The name of the item to look up.</param>
    /// <param name="value">The item, if found, or the default value for the type if not.</param>
    /// <returns><b>true</b> if the item was found, <b>false</b> if not.</returns>
    public bool TryGetValue(string name, out IGorgonVideoOutputInfo value) => _outputs.TryGetValue(name, out value);

    /// <summary>Returns an enumerator that iterates through the collection.</summary>
    /// <returns>An enumerator that can be used to iterate through the collection.</returns>
    public IEnumerator<IGorgonVideoOutputInfo> GetEnumerator() => _outputs.Select(output => output.Value).GetEnumerator();

    /// <summary>Returns an enumerator that iterates through a collection.</summary>
    /// <returns>An <see cref="IEnumerator" /> object that can be used to iterate through the collection.</returns>
    IEnumerator IEnumerable.GetEnumerator() => _outputs.Select(output => output.Value).GetEnumerator();

    /// <inheritdocs/>
    IEnumerator<KeyValuePair<string, IGorgonVideoOutputInfo>> IEnumerable<KeyValuePair<string, IGorgonVideoOutputInfo>>.GetEnumerator()
    {
        foreach (KeyValuePair<string, IGorgonVideoOutputInfo> output in _outputs)
        {
            yield return output;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonVideoAdapterOutputList"/> class.
    /// </summary>
    internal GorgonVideoAdapterOutputList() => _outputs = new Dictionary<string, IGorgonVideoOutputInfo>();

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonVideoAdapterOutputList"/> class.
    /// </summary>
    /// <param name="outputs">The list of outputs to wrap.</param>
    internal GorgonVideoAdapterOutputList(IReadOnlyDictionary<string, IGorgonVideoOutputInfo> outputs) => _outputs = outputs;

}
