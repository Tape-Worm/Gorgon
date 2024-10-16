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
// Created: August 25, 2018 12:04:13 PM
// 

using System.Numerics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;

namespace Gorgon.Examples;

/// <summary>
/// Builds the shadows for our application
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="ShadowBuilder"/> class
/// </remarks>
/// <param name="renderer">The renderer.</param>
/// <param name="effect">The gaussian blur effect to use in order to soften the shadows.</param>
/// <param name="sprite1">The first sprite to draw.</param>
/// <param name="sprite2">The second sprite to draw.</param>
public class ShadowBuilder(Gorgon2D renderer, Gorgon2DGaussBlurEffect effect, GorgonSprite sprite1, GorgonSprite sprite2)
{

    // The renderer.
    private readonly Gorgon2D _renderer = renderer;
    // Our gaussian blur effect.
    private readonly Gorgon2DGaussBlurEffect _gaussBlur = effect;
    // Our sprites to draw.
    private readonly GorgonSprite _sprite1 = sprite1;
    private readonly GorgonSprite _sprite2 = sprite2;

    /// <summary>
    /// Function to build the shadows for the sprites.
    /// </summary>
    /// <returns>A tuple containing the sprites used for our shadows and the texture containing the shadowed sprites.</returns>
    public (GorgonSprite[] shadowSprites, GorgonTexture2DView shadowTexture) Build()
    {
        // To build our shadow, we create a render target that can fit the 2 sprites we have. The size must be a bit larger because the blur will creep outside of the actual 
        // sprite boundaries.
        // Once we have our render target, we draw our sprites to the render target as black (this way all color information is removed).
        // Then, we apply a gaussian blur to the render target several times to "smooth" out the edges.
        // Once that is done, we can create new sprites from our render target texture that point to the blurred image.

        GorgonTexture2DView resultTexture = GorgonTexture2DView.CreateTexture(_renderer.Graphics,
                                                                              new GorgonTexture2DInfo((int)(_sprite1.Size.X + _sprite2.Size.X) * 2,
                                                                                                           (int)(_sprite1.Size.Y + _sprite2.Size.Y) * 2,
                                                                                                           BufferFormat.R8G8B8A8_UNorm)
                                                                              {
                                                                                  Name = "Shadow Texture",
                                                                                  Binding = TextureBinding.RenderTarget | TextureBinding.ShaderResource,
                                                                                  Usage = ResourceUsage.Default
                                                                              });

        using (GorgonRenderTarget2DView rtv = resultTexture.GetRenderTargetView())
        {
            _renderer.Graphics.SetRenderTarget(rtv);
            _renderer.Begin();

            _sprite1.Position = new Vector2(rtv.Width / 4.0f, rtv.Height / 2.0f);
            _sprite1.Color = GorgonColors.Black;

            _sprite2.Position = new Vector2(rtv.Width - (rtv.Width / 4.0f), rtv.Height / 2.0f);
            _sprite2.Color = GorgonColors.Black;

            _renderer.DrawSprite(_sprite1);
            _renderer.DrawSprite(_sprite2);

            _renderer.End();

            for (int i = 0; i < 8; ++i)
            {
                _gaussBlur.Render(resultTexture, rtv);
            }

            // Reset our colors.
            _sprite1.Color = GorgonColors.White;
            _sprite2.Color = GorgonColors.White;
        }

        GorgonSprite[] resultSprites =
        [
            new GorgonSprite
            {
                Texture = resultTexture,
                Size = new Vector2(6 + _sprite1.Size.X, 6 + _sprite1.Size.Y),
                Color = new GorgonColor(GorgonColors.White, 0.85f),
                Anchor = new Vector2(0.5f, 0.5f),
                TextureRegion = resultTexture.ToTexel(new GorgonRectangle((int)(_sprite1.Position.X - (_sprite1.Size.X / 2) - 10),
                                                                       (int)(_sprite1.Position.Y - (_sprite1.Size.Y / 2) - 10),
                                                                       (int)_sprite1.Size.X + 20,
                                                                       (int)_sprite1.Size.Y + 20))
            },
            new GorgonSprite
            {
                Texture = resultTexture,
                Size = new Vector2(6 + _sprite2.Size.X, 6 + _sprite2.Size.Y),
                Color = new GorgonColor(GorgonColors.White, 0.85f),
                Anchor = new Vector2(0.5f, 0.5f),
                TextureRegion = resultTexture.ToTexel(new GorgonRectangle((int)(_sprite2.Position.X - (_sprite2.Size.X / 2) - 10),
                                                                       (int)(_sprite2.Position.Y - (_sprite2.Size.Y / 2) - 10),
                                                                       (int)_sprite2.Size.X + 20,
                                                                       (int)_sprite2.Size.Y + 20))
            }
        ];

        return (resultSprites, resultTexture);
    }
}
