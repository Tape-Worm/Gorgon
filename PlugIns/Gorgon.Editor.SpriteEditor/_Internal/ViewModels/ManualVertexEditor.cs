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
// Created: April 10, 2019 9:48:04 AM
// 
#endregion

using System;
using DX = SharpDX;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Editor.SpriteEditor.Properties;
using System.Linq;
using System.Collections.Generic;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The view model for the manual vertex edit window.
    /// </summary>
    internal class ManualVertexEditor
        : ViewModelBase<ManualInputParameters>, IManualVertexEditor
    {
        #region Variables.
        // The sprite editor settings.
        private ISettings _settings;
        // Flag to indicate whether the interface is active or not.
        private bool _isActive;
        // Flag to indicate whether the interface is moving or not.
        private bool _isMoving;
        // The currently selected vertex index.
        private int _selectedVertexIndex = -1;
        // The message display service.
        private IMessageDisplayService _messageDisplay;
		// The vertices being edited
        private readonly DX.Vector2[] _vertices = new DX.Vector2[4];
        // The command to apply the values.
        private IEditorCommand<object> _applyCommand;
        // The command to cancel the operatin.
        private IEditorCommand<object> _cancelCommand;
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
        
        /// <summary>Property to set or return the position of the input interface.</summary>
        public DX.Point? Position
        {
            get => _settings.ManualVertexEditorBounds == null ? (DX.Point?)null : new DX.Point(_settings.ManualVertexEditorBounds.Value.Left, _settings.ManualVertexEditorBounds.Value.Top);
            set
            {
                DX.Point? point = _settings.ManualVertexEditorBounds == null ? (DX.Point?)null : new DX.Point(_settings.ManualVertexEditorBounds.Value.Left, _settings.ManualVertexEditorBounds.Value.Top);

                if (point == value)
                {
                    return;
                }

                OnPropertyChanging();
                _settings.ManualVertexEditorBounds = value == null ? (DX.Rectangle?)null : new DX.Rectangle(value.Value.X, value.Value.Y, 1, 1);
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
        /// Property to set or return the vertex offset.
        /// </summary>
        public DX.Vector2 Offset
        {
            get => SelectedVertexIndex == -1 ? DX.Vector2.Zero : _vertices[SelectedVertexIndex];
            set
            {
                if (SelectedVertexIndex == -1)
                {
                    return;
                }

                if (_vertices[SelectedVertexIndex].Equals(ref value))
                {
                    return;
                }

                OnPropertyChanging();
                _vertices[SelectedVertexIndex] = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the vertex index.
        /// </summary>
        public int SelectedVertexIndex
        {
            get => _selectedVertexIndex;
            set
            {
                if (_selectedVertexIndex == value)
                {
                    return;
                }

                OnPropertyChanging();
                _selectedVertexIndex = value;
                OnPropertyChanged();
            }
        }

        /// <summary>
        /// Property to set or return the vertices that are being edited.
        /// </summary>
        public IReadOnlyList<DX.Vector2> Vertices
        {
            get => _vertices;
            set
            {
                if (_vertices == value)
                {
                    return;
                }

                if (value == null)
                {
                    OnPropertyChanging();
                    Array.Clear(_vertices, 0, _vertices.Length);
                    OnPropertyChanged();
                    NotifyPropertyChanged(nameof(Offset));
                    return;
                }

                if (_vertices.SequenceEqual(value))
                {
                    return;
                }
								
                OnPropertyChanging();
                for (int i = 0; i < _vertices.Length; ++i)
                {					
                    _vertices[i] = i < value.Count ? value[i] : DX.Vector2.Zero;
                }
                OnPropertyChanged();

                NotifyPropertyChanged(nameof(Offset));
            }
        }

        /// <summary>
        /// Property to return the command used to reset the vertex offset back to 0x0.
        /// </summary>
        public IEditorCommand<object> ResetOffsetCommand
        {
            get;
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
        #endregion

        #region Methods.
        /// <summary>
        /// Function to determine if the sprite vertices can be reset.
        /// </summary>
        /// <returns><b>true</b> if they can be reset, <b>false</b> if not.</returns>
        private bool CanResetVertices() => (SelectedVertexIndex >= 0) && (SelectedVertexIndex < _vertices.Length) && (!DX.Vector2.Zero.Equals(ref _vertices[SelectedVertexIndex]));

        /// <summary>
        /// Function to reset the vertex offsets to 0x0.
        /// </summary>
        private void DoResetVertices()
        {
            try
            {
                Offset = DX.Vector2.Zero;
            }
            catch (Exception ex)
            {
                _messageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
            }
        }

        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <exception cref="ArgumentMissingException">Thrown when any required parameters are missing.</exception>
        /// <remarks>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </remarks>
        protected override void OnInitialize(ManualInputParameters injectionParameters)
        {
            _settings = injectionParameters.Settings ?? throw new ArgumentMissingException(nameof(injectionParameters.Settings), nameof(injectionParameters));
            _messageDisplay = injectionParameters.MessageDisplay ?? throw new ArgumentMissingException(nameof(injectionParameters.MessageDisplay), nameof(injectionParameters));
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="ManualVertexEditor"/> class.</summary>
        public ManualVertexEditor() => ResetOffsetCommand = new EditorCommand<object>(DoResetVertices, CanResetVertices);
        #endregion
    }
}
