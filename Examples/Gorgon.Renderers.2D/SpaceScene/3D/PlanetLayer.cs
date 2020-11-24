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
// Created: May 18, 2019 7:32:37 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Collections;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers;
using Gorgon.Timing;
using DX = SharpDX;

namespace Gorgon.Examples
{
    /// <summary>
    /// The layer responsible for rendering our planet entities.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the layer responsible for rendering our planet(s) using a 3D renderer.
    /// </para>
    /// <para>
    /// This kind of a mini 3D renderer, but I want to stress: <b>IN NO WAY</b> is this the best way to do this. And in fact, it's set up just for this example so many liberties and shortcuts were 
    /// taken. Please do not use this in your own code.
    /// </para>
    /// </remarks>
    internal class PlanetLayer
        : Layer, IDisposable
    {
        #region Constants.
        // The maximum number of available lights.
        private const int MaxLights = 8;
        #endregion

        #region Value Types.
        /// <summary>
        /// Material data.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 16, Size = 16)]
        private struct Material
        {
            /// <summary>
            /// The albedo color.
            /// </summary>
            public GorgonColor Albedo;

            /// <summary>
            /// The offset of the texture.
            /// </summary>
            public DX.Vector2 UVOffset;

            /// <summary>
            /// The specular power for the texture.
            /// </summary>
            public float SpecularPower;
        }

        /// <summary>
        /// Data for a light to pass to the GPU.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = 64, Pack = 4)]
        private struct LightData
        {
            /// <summary>
            /// Color of the light.
            /// </summary>
            public GorgonColor LightColor;

            /// <summary>
            /// Specular color for the light.
            /// </summary>
            public GorgonColor SpecularColor;

            /// <summary>
            /// Position of the light in world space.
            /// </summary>
            public DX.Vector3 LightPosition;

            /// <summary>
            /// Specular highlight power.
            /// </summary>
            public float SpecularPower;

            /// <summary>
            /// Attenuation falloff.
            /// </summary>
            public float Attenuation;

            /// <summary>
            /// The light intensity.
            /// </summary>
            public float Intensity;
        }
        #endregion

        #region Variables.
        // The application graphics interface.
        private readonly GorgonGraphics _graphics;
        // The application resources.
        private readonly ResourceManagement _resources;
        // The layout for a 3D vertex.
        private GorgonInputLayout _vertexLayout;
        // The pipeline state for rendering the planet.
        private readonly GorgonPipelineStateBuilder _stateBuilder;
        // The builder for create a draw call.
        private readonly GorgonDrawIndexCallBuilder _drawCallBuilder;
        // A constant buffer for holding the projection*view matrix.
        private GorgonConstantBufferView _viewProjectionBuffer;
        // A constant buffer for holding the world transformation matrix.
        private GorgonConstantBufferView _worldBuffer;
        // A constant buffer for holding the vectors used for the camera.
        private GorgonConstantBufferView _cameraBuffer;
        // A constant buffer for holding light information.
        private GorgonConstantBufferView _lightBuffer;
        // A constant buffer for holding material information.
        private GorgonConstantBufferView _materialBuffer;
        // The draw call to render.
        private List<GorgonDrawIndexCall> _drawCalls;
        // The light data to send to the constant buffer.
        private readonly LightData[] _lightData = new LightData[MaxLights];
        // The current view matrix, and projection matrix.  We'll pull these from our 2D camera.
        private DX.Matrix _viewMatrix;
        private DX.Matrix _projectionMatrix;
        // A combination of both matrices. This is calculated on every frame update when the view/projection is updated.
        private DX.Matrix _viewProjection;
        // Flag to indicate that we can draw the planet or not.
        private readonly List<Planet> _drawPlanets = new List<Planet>();
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return a list of 3D planets to render.
        /// </summary>
        public IList<Planet> Planets
        {
            get;
        } = new List<Planet>();
        #endregion

        #region Methods.
        /// <summary>
        /// Function to build up the constant buffer data for our shaders.
        /// </summary>
        private void BuildConstantBuffers()
        {
            DX.Matrix worldMatrix = DX.Matrix.Identity;
            DX.Vector3 cameraPos = _viewMatrix.TranslationVector;
            var emptyMaterial = new Material
            {
                Albedo = GorgonColor.White,
                SpecularPower = 1.0f,
                UVOffset = DX.Vector2.Zero
            };

            _viewProjectionBuffer = GorgonConstantBufferView.CreateConstantBuffer(_graphics, ref _viewProjection, "Projection/View Buffer", ResourceUsage.Dynamic);
            _cameraBuffer = GorgonConstantBufferView.CreateConstantBuffer(_graphics, ref cameraPos, "CameraBuffer", ResourceUsage.Dynamic);
            _worldBuffer = GorgonConstantBufferView.CreateConstantBuffer(_graphics, ref worldMatrix, "WorldBuffer", ResourceUsage.Dynamic);
            _materialBuffer = GorgonConstantBufferView.CreateConstantBuffer(_graphics, ref emptyMaterial, "MaterialBuffer", ResourceUsage.Dynamic);

            _lightBuffer = GorgonConstantBufferView.CreateConstantBuffer(_graphics,
                                                    new GorgonConstantBufferInfo("LightDataBuffer")
                                                    {
                                                        Usage = ResourceUsage.Default,
                                                        SizeInBytes = Unsafe.SizeOf<LightData>() * MaxLights
                                                    });
        }

        /// <summary>
        /// Function to update the material on the shader for the specified mesh.
        /// </summary>
        /// <param name="mesh">The mesh to evaluate.</param>
        private void UpdateMaterial(MoveableMesh mesh)
        {
            // Send the material data over to the shader.
            var materialData = new Material
            {
                Albedo = mesh.Material.Albedo,
                UVOffset = mesh.Material.TextureOffset,
                SpecularPower = mesh.Material.SpecularPower
            };

            _materialBuffer.Buffer.SetData(ref materialData);
        }

        /// <summary>
        /// Function to update the world matrix for the mesh for AABB calculations.
        /// </summary>
        /// <param name="mesh">The mesh to update.</param>
        private void UpdateMeshWorldMatrix(MoveableMesh mesh)
        {
            DX.Vector2 position = (DX.Vector2)mesh.Position + Offset;
            DX.Vector2 transformed = position;

            mesh.Position = new DX.Vector3(transformed / ParallaxLevel, mesh.Position.Z);
        }

        /// <summary>
        /// Function to send the current world matrix for a mesh to the shader.
        /// </summary>
        /// <param name="mesh">The mesh containing the world matrix.</param>
        private void UpdateWorldTransform(MoveableMesh mesh)
        {
            UpdateMeshWorldMatrix(mesh);
            _worldBuffer.Buffer.SetData(ref mesh.WorldMatrix);
        }

        /// <summary>Function called to update items per frame on the layer.</summary>
        protected override void OnUpdate()
        {
            if (Camera.NeedsUpdate)
            {
                if (Camera.ProjectionChanged)
                {
                    Camera.GetProjectionMatrix(out DX.Matrix projectionMatrix);
                    SetProjection(ref projectionMatrix);
                }

                if (Camera.ViewChanged)
                {
                    Camera.GetViewMatrix(out DX.Matrix viewMatrix);
                    SetView(ref viewMatrix);
                }
            }

            Camera.GetViewMatrix(out DX.Matrix _);

            _drawPlanets.Clear();

            for (int i = 0; i < Planets.Count; ++i)
            {
                Planet planet = Planets[i];
                planet.Update();

                DX.RectangleF aabb = DX.RectangleF.Empty;
                for (int j = 0; j < planet.Layers.Count; ++j)
                {
                    PlanetaryLayer layer = planet.Layers[j];
                    DX.Vector3 originalPosition = layer.Mesh.Position;

                    UpdateMeshWorldMatrix(layer.Mesh);

                    DX.RectangleF meshAabb = layer.Mesh.GetAABB();

                    if (!meshAabb.IsEmpty)
                    {
                        if (aabb.IsEmpty)
                        {
                            aabb = meshAabb;
                        }
                        else
                        {
                            aabb = DX.RectangleF.Union(meshAabb, aabb);
                        }
                    }

                    layer.Mesh.Position = originalPosition;
                }

                // Cull the planet if it's outside of our view.
                if (Camera.ViewableRegion.Intersects(aabb))
                {
                    _drawPlanets.Add(planet);
                }
            }
        }

        /// <summary>
        /// Function to render the layer data.
        /// </summary>
        public override void Render()
        {
            int drawCallIndex = 0;

            if (_drawPlanets.Count == 0)
            {
                return;
            }

            // Apply active lights
            Array.Clear(_lightData, 0, _lightData.Length);
            for (int i = 0; i < ActiveLights.Count.Min(_lightData.Length); ++i)
            {
                Light light = ActiveLights[i];

                if (light == null)
                {
                    continue;
                }

                _lightData[i] = new LightData
                {
                    SpecularPower = light.SpecularPower,
                    Attenuation = light.Attenuation,
                    LightColor = light.Color,
                    LightPosition = new DX.Vector3(light.Position.X, light.Position.Y, -light.Position.Z),
                    SpecularColor = GorgonColor.White,
                    Intensity = light.Intensity
                };
            }

            if (ActiveLights.Count > 0)
            {
                _lightBuffer.Buffer.SetData(_lightData);
            }

            for (int i = 0; i < _drawPlanets.Count; ++i)
            {
                Planet planet = _drawPlanets[i];
                for (int j = 0; j < planet.Layers.Count; ++j)
                {
                    PlanetaryLayer layer = planet.Layers[j];
                    MoveableMesh mesh = layer.Mesh;

                    UpdateMaterial(mesh);
                    UpdateWorldTransform(mesh);

                    _graphics.Submit(_drawCalls[drawCallIndex++]);
                }
            }
        }

        /// <summary>
        /// Function to load the resources for the layer.
        /// </summary>
        public override void LoadResources()
        {
            _drawCalls = new List<GorgonDrawIndexCall>();
            BuildConstantBuffers();

            for (int i = 0; i < Planets.Count; ++i)
            {
                Planet planet = Planets[i];

                for (int j = 0; j < planet.Layers.Count; ++j)
                {
                    PlanetaryLayer layer = planet.Layers[j];

                    GorgonVertexShader vertexShader = _resources.VertexShaders[layer.Mesh.Material.VertexShader];
                    GorgonPixelShader pixelShader = _resources.PixelShaders[layer.Mesh.Material.PixelShader];

                    // Create our vertex layout now.
                    if (_vertexLayout == null)
                    {
                        _vertexLayout = _vertexLayout = GorgonInputLayout.CreateUsingType<Vertex3D>(_graphics, vertexShader);
                    }

                    // Set up a pipeline state for the mesh.
                    GorgonPipelineState pipelineState = _stateBuilder.Clear()
                                                                     .PixelShader(pixelShader)
                                                                     .VertexShader(vertexShader)
                                                                     .BlendState(layer.Mesh.Material.BlendState)
                                                                     .PrimitiveType(PrimitiveType.TriangleList)
                                                                     .Build();
                    _drawCallBuilder.Clear()
                                .PipelineState(pipelineState)
                                .IndexBuffer(layer.Mesh.IndexBuffer, 0, layer.Mesh.IndexCount)
                                .VertexBuffer(_vertexLayout, new GorgonVertexBufferBinding(layer.Mesh.VertexBuffer, Vertex3D.Size))
                                .ConstantBuffer(ShaderType.Vertex, _viewProjectionBuffer)
                                .ConstantBuffer(ShaderType.Vertex, _worldBuffer, 1)
                                .ConstantBuffer(ShaderType.Pixel, _cameraBuffer)
                                .ConstantBuffer(ShaderType.Pixel, _lightBuffer, 1)
                                .ConstantBuffer(ShaderType.Pixel, _materialBuffer, 2);

                    (int startTexture, int textureCount) = layer.Mesh.Material.Textures.GetDirtyItems();

                    for (int k = startTexture; k < startTexture + textureCount; ++k)
                    {
                        GorgonTexture2DView texture = _resources.Textures[layer.Mesh.Material.Textures[k]].GetShaderResourceView();
                        _drawCallBuilder.ShaderResource(ShaderType.Pixel, texture, k);
                        // We should have this in the material, but since we know what we've got here, this will be fine.
                        _drawCallBuilder.SamplerState(ShaderType.Pixel, GorgonSamplerState.Wrapping, k);
                    }

                    _drawCalls.Add(_drawCallBuilder.Build());
                }
            }

            UpdateLightTransforms();
        }

        /// <summary>
        /// Function to assign a view matrix for the layer.
        /// </summary>
        /// <param name="view">The view (camera) matrix to assign.</param>
        /// <remarks>
        /// <para>
        /// This is necessary so we can project our 3D data into 2D space. We will be using the 2D renderer camera for this, so the matrix will be orthogonal. This can lead to some... er... 
        /// "interesting" issues.  But since we're not making a complex scene here, just set it as-is.
        /// </para>
        /// </remarks>
        private void SetView(ref DX.Matrix view)
        {
            _viewMatrix = view;
            DX.Matrix.Multiply(ref _viewMatrix, ref _projectionMatrix, out _viewProjection);

            _viewProjectionBuffer?.Buffer.SetData(ref _viewProjection);

            if (_cameraBuffer == null)
            {
                return;
            }

            DX.Vector3 cameraPos = view.TranslationVector;
            _cameraBuffer.Buffer.SetData(ref cameraPos);
        }

        /// <summary>
        /// Function to assign a projection matrix for the layer.
        /// </summary>
        /// <param name="view">The projection (camera) matrix to assign.</param>
        /// <param name="aspect">The aspect ratio.</param>
        /// <remarks>
        /// <para>
        /// This is necessary so we can project our 3D data into 2D space. We will be using the 2D renderer camera for this, so the matrix will be orthogonal. This can lead to some... er... 
        /// "interesting" issues.  But since we're not making a complex scene here, just set it as-is.
        /// </para>
        /// </remarks>
        private void SetProjection(ref DX.Matrix projection)
        {
            _projectionMatrix = projection;
            DX.Matrix.Multiply(ref _viewMatrix, ref _projectionMatrix, out _viewProjection);

            _viewProjectionBuffer?.Buffer.SetData(ref _viewProjection);
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            _materialBuffer?.Dispose();
            _viewProjectionBuffer?.Dispose();
            _worldBuffer?.Dispose();
            _cameraBuffer?.Dispose();
            _lightBuffer?.Dispose();
            _vertexLayout?.Dispose();
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="PlanetLayer"/> class.</summary>
        /// <param name="graphics">The graphics interface for the application.</param>
        /// <param name="resources">The resources for the application.</param>
        public PlanetLayer(GorgonGraphics graphics, ResourceManagement resources)
        {
            _graphics = graphics;
            _resources = resources;

            _stateBuilder = new GorgonPipelineStateBuilder(graphics);
            _drawCallBuilder = new GorgonDrawIndexCallBuilder();
        }
        #endregion
    }
}
