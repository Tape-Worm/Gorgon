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
// Created: January 9, 2019 1:59:21 PM
// 
#endregion

using System;
using DX = SharpDX;
using Gorgon.Editor.ImageEditor.ViewModels;
using Gorgon.Graphics.Core;
using System.Windows.Forms;
using Gorgon.Editor.UI;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// The service used to view the image as a texture.
    /// </summary>
    internal interface ITextureViewerService
        : IDisposable
    {
        #region Properties.
        /// <summary>
        /// Property to set or return the current zoom level.
        /// </summary>
        ZoomLevels ZoomLevel
        {
            get;
        }

        /// <summary>
        /// Property to set or return the alpha for the image.
        /// </summary>
        float Alpha
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return whether or not the viewer is in the middle of an animation.
        /// </summary>
        bool IsAnimating
        {
            get;
        }

        /// <summary>
        /// Property to set or return the current texture boundaries.
        /// </summary>
        DX.RectangleF TextureBounds
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function called when the render window changes size.
        /// </summary>
        /// <param name="image">The image to display.</param>
        void WindowResize(IImageContent image);

        /// <summary>
        /// Function to scroll the image.
        /// </summary>
        /// <param name="image">The image to scroll.</param>
        void Scroll(IImageContent image);

        /// <summary>
        /// Function to set the zoom level for the specified image.
        /// </summary>
        /// <param name="zoomLevel">The zoom level to apply.</param>
        /// <param name="image">The image to zoom.</param>
        void SetZoomLevel(ZoomLevels zoomLevel, IImageContent image);

        /// <summary>
        /// Function to indicate that the current animation (if one is playing) should end.
        /// </summary>
        void EndAnimation();

        /// <summary>
        /// Function to update the texture parameters for rendering.
        /// </summary>
        /// <param name="image">The image to retrieve the parameters from.</param>
        void UpdateTextureParameters(IImageContent image);

        /// <summary>
        /// Function to draw the texture.
        /// </summary>
        void Draw(IImageContent image);

        /// <summary>
        /// Function to retrieve the size, in pixels, if the current mip level.
        /// </summary>
        /// <param name="image">The image to retrieve mip information from.</param>
        /// <returns>The width and height of the mip level.</returns>
        DX.Size2 GetMipSize(IImageContent image);

        /// <summary>
        /// Function to create the resources required for the viewer.
        /// </summary>
        /// <param name="backgroundImage">The image used for display in the background.</param>
        void CreateResources(GorgonTexture2DView backgroundImage);

        /// <summary>
        /// Function to update the texture.
        /// </summary>
        /// <param name="image">The image to upload to the texture.</param>
        void UpdateTexture(IImageContent image);

        /// <summary>
        /// Function called when the mouse is moved.
        /// </summary>
        /// <param name="x">The horizontal position of the mouse.</param>
        /// <param name="y">The vertical position of the mouse.</param>
        /// <param name="buttons">The button(s) held down while moving.</param>
        /// <param name="image">The current image.</param>
        void MouseMove(int x, int y, MouseButtons buttons, IImageContent image);

        /// <summary>
        /// Function called when a button on the mouse is held down.
        /// </summary>
        /// <param name="x">The horizontal position of the mouse.</param>
        /// <param name="y">The vertical position of the mouse.</param>
        /// <param name="buttons">The button(s) held down.</param>
        /// <param name="image">The current image.</param>
        void MouseDown(int x, int y, MouseButtons buttons, IImageContent image);

        /// <summary>
        /// Function called when a button the on mouse is released.
        /// </summary>
        /// <param name="x">The horizontal position of the mouse.</param>
        /// <param name="y">The vertical position of the mouse.</param>
        /// <param name="buttons">The button(s) released.</param>
        /// <param name="image">The current image.</param>
        void MouseUp(int x, int y, MouseButtons buttons, IImageContent image);
        #endregion
    }
}