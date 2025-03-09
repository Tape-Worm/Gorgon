
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
// Created: Wednesday, August 12, 2015 9:25:53 PM
// 

using Gorgon.Core;

namespace Gorgon.Windows.Input.Devices;

/// <summary>
/// Information about a gaming device axis
/// </summary>
/// <remarks>
/// <para>
/// This will provide information about the range of movement for the axis, as well as a default value for when the axis is centered or whatever the resting position may be. 
/// </para>
/// </remarks>
/// <param name="Axis">The identifier for the axis.</param>
/// <param name="Range">The range of the axis.</param>
/// <param name="DefaultValue">The default value for the axis when in resting position.</param>
/// <param name="AxisIndex">The index of the axis on the device.</param>
public record class GorgonGamingDeviceAxisInfo(GamingDeviceAxis Axis, GorgonRange<int> Range, int DefaultValue, ushort AxisIndex);