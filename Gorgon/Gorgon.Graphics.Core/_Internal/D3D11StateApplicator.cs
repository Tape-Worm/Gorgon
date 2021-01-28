#region MIT
// 
// Gorgon.
// Copyright (C) 2020 Michael Winsor
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
// Created: December 30, 2020 10:45:30 PM
// 
#endregion

using System;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using Gorgon.Math;
using SharpDX.Mathematics.Interop;
using D3D = SharpDX.Direct3D;
using D3D11 = SharpDX.Direct3D11;
using DX = SharpDX;
using DXGI = SharpDX.DXGI;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Functionality to apply Direct3D 11 states and resources.
    /// </summary>
    internal class D3D11StateApplicator
    {
		#region Variables.
		// An empty UAV count to get around an idiotic design decision (no count exposed on the method) for UAVs on compute shaders
		private static readonly int[] _emptyUavCounts = new int[GorgonReadWriteViewBindings.MaximumReadWriteViewCount];
		// An empty UAV to get around an idiotic design decision (no count exposed on the method) for UAVs on compute shaders
		private static readonly D3D11.UnorderedAccessView[] _emptyUavs = new D3D11.UnorderedAccessView[GorgonReadWriteViewBindings.MaximumReadWriteViewCount];
		// An empty SRV.
		private static readonly D3D11.ShaderResourceView[] _emptySrvs = new D3D11.ShaderResourceView[GorgonShaderResourceViews.MaximumShaderResourceViewCount];
		// An empty set of buffers.
		private static readonly D3D11.Buffer[] _emptyConstantBuffers = new D3D11.Buffer[GorgonConstantBuffers.MaximumConstantBufferCount];
		// Empty sampler states.
		private static readonly D3D11.SamplerState[] _emptySamplers = new D3D11.SamplerState[GorgonSamplerStates.MaximumSamplerStateCount];
		// Empty stream out buffers.
		private static readonly D3D11.StreamOutputBufferBinding[] _emptyStreamOut = new D3D11.StreamOutputBufferBinding[GorgonStreamOutBindings.MaximumStreamOutCount];
		// Empty stream out buffers.
		private static readonly D3D11.VertexBufferBinding[] _emptyVertexBuffers = new D3D11.VertexBufferBinding[D3D11.InputAssemblerStage.VertexInputResourceSlotCount];

		// The list of active render targets.
		private readonly GorgonRenderTargetView[] _renderTargets;

		// The updated set stream out method.
		private static Action<D3D11.StreamOutputStage, int, IntPtr, IntPtr> _setStreamOutTargets;
		// The updated set uavs method.
		private static Action<D3D11.OutputMergerStage, int, IntPtr, D3D11.DepthStencilView, int, int, IntPtr, IntPtr> _setUavs;
		// The updated set compute shader uavs method.
		private static Action<D3D11.CommonShaderStage, int, int, IntPtr, IntPtr> _setCsUavs;
		// The updated set render targets method.
		private static Action<D3D11.OutputMergerStage, int, IntPtr, D3D11.DepthStencilView> _setRenderTargets;
		// The updated set scissor rectangle method.
		private static Action<D3D11.RasterizerStage, int, IntPtr> _setScissorRects;

		// The device context used for applying state and resource information.
		private readonly GorgonGraphics _graphics;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the internal set scissor rectangle method that allows us to assign scissor rectangles via a pointer rather than a params array.
		/// </summary>
		private static void FixSetScissorRects()
		{
			MethodInfo methodInfo = typeof(D3D11.RasterizerStage).GetMethod("SetScissorRects", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(int), typeof(IntPtr) }, null);

			if (methodInfo == null)
			{
				// We'll fall back to the params version if we can't get the method for some reason (this really should never happen).
				Debug.Print("[ERROR] Cannot find method SetScissorRects(int, IntPtr) in SharpDX.Direct3D11 assembly. Maybe a different version?");
				return;
			}

			ParameterExpression rasterizerParam = Expression.Parameter(typeof(D3D11.RasterizerStage), "rasterizer");
			ParameterExpression countParam = Expression.Parameter(typeof(int), "count");
			ParameterExpression ptrParam = Expression.Parameter(typeof(IntPtr), "rectPtr");
			MethodCallExpression caller = Expression.Call(rasterizerParam, methodInfo, countParam, ptrParam);

			LambdaExpression setScissorRects = Expression.Lambda(caller, rasterizerParam, countParam, ptrParam);

			_setScissorRects = (Action<D3D11.RasterizerStage, int, IntPtr>)setScissorRects.Compile();
		}

		/// <summary>
		/// Function to retreive the internal set render targets method.
		/// </summary>
		private static void FixSetRenderTargets()
		{
			MethodInfo methodInfo = typeof(D3D11.OutputMergerStage).GetMethod("SetRenderTargets", BindingFlags.NonPublic | BindingFlags.Instance, null, new[]
			{
				typeof(int),					// numViews
				typeof(IntPtr),					// renderTargetViewsOut
				typeof(D3D11.DepthStencilView)  // depthStencilViewRef
			}, null);


			if (methodInfo == null)
			{
				// We'll fall back to the params version if we can't get the method for some reason (this really should never happen).
				Debug.Print("[ERROR] Cannot find method SetRenderTargets(int, IntPtr, DepthStencilView) in SharpDX.Direct3D11 assembly. Maybe a different version?");
				return;
			}

			ParameterExpression omParam = Expression.Parameter(typeof(D3D11.OutputMergerStage), "omStage");
			ParameterExpression numViewsParam = Expression.Parameter(typeof(int), "numViews");
			ParameterExpression rtvsParam = Expression.Parameter(typeof(IntPtr), "renderTargetViewsOut");
			ParameterExpression dsvParam = Expression.Parameter(typeof(D3D11.DepthStencilView), "depthStencilViewRef");			
			MethodCallExpression caller = Expression.Call(omParam, methodInfo, numViewsParam, rtvsParam, dsvParam);

			LambdaExpression resultMethod = Expression.Lambda(caller, omParam, numViewsParam, rtvsParam, dsvParam);

			_setRenderTargets = (Action<D3D11.OutputMergerStage, int, IntPtr, D3D11.DepthStencilView>)resultMethod.Compile();
		}


		/// <summary>
		/// Function to retrieve the internal set uavs method for a compute shader.
		/// </summary>
		private static void FixCsSetUavs()
		{
			//internal abstract void SetUnorderedAccessViews(int startSlot, int numBuffers, IntPtr unorderedAccessBuffer, IntPtr uavCount);
			MethodInfo methodInfo = typeof(D3D11.CommonShaderStage).GetMethod("SetUnorderedAccessViews", BindingFlags.NonPublic | BindingFlags.Instance, null, new[]
			{
				typeof(int),					// startSlot
				typeof(int),					// numBuffers
				typeof(IntPtr),					// unorderedAccessBuffer
				typeof(IntPtr)					// uavCount
			}, null);


			if (methodInfo == null)
			{
				// We'll fall back to the params version if we can't get the method for some reason (this really should never happen).
				Debug.Print("[ERROR] Cannot find method SetUnorderedAccessViews(IntPtr, int, int, IntPtr) in SharpDX.Direct3D11 assembly. Maybe a different version?");
				return;
			}

			ParameterExpression stageParam = Expression.Parameter(typeof(D3D11.CommonShaderStage), "commonShaderStage");
			ParameterExpression startSlotParam = Expression.Parameter(typeof(int), "startSlot");
			ParameterExpression numBuffersParam = Expression.Parameter(typeof(int), "numBuffers");
			ParameterExpression uavsParam = Expression.Parameter(typeof(IntPtr), "unorderedAccessBuffer");
			ParameterExpression countParam = Expression.Parameter(typeof(IntPtr), "uavCount");
			MethodCallExpression caller = Expression.Call(stageParam, methodInfo, startSlotParam, numBuffersParam, uavsParam, countParam);

			LambdaExpression resultMethod = Expression.Lambda(caller, stageParam, startSlotParam, numBuffersParam, uavsParam, countParam);

			_setCsUavs = (Action<D3D11.CommonShaderStage, int, int, IntPtr, IntPtr>)resultMethod.Compile();
		}

		/// <summary>
		/// Function to retrieve the internal set rtvs and uavs method.
		/// </summary>
		private static void FixSetUavs()
		{			
			MethodInfo methodInfo = typeof(D3D11.OutputMergerStage).GetMethod("SetRenderTargetsAndUnorderedAccessViews", BindingFlags.NonPublic | BindingFlags.Instance, null, new[]
			{ 
				typeof(int),					// numRTVs
				typeof(IntPtr),					// renderTargetViewsOut
				typeof(D3D11.DepthStencilView), // depthStencilViewRef
				typeof(int),					// uAvStartSlot
				typeof(int),					// numUAVs
				typeof(IntPtr),					// unorderedAccessViewsOut
				typeof(IntPtr)					// uAVInitialCountsRef
			}, null);


			if (methodInfo == null)
			{
				// We'll fall back to the params version if we can't get the method for some reason (this really should never happen).
				Debug.Print("[ERROR] Cannot find method SetRenderTargetsAndUnorderedAccessViews(int, IntPtr, DepthStencilView, int, int, IntPtr, IntPtr) in SharpDX.Direct3D11 assembly. Maybe a different version?");
				return;
			}

			ParameterExpression stageParam = Expression.Parameter(typeof(D3D11.OutputMergerStage), "outputMerger");
			ParameterExpression countParam = Expression.Parameter(typeof(int), "numRtvs");
			ParameterExpression renderTargetViewsParam = Expression.Parameter(typeof(IntPtr), "renderTargetViewsOut");
			ParameterExpression depthStencil = Expression.Parameter(typeof(D3D11.DepthStencilView), "depthStencilViewRef");
			ParameterExpression startSlotParam = Expression.Parameter(typeof(int), "uAVStartSlot");
			ParameterExpression numUavsParam = Expression.Parameter(typeof(int), "numUAVs");
			ParameterExpression uavsParam = Expression.Parameter(typeof(IntPtr), "unorderedAccessViewsOut");
			ParameterExpression uavsInitCountParam = Expression.Parameter(typeof(IntPtr), "uAVInitialCountsRef");
			MethodCallExpression caller = Expression.Call(stageParam, methodInfo, countParam, renderTargetViewsParam, depthStencil, startSlotParam, numUavsParam, uavsParam, uavsInitCountParam);

			LambdaExpression resultMethod = Expression.Lambda(caller, stageParam, countParam, renderTargetViewsParam, depthStencil, startSlotParam, numUavsParam, uavsParam, uavsInitCountParam);

			_setUavs = (Action<D3D11.OutputMergerStage, int, IntPtr, D3D11.DepthStencilView, int, int, IntPtr, IntPtr>)resultMethod.Compile();
		}

		/// <summary>
		/// Function to retrieve the internal set stream out method that allows us to assign stream out buffers via a pointer rather than a params array.
		/// </summary>
		private static void FixSetStreamOut()
		{
			MethodInfo methodInfo = typeof(D3D11.StreamOutputStage).GetMethod("SetTargets", BindingFlags.NonPublic | BindingFlags.Instance, null, new[] { typeof(int), typeof(IntPtr), typeof(IntPtr) }, null);

			if (methodInfo == null)
			{
				// We'll fall back to the params version if we can't get the method for some reason (this really should never happen).
				Debug.Print("[ERROR] Cannot find method SetTargets(int, IntPtr, IntPtr) in SharpDX.Direct3D11 assembly. Maybe a different version?");
				return;
			}

			ParameterExpression stageParam = Expression.Parameter(typeof(D3D11.StreamOutputStage), "streamOut");
			ParameterExpression countParam = Expression.Parameter(typeof(int), "numBuffers");
			ParameterExpression ptrParam1 = Expression.Parameter(typeof(IntPtr), "sOTargetsOut");
			ParameterExpression ptrParam2 = Expression.Parameter(typeof(IntPtr), "offsetsRef");
			MethodCallExpression caller = Expression.Call(stageParam, methodInfo, countParam, ptrParam1, ptrParam2);

			LambdaExpression resultMethod = Expression.Lambda(caller, stageParam, countParam, ptrParam1, ptrParam2);

			_setStreamOutTargets = (Action<D3D11.StreamOutputStage, int, IntPtr, IntPtr>)resultMethod.Compile();
		}

		/// <summary>
		/// Function to bind an index buffer resource to the pipeline.
		/// </summary>
		/// <param name="indexBuffer">The index buffer to bind.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void BindIndexBuffer(GorgonIndexBuffer indexBuffer)
		{
			D3D11.Buffer native = indexBuffer?.Native;
			DXGI.Format format = indexBuffer == null ? DXGI.Format.Unknown : (indexBuffer.Use16BitIndices ? DXGI.Format.R16_UInt : DXGI.Format.R32_UInt);
			_graphics.D3DDeviceContext.InputAssembler.SetIndexBuffer(native, format, 0);
		}

		/// <summary>
        /// Function to bind stream buffer resources to the pipeline.
        /// </summary>
        /// <param name="streamOutBindings">The bindings to bind to the pipeline.</param>
        /// <param name="indices">The indices that were modified.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void BindStreamOutBuffers(GorgonStreamOutBindings streamOutBindings, in (int Start, int Count) indices)
		{
			if ((indices.Count <= 0) || (_setStreamOutTargets == null))
			{
				_graphics.D3DDeviceContext.StreamOutput.SetTargets(null);
				return;
			}

			unsafe
			{
				IntPtr* bufferPtr = stackalloc IntPtr[indices.Count];
				int* offsetPtr = stackalloc int[indices.Count];
				
				for (int i = 0; i < indices.Count; ++i)
				{
					bufferPtr[i] = streamOutBindings.Native[i].Buffer?.NativePointer ?? IntPtr.Zero;
					offsetPtr[i] = streamOutBindings.Native[i].Offset;					
				}

				_setStreamOutTargets(_graphics.D3DDeviceContext.StreamOutput, indices.Count, (IntPtr)bufferPtr, (IntPtr)offsetPtr);
			}
		}

		/// <summary>
		/// Function to bind unordered access views to the pipeline for a compute shader.
		/// </summary>
		/// <param name="uavs">The unordered access views to bind.</param>
		/// <param name="indices">The indices that were modified.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void BindCsUavs(GorgonReadWriteViewBindings uavs, in (int Start, int Count) indices)
		{
			if ((uavs == null) || (indices.Count == 0))
			{
				_graphics.D3DDeviceContext.ComputeShader.SetUnorderedAccessViews(0, Array.Empty<D3D11.UnorderedAccessView>(), Array.Empty<int>());
				return;
			}

			unsafe
			{
				IntPtr* uavsPtr = stackalloc IntPtr[indices.Count];
				int* uavInitCountPtr = stackalloc int[indices.Count];

				for (int i = 0; i < indices.Count; ++i)
				{
					uavsPtr[i] = uavs.Native[i]?.NativePointer ?? IntPtr.Zero;
					uavInitCountPtr[i] = uavs.Counts[i];
				}

				_setCsUavs(_graphics.D3DDeviceContext.ComputeShader,						  
						  indices.Start,
						  indices.Count,						  
						  (IntPtr)uavsPtr,
						  (IntPtr)uavInitCountPtr);
			}
		}

		/// <summary>
		/// Function to bind unordered access views to the pipeline.
		/// </summary>
		/// <param name="uavs">The unordered access views to bind.</param>
		/// <param name="indices">The indices that were modified.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void BindUavs(GorgonReadWriteViewBindings uavs, in (int Start, int Count) indices)
		{
			if ((uavs == null) || (indices.Count == 0) || (_setUavs == null))
			{
				_graphics.D3DDeviceContext.OutputMerger.SetUnorderedAccessViews(0, Array.Empty<D3D11.UnorderedAccessView>(), Array.Empty<int>());
				return;
			}

			unsafe
			{
				IntPtr* uavsPtr = stackalloc IntPtr[indices.Count];
				int* uavInitCountPtr = stackalloc int[indices.Count];
				
				for (int i = 0; i < indices.Count; ++i)
				{
					uavsPtr[i] = uavs.Native[i]?.NativePointer ?? IntPtr.Zero;
					uavInitCountPtr[i] = uavs.Counts[i];
				}
								
				_setUavs(_graphics.D3DDeviceContext.OutputMerger,
						 -1,
						 IntPtr.Zero,
						 null,
						 indices.Start,
						 indices.Count,
						 (IntPtr)uavsPtr,
						 (IntPtr)uavInitCountPtr);
			}
		}

		/// <summary>
		/// Function to bind vertex buffer resources to the pipeline.
		/// </summary>
		/// <param name="vertexBufferBindings">The bindings to bind to the pipeline.</param>
		/// <param name="indices">The indices that were modified.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void BindVertexBuffers(GorgonVertexBufferBindings vertexBufferBindings, in (int Start, int Count) indices)
		{
			if (indices.Count <= 0)
			{
				_graphics.D3DDeviceContext.InputAssembler.SetVertexBuffers(0, _emptyVertexBuffers);
				return;
			}

			unsafe
			{				
				IntPtr* bufferPtr = stackalloc IntPtr[indices.Count];
				int* stridePtr = stackalloc int[indices.Count];
				int* offsetPtr = stackalloc int[indices.Count];

				for (int i = 0; i < indices.Count; ++i)
				{
					bufferPtr[i] = vertexBufferBindings.Native[i].Buffer?.NativePointer ?? IntPtr.Zero;
					stridePtr[i] = vertexBufferBindings.Native[i].Stride;
					offsetPtr[i] = vertexBufferBindings.Native[i].Offset;
				}
				
				_graphics.D3DDeviceContext.InputAssembler.SetVertexBuffers(indices.Start, indices.Count, (IntPtr)bufferPtr, (IntPtr)stridePtr, (IntPtr)offsetPtr);
			}
		}

		/// <summary>
		/// Function to assign sampler states to a shader
		/// </summary>
		/// <param name="shaderStage">The shader stage to use.</param>
		/// <param name="samplers">The samplers to apply.</param>
        /// <param name="indices">The indices in the resource array that have been updated.</param>
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		private static void BindSamplers(D3D11.CommonShaderStage shaderStage, GorgonSamplerStates samplers, in (int Start, int Count) indices)
		{
			if ((samplers == null) || (indices.Count == 0))
			{
				shaderStage.SetSamplers(0, _emptySamplers);
				return;
			}

			D3D11.SamplerState[] states = samplers.Native;
			shaderStage.SetSamplers(indices.Start, indices.Count, states);
		}


		/// <summary>
		/// Function to bind the constant buffers to a specific shader stage.
		/// </summary>
		/// <param name="shaderType">The shader stage.</param>
		/// <param name="constantBuffers">The constant buffers to bind.</param>
		/// <param name="indices">The indices in the resource array that have been updated.</param>
		private void BindConstantBuffers(ShaderType shaderType, GorgonConstantBuffers constantBuffers, in (int Start, int Count) indices)
		{
			if ((constantBuffers == null) || (indices.Count == 0))
			{
				switch (shaderType)
				{
					case ShaderType.Vertex:
						_graphics.D3DDeviceContext.VSSetConstantBuffers1(0, _emptyConstantBuffers.Length, _emptyConstantBuffers, null, null);
						break;
					case ShaderType.Pixel:
						_graphics.D3DDeviceContext.PSSetConstantBuffers1(0, _emptyConstantBuffers.Length, _emptyConstantBuffers, null, null);
						break;
					case ShaderType.Geometry:
						_graphics.D3DDeviceContext.GSSetConstantBuffers1(0, _emptyConstantBuffers.Length, _emptyConstantBuffers, null, null);
						break;
					case ShaderType.Domain:
						_graphics.D3DDeviceContext.DSSetConstantBuffers1(0, _emptyConstantBuffers.Length, _emptyConstantBuffers, null, null);
						break;
					case ShaderType.Hull:
						_graphics.D3DDeviceContext.HSSetConstantBuffers1(0, _emptyConstantBuffers.Length, _emptyConstantBuffers, null, null);
						break;
					case ShaderType.Compute:
						_graphics.D3DDeviceContext.CSSetConstantBuffers1(0, _emptyConstantBuffers.Length, _emptyConstantBuffers, null, null);
						break;
				}

				return;
			}

			// Ensure that we pick up changes to the constant buffers view range.
			for (int i = 0; i < indices.Count; ++i)
			{
				GorgonConstantBufferView view = constantBuffers[i + indices.Start];

				if ((view == null) || (!view.ViewAdjusted))
				{
					continue;
				}

				view.ViewAdjusted = false;
				constantBuffers.ViewStart[i] = view.StartElement * 16;
				constantBuffers.ViewCount[i] = (view.ElementCount + 15) & ~15;
			}

			D3D11.Buffer[] buffers = constantBuffers.Native;
			int[] viewStarts = constantBuffers.ViewStart;
			int[] viewCounts = constantBuffers.ViewCount;

			switch (shaderType)
			{
				case ShaderType.Vertex:
					_graphics.D3DDeviceContext.VSSetConstantBuffers1(indices.Start, indices.Count, buffers, viewStarts, viewCounts);
					break;
				case ShaderType.Pixel:
					_graphics.D3DDeviceContext.PSSetConstantBuffers1(indices.Start, indices.Count, buffers, viewStarts, viewCounts);
					break;
				case ShaderType.Geometry:
					_graphics.D3DDeviceContext.GSSetConstantBuffers1(indices.Start, indices.Count, buffers, viewStarts, viewCounts);
					break;
				case ShaderType.Domain:
					_graphics.D3DDeviceContext.DSSetConstantBuffers1(indices.Start, indices.Count, buffers, viewStarts, viewCounts);
					break;
				case ShaderType.Hull:
					_graphics.D3DDeviceContext.HSSetConstantBuffers1(indices.Start, indices.Count, buffers, viewStarts, viewCounts);
					break;
				case ShaderType.Compute:
					_graphics.D3DDeviceContext.CSSetConstantBuffers1(indices.Start, indices.Count, buffers, viewStarts, viewCounts);
					break;
			}
		}

		/// <summary>
		/// Function to bind the shader resource views to a specific shader stage.
		/// </summary>
		/// <param name="shaderStage">The shader stage to update.</param>
		/// <param name="srvs">The shader resource views to bind.</param>
		/// <param name="indices">The indices in the resource array that have been updated.</param>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
		private void BindSrvs(D3D11.CommonShaderStage shaderStage, GorgonShaderResourceViews srvs, in (int Start, int Count) indices)
		{
			if ((srvs == null) || (indices.Count == 0))
			{
				shaderStage.SetShaderResources(0, _emptySrvs);
				return;
			}

			D3D11.ShaderResourceView[] states = srvs.Native;

			shaderStage.SetShaderResources(indices.Start, indices.Count, states);
		}

		/// <summary>
		/// Function to bind resources to the pipeline.
		/// </summary>
		/// <param name="resourceChanges">The resource indices to update and change set.</param>
		/// <param name="resourceState">The resources to bind.</param>
		public void BindResourceState(ResourceRanges resourceChanges, D3DState resourceState)
		{
			ref readonly ResourceStateChanges changes = ref resourceChanges.Changes;

			if (changes == ResourceStateChanges.None)
			{
				return;
			}

			// This is ugly as sin, but there's no elegant way to do this that isn't another performance hit.
			if ((changes & ResourceStateChanges.ComputeShader) == ResourceStateChanges.ComputeShader)
			{
				_graphics.D3DDeviceContext.ComputeShader.Set(resourceState.ComputeShader?.NativeShader);
			}

			if ((changes & ResourceStateChanges.StreamOutBuffers) == ResourceStateChanges.StreamOutBuffers)
			{
				BindStreamOutBuffers(resourceState.StreamOutBindings, in resourceChanges.StreamOutBuffers);
			}

			if ((changes & ResourceStateChanges.InputLayout) == ResourceStateChanges.InputLayout)
			{
				_graphics.D3DDeviceContext.InputAssembler.InputLayout = resourceState.InputLayout?.D3DInputLayout;
			}

			if ((changes & ResourceStateChanges.VertexBuffers) == ResourceStateChanges.VertexBuffers)
			{
				BindVertexBuffers(resourceState.VertexBuffers, in resourceChanges.VertexBuffers);
			}

			if ((changes & ResourceStateChanges.IndexBuffer) == ResourceStateChanges.IndexBuffer)
			{
				BindIndexBuffer(resourceState.IndexBuffer);
			}

			if ((changes & ResourceStateChanges.Uavs) == ResourceStateChanges.Uavs)
			{
				BindUavs(resourceState.ReadWriteViews, in resourceChanges.Uavs);
			}
			
			if ((changes & ResourceStateChanges.PsSamplers) == ResourceStateChanges.PsSamplers)
			{
				BindSamplers(_graphics.D3DDeviceContext.PixelShader, resourceState.PsSamplers, in resourceChanges.PixelShaderSamplers);
			}

			if ((changes & ResourceStateChanges.VsSamplers) == ResourceStateChanges.VsSamplers)
			{
				BindSamplers(_graphics.D3DDeviceContext.VertexShader, resourceState.VsSamplers, in resourceChanges.VertexShaderSamplers);
			}
			
			if ((changes & ResourceStateChanges.GsSamplers) == ResourceStateChanges.GsSamplers)
			{
				BindSamplers(_graphics.D3DDeviceContext.GeometryShader, resourceState.GsSamplers, in resourceChanges.GeometryShaderSamplers);
			}

			if ((changes & ResourceStateChanges.DsSamplers) == ResourceStateChanges.DsSamplers)
			{
				BindSamplers(_graphics.D3DDeviceContext.DomainShader, resourceState.DsSamplers, in resourceChanges.DomainShaderSamplers);
			}

			if ((changes & ResourceStateChanges.HsSamplers) == ResourceStateChanges.HsSamplers)
			{
				BindSamplers(_graphics.D3DDeviceContext.HullShader, resourceState.HsSamplers, in resourceChanges.HullShaderSamplers);
			}

			if ((changes & ResourceStateChanges.CsSamplers) == ResourceStateChanges.CsSamplers)
			{
				BindSamplers(_graphics.D3DDeviceContext.ComputeShader, resourceState.CsSamplers, in resourceChanges.ComputeShaderSamplers);
			}
			
			if ((changes & ResourceStateChanges.VsConstants) == ResourceStateChanges.VsConstants)
			{
				BindConstantBuffers(ShaderType.Vertex, resourceState.VsConstantBuffers, in resourceChanges.VertexShaderConstants);
			}

			if ((changes & ResourceStateChanges.PsConstants) == ResourceStateChanges.PsConstants)
			{
				BindConstantBuffers(ShaderType.Pixel, resourceState.PsConstantBuffers, in resourceChanges.PixelShaderConstants);
			}

			if ((changes & ResourceStateChanges.GsConstants) == ResourceStateChanges.GsConstants)
			{
				BindConstantBuffers(ShaderType.Geometry, resourceState.GsConstantBuffers, in resourceChanges.GeometryShaderConstants);
			}

			if ((changes & ResourceStateChanges.DsConstants) == ResourceStateChanges.DsConstants)
			{
				BindConstantBuffers(ShaderType.Domain, resourceState.DsConstantBuffers, in resourceChanges.DomainShaderConstants);
			}

			if ((changes & ResourceStateChanges.HsConstants) == ResourceStateChanges.HsConstants)
			{
				BindConstantBuffers(ShaderType.Hull, resourceState.HsConstantBuffers, in resourceChanges.HullShaderConstants);
			}

			if ((changes & ResourceStateChanges.CsConstants) == ResourceStateChanges.CsConstants)
			{
				BindConstantBuffers(ShaderType.Compute, resourceState.CsConstantBuffers, in resourceChanges.ComputeShaderConstants);
			}
			
			if ((changes & ResourceStateChanges.VsResourceViews) == ResourceStateChanges.VsResourceViews)
			{
				BindSrvs(_graphics.D3DDeviceContext.VertexShader, resourceState.VsSrvs, in resourceChanges.VertexShaderResources);
			}

			if ((changes & ResourceStateChanges.PsResourceViews) == ResourceStateChanges.PsResourceViews)
			{
				BindSrvs(_graphics.D3DDeviceContext.PixelShader, resourceState.PsSrvs, in resourceChanges.PixelShaderResources);
			}

			if ((changes & ResourceStateChanges.GsResourceViews) == ResourceStateChanges.GsResourceViews)
			{
				BindSrvs(_graphics.D3DDeviceContext.GeometryShader, resourceState.GsSrvs, in resourceChanges.GeometryShaderResources);
			}

			if ((changes & ResourceStateChanges.DsResourceViews) == ResourceStateChanges.DsResourceViews)
			{
				BindSrvs(_graphics.D3DDeviceContext.DomainShader, resourceState.DsSrvs, in resourceChanges.DomainShaderResources);
			}

			if ((changes & ResourceStateChanges.HsResourceViews) == ResourceStateChanges.HsResourceViews)
			{
				BindSrvs(_graphics.D3DDeviceContext.HullShader, resourceState.HsSrvs, in resourceChanges.HullShaderResources);
			}

			if ((changes & ResourceStateChanges.CsResourceViews) == ResourceStateChanges.CsResourceViews)
			{
				BindSrvs(_graphics.D3DDeviceContext.ComputeShader, resourceState.CsSrvs, in resourceChanges.ComputeShaderResources);
			}
			
			if ((changes & ResourceStateChanges.CsUavs) == ResourceStateChanges.CsUavs)
			{
				BindCsUavs(resourceState.CsReadWriteViews, in resourceChanges.ComputeShaderUavs);
			}
		}

		/// <summary>
        /// Function to bind a list of render target views and, optionally, a depth/stencil buffer to the pipeline.
        /// </summary>
        /// <param name="renderTargets">The render targets to bind.</param>
        /// <param name="depthStencil">The depth stencil to bind.</param>
        /// <param name="changes">Flags to indicate which items have changed.</param>
		public void BindRenderTargets(ReadOnlySpan<GorgonRenderTargetView> renderTargets, GorgonDepthStencil2DView depthStencil, in (bool RtvsChanged, bool DsvChanged) changes)
		{
			if ((!changes.RtvsChanged) && (!changes.DsvChanged))
			{
				return;
			}

			if (GorgonGraphics.IsDebugEnabled)
			{
				GorgonRenderTargetView rtv = null;
				for (int i = 0; i < renderTargets.Length; ++i)
				{
					if (renderTargets[i] != null)
					{
						rtv = renderTargets[i];
						break;
					}
				}

				Validator.ValidateRtvAndDsv(depthStencil, rtv, renderTargets);
			}

			if (renderTargets.IsEmpty)
			{
				// If we're unbinding all render targets, then reset all state along with it just to be safe.
				_graphics.ClearState();				
				_graphics.D3DDeviceContext.OutputMerger.SetTargets(depthStencil?.Native, (D3D11.RenderTargetView)null);				
				return;
			}

			unsafe
			{
				int rtvCount = renderTargets.Length.Min(D3D11.OutputMergerStage.SimultaneousRenderTargetCount);
				IntPtr* rtv = stackalloc IntPtr[rtvCount];
				for (int i = 0; i < rtvCount; ++i)
				{
					rtv[i] = renderTargets[i]?.Native.NativePointer ?? IntPtr.Zero;
				}

				_setRenderTargets(_graphics.D3DDeviceContext.OutputMerger, rtvCount, (IntPtr)rtv, depthStencil?.Native);
			}

			if (_renderTargets[0] == null)
			{
				return;
			}

			_graphics.SetViewport(new DX.ViewportF(0, 0, _renderTargets[0].Width, _renderTargets[0].Height));
		}

		/// <summary>
		/// Function to apply a pipeline state to the Direct3D immediate context.
		/// </summary>
		/// <param name="state">The state to apply.</param>
		/// <param name="changes">The state changes to apply.</param>
		/// <param name="blendFactor">The factor used to modulate the pixel shader, render target or both.</param>
		/// <param name="blendSampleMask">The mask used to define which samples get updated in the active render targets.</param>
		/// <param name="depthStencilRef">The depth/stencil reference value used when performing a depth/stencil test.</param>
		public void ApplyPipelineState(GorgonPipelineState state, PipelineStateChanges changes, in GorgonColor blendFactor, int blendSampleMask, int depthStencilRef)
        {
            if (changes == PipelineStateChanges.None)
            {
                return;
            }

			if ((changes & PipelineStateChanges.BlendFactor) == PipelineStateChanges.BlendFactor)
			{
				_graphics.D3DDeviceContext.OutputMerger.BlendFactor = blendFactor.ToRawColor4();
			}

			if ((changes & PipelineStateChanges.BlendSampleMask) == PipelineStateChanges.BlendSampleMask)
			{
				_graphics.D3DDeviceContext.OutputMerger.BlendSampleMask = blendSampleMask;
			}

			if ((changes & PipelineStateChanges.DepthStencilReference) == PipelineStateChanges.DepthStencilReference)
			{
				_graphics.D3DDeviceContext.OutputMerger.DepthStencilReference = depthStencilRef;
			}

			if ((changes & PipelineStateChanges.Topology) == PipelineStateChanges.Topology)
			{
				_graphics.D3DDeviceContext.InputAssembler.PrimitiveTopology = (D3D.PrimitiveTopology)state.PrimitiveType;
			}

			if ((changes & PipelineStateChanges.RasterState) == PipelineStateChanges.RasterState)
			{
				_graphics.D3DDeviceContext.Rasterizer.State = state.D3DRasterState;
			}

			if ((changes & PipelineStateChanges.DepthStencilState) == PipelineStateChanges.DepthStencilState)
			{
				_graphics.D3DDeviceContext.OutputMerger.DepthStencilState = state.D3DDepthStencilState;
			}

			if ((changes & PipelineStateChanges.BlendState) == PipelineStateChanges.BlendState)
			{
				_graphics.D3DDeviceContext.OutputMerger.BlendState = state.D3DBlendState;
			}

			if ((changes & PipelineStateChanges.PixelShader) == PipelineStateChanges.PixelShader)
			{
				_graphics.D3DDeviceContext.PixelShader.Set(state.PixelShader?.NativeShader);
			}

			if ((changes & PipelineStateChanges.VertexShader) == PipelineStateChanges.VertexShader)
			{
				_graphics.D3DDeviceContext.VertexShader.Set(state.VertexShader?.NativeShader);
			}

			if ((changes & PipelineStateChanges.GeometryShader) == PipelineStateChanges.GeometryShader)
			{
				_graphics.D3DDeviceContext.GeometryShader.Set(state.GeometryShader?.NativeShader);
			}

			if ((changes & PipelineStateChanges.DomainShader) == PipelineStateChanges.DomainShader)
			{
				_graphics.D3DDeviceContext.DomainShader.Set(state.DomainShader?.NativeShader);
			}

			if ((changes & PipelineStateChanges.HullShader) == PipelineStateChanges.HullShader)
			{
				_graphics.D3DDeviceContext.HullShader.Set(state.HullShader?.NativeShader);
			}
		}

		/// <summary>
        /// Function to bind viewports to the pipeline.
        /// </summary>
        /// <param name="viewports">The list of viewports to bind.</param>
		public void BindViewports(ReadOnlySpan<DX.ViewportF> viewports)
		{
			if (viewports.IsEmpty)
			{
				_graphics.D3DDeviceContext.Rasterizer.SetViewport(0, 0, 1, 1);
				return;
			}

			unsafe
			{
				int length = viewports.Length.Min(16);

				RawViewportF* viewport = stackalloc RawViewportF[length];

				for (int i = 0; i < length; ++i)
				{
					viewport[i] = viewports[i];
				}

				_graphics.D3DDeviceContext.Rasterizer.SetViewports(viewport, length);
			}
		}

		/// <summary>
		/// Function to bind scissor rectangles to the pipeline.
		/// </summary>
		/// <param name="scissors">The list of scissor rectangles to bind.</param>
		public void BindScissorRectangles(ReadOnlySpan<DX.Rectangle> scissors)
		{
			if (scissors.IsEmpty)
			{
				_graphics.D3DDeviceContext.Rasterizer.SetScissorRectangle(0, 0, 16384, 16384);
				return;
			}

			unsafe
			{
				int length = scissors.Length.Min(16);

				RawRectangle* scissor = stackalloc RawRectangle[length];

				for (int i = 0; i < length; ++i)
				{
					scissor[i] = scissors[i];
				}

				_setScissorRects?.Invoke(_graphics.D3DDeviceContext.Rasterizer, length, (IntPtr)scissor);
			}
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>Initializes a new instance of the <see cref="D3D11StateApplicator" /> class.</summary>
		/// <param name="graphics">The graphics interface that owns this applicator.</param>
		/// <param name="targets">The list of active render targets.</param>
		public D3D11StateApplicator(GorgonGraphics graphics, GorgonRenderTargetView[] targets)
		{
			_graphics = graphics;
			_renderTargets = targets;
		}

		/// <summary>
		/// Static constructor.
		/// </summary>
		static D3D11StateApplicator() 
		{
			FixSetStreamOut();
			FixSetUavs();
			FixCsSetUavs();
			FixSetRenderTargets();
			FixSetScissorRects();
		}
        #endregion
    }
}
