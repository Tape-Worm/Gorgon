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
// Created: May 24, 2018 11:03:35 PM
// 
#endregion

using Gorgon.Collections;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A grouping of states for a shader.
    /// </summary>
    public sealed class ShaderStates<T>
        where T : GorgonShader
    {
        #region Properties.
        /// <summary>
        /// Property to return the sampler states as a read/write property.
        /// </summary>
        internal GorgonSamplerStates RwSamplers
        {
            get; 
            set;
        } = new GorgonSamplerStates();

        /// <summary>
        /// Property to return the currently active shader.
        /// </summary>
        public T Current
        {
            get;
            internal set;
        }

        /// <summary>
        /// Property to return the samplers for the shader.
        /// </summary>
        public IGorgonReadOnlyArray<GorgonSamplerState> Samplers => RwSamplers;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to copy the shader states into another shader state.
        /// </summary>
        /// <param name="states">The states to copy.</param>
        internal void CopyTo(ShaderStates<T> states)
        {
            states.Current = Current;
            RwSamplers.CopyTo(states.RwSamplers);
        }

        /// <summary>
        /// Function to clear the state.
        /// </summary>
        internal void Clear()
        {
            Current = null;
            RwSamplers.Clear();
        }

        /// <summary>
        /// Function to update the sampler list with a single sampler.
        /// </summary>
        /// <param name="sampler">The sampler to assign.</param>
        /// <param name="slot">The slot for the sampler.</param>
        internal void UpdateSampler(GorgonSamplerState sampler, int slot)
        {
            if (RwSamplers[slot]?.Equals(sampler) ?? false)
            {
                return;
            }

            RwSamplers.Clear();
            RwSamplers[slot] = sampler;
        }

        /// <summary>
        /// Function to update the sampler list.
        /// </summary>
        /// <param name="samplers">The samplers to copy.</param>
        internal void UpdateSamplersByRef(GorgonSamplerStates samplers)
        {
            if (samplers == null)
            {
                RwSamplers.Clear();
                return;
            }

            RwSamplers = samplers;
        }

        /// <summary>
        /// Function to update the sampler list.
        /// </summary>
        /// <param name="samplers">The samplers to copy.</param>
        internal void UpdateSamplers(IGorgonReadOnlyArray<GorgonSamplerState> samplers)
        {
            if (samplers == null)
            {
                RwSamplers.Clear();
                return;
            }

            (int start, int count) = samplers.GetDirtyItems();
            
            for (int i = start; i < start + count; ++i)
            {
                if (RwSamplers[i]?.Equals(samplers[i]) ?? false)
                {
                    continue;
                }

                RwSamplers[i] = samplers[i];
            }
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderStates{T}"/> class.
        /// </summary>
        internal ShaderStates()
        {
            // We need to create this through a builder.
        }
        #endregion
    }
}
