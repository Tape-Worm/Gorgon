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
// Created: July 31, 2026 12:40:51 AM
//

using System.Diagnostics;
using System.Media;
using System.Numerics;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.GdiPlus;
using Gorgon.Math;
using Gorgon.Timing;
using Gorgon.UI.WindowsForms;
using NAudio.Extras;
using NAudio.Wave;
using NAudio.Wave.SampleProviders;
using TerraFX.Interop.Windows;

namespace Gorgon.Examples;

/// <summary>
/// This is an example of using the base graphics API.  It's very similar to how Direct 3D 11 works, but with some enhancements
/// to deal with poor error support and other "gotchas" that tend to pop up.  It also has some time saving functionality to
/// deal with mundane tasks like setting up a swap chain, pixel shaders, etc..
/// 
/// This example is a recreation of the Amiga "Boing" demo (https://www.youtube.com/watch?v=8EpOq5H8wUI)
/// 
/// 
/// Before I go any further: This example is NOT a good example of how to write a 3D application.  
/// A good 3D renderer is a monster to write, this example just shows a user the flexibility of Gorgon and that it's capable 
/// of rendering 3D with the lower API level.  Any funky 3D only tricks or complicated scene graph mechanisms that you might 
/// expect are up to the developer to figure out and write
/// 
/// Anyway, on with the show...
/// 
/// In this example we create a swap chain, and set up the application for 3D rendering and build 2 types of objects:  
/// * 2 planes (1 for the floor and another for the rear wall)
/// * A sphere.  
/// 
/// Once the initialization is done, we render the objects.  We transform the sphere using a world matrix for its rotation,
/// translation and scaling.  Note that there's a shadow under the sphere, this is just the same sphere drawn again without
/// a texture and a diffuse hardcoded shader (see shader.hlsl).  To get the shadow in there, we turn off depth-writing, which
/// enables us to render the shadow without it interferring with any geometry but still respecting the depth buffer
/// 
/// One thing to note is the use of the 2D renderer for drawing text.  I had 2 options here:
/// 1. Draw the text manually myself.  And, there was no way in hell I was doing that
/// 2. Use the 2D renderer
/// 
/// You'll note that in the render loop, before we render the text, we call _2D.Begin().  This sets up the initial state for
/// 2D rendering.  Then we call the 2D functions to render a little window, and some text. And finally, we call _2D.End() and
/// that renders the batched 2D commands
/// 
/// This example is considered advanced, and a firm understanding of a graphics API like Direct 3D 11.2 is recommended
/// It's also very "low level", in that there's not a whole lot that's done for you by the API.  It's a very manual process to 
/// get everything initialized and thus there's a lot of set up code.  This is unlike the 2D renderer, which takes very little
/// effort to get up and running (technically, you barely have to touch the base graphics library to get the 2D renderer doing
/// something useful)
/// </summary>
internal class Boing
{
    /// <summary>
    /// This tells the GPU what vertex data to render, along with the matrix used to transform the vertex data on the GPU.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct RenderData
    {
        /// <summary>
        /// The world/view/projection matrix for rendering the geometry.
        /// </summary>
        public Matrix4x4 Wvp;
        /// <summary>
        /// The handle for the vertex buffer to use.
        /// </summary>
        public int VertexBufferHandle;
    }

    /// <summary>
    /// Data passed to the GPU via a constant buffer slot to tell the GPU what items to use for the current material when rendering.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct MaterialData
    {
        /// <summary>
        /// The diffuse color of the object.
        /// </summary>
        public GorgonColor Diffuse;
        /// <summary>
        /// The view handle for the texture.
        /// </summary>
        public int TextureHandle;
        /// <summary>
        /// The handle for the sampler to use.
        /// </summary>
        public int SamplerHandle;
    }

    // The main application form.
    private readonly FormMain _form;
    // The main application loop.
    private readonly GorgonApplicationLoop _loop;
    // The graphics interface to use.
    private readonly GorgonGraphics _graphics;
    // The swap chain for the application.
    private readonly GorgonSwapChain _swap;
    // The depth/stencil buffer to use.
    private GorgonDepthStencilView _depth;
    // The pipeline state for rendering.
    private readonly GorgonGraphicsPso _pipelineState;
    // The draw calls for the application.
    private readonly GorgonIndexedDrawCall[] _drawCalls;
    // The sphere model.
    private readonly Sphere _sphere;
    // The plane models.
    private readonly Plane[] _planes;
    // The material used for the sphere shadow.
    private readonly Material _shadowMaterial;
    // The default material for the geometry.
    private readonly Material _defaultMaterial;
    // The sound to play when the sphere hits the corners.
    private readonly CachedSound _sound;
    // The audio engine used to play the sound.
    private readonly AudioPlaybackEngine _audio;

