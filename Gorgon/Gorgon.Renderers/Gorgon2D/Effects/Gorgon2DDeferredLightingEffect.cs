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
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Renderers.Properties;
using DX = SharpDX;

namespace Gorgon.Renderers
{
    /// <summary>
    /// An effect used to render a scene with deferred per-pixel lighting.
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
    public class Gorgon2DDeferredLightingEffect
        : Gorgon2DEffect
    {
        #region Value Types.
        // Constant buffer data for a point light.
        [StructLayout(LayoutKind.Sequential, Size = 80)]
        private struct PointLightData
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
            /// <summary>
            /// The type of light.
            /// </summary>
            public int LightType;
        }

        // Constant buffer data for a point light.
        [StructLayout(LayoutKind.Sequential, Size = 32, Pack = 16)]
        private struct GlobalEffectData
        {
            /// <summary>
            /// Position for the light in screen space.
            /// </summary>
            public DX.Vector3 CameraPosition;
            /// <summary>
            /// The flag to indicate whether to flip the Y channel on the normal map.
            /// </summary>
            public int FlipYNormal;
        }
        #endregion

        #region Constants.
        /// <summary>
        /// The name of the shader include for Gorgon's <see cref="Gorgon2DDeferredLightingEffect"/>.
        /// </summary>
        public const string Gorgon2DDeferredLightIncludeName = "DeferredLightShaders";
        /// <summary>
        /// The maximum number of lights that can be used at one time.
        /// </summary>
        public const int MaxLightCount = 1024;
        #endregion

        #region Variables.        
        // Our custom vertex shader so we can calculate the tangent and bi-tangent.
        private GorgonVertexShader _vertexDeferShader;
        private Gorgon2DShaderState<GorgonVertexShader> _vertexDeferShaderState;
        // A pixel shader used to render the scene to multiple render targets.
        private GorgonPixelShader _pixelDeferShader;
        private Gorgon2DShaderState<GorgonPixelShader> _pixelDeferShaderState;
        // Our custom vertex shader for per pixel lighting.
        private GorgonVertexShader _vertexLitShader;
        private Gorgon2DShaderState<GorgonVertexShader> _vertexLitShaderState;
        // A pixel shader used to render lights.
        private GorgonPixelShader _pixelLitShader;
        private Gorgon2DShaderState<GorgonPixelShader> _pixelLitShaderState;
        // The buffer that will hold the lighting data for an individual light.
        private GorgonConstantBufferView _lightData;
        // The buffer that will hold the global effect data.
        private GorgonConstantBufferView _globalData;
        // The render target for the gbuffer channels.
        private GorgonRenderTarget2DView[] _gbufferTargets = new GorgonRenderTarget2DView[3];
        // The texture used to draw the gbuffer data.
        private GorgonTexture2DView _gbufferTexture;
        // The batch render state for drawing to the deferred targets.
        private Gorgon2DBatchState _deferredState;
        // The batch render state for drawing with lights.
        private Gorgon2DBatchState _lightingState;
        // The color used to clear the normal map render target.
        private readonly GorgonColor _normalClearColor = new GorgonColor(127.0f / 255.0f, 127.0f / 255.0f, 254.0f / 255.0f, 1.0f);
        // Render target information.
        private readonly GorgonTexture2DInfo _rtvInfo;
        // The filter for diffuse mapping.
        private GorgonSamplerState _diffuseFilter = GorgonSamplerState.Default;
        // The filter for normal mapping.
        private GorgonSamplerState _normalFilter = GorgonSamplerState.PointFiltering;
        // The filter for specular mapping.
        private GorgonSamplerState _specularFilter = GorgonSamplerState.Default;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to set or return the distance on the Z axis for specular hilights.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This sets the number of units on the Z axis that our default camera should be when rendering specular hilights. 
        /// </para>
        /// <para>
        /// This value only applies when a light has <see cref="Gorgon2DLight.SpecularEnabled"/> set to <b>true</b>, and, when <see cref="Gorgon2D.CurrentCamera"/> is set to <b>null</b>.
        /// </para>
        /// <para>
        /// If this value is too small, strange artifacts may appear as the Z value gets closer to the drawing plane (Z = 0.0f).
        /// </para>
        /// </remarks>
        public float DefaultSpecularZDistance
        {
            get;
            set;
        } = -127.0f;

