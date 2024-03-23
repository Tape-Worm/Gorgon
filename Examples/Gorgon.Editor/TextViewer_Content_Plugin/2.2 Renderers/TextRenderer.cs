
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
// Created: August 3, 2020 4:40:15 PM
// 

using System.Numerics;
using Gorgon.Editor.Rendering;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Renderers;

namespace Gorgon.Examples;

/// <summary>
/// This is a renderer that will print our text into the view
/// </summary>
/// <remarks>
/// A renderer is used to convey any graphical/visual content data back to the user. Changes made to the content are reflected in the renderer by monitoring
/// the content view model and adjusting the internal content representation used by the renderer
/// 
/// The renderer works differently than our content control in that it does not wait for events to update itself, it is constantly running using an idle loop 
/// to reflect content changes in real time
/// </remarks>
/// <remarks>Initializes a new instance of the <see cref="TextRenderer"/> class.</remarks>
/// <param name="renderer">The 2D renderer used to render our text.</param>
/// <param name="mainRenderTarget">The main render target for the view.</param>
/// <param name="fonts">The factory used to create fonts.</param>
/// <param name="dataContext">The view model for our text data.</param>
internal class TextRenderer(Gorgon2D renderer, GorgonSwapChain mainRenderTarget, GorgonFontFactory fonts, ITextContent dataContext)
        : DefaultContentRenderer<ITextContent>("TextRenderer", renderer, mainRenderTarget, dataContext)
{

    // The sprite used to render our text data.
    private GorgonTextSprite _textSprite;
    // The factory used to create fonts.
    private readonly GorgonFontFactory _fontFactory = fonts;
    // The fonts used for text rendering.
    private GorgonFont _arial;
    private GorgonFont _timesNewRoman;
    private GorgonFont _papyrus;

    /// <summary>
    /// Function to update the font for the text sprite.
    /// </summary>
    private void ChangeFont()
    {
        _textSprite.Color = DataContext.Color;

        switch (DataContext.FontFace)
        {
            case FontFace.TimesNewRoman:
                _textSprite.Font = _timesNewRoman;
                break;
            case FontFace.Papyrus:
                _textSprite.Font = _papyrus;

                if (_textSprite.Color == _papyrus.OutlineColor1)
                {
                    // Reset to white if our outline color matches our text color.
                    // If we fail to do this, the text will be unreadable.
                    _textSprite.Color = GorgonColors.White;
                }
                break;
            default:
                _textSprite.Font = _arial;
                break;
        }

        _textSprite.Text = DataContext.Text.WordWrap(_textSprite.Font, RenderRegion.Width);

    }

    /// <summary>Handles the PropertyChanged event of the TextColor control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="System.ComponentModel.PropertyChangedEventArgs"/> instance containing the event data.</param>
    private void TextColor_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(ITextColor.SelectedColor):
                _textSprite.Color = DataContext.TextColor.SelectedColor;
                break;
        }
    }

    /// <summary>Releases unmanaged and - optionally - managed resources.</summary>
    /// <param name="disposing">
    ///   <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _papyrus?.Dispose();
            _timesNewRoman?.Dispose();
            _arial?.Dispose();
        }

        base.Dispose(disposing);
    }

    /// <summary>Function called when a property on the <see cref="DefaultContentRenderer{T}.DataContext"/> has been changed.</summary>
    /// <param name="propertyName">The name of the property that was changed.</param>
    /// <remarks>Developers should override this method to detect changes on the content view model and reflect those changes in the rendering.</remarks>
    protected override void OnPropertyChanged(string propertyName)
    {
        base.OnPropertyChanged(propertyName);

        // This method montiors for changes on the view model and updates our rendering accordingly.
        // We do this by intercepting the name of the property that was changed, and updating the 
        // specific functionality of the renderer.
        switch (propertyName)
        {
            case nameof(ITextContent.FontFace):
                // If the view model font property is changed, we need to update our text sprite.
                // So we'll call this method that will update our text sprite with the selected 
                // font.
                //
                // We'll also reset the zoom so we can see the text in case we're zoomed in too far.
                ChangeFont();
                DefaultZoom();
                break;
            case nameof(ITextContent.Text):
                // If we alter the text on the view model, reflect that here now.
                _textSprite.Text = DataContext.Text.WordWrap(_textSprite.Font, RenderRegion.Width);
                break;
            case nameof(ITextContent.Color):
                // If we change the color on the view model, then update our text sprite color.
                _textSprite.Color = DataContext.Color;

                if (_textSprite.Color == _papyrus.OutlineColor1)
                {
                    // Reset to white if our outline color matches our text color.
                    // If we fail to do this, the text will be unreadable.
                    _textSprite.Color = GorgonColors.White;
                }
                break;
        }
    }

    /// <summary>Function to render the background.</summary>
    /// <remarks>Developers can override this method to render a custom background.</remarks>
    protected override void OnRenderBackground()
    {
        // This method is called before rendering the content. We use this to draw any custom backrgound that we wish. 
        //
        // If this method is not overridden, or we call back to the base method, a pattern will be drawn in the 
        // background.
        //
        // In this case, we want the background to zoom/pan along with our content, so we pass in the renderer Camera
        // to the renderer so that it can be affected by the camera movement.                         
        Renderer.Begin(camera: Camera);
        GorgonRectangleF backgroundRegion = new(RenderRegion.Width * -0.5f + 10, RenderRegion.Height * -0.5f + 10, RenderRegion.Width, RenderRegion.Height);
        Renderer.DrawFilledRectangle(backgroundRegion, GorgonColors.Black);
        backgroundRegion.X -= 10;
        backgroundRegion.Y -= 10;
        Renderer.DrawFilledRectangle(backgroundRegion, GorgonColors.White);
        Renderer.End();
    }

    /// <summary>Function to render the content.</summary>
    /// <remarks>This is the method that developers should override in order to draw their content to the view.</remarks>
    protected override void OnRenderContent()
    {
        // This method is called during application idle time for every frame. So this is where we'll draw our content.

        // When rendering our text, we may end up rendering over the edge of our rendering area (especially if the text is large or long).
        // The best way to handle this is to provide a scissor rectangle to clip against the edges of the rendering area.
        // 
        // Setting the clipping rectangle as-is will work when we're at 100% zoom, and centered.  However, when we move the rendering area
        // around by zooming/panning the clipping area will no longer apply. This is because the clip area is contained within client area 
        // space (basically the client space of the control), and this is not affected by our camera.
        //
        // To remedy this we will set the clip area to camera space (in this case, that would be (-Region Width / 2, -Region Height / 2) -
        // (Region Width / 2, Region Height / 2), but this depends on the camera setup). After this we'll call a utility function in the 
        // renderer to convert camera space into client space (ToClient). This will then allow us to set the clipping rectangle based on 
        // our camera position and zoom.
        GorgonRectangleF cameraSpaceRegion = new(RenderRegion.Width * -0.5f, RenderRegion.Height * -0.5f, RenderRegion.Width, RenderRegion.Height);
        GorgonRectangle clientSpaceRegion = (GorgonRectangle)ToClient(cameraSpaceRegion); // The scissor rectangle must be in integer coordinates, ToRectangle will give us this.

        // Set the scissor rectangle. Ideally we'd be doing this when the camera moves/zooms or the client size changes instead of every 
        // frame (way more efficient), but for the purposes of our example, we'll just set it per frame.
        Graphics.SetScissorRect(clientSpaceRegion);

        // When we render, we use the scissor clipping render state, and our built-in rendering camera to allow us to zoom and pan.
        Renderer.Begin(Gorgon2DBatchState.ScissorClipping, Camera);
        Renderer.DrawTextSprite(_textSprite);
        Renderer.End();

        // Turn off clipping so we don't affect anything else.
        Graphics.SetScissorRect(GorgonRectangle.Empty);
    }

    /// <summary>Function called when the renderer needs to load any resource data.</summary>
    /// <remarks>
    /// Developers can override this method to set up their own resources specific to their renderer. Any resources set up in this method should be cleaned up in the associated
    /// <see cref="DefaultContentRenderer{T}.OnUnload"/> method.
    /// </remarks>
    protected override void OnLoad()
    {
        base.OnLoad();

        // This method is called whenever the renderer is switched, and has a corresponding OnUnload method for cleanup.
        // In more complicated scenarios, one renderer doesn't really cut it, so we need to switch between them depending 
        // on the context of our editor operation. However, we don't want transitory resources to stick around when they
        // are not being used, so we use the OnUnload to destroy them, and OnLoad to create them. 
        //
        // Because of this, this method can be called many times over the lifetime of the application.

        // This is also the ideal place to set your rendering region.  This region defines where the limits of the content 
        // display will be. Here, we've set it to the same size as the client (minus 32 pixels on either side). 
        // By default it is set to the entire client area size of the view.
        RenderRegion = new GorgonRectangleF(0, 0, ClientSize.X - 64, ClientSize.Y - 64);

        // We deal in camera space for our content (so we can zoom and pan). The base renderer sets up a camera for this 
        // purpose, which is automatically updated for zooming and panning our content. The camera space view area can be 
        // changed to whatever the user desires, but by default it uses the render region width and height, and 0x0 is 
        // in the center of the region (hence why we are positioning the text at negstive values, which correspond to the
        // upper left coordinates of the render region).
        _textSprite.Position = new Vector2(RenderRegion.Width * -0.5f, RenderRegion.Height * -0.5f);
        _textSprite.LayoutArea = RenderRegion.Size;

        ChangeFont();

        DataContext.TextColor.PropertyChanged += TextColor_PropertyChanged;
    }

    /// <summary>
    /// Function called when the renderer needs to clean up any resource data.
    /// </summary>
    /// <remarks>Developers should always override this method if they've overridden the <see cref="DefaultContentRenderer{T}.OnLoad" /> method. Failure to do so can cause memory leakage.</remarks>
    protected override void OnUnload()
    {
        DataContext.TextColor.PropertyChanged -= TextColor_PropertyChanged;

        base.OnUnload();
    }

    /// <summary>Function to create resources required for the lifetime of the viewer.</summary>
    public void CreateResources()
    {
        _arial = _fontFactory.GetFont(new GorgonFontInfo("Arial", 9.0f, GorgonFontHeightMode.Points)
        {
            Name = "Arial 9pt",
            FontStyle = GorgonFontStyle.Bold
        });

        _timesNewRoman = _fontFactory.GetFont(new GorgonFontInfo("Times New Roman", 18.0f, GorgonFontHeightMode.Points)
        {
            Name = "Times New Roman 18pt"
        });

        _papyrus = _fontFactory.GetFont(new GorgonFontInfo("Papyrus", 20.0f, GorgonFontHeightMode.Points)
        {
            Name = "Papyrus 20pt",
            OutlineColor1 = GorgonColors.Black,
            OutlineColor2 = GorgonColors.Black,
            OutlineSize = 2
        });

        _textSprite = new GorgonTextSprite(_timesNewRoman)
        {
            AllowColorCodes = true,
            Position = new Vector2(RenderRegion.Width * -0.5f, RenderRegion.Height * -0.5f),
            LayoutArea = RenderRegion.Size,
            Color = GorgonColors.Black,
            TextureSampler = GorgonSamplerState.PointFiltering,
            DrawMode = TextDrawMode.OutlinedGlyphs
        };
    }

    /// <summary>
    /// Function to set the view to a default zoom level.
    /// </summary>
    public void DefaultZoom() => MoveTo(Vector2.Zero, 1);

}
