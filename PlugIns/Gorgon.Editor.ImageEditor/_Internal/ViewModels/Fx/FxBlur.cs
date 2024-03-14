#region MIT
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
// Created: February 22, 2020 5:43:26 PM
// 
#endregion

using Gorgon.Editor.UI;
using Gorgon.Math;

namespace Gorgon.Editor.ImageEditor.Fx;

/// <summary>
/// The view model for the gaussian blur effect settings.
/// </summary>
internal class FxBlur
    : HostedPanelViewModelBase<HostedPanelViewModelParameters>, IFxBlur
{
    #region Variables.
    // The amount of blur to apply.
    private int _blurAmount = 1;
    #endregion

    #region Properties.
    /// <summary>Property to return whether the panel is modal.</summary>
    public override bool IsModal => false;

    /// <summary>
    /// Property to set or return the amount of blur to apply.
    /// </summary>
    public int BlurAmount
    {
        get => _blurAmount.Max(1).Min(200);
        set
        {
            if (_blurAmount == value)
            {
                return;
            }

            OnPropertyChanging();
            _blurAmount = value;
            OnPropertyChanged();
        }
    }
    #endregion

    #region Methods.
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
    protected override void OnInitialize(HostedPanelViewModelParameters injectionParameters)
    {
    }
    #endregion
}
