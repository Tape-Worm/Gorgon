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
// Created: November 6, 2017 5:56:30 PM
// 

namespace Gorgon.Graphics.Core;

/// <summary>
/// PCI slot information for a <see cref="GorgonVideoAdapterInfo"/> data structure.
/// </summary>
/// <param name="DeviceID">The PCI device ID for the adapter.</param>
/// <param name="Revision">The PCI ID revision number for the adapter</param>
/// <param name="SubSystemID">The PCI sub system ID for the adapter.</param>
/// <param name="VendorID">The PCI vendor ID for the adapter.</param>
public readonly record struct GorgonVideoAdapterPciInfo(int DeviceID, int Revision, int SubSystemID, int VendorID);
