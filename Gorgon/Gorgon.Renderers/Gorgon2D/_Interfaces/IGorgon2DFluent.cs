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
// Created: August 10, 2018 7:34:05 PM
// 
#endregion

using System;
using System.Collections.Generic;
using Gorgon.Core;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// A fluent interface for the <see cref="Gorgon2D"/> object.
    /// </summary>
    public interface IGorgon2DFluent
        : IDisposable, IGorgonGraphicsObject
    {
        /// <summary>
        /// Function to begin rendering a batch.
        /// </summary>
        /// <param name="batchState">[Optional] Defines common state to use when rendering a batch of objects.</param>
        /// <param name="camera">[Optional] A camera to use when rendering.</param>
        /// <returns>The fluent interface for the 2D interface.</returns>
        /// <exception cref="GorgonException">Thrown if <see cref="Begin"/> is called more than once without calling <see cref="IGorgon2DDrawingFluent.End"/>.</exception>
        /// <remarks>
        /// <para>
        /// The 2D renderer uses batching for performance. This means that drawing items with common states (e.g. blending) can all be sent to the GPU at the same time. To faciliate this, applications
        /// must call this method prior to drawing.
        /// </para>
        /// <para>
        /// When batching occurs, all drawing that shares the same state and texture will be drawn in one deferred draw call to the GPU. However, if too many items are drawn (~10,000 sprites), or the item 
        /// being drawn has a different texture than the previous item, then the batch is broken up and the previous items  will be drawn to the GPU first.  So, best practice is to ensure that everything
        /// that is drawn shares the same texture.  This is typically achieved by using a sprite sheet where multiple sprite images are pack into a single texture.
        /// </para>
        /// <para>
        /// <note type="information">
        /// <para>
        /// One exception to this is the <see cref="GorgonPolySprite"/> object, which is drawn immediately.
        /// </para>
        /// </note>
        /// </para>
        /// <para>
        /// Once rendering is done, the user must call <see cref="IGorgon2DDrawingFluent.End"/> to finalize the rendering. Otherwise, items drawn in the batch will not appear.
        /// </para>
        /// <para>
        /// This method takes an optional <see cref="Gorgon2DBatchState"/> object which allows an application to override the blend state, depth/stencil state (if applicable), rasterization state, and
        /// pixel/vertex shaders and their associated resources. This means that if an application wants to, for example, change blending modes, then a separate call to this method is required after
        /// drawing items with the previous blend state.  
        /// </para>
        /// <para>
        /// The other optional parameter, <paramref name="camera"/>, allows an application to change the view in which the items are drawn for a batch. This takes a <see cref="IGorgon2DCamera"/> object
        /// that defines the projection and view of the scene being rendered. It is possible with this object to change the coordinate system, and to allow perspective rendering for a batch.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// There are a few things to be aware of when rendering:
        /// </para>
        /// <para>
        /// <list type="bullet">
        ///     <item>
        ///         <description>Batches <b>cannot</b> be nested.  Attempting to do so will cause an exception.</description>
        ///     </item>
        ///     <item>
        ///         <description>Applications <b>must</b> call this method prior to drawing anything. Failure to do so will result in an exception.</description>
        ///     </item>
        ///     <item>
        ///         <description>Calls to <see cref="GorgonGraphics.SetRenderTarget"/>, <see cref="GorgonGraphics.SetRenderTargets"/>, <see cref="GorgonGraphics.SetDepthStencil"/>,
        /// <see cref="GorgonGraphics.SetViewport"/>, or <see cref="GorgonGraphics.SetViewports"/> while a batch is in progress is not allowed and will result in an exception if attempted.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="Gorgon2DBatchState"/>
        /// <seealso cref="IGorgon2DCamera"/>
        /// <seealso cref="GorgonPolySprite"/>
        /// <seealso cref="GorgonGraphics"/>
        IGorgon2DDrawingFluent Begin(Gorgon2DBatchState batchState = null, IGorgon2DCamera camera = null);

        /// <summary>
        /// Function to perform an arbitrary update of any required logic prior to rendering.
        /// </summary>
        /// <param name="updateMethod">A method supplied by the user to perform some custom logic on objects that need to be rendered.</param>
        /// <returns>The fluent interface for the 2D interface.</returns>
        IGorgon2DFluent Update(Action<GorgonGraphics> updateMethod);

        /// <summary>
        /// Property to return the bounds of the sprite, with transformation applied.
        /// </summary>
        /// <param name="sprite">The sprite to retrieve the boundaries from.</param>
        /// <param name="result">The measurement result.</param>
        /// <returns>The fluent interface for the 2D interface.</returns>
        /// <remarks>This is the equivalent of an axis aligned bounding box.</remarks>        
        IGorgon2DFluent MeasureSprite(GorgonPolySprite sprite, out DX.RectangleF result);

        /// <summary>
        /// Property to return the bounds of the sprite, with transformation applied.
        /// </summary>
        /// <param name="sprite">The sprite to retrieve the boundaries from.</param>
        /// <param name="result">The measurement result.</param>
        /// <returns>The fluent interface for the 2D interface.</returns>
        /// <remarks>This is the equivalent of an axis aligned bounding box.</remarks>        
        IGorgon2DFluent MeasureSprite(GorgonTextSprite sprite, out DX.RectangleF result);

        /// <summary>
        /// Property to return the bounds of the sprite, with transformation applied.
        /// </summary>
        /// <param name="sprite">The sprite to retrieve the boundaries from.</param>
        /// <param name="result">The measurement result.</param>
        /// <returns>The fluent interface for the 2D interface.</returns>
        /// <remarks>This is the equivalent of an axis aligned bounding box.</remarks>        
        IGorgon2DFluent MeasureSprite(GorgonSprite sprite, out DX.RectangleF result);
    }

    /// <summary>
    /// A fluent interface for drawing commands on a <see cref="Gorgon2D"/> object.
    /// </summary>
    public interface IGorgon2DDrawingFluent
    {
        /// <summary>
        /// Function to execute a callback method for each item in an enumerable list of items.
        /// </summary>
        /// <typeparam name="T">The type of item to draw.</typeparam>
        /// <param name="items">The list of items to enumerate through.</param>
        /// <param name="drawCommands">The callback method containing the drawing commands.</param>
        /// <returns>The fluent interface for drawing.</returns>
        /// <remarks>
        /// <para>
        /// The <paramref name="drawCommands"/> parameter is a function that supplies the current iteration number of the loop, the fluent drawing interface and returns <b>true</b> to indicate that looping 
        /// should continue, or <b>false</b> to stop looping. 
        /// </para>
        /// </remarks>
        IGorgon2DDrawingFluent DrawEach<T>(IEnumerable<T> items, Func<T, IGorgon2DDrawingFluent, bool> drawCommands);

        /// <summary>
        /// Function to evaluate an expression and if the expression is <b>true</b>, then execute a series of drawing commands contained within a callback method.
        /// </summary>
        /// <param name="expression">The expression to evaluate.</param>
        /// <param name="drawCommands">The callback method containing the drawing commands.</param>
        /// <returns>The fluent interface for drawing.</returns>
        IGorgon2DDrawingFluent DrawIf(Func<bool> expression, Action<IGorgon2DDrawingFluent> drawCommands);

        /// <summary>
        /// Function to execute a callback method containing drawing commands for the supplied amount of times.
        /// </summary>
        /// <param name="count">The number of times to loop.</param>
        /// <param name="drawCommands">The callback method containing the drawing commands.</param>
        /// <returns>The fluent interface for drawing.</returns>
        /// <para>
        /// The <paramref name="drawCommands"/> parameter is a function that supplies the current iteration number of the loop, the fluent drawing interface and returns <b>true</b> to indicate that looping 
        /// should continue, or <b>false</b> to stop looping. 
        /// </para>
        IGorgon2DDrawingFluent DrawLoop(int count, Func<int, IGorgon2DDrawingFluent, bool> drawCommands);

        /// <summary>
        /// Function to perform an arbitrary update of any required logic while rendering.
        /// </summary>
        /// <param name="updateMethod">A method supplied by the user to perform some custom logic on objects that need to be rendered.</param>
        /// <returns>The fluent interface for drawing.</returns>
        IGorgon2DDrawingFluent Update(Action<IGorgon2DDrawingFluent> updateMethod);

        /// <summary>
        /// Function to draw a polygonal sprite.
        /// </summary>
        /// <param name="sprite">The polygon sprite to draw.</param>
        /// <returns>The fluent interface for drawing.</returns>
        /// <remarks>
        /// <para>
        /// This method draws a sprite using a polygon as its surface. This is different from other sprite rendering in that:
        /// <list type="bullet">
        ///     <item>
        ///         <description>The surface is not rectangular.</description>
        ///     </item>
        ///     <item>
        ///         <description>It is not batched with other drawing types, and is drawn immediately.  This may be a performance hit.</description>
        ///     </item>
        /// </list>
        /// </para>
        /// <para>
        /// The method takes a <see cref="GorgonPolySprite"/> object which contains <see cref="GorgonPolySpriteVertex"/> objects to define the outer shape (hull) of the polygon. Gorgon will triangulate
        /// the hull into a mesh that can be rendered. 
        /// </para>
        /// <para>
        /// <see cref="GorgonPolySprite"/> objects cannot be created directly, but can be built using the <see cref="GorgonPolySpriteBuilder"/> object.  Please note that these objects implement
        /// <see cref="IDisposable"/>, so users should call the <c>Dispose</c> method when they are done with the objects.
        /// </para>
        /// <para>
        /// <note type="caution">
        /// <para>
        /// For performance reasons, any exceptions thrown from this method will only be thrown when Gorgon is compiled as DEBUG.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonPolySpriteBuilder"/>
        /// <seealso cref="GorgonPolySprite"/>
        /// <seealso cref="GorgonPolySpriteVertex"/>
        IGorgon2DDrawingFluent DrawPolygonSprite(GorgonPolySprite sprite);

        /// <summary>
        /// Function to draw a sprite.
        /// </summary>
        /// <param name="sprite">The sprite object to draw.</param>
        /// <returns>The fluent interface for drawing.</returns>
        /// <remarks>
        /// <para>
        /// This method draws a regular rectangular <see cref="GorgonSprite"/> object. 
        /// </para>
        /// <para>
        /// A <see cref="GorgonSprite"/> is a data object that provides a means to rotate, scale and translate a texture region when rendering. 
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonSprite"/>
        IGorgon2DDrawingFluent DrawSprite(GorgonSprite sprite);

        /// <summary>
        /// Function to draw text.
        /// </summary>
        /// <param name="text">The text to render.</param>
        /// <param name="position">The position of the text.</param>
        /// <param name="font">[Optional] The font to use.</param>
        /// <param name="color">[Optional] The color of the text.</param>
        /// <returns>The fluent interface for drawing.</returns>
        /// <remarks>
        /// <para>
        /// This is a convenience method that allows an application to draw text directly to the currently assigned render target.  
        /// </para>
        /// <para>
        /// If the <paramref name="font"/> parameter is not specified, then the <see cref="Gorgon2D.DefaultFont"/> is used to render the text.
        /// </para>
        /// <para>
        /// If the <paramref name="color"/> parameter is not specified, then the <see cref="GorgonColor.White"/> color is used.
        /// </para>
        /// </remarks>
        IGorgon2DDrawingFluent DrawString(string text, DX.Vector2 position, GorgonFont font = null, GorgonColor? color = null);

        /// <summary>
        /// Function to draw text.
        /// </summary>
        /// <param name="sprite">The text sprite to render.</param>
        /// <returns>The fluent interface for drawing.</returns>
        /// <remarks>
        /// <para>
        /// This method is used to draw a <see cref="GorgonTextSprite"/> to the current render target. A <see cref="GorgonTextSprite"/> is similar to a <see cref="GorgonSprite"/> in that it allows an
        /// application to take a block of text and translate, scale, and rotate the block of text.  
        /// </para>
        /// <para>
        /// Unlike the <see cref="DrawString"/> method, which just renders whatever text is sent to it, a <see cref="GorgonTextSprite"/> can also be used to align text to a boundary (e.g. center, left
        /// align, etc...). 
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonTextSprite"/>
        /// <seealso cref="GorgonSprite"/>
        IGorgon2DDrawingFluent DrawTextSprite(GorgonTextSprite sprite);

        /// <summary>
        /// Function to draw a filled rectangle.
        /// </summary>
        /// <param name="region">The region for the rectangle.</param>
        /// <param name="color">The color of the rectangle.</param>
        /// <param name="texture">[Optional] The texture for the rectangle.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the rectangle.</param>
        /// <returns>The fluent interface for drawing.</returns>
        IGorgon2DDrawingFluent DrawFilledRectangle(DX.RectangleF region,
                                                   GorgonColor color,
                                                   GorgonTexture2DView texture = null,
                                                   DX.RectangleF? textureRegion = null,
                                                   int textureArrayIndex = 0,
                                                   GorgonSamplerState textureSampler = null,
                                                   float depth = 0);

        /// <summary>
        /// Function to draw a simple triangle.
        /// </summary>
        /// <param name="point1">The vertex for the first point in the triangle.</param>
        /// <param name="point2">The vertex for the second point in the triangle.</param>
        /// <param name="point3">The vertex for the third point in the triangle.</param>
        /// <param name="texture">[Optional] The texture for the rectangle.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the rectangle.</param>
        /// <returns>The fluent interface for drawing.</returns>
        IGorgon2DDrawingFluent DrawTriangle(in GorgonTriangleVertex point1,
                                            in GorgonTriangleVertex point2,
                                            in GorgonTriangleVertex point3,
                                            GorgonTexture2DView texture = null,
                                            DX.RectangleF? textureRegion = null,
                                            int textureArrayIndex = 0,
                                            GorgonSamplerState textureSampler = null,
                                            float depth = 0);

        /// <summary>
        /// Function to draw a filled rectangle.
        /// </summary>
        /// <param name="region">The region for the rectangle.</param>
        /// <param name="color">The color of the rectangle.</param>
        /// <param name="thickness">[Optional] The line thickness.</param>
        /// <param name="texture">[Optional] The texture for the rectangle.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the rectangle.</param>
        /// <returns>The fluent interface for drawing.</returns>        
        IGorgon2DDrawingFluent DrawRectangle(DX.RectangleF region,
                                             GorgonColor color,
                                             float thickness = 1.0f,
                                             GorgonTexture2DView texture = null,
                                             DX.RectangleF? textureRegion = null,
                                             int textureArrayIndex = 0,
                                             GorgonSamplerState textureSampler = null,
                                             float depth = 0);

        /// <summary>
        /// Function to draw a line.
        /// </summary>
        /// <param name="x1">The starting horizontal position.</param>
        /// <param name="y1">The starting vertical position.</param>
        /// <param name="x2">The ending horizontal position.</param>
        /// <param name="y2">The ending vertical position.</param>
        /// <param name="color">The color of the line.</param>
        /// <param name="thickness">[Optional] The line thickness.</param>
        /// <param name="texture">[Optional] The texture to render on the line.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="startDepth">[Optional] The depth value for the starting point of the line.</param>
        /// <param name="endDepth">[Optional] The depth value for the ending point of the line.</param>
        /// <returns>The fluent interface for drawing.</returns>
        IGorgon2DDrawingFluent DrawLine(float x1,
                                        float y1,
                                        float x2,
                                        float y2,
                                        GorgonColor color,
                                        float thickness = 1.0f,
                                        GorgonTexture2DView texture = null,
                                        DX.RectangleF? textureRegion = null,
                                        int textureArrayIndex = 0,
                                        GorgonSamplerState textureSampler = null,
                                        float startDepth = 0,
                                        float endDepth = 0);

        /// <summary>
        /// Function to draw an ellipse.
        /// </summary>
        /// <param name="region">The region that will contain the ellipse.</param>
        /// <param name="color">The color of the ellipse.</param>
        /// <param name="smoothness">[Optional] The smoothness of the ellipse.</param>
        /// <param name="texture">[Optional] The texture to render on the ellipse.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the ellipse.</param>
        /// <returns>The fluent interface for drawing.</returns>
        IGorgon2DDrawingFluent DrawFilledEllipse(DX.RectangleF region,
                                                 GorgonColor color,
                                                 float smoothness = 1.0f,
                                                 GorgonTexture2DView texture = null,
                                                 DX.RectangleF? textureRegion = null,
                                                 int textureArrayIndex = 0,
                                                 GorgonSamplerState textureSampler = null,
                                                 float depth = 0);

        /// <summary>
        /// Function to draw an ellipse.
        /// </summary>
        /// <param name="region">The region that will contain the ellipse.</param>
        /// <param name="color">The color of the ellipse.</param>
        /// <param name="startAngle">The starting angle of the arc, in degrees.</param>
        /// <param name="endAngle">The ending angle of the arc, in degrees.</param>
        /// <param name="smoothness">[Optional] The smoothness of the ellipse.</param>
        /// <param name="thickness">[Optional] The ellipse line thickness.</param>
        /// <param name="texture">[Optional] The texture to render on the ellipse.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the ellipse.</param>
        /// <returns>The fluent interface for drawing.</returns>        
        IGorgon2DDrawingFluent DrawArc(DX.RectangleF region,
                                       GorgonColor color,
                                       float startAngle,
                                       float endAngle,
                                       float smoothness = 1.0f,
                                       float thickness = 1.0f,
                                       GorgonTexture2DView texture = null,
                                       DX.RectangleF? textureRegion = null,
                                       int textureArrayIndex = 0,
                                       GorgonSamplerState textureSampler = null,
                                       float depth = 0);

        /// <summary>
        /// Function to draw an ellipse.
        /// </summary>
        /// <param name="region">The region that will contain the ellipse.</param>
        /// <param name="color">The color of the ellipse.</param>
        /// <param name="startAngle">The starting angle of the arc, in degrees.</param>
        /// <param name="endAngle">The ending angle of the arc, in degrees.</param>
        /// <param name="smoothness">[Optional] The smoothness of the ellipse.</param>
        /// <param name="texture">[Optional] The texture to render on the ellipse.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the ellipse.</param>
        /// <returns>The fluent interface for drawing.</returns>
        IGorgon2DDrawingFluent DrawFilledArc(DX.RectangleF region,
                                             GorgonColor color,
                                             float startAngle,
                                             float endAngle,
                                             float smoothness = 1.0f,
                                             GorgonTexture2DView texture = null,
                                             DX.RectangleF? textureRegion = null,
                                             int textureArrayIndex = 0,
                                             GorgonSamplerState textureSampler = null,
                                             float depth = 0);

        /// <summary>
        /// Function to draw an ellipse.
        /// </summary>
        /// <param name="region">The region that will contain the ellipse.</param>
        /// <param name="color">The color of the ellipse.</param>
        /// <param name="smoothness">[Optional] The smoothness of the ellipse.</param>
        /// <param name="thickness">[Optional] The ellipse line thickness.</param>
        /// <param name="texture">[Optional] The texture to render on the ellipse.</param>
        /// <param name="textureRegion">[Optional] The texture coordinates to map to the rectangle.</param>
        /// <param name="textureArrayIndex">[Optional] The array index for a texture array to use.</param>
        /// <param name="textureSampler">[Optional] The texture sampler to apply to the texture.</param>
        /// <param name="depth">[Optional] The depth value for the ellipse.</param>
        /// <returns>The fluent interface for drawing.</returns>        
        IGorgon2DDrawingFluent DrawEllipse(DX.RectangleF region,
                                           GorgonColor color,
                                           float smoothness = 1.0f,
                                           float thickness = 1.0f,
                                           GorgonTexture2DView texture = null,
                                           DX.RectangleF? textureRegion = null,
                                           int textureArrayIndex = 0,
                                           GorgonSamplerState textureSampler = null,
                                           float depth = 0);

        /// <summary>
        /// Property to return the bounds of the sprite, with transformation applied.
        /// </summary>
        /// <param name="sprite">The sprite to retrieve the boundaries from.</param>
        /// <param name="result">The measurement result.</param>
        /// <returns>The fluent interface for drawing.</returns>        
        /// <remarks>This is the equivalent of an axis aligned bounding box.</remarks>        
        IGorgon2DDrawingFluent MeasureSprite(GorgonPolySprite sprite, out DX.RectangleF result);

        /// <summary>
        /// Property to return the bounds of the sprite, with transformation applied.
        /// </summary>
        /// <param name="sprite">The sprite to retrieve the boundaries from.</param>
        /// <param name="result">The measurement result.</param>
        /// <returns>The fluent interface for drawing.</returns>        
        /// <remarks>This is the equivalent of an axis aligned bounding box.</remarks>        
        IGorgon2DDrawingFluent MeasureSprite(GorgonTextSprite sprite, out DX.RectangleF result);

        /// <summary>
        /// Property to return the bounds of the sprite, with transformation applied.
        /// </summary>
        /// <param name="sprite">The sprite to retrieve the boundaries from.</param>
        /// <param name="result">The measurement result.</param>
        /// <returns>The fluent interface for drawing.</returns>        
        /// <remarks>This is the equivalent of an axis aligned bounding box.</remarks>        
        IGorgon2DDrawingFluent MeasureSprite(GorgonSprite sprite, out DX.RectangleF result);

        /// <summary>
        /// Function to end rendering.
        /// </summary>
        /// <returns>The <see cref="IGorgon2DFluent"/> interface to allow continuation of rendering.</returns>
        /// <remarks>
        /// <para>
        /// This finalizes rendering and flushes the current batch data to the GPU. Effectively, this is the method that performs the actual rendering for anything the user has drawn.
        /// </para>
        /// <para>
        /// The 2D renderer uses batching to achieve its performance. Because of this, we define a batch with a call to <see cref="IGorgon2DFluent.Begin"/> and <c>End</c>. So, for optimal performance, it is best to draw
        /// as much drawing as possible within the Begin/End batch body.
        /// </para>
        /// <para>
        /// This method must be paired with a call to <see cref="IGorgon2DFluent.Begin"/>, if it is not, it will do nothing. If this method is not called after a call to <see cref="IGorgon2DFluent.Begin"/>, then nothing (in most cases) 
        /// will be drawn. If a previous call to <see cref="IGorgon2DFluent.Begin"/> is made, and this method is not called, and another call to <see cref="IGorgon2DFluent.Begin"/> is made, an exception is thrown.
        /// </para>
        /// </remarks>
        IGorgon2DFluent End();
    }
}
