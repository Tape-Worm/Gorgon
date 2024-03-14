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
// Created: June 5, 2020 3:45:31 PM
// 
#endregion

using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Editor.AnimationEditor.Services;
using Gorgon.Editor.Content;
using Gorgon.Editor.Metadata;
using Gorgon.Editor.PlugIns;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.IO;
using Gorgon.Math;
using Gorgon.Renderers;
using Microsoft.IO;
using DX = SharpDX;

namespace Gorgon.Editor.AnimationEditor;

#region Enums.
/// <summary>
/// The sprite property affected by the track.
/// </summary>
internal enum TrackSpriteProperty
{
    /// <summary>
    /// No track.
    /// </summary>
    None = 0,
    /// <summary>
    /// Sprite position.
    /// </summary>
    Position = 1,
    /// <summary>
    /// Sprite anchor (relative)
    /// </summary>
    Anchor = 2,
    /// <summary>
    /// Sprite anchor (absolute)
    /// </summary>
    AnchorAbsolute = 3,
    /// <summary>
    /// Sprite size.
    /// </summary>
    Size = 4,
    /// <summary>
    /// Sprite scale (relative)
    /// </summary>
    Scale = 5,
    /// <summary>
    /// Sprite scale (absolute pixels)
    /// </summary>
    ScaledSize = 6,
    /// <summary>
    /// Lower right position.
    /// </summary>
    LowerRight = 7,
    /// <summary>
    /// Upper right position.
    /// </summary>
    UpperRight = 8,
    /// <summary>
    /// Lower left position.
    /// </summary>
    LowerLeft = 9,
    /// <summary>
    /// Upper left position.
    /// </summary>
    UpperLeft = 10,
    /// <summary>
    /// Angle of rotation.
    /// </summary>
    Angle = 11,
    /// <summary>
    /// The opacity of the sprite.
    /// </summary>
    Opacity = 12,
    /// <summary>
    /// The boundaries of the sprite.
    /// </summary>
    Bounds = 13,
    /// <summary>
    /// The texture for the sprite.
    /// </summary>
    Texture = 14,
    /// <summary>
    /// The color of the sprite.
    /// </summary>
    Color = 15,
    /// <summary>
    /// The color of the lower right for the sprite.
    /// </summary>
    LowerRightColor = 16,
    /// <summary>
    /// The color of the lower left for the sprite.
    /// </summary>
    LowerLeftColor = 17,
    /// <summary>
    /// The color of the upper right for the sprite.
    /// </summary>
    UpperRightColor = 18,
    /// <summary>
    /// The color of the upper left for the sprite.
    /// </summary>
    UpperLeftColor = 19
}
#endregion

