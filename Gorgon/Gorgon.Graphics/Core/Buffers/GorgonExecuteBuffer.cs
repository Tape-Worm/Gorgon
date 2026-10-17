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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
//
// Created: October 9, 2026 11:49:15 PM
//

using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A buffer used to store the commands, and the counters for the number of commands to execute, for an execute call.
/// </summary>
/// <remarks>
/// <para>
/// An execute buffer holds <see cref="CommandCount"/> commands of <see cref="CommandSizeInBytes"/> bytes each, followed by <see cref="CounterCount"/> counters. The commands are read by a 
/// <see cref="GorgonExecuteCall"/> or <see cref="GorgonIndexedExecuteCall"/> when it is executed, and the counters hold the number of commands to execute when shaders write the commands on the GPU. The layout 
/// of a command, and examples of writing commands, are described on <see cref="GorgonExecuteCall"/> and <see cref="GorgonIndexedExecuteCall"/>.
/// </para>
/// <para>
/// For an execute call created with <see cref="ExecuteUsage.CpuRendering"/>, the application writes the commands into the buffer like any other buffer data. Counters and read/write access are not needed.
/// </para>
/// <para>
/// For an execute call created with <see cref="ExecuteUsage.GpuRendering"/>, shaders write the commands and the counters through the read/write views returned by 
/// <see cref="GetCommandsReadWriteView(int, int?)"/> and <see cref="GetCountersReadWriteView(int, int?)"/>. The buffer must be created with at least 1 counter, and with the 
/// <see cref="GorgonCommonBufferInfo.HasReadWriteAccess"/> property set to <b>true</b>.
/// </para>
/// </remarks>
/// <seealso cref="GorgonExecuteBufferInfo"/>
/// <seealso cref="GorgonExecuteCall"/>
/// <seealso cref="GorgonIndexedExecuteCall"/>
public sealed unsafe class GorgonExecuteBuffer
    : GorgonGpuBufferCommon, IGorgonExecuteBufferInfo
{
    /// <summary>
    /// A unique key for a view.
    /// </summary>
    /// <param name="StartIndex">The index of the first element in the view.</param>
    /// <param name="Count">The number of elements in the view.</param>
    private readonly record struct ViewKey(long StartIndex, int Count);

    private ComPtr<D3D12MA_Allocation> _bufferAllocation;

    private D3D12_RESOURCE_DESC1 _d3dDesc;
    private readonly GorgonExecuteBufferInfo _info;
    private readonly Dictionary<ViewKey, GorgonRawBufferRwView> _rawUavs = [];
    private readonly Lock _viewLock = new();

    /// <inheritdoc/>
    /// <remarks>
    /// This buffer has its own resource and never has an offset. It will always return 0.
    /// </remarks>
    internal override ulong ResourceOffset => 0;

    /// <inheritdoc/>
    internal override bool IsMegaBufferResource => false;

    /// <summary>
    /// Property to return the offset, in bytes, of the first counter in the buffer.
    /// </summary>
    internal ulong CountersOffset => (ulong)CommandCount * (ulong)CommandSizeInBytes;

    /// <inheritdoc cref="IGorgonExecuteBufferInfo.CommandCount"/>
    public int CommandCount => _info.CommandCount;

    /// <inheritdoc cref="IGorgonExecuteBufferInfo.CommandSizeInBytes"/>
    public int CommandSizeInBytes => _info.CommandSizeInBytes;

    /// <inheritdoc cref="IGorgonExecuteBufferInfo.CounterCount"/>
    public int CounterCount => _info.CounterCount;

    /// <summary>
    /// Function to create the native backing resources for the buffer.
    /// </summary>
    private void CreateNative()
    {
        using ComPtr<ID3D12Resource2> resource = default;
        D3D12_RESOURCE_FLAGS flags = HasReadWriteAccess ? D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_ALLOW_UNORDERED_ACCESS : D3D12_RESOURCE_FLAGS.D3D12_RESOURCE_FLAG_NONE;
        D3D12_RESOURCE_DESC1 desc = D3D12_RESOURCE_DESC1.Buffer((ulong)SizeInBytes, flags);
        D3D12MA_ALLOCATION_DESC allocDesc = new(D3D12_HEAP_TYPE.D3D12_HEAP_TYPE_DEFAULT);

        Graphics.Memory.Allocator.Get()->CreateResource3(&allocDesc, &desc, D3D12_BARRIER_LAYOUT.D3D12_BARRIER_LAYOUT_UNDEFINED,
            null, 0, null,
            _bufferAllocation.GetAddressOf(), Win32.__uuidof<ID3D12Resource2>(), (void**)resource.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => string.Format(Resources.GORGFX_ERR_CANNOT_CREATE_RESOURCE));

        resource.SetD3DDebugName(Name);

        _d3dDesc = desc;

        AssignResource(in resource);
    }

    /// <summary>
    /// Function to return a cached raw read/write view over a range of 4 byte elements in the buffer.
    /// </summary>
    /// <param name="startIndex">The index of the first element in the view.</param>
    /// <param name="count">The number of elements in the view.</param>
    /// <returns>The raw read/write view.</returns>
    private GorgonRawBufferRwView GetRawView(long startIndex, int count)
    {
        using (_viewLock.EnterScope())
        {
            GorgonRawBufferRwView.ValidateRawView(Name, SizeInBytes, ResourceOffset, HasReadWriteAccess);

            ViewKey key = new(startIndex, count);

            if (_rawUavs.TryGetValue(key, out GorgonRawBufferRwView? result))
            {
                return result;
            }

            return _rawUavs[key] = new GorgonRawBufferRwView(Graphics, Name, this, startIndex, count, false);
        }
    }

    /// <inheritdoc/>
    private protected sealed override void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (GorgonRawBufferRwView view in _rawUavs.Values)
            {
                view.Dispose();
            }

            _rawUavs.Clear();
        }

        _bufferAllocation.Dispose();

        base.Dispose(disposing);
    }

    /// <inheritdoc/>
    private protected override void ValidateInfo()
    {
        if ((CommandCount < 1) || (CommandSizeInBytes < sizeof(D3D12_DRAW_ARGUMENTS)) || ((CommandSizeInBytes % sizeof(uint)) != 0) || (CounterCount < 0))
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_EXECUTE_BUFFER_INVALID, Name, CommandCount, CommandSizeInBytes, CounterCount));
        }
    }

    /// <inheritdoc/>
    private protected override void OnGetResourceInfo(out GpuResourceInfo resourceInfo) => resourceInfo = GpuResourceInfo.FromD3D(in _d3dDesc);

    /// <summary>
    /// Function to return a raw read/write view of the commands in the buffer.
    /// </summary>
    /// <param name="firstCommand">[Optional] The index (not the byte offset) of the first command in the view.</param>
    /// <param name="commandCount">[Optional] The number of commands in the view, or <b>null</b> for all commands after the <paramref name="firstCommand"/>.</param>
    /// <returns>A <see cref="GorgonRawBufferRwView"/> for the commands.</returns>
    /// <exception cref="GorgonException">
    /// <inheritdoc cref="GorgonRawBufferRwView.ValidateRawView(string, long, ulong, bool)" path="/exception/para[@type='norw']"/>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This allows shaders to write the commands as raw data when the commands are written on the GPU (see <see cref="ExecuteUsage.GpuRendering"/>). The view starts at the first 32-bit value of the 
    /// <paramref name="firstCommand"/>, and each command is <see cref="CommandSizeInBytes"/> bytes long, so command <i>n</i> in the view starts at byte <i>n</i> × <see cref="CommandSizeInBytes"/>.
    /// </para>
    /// <inheritdoc cref="GorgonShaderBufferRwView" path="/remarks/para"/>
    /// <para>
    /// The range is clipped to the commands in the buffer.
    /// </para>
    /// <inheritdoc cref="GorgonGpuBuffer.GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
    /// <para>
    /// The default values are 0 for the <paramref name="firstCommand"/>, and <b>null</b> for the <paramref name="commandCount"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonShaderBufferRwView.GetViewHandle()"/>
    /// <seealso cref="GorgonRawBufferRwView"/>
    public GorgonRawBufferRwView GetCommandsReadWriteView(int firstCommand = 0, int? commandCount = null)
    {
        firstCommand = firstCommand.Max(0).Min(CommandCount - 1);
        commandCount ??= CommandCount - firstCommand;
        commandCount = commandCount.Value.Max(1).Min(CommandCount - firstCommand);

        int elementsPerCommand = CommandSizeInBytes / sizeof(uint);

        return GetRawView((long)firstCommand * elementsPerCommand, commandCount.Value * elementsPerCommand);
    }

    /// <summary>
    /// Function to return a raw read/write view of the counters in the buffer.
    /// </summary>
    /// <param name="firstCounter">[Optional] The index (not the byte offset) of the first counter in the view.</param>
    /// <param name="counterCount">[Optional] The number of counters in the view, or <b>null</b> for all counters after the <paramref name="firstCounter"/>.</param>
    /// <returns>A <see cref="GorgonRawBufferRwView"/> for the counters.</returns>
    /// <exception cref="GorgonException"><para>
    /// Thrown if the buffer has no counters.
    /// </para>
    /// <inheritdoc cref="GorgonRawBufferRwView.ValidateRawView(string, long, ulong, bool)" path="/exception/para[@type='norw']"/>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This allows shaders to update the counters as raw data when the commands are written on the GPU (see <see cref="ExecuteUsage.GpuRendering"/>). The view starts at the <paramref name="firstCounter"/>, 
    /// and each counter is a 32-bit unsigned integer, so counter <i>n</i> in the view is at byte <i>n</i> × 4. A shader usually adds 1 to a counter (e.g. with <c>InterlockedAdd</c>) for each command it 
    /// writes.
    /// </para>
    /// <para>
    /// The counters must be reset (e.g. with <see cref="GorgonCommandList.ClearReadWriteView(GorgonRawBufferRwView, System.Runtime.Intrinsics.Vector128{int})"/>) before shaders increment them.
    /// </para>
    /// <inheritdoc cref="GorgonShaderBufferRwView" path="/remarks/para"/>
    /// <para>
    /// The range is clipped to the counters in the buffer.
    /// </para>
    /// <inheritdoc cref="GorgonGpuBuffer.GetConstantBufferView(bool)" path="/remarks/para[@type='bindless_doc']"/>
    /// <para>
    /// The default values are 0 for the <paramref name="firstCounter"/>, and <b>null</b> for the <paramref name="counterCount"/>.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonShaderBufferRwView.GetViewHandle()"/>
    /// <seealso cref="GorgonRawBufferRwView"/>
    public GorgonRawBufferRwView GetCountersReadWriteView(int firstCounter = 0, int? counterCount = null)
    {
        if (CounterCount < 1)
        {
            throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_EXECUTE_BUFFER_NO_COUNTERS, Name));
        }

        firstCounter = firstCounter.Max(0).Min(CounterCount - 1);
        counterCount ??= CounterCount - firstCounter;
        counterCount = counterCount.Value.Max(1).Min(CounterCount - firstCounter);

        return GetRawView((long)(CountersOffset / sizeof(uint)) + firstCounter, counterCount.Value);
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~GorgonExecuteBuffer() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonExecuteBuffer"/> class.
    /// </summary>
    /// <param name="graphics"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='graphics']"/></param>
    /// <param name="name"><inheritdoc cref="GorgonGpuResource(GorgonGraphics, string, ComPtr{ID3D12Resource2})" path="/param[@name='name']"/></param>
    /// <param name="info">Information used to create the buffer.</param>
    /// <exception cref="GorgonException">Thrown if the buffer has less than 1 command, a command size that is less than 16 bytes or not a multiple of 4, or a negative number of counters.</exception>
    /// <seealso cref="GorgonExecuteBufferInfo"/>
    public GorgonExecuteBuffer(GorgonGraphics graphics, string name, GorgonExecuteBufferInfo info)
        : base(graphics, name, info)
    {
        _info = info;

        ValidateInfo();

        Graphics.Log.Print($"Creating Gorgon execute buffer '{Name}'.", LoggingLevel.Simple);

        CreateNative();
    }
}
