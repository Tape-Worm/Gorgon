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
// Created: August 1, 2017 11:00:30 PM
// 
#endregion

using System;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// An engine used to perform computation on the GPU.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This system uses multiple threads/waves to perform work in parallel on the GPU, which gives exceptional performance when executing expensive computations.
    /// </para>
    /// <para>
    /// The compute engine sends compuational work to the GPU via a <see cref="GorgonComputeShader"/>. This interface is different from the <see cref="GorgonGraphics"/> interface in that it does not rely 
    /// on the standard GPU pipeline to execute, and is stateful (i.e. applications set a state, run the engine, set another state, run again, etc...).
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// This engine requires a <c>Feature Level 11</c> video device or better. If one is not passed to the constructor, then an exception will be thrown.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphics"/>
    public sealed class GorgonComputeEngine
    {
        #region Constants.
        /// <summary>
        /// The maximum number of thread groups that can be sent when executing the shader.
        /// </summary>
        public const int MaxThreadGroupCount = D3D11.ComputeShaderStage.DispatchMaximumThreadGroupsPerDimension + 1;
        #endregion

        #region Variables.
        // The current compute shader to execute.
        private GorgonComputeShader _currentShader;
        // A buffer for unordered access views to apply.
        private D3D11.UnorderedAccessView[] _uavBuffer = new D3D11.UnorderedAccessView[0];
        // A buffer for uav counts to apply.
        private int[] _countBuffer = new int[0];
        // A buffer for sampler states to apply.
        private readonly D3D11.SamplerState[] _samplerStates = new D3D11.SamplerState[GorgonSamplerStates.MaximumSamplerStateCount];
        // A log for debug logging.
        private readonly IGorgonLog _log;
        #endregion

        #region Properties.
        /// <summary>
        /// Property to return the graphics interface that owns this object.
        /// </summary>
        public GorgonGraphics Graphics
        {
            get;
        }

        /// <summary>
        /// Property to return the constant buffers to bind to the compute shader.
        /// </summary>
        public GorgonConstantBuffers ConstantBuffers
        {
            get;
        }

        /// <summary>
        /// Property to return the unordered access views to bind to the compute shader.
        /// </summary>
        public GorgonUavBindings UnorderedAccessViews
        {
            get;
        }

        /// <summary>
        /// Property to return the shader resource views to bind to the compute shader.
        /// </summary>
        public GorgonShaderResourceViews ShaderResourceViews
        {
            get;
        }

        /// <summary>
        /// Property to return the sampler states to bind to the compute shader.
        /// </summary>
        public GorgonSamplerStates SamplerStates
        {
            get;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to apply any constant buffers required for the shader.
        /// </summary>
        private void ApplyConstantBuffers()
        {
            ref (int Start, int Count) buffers = ref ConstantBuffers.GetDirtyItems();

            // If no buffers are available, then we're done.
            if (buffers.Count == 0)
            {
                return;
            }

            Graphics.D3DDeviceContext.ComputeShader.SetConstantBuffers(buffers.Start, buffers.Count, ConstantBuffers.Native);
        }

        /// <summary>
        /// Function to apply any unordered access views required by the shader.
        /// </summary>
        private void ApplyUavs()
        {
            ref (int Start, int Count) uavs = ref UnorderedAccessViews.GetDirtyItems();

            // If no uavs are available, then we're done.
            if (uavs.Count == 0)
            {
                return;
            }

            Graphics.ValidateComputeWork(UnorderedAccessViews, ref uavs);

            if (_uavBuffer.Length != uavs.Count)
            {
                _uavBuffer = new D3D11.UnorderedAccessView[uavs.Count];
                _countBuffer = new int[uavs.Count];
            }

            for (int i = 0; i < _uavBuffer.Length; ++i)
            {
                _uavBuffer[i] = UnorderedAccessViews.Native[i];
                _countBuffer[i] = UnorderedAccessViews.Counts[i];
            }

            Graphics.D3DDeviceContext.ComputeShader.SetUnorderedAccessViews(uavs.Start, _uavBuffer, _countBuffer);
        }

        /// <summary>
        /// Function to apply any shader resource views required for the shader.
        /// </summary>
        private void ApplySrvs()
        {
            ref (int Start, int Count) views = ref ShaderResourceViews.GetDirtyItems();

            // If no views are available, then we're done.
            if (views.Count == 0)
            {
                return;
            }
            
            Graphics.D3DDeviceContext.ComputeShader.SetShaderResources(views.Start, views.Count, ShaderResourceViews.Native);
        }

        /// <summary>
        /// Function to apply any sampler states required for the shader.
        /// </summary>
        private void ApplySamplers()
        {
            ref (int Start, int Count) samplers = ref SamplerStates.GetDirtyItems();

            // If no views are available, then we're done.
            if (samplers.Count == 0)
            {
                return;
            }

            for (int i = samplers.Start; i < samplers.Start + samplers.Count; ++i)
            {
                GorgonSamplerState state = SamplerStates[i];

                if (state == null)
                {
                    _samplerStates[i - samplers.Start] = null;
                    continue;
                }

                if (state.Native == null)
                {
                    state.Native = SamplerStateFactory.GetSamplerState(Graphics, state, _log);
                }

                _samplerStates[i - samplers.Start] = state.Native;
            }

            Graphics.D3DDeviceContext.ComputeShader.SetSamplers(samplers.Start, samplers.Count, _samplerStates);
        }

        /// <summary>
        /// Function to unbind the compute shader and any buffers from the compute engine.
        /// </summary>
        public void Unbind()
        {
            if (_currentShader != null)
            {
                Graphics.D3DDeviceContext.ComputeShader.Set(null);
                _currentShader = null;
            }

            // Unbind from the GPU.
            (int Start, int Count) buffers = ConstantBuffers.GetDirtyItems();
            if (buffers.Count > 0)
            {
                ConstantBuffers.Clear();
                Graphics.D3DDeviceContext.ComputeShader.SetConstantBuffers(buffers.Start, buffers.Count, ConstantBuffers.Native);
            }

            (int Start, int Count) uavs = UnorderedAccessViews.GetDirtyItems();
            if ((uavs.Count > 0) && (_uavBuffer.Length > 0))
            {
                UnorderedAccessViews.Clear();
                Array.Clear(_uavBuffer, 0, _uavBuffer.Length);
                Graphics.D3DDeviceContext.ComputeShader.SetUnorderedAccessViews(0, _uavBuffer);
            }

            (int Start, int Count) samplers = SamplerStates.GetDirtyItems();
            if (samplers.Count > 0)
            {
                SamplerStates.Clear();
                Graphics.D3DDeviceContext.ComputeShader.SetSamplers(samplers.Start, samplers.Count, _samplerStates);
            }

            (int Start, int Count) views = ShaderResourceViews.GetDirtyItems();
            if (views.Count <= 0)
            {
                return;
            }

            ShaderResourceViews.Clear();
            Graphics.D3DDeviceContext.ComputeShader.SetShaderResources(views.Start, views.Count, ShaderResourceViews.Native);
        }

        /// <summary>
        /// Function to execute a <see cref="GorgonComputeShader"/>.
        /// </summary>
        /// <param name="computeShader">The compute shader to execute.</param>
        /// <param name="threadGroupCountX">The number of thread groups to dispatch in the X direction.</param>
        /// <param name="threadGroupCountY">The number of thread groups to dispatch in the Y direction.</param>
        /// <param name="threadGroupCountZ">The number of thread groups to dispatch in the Z direction.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="computeShader"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="threadGroupCountX"/>, <paramref name="threadGroupCountY"/>, or the <paramref name="threadGroupCountZ"/> parameter 
        /// is less than 0, or not less than <see cref="MaxThreadGroupCount"/>.</exception>
        /// <remarks>
        /// <para>
        /// This will take the <paramref name="computeShader"/> and execute it. This method will also bind any buffers set up to the GPU prior to executing the shader.
        /// </para>
        /// <para>
        /// The <paramref name="computeShader"/> will be run in parallel on many threads within a thread group. To understand how thread indexes map to the number of threads defined in the shader, please 
        /// visit the MSDN documentation for the <a href="https://msdn.microsoft.com/en-us/library/windows/desktop/ff476405(v=vs.85).aspx" target="_blank">Dispatch</a> function.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// For performance reasons, this method will only throw exceptions when Gorgon is compiled as <b>DEBUG</b>.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void Execute(GorgonComputeShader computeShader, int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ)
        {
            computeShader.ValidateObject(nameof(computeShader));
            threadGroupCountX.ValidateRange(nameof(threadGroupCountX), 0, MaxThreadGroupCount);
            threadGroupCountY.ValidateRange(nameof(threadGroupCountY), 0, MaxThreadGroupCount);
            threadGroupCountZ.ValidateRange(nameof(threadGroupCountZ), 0, MaxThreadGroupCount);

            if (computeShader != _currentShader)
            {
                _currentShader = computeShader;
                Graphics.D3DDeviceContext.ComputeShader.Set(_currentShader.NativeShader);
            }

            if (ConstantBuffers.IsDirty)
            {
                ApplyConstantBuffers();
            }
            if (UnorderedAccessViews.IsDirty)
            {
                ApplyUavs();
            }
            if (ShaderResourceViews.IsDirty)
            {
                ApplySrvs();
            }
            if (SamplerStates.IsDirty)
            {
                ApplySamplers();
            }

            Graphics.D3DDeviceContext.Dispatch(threadGroupCountX, threadGroupCountY, threadGroupCountZ);
        }

        /// <summary>
        /// Function to execute a <see cref="GorgonComputeShader"/> using a buffer for argument passing.
        /// </summary>
        /// <param name="computeShader">The compute shader to execute.</param>
        /// <param name="indirectArgs">The buffer containing the arguments for the compute shader.</param>
        /// <param name="threadGroupOffset">[Optional] The offset within the buffer, in bytes, to where the arguments are stored.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="computeShader"/>, or the <paramref name="indirectArgs"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="threadGroupOffset"/> is less than 0.</exception>
        /// <remarks>
        /// <para>
        /// This will take the <paramref name="computeShader"/> and execute it using the <paramref name="indirectArgs"/> buffer. This method will also bind any buffers set up to the GPU prior to executing 
        /// the shader.
        /// </para>
        /// <para>
        /// The <paramref name="indirectArgs"/> buffer must contain the thread group count arguments for the <paramref name="computeShader"/>. The <paramref name="threadGroupOffset"/>, will instruct the GPU 
        /// to begin reading these arguments at the specified offset.
        /// </para>
        /// <para>
        /// This method differs from the <see cref="Execute(Gorgon.Graphics.Core.GorgonComputeShader,int,int,int)"/> overload in that it uses a buffer to retrieve the arguments to send to the next compute 
        /// shader workload. Like the <see cref="GorgonGraphics.SubmitStreamOut"/> method, this method takes a variable sized output from a previous compute shader workload and allows it to be passed directly 
        /// to the shader without having to stall on the CPU side by retrieving count values.
        /// </para>
        /// <para>
        /// <note type="important">
        /// <para>
        /// For performance reasons, this method will only throw exceptions when Gorgon is compiled as <b>DEBUG</b>.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        public void Execute(GorgonComputeShader computeShader, GorgonIndirectArgumentBuffer indirectArgs, int threadGroupOffset = 0)
        {
            computeShader.ValidateObject(nameof(computeShader));
            indirectArgs.ValidateObject(nameof(computeShader));
            threadGroupOffset.ValidateRange(nameof(threadGroupOffset), 0, Int32.MaxValue);

            if (computeShader != _currentShader)
            {
                _currentShader = computeShader;
                Graphics.D3DDeviceContext.ComputeShader.Set(_currentShader.NativeShader);
            }

            if (ConstantBuffers.IsDirty)
            {
                ApplyConstantBuffers();
            }
            if (UnorderedAccessViews.IsDirty)
            {
                ApplyUavs();
            }
            if (ShaderResourceViews.IsDirty)
            {
                ApplySrvs();
            }
            if (SamplerStates.IsDirty)
            {
                ApplySamplers();
            }

            Graphics.D3DDeviceContext.DispatchIndirect(indirectArgs.NativeBuffer, threadGroupOffset);
        }
        #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonComputeEngine"/> class.
        /// </summary>
        /// <param name="graphics">The graphics interface that allows access to the GPU.</param>
        /// <param name="log">[Optional] A log for debug logging.</param>
        /// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
        /// <exception cref="ArgumentException">Thrown if the device is not a feature level 11 or better device.</exception>
        /// <remarks> 
        /// <para>
        /// <note type="warning">
        /// <para>
        /// This engine requires a <c>Feature Level 11</c> video device or better. If one is not present on the <paramref name="graphics"/> parameter, then an exception will be thrown.
        /// </para>
        /// </note>
        /// </para>
        /// </remarks>
        /// <seealso cref="GorgonGraphics"/>
        public GorgonComputeEngine(GorgonGraphics graphics, IGorgonLog log = null)
        {
            if (graphics == null)
            {
                throw new ArgumentNullException(nameof(graphics));
            }

            if (graphics.VideoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0));
            }

            _log = log ?? GorgonLogDummy.DefaultInstance;
            Graphics = graphics;
            ConstantBuffers = new GorgonConstantBuffers();
            UnorderedAccessViews = new GorgonUavBindings();
            ShaderResourceViews = new GorgonShaderResourceViews();
            SamplerStates = new GorgonSamplerStates();
        }
        #endregion
    }
}
