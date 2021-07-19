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
// Created: April 24, 2019 1:54:37 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Gorgon.Diagnostics;
using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Editor.PlugIns;
using Gorgon.IO;
using Gorgon.PlugIns;
using Gorgon.Renderers;

namespace Gorgon.Editor.AnimationEditor
{
    /// <summary>
    /// A registry for the animation codecs used by the plug ins in this assembly.
    /// </summary>
    internal class CodecRegistry
    {
        #region Variables.
        // The cache containing the plug in assemblies.
        private readonly GorgonMefPlugInCache _pluginCache;
        // The service used to manage the plug ins.
        private readonly IGorgonPlugInService _pluginService;
        // The log.
        private readonly IGorgonLog _log;
        // The 2D renderer for the application.
        private readonly Gorgon2D _renderer;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the list of codecs.
        /// </summary>
        public IList<IGorgonAnimationCodec> Codecs
        {
            get;
        } = new List<IGorgonAnimationCodec>();

        /// <summary>
        /// Property to return the codecs cross referenced with known file extension types.
        /// </summary>
        public IList<(GorgonFileExtension extension, IGorgonAnimationCodec codec)> CodecFileTypes
        {
            get;
        } = new List<(GorgonFileExtension extension, IGorgonAnimationCodec codec)>();

        /// <summary>
        /// Property to return the list of animation codec plug ins.
        /// </summary>
        public IList<GorgonAnimationCodecPlugIn> CodecPlugIns
        {
            get;
        } = new List<GorgonAnimationCodecPlugIn>();
        #endregion

