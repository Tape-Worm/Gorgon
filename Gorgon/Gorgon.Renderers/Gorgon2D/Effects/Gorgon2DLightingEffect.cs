#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: July 25, 2018 7:36:58 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Renderers.Properties;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// An effect used to render a scene with per-pixel lighting.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This effect simulates lighting on 2D rendering by using a normal map (and specular map) to determine how to render a pixels shading. It does this by using a G-Buffer which contains a render target 
    /// for the diffuse (unlit color), specular, and normal map data.
    /// </para>
    /// <para>
    /// The effect renders the lighting data in 2 passes. The first renders the diffuse layer using a callback defined by the user to render their objects that they wish to have lit.  Then, the 2nd pass 
    /// combines all the layers of the G-buffer together using additive blending for each <see cref="Lights">light</see> defined and returns the combined lighting data to a render target specified by the 
    /// user. This output can then be combined with the scene using additive blending to produce the desired lighting effect. 
    /// </para>
    /// <para>
    /// <note type="information">
    /// <para>
    /// Please note that this last compositing pass is not done by the effect and must be handled by the user. This is done in the interest of flexibility to allow the user to decide how to best handle 
    /// the compositing of their scene.
    /// </para>
    /// </note>
    /// </para>
    /// <para>
    /// In order for a sprite, or other 2D graphics object to be rendered using lighting, the backing texture must use a texture with an array count of 3 or higher. Each index of the array corresponds to a
    /// layer used by the G-buffer to composite the lighting together. These array indices must be in the following order (while the word sprite is used, this can apply to anything that is textured):
    /// <list type="number">
    /// <item>
    ///		<description>The diffuse layer for the sprite - This is just the unlit colors for your sprite, basically the standard texture you'd normally use. Ideally, this texture should not contain 
    ///		any shading at all as this will be handled by the effect.</description>
    /// </item>
    /// <item>
    ///		<description>The specular layer for the sprite - This is the texture array index that controls which parts of the sprite are "shiny". Leaving this black will produce no specular hilighting at 
    ///		all, while pure white will make all parts of it shiny. This amount of specular is controlled by the <see cref="Gorgon2DLight.SpecularPower"/> property on a light.</description>
    /// </item>
    /// <item>
    ///		<description>The normal map layer for the sprite - This is the texture array index that provides normals for the lighting calculations. This layer must have data in it or else no lighting will 
    ///		be applied. Gorgon does not generate normal map for your texture, however there are a multitude of tools available online to help with this (e.g. CrazyBump, SpriteIlluminator, etc...).</description>
    /// </item>
    /// </list>
    /// </para>
    /// <para>
    /// The user must also supply at least a single light source to effectively view the lighting on the 2D object.
    /// </para>
    /// </remarks>
    public class Gorgon2DLightingEffect
        : Gorgon2DEffect
    {
        #region Value Types.
        // Constant buffer data for a light.
        [StructLayout(LayoutKind.Sequential, Size = 64)]
        private struct LightData
        {
            /// <summary>
            /// Position for the light in screen space.
            /// </summary>
            public DX.Vector4 Position;
            /// <summary>
            /// Color of the light.
            /// </summary>
            public GorgonColor Color;
            /// <summary>
            /// The direction for the light.
            /// </summary>
            public DX.Vector4 LightDirection;
            /// <summary>
            /// Various attributes (x = Specular power, y = Attenuation, z = Intensity, w = Specular enabled flag).
            /// </summary>
            public DX.Vector4 LightAttribs;
        }

        // Constant buffer data for global data.
        [StructLayout(LayoutKind.Sequential, Size = 48, Pack = 16)]
        private struct GlobalEffectData
        {
            /// <summary>
            /// The ambient color.
            /// </summary>
            public GorgonColor AmbientColor;
            /// <summary>
            /// Position for the light in screen space.
            /// </summary>
            public DX.Vector4 CameraPosition;
            /// <summary>
            /// The texture array indices to use for the normal map, and specular map.
            /// </summary>
            public DX.Vector4 ArrayIndices;
        }
        #endregion

        #region Constants.
        /// <summary>
        /// The name of the shader include for Gorgon's <see cref="Gorgon2DLightingEffect"/>.
        /// </summary>
        public const string Gorgon2DLightingShaderIncludeName = "LightingShaders";
        /// <summary>
        /// The maximum number of lights that can be used at one time.
        /// </summary>
        public const int MaxLightCount = 256;
        #endregion

        #region Variables.        
        // Our custom vertex shader for per pixel lighting.
        private GorgonVertexShader _vertexLitShader;
        private Gorgon2DShaderState<GorgonVertexShader> _vertexLitShaderState;
        // A pixel shader used to render lights.
        private GorgonPixelShader _pixelLitShader;
        private Gorgon2DShaderState<GorgonPixelShader> _pixelLitShaderState;
        // The buffer that will hold the lighting data for an individual light.
        private GorgonConstantBufferView _lightBuffer;
        // The buffer that will hold the global effect data.
        private GorgonConstantBufferView _globalBuffer;
        // The batch render state for drawing with lights.
        private Gorgon2DBatchState _lightingState;
        // The texture holding normal maps.
        private GorgonTexture2DView _normalTexture;
        // The texture holding specular maps.
        private GorgonTexture2DView _specularTexture;
        // The light data structure to send to the constant buffer.
        private readonly LightData[] _lightData = new LightData[MaxLightCount];
        // The last light count.
        private int _lastLightCount;
        // The macro to pass in when using array indices instead of separate textures.
        private readonly GorgonShaderMacro _arrayMacro = new GorgonShaderMacro("USE_ARRAY");
        // The data to pass to the effect.
        private GlobalEffectData _effectData = new GlobalEffectData
        {
            AmbientColor = GorgonColor.Black
        };
        // Flag to indicate whether separate textures are being used or array indices.
        private bool _usingArray;
        // The currently active render target view.
        private GorgonRenderTargetView _currentRtv;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the distance on the Z axis for specular hilights.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This is the distance of the viewer from the scene on the z axis. If a camera is in use, the Z position of the camera can be used here since that is the actual distance of the viewer from the 
        /// scene. Otherwise, it can be set to an arbitrary value to mimic view depth when calculating specular.  
        /// </para>
        /// <para>
        /// This value only applies when a light has <see cref="Gorgon2DLight.SpecularEnabled"/> set to <b>true</b>. For lights without specular, then property is not used.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// If this value is too small, strange artifacts may appear as the Z value gets closer to the drawing plane (Z = 0.0f).
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public float SpecularZDistance
        {
            get;
            set;
        } = -127.0f;

        /// <summary>
        /// Property to set or return the global ambient color.
        /// </summary>
        public GorgonColor AmbientColor
        {
            get => _effectData.AmbientColor;
            set => _effectData.AmbientColor = value;
        }

        /// <summary>
        /// Property to return the list of point lights for rendering.
        /// </summary>
        public IList<Gorgon2DLight> Lights
        {
            get;
        } = new List<Gorgon2DLight>(MaxLightCount);
        #endregion

        #region Methods.
        /// <summary>
        /// Function to update the light data in the lighting shader.
        /// </summary>
        /// <param name="index">The index of the light.</param>
        /// <param name="light">The light to retrieve data from.</param>
        /// <param name="camera">The currently active camera.</param>
        /// <param name="viewMatrix">The view matrix used to transform the light position.</param>
        private void UpdateLight(int index, Gorgon2DLight light, IGorgon2DCamera camera, ref DX.Matrix viewMatrix)
        {
            var lightPos = new DX.Vector3(light.Position.X, light.Position.Y, -light.Position.Z);

            if ((camera != null) && (light.LightType == LightType.Point))
            {
                DX.Vector3.Transform(ref lightPos, ref viewMatrix, out DX.Vector4 temp);

                // Offset the position by the anchor, if not, our objects in the view will be off.
                var anchorPos = new DX.Vector3(camera.ViewableRegion.Width * -camera.Anchor.X, camera.ViewableRegion.Height * -camera.Anchor.Y, 0);

                lightPos.X = temp.X - anchorPos.X;
                lightPos.Y = temp.Y - anchorPos.Y;
            }
            
            _lightData[index] = new LightData
            {
                Position = new DX.Vector4(lightPos, (int)light.LightType),
                Color = light.Color,
                LightDirection = new DX.Vector4(new DX.Vector3(light.LightDirection.X, light.LightDirection.Y, -light.LightDirection.Z), 0),
                LightAttribs = new DX.Vector4(light.SpecularPower, light.Attenuation, light.Intensity, light.SpecularEnabled ? 1 : 0),
            };
        }

        /// <summary>
        /// Function to begin actual rendering of the effect.
        /// </summary>
        /// <param name="blendState">A user defined blend state to apply when rendering.</param>
        /// <param name="depthStencilState">A user defined depth/stencil state to apply when rendering.</param>
        /// <param name="rasterState">A user defined rasterizer state to apply when rendering.</param>
        /// <param name="camera">The camera used to transform the lights to camera space.</param>
        /// <returns>A flag indicate whether to continue rendering or not.</returns>
        private PassContinuationState OnBeginRender(GorgonBlendState blendState, GorgonDepthStencilState depthStencilState, GorgonRasterState rasterState, IGorgon2DCamera camera)
        {
            DX.Matrix viewMatrix = DX.Matrix.Identity;

            if (Lights.Count == 0)
            {
                return PassContinuationState.Stop;
            }

            if (camera != null)
            {
                camera.GetViewMatrix(out viewMatrix);
            }

            BeginRender(_currentRtv, blendState, depthStencilState, rasterState);

            for (int i = 0; i < Lights.Count.Min(MaxLightCount); ++i)
            {
                Gorgon2DLight light = Lights[i];

                if (light == null)
                {
                    continue;
                }

                UpdateLight(i, light, camera, ref viewMatrix);
            }

            _lightBuffer.Buffer.SetData(_lightData, count: Lights.Count);

            return BeginPass(0, _currentRtv);
        }

        /// <summary>
        /// Function to perform the actual rendering of the effect.
        /// </summary>
        /// <param name="diffuse">The diffuse texture to render.</param>
        /// <param name="output">The final output target for the effect.</param>
        /// <param name="camera">The camera used to transform the lights to camera space.</param>
        private void OnRender(GorgonTexture2DView diffuse, GorgonRenderTargetView output, IGorgon2DCamera camera)
        {
            if (OnBeginRender(null, null, null, camera) != PassContinuationState.Continue)
            {
                return;
            }

            Renderer.DrawFilledRectangle(new DX.RectangleF(0, 0, output.Width, output.Height),
                                        GorgonColor.White,
                                        diffuse,
                                        new DX.RectangleF(0, 0, 1, 1),
                                        textureSampler: GorgonSamplerState.Default);
            End();        
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected override void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            GorgonPixelShader lightShader = Interlocked.Exchange(ref _pixelLitShader, null);
            GorgonConstantBufferView lightData = Interlocked.Exchange(ref _lightBuffer, null);
            GorgonConstantBufferView globalData = Interlocked.Exchange(ref _globalBuffer, null);

            globalData?.Dispose();
            lightData?.Dispose();
            lightShader?.Dispose();
        }

        /// <summary>
        /// Function called to build a new (or return an existing) 2D batch state.
        /// </summary>
        /// <param name="passIndex">The index of the current rendering pass.</param>
        /// <param name="builders">The builder types that will manage the state of the effect.</param>
        /// <param name="statesChanged"><b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
        /// <returns>The 2D batch state.</returns>
        protected override Gorgon2DBatchState OnGetBatchState(int passIndex, IGorgon2DEffectBuilders builders, bool statesChanged)
        {
            if (_vertexLitShaderState == null)
            {
                _vertexLitShaderState = builders.VertexShaderBuilder.Shader(_vertexLitShader)
                                                   .Build(VertexShaderAllocator);
            }

            // If we added a light and we've exceeded the buffer size, then we need to recompile.
            if ((_lastLightCount < Lights.Count) || (_pixelLitShader == null))
            {
                int newLightBufferSize = ((Lights.Count + 7) & ~7).Min(MaxLightCount);
                Macros.Clear();
                Macros.Add(new GorgonShaderMacro("MAX_LIGHTS", newLightBufferSize.ToString()));

                if (_usingArray)
                {
                    Macros.Add(new GorgonShaderMacro("USE_ARRAY"));
                }                

                _pixelLitShader?.Dispose();
                _pixelLitShader = CompileShader<GorgonPixelShader>(Resources.Lighting, "GorgonPixelShaderLighting");
                _lastLightCount = newLightBufferSize;

                _pixelLitShaderState = null;
            }

            if ((statesChanged) || (_pixelLitShaderState == null))
            {
                builders.PixelShaderBuilder.Clear()
                                           .Shader(_pixelLitShader)
                                           .ConstantBuffer(_lightBuffer, 1)
                                           .ConstantBuffer(_globalBuffer, 2)
                                           .SamplerState(GorgonSamplerState.Default, 0)
                                           .SamplerState(GorgonSamplerState.PointFilteringWrapping, 1)
                                           .SamplerState(GorgonSamplerState.Wrapping, 2);

                if (!_usingArray)
                {
                    builders.PixelShaderBuilder.ShaderResource(_normalTexture ?? Renderer.EmptyNormalMapTexture, 1)
                                               .ShaderResource(_specularTexture ?? Renderer.EmptyBlackTexture, 2);

                    builders.BatchBuilder.Clear()
                                         .BlendState(GorgonBlendState.NoBlending)
                                         .DepthStencilState(GorgonDepthStencilState.Default)
                                         .RasterState(GorgonRasterState.Default);
                }

                _pixelLitShaderState = builders.PixelShaderBuilder.Build(PixelShaderAllocator);
                _lightingState = null;
            }

            if (_lightingState == null)
            {
                _lightingState = builders.BatchBuilder
                                         .PixelShaderState(_pixelLitShaderState)
                                         .VertexShaderState(_vertexLitShaderState)
                                         .Build(BatchStateAllocator);
            }

            return _lightingState;
        }

        /// <summary>Function called prior to rendering.</summary>
        /// <param name="output">The final render target that will receive the rendering from the effect.</param>
        /// <param name="sizeChanged">
        ///   <b>true</b> if the output size changed since the last render, or <b>false</b> if it's the same.</param>
        /// <remarks>
        /// Applications can use this to set up common states and other configuration settings prior to executing the render passes. This is an ideal method to initialize and resize your internal render
        /// targets (if applicable).
        /// </remarks>
        protected override void OnBeforeRender(GorgonRenderTargetView output, bool sizeChanged)
        {
            if (Graphics.RenderTargets[0] != output)
            {
                Graphics.SetRenderTarget(output, Graphics.DepthStencilView);
            }
        }

        /// <summary>Function called prior to rendering a pass.</summary>
        /// <param name="passIndex">The index of the pass to render.</param>
        /// <param name="output">The final render target that will receive the rendering from the effect.</param>
        /// <param name="camera">The currently active camera.</param>
        /// <returns>A <see cref="PassContinuationState"/> to instruct the effect on how to proceed.</returns>
        /// <remarks>
        ///   <para>
        /// Applications can use this to set up per-pass states and other configuration settings prior to executing a single render pass.
        /// </para>
        /// </remarks>
        /// <seealso cref="PassContinuationState"/>
        protected override PassContinuationState OnBeforeRenderPass(int passIndex, GorgonRenderTargetView output, IGorgon2DCamera camera)
        {
            var cameraPos = new DX.Vector4(output.Width * 0.5f, output.Height * 0.5f, SpecularZDistance, 0);

            // If no custom camera is in use, we need to pass in our default viewing information which is normally the output width, and height (by half), and an arbitrary Z value so 
            // the camera position isn't intersecting with the drawing plane (+ height information). Otherwise, our specular hilight will look really messed up.
            if (camera != null)
            {
                cameraPos = new DX.Vector4(camera.Position.X + camera.ViewableRegion.Width * 0.5f, camera.Position.Y + camera.ViewableRegion.Height * 0.5f, camera.Position.Z, 0);
            }

            _effectData.CameraPosition = cameraPos;

            _globalBuffer.Buffer.SetData(ref _effectData);

            return PassContinuationState.Continue;
        }


        /// <summary>
        /// Function called to initialize the effect.
        /// </summary>
        /// <remarks>Applications must implement this method to ensure that any required resources are created, and configured for the effect.</remarks>
        protected override void OnInitialize()
        {
            _globalBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics,
                                                                          new GorgonConstantBufferInfo("Global light effect data.")
                                                                          {
                                                                              SizeInBytes = Unsafe.SizeOf<GlobalEffectData>(),
                                                                              Usage = ResourceUsage.Dynamic
                                                                          });

            _lightBuffer = GorgonConstantBufferView.CreateConstantBuffer(Graphics,
                                                                       new GorgonConstantBufferInfo("Deferred Lighting Light Data Buffer")
                                                                       {
                                                                           SizeInBytes = Unsafe.SizeOf<LightData>() * MaxLightCount,
                                                                           Usage = ResourceUsage.Dynamic
                                                                       });
            _vertexLitShader = CompileShader<GorgonVertexShader>(Resources.Lighting, "GorgonVertexLitShader");
            GorgonShaderFactory.Includes[Gorgon2DLightingShaderIncludeName] = new GorgonShaderInclude(Gorgon2DLightingShaderIncludeName, Resources.Lighting);
        }

        /// <summary>
        /// Function to begin rendering using separate texture views for the normal/specular map.
        /// </summary>
        /// <param name="normal">[Optional] The normal map texture to render.</param>
        /// <param name="specular">[Optional] The specular map texture to render.</param>
        /// <param name="blendState">[Optional] A user defined blend state to apply when rendering.</param>
        /// <param name="depthStencilState">[Optional] A user defined depth/stencil state to apply when rendering.</param>
        /// <param name="rasterState">[Optional] A user defined rasterizer state to apply when rendering.</param>
        /// <param name="camera">[Optional] The camera to use when rendering.</param>
        public void Begin(GorgonTexture2DView normal = null, GorgonTexture2DView specular = null, GorgonBlendState blendState = null, GorgonDepthStencilState depthStencilState = null, GorgonRasterState rasterState = null, IGorgon2DCamera camera = null)
        {
            _currentRtv = Graphics.RenderTargets[0];            

            if ((_normalTexture != normal) || (_specularTexture != specular))
            {
                _normalTexture = normal;
                _specularTexture = specular;
                _pixelLitShaderState = null;
            }

            if (_usingArray)
            {
                Macros.Remove(_arrayMacro);
                _pixelLitShader = null;
                _usingArray = false;
            }

            OnBeginRender(blendState, depthStencilState, rasterState, camera);
        }

        /// <summary>
        /// Function to begin rendering using array indices of the texture being rendered for the normal/specular map.
        /// </summary>
        /// <param name="normalMapIndex">The array index of the texture being rendered that contains the normal map.</param>
        /// <param name="specularMapIndex">The array index of the texture being rendered that contains the specular map.</param>
        /// <param name="blendState">[Optional] A user defined blend state to apply when rendering.</param>
        /// <param name="depthStencilState">[Optional] A user defined depth/stencil state to apply when rendering.</param>
        /// <param name="rasterState">[Optional] A user defined rasterizer state to apply when rendering.</param>
        /// <param name="camera">[Optional] The camera to use when rendering.</param>
        /// <remarks>
        /// <para>
        /// This method takes the texture of whatever is currently being rendered and uses an array index to index into the texture and retrieve the normal and specular map values.
        /// </para>
        /// </remarks>
        public void Begin(int normalMapIndex, int specularMapIndex, GorgonBlendState blendState = null, GorgonDepthStencilState depthStencilState = null, GorgonRasterState rasterState = null, IGorgon2DCamera camera = null)
        {
            _currentRtv = Graphics.RenderTargets[0];

            if (!_usingArray)
            {
                _effectData.ArrayIndices = new DX.Vector4(normalMapIndex, specularMapIndex, 0, 0);
                Macros.Add(_arrayMacro);
                _pixelLitShader = null;
                _usingArray = true;
            }

            OnBeginRender(blendState, depthStencilState, rasterState, camera);
        }
        
        /// <summary>
        /// Function to end rendering.
        /// </summary>
        public void End()
        {
            EndPass(0, _currentRtv);
            EndRender(_currentRtv);
        }

        /// <summary>
        /// Function to render the effect using the array indices of the diffuse texture.
        /// </summary>
        /// <param name="gbuffer">A Gbuffer containing the diffuse, normal and specular maps.</param>
        /// <param name="output">The final output target for the effect.</param>
        /// <param name="camera">[Optional] The camera used to transform the lights to camera space.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="gbuffer"/>, or the <paramref name="output"/> parameter is <b>null</b>.</exception>
        public void Render(Gorgon2DGBuffer gbuffer, GorgonRenderTargetView output, IGorgon2DCamera camera = null)
        {
            gbuffer.ValidateObject(nameof(gbuffer));
            output.ValidateObject(nameof(output));

            _currentRtv = output;

            if (!_usingArray)
            {
                _effectData.ArrayIndices = new DX.Vector4(1, 2, 0, 0);
                Macros.Add(_arrayMacro);
                _pixelLitShader = null;
                _usingArray = true;
            }

            OnRender(gbuffer.GBufferTexture, output, camera);
        }


        /// <summary>
        /// Function to render the effect using the array indices of the diffuse texture.
        /// </summary>
        /// <param name="diffuse">The diffuse texture to render.</param>
        /// <param name="output">The final output target for the effect.</param>
        /// <param name="normalMapIndex">The array index of the diffuse texture that contains the normal map.</param>
        /// <param name="specularMapIndex">The array index of the diffuse texture that contains the specular map.</param>
        /// <param name="camera">[Optional] The camera used to transform the lights to camera space.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="diffuse"/>, or the <paramref name="output"/> parameter is <b>null</b>.</exception>
        public void Render(GorgonTexture2DView diffuse, GorgonRenderTargetView output, int normalMapIndex, int specularMapIndex, IGorgon2DCamera camera = null)
        {
            diffuse.ValidateObject(nameof(diffuse));
            output.ValidateObject(nameof(output));

            _currentRtv = output;

            if (!_usingArray)
            {
                _effectData.ArrayIndices = new DX.Vector4(normalMapIndex.Max(0).Min(diffuse.ArrayCount - 1),
                                                          specularMapIndex.Max(0).Min(diffuse.ArrayCount - 1), 0, 0);
                Macros.Add(_arrayMacro);
                _pixelLitShader = null;
                _usingArray = true;
            }

            OnRender(diffuse, output, camera);
        }

        /// <summary>
        /// Function to render the effect using multiple textures for the normal and specular maps.
        /// </summary>
        /// <param name="diffuse">The diffuse texture to render.</param>
        /// <param name="output">The final output target for the effect.</param>
        /// <param name="normal">[Optional] The normal map texture to render.</param>
        /// <param name="specular">[Optional] The specular map texture to render.</param>
        /// <param name="camera">[Optional] The camera used to transform the lights to camera space.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="diffuse"/>, or the <paramref name="output"/> parameter is <b>null</b>.</exception>
        public void Render(GorgonTexture2DView diffuse, GorgonRenderTargetView output, GorgonTexture2DView normal = null, GorgonTexture2DView specular = null, IGorgon2DCamera camera = null)
        {
            diffuse.ValidateObject(nameof(diffuse));
            output.ValidateObject(nameof(output));

            _currentRtv = output;

            if ((_normalTexture != normal) || (_specularTexture != specular))
            {
                _normalTexture = normal;
                _specularTexture = specular;
                _pixelLitShaderState = null;
            }

            if (_usingArray)
            {
                Macros.Remove(_arrayMacro);                
                _pixelLitShaderState = null;
                _usingArray = false;
            }

            OnRender(diffuse, output, camera);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DLightingEffect"/> class.
        /// </summary>
        /// <param name="renderer">The renderer used to draw with the effect.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> parameter is <b>null</b>.</exception>
        public Gorgon2DLightingEffect(Gorgon2D renderer)
            : base(renderer, Resources.GOR2D_EFFECT_LIGHTING, Resources.GOR2D_EFFECT_LIGHTING_DESC, 1)
        {
        }
        #endregion
    }
}