    // The projection matrix.
    private Matrix4x4 _projection = Matrix4x4.Identity;
    // The world/view/projection matrix.
    private RenderData _renderData;
    // The material data
    private MaterialData _material;
    // Horizontal bounce.
    private static bool _bounceH;
    // Vertical bounce.
    private static bool _bounceV = true;
    // Ball rotation.
    private static float _rotate = -45.0f;
    // Rotation speed.
    private static float _rotateSpeed = 1.0f;
    // Drop speed.
    private static float _dropSpeed = 0.01f;
    // Flag to use orthographic projection.
    private bool _orthoProjection;


    #region Temp Code Until 2D is done.
    private readonly GorgonTimer _timer = new();

    private void UpdateFPS()
    {
        if (_timer.Milliseconds < 250)
        {
            return;
        }

        (long used, long budget) = _graphics.GetBudgetedVideoMemory();

        _form.Text = $"Boinger FPS:{GorgonTiming.AverageFPS:###,###,##0.0} DT: {(GorgonTiming.Delta * 1000):0.000#}  VRAM: {used.FormatMemory()}/{budget.FormatMemory()} ({_graphics.Adapter.Memory.Video.FormatMemory()})";
        _timer.Reset();
    }
    #endregion

    /// <summary>
    /// Function called when the window is resized.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void WindowResized(object? sender, EventArgs e)
    {
        if (sender is null)
        {
            return;
        }

        Form window = (Form)sender;

        if (window.WindowState == FormWindowState.Minimized)
        {
            return;
        }

        _depth.Dispose();

        _swap.Resize(window.Width, window.Height);
        _depth = GorgonDepthStencilView.CreateDepthStencilView(_graphics, "Boinger depth/stencil buffer", BufferFormat.D24_UNorm_S8_UInt, _swap.Width, _swap.Height);

        CalculateProjection();
    }

    /// <summary>
    /// Function called when a key is pressed.
    /// </summary>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void WindowKeyDown(object? sender, KeyEventArgs e)
    {
        if ((!e.Alt) || (e.KeyCode != Keys.Enter))
        {
            return;
        }

        if (_swap.IsWindowed)
        {
            _swap.EnterFullscreen();
        }
        else
        {
            _swap.ExitFullscreen();
        }

        // Refresh our depth buffer, we shouldn't need to, but let's just be safe for now.
        if ((_depth.Texture.Width != _swap.Width) || (_depth.Texture.Height != _swap.Height))
        {
            _depth.Dispose();
            _depth = GorgonDepthStencilView.CreateDepthStencilView(_graphics, "Boinger depth/stencil buffer", BufferFormat.D24_UNorm_S8_UInt, _swap.Width, _swap.Height);
        }

        CalculateProjection();
    }

    /// <summary>
    /// Function called when a key is pressed and released.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <param name="sender">The sender of the event.</param>
    /// <param name="e">The event parameters.</param>
    private void WindowKeyUp(object? sender, KeyEventArgs e)
    {
        if (e.KeyCode == Keys.A)
        {
            _orthoProjection = !_orthoProjection;
            CalculateProjection();
        }
    }

