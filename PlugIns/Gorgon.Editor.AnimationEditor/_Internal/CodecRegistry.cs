
// 
// Gorgon
// Copyright (C) 2025 Michael Winsor
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
using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Editor.Plugins;
using Gorgon.IO;
using Gorgon.Plugins;
using Gorgon.Renderers;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// A registry for the animation codecs used by the plugins in this assembly
/// </summary>
/// <remarks>Initializes a new instance of the <see cref="CodecRegistry"/> class.</remarks>
/// <param name="pluginCache">The cache of plugin assemblies.</param>
/// <param name="renderer">The 2D renderer for the application.</param>
/// <param name="log">The log for debug output.</param>
internal class CodecRegistry(GorgonMefPluginCache pluginCache, Gorgon2D renderer, IGorgonLog log)
{

    // The cache containing the plugin assemblies.
    private readonly GorgonMefPluginCache _pluginCache = pluginCache;
    // The service used to manage the plugins.
    private readonly IGorgonPluginService _pluginService = new GorgonMefPluginService(pluginCache);
    // The log.
    private readonly IGorgonLog _log = log;
    // The 2D renderer for the application.
    private readonly Gorgon2D _renderer = renderer;

    /// <summary>
    /// Property to return the list of codecs.
    /// </summary>
    public IList<IGorgonAnimationCodec> Codecs
    {
        get;
    } = [];

    /// <summary>
    /// Property to return the codecs cross referenced with known file extension types.
    /// </summary>
    public IList<(GorgonFileExtension extension, IGorgonAnimationCodec codec)> CodecFileTypes
    {
        get;
    } = [];

    /// <summary>
    /// Property to return the list of animation codec plugins.
    /// </summary>
    public IList<GorgonAnimationCodecPlugin> CodecPlugins
    {
        get;
    } = [];

