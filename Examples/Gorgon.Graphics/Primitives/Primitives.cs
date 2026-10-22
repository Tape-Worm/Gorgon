// Gorgon.
// Copyright (C) 2026 Michael Winsor
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: October 18, 2026 4:26:31 PM
//

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Numerics;
using System.Windows.Forms;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Input;
using Gorgon.Input.Devices;
using Gorgon.Math;
using Gorgon.Timing;
using Gorgon.UI.WindowsForms;
using MouseButtons = Gorgon.Input.Devices.MouseButtons;

namespace Gorgon.Examples;

/// <summary>
/// The main application class.
/// </summary>
/// <remarks>
/// <para>
/// TODO: Description of the example and its purpose.
/// </para>
/// </remarks>
internal class Primitives
    : IDisposable
{
    // The main application window.
    private readonly Form _window;
    // The graphics interface to use.
    private readonly GorgonGraphics _graphics;
    // The renderer used to render our scene.
    private readonly SimpleRenderer _renderer;
    // The compiler used to build the shaders.
    private readonly GorgonShaderCompiler _compiler;
    // The cache for holding texture resources.
    private readonly Dictionary<string, IGorgonTextureView<GorgonTexture>> _textureCache = [];
    // The cache for the mesh data
    private readonly Dictionary<string, Mesh> _meshes = [];
    // Rotation value.
    private float _objRotation;
    // Rotation value.
    private float _cloudRotation;
    // The buffer used to receive input events from input devices.
    private readonly GorgonInputEventBuffer _inputEvents;
    // The primary input interface.
    private readonly IGorgonInput _input;
    // Input device interfaces.
    private readonly IGorgonKeyboard _keyboard;
    private readonly IGorgonMouse _mouse;
    // Keys used for toggle operations.
    private readonly HashSet<VirtualKeys> _toggleKeys = [];
    // Camera rotation amount.
    private Vector3 _cameraRotation;
    // Mouse sensitivity.
    private float _sensitivity = 1.5f;
    // Flag to lock down the camera look at.
    private bool _lock = false;

    // Temp for FPS counting:
    private readonly GorgonTimer _timer = new();


    /// <summary>
    /// Function called when disposing of unmanaged and managed resources.
    /// </summary>
    /// <param name="disposing"><b>true</b> to dispose both managed and unmanaged resources, <b>false</b> to only dispose of unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _window.Resize -= Window_Resize;
            _window.MouseDown -= Window_MouseDown;

            _input.Dispose();
            _inputEvents.Dispose();

            foreach (Mesh mesh in _meshes.Values)
            {
                mesh.Dispose();
            }

            foreach (IGorgonTextureView<GorgonTexture> texture in _textureCache.Values)
            {
                texture.Dispose();
            }

            _renderer.Dispose();
            _compiler.Dispose();
            _graphics.Dispose();
        }
    }

    /// <summary>
    /// Function called when the window is resized so the swap and depth/stencil buffer can be resized along with it.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event arguments.</param>
    private void Window_Resize(object? sender, EventArgs e)
    {
        Form form = ((Form?)sender) ?? throw new NullReferenceException();

        if (form.WindowState == FormWindowState.Minimized)
        {
            return;
        }

        _renderer.ResizeSwapChain(form.ClientSize.Width, form.ClientSize.Height);
    }

    /// <summary>
    /// Function called when a mouse button is held down.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event arguments.</param>
    private void Window_MouseDown(object? sender, MouseEventArgs e)
    {
        if ((e.Button is not System.Windows.Forms.MouseButtons.Right) || (_mouse is null))
        {
            return;
        }

        _mouse.CursorVisible = false;

        // Capture the cursor so that we can't move it outside the client area.
        Cursor.Clip = _window.RectangleToScreen(new Rectangle(_window.ClientSize.Width / 2, _window.ClientSize.Height / 2, 1, 1));
    }

    /// <summary>
    /// Function called when the mouse wheel is rotated.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event arguments.</param>
    private void Window_MouseWheel(object? sender, MouseEventArgs e)
    {
        if (e.Delta < 0)
        {
            _sensitivity -= 0.05f;

            if (_sensitivity < 0.05f)
            {
                _sensitivity = 0.05f;
            }
        }
        else if (e.Delta > 0)
        {
            _sensitivity += 0.05f;

            if (_sensitivity > 3.0f)
            {
                _sensitivity = 3.0f;
            }
        }
    }

    /// <summary>
    /// Temporary FPS counter until 2D renderer is built.
    /// </summary>
    private void TempFpsCounter()
    {
        if (_timer.Milliseconds < 250)
        {
            return;
        }

        _window.Text = $"FPS: {GorgonTiming.AverageFPS:0.0}, Delta: {(GorgonTiming.AverageDelta * 1000):0.000} msec.";
        /*$"CamRot: {_cameraRotation} Mouse: {_mouse?.Position.X:0}x{_mouse?.Position.Y:0} Sensitivity: {_sensitivity:0.0##}";*/

        _timer.Reset();
    }

    /// <summary>
    /// Function to process mouse data.
    /// </summary>
    private void ProcessMouse()
    {
        if (!_mouse.ParseData(_inputEvents))
        {
            return;
        }

        if ((_mouse.Buttons & MouseButtons.Button2) != MouseButtons.Button2)
        {
            Cursor.Clip = Rectangle.Empty;
            _mouse.CursorVisible = true;
            return;
        }

        if ((_mouse.Buttons & MouseButtons.Button2) == MouseButtons.Button2)
        {
            GorgonPoint delta = _mouse.DeltaPosition;
            _cameraRotation.X += delta.Y.Sign() * (_sensitivity);
            _cameraRotation.Y += delta.X.Sign() * (_sensitivity);
        }
    }

    /// <summary>
    /// Function to process the keyboard commands.
    /// </summary>
    /// <returns>The camera direction.</returns>
    private Vector3 ProcessKeys()
    {
        Vector3 cameraDir = Vector3.Zero;

        _keyboard.ParseData(_inputEvents);

        // Function to determine the state of key for toggle operations.
        bool ToggleKeyCheck(VirtualKeys key)
        {
            if (!_toggleKeys.Contains(key))
            {
                return false;
            }

            if (!_keyboard[key])
            {
                _toggleKeys.Remove(key);
                return true;
            }

            return false;
        }

        // Handle keys we need to toggle.
        if (ToggleKeyCheck(VirtualKeys.B))
        {
            _renderer.ShowAABB = !_renderer.ShowAABB;
        }

        if (ToggleKeyCheck(VirtualKeys.L))
        {
            _lock = !_lock;
        }

        if (_keyboard[VirtualKeys.Escape])
        {
            Application.Exit();
            return Vector3.Zero;
        }

        if (_keyboard[VirtualKeys.Left])
        {
            _cameraRotation.Y -= 40.0f * GorgonTiming.Delta;
        }
        else if (_keyboard[VirtualKeys.Right])
        {
            _cameraRotation.Y += 40.0f * GorgonTiming.Delta;
        }
        if (_keyboard[VirtualKeys.Up])
        {
            _cameraRotation.X -= 40.0f * GorgonTiming.Delta;
        }
        else if (_keyboard[VirtualKeys.Down])
        {
            _cameraRotation.X += 40.0f * GorgonTiming.Delta;
        }
        if (_keyboard[VirtualKeys.PageUp])
        {
            _cameraRotation.Z -= 40.0f * GorgonTiming.Delta;
        }
        else if (_keyboard[VirtualKeys.PageDown])
        {
            _cameraRotation.Z += 40.0f * GorgonTiming.Delta;
        }
        if (_keyboard[VirtualKeys.D])
        {
            cameraDir.X = 2.0f * GorgonTiming.Delta;
        }
        else if (_keyboard[VirtualKeys.A])
        {
            cameraDir.X = -2.0f * GorgonTiming.Delta;
        }
        if (_keyboard[VirtualKeys.W])
        {
            cameraDir.Z = 2.0f * GorgonTiming.Delta;
        }
        else if (_keyboard[VirtualKeys.S])
        {
            cameraDir.Z = -2.0f * GorgonTiming.Delta;
        }
        if (_keyboard[VirtualKeys.Q])
        {
            cameraDir.Y = 2.0f * GorgonTiming.Delta;
        }
        else if (_keyboard[VirtualKeys.E])
        {
            cameraDir.Y = -2.0f * GorgonTiming.Delta;
        }
        else if (_keyboard[VirtualKeys.B])
        {
            _toggleKeys.Add(VirtualKeys.B);
        }
        else if (_keyboard[VirtualKeys.L])
        {
            _toggleKeys.Add(VirtualKeys.L);
        }

        return cameraDir;
    }

    /// <summary>
    /// Function to update the scene camera.
    /// </summary>
    /// <param name="sphere">The sphere to lock on to.</param>
    /// <param name="cameraDir">The direction of the camera.</param>
    private void UpdateCamera(Sphere sphere, Vector3 cameraDir)
    {
        _renderer.Camera.RotateEuler(_cameraRotation.Y, _cameraRotation.X, _cameraRotation.Z);

        Vector3 pos = sphere.Position;
        Quaternion camRotationQ = _renderer.Camera.Rotation;
        Matrix4x4 camRotation = Matrix4x4.CreateFromQuaternion(camRotationQ);
        Vector3 unitZ = Vector3.UnitZ;
        Vector3 forward = Vector3.Transform(unitZ, camRotation);
        Vector3 right;

        if (!_lock)
        {
            Vector3 unitX = Vector3.UnitX;
            right = Vector3.Transform(unitX, camRotation);
        }
        else
        {
            right = Vector3.Transform(pos, camRotation);
        }
        Vector3 up = Vector3.Cross(forward, right);

        right = Vector3.Multiply(right, cameraDir.X);
        _renderer.Camera.Position = Vector3.Add(_renderer.Camera.Position, right);

        forward = Vector3.Multiply(forward, cameraDir.Z);
        _renderer.Camera.Position = Vector3.Add(_renderer.Camera.Position, forward);

        up = Vector3.Multiply(up, cameraDir.Y);
        _renderer.Camera.Position = Vector3.Add(_renderer.Camera.Position, up);

        if (_renderer.Camera.Position.X < -10.5f)
        {
            _renderer.Camera.Position = new Vector3(-10.5f, _renderer.Camera.Position.Y, _renderer.Camera.Position.Z);
        }

        if (_renderer.Camera.Position.X > 10.5f)
        {
            _renderer.Camera.Position = new Vector3(10.5f, _renderer.Camera.Position.Y, _renderer.Camera.Position.Z);
        }

        if (_renderer.Camera.Position.Y < -1.35f)
        {
            _renderer.Camera.Position = new Vector3(_renderer.Camera.Position.X, -1.35f, _renderer.Camera.Position.Z);
        }

        if (_renderer.Camera.Position.Y > 25.0f)
        {
            _renderer.Camera.Position = new Vector3(_renderer.Camera.Position.X, 25.0f, _renderer.Camera.Position.Z);
        }

        if (_renderer.Camera.Position.Z > 13.25f)
        {
            _renderer.Camera.Position = new Vector3(_renderer.Camera.Position.X, _renderer.Camera.Position.Y, 13.25f);
        }

        if (_renderer.Camera.Position.Z < -10.5f)
        {
            _renderer.Camera.Position = new Vector3(_renderer.Camera.Position.X, _renderer.Camera.Position.Y, -10.5f);
        }

        if (_lock)
        {
            _renderer.Camera.LookAt(pos);
        }
    }

    /// <summary>
    /// Function called during CPU idle time to update our rendering.
    /// </summary>
    /// <returns><b>true</b> to continue, <b>false</b> to stop.</returns>
    private bool Idle()
    {
        TempFpsCounter();

        _input.GetInput(InputDeviceType.Keyboard | InputDeviceType.Mouse, _inputEvents);

        Vector3 cameraDir = ProcessKeys();
        ProcessMouse();
        //Vector3 cameraDir = Vector3.Zero;

        Triangle triangle = (Triangle)_meshes["Triangle"];
        Plane plane = (Plane)_meshes["Plane"];
        Cube cube = (Cube)_meshes["Cube"];
        Sphere sphere = (Sphere)_meshes["Sphere"];
        IcoSphere icoSphere = (IcoSphere)_meshes["IcoSphere"];
        Sphere clouds = (Sphere)_meshes["Clouds"];

        UpdateCamera(sphere, cameraDir);

        _cloudRotation += 2.0f * GorgonTiming.Delta;
        _objRotation += 50.0f * GorgonTiming.Delta;

        if (_cloudRotation > 359.9f)
        {
            _cloudRotation -= 359.9f;
        }

        if (_objRotation > 359.9f)
        {
            _objRotation -= 359.9f;
        }

        triangle.Material?.TextureOffset = new Vector2(0, triangle.Material.TextureOffset.Y - (0.125f * GorgonTiming.Delta));
        plane.Material?.TextureOffset = new Vector2(0, plane.Material.TextureOffset.Y - (0.125f * GorgonTiming.Delta));

        cube.Rotation = new Vector3(_objRotation, _objRotation, _objRotation);
        icoSphere.Rotation = new Vector3(0, icoSphere.Rotation.Y + (4.0f * GorgonTiming.Delta), 0);
        clouds.Rotation = new Vector3(0, _cloudRotation, 0);
        sphere.Position = new Vector3(-2.0f, (_objRotation.ToRadians().Sin().Abs() * 2.0f) - 1.10f, 0.75f);
        sphere.Rotation = new Vector3(_objRotation, _objRotation, 0);

        _renderer.Render();

        return true;
    }

    /// <summary>
    /// Function to build the geometry we want in the scene.
    /// </summary>
    private void BuildGeometry()
    {
        Vector3 fnU = new(0.5f, 1.0f, 0);
        Vector3 fnV = new(1.0f, 1.0f, 0);
        Vector3 faceNormal = Vector3.Cross(fnU, fnV);
        faceNormal = Vector3.Normalize(faceNormal);

        Triangle triangle = new(_graphics,
                                 new GorgonVertexPosNormUvTangent
                                 {
                                     Position = new Vector4(-12.5f, -1.5f, 12.5f, 1),
                                     Normal = faceNormal,
                                     UV = new Vector2(0, 1.0f)
                                 },
                                 new GorgonVertexPosNormUvTangent
                                 {
                                     Position = new Vector4(0, 24.5f, 12.5f, 1),
                                     Normal = faceNormal,
                                     UV = new Vector2(0.5f, 0.0f)
                                 },
                                 new GorgonVertexPosNormUvTangent
                                 {
                                     Position = new Vector4(12.5f, -1.5f, 12.5f, 1),
                                     Normal = faceNormal,
                                     UV = new Vector2(1.0f, 1.0f)
                                 })
        {
            Material = new MeshMaterial(_renderer.PsoFactory.GetPso("WaterDepth"))
            {
                Textures = {
                    [0] = _textureCache["UV"],
                    [1] = _textureCache["Water_Normal"],
                    [2] = _textureCache["Water_Specular"]
                }
            },
            Position = new Vector3(0, 0, 1.0f)
        };

        Plane plane = new(_graphics, new Vector2(25.0f, 25.0f), new RectangleF(0, 0, 1.0f, 1.0f), new Vector3(90, 0, 0), 32, 32)
        {
            Position = new Vector3(0, -1.5f, 1.0f),
            Material = new MeshMaterial(_renderer.PsoFactory.GetPso("WaterDepthStrip"))
            {
                Textures = {
                    [0] = _textureCache["UV"],
                    [1] = _textureCache["Water_Normal"],
                    [2] = _textureCache["Water_Specular"]
                }
            }
        };

        Cube cube = new(_graphics, new Vector3(1, 1, 1), new RectangleF(0, 0, 1.0f, 1.0f), new Vector3(45.0f, 0, 0))
        {
            Position = new Vector3(0, 0, 1.5f),
            Material = new MeshMaterial(_renderer.PsoFactory.GetPso("BumpDepth"))
            {
                SpecularPower = 0,
                Textures =
                        {
                            [0] = _textureCache["UV"],
                            [1] = _textureCache["GorgonNormalMap"]
                        }
            }
        };

        Sphere sphere = new(_graphics, 1.0f, new RectangleF(0.0f, 0.0f, 1.0f, 1.0f), Vector3.Zero, 64, 64)
        {
            Position = new Vector3(-2.0f, 1.0f, 0.75f),
            Material = new MeshMaterial(_renderer.PsoFactory.GetPso("LitDepth"))
            {
                SpecularPower = 0.75f,
                Textures = {
                    [0] = _textureCache["Earth"]
                }
            }
        };

        IcoSphere icoSphere = new(_graphics, 5.0f, new GorgonRectangleF(0, 0, 1, 1), Vector3.Zero, 3)
        {
            Rotation = new Vector3(0, -45.0f, 0),
            Position = new Vector3(10, 2, 9.5f),
            Material = new MeshMaterial(_renderer.PsoFactory.GetPso("BumpDepth"))
            {
                Textures = {
                    [0] = _textureCache["Earth"],
                    [1] = _textureCache["Earth_Normal"],
                    [2] = _textureCache["Earth_Specular"]
                }
            }
        };

        Sphere clouds = new(_graphics, 5.125f, new RectangleF(0.0f, 0.0f, 1.0f, 1.0f), Vector3.Zero, 16)
        {
            Position = new Vector3(10, 2, 9.5f),
            Material = new MeshMaterial(_renderer.PsoFactory.GetPso("LitAddBlendDepth"))
            {
                Textures = {
                    [0] = _textureCache["Clouds"]
                }
            }
        };

        _meshes.Add("Triangle", triangle);
        _renderer.AddMesh(triangle);
        _meshes.Add("Plane", plane);
        _renderer.AddMesh(plane);
        _meshes.Add("Cube", cube);
        _renderer.AddMesh(cube);
        _meshes.Add("Sphere", sphere);
        _renderer.AddMesh(sphere);
        _meshes.Add("IcoSphere", icoSphere);
        _renderer.AddMesh(icoSphere);
        _meshes.Add("Clouds", clouds);
        _renderer.AddMesh(clouds);

        _renderer.UploadMeshData();
    }

    /// <summary>
    /// Function to load the textures for the geometry.
    /// </summary>
    private void LoadTextures()
    {
        GorgonCodecDds dds = new();
        GorgonCodecPng png = new();
        GorgonCodecJpeg jpg = new();

        void LoadTexture(string fileName, string name, IGorgonImageCodec codec)
        {
            using IGorgonImage image = codec.FromFile(Path.Combine(GorgonExample.GetResourcePath(Path.Combine("Textures", "Primitives")).FullName, fileName));
            _textureCache[name] = IGorgonTextureView<GorgonTexture>.CreateTexture(_graphics, name, image);
        }

        LoadTexture("black_2x2.png", "Black", png);
        LoadTexture("UV.png", "UV", png);
        LoadTexture("earthmap1k.jpg", "Earth", jpg);
        LoadTexture("earthspec1k.jpg", "Earth_Specular", jpg);
        LoadTexture("earthbump1k_NRM.dds", "Earth_Normal", dds);
        LoadTexture("earthcloudmap.png", "Clouds", png);
        LoadTexture("normalmap.png", "GorgonNormalMap", png);
        LoadTexture("Rain_Height_NRM.dds", "Water_Normal", dds);
        LoadTexture("Rain_Height_SPEC.dds", "Water_Specular", dds);

        _renderer.DefaultTexture = _textureCache["Black"];
    }

    /// <summary>
    /// Function to load model, texture, and shader data for the scene.
    /// </summary>
    private void LoadData()
    {
        GorgonShader vertexShader = _compiler.Compile(Resources.Shaders, "PrimVS", ShaderType.VertexShader);
        GorgonShader litPixelShader = _compiler.Compile(Resources.Shaders, "PrimPS", ShaderType.PixelShader);
        GorgonShader bumpPixelShader = _compiler.Compile(Resources.Shaders, "PrimPSBump", ShaderType.PixelShader);
        GorgonShader waterPixelShader = _compiler.Compile(Resources.Shaders, "PrimPSWaterBump", ShaderType.PixelShader);

        GorgonGraphicsPsoBuilder psoBuilder = new(_graphics);

        psoBuilder.OutputFormat(BufferFormat.R8G8B8A8_UNorm)
                  .DepthStencilState(GorgonDepthStencilState.DepthEnabled, BufferFormat.D24_UNorm_S8_UInt)
                  .PixelShader(litPixelShader)
                  .BlendState(GorgonBlendState.AdditiveBlend);

        _renderer.PsoFactory.CreateOrGetPso("LitAddBlendDepth", vertexShader, psoBuilder);

        psoBuilder.BlendState(GorgonBlendState.NoBlending);

        _renderer.PsoFactory.CreateOrGetPso("LitDepth", vertexShader, psoBuilder);

        psoBuilder.PixelShader(bumpPixelShader);

        _renderer.PsoFactory.CreateOrGetPso("BumpDepth", vertexShader, psoBuilder);

        psoBuilder.PixelShader(waterPixelShader);

        _renderer.PsoFactory.CreateOrGetPso("WaterDepth", vertexShader, psoBuilder);

        psoBuilder.PrimitiveType(PrimitiveType.TriangleStrip)
                  .IndexBufferStripCutIdentifier(IndexBufferStripCutIdentifier.StopWith32BitMax);

        _renderer.PsoFactory.CreateOrGetPso("WaterDepthStrip", vertexShader, psoBuilder);

        LoadTextures();

        BuildGeometry();

        // Set up the lights.
        _renderer.AddLight(new GorgonPointLight()
        {
            Position = new Vector3(1.0f, 2.0f, -3.0f),
            SpecularPower = 256.0f,
            ConstantAttenuation = 1.0f,
            LinearAttenuation = 0.25f
        });

        _renderer.AddLight(new GorgonPointLight
        {
            Position = new Vector3(-5.0f, 5.0f, 4.5f),
            Color = GorgonColors.Yellow,
            SpecularPower = 1024.0f,
            ConstantAttenuation = 1.0f,
            LinearAttenuation = 0.09f,
            QuadraticAttenuation = 0.032f,
            Range = 50.0f
        });

        _renderer.AddLight(new GorgonPointLight
        {
            Position = new Vector3(5.0f, 5.25f, 9.5f),
            Color = GorgonColors.Red,
            SpecularPower = 0.0f,
            ConstantAttenuation = 0,
            QuadraticAttenuation = 0.25f
        });
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to run the application.
    /// </summary>
    public void Run()
    {
        GorgonExample.LoadResources(_graphics);

        LoadData();

        GorgonApplicationLoop appLoop = GorgonApplicationLoop.Create(GorgonExample.Log);

        try
        {
            _window.Resize += Window_Resize;
            _window.MouseDown += Window_MouseDown;
            _window.MouseWheel += Window_MouseWheel;

            appLoop.Run(Idle);

            GorgonExample.EndInit();

            Application.Run(_window);
        }
        finally
        {
            GorgonExample.ShutDown();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Primitives"/> class.
    /// </summary>
    /// <param name="graphicsFactory">The graphics factory used to create graphics objects.</param>
    /// <param name="adapter">The video adapter to use.</param>
    public Primitives(Form window, GorgonGraphicsFactory graphicsFactory, GorgonVideoAdapterInfo adapter)
    {
        _window = window;
        _graphics = graphicsFactory.CreateGraphics(adapter);
        _compiler = new GorgonShaderCompiler(_graphics);
        _renderer = new SimpleRenderer(_graphics, _window);
        _inputEvents = new GorgonInputEventBuffer();
        _input = GorgonInput.CreateInput(InputFlags.KeyboardAndMouse, GorgonExample.Log);
        _keyboard = new GorgonKeyboard();
        _mouse = new GorgonMouse();
    }
}
