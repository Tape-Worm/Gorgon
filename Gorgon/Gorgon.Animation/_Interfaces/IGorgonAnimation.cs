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

using System.Collections.Generic;
using Gorgon.Core;
using Gorgon.Graphics;
using Newtonsoft.Json;

namespace Gorgon.Animation
{
    /// <summary>
    /// The interface that defines an animation.
    /// </summary>
    /// <remarks>
    /// <para>
    /// An animation is composed of multiple <see cref="IGorgonAnimationTrack{T}">tracks</see> that represent various properties on an object that can be changed over time. Each track is itself composed of multiple
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
    /// <seealso cref="IGorgonAnimationTrack{T}"/>
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
        /// Property to return the tracks used to update any values using a single floating point value.
        /// </summary>
        [JsonProperty("singletracks")]
        IReadOnlyDictionary<string, IGorgonAnimationTrack<GorgonKeySingle>> SingleTracks
        {
            get;
        }

        /// <summary>
        /// Property to return the tracks used to update any values using a 2D vector.
        /// </summary>
        [JsonProperty("vector2tracks")]
        IReadOnlyDictionary<string, IGorgonAnimationTrack<GorgonKeyVector2>> Vector2Tracks
        {
            get;
        }

        /// <summary>
        /// Property to return the track used to update any values using a 3D vector.
        /// </summary>
        [JsonProperty("vector3tracks")]
        IReadOnlyDictionary<string, IGorgonAnimationTrack<GorgonKeyVector3>> Vector3Tracks
        {
            get;
        }

        /// <summary>
        /// Property to return the track used to update any values using a 4D vector.
        /// </summary>
        [JsonProperty("vector4tracks")]
        IReadOnlyDictionary<string, IGorgonAnimationTrack<GorgonKeyVector4>> Vector4Tracks
        {
            get;
        }

        /// <summary>
        /// Property to return the track used to update any values using a <see cref="GorgonColor"/>.
        /// </summary>
        [JsonProperty("colortracks")]
        IReadOnlyDictionary<string, IGorgonAnimationTrack<GorgonKeyGorgonColor>> ColorTracks
        {
            get;
        }

        /// <summary>
        /// Property to return the track used to update any values using a SharpDX <c>RectangleF</c>.
        /// </summary>
        [JsonProperty("recttracks")]
        IReadOnlyDictionary<string, IGorgonAnimationTrack<GorgonKeyRectangle>> RectangleTracks
        {
            get;
        }

        /// <summary>
        /// Property to return the tracks used for updating a 2D texture on an object.
        /// </summary>
        [JsonProperty("textures")]
        IReadOnlyDictionary<string, IGorgonAnimationTrack<GorgonKeyTexture2D>> Texture2DTracks
        {
            get;
        }
    }
}