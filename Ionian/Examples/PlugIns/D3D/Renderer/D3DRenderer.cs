#region LGPL.
// 
// Examples.
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
// Created: Monday, April 07, 2008 8:26:45 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using SharpUtilities;
using SharpUtilities.Utility;
using SharpUtilities.Mathematics;
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
        private Image _lastImage = null;                                    // Last image to use.
        private float _angle = 0.0f;                                        // Angle in degrees.
        private Cube _cube = null;                                          // Our cube object.
        private Random _rndChan = new Random();                             // Random number generator.
        private Random _rndDir = new Random();                              // Random number generator.
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
            _d3dObjects.Device.SetTransform(SlimDX.Direct3D9.TransformState.Projection, _projection);
            _d3dObjects.Device.SetTransform(SlimDX.Direct3D9.TransformState.View, _view);
            _d3dObjects.RenderStates.ShadingMode = ShadingMode.Gouraud;
            _d3dObjects.RenderStates.SpecularEnabled = true;
            _d3dObjects.RenderStates.LightingEnabled = true;
            _d3dObjects.RenderStates.CullingMode = CullingMode.None;
            _d3dObjects.RenderStates.AlphaBlendEnabled = true;
            _d3dObjects.RenderStates.SourceAlphaBlendOperation = Gorgon.CurrentRenderTarget.SourceBlend;
            _d3dObjects.RenderStates.DestinationAlphaBlendOperation = Gorgon.CurrentRenderTarget.DestinationBlend;
            
            _lastImage = _d3dObjects.GetImage(0);

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
            int chan = _rndChan.Next(3000);                                         // Random channel.
            float delta = (float)(_rndChan.NextDouble()) * (frameTime * 250.0f);    // Random delta.

            if (_d3dObjects.DeviceNeedsReset)
                return;
          
            _angle -= 15.0f * frameTime;

            if ((chan >= 0) && (chan <= 1000))
            {
                if (_rndDir.Next(1000) > _rndChan.Next(500))
                {
                    if ((_cubeColor.Red - delta) > 0.0f)
                        _cubeColor.Red -= delta;
                }
                else
                {
                    if ((_cubeColor.Red + delta) < 1.0f)
                        _cubeColor.Red += delta;
                }
            }

            if ((chan > 1000) && (chan <= 2000))
            {
                if (_rndDir.Next(1000) > _rndChan.Next(500))
                {
                    if ((_cubeColor.Green - delta) > 0.0f)
                        _cubeColor.Green -= delta;
                }
                else
                {
                    if ((_cubeColor.Green + delta) < 1.0f)
                        _cubeColor.Green += delta;
                }
            }


            if ((chan > 1000) && (chan <= 3000))
            {
                if (_rndDir.Next(1000) > _rndChan.Next(500))
                {
                    if ((_cubeColor.Blue - delta) > 0.0f)
                        _cubeColor.Blue -= delta;
                }
                else
                {
                    if ((_cubeColor.Blue + delta) < 1.0f)
                        _cubeColor.Blue += delta;
                }
            }

            _cube.Diffuse = _cubeColor;
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

            _d3dObjects.Device.SetTransform(SlimDX.Direct3D9.TransformState.World, SlimDX.Matrix.Identity);
            _d3dObjects.Device.SetTransform(SlimDX.Direct3D9.TransformState.View, SlimDX.Matrix.Identity);
            _d3dObjects.RenderStates.CullingMode = CullingMode.Clockwise;
            _d3dObjects.RenderStates.LightingEnabled = false;
            _d3dObjects.RenderStates.ShadingMode = ShadingMode.Flat;
            _d3dObjects.RenderStates.SpecularEnabled = false;
            _d3dObjects.Device.EndScene();

            // Reset back to the Gorgon buffers and states - if we don't do this then Gorgon will unaware of any changes and things
            // will look odd.
            _d3dObjects.SetImage(0, _lastImage);
            _d3dObjects.Device.VertexDeclaration = _d3dObjects.SystemVertexType;
            _d3dObjects.Device.SetStreamSource(0, _d3dObjects.SystemVertexBuffer, 0, _d3dObjects.SystemVertexSize);
            _d3dObjects.Device.Indices = _d3dObjects.SystemIndexBuffer;
            _d3dObjects.ResetProjectionViewState();

            _text.Text = "Cube color ->\n\tR: " + _cubeColor.Red.ToString("0.000") + "\n\tG: " + _cubeColor.Green.ToString("0.000") + "\n\tB: " + _cubeColor.Blue.ToString("0.000");
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
                _cube.Diffuse = _cubeColor;
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
