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
// Created: January 14, 2019 12:33:21 PM
// 
#endregion

using System;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Editor.ImageEditor.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Timing;
using DX = SharpDX;

namespace Gorgon.Editor.ImageEditor
{
    /// <summary>
    /// A renderer used to render volumetric (3D) textures.
    /// </summary>
    internal class VolumeRenderer
        : IDisposable
    {
        #region Value Types.
        /// <summary>
        /// The parameters for rendering the volume.
        /// </summary>
        [StructLayout(LayoutKind.Sequential, Pack = 1)]
        private struct VolumeRayParameters
        {
            /// <summary>
            /// The number of steps for ray marching.
            /// </summary>
            public DX.Vector3 Steps;
            /// <summary>
            /// The side to render, and number of iterations.
            /// </summary>
            public int Iterations;
        }
        #endregion

        #region Variables.
        // The graphics interface to use.
        private readonly GorgonGraphics _graphics;
        // The constant buffer holding the world/projection/view matrix transform.
        private GorgonConstantBuffer _cubeTransform;
        // Sections for the volumetric rendering.
        private readonly GorgonRenderTarget2DView[] _volumeRtSections = new GorgonRenderTarget2DView[3];
        private readonly GorgonTexture2DView[] _volumeSections = new GorgonTexture2DView[3];
        // Position rendering shader.
        private GorgonPixelShader _cubePosShader;
        // Direction rendering shader.
        private GorgonPixelShader _cubeDirShader;
        // The parameters for the volume ray casting.
        private GorgonConstantBuffer _volumeRayParams;
        // The parameters for the volume ray casting.
        private GorgonConstantBuffer _volumeScaleFactor;
        // A cube to render the volume into.
        private Cube _cube;
        // The layout used for the cube.
        private GorgonInputLayout _inputLayout;
        // Cube vertex shader.
        private GorgonVertexShader _cubeVs;
        // The draw call for the cube volume back face.
        private GorgonDrawIndexCall _cubePosDrawFrontCull;
        // The draw call for the cube volume front face.
        private GorgonDrawIndexCall _cubePosDrawCull;
        // The draw call for the cube.
        private GorgonDrawIndexCall _cubeDirDrawCall;
        // The angle of rotation for the cube.
        private float _rotationAngle;
        // The projection matrix.
        private DX.Matrix _projection;
        // The view matrix.
        private DX.Matrix _view;
        // The view for the volume texture.
        private GorgonTexture3DView _textureView;
        // The viewport for the cube.
        private DX.ViewportF _cubeView;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the region on the screen to render the volume.
        /// </summary>
        public DX.RectangleF VolumeRegion
        {
            get;
            private set;
        }
        #endregion

        #region Methods.

        /// <summary>
        /// Function to update the cube transform and send it to the GPU.
        /// </summary>
        private void UpdateCubeTransform()
        {
            _rotationAngle += GorgonTiming.Delta * 30.0f;

            if (_rotationAngle > 360.0f)
            {
                _rotationAngle = 360.0f - _rotationAngle;
            }

            // Build our world/view/projection matrix to send to
            // the shader.
            _cube.RotateXYZ(_rotationAngle, _rotationAngle, 0);

            DX.Matrix.Multiply(ref _cube.WorldMatrix, ref _view, out DX.Matrix temp);
            DX.Matrix.Multiply(ref temp, ref _projection, out DX.Matrix wvp);
            DX.Matrix.Transpose(ref wvp, out wvp);

            _cubeTransform.SetData(ref wvp);
        }

        /// <summary>
        /// Function to rebuild the volumetric data used to render the volume texture.
        /// </summary>
        private void RebuildVolumeData()
        {
            for (int i = 0; i < _volumeRtSections.Length; i++)
            {
                _volumeSections[i]?.Dispose();
                _volumeRtSections[i]?.Dispose();
                _volumeRtSections[i] = GorgonRenderTarget2DView.CreateRenderTarget(_graphics, new GorgonTexture2DInfo($"Vol_RTV_{i}")
                {
                    Binding = TextureBinding.ShaderResource,
                    Format = BufferFormat.R16G16B16A16_Float,
                    Width = (int)VolumeRegion.Width,
                    Height = (int)VolumeRegion.Height
                });
                _volumeSections[i] = _volumeRtSections[i].GetShaderResourceView();
            }

            var drawBuilder = new GorgonDrawIndexCallBuilder();
            _cubeDirDrawCall = drawBuilder
                .ResetTo(_cubeDirDrawCall)
                .ShaderResource(ShaderType.Pixel, _textureView, 0)
                .SamplerState(ShaderType.Pixel, GorgonSamplerState.Default, 0)
                .ShaderResource(ShaderType.Pixel, _volumeSections[0], 1)
                .SamplerState(ShaderType.Pixel, GorgonSamplerState.Default, 1)
                .ShaderResource(ShaderType.Pixel, _volumeSections[1], 2)
                .SamplerState(ShaderType.Pixel, GorgonSamplerState.Default, 2)
                .Build();
        }

        /// <summary>
        /// Function to resize the rendering region.
        /// </summary>
        /// <param name="clienSize">The size of the client area for the content view.</param>
        public void ResizeRenderRegion(DX.Size2 clientSize)
        {
            float newWidth = (clientSize.Width / 5.0f).Max(64).Min(640);
            float aspect = (float)clientSize.Height / clientSize.Width;
            var cubeRegionSize = new DX.Size2F(newWidth, newWidth * aspect);

            VolumeRegion = new DX.RectangleF(clientSize.Width - cubeRegionSize.Width - 1, 1, cubeRegionSize.Width, cubeRegionSize.Height);
            DX.Matrix.PerspectiveFovLH(60.0f.ToRadians(), (float)clientSize.Width / clientSize.Height, 0.1f, 1000.0f, out _projection);
            _cubeView = new DX.ViewportF(VolumeRegion.Left, VolumeRegion.Top, VolumeRegion.Width, VolumeRegion.Height, 0, 1);

            if (_textureView == null)
            {
                return;
            }

            RebuildVolumeData();
        }

        /// <summary>
        /// Function to assign the volume texture to render.
        /// </summary>
        /// <param name="texture">The volume texture to render.</param>
        public void AssignTexture(GorgonTexture3DView texture)
        {
            _textureView = texture;

            var size = new DX.Vector3(texture.Width, texture.Height, texture.Depth);
            float maxSize = texture.Width.Max(texture.Height).Max(texture.Depth);
            var volParams = new VolumeRayParameters
            {
                Steps = new DX.Vector3(1.0f / texture.Width,
                1.0f / texture.Height,
                1.0f / texture.Depth) * 0.5f,
                Iterations = (int)(maxSize * 2.0f)
            };
            _volumeRayParams.SetData(ref volParams);

            var scaleFactor = new DX.Vector4(1.0f, 1.0f, 1.0f / (maxSize / size.Z), 1.0f);
            _volumeScaleFactor.SetData(ref scaleFactor);

            RebuildVolumeData();
        }

        /// <summary>
        /// Function to perform the actual rendering.
        /// </summary>
        public void Render()
        {
            UpdateCubeTransform();

            // Draw the window with our volume texture mapped to a cube.
            GorgonRenderTargetView currentRtv = _graphics.RenderTargets[0];
            DX.ViewportF oldViewport = _graphics.Viewports[0];

            // Draw the volume sections.
            _graphics.SetRenderTarget(_volumeRtSections[0]);
            _volumeRtSections[0].Clear(GorgonColor.BlackTransparent);
            _graphics.Submit(_cubePosDrawCull);
            _graphics.SetRenderTarget(_volumeRtSections[1]);
            _volumeRtSections[1].Clear(GorgonColor.BlackTransparent);
            _graphics.Submit(_cubePosDrawFrontCull);
            _graphics.SetRenderTarget(_volumeRtSections[2]);

            // Draw the actual cube.
            _graphics.SetRenderTarget(currentRtv);
            _graphics.SetViewport(_cubeView);
            _graphics.Submit(_cubeDirDrawCall);
            _graphics.SetViewport(oldViewport);
        }

        /// <summary>
        /// Function used to build the resources required by the volume renderer.
        /// </summary>
        /// <param name="clientSize">The size of the content client area.</param>
        public void CreateResources(DX.Size2 clientSize)
        {
            _cubeVs = GorgonShaderFactory.Compile<GorgonVertexShader>(_graphics, Resources.VolumeRenderShaders, "VolumeVS", true);
            _cubePosShader = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics, Resources.VolumeRenderShaders, "VolumePositionPS", true);
            _cubeDirShader = GorgonShaderFactory.Compile<GorgonPixelShader>(_graphics, Resources.VolumeRenderShaders, "VolumeRayCastPS", true);
            _inputLayout = GorgonInputLayout.CreateUsingType<CubeVertex>(_graphics, _cubeVs);
            _cube = new Cube(_graphics, _inputLayout);

            _cubeTransform = new GorgonConstantBuffer(_graphics, new GorgonConstantBufferInfo
            {
                SizeInBytes = DX.Matrix.SizeInBytes
            });

            _volumeRayParams = new GorgonConstantBuffer(_graphics, new GorgonConstantBufferInfo
            {
                SizeInBytes = Unsafe.SizeOf<VolumeRayParameters>()
            });

            _volumeScaleFactor = new GorgonConstantBuffer(_graphics, new GorgonConstantBufferInfo
            {
                SizeInBytes = DX.Vector4.SizeInBytes
            });

            // Our camera is never changing, so we only need to define it here.
            DX.Matrix.Translation(0, 0, 1.5f, out _view);
            ResizeRenderRegion(clientSize);

            UpdateCubeTransform();

            var pipelineBuilder = new GorgonPipelineStateBuilder(_graphics);
            var drawBuilder = new GorgonDrawIndexCallBuilder();

            pipelineBuilder
                .PixelShader(_cubePosShader)
                .VertexShader(_cubeVs);

            // Position draw calls.
            _cubePosDrawCull = drawBuilder
                .ConstantBuffer(ShaderType.Vertex, _cubeTransform.GetView())
                .ConstantBuffer(ShaderType.Vertex, _volumeScaleFactor.GetView(), 1)
                .PipelineState(pipelineBuilder)
                .IndexBuffer(_cube.IndexBuffer, indexCount: _cube.IndexBuffer.IndexCount)
                .VertexBuffer(_inputLayout, _cube.VertexBuffer[0])
                .Build();

            pipelineBuilder
                .RasterState(GorgonRasterState.CullFrontFace);

            _cubePosDrawFrontCull = drawBuilder
                .PipelineState(pipelineBuilder)
                .Build();

            // Raycasting draw call.
            pipelineBuilder
                .PixelShader(_cubeDirShader)
                .RasterState(GorgonRasterState.Default);

            _cubeDirDrawCall = drawBuilder
                .ConstantBuffer(ShaderType.Pixel, _volumeRayParams.GetView(), 0)
                .PipelineState(pipelineBuilder)
                .Build();
        }

        /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
        public void Dispose()
        {
            for (int i = 0; i < _volumeRtSections.Length; ++i)
            {
                _volumeSections[i]?.Dispose();
                _volumeRtSections[i]?.Dispose();
            }
            _textureView = null;

            _cube?.Dispose();
            _volumeScaleFactor?.Dispose();
            _volumeRayParams?.Dispose();
            _cubeTransform?.Dispose();
            _cubePosShader?.Dispose();
            _cubeDirShader?.Dispose();
            _cubeVs?.Dispose();
            _inputLayout?.Dispose();

            _cubeTransform = null;
            _inputLayout = null;
            _cube = null;
            _volumeScaleFactor = null;
            _volumeRayParams = null;
            _cubePosShader = null;
            _cubeDirShader = null;
            _cubeVs = null;
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>Initializes a new instance of the <see cref="VolumeRenderer"/> class.</summary>
        /// <param name="graphics">The graphics interface to use.</param>
        public VolumeRenderer(GorgonGraphics graphics) => _graphics = graphics;
        #endregion
    }
}
