#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Sunday, January 13, 2013 6:49:00 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Fonts;
using Gorgon.Graphics.Imaging;
using Gorgon.Input;
using Gorgon.Math;
using Gorgon.PlugIns;
using Gorgon.Renderers;
using Gorgon.UI;
using DX = SharpDX;
using FontStyle = Gorgon.Graphics.Fonts.FontStyle;
using GorgonMouseButtons = Gorgon.Input.MouseButtons;

namespace Gorgon.Examples
{
    /// <summary>
    /// Main application form.
    /// </summary>
    /// <remarks>
    /// This is an updated version of the INeedYourInput example from the previous version of Gorgon.
    /// 
    /// The keys for the example are as follows:
    /// F - Switch between full screen and windowed mode.
    /// Up arrow - Increase pen radius.
    /// Down arrow - Decrease pen radius.
    /// F1, F2, F3 - Switch between modulated, additive and no blending respectively.
    /// C - Clear the buffer.
    /// J - Switch to raw input and joysticks (if available).  Press J to cycle through joysticks and to get back to the win forms keyboard/mouse interface.
    /// ESC - Close the example.
    /// 
    /// Mouse controls:
    /// Left mouse button - Draw with blue pen.
    /// Right mouse button - Draw with red pen.
    /// Scroll wheel - Increase/decrease pen size.
    /// 
    /// Joystick control:
    /// Primary button - Draw with black pen.
    /// </remarks>
    public partial class MainForm
        : Form
    {
        #region Variables.
        // The graphics interface.
        private GorgonGraphics _graphics;
        // Primary swap chain.
        private GorgonSwapChain _screen;
        // The 2D renderer.
        private Gorgon2D _2D;
        // The factory used to produce fonts.
        private GorgonFontFactory _fontFactory;
        // Text font. 
        private GorgonFont _font;
        // Input factory.
        private GorgonRawInput _input;
        // Mouse object.
        private GorgonRawMouse _mouse;
        // Gaming device drivers.
        private IReadOnlyList<IGorgonGamingDeviceDriver> _drivers;
        // Joystick list.
        private List<IGorgonGamingDevice> _joystickList;
        // Joystick.
        private IGorgonGamingDevice _joystick;
        // Text sprite object.
        private GorgonTextSprite _messageSprite;
        // Back buffer.
        private GorgonRenderTarget2DView _backBuffer;
        // The back buffer texture view.
        private GorgonTexture2DView _backBufferView;
        // Backup image.
        private GorgonTexture2D _backupImage;
        // Pen radius.
        private float _radius = 6.0f;
        // Joystick index counter.
        private int _counter = -2;
        // Flag to indicate that we are using windows forms input.
        private bool _useWinFormsInput = true;
        // No blending batch state.
        private Gorgon2DBatchState _noBlending;
        // Inverted drawing.
        private Gorgon2DBatchState _inverted;
        // No blending when drawing.
        private Gorgon2DBatchState _drawNoBlend;
        // Modulated blending when drawing.
        private Gorgon2DBatchState _drawModulatedBlend;
        // Additive blending when drawing.
        private Gorgon2DBatchState _drawAdditiveBlend;
        // Current blending mode.
        private Gorgon2DBatchState _currentBlend;
        // The builder used for building batch states for the blending modes.
        private readonly Gorgon2DBatchStateBuilder _blendBuilder = new();
        // Our assembly cache for our plugins.
        private GorgonMefPlugInCache _assemblyCache;
        #endregion

        #region Methods.
        /// <summary>
        /// Handles the <see cref="E:KeyDown" /> event.
        /// </summary>
        /// <param name="e">The <see cref="KeyEventArgs" /> instance containing the event data.</param>
        protected override void OnKeyUp(KeyEventArgs e)
        {
            switch (e.KeyCode)
            {
                case Keys.Escape:
                    Close();            // Close
                    break;
                case Keys.F:
                    if (_screen.IsWindowed)
                    {
                        _screen.EnterFullScreen();
                    }
                    else
                    {
                        _screen.ExitFullScreen();
                    }
                    break;
                case Keys.Down:
                    _radius -= 1.0f;
                    if (_radius < 2.0f)
                    {
                        _radius = 2.0f;
                    }
                    break;
                case Keys.Up:
                    _radius += 1.0f;
                    if (_radius > 50.0f)
                    {
                        _radius = 50.0f;
                    }
                    break;
                case Keys.F1:
                    _currentBlend = _drawModulatedBlend;
                    break;
                case Keys.F2:
                    _currentBlend = _drawAdditiveBlend;
                    break;
                case Keys.F3:
                    _currentBlend = _drawNoBlend;
                    break;
                case Keys.C:
                    // Fill the back up image with white
                    _backBuffer.Clear(GorgonColor.White);
                    _backBuffer.Texture.CopyTo(_backupImage);
                    break;
                case Keys.J:
                    // Disable if we go beyond the end of the list.
                    _counter++;

                    if (_counter == -1)
                    {
                        // Clip the mouse cursor to our client area.
                        Rectangle screenRect = Cursor.Clip = RectangleToScreen(ClientRectangle);
                        _mouse.PositionConstraint = new DX.Rectangle(screenRect.Left, screenRect.Top, screenRect.Width, screenRect.Height);
                        // Set the position to the current mouse position.
                        _mouse.Position = new DX.Point(Cursor.Position.X, Cursor.Position.Y);

                        _input.RegisterDevice(_mouse);
                        _useWinFormsInput = false;
                        _messageSprite.Text = "Using mouse and keyboard (Raw Input)";
                        break;
                    }

                    if ((_joystickList.Count == 0) || ((_counter >= _joystickList.Count) && (_joystick is not null)))
                    {
                        if (!_useWinFormsInput)
                        {
                            Cursor.Clip = Rectangle.Empty;
                            _input.UnregisterDevice(_mouse);
                        }

                        _useWinFormsInput = true;
                        _joystick = null;
                        _counter = -2;
                        _messageSprite.Text = "Using mouse and keyboard (Windows Forms).";
                        break;
                    }

                    // If we previously had raw input on, turn it off.
                    if (!_useWinFormsInput)
                    {
                        Cursor.Clip = Rectangle.Empty;
                        _input.UnregisterDevice(_mouse);
                        _useWinFormsInput = true;
                    }

                    // Move to the next joystick.
                    _joystick = _joystickList[_counter];
                    _messageSprite.Text = "Using joystick " + _joystick.Info.Description;
                    break;
            }
        }

        /// <summary>
        /// Handles the <see cref="E:MouseWheel" /> event.
        /// </summary>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        protected override void OnMouseWheel(MouseEventArgs e)
        {
            base.OnMouseWheel(e);

            if (!_useWinFormsInput)
            {
                return;
            }

            _radius += e.Delta.Sign();

            if (_radius < 2.0f)
            {
                _radius = 2.0f;
            }
            if (_radius > 50.0f)
            {
                _radius = 50.0f;
            }
        }

        /// <summary>
        /// Handles the <see cref="E:MouseDown" /> event.
        /// </summary>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (!_useWinFormsInput)
            {
                return;
            }

            GorgonMouseButtons buttons = GorgonMouseButtons.None;

            if ((e.Button & System.Windows.Forms.MouseButtons.Left) == System.Windows.Forms.MouseButtons.Left)
            {
                buttons |= GorgonMouseButtons.Left;
            }

            if ((e.Button & System.Windows.Forms.MouseButtons.Right) == System.Windows.Forms.MouseButtons.Right)
            {
                buttons |= GorgonMouseButtons.Right;
            }

            MouseInput(this, new GorgonMouseEventArgs(buttons, GorgonMouseButtons.None, new DX.Point(e.Location.X, e.Location.Y), e.Delta, DX.Point.Zero, e.Delta, 0, false));
        }