        #region Methods.
        /// <summary>
        /// Function to load external animation codec plug ins.
        /// </summary>
        /// <param name="settings">The settings containing the plug in path.</param>
        private void LoadCodecPlugIns(AnimationImportSettings settings)
        {
            if (settings.CodecPlugInPaths.Count == 0)
            {
                return;
            }

            _log.Print("Loading animation codecs...", LoggingLevel.Intermediate);

            IReadOnlyList<PlugInAssemblyState> assemblies = _pluginCache.ValidateAndLoadAssemblies(settings.CodecPlugInPaths.Select(item => item.Value), _log);

            if (assemblies.Count == 0)
            {
                _log.Print("Animation codec plug in assemblies were not loaded. There may not have been any plug assemblies, or they may already be referenced.", LoggingLevel.Verbose);
            }

            // Load all the codecs contained within the plug in (a plug in can have multiple codecs).
            foreach (GorgonAnimationCodecPlugIn plugin in _pluginService.GetPlugIns<GorgonAnimationCodecPlugIn>())
            {
                foreach (GorgonAnimationCodecDescription desc in plugin.Codecs)
                {
                    CodecPlugIns.Add(plugin);

                    if (Codecs.Any(item => string.Equals(item.GetType().FullName, desc.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        _log.Print($"WARNING: The animation codec '{desc.Name}' is already loaded, skipping this one...", LoggingLevel.Verbose);
                        continue;
                    }

                    IGorgonAnimationCodec codec = plugin.CreateCodec(desc.Name, _renderer);

                    if (codec is null)
                    {
                        _log.Print($"ERROR: The animation codec '{desc.Name}' was not created (returned NULL).", LoggingLevel.Simple);
                        continue;
                    }

                    Codecs.Add(codec);
                }
            }
        }

        /// <summary>
        /// Function to remove an animation codec plug in from the registry.
        /// </summary>
        /// <param name="plugin">The plug in to remove.</param>
        public void RemoveCodecPlugIn(GorgonAnimationCodecPlugIn plugin)
        {
            if (plugin is null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            if (!CodecPlugIns.Contains(plugin))
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

                (GorgonFileExtension extension, IGorgonAnimationCodec codecType)[] types = CodecFileTypes.Where(item => item.codec == codec).ToArray();

                foreach ((GorgonFileExtension extension, IGorgonAnimationCodec codecType) type in types)
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
        public IReadOnlyList<GorgonAnimationCodecPlugIn> AddCodecPlugIn(string path, out IReadOnlyList<string> errors)
        {
            var localErrors = new List<string>();
            errors = localErrors;

            var result = new List<GorgonAnimationCodecPlugIn>();
            _log.Print("Loading animation codecs...", LoggingLevel.Intermediate);

            IReadOnlyList<PlugInAssemblyState> assemblies = _pluginCache.ValidateAndLoadAssemblies(new[] { path }, _log);

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
            var assemblyName = AssemblyName.GetAssemblyName(path);
            IReadOnlyList<GorgonAnimationCodecPlugIn> pluginList = _pluginService.GetPlugIns<GorgonAnimationCodecPlugIn>(assemblyName);

            if (pluginList.Count == 0)
            {
                localErrors.Add(string.Format(Resources.GORANM_ERR_NO_CODECS, Path.GetFileName(path)));
                return result;
            }

            // Load all the codecs contained within the plug in (a plug in can have multiple codecs).
            foreach (GorgonAnimationCodecPlugIn plugin in pluginList)
            {
                if (CodecPlugIns.Any(item => string.Equals(plugin.Name, item.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    _log.Print($"WARNING: Codec plug in '{plugin.Name}' is already loaded.", LoggingLevel.Intermediate);
                    localErrors.Add(string.Format(Resources.GORANM_ERR_CODEC_PLUGIN_ALREADY_LOADED, plugin.Name));
                    continue;
                }

                CodecPlugIns.Add(plugin);
                int count = plugin.Codecs.Count;

                foreach (GorgonAnimationCodecDescription desc in plugin.Codecs)
                {
                    if (Codecs.Any(item => string.Equals(desc.Name, item.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        _log.Print($"WARNING: Codec '{desc.Name}' is already loaded.", LoggingLevel.Intermediate);
                        localErrors.Add(string.Format(Resources.GORANM_ERR_CODEC_ALREADY_LOADED, desc.Name));
                        --count;
                        continue;
                    }

                    IGorgonAnimationCodec animationCodec = plugin.CreateCodec(desc.Name, _renderer);

                    if (animationCodec is null)
                    {
                        _log.Print($"ERROR: Could not create animation codec '{desc.Name}' from plug in '{plugin.PlugInPath}'.", LoggingLevel.Verbose);
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
                    result.Add(plugin);
                }
            }

            return result;
        }

        /// <summary>
        /// Function to load the codecs from our settings data.
        /// </summary>
        /// <param name="settings">The settings containing the plug in paths.</param>
        public void LoadFromSettings(AnimationImportSettings settings)
        {
            Codecs.Clear();
            CodecFileTypes.Clear();

            // Get built-in codec list.
            Codecs.Add(new GorgonV31AnimationBinaryCodec(_renderer));
            Codecs.Add(new GorgonV31AnimationJsonCodec(_renderer));
            Codecs.Add(new GorgonV3AnimationBinaryCodec(_renderer));
            Codecs.Add(new GorgonV3AnimationJsonCodec(_renderer));

            LoadCodecPlugIns(settings);

            IEnumerable<(GorgonFileExtension extension, IGorgonAnimationCodec codec)> codecGrouping = from codec in Codecs
                                                                                                   from extension in codec.FileExtensions
                                                                                                   select (extension, codec);

            foreach ((GorgonFileExtension extension, IGorgonAnimationCodec codec) codecFile in codecGrouping)
            {
                CodecFileTypes.Add(codecFile);
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="CodecRegistry"/> class.</summary>
        /// <param name="pluginCache">The cache of plug in assemblies.</param>
        /// <param name="renderer">The 2D renderer for the application.</param>
        /// <param name="log">The log for debug output.</param>
        public CodecRegistry(GorgonMefPlugInCache pluginCache, Gorgon2D renderer, IGorgonLog log)
        {
            _pluginCache = pluginCache;
            _renderer = renderer;
            _pluginService = new GorgonMefPlugInService(pluginCache);
            _log = log;
        }
        #endregion
    }
}
