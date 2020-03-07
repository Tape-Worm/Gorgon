﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: February 23, 2020 12:42:33 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging;
using Gorgon.IO;

namespace Gorgon.Editor.ImageEditor.Fx
{
    /// <summary>
    /// The context controller for the FX context.
    /// </summary>
    internal class FxContext
        : EditorContext<FxContextParameters>, IFxContext
    {
        #region Variables.
        // The image content view model.
        private IImageContent _imageContent;
        // The services from the host application.
        private IHostContentServices _hostServices;
        // Flag to indicate whether effects have been applied.
        private bool _effectsApplied;
        // The effects service used to apply effects to the image or generate previews.
        private IFxService _fxService;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the view model for the blur fx settings.
        /// </summary>
        public IFxBlur BlurSettings
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the view model for the sharpen effect settings.
        /// </summary>
        public IFxSharpen SharpenSettings
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return whether effects have been applied to the image.
        /// </summary>
        public bool EffectsUpdated
        {
            get => _effectsApplied;
            private set
            {
                if (_effectsApplied == value)
                {
                    return;
                }

                OnPropertyChanging();
                _effectsApplied = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the name of this object.</summary>
        /// <remarks>For best practice, the name should only be set once during the lifetime of an object. Hence, this interface only provides a read-only implementation of this
        /// property.</remarks>
        public override string Name => "ContextFx";

        /// <summary>
        /// Property to return the service used to apply effects and generate previews for effects.
        /// </summary>
        public IFxService FxService
        {
            get => _fxService;
            private set
            {
                if (_fxService == value)
                {
                    return;
                }

                OnPropertyChanging();
                _fxService = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the command used to assign the working image
        /// </summary>
        public IEditorCommand<IGorgonImage> SetImageCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to show the blur settings.
        /// </summary>
        public IEditorCommand<object> ShowBlurCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to apply the grayscale effect.
        /// </summary>
        public IEditorCommand<object> GrayScaleCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to apply the invert effect.
        /// </summary>
        public IEditorCommand<object> InvertCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to show the sharpen effect settings.
        /// </summary>
        public IEditorCommand<object> ShowSharpenCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to cancel the effects operations.
        /// </summary>
        public IEditorCommand<object> CancelCommand
        {
            get;
        }

        /// <summary>
        /// Property to set or return the command to apply the effects to the final image.
        /// </summary>
        public IEditorCommand<object> ApplyCommand
        {
            get;
            set;
        }
        #endregion

        #region Methods.        
        /// <summary>
        /// Function to set the working image for the blur effect.
        /// </summary>
        /// <param name="imageData">The image to use as a basis for the working image.</param>
        private void DoSetImage(IGorgonImage imageData)
        {
            _hostServices.BusyService.SetBusy();

            try
            {
                _fxService.SetImage(imageData, _imageContent.ImageType == ImageType.Image3D ? _imageContent.CurrentDepthSlice : _imageContent.CurrentArrayIndex, _imageContent.CurrentMipLevel);
            }
            catch (Exception ex)
            {
                _hostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
            }
            finally
            {
                _hostServices.BusyService.SetIdle();
            }
        }

        /// <summary>
        /// Function to determine if the gaussian blur settings can be shown or not.
        /// </summary>
        /// <returns><b>true</b> if the settings can be shown, <b>false</b> if not.</returns>
        private bool CanShowGaussBlurSettings() => _imageContent.CurrentHostedPanel == null;

        /// <summary>
        /// Function to show the gaussian blur effect settings.
        /// </summary>
        private void DoShowGaussBlurSettings()
        {
            _hostServices.BusyService.SetBusy();

            try
            {
                _imageContent.CurrentHostedPanel = BlurSettings;
            }
            catch (Exception ex)
            {
                _hostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
            }
            finally
            {
                _hostServices.BusyService.SetIdle();
            }
        }

        /// <summary>
        /// Function to cancel the effects operations.
        /// </summary>
        private void DoCancelFx()
        {
            try
            {
                _imageContent.CurrentHostedPanel = null;
                _imageContent.CommandContext = null;
                _fxService.SetImage(null, 0, 0);
            }
            catch (Exception ex)
            {
                _hostServices.Log.Print("[Error] Cannot cancel the effects operation.", LoggingLevel.Simple);
                _hostServices.Log.LogException(ex);
            }
        }

        /// <summary>
        /// Function to apply the current effect that is in preview mode to the working image.
        /// </summary>
        private void DoPreviewedEffect()
        {
            IGorgonImage image = null;

            _hostServices.BusyService.SetBusy();

            try
            {
                _fxService.ApplyPreviewedEffect();

                _imageContent.CurrentHostedPanel = null;
                EffectsUpdated = true;
            }
            catch (Exception ex)
            {
                _hostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_APPLYING_EFFECTS);
            }
            finally
            {
                _hostServices.BusyService.SetIdle();
                image?.Dispose();
            }
        }

        /// <summary>
        /// Function to determine if the grayscale effect can be applied.
        /// </summary>
        /// <returns></returns>
        private bool CanGrayScale() => _imageContent.CurrentHostedPanel == null;

        /// <summary>
        /// Function to apply the grayscale effect to the working image.
        /// </summary>
        private void DoGrayScale()
        {
            try
            {
                _fxService.ApplyGrayScale();
                EffectsUpdated = true;
            }
            catch (Exception ex)
            {
                _hostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_GRAYSCALE);
            }
        }

        /// <summary>
        /// Function to determine if the invert effect can be applied.
        /// </summary>
        /// <returns></returns>
        private bool CanInvert() => _imageContent.CurrentHostedPanel == null;

        /// <summary>
        /// Function to apply the invert effect to the working image.
        /// </summary>
        private void DoInvert()
        {
            try
            {
                _fxService.ApplyInvert();
                EffectsUpdated = true;
            }
            catch (Exception ex)
            {
                _hostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_GRAYSCALE);
            }
        }

        /// <summary>
        /// Function to determine if the sharpen settings can be shown.
        /// </summary>
        /// <returns><b>true</b> if they can be shown, <b>false</b> if not.</returns>
        private bool CanShowSharpen() => _imageContent.CurrentHostedPanel == null;

        /// <summary>
        /// Function to display the sharpen effect settings.
        /// </summary>
        private void DoShowSharpen()
        {
            _hostServices.BusyService.SetBusy();

            try
            {
                _imageContent.CurrentHostedPanel = SharpenSettings;
            }
            catch (Exception ex)
            {
                _hostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_UPDATING_IMAGE);
            }
            finally
            {
                _hostServices.BusyService.SetIdle();
            }
        }

        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        ///   <para>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </para>
        ///   <para>
        /// This method is only ever called after the view model has been created, and never again during the lifetime of the view model.
        /// </para>
        /// </remarks>
        protected override void OnInitialize(FxContextParameters injectionParameters)
        {
            _imageContent = injectionParameters.ImageContent;
            BlurSettings = injectionParameters.BlurSettings;
            SharpenSettings = injectionParameters.SharpenSettings;
            _hostServices = injectionParameters.HostServices;            
            _fxService = injectionParameters.FxService;

            BlurSettings.OkCommand = new EditorCommand<object>(DoPreviewedEffect);
            SharpenSettings.OkCommand = new EditorCommand<object>(DoPreviewedEffect);
        }

        /// <summary>Function called when the associated view is loaded.</summary>
        public override void OnLoad()
        {
            base.OnLoad();
            EffectsUpdated = false;
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public override void OnUnload()
        {
            _fxService.SetImage(null, 0, 0);
            base.OnUnload();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="FxContext"/> class.</summary>
        /// <param name="imageContent">The image content view model.</param>
        /// <param name="blur">The blur Fx view model.</param>
        /// <param name="hostServices">The services from the host application.</param>
        public FxContext()
        {
            SetImageCommand = new EditorCommand<IGorgonImage>(DoSetImage);
            ShowBlurCommand = new EditorCommand<object>(DoShowGaussBlurSettings, CanShowGaussBlurSettings);
            CancelCommand = new EditorCommand<object>(DoCancelFx);
            GrayScaleCommand = new EditorCommand<object>(DoGrayScale, CanGrayScale);
            InvertCommand = new EditorCommand<object>(DoInvert, CanInvert);
            ShowSharpenCommand = new EditorCommand<object>(DoShowSharpen, CanShowSharpen);
        }
        #endregion
    }
}
