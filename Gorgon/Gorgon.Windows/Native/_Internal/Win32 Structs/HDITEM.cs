#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Wednesday, August 12, 2015 7:38:43 PM
// 
#endregion

using System.Runtime.InteropServices;
using System.Runtime.InteropServices.Marshalling;

// ReSharper disable InconsistentNaming
namespace Gorgon.Native;

/// <summary>
/// Header item structure.
/// </summary>
[StructLayout(LayoutKind.Sequential)]
internal struct HDITEM
{
    public uint mask;
    public int cxy;    
    public string pszText;
    public nint hbm;
    public int cchTextMax;
    public int fmt;
    public nint lParam;
    public int iImage;
    public int iOrder;
    public uint type;
    public nint pvFilter;
    public uint state;
}