        /// <summary>
        /// Handles the <see cref="E:MouseMove" /> event.
        /// </summary>
        /// <param name="e">The <see cref="MouseEventArgs"/> instance containing the event data.</param>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (!_useWinFormsInput)
            {
                return;
            }

            GorgonMouseButtons buttons = GorgonMouseButtons.None;

            if ((e.Button & System.Windows.Forms.MouseButtons.Left) == System.Windows.Forms.MouseButtons.Left)
            {
                buttons |= GorgonMouseButtons.Left;
            }

            if ((e.Button & System.Windows.Forms.MouseButtons.Right) == System.Windows.Forms.MouseButtons.Right)
            {
                buttons |= GorgonMouseButtons.Right;
            }

            MouseInput(this, new GorgonMouseEventArgs(buttons, GorgonMouseButtons.None, new DX.Point(e.Location.X, e.Location.Y), e.Delta, DX.Point.Zero, e.Delta, 0, false));
        }

        /// <summary>
        /// Function called when the mouse wheel is rotated.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="GorgonMouseEventArgs"/> instance containing the event data.</param>
        private void Mouse_MouseWheelMove(object sender, GorgonMouseEventArgs e)
        {
            _radius += e.WheelDelta.Sign();

            if (_radius < 2.0f)
            {
                _radius = 2.0f;
            }
            if (_radius > 10.0f)
            {
                _radius = 10.0f;
            }
        }

        /// <summary>
        /// Handles the button and movement events of the _mouse control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="GorgonMouseEventArgs" /> instance containing the event data.</param>
        private void MouseInput(object sender, GorgonMouseEventArgs e)
        {
            if (_joystick is not null)
            {
                return;
            }

            Color drawColor = Color.Black;      // Drawing color.
            var mousePos = new Point(e.Position.X, e.Position.Y);
            Point position = _useWinFormsInput ? mousePos : PointToClient(mousePos);

            if (e.Buttons == GorgonMouseButtons.None)
            {
                return;
            }

            // Draw to the back buffer.
            _graphics.SetRenderTarget(_backBuffer);
            _2D.Begin(_currentBlend);
            if ((e.Buttons & GorgonMouseButtons.Left) == GorgonMouseButtons.Left)
            {
                drawColor = Color.FromArgb(64, 0, 0, 192);
            }

            if ((e.Buttons & GorgonMouseButtons.Right) == GorgonMouseButtons.Right)
            {
                drawColor = Color.FromArgb(64, 192, 0, 0);
            }

            // Draw the pen.
            var penPosition = new DX.RectangleF(position.X - (_radius / 2.0f), position.Y - (_radius / 2.0f), _radius, _radius);
            if (_radius > 3.0f)
            {
                _2D.DrawFilledEllipse(penPosition, drawColor);
            }
            else
            {
                _2D.DrawFilledRectangle(penPosition, drawColor);
            }
            _2D.End();
            _graphics.SetRenderTarget(_screen.RenderTargetView);
        }

        /// <summary>
        /// Function called after a swap chain is resized.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SwapChainResizedEventArgs"/> instance containing the event data.</param>
        private void AfterSwapChainResized(object sender, SwapChainResizedEventArgs e)
        {
            // Restore the render target buffer and restore the contents of it.
            _backBuffer = GorgonRenderTarget2DView.CreateRenderTarget(_graphics,
                                                                      new GorgonTexture2DInfo("Backbuffer")
                                                                      {
                                                                          Width = ClientSize.Width,
                                                                          Height = ClientSize.Height,
                                                                          Format = BufferFormat.R8G8B8A8_UNorm
                                                                      });
            _backBuffer.Clear(Color.White);
            _backupImage.CopyTo(_backBuffer.Texture, new DX.Rectangle(0, 0, _backBuffer.Width, _backBuffer.Height));

            _backBufferView = _backBuffer.GetShaderResourceView();
        }

        /// <summary>
        /// Function called before a swap chain is resized.
        /// </summary>
        /// <param name="sender">The sender.</param>
        /// <param name="e">The <see cref="SwapChainResizingEventArgs"/> instance containing the event data.</param>
        private void BeforeSwapChainResized(object sender, SwapChainResizingEventArgs e)
        {
            // Copy the render target texture to a temporary buffer and resize the main buffer.
            // The copy the temporary buffer back to the main buffer.
            _backBuffer.Texture.CopyTo(_backupImage, new DX.Rectangle(0, 0, e.NewSize.Width, e.NewSize.Height));
            _backBufferView.Dispose();
            _backBuffer.Dispose();
        }

        /// <summary>
        /// Function to process during idle time.
        /// </summary>
        /// <returns><b>true</b> to continue processing, <b>false</b> to end processing.</returns>
        private bool Gorgon_Idle()
        {
            // Cursor position.
            Point mousePosition = PointToClient(!_useWinFormsInput ? new Point(_mouse.Position.X, _mouse.Position.Y) : Cursor.Position);
            var cursorPosition = new DX.Vector2(mousePosition.X, mousePosition.Y);

            if (!_useWinFormsInput)
            {
                Cursor.Position = PointToScreen(mousePosition);
            }

            // Dump to the screen.
            _2D.Begin(_noBlending);
            _2D.DrawFilledRectangle(new DX.RectangleF(0, 0, _backBuffer.Width, _backBuffer.Height), GorgonColor.White, _backBufferView, new DX.RectangleF(0, 0, 1, 1));
            _2D.End();

            if (_joystick is not null)
            {
                // Poll the joystick.
                _joystick.Poll();

                GorgonRange xAxisRange = _joystick.Info.AxisInfo[GamingDeviceAxis.XAxis].Range;
                GorgonRange yAxisRange = _joystick.Info.AxisInfo[GamingDeviceAxis.YAxis].Range;
                GorgonRange throttleRange = GorgonRange.Empty;

                if (_joystick.Info.AxisInfo.TryGetValue(GamingDeviceAxis.Throttle, out GorgonGamingDeviceAxisInfo info))
                {
                    throttleRange = info.Range;
                }

                // Adjust position to match screen coordinates.
                cursorPosition = new DX.Vector2(_joystick.Axis[GamingDeviceAxis.XAxis].Value - xAxisRange.Minimum,
                                             _joystick.Axis[GamingDeviceAxis.YAxis].Value - yAxisRange.Minimum);
                cursorPosition.X = cursorPosition.X / (xAxisRange.Range + 1) * _screen.Width;
                cursorPosition.Y = _screen.Height - (cursorPosition.Y / (yAxisRange.Range + 1) * _screen.Height);

                if (throttleRange.Range != 0)
                {
                    _radius = ((1.0f - (_joystick.Axis[GamingDeviceAxis.Throttle].Value / (float)throttleRange.Range)) * 8) + 2;
                }
            }


            // Draw cursor.
            _2D.Begin(_inverted);
            if (_radius > 3.0f)
            {
                _2D.DrawFilledEllipse(new DX.RectangleF(cursorPosition.X - (_radius / 2.0f), cursorPosition.Y - (_radius / 2.0f), _radius, _radius), Color.White);
            }
            else
            {
                _2D.DrawFilledRectangle(new DX.RectangleF(cursorPosition.X - (_radius / 2.0f), cursorPosition.Y - (_radius / 2.0f), _radius, _radius), Color.White);
            }
            _2D.End();

            // If we have a joystick button down, then draw a black dot.
            if ((_joystick is not null) && (_joystick.Button[0] == GamingDeviceButtonState.Down))
            {
                var penPosition = new DX.RectangleF(cursorPosition.X - (_radius / 2.0f), cursorPosition.Y - (_radius / 2.0f), _radius, _radius);
                _graphics.SetRenderTarget(_backBuffer);
                _2D.Begin();

                if (_radius > 3.0f)
                {
                    _2D.DrawFilledEllipse(penPosition, Color.Black);
                }
                else
                {
                    _2D.DrawFilledRectangle(penPosition, Color.Black);
                }
                _2D.End();
                _graphics.SetRenderTarget(_screen.RenderTargetView);
            }

            _2D.Begin();
            _2D.DrawTextSprite(_messageSprite);
            _2D.End();

            _screen.Present(1);

            return true;
        }

        /// <summary>
        /// </summary>
        /// <param name="e">An <see cref="System.EventArgs" /> that contains the event data.</param>
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);

            // We do this check because a Maximized state does not call the 
            // OnResizeEnd method.
            if (WindowState == FormWindowState.Maximized)
            {
                OnResizeEnd(e);
            }
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.ResizeEnd" /> event.
        /// </summary>
        /// <param name="e">A <see cref="System.EventArgs" /> that contains the event data.</param>
        protected override void OnResizeEnd(EventArgs e)
        {
            base.OnResizeEnd(e);

            var currentImageSize = new Size(_backBuffer.Width, _backBuffer.Height);

            // Copy the render target texture to a temporary buffer and resize the main buffer.
            // The copy the temporary buffer back to the main buffer.
            _backBuffer.Texture.CopyTo(_backupImage, new DX.Rectangle(0, 0, currentImageSize.Width, currentImageSize.Height));
            _backBufferView.Dispose();
            _backBuffer.Dispose();
            _backBuffer = GorgonRenderTarget2DView.CreateRenderTarget(_graphics,
                                                                      new GorgonTexture2DInfo("Backbuffer")
                                                                      {
                                                                          Width = ClientSize.Width,
                                                                          Height = ClientSize.Height,
                                                                          Format = BufferFormat.R8G8B8A8_UNorm
                                                                      });
            _backBuffer.Clear(Color.White);
            _backupImage.CopyTo(_backBuffer.Texture, new DX.Rectangle(0, 0, _backBuffer.Width, _backBuffer.Height));

            _backBufferView = _backBuffer.GetShaderResourceView();
        }

        /// <summary>
        /// Handles the <see cref="E:Activated" /> event.
        /// </summary>
        /// <param name="e">The <see cref="EventArgs"/> instance containing the event data.</param>
        protected override void OnActivated(EventArgs e)
        {
            base.OnActivated(e);

            // If the window loses focus, some of our gaming devices will become unacquired. 
            // This means that no data will be received from the gaming devices until they are acquired.
            // Doing this on window activation is the best way to ensure that happens.
            foreach (IGorgonGamingDevice joystick in _joystickList)
            {
                joystick.IsAcquired = true;
            }
        }

        protected override void OnDeactivate(EventArgs e)
        {
            base.OnDeactivate(e);

            // Mark each gaming device as unacquired if we lose focus.
            foreach (IGorgonGamingDevice joystick in _joystickList)
            {
                joystick.IsAcquired = true;
            }
        }

        /// <summary>
        /// Handles the <see cref="E:FormClosing" /> event.
        /// </summary>
        /// <param name="e">The <see cref="FormClosingEventArgs"/> instance containing the event data.</param>
        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            base.OnFormClosing(e);

            // Always dispose your devices when the window is shutting down.
            // Failure to do so can lead to unpredictable results.
            if (_joystickList is not null)
            {
                foreach (IGorgonGamingDevice joystick in _joystickList)
                {
                    joystick.Dispose();
                }
            }

            if (_drivers is not null)
            {
                foreach (IGorgonGamingDeviceDriver driver in _drivers)
                {
                    driver.Dispose();
                }
            }

            _input?.UnregisterDevice(_mouse);
            _input?.Dispose();

            _fontFactory?.Dispose();
            _assemblyCache?.Dispose();
        }

        /// <summary>
        /// Raises the <see cref="E:System.Windows.Forms.Form.Load"></see> event.
        /// </summary>
        /// <param name="e">An <see cref="System.EventArgs"></see> that contains the event data.</param>
        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            try
            {
                Debug.Assert(IsHandleCreated, "No handle");
                GorgonExample.PlugInLocationDirectory = new DirectoryInfo(ExampleConfig.Default.PlugInLocation);

                // Load the assembly.
                _assemblyCache = new GorgonMefPlugInCache(GorgonApplication.Log);

                // Create the plugin service.
                IGorgonPlugInService plugInService = new GorgonMefPlugInService(_assemblyCache);

                // Create the factory to retrieve gaming device drivers.
                var factory = new GorgonGamingDeviceDriverFactory(_assemblyCache);

                // Create the raw input interface.
                _input = new GorgonRawInput(this, GorgonApplication.Log);

                // Get available gaming device driver plug ins.
                _drivers = factory.LoadAllDrivers(Path.Combine(GorgonExample.GetPlugInPath().FullName, "Gorgon.Input.*.dll"));

                _joystickList = new List<IGorgonGamingDevice>();

                // Get all gaming devices from the drivers.
                foreach (IGorgonGamingDeviceDriver driver in _drivers)
                {
                    IReadOnlyList<IGorgonGamingDeviceInfo> infoList = driver.EnumerateGamingDevices(true);

                    foreach (IGorgonGamingDeviceInfo info in infoList)
                    {
                        IGorgonGamingDevice device = driver.CreateGamingDevice(info);

                        // Turn off dead zones for this example.
                        foreach (IGorgonGamingDeviceAxis axis in device.Axis)
                        {
                            axis.DeadZone = GorgonRange.Empty;
                        }

                        _joystickList.Add(device);
                    }
                }

                // Create mouse.
                _mouse = new GorgonRawMouse();

                // Create the graphics interface.
                ClientSize = new Size(ExampleConfig.Default.Resolution.Width, ExampleConfig.Default.Resolution.Height);

                IReadOnlyList<IGorgonVideoAdapterInfo> adapters = GorgonGraphics.EnumerateAdapters();
                _graphics = new GorgonGraphics(adapters[0], log: GorgonApplication.Log);
                _screen = new GorgonSwapChain(_graphics, this, new GorgonSwapChainInfo("INeedYourInput Swapchain")
                {
                    Width = ExampleConfig.Default.Resolution.Width,
                    Height = ExampleConfig.Default.Resolution.Height,
                    Format = BufferFormat.R8G8B8A8_UNorm
                });
                _graphics.SetRenderTarget(_screen.RenderTargetView);

                if (!ExampleConfig.Default.IsWindowed)
                {
                    _screen.EnterFullScreen();
                }

                // For the backup image. Used to make it as large as the monitor that we're on.
                var currentScreen = Screen.FromHandle(Handle);

                // Relocate the window to the center of the screen.				
                Location = new Point(currentScreen.Bounds.Left + (currentScreen.WorkingArea.Width / 2) - (ClientSize.Width / 2),
                                     currentScreen.Bounds.Top + (currentScreen.WorkingArea.Height / 2) - (ClientSize.Height / 2));


                // Create the 2D renderer.
                _2D = new Gorgon2D(_graphics);

                // Create the text font.
                _fontFactory = new GorgonFontFactory(_graphics);
                _font = _fontFactory.GetFont(new GorgonFontInfo("Arial", 9.0f, FontHeightMode.Points, "Arial 9pt")
                {
                    FontStyle = FontStyle.Bold,
                    AntiAliasingMode = FontAntiAliasMode.AntiAlias
                });

                // Create text sprite.
                _messageSprite = new GorgonTextSprite(_font, "Using mouse and keyboard (Windows Forms).")
                {
                    Color = Color.Black
                };

                // Create a back buffer.
                _backBuffer = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo("Backbuffer storage")
                {
                    Width = _screen.Width,
                    Height = _screen.Height,
                    Format = _screen.Format
                });
                _backBuffer.Clear(Color.White);
                _backBufferView = _backBuffer.GetShaderResourceView();

                // Clear our backup image to white to match our primary screen.
                using (IGorgonImage image = new GorgonImage(new GorgonImageInfo(ImageType.Image2D, _screen.Format)
                {
                    Width = _screen.Width,
                    Height = _screen.Height,
                    Format = _screen.Format
                }))
                {
                    image.Buffers[0].Fill(0xff);
                    _backupImage = image.ToTexture2D(_graphics,
                                                     new GorgonTexture2DLoadOptions
                                                     {
                                                         Binding = TextureBinding.None,
                                                         Usage = ResourceUsage.Staging
                                                     });
                }

                // Set gorgon events.
                _screen.SwapChainResizing += BeforeSwapChainResized;
                _screen.SwapChainResized += AfterSwapChainResized;

                // Enable the mouse.
                Cursor = Cursors.Cross;
                _mouse.MouseButtonDown += MouseInput;
                _mouse.MouseMove += MouseInput;
                _mouse.MouseWheelMove += Mouse_MouseWheelMove;

                // Set the mouse position.
                _mouse.Position = new DX.Point(ClientSize.Width / 2, ClientSize.Height / 2);

                _noBlending = _blendBuilder.BlendState(GorgonBlendState.NoBlending)
                                               .Build();
                _inverted = _blendBuilder.BlendState(GorgonBlendState.Inverted)
                                             .Build();

                // Set up blending states for our pen.
                var blendStateBuilder = new GorgonBlendStateBuilder();
                _currentBlend = _drawModulatedBlend = _blendBuilder.BlendState(blendStateBuilder
                                                                               .ResetTo(GorgonBlendState.Default)
                                                                               .DestinationBlend(alpha: Blend.One)
                                                                               .Build())
                                                                   .Build();

                _drawAdditiveBlend = _blendBuilder.BlendState(blendStateBuilder
                                                              .ResetTo(GorgonBlendState.Additive)
                                                              .DestinationBlend(alpha: Blend.One)
                                                              .Build())
                                                  .Build();

                _drawNoBlend = _blendBuilder.BlendState(blendStateBuilder
                                                        .ResetTo(GorgonBlendState.NoBlending)
                                                        .DestinationBlend(alpha: Blend.One)
                                                        .Build())
                                            .Build();

                GorgonApplication.IdleMethod = Gorgon_Idle;
            }
            catch (ReflectionTypeLoadException refEx)
            {
                string refErr = string.Join("\n", refEx.LoaderExceptions.Select(item => item.Message));
                GorgonDialogs.ErrorBox(this, refErr);
            }
            catch (Exception ex)
            {
                GorgonExample.HandleException(ex);
                GorgonApplication.Quit();
            }
        }
        #endregion

        #region Constructor/Destructor.
        /// <summary>
        /// Constructor.
        /// </summary>
        public MainForm() => InitializeComponent();
        #endregion
    }
}