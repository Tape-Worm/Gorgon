// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: July 6, 2025 3:49:07 PM
//

using System.Buffers;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Memory;
using Gorgon.Native;
using Gorgon.Timing;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Delegate used to define a message callback for the Direct 3D 12 debug information queue.
/// </summary>
/// <param name="message">The message from the information queue.</param>
/// <param name="category">The category of the message.</param>
/// <param name="severity">The severity of the message.</param>
/// <param name="messageID">The ID of the message.</param>
public delegate void GorgonDebugInformationCallback(string message, DebugInfoCategory category, DebugInfoSeverity severity, string messageID);

/// <summary>
/// The primary object for the Gorgon Graphics system
/// </summary>
public unsafe sealed class GorgonGraphics
    : IDisposable
{
    /// <summary>
    /// The maximum time to wait for a fence to be signalled, in milliseconds.
    /// </summary>
    internal const int WaitFenceTimeout = 10_000;

    /// <summary>
    /// The maximum number of slots for root constant buffer values.
    /// </summary>
    public const int MaxRootConstantCount = 16;

    private ComPtr<IDXGIFactory7> _dxgiFactory;
    private ComPtr<IDXGIAdapter4> _dxgiAdapter;
    private ComPtr<ID3D12Device14> _d3dDevice;
    private ComPtr<ID3D12InfoQueue1> _d3dInfoQueue;
    private ComPtr<D3D12MA_Allocator> _allocator;
    private ComPtr<ID3D12RootSignature> _rootSignature;

    private readonly GorgonGraphicsFactory _parentFactory;
    private readonly GorgonGraphicsDebug? _debug;
    private readonly Dictionary<BufferFormat, GorgonBufferFormatSupport> _formatSupport = [];
    private readonly Lock _queuedSubmitLock = new();
    private uint _infoQueueCookie = uint.MaxValue;
    private GorgonNativeBuffer<Guid>? _infoQueueInstanceGuid;
    private static readonly Lock _infoQueueCallbackLock = new();
    private readonly static Dictionary<Guid, GorgonDebugInformationCallback> _infoQueueCallbackMethods = [];
    private int _currentFrame;    
    private readonly ConcurrentBag<ComPtr<ID3D12CommandList>> _commands =[];

    #region Temporary - Delete me.    
    private ComPtr<ID3D12PipelineState> _pso;

    [Obsolete("This is temporary.")]
    internal ref readonly ComPtr<ID3D12PipelineState> Pso => ref _pso;

    [Obsolete("This is temporary.")]
    private void CreatePso(GorgonShader vertexShader, GorgonShader pixelShader)
    {
        if (!_pso.IsNull)
        {
            return;
        }

        /*
        byte[] semName1 = Encoding.ASCII.GetBytes("POSITION\0");
        byte[] semName2 = Encoding.ASCII.GetBytes("COLOR\0");
        byte[] semName3 = Encoding.ASCII.GetBytes("TEXCOORD\0");

        fixed (byte* semName1Ptr = semName1)
        fixed (byte* semName2Ptr = semName2)
        fixed (byte* semName3Ptr = semName3)*/
        fixed (void* vsPtr = vertexShader.ShaderData)
        fixed (void* psPtr = pixelShader.ShaderData)
        {
            /*
            D3D12_INPUT_ELEMENT_DESC* iaDesc = stackalloc D3D12_INPUT_ELEMENT_DESC[3]
            {
                new()
                {
                    SemanticName = (sbyte*)semName1Ptr,
                    Format = DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT,
                    InputSlotClass = D3D12_INPUT_CLASSIFICATION.D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA
                },
                new()
                {
                    SemanticName = (sbyte*)semName2Ptr,
                    Format = DXGI_FORMAT.DXGI_FORMAT_R32G32B32A32_FLOAT,
                    InputSlotClass = D3D12_INPUT_CLASSIFICATION.D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA,
                    AlignedByteOffset = 16
                },
                new()
                {
                    SemanticName = (sbyte*)semName3Ptr,
                    Format = DXGI_FORMAT.DXGI_FORMAT_R32G32_FLOAT,
                    InputSlotClass = D3D12_INPUT_CLASSIFICATION.D3D12_INPUT_CLASSIFICATION_PER_VERTEX_DATA,
                    AlignedByteOffset = 32
                }
            };
            */
            D3D12_GRAPHICS_PIPELINE_STATE_DESC desc = new()
            {
                pRootSignature = _rootSignature.Get(),
                VS = new D3D12_SHADER_BYTECODE(vsPtr, (nuint)vertexShader.ShaderData.Length),
                PS = new D3D12_SHADER_BYTECODE(psPtr, (nuint)pixelShader.ShaderData.Length),
                RasterizerState = D3D12_RASTERIZER_DESC.DEFAULT,
                DepthStencilState = D3D12_DEPTH_STENCIL_DESC.DEFAULT,
                SampleMask = uint.MaxValue,
                PrimitiveTopologyType = D3D12_PRIMITIVE_TOPOLOGY_TYPE.D3D12_PRIMITIVE_TOPOLOGY_TYPE_TRIANGLE,
                NumRenderTargets = 1,
                SampleDesc = new DXGI_SAMPLE_DESC(count: 1, quality: 0)
            };

            desc.BlendState.RenderTarget[0] = new D3D12_RENDER_TARGET_BLEND_DESC()
            {
                BlendEnable = true,
                LogicOpEnable = false,
                LogicOp = D3D12_LOGIC_OP.D3D12_LOGIC_OP_NOOP,
                BlendOp = D3D12_BLEND_OP.D3D12_BLEND_OP_ADD,
                BlendOpAlpha = D3D12_BLEND_OP.D3D12_BLEND_OP_ADD,
                SrcBlend = D3D12_BLEND.D3D12_BLEND_SRC_ALPHA,
                DestBlend = D3D12_BLEND.D3D12_BLEND_INV_SRC_ALPHA,
                SrcBlendAlpha = D3D12_BLEND.D3D12_BLEND_ONE,
                DestBlendAlpha = D3D12_BLEND.D3D12_BLEND_INV_SRC_ALPHA,
                RenderTargetWriteMask = (byte)D3D12_COLOR_WRITE_ENABLE.D3D12_COLOR_WRITE_ENABLE_ALL
            };

            desc.DepthStencilState.DepthEnable = false;
            desc.RTVFormats[0] = DXGI_FORMAT.DXGI_FORMAT_R8G8B8A8_UNORM;

            _d3dDevice.Get()->CreateGraphicsPipelineState(&desc, Win32.__uuidof<ID3D12PipelineState>(), (void**)_pso.GetAddressOf())
                .ThrowIfFailed(GorgonResult.CannotCreate, () => "Failed PSO");
        }
    }

    [Obsolete("This is temporary.")]
    private void DestroyTempStuff()
    {
        _pso.Dispose();
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="vertexShader"></param>
    /// <param name="pixelShader"></param>
    [Obsolete("This is temporary.")]
    public void CreateScaffoldingForTesting(GorgonShader vertexShader, GorgonShader pixelShader)
    {
        CreatePso(vertexShader, pixelShader);
    }
    #endregion

    /// <summary>
    /// Property to return the internal Direct 3D 12 device pointer.
    /// </summary>
    internal ref readonly ComPtr<ID3D12Device14> D3DDevice => ref _d3dDevice;

    /// <summary>
    /// Property to return the internal DXGI adapter pointer.
    /// </summary>    
    internal ref readonly ComPtr<IDXGIAdapter4> DXGIAdapter => ref _dxgiAdapter;

    /// <summary>
    /// Property to return the internal DXGI factory pointer.
    /// </summary>    
    internal ref readonly ComPtr<IDXGIFactory7> DXGIFactory => ref _dxgiFactory;

    /// <summary>
    /// Property to return the descriptor services.
    /// </summary>
    internal DescriptorServices Descriptors
    {
        get;
    }

    /// <summary>
    /// Property to return the memory services.
    /// </summary>
    internal MemoryServices Memory
    {
        get;
    }

    /// <summary>
    /// Property to return the queue related services.
    /// </summary>
    internal QueueServices Queues
    {
        get;
    }

    /// <summary>
    /// Property to return the predefined sampler states.
    /// </summary>
    internal SamplerStates SamplerStates
    {
        get;
    }

    /// <summary>
    /// Property to return the logging interface for the Gorgon graphics API.
    /// </summary>
    public IGorgonLog Log
    {
        get;
    }

    /// <summary>
    /// Property to return whether Gorgon is in debug mode or not.
    /// </summary>
    public bool IsInDebugMode => _debug is not null;

    /// <summary>
    /// Property to return the selected video adapter used for rendering with this graphics interface.
    /// </summary>
    public GorgonVideoAdapterInfo Adapter
    {
        get;
    }

    /// <summary>
    /// Property to return the support for the various buffer formats.
    /// </summary>
    /// <seealso cref="BufferFormat"/>
    public IReadOnlyDictionary<BufferFormat, GorgonBufferFormatSupport> FormatSupport => _formatSupport;

    /// <summary>
    /// Property to return the number of frames that can be in flight on the GPU while rendering.
    /// </summary>
    public int InFlightFrameCount
    {
        get;
    }

    /// <summary>
    /// Property to return the current frame being worked on.
    /// </summary>
    /// <remarks>
    /// To see the total number of frames that can be worked on at once, check the <see cref="InFlightFrameCount"/>.
    /// </remarks>
    /// <see cref="InFlightFrameCount"/>
    public int CurrentFrame => _currentFrame;

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        ComPtr<D3D12MA_Allocator> allocator = default;

        if (disposing)
        {
            // Ensure the frame is ended.
            FinalizeFrame();

            // Let the GPU finish whatever it's doing.
            WaitForGpu(WaitFenceTimeout);

            if (_debug is not null)
            {
                UnregisterDebugInformationCallback();
            }

            // Ensure everything is returned before we continue.
            this.DisposeAll();

            SamplerStates.Dispose();
            allocator = new ComPtr<D3D12MA_Allocator>(Memory.Allocator);
            Descriptors.Dispose();
            Memory.Dispose();

            _infoQueueInstanceGuid?.Dispose();

            // Remove our queues.
            Queues.Dispose();

            _parentFactory.Unregister(this);

            Log.Print($"Destroying {nameof(D3D12MA_Allocator)}...", LoggingLevel.Verbose);

            if (IsInDebugMode)
            {
                Log.Print($"[DEBUG] Destroying {nameof(ID3D12InfoQueue1)}...", LoggingLevel.Verbose);
            }

            Log.Print($"Destroying {nameof(ID3D12Device14)}...", LoggingLevel.Verbose);
            Log.Print($"Destroying {nameof(IDXGIAdapter4)}, and {nameof(IDXGIFactory7)}...", LoggingLevel.Verbose);

            if (!allocator.IsNull)
            {
                DumpMemoryStats(in allocator);
            }
        }

        DestroyTempStuff();

        _rootSignature.Dispose();
        _allocator.Dispose();
        _d3dInfoQueue.Dispose();
        _d3dDevice.Dispose();
        _dxgiAdapter.Dispose();
        _dxgiFactory.Dispose();

        allocator.Dispose();

        if (disposing)
        {
            // Finally, tell us if we've any objects left over.
            _debug?.Report();
        }
    }

    /// <summary>
    /// Function called when a debug message has been recieved.
    /// </summary>
    /// <param name="category">The category of message.</param>
    /// <param name="severity">The severity of message.</param>
    /// <param name="messageID">The ID of the message.</param>
    /// <param name="description">The message text, in ANSI.</param>
    /// <param name="context">The user supplied context.</param>
    [UnmanagedCallersOnly]
    private static void DebugInfoCallback(D3D12_MESSAGE_CATEGORY category, D3D12_MESSAGE_SEVERITY severity, D3D12_MESSAGE_ID messageID, sbyte* description, void* context)
    {
        using (_infoQueueCallbackLock.EnterScope())
        {
            if (_infoQueueCallbackMethods.Count == 0)
            {
                return;
            }

            Guid instanceGuid = *(Guid*)context;

            if (_infoQueueCallbackMethods.TryGetValue(instanceGuid, out GorgonDebugInformationCallback? callback))
            {
                string value = Marshal.PtrToStringAnsi((nint)description) ?? string.Empty;
                callback(value, (DebugInfoCategory)category, (DebugInfoSeverity)severity, messageID.ToString());
            }
        }
    }

    /// <summary>
    /// Function to create the COM object interface used for the Direct 3D 12 information queue for debugging.
    /// </summary>
    /// <returns>The pointer to the D3D 12 information queue.</returns>
    private ComPtr<ID3D12InfoQueue1> CreateDebugInfoQueue()
    {        
        ComPtr<ID3D12InfoQueue1> result = default;

        Log.Print($"[DEBUG] Creating a {nameof(ID3D12InfoQueue1)} object for callbacks...", LoggingLevel.Verbose);

        HRESULT err = D3DDevice.As(ref result);

        if (err.FAILED)
        {
            Log.PrintWarning(err, $"Unable to retrieve the {nameof(ID3D12InfoQueue1)} object. Debugging information will be limited.", LoggingLevel.Intermediate);
            return default;
        }

        // Add any messages we want to ignore here:
        D3D12_MESSAGE_ID* filters = stackalloc D3D12_MESSAGE_ID[2]
        {
            // We don't care if the clear colour isn't optimized.
            D3D12_MESSAGE_ID.D3D12_MESSAGE_ID_CLEARRENDERTARGETVIEW_MISMATCHINGCLEARVALUE,
            D3D12_MESSAGE_ID.D3D12_MESSAGE_ID_CREATE_SAMPLER_COMPARISON_FUNC_IGNORED
        };

        D3D12_INFO_QUEUE_FILTER filter = new()
        {
            DenyList = new D3D12_INFO_QUEUE_FILTER_DESC()
            {
                NumIDs = 1,
                pIDList = filters
            }
        };

        result.Get()->AddStorageFilterEntries(&filter);

        return result;
    }

    /// <summary>
    /// Function to retrieve the multi sample maximum quality level support for a given format.
    /// </summary>
    /// <param name="format">The DXGI format support to evaluate.</param>
    /// <param name="tiled"><b>true</b> for tiled resources, <b>false</b> if not.</param>
    /// <returns>A <see cref="GorgonMultisampleInfo"/> value containing the max count and max quality level.</returns>
    private GorgonMultisampleInfo GetMultisampleSupport(DXGI_FORMAT format, bool tiled)
    {
        ID3D12Device14* devPtr = _d3dDevice.Get();

        for (uint count = D3D12.D3D12_MAX_MULTISAMPLE_SAMPLE_COUNT; count > 1; --count)
        {
            D3D12_FEATURE_DATA_MULTISAMPLE_QUALITY_LEVELS samples = new()
            {
                Format = format,
                SampleCount = count,
                Flags = tiled ? D3D12_MULTISAMPLE_QUALITY_LEVEL_FLAGS.D3D12_MULTISAMPLE_QUALITY_LEVELS_FLAG_TILED_RESOURCE : D3D12_MULTISAMPLE_QUALITY_LEVEL_FLAGS.D3D12_MULTISAMPLE_QUALITY_LEVELS_FLAG_NONE
            };

            if (devPtr->CheckFeatureSupport(D3D12_FEATURE.D3D12_FEATURE_MULTISAMPLE_QUALITY_LEVELS, &samples, (uint)sizeof(D3D12_FEATURE_DATA_MULTISAMPLE_QUALITY_LEVELS)).FAILED)
            {
                continue;
            }

            if (samples.NumQualityLevels < 1)
            {
                continue;
            }

            return new GorgonMultisampleInfo((int)count, (int)samples.NumQualityLevels - 1);
        }

        return GorgonMultisampleInfo.NoMultisampling;
    }

    /// <summary>
    /// Function to enumerate all format support information for the current device object.
    /// </summary>
    private void EnumerateBufferFormatSupport()
    {
        IEnumerable<DXGI_FORMAT> formats = Enum.GetValues<DXGI_FORMAT>().Where(item => item is not DXGI_FORMAT.DXGI_FORMAT_FORCE_UINT and not DXGI_FORMAT.DXGI_FORMAT_R1_UNORM
        and not DXGI_FORMAT.DXGI_FORMAT_SAMPLER_FEEDBACK_MIN_MIP_OPAQUE and not DXGI_FORMAT.DXGI_FORMAT_SAMPLER_FEEDBACK_MIP_REGION_USED_OPAQUE);

        foreach (DXGI_FORMAT format in formats)
        {
            D3D12_FEATURE_DATA_FORMAT_SUPPORT support = new()
            {
                Format = format
            };

            if (D3DDevice.Get()->CheckFeatureSupport(D3D12_FEATURE.D3D12_FEATURE_FORMAT_SUPPORT, &support, (uint)sizeof(D3D12_FEATURE_DATA_FORMAT_SUPPORT)).FAILED)
            {
                continue;
            }

            GorgonMultisampleInfo msInfo = GorgonMultisampleInfo.NoMultisampling;

            if (((support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_MULTISAMPLE_RENDERTARGET) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_MULTISAMPLE_RENDERTARGET)
                || ((support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_MULTISAMPLE_LOAD) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_MULTISAMPLE_LOAD))
            {
                msInfo = GetMultisampleSupport(format, (support.Support2 & D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_TILED) == D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_TILED);
            }

            D3D12_FEATURE_DATA_FORMAT_INFO formatInfo = new()
            {
                Format = format,
                PlaneCount = 0
            };

            if (D3DDevice.Get()->CheckFeatureSupport(D3D12_FEATURE.D3D12_FEATURE_FORMAT_INFO, &formatInfo, (uint)sizeof(D3D12_FEATURE_DATA_FORMAT_INFO)).FAILED)
            {
                formatInfo.PlaneCount = 1;
            }

            GorgonBufferFormatSupport bufferSupport = new((BufferFormat)format,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_BUFFER) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_BUFFER,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_IA_VERTEX_BUFFER) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_IA_VERTEX_BUFFER,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_IA_VERTEX_BUFFER) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_IA_INDEX_BUFFER,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_SO_BUFFER) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_SO_BUFFER,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_TEXTURE1D) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_TEXTURE1D,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_TEXTURE2D) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_TEXTURE2D,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_TEXTURE3D) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_TEXTURE3D,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_TEXTURECUBE) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_TEXTURECUBE,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_SHADER_LOAD) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_SHADER_LOAD,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_SHADER_SAMPLE) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_SHADER_SAMPLE,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_SHADER_SAMPLE_COMPARISON) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_SHADER_SAMPLE_COMPARISON,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_MIP) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_MIP,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_RENDER_TARGET) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_RENDER_TARGET,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_BLENDABLE) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_BLENDABLE,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_DEPTH_STENCIL) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_DEPTH_STENCIL,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_MULTISAMPLE_RESOLVE) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_MULTISAMPLE_RESOLVE,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_DISPLAY) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_DISPLAY,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_CAST_WITHIN_BIT_LAYOUT) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_CAST_WITHIN_BIT_LAYOUT,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_MULTISAMPLE_RENDERTARGET) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_MULTISAMPLE_RENDERTARGET,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_MULTISAMPLE_LOAD) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_MULTISAMPLE_LOAD,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_SHADER_GATHER) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_SHADER_GATHER,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_BACK_BUFFER_CAST) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_BACK_BUFFER_CAST,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_TYPED_UNORDERED_ACCESS_VIEW) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_TYPED_UNORDERED_ACCESS_VIEW,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_SHADER_GATHER_COMPARISON) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_SHADER_GATHER_COMPARISON,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_DECODER_OUTPUT) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_DECODER_OUTPUT,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_VIDEO_ENCODER) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_VIDEO_ENCODER,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_VIDEO_PROCESSOR_OUTPUT) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_VIDEO_PROCESSOR_OUTPUT,
                (support.Support1 & D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_VIDEO_PROCESSOR_INPUT) == D3D12_FORMAT_SUPPORT1.D3D12_FORMAT_SUPPORT1_VIDEO_PROCESSOR_INPUT,
                (support.Support2 & D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_UAV_ATOMIC_ADD) == D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_UAV_ATOMIC_ADD,
                (support.Support2 & D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_UAV_ATOMIC_BITWISE_OPS) == D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_UAV_ATOMIC_BITWISE_OPS,
                (support.Support2 & D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_UAV_ATOMIC_COMPARE_STORE_OR_COMPARE_EXCHANGE) == D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_UAV_ATOMIC_COMPARE_STORE_OR_COMPARE_EXCHANGE,
                (support.Support2 & D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_UAV_ATOMIC_EXCHANGE) == D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_UAV_ATOMIC_EXCHANGE,
                (support.Support2 & D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_UAV_ATOMIC_SIGNED_MIN_OR_MAX) == D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_UAV_ATOMIC_SIGNED_MIN_OR_MAX,
                (support.Support2 & D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_UAV_ATOMIC_UNSIGNED_MIN_OR_MAX) == D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_UAV_ATOMIC_UNSIGNED_MIN_OR_MAX,
                (support.Support2 & D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_UAV_TYPED_LOAD) == D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_UAV_TYPED_LOAD,
                (support.Support2 & D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_UAV_TYPED_STORE) == D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_UAV_TYPED_STORE,
                (support.Support2 & D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_OUTPUT_MERGER_LOGIC_OP) == D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_OUTPUT_MERGER_LOGIC_OP,
                (support.Support2 & D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_TILED) == D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_TILED,
                (support.Support2 & D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_MULTIPLANE_OVERLAY) == D3D12_FORMAT_SUPPORT2.D3D12_FORMAT_SUPPORT2_MULTIPLANE_OVERLAY,
                formatInfo.PlaneCount,
                msInfo);

            _formatSupport[bufferSupport.Format] = bufferSupport;
        }
    }

    /// <summary>
    /// Function to create the main resource memory allocator.
    /// </summary>
    /// <param name="adapterName">Name of the adapter.</param>
    /// <param name="adapter">The selected video adapter to use.</param>
    /// <param name="device">The D3D 12 device object.</param>
    /// <returns>The pointer to the main allocator.</returns>
    private ComPtr<D3D12MA_Allocator> BuildAllocator(string adapterName, ComPtr<IDXGIAdapter4> adapter, ComPtr<ID3D12Device14> device)
    {
        ComPtr<D3D12MA_Allocator> result = default;        

        D3D12MA_ALLOCATOR_DESC desc = new()
        {
            Flags = D3D12MA_ALLOCATOR_FLAGS.D3D12MA_ALLOCATOR_FLAG_NONE,
            pAdapter = (PIDXGIAdapter4)adapter.Get(),
            pDevice = (PID3D12Device14)device.Get(),            
            PreferredBlockSize = 0 // Defaults to 64 MB.
        };
        
        Log.Print($"Creating {nameof(D3D12MA_Allocator)} on {adapterName}.", LoggingLevel.Verbose);
        
        D3D12MemAlloc.D3D12MA_CreateAllocator(&desc, result.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_ALLOCATOR, adapterName));
        
        return result;
    }

    /// <summary>
    /// Function to finalize the currently active frame.
    /// </summary>
    private void FinalizeFrame()
    {
        int nextFrame = Interlocked.Increment(ref _currentFrame);
        nextFrame %= InFlightFrameCount;

        ulong gfxNextFence = Queues.GraphicsQueue.IncrementFence();
        ulong computeNextFence = Queues.ComputeQueue.IncrementFence();
        ulong copyNextFence = Queues.CopyQueue.IncrementFence();

        Interlocked.Exchange(ref Queues.GraphicsQueue.FrameFenceValue[nextFrame], gfxNextFence);
        Interlocked.Exchange(ref Queues.ComputeQueue.FrameFenceValue[nextFrame], computeNextFence);
        Interlocked.Exchange(ref Queues.CopyQueue.FrameFenceValue[nextFrame], copyNextFence);
        Interlocked.Exchange(ref _currentFrame, nextFrame);

        Queues.GraphicsQueue.CollectOutstandingLists();

        SpinWait waiter = new();

        while (!_commands.IsEmpty)
        {
            if (!_commands.TryTake(out ComPtr<ID3D12CommandList> command))
            {
                waiter.SpinOnce();
                continue;
            }

            command.Dispose();
        }
    }

    /// <summary>
    /// Function to submit a single command list at the end of a frame.
    /// </summary>
    /// <param name="commandList">The command list to execute.</param>
    private void SubmitCommandList(GorgonCommandList commandList)
    {
        if (commandList.Graphics != this)
        {
            Log.PrintWarning($"The command list {commandList.Name} was not created by the same graphics object. It will be skipped. Only submit command lists created by the same graphics object.", LoggingLevel.Simple);
            return;
        }

        if (commandList.Queue != Queues.GraphicsQueue)
        {
            Log.PrintWarning($"The command list {commandList.Name} is not from the same queue. It will be skipped. Only submit command lists that are on the same queue.", LoggingLevel.Intermediate);
            return;
        }

        commandList.Close();
        Queues.GraphicsQueue.Execute(commandList, true);

        if (commandList.Presenters.Count > 0)
        {
            commandList.Present();
        }
    }

    /// <summary>
    /// Function to submit multiple command lists at the end of a frame.
    /// </summary>
    /// <param name="commandLists">The list of command lists to execute.</param>
    private void SubmitCommandLists(ReadOnlySpan<GorgonCommandList> commandLists)
    {
        int commandCount = 0;
        int presenterCount = 0;
        ArrayPool<GorgonCommandList> pool = GorgonArrayPools<GorgonCommandList>.GetBestPool(commandLists.Length);
        GorgonCommandList[] commands = pool.Rent(commandLists.Length);
        GorgonCommandList[] presenterCommands = pool.Rent(commandLists.Length);
        ReadOnlySpan<GorgonCommandList> finalCommandList = default;

        try
        {
            for (int i = 0; i < commandLists.Length; ++i)
            {
                GorgonCommandList list = commandLists[i];

                if (list.Graphics != this)
                {
                    Log.PrintWarning($"The command list '{list.Name}' at index {i} was not created by the same graphics object. It will be skipped. Only submit command lists created by the same graphics object.", LoggingLevel.Simple);
                    continue;
                }

                if (list.Queue != Queues.GraphicsQueue)
                {
                    Log.PrintWarning($"The command list '{list.Name}' at index {i} is not from the same queue. It will be skipped. Only submit multple command lists that are on the same queue.", LoggingLevel.Intermediate);
                    continue;
                }

                Queues.GraphicsQueue.Tracker.TrackResource(list.D3DGraphicsCommandList);
                list.Close();

                if (list.Presenters.Count > 0)
                {
                    presenterCommands[presenterCount++] = list;
                }

                commands[commandCount++] = list;
            }

            if (commandCount == 0)
            {
                Log.PrintWarning("There are no command lists passed to this method that can be executed.", LoggingLevel.Simple);
                return;
            }

            finalCommandList = commands.AsSpan(0, commandCount);
            Queues.GraphicsQueue.Execute(finalCommandList, true);

            for (int i = 0; i < presenterCount; ++i)
            {
                presenterCommands[i].Present();
            }
        }
        finally
        {
            if (!finalCommandList.IsEmpty)
            {
                for (int i = 0; i < finalCommandList.Length; ++i)
                {
                    Queues.GraphicsQueue.ListPool.Return(finalCommandList[i]);
                }
            }

            pool.Return(presenterCommands, true);
            pool.Return(commands, true);
        }
    }

    /// <summary>
    /// Function to create a global root signature for Gorgon.
    /// </summary>
    /// <returns>The COM pointer to the root signature.</returns>
    /// <exception cref="GorgonException">Thrown if the root signature failed to create.</exception>
    private ComPtr<ID3D12RootSignature> CreateRootSignature()
    {
        D3D12_VERSIONED_ROOT_SIGNATURE_DESC rootDesc = default;

        using ComPtr<ID3D12DeviceConfiguration1> devConfig = default;
        using ComPtr<ID3DBlob> serialized = default;
        using ComPtr<ID3DBlob> errors = default;
        ComPtr<ID3D12RootSignature> result = default;

        D3D12_ROOT_PARAMETER1* paramList = stackalloc D3D12_ROOT_PARAMETER1[MaxRootConstantCount];

        for (uint i = 0; i < MaxRootConstantCount; ++i)
        {
            paramList[i].InitAsConstantBufferView(i);
        }

        D3D12_VERSIONED_ROOT_SIGNATURE_DESC.Init_1_2(ref rootDesc, MaxRootConstantCount, paramList, 0, null,
                                                     D3D12_ROOT_SIGNATURE_FLAGS.D3D12_ROOT_SIGNATURE_FLAG_CBV_SRV_UAV_HEAP_DIRECTLY_INDEXED
                                                   | D3D12_ROOT_SIGNATURE_FLAGS.D3D12_ROOT_SIGNATURE_FLAG_SAMPLER_HEAP_DIRECTLY_INDEXED);

        Log.Print("Creating Gorgon D3D 12 global bindless root signature...", LoggingLevel.Verbose);

        _d3dDevice.As(&devConfig)
            .ThrowIfFailed((hr) => new InvalidCastException(null, hr));

        HRESULT err = devConfig.Get()->SerializeVersionedRootSignature(&rootDesc, serialized.GetAddressOf(), errors.GetAddressOf());

        if ((err.FAILED) || (serialized.IsNull))
        {
            string exceptionText;

            if (errors.Get() is not null)
            {
                byte* errorData = (byte*)errors.Get()->GetBufferPointer();
                exceptionText = Encoding.ASCII.GetString(errorData, (int)errors.Get()->GetBufferSize());
            }
            else
            {
                Log.PrintError($"Error creating the root signature, HRESULT code = 0x{err.Value:x}. No error message was returned.", LoggingLevel.Simple);
                exceptionText = $"HRESULT: 0x{err.Value:x}";
            }

            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_ROOT, exceptionText));
        }
            
        _d3dDevice.Get()->CreateRootSignature(0, serialized.Get()->GetBufferPointer(), serialized.Get()->GetBufferSize(), Win32.__uuidof<ID3D12RootSignature>(), (void**)result.GetAddressOf());

        return result;
    }

    /// <summary>
    /// Function to dump memory statistics to the debug output.
    /// </summary>
    /// <param name="allocator">The D3D12MA allocator pointer.</param>
    private void DumpMemoryStats(ref readonly ComPtr<D3D12MA_Allocator> allocator)
    {
        D3D12MA_Budget local = default;
        D3D12MA_Budget nonLocal = default;
        D3D12MA_TotalStatistics stats = default;

        if (allocator.IsNull)
        {
            Debug.WriteLine("Memory allocator has been disposed. No stats available.");
            return;
        }

        allocator.Get()->GetBudget(&local, &nonLocal);
        allocator.Get()->CalculateStatistics(&stats);

        Debug.WriteLine("Memory Data:");
        Debug.WriteLine("=======================================================================================================");

        Debug.WriteLine("Local (VRAM):");
        Debug.WriteLine($"   Budget: {local.BudgetBytes.FormatMemory()}");
        Debug.WriteLine($"   Used: {local.UsageBytes.FormatMemory()}");
        Debug.WriteLine($"   Free: {(local.BudgetBytes - local.UsageBytes).FormatMemory()}\n");

        Debug.WriteLine("Non-Local (System):");
        Debug.WriteLine($"   Budget: {nonLocal.BudgetBytes.FormatMemory()}");
        Debug.WriteLine($"   Used: {nonLocal.UsageBytes.FormatMemory()}");
        Debug.WriteLine($"   Free: {(nonLocal.BudgetBytes - nonLocal.UsageBytes).FormatMemory()}\n");

        Debug.WriteLine($"In use by resources: {stats.Total.Stats.AllocationBytes.FormatMemory()} ({stats.Total.Stats.AllocationCount} objects)");
        Debug.WriteLine($"Reserved from OS: {stats.Total.Stats.BlockBytes.FormatMemory()}");
        Debug.WriteLine($"Available in pools: {(stats.Total.Stats.BlockBytes - stats.Total.Stats.AllocationBytes).FormatMemory()}");
        Debug.WriteLine($"Largest contiguous free: {stats.Total.UnusedRangeSizeMax.FormatMemory()}");
    }

    /// <summary>
    /// Function to dump memory statistics to the debug output.
    /// </summary>
    internal void ReportMemory() => DumpMemoryStats(in Memory.Allocator);

    /// <inheritdoc/>
    public void Dispose()
    {
        Log.Print("Waiting for GPU to finish work...", LoggingLevel.Intermediate);
        GorgonTimer sw = new();
        // Wait for the GPU to finish whatever it's doing.
        // Give it 5 seconds, if we take longer than this, we're probably in trouble.
        WaitForGpu(WaitFenceTimeout);
        Log.Print($"GPU finished work in {sw.Elapsed}.", LoggingLevel.Intermediate);

        Log.Print("Shutting down graphics interface...", LoggingLevel.Simple);

        Dispose(true);

        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to register a callback method for capturing debug information messages from the Direct 3D 12 runtime.
    /// </summary>
    /// <param name="callback">The callback method to execute.</param>
    /// <remarks>
    /// <para>
    /// This will allow applications to intercept debug messages from the underlying Direct 3D 12 runtime and act on them. The callback method will be a <see cref="GorgonDebugInformationCallback"/> delegate 
    /// type. 
    /// </para>
    /// <para>
    /// When the debug information callback is no longer required, call the <see cref="UnregisterDebugInformationCallback"/> method.
    /// </para>
    /// <para>
    /// <note type="information">
    /// <para>
    /// Note that this method only works when <see cref="IsInDebugMode"/> is <b>true</b>, otherwise it does nothing and the callback will not be called. Applications can activate debug mode via the 
    /// <see cref="GorgonGraphicsFactory(GorgonGraphicsDebugFlags, IGorgonLog?)"/> constructor.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonGraphicsFactory"/>
    /// <seealso cref="GorgonDebugInformationCallback"/>
    /// <seealso cref="IsInDebugMode"/>
    /// <seealso cref="UnregisterDebugInformationCallback"/>
    public void RegisterDebugInformationCallback(GorgonDebugInformationCallback callback)
    {
        if (!IsInDebugMode)
        {
            return;
        }

        UnregisterDebugInformationCallback();

        using (_infoQueueCallbackLock.EnterScope())
        {
            uint cookie = 0;

            Log.Print("Registering Direct 3D 12 queue information callback.", LoggingLevel.Verbose);

            _infoQueueInstanceGuid = new GorgonNativeBuffer<Guid>(sizeof(Guid));
            _infoQueueInstanceGuid[0] = Guid.NewGuid();

            HRESULT err = _d3dInfoQueue.Get()->RegisterMessageCallback(&DebugInfoCallback, D3D12_MESSAGE_CALLBACK_FLAGS.D3D12_MESSAGE_CALLBACK_FLAG_NONE, (void*)_infoQueueInstanceGuid, &cookie);

            if (err.FAILED)
            {
                Log.PrintError(err, "There was an error registering the callback for the information queue.", LoggingLevel.Intermediate);
                return;
            }

            _infoQueueCookie = cookie;
            _infoQueueCallbackMethods[_infoQueueInstanceGuid[0]] = callback;
        }
    }

    /// <summary>
    /// Function to unregister a previously registered debug information callback.
    /// </summary>
    /// <seealso cref="RegisterDebugInformationCallback(GorgonDebugInformationCallback)"/>
    public void UnregisterDebugInformationCallback()
    {
        using (_infoQueueCallbackLock.EnterScope())
        {
            GorgonNativeBuffer<Guid>? guidBuffer = Interlocked.Exchange(ref _infoQueueInstanceGuid, null);

            if ((guidBuffer is null) || (_d3dInfoQueue.IsNull) || (_infoQueueCallbackMethods.Count == 0) || (!_infoQueueCallbackMethods.Remove(guidBuffer[0])))
            {
                return;
            }

            Log.Print($"[DEBUG] Removing {nameof(ID3D12InfoQueue1)} information callback.", LoggingLevel.Verbose);
            Log.PrintError(_d3dInfoQueue.Get()->UnregisterMessageCallback(_infoQueueCookie), "There was an error unregistering the callback from the information queue.", LoggingLevel.Intermediate);
            _infoQueueCookie = uint.MaxValue;
            guidBuffer.Dispose();
        }
    }

    /// <summary>
    /// Function to make the CPU wait for the GPU to finish its current timeline of work.
    /// </summary>
    /// <param name="timeout">[Optional] The number of milliseconds to wait before continuing.</param>
    public void WaitForGpu(int timeout = Timeout.Infinite) 
    {
        Queues.GraphicsQueue.WaitForGpu(timeout);
        Queues.ComputeQueue.WaitForGpu(timeout);
        Queues.CopyQueue.WaitForGpu(timeout); 
    }

    /// <summary>
    /// Function to create a new command list.
    /// </summary>
    /// <param name="commandListName">[Optional] The name to apply to the command list returned by this method.</param>
    /// <returns>A new <see cref="GorgonCommandList"/> for sending commands to the GPU.</returns>
    /// <remarks>
    /// <para>
    /// This method is used to create command lists to send commands to the GPU. Multiple lists can be created for use in multiple threads, which helps reduce the cost of setting up rendering.
    /// </para>
    /// <para>
    /// The optional <paramref name="commandListName"/> parameter is used to help identify the command list in debugging scenarios, so while it is optional, it is recommended to set this value as it will 
    /// assist in locating which command list is causing an issue.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// While Gorgon is capable of using multiple threads to render, a <see cref="GorgonCommandList"/> is <b>NOT</b> thread safe. Only use a single list per thread.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonCommandList"/>
    public GorgonCommandList GetCommandList(string? commandListName = null)
    {
        commandListName = GorgonGraphicsFactory.GenerateName(commandListName ?? string.Empty, nameof(GorgonCommandList));

        CommandAllocator allocator = Queues.GraphicsQueue.AllocatorPool.Get(commandListName);
        GorgonCommandList list = Queues.GraphicsQueue.ListPool.Get(commandListName, allocator);

        list.BeginRecording(_currentFrame, in _rootSignature);

        _commands.Add(new ComPtr<ID3D12CommandList>(list.D3DCommandList));

        return list;
    }

    /// <summary>
    /// Function to end the current frame, move to the next frame, and clean up per frame resources.
    /// </summary>
    /// <param name="commandList">The command list to execute at the end of the frame.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#warning FINISHME: Need to document.
    public void Submit(GorgonCommandList commandList)
    {
        SubmitCommandList(commandList);

        FinalizeFrame();
    }

    /// <summary>
    /// Function to end the current frame, move to the next frame, and clean up per frame resources.
    /// </summary>
    /// <param name="commandLists">The command lists to execute at the end of the frame.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
#warning FINISHME: Need to document.
    public void Submit(ReadOnlySpan<GorgonCommandList> commandLists)
    {
        if (commandLists.Length != 0)
        {
            SubmitCommandLists(commandLists);
        }
        else
        {
            Log.PrintWarning("There are no command lists passed to this method.", LoggingLevel.Intermediate);
        }

        FinalizeFrame();
    }

    /// <summary>
    /// Function to make the GPU wait for the copy queue if it's in the process of copying data.
    /// </summary>
    /// <exception cref="GorgonException">Thrown if there was a failure during the wait operation.</exception>
    /// <remarks>
    /// <para>
    /// This method is meant to make the graphics queue wait for the copy queue on the GPU. Developers can use this to synchronize the GPU queues. For example, if the copy queue is busy updating a texture 
    /// required by the graphics queue, this will allow the graphics queue to wait until that operation has finished and then it will continue its work.
    /// </para>
    /// <para>
    /// To make use of the copy queue, developers can use the <see cref="GorgonResourceCopier"/> functionality.
    /// </para>
    /// <para>
    /// <note type="information">
    /// <para>
    /// This method is different from the <see cref="WaitForGpu"/> method in that it causes the GPU graphics queue to wait for the copy queue to finish its work. While the <see cref="WaitForGpu"/> stalls 
    /// the CPU until the GPU is finished its work.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="WaitForGpu"/>
    /// <seealso cref="WaitForCompute"/>
    /// <seealso cref="GorgonResourceCopier"/>
    public void WaitForCopy()
    {
        ulong copyQueueFence = Queues.CopyQueue.FenceValue;
        Queues.GraphicsQueue.D3DQueue.Get()->Wait((PID3D12Fence1)Queues.CopyQueue.D3DFence.Get(), copyQueueFence)
            .ThrowIfFailed(GorgonResult.CannotExecute, () => string.Format(Resources.GORGFX_ERR_WAIT_FAILED, nameof(Queues.GraphicsQueue), nameof(Queues.CopyQueue)));
    }

    /// <summary>
    /// Function to make the GPU wait for the compute queue if it's in the process of working with data.
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="WaitForCopy" path="/exception"/>
    /// <para>
    /// This method is meant to make the graphics queue wait for the compute queue on the GPU. Developers can use this to synchronize the GPU queues. For example, if the compute copy queue is busy updating 
    /// a texture required by the graphics queue, this will allow the graphics queue to wait until that operation has finished and then it will continue its work.
    /// </para>
    /// <para>
    /// To make use of the copy queue, developers can use the <see cref="GorgonComputeEngine"/> functionality.
    /// </para>
    /// <para>
    /// <note type="information">
    /// <para>
    /// This method is different from the <see cref="WaitForGpu"/> method in that it causes the GPU graphics queue to wait for the compute queue to finish its work. While the <see cref="WaitForGpu"/> stalls 
    /// the CPU until the GPU is finished its work.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    /// <seealso cref="WaitForGpu"/>
    /// <seealso cref="GorgonResourceCopier"/>
    /// <seealso cref="WaitForCopy"/>
    /// <seealso cref="GorgonComputeEngine"/>
    public void WaitForCompute()
    {
        ulong computeQueueFence = Queues.ComputeQueue.FenceValue;
        Queues.GraphicsQueue.D3DQueue.Get()->Wait((PID3D12Fence1)Queues.ComputeQueue.D3DFence.Get(), computeQueueFence)
            .ThrowIfFailed(GorgonResult.CannotExecute, () => string.Format(Resources.GORGFX_ERR_WAIT_FAILED, nameof(Queues.GraphicsQueue), nameof(Queues.ComputeQueue)));
    }

    /// <summary>
    /// Function to perform clean up of unused heaps.
    /// </summary>
    /// <param name="forceDotNetGc">[Optional] <b>true</b> to force a .NET garbage collection, <b>false</b> to only collect internal data structures.</param>
    /// <remarks>
    /// <para>
    /// Applications can call this method periodically (e.g. on a level load) to clean up any pooled memory that is no longer required.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// This method will clean up all data that is not currently in use by the GPU. This can potentially be a very slow operation, especially when the <paramref name="forceDotNetGc"/> value is set to <b>true</b>. 
    /// Please execise caution when using this method, and only call it if absolutely necessary.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void GarbageCollect(bool forceDotNetGc = false)
    {
        WaitForGpu(WaitFenceTimeout);

        Queues.GarbageCollect();
        Memory.GarbageCollect();
        Descriptors.GarbageCollect();

        if (!forceDotNetGc)
        {
            return;
        }

        GC.Collect(2, GCCollectionMode.Aggressive, true, true);
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~GorgonGraphics() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGraphics"/> class.
    /// </summary>
    /// <param name="adapter">The video adapter to use for rendering.</param>
    /// <param name="parentFactory">The parent factory that created this instance.</param>
    /// <param name="frameCount">The number of frames that can be in flight.</param>
    /// <param name="bufferPercent">The percentage of VRAM to use for the mega buffer.</param>
    /// <param name="dxgiFactory">The DXGI factory used by this interface.</param>
    /// <param name="dxgiAdapter">The DXGI adapter for the selected video adapter.</param>
    /// <param name="d3dDevice">The Direct 3D 12 device used to render.</param>
    /// <param name="debug">The GPU debugging interface.</param>
    /// <param name="log">The log used for debug messaging.</param>
    internal GorgonGraphics(GorgonVideoAdapterInfo adapter, GorgonGraphicsFactory parentFactory, int frameCount, int bufferPercent, ComPtr<IDXGIFactory7> dxgiFactory, ComPtr<IDXGIAdapter4> dxgiAdapter, ComPtr<ID3D12Device14> d3dDevice, GorgonGraphicsDebug? debug, IGorgonLog log)
    {
        Log = log;

        Log.Print($"Creating {nameof(GorgonGraphics)} on video adapter {adapter.Name} with in-flight frame count of {frameCount}.", LoggingLevel.Simple);

        Adapter = adapter;
        InFlightFrameCount = frameCount;
        _parentFactory = parentFactory;
        _debug = debug;

        _dxgiFactory = dxgiFactory;
        _dxgiAdapter = dxgiAdapter;
        _d3dDevice = d3dDevice;

        if (debug is not null)
        {
            _d3dInfoQueue = CreateDebugInfoQueue();
        }

        ComPtr<D3D12MA_Allocator> allocator = BuildAllocator(adapter.Name, _dxgiAdapter, _d3dDevice);

        Memory = new MemoryServices(new CpuResourceHeapPool(this, false), 
            new CpuResourceHeapPool(this, true), 
            new MegaBufferPool(this, bufferPercent),
            in allocator);

        Descriptors = new DescriptorServices(new GpuDescriptorHeap(this, D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_SAMPLER, 2_048),
            new GpuDescriptorHeap(this, D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_CBV_SRV_UAV, 131_072),
            new CpuDescriptorHeapPool(this, D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_RTV),
            new CpuDescriptorHeapPool(this, D3D12_DESCRIPTOR_HEAP_TYPE.D3D12_DESCRIPTOR_HEAP_TYPE_DSV));

        Queues = new QueueServices(new CommandQueue(this, D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_DIRECT),
            new CommandQueue(this, D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COPY),
            new CommandQueue(this, D3D12_COMMAND_LIST_TYPE.D3D12_COMMAND_LIST_TYPE_COMPUTE));

        SamplerStates = new SamplerStates(this);

        EnumerateBufferFormatSupport();
        _rootSignature = CreateRootSignature();
    }
}