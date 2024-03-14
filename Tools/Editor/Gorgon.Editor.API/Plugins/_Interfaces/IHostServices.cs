
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
// Created: January 22, 2020 10:28:50 PM
// 


using Gorgon.Diagnostics;
using Gorgon.Editor.Services;

namespace Gorgon.Editor.PlugIns;

/// <summary>
/// A list of services passed from the host application to plug ins
/// </summary>
public interface IHostServices
{
    /// <summary>
    /// Property to return the log for debug messages.
    /// </summary>
    IGorgonLog Log
    {
        get;
    }

    /// <summary>
    /// Property to return the serivce used to show busy states.
    /// </summary>
    IBusyStateService BusyService
    {
        get;
    }

    /// <summary>
    /// Property to return the service used to show message dialogs.
    /// </summary>
    IMessageDisplayService MessageDisplay
    {
        get;
    }

    /// <summary>
    /// Property to return the service used to send and retrieve data to and from the clipboard.
    /// </summary>
    IClipboardService ClipboardService
    {
        get;
    }
}
