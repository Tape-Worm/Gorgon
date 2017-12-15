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
// Created: July 20, 2017 9:32:55 PM
// 
#endregion

using System.Collections.Generic;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A pipeline state grouping.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This allows an application to create groups of <see cref="GorgonPipelineState"/> objects so the states will not need to be created them over and over (and thus impair performance) unnecessarily.
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
    /// Typically, an application will register a cache group by using a unique name when calling the <see cref="GorgonGraphics.GetPipelineStateGroup"/> on the <see cref="GorgonGraphics"/> interface. That 
    /// method will return a container of this type and then an application can store its own pipeline cache by using a <see cref="long"/> value as a key to quickly retrieve an existing pipeline state for 
    /// use when rendering.
    /// </para>
    /// </remarks>
    public interface IGorgonPipelineStateGroup
        : IReadOnlyCollection<GorgonPipelineState>
    {
        #region Properties.
        /// <summary>
        /// Property to return the name of the group for these states.
        /// </summary>
        string GroupName
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to attempt a retrieval of a state by its key.
        /// </summary>
        /// <param name="key">The key to look up.</param>
        /// <param name="state">The state associated with the key.</param>
        /// <returns><b>true</b> if found, <b>false</b> if not.</returns>
        bool TryGetValue(long key, out GorgonPipelineState state);

        /// <summary>
        /// Function to cache a <see cref="GorgonPipelineState"/> with the specified key.
        /// </summary>
        /// <param name="key">The key for the state.</param>
        /// <param name="state">The state to cache.</param>
        /// <remarks>
        /// <para>
        /// <note type="important">
        /// <para>
        /// For performance considerations, any exceptions thrown from this method will only be thrown when Gorgon is compiled as <b>DEBUG</b>.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        void Cache(long key, GorgonPipelineState state);

        /// <summary>
        /// Function to invalidate the cache.
        /// </summary>
        void Invalidate();
        #endregion
    }
}