    /// <summary>
    /// Function to update the ball position.
    /// </summary>
    private void UpdateBall()
    {
        bool triggerSound = false;
        Vector3 position = _sphere.Position;

        if (_bounceV)
        {
            _dropSpeed += 9.8f * GorgonTiming.Delta;
        }
        else
        {
            _dropSpeed -= 9.8f * GorgonTiming.Delta;
        }

        if (!_bounceH)
        {
            position.X -= 4.0f * GorgonTiming.Delta;
        }
        else
        {
            position.X += 4.0f * GorgonTiming.Delta;
        }

        if (!_bounceV)
        {
            position.Y += 4.0f * GorgonTiming.Delta * (_dropSpeed / 20f);
        }
        else
        {
            position.Y -= 4.0f * GorgonTiming.Delta * (_dropSpeed / 20f);
        }

        if (position.X is < (-2.3f) or > 2.3f)
        {
            triggerSound = true;
            _bounceH = !_bounceH;
            if (_bounceH)
            {
                position.X = -2.3f;
            }
            else
            {
                position.X = 2.3f;
            }
        }

        if (position.Y is > 2.0f or < (-2.5f))
        {
            triggerSound = true;
            _bounceV = !_bounceV;
            if (!_bounceV)
            {
                position.Y = -2.5f;
                _dropSpeed = 20f;
            }
        }

        _sphere.Rotation = new Vector3(0, _rotate, -12.0f);
        _sphere.Position = position;

        _rotate += 90.0f * GorgonTiming.Delta * (_rotateSpeed.Sin() * 1.5f);
        _rotateSpeed += GorgonTiming.Delta / 1.25f;

        if (triggerSound)
        {
            _audio.PlaySound(_sound);
        }
    }

    /// <summary>
    /// Function called for idle-time processing.
    /// </summary>
    /// <returns><b>true</b> to continue processing, <b>false</b> to end processing.</returns>
    private bool Idle()
    {
        UpdateFPS();
        UpdateBall();

        _swap.WaitForFrameLatency();

        GorgonCommandList commandList = _graphics.GetCommandList("Main Boinger Command List")
                                                 .AddPresenter(_swap, 1)
                                                 .SetRenderTarget(_swap.Target, _depth)
                                                 .SetViewport(new GorgonViewport(0, 0, _swap.Width, _swap.Height)
                                                 {
                                                     MinimumDepth = 0,
                                                     MaximumDepth = 1.0f
                                                 })
                                                 .SetScissorRectangle(new GorgonRectangle(0, 0, _swap.Width, _swap.Height))
                                                 .ClearDepth(_depth, 1.0f)
                                                 .ClearRenderTarget(_swap.Target, new GorgonColor(0.678431f, 0.678431f, 0.678431f));

        for (int i = 0; i < _planes.Length; ++i)
        {
            RenderModel(commandList, _planes[i], i + 1);
        }

        Vector3 spherePosition = _sphere.Position;
        Vector3 sphereRotation = _sphere.Rotation;

        // Offset the position of the ball so we can fake a shadow under the ball.
        _sphere.Position = _orthoProjection ? new Vector3(spherePosition.X + 0.2f, spherePosition.Y - 0.2f, spherePosition.Z + 0.5f) 
                                            : new Vector3(spherePosition.X, spherePosition.Y - 0.125f, spherePosition.Z + 0.5f);
        // Scale on the z-axis so the ball "shadow" has no real depth, and on the x & y to make it look slightly bigger.
        if (!_orthoProjection)
        {
            _sphere.Scale = new Vector3(1.125f, 1.125f, 0.001f);
        }
        // Reset the rotation so we don't rotate our flattened ball "shadow" (it'd look real weird if it rotated).
        _sphere.Rotation = Vector3.Zero;
        // Render as black with alpha of 0.5 to simulate a shadow.
        _sphere.Material = _shadowMaterial;

        // Render the shadow.
        RenderModel(commandList, _sphere, 0);

        // Restore our original positioning so we can render the ball in the correct place.
        _sphere.Position = spherePosition;
        // Reset scale on the z-axis so the ball so it'll be normal.
        if (!_orthoProjection)
        {        
            _sphere.Scale = Vector3.One;
        }
        // Reset the rotation.
        _sphere.Rotation = sphereRotation;
        // Reset the sphere material.
        _sphere.Material = _defaultMaterial;

        RenderModel(commandList, _sphere, 0);

        GorgonExample.BlitLogo(commandList);

        _graphics.Submit(commandList);

        return true;
    }

    /// <summary>
    /// Function to render a model.
    /// </summary>
    /// <param name="list">The command list to use when rendering.</param>
    /// <param name="model">The model to render.</param>
    /// <param name="dcIndex">The index of the draw call.</param>
    private void RenderModel(GorgonCommandList list, Model model, int dcIndex)
    {
        Debug.Assert(model.VertexBufferView is not null, "The vertex buffer for the model is null.");

        _renderData.VertexBufferHandle = model.VertexBufferView.GetViewHandle();

        UpdateMaterial(model);
        UpdateWVP(in model.WorldMatrix);

        list.WriteConstant(0, in _renderData)
            .WriteConstant(1, in _material)
            .Draw(_drawCalls[dcIndex]);
    }

