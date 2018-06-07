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
// Created: June 6, 2018 12:53:53 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using DX = SharpDX;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Renderers.Properties;

namespace Gorgon.Renderers
{
    /// <summary>
    /// TODO: Fill me in.
    /// </summary>
    public class Gorgon2D
        : IDisposable, IGorgonGraphicsObject
    {
        #region Variables.
        // The flag to indicate that the renderer is initialized.
        private bool _initialized;
        // The primary render target.
        private readonly GorgonRenderTarget2DView _primaryTarget;
        // The default vertex shader used by the renderer.
        private GorgonVertexShader _defaultVertexShader;
        // The default pixel shader used by the renderer.
        private GorgonPixelShader _defaultPixelShader;
        // The layout used to define a vertex to the vertex shader.
        private GorgonInputLayout _vertexLayout;
        // The renderer used to draw sprites.
        private SpriteRenderer _spriteRenderer;
        // The default texture to render.
        private GorgonTexture2DView _defaultTexture;
        // The buffer that holds the view and projection matrices.
        private GorgonConstantBufferView _viewProjection;
        // A factory used to create draw calls.
        private DrawCallFactory _drawCallFactory;
        // The currently active draw call.
        private GorgonDrawIndexCall _currentDrawCall;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the log used to log debug messages.
        /// </summary>
        public IGorgonLog Log => _primaryTarget.Graphics.Log;

        /// <summary>
        /// Property to return the <see cref="GorgonGraphics"/> interface that owns this renderer.
        /// </summary>
        public GorgonGraphics Graphics => _primaryTarget.Graphics;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to initialize the renderer.
        /// </summary>
        private void Initialize()
        {
            if (_initialized)
            {
                return;
            }

            if (!GorgonShaderFactory.Includes.ContainsKey("Gorgon2DShaders"))
            {
                GorgonShaderFactory.Includes["Gorgon2DShaders"] = new GorgonShaderInclude("Gorgon2DShaders", Resources.BasicSprite);
            }

            _defaultVertexShader = GorgonShaderFactory.Compile<GorgonVertexShader>(Graphics, Resources.BasicSprite, "GorgonVertexShader", GorgonGraphics.IsDebugEnabled);
            _defaultPixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(Graphics, Resources.BasicSprite, "GorgonPixelShaderTextured", GorgonGraphics.IsDebugEnabled);

            _vertexLayout = GorgonInputLayout.CreateUsingType<Gorgon2DVertex>(Graphics, _defaultVertexShader);

            // We need to ensure that we have a default texture in case we decide not to send a texture in.
            GorgonTexture2D textureResource = Resources.White_2x2.ToTexture2D(Graphics,
                                                                              new GorgonTextureLoadOptions
                                                                              {
                                                                                  Name = "Default White 2x2 Texture",
                                                                                  Binding = TextureBinding.ShaderResource,
                                                                                  Usage = ResourceUsage.Immutable
                                                                              });
            _defaultTexture = textureResource.GetShaderResourceView();

            DX.Matrix.OrthoOffCenterLH(0,
                                       _primaryTarget.Width,
                                       _primaryTarget.Height,
                                       0,
                                       0.0f,
                                       1.0f,
                                       out DX.Matrix projection);

            _viewProjection = GorgonConstantBufferView.CreateConstantBuffer(Graphics, ref projection, name: "View * Projection Matrix Buffer");

            // Set up the sprite renderer.
            _spriteRenderer.InitializeResources();

            _drawCallFactory.InputLayout = _vertexLayout;
            _drawCallFactory.IndexBuffer = _spriteRenderer.IndexBuffer;
            _drawCallFactory.VertexBuffer = _spriteRenderer.VertexBuffer;
            _drawCallFactory.DefaultPixelShader = _defaultPixelShader;
            _drawCallFactory.DefaultVertexShader = _defaultVertexShader;
            _drawCallFactory.ProjectionViewBuffer = _viewProjection;
            _drawCallFactory.DefaultTexture = _defaultTexture;

            // Set the initial render target.
            Graphics.SetRenderTarget(_primaryTarget);

            _initialized = true;
        }

        /// <summary>
        /// Function to begin rendering.
        /// </summary>
        public void Begin()
        {
            // If we're not initialized, then do so now.
            // Note that this is not thread safe.
            if (!_initialized)
            {
                Initialize();
            }
        }

        public void Draw(Gorgon2DRenderable sprite)
        {
            GorgonDrawIndexCall drawCall = _drawCallFactory.GetDrawIndexCall(sprite);

            if ((_currentDrawCall != null) && (drawCall != _currentDrawCall))
            {
                _spriteRenderer.RenderBatches(_currentDrawCall);
            }

            _currentDrawCall = drawCall;

            _spriteRenderer.QueueSprite(sprite);
        }

        /// <summary>
        /// Function to end rendering.
        /// </summary>
        public void End()
        {
            _spriteRenderer.RenderBatches(_currentDrawCall);
            _currentDrawCall = null;
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            GorgonVertexShader vertexShader = Interlocked.Exchange(ref _defaultVertexShader, null);
            GorgonPixelShader pixelShader = Interlocked.Exchange(ref _defaultPixelShader, null);
            GorgonInputLayout layout = Interlocked.Exchange(ref _vertexLayout, null);
            SpriteRenderer spriteRenderer = Interlocked.Exchange(ref _spriteRenderer, null);
            GorgonTexture2DView texture = Interlocked.Exchange(ref _defaultTexture, null);
            GorgonConstantBufferView viewProj = Interlocked.Exchange(ref _viewProjection, null);
            
            viewProj?.Buffer?.Dispose();
            texture?.Texture?.Dispose();
            spriteRenderer?.Dispose();
            layout?.Dispose();
            vertexShader?.Dispose();
            pixelShader?.Dispose();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2D"/> class.
        /// </summary>
        /// <param name="target">The render target that will receive the rendering data.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="target"/> parameter is <b>null</b>.</exception>
        public Gorgon2D(GorgonRenderTarget2DView target)
        {
            _primaryTarget = target ?? throw new ArgumentNullException(nameof(target));
            _spriteRenderer = new SpriteRenderer(target.Graphics);
            _drawCallFactory = new DrawCallFactory(target.Graphics);
        }
        #endregion
    }
}
