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
// Created: May 19, 2019 11:01:36 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Editor;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.IO;
using Gorgon.IO.Providers;
using Gorgon.PlugIns;
using Gorgon.Renderers;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Examples;

/// <summary>
/// The resource management system.
/// </summary>
/// <remarks>
/// This is responsible for caching all of our data for the scene. We load sprites, textures, 3D meshes, post processing effects and individual effects into the manager 
/// and then access them via dictionaries so we can get at our data by name.
/// </remarks>
internal class ResourceManagement
    : IDisposable
{
    #region Variables.
    // The cache used to hold texture data.
    private readonly GorgonTextureCache<GorgonTexture2D> _textureCache;
    // The loader used to read content data from the file system.
    private IGorgonContentLoader _contentLoader;
    // The graphics interface for the application.
    private readonly GorgonGraphics _graphics;
    // The 2D renderer interface for the application.
    private readonly Gorgon2D _renderer;
    // The plug in service for the application.
    private readonly GorgonMefPlugInCache _plugIns;
    // The file system where resources are kept.
    private IGorgonFileSystem _fileSystem;
    // The list of shaders.
    private readonly Dictionary<string, GorgonPixelShader> _pixelShaders = new(StringComparer.OrdinalIgnoreCase);
    private readonly Dictionary<string, GorgonVertexShader> _vertexShaders = new(StringComparer.OrdinalIgnoreCase);
    // The list of 3D models.
    private readonly Dictionary<string, MoveableMesh> _meshes = new(StringComparer.OrdinalIgnoreCase);
    // The list of animations.
    private readonly Dictionary<string, IGorgonAnimation> _animations = new(StringComparer.OrdinalIgnoreCase);
    // The list of post process renderers.
    private readonly Dictionary<string, Gorgon2DCompositor> _postProcess = new(StringComparer.OrdinalIgnoreCase);
    // The list of effects used by the application.
    private readonly Dictionary<string, Gorgon2DEffect> _effects = new(StringComparer.OrdinalIgnoreCase);
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the effects used by the application.
    /// </summary>
    public IReadOnlyDictionary<string, Gorgon2DEffect> Effects => _effects;

    /// <summary>
    /// Property to return the loaded textures.
    /// </summary>
    public IReadOnlyDictionary<string, GorgonTexture2D> Textures
    {
        get;
        private set;
    } = new Dictionary<string, GorgonTexture2D>();

    /// <summary>
    /// Property to return the loaded sprites.
    /// </summary>
    public IReadOnlyDictionary<string, GorgonSprite> Sprites
    {
        get;
        private set;
    } = new Dictionary<string, GorgonSprite>();

    /// <summary>
    /// Property to return the loaded pixel shaders.
    /// </summary>
    public IReadOnlyDictionary<string, GorgonPixelShader> PixelShaders => _pixelShaders;

    /// <summary>
    /// Property to return the loaded vertex shaders.
    /// </summary>
    public IReadOnlyDictionary<string, GorgonVertexShader> VertexShaders => _vertexShaders;

    /// <summary>
    /// Property to return the meshes for the 3D entities.
    /// </summary>
    public IReadOnlyDictionary<string, MoveableMesh> Meshes => _meshes;

    /// <summary>
    /// Property to return the animations for all entities.
    /// </summary>
    public IReadOnlyDictionary<string, IGorgonAnimation> Animations => _animations;

    /// <summary>
    /// Propert to return all of the post process compositors.
    /// </summary>
    public IReadOnlyDictionary<string, Gorgon2DCompositor> PostProcessCompositors => _postProcess;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to load the images for the textures.
    /// </summary>
    /// <param name="path">The path to the textures.</param>
    /// <returns>A list of loaded images.</returns>
    private async Task<IReadOnlyDictionary<string, GorgonTexture2D>> LoadImageData(string path)
    {
        var result = new Dictionary<string, GorgonTexture2D>(StringComparer.OrdinalIgnoreCase);

        IReadOnlyList<IGorgonVirtualFile> textureFiles = _fileSystem.GetContentItems(path, CommonEditorContentTypes.ImageType);

        foreach (IGorgonVirtualFile imagePath in textureFiles)
        {                
            result[imagePath.FullPath] = await _contentLoader.LoadTextureAsync(imagePath.FullPath);
        }

        return result;
    }

    /// <summary>
    /// Function to load the sprites.
    /// </summary>
    /// <param name="path">The path to the sprites.</param>
    /// <returns>A list of loaded sprites.</returns>
    private async Task<IReadOnlyDictionary<string, GorgonSprite>> LoadSpriteDataAsync(string path)
    {
        var result = new Dictionary<string, GorgonSprite>(StringComparer.OrdinalIgnoreCase);

        IReadOnlyList<IGorgonVirtualFile> spriteFiles = _fileSystem.GetContentItems(path, CommonEditorContentTypes.SpriteType);

        foreach (IGorgonVirtualFile spritePath in spriteFiles)
        {
            result[spritePath.FullPath] = await _contentLoader.LoadSpriteAsync(spritePath.FullPath);
        }

        return result;
    }

    /// <summary>
    /// Function to generate animations for the entities in the application.
    /// </summary>
    private void GenerateAnimations()
    {
        var builder = new GorgonAnimationBuilder();

        IGorgonAnimation planetRotation = builder
            .Clear()
            .EditVector3("Rotation")
            .SetKey(new GorgonKeyVector3(0, new Vector3(0, 0, 180)))
            .SetKey(new GorgonKeyVector3(600, new Vector3(0, 360, 180)))
            .SetInterpolationMode(TrackInterpolationMode.Linear)
            .EndEdit()
            .Build("PlanetRotation");

        planetRotation.IsLooped = true;
        _animations[planetRotation.Name] = planetRotation;

        IGorgonAnimation cloudRotation = builder
            .Clear()
            .EditVector3("Rotation")
            .SetKey(new GorgonKeyVector3(0, new Vector3(0, 0, 180)))
            .SetKey(new GorgonKeyVector3(600, new Vector3(100, 180, 180)))
            .SetKey(new GorgonKeyVector3(1200, new Vector3(0, 360, 180)))
            .SetInterpolationMode(TrackInterpolationMode.Linear)
            .EndEdit()                
            .Build("CloudRotation");

        cloudRotation.IsLooped = true;
        _animations[cloudRotation.Name] = cloudRotation;

        GorgonSprite[] frames = Sprites.Where(item => item.Key.StartsWith("/sprites/Fighter_Engine_F", StringComparison.OrdinalIgnoreCase))
                                                  .OrderBy(item => item.Key)
                                                  .Select(item => item.Value)
                                                  .ToArray();

        IGorgonAnimation engineGlow = builder
            .Clear()
            .Edit2DTexture("Texture")
            .SetKey(new GorgonKeyTexture2D(0, frames[0].Texture, frames[0].TextureRegion, frames[0].TextureArrayIndex))
            .SetKey(new GorgonKeyTexture2D(0.1f, frames[1].Texture, frames[1].TextureRegion, frames[1].TextureArrayIndex))
            .SetKey(new GorgonKeyTexture2D(0.2f, frames[2].Texture, frames[2].TextureRegion, frames[2].TextureArrayIndex))
            .SetKey(new GorgonKeyTexture2D(0.3f, frames[1].Texture, frames[1].TextureRegion, frames[1].TextureArrayIndex))
            .EndEdit()
            .Build("EngineGlow");
        engineGlow.IsLooped = true;
        _animations[engineGlow.Name] = engineGlow;
    }

    /// <summary>
    /// Function to build up our post process compositor list.
    /// </summary>
    private void BuildPostProcessCompositors()
    {
        var chromatic = new Gorgon2DChromaticAberrationEffect(_renderer)
        {
            Intensity = 0.125f
        };

        var bloom = new Gorgon2DBloomEffect(_renderer)
        {
            BloomIntensity = 7.0f,
            BlurAmount = 10.0f,
            Color = new GorgonColor(191 / 255.0f, 80 / 255.0f, 21 / 255.0f, 1),
            ColorIntensity = 4,
            Threshold = 1.02f,
            DirtTexture = Textures["/images/LensDirt_2.dds"].GetShaderResourceView(),
            DirtIntensity = 192
        };
        bloom.Precache();

        var finalPostProcess = new Gorgon2DCompositor(_renderer);
        finalPostProcess
            .Clear()
            .InitialClearColor(GorgonColor.BlackTransparent)
            .FinalClearColor(GorgonColor.BlackTransparent)
            .EffectPass("Bloom", bloom)                
            .EffectPass("Chromatic Aberration", chromatic);

        _postProcess["Final Pass"] = finalPostProcess;

        var deferredLighting = new Gorgon2DLightingEffect(_renderer);
        var gbuffer = new Gorgon2DGBuffer(_renderer, 1280, 800);
        deferredLighting.AmbientColor = new GorgonColor(0.025f, 0.025f, 0.025f, 1.0f);
        _effects[nameof(bloom)] = bloom;
        _effects[nameof(chromatic)] = chromatic;
        _effects[nameof(deferredLighting)] = deferredLighting;
        _effects[nameof(gbuffer)] = gbuffer;
    }

    /// <summary>
    /// Function to generate our 3D models for the scene.
    /// </summary>
    /// <remarks>
    /// <para>
    /// In this example we will be mixing 2D and 3D scene elements. So, of course, in 3D, we'll need something to render.  Since this is a space scene, we'll be rendering sphere(s). I've chosen to 
    /// generate these models instead of loading them from disk because it's a pain to write a loader for those things, and I don't focus on 3D much in Gorgon. So, for now we'll use stock primitives.
    /// </para>
    /// </remarks>
    private void Generate3DModels()
    {
        // The 3D model for the earth. It has a diffuse/albedo map, a normal map and a specular map.
        var earthSphere = new IcoSphere(_graphics,
                                        3.0f,
                                        new DX.RectangleF(0, 0, 1, 1),
                                        Vector3.Zero,
                                        3)
        {
            Rotation = new Vector3(0, 45.0f, 0),     // Start off with a 45 degree rotation on the Y axis so we can see our textures a little better on startup.
            Position = Vector3.Zero,                 // Set our translation to nothing for now, our "planet" type will handle positioning.
            Material =
            {
                SpecularPower = 0.0f,
                PixelShader = "PlanetPS",				// The 3D model materials are quite simple.  They just reference their resources by name. We do live in the age of super duper 
                    VertexShader = "PlanetVS",                // fast computers, so doing a few lookups is not going to kill our performance. This allows us to weakly reference the resources 
                    Textures =                                // which in turn keeps things nicely decoupled.
                {
                    [0] = "/images/earthmap1k",
                    [1] = "/images/earthbump1k_NRM.dds",
                    [2] = "/images/earthspec1k"
                }
            }
        };

        // This will serve as the cloud layer, it'll just sit on top of the other sphere to give the appearance of cloud cover moving over the land.
        // This will be additively blended with the land sphere above and use another pixel shader that has a slight emissive effect.
        var earthCloudSphere = new IcoSphere(_graphics,
                                             3.01f,
                                             new DX.RectangleF(0, 0, 1, 1),
                                             Vector3.Zero,
                                             3)
        {
            Position = new Vector3(0, 0, -0.2f),     // Offset the clouds. If we render at the same place in Ortho camera mode, then it'll just overwrite, offsetting like this
            Material =									// ensures that the clouds appear on top.
            {
                SpecularPower = 0,
                PixelShader = "PlanetCloudPS",
                VertexShader = "PlanetVS",
                BlendState = GorgonBlendState.Additive,
                Textures =
                {
                    [0] = "/images/earthcloudmap"
                }
            }
        };

        // Validate our references here so we don't have to wait too long to find out if I messed up.
        // Local functions are one of the best things added to C#. Delphi's had this for centuries.
        void Validate(Mesh mesh)
        {
            if (!_vertexShaders.ContainsKey(mesh.Material.VertexShader))
            {
                throw new GorgonException(GorgonResult.CannotCreate, $"The vertex shader '{mesh.Material.VertexShader}' is missing.");
            }

            if (!_pixelShaders.ContainsKey(mesh.Material.PixelShader))
            {
                throw new GorgonException(GorgonResult.CannotCreate, $"The pixel shader '{mesh.Material.PixelShader}' is missing.");
            }

            for (int i = 0; i < mesh.Material.Textures.Length; ++i)
            {
                if (string.IsNullOrWhiteSpace(mesh.Material.Textures[i]))
                {
                    continue;
                }

                if (!Textures.ContainsKey(mesh.Material.Textures[i]))
                {
                    throw new GorgonException(GorgonResult.CannotCreate, $"The texture '{mesh.Material.Textures[i]}' is missing.");
                }
            }
        }

        Validate(earthSphere);
        Validate(earthCloudSphere);

        _meshes[nameof(earthSphere)] = earthSphere;
        _meshes[nameof(earthCloudSphere)] = earthCloudSphere;
    }

    /// <summary>
    /// Function to load in the file system for the application.
    /// </summary>
    /// <param name="path">The path to the file system.</param>
    public void Load(string path)
    {
        // Load the file system containing our application data (sprites, images, etc...)
        IGorgonFileSystemProviderFactory providerFactory = new GorgonFileSystemProviderFactory(_plugIns, GorgonApplication.Log);
        IGorgonFileSystemProvider provider = providerFactory.CreateProvider(Path.Combine(GorgonExample.GetPlugInPath().FullName, "Gorgon.FileSystem.GorPack.dll"), 
                                                                            "Gorgon.IO.GorPack.GorPackProvider");
        _fileSystem = new GorgonFileSystem(provider, GorgonApplication.Log);
        _fileSystem.Mount(path);

        _contentLoader = _fileSystem.CreateContentLoader(_renderer, _textureCache);            
    }

    /// <summary>
    /// Function to load all the resources from the file system.
    /// </summary>
    /// <returns>A task for asynchronous operation.</returns>
    public async Task LoadResourcesAsync()
    {
        foreach (KeyValuePair<string, GorgonTexture2D> texture in Textures)
        {
            _textureCache.ReturnTexture(texture.Value);
        }


        Textures = await LoadImageData("/images/");

        Sprites = await LoadSpriteDataAsync("/sprites/");

        await Task.Run(() =>
        {
            _vertexShaders["PlanetVS"] = GorgonShaderFactory.Compile<GorgonVertexShader>(_graphics, Resources.Shaders3D, "PrimVS", GorgonGraphics.IsDebugEnabled);
            _pixelShaders["PlanetPS"] = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics, Resources.Shaders3D, "PrimPSBump", GorgonGraphics.IsDebugEnabled);
            _pixelShaders["PlanetCloudPS"] = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics, Resources.Shaders3D, "PrimPSNoBump", GorgonGraphics.IsDebugEnabled);
        });

        await Task.Run(() => Generate3DModels());

        await Task.Run(() => BuildPostProcessCompositors());

        // No need to make this async, should be plenty fast.
        GenerateAnimations();
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        _textureCache.Dispose();

        foreach (KeyValuePair<string, GorgonVertexShader> shader in _vertexShaders)
        {
            shader.Value.Dispose();
        }

        foreach (KeyValuePair<string, GorgonPixelShader> shader in _pixelShaders)
        {
            shader.Value.Dispose();
        }

        foreach (KeyValuePair<string, MoveableMesh> mesh in _meshes)
        {
            mesh.Value.Dispose();
        }

        foreach (KeyValuePair<string, Gorgon2DEffect> effect in _effects)
        {
            effect.Value.Dispose();
        }

        foreach (KeyValuePair<string, Gorgon2DCompositor> compositor in _postProcess)
        {
            compositor.Value.Dispose();
        }
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the <see cref="ResourceManagement"/> class.</summary>
    /// <param name="renderer">The renderer for the application.</param>
    /// <param name="plugIns>The plugin service used to load file system providers.</param>
    public ResourceManagement(Gorgon2D renderer, GorgonMefPlugInCache plugIns)
    {
        _renderer = renderer;
        _graphics = renderer.Graphics;
        _plugIns = plugIns;
        _textureCache = new GorgonTextureCache<GorgonTexture2D>(renderer.Graphics);
    }
    #endregion
}
