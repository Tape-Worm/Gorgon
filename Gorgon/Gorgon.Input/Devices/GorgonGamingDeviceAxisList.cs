
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Sunday, July 5, 2015 1:22:12 PM
// 

using System.Collections;
using System.Diagnostics.CodeAnalysis;

namespace Gorgon.Input.Devices;

/// <summary>
/// A list of axis data values for a gaming device
/// </summary>
public sealed class GorgonGamingDeviceAxisList
    : IReadOnlyList<IGorgonGamingDeviceAxis>
{
    // The list of available axes.
    private readonly List<GamingDeviceAxis> _axisList = [];
    // The list of information about each axis.
    private readonly Dictionary<GamingDeviceAxis, IGorgonGamingDeviceAxis> _infoList = [];

    /// <summary>
    /// Property to return the number of axes on the gaming device.
    /// </summary>
    public int Count => _axisList.Count;

    /// <summary>
    /// Property to return the range for the specified <see cref="GamingDeviceAxis"/>.
    /// </summary>
    public IGorgonGamingDeviceAxis this[GamingDeviceAxis axis] => _infoList[axis];

    /// <summary>
    /// Gets the <see cref="GorgonGamingDeviceAxisInfo"/> at the specified index in the axis list.
    /// </summary>
    public IGorgonGamingDeviceAxis this[int index]
    {
        get
        {
            ArgumentOutOfRangeException.ThrowIfLessThan(index, 0, nameof(index));
            ArgumentOutOfRangeException.ThrowIfGreaterThanOrEqual(index, _axisList.Count, nameof(index));

            return _infoList[_axisList[index]];
        }
    }

    /// <summary>
    /// Function to add an axis to the list.
    /// </summary>
    /// <param name="axisData">The data to add for the axis.</param>
    internal void Add(IGorgonGamingDeviceAxis axisData)
    {
        if (_infoList.ContainsKey(axisData.Axis))
        {
            return;
        }

        _axisList.Add(axisData.Axis);
        _infoList[axisData.Axis] = axisData;
    }

    /// <summary>
    /// Returns an enumerator that iterates through the collection.
    /// </summary>
    /// <returns>
    /// An enumerator that can be used to iterate through the collection.
    /// </returns>
    /// <filterpriority>1</filterpriority>
    public IEnumerator<IGorgonGamingDeviceAxis> GetEnumerator() => _infoList.Select(item => item.Value).GetEnumerator();

    /// <summary>
    /// Returns an enumerator that iterates through a collection.
    /// </summary>
    /// <returns>
    /// An <see cref="IEnumerator"/> object that can be used to iterate through the collection.
    /// </returns>
    /// <filterpriority>2</filterpriority>
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_infoList.Values).GetEnumerator();

    /// <summary>
    /// Function to determine if a specific <see cref="GamingDeviceAxis"/> is supported.
    /// </summary>
    /// <param name="axis">Axis to look up.</param>
    /// <returns><b>true</b> if the axis is supported, <b>false</b> if not.</returns>
    public bool Contains(GamingDeviceAxis axis) => _infoList.ContainsKey(axis);

    /// <summary>
    /// Function to retrieve a <see cref="GorgonGamingDeviceAxisInfo"/>.
    /// </summary>
    /// <param name="axis">The axis to look up.</param>
    /// <param name="result">The <see cref="GorgonGamingDeviceAxisInfo"/> specified by the axis.</param>
    /// <returns><b>true</b> if the axis was found in this list, or <b>false</b> if not.</returns>
    /// <remarks>
    /// If the <paramref name="axis"/> was not found, then the <paramref name="result"/> parameter will return a <see cref="GorgonGamingDeviceAxisInfo"/> with default values. Because of this, it is strongly 
    /// recommened to use the method return value to determine if the item exists or not.
    /// </remarks>    
    public bool TryGetValue(GamingDeviceAxis axis, [NotNullWhen(true)] out IGorgonGamingDeviceAxis? result) => _infoList.TryGetValue(axis, out result);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGamingDeviceAxisList" /> class.
    /// </summary>
    /// <param name="data">A list of items to add to the collection.</param>
    internal GorgonGamingDeviceAxisList(IEnumerable<IGorgonGamingDeviceAxis> data)
    {
        foreach (IGorgonGamingDeviceAxis item in data)
        {
            Add(item);
        }
    }
}
