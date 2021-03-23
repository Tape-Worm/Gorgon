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
// Created: December 22, 2018 8:59:01 PM
// 
#endregion

using System;
using System.Numerics;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using Gorgon.Editor.Properties;
using Gorgon.Editor.Rendering;
using Gorgon.Editor.UI;
using Gorgon.Editor.UI.Views;
using Gorgon.Editor.ViewModels;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using Gorgon.Renderers;
using DX = SharpDX;

namespace Gorgon.Editor.Views
{
    /// <summary>
    /// The preview window for editor content.
    /// </summary>
    internal partial class ContentPreviewPanel
        : EditorBaseControl, IDataContext<IContentPreview>
    {
        #region Variables.
        // The swap chain for the window.
        private GorgonSwapChain _swapChain;
        // The renderer used to draw the preview image.
        private Gorgon2D _renderer;
        // The default texture to render if no preview is found.
        private GorgonTexture2DView _defaultTexture;
        // The texture used for previewing.
        private GorgonTexture2DView _previewTexture;
        // The texture to display when loading a preview.
        private GorgonTexture2DView _loadingTexture;
        // The title font.
        private GorgonFont _titleFont;
        // The text to draw.
        private GorgonTextSprite _titleText;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the application graphics context.
        /// </summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IGraphicsContext GraphicsContext
        {
            get;
            set;
        }

        /// <summary>Property to return the data context assigned to this view.</summary>
        [Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IContentPreview DataContext
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>Handles the PropertyChanged event of the DataContext control.</summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="PropertyChangedEventArgs"/> instance containing the event data.</param>        
        private void DataContext_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            switch (e.PropertyName)
            {
                case nameof(IContentPreview.IsLoading):
                    RenderImage();
                    break;
                case nameof(IContentPreview.PreviewImage):
                    UpdateImageTexture(DataContext.PreviewImage);
                    break;
                case nameof(IContentPreview.Title):
                    _titleText.Text = DataContext.Title.WordWrap(_titleFont, _swapChain.Width);
                    RenderImage();
                    break;
            }
        }

        /// <summary>
        /// Function to update the preview texture using the new image supplied.
        /// </summary>
        /// <param name="image">The image to display, or <b>null</b> to display the default.</param>
        private void UpdateImageTexture(IGorgonImage image)
        {
            _previewTexture?.Dispose();
            _previewTexture = null;

            if ((image is null) || (GraphicsContext is null) || (_swapChain is null))
            {
                RenderImage();
                return;
            }

            _previewTexture = GorgonTexture2DView.CreateTexture(GraphicsContext.Graphics, new GorgonTexture2DInfo(image.Width, image.Height, image.Format)
            {
                Name = "Content Preview",
                ArrayCount = 1,
                Binding = TextureBinding.ShaderResource,
                MipLevels = 1,
                Usage = ResourceUsage.Immutable
            }, image);

            RenderImage();
        }

        /// <summary>
        /// Function to clean up the resources for the preview window.
        /// </summary>
        private void CleanupResources()
        {
            _loadingTexture?.Dispose();
            _defaultTexture?.Dispose();

            if (_swapChain is not null)
            {
                GraphicsContext.ReturnSwapPresenter(ref _swapChain);
            }
        }

        /// <summary>
        /// Function to create the resources for the preview window.
        /// </summary>
        private void CreateResources()
        {
            _swapChain = GraphicsContext.LeaseSwapPresenter(PanelDisplay);
            _renderer = GraphicsContext.Renderer2D;
            _titleFont = GraphicsContext.FontFactory.GetFont(new GorgonFontInfo(Font.FontFamily.Name, 10.0f, FontHeightMode.Points)
            {
                Name = "PreviewTitleFont",
                OutlineSize = 2,
                OutlineColor1 = GorgonColor.Black,
                OutlineColor2 = GorgonColor.Black,
                FontStyle = Graphics.Fonts.FontStyle.Bold
            });

            _titleText = new GorgonTextSprite(_titleFont)
            {
                DrawMode = TextDrawMode.OutlinedGlyphs,
                Alignment = Gorgon.UI.Alignment.Center,
                LayoutArea = new DX.Size2F(_swapChain.Width, _swapChain.Height)
            };

            using (var stream = new MemoryStream(Resources.no_thumbnail_256x256))
            {
                _defaultTexture = GorgonTexture2DView.FromStream(GraphicsContext.Graphics, stream, new GorgonCodecDds(), options: new GorgonTexture2DLoadOptions
                {
                    Name = "DefaultPreviewTexture",
                    Binding = TextureBinding.ShaderResource,
                    Usage = ResourceUsage.Immutable
                });
            }

            using var loadingStream = new MemoryStream(Resources.LoadingBg);
            _loadingTexture = GorgonTexture2DView.FromStream(GraphicsContext.Graphics, loadingStream, new GorgonCodecDds(), options: new GorgonTexture2DLoadOptions
            {
                Name = "LoadingPreviewTexture",
                Binding = TextureBinding.ShaderResource,
                Usage = ResourceUsage.Immutable
            });
        }

        /// <summary>
        /// Function used to unassign events on the data context.
        /// </summary>
        private void UnassignEvents()
        {
            if (DataContext is null)
            {
                return;
            }

            DataContext.PropertyChanged -= DataContext_PropertyChanged;
        }

        /// <summary>
        /// Function to reset the view to its default state when the data context is reset.
        /// </summary>
        private void ResetDataContext()
        {
            _titleText.Text = string.Empty;
            _previewTexture?.Dispose();
            _previewTexture = null;
        }

        /// <summary>
        /// Function to initialize the view with the data context.
        /// </summary>
        /// <param name="dataContext">The data context being assigned.</param>
        private void InitializeFromDataContext(IContentPreview dataContext)
        {
            if (dataContext is null)
            {
                ResetDataContext();
                return;
            }

            _titleText.Text = dataContext.Title.WordWrap(_titleFont, ClientSize.Width);
            UpdateImageTexture(dataContext.PreviewImage);
        }

        /// <summary>
        /// Function to render the image into the preview area.
        /// </summary>
        private void RenderImage()
        {
            if ((IsDesignTime) || (_swapChain is null))
            {
                return;
            }

            GorgonTexture2DView image = _previewTexture ?? _defaultTexture;

            if (DataContext?.IsLoading ?? false)
            {
                image = _loadingTexture;
            }

            _swapChain.RenderTargetView.Clear(BackColor);

            GraphicsContext.Graphics.SetRenderTarget(_swapChain.RenderTargetView);

            _renderer.Begin();

            var halfClient = new Vector2(ClientSize.Width / 2.0f, ClientSize.Height / 2.0f);
            float scale = ((float)ClientSize.Width / image.Width).Min((float)ClientSize.Height / image.Height);
            float width = image.Width * scale;
            float height = image.Height * scale;
            float x = halfClient.X - (width / 2.0f);
            float y = halfClient.Y - (height / 2.0f);

            _renderer.DrawFilledRectangle(new DX.RectangleF(x, y, width, height), GorgonColor.White, image, new DX.RectangleF(0, 0, 1, 1));

            _titleText.LayoutArea = new DX.Size2F(_swapChain.Width, _titleText.Size.Height * 1.5f);
            _titleText.Position = new Vector2(0, _swapChain.Height - (_titleText.Size.Height * 1.5f).Min(_swapChain.Height * 0.25f));// _swapChain.Height - _titleText.Size.Height);

            _renderer.DrawFilledRectangle(new DX.RectangleF(0, _titleText.Position.Y, _swapChain.Width, _titleText.LayoutArea.Value.Height), new GorgonColor(0, 0, 0, 0.5f));

            _renderer.DrawTextSprite(_titleText);
            _renderer.End();

            _swapChain.Present(1);
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.Resize"/> event.</summary>
        /// <param name="e">An <see cref="System.EventArgs"/> that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            RenderImage();
        }

        /// <summary>Paints the background of the control.</summary>
        /// <param name="e">A <see cref="System.Windows.Forms.PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaintBackground(PaintEventArgs e)
        {
            base.OnPaintBackground(e);
            RenderImage();
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.Control.Paint"/> event.</summary>
        /// <param name="e">A <see cref="System.Windows.Forms.PaintEventArgs"/> that contains the event data.</param>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            RenderImage();
        }

        /// <summary>Raises the <see cref="E:System.Windows.Forms.UserControl.Load"/> event.</summary>
        /// <param name="e">An <see cref="System.EventArgs"/> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            if (IsDesignTime)
            {
                return;
            }

            CreateResources();
        }

        /// <summary>Function to assign a data context to the view as a view model.</summary>
        /// <param name="dataContext">The data context to assign.</param>
        /// <remarks>Data contexts should be nullable, in that, they should reset the view back to its original state when the context is null.</remarks>
        public void SetDataContext(IContentPreview dataContext)
        {
            UnassignEvents();

            DataContext = null;
            InitializeFromDataContext(dataContext);
            DataContext = dataContext;

            if (DataContext is null)
            {
                return;
            }

            DataContext.PropertyChanged += DataContext_PropertyChanged;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="Gorgon.Editor.Views.ContentPreview"/> class.</summary>
        public ContentPreviewPanel()
        {
            SetStyle(ControlStyles.UserPaint, true);
            SetStyle(ControlStyles.AllPaintingInWmPaint, true);
            SetStyle(ControlStyles.ResizeRedraw, true);

            InitializeComponent();
        }
        #endregion
    }
}
