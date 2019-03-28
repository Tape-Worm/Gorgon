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
// Created: March 23, 2019 4:26:04 PM
// 
#endregion

using DX = SharpDX;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The view model for the manual input for sprite clipping.
    /// </summary>
    internal class ManualRectInputVm
        : ViewModelBase<IViewModelInjection>, IManualRectInputVm
    {
        #region Variables.
        // The rectangle to update.
        private DX.RectangleF _rect;
        // Flag to indicate whether the interface is active or not.
        private bool _isActive;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return whether the manual input interface is active or not.
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set
            {
                if (_isActive == value)
                {
                    return;
                }

                OnPropertyChanging();
                _isActive = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the rectangle dimensions.
        /// </summary>
        public DX.RectangleF Rectangle
        {
            get => _rect;
            set
            {
                if (_rect.Equals(ref value))
                {
                    return;
                }

                OnPropertyChanging();
                _rect = value;
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
            // Nothing to do.
        }
        #endregion

        #region Constructor/Finalizer.

        #endregion
    }
}
