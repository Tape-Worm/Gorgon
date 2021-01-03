#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: July 25, 2017 9:36:23 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Defines the layout of an input item within a buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This defines the layout of a piece of data within a stream out buffer. The layout is defined by a list of <see cref="GorgonStreamOutElement"/> values that determine 
    /// how the data within the layout is arranged.
    /// </para>
    /// </remarks>
    public sealed class GorgonStreamOutLayout
        : GorgonNamedObject
    {
        #region Variables.
        // Elements used to build the layout.
        private readonly GorgonStreamOutElement[] _elements;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the native elements for the stream output.
        /// </summary>
        internal D3D11.StreamOutputElement[] Native
        {
            get;
        }

        /// <summary>
        /// Property to return the input elements for this layout.
        /// </summary>
        public IReadOnlyList<GorgonStreamOutElement> Elements => _elements;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to determine if an element already exists with the same context, index and slot.
        /// </summary>
        /// <param name="elements">The list of elements to compare against.</param>
        /// <param name="element">The element to search for.</param>
        /// <param name="index">The index of the current element.</param>
        /// <param name="parameterName">The name of the parameter being validated.</param>
        private static void FindDuplicateElements(IList<GorgonStreamOutElement> elements, in GorgonStreamOutElement element, int index, string parameterName)
        {
            for (int i = 0; i < elements.Count; ++i)
            {
                // Skip the element that we're testing against.
                if (index == i)
                {
                    continue;
                }

                GorgonStreamOutElement currentElement = elements[i];

                if (GorgonStreamOutElement.Equals(in currentElement, in element))
                {
                    throw new ArgumentException(string.Format(Resources.GORGFX_ERR_LAYOUT_ELEMENT_IN_USE, i, element.Context), parameterName);
                }
            }
        }

        /// <summary>Indicates whether the current object is equal to another object of the same type.</summary>
        /// <returns>true if the current object is equal to the <paramref name="layout" /> parameter; otherwise, false.</returns>
        /// <param name="layout">An object to compare with this object.</param>
        public bool Equals(GorgonStreamOutLayout layout)
        {
            if (layout?._elements.Length != _elements.Length)
            {
                return false;
            }

            for (int i = 0; i < _elements.Length; ++i)
            {
                if (!GorgonStreamOutElement.Equals(in _elements[i], in layout._elements[i]))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonStreamOutLayout"/> class.
        /// </summary>
        /// <param name="name">Name of the object.</param>
        /// <param name="elements">The input elements to assign to this layout.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, or the <paramref name="elements"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/>, or the <paramref name="elements"/> parameter is empty.</exception>
        /// <exception cref="ArgumentException">Thrown when an element with the same context, slot and index appears more than once in the <paramref name="elements"/> parameter.</exception>
        /// <remarks>
        /// <para>
        /// The <paramref name="elements"/> list can contain up to 64 elements. If the list is longer than 64 elements, then it will be truncated to 64.
        /// </para>
        /// </remarks>
        public GorgonStreamOutLayout(string name, IEnumerable<GorgonStreamOutElement> elements)
            : base(name)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentEmptyException(nameof(name));
            }

            Name = name;

            // Make a copy so we don't allow changing of the original reference.
            _elements = elements?.Take(64).ToArray() ?? throw new ArgumentNullException(nameof(elements));

            if (_elements.Length == 0)
            {
                throw new ArgumentEmptyException(nameof(elements));
            }

            // Check for duplicated elements.
            for (int i = 0; i < _elements.Length; ++i)
            {
                FindDuplicateElements(_elements, _elements[i], i, nameof(elements));
            }

            Native = new D3D11.StreamOutputElement[_elements.Length];
            for (int i = 0; i < _elements.Length; ++i)
            {
                Native[i] = _elements[i].NativeElement;
            }
        }
        #endregion
    }
}
