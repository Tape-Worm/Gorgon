
// 
// Gorgon
// Copyright (C) 2020 Michael Winsor
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
// Created: December 8, 2020 2:51:07 PM
// 

using XI = SharpDX.XInput;

namespace Gorgon.Input.XInput;

/// <summary>
/// Extension methods for XInput
/// </summary>
internal static class XInputExtensions
{
    // Mappings for the GUIDs -> user indices.
    private static readonly Guid _one = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1);
    private static readonly Guid _two = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2);
    private static readonly Guid _three = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 3);
    private static readonly Guid _four = new(0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 4);

    /// <summary>
    /// Function to convert an XInput user index into a GUID.
    /// </summary>
    /// <param name="userindex">The user index to convert.</param>
    /// <returns>The guid associated with the user index.</returns>
    public static Guid ToGuid(this XI.UserIndex userindex) => userindex switch
    {
        XI.UserIndex.One => _one,
        XI.UserIndex.Two => _two,
        XI.UserIndex.Three => _three,
        XI.UserIndex.Four => _four,
        _ => Guid.Empty,
    };

    /// <summary>
    /// Function to conver a GUID to an XInput user index.
    /// </summary>
    /// <param name="guid">The guid to evaluate.</param>
    /// <returns>The XInput user index.</returns>
    public static XI.UserIndex ToUserIndex(this Guid guid)
    {

        if (guid.Equals(_one))
        {
            return XI.UserIndex.One;
        }

        if (guid.Equals(_two))
        {
            return XI.UserIndex.Two;
        }

        if (guid.Equals(_three))
        {
            return XI.UserIndex.Three;
        }

        return guid.Equals(_four) ? XI.UserIndex.Four : XI.UserIndex.Any;

    }
}