    /// <summary>
    /// Function to load external animation codec plugins.
    /// </summary>
    /// <param name="settings">The settings containing the plugin path.</param>
    private void LoadCodecPlugins(AnimationImportSettings settings)
    {
        if (settings.CodecPluginPaths.Count == 0)
        {
            return;
        }

        _log.Print("Loading animation codecs...", LoggingLevel.Intermediate);

        IReadOnlyList<PluginAssemblyState> assemblies = _pluginCache.ValidateAndLoadAssemblies(settings.CodecPluginPaths.Select(item => item.Value), _log);

        if (assemblies.Count == 0)
        {
            _log.Print("Animation codec plugin assemblies were not loaded. There may not have been any plug assemblies, or they may already be referenced.", LoggingLevel.Verbose);
        }

        // Load all the codecs contained within the plugin (a plugin can have multiple codecs).
        foreach (GorgonAnimationCodecPlugin Plugin in _pluginService.GetPlugins<GorgonAnimationCodecPlugin>())
        {
            foreach (GorgonAnimationCodecDescription desc in Plugin.Codecs)
            {
                CodecPlugins.Add(Plugin);

                if (Codecs.Any(item => string.Equals(item.GetType().FullName, desc.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    _log.PrintWarning($"The animation codec '{desc.Name}' is already loaded, skipping this one...", LoggingLevel.Verbose);
                    continue;
                }

                IGorgonAnimationCodec codec = Plugin.CreateCodec(desc.Name, _renderer);

                if (codec is null)
                {
                    _log.PrintError($"The animation codec '{desc.Name}' was not created (returned NULL).", LoggingLevel.Simple);
                    continue;
                }

                Codecs.Add(codec);
            }
        }
    }

    /// <summary>
    /// Function to remove an animation codec plugin from the registry.
    /// </summary>
    /// <param name="plugin">The plugin to remove.</param>
    public void RemoveCodecPlugin(GorgonAnimationCodecPlugin plugin)
    {
        if (plugin is null)
        {
            throw new ArgumentNullException(nameof(plugin));
        }

        if (!CodecPlugins.Contains(plugin))
        {
            return;
        }

        foreach (GorgonAnimationCodecDescription desc in plugin.Codecs)
        {
            IGorgonAnimationCodec codec = Codecs.FirstOrDefault(item => string.Equals(item.GetType().FullName, desc.Name, StringComparison.OrdinalIgnoreCase));

            if (codec is not null)
            {
                Codecs.Remove(codec);
            }

            (GorgonFileExtension extension, IGorgonAnimationCodec codecType)[] types = [.. CodecFileTypes.Where(item => item.codec == codec)];

            foreach ((GorgonFileExtension extension, IGorgonAnimationCodec codecType) type in types)
            {
                CodecFileTypes.Remove(type);
            }
        }

        _pluginService.Unload(plugin.Name);

        CodecPlugins.Remove(plugin);
    }

    /// <summary>
    /// Function to add a codec to the registry.
    /// </summary>
    /// <param name="path">The path to the codec assembly.</param>
    /// <param name="errors">A list of errors if the plugin fails to load.</param>
    /// <returns>A list of codec plugs ins that were loaded.</returns>
    public IReadOnlyList<GorgonAnimationCodecPlugin> AddCodecPlugin(string path, out IReadOnlyList<string> errors)
    {
        List<string> localErrors = [];
        errors = localErrors;

        List<GorgonAnimationCodecPlugin> result = [];
        _log.Print("Loading animation codecs...", LoggingLevel.Intermediate);

        IReadOnlyList<PluginAssemblyState> assemblies = _pluginCache.ValidateAndLoadAssemblies([path], _log);

        if (assemblies.Count == 0)
        {
            _log.Print("Assembly was not loaded. This means that most likely it's already referenced.", LoggingLevel.Verbose);
        }

        IEnumerable<PluginAssemblyState> failedAssemblies = assemblies.Where(item => !item.IsAssemblyLoaded);

        foreach (PluginAssemblyState failure in failedAssemblies)
        {
            localErrors.Add(failure.LoadFailureReason);
        }

        if (localErrors.Count > 0)
        {
            return result;
        }

        // Since we can't unload an assembly, we'll have to force a rescan of the plugins. We may have unloaded one prior, and we might need to get it back.
        _pluginService.ScanPlugins();
        AssemblyName assemblyName = AssemblyName.GetAssemblyName(path);
        IReadOnlyList<GorgonAnimationCodecPlugin> PluginList = _pluginService.GetPlugins<GorgonAnimationCodecPlugin>(assemblyName);

        if (PluginList.Count == 0)
        {
            localErrors.Add(string.Format(Resources.GORANM_ERR_NO_CODECS, Path.GetFileName(path)));
            return result;
        }

        // Load all the codecs contained within the plugin (a plugin can have multiple codecs).
        foreach (GorgonAnimationCodecPlugin Plugin in PluginList)
        {
            if (CodecPlugins.Any(item => string.Equals(Plugin.Name, item.Name, StringComparison.OrdinalIgnoreCase)))
            {
                _log.PrintWarning($"Codec plugin '{Plugin.Name}' is already loaded.", LoggingLevel.Intermediate);
                localErrors.Add(string.Format(Resources.GORANM_ERR_CODEC_plugin_ALREADY_LOADED, Plugin.Name));
                continue;
            }

            CodecPlugins.Add(Plugin);
            int count = Plugin.Codecs.Count;

            foreach (GorgonAnimationCodecDescription desc in Plugin.Codecs)
            {
                if (Codecs.Any(item => string.Equals(desc.Name, item.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    _log.PrintWarning($"Codec '{desc.Name}' is already loaded.", LoggingLevel.Intermediate);
                    localErrors.Add(string.Format(Resources.GORANM_ERR_CODEC_ALREADY_LOADED, desc.Name));
                    --count;
                    continue;
                }

                IGorgonAnimationCodec animationCodec = Plugin.CreateCodec(desc.Name, _renderer);

                if (animationCodec is null)
                {
                    _log.PrintError($"Could not create animation codec '{desc.Name}' from plugin '{Plugin.PluginPath}'.", LoggingLevel.Verbose);
                    localErrors.Add(string.Format(Resources.GORANM_ERR_CODEC_LOAD_FAIL, desc.Name));
                    --count;
                    continue;
                }

                Codecs.Add(animationCodec);

                foreach (GorgonFileExtension extension in animationCodec.FileExtensions)
                {
                    (GorgonFileExtension, IGorgonAnimationCodec) codecExtension = (extension, animationCodec);
                    CodecFileTypes.Add(codecExtension);
                }
            }

            if (count > 0)
            {
                result.Add(Plugin);
            }
        }

        return result;
    }

    /// <summary>
    /// Function to load the codecs from our settings data.
    /// </summary>
    /// <param name="settings">The settings containing the plugin paths.</param>
    public void LoadFromSettings(AnimationImportSettings settings)
    {
        Codecs.Clear();
        CodecFileTypes.Clear();

        // Get built-in codec list.
        Codecs.Add(new GorgonV31AnimationBinaryCodec(_renderer));
        Codecs.Add(new GorgonV31AnimationJsonCodec(_renderer));

        LoadCodecPlugins(settings);

        IEnumerable<(GorgonFileExtension extension, IGorgonAnimationCodec codec)> codecGrouping = from codec in Codecs
                                                                                                  from extension in codec.FileExtensions
                                                                                                  select (extension, codec);

        foreach ((GorgonFileExtension extension, IGorgonAnimationCodec codec) codecFile in codecGrouping)
        {
            CodecFileTypes.Add(codecFile);
        }
    }
}
