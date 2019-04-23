using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

            IReadOnlyList<PlugInRecord> assemblies = _pluginCache.ValidateAndLoadAssemblies(settings.CodecPlugInPaths.Select(item => new FileInfo(item.Value)), _log);

            if (assemblies.Count == 0)
            {
                _log.Print("No image codecs found.", LoggingLevel.Verbose);
                return;
            }

            IGorgonPlugInService plugins = new GorgonMefPlugInService(_pluginCache, _log);

            // Load all the codecs contained within the plug in (a plug in can have multiple codecs).
            foreach (GorgonImageCodecPlugIn plugin in plugins.GetPlugIns<GorgonImageCodecPlugIn>())
            {
                foreach (GorgonImageCodecDescription desc in plugin.Codecs)
                {
                    CodecPlugIns.Add(plugin);
                    Codecs.Add(plugin.CreateCodec(desc.Name));
                }
            }
        }

        /// <summary>
        /// Function to remove an image codec from the registry.
        /// </summary>
        /// <param name="plugin">The codec to remove.</param>
        public void RemoveCodec(IGorgonImageCodec codec)
        {
            if (codec == null)
            {
                throw new ArgumentNullException(nameof(codec));
            }

            if (!Codecs.Contains(codec))
            {
                return;
            }

            (GorgonFileExtension extension, IGorgonImageCodec codecType)[] types = CodecFileTypes
                                                                                    .Where(item => item.codec == codec)
                                                                                    .ToArray();

            foreach ((GorgonFileExtension extension, IGorgonImageCodec codecType) type in types)
            {
                CodecFileTypes.Remove(type);
            }

            Codecs.Remove(codec);

            GorgonImageCodecDescription[] descs = CodecPlugIns
                    .SelectMany(item => item.Codecs)
                    .Where(item => string.Equals(item.Name, codec.GetType().FullName, StringComparison.OrdinalIgnoreCase))
                    .ToArray();

            if (descs.Length != 1)
            {
                return;
            }

            GorgonImageCodecPlugIn[] plugins = CodecPlugIns.Where(item => item.Codecs.Contains(descs[0]))
                                                           .ToArray();

            foreach (GorgonImageCodecPlugIn plugin in plugins)
            {
                CodecPlugIns.Remove(plugin);
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

            CodecPlugIns.Remove(plugin);
        }

        /// <summary>
        /// Function to add a codec to the registry.
        /// </summary>
        /// <param name="path">The path to the codec assembly.</param>
        /// <param name="errors">A list of errors if the plug in fails to load.</param>
        /// <returns>A list of codec plugs ins that were loaded.</returns>
        public IReadOnlyList<GorgonImageCodecPlugIn> AddCodec(string path, out IReadOnlyList<string> errors)
        {
            var localErrors = new List<string>();
            errors = localErrors;

            var result = new List<GorgonImageCodecPlugIn>();
            _log.Print("Loading image codecs...", LoggingLevel.Intermediate);

            IReadOnlyList<PlugInRecord> assemblies = _pluginCache.ValidateAndLoadAssemblies(new[] { new FileInfo(path) }, _log);

            if (assemblies.Count == 0)
            {
                _log.Print("No image codecs found.", LoggingLevel.Verbose);
                return result;
            }

            IEnumerable<PlugInRecord> failedAssemblies = assemblies.Where(item => !item.IsAssemblyLoaded);

            foreach (PlugInRecord failure in failedAssemblies)
            {
                localErrors.Add(failure.LoadFailureReason);
            }

            if (localErrors.Count > 0)
            {
                return result;
            }

            IGorgonPlugInService plugins = new GorgonMefPlugInService(_pluginCache, _log);

            // Load all the codecs contained within the plug in (a plug in can have multiple codecs).
            foreach (GorgonImageCodecPlugIn plugin in plugins.GetPlugIns<GorgonImageCodecPlugIn>()
															 .Where(item => string.Equals(item.PlugInPath, path, StringComparison.OrdinalIgnoreCase)))
            {
                if (CodecPlugIns.Any(item => string.Equals(plugin.Name, item.Name, StringComparison.OrdinalIgnoreCase)))
                {
                    _log.Print($"[WARNING] Codec plug in '{plugin.Name}' is already loaded.", LoggingLevel.Intermediate);
                    localErrors.Add(string.Format(Resources.GORIMG_ERR_CODEC_ALREADY_LOADED, plugin.Name));
                    continue;
                }

                CodecPlugIns.Add(plugin);

                foreach (GorgonImageCodecDescription desc in plugin.Codecs)
                {
                    if (Codecs.Any(item => string.Equals(desc.Name, item.Name, StringComparison.OrdinalIgnoreCase)))
                    {
                        _log.Print($"[WARNING] Codec '{desc.Name}' is already loaded.", LoggingLevel.Intermediate);
                        localErrors.Add(string.Format(Resources.GORIMG_ERR_CODEC_ALREADY_LOADED, desc.Name));
                        return result;
                    }

                    IGorgonImageCodec imageCodec = plugin.CreateCodec(desc.Name);

                    if (imageCodec == null)
                    {
                        _log.Print($"[ERROR] Could not create image codec '{desc.Name}' from plug in '{plugin.PlugInPath}'.", LoggingLevel.Verbose);
                        localErrors.Add(string.Format(Resources.GORIMG_ERR_CODEC_LOAD_FAIL, desc.Name));
                        return result;
                    }

                    Codecs.Add(imageCodec);                    

                    foreach (string extension in imageCodec.CodecCommonExtensions)
                    {
                        CodecFileTypes.Add((new GorgonFileExtension(extension), imageCodec));
                    }
                }								

                result.Add(plugin);
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
                    CodecFileTypes.Add((new GorgonFileExtension(extension), codec));
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
            _log = log;
        }
        #endregion
    }
}
