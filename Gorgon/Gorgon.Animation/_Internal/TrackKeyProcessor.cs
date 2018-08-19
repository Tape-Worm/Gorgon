﻿#region MIT
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
// Created: August 18, 2018 7:52:17 PM
// 
#endregion

using DX = SharpDX;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;

namespace Gorgon.Animation
{
    /// <summary>
    /// A processor used to process the tracks for an animation.
    /// </summary>
    internal static class TrackKeyProcessor
    {
        /// <summary>
        /// Function to update the vector3 property of the object that the animation is being applied to.
        /// </summary>
        /// <param name="animation">The animation that is currently running.</param>
        /// <param name="track">The track to evaluate.</param>
        /// <param name="time">The current time for the animation.</param>
        /// <param name="result">The result value to apply to the vector3 object property.</param>
        /// <returns><b>true</b> if there's a value to update, <b>false</b> if not.</returns>
	    public static bool TryUpdateVector3(IGorgonAnimation animation, IGorgonTrack<GorgonKeyVector3> track, float time, out DX.Vector3 result)
	    {
	        switch (track.KeyFrames.Count)
	        {
	            case 0:
	                result = DX.Vector3.Zero;
	                return false;
	            case 1:
	                result = track.KeyFrames[0].Value; 
	                return true;
	        }

	        (GorgonKeyVector3 prev, GorgonKeyVector3 next, int prevKeyIndex, float deltaTime) = TweenKey.GetNearestKeys(track, time, animation.Length);

	        switch (track.InterpolationMode)
	        {
	            case TrackInterpolationMode.Linear:
	                DX.Vector3.Lerp(ref prev.Value, ref next.Value, deltaTime, out result);
	                break;
	            case TrackInterpolationMode.Spline:
	                result = (DX.Vector3)track.SplineController.GetInterpolatedValue(prevKeyIndex, deltaTime);
	                break;
	            default:
	                result = next.Value;
	                break;
	        }

	        return true;
	    }

        /// <summary>
        /// Function to update the color of the object that the animation is being applied to.
        /// </summary>
        /// <param name="animation">The animation that is currently running.</param>
        /// <param name="track">The track to evaluate.</param>
        /// <param name="time">The current time for the animation.</param>
        /// <param name="result">The result value to apply to the object color.</param>
        /// <returns><b>true</b> if there's a value to update, <b>false</b> if not.</returns>
        public static bool TryUpdateColor(IGorgonAnimation animation, IGorgonTrack<GorgonKeyGorgonColor> track, float time, out GorgonColor result)
        {
            switch (track.KeyFrames.Count)
            {
                case 0:
                    result = GorgonColor.BlackTransparent;
                    return false;
                case 1:
                    result = track.KeyFrames[0].Value; 
                    return true;
            }

            (GorgonKeyGorgonColor prev, GorgonKeyGorgonColor next, int prevKeyIndex, float deltaTime) = TweenKey.GetNearestKeys(track, time, animation.Length);

            switch (track.InterpolationMode)
            {
                case TrackInterpolationMode.Linear:
                    GorgonColor.Lerp(in prev.Value, in next.Value, deltaTime, out result);
                    break;
                case TrackInterpolationMode.Spline:
                    result = track.SplineController.GetInterpolatedValue(prevKeyIndex, deltaTime);
                    break;
                default:
                    result = next.Value;
                    break;
            }

            return true;
        }

        /// <summary>
        /// Function to update the rectangle boundaries of the object that the animation is being applied to.
        /// </summary>
        /// <param name="animation">The animation that is currently running.</param>
        /// <param name="track">The track to evaluate.</param>
        /// <param name="time">The current time for the animation.</param>
        /// <param name="result">The result value to apply to the object bounds.</param>
        /// <returns><b>true</b> if there's a value to update, <b>false</b> if not.</returns>
        public static bool TryUpdateRectBounds(IGorgonAnimation animation, IGorgonTrack<GorgonKeyRectangle> track, float time, out DX.RectangleF result)
        {
            switch (track.KeyFrames.Count)
            {
                case 0:
                    result = DX.RectangleF.Empty;
                    return false;
                case 1:
                    result = track.KeyFrames[0].Value; 
                    return true;
            }

            (GorgonKeyRectangle prev, GorgonKeyRectangle next, int prevKeyIndex, float deltaTime) = TweenKey.GetNearestKeys(track, time, animation.Length);

            switch (track.InterpolationMode)
            {
                case TrackInterpolationMode.Linear:
                    result = new DX.RectangleF(prev.Value.X + ((next.Value.X - prev.Value.X) * deltaTime),
                                               prev.Value.Y + ((next.Value.Y - prev.Value.Y) * deltaTime),
                                               prev.Value.Width + ((next.Value.Width - prev.Value.Width) * deltaTime),
                                               prev.Value.Height + ((next.Value.Height - prev.Value.Height) * deltaTime));
                    break;
                case TrackInterpolationMode.Spline:
                    DX.Vector4 splineResult = track.SplineController.GetInterpolatedValue(prevKeyIndex, deltaTime);
                    result = new DX.RectangleF(splineResult.X, splineResult.Y, splineResult.Z, splineResult.W);
                    break;
                default:
                    result = next.Value;
                    break;
            }

            return true;
        }

        /// <summary>
        /// Function to update the 2D texture values for the object that the animation is being applied to.
        /// </summary>
        /// <param name="animation">The animation that is currently running.</param>
        /// <param name="track">The track to evaluate.</param>
        /// <param name="time">The current time for the animation.</param>
        /// <param name="texture">The texture to switch to.</param>
        /// <param name="texCoordinates">The texture coordinates to use.</param>
        /// <param name="textureArrayIndex">The current texture array index to use.</param>
        /// <returns><b>true</b> if there's a value to update, <b>false</b> if not.</returns>
        public static bool TryUpdateTexture2D(IGorgonAnimation animation, float time, IGorgonTrack<GorgonKeyTexture2D> track, out GorgonTexture2DView texture, out DX.RectangleF texCoordinates, out int textureArrayIndex)
        {
            switch (track.KeyFrames.Count)
            {
                case 0:
                    texture = null;
                    texCoordinates = DX.RectangleF.Empty;
                    textureArrayIndex = 0;
                    return false;
                case 1:
                    GorgonKeyTexture2D texKey = track.KeyFrames[0];
                    texture = texKey.Value;
                    texCoordinates = texKey.TextureCoordinates;
                    textureArrayIndex = texKey.TextureArrayIndex;
                    return true;
            }
            
            (GorgonKeyTexture2D prev, GorgonKeyTexture2D next,_,_) = TweenKey.GetNearestKeys(track, time, animation.Length);

            GorgonKeyTexture2D correctKey = time.EqualsEpsilon(next.Time) ? next : prev;

            texture = correctKey.Value;
            texCoordinates = correctKey.TextureCoordinates;
            textureArrayIndex = correctKey.TextureArrayIndex;

            return true;
        }
    }
}
