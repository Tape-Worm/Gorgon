﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: November 9, 2018 3:56:48 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ImageEditor.ViewModels
{
    /// <summary>
    /// The image content view model.
    /// </summary>
    internal class ImageContentVm
        : ViewModelBase<IViewModelInjection>, IImageContentVm
    {
        #region Variables.
        // The current zoom level.
        private ZoomLevels _zoomLevels = ZoomLevels.ToWindow;
        #endregion

        #region Properties.
        /// <summary>Property to set or return the zoom level.</summary>
        public ZoomLevels ZoomLevel
        {
            get => _zoomLevels;
            set
            {
                if (_zoomLevels == value)
                {
                    return;
                }

                OnPropertyChanging();
                _zoomLevels = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Methods.
        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(IViewModelInjection injectionParameters)
        {
            // Not needed right now
        }
        #endregion

        #region Constructor/Finalizer.

        #endregion
    }
}
