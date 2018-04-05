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
// Created: July 20, 2017 5:33:51 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using Gorgon.Diagnostics;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// An application specific group of pipeline states.
    /// </summary>
    /// <typeparam name="TKey">The type used for the key describing a unique <see cref="GorgonPipelineState"/>.</typeparam>
    /// <remarks>
    /// <para>
    /// This will contain a list of <see cref="GorgonPipelineState"/> objects that are being kept for reuse by an application, or other functionality. 
    /// </para>
    /// <para>
    /// Gorgon stores each pipeline state created by an application in a main cache which is located on the <see cref="GorgonGraphics"/> interface, and is accessible using the 
    /// <see cref="GorgonGraphics.GetPipelineState"/> method and the <see cref="GorgonGraphics.CachedPipelineStates"/> property. These pipeline states are a list of all states across the entire application 
    /// and is not built for random access. Due to this, it is difficult to manage these states from another piece of code (this is by design). 
    /// </para>
    /// <para>
    /// Applications will typically call <see cref="GorgonGraphics.GetPipelineState"/> to retrieve a new pipeline state, or, if it has been cached, an existing one. However, even retrieving the cached 
    /// pipeline over and over in a tight loop (such as a render loop) is not considered best practice. To help keep cached states manageable by an application, Gorgon supports the concept of a cached 
    /// pipeline state group. This is completely application specific and is meant to allow applications to quickly retrieve pipeline states for their own use (although group sharing is not disallowed). 
    /// </para>
    /// <para>
    /// Typically, an application will register a cache group by using a unique name when calling the <see cref="GorgonGraphics.GetPipelineStateGroup{T}"/> on the <see cref="GorgonGraphics"/> interface. That 
    /// method will return a container of this type and then an application can store its own pipeline cache by using a key (defined by the <typeparamref name="TKey"/> type parameter) to quickly retrieve 
    /// an existing pipeline state for use when rendering.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonPipelineState"/>
    /// <seealso cref="GorgonGraphics"/>
    public class GorgonPipelineStateGroup<TKey>
        : IPipelineStateGroup
    {
        #region Variables.
        // The list of states for this group.
        private readonly Dictionary<TKey, GorgonPipelineState> _states = new Dictionary<TKey, GorgonPipelineState>();
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the name of the group for these states.
        /// </summary>
        public string GroupName
        {
            get;
        }

        /// <summary>
        /// Property to return the number of pipeline states in this group.
        /// </summary>
        public int Count => _states.Count;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to invalidate the cache.
        /// </summary>
        void IPipelineStateGroup.Invalidate()
        {
            _states.Clear();
        }

        /// <summary>
        /// Function to cache a <see cref="GorgonPipelineState"/> with the specified key.
        /// </summary>
        /// <param name="key">The key for the state.</param>
        /// <param name="state">The state to cache.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="state"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// <note type="important">
        /// <para>
        /// For performance considerations, any exceptions thrown from this method will only be thrown when Gorgon is compiled as <b>DEBUG</b>.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void Cache(TKey key, GorgonPipelineState state)
        {
            state.ValidateObject(nameof(state));

            _states[key] = state;
        }

        /// <summary>
        /// Function to attempt a retrieval of a state by its key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="state">The state associated with the key.</param>
        /// <returns><b>true</b> if found, <b>false</b> if not.</returns>
        public bool TryGetValue(TKey key, out GorgonPipelineState state) => _states.TryGetValue(key, out state);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<GorgonPipelineState> GetEnumerator()
        {
            foreach (KeyValuePair<TKey, GorgonPipelineState> state in _states)
            {
                yield return state.Value;
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through a collection.
        /// </summary>
        /// <returns>An <see cref="T:System.Collections.IEnumerator" /> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonPipelineStateGroup{T}"/> class.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        internal GorgonPipelineStateGroup(string groupName)
        {
            GroupName = groupName;
        }
        #endregion
    }
}
