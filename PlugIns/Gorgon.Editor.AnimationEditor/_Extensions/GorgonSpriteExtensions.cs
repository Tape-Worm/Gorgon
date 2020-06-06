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
// Created: July 15, 2020 3:56:31 PM
// 
#endregion

using DX = SharpDX;
using Gorgon.Renderers;

namespace Gorgon.Editor.AnimationEditor
{
    /// <summary>
    /// Extension methods for the sprite.
    /// </summary>
    internal static class GorgonSpriteExtensions
    {
        /// <summary>
        /// Function to retrieve the floating point values from a sprite property.
        /// </summary>
        /// <param name="sprite">The sprite to read.</param>
        /// <param name="property">The property to read.</param>
        /// <returns>The floating point values.</returns>
        public static DX.Vector4 GetFloatValues(this GorgonSprite sprite, TrackSpriteProperty property)
        {
            if (sprite == null)
            {
                return DX.Vector4.Zero;
            }

            switch (property)
            {
                case TrackSpriteProperty.Angle:
                    return new DX.Vector4(sprite.Angle, 0, 0, 0);
                case TrackSpriteProperty.Opacity:
                    return new DX.Vector4(sprite.Color.Alpha, 0, 0, 0);
                case TrackSpriteProperty.Color:
                    return sprite.Color;
                case TrackSpriteProperty.UpperLeftColor:
                    return sprite.CornerColors.UpperLeft;
                case TrackSpriteProperty.UpperRightColor:
                    return sprite.CornerColors.UpperRight;
                case TrackSpriteProperty.LowerLeftColor:
                    return sprite.CornerColors.LowerLeft;
                case TrackSpriteProperty.LowerRightColor:
                    return sprite.CornerColors.LowerRight;
                case TrackSpriteProperty.Anchor:                
                    return new DX.Vector4(sprite.Anchor, 0, 0);
                case TrackSpriteProperty.AnchorAbsolute:
                    return new DX.Vector4(sprite.AbsoluteAnchor, 0, 0);
                case TrackSpriteProperty.UpperLeft:
                    return new DX.Vector4(sprite.CornerOffsets.UpperLeft, 0);                    
                case TrackSpriteProperty.UpperRight:
                    return new DX.Vector4(sprite.CornerOffsets.UpperRight, 0);                    
                case TrackSpriteProperty.LowerLeft:
                    return new DX.Vector4(sprite.CornerOffsets.LowerLeft, 0);                    
                case TrackSpriteProperty.LowerRight:
                    return new DX.Vector4(sprite.CornerOffsets.LowerRight, 0);
                case TrackSpriteProperty.Position:
                    return new DX.Vector4(sprite.Position, 0, 0);
                case TrackSpriteProperty.Size:
                    return new DX.Vector4(sprite.Size.Width, sprite.Size.Height, 0, 0);
                case TrackSpriteProperty.Scale:
                    return new DX.Vector4(sprite.Scale, 0, 0);
                case TrackSpriteProperty.ScaledSize:
                    return new DX.Vector4(sprite.ScaledSize.Width, sprite.ScaledSize.Height, 0, 0);
                default:
                    return DX.Vector4.Zero;
            }
        }

        /// <summary>
        /// Function to assign a sprite property value based on the track property and values from the keyframe.
        /// </summary>
        /// <param name="sprite">The sprite to update.</param>
        /// <param name="property">The property to update.</param>
        /// <param name="values">The values to assign.</param>
        public static void SetFloatValues(this GorgonSprite sprite, TrackSpriteProperty property, DX.Vector4 values)
        {
            if (sprite == null)
            {
                return;
            }

            switch (property)
            {
                case TrackSpriteProperty.Angle:
                    sprite.Angle = values.X;
                    break;
                case TrackSpriteProperty.Opacity:
                    sprite.Color = new Graphics.GorgonColor(sprite.Color, values.X);
                    break;
                case TrackSpriteProperty.UpperLeftColor:
                    sprite.CornerColors.UpperLeft = new Graphics.GorgonColor(values);
                    break;
                case TrackSpriteProperty.UpperRightColor:
                    sprite.CornerColors.UpperRight = new Graphics.GorgonColor(values);
                    break;
                case TrackSpriteProperty.LowerLeftColor:
                    sprite.CornerColors.LowerLeft = new Graphics.GorgonColor(values);
                    break;
                case TrackSpriteProperty.LowerRightColor:
                    sprite.CornerColors.LowerRight = new Graphics.GorgonColor(values);
                    break;
                case TrackSpriteProperty.Color:
                    sprite.Color = new Graphics.GorgonColor(values);
                    break;
                case TrackSpriteProperty.Anchor:
                    sprite.Anchor = (DX.Vector2)values;
                    break;
                case TrackSpriteProperty.AnchorAbsolute:
                    sprite.AbsoluteAnchor = (DX.Vector2)values;
                    break;
                case TrackSpriteProperty.UpperLeft:
                    sprite.CornerOffsets.UpperLeft = (DX.Vector3)values;
                    break;
                case TrackSpriteProperty.UpperRight:
                    sprite.CornerOffsets.UpperRight = (DX.Vector3)values;
                    break;
                case TrackSpriteProperty.LowerLeft:
                    sprite.CornerOffsets.LowerLeft = (DX.Vector3)values;
                    break;
                case TrackSpriteProperty.LowerRight:
                    sprite.CornerOffsets.LowerRight = (DX.Vector3)values;
                    break;
                case TrackSpriteProperty.Position:
                    sprite.Position = (DX.Vector2)values;
                    break;
                case TrackSpriteProperty.Size:
                    sprite.Size = new DX.Size2F(values.X, values.Y);
                    break;
                case TrackSpriteProperty.Scale:
                    sprite.Scale = (DX.Vector2)values;
                    break;
                case TrackSpriteProperty.ScaledSize:
                    sprite.ScaledSize = new DX.Size2F(values.X, values.Y);
                    break;
            }
        }
    }
}
