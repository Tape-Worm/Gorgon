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

using System;
using Gorgon.Editor.Services;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Editor.UI;
using DX = SharpDX;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The view model for the manual rectangle editor for sprite clipping.
    /// </summary>
    internal class ManualRectangleEditor
        : ViewModelBase<ManualInputParameters>, IManualRectangleEditor
    {
        #region Variables.
        // The rectangle to update.
        private DX.RectangleF _rect;
        // Flag to indicate whether the interface is active or not.
        private bool _isActive;
        // The sprite editor settings.
        private IEditorPlugInSettings _settings;
        // Flag to indicate that the interface is moving.
        private bool _isMoving;
        // The current texture array index.
        private int _textureArrayIndex;
        // The service used to display messages to the user.
        private IMessageDisplayService _messageDisplay;
        // The fixed size width/height.
        private DX.Size2F _fixedSize = new DX.Size2F(32, 32);
        // Flag to indicate that the selection rectangle is a fixed size.
        private bool _isFixedSize;
        // The command to execute to set the region to the full size of the texture.
        private IEditorCommand<object> _fullSizeCommand;
        // The command to apply the values.
        private IEditorCommand<object> _applyCommand;
        // The command to cancel the operatin.
        private IEditorCommand<object> _cancelCommand;
        // The padding to apply to the selection rectangle.
        private int _padding;
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

                if (IsFixedSize)
                {
                    _rect = new DX.RectangleF(value.Left, value.Top, FixedSize.Width, FixedSize.Height);
                }
                else
                {
                    _rect = value;
                }
                OnPropertyChanged();
            }
        }


        /// <summary>
        /// Property to set or return the current texture array index.
        /// </summary>
        public int TextureArrayIndex
        {
            get => _textureArrayIndex;
            set
            {
                if (_textureArrayIndex == value)
                {
                    return;
                }

                OnPropertyChanging();
                _textureArrayIndex = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return the position of the input interface.</summary>
        public DX.Point? Position
        {
            get => _settings.ManualRectangleEditorBounds == null ? (DX.Point?)null : new DX.Point(_settings.ManualRectangleEditorBounds.Value.Left, _settings.ManualRectangleEditorBounds.Value.Top);
            set
            {
                DX.Point? point = _settings.ManualRectangleEditorBounds == null ? (DX.Point?)null : new DX.Point(_settings.ManualRectangleEditorBounds.Value.Left, _settings.ManualRectangleEditorBounds.Value.Top);

                if (point == value)
                {
                    return;
                }

                OnPropertyChanging();
                _settings.ManualRectangleEditorBounds = value == null ? (DX.Rectangle?)null : new DX.Rectangle(value.Value.X, value.Value.Y, 1, 1);
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return whether the manual input interface is moving.</summary>
        public bool IsMoving
        {
            get => _isMoving;
            set
            {
                if (_isMoving == value)
                {
                    return;
                }

                OnPropertyChanging();
                _isMoving = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the command used to apply the coordinates to the sprite.
        /// </summary>
        public IEditorCommand<object> ApplyCommand
        {
            get => _applyCommand;
            set
            {
                if (_applyCommand == value)
                {
                    return;
                }

                OnPropertyChanging();
                _applyCommand = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the command used to cancel the operation.
        /// </summary>
        public IEditorCommand<object> CancelCommand
        {
            get => _cancelCommand;
            set
            {
                if (_cancelCommand == value)
                {
                    return;
                }

                OnPropertyChanging();
                _cancelCommand = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return whether to use a fixed size for the rectangle, or free form.</summary>
        public bool IsFixedSize
        {
            get => _isFixedSize;
            private set
            {
                if (_isFixedSize == value)
                {
                    return;
                }

                OnPropertyChanging();
                _isFixedSize = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the width and height to use when</summary>
        public DX.Size2F FixedSize
        {
            get => _fixedSize;
            private set
            {
                if (_fixedSize.Equals(value))
                {
                    return;
                }

                OnPropertyChanging();
                _fixedSize = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to return the command used to toggle the fixed size rectangle.
        /// </summary>
        public IEditorCommand<DX.Size2F> ToggleFixedSizeCommand
        {
            get;
        }

        /// <summary>Property to return the command to execute when assigning the fixed width/height.</summary>
        public IEditorCommand<DX.Size2F> SetFixedWidthHeightCommand
        {
            get;
        }

        /// <summary>
        /// Property to set or return the command to set the region to the full size of the texture.
        /// </summary>
        public IEditorCommand<object> SetFullSizeCommand
        {
            get => _fullSizeCommand;
            set
            {
                if (_fullSizeCommand == value)
                {
                    return;
                }

                OnPropertyChanging();
                _fullSizeCommand = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the padding, in pixels, applied to the selection rectangle.
        /// </summary>
        public int Padding
        {
            get => _padding;
            set
            {
                if (_padding == value)
                {
                    return;
                }

                OnPropertyChanging();
                _padding = value;
                OnPropertyChanged();
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to determine if the fixed size functionality can be toggled on or off.
        /// </summary>
        /// <param name="size">The current fixed width and height.</param>
        /// <returns><b>true</b> if the fixed size can be toggled, <b>false</b> if not.</returns>
        private bool CanToggleFixedSize(DX.Size2F size) => (size.Width > 0) && (size.Height > 0);

        /// <summary>
        /// Fucntion to toggle the fixed size functionality.
        /// </summary>
        /// <param name="size">The size to assign.</param>
        private void DoToggleFixedSize(DX.Size2F size)
        {
            try
            {
                IsFixedSize = !IsFixedSize;

                if (IsFixedSize)
                {
                    FixedSize = size;
                    Rectangle = new DX.RectangleF(_rect.X, _rect.Y, FixedSize.Width, FixedSize.Height);
                }
            }
            catch (Exception ex)
            {
                _messageDisplay.ShowError(ex, Resources.GORSPR_ERR_FIXED_SIZE);
            }
        }


        /// <summary>
        /// Function to determine if a fixed width/height can be assigned.
        /// </summary>
        /// <param name="size">The width and height to set.</param>
        /// <returns><b>true</b> if the fixed width/height values can be assigned, <b>false</b> if not.</returns>
        private bool CanSetFixedWidthHeight(DX.Size2F size) => (!size.Equals(DX.Size2F.Empty)) && (IsFixedSize);

        /// <summary>
        /// Fucntion to assign a new fixed width/height.
        /// </summary>
        /// <param name="size">The size to assign.</param>
        private void DoSetFixedWidthHeight(DX.Size2F size)
        {
            try
            {
                FixedSize = size;
                Rectangle = new DX.RectangleF(_rect.X, _rect.Y, FixedSize.Width, FixedSize.Height);
            }
            catch (Exception ex)
            {
                _messageDisplay.ShowError(ex, Resources.GORSPR_ERR_FIXED_SIZE);
            }
        }

        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(ManualInputParameters injectionParameters)
        {
            _messageDisplay = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(injectionParameters.MessageDisplay), nameof(injectionParameters));
            _settings = injectionParameters.Settings ?? throw new ArgumentMissingException(nameof(injectionParameters.Settings), nameof(injectionParameters));
        }
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="T:Gorgon.Editor.SpriteEditor.ManualRectangleEditor"/> class.</summary>
        public ManualRectangleEditor()
        {
            SetFixedWidthHeightCommand = new EditorCommand<DX.Size2F>(DoSetFixedWidthHeight, CanSetFixedWidthHeight);
            ToggleFixedSizeCommand = new EditorCommand<DX.Size2F>(DoToggleFixedSize, CanToggleFixedSize);
        }
        #endregion
    }
}
