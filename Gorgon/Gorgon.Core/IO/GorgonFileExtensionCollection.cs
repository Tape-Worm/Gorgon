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
// Created: Sunday, September 22, 2013 8:43:37 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Collections;
using Gorgon.Properties;

namespace Gorgon.IO
{
    /// <summary>
    /// A collection of file extensions.
    /// </summary>
    public class GorgonFileExtensionCollection
        : GorgonBaseNamedObjectDictionary<GorgonFileExtension>
    {
        #region Properties.
        /// <summary>
        /// Property to set or return an extension in the collection.
        /// </summary>
        public GorgonFileExtension this[string extension]
        {
            get
            {
                if (extension.StartsWith(".", StringComparison.Ordinal))
                {
                    extension = extension.Substring(1);
                }

                if (!Contains(extension))
                {
                    throw new KeyNotFoundException(string.Format(Resources.GOR_ERR_FILE_EXTENSION_NOT_FOUND, extension));
                }

                return Items[extension];
            }
            set
            {
                if (extension.StartsWith(".", StringComparison.Ordinal))
                {
                    extension = extension.Substring(1);
                }

                if (!Contains(extension))
                {
                    Add(value);
                    return;
                }

                UpdateItem(extension, value);
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to add a file extension to the collection.
        /// </summary>
        /// <param name="extension">Extension to add to the collection.</param>
        public void Add(GorgonFileExtension extension)
        {
            if (extension.Extension == null)
            {
                extension = new GorgonFileExtension(string.Empty, extension.Description);
            }

            if (Contains(extension))
            {
                throw new ArgumentException(string.Format(Resources.GOR_ERR_FILE_EXTENSION_EXISTS, extension.Extension));
            }

            Items.Add(extension.Extension, extension);
        }

        /// <summary>
        /// Function to remove a file extension from the collection.
        /// </summary>
        /// <param name="extension">The file extension to remove from the collection.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="extension"/> parameter is <b>null</b>.</exception>
        /// <exception cref="KeyNotFoundException">Thrown when the <paramref name="extension"/> could not be found in the collection.</exception>
        public void Remove(string extension)
        {
            if (extension == null)
            {
                throw new ArgumentNullException(nameof(extension));
            }

            if (extension.StartsWith(".", StringComparison.Ordinal))
            {
                extension = extension.Substring(1);
            }

            if (!Contains(extension))
            {
                throw new KeyNotFoundException(string.Format(Resources.GOR_ERR_FILE_EXTENSION_NOT_FOUND, extension));
            }

            Items.Remove(extension);
        }

        /// <summary>
        /// Function to remove a file extension from the collection.
        /// </summary>
        /// <param name="extension">The file extension to remove from the collection.</param>
        /// <exception cref="KeyNotFoundException">Thrown when the <paramref name="extension"/> could not be found in the collection.</exception>
        public void Remove(GorgonFileExtension extension)
        {
            if (!Contains(extension))
            {
                throw new KeyNotFoundException(string.Format(Resources.GOR_ERR_FILE_EXTENSION_NOT_FOUND, extension));
            }

            RemoveItem(extension);
        }

        /// <summary>
        /// Function to clear all items from the collection.
        /// </summary>
        public void Clear() => Items.Clear();
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonFileExtensionCollection"/> class.
        /// </summary>
        public GorgonFileExtensionCollection()
            : base(false)
        {
        }
        #endregion
    }
}
