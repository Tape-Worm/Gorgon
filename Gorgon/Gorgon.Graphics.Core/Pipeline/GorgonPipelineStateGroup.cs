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
    /// A group of pipeline states.
    /// </summary>
    public class GorgonPipelineStateGroup
        : IReadOnlyCollection<GorgonPipelineState>
    {
        #region Variables.
        // The list of states for this group.
        private readonly Dictionary<long, GorgonPipelineState> _states = new Dictionary<long, GorgonPipelineState>();
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
        internal void Invalidate()
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
        public void Cache(long key, GorgonPipelineState state)
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
        public bool TryGetValue(long key, out GorgonPipelineState state) => _states.TryGetValue(key, out state);

        /// <summary>
        /// Returns an enumerator that iterates through the collection.
        /// </summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<GorgonPipelineState> GetEnumerator()
        {
            foreach (KeyValuePair<long, GorgonPipelineState> state in _states)
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
        /// Initializes a new instance of the <see cref="GorgonPipelineStateGroup"/> class.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        internal GorgonPipelineStateGroup(string groupName)
        {
            GroupName = groupName;
        }
        #endregion
    }
}
