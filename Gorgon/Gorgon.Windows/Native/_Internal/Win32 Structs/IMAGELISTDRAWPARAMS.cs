
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
// Created: August 29, 2018 8:01:34 PM
// 


using System.Runtime.InteropServices;
// ReSharper disable All

namespace Gorgon.Native;

[StructLayout(LayoutKind.Sequential)]
internal struct IMAGELISTDRAWPARAMS
{
    internal int cbSize;
    internal nint himl;
    internal int i;
    internal nint hdcDst;
    internal int x;
    internal int y;
    internal int cx;
    internal int cy;
    internal int xBitmap; // x offest from the upperleft of bitmap
    internal int yBitmap; // y offset from the upperleft of bitmap
    internal int rgbBk;
    internal int rgbFg;
    internal int fStyle;
    internal int dwRop;
    internal int fState;
    internal int Frame;
    internal int crEffect;
}
