
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: June 17, 2020 8:10:13 PM
// 

using System.Numerics;
using Gorgon.Animation;
using Gorgon.Editor.Content;
using Gorgon.Renderers;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// A factory for creating view models
/// </summary>
internal interface IViewModelFactory
{
    /// <summary>
    /// Function to create a new animation track.
    /// </summary>
    /// <param name="trackRegistration">The registration for the track.</param>
    /// <param name="keyCount">The number of keys in the track.</param>
    /// <param name="keyFrames">[Optional] Default key frames to apply to the track.</param>
    /// <returns>The track view model.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="trackRegistration"/> parameter is <b>null</b>.</exception>
    ITrack CreateTrack(GorgonTrackRegistration trackRegistration, int keyCount, IReadOnlyList<IKeyFrame> keyFrames = null);

    /// <summary>
    /// Function to create a key frame view model for texture data.
    /// </summary>
    /// <param name="time">The time for the key frame.</param>
    /// <param name="textureValue">The texture data to assign.</param>
    /// <returns>A new key frame view model with the specified data.</returns>
    IKeyFrame CreateKeyFrame(float time, ref TextureValue textureValue);

    /// <summary>
    /// Function to create an empty key frame view model for texture data.
    /// </summary>
    /// <param name="time">The time for the key frame.</param>
    /// <param name="dataType">The type of data to store in the key frame.</param>
    /// <returns>A new key frame view model with no data.</returns>
    IKeyFrame CreateKeyFrame(float time, AnimationTrackKeyType dataType);

    /// <summary>
    /// Function to create a key frame view model for floating point data.
    /// </summary>
    /// <param name="time">The time for the key frame.</param>
    /// <param name="dataType">The type of data for the key frame.</param>
    /// <param name="floatValue">The floating point data to assign.</param>
    /// <returns>A new key frame view model with the specified data.</returns>
    IKeyFrame CreateKeyFrame(float time, AnimationTrackKeyType dataType, Vector4 floatValue);

    /// <summary>
    /// Function to create the default texture track for an animation.
    /// </summary>
    /// <param name="primarySprite">The primary sprite to retrieve the texture from.</param>
    /// <param name="frameCount">The number of frames in the track.</param>
    /// <returns>The new texture track, or <b>null</b> if no primary sprite is available.</returns>
    Task<ITrack> CreateDefaultTextureTrackAsync((GorgonSprite sprite, IContentFile spriteFile, IContentFile textureFile) primarySprite, int frameCount);

    /// <summary>
    /// Function to create a new key frame based on the texture data in a sprite.
    /// </summary>
    /// <param name="time">The time, in seconds, for the key frame.</param>
    /// <param name="file">The file containing the sprite data to load.</param>
    /// <returns>A new <see cref="IKeyFrame"/> object.</returns>
    Task<IKeyFrame> CreateTextureKeyFrameAsync(float time, IContentFile file);
}
