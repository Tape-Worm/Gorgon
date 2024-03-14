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
// Created: August 20, 2018 1:30:41 PM
// 
#endregion

using System.Numerics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Cameras;
using DX = SharpDX;

namespace Gorgon.Renderers;

/// <summary>
/// A controller used to update a camera.
/// </summary>
internal class CameraController
    : IDisposable, IGorgonGraphicsObject
{
    #region Variables.
    // The view * projection matrix.
    private Matrix4x4 _viewProjectionMatrix = Matrix4x4.Identity;
    // The current camera that has had its data uploaded to the GPU.
    private GorgonCameraCommon _current;
    // The buffer for holding the camera GPU data.
    private GorgonConstantBufferView _cameraBuffer;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the buffer used for the GPU camera data.
    /// </summary>
    public GorgonConstantBufferView CameraBuffer => _cameraBuffer;

    /// <summary>
    /// Property to return the graphics interface that built this object.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    }
    #endregion

    #region Methods.
    /// <summary>
    /// Function to update the camera data on the GPU.
    /// </summary>
    /// <param name="camera">The camera to update.</param>
    public void UpdateCamera(GorgonCameraCommon camera)
    {
        if (camera.AllowUpdateOnResize)
        {
            var viewportSize = new DX.Size2F(Graphics.Viewports[0].Width, Graphics.Viewports[0].Height);

            if (!camera.ViewDimensions.Equals(viewportSize))
            {
                camera.ViewDimensions = viewportSize;
            }
        }

        bool camChanged = (_current != camera) || (camera.Changes != CameraChange.None);

        if (!camChanged)
        {
            return;
        }

        // Build the view/projection matrix.
        ref readonly Matrix4x4 viewMatrix = ref camera.GetViewMatrix();
        ref readonly Matrix4x4 projMatrix = ref camera.GetProjectionMatrix();
        _viewProjectionMatrix = Matrix4x4.Multiply(viewMatrix, projMatrix);            
        CameraBuffer.Buffer.SetData(in _viewProjectionMatrix);

        _current = camera;
    }

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public void Dispose()
    {
        GorgonConstantBufferView buffer = Interlocked.Exchange(ref _cameraBuffer, null);
        buffer?.Dispose();
    }
    #endregion

    #region Constructor/Finalizer.
    /// <summary>
    /// Initializes a new instance of the <see cref="CameraController"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface used for the camera buffer data.</param>
    public CameraController(GorgonGraphics graphics)
    {
        Graphics = graphics;
        _cameraBuffer = GorgonConstantBufferView.CreateConstantBuffer(graphics,
                                                                      in _viewProjectionMatrix,
                                                                      "Gorgon 2D Camera Constant Buffer",
                                                                      ResourceUsage.Dynamic);
    }
    #endregion
}