        /// <summary>
        /// Property to return the list of point lights for rendering.
        /// </summary>
        public IList<Gorgon2DLight> Lights
        {
            get;
        } = new List<Gorgon2DLight>(8);

        /// <summary>
        /// Property to set or return whether the Y (green channel) in the normal map should be flipped. 
        /// </summary>
        /// <remarks>
        /// This property is no longer supported. Use an application such as Adobe Photoshop or Gimp to flip the normal axes. 
        /// </remarks>
        [Obsolete("This property is not working correctly and will be removed. Users should flip their normal map axes using an application such as Photoshop or Gimp.")]
        public bool FlipYNormal
        {
            get;
            set;
        }

        /// <summary>
        /// Property to set or return the texture filtering to apply to the diffuse texture.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="GorgonSamplerState.Default"/>.
        /// </remarks>
        public GorgonSamplerState DiffuseFiltering
        {
            get;
            set;
        } = GorgonSamplerState.Default;

        /// <summary>
        /// Property to set or return the texture filtering to apply to the normal maps.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="GorgonSamplerState.PointFiltering"/>.
        /// </remarks>
        public GorgonSamplerState NormalFiltering
        {
            get;
            set;
        } = GorgonSamplerState.PointFiltering;

        /// <summary>
        /// Property to set or return the texture filtering to apply to the specular maps.
        /// </summary>
        /// <remarks>
        /// The default value is <see cref="GorgonSamplerState.Default"/>.
        /// </remarks>
        public GorgonSamplerState SpecularFiltering
        {
            get;
            set;
        } = GorgonSamplerState.Default;
        #endregion

        #region Methods.
        /// <summary>
        /// Function to build up the render targets.
        /// </summary>
        /// <param name="width">The width of the render targets.</param>
        /// <param name="height">The height of the render targets.</param>
        /// <param name="format">The format of the buffer.</param>
        private void BuildRenderTargets(int width, int height, BufferFormat format)
        {
            // For the lighting effect, we use a deferred rendering technique where we have 3 render targets for diffuse, specularity, and normal mapping.
            // These targets are sub resources of the same texture resource (array indices).

            // Diffuse.
            _rtvInfo.Width = width;
            _rtvInfo.Height = height;
            _rtvInfo.Format = format;
            _rtvInfo.ArrayCount = 2;

            _gbufferTargets[0] = Graphics.TemporaryTargets.Rent(_rtvInfo, "Gorgon 2D GBuffer - Diffuse/Specular", false);
            _gbufferTargets[0].Clear(GorgonColor.Black);
            // Specular.
            _gbufferTargets[1] = _gbufferTargets[0].Texture.GetRenderTargetView(arrayIndex: 1, arrayCount: 1);

            _rtvInfo.Format = BufferFormat.R8G8B8A8_UNorm;
            _rtvInfo.ArrayCount = 1;

            // Normals.
            // We'll clear it before the pass, the default color is insufficient.
            _gbufferTargets[2] = Graphics.TemporaryTargets.Rent(_rtvInfo, "Gorgon 2D Buffer - Normals", false);
            GorgonTexture2DView normalSrv = _gbufferTargets[2].GetShaderResourceView();

            if ((_pixelDeferShaderState.ShaderResources[1] != normalSrv)
                || (_diffuseFilter != DiffuseFiltering)
                || (_normalFilter != NormalFiltering)
                || (_specularFilter != SpecularFiltering))
            {
                _diffuseFilter = DiffuseFiltering ?? GorgonSamplerState.Default;
                _normalFilter = NormalFiltering ?? GorgonSamplerState.PointFiltering;
                _specularFilter = SpecularFiltering ?? GorgonSamplerState.Default;

                _pixelDeferShaderState = PixelShaderBuilder.ResetTo(_pixelDeferShaderState)
                                                    .SamplerState(_diffuseFilter, 0)
                                                    .SamplerState(_normalFilter, 1)
                                                    .SamplerState(_specularFilter, 2)
                                                    .Build();

                _pixelLitShaderState = PixelShaderBuilder
                                                .ResetTo(_pixelLitShaderState)
                                                .ShaderResource(normalSrv, 1)
                                                .SamplerState(_diffuseFilter, 0)
                                                .SamplerState(_normalFilter, 1)
                                                .SamplerState(_specularFilter, 2)
                                                .Build();

                _lightingState = BatchStateBuilder
                                                .ResetTo(_lightingState)
                                                .PixelShaderState(_pixelLitShaderState)
                                                .Build();

            }


            _gbufferTexture = _gbufferTargets[0].Texture.GetShaderResourceView();
        }

