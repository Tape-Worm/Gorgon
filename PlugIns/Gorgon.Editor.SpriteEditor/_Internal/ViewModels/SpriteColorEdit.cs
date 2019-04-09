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
// Created: April 4, 2019 8:59:29 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Editor.Services;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Graphics;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The view model for the sprite color editor.
    /// </summary>
    internal class SpriteColorEdit
        : ViewModelBase<IViewModelInjection>, ISpriteColorEdit
    {
        #region Variables.
        // The service used to show messages to the user.
        private IMessageDisplayService _messageDisplay; 
        // The original color for the sprite.
        private readonly GorgonColor[] _originalColor = new GorgonColor[4];
        // The current color for the sprite.
        private readonly GorgonColor[] _color = new GorgonColor[4];
		// The currently selected vertex.
        private readonly bool[] _selectedVertex = new bool[4];
        // Flag to indicate whether the panel is active or not.
        private bool _isActive;
		// The currently selected color.
        private GorgonColor _selectedColor = GorgonColor.BlackTransparent;
        #endregion

        #region Properties.
        /// <summary>Property to return whether the panel is modal.</summary>
        public bool IsModal => true;


        /// <summary>
        /// Property to set or return the currently selected color for an individual vertex.
        /// </summary>
        public GorgonColor SelectedColor
        {
            get => _selectedColor;
            set
            {
                if (_selectedColor.Equals(in value))
                {
                    return;
                }

                OnPropertyChanging();
                _selectedColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return the selected vertex.</summary>
        public IReadOnlyList<bool> SelectedVertices
        {
            get => _selectedVertex;
            set
            {
                if (value == null)
                {
                    for (int i = 0; i < _selectedVertex.Length; ++i)
                    {
                        _selectedVertex[i] = false;
                    }
                    return;
                }

                if (value.SequenceEqual(_selectedVertex))
                {
                    return;
                }

                OnPropertyChanging();
                for (int i = 0; i < _selectedVertex.Length; ++i)
                {
                    _selectedVertex[i] = i < value.Count ? value[i] : false;
                }
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return the color to apply to the sprite.</summary>
        public IReadOnlyList<GorgonColor> SpriteColor
        {
            get => _color;
            set
            {
                if (value == null)
                {
                    return;
                }
								
                if (_color.SequenceEqual(value))
                {
                    return;
                }

                OnPropertyChanging();
                for (int i = 0; i < _color.Length; ++i)
                {
                    _color[i] = i < value.Count ? value[i] : GorgonColor.BlackTransparent;
                }
                OnPropertyChanged();
            }
        }

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

        /// <summary>Property to set or return the original color for the sprite.</summary>
        public IReadOnlyList<GorgonColor> OriginalSpriteColor
        {
            get => _originalColor;
            set
            {
                if (value == null)
                {
                    return;
                }

                if (_originalColor.SequenceEqual(value))
                {
                    return;
                }

                OnPropertyChanging();
                for (int i = 0; i < _originalColor.Length; ++i)
                {
                    _originalColor[i] = i < value.Count ? value[i] : GorgonColor.BlackTransparent;
                }
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the command used to cancel the operation.</summary>
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
        #endregion

        #region Methods.
        /// <summary>
        /// Function called when the panel is cancelled by the user.
        /// </summary>
        private void DoCancel()
        {
            try
            {
                IsActive = false;
            }
            catch (Exception ex)
            {
                _messageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
            }
        }

        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(IViewModelInjection injectionParameters) =>
            _messageDisplay = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(IViewModelInjection.MessageDisplay), nameof(injectionParameters));
        #endregion

        #region Constructor.
        /// <summary>Initializes a new instance of the <see cref="SpriteColorEdit"/> class.</summary>
        public SpriteColorEdit()
        {
            CancelCommand = new EditorCommand<object>(DoCancel);

            for (int i = 0; i < 4; ++i)
            {
                _selectedVertex[i] = true;
                _color[i] = _originalColor[i] = GorgonColor.BlackTransparent;
            }
        }
        #endregion
    }
}
