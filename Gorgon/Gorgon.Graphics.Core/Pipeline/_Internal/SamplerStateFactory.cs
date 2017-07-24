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
// Created: July 8, 2017 7:11:38 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Diagnostics;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A factory used to create texture samplers.
    /// </summary>
    static class SamplerStateFactory
    {
        #region Variables.
        // The cached sampler states used for lookup.
        private static readonly List<(GorgonSamplerState Sampler, int Index)> _cachedSamplerStates;
        // Synchronization lock for multiple threads.
        private static readonly object _syncLock = new object();
        #endregion

        #region Methods.
        /// <summary>
        /// Function to clear the state cache.
        /// </summary>
        public static void ClearCache()
        {
            lock (_syncLock)
            {
                for (int i = 0; i < _cachedSamplerStates.Count; ++i)
                {
                    (GorgonSamplerState Sampler, int Index) samplerInfo = _cachedSamplerStates[i];
                    samplerInfo.Sampler.Native?.Dispose();
                    samplerInfo.Sampler.Native = null;
                }

                _cachedSamplerStates.Clear();
            }
        }

        /// <summary>
        /// Function to create or retrieve a texture sampler from the information passed.
        /// </summary>
        /// <param name="graphics">The graphics interface used to create the texture sampler.</param>
        /// <param name="info">The information used to retrieve the sample.</param>
        /// <param name="log">The log file used for debugging.</param>
        /// <returns>A new/existing D3D11 sampler state.</returns>
        public static D3D11.SamplerState GetSamplerState(GorgonGraphics graphics, GorgonSamplerState info, IGorgonLog log)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            lock (_syncLock)
            {
                // Look up an existing sampler.
                for (int i = 0; i < _cachedSamplerStates.Count; ++i)
                {
                    (GorgonSamplerState Sampler, int Index) samplerInfo = _cachedSamplerStates[i];

                    if ((samplerInfo.Sampler.Native != null) && (samplerInfo.Sampler.Equals(info)))
                    {
                        return samplerInfo.Sampler.Native;
                    }
                }

                // If we've made it this far, then we haven't cached this particular state yet.
                log.Print("Creating D3D11 texture sampler state...", LoggingLevel.Verbose);

                // Grab the native descriptor.
                D3D11.SamplerStateDescription desc = info.ToSamplerStateDesc();
                info.Native = new D3D11.SamplerState(graphics.VideoDevice.D3DDevice(), desc)
                              {
                                  DebugName = $"GorgonSamplerState #{_cachedSamplerStates.Count}: D3D11 Sampler State"
                              };
                _cachedSamplerStates.Add((info, _cachedSamplerStates.Count));

                return info.Native;
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes static members of the <see cref="SamplerStateFactory"/> class.
        /// </summary>
        static SamplerStateFactory()
        {
            _cachedSamplerStates = new List<(GorgonSamplerState, int)>(4096);
        }
        #endregion
    }
}
