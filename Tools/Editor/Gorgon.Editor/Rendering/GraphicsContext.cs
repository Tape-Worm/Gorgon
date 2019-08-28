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
// Created: August 26, 2018 6:59:36 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Renderers;

namespace Gorgon.Editor.Rendering
{
    /// <summary>
    /// A graphics context containing the current graphics interface and renderer.
    /// </summary>
    internal class GraphicsContext
        : IGraphicsContext, IDisposable
    {
        #region Variables.
        // Leases for a swap chain.
        private readonly Dictionary<string, WeakReference<GorgonSwapChain>> _swapChainLeases = new Dictionary<string, WeakReference<GorgonSwapChain>>(StringComparer.OrdinalIgnoreCase);
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return information about the video adapter selected.
        /// </summary>
        public IGorgonVideoAdapterInfo VideoAdapter => Graphics.VideoAdapter;

        /// <summary>
        /// Property to return the graphics interface for the application.
        /// </summary>
        public GorgonGraphics Graphics
        {
            get;
        }

        /// <summary>Property to return the factory used to create fonts.</summary>
        public GorgonFontFactory FontFactory
        {
            get;
        }

        /// <summary>
        /// Property to return the 2D renderer for the application.
        /// </summary>
        public Gorgon2D Renderer2D
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to return a leased out swap chain.
        /// </summary>
        /// <param name="swapChain">The swap chain to return.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="swapChain"/> parameter is <b>null</b>.</exception>
        public void ReturnSwapPresenter(ref GorgonSwapChain swapChain)
        {
            if (swapChain == null)
            {
                throw new ArgumentNullException(nameof(swapChain));
            }

            // This swap chain does not exist.
            if (swapChain.Window.IsDisposed)
            {
                return;
            }

            // This swap chain is not cached here, so do nothing.
            if (!_swapChainLeases.TryGetValue(swapChain.Window.Name, out WeakReference<GorgonSwapChain> cachedSwap))
            {
                return;
            }

            if (!cachedSwap.TryGetTarget(out swapChain))
            {
                return;
            }

            _swapChainLeases.Remove(swapChain.Window.Name);
            swapChain.Dispose();
        }

        /// <summary>
        /// Function to retrieve the swap chain for a specific control.
        /// </summary>
        /// <param name="control">The control that will be bound to the swap chain.</param>
        /// <returns>A new swap chain bound to the control.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="control"/> parameter is <b>null</b>.</exception>
        public GorgonSwapChain LeaseSwapPresenter(Control control)
        {
            if (control == null)
            {
                throw new ArgumentNullException(nameof(control));
            }

            // Check for swap chains that are no longer connected to their controls.
            (string name, WeakReference<GorgonSwapChain> swap)[] expiredSwaps = _swapChainLeases.Where(item => (!item.Value.TryGetTarget(out GorgonSwapChain swap))
                                                                                                               || (swap.Window.IsDisposed))
                                                                                                .Select(item => (item.Key, item.Value))
                                                                                                .ToArray();

            // Destroy the swap chain (if necessary, the controls should be doing this themselves).
            foreach ((string name, WeakReference<GorgonSwapChain> swapRef) in expiredSwaps)
            {
                if (swapRef.TryGetTarget(out GorgonSwapChain swap))
                {
                    swap.Dispose();
                }

                if (_swapChainLeases.ContainsKey(name))
                {
                    _swapChainLeases.Remove(name);
                }
            }

            if (_swapChainLeases.TryGetValue(control.Name, out WeakReference<GorgonSwapChain> swapLeaseRef))
            {
                if (swapLeaseRef.TryGetTarget(out GorgonSwapChain swap))
                {
                    return swap;
                }

                // The swap chain is dead, get rid of it and recreate.
                _swapChainLeases.Remove(control.Name);
            }

            var resultSwap = new GorgonSwapChain(Graphics, control,
                                                             new GorgonSwapChainInfo($"{control.Name} Swap Chain")
                                                             {
                                                                 Width = control.ClientSize.Width,
                                                                 Height = control.ClientSize.Height,
                                                                 Format = BufferFormat.R8G8B8A8_UNorm,
                                                                 UseFlipMode = true
                                                             });

            _swapChainLeases[control.Name] = new WeakReference<GorgonSwapChain>(resultSwap);

            return resultSwap;
        }

        /// <summary>
        /// Function to create the graphics context.
        /// </summary>
        /// <param name="log">The log interface.</param>
        /// <returns>A new graphics context object.</returns>
        /// <exception cref="GorgonException">Thrown when no suitable video device could be found on the system.</exception>
        public static GraphicsContext Create(IGorgonLog log)
        {
            IReadOnlyList<IGorgonVideoAdapterInfo> adapters = GorgonGraphics.EnumerateAdapters(log: log);

            if (adapters.Count == 0)
            {
                throw new GorgonException(GorgonResult.CannotCreate, Resources.GOREDIT_ERR_NO_SUITABLE_ADAPTER);
            }

            // Choose the adapter with the highest feature level.
            IGorgonVideoAdapterInfo adapter = adapters.OrderByDescending(item => item.FeatureSet).First();

            var graphics = new GorgonGraphics(adapter, log: log);
            var fontFactory = new GorgonFontFactory(graphics);

            return new GraphicsContext(graphics, fontFactory);
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            foreach (KeyValuePair<string, WeakReference<GorgonSwapChain>> swaps in _swapChainLeases)
            {
                if (swaps.Value.TryGetTarget(out GorgonSwapChain swap))
                {
                    swap.Dispose();
                }
            }

            FontFactory.Dispose();
            _swapChainLeases.Clear();
            Renderer2D?.Dispose();
            Graphics?.Dispose();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GraphicsContext"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface.</param>
        /// <param name="fontFactory">The font factory for the graphics context.</param>
        private GraphicsContext(GorgonGraphics graphics, GorgonFontFactory fontFactory)
        {
            Graphics = graphics;
            FontFactory = fontFactory;
            Renderer2D = new Gorgon2D(graphics);
        }
        #endregion

    }
}