        /// <summary>
        /// Function to update the light data in the lighting shader.
        /// </summary>
        /// <param name="light">The light to retrieve data from.</param>
        private void UpdateLight(Gorgon2DLight light)
        {
            var lightData = new PointLightData
            {
                Position = new DX.Vector4(new DX.Vector3(light.Position.X, light.Position.Y, -light.Position.Z), 1.0f),
                Color = light.Color,
                LightDirection = new DX.Vector4(new DX.Vector3(light.LightDirection.X, light.LightDirection.Y, -light.LightDirection.Z), 0),
                LightAttribs = new DX.Vector4(light.SpecularPower, light.Attenuation, light.Intensity, light.SpecularEnabled ? 1 : 0),
                LightType = (int)light.LightType
            };

            _lightData.Buffer.SetData(ref lightData);
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

            GorgonVertexShader vertexShader = Interlocked.Exchange(ref _vertexDeferShader, null);
            GorgonPixelShader deferredShader = Interlocked.Exchange(ref _pixelDeferShader, null);
            GorgonPixelShader lightShader = Interlocked.Exchange(ref _pixelLitShader, null);
            GorgonConstantBufferView lightData = Interlocked.Exchange(ref _lightData, null);
            GorgonConstantBufferView globalData = Interlocked.Exchange(ref _globalData, null);
            GorgonRenderTargetView[] targets = Interlocked.Exchange(ref _gbufferTargets, null);
            GorgonTexture2DView textureView = Interlocked.Exchange(ref _gbufferTexture, null);

            textureView?.Dispose();

            for (int i = 0; i < targets.Length; ++i)
            {
                targets[i]?.Dispose();
            }

            globalData?.Dispose();
            lightData?.Dispose();
            lightShader?.Dispose();
            deferredShader?.Dispose();
            vertexShader?.Dispose();
        }

