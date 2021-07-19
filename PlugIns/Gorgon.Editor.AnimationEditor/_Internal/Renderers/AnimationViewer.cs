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
// Created: June 8, 2020 7:33:05 PM
// 
#endregion

using System.ComponentModel;
using System.Numerics;
using System.Threading;
using System.Windows.Forms;
using Gorgon.Animation;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.AnimationEditor
{
    /// <summary>
    /// Provides rendering functionality for the animation editor.
    /// </summary>
    internal abstract class AnimationViewer
        : DefaultContentRenderer<IAnimationContent>
    {
        #region Variables.
        // The main render target view.
        private GorgonRenderTarget2DView _mainRtv;
        // The main render target texture.
        private GorgonTexture2DView _main;
        // The sprite representing the onionskin prior to the current frame.
        private GorgonSprite _onionBefore;
        // The sprite representing the onionskin prior after the current frame.
        private GorgonSprite _onionAfter;
        // The previous key.
        private IKeyFrame _prevKey;
        // The next key.
        private IKeyFrame _nextKey;
        // The effect for the onion skin.
        private Gorgon2DSilhouetteEffect _silhouette;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the silhouette effect for the onion skin.
        /// </summary>
        protected Gorgon2DSilhouetteEffect Silhouette => _silhouette;

        /// <summary>
        /// Property to return the ID of the selected track.
        /// </summary>
        protected TrackSpriteProperty SelectedTrackID => DataContext.Selected.Count == 0 ? TrackSpriteProperty.None : DataContext.Selected[0].Track.SpriteProperty;

        /// <summary>
        /// Property to set or return whether the view supports onion skinning.
        /// </summary>
        protected bool SupportsOnionSkinning
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the sprite to update with the animation data.
        /// </summary>
        protected GorgonSprite Sprite
        {
            get;
            private set;
        }

        /// <summary>
        /// Property to return the canvas that will receive the rendering for the animated sprite.
        /// </summary>
        protected GorgonRenderTarget2DView Canvas => _mainRtv;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to create the main render target view.
        /// </summary>
        private void CreateRtv()
        {
            GorgonRenderTarget2DView rtv = Interlocked.Exchange(ref _mainRtv, null);
            GorgonTexture2DView main = Interlocked.Exchange(ref _main, null);
            rtv?.Dispose();
            main?.Dispose();

            _mainRtv = GorgonRenderTarget2DView.CreateRenderTarget(Graphics, new GorgonTexture2DInfo((int)RenderRegion.Width, (int)RenderRegion.Height, MainRenderTarget.Format)
            {
                Name = "Animation Editor Main Rtv",
                Binding = TextureBinding.ShaderResource
            });
            _main = _mainRtv.GetShaderResourceView();
        }

        /// <summary>
        /// Function to set up the area to render.
        /// </summary>
        private void SetRenderRegion()
        {
            if (DataContext.BackgroundImage is null)
            {
                RenderRegion = new DX.RectangleF(0, 0, DataContext.Settings.DefaultResolution.Width, DataContext.Settings.DefaultResolution.Height);
            }
            else
            {
                RenderRegion = new DX.RectangleF(0, 0, DataContext.BackgroundImage.Width, DataContext.BackgroundImage.Height);
            }

            CreateRtv();
        }

        /// <summary>Function to render the background.</summary>
        /// <remarks>Developers can override this method to render a custom background.</remarks>
        private void DrawBackgroundImage()
        {
            var region = new DX.RectangleF(RenderRegion.Width * -Camera.Anchor.X,
                                           RenderRegion.Height * -Camera.Anchor.Y,
                                           RenderRegion.Width,
                                           RenderRegion.Height);

            Renderer.DrawFilledRectangle(region, new GorgonColor(GorgonColor.SteelBlue, 0.25f));

            if (DataContext?.BackgroundImage is not null)
            {
                Renderer.DrawFilledRectangle(region, GorgonColor.White, DataContext.BackgroundImage, new DX.RectangleF(0, 0, 1, 1));
            }
        }

        /// <summary>
        /// Function to create the onion skin sprites.
        /// </summary>
        private void CreateOnionSprites()
        {
            _onionBefore = new GorgonSprite(DataContext.PrimarySprite)
            {
                Color = new GorgonColor(GorgonColor.BluePure, 0.25f)
            };
            _onionAfter = new GorgonSprite(_onionBefore)
            {
                Color = new GorgonColor(GorgonColor.RedPure, 0.25f)
            };

            UpdateOnionSkin();
        }

        /// <summary>
        /// Function to update the onion skin.
        /// </summary>
        private void UpdateOnionSkin()
        {
            if ((DataContext?.CommandContext is null) || (DataContext.CommandContext != DataContext.KeyEditor) || (DataContext.Selected.Count == 0))
            {
                return;
            }

            ITrack selectedTrack = DataContext.Selected[0].Track;
            IKeyFrame prev;
            IKeyFrame next;

            void SetTrackKeyValue(GorgonSprite sprite, ITrack track, IKeyFrame keyFrame)
            {
                // Take the most current text info.
                sprite.Texture = DataContext.WorkingSprite.Texture;
                sprite.TextureArrayIndex = DataContext.WorkingSprite.TextureArrayIndex;
                sprite.TextureRegion = DataContext.WorkingSprite.TextureRegion;

                Vector4 defaultValue = DataContext.WorkingSprite.GetFloatValues(track.SpriteProperty);

                sprite.SetFloatValues(track.SpriteProperty, keyFrame is null ? defaultValue : keyFrame.FloatValue);
            }

            for (int t = 0; t < DataContext.Tracks.Count; ++t)
            {
                ITrack track = DataContext.Tracks[t];

                int keyIndex = DataContext.Selected[0].SelectedKeys[0].KeyIndex;
                prev = track.KeyFrames[keyIndex];
                next = track.KeyFrames[keyIndex];

                // Onionskin sprites have their own constant color values, so we won't be updating those values.
                if ((track.KeyType == AnimationTrackKeyType.Color)
                    || (track.SpriteProperty == TrackSpriteProperty.Opacity))
                {
                    continue;
                }

                // Locate the nearest keys from this key.
                if (keyIndex > 0)
                {
                    // Get the previous key.
                    for (int k = keyIndex - 1; k >= 0; --k)
                    {
                        prev = track.KeyFrames[k];

                        if (prev is not null)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    if ((DataContext.IsLooping) && (track.KeyFrames[track.KeyFrames.Count - 1] is not null))
                    {
                        prev = track.KeyFrames[track.KeyFrames.Count - 1];
                    }
                    else
                    {
                        prev = null;
                    }
                }

                if (keyIndex < track.KeyFrames.Count - 1)
                {
                    // Get the previous key.
                    for (int k = keyIndex + 1; k < track.KeyFrames.Count; ++k)
                    {
                        next = track.KeyFrames[k];

                        if (next is not null)
                        {
                            break;
                        }
                    }
                }
                else
                {
                    if ((DataContext.IsLooping) && (track.KeyFrames[0] is not null))
                    {
                        next = track.KeyFrames[0];
                    }
                    else
                    {
                        next = null;
                    }

                    if (next == prev)
                    {
                        prev = null;
                    }
                }

                SetTrackKeyValue(_onionBefore, track, prev);
                SetTrackKeyValue(_onionAfter, track, next);

                if (selectedTrack == track)
                {
                    _prevKey = prev;
                    _nextKey = next;
                }
            }
        }

        /// <summary>Handles the PropertyChanged event of the KeyEditor control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void KeyEditor_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (DataContext.KeyEditor.CurrentEditor?.Track is null)
            {
                return;
            }

            OnKeyEditorContextPropertyChanged(e.PropertyName);
        }

        /// <summary>Handles the PropertyChanged event of the Settings control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void Settings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (!IsEnabled)
            {
                return;
            }

            switch (e.PropertyName)
            {
                case nameof(ISettings.DefaultResolution):
                    if (DataContext.BackgroundImage is null)
                    {
                        SetRenderRegion();
                        DefaultZoom();
                    }
                    break;
            }

            OnSettingsChanged(e.PropertyName);
        }

        /// <summary>Handles the PropertyChanged event of the CurrentPanel control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>
        private void CurrentPanel_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IKeyValueEditor.Value):
                    UpdateOnionSkin();
                    break;
            }

            OnKeyFramePanelPropertyChanged(e.PropertyName);
        }

        /// <summary>
        /// Function called when the animation editor settings are changed.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected virtual void OnSettingsChanged(string propertyName)
        {

        }

        /// <summary>
        /// Function called when a property is changed on the keyframe editor panel.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected virtual void OnKeyFramePanelPropertyChanged(string propertyName)
        {

        }

        /// <summary>
        /// Function called when a property is changed on the key editor context.
        /// </summary>
        /// <param name="propertyName">The name of the changed property.</param>
        protected virtual void OnKeyEditorContextPropertyChanged(string propertyName)
        {

        }

        /// <summary>Function called when a property on the <see cref="DefaultContentRenderer{T}.DataContext"/> is changing.</summary>
        /// <param name="propertyName">The name of the property that is changing.</param>
        /// <remarks>Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.</remarks>
        protected override void OnPropertyChanging(string propertyName)
        {
            base.OnPropertyChanging(propertyName);

            switch (propertyName)
            {
                case nameof(IAnimationContent.CommandContext):
                case nameof(IAnimationContent.CurrentPanel):
                    if (DataContext.CommandContext != DataContext.KeyEditor)
                    {
                        break;
                    }

                    if (DataContext.CurrentPanel is not null)
                    {
                        DataContext.CurrentPanel.PropertyChanged -= CurrentPanel_PropertyChanged;
                    }
                    break;
            }
        }

        /// <summary>Function called when a property on the <see cref="DefaultContentRenderer{T}.DataContext"/> has been changed.</summary>
        /// <param name="propertyName">The name of the property that was changed.</param>
        /// <remarks>Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.</remarks>
        protected override void OnPropertyChanged(string propertyName)
        {
            base.OnPropertyChanged(propertyName);

            if (!IsEnabled)
            {
                return;
            }

            switch (propertyName)
            {
                case nameof(IAnimationContent.CommandContext):
                case nameof(IAnimationContent.CurrentPanel):
                    if (DataContext.CommandContext != DataContext.KeyEditor)
                    {
                        break;
                    }

                    if (DataContext.CurrentPanel is not null)
                    {
                        DataContext.CurrentPanel.PropertyChanged += CurrentPanel_PropertyChanged;
                    }

                    UpdateOnionSkin();
                    break;
                case nameof(IAnimationContent.WorkingSprite):
                    UpdateOnionSkin();
                    break;
                case nameof(IAnimationContent.BackgroundImage):
                    SetRenderRegion();
                    DefaultZoom();
                    break;
            }
        }

        /// <summary>Function to render the background.</summary>
        /// <remarks>Developers can override this method to render a custom background.</remarks>
        protected sealed override void OnRenderBackground()
        {
            var textureSize = new DX.RectangleF(0, 0, (float)ClientSize.Width / BackgroundPattern.Width, (float)ClientSize.Height / BackgroundPattern.Height);

            Renderer.Begin();
            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, ClientSize.Width, ClientSize.Height),
                                                           GorgonColor.White, BackgroundPattern, textureSize);
            Renderer.End();
        }

        /// <summary>
        /// Function to draw the sprite anchor as a circle on the view.
        /// </summary>
        protected void DrawAnchorPoint()
        {
            Vector2 spriteAnchor = ToClient(new Vector2(Sprite.Position.X - RenderRegion.Width * 0.5f,
                                                              Sprite.Position.Y - RenderRegion.Height * 0.5f));

            Renderer.DrawEllipse(new DX.RectangleF(spriteAnchor.X - 4, spriteAnchor.Y - 4, 8, 8), GorgonColor.Black);
            Renderer.DrawEllipse(new DX.RectangleF(spriteAnchor.X - 3, spriteAnchor.Y - 3, 6, 6), GorgonColor.White);
        }

        /// <summary>
        /// Function to draw the animation.
        /// </summary>
        protected abstract void DrawAnimation();

        /// <summary>
        /// Function to draw any gizmos for UI components.
        /// </summary>
        protected virtual void DrawGizmos()
        {
            Renderer.Begin();
            DrawAnchorPoint();
            Renderer.End();
        }

        /// <summary>Function to handle a preview key down event.</summary>
        /// <param name="args">The arguments for the event.</param>
        /// <remarks>Developers can override this method to handle a preview key down event in their own content view.</remarks>
        protected override void OnPreviewKeyDown(PreviewKeyDownEventArgs args)
        {
            if (!IsEnabled)
            {
                return;
            }

            switch (args.KeyCode)
            {
                case Keys.Home when (Sprite is not null) && ((args.Modifiers & Keys.Control) == Keys.Control) && ((args.Modifiers & Keys.Shift) == Keys.Shift):
                    ZoomToSprite(Sprite);
                    args.IsInputKey = true;
                    break;
                case Keys.Escape:
                    if ((DataContext.CommandContext == DataContext.KeyEditor) && (DataContext.ActivateKeyEditorCommand is not null) && (DataContext.ActivateKeyEditorCommand.CanExecute(null)))
                    {
                        DataContext.ActivateKeyEditorCommand.Execute(null);
                        args.IsInputKey = true;
                    }
                    break;
            }

            base.OnPreviewKeyDown(args);
        }

        /// <summary>Function to render the content.</summary>
        /// <remarks>This is the method that developers should override in order to draw their content to the view.</remarks>
        protected sealed override void OnRenderContent()
        {
            base.OnRenderContent();

            _mainRtv.Clear(GorgonColor.BlackTransparent);
            Graphics.SetRenderTarget(_mainRtv);

            UpdateOnionSkin();

            if ((SupportsOnionSkinning) && (DataContext.Settings.UseOnionSkinning) && (_prevKey is not null))
            {
                Silhouette.Begin();
                Renderer.DrawSprite(_onionBefore);
                Silhouette.End();
            }

            DrawAnimation();

            if ((SupportsOnionSkinning) && (DataContext.Settings.UseOnionSkinning) && (_nextKey is not null))
            {
                Silhouette.Begin();
                Renderer.DrawSprite(_onionAfter);
                Silhouette.End();
            }

            Graphics.SetRenderTarget(MainRenderTarget);
            Renderer.Begin(camera: Camera);
            DrawBackgroundImage();
            Renderer.End();

            Renderer.Begin(Gorgon2DBatchState.PremultipliedBlendAlphaOverwrite, Camera);
            Renderer.DrawFilledRectangle(new DX.RectangleF(RenderRegion.Width * -Camera.Anchor.X, RenderRegion.Height * -Camera.Anchor.Y, _main.Width, _main.Height),
                                         GorgonColor.White,
                                         _main,
                                         new DX.RectangleF(0, 0, 1, 1),
                                         textureSampler: (DataContext.PrimarySprite is null ? GorgonSamplerState.PointFiltering : DataContext.PrimarySprite.TextureSampler));
            Renderer.End();
            DrawGizmos();
        }

        /// <summary>Function to handle a mouse down event.</summary>
        /// <param name="args">The arguments for the event.</param>
        /// <remarks>Developers can override this method to handle a mouse down event in their own content view.</remarks>
        protected override void OnMouseDown(MouseArgs args)
        {
            if ((args.ButtonClickCount > 1) && ((args.Modifiers & Keys.Control) == Keys.Control) && (Sprite is not null))
            {
                ZoomToSprite(Sprite);
                args.Handled = true;                
            }

            base.OnMouseDown(args);
        }

        /// <summary>Function called when the renderer needs to load any resource data.</summary>
        /// <remarks>
        /// Developers can override this method to set up their own resources specific to their renderer. Any resources set up in this method should be cleaned up in the associated
        /// <see cref="DefaultContentRenderer{T}.OnUnload"/> method.
        /// </remarks>
        protected override void OnLoad()
        {
            base.OnLoad();

            Sprite = DataContext.WorkingSprite;
            SetRenderRegion();

            CreateOnionSprites();

            DataContext.Settings.PropertyChanged += Settings_PropertyChanged;
            DataContext.KeyEditor.PropertyChanged += KeyEditor_PropertyChanged;

            if (DataContext.CurrentPanel is not null)
            {
                DataContext.CurrentPanel.PropertyChanged += CurrentPanel_PropertyChanged;
            }
        }

        /// <summary>Function called when the renderer needs to clean up any resource data.</summary>
        /// <remarks>
        /// Developers should always override this method if they've overridden the <see cref="DefaultContentRenderer{T}.OnLoad"/> method. Failure to do so can cause memory leakage.
        /// </remarks>
        protected override void OnUnload()
        {
            DataContext.KeyEditor.PropertyChanged -= KeyEditor_PropertyChanged;
            DataContext.Settings.PropertyChanged -= Settings_PropertyChanged;

            GorgonRenderTarget2DView rtv = Interlocked.Exchange(ref _mainRtv, null);
            GorgonTexture2DView main = Interlocked.Exchange(ref _main, null);
            rtv?.Dispose();
            main?.Dispose();

            if (DataContext.CurrentPanel is not null)
            {
                DataContext.CurrentPanel.PropertyChanged -= CurrentPanel_PropertyChanged;
            }

            if ((DataContext?.UpdateAnimationPreviewCommand is not null) && (DataContext.UpdateAnimationPreviewCommand.CanExecute(null)))
            {
                DataContext.UpdateAnimationPreviewCommand.Execute(null);
            }

            base.OnUnload();
        }

        /// <summary>
        /// Function called during resource creation.
        /// </summary>
        protected virtual void OnCreateResources()
        {
        }

        /// <summary>
        /// Function to zoom to the specified sprite.
        /// </summary>
        /// <param name="sprite">The sprite to zoom to.</param>
        protected void ZoomToSprite(GorgonSprite sprite)
        {
            if (sprite is null)
            {
                return;
            }

            DX.RectangleF spriteRegion = Renderer.GetAABB(sprite);
            spriteRegion.Inflate(spriteRegion.Width * 0.5f, spriteRegion.Height * 0.5f);
            DX.RectangleF originalRegion = spriteRegion;

            if (spriteRegion.Width > spriteRegion.Height)
            {
                spriteRegion.Height = spriteRegion.Width;
            }
            else
            {
                spriteRegion.Width = spriteRegion.Height;
            }

            spriteRegion.X -= spriteRegion.Width * 0.5f - originalRegion.Width * 0.5f + RenderRegion.Width * 0.5f;
            spriteRegion.Y -= spriteRegion.Height * 0.5f - originalRegion.Height * 0.5f + RenderRegion.Height * 0.5f;

            ZoomLevels spriteZoomLevel = GetNearestZoomFromRectangle(spriteRegion);

            Vector3 spritePosition = Camera.Unproject(new Vector3(spriteRegion.X + spriteRegion.Width * 0.5f, spriteRegion.Y + spriteRegion.Height * 0.5f, 0));

            MoveTo(new Vector2(spritePosition.X, spritePosition.Y), spriteZoomLevel.GetScale());
        }

        /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
        /// <param name="disposing">
        ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                Gorgon2DSilhouetteEffect effect = Interlocked.Exchange(ref _silhouette, null);
                effect?.Dispose();
            }

            base.Dispose(disposing);
        }

        /// <summary>Function to create resources required for the lifetime of the viewer.</summary>
        public void CreateResources()
        {
            _silhouette = new Gorgon2DSilhouetteEffect(Renderer);
            OnCreateResources();
        }

        /// <summary>Function to set the default zoom/offset for the viewer.</summary>
        public abstract void DefaultZoom();
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="AnimationViewer"/> class.</summary>
        /// <param name="name">The name of the renderer.</param>
        /// <param name="renderer">The main renderer for the content view.</param>
        /// <param name="swapChain">The swap chain for the content view.</param>
        /// <param name="dataContext">The view model to assign to the renderer.</param>        
        /// <param name="supportOnionSkin"><b>true</b> if the view supports onion skinning, or <b>false</b> if not.</param>
        protected AnimationViewer(string name, Gorgon2D renderer, GorgonSwapChain swapChain, IAnimationContent dataContext, bool supportOnionSkin)
            : base(name, renderer, swapChain, dataContext) => SupportsOnionSkinning = supportOnionSkin;
        #endregion
    }
}
