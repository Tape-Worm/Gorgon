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
// Created: April 22, 2019 10:47:26 PM
// 
#endregion

using System;
using System.Threading;
using Gorgon.Core;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI.ViewModels;
using Gorgon.PlugIns;

namespace Gorgon.Editor.ImageEditor
{
	/// <summary>
    /// A factory used to build data that is shared between the importer and image editor plugins.
    /// </summary>
    internal static class SharedDataFactory
    {
        #region Variables.
		// A weak reference to the common services in the application.
        private static WeakReference<IViewModelInjection> _commonServices;
		// The service used to handling content plug ins.
        private static WeakReference<IContentPlugInService> _plugInService;
        // The service used to handling content plug ins.
		// We will keep this alive and undisposed since it's meant to live for the lifetime of the application.
        private static Lazy<GorgonMefPlugInCache> _plugInCache;
        // The factory that creates/loads the settings.
        private static Lazy<ImageEditorSettings> _settingsFactory;
		// The factory that creates/loads the codec registry.
        private static Lazy<ICodecRegistry> _codecRegistryFactory;
        // The factory that creates/loads the settings view model.
        private static Lazy<ISettings> _settingsViewModelFactory;
        #endregion

        #region Methods.
		/// <summary>
        /// Function to retrieve the common plug in cache.
        /// </summary>
        /// <returns>The plug in cache.</returns>
        private static GorgonMefPlugInCache GetPlugInCache()
        {
            if (!_commonServices.TryGetTarget(out IViewModelInjection commonServices))
            {
                throw new GorgonException(GorgonResult.CannotCreate);
            }

            return new GorgonMefPlugInCache(commonServices.Log);
        }

		/// <summary>
        /// Function to load the settings for the image editor/importer.
        /// </summary>
        /// <returns>The settings for both plug ins.</returns>
        private static ImageEditorSettings LoadSettings()
        {
            if (!_plugInService.TryGetTarget(out IContentPlugInService plugInService))
            {
                throw new GorgonException(GorgonResult.CannotCreate);
            }

            ImageEditorSettings settings = plugInService.ReadContentSettings<ImageEditorSettings>(ImageEditorPlugIn.SettingsName);

            if (settings == null)
            {
                settings = new ImageEditorSettings();
            }

            return settings;
        }		

		/// <summary>
        /// Function to retrieve the codec registry and load the initial codecs from the settings.
        /// </summary>
        /// <returns>The codec registry.</returns>
        private static ICodecRegistry GetCodecRegistry() 
        {
            if (!_commonServices.TryGetTarget(out IViewModelInjection commonServices))
            {
                throw new GorgonException(GorgonResult.CannotCreate);
            }

            var result =  new CodecRegistry(_plugInCache.Value, commonServices.Log);
            result.LoadFromSettings(_settingsFactory.Value);
            return result;
        }

		/// <summary>
        /// Function to retrieve the settings view model.
        /// </summary>
        /// <returns>The settings view model.</returns>
        private static ISettings GetSettingsViewModel()
        {
            if ((!_commonServices.TryGetTarget(out IViewModelInjection commonServices))
				|| (!_plugInService.TryGetTarget(out IContentPlugInService plugInService)))
            {
                throw new GorgonException(GorgonResult.CannotCreate);
            }

            IFileDialogService dialog = new FileOpenDialogService
            {
                DialogTitle = Resources.GORIMG_CAPTION_SELECT_CODEC_DLL,
                FileFilter = Resources.GORIMG_FILTER_SELECT_CODEC
            };

            var result = new Settings();
            result.Initialize(new SettingsParameters(_settingsFactory.Value, _codecRegistryFactory.Value, dialog, plugInService, _plugInCache.Value, commonServices));
            return result;
        }

        /// <summary>
        /// Function to retrieve the shared data for the plug ins in this assembly.
        /// </summary>
        /// <param name="plugInService">The application plug in service.</param>
        /// <param name="commonServices">The common services for the application.</param>
        /// <returns>A tuple containing the shared codec registry and the settings view model.</returns>
        public static (ICodecRegistry codecRegisry, ISettings settingsViewModel) GetSharedData(IContentPlugInService plugInService, IViewModelInjection commonServices)
        {
            Interlocked.CompareExchange(ref _plugInService, new WeakReference<IContentPlugInService>(plugInService), null);
            Interlocked.CompareExchange(ref _commonServices, new WeakReference<IViewModelInjection>(commonServices), null);
            return (_codecRegistryFactory.Value, _settingsViewModelFactory.Value);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes static members of the <see cref="T:Gorgon.Editor.ImageEditor.SharedDataFactory"/> class.</summary>
        static SharedDataFactory()
        {
            _plugInCache = new Lazy<GorgonMefPlugInCache>(GetPlugInCache, LazyThreadSafetyMode.ExecutionAndPublication);
            _settingsFactory = new Lazy<ImageEditorSettings>(LoadSettings, LazyThreadSafetyMode.ExecutionAndPublication);
            _codecRegistryFactory = new Lazy<ICodecRegistry>(GetCodecRegistry, LazyThreadSafetyMode.ExecutionAndPublication);
            _settingsViewModelFactory = new Lazy<ISettings>(GetSettingsViewModel, LazyThreadSafetyMode.ExecutionAndPublication);
        }
		#endregion
    }
}
