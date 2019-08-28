#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: March 4, 2017 12:47:54 PM
// 
#endregion

using System;
using System.Windows.Forms;
using Gorgon.Examples;
using Gorgon.UI;

namespace Gorgon.Graphics.Example
{
    /// <summary>
    /// Glass cube example program.
    /// 
    /// This example introduces some animation, a texured 3D cube, and handling resizing of the swap chain.  
    /// 
    /// For the animation, we give our cube a world matrix which defines how to transform the vertices of the cube in world space. This, like the projection and view matrix is transmitted 
    /// to the GPU via a constant buffer by updating the matrix, and multiplying it by the world and view matrix once per frame. Once the transformation matrix is combined, we then upload
    /// it to the constant buffer and the vertex shader will then transform the vertices for the cube accordingly.
    /// 
    /// Resizing the swap chain is optional, but if Gorgon is told not to handle the buffer resizing (like in other examples), the results can be less than ideal. When the swap chain is 
    /// configured with StretchBackBuffer = False, then the contents of the back buffer are stretched when presented to the front buffer surface. This can give a blurry look if the back 
    /// buffer is smaller than the window size. To mitigate this, we set the StretchBackBuffer = True (which is the default). 
    /// 
    /// But, when we resize the window we lose our cube! Why does this happen? Well, in order to resize the swap chain back buffers the swap chain must be removed from the graphics 
    /// pipeline (that is, unset) as our active render target. Gorgon will happily do this for us by disposing of the render target view and render target texture from the swap chain when 
    /// resizing. However, it will not remember that the view/texture were assigned as the current render target (we could do this, but it causes complications in more complex scenarios 
    /// like multi-monitor rendering). So, to counter this, a swap chain has a Before/After swap chain resized event that we can hook into. In the after resize event, we just merely 
    /// reassign the swap chain as the render target in the draw call. We also must resize the view port and the projection matrix to match our window (and assign it to the draw call), or 
    /// else things will not render in the correct position or size within the window.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            try
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);

                GorgonApplication.Run(new Form());
            }
            catch (Exception ex)
            {
                GorgonExample.HandleException(ex);
            }
        }
    }
}
