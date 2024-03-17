
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
// Created: April 24, 2019 1:54:37 PM
// 


using System.Reflection;
using Gorgon.Diagnostics;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.IO;
using Gorgon.PlugIns;
using Gorgon.Renderers;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// A registry for the sprite codecs used by the plug ins in this assembly
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="CodecRegistry"/> class.</remarks>
/// <param name="pluginCache">The cache of plug in assemblies.</param>
/// <param name="renderer">The 2D renderer for the application.</param>
/// <param name="log">The log for debug output.</param>
internal class CodecRegistry(GorgonMefPlugInCache pluginCache, Gorgon2D renderer, IGorgonLog log)
{

    // The cache containing the plug in assemblies.
    private readonly GorgonMefPlugInCache _pluginCache = pluginCache;
    // The service used to manage the plug ins.
    private readonly IGorgonPlugInService _pluginService = new GorgonMefPlugInService(pluginCache);
    // The log.
    private readonly IGorgonLog _log = log;
    // The 2D renderer for the application.
    private readonly Gorgon2D _renderer = renderer;



    /// <summary>
    /// Property to return the list of codecs.
    /// </summary>
    public IList<IGorgonSpriteCodec> Codecs
    {
        get;
    } = [];

    /// <summary>
    /// Property to return the codecs cross referenced with known file extension types.
    /// </summary>
    public IList<(GorgonFileExtension extension, IGorgonSpriteCodec codec)> CodecFileTypes
    {
        get;
    } = [];

    /// <summary>
    /// Property to return the list of sprite codec plug ins.
    /// </summary>
    public IList<GorgonSpriteCodecPlugIn> CodecPlugIns
    {
        get;
    } = [];



