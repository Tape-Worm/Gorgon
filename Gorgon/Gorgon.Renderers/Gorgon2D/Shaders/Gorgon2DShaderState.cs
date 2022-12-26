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
// Created: June 13, 2018 4:13:14 PM
// 
#endregion

using Gorgon.Collections;
using Gorgon.Graphics.Core;
using Gorgon.Renderers.Cameras;

namespace Gorgon.Renderers;

/// <summary>
/// A shader state for use with a <see cref="Gorgon2DBatchState"/>.
/// </summary>
/// <typeparam name="T">The type of shader. Must inherit from <see cref="GorgonShader"/>.</typeparam>
/// <remarks>
/// <para>
/// This provides state information wrapped around shaders based on <see cref="GorgonShader"/>. These states are used for passing shader programs and related states to the 
/// <see cref="Gorgon2DBatchState"/> when setting up a batch render via <see cref="Gorgon2D.Begin(Gorgon2DBatchState, GorgonCameraCommon)"/>.
/// </para>
/// <para>
/// If a custom pixel or vertex shader is assigned to the state, then developers should note which resource slots, and constant buffer slots are used by the 2D renderer itself. Gorgon will allow 
/// overriding of these slots, but in those cases, some information may no longer available and things may not work as expected. The following slots are use by the 2D 
/// renderer:
/// <para>
/// <list type="table">
///		<listheader>
///			<term>Shader Type</term>
///			<term>Resource type</term>
///			<term>Slot #</term>
///			<term>Purpose</term>
///		</listheader>
///		<item>
///		    <term>Pixel</term>
///		    <term>Texture/Sampler</term>
///		    <term>0</term>
///		    <term>Primary sprite texture/sampler.</term>
///		</item>
///		<item>
///		    <term>Pixel</term>
///		    <term>Texture</term>
///		    <term>1</term>
///		    <term>Additional texture for effects.</term>
///		</item>
///		<item>
///		    <term>Pixel</term>
///		    <term>Constants</term>
///		    <term>0</term>
///		    <term>Data for alpha testing.</term>
///		</item>
///		<item>
///		    <term>Pixel and Vertex</term>
///		    <term>Constants</term>
///		    <term>12</term>
///		    <term>Timing data.</term>
///		</item>
///		<item>
///		    <term>Pixel and Vertex</term>
///		    <term>Constants</term>
///		    <term>13</term>
///		    <term>Miscellaneous data (e.g. target width and height)</term>
///		</item>
///		<item>
///		    <term>Vertex</term>
///		    <term>Constants</term>
///		    <term>0</term>
///		    <term>View/Projection matrix for the <see cref="Gorgon2D.CurrentCamera"/> (or the default camera if <b>null</b>).</term>
///		</item>
///		<item>
///		    <term>Vertex</term>
///		    <term>Constants</term>
///		    <term>1</term>
///		    <term>Data for a <see cref="GorgonPolySprite">polygon sprite</see>.</term>
///		</item>
/// </list>
/// </para>
/// Following this list, a developer can use any texture slot from 2 and up, and any constant buffer slots between 2 (or 1 for pixel shaders) and 11 for their own data.
/// </para>    
/// </remarks>
/// <seealso cref="GorgonPipelineState"/>
/// <seealso cref="GorgonDrawCall"/>
/// <seealso cref="Gorgon2DBatchState"/>
/// <seealso cref="GorgonShader"/>
public sealed class Gorgon2DShaderState<T>
    where T : GorgonShader
{
    #region Variables.
    /// <summary>
    /// The constant buffers, with read/write access.
    /// </summary>
    internal GorgonArray<GorgonConstantBufferView> RwConstantBuffers = new(GorgonConstantBuffers.MaximumConstantBufferCount);
    /// <summary>
    /// The texture samplers, with read/write access.
    /// </summary>
    internal GorgonArray<GorgonSamplerState> RwSamplers = new(GorgonSamplerStates.MaximumSamplerStateCount);
    /// <summary>
    /// The shader resource views, with read/write access.
    /// </summary>
    internal GorgonArray<GorgonShaderResourceView> RwSrvs = new(16);
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the shader.
    /// </summary>
    public T Shader
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the samplers for the shader.
    /// </summary>
    public IGorgonReadOnlyArray<GorgonSamplerState> Samplers => RwSamplers;

    /// <summary>
    /// Property to return the constant buffers for the shader.
    /// </summary>
    public IGorgonReadOnlyArray<GorgonConstantBufferView> ConstantBuffers => RwConstantBuffers;

    /// <summary>
    /// Property to return the list of shader resources for the shader.
    /// </summary>
    public IGorgonReadOnlyArray<GorgonShaderResourceView> ShaderResources => RwSrvs;
    #endregion

    #region Constructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonShaderResources"/> class.
    /// </summary>
    internal Gorgon2DShaderState()
    {
    }
    #endregion
}
