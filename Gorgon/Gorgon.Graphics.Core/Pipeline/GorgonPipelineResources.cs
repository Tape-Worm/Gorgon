#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: August 17, 2016 9:40:02 PM
// 
#endregion

using System;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// Flags to indicate what part of the pipeline was changed.
	/// </summary>
	[Flags]
	public enum PipelineResourceChangeFlags
		: ulong
	{
		/// <summary>
		/// No changes.
		/// </summary>
		None = 0,
		/// <summary>
		/// Shader resources for the pixel shader changed.
		/// </summary>
		PixelShaderResource = 0x20,
		/// <summary>
		/// Shader resources for the vertex shader changed.
		/// </summary>
		VertexShaderResource = 0x40,
		/// <summary>
		/// Samplers for the pixel shader changed.
		/// </summary>
		PixelShaderSampler = 0x80,
		/// <summary>
		/// <para>
		/// Samplers for the vertex shader changed.
		/// </para>
		/// <para>
		/// <note type="important">
		/// <para>
		/// This only applies to an <see cref="IGorgonVideoDevice"/> that has a <see cref="IGorgonVideoDevice.RequestedFeatureLevel"/> of <c>Level_11_0</c> or better.
		/// </para>
		/// </note>
		/// </para>
		/// </summary>
		VertexShaderSampler = 0x100,
		/// <summary>
		/// All states have changed.
		/// </summary>
		All = PixelShaderResource 
			| VertexShaderResource
			| PixelShaderSampler
			| VertexShaderSampler
	}

	/// <summary>
	/// Used to bind resource types (e.g. buffers, textures, etc...) to the GPU pipeline.
	/// </summary>
	public sealed class GorgonPipelineResources
	{
		#region Constants.
		/// <summary>
		/// The maximum number of allowed shader resources that can be bound at the same time.
		/// </summary>
		public const int MaximumShaderResourceViewCount = D3D11.CommonShaderStage.InputResourceSlotCount;
		#endregion

		#region Variables.
		// The available changes on this resource list.
		private PipelineResourceChangeFlags _changes;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the vertex shader resources to bind to the pipeline.
		/// </summary>
		public GorgonShaderResourceViews VertexShaderResourceViews
		{
			get;
		} = new GorgonShaderResourceViews();

		/// <summary>
		/// Property to set or return the list of pixel shader resource views.
		/// </summary>
		public GorgonShaderResourceViews PixelShaderResourceViews
		{
			get;
		} = new GorgonShaderResourceViews();

		/// <summary>
		/// Property to return the pixel shader samplers to bind to the pipeline.
		/// </summary>
		public GorgonSamplerStates PixelShaderSamplers
		{
			get;
		} = new GorgonSamplerStates();

		/// <summary>
		/// Property to return the vertex shader samplers to bind to the pipeline.
		/// </summary>
		/// <remarks>
		/// <para>
		/// <note type="important">
		/// <para>
		/// This only applies to an <see cref="IGorgonVideoDevice"/> that has a <see cref="IGorgonVideoDevice.RequestedFeatureLevel"/> of <c>Level_11_0</c> or better.
		/// </para>
		/// </note>
		/// </para>
		/// </remarks>
		public GorgonSamplerStates VertexShaderSamplers
		{
			get;
		} = new GorgonSamplerStates();
		#endregion

		#region Methods.
		/// <summary>
		/// Function to determine if the shader resources on this resource list are the same as another.
		/// </summary>
		/// <param name="theseResources">The resources to compare from this resource list.</param>
		/// <param name="otherResources">The resources to compare from the other resource list.</param>
		/// <returns><b>true</b> if there's a change, or <b>false</b> if not.</returns>
		private static bool ShaderResourcesChanged(GorgonShaderResourceViews theseResources, GorgonShaderResourceViews otherResources)
		{
			// If the resources are the same, then just check the dirty flag.
			if (theseResources == otherResources)
			{
				return theseResources.IsDirty;
			}

			ref NativeBinding<D3D11.ShaderResourceView> sourceViews = ref theseResources.GetNativeBindings();
			ref NativeBinding<D3D11.ShaderResourceView> destViews = ref otherResources.GetNativeBindings();

			return CompareNativeBinding(ref sourceViews, ref destViews);
		}

		/// <summary>
		/// Function to perform the comparison between two sets of native bindings.
		/// </summary>
		/// <typeparam name="T">The type of data in the binding.</typeparam>
		/// <param name="theseBindings">These bindings to use for comparison.</param>
		/// <param name="otherBindings">The other bindings to use for comparison.</param>
		/// <returns><b>true</b> if the bindings are equal, <b>false</b> if not.</returns>
		private static bool CompareNativeBinding<T>(ref NativeBinding<T> theseBindings, ref NativeBinding<T> otherBindings)
			where T : class
		{
			// If there's a change in slots and counts, then we have a change.
			if ((theseBindings.StartSlot != otherBindings.StartSlot)
			    || (theseBindings.Count != otherBindings.Count))
			{
				return true;
			}

			// Otherwise, we'll have to go through and compare each element.
			for (int i = theseBindings.StartSlot; i < theseBindings.StartSlot + theseBindings.Count; ++i)
			{
				if (theseBindings.Bindings[i] != otherBindings.Bindings[i])
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Function to determine if the shader samplers on this resource list are the same as another.
		/// </summary>
		/// <param name="theseResources">The resources to compare from this resource list.</param>
		/// <param name="otherResources">The resources to compare from the other resource list.</param>
		/// <returns><b>true</b> if there's a change, or <b>false</b> if not.</returns>
		private static bool SamplersChanged(GorgonSamplerStates theseResources, GorgonSamplerStates otherResources)
		{
			// If the resources are the same, then just check the dirty flag.
			if (theseResources == otherResources)
			{
				return theseResources.IsDirty;
			}

			ref NativeBinding<D3D11.SamplerState> sourceViews = ref theseResources.GetNativeBindings();
			ref NativeBinding<D3D11.SamplerState> destViews = ref otherResources.GetNativeBindings();

			return CompareNativeBinding(ref sourceViews, ref destViews);
		}

		/// <summary>
		/// Function to assign the sampler states to the resource list.
		/// </summary>
		/// <param name="device">The Direct 3D 11 device context to use.</param>
		/// <param name="shaderType">The type of shader to set the resources on.</param>
		internal void SetShaderSamplers(D3D11.DeviceContext1 device, ShaderType shaderType)
		{
			switch (shaderType)
			{
				case ShaderType.Pixel:
					ref NativeBinding<D3D11.SamplerState> psBinding = ref PixelShaderSamplers.GetNativeBindings();
					device.PixelShader.SetSamplers(psBinding.StartSlot, psBinding.Count, psBinding.Bindings);
					break;
				case ShaderType.Vertex:
					ref NativeBinding<D3D11.SamplerState> vsBinding = ref VertexShaderSamplers.GetNativeBindings();
					device.PixelShader.SetSamplers(vsBinding.StartSlot, vsBinding.Count, vsBinding.Bindings);
					break;
			}
		}

		/// <summary>
		/// Function to set the shader resource views for a shader.
		/// </summary>
		/// <param name="device">The Direct 3D 11 device context to use.</param>
		/// <param name="shaderType">The type of shader to set the resources on.</param>
		internal void SetShaderResourceViews(D3D11.DeviceContext1 device, ShaderType shaderType)
		{
			switch (shaderType)
			{
				case ShaderType.Pixel:
					ref NativeBinding<D3D11.ShaderResourceView> psBinding = ref PixelShaderResourceViews.GetNativeBindings();
					device.PixelShader.SetShaderResources(psBinding.StartSlot, psBinding.Count, psBinding.Bindings);
					break;
				case ShaderType.Vertex:
					ref NativeBinding<D3D11.ShaderResourceView> vsBinding = ref VertexShaderResourceViews.GetNativeBindings();
					device.VertexShader.SetShaderResources(vsBinding.StartSlot, vsBinding.Count, vsBinding.Bindings);
					break;
			}
		}

		/// <summary>
		/// Function to retrieve the differences between this pipeline resources and another.
		/// </summary>
		/// <param name="resources">The resources to compare with.</param>
		/// <returns>The differences between the resources.</returns>
		internal PipelineResourceChangeFlags GetDifference(GorgonPipelineResources resources)
		{
			// If we had no state prior to this, then return all states as changed.
			if (resources == null)
			{
				return PipelineResourceChangeFlags.All;
			}

			PipelineResourceChangeFlags changes = _changes;
			_changes = PipelineResourceChangeFlags.None;

			var result = PipelineResourceChangeFlags.None;

			if (ShaderResourcesChanged(VertexShaderResourceViews, resources.VertexShaderResourceViews))
			{
				result |= PipelineResourceChangeFlags.VertexShaderResource;
			}

			if (SamplersChanged(VertexShaderSamplers, resources.VertexShaderSamplers))
			{
				result |= PipelineResourceChangeFlags.VertexShaderSampler;
			}

			if (SamplersChanged(PixelShaderSamplers, resources.PixelShaderSamplers))
			{
				result |= PipelineResourceChangeFlags.PixelShaderSampler;
			}

			if (ShaderResourcesChanged(PixelShaderResourceViews, resources.PixelShaderResourceViews))
			{
				result |= PipelineResourceChangeFlags.PixelShaderResource;
			}

			return result | changes;
		}

		/// <summary>
		/// Function to reset to an empty resources object.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Applications should call this after the <see cref="GorgonDrawCallBase"/> is submitted to the <see cref="GorgonGraphics"/> interface if the resources assigned need to be modified.
		/// </para>
		/// </remarks>
		public void Reset()
		{
			PixelShaderResourceViews.Clear();
			VertexShaderResourceViews.Clear();
			PixelShaderSamplers.Clear();

			_changes = PipelineResourceChangeFlags.None;
		}
		#endregion
	}
}