    /// <summary>
    /// Function to load the texture from an embedded image file.
    /// </summary>
    /// <param name="graphics">The graphics interface to use.</param>
    /// <returns>The texture and shader view for the renderer.</returns>
    private static IGorgonTextureView<GorgonTexture> LoadTexture(GorgonGraphics graphics)
    {
        using Bitmap gdiBitmap = Resources.Texture;
        using IGorgonImage image = gdiBitmap.ToGorgonImage();

        return IGorgonTextureView<GorgonTexture>.CreateTexture(graphics, "Boinger Texture", image);
    }

    /// <summary>
    /// Function to create the pipeline state object for the application.
    /// </summary>
    /// <param name="psoFactory">The factory used to create a pipeline state object.</param>
    /// <returns>The pipeline state object.</returns>
    private GorgonGraphicsPso CreatePso(GorgonGraphicsPsoFactory psoFactory)
    {
        // The first thing we need are shaders to draw our geometry.
        using GorgonShaderCompiler compiler = new(_graphics);

        CompileFlags compileFlags = _graphics.IsInDebugMode ? CompileFlags.Debug : CompileFlags.OptimizationLevel3;

        GorgonShader vertexShader = compiler.Compile(Resources.Shader, "BoingerVS", ShaderType.VertexShader, flags: compileFlags);
        GorgonShader pixelShader = compiler.Compile(Resources.Shader, "BoingerPS", ShaderType.PixelShader, flags: compileFlags);

        // Now we assemble the pipeline state to use for rendering.
        GorgonGraphicsPsoBuilder builder = new(_graphics);

        builder.PixelShader(pixelShader)
               .BlendState(GorgonBlendState.ModulatedColorBlend)
               .DepthStencilState(GorgonDepthStencilState.DepthLessEqualEnabled, _depth.Format)
               .OutputFormat(_swap.Format);

        // Now return the state of the pipeline for rendering.        
        return psoFactory.CreateOrGetPso("Boinger PSO", vertexShader, builder);
    }

    /// <summary>
    /// Function to create the sphere used to bounce around the screen.
    /// </summary>
    /// <param name="graphics">The graphics interface to use.</param>
    /// <param name="material">The material to apply to the sphere.</param>
    /// <returns>The sphere model.</returns>
    private static Sphere CreateSphere(GorgonGraphics graphics, Material material)
    {
        // Create our sphere.
        // Again, here we're using texels to align the texture coordinates to the other image packed into the texture (atlasing).  
        Vector2 textureOffset = material.Texture.Texture.ToTexel(new GorgonPoint(516, 0));
        // This is to scale our texture coordinates because the actual image is much smaller (255x255) than the full texture (1024x512).
        Vector2 textureSize = material.Texture.Texture.ToTexel(new GorgonPoint(255, 255));

        // Give the sphere a place to live.
        return new Sphere(graphics, 1.0f, textureOffset, textureSize)
        {
            Position = new Vector3(2.2f, 1.5f, 2.5f),
            Material = material
        };
    }

    /// <summary>
    /// Function to create the back, and bottom planes for the scene.
    /// </summary>
    /// <param name="graphics">The graphics interface to use.</param>
    /// <param name="material">The material to apply to the planes.</param>
    /// <returns>The plane models.</returns>
    private static Plane[] CreatePlanes(GorgonGraphics graphics, Material material)
    {
        // This is to scale our texture coordinates because the actual image is much smaller (255x255) than the full texture (1024x512).
        Vector2 textureSize = material.Texture.Texture.ToTexel(new GorgonPoint(511, 511));

        // And here we set up the planes with a material, and initial positioning.
        return [new Plane(graphics, new Vector2(3.5f), new GorgonRectangleF(0, 0, textureSize.X, textureSize.Y))
                          {
                              Material = material,
                              Position = new Vector3(0, 0, 3.0f)
                          },
                       new Plane(graphics, new Vector2(3.5f), new GorgonRectangleF(0, 0, textureSize.X, textureSize.Y))
                          {
                              Material = material,
                              Position = new Vector3(0, -3.5f, 3.5f),
                              Rotation = new Vector3(90.0f, 0, 0)
                          }
                  ];
    }

