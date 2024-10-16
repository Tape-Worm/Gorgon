﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 24, 2018 2:33:28 PM
// 

using System.Numerics;
using Gorgon.Animation;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Renderers;
using Gorgon.UI;

namespace Gorgon.Examples;

/// <summary>
/// Main application form
/// </summary>
public partial class Form
    : System.Windows.Forms.Form
{

    // Our primary graphics interface.
    private GorgonGraphics _graphics;
    // The swap chain for the left panel.
    private GorgonSwapChain _leftPanel;
    // The swap chain for the right panel.
    private GorgonSwapChain _rightPanel;
    // The 2D renderer to use.
    private Gorgon2D _renderer;
    // The font for the application.
    private GorgonFont _appFont;
    // The texture containing the animation frames.
    private GorgonTexture2DView _torusTexture;
    // The left torus to animate.
    private GorgonSprite _torusLeft;
    // The right torus to animate.
    private GorgonSprite _torusRight;
    // The left animation for the torus.
    private IGorgonAnimation _torusAnim;
    // The controller for the animation on the left.
    private GorgonSpriteAnimationController _controllerLeft;
    // The controller for the animation on the right.
    private GorgonSpriteAnimationController _controllerRight;
    // The scale of the left sprite.
    private Vector2 _scale = new(2, 2);
    // The original size of the left panel.
    private Vector2 _originalSize;

    /// <summary>
    /// Handles the SplitterMoved event of the SplitViews control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="SplitterEventArgs"/> instance containing the event data.</param>
    private void SplitViews_SplitterMoved(object sender, SplitterEventArgs e)
    {
        if (_graphics is null)
        {
            return;
        }

        _scale = new Vector2((GroupControl1.ClientSize.Width / _originalSize.X) * 2, (GroupControl1.ClientSize.Width / _originalSize.X) * 2);
    }

    /// <summary>
    /// Handles the Click event of the ButtonAnimation control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
    private void ButtonAnimation_Click(object sender, EventArgs e)
    {
        if (_controllerRight.State != AnimationState.Playing)
        {
            _controllerRight.Resume();
        }
        else
        {
            _controllerRight.Pause();
        }
    }

    /// <summary>
    /// Function to build up an animation for the torus.
    /// </summary>
    private void BuildAnimation()
    {
        GorgonAnimationBuilder builder = new();

        IGorgonTrackKeyBuilder<GorgonKeyTexture2D> track = builder.Edit2DTexture("Texture");

        float time = 0;
        int frameCount = 0;
        float frameTime = 1 / 30f;
        for (int y = 0; y < _torusTexture.Height && frameCount < 60; y += 64)
        {
            for (int x = 0; x < _torusTexture.Width && frameCount < 60; x += 64, frameCount++)
            {
                GorgonRectangleF texCoords = _torusTexture.ToTexel(new GorgonRectangle(x, y, 64, 64));

                track.SetKey(new GorgonKeyTexture2D(time, _torusTexture, texCoords, 0));

                // 30 FPS.
                time += frameTime;
            }
        }

        track.EndEdit();

        _torusAnim = builder.Build("Torus Animation", 30);
        _torusAnim.IsLooped = true;

        _controllerLeft = new GorgonSpriteAnimationController();
        _controllerRight = new GorgonSpriteAnimationController();

        _controllerLeft.Play(_torusLeft, _torusAnim);
        _controllerRight.Play(_torusRight, _torusAnim);
        _controllerRight.Pause();
    }

    /// <summary>
    /// Function to initialize the example.
    /// </summary>
    private void Initialize()
    {
        GorgonExample.ResourceBaseDirectory = new DirectoryInfo(ExampleConfig.Default.ResourceLocation);

        IReadOnlyList<IGorgonVideoAdapterInfo> adapters = GorgonGraphics.EnumerateAdapters(log: GorgonApplication.Log);

        if (adapters.Count == 0)
        {
            throw new GorgonException(GorgonResult.CannotCreate, "This example requires a Direct3D 11.2 capable video card.\nThe application will now close.");
        }

        _graphics = new GorgonGraphics(adapters[0]);

        _leftPanel = new GorgonSwapChain(_graphics,
                                         GroupControl1,
                                         new GorgonSwapChainInfo(GroupControl1.ClientSize.Width, GroupControl1.ClientSize.Height, BufferFormat.R8G8B8A8_UNorm)
                                         {
                                             Name = "Left Panel SwapChain"
                                         });

        _rightPanel = new GorgonSwapChain(_graphics,
                                          GroupControl2,
                                          new GorgonSwapChainInfo(_leftPanel, "Right Panel SwapChain")
                                          {
                                              Width = GroupControl2.ClientSize.Width,
                                              Height = GroupControl2.ClientSize.Height
                                          });

        _renderer = new Gorgon2D(_graphics);

        _torusTexture = GorgonTexture2DView.FromFile(_graphics,
                                                     Path.Combine(GorgonExample.GetResourcePath(@"Textures\GiveMeSomeControl\").FullName, "Torus.png"),
                                                     new GorgonCodecPng(),
                                                     new GorgonTexture2DLoadOptions
                                                     {
                                                         Binding = TextureBinding.ShaderResource,
                                                         Name = "Torus Animation Sheet",
                                                         Usage = ResourceUsage.Immutable
                                                     });

        _torusLeft = new GorgonSprite
        {
            Anchor = new Vector2(0.5f, 0.5f),
            Size = new Vector2(64, 64),
            TextureSampler = GorgonSamplerState.PointFiltering
        };
        _torusRight = new GorgonSprite
        {
            Anchor = new Vector2(0.5f, 0.5f),
            Size = new Vector2(64, 64),
            TextureSampler = GorgonSamplerState.PointFiltering
        };

        BuildAnimation();

        GorgonExample.LoadResources(_graphics);

        _appFont = GorgonExample.Fonts.GetFont(new GorgonFontInfo(Font.FontFamily.Name, Font.Size * 1.33333f, GorgonFontHeightMode.Points)
        {
            Name = "Form Font",
            Characters = "SpdtoxDrag me!\u2190:1234567890.",
            TextureWidth = 128,
            TextureHeight = 128,
            OutlineSize = 2,
            FontStyle = GorgonFontStyle.Bold,
            OutlineColor1 = GorgonColors.Black,
            OutlineColor2 = GorgonColors.Black
        });
    }

    /// <summary>
    /// Function to process the example functionality during idle time.
    /// </summary>
    /// <returns><b>true</b> to continue, <b>false</b> to stop.</returns>
    private bool Idle()
    {
        _leftPanel.RenderTargetView.Clear(GroupControl1.BackColor);
        _rightPanel.RenderTargetView.Clear(GroupControl2.BackColor);

        _graphics.SetRenderTarget(_leftPanel.RenderTargetView);

        _renderer.Begin();
        _torusLeft.Scale = _scale;
        _torusLeft.Position = new Vector2(_leftPanel.Width / 2.0f, _leftPanel.Height / 2.0f);
        _renderer.DrawSprite(_torusLeft);
        _renderer.End();

        GorgonExample.DrawStatsAndLogo(_renderer);

        _graphics.SetRenderTarget(_rightPanel.RenderTargetView);

        _renderer.Begin();
        _torusLeft.Scale = Vector2.One;

        _torusRight.Color = GorgonColors.Red;
        _torusRight.Position = new Vector2((_rightPanel.Width / 2.0f) - 64, (_rightPanel.Height / 2.0f) - 64);
        _renderer.DrawSprite(_torusRight);

        _torusRight.Color = GorgonColors.Green;
        _torusRight.Position = new Vector2((_rightPanel.Width / 2.0f) + 64, (_rightPanel.Height / 2.0f) - 64);
        _renderer.DrawSprite(_torusRight);

        _torusRight.Color = GorgonColors.Blue;
        _torusRight.Position = new Vector2((_rightPanel.Width / 2.0f) - 64, (_rightPanel.Height / 2.0f) + 64);
        _renderer.DrawSprite(_torusRight);

        _torusRight.Color = GorgonColors.White;
        _torusRight.Position = new Vector2((_rightPanel.Width / 2.0f) + 64, (_rightPanel.Height / 2.0f) + 64);
        _renderer.DrawSprite(_torusRight);

        _renderer.DrawString("\u2190Drag me!", new Vector2(0, _rightPanel.Height / 4.0f), _appFont, GorgonColors.White);

        if (_controllerRight.State != AnimationState.Playing)
        {
            _renderer.DrawString("Speed: Stopped", new Vector2(0, 64), _appFont, GorgonColors.White);
        }
        else
        {
            _renderer.DrawString($"Speed: {TrackSpeed.Value / 5.0f:0.0#}", new Vector2(0, 64), _appFont, GorgonColors.White);
        }

        _renderer.End();

        GorgonExample.DrawStatsAndLogo(_renderer);

        _leftPanel.Present(1);
        _rightPanel.Present(1);

        _torusAnim.Speed = -1.0f;
        _controllerLeft.Update();

        if (_controllerRight.State != AnimationState.Playing)
        {
            _controllerRight.Resume();
            _controllerRight.Time = (TrackSpeed.Value / 10.0f) * _torusAnim.Length;
            _controllerRight.Pause();
        }
        else
        {
            _torusAnim.Speed = TrackSpeed.Value / 5.0f;
            _controllerRight.Update();
        }

        return true;
    }

    /// <summary>Raises the <see cref="E:System.Windows.Forms.Form.FormClosing" /> event.</summary>
    /// <param name="e">A <see cref="FormClosingEventArgs" /> that contains the event data. </param>
    protected override void OnFormClosing(FormClosingEventArgs e)
    {
        base.OnFormClosing(e);

        GorgonExample.UnloadResources();

        _renderer?.Dispose();
        _leftPanel?.Dispose();
        _rightPanel?.Dispose();
        _graphics?.Dispose();
    }

    /// <summary>Raises the <see cref="E:System.Windows.Forms.Form.Load" /> event.</summary>
    /// <param name="e">An <see cref="EventArgs" /> that contains the event data. </param>
    protected override void OnLoad(EventArgs e)
    {
        base.OnLoad(e);

        try
        {
            Show();

            Cursor.Current = Cursors.WaitCursor;

            Initialize();

            _originalSize = new Vector2(GroupControl1.ClientSize.Width, GroupControl2.ClientSize.Height);

            GorgonApplication.IdleMethod = Idle;
        }
        catch (Exception ex)
        {
            GorgonExample.HandleException(ex);
            GorgonApplication.Quit();
        }
        finally
        {
            Cursor.Current = Cursors.Default;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Form"/> class.
    /// </summary>
    public Form() => InitializeComponent();

}
