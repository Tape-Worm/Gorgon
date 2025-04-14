// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: March 3, 2025 11:57:01 PM
//

using System.Numerics;
using Gorgon.Input.Devices;

namespace Gorgon.Examples;

/// <summary>
/// A record for controller information.
/// </summary>
/// <param name="Device">The input device for this controller.</param>
/// <param name="Spray">The spray interface for drawing.</param>
/// <remarks>
/// <para>
/// We use this record to link together the controller device, the spray can object and the position of the controller icon.
/// </para>
/// </remarks>
internal record class Controller(IGorgonGamingDevice Device, SprayCan Spray)
{
    /// <summary>
    /// Property to set or return the position of the controller icon.
    /// </summary>
    public Vector2 Position
    {
        get;
        set;
    }
}

