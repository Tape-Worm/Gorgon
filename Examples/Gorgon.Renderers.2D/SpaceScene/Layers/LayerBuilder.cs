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
// Created: May 24, 2019 4:50:37 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Examples
{
    /// <summary>
    /// Function to build the layers for our example.
    /// </summary>
    /// <remarks>
    /// This is here for convenience. In a real game, we'd load this as data and build our scene from that.  
    /// </remarks>
    internal static class LayerBuilder
    {
        /// <summary>
        /// Function to build the background layer representing distant stars.
        /// </summary>
        /// <param name="renderer">The 2D renderer for the application.</param>
        /// <param name="resources">The resources for the application.</param>
        /// <returns>The background layer.</returns>
        public static BgStarLayer GetBackgroundLayer(Gorgon2D renderer, ResourceManagement resources)
        {
            var backgroundLayer = new BgStarLayer(renderer)
            {
                PostProcessGroup = "Final Pass",					// These post process groups allow us to assign which sprite layers end up in post processing, and which are blitted immediately.
                StarsTexture = resources.Textures["StarsNoAlpha"]
            };

            backgroundLayer.LoadResources();
            return backgroundLayer;
        }

        /// <summary>
        /// Function to build the layer that contains the sun.
        /// </summary>
        /// <param name="renderer">The 2D renderer for the application.</param>
        /// <param name="resources">The resources for the application.</param>
        /// <returns>The sprite layer that will contain the sun sprite.</returns>
        public static SpritesLayer GetSunLayer(Gorgon2D renderer, ResourceManagement resources)
        {
            GorgonSprite sunSprite = resources.Sprites["Star"];
            var sunLayer = new SpritesLayer(renderer)
            {
                ParallaxLevel = 2980.0f,			// The sun is pretty far away the last I checked.
                Sprites =
                {
                    new SpriteEntity("Sun")
                    {
                        Sprite = sunSprite,
                        LocalPosition = new DX.Vector2(1200, -650)
                    }
                },
                PostProcessGroup = "Final Pass",
                Lights =
                {
                    new Light
                    {
                        Attenuation = float.MaxValue.Sqrt(),
                        Color = GorgonColor.White,
                        SpecularPower = 6.0f,
                        LocalLightPosition = new DX.Vector3(1200, -650, -1.0f),
                        Intensity = 13.07f
                    }
                }
            };
            sunLayer.LoadResources();

            return sunLayer;
        }

        /// <summary>
        /// Function to return the 3D layer that contains our planet.
        /// </summary>
        /// <param name="graphics">The graphics interface for the application.</param>
        /// <param name="resources">The resources for the application.</param>
        /// <returns>The 3D planet layer.</returns>
        public static PlanetLayer GetPlanetLayer(GorgonGraphics graphics, ResourceManagement resources)
        {
            // Create our planet.
            var planetLayer = new PlanetLayer(graphics, resources)
            {
                ParallaxLevel = 50,		// Kinda far away.
                Planets =
                {
                    new Planet(new PlanetaryLayer[]
                    {
                        new PlanetaryLayer(resources.Meshes["earthSphere"])
                        {
                            Animation = resources.Animations["PlanetRotation"]
                        },
                        new PlanetaryLayer(resources.Meshes["earthCloudSphere"])
                        {
                            Animation = resources.Animations["CloudRotation"]
                        },
                    })
                    {
                        Position = new DX.Vector3(-30, -15, 4.0f)
                    }
                }
            };

            // Add an ambient light to the planet so we can see some details without a major light source.
            planetLayer.Lights.Add(new Light
            {
                Attenuation = float.MaxValue.Sqrt(),
                Color = GorgonColor.White,
                SpecularPower = 0.0f,
                Intensity = 0.3f,
                LocalLightPosition = new DX.Vector3(0, 0, -10000.0f),
                Layers =
                {
                    planetLayer
                }
            });

            planetLayer.LoadResources();
            planetLayer.PostProcessGroup = "Final Pass";

            return planetLayer;
        }

        /// <summary>
        /// Function to retrieve the layer with our ship on it.
        /// </summary>
        /// <param name="renderer">The 2D renderer for the application.</param>
        /// <param name="resources">The resources for the application.</param>
        /// <returns>The sprite layer containing our ship.</returns>
        public static SpritesLayer GetShipLayer(Gorgon2D renderer, ResourceManagement resources)
        {
            var ship = new SpritesLayer(renderer)
            {
                DeferredLighter = resources.Effects["deferredLighting"] as Gorgon2DLightingEffect,
                GBuffer = resources.Effects["gbuffer"] as Gorgon2DGBuffer,
                Sprites =
                {
                    new SpriteEntity("BigShip")
                    {
                        Sprite = resources.Sprites["BigShip"],
                        Color = GorgonColor.White,
                        Rotation = -95.0f,
                        IsLit = true,
                        Visible = false
                    },
                    new SpriteEntity("BigShip_Illum")
                    {
                        Sprite = resources.Sprites["BigShip_Illum"],
                        Color = GorgonColor.YellowPure * 0.7f,
                        Rotation = -95.0f,
                        IsLit = false,
                        Visible = false,
                        BlendState = GorgonBlendState.Additive
                    },
                    new SpriteEntity("EngineGlow")
                    {
                        Sprite = resources.Sprites["Fighter_Engine_F0"],
                        Color = new GorgonColor(GorgonColor.CyanPure, 0),
                        Rotation = -45.0f,
                        Anchor = new DX.Vector2(0.5f, -1.5f),
                        Animation = resources.Animations["EngineGlow"]
                    },
                    new SpriteEntity("Fighter")
                    {
                        Sprite = resources.Sprites["Fighter"],
                        Color = GorgonColor.White,
                        Rotation = -45.0f,
                        IsLit = true
                    }
                },
                Offset = DX.Vector2.Zero,
                PostProcessGroup = "Final Pass",
                Lights =
                {
                    new Light
                    {
                        Intensity = 0.25f,
                        LightType = LightType.Directional,
                        LightDirection = new DX.Vector3(1.0f, 0.0f, 0.7071068f),
                        SpecularEnabled = true,
                        SpecularPower = 128,
                        Color = GorgonColor.White,
                    }
                }
            };

            ship.Lights[0].Layers.Add(ship);
            ship.LoadResources();

            return ship;
        }
    }
}
