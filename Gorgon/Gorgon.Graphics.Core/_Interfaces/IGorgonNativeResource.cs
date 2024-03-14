
// 
// Gorgon
// Copyright (C) 2021 Michael Winsor
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
// Created: February 11, 2021 11:38:43 PM
// 



namespace Gorgon.Graphics;

/// <summary>
/// Provides access to native pointers for resource objects (e.g. textures, devices, etc...)
/// </summary>
/// <remarks>
/// <para>
/// External APIs to Gorgon sometimes require access to the underlying texture objects, device objects, etc...  This interface provides those items as native pointers so that they can be wrapped 
/// directly by the external calls (including SharpDX)
/// </para>
/// </remarks>
public interface IGorgonNativeResource
{
    /// <summary>
    /// Property to return the native handle for the underlying resource object.
    /// </summary>
    /// <remarks>
    /// The property can be used to interoperate with functionality that require direct access to Direct 3D objects.
    /// </remarks>        
    nint Handle
    {
        get;
    }
}
