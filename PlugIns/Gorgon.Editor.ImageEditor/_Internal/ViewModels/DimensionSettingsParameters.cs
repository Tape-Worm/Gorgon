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
// Created: February 5, 2019 7:57:48 PM
// 
#endregion

using System;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;

namespace Gorgon.Editor.ImageEditor.ViewModels
{
    /// <summary>
    /// Parameters for the <see cref="IDimensionSettings"/> view model.
    /// </summary>
    internal class DimensionSettingsParameters
        : HostedPanelViewModelParameters
    {
        #region Properties.
        /// <summary>
        /// Property to return the video adapter used by the application.
        /// </summary>
        public IGorgonVideoAdapterInfo VideoAdapter
        {
            get;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="DimensionSettingsParameters"/> class.</summary>
        /// <param name="hostServices">Services from the host application.</param>
        /// <exception cref="ArgumentNullException">Thrown when any parameter is <b>null</b>.</exception>
        public DimensionSettingsParameters(IHostContentServices hostServices)
            : base(hostServices) => VideoAdapter = hostServices.GraphicsContext.Graphics.VideoAdapter;
        #endregion

    }
}
