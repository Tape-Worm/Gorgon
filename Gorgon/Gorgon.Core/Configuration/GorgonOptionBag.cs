#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: June 29, 2016 8:13:58 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Collections.Specialized;
using Gorgon.Properties;

namespace Gorgon.Configuration
{
    /// <summary>
    /// Provides a functionality for setting and reading various options from a predefined option bag.
    /// </summary>
    public sealed class GorgonOptionBag
        : GorgonNamedObjectList<IGorgonOption>, IGorgonOptionBag
    {
        #region Methods.
        /// <summary>
        /// Function to retrieve the value for an option.
        /// </summary>
        /// <typeparam name="T">The type of data for the option.</typeparam>
        /// <param name="optionName">The name of the option.</param>
        /// <returns>The value stored with the option.</returns>
        public T GetOptionValue<T>(string optionName) => GetItemByName(optionName).GetValue<T>();

        /// <summary>
        /// Function to assign a value for an option.
        /// </summary>
        /// <typeparam name="T">The type of data for the option.</typeparam>
        /// <param name="optionName">The name of the option.</param>
        /// <param name="value">The value to assign to the option.</param>
        public void SetOptionValue<T>(string optionName, T value)
        {
            if (!Contains(optionName))
            {
                throw new KeyNotFoundException();
            }

            GetItemByName(optionName).SetValue(value);
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonOptionBag"/> class.
        /// </summary>
        /// <param name="options">Values and types used to initialize the options.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="options"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown when the <paramref name="options"/> contains a value that is already in this option bag.</exception>
        public GorgonOptionBag(IEnumerable<IGorgonOption> options)
            : base(false)
        {
            if (options == null)
            {
                throw new ArgumentNullException(nameof(options));
            }

            foreach (IGorgonOption option in options)
            {
                // Don't allow duplicates.
                if ((Contains(option.Name))
                    || (Contains(option)))
                {
                    throw new ArgumentException(string.Format(Resources.GOR_ERR_OPTION_ALREADY_EXISTS, option.Name), nameof(options));
                }

                Add(option);
            }
        }
        #endregion
    }
}
