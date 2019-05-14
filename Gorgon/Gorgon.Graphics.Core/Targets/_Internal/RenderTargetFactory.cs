#region MIT
// 
// Gorgon.
// Copyright (C) 2019 Michael Winsor
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
// Created: May 14, 2019 11:11:34 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Diagnostics;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// A factory for creating/retrieving render targets for temporary use.
    /// </summary>
    /// <remarks>
    /// <para>
    /// During the lifecycle of an application, many render targets may be required. In most instances, creating a render target for a scene and disposing of it when done is all that is required. But, when 
    /// dealing with effects, shaders, etc... render targets may need to be created and released during a frame and doing this several times in a frame can be costly.
    /// </para>
    /// <para>
    /// This factory allows applications to "rent" a render target and return it when done with only an initial cost when creating a target for the first time. This way, temporary render targets can be reused 
    /// when needed.
    /// </para>
    /// <para>
    /// When the factory retrieves a target, it will check its internal pool and see if a render target already exists. If it exists, and is not in use, it will return the existing target. If the target 
    /// does not exist, or is being used elsewhere, then a new target is created and added to the pool.
    /// </para>
    /// <para>
    /// Targets retrieved by this factory must be returned when they are no longer needed, otherwise the purpose of the factory is defeated. 
    /// </para>
    /// </remarks>
    internal class RenderTargetFactory
        : IDisposable, IGorgonRenderTargetFactory
    {
        #region Variables.
        // The list of render targets handled by this factory.
        private readonly List<GorgonRenderTarget2DView> _renderTargets = new List<GorgonRenderTarget2DView>();
        // The list of preallocated default shader resource views.
        private readonly List<GorgonShaderResourceView> _srvs = new List<GorgonShaderResourceView>();
        // The list of indexes for previously rented targets.
        private readonly HashSet<GorgonRenderTarget2DView> _rented = new HashSet<GorgonRenderTarget2DView>();
        // The graphics interface that owns this factory.
        private readonly GorgonGraphics _graphics;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to rent a render target from the factory.
        /// </summary>
        /// <param name="targetInfo">The information about the render target to retrieve.</param>
        /// <param name="name">[Optional] A user defined name for a new render target.</param>
        /// <param name="clearOnRetrieve">[Optional] <b>true</b> to clear the render target when retrieved, or <b>false</b> to leave the contents as-is.</param>
        /// <returns>The requested render target.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="targetInfo"/> parameter is <b>null</b>.</exception>
        /// <remarks>
        /// <para>
        /// All calls to this method should be paired with a call to the <see cref="Return"/> method.  Failure to do so may result in a leak.
        /// </para>
        /// <para>
        /// The optional <paramref name="clearOnRetrieve"/> parameter, if set to <b>true</b>,, will clear the contents of a render target that is being reused 
        /// prior to returning it.  In some cases this is not ideal, so setting it to <b>false</b> will preserve the contents.  New render targets will always 
        /// be cleared.
        /// </para>
        /// <note type="caution">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </remarks>
        public GorgonRenderTarget2DView Rent(IGorgonTexture2DInfo targetInfo, string name = null, bool clearOnRetrieve = true)
        {
            targetInfo.ValidateObject(nameof(targetInfo));

            // Ensure the information is valid.
            targetInfo = new GorgonTexture2DInfo(targetInfo, string.IsNullOrWhiteSpace(name) ? $"TempTarget_{_srvs.Count}_{targetInfo.Name}" : name)
            {
                Binding = targetInfo.Binding | TextureBinding.RenderTarget,
                Usage = ResourceUsage.Default
            };

            for (int i = 0; i < _renderTargets.Count; ++i)
            {
                GorgonRenderTarget2DView rtv = _renderTargets[i];
                GorgonTexture2D target = _renderTargets[i].Texture;

                if ((!_rented.Contains(rtv)) && (target.Width == targetInfo.Width) && (target.Height == targetInfo.Height) && (target.MipLevels == targetInfo.MipLevels)
                    && (target.ArrayCount == targetInfo.ArrayCount) && (target.Format == targetInfo.Format) && (target.Binding == targetInfo.Binding)
                    && (target.MultisampleInfo.Equals(targetInfo.MultisampleInfo)) && (targetInfo.IsCubeMap == target.IsCubeMap))
                {
                    if (clearOnRetrieve)
                    {
                        rtv.Clear(GorgonColor.BlackTransparent);
                    }

                    _renderTargets.Remove(rtv);
                    _rented.Add(rtv);
                    return rtv;
                }
            }

            var newRtv = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, targetInfo);
            // Cache a default shader resource view (the texture holds the cache, we hold a separate one so we can clean it up later).
            _srvs.Add(newRtv.GetShaderResourceView());
            newRtv.OwnerFactory = this;
            _rented.Add(newRtv);
            newRtv.Clear(GorgonColor.BlackTransparent);            
            return newRtv;
        }

        /// <summary>
        /// Function to return the render target to the factory.
        /// </summary>
        /// <param name="rtv">The render target to return.</param>
        /// <returns><b>true</b> if the target was returned successfully, <b>false</b> if not.</returns>
        public bool Return(GorgonRenderTarget2DView rtv)
        {
            if ((rtv == null) || (rtv.OwnerFactory != this))
            {
                return false;
            }

            if (!_rented.Remove(rtv))
            {
                return false;
            }

            _renderTargets.Add(rtv);
            return true;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            foreach (GorgonShaderResourceView srv in _srvs)
            {
                srv.Dispose();
            }

            foreach (GorgonRenderTarget2DView rtv in _renderTargets)
            {
                rtv.OwnerFactory = null;
                rtv.Dispose();
            }

            // For currently rented targets, detach from the factory so we don't disrupt anyting. This should not happen, but better safe than sorry.
            foreach (GorgonRenderTarget2DView rtv in _rented)
            {
                rtv.OwnerFactory = null;
            }

            _rented.Clear();
            _renderTargets.Clear();
        }
        #endregion

        #region Constructor/Finalizer.		
        /// <summary>Initializes a new instance of the <see cref="RenderTargetFactory"/> class.</summary>
        /// <param name="graphics">The graphics interface that owns this factory.</param>
        public RenderTargetFactory(GorgonGraphics graphics) => _graphics = graphics;
        #endregion
    }
}
