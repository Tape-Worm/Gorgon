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
using System.Text;
using System.Threading;
using Gorgon.Core;
using DX = SharpDX;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Renderers.Properties;
using Gorgon.UI;

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
        // The buffer used to perform alpha testing.
        private GorgonConstantBufferView _alphaTest;
        // A factory used to create draw calls.
        private DrawCallFactory _drawCallFactory;
        // The currently active draw call.
        private GorgonDrawIndexCall _currentDrawCall;
        // The previously assigned batch state.
        private readonly Gorgon2DBatchState _lastBatchState = new Gorgon2DBatchState();
        // The last sprite that was put into the system.
        private BatchRenderable _lastRenderable;
        // The current alpha test data.
        private AlphaTestData _alphaTestData;
        // Flag to indicate that the begin method has been called.
        private int _beginCalled;
        // A buffer used for text manipulation in the DrawText method.
        private StringBuilder _textBuffer = new StringBuilder(256);
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
        /// Function to update the alpha test data.
        /// </summary>
        /// <param name="currentData">The data to write into the buffer.</param>
        private void UpdateAlphaTest(ref AlphaTestData currentData)
        {
            if (currentData.Equals(_alphaTestData))
            {
                return;
            }

            _alphaTest.Buffer.SetData(ref currentData);
            _alphaTestData = currentData;
        }

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

            // Set up the sprite renderer buffers.
            DX.Matrix.OrthoOffCenterLH(0,
                                       _primaryTarget.Width,
                                       _primaryTarget.Height,
                                       0,
                                       0.0f,
                                       1.0f,
                                       out DX.Matrix projection);

            _viewProjection = GorgonConstantBufferView.CreateConstantBuffer(Graphics, ref projection, "View * Projection Matrix Buffer");

            _alphaTestData = new AlphaTestData(true, GorgonRangeF.Empty);
            _alphaTest = GorgonConstantBufferView.CreateConstantBuffer(Graphics, ref _alphaTestData, "Alpha Test Buffer");

            _spriteRenderer = new SpriteRenderer(Graphics);
            _drawCallFactory = new DrawCallFactory(Graphics, _defaultTexture, _vertexLayout)
                               {
                                   ProjectionViewBuffer = _viewProjection,
                                   AlphaTestBuffer = _alphaTest
                               };

            // Set up the initial state.
            _lastBatchState.PixelShader = _defaultPixelShader;
            _lastBatchState.VertexShader = _defaultVertexShader;
            _lastBatchState.BlendState = GorgonBlendState.Default;
            _lastBatchState.RasterState = GorgonRasterState.Default;
            _lastBatchState.DepthStencilState = GorgonDepthStencilState.Default;

            // Set the initial render target.
            Graphics.SetRenderTarget(_primaryTarget);

            _initialized = true;
        }

        /// <summary>
        /// Function to begin rendering.
        /// </summary>
        /// <param name="batchState">[Optional] Defines common global state to use when rendering a batch of objects.</param>
        public void Begin(Gorgon2DBatchState batchState = null)
        {
            if (Interlocked.Exchange(ref _beginCalled, 1) == 1)
            {
                return;
            }

            // If we're not initialized, then do so now.
            // Note that this is not thread safe.
            if (!_initialized)
            {
                Initialize();
            }

            _lastRenderable = null;
            _lastBatchState.PixelShader = batchState?.PixelShader ?? _defaultPixelShader;
            _lastBatchState.VertexShader = batchState?.VertexShader ?? _defaultVertexShader;
            _lastBatchState.BlendState = batchState?.BlendState ?? GorgonBlendState.Default;
            _lastBatchState.RasterState = batchState?.RasterState ?? GorgonRasterState.Default;
            _lastBatchState.DepthStencilState = batchState?.DepthStencilState ?? GorgonDepthStencilState.Default;
        }

        /// <summary>
        /// Function to check for changes in the batch state, and render the previous batch if necessary.
        /// </summary>
        /// <param name="renderable"></param>
        private void RenderBatchOnChange(BatchRenderable renderable)
        {
            // If we're sending the same guy in, there's no point in jumping through all of these hoops.
            if ((_lastRenderable != null) && (_spriteRenderer.RenderableStateComparer.Equals(_lastRenderable, renderable)))
            {
                return;
            }

            GorgonDrawIndexCall drawCall = _drawCallFactory.GetDrawIndexCall(renderable, _lastBatchState, _spriteRenderer);

            if ((_currentDrawCall != null) && (drawCall != _currentDrawCall))
            {
                if (_lastRenderable != null)
                {
                    UpdateAlphaTest(ref _lastRenderable.AlphaTestData);
                }

                _spriteRenderer.RenderBatches(_currentDrawCall);
            }

            _lastRenderable = renderable;
            // All states are reconciled, so reset the change flag.
            _lastRenderable.StateChanged = false;

            _currentDrawCall = drawCall;
        }

        /// <summary>
        /// Function to draw a sprite.
        /// </summary>
        /// <param name="sprite"></param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="InvalidOperationException">Thrown if <see cref="Begin"/> has not been called prior to calling this method.</exception>
        public void Draw(GorgonSprite sprite)
        {
            sprite.ValidateObject(nameof(sprite));

            if (_beginCalled == 0)
            {
                throw new InvalidOperationException(Resources.GOR2D_ERR_BEGIN_NOT_CALLED);
            }

            BatchRenderable renderable = sprite.Renderable;

            RenderBatchOnChange(renderable);

            if (sprite.IsUpdated)
            {
                _spriteRenderer.SpriteTransformer.Transform(renderable);
            }

            _spriteRenderer.QueueSprite(renderable, 6);
        }

        /// <summary>
        /// Function to draw text.
        /// </summary>
        /// <param name="sprite">The text sprite to render.</param>
        public void DrawText(GorgonTextSprite sprite)
        {
            sprite.ValidateObject(nameof(sprite));

            _textBuffer.Length = 0;
            int textLength = sprite.Text.Length;
            
            // If there's no text, then there's nothing to render.
            if (textLength == 0)
            {
                return;
            }

            TextRenderable renderable = sprite.Renderable;

            // Flush the previous batch if we have one that's different from the upcoming batch.
            RenderBatchOnChange(renderable);

            _textBuffer.Append(sprite.Text);
            
            int vertexOffset = 0;
            DX.Vector2 position = DX.Vector2.Zero;
            int charRenderCount = 0;
            Alignment alignment = sprite.Alignment;

            GorgonFont font = sprite.Font;
            bool drawOutlines = ((sprite.DrawMode != TextDrawMode.GlyphsOnly) && (font.HasOutline));
            int drawCount = ((drawOutlines) && (sprite.DrawMode == TextDrawMode.OutlinedGlyphs))? 2 : 1;
            float fontHeight = font.FontHeight;
            bool hasKerning = (font.Info.UseKerningPairs) && (font.KerningPairs.Count > 0);
            IDictionary<GorgonKerningPair, int> kerningValues = font.KerningPairs;
            float lineSpaceMultiplier = sprite.LineSpace;
            
            for (int line = 0; line < renderable.Lines.Length; ++line)
            {
                string textLine = sprite.Lines[line];
                textLength = textLine.Length;

                for (int dc = 0; dc < drawCount; ++dc)
                {
                    bool isOutlinePass = (drawOutlines) && (dc == 0);

                    DX.Size2F lineMeasure = DX.Size2F.Empty;

                    if (alignment != Alignment.UpperLeft)
                    {
                        lineMeasure = font.MeasureLine(textLine, (drawOutlines) && (dc == 0), lineSpaceMultiplier);
                    }

                    position.X = 0;

                    for (int i = 0; i < textLength; ++i)
                    {
                        char character = textLine[i];
                        int kernAmount = 0;

                        if (!font.Glyphs.TryGetValue(character, out GorgonGlyph glyph))
                        {
                            if (!font.TryGetDefaultGlyph(out glyph))
                            {
                                continue;
                            }
                        }

                        // Handle whitespace by just advancing our position, we don't need geometry for this.
                        if ((char.IsWhiteSpace(character))
                            || (glyph.TextureView == null))
                        {
                            position.X += glyph.Advance;
                            continue;
                        }

                        // If we have a change of texture, then we need to let the renderer know that we need a flush.
                        if ((renderable.Texture != null) && (renderable.Texture != glyph.TextureView))
                        {
                            RenderBatchOnChange(renderable);
                            renderable.HasTextureChanges = true;
                        }

                        renderable.Texture = glyph.TextureView;

                        if (sprite.IsUpdated)
                        {
                            if ((hasKerning) && (i < textLength - 1))
                            {
                                var kernPair = new GorgonKerningPair(character, textLine[i + 1]);
                                kerningValues.TryGetValue(kernPair, out kernAmount);
                            }


                            _spriteRenderer.TextSpriteTransformer.Transform(renderable,
                                                                            glyph,
                                                                            ref position,
                                                                            vertexOffset,
                                                                            isOutlinePass,
                                                                            lineMeasure.Width);

                            vertexOffset += 4;
                            position.X += glyph.Advance + kernAmount;
                        }

                        ++charRenderCount;
                    }
                }

                position.Y += fontHeight * lineSpaceMultiplier;
            }

            if (charRenderCount != 0)
            {
                _spriteRenderer.QueueSprite(renderable, charRenderCount * 6);
            }
            
            renderable.VertexCountChanged = false;
            renderable.HasTransformChanges = false;
            renderable.HasVertexChanges = false;
            renderable.HasTextureChanges = false;
            renderable.HasVertexColorChanges = false;
        }

        // TODO: Turn this into a real thing, it works really well.
        // ReSharper disable FieldCanBeMadeReadOnly.Local
        private GorgonSprite _lineSprite = new GorgonSprite();
        private GorgonSamplerState _testSampler;
        private GorgonSamplerStateBuilder _sampleBuilder;

