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
using Newtonsoft.Json;

namespace Gorgon.Animation
{
    /// <summary>
    /// The interface that defines an animation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An animation is composed of multiple <see cref="IGorgonTrack{T}">tracks</see> that represent various properties on an object that can be changed over time. Each track is itself composed of multiple
    /// markers in time called <see cref="IGorgonKeyFrame">key frames</see>. These markers indicate what the value of a property should be at a given time.
    /// </para>
    /// <para>
    /// To create an animation, a <see cref="GorgonAnimationBuilder"/> is used to build up the key frames for each track that is to be animated. Because of this, the animation tracks and key frames are
    /// immutable.
    /// </para>
    /// <para>
    /// The available tracks on an animation are for common usage scenarios such as moving, scaling, rotating, etc... Please note that some animation controllers will not make use of some track types.
    /// </para>
    /// <para>
    /// In order to play an animation, a <see cref="GorgonAnimationController{T}"/> must be used.
    /// </para>
    /// </remarks>
    /// <seealso cref="IGorgonTrack{T}"/>
    /// <seealso cref="IGorgonKeyFrame"/>
    /// <seealso cref="GorgonAnimationBuilder"/>
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
        [JsonIgnore]
        float Speed
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the length of the animation (in seconds).
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
        [JsonProperty("positions")]
        IGorgonTrack<GorgonKeyVector3> PositionTrack
        {
            get;
        }

        /// <summary>
        /// Property to return the track used to update the rotation of an object.
        /// </summary>
        /// <remarks>
        /// <para>
        /// The rotation track is made up of <see cref="GorgonKeyVector3"/> key frame types where the X, Y and Z values represent the x axis, y axis and z axis of rotation. All values are in degrees.
        /// </para>
        /// <para>
        /// Note that not all controller types will use every axis when rotating. 
        /// </para>
        /// </remarks>
        [JsonProperty("rotations")]
        IGorgonTrack<GorgonKeyVector3> RotationTrack
        {
            get;
        }

        /// <summary>
        /// Property to return the track used to update the scale of an object.
        /// </summary>
        [JsonProperty("scales")]
        IGorgonTrack<GorgonKeyVector3> ScaleTrack
        {
            get;
        }

        /// <summary>
        /// Property to return the track used to update the color of an object.
        /// </summary>
        [JsonProperty("colors")]
        IGorgonTrack<GorgonKeyGorgonColor> ColorTrack
        {
            get;
        }

        /// <summary>
        /// Property to return the track used for rectangular boundaries of an object.
        /// </summary>
        [JsonProperty("bounds")]
        IGorgonTrack<GorgonKeyRectangle> RectBoundsTrack
        {
            get;
        }

        /// <summary>
        /// Property to return the track used for the size of an object.
        /// </summary>
        [JsonProperty("size")]
        IGorgonTrack<GorgonKeyVector3> SizeTrack
        {
            get;
        }

        /// <summary>
        /// Property to return the track used for updating a 2D texture on an object.
        /// </summary>
        [JsonProperty("textures")]
        IGorgonTrack<GorgonKeyTexture2D> Texture2DTrack
        {
            get;
        }
    }
}