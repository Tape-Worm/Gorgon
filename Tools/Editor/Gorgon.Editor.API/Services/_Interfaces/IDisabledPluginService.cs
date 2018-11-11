﻿#region MIT
// 
// Gorgon.
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
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: November 10, 2018 8:47:16 PM
// 
#endregion

using System.Collections.Generic;
using Gorgon.Editor.Plugins;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// Provides a list of plug ins that were disabled on application start up.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Use this interface to determine if a plug in is available for use or not.
    /// </para>
    /// </remarks>
    public interface IDisabledPluginService
    {
        /// <summary>
        /// Property to return the list of disabled plug ins.
        /// </summary>
        IReadOnlyDictionary<string, IDisabledPlugin> DisabledPlugins
        {
            get;
        }
    }
}
