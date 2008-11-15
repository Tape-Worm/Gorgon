#region MIT.
// 
// Gorgon.
// Copyright (C) 2008 Michael Winsor
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
// Created: TOBEREPLACED
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Dialogs;
using GorgonLibrary;
using GorgonLibrary.Graphics;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Example
{
    /// <summary>
    /// Our Direct 3D renderer.
    /// </summary>
    /// <remarks>This isn't a tutorial in 3D (in fact there's a lot things that are done very poorly here, so please don't use
    /// this as a tutorial on how to do 3D work).  This is provided as a sample to show how you can take control of the underlying
    /// SlimDX Direct3D objects to inject your own rendering.</remarks>
    public class D3DRenderer
        : IRenderer
    {
        #region Value Types.
        /// <summary>
        /// Vertex for our model.
        /// </summary>
        public struct Vertex
        {
            /// <summary>
            /// Position of the vertex.
            /// </summary>
            public SlimDX.Vector3 Position;
            /// <summary>
            /// Normal for the vertex.
            /// </summary>
            public SlimDX.Vector3 Normal;
            /// <summary>
            /// Texture coordinate.
            /// </summary>
            public SlimDX.Vector2 TexUV;
        }
        #endregion

        #region Variables.
        private D3DObjects _d3dObjects = null;                              // Interface to our direct 3D objects.
        private bool _disposed = false;                                     // Flag to indicate whether we've disposed or not.
        private SlimDX.Matrix _projection = SlimDX.Matrix.Identity;         // Projection matrix.
        private SlimDX.Matrix _view = SlimDX.Matrix.Identity;               // View matrix.
        private SlimDX.Direct3D9.Light _light;                              // Our global light.
        private float _angle = 0.0f;                                        // Angle in degrees.
        private Cube _cube = null;                                          // Our cube object.
        private SlimDX.Color4 _cubeColor;                                   // Cube color.
        private Font _font = null;                                          // Text font.
        private TextSprite _text = null;                                    // Text to display.
        #endregion

        #region Methods.
        /// <summary>
        /// Handles the DeviceResetState event of the _d3dObjects control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void _d3dObjects_DeviceResetState(object sender, EventArgs e)
        {
            // In here we can reset if the device is reset.
        }

        /// <summary>
        /// Handles the DeviceLostState event of the _d3dObjects control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
        void _d3dObjects_DeviceLostState(object sender, EventArgs e)
        {
            // In here we can reset if the device is reset.
        }
        #endregion

        #region Constructor.
        /// <summary>
        /// Initializes a new instance of the <see cref="D3DRenderer"/> class.
        /// </summary>
        /// <param name="d3dInterface">The D3D interface to use.</param>
        public D3DRenderer(D3DObjects d3dInterface)
        {
            _d3dObjects = d3dInterface;
            _d3dObjects.DeviceLostState += new EventHandler(_d3dObjects_DeviceLostState);
            _d3dObjects.DeviceResetState += new EventHandler(_d3dObjects_DeviceResetState);            
        }
        #endregion

        #region IRenderer Members
        /// <summary>
        /// Function to begin the rendering.
        /// </summary>
        public void Begin()
        {
            if (_d3dObjects.DeviceNeedsReset)
                return;

            // Store our state and flush the rendering pipeline.
            _d3dObjects.PushStates();

            // Define our 3D states.
            _d3dObjects.Device.SetTransform(SlimDX.Direct3D9.TransformState.Projection, _projection);
            _d3dObjects.Device.SetTransform(SlimDX.Direct3D9.TransformState.View, _view);
            _d3dObjects.RenderStates.ShadingMode = ShadingMode.Gouraud;
            _d3dObjects.RenderStates.SpecularEnabled = true;
            _d3dObjects.RenderStates.LightingEnabled = true;
            _d3dObjects.RenderStates.CullingMode = CullingMode.None;
            _d3dObjects.RenderStates.AlphaBlendEnabled = true;
            _d3dObjects.RenderStates.SourceAlphaBlendOperation = AlphaBlendOperation.SourceAlpha;
            _d3dObjects.RenderStates.DestinationAlphaBlendOperation = AlphaBlendOperation.InverseSourceAlpha;
			_d3dObjects.ImageStates[0].MagnificationFilter = ImageFilters.Point;
			_d3dObjects.ImageStates[0].MinificationFilter = ImageFilters.Point;            

            // Ensure the light is setup.
            _d3dObjects.Device.SetLight(0, _light);
            _d3dObjects.Device.EnableLight(0, true);

            _d3dObjects.Device.BeginScene();
        }

        /// <summary>
        /// Function to render the scene.
        /// </summary>
        /// <param name="frameTime">Frame delta time.</param>
        public void Render(float frameTime)
        {       
            if (_d3dObjects.DeviceNeedsReset)
                return;

            _angle -= 15.0f * frameTime;
            if (_angle < -359.9999f)
                _angle = 0.0f;
            _cube.Diffuse = _cubeColor.ToColor();
            _cube.RotateXYZ(_angle, _angle, _angle);
            _cube.Draw();            
        }

        /// <summary>
        /// Function to end the rendering.
        /// </summary>
        public void End()
        {
            if (_d3dObjects.DeviceNeedsReset)
                return;

            _d3dObjects.Device.EndScene();

            // Restore the Gorgon states.
            _d3dObjects.PopStates();

            _text.Smoothing = Smoothing.Smooth;
            _text.Text = "Cube Angle: " + _angle.ToString("0.000") + "\nHit F to toggle smoothing " + 
                ((Gorgon.GlobalStateSettings.GlobalSmoothing == Smoothing.None) ? "On" : "Off") + ".";
            _text.Draw();
        }

        /// <summary>
        /// Function to initialize the 3D renderer.
        /// </summary>
        public void Initialize()
        {
            try
            {
                // Set up our matrices.
                float aspect = (float)Gorgon.CurrentRenderTarget.Width / (float)Gorgon.CurrentRenderTarget.Height;
                _projection = SlimDX.Matrix.PerspectiveFovLH(MathUtility.Radians(60.0f), aspect, 0.1f, 1000.0f);
                _view = SlimDX.Matrix.Translation(0.0f, 0.0f, 1.5f);

                // Set up stuff so we can actually see what's going on.
                _d3dObjects.Device.SetRenderState(SlimDX.Direct3D9.RenderState.Ambient, new SlimDX.Color4(0.2f, 0.2f, 0.2f).ToArgb());
                _light = new SlimDX.Direct3D9.Light();
                _light.Position = new SlimDX.Vector3(0.0f, 50.0f, -52.0f);
                _light.Type = SlimDX.Direct3D9.LightType.Point;
                _light.Diffuse = new SlimDX.Color4(1.0f, 1.0f, 1.0f);
                _light.Ambient = new SlimDX.Color4(0.15f, 0.15f, 0.15f);
                _light.Specular = new SlimDX.Color4(1.0f, 1.0f, 1.0f);
                _light.Attenuation0 = 1.0f;
                _light.Range = 1000.0f;

                // Create our font.
                _font = new Font("Arial", "Arial", 9.0f, true, true);
                _text = new TextSprite("TextDisplay", string.Empty, _font, System.Drawing.Color.Black);

                // Initialize our buffers and vertex data.
                _cube = new Cube(_d3dObjects);
                _cubeColor = new SlimDX.Color4(1.0f, 1.0f, 1.0f);
                _cube.Diffuse = _cubeColor.ToColor();
                _cube.Texture = Image.FromFile(@"..\..\..\..\Resources\D3D\Glass.png");
            }
            catch (Exception ex)
            {
                UI.ErrorBox(null, "There was an error initializing the Direct 3D plug-in.", ex);
            }
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
                if (disposing)
                {
                    // Remove events.
                    _d3dObjects.DeviceLostState -= new EventHandler(_d3dObjects_DeviceLostState);
                    _d3dObjects.DeviceResetState -= new EventHandler(_d3dObjects_DeviceResetState);

                    if (_cube != null)
                        _cube.Dispose();
                    if (_font != null)
                        _font.Dispose();

                    _d3dObjects = null;
                    _cube = null;
                }

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
