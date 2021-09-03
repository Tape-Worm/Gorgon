#region MIT
// 
// Gorgon.
// Copyright (C) 2021 Michael Winsor
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
// Created: August 30, 2021 11:38:42 PM
// 
#endregion

using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Fonts;

namespace Gorgon.Editor.FontEditor
{
    /// <summary>
    /// The view model for the text color editor.
    /// </summary>
    internal class FontOutline
        : HostedPanelViewModelBase<HostedPanelViewModelParameters>, IFontOutline
    {
        #region Variables.
        // The original color for the sprite.
        private GorgonColor _originalStartColor = GorgonColor.Black;
        // The current color for the sprite.
        private GorgonColor _startColor = GorgonColor.Black;
        // The original color for the sprite.
        private GorgonColor _originalEndColor = GorgonColor.Black;
        // The current color for the sprite.
        private GorgonColor _endColor = GorgonColor.Black;
        // The size of the outline.
        private int _outlineSize;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the size of the outline.
        /// </summary>
        public int OutlineSize
        {
            get => _outlineSize;
            set
            {
                if (_outlineSize == value)
                {
                    return;
                }

                OnPropertyChanging();
                _outlineSize = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return the currently selected start color.</summary>
        public GorgonColor SelectedStartColor
        {
            get => _startColor;
            set
            {
                if (_startColor.Equals(in value))
                {
                    return;
                }

                OnPropertyChanging();
                _startColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the original start color.
        /// </summary>
        public GorgonColor OriginalStartColor
        {
            get => _originalStartColor;
            set
            {
                if (_originalStartColor.Equals(in value))
                {
                    return;
                }

                OnPropertyChanging();
                _originalStartColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the original color for the text.
        /// </summary>
        public GorgonColor OriginalEndColor
        {
            get => _originalEndColor;
            set
            {
                if (_originalEndColor.Equals(in value))
                {
                    return;
                }

                OnPropertyChanging();
                _originalEndColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the currently selected color.
        /// </summary>
        public GorgonColor SelectedEndColor
        {
            get => _outlineSize < 3 ? _startColor : _endColor;
            set
            {
                if (_endColor.Equals(in value))
                {
                    return;
                }

                OnPropertyChanging();
                _endColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return whether the panel is modal.</summary>
        public override bool IsModal => true;
        #endregion

        #region Methods.
        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(HostedPanelViewModelParameters injectionParameters)
        {
            // Nothing to inject.
        }
        #endregion
    }
}
