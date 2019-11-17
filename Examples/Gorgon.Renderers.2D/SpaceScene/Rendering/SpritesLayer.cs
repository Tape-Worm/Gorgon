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
// Created: May 21, 2019 11:13:16 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Examples
{
    /// <summary>
    /// A layer used to display sprites.
    /// </summary>
    internal class SpritesLayer
        : Layer2D
    {
        #region Variables.
        // The list of sprites that are lit.
        private List<Gorgon2DBatchState> _states = new List<Gorgon2DBatchState>();
        // The list of sprites organized by name.
        private Dictionary<string, SpriteEntity> _spriteByName = new Dictionary<string, SpriteEntity>();
        // A list of sprite entities to draw.
        private List<(int index, SpriteEntity entity)> _drawList = new List<(int index, SpriteEntity entity)>();
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the sprites for this layer.
        /// </summary>
        public List<SpriteEntity> Sprites
        {
            get;
        } = new List<SpriteEntity>();

        /// <summary>
        /// Property to set or return the gbuffer for lighting.
        /// </summary>
        public Gorgon2DGBuffer GBuffer
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the deferred lighting effect.
        /// </summary>
        public Gorgon2DLightingEffect DeferredLighter
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to perform a culling pass to exclude objects that are not visible.
        /// </summary>
        /// <param name="index">The index of the entity.</param>
        /// <param name="entity">The entity to evaluate.</param>
        /// <returns><b>true</b> if the sprite was culled, <b>false</b> if not.</returns>
        private bool DoCullingPass(int index, SpriteEntity entity)
        {
            GorgonSprite sprite = entity.Sprite;

            sprite.Depth = 0.1f;
            sprite.Position = entity.Position;
            sprite.Angle = entity.Rotation;
            sprite.Scale = new DX.Vector2(entity.Scale);
            sprite.Anchor = entity.Anchor;

            Renderer.GetAABB(sprite, out DX.RectangleF bounds);

            if ((Camera.ViewableRegion.Intersects(bounds))
                && (entity.Visible))
            {
                _drawList.Add((index, entity));
                return false;
            }

            return true;
        }

        /// <summary>Function called to update items per frame on the layer.</summary>
        protected override void OnUpdate()
        {
            if (DeferredLighter != null)
            {
                DeferredLighter.Lights.Clear();

                for (int i = 0; i < ActiveLights.Count; ++i)
                {
                    DeferredLighter.Lights.Add(ActiveLights[i]);
                }
            }

            _drawList.Clear();

            for (int i = 0; i < Sprites.Count; i++)
            {
                SpriteEntity sprite = Sprites[i];

                DX.Vector2 transformed = sprite.LocalPosition + Offset;

                sprite.Position = transformed / ParallaxLevel;

                DoCullingPass(i, sprite);
            }
        }

        /// <summary>Function used to render data into the layer.</summary>
        public override void Render()
        {
            Gorgon2DBatchState lastState = null;

            if (_drawList.Count == 0)
            {
                return;
            }

            bool needStateChange = true;
            bool wasLit = false;

            GorgonRenderTargetView rtv = Graphics.RenderTargets[0];

            if (rtv == null)
            {
                return;
            }

            if ((GBuffer != null) && 
                ((GBuffer.Diffuse.Width != rtv.Width) || (GBuffer.Diffuse.Height != rtv.Height)))
            {
                GBuffer.Resize(rtv.Width, rtv.Height);
            }                       

            for (int i = 0; i < _drawList.Count; ++i)
            {
                (int spriteIndex, SpriteEntity entity) = _drawList[i];
                GorgonSprite sprite = entity.Sprite;
                Gorgon2DBatchState currentState = _states[spriteIndex];

                needStateChange = (i == 0) || (lastState != currentState) || (wasLit != entity.IsLit);

                if (needStateChange)
                {
                    if ((!entity.IsLit) || (GBuffer == null))
                    {
                        if ((wasLit) && (GBuffer != null))
                        {
                            GBuffer.End();
                            if ((DeferredLighter != null) && (DeferredLighter.Lights.Count > 0))
                            {
                                DeferredLighter.Render(GBuffer, rtv, Camera);
                            }
                        }
                        Renderer.End();
                        Renderer.Begin(currentState, camera: Camera);
                    }
                    else
                    {
                        if (wasLit)
                        {
                            GBuffer.End();                            
                            if ((DeferredLighter != null) && (DeferredLighter.Lights.Count > 0))
                            {
                                DeferredLighter.Render(GBuffer, rtv, Camera);
                            }
                        }
                        Renderer.End();
                        GBuffer.ClearGBuffer();
                        GBuffer.Begin(2, 1, currentState?.BlendState, currentState?.DepthStencilState, camera: Camera);
                    }

                    wasLit = entity.IsLit;
                    needStateChange = false;
                    lastState = currentState;
                }

                sprite.Depth = 0.1f;
                sprite.Position = entity.Position;
                sprite.Angle = entity.Rotation;
                sprite.Scale = new DX.Vector2(entity.Scale);
                sprite.Color = entity.Color;
                sprite.Anchor = entity.Anchor;

                Renderer.DrawSprite(sprite);
            }

            GBuffer?.End();
            if ((wasLit) && (DeferredLighter != null) && (DeferredLighter.Lights.Count > 0))
            {
                DeferredLighter.Render(GBuffer, rtv, Camera);
            }
            Renderer.End();
        }

        /// <summary>Function used to load in resources required by the layer.</summary>
        public override void LoadResources()
        {
            var builder = new Gorgon2DBatchStateBuilder();
            GorgonBlendState state = null;

            var batchStates = new Dictionary<GorgonBlendState, Gorgon2DBatchState>();
            batchStates.Add(GorgonBlendState.Default, null);

            foreach (GorgonBlendState blendState in Sprites.Select(item => item.BlendState).Distinct())
            {
                if ((blendState == null) || (blendState == GorgonBlendState.Default))
                {
                    continue;
                }

                batchStates.Add(blendState, builder.Clear()
                                                   .BlendState(blendState)
                                                   .Build());
            }

            foreach (SpriteEntity entity in Sprites)
            {
                GorgonBlendState currentState = entity.BlendState ?? GorgonBlendState.Default;

                if (currentState == GorgonBlendState.Default)
                {
                    _states.Add(null);
                }
                else
                {
                    _states.Add(batchStates[currentState]);
                }

                _spriteByName.Add(entity.Name, entity);
            }
        }

        /// <summary>
        /// Function to retrieve a sprite from the layer by its name.
        /// </summary>
        /// <param name="name">The name of the sprite.</param>
        /// <returns>The sprite entity with the specified name.</returns>
        public SpriteEntity GetSpriteByName(string name) => _spriteByName[name];
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="SpritesLayer"/> class.</summary>
        /// <param name="renderer">The 2D renderer for the application.</param>
        public SpritesLayer(Gorgon2D renderer)
            : base(renderer)
        {
        }
        #endregion
    }
}
