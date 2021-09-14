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
// Created: July 1, 2020 12:34:36 AM
// 
#endregion

using System;
using System.Buffers;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Animation;
using Gorgon.Collections;
using Gorgon.Editor.AnimationEditor.Properties;
using Gorgon.Editor.Content;
using Gorgon.Editor.Services;
using Gorgon.Editor.UI;
using Gorgon.Math;

namespace Gorgon.Editor.AnimationEditor
{
    /// <summary>
    /// The view model for the key editor context.
    /// </summary>
    internal class KeyEditorContext
        : EditorContext<KeyEditorContextParameters>, IKeyEditorContext
    {
        #region Classes.
        /// <summary>
        /// Arguments for undoing or redoing a key assignment.
        /// </summary>
        private class SetKeyUndoRedoArgs
        {
            /// <summary>
            /// The list of keys that were selected at the time of the operation.
            /// </summary>
            public List<TrackKeySelection.KeySelection> SelectedKeys;
        }

        /// <summary>
        /// Arguments for undoing or redoing removal of keyframes.
        /// </summary>
        private class RemoveKeyUndoRedoArgs
        {
            /// <summary>
            /// The list of keys that were selected at the time of the operation.
            /// </summary>
            public List<TrackKeySelection> SelectedKeys;
        }

        /// <summary>
        /// Arguments for undoing copy/move of keyframes.
        /// </summary>
        private class CopyMoveUndoArgs
        {
            /// <summary>
            /// The list of keyframes replaced.
            /// </summary>
            public List<(ITrack track, Stack<TrackKeySelection.KeySelection> keyFrames)> Keys;
        }

        /// <summary>
        /// Arguments for redoing copy/move of keyframes.
        /// </summary>
        private class CopyMoveRedoArgs
        {
            /// <summary>
            /// The list of keyframes replaced.
            /// </summary>
            public IReadOnlyList<TrackKeySelection> Keys;
            /// <summary>
            /// The index of the key to start copying into.
            /// </summary>
            public int DestKeyIndex;
            /// <summary>
            /// Flag to indicate that the keyframes are to be moved.
            /// </summary>
            public bool Move;
        }

        #endregion

        #region Constants.
        /// <summary>
        /// The name of the context.
        /// </summary>
        public const string ContextName = "ContextKeyEditor";
        #endregion

        #region Variables.
        // The project file manager.
        private IContentFileManager _fileManager;
        // The list of sprites selected for loading texture keys.
        private readonly List<IContentFile> _selectedSprites = new();
        // Services from the plug in.
        private ContentServices _contentServices;
        // The current editor for floating point values.
        private IKeyValueEditor _currentEditor;
        // The sprite animation contorller.
        private GorgonSpriteAnimationController _controller;
        // The animation content.
        private IAnimationContent _content;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the key value editor.
        /// </summary>
        public IKeyValueEditor FloatKeysEditor
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the key value editor for color values.
        /// </summary>
        public IColorValueEditor ColorKeysEditor
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the currently active editor.
        /// </summary>
        public IKeyValueEditor CurrentEditor
        {
            get => _currentEditor;
            private set
            {
                if (value == _currentEditor)
                {
                    return;
                }

                OnPropertyChanging();
                _currentEditor = value;
                OnPropertyChanged();
            }
        }

        /// <summary>Property to return the context name.</summary>
        /// <remarks>This value is used as a unique ID for the context.</remarks>
        public override string Name => ContextName;

        /// <summary>
        /// Property to return the command to undo an operation.
        /// </summary>
        public IEditorCommand<object> UndoCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to redo an operation.
        /// </summary>
        public IEditorCommand<object> RedoCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to set the key value.
        /// </summary>
        public IEditorAsyncCommand<object> SetKeyCommand
        {
            get;
        }

        /// <summary>Property to return the command used to load a sprite from the project (for use in texture animations).</summary>
        public IEditorAsyncCommand<IReadOnlyList<string>> LoadSpriteCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to remove selected keyframes.
        /// </summary>
        public IEditorCommand<object> RemoveKeyCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command to clear all key frames.
        /// </summary>
        public IEditorCommand<object> ClearKeysCommand
        {
            get;
        }

        /// <summary>Property to return whether the clipboard has data or not.</summary>
        public bool HasData
        {
            get;
        }

        /// <summary>Property to return the command that returns the type of data present on the clipboard (if any).</summary>
        public IEditorCommand<GetClipboardDataTypeArgs> GetClipboardDataTypeCommand
        {
            get;
        }

        /// <summary>Property to return the command used to clear the clipboard.</summary>
        public IEditorCommand<object> ClearCommand
        {
            get;
        }

        /// <summary>Property to return the command used to copy data into the clipboard.</summary>
        public IEditorCommand<object> CopyDataCommand
        {
            get;
        }

        /// <summary>Property to return the command used to paste data from the clipboard.</summary>
        public IEditorAsyncCommand<object> PasteDataCommand
        {
            get;
        }

        /// <summary>
        /// Property to return the command used to copy/move key frames.
        /// </summary>
        public IEditorAsyncCommand<KeyFrameCopyMoveData> CopyMoveFramesCommand
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>Handles the PropertyChanged event of the Content control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void Content_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IAnimationContent.Selected):
                    ShowEditorPanel();
                    break;
            }
        }


        /// <summary>
        /// Function to remove the selected key frames.
        /// </summary>
        /// <param name="selectedKeyFrames">The list of selected key frames and tracks.</param>
        private void RemoveKeyFrames(IReadOnlyList<TrackKeySelection> selectedKeyFrames)
        {
            RemoveKeyUndoRedoArgs undoArgs;
            RemoveKeyUndoRedoArgs redoArgs;

            IKeyFrame[] keyFrames = null;

            // Restore the keyframes.
            async Task RestoreAsync(IReadOnlyList<TrackKeySelection> selected)
            {
                try
                {
                    var commandArgs = new SetKeyFramesArgs();

                    ShowWaitPanel(Resources.GORANM_TEXT_PLEASE_WAIT);

                    keyFrames = ArrayPool<IKeyFrame>.Shared.Rent(_content.MaxKeyCount);

                    foreach (TrackKeySelection track in selected)
                    {
                        Array.Clear(keyFrames, 0, keyFrames.Length);

                        for (int i = 0; i < track.Track.KeyFrames.Count; ++i)
                        {
                            IKeyFrame keyFrame = track.Track.KeyFrames[i];
                            TrackKeySelection.KeySelection selectedKeyFrame = track.SelectedKeys.FirstOrDefault(item => item.KeyIndex == i);

                            if (selectedKeyFrame is not null)
                            {
                                keyFrame = selectedKeyFrame.KeyFrame;
                                await _contentServices.KeyProcessor.RestoreTextureAsync(keyFrame);
                            }

                            keyFrames[i] = keyFrame;
                        }

                        _contentServices.KeyProcessor.AssignKeyFrames(track.Track, keyFrames, _content.MaxKeyCount);
                    }

                    HostServices.ClipboardService.Clear();
                    NotifyPropertyChanged(nameof(PasteDataCommand));
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_REMOVE_KEYFRAMES);
                }
                finally
                {
                    if (keyFrames is not null)
                    {
                        ArrayPool<IKeyFrame>.Shared.Return(keyFrames, true);
                    }
                    HideWaitPanel();
                }
            }

            // Remove the key frames.
            bool Remove(IReadOnlyList<TrackKeySelection> selected)
            {
                try
                {
                    var commandArgs = new SetKeyFramesArgs();

                    HostServices.BusyService.SetBusy();

                    keyFrames = ArrayPool<IKeyFrame>.Shared.Rent(_content.MaxKeyCount);

                    foreach (TrackKeySelection track in selected)
                    {
                        Array.Clear(keyFrames, 0, keyFrames.Length);

                        for (int i = 0; i < track.Track.KeyFrames.Count; ++i)
                        {
                            IKeyFrame keyFrame = track.Track.KeyFrames[i];

                            if (track.SelectedKeys.Any(item => item.KeyFrame == keyFrame))
                            {
                                if ((keyFrame?.TextureValue.Texture is not null) && (keyFrame?.TextureValue.TextureFile is not null))
                                {
                                    // Unload this texture.
                                    _contentServices.KeyProcessor.UnloadTextureKeyframe(keyFrame);
                                }
                                continue;
                            }

                            keyFrames[i] = keyFrame;
                        }

                        _contentServices.KeyProcessor.AssignKeyFrames(track.Track, keyFrames, _content.MaxKeyCount);
                    }

                    HostServices.ClipboardService.Clear();
                    NotifyPropertyChanged(nameof(PasteDataCommand));
                    return true;
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_REMOVE_KEYFRAMES);
                    return false;
                }
                finally
                {
                    if (keyFrames is not null)
                    {
                        ArrayPool<IKeyFrame>.Shared.Return(keyFrames, true);
                    }
                    HostServices.BusyService.SetIdle();
                }
            }

            Task RedoAsync(RemoveKeyUndoRedoArgs args, CancellationToken cancel) => Task.FromResult(Remove(args.SelectedKeys));
            Task UndoAsync(RemoveKeyUndoRedoArgs args, CancellationToken cancel) => RestoreAsync(args.SelectedKeys);

            undoArgs = new RemoveKeyUndoRedoArgs
            {
                SelectedKeys = new List<TrackKeySelection>(selectedKeyFrames.Where(item => item is not null)
                                                                            .Select(item => new TrackKeySelection(item.TrackIndex, item.Track, item.SelectedKeys.Take(_content.MaxKeyCount).ToArray())))
            };

            redoArgs = new RemoveKeyUndoRedoArgs
            {
                SelectedKeys = new List<TrackKeySelection>(selectedKeyFrames.Where(item => item is not null)
                                                                            .Select(item => new TrackKeySelection(item.TrackIndex, item.Track, item.SelectedKeys.Take(_content.MaxKeyCount).ToArray())))
            };

            if (Remove(redoArgs.SelectedKeys))
            {
                _contentServices.UndoService.Record(Resources.GORANM_DESC_UNDO_REMOVE_KEYFRAMES, UndoAsync, RedoAsync, undoArgs, redoArgs);
                NotifyPropertyChanged(nameof(UndoCommand));
            }
        }

        /// <summary>
        /// Function to activate the editor panel based on the track selected.
        /// </summary>
        private void ShowEditorPanel()
        {
            HostServices.BusyService.SetBusy();
            try
            {
                if ((_content.Selected.Count != 1) || (_content.Selected[0].SelectedKeys.Count != 1))
                {
                    CurrentEditor = null;
                    return;
                }

                TrackKeySelection selectedTrack = _content.Selected[0];
                TrackKeySelection.KeySelection selectedKey = _content.Selected[0].SelectedKeys[0];
                Vector4? value = selectedKey.KeyFrame?.FloatValue ?? _contentServices.KeyProcessor.GetTrackFloatValues(selectedTrack.Track, selectedKey.TimeIndex, 
                                                                                                                          _controller.CurrentAnimation, _content.WorkingSprite);

                // Reset the track/key value.
                if (CurrentEditor is not null)
                {
                    CurrentEditor.WorkingSprite = null;
                    CurrentEditor.Track = null;
                }

                switch (selectedTrack.Track.KeyType)
                {
                    case AnimationTrackKeyType.Color:
                    case AnimationTrackKeyType.Single when selectedTrack.Track.SpriteProperty == TrackSpriteProperty.Opacity:
                        CurrentEditor = ColorKeysEditor;
                        ColorKeysEditor.Track = selectedTrack;
                        ColorKeysEditor.WorkingSprite = _content.WorkingSprite;
                        if (value is not null)
                        {
                            ColorKeysEditor.Value = value.Value;
                        }                        
                        break;
                    case AnimationTrackKeyType.Single:
                    case AnimationTrackKeyType.Vector2:
                        CurrentEditor = FloatKeysEditor;
                        FloatKeysEditor.Track = selectedTrack;
                        FloatKeysEditor.WorkingSprite = _content.WorkingSprite;
                        if (value is not null)
                        {
                            FloatKeysEditor.Value = value.Value;
                        }                        
                        break;
                    case AnimationTrackKeyType.Texture2D:
                        CurrentEditor = null;
                        break;
                }
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_CANNOT_DISPLAY_EDITOR);
            }
            finally
            {
                HostServices.BusyService.SetIdle();
            }
        }

        /// <summary>
        /// Function to assign texture data to a slot in a track.
        /// </summary>
        /// <param name="track">The track containing the key frame.</param>
        /// <param name="keyIndex">The index of the key.</param>
        /// <param name="time">The time, in seconds, for the key frame.</param>
        /// <param name="textureValue">The value to assign.</param>
        /// <returns>A new key frame if one does not exist, an updated key frame if it already exists, or <b>null</b> to indicate that no data is in the key frame slot.</returns>
        private async Task<IKeyFrame> SetTextureKeyFrameDataAsync(ITrack track, int keyIndex, float time, TextureValue? textureValue)
        {
            IKeyFrame selectedKeyFrame = track.KeyFrames[keyIndex];

            if (selectedKeyFrame?.TextureValue.Texture is not null)
            {
                _contentServices.KeyProcessor.UnloadTextureKeyframe(selectedKeyFrame);
            }

            if (textureValue is null)
            {
                return null;
            }

            // If the currently selected key frame is null, then treat this as a new key frame and populate it with data using the processor.
            selectedKeyFrame = _contentServices.ViewModelFactory.CreateKeyFrame(time, AnimationTrackKeyType.Texture2D);
            await _contentServices.KeyProcessor.CopyTextureToKeyFrameAsync(textureValue.Value, selectedKeyFrame);

            return selectedKeyFrame;
        }

        /// <summary>
        /// Function to assign floating point values to a slot in a track.
        /// </summary>
        /// <param name="track">The track containing the key frame.</param>
        /// <param name="time">The time, in seconds, for the key frame.</param>
        /// <param name="floatValues">The floating point values to assign.</param>
        /// <returns>A new key frame if one does not exist, an updated key frame if it already exists, or <b>null</b> to indicate that no data is in the key frame slot.</returns>
        private IKeyFrame SetFloatValuesKeyFrameData(ITrack track, float time, Vector4? floatValues)
        {
            IKeyFrame result = null;

            if (floatValues is not null)
            {
                // If the currently selected key frame is null, then treat this as a new key frame and populate it with data using the processor.
                result = _contentServices.ViewModelFactory.CreateKeyFrame(time, track.KeyType);
                result.FloatValue = floatValues.Value;
            }

            return result;
        }

        /// <summary>
        /// Function to assign a set of sprite files to the selected keys.
        /// </summary>
        /// <param name="selectedFiles">The files that were selected.</param>
        /// <param name="floatValues">The floating point values to assign.</param>
        /// <returns>A task for asynchronous operation.</returns>
        private async Task SetKeyAsync(IReadOnlyList<IContentFile> selectedFiles, Vector4? floatValues)
        {
            // Function to create the arguments for the undo/redo.
            SetKeyUndoRedoArgs CreateArgs() =>
                new()
                {
                    SelectedKeys = new List<TrackKeySelection.KeySelection>(_content.Selected[0].SelectedKeys.Select(item => new TrackKeySelection.KeySelection(item))),
                };            

            void SetupKeyframeBuffer(ITrack track, ref IKeyFrame[] buffer)
            {
                if (buffer is null)
                {
                    buffer = ArrayPool<IKeyFrame>.Shared.Rent(track.KeyFrames.Count);
                }

                track.KeyFrames.CopyTo(buffer);
            }

            // Function to assign the keys with the sprite files passed in, or from the list of keys passed in.
            async Task SetAsync(List<TrackKeySelection.KeySelection> keys, IReadOnlyList<IContentFile> spriteFiles, Vector4? newFloatValues)
            {
                IKeyFrame[] keyFrames = null;

                try
                {
                    ShowWaitPanel(Resources.GORANM_TEXT_SETTING_KEY);

                    int fileIndex = 0;                    
                    ITrack currentTrack = null;

                    foreach (TrackKeySelection.KeySelection key in keys)
                    {
                        if (currentTrack != key.Track)
                        {
                            SetupKeyframeBuffer(key.Track, ref keyFrames);
                            currentTrack = key.Track;
                        }

                        IKeyFrame selectedKeyFrame = key.KeyFrame;

                        switch (currentTrack.KeyType)
                        {
                            case AnimationTrackKeyType.Single when currentTrack.SpriteProperty == TrackSpriteProperty.Opacity:
                                var opacity = new Vector4(newFloatValues?.X ?? key.KeyFrame?.FloatValue.X ?? 1.0f, 0, 0, 0);
                                keyFrames[key.KeyIndex] = SetFloatValuesKeyFrameData(currentTrack, key.TimeIndex, opacity);
                                break;
                            case AnimationTrackKeyType.Single:
                            case AnimationTrackKeyType.Vector2:
                            case AnimationTrackKeyType.Vector3:
                            case AnimationTrackKeyType.Vector4:
                            case AnimationTrackKeyType.Rectangle:
                            case AnimationTrackKeyType.Color:
                                keyFrames[key.KeyIndex] = SetFloatValuesKeyFrameData(currentTrack, key.TimeIndex, newFloatValues ?? key.KeyFrame?.FloatValue);
                                break;
                            case AnimationTrackKeyType.Texture2D:
                                if (spriteFiles is null)
                                {
                                    keyFrames[key.KeyIndex] = await SetTextureKeyFrameDataAsync(currentTrack, key.KeyIndex, key.TimeIndex, key.KeyFrame?.TextureValue);
                                    continue;
                                }

                                if (spriteFiles.Count == 0)
                                {
                                    keyFrames[key.KeyIndex] = null;
                                    break;
                                }

                                // If we have a previous texture value in here, then unload the texture.
                                if (selectedKeyFrame?.TextureValue.Texture is not null)
                                {
                                    _contentServices.KeyProcessor.UnloadTextureKeyframe(selectedKeyFrame);
                                }

                                // Create a new key.
                                keyFrames[key.KeyIndex] = await _contentServices.ViewModelFactory.CreateTextureKeyFrameAsync(key.TimeIndex, spriteFiles[fileIndex++]);

                                if (fileIndex >= spriteFiles.Count)
                                {
                                    fileIndex = 0;
                                }
                                break;
                        }

                        // If we currently have this keyframe selected, then update the selection.
                        TrackKeySelection.KeySelection actualSelected = _content.Selected[0].SelectedKeys.FirstOrDefault(item => (item is not null) && (item.KeyIndex == key.KeyIndex));
                        if (actualSelected is not null)
                        {
                            actualSelected.KeyFrame = keyFrames[key.KeyIndex];
                        }
                        key.KeyFrame = keyFrames[key.KeyIndex];
                    }

                    // Update the undo/redo arguments.
                    _contentServices.KeyProcessor.AssignKeyFrames(currentTrack, keyFrames, _content.MaxKeyCount);
                    if (CurrentEditor is null)
                    {
                        return;
                    }

                    CurrentEditor.Value = _content.WorkingSprite.GetFloatValues(keys[0].Track.SpriteProperty);
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_SETTING_KEY);
                }
                finally
                {
                    if (keyFrames is not null)
                    {
                        ArrayPool<IKeyFrame>.Shared.Return(keyFrames);
                    }
                    HideWaitPanel();
                }
            }

            Task UndoRedoAsync(SetKeyUndoRedoArgs args, CancellationToken cancelToken) => SetAsync(args.SelectedKeys, null, null);

            SetKeyUndoRedoArgs undoArgs = CreateArgs();
            SetKeyUndoRedoArgs redoArgs = CreateArgs();

            await SetAsync(redoArgs.SelectedKeys, selectedFiles, floatValues);            

            if (redoArgs is not null)
            {
                _contentServices.UndoService.Record(Resources.GORANM_DESC_UNDO_SET_KEY, UndoRedoAsync, UndoRedoAsync, undoArgs, redoArgs);
                NotifyPropertyChanged(nameof(UndoCommand));
            }
        }

        /// <summary>
        /// Function to copy/move keyframes around within tracks.
        /// </summary>
        /// <param name="selectedKeys">The list of selected keys/tracks.</param>
        /// <param name="destKeyIndex">The index of the destination key to start copying into.</param>
        /// <param name="move"><b>true</b> to move the keyframe data to its destination, <b>false</b> to copy.</param>
        /// <returns>A task for asynchronous operation.</returns>
        private async Task CopyMoveFramesAsync(IReadOnlyList<TrackKeySelection> selectedKeys, int destKeyIndex, bool move)
        {
            CopyMoveUndoArgs undoArgs;
            CopyMoveRedoArgs redoArgs;

            async Task RestoreAsync(CopyMoveUndoArgs args)
            {
                IKeyFrame[] keyFrames = null;

                try
                {
                    keyFrames = ArrayPool<IKeyFrame>.Shared.Rent(_content.MaxKeyCount);

                    foreach ((ITrack track, Stack<TrackKeySelection.KeySelection> keys) in args.Keys)
                    {
                        Array.Clear(keyFrames, 0, track.KeyFrames.Count);

                        for (int i = 0; i < track.KeyFrames.Count; ++i)
                        {
                            keyFrames[i] = track.KeyFrames[i];
                        }

                        if (track.KeyType == AnimationTrackKeyType.Texture2D)
                        {
                            ShowWaitPanel(Resources.GORANM_TEXT_PLEASE_WAIT);
                        }

                        foreach (TrackKeySelection.KeySelection key in keys)
                        {
                            IKeyFrame srcKey = key.KeyFrame;
                            IKeyFrame destKey = keyFrames[key.KeyIndex];

                            if ((srcKey is not null) && (srcKey.TextureValue.Texture is null) && (srcKey.TextureValue.TextureFile is not null))
                            {
                                await _contentServices.KeyProcessor.RestoreTextureAsync(srcKey);
                            }

                            if (destKey?.TextureValue.Texture is not null)
                            {
                                _contentServices.KeyProcessor.UnloadTextureKeyframe(destKey);
                            }

                            keyFrames[key.KeyIndex] = srcKey;
                        }

                        _contentServices.KeyProcessor.AssignKeyFrames(track, keyFrames, _content.MaxKeyCount);
                        HideWaitPanel();
                    }
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, move ? Resources.GORANM_ERR_MOVING_KEYFRAMES : Resources.GORANM_ERR_COPYING_KEYFRAMES);
                }
                finally
                {
                    HideWaitPanel();
                    if (keyFrames is not null)
                    {
                        ArrayPool<IKeyFrame>.Shared.Return(keyFrames, true);
                    }
                }
            }

            async Task<bool> CopyMoveAsync(CopyMoveRedoArgs args, CopyMoveUndoArgs undo)
            {
                IKeyFrame[] keyFrames = null;

                try
                {
                    foreach (TrackKeySelection trackSel in args.Keys)
                    {
                        int destIndex = args.DestKeyIndex;

                        if (trackSel.Track.KeyType == AnimationTrackKeyType.Texture2D)
                        {
                            ShowWaitPanel(Resources.GORANM_TEXT_PLEASE_WAIT);
                        }

                        if (keyFrames is null)
                        {
                            keyFrames = ArrayPool<IKeyFrame>.Shared.Rent(trackSel.Track.KeyFrames.Count);
                        }


                        Array.Clear(keyFrames, 0, keyFrames.Length);
                        for (int k = 0; k < trackSel.Track.KeyFrames.Count; ++k)
                        {
                            keyFrames[k] = trackSel.Track.KeyFrames[k];
                        }

                        Stack<TrackKeySelection.KeySelection> undoSel = null;

                        if (undo is not null)
                        {
                            undoSel = new Stack<TrackKeySelection.KeySelection>();
                            undo.Keys.Add((trackSel.Track, undoSel));
                        }

                        // Remove the original keys.
                        if (args.Move)
                        {
                            foreach (TrackKeySelection.KeySelection keySel in trackSel.SelectedKeys)
                            {
                                IKeyFrame srcKeyFrame = trackSel.Track.KeyFrames[keySel.KeyIndex];
                                undoSel?.Push(new TrackKeySelection.KeySelection(keySel));

                                if (srcKeyFrame is not null)
                                {
                                    if (srcKeyFrame.TextureValue.Texture is not null)
                                    {
                                        _contentServices.KeyProcessor.UnloadTextureKeyframe(srcKeyFrame);
                                    }

                                    keyFrames[keySel.KeyIndex] = null;
                                }
                            }
                        }

                        int lastKeyIndex = trackSel.SelectedKeys[0].KeyIndex;

                        foreach (TrackKeySelection.KeySelection keySel in trackSel.SelectedKeys)
                        {
                            destIndex += (keySel.KeyIndex - lastKeyIndex);
                            lastKeyIndex = keySel.KeyIndex;

                            // Move to the next keyframe, but if we reach the end of the track, then move to the next track.
                            if (destIndex >= trackSel.Track.KeyFrames.Count)
                            {
                                break;
                            }

                            IKeyFrame destKeyFrame = trackSel.Track.KeyFrames[destIndex];
                            // If we've already got this keyframe captured for undo, then don't add a duplicate, it will get restored to its original
                            // state earlier in the stack.
                            if ((undoSel is not null) && (!undoSel.Any(item => item?.KeyIndex == destIndex)))
                            {
                                undoSel?.Push(new TrackKeySelection.KeySelection(trackSel.Track, destIndex, _content.Fps));
                            }

                            // If we've got a texture in the destination, unload it.
                            if ((destKeyFrame is not null) && (trackSel.Track.KeyType == AnimationTrackKeyType.Texture2D) && (destKeyFrame.TextureValue.Texture is not null))
                            {
                                _contentServices.KeyProcessor.UnloadTextureKeyframe(destKeyFrame);
                            }

                            if (keySel.KeyFrame is not null)
                            {
                                IKeyFrame newFrame = _contentServices.ViewModelFactory.CreateKeyFrame(destKeyFrame is null ? (destIndex / _content.Fps) : destKeyFrame.Time, trackSel.Track.KeyType);
                                newFrame.FloatValue = keySel.KeyFrame.FloatValue;
                                newFrame.TextureValue = keySel.KeyFrame.TextureValue;

                                if ((newFrame.TextureValue.TextureFile is not null) && (trackSel.Track.KeyType == AnimationTrackKeyType.Texture2D))
                                {
                                    await _contentServices.KeyProcessor.RestoreTextureAsync(newFrame);
                                }

                                keyFrames[destIndex] = newFrame;
                            }
                            else
                            {
                                keyFrames[destIndex] = null;
                            }
                        }

                        _contentServices.KeyProcessor.AssignKeyFrames(trackSel.Track, keyFrames, trackSel.Track.KeyFrames.Count);

                        HideWaitPanel();
                    }

                    if (CurrentEditor is not null)
                    {
                        CurrentEditor.Value = _content.WorkingSprite.GetFloatValues(_content.Selected[0].Track.SpriteProperty);
                    }

                    return true;
                }
                catch (Exception ex)
                {
                    HostServices.MessageDisplay.ShowError(ex, move ? Resources.GORANM_ERR_MOVING_KEYFRAMES : Resources.GORANM_ERR_COPYING_KEYFRAMES);
                    return false;
                }
                finally
                {
                    if (keyFrames is not null)
                    {
                        ArrayPool<IKeyFrame>.Shared.Return(keyFrames, true);
                    }
                    HideWaitPanel();
                }
            }

            redoArgs = new CopyMoveRedoArgs
            {
                Keys = selectedKeys,
                DestKeyIndex = destKeyIndex,
                Move = move
            };
            undoArgs = new CopyMoveUndoArgs
            {
                Keys = new List<(ITrack, Stack<TrackKeySelection.KeySelection>)>()
            };

            Task RedoTask(CopyMoveRedoArgs args, CancellationToken _) => CopyMoveAsync(args, null);
            Task UndoTask(CopyMoveUndoArgs args, CancellationToken _) => RestoreAsync(args);

            if (await CopyMoveAsync(redoArgs, undoArgs))
            {
                _contentServices.UndoService.Record(move ? Resources.GORANM_DESC_UNDO_MOVEKEYS : Resources.GORANM_DESC_UNDO_COPYKEYS, UndoTask, RedoTask, undoArgs, redoArgs);
                NotifyPropertyChanged(nameof(UndoCommand));
            }
        }

        /// <summary>
        /// Function to determine if the key can be set.
        /// </summary>
        /// <returns><b>true</b> if the key can be set, <b>false</b> if not.</returns>
        private bool CanSetKey()
        {
            if (_content.Selected.Count != 1)
            {
                return false;
            }

            _selectedSprites.Clear();

            // If any of the selected tracks are texture tracks, and we have no files, then don't allow the operation.
            if (_content.Selected[0].SelectedKeys.Any(item => item.Track.KeyType == AnimationTrackKeyType.Texture2D))
            {
                _selectedSprites.AddRange(_fileManager.GetSelectedFiles().Select(item => _fileManager.GetFile(item)));

                // If we have sprites selected, and none of them are sprites.
                if ((_selectedSprites.Count == 0) || (_selectedSprites.Any(item => (item is null) || (!_contentServices.IOService.IsContentSprite(item)))))
                {
                    _selectedSprites.Clear();
                    return false;
                }

                _selectedSprites.Clear();
            }

            // Do not set multiple key frames across tracks.
            return true;
        }

        /// <summary>
        /// Function to assign key values.
        /// </summary>        
        private async Task DoSetKeyAsync()
        {
            Vector4? floatValues = null;

            try
            {
                // Retrieve the selected file(s).
                _selectedSprites.Clear();

                // We'll be using sprite files for the texture keys.
                if (_content.Selected[0].Track.KeyType == AnimationTrackKeyType.Texture2D)
                {
                    _selectedSprites.AddRange(_fileManager.GetSelectedFiles()
                                                          .Reverse()
                                                          .Select(item => _fileManager.GetFile(item))
                                                          .Where(item => (item is not null) && (_contentServices.IOService.IsContentSprite(item))));
                }
                else
                {
                    floatValues = CurrentEditor.Value;
                }

                await SetKeyAsync(_selectedSprites, floatValues);
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_SETTING_KEY);                
            }
        }

        /// <summary>
        /// Function to determine if sprites can be loaded.
        /// </summary>
        /// <param name="paths">The paths to the sprites to load.</param>
        /// <returns><b>true</b> if the sprites can be loaded, <b>false</b> if not.</returns>
        private bool CanLoadSprite(IReadOnlyList<string> paths)
        {
            if ((_content.Selected.Count == 0) || (paths is null) || (paths.Count == 0) || (_content.Selected.Any(item => item.Track.KeyType != AnimationTrackKeyType.Texture2D)))
            {
                return false;
            }

            IEnumerable<IContentFile> files = paths.Where(item => !string.IsNullOrWhiteSpace(item))
                                                   .Select(item => _fileManager.GetFile(item));

            return files.All(item => (item is not null) && (_contentServices.IOService.IsContentSprite(item)));                                                   
        }

        /// <summary>
        /// Function to load sprites as key frames.
        /// </summary>
        /// <param name="paths">The paths to the sprites to load.</param>
        /// <returns>A task for asynchronous operation.</returns>
        private async Task DoLoadSprite(IReadOnlyList<string> paths)
        {
            try
            {
                // Retrieve the selected file(s).
                _selectedSprites.Clear();
                _selectedSprites.AddRange(paths.Reverse()
                                               .Select(item => _fileManager.GetFile(item)));
                await SetKeyAsync(_selectedSprites, null);
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_SETTING_KEY);
            }
        }


        /// <summary>
        /// Function to determine if an undo operation is possible.
        /// </summary>
        /// <returns><b>true</b> if the last action can be undone, <b>false</b> if not.</returns>
        private bool CanUndo() => (_contentServices.UndoService.CanUndo);

        /// <summary>
        /// Function to determine if a redo operation is possible.
        /// </summary>
        /// <returns><b>true</b> if the last action can be redone, <b>false</b> if not.</returns>
        private bool CanRedo() => (_contentServices.UndoService.CanRedo);

        /// <summary>
        /// Function called when a redo operation is requested.
        /// </summary>
        private async void DoRedoAsync()
        {
            try
            {
                await _contentServices.UndoService.Redo();
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_REDO);
            }
        }

        /// <summary>
        /// Function called when an undo operation is requested.
        /// </summary>
        private async void DoUndoAsync()
        {
            try
            {
                await _contentServices.UndoService.Undo();
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_UNDO);
            }
        }

        /// <summary>
        /// Function to determine if the selected key frames can be removed.
        /// </summary>
        /// <returns><b>true</b> if the keyframes can be removed, <b>false</b> if not.</returns>
        private bool CanRemoveKeyframes() => (_content.CommandContext == this) && (_content.Selected.Count > 0) && (_content.Selected.SelectMany(item => item.SelectedKeys).Any(item => item is not null));

        /// <summary>
        /// Function to remove the selected key frames.
        /// </summary>
        /// <returns>A task for asynchronous operation.</returns>
        private void DoRemoveKeyFrames()
        {
            if (HostServices.MessageDisplay.ShowConfirmation(Resources.GORANM_CONFIRM_REMOVE_KEYFRAMES) == MessageResponse.No)
            {
                return;
            }

            RemoveKeyFrames(_content.Selected);
        }

        /// <summary>
        /// Function to determine if the keyframes can be cleared.
        /// </summary>
        /// <returns><b>true</b> if the keyframes can be cleared, <b>false</b> if not.</returns>
        private bool CanClearKeyframes() => (_content.CommandContext == this) && (_content.Tracks.Count > 0) && (_content.Tracks.SelectMany(item => item.KeyFrames).Any(item => item is not null));

        /// <summary>
        /// Function to clear all keyframes from the animation.
        /// </summary>
        private void DoClearKeyFrames()
        {
            if (HostServices.MessageDisplay.ShowConfirmation(Resources.GORANM_CONFIRM_CLEAR_KEYFRAMES) == MessageResponse.No)
            {
                return;
            }

            TrackKeySelection[] selection = null;

            try
            {
                selection = ArrayPool<TrackKeySelection>.Shared.Rent(_content.Tracks.Count);

                for (int i = 0; i < _content.Tracks.Count; ++i)
                {
                    ITrack track = _content.Tracks[i];
                    selection[i] = new TrackKeySelection(i, track, track.KeyFrames.Select((item, index) => new TrackKeySelection.KeySelection(track, index, _content.Fps)).ToArray());
                }

                // Undo/redo functionality is in this method:
                RemoveKeyFrames(selection);
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_CLEAR_KEYFRAMES);
            }
            finally
            {
                if (selection is not null)
                {
                    ArrayPool<TrackKeySelection>.Shared.Return(selection, true);
                }
            }
        }

        /// <summary>
        /// Function to determine if frames can be copied/moved.
        /// </summary>
        /// <param name="args">The command argument.</param>
        /// <returns><b>true</b> if the keys can be copied/moved, <b>false</b> if not.</returns>
        private bool CanCopyMoveFrames(KeyFrameCopyMoveData args)
        {
            if ((args is null) || (args.KeyFrames.Count == 0) || (_content.CommandContext != this))
            {
                return false;
            }

            int minKeyIndex = args.KeyFrames.SelectMany(item => item.SelectedKeys).Min(item => item.KeyIndex);

            return ((args.DestinationKeyIndex >= 0) && (args.DestinationKeyIndex != minKeyIndex));
        }

        /// <summary>
        /// Function to copy/move keyframes within a track.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        /// <returns>A task for asynchronous operation.</returns>
        private async Task DoCopyMoveFramesAsync(KeyFrameCopyMoveData args)
        {
            try
            {
                var copiedData = new List<TrackKeySelection>();

                // Duplicate the data in the selection.
                foreach (TrackKeySelection trackSel in args.KeyFrames)
                {
                    var copiedKeys = new List<TrackKeySelection.KeySelection>();
                    foreach (TrackKeySelection.KeySelection keySel in trackSel.SelectedKeys.OrderBy(item => item.TimeIndex))
                    {
                        IKeyFrame keyFrame = keySel.KeyFrame;

                        if (keyFrame is not null)
                        {
                            if (keyFrame.DataType != AnimationTrackKeyType.Texture2D)
                            {
                                keyFrame = _contentServices.ViewModelFactory.CreateKeyFrame(keyFrame.Time, keyFrame.DataType, keyFrame.FloatValue);
                            }
                            else
                            {
                                var textureValue = new TextureValue(null, keyFrame.TextureValue.TextureFile, keyFrame.TextureValue.ArrayIndex, keyFrame.TextureValue.TextureCoordinates);
                                keyFrame = _contentServices.ViewModelFactory.CreateKeyFrame(keyFrame.Time, ref textureValue);
                            }
                        }

                        copiedKeys.Add(new TrackKeySelection.KeySelection(trackSel.Track, keySel.KeyIndex, keyFrame, _content.Fps));
                    }

                    copiedData.Add(new TrackKeySelection(trackSel.TrackIndex, trackSel.Track, copiedKeys));
                }

                await CopyMoveFramesAsync(copiedData, args.DestinationKeyIndex, args.Operation == CopyMoveOperation.Move);

                var selection = new (int trackIndex, IReadOnlyList<int> keys)[]
                {
                    (args.KeyFrames[0].TrackIndex, new[] { args.DestinationKeyIndex })
                };

                if ((_content.SelectTrackAndKeysCommand is not null) && (_content.SelectTrackAndKeysCommand.CanExecute(selection)))
                {
                    _content.SelectTrackAndKeysCommand.Execute(selection);
                }
            }
            catch (Exception ex) when (args.Operation == CopyMoveOperation.Move)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_MOVING_KEYFRAMES);
            }
            catch (Exception ex) when (args.Operation == CopyMoveOperation.Copy)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_COPYING_KEYFRAMES);
            }
        }

        /// <summary>
        /// Function called to determine if data can be pasted from the clipboard.
        /// </summary>
        /// <returns><b>true</b> if the data can be pasted, <b>false</b> if not.</returns>
        private bool CanPasteKeyframes()
        {
            if ((!HostServices.ClipboardService.HasData) || (_content.Selected is null) || (_content.Selected.Count == 0))
            {
                return false;
            }

            if (!HostServices.ClipboardService.IsType<IKeyFrameCopyMoveData>())
            {
                return false;
            }

            IKeyFrameCopyMoveData data = HostServices.ClipboardService.GetData<IKeyFrameCopyMoveData>();

            return (data is not null) && (data.KeyFrames.Count != 0);
        }

        /// <summary>
        /// Function called to paste keyframes from the clipboard.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        private async Task DoPasteKeyframesAsync()
        {
            try
            {
                int startDest = _content.Selected[0].SelectedKeys[_content.Selected[0].SelectedKeys.Count - 1].KeyIndex;
                IKeyFrameCopyMoveData data = HostServices.ClipboardService.GetData<IKeyFrameCopyMoveData>();

                await CopyMoveFramesAsync(data.KeyFrames, startDest, data.Operation == CopyMoveOperation.Move);

                if (data.Operation == CopyMoveOperation.Move)
                {
                    HostServices.ClipboardService.Clear();
                    NotifyPropertyChanged(nameof(PasteDataCommand));
                }
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_PASTE_KEYFRAMES);
            }
        }

        /// <summary>
        /// Function to determine if keyframes can be copied to the clipboard.
        /// </summary>
        /// <param name="args">The arguments for the command.</param>
        /// <returns><b>true</b> if the keyframes can be copied to the clipboard, <b>false</b> if not.</returns>
        private bool CanCopyKeyFrames(object args)
        {
            var copyData = args as IKeyFrameCopyMoveData;

            return (copyData?.KeyFrames is not null) && (copyData.KeyFrames.Count > 0) && (_content.CommandContext == this);
        }

        /// <summary>
        /// Function to copy the selected keyframes.
        /// </summary>
        /// <param name="args">The arguments for copying.</param>
        private void DoCopyKeyFrames(object args)
        {
            var copyData = (IKeyFrameCopyMoveData)args;

            try
            {
                var copiedData = new List<TrackKeySelection>();

                // Duplicate the data in the selection.
                foreach (TrackKeySelection trackSel in copyData.KeyFrames)
                {
                    var copiedKeys = new List<TrackKeySelection.KeySelection>();
                    foreach (TrackKeySelection.KeySelection keySel in trackSel.SelectedKeys.OrderBy(item => item.TimeIndex))
                    {
                        IKeyFrame keyFrame = keySel.KeyFrame;

                        if (keyFrame is not null)
                        {
                            if (keyFrame.DataType != AnimationTrackKeyType.Texture2D)
                            {
                                keyFrame = _contentServices.ViewModelFactory.CreateKeyFrame(keyFrame.Time, keyFrame.DataType, keyFrame.FloatValue);
                            }
                            else
                            {
                                var textureValue = new TextureValue(null, keyFrame.TextureValue.TextureFile, keyFrame.TextureValue.ArrayIndex, keyFrame.TextureValue.TextureCoordinates);
                                keyFrame = _contentServices.ViewModelFactory.CreateKeyFrame(keyFrame.Time, ref textureValue);
                            }
                        }

                        copiedKeys.Add(new TrackKeySelection.KeySelection(trackSel.Track, keySel.KeyIndex, keyFrame, _content.Fps));
                    }

                    copiedData.Add(new TrackKeySelection(trackSel.TrackIndex, trackSel.Track, copiedKeys));
                }

                HostServices.ClipboardService.CopyItem(new KeyFrameCopyMoveData
                {
                    KeyFrames = copiedData,
                    Operation = copyData.Operation
                });

                NotifyPropertyChanged(nameof(PasteDataCommand));                
            }
            catch (Exception ex) when (copyData.Operation == CopyMoveOperation.Copy)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_COPY_KEYFRAMES);
            }
            catch (Exception ex)
            {
                HostServices.MessageDisplay.ShowError(ex, Resources.GORANM_ERR_CUT_KEYFRAMES);
            }
        }

        /// <summary>Function to inject dependencies for the view model.</summary>
        /// <param name="injectionParameters">The parameters to inject.</param>
        /// <remarks>
        ///   <para>
        /// Applications should call this when setting up the view model for complex operations and/or dependency injection. The constructor should only be used for simple set up and initialization of objects.
        /// </para>
        ///   <para>
        /// This method is only ever called after the view model has been created, and never again during the lifetime of the view model.
        /// </para>
        /// </remarks>
        protected override void OnInitialize(KeyEditorContextParameters injectionParameters)
        {
            _content = injectionParameters.Content;
            _fileManager = injectionParameters.FileManager;
            _contentServices = injectionParameters.ContentServices;
            _controller = injectionParameters.AnimationController;            
            FloatKeysEditor = injectionParameters.FloatValueKeyEditor;
            ColorKeysEditor = injectionParameters.ColorValueKeyEditor;
        }

        /// <summary>Function called when the associated view is loaded.</summary>
        /// <remarks>
        ///   <para>
        /// This method should be overridden when there is a need to set up functionality (e.g. events) when the UI first loads with the view model attached. Unlike the <see cref="ViewModelBase{T, THs}.Initialize(T)"/> method, this
        /// method may be called multiple times during the lifetime of the application.
        /// </para>
        ///   <para>
        /// Anything that requires tear down should have their tear down functionality in the accompanying <see cref="ViewModelBase{T, THs}.Unload"/> method.
        /// </para>
        /// </remarks>
        /// <seealso cref="ViewModelBase{T, THs}.Initialize(T)" />
        /// <seealso cref="ViewModelBase{T, THs}.Unload" />
        protected override void OnLoad() 
        {
            base.OnLoad();

            CurrentEditor = null;
            HostServices.ClipboardService.Clear();
            _content.PropertyChanged += Content_PropertyChanged;

            ShowEditorPanel();            
        }

        /// <summary>Function called when the associated view is unloaded.</summary>
        /// <remarks>This method is used to perform tear down and clean up of resources.</remarks>
        /// <seealso cref="ViewModelBase{T, THs}.Load" />
        protected override void OnUnload()
        {
            HostServices.ClipboardService.Clear();
            _content.PropertyChanged -= Content_PropertyChanged;

            if (CurrentEditor is not null)
            {
                CurrentEditor.WorkingSprite = null;
                CurrentEditor.Track = null;
            }

            CurrentEditor = null;

            base.OnUnload();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="KeyEditorContext"/> class.</summary>
        public KeyEditorContext()
        {
            SetKeyCommand = new EditorAsyncCommand<object>(DoSetKeyAsync, CanSetKey);
            LoadSpriteCommand = new EditorAsyncCommand<IReadOnlyList<string>>(DoLoadSprite, CanLoadSprite);
            UndoCommand = new EditorCommand<object>(DoUndoAsync, CanUndo);
            RedoCommand = new EditorCommand<object>(DoRedoAsync, CanRedo);
            RemoveKeyCommand = new EditorCommand<object>(DoRemoveKeyFrames, CanRemoveKeyframes);
            ClearKeysCommand = new EditorCommand<object>(DoClearKeyFrames, CanClearKeyframes);
            CopyDataCommand = new EditorCommand<object>(DoCopyKeyFrames, CanCopyKeyFrames);
            PasteDataCommand = new EditorAsyncCommand<object>(DoPasteKeyframesAsync, CanPasteKeyframes);
            CopyMoveFramesCommand = new EditorAsyncCommand<KeyFrameCopyMoveData>(DoCopyMoveFramesAsync, CanCopyMoveFrames);
        }
        #endregion
    }
}
