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
// Created: July 30, 2017 1:49:31 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Collections;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using DX = SharpDX;

namespace Gorgon.Examples
{
    /// <summary>
    /// A simple renderer used to display and animate the scene data.
    /// </summary>
    internal class SimpleRenderer
        : IDisposable
    {
        #region Constants.
        // The maximum number of available lights.
        private const int MaxLights = 8;
        #endregion

        #region Value Types.
        /// <summary>
        /// View/projection matrices.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = 192)]
        private struct ViewProjectionData
        {
            /// <summary>
            /// The view matrix.
            /// </summary>
            public DX.Matrix View;

            /// <summary>
            /// The projection matrix.
            /// </summary>
            public DX.Matrix Projection;

            /// <summary>
            /// The view * project matrix.
            /// </summary>
            public DX.Matrix ViewProjection;
        }

        /// <summary>
        /// Camera data.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Size = 48, Pack = 4)]
        private struct CameraData
        {
            /// <summary>
            /// Camera position.
            /// </summary>
            public DX.Vector3 CameraPosition;

            /// <summary>
            /// Camera look at target.
            /// </summary>
            public DX.Vector3 CameraLookAt;

            /// <summary>
            /// Camera up vector.
            /// </summary>
            public DX.Vector3 CameraUp;
        }

        /// <summary>
        /// Material data.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 16, Size = 16)]
        private struct Material
        {
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
        }
        #endregion

        #region Variables.
        // The graphics interface to use.
        private readonly GorgonGraphics _graphics;
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
        // The light data to send to the constant buffer.
        private readonly LightData[] _lightData = new LightData[MaxLights];
        // The list of meshes to render.
        private readonly ObservableCollection<Mesh> _meshes = new ObservableCollection<Mesh>();
        // The draw calls for the available meshes.
        private readonly List<GorgonDrawIndexCall> _drawCalls = new List<GorgonDrawIndexCall>();
        // The layout of a vertex.
        private GorgonInputLayout _vertexLayout;
        // The default sampler state.
        private readonly GorgonSamplerState _defaultSampler;
        // The builder used to create draw calls.
        private readonly GorgonDrawIndexCallBuilder _drawBuilder;
        // The builder used to create pipeline state.
        private readonly GorgonPipelineStateBuilder _stateBuilder;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the list of meshes to render.
        /// </summary>
        public IList<Mesh> Meshes => _meshes;

        /// <summary>
        /// Property to return the lights used in rendering.
        /// </summary>
        public GorgonArray<Light> Lights
        {
            get;
        }

        /// <summary>
        /// Property to return the camera used for rendering.
        /// </summary>
        public Camera Camera
        {
            get;
            set;
        }

        /// <summary>
        /// Property to return the shaders loaded into the renderer.
        /// </summary>
        public Dictionary<string, GorgonShader> ShaderCache
        {
            get;
        }

        /// <summary>
        /// Property to return the textures loaded into the renderer.
        /// </summary>
        public Dictionary<string, GorgonTexture2DView> TextureCache
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to create a draw call based on a mesh.
        /// </summary>
        /// <param name="mesh">The mesh to evaluate.</param>
        private void AddDrawCall(Mesh mesh)
        {
            // Find out which shaders are used by the mesh and retrieve them from the cache.
            if (!ShaderCache.TryGetValue(mesh.Material.VertexShader, out GorgonShader shader))
            {
                return;
            }

            var vertexShader = (GorgonVertexShader)shader;

            if (!ShaderCache.TryGetValue(mesh.Material.PixelShader, out shader))
            {
                return;
            }

            // Not an ideal place for this, but since we don't know when a vertex shader will be added to our renderer, and we require a vertex shader for our 
            // input layout, then this will have to suffice.  In a real renderer, we'd cache the vertex inputs per shader and just look it up because the type 
            // of vertex may not be what we expect. In this case, we *know* that we're using Vertex3D and no other vertex type, so we're 100% safe doing it 
            // here in this example.
            if ((_vertexLayout == null) && (vertexShader != null))
            {
                _vertexLayout = GorgonInputLayout.CreateUsingType<Vertex3D>(_graphics, vertexShader);
            }

            var pixelShader = (GorgonPixelShader)shader;

            GorgonPipelineState pipelineState = _stateBuilder.Clear()
                                                             .DepthStencilState(mesh.IsDepthWriteEnabled
                                                                                    ? GorgonDepthStencilState.DepthEnabled
                                                                                    : GorgonDepthStencilState.DepthEnabledNoWrite)
                                                             .PixelShader(pixelShader)
                                                             .VertexShader(vertexShader)
                                                             .BlendState(mesh.Material.BlendState)
                                                             .PrimitiveType(mesh.PrimitiveType)
                                                             .Build();

            _drawBuilder.Clear()
                        .PipelineState(pipelineState)
                        .IndexBuffer(mesh.IndexBuffer, 0, mesh.IndexCount)
                        .VertexBuffer(_vertexLayout, new GorgonVertexBufferBinding(mesh.VertexBuffer, Vertex3D.Size))
                        .ConstantBuffer(ShaderType.Vertex, _viewProjectionBuffer)
                        .ConstantBuffer(ShaderType.Vertex, _worldBuffer, 1)
                        .ConstantBuffer(ShaderType.Pixel, _cameraBuffer)
                        .ConstantBuffer(ShaderType.Pixel, _lightBuffer, 1)
                        .ConstantBuffer(ShaderType.Pixel, _materialBuffer, 2);
                                                       
            ref readonly (int Start, int Count) textures = ref mesh.Material.Textures.GetDirtyItems();

            for (int i = textures.Start; i < textures.Start + textures.Count; ++i)
            {
                if (!TextureCache.TryGetValue(mesh.Material.Textures[i], out GorgonTexture2DView texture))
                {
                    continue;
                }

                _drawBuilder.ShaderResource(ShaderType.Pixel, texture, i - textures.Start)
                            .SamplerState(ShaderType.Pixel, _defaultSampler, i - textures.Start);
            }

            _drawCalls.Add(_drawBuilder.Build());
        }

        /// <summary>
        /// Handles the CollectionChanged event of the Meshes control.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">The <see cref="NotifyCollectionChangedEventArgs"/> instance containing the event data.</param>
        private void Meshes_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    AddDrawCall(e.NewItems.OfType<Mesh>().FirstOrDefault());
                    break;
                case NotifyCollectionChangedAction.Remove:
                    _drawCalls.RemoveAt(e.OldStartingIndex);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    _drawCalls.Clear();
                    break;
            }
        }

        /// <summary>
        /// Function to determine if any lights have a dirty property.
        /// </summary>
        /// <returns><b>true</b> if a light is dirty, or <b>false</b> if not.</returns>
        private bool LightPropsDirty()
        {
            if (Lights.IsDirty)
            {
                return true;
            }

            for (int i = 0; i < Lights.Length; ++i)
            {
                if ((Lights[i] != null) && (Lights[i].IsDirty))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Function to update the list of lights.
        /// </summary>
        private void UpdateLights()
        {
            if (!LightPropsDirty())
            {
                return;
            }

            ref readonly (int Start, int Count) dirtyLights = ref Lights.GetDirtyItems();

            int end = (dirtyLights.Start + dirtyLights.Count).Min(MaxLights);

            for (int i = dirtyLights.Start; i < end; ++i)
            {
                _lightData[i] = new LightData
                                {
                                    SpecularPower = Lights[i].SpecularPower,
                                    Attenuation = Lights[i].Attenuation,
                                    LightColor = Lights[i].LightColor,
                                    LightPosition = Lights[i].LightPosition,
                                    SpecularColor = Lights[i].SpecularColor
                                };

                Lights[i].IsDirty = false;
            }

            // Send to the constant buffer right away.
            _lightBuffer.Buffer.SetData(_lightData);
        }

        /// <summary>
        /// Function to initialize the constant buffers for this renderer.
        /// </summary>
        private void InitializeConstantBuffers()
        {

            _viewProjectionBuffer = new GorgonConstantBuffer(_graphics,
                                                             new GorgonConstantBufferInfo("Projection/View Buffer")
                                                             {
                                                                 SizeInBytes = Unsafe.SizeOf<ViewProjectionData>(),
                                                                 Usage = ResourceUsage.Dynamic
                                                             }).GetView();

            _worldBuffer = new GorgonConstantBuffer(_graphics,
                                                    new GorgonConstantBufferInfo("WorldBuffer")
                                                    {
                                                        Usage = ResourceUsage.Dynamic,
                                                        SizeInBytes = DX.Matrix.SizeInBytes
                                                    }).GetView();

            _cameraBuffer = new GorgonConstantBuffer(_graphics,
                                                     new GorgonConstantBufferInfo("CameraBuffer")
                                                     {
                                                         Usage = ResourceUsage.Dynamic,
                                                         SizeInBytes = Unsafe.SizeOf<CameraData>()
                                                     }).GetView();

            _materialBuffer = new GorgonConstantBuffer(_graphics,
                                                       new GorgonConstantBufferInfo("MaterialBuffer")
                                                       {
                                                           Usage = ResourceUsage.Dynamic,
                                                           SizeInBytes = Unsafe.SizeOf<Material>()
                                                       }).GetView();

            _lightBuffer = new GorgonConstantBuffer(_graphics,
                                                    new GorgonConstantBufferInfo("LightDataBuffer")
                                                    {
                                                        Usage = ResourceUsage.Dynamic,
                                                        SizeInBytes = Unsafe.SizeOf<LightData>() * MaxLights
                                                    }).GetView();

            // Initialize the constant buffers.
            var emptyViewProjection = new ViewProjectionData
                                      {
                                          Projection = DX.Matrix.Identity,
                                          View = DX.Matrix.Identity,
                                          ViewProjection = DX.Matrix.Identity
                                      };
            DX.Matrix emptyWorld = DX.Matrix.Identity;

            var emptyCamera = new CameraData
                              {
                                  CameraLookAt = new DX.Vector3(0, 0, -1.0f),
                                  CameraUp = new DX.Vector3(0, 1, 0),
                                  CameraPosition = DX.Vector3.Zero
                              };

            var emptyMaterial = new Material
                                {
                                    SpecularPower = 1.0f,
                                    UVOffset = DX.Vector2.Zero
                                };

            _viewProjectionBuffer.Buffer.SetData(ref emptyViewProjection);
            _worldBuffer.Buffer.SetData(ref emptyWorld);
            _cameraBuffer.Buffer.SetData(ref emptyCamera);
            _materialBuffer.Buffer.SetData(ref emptyMaterial);
        }

        /// <summary>
        /// Function to update the materials for a mesh.
        /// </summary>
        private void UpdateMaterials(MeshMaterial material)
        {
            var materialData = new Material
                                    {
                                        UVOffset = material.TextureOffset,
                                        SpecularPower = material.SpecularPower
                                    };
            _materialBuffer.Buffer.SetData(ref materialData);
        }

        /// <summary>
        /// Function to render the scene.
        /// </summary>
        public void Render()
        {
            if (Camera == null)
            {
                return;
            }

            UpdateLights();

            // Send the camera projection and view to the GPU.
            if ((Camera.IsViewDirty) || (Camera.IsProjectionDirty))
            {
                if (Camera.IsViewDirty)
                {
                    var camData = new CameraData
                                  {
                                      CameraLookAt = Camera.LookAt,
                                      CameraPosition = Camera.EyePosition,
                                      CameraUp = Camera.Up
                                  };
                    _cameraBuffer.Buffer.SetData(ref camData);
                }

                var viewProjData = new ViewProjectionData
                                   {
                                       Projection = Camera.GetProjectionMatrix(),
                                       View = Camera.GetViewMatrix(),
                                       ViewProjection = Camera.GetViewProjectionMatrix()
                                   };
                _viewProjectionBuffer.Buffer.SetData(ref viewProjData);
            }

            for (int i = 0; i < _drawCalls.Count; ++i)
            {
                if (_meshes[i] is MoveableMesh movableMesh)
                {
                    _worldBuffer.Buffer.SetData(ref movableMesh.WorldMatrix);
                }

                UpdateMaterials(_meshes[i].Material);
                _graphics.Submit(_drawCalls[i]);
            }
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            _materialBuffer?.Dispose();
            _viewProjectionBuffer?.Dispose();
            _worldBuffer?.Dispose();
            _cameraBuffer?.Dispose();
            _lightBuffer?.Dispose();
            _vertexLayout?.Dispose();

            foreach (KeyValuePair<string, GorgonShader> shader in ShaderCache)
            {
                shader.Value.Dispose();
            }

            foreach (KeyValuePair<string, GorgonTexture2DView> texture in TextureCache)
            {
                texture.Value.Dispose();
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="SimpleRenderer"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface to use for rendering.</param>
        public SimpleRenderer(GorgonGraphics graphics)
        {
            _graphics = graphics ?? throw new ArgumentNullException(nameof(graphics));

            _drawBuilder = new GorgonDrawIndexCallBuilder();
            _stateBuilder = new GorgonPipelineStateBuilder(_graphics);
            var sampleBuilder = new GorgonSamplerStateBuilder(_graphics);
            _defaultSampler = sampleBuilder.Wrapping(TextureWrap.Wrap, TextureWrap.Wrap).Build();

            TextureCache = new Dictionary<string, GorgonTexture2DView>(StringComparer.Ordinal);
            ShaderCache = new Dictionary<string, GorgonShader>(StringComparer.Ordinal);
            Lights = new GorgonArray<Light>(8);
            _meshes.CollectionChanged += Meshes_CollectionChanged;

            InitializeConstantBuffers();
        }
        #endregion
    }
}
