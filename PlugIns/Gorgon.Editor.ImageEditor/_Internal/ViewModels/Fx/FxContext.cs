
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: February 23, 2020 12:42:33 PM
// 

using Gorgon.Diagnostics;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Imaging;

namespace Gorgon.Editor.ImageEditor.Fx;

/// <summary>
/// The context controller for the FX context
/// </summary>
internal class FxContext
    : EditorContext<FxContextParameters>, IFxContext
{

    // The image content view model.
    private IImageContent _imageContent;
    // The services from the host application.
    private IHostContentServices _hostServices;
    // Flag to indicate whether effects have been applied.
    private bool _effectsApplied;
    // The effects service used to apply effects to the image or generate previews.
    private IFxService _fxService;

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
    /// Property to return the view model for the emboss effect settings.
    /// </summary>
    public IFxEmboss EmbossSettings
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the view model for the edge detection effect settings.
    /// </summary>
    public IFxEdgeDetect EdgeDetectSettings
    {
        get;
        private set;
    }

    /// <summary>Property to return the view model for the posterize effect settings.</summary>
    public IFxPosterize PosterizeSettings
    {
        get;
        private set;
    }

    /// <summary>Property to return the view model for the one bit effect settings.</summary>
    public IFxOneBit OneBitSettings
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
    /// Property to return the command to show the emboss effect settings.
    /// </summary>
    public IEditorCommand<object> ShowEmbossCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to show the edge detection settings.
    /// </summary>
    public IEditorCommand<object> ShowEdgeDetectCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to show the one bit effect settings.
    /// </summary>
    public IEditorCommand<object> ShowOneBitCommand
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

    /// <summary>Property to return the command to apply the burn effect.</summary>
    public IEditorCommand<object> BurnCommand
    {
        get;
    }

    /// <summary>
    /// Property to return the command to apply the dodge effect.
    /// </summary>
    public IEditorCommand<object> DodgeCommand
    {
        get;
    }

    /// <summary>Property to return the command to show the posterize settings.</summary>
    public IEditorCommand<object> ShowPosterizeCommand
    {
        get;
    }

    /// <summary>
    /// Function to set the working image for the blur effect.
    /// </summary>
    /// <param name="imageData">The image to use as a basis for the working image.</param>
    private void DoSetImage(IGorgonImage imageData)
    {
        _hostServices.BusyService.SetBusy();

        try
        {
            _fxService.SetImage(imageData, _imageContent.ImageType == ImageDataType.Image3D ? _imageContent.CurrentDepthSlice : _imageContent.CurrentArrayIndex, _imageContent.CurrentMipLevel);
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
    private bool CanShowGaussBlurSettings() => _imageContent.CurrentPanel is null;

    /// <summary>
    /// Function to show the gaussian blur effect settings.
    /// </summary>
    private void DoShowGaussBlurSettings()
    {
        _hostServices.BusyService.SetBusy();

        try
        {
            _imageContent.CurrentPanel = BlurSettings;
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
            _imageContent.CurrentPanel = null;
            _imageContent.CommandContext = null;
            _fxService.SetImage(null, 0, 0);
        }
        catch (Exception ex)
        {
            _hostServices.Log.Print("ERROR: Cannot cancel the effects operation.", LoggingLevel.Simple);
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

            _imageContent.CurrentPanel = null;
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
    private bool CanGrayScale() => _imageContent.CurrentPanel is null;

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
            _hostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_APPLYING_EFFECTS);
        }
    }

    /// <summary>
    /// Function to determine if the invert effect can be applied.
    /// </summary>
    /// <returns><b>true</b> if the effect can be applied, <b>false</b> if not.</returns>
    private bool CanInvert() => _imageContent.CurrentPanel is null;

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
            _hostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_APPLYING_EFFECTS);
        }
    }

    /// <summary>
    /// Function to determine if the burn effect can be applied.
    /// </summary>
    /// <returns><b>true</b> if the effect can be applied, <b>false</b> if not.</returns>
    private bool CanBurn() => _imageContent.CurrentPanel is null;

    /// <summary>
    /// Function to apply the burn effect to the working image.
    /// </summary>
    private void DoBurn()
    {
        try
        {
            _fxService.ApplyDodgeBurn(false);
            EffectsUpdated = true;
        }
        catch (Exception ex)
        {
            _hostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_APPLYING_EFFECTS);
        }
    }

    /// <summary>
    /// Function to determine if the dodge effect can be applied.
    /// </summary>
    /// <returns><b>true</b> if the effect can be applied, <b>false</b> if not.</returns>
    private bool CanDodge() => _imageContent.CurrentPanel is null;

    /// <summary>
    /// Function to apply the burn effect to the working image.
    /// </summary>
    private void DoDodge()
    {
        try
        {
            _fxService.ApplyDodgeBurn(true);
            EffectsUpdated = true;
        }
        catch (Exception ex)
        {
            _hostServices.MessageDisplay.ShowError(ex, Resources.GORIMG_ERR_APPLYING_EFFECTS);
        }
    }

    /// <summary>
    /// Function to determine if the sharpen settings can be shown.
    /// </summary>
    /// <returns><b>true</b> if they can be shown, <b>false</b> if not.</returns>
    private bool CanShowSharpen() => _imageContent.CurrentPanel is null;

    /// <summary>
    /// Function to display the sharpen effect settings.
    /// </summary>
    private void DoShowSharpen()
    {
        _hostServices.BusyService.SetBusy();

        try
        {
            _imageContent.CurrentPanel = SharpenSettings;
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
    /// Function to determine if the emboss settings can be shown.
    /// </summary>
    /// <returns><b>true</b> if they can be shown, <b>false</b> if not.</returns>
    private bool CanShowEmboss() => _imageContent.CurrentPanel is null;

    /// <summary>
    /// Function to display the emboss effect settings.
    /// </summary>
    private void DoShowEmboss()
    {
        _hostServices.BusyService.SetBusy();

        try
        {
            _imageContent.CurrentPanel = EmbossSettings;
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
    /// Function to determine if the posterize effect settings can be displayed.
    /// </summary>
    /// <returns><b>true</b> if the effect settings can be displayed, <b>false</b> if not.</returns>
    private bool CanShowPosterize() => _imageContent.CurrentPanel is null;

    /// <summary>
    /// Function to show the posterize effect settings.
    /// </summary>
    private void DoShowPosterize()
    {
        _hostServices.BusyService.SetBusy();

        try
        {
            _imageContent.CurrentPanel = PosterizeSettings;
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
    /// Function to determine if the 1 bit effect settings can be displayed.
    /// </summary>
    /// <returns><b>true</b> if the effect settings can be displayed, <b>false</b> if not.</returns>
    private bool CanShowOneBit() => _imageContent.CurrentPanel is null;

    /// <summary>
    /// Function to show the 1 bit effect settings.
    /// </summary>
    private void DoShowOneBit()
    {
        _hostServices.BusyService.SetBusy();

        try
        {
            _imageContent.CurrentPanel = OneBitSettings;
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
    /// Function to determine if the edge detect effect settings can be displayed.
    /// </summary>
    /// <returns><b>true</b> if the effect settings can be displayed, <b>false</b> if not.</returns>
    private bool CanShowEdgeDetect() => _imageContent.CurrentPanel is null;

    /// <summary>
    /// Function to show the edge detection effect settings.
    /// </summary>
    private void DoShowEdgeDetect()
    {
        _hostServices.BusyService.SetBusy();

        try
        {
            _imageContent.CurrentPanel = EdgeDetectSettings;
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
        _hostServices = injectionParameters.HostServices;
        _fxService = injectionParameters.FxService;

        BlurSettings = injectionParameters.BlurSettings;
        SharpenSettings = injectionParameters.SharpenSettings;
        EmbossSettings = injectionParameters.EmbossSettings;
        EdgeDetectSettings = injectionParameters.EdgeDetectSettings;
        PosterizeSettings = injectionParameters.PosterizeSettings;
        OneBitSettings = injectionParameters.OneBitSettings;

        BlurSettings.OkCommand = new EditorCommand<object>(DoPreviewedEffect);
        SharpenSettings.OkCommand = new EditorCommand<object>(DoPreviewedEffect);
        EmbossSettings.OkCommand = new EditorCommand<object>(DoPreviewedEffect);
        EdgeDetectSettings.OkCommand = new EditorCommand<object>(DoPreviewedEffect);
        PosterizeSettings.OkCommand = new EditorCommand<object>(DoPreviewedEffect);
        OneBitSettings.OkCommand = new EditorCommand<object>(DoPreviewedEffect);
    }

    /// <summary>Function called when the associated view is loaded.</summary>
    protected override void OnLoad()
    {
        base.OnLoad();

        EffectsUpdated = false;
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    protected override void OnUnload()
    {
        _fxService.SetImage(null, 0, 0);

        base.OnUnload();
    }

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
        BurnCommand = new EditorCommand<object>(DoBurn, CanBurn);
        DodgeCommand = new EditorCommand<object>(DoDodge, CanDodge);
        ShowSharpenCommand = new EditorCommand<object>(DoShowSharpen, CanShowSharpen);
        ShowEmbossCommand = new EditorCommand<object>(DoShowEmboss, CanShowEmboss);
        ShowEdgeDetectCommand = new EditorCommand<object>(DoShowEdgeDetect, CanShowEdgeDetect);
        ShowPosterizeCommand = new EditorCommand<object>(DoShowPosterize, CanShowPosterize);
        ShowOneBitCommand = new EditorCommand<object>(DoShowOneBit, CanShowOneBit);
    }
}