#pragma warning disable 1591
        public void DrawLineForGiggles(int x1, int y1, int x2, int y2, GorgonColor color, GorgonTexture2DView texture, float size = 20.0f)
        {
            if (_sampleBuilder == null)
            {
                _sampleBuilder = new GorgonSamplerStateBuilder(Graphics);
                _testSampler = _sampleBuilder.ResetTo(GorgonSamplerState.PointFiltering)
                                             .Wrapping(TextureWrap.Wrap, TextureWrap.Wrap)
                                             .Build();
                _lineSprite.TextureSampler = _testSampler;
            }

            DX.Vector2 diff = new DX.Vector2(x2 - x1, y2 - y1);
            
            _lineSprite.CornerColors.SetAll(color);
            DX.Vector2 crossStart = new DX.Vector2(diff.Y, -diff.X);
            crossStart.Normalize();

            crossStart *= size / 2.0f;

            _lineSprite.Bounds = new DX.RectangleF(x1, y1, diff.X, diff.Y);
            _lineSprite.Renderable.HasTextureChanges = _lineSprite.Renderable.HasTransformChanges =
                                                           _lineSprite.Renderable.HasVertexChanges =
                                                               _lineSprite.Renderable.HasVertexColorChanges = false;
            
            _lineSprite.Renderable.Vertices[0].Position = new DX.Vector4(x1 + crossStart.X, y1 + crossStart.Y, 0, 1.0f);
            _lineSprite.Renderable.Vertices[1].Position = new DX.Vector4(x2 + crossStart.X, y2 + crossStart.Y, 0, 1.0f);
            _lineSprite.Renderable.Vertices[2].Position = new DX.Vector4(x1 - crossStart.X, y1 - crossStart.Y, 0, 1.0f);
            _lineSprite.Renderable.Vertices[3].Position = new DX.Vector4(x2 - crossStart.X, y2 - crossStart.Y, 0, 1.0f);
            
            //_lineSprite.Bounds = new DX.RectangleF(x1, y1, diff.X, size.Max(1.0f));

            //_lineSprite.Angle = 45.0f;// angle;
            

            if (texture != null)
            {
                _lineSprite.Renderable.Vertices[0].UV = new DX.Vector3((x1 + crossStart.X) / texture.Width, (y1 + crossStart.Y) / texture.Height, 0);
                _lineSprite.Renderable.Vertices[1].UV = new DX.Vector3((x2 + crossStart.X) / texture.Width, (y2 + crossStart.Y) / texture.Height, 0);
                _lineSprite.Renderable.Vertices[2].UV = new DX.Vector3((x1 - crossStart.X) / texture.Width, (y1 - crossStart.Y) / texture.Height, 0);
                _lineSprite.Renderable.Vertices[3].UV = new DX.Vector3((x2 - crossStart.X) / texture.Width, (y2 - crossStart.Y) / texture.Height, 0);
                _lineSprite.Texture = texture;
            }

            Draw(_lineSprite);
        }
#pragma warning restore 1591
        // ReSharper restore FieldCanBeMadeReadOnly.Local

        /// <summary>
        /// Function to end rendering.
        /// </summary>
        public void End()
        {
            if (Interlocked.Exchange(ref _beginCalled, 0) == 0)
            {
                return;
            }

            if (_lastRenderable != null)
            {
                UpdateAlphaTest(ref _lastRenderable.AlphaTestData);
            }
            
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
            GorgonConstantBufferView alphaTest = Interlocked.Exchange(ref _alphaTest, null);
            
            spriteRenderer?.Dispose();
            alphaTest?.Buffer?.Dispose();
            viewProj?.Buffer?.Dispose();
            texture?.Texture?.Dispose();
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
        }
        #endregion
    }
}
