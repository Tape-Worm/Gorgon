﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: May 18, 2019 5:09:46 PM
// 

using System.Numerics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using Gorgon.Renderers.Cameras;

namespace Gorgon.Examples;

/// <summary>
/// Provides rendering functionality for the appilcation
/// </summary>
internal class SceneRenderer
    : IDisposable
{

    // The main graphics interface.
    private readonly GorgonGraphics _graphics;
    // The 2D renderer.
    private readonly Gorgon2D _renderer;
    // The resource manager.
    private readonly ResourceManagement _resources;
    // The camera used for rendering.
    private readonly GorgonOrthoCamera _camera;
    // The screen buffer.
    private GorgonRenderTarget2DView _screen;
    // The list of layers to render.
    private readonly List<Layer> _layers = [];
    // Post processing groups.  Use to render specific layers with post processing effects.
    private readonly List<(string name, Gorgon2DCompositor compositor)> _postProcessGroups = [];
    private readonly Dictionary<string, List<Layer>> _postProcessLayers = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Function to copy the contents of a texture to the screen buffer.
    /// </summary>
    /// <param name="src">The texture to copy.</param>
    private void FlipToScreen(GorgonTexture2DView src)
    {
        _graphics.SetRenderTarget(_screen);
        _renderer.Begin();
        _renderer.DrawFilledRectangle(new GorgonRectangleF(0, 0, _screen.Width, _screen.Height),
                                    GorgonColors.White,
                                    src,
                                    new GorgonRectangleF(0, 0, 1, 1),
                                    textureSampler: GorgonSamplerState.Default);

        _renderer.End();
    }

    /// <summary>
    /// Function to apply lighting to the layers.
    /// </summary>
    private void ApplyLightingToLayers()
    {
        // Turn off lighting before attempting to update the lights.
        for (int i = 0; i < _layers.Count; ++i)
        {
            _layers[i].ClearActiveLights();
        }

        for (int i = 0; i < _layers.Count; ++i)
        {
            Layer layer = _layers[i];

            for (int j = 0; j < layer.Lights.Count; ++j)
            {
                Light light = layer.Lights[j];

                // If we don't specify layers to light up, assume all layers receive light.
                if (light.Layers.Count == 0)
                {
                    for (int k = 0; k < _layers.Count; ++k)
                    {
                        _layers[k].ApplyLight(light);
                    }

                    continue;
                }

                foreach (Layer lightLayer in light.Layers)
                {
                    lightLayer.ApplyLight(light);
                }
            }
        }
    }

    /// <summary>
    /// Function to render the scene.
    /// </summary>
    public void Render()
    {
        GorgonRenderTarget2DView sceneBuffer = _graphics.TemporaryTargets.Rent(_screen, "Work Buffer", true);
        GorgonTexture2DView sceneSrv = sceneBuffer.GetShaderResourceView();

        _graphics.SetRenderTarget(sceneBuffer);

        ApplyLightingToLayers();

        _screen.Clear(GorgonColors.BlackTransparent);

        // If we have no post processing, then just blit to the screen target.
        if (_postProcessGroups.Count == 0)
        {
            for (int i = 0; i < _layers.Count; ++i)
            {
                _layers[i].Render();
            }

            FlipToScreen(sceneSrv);
        }
        else
        {
            for (int i = 0; i < _postProcessGroups.Count; ++i)
            {
                (string name, Gorgon2DCompositor compositor) = _postProcessGroups[i];

                if ((!_postProcessLayers.TryGetValue(name, out List<Layer> layers))
                    || (layers.Count == 0))
                {
                    continue;
                }

                sceneBuffer.Clear(GorgonColors.BlackTransparent);
                _graphics.SetRenderTarget(sceneBuffer);

                // Render the data.
                for (int j = 0; j < layers.Count; ++j)
                {
                    layers[j].Render();
                }

                if (compositor is null)
                {
                    FlipToScreen(sceneSrv);
                }
                else
                {
                    compositor.Render(sceneSrv, _screen);
                }
            }
        }

        _graphics.TemporaryTargets.Return(sceneBuffer);
    }

    /// <summary>
    /// Function called when the swap chain is resized.
    /// </summary>
    /// <param name="screen">The updated screen render target.</param>
    public void OnResize(GorgonRenderTarget2DView screen)
    {
        _screen = screen;

        if (_screen is null)
        {
            return;
        }

        Vector2 aspect;
        if (_screen.Width > _screen.Height)
        {
            aspect = new Vector2((float)screen.Width / screen.Height, 1.0f);
        }
        else
        {
            aspect = new Vector2(1, (float)screen.Height / screen.Width);
        }

        // Adjust the viewable area to match our aspect ratio.
        // This will give our view a range of -1x-1 - 1x1.
        //_camera.ViewDimensions = new Vector2(2 * aspect.X, 2 * aspect.Y);
        _camera.ViewDimensions = new Vector2(2 * aspect.X, 2 * aspect.Y);

        // All of our sprites are in pixel size, in order to bring them into resolution independent space, we need to adjust their sizes
        // (else they'll be massive).
        foreach (GorgonSprite sprite in _resources.Sprites.Values)
        {
            // Since we're altering the size of the sprites, we'll need to get the original width/height from another place.
            // This can be extracted from the sprite texture region (assuming the region is 1:1 with pixel space, scaling the texture coordinates will mess this up).
            Vector2 size = sprite.Texture.ToPixel(sprite.TextureRegion.Size);

            // Scale the size of the sprite to match our base resolution of 1920x1080.
            Vector2 newSize = new(size.X / screen.Width * _camera.ViewDimensions.X * 0.75f, size.Y / screen.Height * _camera.ViewDimensions.Y * 0.75f);

            sprite.Size = newSize;
        }

        foreach (Layer layer in _layers)
        {
            layer.OnResize(new GorgonPoint(screen.Width, screen.Height));
        }
    }

    /// <summary>
    /// Function to load the resources for the renderer.
    /// </summary>
    public void LoadResources()
    {
        _postProcessLayers["__NULL__"] = [];
        _postProcessGroups.Add(("Final Pass", _resources.PostProcessCompositors["Final Pass"]));
        _postProcessGroups.Add(("Space Ships", null));

        // Split layers into groups.
        foreach (Layer layer in _layers)
        {
            if (string.IsNullOrWhiteSpace(layer.PostProcessGroup))
            {
                _postProcessLayers["__NULL__"].Add(layer);
                continue;
            }

            if (!_postProcessLayers.TryGetValue(layer.PostProcessGroup, out List<Layer> layers))
            {
                _postProcessLayers[layer.PostProcessGroup] = layers = [];
            }

            layers.Add(layer);
        }

        // Use this adjust the sprite data to our coordinate space.
        OnResize(_screen);
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        foreach (IDisposable layer in _layers.OfType<IDisposable>())
        {
            layer.Dispose();
        }
    }

    /// <summary>Initializes a new instance of the <see cref="Examples.Renderer"/> class.</summary>
    /// <param name="renderer">The renderer.</param>
    /// <param name="resources">The resources.</param>
    /// <param name="screen">The main render target for the scene.</param>
    /// <param name="camera">The camera for the scene.</param>
    public SceneRenderer(Gorgon2D renderer, ResourceManagement resources, GorgonRenderTarget2DView screen, LayerCamera layerController, GorgonOrthoCamera camera)
    {
        _graphics = renderer.Graphics;
        _renderer = renderer;
        _resources = resources;
        _screen = screen;
        _layers.AddRange(layerController.Layers);
        _camera = camera;
    }
}
