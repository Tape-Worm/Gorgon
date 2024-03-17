using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Renderers;
using Gorgon.Renderers.Cameras;
using Gorgon.Renderers.Geometry;
using Gorgon.Timing;
using Gorgon.UI;
using DX = SharpDX;

namespace Gorgon.Examples;

/// <summary>
/// The main application class
/// </summary>
internal static class Program
{
    /// <summary>
    /// GPU data for sending the world and WVP matrices.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct MatrixGpuData
    {
        /// <summary>
        /// The world matrix.
        /// </summary>
        public Matrix4x4 WorldMatrix;
        /// <summary>
        /// The world/view/projection matrix.
        /// </summary>
        public Matrix4x4 WvpMatrix;
    }

    /// <summary>
    /// GPU data for sending the material data to the GPU.
    /// </summary>
    [StructLayout(LayoutKind.Sequential)]
    private struct MaterialGpuData
    {
        /// <summary>
        /// The diffuse color.
        /// </summary>
        public GorgonColor Diffuse;
        /// <summary>
        /// The emissive color.
        /// </summary>
        public GorgonColor Emissive;
        /// <summary>
        /// The specular color.
        /// </summary>
        public GorgonColor Specular;
        /// <summary>
        /// The camera position.
        /// </summary>
        public Vector3 CameraPos;
    }



    // The form for the application.
    private static FormMain _mainForm;
    // The primary graphics interface.
    private static GorgonGraphics _graphics;
    // The 2D renderer.
    private static Gorgon2D _renderer2d;
    // The main swap chain.
    private static GorgonSwapChain _screen;
    // The model to display.
    private static Model _model;
    // Our primary vertex shader.
    private static GorgonVertexShader _vertexShader;
    // Our primary pixel shader.	
    private static GorgonPixelShader _pixelShader;
    // Input layout.
    private static GorgonInputLayout _inputLayout;
    // Our world/view/project matrix buffer.
    private static GorgonConstantBufferView _wvpBuffer;
    // Our material buffer.
    private static GorgonConstantBufferView _materialBuffer;
    // The camera.
    private static GorgonPerspectiveCamera _camera;
    // The draw calls for the model.
    private static readonly Dictionary<Material, GorgonDrawIndexCall> _drawCall = [];
    // The depth buffer.
    private static GorgonDepthStencil2DView _depthBuffer;
    // The list of textures for the model.
    private static readonly Dictionary<string, GorgonTexture2DView> _textureList = new(StringComparer.OrdinalIgnoreCase);
    // The material data to send to the GPU.
    private static MaterialGpuData _materialGpu = new();
    // The matrix data to the send to the GPU.
    private static MatrixGpuData _matrixGpu = new();



    /// <summary>Handles the SwapChainResized event of the Screen control.</summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="SwapChainResizedEventArgs" /> instance containing the event data.</param>
    private static void Screen_SwapChainResized(object sender, SwapChainResizedEventArgs e)
    {
        // If we resize the window, ensure that the depth buffer is resized as well.
        BuildDepthBuffer(e.Size.Width, e.Size.Height);
        _camera.ViewDimensions = new DX.Size2F(e.Size.Width, e.Size.Height);
    }

