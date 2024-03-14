
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
// Created: Wednesday, August 12, 2015 9:29:01 PM
// 


using Gorgon.Core;

namespace Gorgon.Input;

/// <summary>
/// A representation of a gaming device axis
/// </summary>
/// <remarks>
/// This will provide the current value for the axis that it represents, as well as its dead zone range. Users may set either of these values as required
/// </remarks>
internal class GamingDeviceAxisProperties
    : IGorgonGamingDeviceAxis
{

    // The value for the axis.
    private int _value;
    // The default value for the axis.
    private readonly int _defaultValue;



    /// <summary>
    /// Property to return the identifier for the axis.
    /// </summary>
    public GamingDeviceAxis Axis
    {
        get;
    }

    /// <summary>
    /// Property to set or return the value representing the position of a gaming device axis.
    /// </summary>
    /// <remarks>
    /// When a <see cref="DeadZone"/> is applied to the axis, the value will remain at its <see cref="GorgonGamingDeviceAxisInfo.DefaultValue"/> until it exits the dead zone range.
    /// </remarks>
    public int Value
    {
        get => DeadZone.Equals(GorgonRange.Empty) || !DeadZone.Contains(_value) ? _value : _defaultValue;
        set => _value = value;
    }

    /// <summary>
    /// Property to set or return the dead zone value for the axis.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This will apply a dead zone area for the axis. A dead zone is used to limit the sensitivity of a gaming device to ensure that the device does not register movement until a certain threshold 
    /// is exceeded. This is necessary as a slight movement of the gaming device may cause undesired actions.
    /// </para>
    /// <para>
    /// This value should be within the axis range returned by the <see cref="GorgonGamingDeviceAxisInfo.Range"/> property. If it exceeds the axis range, then no movement will be registered.
    /// </para>
    /// <para>
    /// If the <see cref="Value"/> for the axis position is within this dead zone range, the <see cref="Value"/> property will return its <see cref="GorgonGamingDeviceAxisInfo.DefaultValue"/> 
    /// until it exceeds the dead zone threshold. 
    /// </para>
    /// <para>
    /// Specify <see cref="GorgonRange.Empty"/> to disable the dead zone on the axis.
    /// </para>
    /// </remarks>
    public GorgonRange DeadZone
    {
        get;
        set;
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="GamingDeviceAxisProperties"/> class.
    /// </summary>
    /// <param name="info">The information for the axis.</param>
    internal GamingDeviceAxisProperties(GorgonGamingDeviceAxisInfo info)
    {
        Axis = info.Axis;
        _defaultValue = info.DefaultValue;
    }

}