    /// <summary>
    /// Function to upload the geometry into the GPU.
    /// </summary>
    private void UploadGeometry()
    {
        GorgonCommandList list = _graphics.GetCommandList("Boinger upload list");

        _sphere.Upload(list);
        for (int i = 0; i < _planes.Length; ++i)
        {
            _planes[i].Upload(list);
        }

        _graphics.Submit(list);
        _graphics.WaitForGpu();
    }

    /// <summary>
    /// Function to build the draw calls for the objects to render.
    /// </summary>
    /// <param name="sphere">The sphere to render.</param>
    /// <param name="planes">The planes to render.</param>
    /// <param name="pipelineState">The pipeline state for each draw call.</param>
    /// <returns>The draw calls for the application to execute.</returns>
    private GorgonIndexedDrawCall[] CreateDrawCalls()
    {
        Debug.Assert(_sphere?.VertexBufferView is not null, "Vertex buffer not available for sphere.");
        Debug.Assert(_sphere?.IndexBuffer is not null, "Index buffer not available for sphere.");
        Debug.Assert(_sphere?.Material is not null, "The sphere material is null");

        GorgonIndexedDrawCall[] result = new GorgonIndexedDrawCall[_planes.Length + 1];

        result[0] = new(_sphere.IndexCount, _sphere.IndexBuffer, _pipelineState);

        result[0].AssignTexture(new GorgonUsedTexture(_sphere.Material.Texture, ShaderStage.Pixel, TextureUsage.ReadOnly));
        result[0].AssignBuffer(new GorgonUsedBuffer(_sphere.VertexBufferView, ShaderStage.Vertex, BufferUsage.VertexBuffer));

        for (int i = 0; i < _planes.Length; ++i)
        {
            Plane plane = _planes[i];

            Debug.Assert(plane.VertexBufferView is not null, $"Vertex buffer not available for plane {i}.");
            Debug.Assert(plane.IndexBuffer is not null, $"Index buffer not available for plane {i}.");
            Debug.Assert(plane.Material is not null, "The plane material is null");

            result[i + 1] = new(plane.IndexCount, plane.IndexBuffer, _pipelineState)
            {
                UsedBuffers =
                {
                    new GorgonUsedBuffer(plane.VertexBufferView, ShaderStage.Vertex, BufferUsage.VertexBuffer)
                },
                UsedTextures =
                {
                    new GorgonUsedTexture(plane.Material.Texture, ShaderStage.Pixel, TextureUsage.ReadOnly)
                }
            };
        }

        return result;
    }

    /// <summary>
    /// Function to update the projection matrix.
    /// </summary>
    private void CalculateProjection()
    {
        if (!_orthoProjection)
        {
            _projection = Matrix4x4.CreatePerspectiveFieldOfViewLeftHanded((80.0f).ToRadians(), (float)_swap.Width / _swap.Height, 0.1f, 1000f);
            return;
        }

        if (_swap.Width > _swap.Height)
        {
            _projection = Matrix4x4.CreateOrthographicLeftHanded(((float)_swap.Width / _swap.Height) * 8.0f, 8.0f, 0.1f, 1000.0f);
        }
        else
        {
            _projection = Matrix4x4.CreateOrthographicLeftHanded(13.0f, ((float)_swap.Height / _swap.Width) * 13.0f, 0.1f, 1000.0f);
        }
    }

    /// <summary>
    /// Function to update the material data from the specified model for rendering.
    /// </summary>
    private void UpdateMaterial(Model model)
    {
        Debug.Assert(model.VertexBufferView is not null, "No vertex buffer on model.");
        Debug.Assert(model.Material is not null, "No material on model.");

        ref MaterialData materialData = ref _material;

        materialData.Diffuse = model.Material.Diffuse;
        materialData.TextureHandle = model.Material.Texture.GetViewHandle();
        materialData.SamplerHandle = GorgonSampler.Default(_graphics).GetViewHandle();
    }

