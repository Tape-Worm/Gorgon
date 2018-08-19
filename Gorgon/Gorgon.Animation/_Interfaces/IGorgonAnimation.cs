#region MIT
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
// Created: August 16, 2018 2:23:22 PM
// 
#endregion

using Gorgon.Core;

namespace Gorgon.Animation
{
    /// <summary>
    /// The interface that defines an animation.
    /// </summary>
    public interface IGorgonAnimation
        : IGorgonNamedObject
    {
        /// <summary>
        /// Property to set or return the number of times to loop an animation.
        /// </summary>
        int LoopCount
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the speed of the animation.
        /// </summary>
        /// <remarks>Setting this value to a negative value will make the animation play backwards.</remarks>
        float Speed
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the length of the animation (in seconds).
        /// </summary>
        float Length
        {
            get;
        }

        /// <summary>
        /// Property to set or return whether this animation should be looping or not.
        /// </summary>
        bool IsLooped
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the track used to update positioning of an object.
        /// </summary>
        IGorgonTrack<GorgonKeyVector3> PositionTrack
        {
            get;
        }

        /// <summary>
        /// Property to return the track used to update the rotation of an object.
        /// </summary>
        IGorgonTrack<GorgonKeyVector3> RotationTrack
        {
            get;
        }

        /// <summary>
        /// Property to return the track used to update the scale of an object.
        /// </summary>
        IGorgonTrack<GorgonKeyVector3> ScaleTrack
        {
            get;
        }

        /// <summary>
        /// Property to return the track used to update the color of an object.
        /// </summary>
        IGorgonTrack<GorgonKeyGorgonColor> ColorTrack
        {
            get;
        }

        /// <summary>
        /// Property to return the track used for rectangular boundaries of an object.
        /// </summary>
        IGorgonTrack<GorgonKeyRectangle> RectBoundsTrack
        {
            get;
        }

        /// <summary>
        /// Property to return the track used for updating a 2D texture on an object.
        /// </summary>
        IGorgonTrack<GorgonKeyTexture2D> Texture2DTrack
        {
            get;
        }
    }
}