    /// <summary>
    /// Function to build or rebuild the depth buffer.
    /// </summary>
    /// <param name="width">The width of the depth buffer.</param>
    /// <param name="height">The height of the depth buffer.</param>
    private static void BuildDepthBuffer(int width, int height)
    {
        _graphics.SetDepthStencil(null);
        _depthBuffer?.Dispose();
        _depthBuffer = GorgonDepthStencil2DView.CreateDepthStencil(_graphics,
                                                                   new GorgonTexture2DInfo(width, height, BufferFormat.D24_UNorm_S8_UInt)
                                                                   {
                                                                       Name = "Depth Buffer",
                                                                       Usage = ResourceUsage.Default
                                                                   });
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
    private static void UpdateWVP(in Matrix4x4 world)
    {
        // Build our world/view/projection matrix to send to
        // the shader.
        ref readonly Matrix4x4 viewMatrix = ref _camera.GetViewMatrix();
        ref readonly Matrix4x4 projMatrix = ref _camera.GetProjectionMatrix();

        Matrix4x4 temp = Matrix4x4.Multiply(world, viewMatrix);
        Matrix4x4 wvp = Matrix4x4.Multiply(temp, projMatrix);

        ref Matrix4x4 gpuWorld = ref _matrixGpu.WorldMatrix;
        ref Matrix4x4 gpuWvp = ref _matrixGpu.WvpMatrix;

        gpuWorld = world;

        // Direct 3D 11 requires that we transpose our matrix 
        // before sending it to the shader.
        gpuWvp = Matrix4x4.Transpose(wvp);

        // Update the constant buffer.
        _wvpBuffer.Buffer.SetData(in _matrixGpu);
    }

    /// <summary>
    /// Function to handle idle time for the application.
    /// </summary>
    /// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
    private static bool Idle()
    {
        // Send our matrices to the GPU.
        UpdateWVP(in _model.GetWorldMatrix());

        // Animate the model.
        _model.RotateX = 15.0f;
        _model.RotateY += 90 * GorgonTiming.Delta;

        if (_model.RotateY > 360.0f)
        {
            _model.RotateY -= 360.0f;
        }

        _screen.RenderTargetView.Clear(GorgonColor.CornFlowerBlue);
        _depthBuffer.Clear(1.0f, 0);

        _graphics.SetRenderTarget(_screen.RenderTargetView, _depthBuffer);

        // Get a reference to the GPU data.
        ref GorgonColor matDiffuse = ref _materialGpu.Diffuse;
        ref GorgonColor matEmissive = ref _materialGpu.Emissive;
        ref GorgonColor matSpecular = ref _materialGpu.Specular;
        ref Vector3 matCamPos = ref _materialGpu.CameraPos;

        foreach (KeyValuePair<Material, List<(int baseStart, int start, int count)>> mesh in _model.Meshes)
        {
            // Get the appropriate draw call for the meshes.
            // We have sorted our draw calls by material in this case, so for each material 
            // we draw all meshes associated with it.
            //
            // In an actual 3D renderer you'd also want to draw by Z - front to back for 
            // materials without translucency (alpha) and z-writing turned on. And back to
            // front for those items with translucency, which z-writing turned off.
            if (!_drawCall.TryGetValue(mesh.Key, out GorgonDrawIndexCall drawCall))
            {
                continue;
            }

            // Update the material colors.
            matDiffuse = mesh.Key.Diffuse;
            matEmissive = mesh.Key.Emissive;
            matSpecular = new GorgonColor(mesh.Key.Specular, mesh.Key.SpecularPower);
            matCamPos = _camera.Position;

            // Send the material to the GPU.
            _materialBuffer.Buffer.SetData(in _materialGpu);

            for (int i = 0; i < mesh.Value.Count; ++i)
            {
                (int baseStart, int start, int count) = mesh.Value[i];
                drawCall.IndexStart = start;
                drawCall.IndexCount = count;
                drawCall.BaseVertexIndex = baseStart;

                _graphics.Submit(drawCall);
            }
        }

        GorgonExample.DrawStatsAndLogo(_renderer2d);

        _screen.Present();
        return true;
    }

    /// <summary>
    /// Function to initialize the states for the objects to draw.
    /// </summary>
    private static void InitializeStates()
    {
        GorgonDrawIndexCallBuilder drawBuilder = new();
        GorgonPipelineStateBuilder stateBuilder = new(_graphics);

        // This will initialize the 2D renderer early so we can get access to its default white texture.
        _renderer2d.Begin();
        _renderer2d.End();

        // We'll pre-bake our draw calls so that we can draw per-material/texture. In a real 3D renderer you'd want this be more flexible.
        foreach (Material material in _model.Meshes.Keys)
        {
            _drawCall[material] = drawBuilder.VertexBuffer(_inputLayout, new GorgonVertexBufferBinding(_model.VertexData, GorgonVertexPosNormColorUv.SizeInBytes))
                                             .IndexBuffer(_model.IndexData)
                                             .ConstantBuffer(ShaderType.Vertex, _wvpBuffer)
                                             .ShaderResource(ShaderType.Pixel, material.Texture is null ? _renderer2d.EmptyWhiteTexture : material.Texture)
                                             .SamplerState(ShaderType.Pixel, material.TextureSampler)
                                             .ConstantBuffer(ShaderType.Pixel, _materialBuffer)
                                             .PipelineState(stateBuilder.DepthStencilState(GorgonDepthStencilState.DepthStencilEnabled)
                                                                        .PrimitiveType(PrimitiveType.TriangleList)
                                                                        .PixelShader(_pixelShader)
                                                                        .VertexShader(_vertexShader))
                                             .Build();
        }

        // Set up our camera.
        _camera = new GorgonPerspectiveCamera(_graphics, new DX.Size2F(_screen.Width, _screen.Height), 0.125f, 500.0f)
        {
            Fov = 75,
            // Position the camera to center on the model using its AABB.
            Position = new Vector3(0, _model.MaxY * 0.5f, -(_model.MaxZ - _model.MinZ))
        };
    }

    /// <summary>
    /// Function to initialize the application.
    /// </summary>
    private static void Initialize()
    {
        GorgonExample.ResourceBaseDirectory = new DirectoryInfo(ExampleConfig.Default.ResourceLocation);

        try
        {
            // Create our form.
            _mainForm = GorgonExample.Initialize(new DX.Size2(ExampleConfig.Default.Resolution.Width, ExampleConfig.Default.Resolution.Height), "Asset Importer");

            // Find out which devices we have installed in the system.
            IReadOnlyList<IGorgonVideoAdapterInfo> deviceList = GorgonGraphics.EnumerateAdapters();

            if (deviceList.Count == 0)
            {
                GorgonDialogs.ErrorBox(_mainForm, "There are no suitable video adapters available in the system. This example is unable to continue and will now exit.");
                GorgonApplication.Quit();
                return;
            }

            _graphics = new GorgonGraphics(deviceList[0]);

            // Create a 1280x800 window with a depth buffer.
            // We can modify the resolution in the config file for the application, but like other Gorgon examples, the default is 1280x800.
            _screen = new GorgonSwapChain(_graphics,
                                          _mainForm,
                                          new GorgonSwapChainInfo(ExampleConfig.Default.Resolution.Width,
                                                                     ExampleConfig.Default.Resolution.Height,
                                                                     BufferFormat.R8G8B8A8_UNorm)
                                          {
                                              Name = "Main"
                                          });

            // Create a 2D renderer so we can draw information.
            _renderer2d = new Gorgon2D(_graphics);

            // Create our shaders.
            // Our vertex shader.  This is a simple shader, it just processes a vertex by multiplying it against
            // the world/view/projection matrix and spits it back out.
            _vertexShader = GorgonShaderFactory.Compile<GorgonVertexShader>(_graphics, Resources.Shader, "ModelVS");

            // Our main pixel shader.  This is a very simple shader, it just reads a texture and spits it back out.  Has no
            // diffuse capability.
            _pixelShader = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics, Resources.Shader, "ModelPS");

            // Create the vertex input layout.
            // We need to create a layout for our vertex type because the shader won't know how to interpret the data we're sending it otherwise.  
            // This is why we need a vertex shader before we even create the layout.
            _inputLayout = GorgonInputLayout.CreateUsingType<GorgonVertexPosNormColorUv>(_graphics, _vertexShader);

            // Create our constant buffers.			
            // Our constant buffers are how we send data to our shaders.  This one in particular will be responsible for sending our world/view/projection matrix 
            // to the vertex shader.  
            _wvpBuffer = GorgonConstantBufferView.CreateConstantBuffer(_graphics,
                                                                       new GorgonConstantBufferInfo(Unsafe.SizeOf<MatrixGpuData>())
                                                                       {
                                                                           Name = "WVPBuffer",
                                                                           Usage = ResourceUsage.Default
                                                                       });

            // This constant buffer will contain data used to update the material for each mesh
            _materialBuffer = GorgonConstantBufferView.CreateConstantBuffer(_graphics,
                                                                       new GorgonConstantBufferInfo(Unsafe.SizeOf<MatrixGpuData>())
                                                                       {
                                                                           Name = "MaterialBuffer",
                                                                           Usage = ResourceUsage.Dynamic
                                                                       });

            // Create a depth buffer so that the model draws correctly.
            BuildDepthBuffer(_screen.Width, _screen.Height);

            _model = Model.Load(_graphics, Path.Combine(GorgonExample.GetResourcePath(@"Models\AssImp").FullName, "NCC1701A.ms3d"), _textureList);

            // Set up stuff.
            InitializeStates();

            GorgonExample.LoadResources(_graphics);

            // Handle screen size transitions.
            _screen.SwapChainResized += Screen_SwapChainResized;
        }
        finally
        {
            GorgonExample.EndInit();
        }
    }

    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static void Main()
    {
        // To customize application configuration such as set high DPI settings or default font,
        // see https://aka.ms/applicationconfiguration.
        ApplicationConfiguration.Initialize();
        try
        {
            Initialize();
            GorgonApplication.Run(_mainForm, Idle);
        }
        finally
        {
            foreach (GorgonTexture2DView texture in _textureList.Values)
            {
                texture?.Dispose();
            }
            _materialBuffer?.Dispose();
            _wvpBuffer?.Dispose();
            _inputLayout?.Dispose();
            _pixelShader?.Dispose();
            _vertexShader?.Dispose();
            _model?.Dispose();
            _screen?.Dispose();
            _graphics?.Dispose();

            GorgonExample.UnloadResources();
        }
    }

}
