// 
// Gorgon
// Copyright (C) 2015 Michael Winsor
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
// Created: Thursday, September 10, 2015 10:11:39 PM
// 

using System.Collections;

namespace Gorgon.Windows.Input.Devices;

/// <summary>
/// A list containing the current <see cref="KeyState"/> for each key in the <see cref="VirtualKeys"/> enumeration. 
/// </summary>
public class GorgonKeyStateCollection
    : IReadOnlyList<KeyState>
{
    // Keyboard key state.
    private readonly Dictionary<VirtualKeys, KeyState> _keyStates = new(new VirtualKeysEqualityComparer());
    // The individual key list.
    private readonly static VirtualKeys[] _virtualKeys = Enum.GetValues<VirtualKeys>();

    /// <summary>
    /// Gets the number of elements contained in the <see cref="ICollection{T}"></see>.
    /// </summary>
    /// <returns>The number of elements contained in the <see cref="ICollection{T}"></see>.</returns>
    public int Count => _keyStates.Count;

    /// <summary>
    /// Property to return the state of a given key, by index.
    /// </summary>
    public KeyState this[int index] => _keyStates[_virtualKeys[index]];

    /// <summary>
    /// Property to return the state of a given key.
    /// </summary>
    /// <param name="key">Key to check.</param>
    /// <returns>The state of the key.</returns>
    public KeyState this[VirtualKeys key]
    {
        get
        {
            if (!_keyStates.TryGetValue(key, out KeyState result))
            {
                _keyStates.Add(key, KeyState.Up);
            }

            return result;
        }
        set => _keyStates[key] = value;
    }

    /// <summary>
    /// Function to reset the key states.
    /// </summary>
    public void Reset()
    {
        foreach (VirtualKeys key in _virtualKeys)
        {
            this[key] = KeyState.Up;
        }
    }

    /// <summary>
    /// Function to reset any modifier keys.
    /// </summary>
    public void ResetModifiers()
    {
        this[VirtualKeys.Alt] = KeyState.Up;
        this[VirtualKeys.RightAlt] = KeyState.Up;
        this[VirtualKeys.LeftAlt] = KeyState.Up;
        this[VirtualKeys.Shift] = KeyState.Up;
        this[VirtualKeys.LeftShift] = KeyState.Up;
        this[VirtualKeys.RightShift] = KeyState.Up;
        this[VirtualKeys.Control] = KeyState.Up;
        this[VirtualKeys.RightControl] = KeyState.Up;
        this[VirtualKeys.LeftControl] = KeyState.Up;
        this[VirtualKeys.AltModifier] = KeyState.Up;
        this[VirtualKeys.ControlModifier] = KeyState.Up;
        this[VirtualKeys.ShiftModifier] = KeyState.Up;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// A <see cref="IEnumerator{T}"></see> that can be used to iterate through the collection.
    /// </returns>
    public IEnumerator<KeyState> GetEnumerator()
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (KeyValuePair<VirtualKeys, KeyState> state in _keyStates)
        {
            yield return state.Value;
        }
    }

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator" /> object that can be used to iterate through the collection.
    /// </returns>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonKeyStateCollection"/> class.
    /// </summary>
    internal GorgonKeyStateCollection()
    {
    }
}
