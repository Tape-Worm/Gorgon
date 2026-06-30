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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: April 13, 2026 6:56:41 PM
//

using System.Runtime.CompilerServices;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Math;
using TerraFX.Interop.DirectX;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides a sampler for shaders to sample texture data.
/// </summary>
public sealed unsafe class GorgonSampler
    : IDisposable, IGorgonNamedObject
{
    private readonly GpuDescriptorHeap _descriptors;
    private GpuDescriptorAllocation _allocation = GpuDescriptorAllocation.Null;

    /// <summary>
    /// The minimum LOD bias value for the <see cref="MipLodBias"/> value.
    /// </summary>
    public const float MipLodBiasMinimum = D3D12.D3D12_MIP_LOD_BIAS_MIN;
    /// <summary>
    /// The maximum LOD bias value for the <see cref="MipLodBias"/> value.
    /// </summary>
    public const float MipLodBiasMaximum = D3D12.D3D12_MIP_LOD_BIAS_MAX;

    /// <summary>
    /// Property to return the descriptor allocation for the sampler.
    /// </summary>
    internal ref readonly GpuDescriptorAllocation Allocation => ref _allocation;

    /// <summary>
    /// Property to return the graphics interface associated with this sampler.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    }    

    /// <inheritdoc/>
    public string Name
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the comparison function for the sampler.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="ComparisonFunction.None"/>.
    /// </para>
    /// </remarks>
    public ComparisonFunction Comparison
    {
        get;
        internal set;
    } = ComparisonFunction.None;

    /// <summary>
    /// Property to return the border color for the sampler.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When the <see cref="BorderUsesIntegerColor"/> value is <b>true</b>, the components, red, green, blue and alpha, are interpreted as 32-bit integer values ranging from 0 to 255.
    /// </para>
    /// <para>
    /// This value is used when the <see cref="UAddressing"/>, <see cref="VAddressing"/> and/or <see cref="WAddressing"/> properties are set to <see cref="TextureAddressing.Border"/>.
    /// </para>
    /// <para>
    /// The default value is <see cref="GorgonColors.White"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="BorderUsesIntegerColor"/>
    public GorgonColor BorderColor
    {
        get;
        internal set;
    } = GorgonColors.White;

    /// <summary>
    /// Property to return the minimum level of detail for the sampler.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the lower end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any level higher than that is less detailed.
    /// </para>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    public float MinimumLod
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the offset to apply to the calculated mip level.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This offset from the calculated mipmap level. If the texture should be sampled at mipmap level 3 and <see cref="MipLodBias"/> is 2, the texture will be sampled at mipmap level 5.
    /// </para>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    public float MipLodBias
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the maximum level of detail for the sampler.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the upper end of the mipmap range to clamp access to, where 0 is the largest and most detailed mipmap level and any level higher than that is less detailed. 
    /// </para>
    /// <para>
    /// This value must be greater than or equal to <see cref="MinimumLod"/>. To have no upper limit on LOD, set this member to <see cref="float.MaxValue"/>.
    /// </para>
    /// <para>
    /// The default value is <see cref="float.MaxValue"/>.
    /// </para>
    /// </remarks>
    public float MaximumLod
    {
        get;
        internal set;
    } = float.MaxValue;

    /// <summary>
    /// Property to return the maximum clamping value for anisotropic filters.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value only applies when the <see cref="Filter"/> property is set to one of the anisotropic filters in <see cref="TextureFilter"/>.
    /// </para>
    /// <para>
    /// The default value is 16.
    /// </para>
    /// </remarks>
    public int MaxAnisotropy
    {
        get;
        internal set;
    } = 16;

    /// <summary>
    /// Property to return the flag that indicates that the <see cref="BorderColor"/> will be interpreted as a 32-bit integer value.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When this value is <b>true</b>, the <see cref="BorderColor"/> red, green, blue and alpha components will be interpreted as 32-bit integer values that range from 0 - 255.
    /// </para>
    /// <para>
    /// The default value is <b>false</b>.
    /// </para>
    /// </remarks>
    /// <seealso cref="BorderColor"/>
    public bool BorderUsesIntegerColor
    {
        get;
        internal set;
    }

    /// <summary>
    /// Property to return the addressing mode on the texture U axis.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="TextureAddressing.Clamp"/>.
    /// </para>
    /// </remarks>
    public TextureAddressing UAddressing
    {
        get;
        internal set;
    } = TextureAddressing.Clamp;

    /// <summary>
    /// Property to return the addressing mode on the texture V axis.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="TextureAddressing.Clamp"/>.
    /// </para>
    /// </remarks>
    public TextureAddressing VAddressing
    {
        get;
        internal set;
    } = TextureAddressing.Clamp;

    /// <summary>
    /// Property to return the addressing mode on the texture W axis.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="TextureAddressing.Clamp"/>.
    /// </para>
    /// </remarks>
    public TextureAddressing WAddressing
    {
        get;
        internal set;
    } = TextureAddressing.Clamp;

    /// <summary>
    /// Property to return the type of filtering to perform when sampling.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The default value is <see cref="TextureFilter.PointMinMagMip"/>.
    /// </para>
    /// </remarks>
    public TextureFilter Filter
    {
        get;
        internal set;
    } = TextureFilter.PointMinMagMip;

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            this.UnregisterDisposable(Graphics);

            Graphics.Log.Print($"Destroying sampler '{Name}'...", LoggingLevel.Simple);

            ResetDescriptor();            
        }
    }

    /// <summary>
    /// Function to create the native descriptor for the sampler.
    /// </summary>
    private void AllocateDescriptors()
    {
        ResetDescriptor();

        Graphics.Log.Print($"Creating sampler '{Name}'...", LoggingLevel.Simple);

        _descriptors.Allocate(1, out _allocation);

        float minLod = MinimumLod.Max(0);

        D3D12_SAMPLER_DESC2 desc = new()
        {
            AddressU = (D3D12_TEXTURE_ADDRESS_MODE)UAddressing,
            AddressV = (D3D12_TEXTURE_ADDRESS_MODE)VAddressing,
            AddressW = (D3D12_TEXTURE_ADDRESS_MODE)WAddressing,
            Filter = (D3D12_FILTER)Filter,
            ComparisonFunc = (D3D12_COMPARISON_FUNC)Comparison,
            MinLOD = minLod,
            MipLODBias = MipLodBias.Max(MipLodBiasMinimum).Min(MipLodBiasMaximum),
            MaxLOD = MaximumLod.Max(minLod).Max(0),
            MaxAnisotropy = (uint)(MaxAnisotropy.Max(1).Min(16)),
            Flags = BorderUsesIntegerColor ? D3D12_SAMPLER_FLAGS.D3D12_SAMPLER_FLAG_UINT_BORDER_COLOR : D3D12_SAMPLER_FLAGS.D3D12_SAMPLER_FLAG_NONE
        };

        if (!BorderUsesIntegerColor)
        {
            desc.FloatBorderColor[0] = BorderColor.Red.Max(0).Min(1.0f);
            desc.FloatBorderColor[1] = BorderColor.Green.Max(0).Min(1.0f);
            desc.FloatBorderColor[2] = BorderColor.Blue.Max(0).Min(1.0f);
            desc.FloatBorderColor[3] = BorderColor.Alpha.Max(0).Min(1.0f);
        }
        else
        {
            desc.UintBorderColor[0] = (uint)(BorderColor.Red.Max(0).Min(1.0f) * 255.0f);
            desc.UintBorderColor[1] = (uint)(BorderColor.Green.Max(0).Min(1.0f) * 255.0f);
            desc.UintBorderColor[2] = (uint)(BorderColor.Blue.Max(0).Min(1.0f) * 255.0f);
            desc.UintBorderColor[3] = (uint)(BorderColor.Alpha.Max(0).Min(1.0f) * 255.0f);
        }

        D3D12_CPU_DESCRIPTOR_HANDLE handle = _descriptors.D3DCpuHandle;

        handle.Offset(_allocation.Offset, _descriptors.DescriptorSize);
        Graphics.D3DDevice.Get()->CreateSampler2(&desc, handle);
    }

    /// <summary>
    /// Function to reset the descriptor for the sampler.
    /// </summary>
    internal void ResetDescriptor()
    {
        if (!_allocation.Equals(GpuDescriptorAllocation.Null))
        {
            Graphics.Log.Print($"Freeing D3D 12 sampler descriptor for '{Name}'...", LoggingLevel.Verbose);
            _descriptors.Free(ref _allocation);
        }
    }

    /// <summary>
    /// Function to return the default sampler state.
    /// </summary>
    /// <param name="graphics">The graphics interface associated with the sampler.</param>
    /// <inheritdoc cref="SamplerStates.Default" path="/remarks"/>
    public static GorgonSampler Default(GorgonGraphics graphics) => graphics.SamplerStates.Default;

    /// <summary>
    /// Function to return a wrapping sampler state.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="SamplerStates.Wrapping" path="/param[@name='graphics']"/></param>
    /// <inheritdoc cref="SamplerStates.Wrapping" path="/remarks"/>
    public static GorgonSampler Wrapping(GorgonGraphics graphics) => graphics.SamplerStates.Wrapping;

    /// <summary>
    /// Function to return the linear sampler state.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="SamplerStates.Linear" path="/param[@name='graphics']"/></param>
    /// <inheritdoc cref="SamplerStates.Linear" path="/remarks"/>
    public static GorgonSampler Linear(GorgonGraphics graphics) => graphics.SamplerStates.Linear;

    /// <summary>
    /// Function to return a linear wrapping sampler state.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="SamplerStates.LinearWrapping" path="/param[@name='graphics']"/></param>
    /// <inheritdoc cref="SamplerStates.LinearWrapping" path="/remarks"/>
    public static GorgonSampler LinearWrapping(GorgonGraphics graphics) => graphics.SamplerStates.LinearWrapping;

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc cref="GorgonConstantBufferView.GetViewHandle()"/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int GetViewHandle()
    {
        if (_allocation.Equals(in GpuDescriptorAllocation.Null))
        {
            AllocateDescriptors();
        }

        return _allocation.Offset;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonSampler"/> class.
    /// </summary>
    /// <param name="graphics">The graphics object associated with this sampler.</param>
    /// <param name="name">The name of the sampler.</param>
    internal GorgonSampler(GorgonGraphics graphics, string name)
    {        
        Name = GorgonGraphicsFactory.GenerateName(name, nameof(GorgonSampler));
        Graphics = graphics;

        _descriptors = Graphics.Descriptors.GpuSamplerDescriptors;

        this.RegisterDisposable(Graphics);
    }
}