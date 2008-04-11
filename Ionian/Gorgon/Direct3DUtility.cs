#region LGPL.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Saturday, March 15, 2008 2:00:58 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using GorgonLibrary.Graphics;

namespace GorgonLibrary.Internal
{
    /// <summary>
    /// Object used to interface to internal Gorgon D3D objects.
    /// </summary>
    /// <remarks>This is meant for plug-in developers to use as an entry point to gain full access to Gorgon's D3D internals (i.e. if a developer wanted to merge 3D stuff with Gorgon, they'd use this class to gain access to everything).
    /// <para>Since this is strictly for plug-ins, this class can only be accessed from the <see cref="GorgonLibrary.PlugIns.PlugInEntryPoint">PlugInEntryPoint</see> superclass as the protected method <see cref="GorgonLibrary.PlugIns.PlugInEntryPoint.GetD3DObjects">GetD3DObjects()</see>.</para>
    /// <para>BE VERY CAREFUL WITH THIS OBJECT!  It cannot be stressed enough, you will be messing with the guts of Gorgon and as such you can easily cause it to crash by doing something wrong.</para></remarks>
    public class D3DObjects
        : IDeviceStateObject, IDisposable
    {
        #region Variables.
        private bool _disposed = false;             // Flag to indicate that the object is disposed.
        #endregion

        #region Events.
        /// <summary>
        /// Event fired during a device lost state.
        /// </summary>
        public event EventHandler DeviceLostState;

        /// <summary>
        /// Event fired during a device reset state.
        /// </summary>
        public event EventHandler DeviceResetState;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return whether the device needs a reset or not.
        /// </summary>
        public bool DeviceNeedsReset
        {
            get
            {
                return Gorgon.Screen.DeviceNotReset;
            }
        }

        /// <summary>
        /// Property to return the Direct 3D device object used by Gorgon.
        /// </summary>
        public D3D9.Device Device
        {
            get
            {
                return Gorgon.Screen.Device;
            }
        }        

        /// <summary>
        /// Property to return the vertex buffer used by Gorgon.
        /// </summary>
        public D3D9.VertexBuffer SystemVertexBuffer
        {
            get
            {
                return Geometry.VertexBuffer.D3DVertexBuffer;
            }
        }

        /// <summary>
        /// Property to return the index buffer used by Gorgon.
        /// </summary>
        public D3D9.IndexBuffer SystemIndexBuffer
        {
            get
            {
                return Geometry.IndexBuffer.D3DIndexBuffer;
            }
        }

        /// <summary>
        /// Property to return the vertex declaration primarily used by Gorgon.
        /// </summary>
        public D3D9.VertexDeclaration SystemVertexType
        {
            get
            {
                return Gorgon.Renderer.VertexTypes[0].D3DVertexDeclaration;
            }
        }

        /// <summary>
        /// Property to return the size of the system vertices in bytes.
        /// </summary>
        public int SystemVertexSize
        {
            get
            {
                return Gorgon.Renderer.VertexTypes[0].VertexSize(0);
            }
        }

        /// <summary>
        /// Property to return a list of render states.
        /// </summary>
        public RenderStates RenderStates
        {
            get
            {
                return Gorgon.Renderer.RenderStates;
            }
        }

        /// <summary>
        /// Property to return a list of image states.
        /// </summary>
        public ImageLayerStates[] ImageStates
        {
            get
            {
                return Gorgon.Renderer.ImageLayerStates;
            }
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to reset the projection and view states.
        /// </summary>
        public void ResetProjectionViewState()
        {
            if (!Gorgon.Screen.DeviceNotReset)
            {
                // Reset.
                Gorgon.Renderer.CurrentViewport = Gorgon.CurrentRenderTarget.DefaultView;
                Gorgon.Renderer.CurrentProjectionMatrix = Gorgon.CurrentRenderTarget.ProjectionMatrix;

                // Change the clipper to the render target dimensions.
                Gorgon.CurrentClippingViewport = Gorgon.CurrentRenderTarget.DefaultView;
            }
        }

        /// <summary>
        /// Function to set the current image.
        /// </summary>
        /// <param name="stage">Image stage to set.</param>
        /// <param name="image">Image to set.</param>
        public void SetImage(int stage, Image image)
        {
            Gorgon.Renderer.SetImage(stage, image);
        }

        /// <summary>
        /// Function to return the current image.
        /// </summary>
        /// <param name="stage">Image stage to return.</param>
        /// <returns>The currently set image at the specified stage.</returns>
        public Image GetImage(int stage)
        {
            return Gorgon.Renderer.GetImage(stage);
        }        

        /// <summary>
        /// Function to retrieve the Direct 3D texture object from an image object.
        /// </summary>
        /// <param name="image">Image that holds the texture.</param>
        /// <returns>The internal D3D texture object.</returns>
        public D3D9.Texture GetTextureFromImage(Image image)
        {
            if (image == null)
                return null;

            return image.D3DTexture;
        }

        /// <summary>
        /// Function to retrieve the Direct 3D effect object from a shader object.
        /// </summary>
        /// <param name="shader">Shader object that holds the effect.</param>
        /// <returns>The internal D3D effect object.</returns>
        public D3D9.Effect GetEffectFromShader(Shader shader)
        {
            if (shader == null)
                return null;

            return shader.D3DEffect;
        }

        /// <summary>
        /// Function to retrieve the Direct 3D surface object from a render target.
        /// </summary>
        /// <param name="target">Target that holds the surface.</param>
        /// <returns>The internal D3D surface object.</returns>
        public D3D9.Surface GetSurfaceFromTarget(RenderTarget target)
        {
            if (target == null)
                return null;

            return target.SurfaceBuffer;
        }

        /// <summary>
        /// Function to retrieve the Direct 3D depth buffer surface object from a render target.
        /// </summary>
        /// <param name="target">Target that holds the surface.</param>
        /// <returns>The internal D3D surface object.</returns>
        public D3D9.Surface GetDepthBufferFromTarget(RenderTarget target)
        {
            if (target == null)
                return null;
            return target.DepthBuffer;
        }

        /// <summary>
        /// Function to retrieve the Direct 3D texture from a render image object.
        /// </summary>
        /// <param name="target">Target that holds the rendering image.</param>
        /// <returns>The internal D3D texture object.</returns>
        public D3D9.Texture GetTextureFromRenderImage(RenderImage target)
        {
            if (target == null)
                return null;

            return target.Image.D3DTexture;
        }

        /// <summary>
        /// Function to retrieve the Direct 3D swap chain from a render window object.
        /// </summary>
        /// <param name="target">Target that holds the swap chain.</param>
        /// <returns>The internal D3D swap chain object.</returns>
        public D3D9.SwapChain GetSwapChainFromRenderWindow(RenderWindow target)
        {
            if (target == null)
                return null;

            return target.SwapChain;
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="D3DObjects"/> class.
        /// </summary>
        internal D3DObjects()
        {
            if (!Gorgon.IsInitialized)
                throw new NotInitializedException();

            if (Gorgon.Screen == null)
                throw new DeviceNotValidException();

            DeviceStateList.Add(this);
        }

        /// <summary>
        /// Releases unmanaged resources and performs other cleanup operations before the
        /// <see cref="D3DObjects"/> is reclaimed by garbage collection.
        /// </summary>
        ~D3DObjects()
        {
            Dispose(false);
        }
        #endregion

        #region IDeviceStateObject Members
        /// <summary>
        /// Function called when the device is in a lost state.
        /// </summary>
        public void DeviceLost()
        {
            if (DeviceLostState != null)
                DeviceLostState(this, EventArgs.Empty);
        }

        /// <summary>
        /// Function called when the device is reset.
        /// </summary>
        public void DeviceReset()
        {
            if (DeviceResetState != null)
                DeviceResetState(this, EventArgs.Empty);
        }

        /// <summary>
        /// Function to force the loss of the objects data.
        /// </summary>
        public void ForceRelease()
        {
            DeviceLost();
        }
        #endregion

        #region IDisposable Members
        /// <summary>
        /// Releases unmanaged and - optionally - managed resources
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        private void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (DeviceStateList.Contains(this))
                    DeviceStateList.Remove(this);
                _disposed = true;
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        #endregion
    }
}
