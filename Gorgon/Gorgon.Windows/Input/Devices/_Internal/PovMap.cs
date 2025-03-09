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
// Created: February 10, 2025 7:51:17 PM
//

namespace Gorgon.Windows.Input.Devices;

/// <summary>
/// A structure used to map a point of view (POV) hat information.
/// </summary>
/// <param name="Index">The index of the POV hat.</param>
/// <param name="LogicalMin">The smallest value returned by the hat.</param>
/// <param name="LogicalMax">The largest value returned by the hat.</param>
/// <param name="PhysicalMin">The smallest physical value, in degrees.</param>
/// <param name="PhysicalMax">The largest physical value, in degrees.</param>
/// <param name="UnitDegree">The number of degrees for each logical unit.</param>
internal record class PovMap(ushort Index, int LogicalMin, int LogicalMax, int PhysicalMin, int PhysicalMax, int UnitDegree);