/// <summary>
/// Gorgon sprite editor content plug in interface.
/// </summary>
internal class AnimationEditorPlugIn
    : ContentPlugIn, IContentPlugInMetadata, IViewModelFactory
{
    #region Constants.
    // The attribute key name for the animation codec attribute.
    private const string CodecAttr = "AnimationCodec";

    /// <summary>
    /// The name of the background image dependency.
    /// </summary>
    public const string BgImageDependencyName = "bgimage";
    #endregion

    #region Variables.
    // The default codec for the animations.
    private IGorgonAnimationCodec _defaultCodec;
    // The default image codec for textures.
    private IGorgonImageCodec _defaultImageCodec;
    // The default sprite codec for sprites.
    private IGorgonSpriteCodec _defaultSpriteCodec;
    // The settings for the plug in.
    private AnimationEditorSettings _settings = new();
    // The settings for the plug in.
    private Settings _pluginSettings;
    // The texture cache for animation textures.
    private ITextureCache _textureCache;
    // The undo service for the plug in.
    private IUndoService _undoService;
    // The service used to set up a new animation.
    private NewAnimationService _newAnimation;
    // The service for handling animation I/O functionality.
    private AnimationIOService _ioService;
    // The list of excluded track types.
    private static readonly List<GorgonTrackRegistration> _excludedTracks = new()
    {
        GorgonSpriteAnimationController.BoundsTrack,
        GorgonSpriteAnimationController.TextureArrayIndexTrack,
        GorgonSpriteAnimationController.TextureCoordinatesTrack,
        GorgonSpriteAnimationController.UpperLeftPosition3DTrack,
        GorgonSpriteAnimationController.LowerLeftPosition3DTrack,
        GorgonSpriteAnimationController.UpperRightPosition3DTrack,
        GorgonSpriteAnimationController.LowerRightPosition3DTrack,
        GorgonSpriteAnimationController.Position3DTrack,
        GorgonSpriteAnimationController.DepthTrack
    };
    // The list of metadata items for each track.
    private IReadOnlyDictionary<int, KeyValueMetadata> _keyMetadata;

    /// <summary>
    /// The file name for the file that stores the settings.
    /// </summary>
    public readonly static string SettingsFilename = typeof(AnimationEditorPlugIn).FullName;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the default file extension used by files generated by this content plug in.
    /// </summary>
    /// <remarks>
    /// Plug in developers can override this to default the file name extension for their content when creating new content with <see cref="GetDefaultContentAsync(string, HashSet{string})"/>.
    /// </remarks>
    protected override GorgonFileExtension DefaultFileExtension => _defaultCodec.FileExtensions.Count > 0 ? _defaultCodec.FileExtensions[0] : default;

    /// <summary>Property to return the name of the plug in.</summary>
    string IContentPlugInMetadata.PlugInName => Name;

    /// <summary>Property to return the description of the plugin.</summary>
    string IContentPlugInMetadata.Description => Description;

    /// <summary>Property to return the ID of the small icon for this plug in.</summary>
    public Guid SmallIconID
    {
        get;
    }

    /// <summary>Property to return the ID of the new icon for this plug in.</summary>
    public Guid NewIconID
    {
        get;
    }

    /// <summary>Property to return whether or not the plugin is capable of creating content.</summary>
    public override bool CanCreateContent => true;

    /// <summary>Property to return the ID for the type of content produced by this plug in.</summary>
    public override string ContentTypeID => CommonEditorContentTypes.AnimationType;

    /// <summary>Property to return the friendly (i.e shown on the UI) name for the type of content.</summary>
    public string ContentType => Resources.GORANM_CONTENT_TYPE;
    #endregion

    #region Methods.
    /// <summary>
    /// Function to retrieve a list of metadata items for all keys in all available tracks.
    /// </summary>
    /// <param name="tracks">The list of tracks that are available for editing.</param>
    /// <param name="renderRegionSize">The width and height of the render region.</param>
    /// <param name="spriteAnchor">The anchor for the sprite to use as default.</param>
    /// <returns>A list of tracks available for editing.</returns>
    private IReadOnlyDictionary<int, KeyValueMetadata> GetKeyMetadata(IReadOnlyList<GorgonTrackRegistration> tracks)
    {
        var result = new Dictionary<int, KeyValueMetadata>();

        // For this, we have to use property names because the metadata needs to be tailored to the property on the sprite.
        foreach (GorgonTrackRegistration reg in tracks)
        {
            // Don't include forbidden tracks.
            if (_excludedTracks.Contains(reg))
            {
                continue;
            }

            var metadata = new KeyValueMetadata();

            switch (reg.TrackName)
            {
                case nameof(GorgonSprite.Angle):
                    metadata.ValueCount = 1;
                    metadata.SetData(0, reg.Description, 3, -36_000, 36_000);
                    break;
                case nameof(GorgonSprite.CornerOffsets.LowerRight):
                case nameof(GorgonSprite.CornerOffsets.UpperRight):
                case nameof(GorgonSprite.CornerOffsets.LowerLeft):
                case nameof(GorgonSprite.CornerOffsets.UpperLeft):
                case nameof(GorgonSprite.Position):
                    metadata.ValueCount = 2;
                    metadata.SetData(0, Resources.GORANM_TEXT_X, 0, short.MinValue, short.MaxValue);
                    metadata.SetData(1, Resources.GORANM_TEXT_Y, 0, short.MinValue, short.MaxValue);
                    break;
                case nameof(GorgonSprite.Anchor):
                    metadata.ValueCount = 2;
                    metadata.SetData(0, Resources.GORANM_TEXT_X, 6, -1024, 1024);
                    metadata.SetData(1, Resources.GORANM_TEXT_Y, 6, -1024, 1024);
                    break;
                case nameof(GorgonSprite.AbsoluteAnchor):
                    metadata.ValueCount = 2;
                    metadata.SetData(0, Resources.GORANM_TEXT_X, 0, short.MinValue, short.MaxValue);
                    metadata.SetData(1, Resources.GORANM_TEXT_Y, 0, short.MinValue, short.MaxValue);
                    break;
                case nameof(GorgonSprite.Size):
                    metadata.ValueCount = 2;
                    metadata.SetData(0, Resources.GORANM_TEXT_WIDTH, 0, 1, short.MaxValue);
                    metadata.SetData(1, Resources.GORANM_TEXT_HEIGHT, 0, 1, short.MaxValue);
                    break;
                case nameof(GorgonSprite.ScaledSize):
                    metadata.ValueCount = 2;
                    metadata.SetData(0, Resources.GORANM_TEXT_WIDTH, 0, 1, short.MaxValue);
                    metadata.SetData(1, Resources.GORANM_TEXT_HEIGHT, 0, 1, short.MaxValue);
                    break;
                case nameof(GorgonSprite.Scale):
                    metadata.ValueCount = 2;
                    metadata.SetData(0, Resources.GORANM_TEXT_WIDTH, 6, 0.000001f, short.MaxValue);
                    metadata.SetData(1, Resources.GORANM_TEXT_HEIGHT, 6, 0.000001f, short.MaxValue);
                    break;
                case nameof(GorgonSprite.Color):
                case nameof(GorgonSprite.Texture):
                case GorgonSpriteAnimationController.OpacityTrackName:
                case GorgonSpriteAnimationController.UpperLeftColorTrackName:
                case GorgonSpriteAnimationController.UpperRightColorTrackName:
                case GorgonSpriteAnimationController.LowerLeftColorTrackName:
                case GorgonSpriteAnimationController.LowerRightColorTrackName:                    
                    // Skip these tracks, we'll be using another editor for them.
                    continue;
                default:
                    HostContentServices.Log.Print($"WARNING: Cannot set metadata for the track '{reg.Description}'.  This track cannot be edited by the animation editor.", LoggingLevel.Intermediate);
                    continue;
            }

            result[reg.ID] = metadata;
        }

        return result;
    }

    /// <summary>
    /// Function to update the dependencies for the animation.
    /// </summary>
    /// <param name="fileStream">The stream for the file.</param>
    /// <param name="dependencyList">The list of dependency file paths.</param>
    private void UpdateDependencies(Stream fileStream, Dictionary<string, List<string>> dependencyList)
    {
        // Check for primary sprite dependency.
        if ((!dependencyList.TryGetValue(CommonEditorContentTypes.SpriteType, out List<string> spriteNames))
            || (spriteNames is null)
            || (spriteNames.Any(item => !ContentFileManager.FileExists(item))))
        {
            if (spriteNames is null)
            {
                dependencyList[CommonEditorContentTypes.SpriteType] = spriteNames = new List<string>();
            }

            if (spriteNames.Count == 0)
            {
                HostContentServices.Log.Print($"WARNING: The primary sprite was not found, it will have to be reassigned.", LoggingLevel.Intermediate);
            }
        }

        // Check for all textures. If they're all there then we don't need to update anything.
        if ((dependencyList.TryGetValue(CommonEditorContentTypes.ImageType, out List<string> textureNames))
            && (textureNames is not null)
            && (textureNames.All(item => ContentFileManager.FileExists(item))))
        {
            return;
        }

        if (textureNames is null)
        {
            dependencyList[CommonEditorContentTypes.ImageType] = textureNames = new List<string>();
        }

        // We couldn't find the texture in the dependency list (either did not exist, or there were no dependencies).
        IReadOnlyList<string> embeddedTextures = _defaultCodec.GetAssociatedTextureNames(fileStream);

        // Remove any duplicate names.
        foreach (string textureName in embeddedTextures)
        {
            if ((string.IsNullOrWhiteSpace(textureName)) || (!ContentFileManager.FileExists(textureName)))
            {
                HostContentServices.Log.Print($"WARNING: Texture '{textureName}' not found, the animation may not render correctly.", LoggingLevel.Intermediate);
                continue;
            }

            textureNames.Add(textureName);
        }
    }

    /// <summary>
    /// Function to update the metadata for a file that is missing metadata attributes.
    /// </summary>
    /// <param name="attributes">The attributes to update.</param>        
    private void UpdateFileMetadataAttributes(Dictionary<string, string> attributes)
    {
        if ((attributes.TryGetValue(CodecAttr, out string currentCodecType))
            && (!string.IsNullOrWhiteSpace(currentCodecType)))
        {
            attributes.Remove(CodecAttr);
        }

        if ((attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out string currentContentType))
            && (string.Equals(currentContentType, CommonEditorContentTypes.AnimationType, StringComparison.OrdinalIgnoreCase)))
        {
            attributes.Remove(CommonEditorConstants.ContentTypeAttr);
        }

        string codecType = _defaultCodec.GetType().FullName;
        if ((!attributes.TryGetValue(CodecAttr, out currentCodecType))
            || (!string.Equals(currentCodecType, codecType, StringComparison.OrdinalIgnoreCase)))
        {
            attributes[CodecAttr] = codecType;
        }

        if ((!attributes.TryGetValue(CommonEditorConstants.ContentTypeAttr, out currentContentType))
            || (!string.Equals(currentContentType, CommonEditorContentTypes.AnimationType, StringComparison.OrdinalIgnoreCase)))
        {
            attributes[CommonEditorConstants.ContentTypeAttr] = CommonEditorContentTypes.AnimationType;
        }
    }

    /// <summary>
    /// Function to load any background image if one exists.
    /// </summary>
    /// <param name="ioService">The animation file management service.</param>
    /// <param name="dependencies">The dependencies for the animation.</param>
    /// <returns>The texture and the file representing the texture.</returns>
    private async Task<(GorgonTexture2DView texture, IContentFile file)> LoadBackgroundTextureAsync(AnimationIOService ioService, IReadOnlyDictionary<string, List<string>> dependencies)
    {
        if (!dependencies.TryGetValue(BgImageDependencyName, out List<string> files))
        {
            return (null, null);
        }

        if ((files.Count == 0) || (string.IsNullOrWhiteSpace(files[0])))
        {
            return (null, null);
        }

        IContentFile file = ContentFileManager.GetFile(files[0]);

        if (file is null)
        {
            HostContentServices.Log.Print($"WARNING: The background image file '{files[0]}' was not found.", LoggingLevel.Intermediate);
            return (null, null);
        }

        if (!ioService.IsContentImage(file))
        {
            HostContentServices.Log.Print($"WARNING: The background image file '{files[0]}' is not valid image content.", LoggingLevel.Intermediate);
            return (null, null);
        }

        HostContentServices.Log.Print($"Background image file '{files[0]}' is loading...", LoggingLevel.Intermediate);
        (GorgonTexture2DView, IContentFile) result = (await _textureCache.GetTextureAsync(file), file);
        file.IsOpen = true;
        return result;
    }

    /// <summary>
    /// Function to correct any mismatch in textures assigned to the animation.
    /// </summary>
    /// <param name="animation">The animation to repair.</param>
    /// <param name="textures">The textures to apply.</param>
    private void FixTexturePathing(IGorgonAnimation animation, AnimationIOService.TextureDependencies textures)
    {
        if (textures.Textures.Count == 0)
        {
            return;
        }

        // Attempt to match textures by key frame index (should have a 1:1).
        foreach (IGorgonAnimationTrack<GorgonKeyTexture2D> track in animation.Texture2DTracks.Values)
        {
            for (int i = 0; i < track.KeyFrames.Count; ++i)
            {
                GorgonKeyTexture2D textureKey = track.KeyFrames[i];

                int textureIndex = i.Min(textures.Textures.Count - 1);

                if ((!string.IsNullOrWhiteSpace(textureKey.TextureName)) && (textureKey.Value is null))
                {
                    textureKey.Value = textures.Textures[textureIndex];                        
                }
            }
        }
    }

    /// <summary>
    /// Function to retrieve the keys from an animation and add them to a track view model.
    /// </summary>
    /// <param name="animation">The animation to evaluate.</param>
    /// <param name="tracks">The track view models.</param>
    /// <param name="fileManager">The file manager for the project.</param>
    /// <param name="keyCount">The number of keys per track.</param>
    private void GetKeys(IGorgonAnimation animation, IReadOnlyList<Track> tracks, IContentFileManager fileManager, int keyCount)
    {
        Track GetViewModel(string name) => tracks.FirstOrDefault(item => string.Equals(item.Name, name, StringComparison.OrdinalIgnoreCase));
        int GetKeyIndex<T>(T key) where T : IGorgonKeyFrame => (int)(key.Time * animation.Fps).Round();
        IKeyFrame[] keyFrames;

        foreach (KeyValuePair<string, IGorgonAnimationTrack<GorgonKeySingle>> track in animation.SingleTracks)
        {
            Track trackViewModel = GetViewModel(track.Value.Name);

            if (trackViewModel is null)
            {
                continue;
            }

            if (track.Value.KeyFrames.Count == 0)
            {
                trackViewModel.KeyFrames = Array.Empty<IKeyFrame>();
                continue;
            }

            keyFrames = ArrayPool<IKeyFrame>.Shared.Rent(keyCount);

            for (int i = 0; i < track.Value.KeyFrames.Count; i++)
            {                    
                GorgonKeySingle key = track.Value.KeyFrames[i];
                var keyFrame = new KeyFrame();
                keyFrame.Initialize(new KeyFrameParameters(key, HostContentServices));
                keyFrames[GetKeyIndex(key)] = keyFrame;
            }

            trackViewModel.KeyFrames = keyFrames;
            ArrayPool<IKeyFrame>.Shared.Return(keyFrames, true);
        }

        foreach (KeyValuePair<string, IGorgonAnimationTrack<GorgonKeyVector2>> track in animation.Vector2Tracks)
        {
            Track trackViewModel = GetViewModel(track.Value.Name);

            if (trackViewModel is null)
            {
                continue;
            }

            if (track.Value.KeyFrames.Count == 0)
            {
                trackViewModel.KeyFrames = Array.Empty<IKeyFrame>();
                continue;
            }

            keyFrames = ArrayPool<IKeyFrame>.Shared.Rent(keyCount);

            for (int i = 0; i < track.Value.KeyFrames.Count; i++)
            {
                GorgonKeyVector2 key = track.Value.KeyFrames[i];
                var keyFrame = new KeyFrame();
                keyFrame.Initialize(new KeyFrameParameters(key, HostContentServices));
                keyFrames[GetKeyIndex(key)] = keyFrame;
            }

            trackViewModel.KeyFrames = keyFrames;
            ArrayPool<IKeyFrame>.Shared.Return(keyFrames, true);
        }

        foreach (KeyValuePair<string, IGorgonAnimationTrack<GorgonKeyVector3>> track in animation.Vector3Tracks)
        {
            Track trackViewModel = GetViewModel(track.Value.Name);

            if (trackViewModel is null)
            {
                continue;
            }

            if (track.Value.KeyFrames.Count == 0)
            {
                trackViewModel.KeyFrames = Array.Empty<IKeyFrame>();
                continue;
            }

            keyFrames = ArrayPool<IKeyFrame>.Shared.Rent(keyCount);

            for (int i = 0; i < track.Value.KeyFrames.Count; i++)
            {
                GorgonKeyVector3 key = track.Value.KeyFrames[i];
                var keyFrame = new KeyFrame();
                keyFrame.Initialize(new KeyFrameParameters(key, HostContentServices));
                keyFrames[GetKeyIndex(key)] = keyFrame;
            }

            trackViewModel.KeyFrames = keyFrames;
            ArrayPool<IKeyFrame>.Shared.Return(keyFrames, true);
        }

        foreach (KeyValuePair<string, IGorgonAnimationTrack<GorgonKeyVector4>> track in animation.Vector4Tracks)
        {
            Track trackViewModel = GetViewModel(track.Value.Name);

            if (trackViewModel is null)
            {
                continue;
            }

            if (track.Value.KeyFrames.Count == 0)
            {
                trackViewModel.KeyFrames = Array.Empty<IKeyFrame>();
                continue;
            }

            keyFrames = ArrayPool<IKeyFrame>.Shared.Rent(keyCount);

            for (int i = 0; i < track.Value.KeyFrames.Count; i++)
            {
                GorgonKeyVector4 key = track.Value.KeyFrames[i];
                var keyFrame = new KeyFrame();
                keyFrame.Initialize(new KeyFrameParameters(key, HostContentServices));
                keyFrames[GetKeyIndex(key)] = keyFrame;
            }

            trackViewModel.KeyFrames = keyFrames;
            ArrayPool<IKeyFrame>.Shared.Return(keyFrames, true);
        }

        foreach (KeyValuePair<string, IGorgonAnimationTrack<GorgonKeyRectangle>> track in animation.RectangleTracks)
        {
            Track trackViewModel = GetViewModel(track.Value.Name);

            if (trackViewModel is null)
            {
                continue;
            }

            if (track.Value.KeyFrames.Count == 0)
            {
                trackViewModel.KeyFrames = Array.Empty<IKeyFrame>();
                continue;
            }

            keyFrames = ArrayPool<IKeyFrame>.Shared.Rent(keyCount);

            for (int i = 0; i < track.Value.KeyFrames.Count; i++)
            {
                GorgonKeyRectangle key = track.Value.KeyFrames[i];
                var keyFrame = new KeyFrame();
                keyFrame.Initialize(new KeyFrameParameters(key, HostContentServices));
                keyFrames[GetKeyIndex(key)] = keyFrame;
            }

            trackViewModel.KeyFrames = keyFrames;
            ArrayPool<IKeyFrame>.Shared.Return(keyFrames, true);
        }

        foreach (KeyValuePair<string, IGorgonAnimationTrack<GorgonKeyGorgonColor>> track in animation.ColorTracks)
        {
            Track trackViewModel = GetViewModel(track.Value.Name);

            if (trackViewModel is null)
            {
                continue;
            }

            if (track.Value.KeyFrames.Count == 0)
            {
                trackViewModel.KeyFrames = Array.Empty<IKeyFrame>();
                continue;
            }

            keyFrames = ArrayPool<IKeyFrame>.Shared.Rent(keyCount);

            for (int i = 0; i < track.Value.KeyFrames.Count; i++)
            {
                GorgonKeyGorgonColor key = track.Value.KeyFrames[i];
                var keyFrame = new KeyFrame();
                keyFrame.Initialize(new KeyFrameParameters(key, HostContentServices));
                keyFrames[GetKeyIndex(key)] = keyFrame;
            }

            trackViewModel.KeyFrames = keyFrames;
            ArrayPool<IKeyFrame>.Shared.Return(keyFrames, true);
        }

        foreach (KeyValuePair<string, IGorgonAnimationTrack<GorgonKeyTexture2D>> track in animation.Texture2DTracks)
        {
            Track trackViewModel = GetViewModel(track.Value.Name);

            if (trackViewModel is null)
            {
                continue;
            }

            if (track.Value.KeyFrames.Count == 0)
            {
                trackViewModel.KeyFrames = Array.Empty<IKeyFrame>();
                continue;
            }

            keyFrames = ArrayPool<IKeyFrame>.Shared.Rent(keyCount);

            for (int i = 0; i < track.Value.KeyFrames.Count; i++)
            {                    
                GorgonKeyTexture2D key = track.Value.KeyFrames[i];
                int index = GetKeyIndex(key);

                // If no texture name was returned, then do not assign a keyframe.
                if (string.IsNullOrWhiteSpace(key.TextureName))
                {
                    keyFrames[index] = null;
                    continue;
                }

                IContentFile textureFile = fileManager.GetFile(key.TextureName);

                if (textureFile is null)
                {
                    // If we cannot locate the file for the texture, then replace this with an empty key frame.
                    HostContentServices.Log.Print($"WARNING: The key at index {i}, for track '{track.Value.Name}' has a texture named '{key.TextureName}', but that texture file was not found in the file system. This will be replaced with an empty texture.", LoggingLevel.Intermediate);
                    keyFrames[index] = null;
                    continue;
                }

                var keyFrame = new KeyFrame();
                keyFrame.Initialize(new KeyFrameParameters(textureFile, key, HostContentServices));
                keyFrames[index] = keyFrame;
            }

            trackViewModel.KeyFrames = keyFrames;
            ArrayPool<IKeyFrame>.Shared.Return(keyFrames, true);
        }
    }

    /// <summary>
    /// Function to update the texture cache with texture values from a texture track.
    /// </summary>
    /// <param name="textureKeys">The list of texture keys.</param>
    /// <returns>A task for asynchronous operation.</returns>
    private async Task UpdateTextureCacheAsync(IReadOnlyList<IKeyFrame> textureKeys)
    {
        IEnumerable<(GorgonTexture2DView texture, int keyCount)> textureGrouping = (from textureKey in textureKeys
                                                                                    where textureKey?.TextureValue.Texture is not null
                                                                                    group textureKey by textureKey.TextureValue.Texture into g
                                                                                    // We exclude the first key because it's already loaded into the 
                                                                                    // cache when we loaded the dependencies.  
                                                                                    let textureCount = (texture: g.Key, keyCount: g.Count() - 1)
                                                                                    where textureCount.keyCount > 0
                                                                                    select textureCount);

        foreach ((GorgonTexture2DView texture, int keyCount) in textureGrouping)
        {
            for (int i = 0; i < keyCount; ++i)
            {
                await _textureCache.AddTextureAsync(texture);
            }
        }
    }

    /// <summary>
    /// Function to build the track view models.
    /// </summary>
    /// <typeparam name="T">The type of key frame for the keys in the track.</typeparam>
    /// <param name="trackList">The list of track view models to update.</param>
    /// <param name="excludedTracks">The list of tracks to be excluded.</param>
    /// <param name="controller">The controller for the animation.</param>
    /// <param name="keyCount">The number of keys in the track.</param>
    /// <param name="tracks">The list of tracks to evaluate.</param>
    private void BuildAnimationTrackViewModels<T>(List<Track> trackList, List<Track> excludedTracks, GorgonSpriteAnimationController controller, int keyCount, IReadOnlyDictionary<string, IGorgonAnimationTrack<T>> tracks)
        where T : IGorgonKeyFrame
    {
        // Retrieves a track registration based on the name of the track.
        GorgonTrackRegistration GetRegistration(string trackName) => controller.RegisteredTracks.FirstOrDefault(item => string.Equals(item.TrackName, trackName, StringComparison.OrdinalIgnoreCase));

        foreach (KeyValuePair<string, IGorgonAnimationTrack<T>> track in tracks)
        {
            // Do not include empty tracks.
            if (track.Value.KeyFrames.Count == 0)
            {
                continue;
            }

            GorgonTrackRegistration registration = GetRegistration(track.Key);

            if (registration is null)
            {
                continue;
            }

            _keyMetadata.TryGetValue(registration.ID, out KeyValueMetadata metadata);
            bool excluded = _excludedTracks.Contains(registration);
            track.Value.IsEnabled = !excluded;

            var newTrack = new Track();
            newTrack.Initialize(new TrackParameters(registration, 
                                                    track.Value.InterpolationMode, 
                                                    track.Value.SupportsInterpolation, 
                                                    keyCount, 
                                                    _undoService, 
                                                    HostContentServices)
            {
                KeyMetadata = metadata
            });

            if (!excluded)
            {
                trackList.Add(newTrack);
            }
            else
            {
                excludedTracks.Add(newTrack);
            }
        }
    }

    /// <summary>
    /// Function to validate the track types in the animation.
    /// </summary>
    /// <param name="file">The file containing the animation.</param>
    /// <param name="animation">The animation to validate.</param>
    /// <param name="controller">The controller for animating sprites.</param>
    private void ValidateAnimation(IContentFile file, IGorgonAnimation animation, GorgonSpriteAnimationController controller)
    {
        IEnumerable<(string name, AnimationTrackKeyType dataType)> trackRegNames = animation.SingleTracks.Select(item => (item.Value.Name, item.Value.KeyFrameDataType))
                                                                                                         .Concat(animation.Vector2Tracks.Select(item => (item.Value.Name, item.Value.KeyFrameDataType)))
                                                                                                         .Concat(animation.Vector3Tracks.Select(item => (item.Value.Name, item.Value.KeyFrameDataType)))
                                                                                                         .Concat(animation.Vector4Tracks.Select(item => (item.Value.Name, item.Value.KeyFrameDataType)))
                                                                                                         .Concat(animation.RectangleTracks.Select(item => (item.Value.Name, item.Value.KeyFrameDataType)))
                                                                                                         .Concat(animation.ColorTracks.Select(item => (item.Value.Name, item.Value.KeyFrameDataType)))
                                                                                                         .Concat(animation.Texture2DTracks.Select(item => (item.Value.Name, item.Value.KeyFrameDataType)));
        var unsupported = new List<string>();

        // Check for excluded tracks.
        foreach (GorgonTrackRegistration reg in _excludedTracks)
        {
            if (trackRegNames.Any(item => string.Equals(item.name, reg.TrackName, StringComparison.OrdinalIgnoreCase)))
            {
                if (_settings.WarnUnsupportedTracks)
                {
                    unsupported.Add($"{(string.IsNullOrEmpty(reg.Description) ? reg.TrackName : reg.Description)} [{reg.KeyType}]");
                }

                HostServices.Log.Print($"WARNING: Animation '{file.Path}' contains a '{reg.TrackName} [{reg.KeyType}]' track, which is not supported by the editor at this time.", LoggingLevel.Intermediate);
            }
        }

        // Check to see if the animation contains tracks that are not known to the controller, then log the warning and remove the track here.
        foreach ((string name, AnimationTrackKeyType dataType) in trackRegNames)
        {
            if (controller.RegisteredTracks.All(item => !string.Equals(item.TrackName, name, StringComparison.OrdinalIgnoreCase)))
            {
                if (_settings.WarnUnsupportedTracks)
                {
                    unsupported.Add($"{name} [{dataType}]");
                }

                HostServices.Log.Print($"WARNING: Animation '{file.Path}' contains a '{name} [{dataType}]' track, which is not supported for sprite animations and will be removed.", LoggingLevel.Intermediate);
            }
        }

        if (unsupported.Count > 0)
        {
            HostServices.MessageDisplay.ShowWarning(Resources.GORANM_WRN_UNSUPPORTED_TRACKS, details: string.Join("\n", unsupported));
        }
    }

    /// <summary>Function to retrieve the settings interface for this plug in.</summary>
    /// <returns>The settings interface view model.</returns>
    /// <remarks>
    ///   <para>
    /// Implementors who wish to supply customizable settings for their plug ins from the main "Settings" area in the application can override this method and return a new view model based on
    /// the base <see cref="ISettingsCategory"/> type. Returning <b>null</b> will mean that the plug in does not have settings that can be managed externally.
    /// </para>
    ///   <para>
    /// Plug ins must register the view associated with their settings panel via the <see cref="ViewFactory.Register{T}(Func{Control})"/> method when the plug in first loaded,
    /// or else the panel will not show in the main settings area.
    /// </para>
    /// </remarks>
    protected override ISettingsCategory OnGetSettings() => _pluginSettings;

    /// <summary>Function to retrieve the default content name, and data.</summary>
    /// <param name="generatedName">A default name generated by the application.</param>
    /// <param name="metadata">Custom metadata for the content.</param>
    /// <returns>The default content name along with the content data serialized as a byte array. If either the name or data are <b>null</b>, then the user cancelled..</returns>
    /// <remarks>
    ///   <para>
    /// Plug in authors may override this method so a custom UI can be presented when creating new content, or return a default set of data and a default name, or whatever they wish.
    /// </para>
    ///   <para>
    /// If an empty string (or whitespace) is returned for the name, then the <paramref name="generatedName" /> will be used.
    /// </para>
    /// </remarks>
    protected override Task<(string name, RecyclableMemoryStream data)> OnGetDefaultContentAsync(string generatedName, ProjectItemMetadata metadata)
    {
        string currentDirectory = ContentFileManager.CurrentDirectory.FormatDirectory('/');

        // Creates an animation object and converts it to a byte array.
        RecyclableMemoryStream CreateAnimation(string name, float length, float fps, IContentFile primarySpriteFile, IContentFile bgTextureFile)
        {
            var builder = new GorgonAnimationBuilder();                

            metadata.Attributes[CodecAttr] = _defaultCodec.GetType().FullName;
            metadata.Attributes[CommonEditorConstants.IsNewAttr] = bool.TrueString;

            if ((bgTextureFile is not null) && (ContentFileManager.FileExists(bgTextureFile.Path)))
            {
                HostContentServices.Log.Print($"Assigning '{bgTextureFile.Path}' as background image for animation.", LoggingLevel.Verbose);                    
                metadata.DependsOn[BgImageDependencyName] = new List<string>
                {
                    bgTextureFile.Path
                };
            }

            if ((primarySpriteFile is not null) && (ContentFileManager.FileExists(primarySpriteFile.Path)))
            {
                HostContentServices.Log.Print($"Loading primary sprite '{primarySpriteFile.Path}'...", LoggingLevel.Verbose);
                using Stream spriteStream = ContentFileManager.OpenStream(primarySpriteFile.Path, FileMode.Open);
                string texturePath = _defaultSpriteCodec.GetAssociatedTextureName(spriteStream);
                GorgonSprite sprite = _defaultSpriteCodec.FromStream(spriteStream, HostContentServices.GraphicsContext.Renderer2D.EmptyWhiteTexture);

                if ((!string.IsNullOrWhiteSpace(texturePath)) && (ContentFileManager.FileExists(texturePath)))
                {
                    if (_settings.AddTextureTrackForPrimarySprite)
                    {                            
                        builder.Edit2DTexture(GorgonSpriteAnimationController.TextureTrack.TrackName)
                               .SetKey(new GorgonKeyTexture2D(0, texturePath, sprite.TextureRegion, sprite.TextureArrayIndex))
                               .EndEdit();
                        metadata.DependsOn[CommonEditorContentTypes.ImageType] = new List<string> { texturePath };
                    }

                    metadata.DependsOn[CommonEditorContentTypes.SpriteType] = new List<string> { primarySpriteFile.Path };
                }
                else
                {
                    HostContentServices.Log.Print($"WARNING: Primary sprite '{primarySpriteFile.Path}' was found, but its associated texture was not. Skipping...", LoggingLevel.Intermediate);
                }
            }

            metadata.Attributes[CodecAttr] = _defaultCodec.GetType().FullName;

            IGorgonAnimation animation = builder.Build(currentDirectory + name.FormatFileName(), fps, length);

            var stream = CommonEditorResources.MemoryStreamManager.GetStream() as RecyclableMemoryStream;
            _defaultCodec.Save(animation, stream);
            return stream;
        }

        (string newName, float animLength, float animFps, IContentFile primarySprite, IContentFile bgTexture) = _newAnimation.GetNewAnimationName(currentDirectory, generatedName, null, null);

        return !string.IsNullOrWhiteSpace(newName)
            ? Task.FromResult<(string, RecyclableMemoryStream)>((newName, CreateAnimation(newName, animLength, animFps, primarySprite, bgTexture)))
            : Task.FromResult<(string, RecyclableMemoryStream)>((null, null));
    }

    /// <summary>Function to create an empty key frame view model for texture data.</summary>
    /// <param name="time">The time for the key frame.</param>
    /// <param name="dataType">The type of data to store in the key frame.</param>
    /// <returns>A new key frame view model with no data.</returns>
    IKeyFrame IViewModelFactory.CreateKeyFrame(float time, AnimationTrackKeyType dataType)
    {
        var result = new KeyFrame();
        result.Initialize(new KeyFrameParameters(time, dataType, Vector4.Zero, HostContentServices));
        return result;
    }

    /// <summary>Function to create a key frame view model for texture data.</summary>
    /// <param name="time">The time for the key frame.</param>
    /// <param name="textureValue">The texture data to assign.</param>
    /// <returns>A new key frame view model with the specified data.</returns>
    IKeyFrame IViewModelFactory.CreateKeyFrame(float time, ref TextureValue textureValue)
    {
        var result = new KeyFrame();
        result.Initialize(new KeyFrameParameters(time, ref textureValue, HostContentServices));
        return result;
    }

    /// <summary>
    /// Function to create a key frame view model for floating point data.
    /// </summary>
    /// <param name="time">The time for the key frame.</param>
    /// <param name="dataType">The type of data in the key frame.</param>
    /// <param name="floatValue">The floating point data to assign.</param>
    /// <returns>A new key frame view model with the specified data.</returns>
    IKeyFrame IViewModelFactory.CreateKeyFrame(float time, AnimationTrackKeyType dataType, Vector4 floatValue)
    {
        var result = new KeyFrame();
        result.Initialize(new KeyFrameParameters(time, dataType, floatValue, HostContentServices));
        return result;
    }

    /// <summary>
    /// Function to create a new animation track.
    /// </summary>
    /// <param name="trackRegistration">The registration for the track.</param>
    /// <param name="keyCount">The number of keys in the track.</param>
    /// <param name="keyFrames">[Optional] Default key frames to apply to the track.</param>
    /// <returns>The track view model.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="trackRegistration"/> parameter is <b>null</b>.</exception>
    ITrack IViewModelFactory.CreateTrack(GorgonTrackRegistration trackRegistration, int keyCount, IReadOnlyList<IKeyFrame> keyFrames)
    {
        if (trackRegistration is null)
        {
            throw new ArgumentNullException(nameof(trackRegistration));
        }

        TrackInterpolationMode trackInterpolationMode = TrackInterpolationMode.None;

        if ((trackRegistration.SupportedInterpolation & TrackInterpolationMode.Linear) == TrackInterpolationMode.Linear)
        {
            trackInterpolationMode = TrackInterpolationMode.Linear;
        } else if ((trackRegistration.SupportedInterpolation & TrackInterpolationMode.Spline) == TrackInterpolationMode.Spline)
        {
            trackInterpolationMode = TrackInterpolationMode.Spline;
        }

        _keyMetadata.TryGetValue(trackRegistration.ID, out KeyValueMetadata metadata);

        var track = new Track();
        track.Initialize(new TrackParameters(trackRegistration, 
                                             trackInterpolationMode, 
                                             trackRegistration.SupportedInterpolation, 
                                             keyCount, 
                                             _undoService, 
                                             HostContentServices)
        {
            KeyMetadata = metadata
        });

        if (keyFrames is not null)
        {
            track.KeyFrames = keyFrames;
        }

        return track;
    }

    /// <summary>
    /// Function to create a new key frame based on the texture data in a sprite.
    /// </summary>
    /// <param name="time">The time, in seconds, for the key frame.</param>
    /// <param name="spriteFile">The file containing the sprite data to load.</param>
    /// <returns>A new <see cref="IKeyFrame"/> object.</returns>
    async Task<IKeyFrame> IViewModelFactory.CreateTextureKeyFrameAsync(float time, IContentFile spriteFile)
    {
        var factory = (IViewModelFactory)this;
        TextureValue textureValue;

        if (!_ioService.IsContentSprite(spriteFile))
        {
            throw new ArgumentException(string.Format(Resources.GORANM_ERR_NOT_SPRITE, spriteFile.Path), nameof(spriteFile));
        }

        HostContentServices.Log.Print($"Loading sprite data from '{spriteFile.Path}'...", LoggingLevel.Verbose);

        (GorgonSprite textureSprite, IContentFile textureFile) = await _ioService.LoadSpriteAsync(spriteFile);
        spriteFile.IsOpen = false;

        if (textureFile is not null)
        {
            textureFile.IsOpen = true;
        }

        if (textureSprite is null)
        {
            HostContentServices.Log.Print($"WARNING: The sprite in file '{spriteFile.Path}' could not be loaded. No texture will be assigned to the key frame. This may cause undesirable results.", LoggingLevel.Intermediate);
            return default;
        }

        textureValue = new TextureValue(textureSprite.Texture, textureFile, textureSprite.TextureArrayIndex, textureSprite.TextureRegion);
        HostContentServices.Log.Print($"Texture data created from sprite '{spriteFile.Path}'.", LoggingLevel.Verbose);
        return factory.CreateKeyFrame(time, ref textureValue);
    }

    /// <summary>
    /// Function to create the default texture track for an animation.
    /// </summary>
    /// <param name="primarySprite">The primary sprite to retrieve the texture from.</param>
    /// <param name="frameCount">The number of frames in the track.</param>
    /// <returns>The new texture track, or <b>null</b> if no primary sprite is available.</returns>
    async Task<ITrack> IViewModelFactory.CreateDefaultTextureTrackAsync((GorgonSprite sprite, IContentFile spriteFile, IContentFile textureFile) primarySprite, int frameCount)
    {
        if (primarySprite.sprite?.Texture is null)
        {
            return null;
        }

        var factory = (IViewModelFactory)this;
        IKeyFrame[] keyFrames = null;

        try
        {
            keyFrames = ArrayPool<IKeyFrame>.Shared.Rent(frameCount);

            var textureValue = new TextureValue(primarySprite.sprite.Texture, primarySprite.textureFile, primarySprite.sprite.TextureArrayIndex, primarySprite.sprite.TextureRegion);
            keyFrames[0] = factory.CreateKeyFrame(0, ref textureValue);

            await _textureCache.AddTextureAsync(primarySprite.sprite.Texture);

            return factory.CreateTrack(GorgonSpriteAnimationController.TextureTrack, keyFrames.Length, keyFrames);
        }
        finally
        {
            if (keyFrames is not null)
            {
                ArrayPool<IKeyFrame>.Shared.Return(keyFrames, true);
            }
        }

    }

    /// <summary>Function to determine if the content plugin can open the specified file.</summary>
    /// <param name="filePath">The path to the file to evaluate.</param>
    /// <returns>
    ///   <b>true</b> if the plugin can open the file, or <b>false</b> if not.</returns>
    public bool CanOpenContent(string filePath)
    {
        if (filePath is null)
        {
            throw new ArgumentNullException(nameof(filePath));
        }

        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentEmptyException(nameof(filePath));
        }

        IContentFile file = ContentFileManager.GetFile(filePath);

        Debug.Assert(file is not null, $"File '{filePath}' doesn't exist, but it should!");

        using Stream stream = ContentFileManager.OpenStream(filePath, FileMode.Open);
        if (!_defaultCodec.IsReadable(stream))
        {
            return false;
        }

        UpdateFileMetadataAttributes(file.Metadata.Attributes);
        UpdateDependencies(stream, file.Metadata.DependsOn);
        return true;
    }

    /// <summary>Function to retrieve the icon used for new content creation.</summary>
    /// <returns>An image for the icon.</returns>
    /// <remarks>This method is never called when <see cref="CanCreateContent"/> is <b>false</b>.</remarks>
    public System.Drawing.Image GetNewIcon() => Resources.animation_editor_24x24;

    /// <summary>Function to retrieve the small icon for the content plug in.</summary>
    /// <returns>An image for the small icon.</returns>
    public System.Drawing.Image GetSmallIcon() => Resources.animation_editor_16x16;

    /// <summary>get thumbnail as an asynchronous operation.</summary>
    /// <param name="contentFile">The content file used to retrieve the data to build the thumbnail with.</param>
    /// <param name="filePath">The path to the thumbnail file to write.</param>
    /// <param name="cancelToken">The token used to cancel the thumbnail generation.</param>
    /// <returns>A <see cref="IGorgonImage"/> containing the thumbnail image data.</returns>
    public Task<IGorgonImage> GetThumbnailAsync(IContentFile contentFile, string filePath, CancellationToken cancelToken) =>
        Task.Run(() =>
                {
                    using MemoryStream imageStream = CommonEditorResources.MemoryStreamManager.GetStream(Resources.anim_thumbnail_256x256);
                    return _defaultImageCodec.FromStream(imageStream);
                });

    /// <summary>Function to open a content object from this plugin.</summary>
    /// <param name="file">The file that contains the content.</param>
    /// <param name="fileManager">The file manager used to access other content files.</param>
    /// <param name="scratchArea">The file system for the scratch area used to write transitory information.</param>
    /// <param name="undoService">The undo service for the plug in.</param>
    /// <returns>A new <see cref="IEditorContent"/> object.</returns>
    /// <remarks>
    /// The <paramref name="scratchArea" /> parameter is the file system where temporary files to store transitory information for the plug in is stored. This file system is destroyed when the
    /// application or plug in is shut down, and is not stored with the project.
    /// </remarks>
    protected async override Task<IEditorContent> OnOpenContentAsync(IContentFile file, IContentFileManager fileManager, IGorgonFileSystemWriter<Stream> scratchArea, IUndoService undoService)
    {
        var content = new AnimationContent();
        IGorgonAnimation animation;
        Stream stream = null;
        AnimationIOService.TextureDependencies textures = null;
        AnimationIOService.PrimarySpriteDependency primarySprite = null;
        var controller = new GorgonSpriteAnimationController();
        GorgonTexture2DView bgTexture = null;
        IContentFile bgTextureFile = null;

        try
        {
            _undoService = undoService;
            _textureCache ??= new TextureCache(HostContentServices.GraphicsContext.Graphics, ContentFileManager, scratchArea, _defaultImageCodec, HostContentServices.Log);

            _ioService = new AnimationIOService(ContentFileManager,
                                                _textureCache,
                                                _defaultCodec,                                                    
                                                _defaultSpriteCodec, 
                                                HostContentServices.Log);
            var keyProcessor = new KeyProcessorService(_textureCache, HostContentServices.Log);
            var contentServices = new ContentServices(_ioService, _textureCache, undoService, keyProcessor, _newAnimation, this);

            (textures, primarySprite) = await _ioService.LoadDependenciesAsync(file);
            (bgTexture, bgTextureFile) = await LoadBackgroundTextureAsync(_ioService, file.Metadata.DependsOn);

            DX.Size2 size = bgTexture is not null ? new DX.Size2(bgTexture.Width, bgTexture.Height) : _settings.DefaultResolution;

            if (primarySprite?.PrimarySprite is not null)
            {
                primarySprite.PrimarySprite.Position = new Vector2((int)(size.Width * 0.5f), (int)(size.Height * 0.5f));
            }

            // Load the sprite now. 
            stream = ContentFileManager.OpenStream(file.Path, FileMode.Open);
            animation = _defaultCodec.FromStream(stream, name: file.Path);

            // Validate the animation tracks to ensure they're not using tracks we cannot support.
            ValidateAnimation(file, animation, controller);

            FixTexturePathing(animation, textures);

            // Convert tracks and keys into view models.
            int maxKeyCount = (int)(animation.Length * animation.Fps).Round();

            _keyMetadata = GetKeyMetadata(controller.RegisteredTracks);

            var tracks = new List<Track>();
            var excluded = new List<Track>();
            BuildAnimationTrackViewModels(tracks, excluded, controller, maxKeyCount, animation.SingleTracks);
            BuildAnimationTrackViewModels(tracks, excluded, controller, maxKeyCount, animation.Vector2Tracks);
            BuildAnimationTrackViewModels(tracks, excluded, controller, maxKeyCount, animation.Vector3Tracks);
            BuildAnimationTrackViewModels(tracks, excluded, controller, maxKeyCount, animation.Vector4Tracks);
            BuildAnimationTrackViewModels(tracks, excluded, controller, maxKeyCount, animation.RectangleTracks);
            BuildAnimationTrackViewModels(tracks, excluded, controller, maxKeyCount, animation.ColorTracks);
            BuildAnimationTrackViewModels(tracks, excluded, controller, maxKeyCount, animation.Texture2DTracks);

            GetKeys(animation, tracks, fileManager, maxKeyCount);

            IEnumerable<ITrack> textureTracks = tracks.Where(item => (item.ID == GorgonSpriteAnimationController.TextureTrack.ID) 
                                                          && (item.KeyFrames.Any(item2 => item2 is not null)));
            foreach (ITrack track in textureTracks)
            {
                await UpdateTextureCacheAsync(track.KeyFrames);
            }

            // Filter any tracks that we can't really represent in the editor.
            var availableTracks = new ObservableCollection<GorgonTrackRegistration>(controller.RegisteredTracks.Where(item => !tracks.Any(trackItem => item.ID == trackItem.ID)
                                                                                                                // Skip superfluous tracks and depth related tracks, we have no 
                                                                                                                // good way to represent them at this time.
                                                                                                                && (!_excludedTracks.Contains(item))));

            // Set up sub panel view models.
            var newTrack = new AddTrack();
            newTrack.Initialize(new AddTrackParameters(availableTracks, HostContentServices));

            var animProperties = new AnimProperties();
            animProperties.Initialize(new PropertiesParameters(animation, HostContentServices));                

            var floatKeyEditor = new KeyValueEditor();
            floatKeyEditor.Initialize(new HostedPanelViewModelParameters(HostContentServices));

            var colorKeyEditor = new ColorValueEditor();
            colorKeyEditor.Initialize(new HostedPanelViewModelParameters(HostContentServices));

            var keyEditor = new KeyEditorContext();
            keyEditor.Initialize(new KeyEditorContextParameters(content, fileManager, floatKeyEditor, colorKeyEditor, controller, contentServices, HostContentServices));

            content.Initialize(new AnimationContentParameters(file,
                                                              animation,
                                                              new ObservableCollection<ITrack>(tracks),
                                                              excluded,
                                                              newTrack,
                                                              animProperties,
                                                              keyEditor,
                                                              controller,
                                                              _pluginSettings,
                                                              fileManager,                                                                  
                                                              contentServices,
                                                              HostContentServices)
                                {
                                    PrimarySprite = primarySprite,
                                    BackgroundTexture = bgTexture,
                                    BackgroundTextureFile = bgTextureFile
                                });

            return content;
        }
        catch
        {
            _textureCache?.Dispose();
            throw;
        }
        finally
        {
            stream?.Dispose();
        }
    }

    /// <summary>Function to register plug in specific search keywords with the system search.</summary>
    /// <typeparam name="T">The type of object being searched, must implement <see cref="IGorgonNamedObject"/>.</typeparam>
    /// <param name="searchService">The search service to use for registration.</param>
    protected override void OnRegisterSearchKeywords<T>(ISearchService<T> searchService)
    {
        // Not needed yet.
    }

    /// <summary>Function to provide clean up for the plugin.</summary>
    protected override void OnShutdown()
    {
        if (_settings is not null)
        {
            // Persist any settings.
            HostContentServices.ContentPlugInService.WriteContentSettings(SettingsFilename, _settings);
        }

        _textureCache?.Dispose();
        _textureCache = null;
        _undoService?.ClearStack();

        ViewFactory.Unregister<IAnimationContent>();
        ViewFactory.Unregister<ISettings>();
        base.OnShutdown();
    }

    /// <summary>Function to allow custom plug ins to implement custom actions when a project is created/opened.</summary>
    protected override void OnProjectOpened()
    {
        base.OnProjectOpened();
        _newAnimation = new NewAnimationService(ContentFileManager, _defaultSpriteCodec, _defaultImageCodec);
    }

    /// <summary>Function to provide initialization for the plugin.</summary>
    /// <remarks>This method is only called when the plugin is loaded at startup.</remarks>
    protected override void OnInitialize()
    {
        base.OnInitialize();

        _defaultCodec = new GorgonV31AnimationBinaryCodec(HostContentServices.GraphicsContext.Renderer2D);
        _defaultImageCodec = new GorgonCodecDds();
        _defaultSpriteCodec = new GorgonV3SpriteBinaryCodec(HostContentServices.GraphicsContext.Renderer2D);

        AnimationEditorSettings settings = HostContentServices.ContentPlugInService.ReadContentSettings<AnimationEditorSettings>(SettingsFilename);
        if (settings is not null)
        {
            _settings = settings;
        }

        _pluginSettings = new Settings();
        _pluginSettings.Initialize(new SettingsParameters(_settings, HostContentServices));

        ViewFactory.Register<ISettings>(() => new AnimationSettingsPanel());
        ViewFactory.Register<IAnimationContent>(() => new AnimationEditorView(settings));            
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>Initializes a new instance of the AnimationEditorPlugIn class.</summary>
    public AnimationEditorPlugIn()
        : base(Resources.GORANM_DESC)
    {
        SmallIconID = Guid.NewGuid();
        NewIconID = Guid.NewGuid();
    }
    #endregion
}