    /// <summary>
    /// Function to load external sprite codec plug ins.
    /// </summary>
    /// <param name="settings">The settings containing the plug in path.</param>
    private void LoadCodecPlugIns(SpriteImportSettings settings)
    {
        if (settings.CodecPlugInPaths.Count == 0)
        {
            return;
        }

        _log.Print("Loading sprite codecs...", LoggingLevel.Intermediate);

        IReadOnlyList<PlugInAssemblyState> assemblies = _pluginCache.ValidateAndLoadAssemblies(settings.CodecPlugInPaths.Select(item => item.Value), _log);

        if (assemblies.Count == 0)
        {
            _log.Print("Sprite codec plug in assemblies were not loaded. There may not have been any plug assemblies, or they may already be referenced.", LoggingLevel.Verbose);
        }

        // Load all the codecs contained within the plug in (a plug in can have multiple codecs).
        foreach (GorgonSpriteCodecPlugIn plugin in _pluginService.GetPlugIns<GorgonSpriteCodecPlugIn>())
        {
            foreach (GorgonSpriteCodecDescription desc in plugin.Codecs)
            {
                CodecPlugIns.Add(plugin);

                if (Codecs.Any(item => string.Equals(item.GetType().FullName, desc.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    _log.Print($"WARNING: The sprite codec '{desc.Name}' is already loaded, skipping this one...", LoggingLevel.Verbose);
                    continue;
                }

                IGorgonSpriteCodec codec = plugin.CreateCodec(desc.Name, _renderer);

                if (codec is null)
                {
                    _log.Print($"ERROR: The sprite codec '{desc.Name}' was not created (returned NULL).", LoggingLevel.Simple);
                    continue;
                }

                Codecs.Add(codec);
            }
        }
    }

    /// <summary>
    /// Function to remove an sprite codec plug in from the registry.
    /// </summary>
    /// <param name="plugin">The plug in to remove.</param>
    public void RemoveCodecPlugIn(GorgonSpriteCodecPlugIn plugin)
    {
        if (plugin is null)
        {
            throw new ArgumentNullException(nameof(plugin));
        }

        if (!CodecPlugIns.Contains(plugin))
        {
            return;
        }

        foreach (GorgonSpriteCodecDescription desc in plugin.Codecs)
        {
            IGorgonSpriteCodec codec = Codecs.FirstOrDefault(item => string.Equals(item.GetType().FullName, desc.Name, StringComparison.OrdinalIgnoreCase));

            if (codec is not null)
            {
                Codecs.Remove(codec);
            }

            (GorgonFileExtension extension, IGorgonSpriteCodec codecType)[] types = CodecFileTypes.Where(item => item.codec == codec).ToArray();

            foreach ((GorgonFileExtension extension, IGorgonSpriteCodec codecType) type in types)
            {
                CodecFileTypes.Remove(type);
            }
        }

        _pluginService.Unload(plugin.Name);

        CodecPlugIns.Remove(plugin);
    }

    /// <summary>
    /// Function to add a codec to the registry.
    /// </summary>
    /// <param name="path">The path to the codec assembly.</param>
    /// <param name="errors">A list of errors if the plug in fails to load.</param>
    /// <returns>A list of codec plugs ins that were loaded.</returns>
    public IReadOnlyList<GorgonSpriteCodecPlugIn> AddCodecPlugIn(string path, out IReadOnlyList<string> errors)
    {
        List<string> localErrors = [];
        errors = localErrors;

        List<GorgonSpriteCodecPlugIn> result = [];
        _log.Print("Loading sprite codecs...", LoggingLevel.Intermediate);

        IReadOnlyList<PlugInAssemblyState> assemblies = _pluginCache.ValidateAndLoadAssemblies([path], _log);

        if (assemblies.Count == 0)
        {
            _log.Print("Assembly was not loaded. This means that most likely it's already referenced.", LoggingLevel.Verbose);
        }

        IEnumerable<PlugInAssemblyState> failedAssemblies = assemblies.Where(item => !item.IsAssemblyLoaded);

        foreach (PlugInAssemblyState failure in failedAssemblies)
        {
            localErrors.Add(failure.LoadFailureReason);
        }

        if (localErrors.Count > 0)
        {
            return result;
        }

        // Since we can't unload an assembly, we'll have to force a rescan of the plug ins. We may have unloaded one prior, and we might need to get it back.
        _pluginService.ScanPlugIns();
        AssemblyName assemblyName = AssemblyName.GetAssemblyName(path);
        IReadOnlyList<GorgonSpriteCodecPlugIn> pluginList = _pluginService.GetPlugIns<GorgonSpriteCodecPlugIn>(assemblyName);

        if (pluginList.Count == 0)
        {
            localErrors.Add(string.Format(Resources.GORSPR_ERR_NO_CODECS, Path.GetFileName(path)));
            return result;
        }

        // Load all the codecs contained within the plug in (a plug in can have multiple codecs).
        foreach (GorgonSpriteCodecPlugIn plugin in pluginList)
        {
            if (CodecPlugIns.Any(item => string.Equals(plugin.Name, item.Name, StringComparison.OrdinalIgnoreCase)))
            {
                _log.Print($"WARNING: Codec plug in '{plugin.Name}' is already loaded.", LoggingLevel.Intermediate);
                localErrors.Add(string.Format(Resources.GORSPR_ERR_CODEC_PLUGIN_ALREADY_LOADED, plugin.Name));
                continue;
            }

            CodecPlugIns.Add(plugin);
            int count = plugin.Codecs.Count;

            foreach (GorgonSpriteCodecDescription desc in plugin.Codecs)
            {
                if (Codecs.Any(item => string.Equals(desc.Name, item.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    _log.Print($"WARNING: Codec '{desc.Name}' is already loaded.", LoggingLevel.Intermediate);
                    localErrors.Add(string.Format(Resources.GORSPR_ERR_CODEC_ALREADY_LOADED, desc.Name));
                    --count;
                    continue;
                }

                IGorgonSpriteCodec spriteCodec = plugin.CreateCodec(desc.Name, _renderer);

                if (spriteCodec is null)
                {
                    _log.Print($"ERROR: Could not create sprite codec '{desc.Name}' from plug in '{plugin.PlugInPath}'.", LoggingLevel.Verbose);
                    localErrors.Add(string.Format(Resources.GORSPR_ERR_CODEC_LOAD_FAIL, desc.Name));
                    --count;
                    continue;
                }

                Codecs.Add(spriteCodec);

                foreach (GorgonFileExtension extension in spriteCodec.FileExtensions)
                {
                    (GorgonFileExtension, IGorgonSpriteCodec) codecExtension = (extension, spriteCodec);
                    CodecFileTypes.Add(codecExtension);
                }
            }

            if (count > 0)
            {
                result.Add(plugin);
            }
        }

        return result;
    }

    /// <summary>
    /// Function to load the codecs from our settings data.
    /// </summary>
    /// <param name="settings">The settings containing the plug in paths.</param>
    public void LoadFromSettings(SpriteImportSettings settings)
    {
        Codecs.Clear();
        CodecFileTypes.Clear();

        // Get built-in codec list.
        Codecs.Add(new GorgonV3SpriteBinaryCodec(_renderer));
        Codecs.Add(new GorgonV3SpriteJsonCodec(_renderer));
        Codecs.Add(new GorgonV2SpriteCodec(_renderer));
        Codecs.Add(new GorgonV1SpriteBinaryCodec(_renderer));

        LoadCodecPlugIns(settings);

        IEnumerable<(GorgonFileExtension extension, IGorgonSpriteCodec codec)> codecGrouping = from codec in Codecs
                                                                                               from extension in codec.FileExtensions
                                                                                               select (extension, codec);

        foreach ((GorgonFileExtension extension, IGorgonSpriteCodec codec) codecFile in codecGrouping)
        {
            CodecFileTypes.Add(codecFile);
        }
    }


}
