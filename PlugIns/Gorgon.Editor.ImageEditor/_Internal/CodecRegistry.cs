using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Gorgon.Diagnostics;
using Gorgon.Editor.Plugins;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Plugins;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// A common registry for our image codecs.
    /// </summary>
    internal class CodecRegistry
    {
        #region Variables.
		// The cache containing the plug in assemblies.
        private readonly GorgonMefPluginCache _pluginCache;
        // The plug in settings.
        private readonly ISettings _settings;
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
        public IList<GorgonImageCodecPlugin> CodecPlugins
        {
            get;
        } = new List<GorgonImageCodecPlugin>();		
        #endregion

        #region Methods.
        /// <summary>
        /// Function to load external image codec plug ins.
        /// </summary>
        private void LoadCodecPlugins()
        {
            if (_settings.CodecPluginPaths.Count == 0)
            {
                return;
            }

            _log.Print("Loading image codecs...", LoggingLevel.Intermediate);

            _pluginCache.ValidateAndLoadAssemblies(_settings.CodecPluginPaths.Select(item => new FileInfo(item.path)), _log);
            IGorgonPluginService plugins = new GorgonMefPluginService(_pluginCache, _log);

            // Load all the codecs contained within the plug in (a plug in can have multiple codecs).
            foreach (GorgonImageCodecPlugin plugin in plugins.GetPlugins<GorgonImageCodecPlugin>())
            {
                foreach (GorgonImageCodecDescription desc in plugin.Codecs)
                {					
                    CodecPlugins.Add(plugin);
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

            GorgonImageCodecDescription[] descs = CodecPlugins
					.SelectMany(item => item.Codecs)
					.Where(item => string.Equals(item.Name, codec.GetType().FullName, StringComparison.OrdinalIgnoreCase))
					.ToArray();

            if (descs.Length != 1)
            {
                return;
            }

            GorgonImageCodecPlugin[] plugins = CodecPlugins.Where(item => item.Codecs.Contains(descs[0]))
														   .ToArray();

            foreach (GorgonImageCodecPlugin plugin in plugins)
            {
                CodecPlugins.Remove(plugin);
            }
        }

        /// <summary>
        /// Function to remove an image codec plug in from the registry.
        /// </summary>
        /// <param name="plugin">The plug in to remove.</param>
        public void RemoveCodecPlugin(GorgonImageCodecPlugin plugin)
        {
            if (plugin == null)
            {
                throw new ArgumentNullException(nameof(plugin));
            }

            if (!CodecPlugins.Contains(plugin))
            {
                return;
            }

            foreach (GorgonImageCodecDescription desc in plugin.Codecs)
            {
                IGorgonImageCodec codec = Codecs.FirstOrDefault(item => string.Equals(item.Name, desc.Name, StringComparison.OrdinalIgnoreCase));

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

            CodecPlugins.Remove(plugin);
        }

        /// <summary>
        /// Function to load the codecs from our settings data.
        /// </summary>
        public void LoadFromSettings()
        {
            Codecs.Clear();
            CodecFileTypes.Clear();

            // Get built-in codec list.
            Codecs.Add(new GorgonCodecPng());
            Codecs.Add(new GorgonCodecJpeg());
            Codecs.Add(new GorgonCodecTga());
            Codecs.Add(new GorgonCodecBmp());
            Codecs.Add(new GorgonCodecGif());

            LoadCodecPlugins();

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
        /// <param name="settings">The settings.</param>
        /// <param name="pluginCache">The cache of plug in assemblies.</param>
        /// <param name="log">The log for debug output.</param>
        public CodecRegistry(ISettings settings, GorgonMefPluginCache pluginCache, IGorgonLog log)
        {
            _settings = settings;
            _pluginCache = pluginCache;
            _log = log;
        }
		#endregion
    }
}
