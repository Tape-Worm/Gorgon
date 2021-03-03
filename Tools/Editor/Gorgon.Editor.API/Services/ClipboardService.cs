#region MIT
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
// Created: September 19, 2018 8:11:03 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Editor.Properties;

namespace Gorgon.Editor.Services
{
    /// <summary>
    /// The clipboard service used to assign or retrieve data from the internal clipboard.
    /// </summary>
    public class ClipboardService
        : IClipboardService
    {
        #region Variables.
        // The data stored on the clipboard.
        private object _data;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether or not there is data on the clipboard.
        /// </summary>
        public bool HasData => _data != null;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to return the data from the clipboard as the specified type.
        /// </summary>
        /// <typeparam name="T">The type of data to retrieve.</typeparam>
        /// <returns>The data as the specified type.</returns>
        /// <exception cref="GorgonException">Thrown if there is no data, or no data of type <typeparamref name="T"/> on the clipboard.</exception>
        /// <remarks>
        /// <para>
        /// If there is no data of the specified type on the clipboard, then this method will throw an exception. Check for data of the specified type using the <see cref="IsType{T}"/> method prior to 
        /// calling this method.
        /// </para>
        /// </remarks>
        public T GetData<T>()
        {
            Type type = typeof(T);

            if (_data is null)
            {
                throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_CLIPBOARD_NO_DATA_OF_TYPE, type.FullName));
            }

            Type objectType = _data.GetType();

            return (type != objectType)
                && (!type.IsInstanceOfType(_data))
                && (!type.IsAssignableFrom(objectType))
                && (!objectType.IsAssignableFrom(type))
                && (!type.IsSubclassOf(objectType))
                && (!objectType.IsSubclassOf(type))
                ? throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GOREDIT_ERR_CLIPBOARD_NO_DATA_OF_TYPE, type.FullName))
                : (T)_data;
        }

        /// <summary>
        /// Function to clear the clipboard contents.
        /// </summary>
        public void Clear() => _data = null;

        /// <summary>
        /// Function to return whether or not the data on the clipboard is of the type specified.
        /// </summary>
        /// <typeparam name="T">The type to check.</typeparam>
        /// <returns><b>true</b> if the data is of the type specified, <b>false</b> if not.</returns>
        /// <remarks>
        /// <para>
        /// If there is no data on the clipboard, then this will always return <b>false</b>.
        /// </para>
        /// </remarks>
        public bool IsType<T>()
        {
            Type type = typeof(T);

            if (_data is null)
            {
                return false;
            }

            Type objectType = _data.GetType();

            return ((type == objectType)
                || (type.IsInstanceOfType(_data))
                || (type.IsAssignableFrom(objectType))
                || (objectType.IsAssignableFrom(type))
                || (type.IsSubclassOf(objectType))
                || (objectType.IsSubclassOf(type)));
        }

        /// <summary>
        /// Function to place an item on the clipboard for copying.
        /// </summary>
        /// <param name="item">The item to copy.</param>
        public void CopyItem(object item)
        {
            if (item is null)
            {
                return;
            }

            _data = item;
        }
        #endregion
    }
}
