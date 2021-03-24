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
// Created: July 6, 2020 11:22:06 PM
// 
#endregion

using System.Numerics;
using Gorgon.Editor.UI;
using Gorgon.Graphics;

namespace Gorgon.Editor.AnimationEditor
{
    /// <summary>
    /// The view model for editing the colors for a key.
    /// </summary>
    internal class ColorValueEditor
        : KeyValueEditor, IColorValueEditor
    {
        #region Variables.
        // The original color for the sprite.
        private GorgonColor _originalColor = GorgonColor.White;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether to edit alpha values or color values.
        /// </summary>
        public bool AlphaOnly => (Track is not null) && (Track.Track.SpriteProperty == TrackSpriteProperty.Opacity);

        /// <summary>Property to set or return the color to apply to the sprite.</summary>
        public GorgonColor NewColor
        {
            get
            {
                Vector4 values = WorkingSprite?.GetFloatValues(Track.Track.SpriteProperty) ?? GorgonColor.White;

                return AlphaOnly ? new GorgonColor(0, 0, 0, values.X) : values;
            }
            set
            {
                if (Track is null)
                {
                    return;
                }

                GorgonColor color = WorkingSprite?.GetFloatValues(Track.Track.SpriteProperty) ?? GorgonColor.White;
                if (color.Equals(in value))
                {
                    return;
                }

                OnPropertyChanging();
                NotifyPropertyChanging(nameof(IKeyValueEditor.Value));
                WorkingSprite.SetFloatValues(Track.Track.SpriteProperty, AlphaOnly ? new Vector4(value.Alpha, 0, 0, 0) : value);
                OnPropertyChanged();
                NotifyPropertyChanged(nameof(IKeyValueEditor.Value));
            }
        }

        /// <summary>Property to set or return the original color for the sprite.</summary>
        public GorgonColor OriginalColor
        {
            get => _originalColor;
            set
            {
                if (value.Equals(_originalColor))
                {
                    return;
                }

                OnPropertyChanging();
                _originalColor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to set or return the value for the key frame.</summary>
        Vector4 IKeyValueEditor.Value
        {
            get
            {
                Vector4 values = WorkingSprite?.GetFloatValues(Track.Track.SpriteProperty) ?? Vector4.One;

                return AlphaOnly ? new Vector4(values.X, 0, 0, 0) : values;
            }
            set
            {
                if (Track is null)
                {
                    return;
                }

                Vector4 colorValue = WorkingSprite?.GetFloatValues(Track.Track.SpriteProperty) ?? Vector4.One;
                if (colorValue.Equals(value))
                {
                    return;
                }

                OnPropertyChanging();
                NotifyPropertyChanging(nameof(NewColor));
                WorkingSprite.SetFloatValues(Track.Track.SpriteProperty, AlphaOnly ? new Vector4(value.X, 0, 0, 0) : value);
                OnPropertyChanged();
                NotifyPropertyChanged(nameof(NewColor));
            }
        }
        #endregion

        #region Methods.
        /// <summary>Function used to notify when the <see cref="KeyValueEditor{T}.Track"/> property is assigned.</summary>
        protected override void OnTrackSet()
        {
            NotifyPropertyChanging(nameof(AlphaOnly));
            NotifyPropertyChanged(nameof(AlphaOnly));
        }

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
