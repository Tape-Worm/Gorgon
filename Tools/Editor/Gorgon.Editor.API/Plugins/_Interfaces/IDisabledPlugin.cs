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
// Created: November 10, 2018 8:45:04 PM
// 
#endregion

namespace Gorgon.Editor.PlugIns
{
    /// <summary>
    /// The reason code for disabling the plug in.
    /// </summary>
    public enum DisabledReasonCode
    {
        /// <summary>
        /// User manually disabled plug in.
        /// </summary>
        User = 0,
        /// <summary>
        /// An error occurred when loading the plug in.
        /// </summary>
        Error = 1,
        /// <summary>
        /// A validation error occurred after the plug in was loaded.
        /// </summary>
        ValidationError = 2
    }

    /// <summary>
    /// A plug in that was disabled for a reason.
    /// </summary>
    public interface IDisabledPlugIn
    {
        /// <summary>
        /// Property to return a description that explains why a plug in was disabled.
        /// </summary>
        string Description
        {
            get;
        }

        /// <summary>
        /// Property to return the name of the disabled plug in.
        /// </summary>
        string PlugInName
        {
            get;
        }

        /// <summary>
        /// Property to return the code to indicate how the plug in was disabled.
        /// </summary>
        DisabledReasonCode ReasonCode
        {
            get;
        }

		/// <summary>
        /// Property to return the assembly path.
        /// </summary>
		string Path
        {
            get;
        }
    }
}