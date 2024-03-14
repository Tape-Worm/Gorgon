
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
// Created: June 16, 2020 4:13:21 PM
// 


using System.Numerics;
using Gorgon.Animation;
using Gorgon.Editor.Content;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.UI.ViewModels;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// Parameters for a <see cref="IKeyFrame"/> view model
/// </summary>
internal class KeyFrameParameters
    : ViewModelInjection<IHostContentServices>
{
    /// <summary>
    /// Property to return the time index for the key.
    /// </summary>
    public float Time
    {
        get;
    }

    /// <summary>
    /// Property to return the floating point values to animate.
    /// </summary>
    public Vector4? FloatValues
    {
        get;
    }

    /// <summary>
    /// Property to return the texture value to animate.
    /// </summary>
    public TextureValue? TextureValue
    {
        get;
    }

    /// <summary>
    /// Property to return the type of data in the key.
    /// </summary>
    public AnimationTrackKeyType KeyType
    {
        get;
    }

    /// <summary>Initializes a new instance of the <see cref="KeyFrameParameters"/> class.</summary>
    /// <param name="key">The animation key to consume.</param>
    /// <param name="hostServices">The host services from the application.</param>
    public KeyFrameParameters(GorgonKeySingle key, IHostContentServices hostServices)
        : base(hostServices)
    {
        Time = key.Time;
        KeyType = AnimationTrackKeyType.Single;
        TextureValue = null;
        FloatValues = new Vector4(key.Value, 0, 0, 0);
    }

    /// <summary>Initializes a new instance of the <see cref="KeyFrameParameters"/> class.</summary>
    /// <param name="key">The animation key to consume.</param>
    /// <param name="hostServices">The host services from the application.</param>
    public KeyFrameParameters(GorgonKeyVector2 key, IHostContentServices hostServices)
        : base(hostServices)
    {
        Time = key.Time;
        KeyType = AnimationTrackKeyType.Vector2;
        TextureValue = null;
        FloatValues = new Vector4(key.Value, 0, 0);
    }

    /// <summary>Initializes a new instance of the <see cref="KeyFrameParameters"/> class.</summary>
    /// <param name="key">The animation key to consume.</param>
    /// <param name="hostServices">The host services from the application.</param>
    public KeyFrameParameters(GorgonKeyVector3 key, IHostContentServices hostServices)
        : base(hostServices)
    {
        Time = key.Time;
        KeyType = AnimationTrackKeyType.Vector3;
        TextureValue = null;
        FloatValues = new Vector4(key.Value, 0);
    }

    /// <summary>Initializes a new instance of the <see cref="KeyFrameParameters"/> class.</summary>
    /// <param name="key">The animation key to consume.</param>
    /// <param name="hostServices">The host services from the application.</param>
    public KeyFrameParameters(GorgonKeyVector4 key, IHostContentServices hostServices)
        : base(hostServices)
    {
        KeyType = AnimationTrackKeyType.Vector4;
        TextureValue = null;
        FloatValues = key.Value;
    }

    /// <summary>Initializes a new instance of the <see cref="KeyFrameParameters"/> class.</summary>
    /// <param name="key">The animation key to consume.</param>
    /// <param name="hostServices">The host services from the application.</param>
    public KeyFrameParameters(GorgonKeyRectangle key, IHostContentServices hostServices)
        : base(hostServices)
    {
        Time = key.Time;
        KeyType = AnimationTrackKeyType.Rectangle;
        TextureValue = null;
        FloatValues = new Vector4(key.Value.Left, key.Value.Top, key.Value.Right, key.Value.Bottom);
    }

    /// <summary>Initializes a new instance of the <see cref="KeyFrameParameters"/> class.</summary>
    /// <param name="key">The animation key to consume.</param>
    /// <param name="hostServices">The host services from the application.</param>
    public KeyFrameParameters(GorgonKeyGorgonColor key, IHostContentServices hostServices)
        : base(hostServices)
    {
        Time = key.Time;
        KeyType = AnimationTrackKeyType.Color;
        TextureValue = null;
        FloatValues = new Vector4(key.Value.Red, key.Value.Green, key.Value.Blue, key.Value.Alpha);
    }

    /// <summary>Initializes a new instance of the <see cref="KeyFrameParameters"/> class.</summary>
    /// <param name="textureFile">The file containing the texture.</param>
    /// <param name="key">The animation key to consume.</param>
    /// <param name="hostServices">The host services from the application.</param>
    public KeyFrameParameters(IContentFile textureFile, GorgonKeyTexture2D key, IHostContentServices hostServices)
        : base(hostServices)
    {
        Time = key.Time;
        KeyType = AnimationTrackKeyType.Texture2D;
        TextureValue = new TextureValue(key.Value, textureFile, key.TextureArrayIndex, key.TextureCoordinates);
    }

    /// <summary>Initializes a new instance of the <see cref="KeyFrameParameters"/> class.</summary>
    /// <param name="time">The time index for the key frame.</param>
    /// <param name="type">The type of data to store in the key.</param>
    /// <param name="value">The floating point values to assign.</param>
    /// <param name="hostServices">The host serivces.</param>
    public KeyFrameParameters(float time, AnimationTrackKeyType type, Vector4 value, IHostContentServices hostServices)
        : base(hostServices)
    {
        Time = time;
        KeyType = type;
        FloatValues = value;
    }

    /// <summary>Initializes a new instance of the <see cref="KeyFrameParameters"/> class.</summary>
    /// <param name="time">The time index for the key frame.</param>
    /// <param name="textureValue">The texture value to assign.</param>
    /// <param name="hostServices">The host services.</param>
    public KeyFrameParameters(float time, ref TextureValue textureValue, IHostContentServices hostServices)
        : base(hostServices)
    {
        Time = time;
        KeyType = AnimationTrackKeyType.Texture2D;
        TextureValue = textureValue;
    }
}
