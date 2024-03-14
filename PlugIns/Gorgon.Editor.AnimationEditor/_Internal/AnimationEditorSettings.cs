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
// Created: March 22, 2019 10:06:04 AM
// 
#endregion

using Newtonsoft.Json;
using DX = SharpDX;

namespace Gorgon.Editor.AnimationEditor;

/// <summary>
/// The settings for the animation importer plug in.
/// </summary>
internal class AnimationImportSettings
{
    /// <summary>
    /// Property to return the list of additional animation codec plug ins to load.
    /// </summary>
    [JsonProperty]
    public Dictionary<string, string> CodecPlugInPaths
    {
        get;
        private set;
    } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Property to set or return the last codec plug in path.
    /// </summary>
    [JsonProperty]
    public string LastCodecPlugInPath
    {
        get;
        set;
    }
}

/// <summary>
/// The settings for the sprite editor plug in.
/// </summary>
internal class AnimationEditorSettings
{
    /// <summary>
    /// Property to set or return the offset, in pixels, of the splitter on the editor view.
    /// </summary>
    [JsonProperty]
    public int SplitOffset
    {
        get;
        set;
    } = 569;

    /// <summary>
    /// Property to set or return whether the background image should be animated when no primary sprite is present.
    /// </summary>
    [JsonProperty]
    public bool AnimateBgNoPrimary
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Property to set or return whether to use onion skinning.
    /// </summary>
    [JsonProperty]
    public bool UseOnionSkin
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Property to set or return the default screen resolution for the animation.
    /// </summary>
    [JsonProperty]
    public DX.Size2 DefaultResolution
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return whether to create a texture track in an empty animation on primary sprite assignment.
    /// </summary>
    [JsonProperty]
    public bool AddTextureTrackForPrimarySprite
    {
        get;
        set;
    } = true;

    /// <summary>
    /// Property to set or return whether a warning will be shown when an animation with unsupported tracks is loaded.
    /// </summary>
    public bool WarnUnsupportedTracks
    {
        get;
        set;
    } = true;

    /// <summary>Initializes a new instance of the <see cref="AnimationEditorSettings"/> class.</summary>
    public AnimationEditorSettings() => DefaultResolution = new DX.Size2(Screen.PrimaryScreen.Bounds.Width, Screen.PrimaryScreen.Bounds.Height);
}
