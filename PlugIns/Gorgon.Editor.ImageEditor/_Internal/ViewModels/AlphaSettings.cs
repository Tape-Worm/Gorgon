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
// Created: August 31, 2019 9:12:06 PM
// 
#endregion

using System;
using System.Linq;
using Gorgon.Core;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.ViewModels;
using Gorgon.Math;

namespace Gorgon.Editor.ImageEditor
{
    internal class AlphaSettings
        : ViewModelBase<IViewModelInjection>, IAlphaSettings
    {
        #region Variables.
        // Flag to indicate that the editor is active.
        private bool _isActive;
        // The number of mip map levels in the image.
        private int _alpha = 255;
        // The service used to display message notifications.
        private IMessageDisplayService _messageDisplay;
        // The range of alpha values to update.
        private GorgonRange _alphaUpdateRange = new GorgonRange(0, 255);
        #endregion

        #region Properties.
        /// <summary>Property to return whether the panel is modal.</summary>
        public bool IsModal => true;

        /// <summary>Property to set or return whether the crop/resize settings is active or not.</summary>
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

        /// <summary>Property to set or return the command used to cancel the operation.</summary>
        public IEditorCommand<object> CancelCommand
        {
            get;
        }

        /// <summary>Property to set or return the command used to apply the operation.</summary>
        public IEditorCommand<object> OkCommand
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the lower and upper bounds of the alpha range to update.
        /// </summary>
        public GorgonRange UpdateRange
        {
            get => _alphaUpdateRange;
            set
            {
                if (_alphaUpdateRange.Equals(value))
                {
                    return;
                }

                OnPropertyChanging();
                _alphaUpdateRange = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return the alpha value to set.</summary>
        public int AlphaValue
        {
            get => _alpha;
            set
            {
                value = value.Max(0).Min(255);

                if (_alpha == value)
                {
                    return;
                }

                OnPropertyChanging();
                _alpha = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to cancel the dimension change operation.
        /// </summary>
        private void DoCancel()
        {
            try
            {
                IsActive = false;
            }
            catch (Exception ex)
            {
                _messageDisplay.ShowError(ex, Resources.GORIMG_ERR_CANCEL_OP);
            }
        }
        #endregion

        #region Methods.
        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(IViewModelInjection injectionParameters) => 
            _messageDisplay = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(injectionParameters.MessageDisplay), nameof(injectionParameters));
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.ImageEditor.AlphaSettings"/> class.</summary>
        public AlphaSettings() => CancelCommand = new EditorCommand<object>(DoCancel);
        #endregion
    }
}
