#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Monday, October 14, 2013 6:18:12 PM
// 
#endregion

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Resources;
using System.Threading;

namespace Gorgon.Design
{
    /// <summary>
    /// A localizable version of the display name attribute.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property)]
    public class LocalDisplayNameAttribute
        : DisplayNameAttribute
    {
        #region Methods.
        /// <summary>
        /// Function to retrieve the resource string.
        /// </summary>
        /// <param name="resourcesType">Type of resource object.</param>
        /// <param name="resourceName">Name of the resource object.</param>
        /// <returns>The string value for the resource object.</returns>
        private static string GetString(Type resourcesType, string resourceName)
        {
            Debug.Assert(resourcesType.FullName != null, nameof(resourcesType) + ".FullName = null");
            var manager = new ResourceManager(resourcesType.FullName, resourcesType.Assembly);
            return manager.GetString(resourceName, Thread.CurrentThread.CurrentUICulture);
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="LocalDisplayNameAttribute"/> class.
        /// </summary>
        /// <param name="resourcesType">The type of resources for the assembly.</param>
        /// <param name="resourceName">Name of the resource to look up.</param>
        public LocalDisplayNameAttribute(Type resourcesType, [Localizable(false)] string resourceName)
            : base(GetString(resourcesType, resourceName))
        {
        }
        #endregion
    }
}
