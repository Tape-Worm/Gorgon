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
// Created: May 15, 2020 10:52:06 PM
// 
#endregion

using System;
using System.Numerics;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Diagnostics;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.SpriteEditor.Properties;
using Gorgon.Editor.UI;
using Gorgon.Math;

namespace Gorgon.Editor.SpriteEditor
{
    /// <summary>
    /// The view model for the vertex editor.
    /// </summary>
    internal class SpriteVertexEditContext
        : EditorContext<SpriteVertexEditContextParameters>, ISpriteVertexEditContext
    {
        #region Constants.
        /// <summary>
        /// The name of the viewer associated with this context.
        /// </summary>
        public const string ViewerName = "ContextCornerOffsets";
        #endregion

        #region Variables.
        // The sprite content view model that owns this view model.
        private ISpriteContent _spriteContent;
        // The services from the host application.
        private IHostContentServices _hostServices;
        // The index of the selected vertex.
        private int _selectedVertex = -1;
        // The list of vertices.
        private readonly Vector2[] _vertices = new Vector2[4];
        #endregion

        #region Properties.
        /// <summary>Property to set or return the vertex offset for the selected vertex.</summary>
        public Vector2 Offset
        {
            get => SelectedVertexIndex == -1 ? Vector2.Zero : _vertices[SelectedVertexIndex];
            set
            {
                if (SelectedVertexIndex == -1)
                {
                    return;
                }

                if (_vertices[SelectedVertexIndex].Equals(value))
                {
                    return;
                }

                OnPropertyChanging();
                NotifyPropertyChanging(nameof(Vertices));

                _vertices[SelectedVertexIndex] = value;
                OnPropertyChanged();
                NotifyPropertyChanged(nameof(Vertices));
            }
        }

        /// <summary>Property to set or return the vertex index.</summary>
        public int SelectedVertexIndex
        {
            get => _selectedVertex;
            set
            {
                if (_selectedVertex == value)
                {
                    return;
                }

                OnPropertyChanging();
                _selectedVertex = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the command used to reset the vertex offset back to 0x0.</summary>
        public IEditorCommand<object> ResetOffsetCommand
        {
            get;
        }

        /// <summary>Property to set or return the vertices that are being edited.</summary>
        public IReadOnlyList<Vector2> Vertices
        {
            get => _vertices;
            set
            {
                if (_vertices == value)
                {
                    return;
                }

                if (value is null)
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
                    _vertices[i] = i < value.Count ? value[i] : Vector2.Zero;
                }
                OnPropertyChanged();

                NotifyPropertyChanged(nameof(Offset));
            }
        }

        /// <summary>Property to return the context name.</summary>
        /// <remarks>This value is used as a unique ID for the context.</remarks>
        public override string Name => ViewerName;

        /// <summary>Property to set or return the command used to apply the updated clipping coordinates.</summary>
        public IEditorCommand<object> ApplyCommand
        {
            get;
            set;
        }

        /// <summary>Property to return the command used to cancel the sprite clipping operation.</summary>
        public IEditorCommand<object> CancelCommand
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to determine if the vertices can be reset to their original positions.
        /// </summary>
        /// <returns><b>true</b> if the vertices can be reset, <b>false</b> if not.</returns>
        private bool CanReset() => SelectedVertexIndex == -1
                ? _vertices.Any(item => (!item.X.EqualsEpsilon(0)) || (!item.Y.EqualsEpsilon(0)))
                : (!_vertices[SelectedVertexIndex].X.EqualsEpsilon(0)) || (!_vertices[SelectedVertexIndex].Y.EqualsEpsilon(0));

        /// <summary>
        /// Function to reset the vertex offsets back to the default position.
        /// </summary>
        private void DoReset()
        {
            Vector2[] defaultPos = null;

            try
            {
                defaultPos = ArrayPool<Vector2>.Shared.Rent(_vertices.Length);
                for (int i = 0; i < _vertices.Length; ++i)
                {
                    defaultPos[i] = SelectedVertexIndex == -1 ? Vector2.Zero : _vertices[i];
                }

                if (SelectedVertexIndex != -1)
                {
                    defaultPos[SelectedVertexIndex] = Vector2.Zero;
                }

                Vertices = defaultPos;
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORSPR_ERR_UPDATING);
            }
            finally
            {
                if (defaultPos is not null)
                {
                    ArrayPool<Vector2>.Shared.Return(defaultPos);
                }
            }
        }

        /// <summary>
        /// Function to cancel the operation.
        /// </summary>
        private void DoCancel()
        {
            try
            {
                _spriteContent.CommandContext = null;
            }
            catch (Exception ex)
            {
                _hostServices.Log.Print("Error cancelling sprite vertex editing.", LoggingLevel.Verbose);
                _hostServices.Log.LogException(ex);
            }
        }

        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        protected override void OnInitialize(SpriteVertexEditContextParameters injectionParameters)
        {
            _hostServices = injectionParameters.HostServices;
            _spriteContent = injectionParameters.SpriteContent;            
        }

        /// <summary>Function called when the associated view is loaded.</summary>
        public override void OnLoad()
        {
            base.OnLoad();
            SelectedVertexIndex = -1;
            for (int i = 0; i < _vertices.Length; ++i)
            {
                _vertices[i] = new Vector2(_spriteContent.VertexOffsets[i].X, _spriteContent.VertexOffsets[i].Y);
            }
        }

        /// <summary>Function called when the associated view is unloaded.</summary>
        public override void OnUnload()
        {
            SelectedVertexIndex = -1;
            Array.Clear(_vertices, 0, _vertices.Length);
            base.OnUnload();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="SpriteVertexEditContext"/> class.</summary>
        public SpriteVertexEditContext()
        {
            ResetOffsetCommand = new EditorCommand<object>(DoReset, CanReset);
            CancelCommand = new EditorCommand<object>(DoCancel);
        }
        #endregion
    }
}
