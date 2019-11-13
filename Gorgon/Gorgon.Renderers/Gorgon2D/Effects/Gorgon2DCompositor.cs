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
// Created: August 2, 2018 12:25:08 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using Gorgon.Collections.Specialized;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A compositor system used to chain multiple effects together.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This processor will take a scene and composite it using a series of effects (or plain rendering without effects).  The results of these effects will be passed on to the next effect and rendered
    /// until all effects are processed. The final image is then output to a render target specified by the user.
    /// </para>
    /// </remarks>
    public class Gorgon2DCompositor
        : IDisposable, IGorgonGraphicsObject, IReadOnlyList<IGorgon2DCompositorPass>
    {
        #region Variables.
        // The final output for the scene.
        private GorgonRenderTargetView _final;
        // The ping render target in the ping-pong target scheme.
        private GorgonRenderTarget2DView _pingTarget;
        // The pong render target in the ping-pong target scheme.
        private GorgonRenderTarget2DView _pongTarget;
        // The texture view for the ping target.
        private GorgonTexture2DView _pingTexture;
        // The texture view for the pong target.
        private GorgonTexture2DView _pongTexture;
        // The ordered list where the actual passes are stored.
        private readonly GorgonNamedObjectList<CompositionPass> _passes = new GorgonNamedObjectList<CompositionPass>();
        // The unique list of effects
        private readonly Dictionary<string, int> _passLookup = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);
        // The color used to clear the initial render target.
        private GorgonColor? _initialClear = GorgonColor.BlackTransparent;
        // The color used to clear the final render target.
        private GorgonColor? _finalClear = GorgonColor.BlackTransparent;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the graphics interface that built this object.
        /// </summary>
        public GorgonGraphics Graphics
        {
            get;
        }

        /// <summary>
        /// Property to return the renderer to use when rendering the scene.
        /// </summary>
        public Gorgon2D Renderer
        {
            get;
        }

        /// <summary>
        /// Property to return the number of passes.
        /// </summary>
        public int Count => _passes.Count;

        /// <summary>
        /// Property to return the passes registered within the compositor by index.
        /// </summary>
        public IGorgon2DCompositorPass this[int index] => _passes[index];

        /// <summary>
        /// Property to return the passes registered within the compositor by name.
        /// </summary>
        public IGorgon2DCompositorPass this[string name]
        {
            get
            {
                if (!_passLookup.TryGetValue(name, out int index))
                {
#pragma warning disable CA1065 // Do not raise exceptions in unexpected locations
                    throw new KeyNotFoundException();
#pragma warning restore CA1065 // Do not raise exceptions in unexpected locations
                }

                return this[index];
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to determine if the resources need updating or not.
        /// </summary>
        /// <param name="outputTarget">The final render output target.</param>
        /// <returns><b>true</b> if the resources need updating, or <b>false</b> if not.</returns>
        private bool NeedsResourceUpdate(GorgonRenderTargetView outputTarget) => (_final == null)
                                                                                 || (_pingTarget.Width != outputTarget.Width)
                                                                                 || (_pingTarget.Height != outputTarget.Height)
                                                                                 || (_pingTarget.Format != outputTarget.Format);

        /// <summary>
        /// Function to free any resources allocated by the compositor.
        /// </summary>
        private void FreeResources()
        {
            GorgonTexture2DView pingTexture = Interlocked.Exchange(ref _pingTexture, null);
            GorgonTexture2DView pongTexture = Interlocked.Exchange(ref _pongTexture, null);
            GorgonRenderTarget2DView pingTarget = Interlocked.Exchange(ref _pingTarget, null);
            GorgonRenderTarget2DView pongTarget = Interlocked.Exchange(ref _pongTarget, null);

            pingTexture?.Dispose();
            pongTexture?.Dispose();
            pingTarget?.Dispose();
            pongTarget?.Dispose();
        }

        /// <summary>
        /// Function to create the required resources for the processor.
        /// </summary>
        /// <param name="outputTarget">The final output target.</param>
        private void CreateResources(GorgonRenderTargetView outputTarget)
        {
            FreeResources();

            _final = outputTarget;

            _pingTarget = GorgonRenderTarget2DView.CreateRenderTarget(Graphics, new GorgonTexture2DInfo("Gorgon 2D Post Process Ping Render Target")
            {
                Format = outputTarget.Format,
                Width = outputTarget.Width,
                Height = outputTarget.Height,
                Binding = TextureBinding.ShaderResource
            });
            _pingTexture = _pingTarget.GetShaderResourceView();

            _pongTarget = GorgonRenderTarget2DView.CreateRenderTarget(Graphics, new GorgonTexture2DInfo(_pingTarget, "Gorgon 2D Post Process Pong Render Target"));
            _pongTexture = _pongTarget.GetShaderResourceView();
        }

        /// <summary>
        /// Function to render the initial scene to the initial render target.
        /// </summary>
        /// <param name="lastTargetTexture">The last texture used as a target.</param>
        private void CopyToFinal(GorgonTexture2DView lastTargetTexture)
        {
            Graphics.SetRenderTarget(_final);

            if (_finalClear != null)
            {
                _final.Clear(_finalClear.Value);
            }

            // Copy the composited output into the final render target specified by the user.
            Renderer.Begin(Gorgon2DBatchState.NoBlend);
            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, lastTargetTexture.Width, lastTargetTexture.Height),
                                         GorgonColor.White,
                                         lastTargetTexture,
                                         new DX.RectangleF(0, 0, 1, 1));
            Renderer.End();
        }

        /// <summary>
        /// Function to render a scene that does not use an effect.
        /// </summary>
        /// <param name="pass">The current pass.</param>
        /// <param name="currentTarget">The currently active render target.</param>
        /// <param name="currentTexture">The texture for the previously active render target.</param>
        private void RenderNoEffectPass(CompositionPass pass, GorgonRenderTargetView currentTarget, GorgonTexture2DView currentTexture)
        {
            if (Graphics.RenderTargets[0] != currentTarget)
            {
                Graphics.SetRenderTarget(currentTarget, Graphics.DepthStencilView);
            }

            Renderer.Begin(pass.BatchState, pass.Camera);
            pass.NoEffectRenderMethod(Renderer, currentTexture, currentTarget);
            Renderer.End();
        }

        /// <summary>
        /// Function to add an effect, and an optional rendering action to the compositor queue.
        /// </summary>
        /// <param name="pass">The effect pass to add.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="pass"/> parameter is <b>null</b>.</exception>
        /// <seealso cref="CompositionPass"/>
        private Gorgon2DCompositor Pass(CompositionPass pass)
        {
            if (pass == null)
            {
                throw new ArgumentNullException(nameof(pass));
            }

            if (_passLookup.TryGetValue(pass.Name, out int prevIndex))
            {
                _passes[prevIndex] = pass;
            }
            else
            {
                _passLookup[pass.Name] = _passes.Count;
                _passes.Add(pass);
            }

            return this;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Interlocked.Exchange(ref _final, null);
            FreeResources();
        }

        /// <summary>
        /// Function to assign the color to use when clearing the initial render target when rendering begins.
        /// </summary>
        /// <param name="value">The color to use when clearing.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        /// <remarks>
        /// <para>
        /// When this value is non-<b>null</b>, the very first rendering of the scene will have its render target initialized to this color. If the value is <b>null</b>, then the render target will not be
        /// cleared at all.
        /// </para>
        /// <para>
        /// The default value is <see cref="GorgonColor.BlackTransparent"/>.
        /// </para>
        /// </remarks>
        public Gorgon2DCompositor InitialClearColor(GorgonColor? value)
        {
            _initialClear = value;
            return this;
        }

        /// <summary>
        /// Function to assign the color to use when clearing the final render target when rendering completes.
        /// </summary>
        /// <param name="value">The color to use when clearing.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        /// <remarks>
        /// <para>
        /// When this value is non-<b>null</b>, the very last rendering of the scene will clear the render target supplied by the user in the <see cref="Render"/> method to the <paramref name="value"/>
        /// specified. If the value is <b>null</b>, then the render target will not be cleared at all.
        /// </para>
        /// <para>
        /// The default value is <see cref="GorgonColor.BlackTransparent"/>.
        /// </para>
        /// </remarks>
        public Gorgon2DCompositor FinalClearColor(GorgonColor? value)
        {
            _finalClear = value;
            return this;
        }

        /// <summary>
        /// Function to clear the effects from the compositor.
        /// </summary>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor Clear()
        {
            _passes.Clear();
            _passLookup.Clear();
            return this;
        }

        /// <summary>
        /// Function to remove the first matching pass in the effect chain.
        /// </summary>
        /// <param name="index">The index of the pass to remove.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor RemovePass(int index)
        {
            if ((index < 0) || (index >= _passes.Count))
            {
                return this;
            }

            CompositionPass pass = _passes[index];
            _passes.Remove(index);
            _passLookup.Remove(pass.Name);
            return this;
        }

        /// <summary>
        /// Function to remove the first matching pass in the effect chain.
        /// </summary>
        /// <param name="effectName">The name of the pass to remove.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor RemovePass(string effectName)
        {
            if (string.IsNullOrWhiteSpace(effectName))
            {
                return this;
            }

            if (!_passLookup.TryGetValue(effectName, out int index))
            {
                return this;
            }

            _passes.Remove(index);
            _passLookup.Remove(effectName);
            return this;
        }


        /// <summary>
        /// Function to add an effect, and an optional rendering action to the compositor queue.
        /// </summary>
        /// <param name="name">A name for the pass.</param>
        /// <param name="effect">The effect to add as a pass.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, or the <paramref name="effect"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        public Gorgon2DCompositor EffectPass(string name, IGorgon2DCompositorEffect effect)
        {
            if (effect == null)
            {
                throw new ArgumentNullException(nameof(effect));
            }            

            return Pass(new CompositionPass(name)
            {
                Effect = effect
            });
        }

        /// <summary>
        /// Function to add a pass that will render without applying any effect.
        /// </summary>
        /// <param name="name">A name for the pass.</param>
        /// <param name="renderMethod">The callback method used to render the pass.</param>
        /// <param name="batchState">[Optional] The batch rendering state to apply.</param>
        /// <param name="camera">[Optional] The camera to use when rendering the pass.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="name"/>, or the <paramref name="renderMethod"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="name"/> parameter is empty.</exception>
        /// <remarks>
        /// <para>
        /// This allows an application to render a scene without applying any kind of effect.  The <paramref name="renderMethod"/> is a method that will take the following parameters:
        /// <list type="number">
        ///     <item>
        ///         <description>A renderer instance that can be used to draw.</description>
        ///     </item>
        ///     <item>
        ///         <description>The last texture that was processed by a previous effect.</description>
        ///     </item>
        ///     <item>
        ///         <description>The current render target.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </remarks>
        public Gorgon2DCompositor RenderingPass(string name, Action<Gorgon2D, GorgonTexture2DView, GorgonRenderTargetView> renderMethod, Gorgon2DBatchState batchState = null, IGorgon2DCamera camera = null)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }

            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentEmptyException(nameof(name));
            }

            if (renderMethod == null)
            {
                throw new ArgumentNullException(nameof(renderMethod));
            }
            
            if (_passLookup.TryGetValue(name, out int prevIndex))
            {
                _passes[prevIndex] = new CompositionPass(name)
                {
                    NoEffectRenderMethod = renderMethod,
                    BatchState = batchState,
                    Camera = camera
                };
            }
            else
            {
                _passLookup[name] = _passes.Count;
                _passes.Add(new CompositionPass(name)
                {
                    NoEffectRenderMethod = renderMethod,
                    BatchState = batchState,
                    Camera = camera
                });
            }
            
            return this;
        }

        /// <summary>
        /// Function to move the pass with the specified name to a new location in the list.
        /// </summary>
        /// <param name="name">The name of the pass to move.</param>
        /// <param name="newPassIndex">The new index for the pass.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor MovePass(string name, int newPassIndex)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return this;
            }

            return !_passLookup.TryGetValue(name, out int passIndex) ? this : MovePass(passIndex, newPassIndex);
        }

        /// <summary>
        /// Function to move the pass at the specified index to a new location in the list.
        /// </summary>
        /// <param name="passIndex">The index of the pass to move.</param>
        /// <param name="newPassIndex">The new index for the pass.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor MovePass(int passIndex, int newPassIndex)
        {
            passIndex = passIndex.Max(0).Min(_passLookup.Count - 1);
            newPassIndex = newPassIndex.Max(0).Min(_passLookup.Count);

            if (newPassIndex == passIndex)
            {
                return this;
            }

            CompositionPass pass = _passes[passIndex];

            _passes[passIndex] = null;
            _passes.Insert(newPassIndex, pass);
            _passes.Remove((CompositionPass)null);

            for (int i = 0; i < _passes.Count; ++i)
            {
                pass = _passes[i];
                _passLookup[pass.Name] = i;
            }

            return this;
        }

        /// <summary>
        /// Function to render the scene for the compositor effects.
        /// </summary>
        /// <param name="source">The texture used to begin the post processing.</param>
        /// <param name="output">The final output render target for the compositor.</param>
        /// <returns>The fluent interface for the effects processor.</returns>
        public Gorgon2DCompositor Render(GorgonTexture2DView source, GorgonRenderTargetView output)
        {
            source.ValidateObject(nameof(source));
            output.ValidateObject(nameof(output));

            // Create or update our resources
            if ((_final != output) || (NeedsResourceUpdate(output)))
            {
                CreateResources(output);
            }

            // If we have no effects, then, just output the scene to the render target as-is.
            if (_passLookup.Count == 0)
            {
                CopyToFinal(source);
                return this;
            }

            if (_initialClear != null)
            {
                _pingTarget.Clear(_initialClear.Value);
                _pongTarget.Clear(_initialClear.Value);
            }

            (GorgonRenderTargetView target, GorgonTexture2DView texture) current = (_pingTarget, source);

            // Iterate through our items.
            for (int i = 0; i < _passes.Count; ++i)
            {
                CompositionPass pass = _passes[i];

                if (!pass.Enabled)
                {
                    continue;
                }

                GorgonRenderTargetView currentTarget = current.target;
                GorgonTexture2DView currentTexture = current.texture;
                GorgonTexture2DView nextTexture = ((currentTexture == source) || (currentTexture == _pongTexture)) ? _pingTexture : _pongTexture;
                GorgonRenderTarget2DView nextTarget = current.target == _pingTarget ? _pongTarget : _pingTarget;

                if (pass.ClearColor != null)
                {
                    currentTarget.Clear(pass.ClearColor.Value);
                }

                if (pass.Effect != null)
                {
                    pass.Effect.Render(currentTexture, currentTarget);
                }
                else
                {
                    RenderNoEffectPass(pass, currentTarget, currentTexture);
                }

                current = (nextTarget, nextTexture);
            }

            CopyToFinal(current.texture);

            return this;
        }

        /// <summary>Returns an enumerator that iterates through the collection.</summary>
        /// <returns>An enumerator that can be used to iterate through the collection.</returns>
        public IEnumerator<IGorgon2DCompositorPass> GetEnumerator() => _passes.GetEnumerator();

        /// <summary>Returns an enumerator that iterates through a collection.</summary>
        /// <returns>An <see cref="IEnumerator"/> object that can be used to iterate through the collection.</returns>
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)_passes).GetEnumerator();
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DCompositor"/> class.
        /// </summary>
        /// <param name="renderer">The renderer to use when rendering the effects.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> parameter is <b>null</b>.</exception>
        public Gorgon2DCompositor(Gorgon2D renderer)
        {
            Renderer = renderer ?? throw new ArgumentNullException(nameof(renderer));
            Graphics = renderer.Graphics;
        }
        #endregion
    }
}
