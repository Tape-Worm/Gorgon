using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Gorgon.Diagnostics;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.PlugIns;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.PlugIns;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// A registry for the image codecs used by the plug ins in this assembly.
    /// </summary>
    internal class CodecRegistry
		: ICodecRegistry
    {
        #region Variables.
        // The cache containing the plug in assemblies.
        private readonly GorgonMefPlugInCache _pluginCache;
		// The service used to manage the plug ins.
        private readonly IGorgonPlugInService _pluginService;
        // The log.
        private readonly IGorgonLog _log;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the list of codecs.
        /// </summary>
        public IList<IGorgonImageCodec> Codecs
        {
            get;
        } = new List<IGorgonImageCodec>();

        /// <summary>
        /// Property to return the codecs cross referenced with known file extension types.
        /// </summary>
        public IList<(GorgonFileExtension extension, IGorgonImageCodec codec)> CodecFileTypes
        {
            get;
        } = new List<(GorgonFileExtension extension, IGorgonImageCodec codec)>();

        /// <summary>
        /// Property to return the list of image codec plug ins.
        /// </summary>
        public IList<GorgonImageCodecPlugIn> CodecPlugIns
        {
            get;
        } = new List<GorgonImageCodecPlugIn>();
        #endregion

        #region Methods.
        /// <summary>
        /// Function to load external image codec plug ins.
        /// </summary>
        /// <param name="settings">The settings containing the plug in path.</param>
        private void LoadCodecPlugIns(ImageEditorSettings settings)
        {
            if (settings.CodecPlugInPaths.Count == 0)
            {
                return;
            }

            _log.Print("Loading image codecs...", LoggingLevel.Intermediate);

            IReadOnlyList<PlugInAssemblyState> assemblies = _pluginCache.ValidateAndLoadAssemblies(settings.CodecPlugInPaths.Select(item => new FileInfo(item.Value)), _log);

            if (assemblies.Count == 0)
            {
                _log.Print("Image codec plug in assemblies were not loaded. There may not have been any plug assemblies, or they may already be referenced.", LoggingLevel.Verbose);
            }

            // Load all the codecs contained within the plug in (a plug in can have multiple codecs).
            foreach (GorgonImageCodecPlugIn plugin in _pluginService.GetPlugIns<GorgonImageCodecPlugIn>())
            {
                foreach (GorgonImageCodecDescription desc in plugin.Codecs)
                {
                    CodecPlugIns.Add(plugin);

                    if (Codecs.Any(item => string.Equals(item.GetType().FullName, desc.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        _log.Print($"[WARNING] The image codec '{desc.Name}' is already loaded, skipping this one...", LoggingLevel.Verbose);
                        continue;
                    }

                    IGorgonImageCodec codec = plugin.CreateCodec(desc.Name);

                    if (codec == null)
                    {
                        _log.Print($"[ERROR] The image codec '{desc.Name}' was not created (returned NULL).", LoggingLevel.Simple);
                        continue;
                    }

                    Codecs.Add(codec);
                }
            }
        }

        /// <summary>
        /// Function to remove an image codec plug in from the registry.
        /// </summary>
        /// <param name="plugin">The plug in to remove.</param>
        public void RemoveCodecPlugIn(GorgonImageCodecPlugIn plugin)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            if (!CodecPlugIns.Contains(plugin))
            {
                return;
            }

            foreach (GorgonImageCodecDescription desc in plugin.Codecs)
            {
                IGorgonImageCodec codec = Codecs.FirstOrDefault(item => string.Equals(item.GetType().FullName, desc.Name, StringComparison.OrdinalIgnoreCase));

                if (codec != null)
                {
                    Codecs.Remove(codec);
                }

                (GorgonFileExtension extension, IGorgonImageCodec codecType)[] types = CodecFileTypes.Where(item => item.codec == codec).ToArray();

                foreach ((GorgonFileExtension extension, IGorgonImageCodec codecType) type in types)
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
        public IReadOnlyList<GorgonImageCodecPlugIn> AddCodecPlugIn(string path, out IReadOnlyList<string> errors)
        {
            var localErrors = new List<string>();
            errors = localErrors;

            var result = new List<GorgonImageCodecPlugIn>();
            _log.Print("Loading image codecs...", LoggingLevel.Intermediate);

            IReadOnlyList<PlugInAssemblyState> assemblies = _pluginCache.ValidateAndLoadAssemblies(new[] { new FileInfo(path) }, _log);

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
            IReadOnlyList<GorgonImageCodecPlugIn> pluginList = _pluginService.GetPlugIns<GorgonImageCodecPlugIn>(assemblyName);

            if (pluginList.Count == 0)
            {
                localErrors.Add(string.Format(Resources.GORIMG_ERR_NO_CODECS, Path.GetFileName(path)));
                return result;
            }

            // Load all the codecs contained within the plug in (a plug in can have multiple codecs).
            foreach (GorgonImageCodecPlugIn plugin in pluginList)
            {
                if (CodecPlugIns.Any(item => string.Equals(plugin.Name, item.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    _log.Print($"[WARNING] Codec plug in '{plugin.Name}' is already loaded.", LoggingLevel.Intermediate);
                    localErrors.Add(string.Format(Resources.GORIMG_ERR_CODEC_PLUGIN_ALREADY_LOADED, plugin.Name));
                    continue;
                }

                CodecPlugIns.Add(plugin);
                int count = plugin.Codecs.Count;

                foreach (GorgonImageCodecDescription desc in plugin.Codecs)
                {
                    if (Codecs.Any(item => string.Equals(desc.Name, item.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        _log.Print($"[WARNING] Codec '{desc.Name}' is already loaded.", LoggingLevel.Intermediate);
                        localErrors.Add(string.Format(Resources.GORIMG_ERR_CODEC_ALREADY_LOADED, desc.Name));
                        --count;
                        continue;
                    }

                    IGorgonImageCodec imageCodec = plugin.CreateCodec(desc.Name);

                    if (imageCodec == null)
                    {
                        _log.Print($"[ERROR] Could not create image codec '{desc.Name}' from plug in '{plugin.PlugInPath}'.", LoggingLevel.Verbose);
                        localErrors.Add(string.Format(Resources.GORIMG_ERR_CODEC_LOAD_FAIL, desc.Name));
                        --count;
                        continue;
                    }

                    Codecs.Add(imageCodec);

                    foreach (string extension in imageCodec.CodecCommonExtensions)
                    {
                        (GorgonFileExtension fileExtension, IGorgonImageCodec) codecExtension = (new GorgonFileExtension(extension), imageCodec);

                        if (CodecFileTypes.Any(item => item.extension.Equals(codecExtension.fileExtension)))
                        {
                            _log.Print($"[WARNING] Another previously loaded codec already uses the file extension '{extension}'.  This file extension will not be registered to the '{imageCodec.Name}' codec.", LoggingLevel.Verbose);
                            continue;
                        }

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
        public void LoadFromSettings(ImageEditorSettings settings)
        {
            Codecs.Clear();
            CodecFileTypes.Clear();

            // Get built-in codec list.
            Codecs.Add(new GorgonCodecPng());
            Codecs.Add(new GorgonCodecJpeg());
            Codecs.Add(new GorgonCodecTga());
            Codecs.Add(new GorgonCodecBmp());
            Codecs.Add(new GorgonCodecGif());

            LoadCodecPlugIns(settings);

            foreach (IGorgonImageCodec codec in Codecs)
            {
                foreach (string extension in codec.CodecCommonExtensions)
                {
                    (GorgonFileExtension fileExtension, IGorgonImageCodec) codecExtension = (new GorgonFileExtension(extension), codec);

                    if (CodecFileTypes.Any(item => item.extension.Equals(codecExtension.fileExtension)))
                    {
                        _log.Print($"[WARNING] Another previously loaded codec already uses the file extension '{extension}'.  This file extension will not be registered to the '{codec.Name}' codec.", LoggingLevel.Verbose);
                        continue;
                    }

                    CodecFileTypes.Add(codecExtension);
                }
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="CodecRegistry"/> class.</summary>
        /// <param name="pluginCache">The cache of plug in assemblies.</param>
        /// <param name="log">The log for debug output.</param>
        public CodecRegistry(GorgonMefPlugInCache pluginCache, IGorgonLog log)
        {
            _pluginCache = pluginCache;
            _pluginService = new GorgonMefPlugInService(pluginCache);
            _log = log;
        }
        #endregion
    }
}
