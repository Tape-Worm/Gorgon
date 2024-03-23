
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
// Created: May 22, 2020 7:15:42 PM
// 

using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;

namespace Gorgon.Editor.SpriteEditor;

/// <summary>
/// The view model for the sprite texture wrapping editor
/// </summary>
internal class SpriteTextureWrapEdit
    : HostedPanelViewModelBase<SpriteTextureWrapEditParameters>, ISpriteTextureWrapEdit
{

    // The builder used to create samplers.
    private GorgonSamplerStateBuilder _samplerBuilder;
    // The current sampler state for the sprite.
    private GorgonSamplerState _current = GorgonSamplerState.Default;
    // Horiztonal wrapping state.
    private TextureWrap _hWrap = TextureWrap.Clamp;
    // Vertical wrapping state.
    private TextureWrap _vWrap = TextureWrap.Clamp;
    // The current color for the border.
    private GorgonColor _border = GorgonColors.White;

    /// <summary>Property to return whether the panel is modal.</summary>
    public override bool IsModal => false;

    /// <summary>Property to set or return the current horizontal wrapping state.</summary>
    public TextureWrap HorizontalWrapping
    {
        get => _hWrap;
        set
        {
            if (_hWrap == value)
            {
                return;
            }

            OnPropertyChanging();
            _hWrap = value;
            BuildSampler();
            OnPropertyChanged();
        }
    }

    /// <summary>Property to set or return the current vertical wrapping state.</summary>
    public TextureWrap VerticalWrapping
    {
        get => _vWrap;
        set
        {
            if (_vWrap == value)
            {
                return;
            }

            OnPropertyChanging();
            _vWrap = value;
            BuildSampler();
            OnPropertyChanged();
        }
    }

    /// <summary>Property to set or return the current border color.</summary>
    public GorgonColor BorderColor
    {
        get => _border;
        set
        {
            if (_border.Equals(value))
            {
                return;
            }

            OnPropertyChanging();
            _border = value;
            BuildSampler();
            OnPropertyChanged();
        }
    }

    /// <summary>Property to return the set or return the current sampler state for the sprite.</summary>
    public GorgonSamplerState CurrentSampler
    {
        get => _current;
        set
        {
            value ??= GorgonSamplerState.Default;

            if ((value == _current) || (value.Equals(_current)))
            {
                return;
            }

            OnPropertyChanging();
            _current = value;
            _samplerBuilder.ResetTo(value);
            BorderColor = value.BorderColor;
            VerticalWrapping = value.WrapV;
            HorizontalWrapping = value.WrapU;
            OnPropertyChanged();
        }
    }

    /// <summary>
    /// Function to build a sampler based on state.
    /// </summary>
    private void BuildSampler()
    {
        HostServices.BusyService.SetBusy();

        try
        {
            if ((GorgonSamplerState.Default.BorderColor == BorderColor)
                && (GorgonSamplerState.Default.WrapU == HorizontalWrapping)
                && (GorgonSamplerState.Default.WrapV == VerticalWrapping)
                && (GorgonSamplerState.Default.Filter == SampleFilter.MinMagMipLinear))
            {
                CurrentSampler = GorgonSamplerState.Default;
                return;
            }

            if ((GorgonSamplerState.Default.BorderColor == BorderColor)
                && (GorgonSamplerState.Default.WrapU == HorizontalWrapping)
                && (GorgonSamplerState.Default.WrapV == VerticalWrapping)
                && (GorgonSamplerState.Default.Filter == SampleFilter.MinMagMipPoint))
            {
                CurrentSampler = GorgonSamplerState.PointFiltering;
                return;
            }

            CurrentSampler = _samplerBuilder.ResetTo(_current)
                                            .Wrapping(_hWrap, _vWrap, borderColor: BorderColor)
                                            .Build();
        }
        catch (Exception ex)
        {
            HostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
        }
        finally
        {
            HostServices.BusyService.SetIdle();
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
    protected override void OnInitialize(SpriteTextureWrapEditParameters injectionParameters) => _samplerBuilder = injectionParameters.SamplerStateBuilder;

}