        /// <summary>
        /// Function called to build a new (or return an existing) 2D batch state.
        /// </summary>
        /// <param name="passIndex">The index of the current rendering pass.</param>
        /// <param name="statesChanged"><b>true</b> if the blend, raster, or depth/stencil state was changed. <b>false</b> if not.</param>
        /// <returns>The 2D batch state.</returns>
        protected override Gorgon2DBatchState OnGetBatchState(int passIndex, bool statesChanged)
        {
            switch (passIndex)
            {
                case 0:
                    if ((statesChanged) || (_deferredState == null))
                    {
                        _deferredState = BatchStateBuilder
                                            .PixelShaderState(_pixelDeferShaderState)
                                            .VertexShaderState(_vertexDeferShaderState)
                                            .BlendState(BlendStateOverride ?? GorgonBlendState.NoBlending)
                                            .Build();
                    }

                    return _deferredState;
                default:
                    return _lightingState;
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
            Debug.Assert(passIndex >= 0, "Illegal pass index.");

            int lightIndex = passIndex - 1;
            if ((Lights.Count == 0) || (lightIndex >= Lights.Count))
            {
                return PassContinuationState.Stop;
            }

            switch (passIndex)
            {
                case 0:
                    // Array 0 is cleared on rental.
                    _gbufferTargets[1].Clear(GorgonColor.Black);
                    _gbufferTargets[2].Clear(_normalClearColor);
                    Graphics.SetRenderTargets(_gbufferTargets, Graphics.DepthStencilView);
                    return PassContinuationState.Continue;
                case 1:
                    Graphics.SetRenderTarget(output, Graphics.DepthStencilView);
                    return PassContinuationState.Continue;
            }

            return PassContinuationState.Stop;
        }

        /// <summary>
        /// Function called prior to rendering.
        /// </summary>
        /// <param name="output">The final render target that will receive the rendering from the effect.</param>
        /// <param name="camera">The currently active camera.</param>
        /// <param name="sizeChanged"><b>true</b> if the output size changed since the last render, or <b>false</b> if it's the same.</param>
        /// <remarks>
        /// <para>
        /// Applications can use this to set up common states and other configuration settings prior to executing the render passes. This is an ideal method to initialize and resize your internal render
        /// targets (if applicable).
        /// </para>
        /// </remarks>
        protected override void OnBeforeRender(GorgonRenderTargetView output, IGorgon2DCamera camera, bool sizeChanged)
        {
            BuildRenderTargets(output.Width, output.Height, output.Format);

            var globals = new GlobalEffectData
            {
                FlipYNormal = 0,
                // If no custom camera is in use, we need to pass in our default viewing information which is normally the output width, and height (by half), and an arbitrary Z value so 
                // the camera position isn't intersecting with the drawing plane (+ height information). Otherwise, our specular hilight will look really messed up.
                CameraPosition = Renderer.CurrentCamera == null ? new DX.Vector3(output.Width / 2.0f, output.Height / 2.0f, DefaultSpecularZDistance) : camera.Position
            };
            _globalData.Buffer.SetData(ref globals);
        }

        /// <summary>Function called after rendering is complete.</summary>
        /// <param name="output">The final render target that will receive the rendering from the effect.</param>
        /// <remarks>Applications can use this to clean up and/or restore any states when rendering is finished. This is an ideal method to copy any rendering imagery to the final output render target.</remarks>
        protected override void OnAfterRender(GorgonRenderTargetView output)
        {
            Graphics.TemporaryTargets.Return(_gbufferTargets[0]);
            Graphics.TemporaryTargets.Return(_gbufferTargets[2]);
        }

        /// <summary>
        /// Function called to render a single effect pass.
        /// </summary>
        /// <param name="passIndex">The index of the pass being rendered.</param>
        /// <param name="renderMethod">The method used to render a scene for the effect.</param>
        /// <param name="output">The render target that will receive the final render data.</param>
        /// <remarks>
        /// <para>
        /// Applications must implement this in order to see any results from the effect.
        /// </para>
        /// </remarks>
        protected override void OnRenderPass(int passIndex, Action<int, int, DX.Size2> renderMethod, GorgonRenderTargetView output)
        {
            if (passIndex == 0)
            {
                renderMethod(passIndex, PassCount, new DX.Size2(output.Width, output.Height));
                return;
            }

            // We'll do our own drawing so we can ensure we have screen space.
            IGorgon2DCamera currentCamera = Renderer.CurrentCamera;
            Renderer.End();

            var destRect = new DX.RectangleF(0, 0, output.Width, output.Height);
            float minZ = 0;

            // Calculate our full screen blit area in camera projection if we're using another camera.
            if (currentCamera != null)
            {
                minZ = currentCamera.MinimumDepth;
                destRect = currentCamera.ViewableRegion;
            }

            for (int i = 0; i < Lights.Count; ++i)
            {
                Gorgon2DLight light = Lights[i];

                if (light == null)
                {
                    continue;
                }

                UpdateLight(light);

                Renderer.Begin(_lightingState, currentCamera);
                Renderer.DrawFilledRectangle(destRect,
                                            GorgonColor.White,
                                            _gbufferTexture,
                                            new DX.RectangleF(0, 0, 1, 1),
                                            textureSampler: GorgonSamplerState.Default,
                                            depth: minZ);
                Renderer.End();
            }
        }

        /// <summary>
        /// Function called to initialize the effect.
        /// </summary>
        /// <remarks>Applications must implement this method to ensure that any required resources are created, and configured for the effect.</remarks>
        protected override void OnInitialize()
        {
            var globalData = new GlobalEffectData
            {
                CameraPosition = DX.Vector3.Zero,
                FlipYNormal = 0
            };

            _globalData = GorgonConstantBufferView.CreateConstantBuffer(Graphics, ref globalData, "Global deferred light effect data.", ResourceUsage.Default);

            _lightData = GorgonConstantBufferView.CreateConstantBuffer(Graphics,
                                                                       new GorgonConstantBufferInfo("Deferred Lighting Light Data Buffer")
                                                                       {
                                                                           SizeInBytes = Unsafe.SizeOf<PointLightData>(),
                                                                           Usage = ResourceUsage.Dynamic
                                                                       });

            Macros.Add(new GorgonShaderMacro("DEFERRED_LIGHTING"));
            _vertexDeferShader = CompileShader<GorgonVertexShader>(Resources.Lighting, "GorgonVertexLightingShader");
            _vertexDeferShaderState = VertexShaderBuilder.Shader(_vertexDeferShader)
                                               .Build();

            _pixelDeferShader = CompileShader<GorgonPixelShader>(Resources.Lighting, "GorgonPixelShaderDeferred");
            _pixelDeferShaderState = PixelShaderBuilder.Shader(_pixelDeferShader)
                                                .SamplerState(_diffuseFilter, 0)
                                                .SamplerState(_normalFilter, 1)
                                                .SamplerState(_specularFilter, 2)
                                                .Build();

            Macros.Clear();
            Macros.Add(new GorgonShaderMacro("LIGHTS"));
            _vertexLitShader = CompileShader<GorgonVertexShader>(Resources.Lighting, "GorgonVertexLitShader");
            _vertexLitShaderState = VertexShaderBuilder.Shader(_vertexLitShader)
                                               .Build();

            _pixelLitShader = CompileShader<GorgonPixelShader>(Resources.Lighting, "GorgonPixelShaderLighting");
            _pixelLitShaderState = PixelShaderBuilder.Shader(_pixelLitShader)
                                             .ConstantBuffer(_lightData, 1)
                                             .ConstantBuffer(_globalData, 2)
                                                .SamplerState(_diffuseFilter, 0)
                                                .SamplerState(_normalFilter, 1)
                                                .SamplerState(_specularFilter, 2)
                                             .Build();

            // Rebuild our states for the new pixel shaders.
            _lightingState = BatchStateBuilder.PixelShaderState(_pixelLitShaderState)
                                              .VertexShaderState(_vertexLitShaderState)
                                                .BlendState(GorgonBlendState.Additive)
                                                .Build();
        }


        /// <summary>
        /// Function to create a compatible image to use when drawing with this effect.
        /// </summary>
        /// <param name="diffuse">The diffuse map.</param>
        /// <param name="normalMap">The normal map.</param>
        /// <param name="specularMap">[Optional] The specular map.</param>
        /// <returns>A new image with the maps combined as an image array to use with the effect.</returns>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="diffuse"/>, or the <paramref name="normalMap"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown if any of the images passed in are not a <see cref="ImageType.Image2D"/> type.</exception>
        /// <remarks>
        /// <para>
        /// Since the lighting effect requires that textures be drawn using an array of 3 images, developers can use this helper method to create a new image based on separate images that will be suitable 
        /// for rendering with this effect.
        /// </para>
        /// <para>
        /// The resulting image from this method is guaranteed to contain the correct image data, in the correct order, for use with this effect. The image can then be used in any of the 
        /// texture types for rendering.
        /// </para>
        /// <para>
        /// The images passed to this method must be a 2D image type, otherwise an exception will thrown.
        /// </para>
        /// <para>
        /// All images passed to this method should be the same width, height and pixel format. Otherwise undesired artifacts may appear on the generated maps.
        /// </para>
        /// </remarks>
        /// <seealso cref="IGorgonImage"/>
        /// <seealso cref="GorgonTexture2D"/>
        /// <seealso cref="GorgonTexture2DView"/>
        public IGorgonImage CreateLightingImage(IGorgonImage diffuse, IGorgonImage normalMap, IGorgonImage specularMap = null)
        {
            if (diffuse == null)
            {
                throw new ArgumentNullException(nameof(diffuse));
            }

            if (normalMap == null)
            {
                throw new ArgumentNullException(nameof(normalMap));
            }

            if (diffuse.ImageType != ImageType.Image2D)
            {
                throw new ArgumentException(string.Format(Resources.GOR2D_ERR_2D_IMAGE_ONLY, nameof(diffuse)));
            }

            if (normalMap.ImageType != ImageType.Image2D)
            {
                throw new ArgumentException(string.Format(Resources.GOR2D_ERR_2D_IMAGE_ONLY, nameof(normalMap)));
            }

            if ((specularMap != null) && (specularMap.ImageType != ImageType.Image2D))
            {
                throw new ArgumentException(string.Format(Resources.GOR2D_ERR_2D_IMAGE_ONLY, nameof(specularMap)));
            }

            IGorgonImage result = null;
            IGorgonImage workSpecular = specularMap?.Clone();
            IGorgonImage workDiffuse = diffuse.Clone();
            IGorgonImage workNormal = normalMap.Clone();

            if (workSpecular == null)
            {
                workSpecular = new GorgonImage(new GorgonImageInfo(ImageType.Image2D, diffuse.Format)
                {
                    Width = diffuse.Width,
                    Height = diffuse.Height
                });
                workSpecular.Buffers[0].Fill(0x00);
            }

            try
            {
                // Ensure formats are the same across all array entries.
                if (workNormal.Format != workDiffuse.Format)
                {
                    workNormal.ConvertToFormat(workDiffuse.Format);
                }

                if (workSpecular.Format != workDiffuse.Format)
                {
                    workSpecular.ConvertToFormat(workDiffuse.Format);
                }

                // Ensure width and height matches.
                if ((workNormal.Width != workDiffuse.Width) || (workDiffuse.Height != workNormal.Height))
                {
                    workNormal.Resize(workDiffuse.Width, workDiffuse.Height, 1);
                }

                if ((workSpecular.Width != workDiffuse.Width) || (workSpecular.Height != workNormal.Height))
                {
                    workSpecular.Resize(workDiffuse.Width, workDiffuse.Height, 1);
                }

                var info = new GorgonImageInfo(ImageType.Image2D, diffuse.Format)
                {
                    Width = diffuse.Width,
                    Height = diffuse.Height,
                    ArrayCount = 3
                };

                result = new GorgonImage(info);
                workDiffuse.Buffers[0].CopyTo(result.Buffers[0, 0]);
                workSpecular.Buffers[0].CopyTo(result.Buffers[0, 1]);
                workNormal.Buffers[0].CopyTo(result.Buffers[0, 2]);

                return result;
            }
            catch
            {
                result?.Dispose();
                throw;
            }
            finally
            {
                workDiffuse?.Dispose();
                workNormal?.Dispose();
                workSpecular.Dispose();
            }
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="Gorgon2DDeferredLightingEffect"/> class.
        /// </summary>
        /// <param name="renderer">The renderer used to draw with the effect.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="renderer"/> parameter is <b>null</b>.</exception>
        public Gorgon2DDeferredLightingEffect(Gorgon2D renderer)
            : base(renderer, "Deferred Lighting", "Renders a scene with deferred per-pixel lighting.", 2) => _rtvInfo = new GorgonTexture2DInfo()
            {
                Binding = TextureBinding.ShaderResource | TextureBinding.RenderTarget
            };
        #endregion
    }
}