    /// <summary>
    /// Function to update the world/view/projection matrix.
    /// </summary>
    /// <param name="world">The world matrix to update.</param>
    /// <remarks>
    /// <para>
    /// This is what sends the transformation information for the model plus any view space transforms (projection & view) to the GPU so the shader can transform the vertices in the 
    /// model and project them into 2D space on your render target.
    /// </para>
    /// </remarks>
    private void UpdateWVP(ref readonly Matrix4x4 world)
    {
        // Build our world/view/projection matrix to send to
        // the shader.
        Matrix4x4 viewMatrix = Matrix4x4.CreateTranslation(0, 0, 2.2f);
        ref readonly Matrix4x4 projMatrix = ref _projection;

        Matrix4x4 temp = Matrix4x4.Multiply(world, viewMatrix);
        Matrix4x4 wvp = Matrix4x4.Multiply(temp, projMatrix);

        ref Matrix4x4 data = ref _renderData.Wvp;
        data = Matrix4x4.Transpose(wvp);
    }

    /// <summary>
    /// Function to execute the loop for the application.
    /// </summary>
    private void Run()
    {
        GorgonExample.LoadResources(_graphics);

        CalculateProjection();

        GorgonExample.EndInit();

        try
        {
            _form.Resize += WindowResized;
            _form.KeyDown += WindowKeyDown;
            _form.KeyUp += WindowKeyUp;

            _loop.Run(Idle);

            Application.Run(_form);
        }
        finally
        {
            _form.Resize -= WindowResized;
            _form.KeyDown -= WindowKeyDown;
            _form.KeyUp -= WindowKeyUp;

            // This can change during the lifetime of the application, so we dispose of it here.
            _depth.Dispose();
        }
    }    

    /// <summary>
    /// Function to run the application.
    /// </summary>
    /// <param name="form">The main form for the application.</param>
    /// <param name="graphics">The graphics interface for the application.</param>
    /// <param name="swap">The primary swap chain for the application.</param>
    public static void Run(FormMain form, GorgonGraphics graphics, GorgonSwapChain swap)
    {
        using GorgonApplicationLoop loop = GorgonApplicationLoop.Create(graphics.Log);
        using AudioPlaybackEngine audio = new();
        using GorgonGraphicsPsoFactory psoFactory = new(graphics);   
        using IGorgonTextureView<GorgonTexture> texture = LoadTexture(graphics);

        Material defaultMaterial = new(GorgonColors.White, texture, GorgonSampler.Default(graphics));

        using Sphere sphere = CreateSphere(graphics, defaultMaterial);        
        Plane[] planes = [];        

        try
        {
            planes = CreatePlanes(graphics, defaultMaterial);

            Boing program = new(form, graphics, swap, loop, audio, psoFactory, sphere, planes);            
            program.Run();
        }
        finally
        {
            for (int i = 0; i < planes.Length; i++)
            {
                planes[i].Dispose();
            }

            loop.Dispose();

            GorgonExample.ShutDown();
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Boing"/> class.
    /// </summary>
    /// <param name="form">The application window.</param>
    /// <param name="graphics">The graphics interface to use.</param>
    /// <param name="swap">The swap chain for the application.</param>
    /// <param name="loop">The application main loop.</param>
    /// <param name="audio">The audio engine used to play sound.</param>
    /// <param name="psoFactory">The factory used to create pipeline state objects.</param>
    /// <param name="sphere">The sphere model.</param>
    /// <param name="planes">The plane models.</param>
    private Boing(FormMain form, GorgonGraphics graphics, GorgonSwapChain swap, GorgonApplicationLoop loop, AudioPlaybackEngine audio,
                     GorgonGraphicsPsoFactory psoFactory, Sphere sphere, Plane[] planes)
    {
        Debug.Assert(sphere.Material is not null, "Sphere has no material");

        _form = form;
        _graphics = graphics;
        _swap = swap;
        _loop = loop;
        _audio = audio;
        _sphere = sphere;
        _planes = planes;

        // We create the depth/stencil here because it is transient.
        _depth = GorgonDepthStencilView.CreateDepthStencilView(graphics, "Boinger depth/stencil buffer", BufferFormat.D24_UNorm_S8_UInt, swap.Width, swap.Height);
        _pipelineState = CreatePso(psoFactory);
        _sound = new(GorgonExample.GetResourcePath("Boing.mp3").FullName);
        _defaultMaterial = sphere.Material;
        _shadowMaterial = _defaultMaterial with
        {
            Diffuse = new GorgonColor(0, 0, 0, 0.5f)
        };
        _drawCalls = CreateDrawCalls();
        UploadGeometry();
    }
}