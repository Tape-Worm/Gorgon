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
// Created: October 7, 2026 12:35:00 AM
//
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Defines parameters for executing a list of draw commands stored in a GPU buffer.
/// </summary>
/// <remarks>
/// <para>
/// This is used by the <see cref="GorgonCommandList.Execute(GorgonExecuteCall)"/> method, and is created with <see cref="CreateCpuRendering"/> or <see cref="CreateGpuRendering"/>.
/// </para>
/// <inheritdoc cref="GorgonExecuteCallCommon" path="/remarks/para[@type='execute']"/>
/// <para>
/// Every command in the <see cref="GorgonExecuteCallCommon.Commands"/> buffer is a record of 32-bit values, laid out in this order:
/// <list type="number">
/// <item><description>The <see cref="GorgonExecuteCallCommon.ConstantCount"/> root constant values, if any, in order starting at the <see cref="GorgonExecuteCallCommon.ConstantStart"/>.</description></item>
/// <item><description>The number of vertices to draw for each instance (unsigned).</description></item>
/// <item><description>The number of instances to draw (unsigned).</description></item>
/// <item><description>The location of the first vertex (unsigned).</description></item>
/// <item><description>The location of the first instance (unsigned).</description></item>
/// <item><description>Optional application data, up to the <see cref="GorgonExecuteCallCommon.CommandSizeInBytes"/>. The GPU ignores these bytes.</description></item>
/// </list>
/// </para>
/// <para>
/// This makes the minimum size of a command 4 bytes for each root constant, plus 16 bytes for the draw arguments.
/// </para>
/// <para>
/// <note type="important">
/// <para>
/// Every command must contain all four draw arguments after its root constants, even when the values are the same for every command. The GPU reads the arguments for each draw from the command itself, so a 
/// command without them draws with whatever values are at that location in the buffer.
/// </para>
/// </note>
/// </para>
/// </remarks>
/// <example>
/// <para>
/// The following example creates commands that each set 1 root constant (the index of an object to draw), followed by the draw arguments, and writes them on the CPU.
/// </para>
/// The C# code:
/// <code language="csharp">
/// <![CDATA[
/// // One command: 1 root constant, then the draw arguments (20 bytes).
/// [StructLayout(LayoutKind.Sequential, Pack = 4)]
/// public struct DrawCommand
/// {
///     public uint ObjectIndex;       // Root constant 0.
///     public uint VertexCount;       // The draw arguments must always follow the root constants.
///     public uint InstanceCount;
///     public uint StartVertex;
///     public uint StartInstance;
/// }
/// 
/// DrawCommand[] data = new DrawCommand[objectCount];
/// 
/// for (int i = 0; i < data.Length; ++i)
/// {
///     data[i] = new DrawCommand
///     {
///         ObjectIndex = (uint)i,
///         VertexCount = 6,
///         InstanceCount = 1
///     };
/// }
/// 
/// GorgonExecuteBuffer commands = new(graphics, "Commands", new GorgonExecuteBufferInfo(data.Length, Unsafe.SizeOf<DrawCommand>()));
/// GorgonExecuteCall executeCall = GorgonExecuteCall.CreateCpuRendering(pso, commands, constantStart: 0, constantCount: 1);
/// 
/// commandList.CopyRange<DrawCommand>(data, commands);
/// commandList.Execute(executeCall);
/// ]]>
/// </code>
/// The HLSL code that reads the root constant:
/// <code>
/// <![CDATA[
/// cbuffer ExecuteConstants : register(b0, space1)
/// {
///     uint4 _constants;
/// };
/// 
/// // _constants.x holds the ObjectIndex for the command being drawn.
/// ]]>
/// </code>
/// <para>
/// The following example writes the same commands from a shader on the GPU, for an execute call created with <see cref="CreateGpuRendering"/>. The shader decides which objects to draw, and counts the commands 
/// it writes in counter 0.
/// </para>
/// The HLSL code:
/// <code>
/// <![CDATA[
/// struct WriterData
/// {
///     int CommandsHandle;
///     int CountersHandle;
///     uint VertexCount;
/// };
/// 
/// ConstantBuffer<WriterData> _writer : register(b0);
/// 
/// // Called for each object that should be drawn.
/// void AppendCommand(uint objectIndex)
/// {
///     RWByteAddressBuffer commands = ResourceDescriptorHeap[_writer.CommandsHandle];
///     RWByteAddressBuffer counters = ResourceDescriptorHeap[_writer.CountersHandle];
/// 
///     // Counter 0 counts the commands, and gives this command its slot in the buffer.
///     uint slot;
///     counters.InterlockedAdd(0, 1, slot);
/// 
///     uint offset = slot * 20;                                            // The CommandSizeInBytes.
///     commands.Store(offset, objectIndex);                                // Root constant 0.
///     commands.Store4(offset + 4, uint4(_writer.VertexCount, 1, 0, 0));   // The draw arguments.
/// }
/// ]]>
/// </code>
/// The C# code:
/// <code language="csharp">
/// <![CDATA[
/// GorgonExecuteBuffer commands = new(graphics, "Commands", new GorgonExecuteBufferInfo(maxObjectCount, 20)
/// {
///     CounterCount = 1,
///     HasReadWriteAccess = true
/// });
/// GorgonExecuteCall executeCall = GorgonExecuteCall.CreateGpuRendering(pso, commands, constantStart: 0, constantCount: 1);
/// 
/// GorgonRawBufferRwView commandsView = commands.GetCommandsReadWriteView();
/// GorgonRawBufferRwView countersView = commands.GetCountersReadWriteView();
/// 
/// // The counters must start at 0 before the shader increments them.
/// commandList.ClearReadWriteView(countersView, Vector128<int>.Zero);
/// 
/// // Run the shader that writes the commands. Its draw call lists both views as read/write buffers, and the
/// // view handles are passed to the shader in its WriterData.
/// commandList.Draw(writerDrawCall);
/// 
/// // Executes as many commands as the shader counted, up to the CommandExecutionCount.
/// commandList.Execute(executeCall);
/// ]]>
/// </code>
/// </example>
/// <seealso cref="GorgonCommandList.Execute(GorgonExecuteCall)"/>
/// <seealso cref="GorgonDrawCall"/>
/// <seealso cref="GorgonExecuteBuffer"/>
public unsafe sealed class GorgonExecuteCall
        : GorgonExecuteCallCommon
{
    /// <summary>
    /// Function to calculate the minimum size, in bytes, of a single command for this call.
    /// </summary>
    /// <param name="constantCount">The number of root constants that each command sets.</param>
    /// <returns>The minimum size of a command, in bytes.</returns>
    private static int CalculateCommandSize(int constantCount) => (constantCount * sizeof(uint)) + sizeof(D3D12_DRAW_ARGUMENTS);

    /// <inheritdoc/>
    private protected override ComPtr<ID3D12CommandSignature> CreateNative()
    {
        D3D12_INDIRECT_ARGUMENT_DESC* args = stackalloc D3D12_INDIRECT_ARGUMENT_DESC[3];
        int argCount = GetConstantArguments(args);

        args[argCount++] = new D3D12_INDIRECT_ARGUMENT_DESC
        {
            Type = D3D12_INDIRECT_ARGUMENT_TYPE.D3D12_INDIRECT_ARGUMENT_TYPE_DRAW
        };

        D3D12_COMMAND_SIGNATURE_DESC sigDesc = new()
        {
            ByteStride = (uint)CommandSizeInBytes,
            NumArgumentDescs = (uint)argCount,
            pArgumentDescs = args
        };

        // D3D12 rejects a root signature when the commands do not change any root arguments, and requires one when they do.
        ID3D12RootSignature* rootSignature = ConstantCount > 0 ? Graphics.D3DRootSignature.Get() : null;
        ComPtr<ID3D12CommandSignature> result = default;

        Graphics.D3DDevice.Get()->CreateCommandSignature(&sigDesc, rootSignature, Win32.__uuidof<ID3D12CommandSignature>(), (void**)result.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCreate, () => Resources.GORGFX_ERR_CANNOT_CREATE_EXECUTION_CALL);

        return result;
    }

    /// <summary>
    /// Function to create an execute call whose commands are written by the application on the CPU.
    /// </summary>
    /// <param name="pso">The graphics pipeline state object to use for every command in the call.</param>
    /// <param name="commands">The buffer that holds the commands.</param>
    /// <param name="constantStart">[Optional] The index (not the byte offset) of the first root constant that each command sets.</param>
    /// <param name="constantCount">[Optional] The number of root constants that each command sets.</param>
    /// <returns>A new <see cref="GorgonExecuteCall"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><para>
    /// Thrown if the <paramref name="constantStart"/> or the <paramref name="constantCount"/> is less than 0.
    /// </para>
    /// <para>
    /// Thrown if the <paramref name="constantStart"/> plus the <paramref name="constantCount"/> is greater than <see cref="GorgonGraphics.MaxRootConstantCount"/>.
    /// </para>
    /// </exception>
    /// <exception cref="ArgumentException">Thrown if the commands in the <paramref name="commands"/> buffer are smaller than the minimum size of a command for the call.</exception>
    /// <remarks>
    /// <para type="cpu">
    /// The application writes the commands into the <paramref name="commands"/> buffer, and the <see cref="GorgonExecuteCallCommon.CommandExecutionCount"/> decides how many are executed.
    /// </para>
    /// <para type="command_size">
    /// The <see cref="GorgonExecuteBuffer.CommandSizeInBytes"/> of the <paramref name="commands"/> buffer must be at least the minimum size of a command for the call: 4 bytes for each root constant, plus 16 
    /// bytes for the draw arguments.
    /// </para>
    /// <para type="constants">
    /// See <see cref="GorgonExecuteCallCommon.ConstantStart"/> for how the root constants are declared in shaders.
    /// </para>
    /// <para>
    /// The default values are 0 for the <paramref name="constantStart"/>, and 0 for the <paramref name="constantCount"/>.
    /// </para>
    /// </remarks>
    public static GorgonExecuteCall CreateCpuRendering(GorgonGraphicsPso pso, GorgonExecuteBuffer commands, int constantStart = 0, int constantCount = 0)
    {
        Validate(ExecuteUsage.CpuRendering, constantStart, constantCount, commands, CalculateCommandSize(constantCount));

        return new GorgonExecuteCall(pso, ExecuteUsage.CpuRendering, constantStart, constantCount, commands);
    }

    /// <summary>
    /// Function to create an execute call whose commands, and the number of commands to execute, are written by shaders on the GPU.
    /// </summary>
    /// <param name="pso"><inheritdoc cref="CreateCpuRendering" path="/param[@name='pso']"/></param>
    /// <param name="commands">The buffer that holds the commands, and the counters for the number of commands to execute.</param>
    /// <param name="constantStart"><inheritdoc cref="CreateCpuRendering" path="/param[@name='constantStart']"/></param>
    /// <param name="constantCount"><inheritdoc cref="CreateCpuRendering" path="/param[@name='constantCount']"/></param>
    /// <inheritdoc cref="CreateCpuRendering" path="/returns"/>
    /// <exception cref="ArgumentOutOfRangeException"><para>
    /// Thrown if the <paramref name="constantStart"/> or the <paramref name="constantCount"/> is less than 0.
    /// </para>
    /// <para>
    /// Thrown if the <paramref name="constantStart"/> plus the <paramref name="constantCount"/> is greater than <see cref="GorgonGraphics.MaxRootConstantCount"/>.
    /// </para>
    /// </exception>
    /// <exception cref="ArgumentException"><para>
    /// Thrown if the commands in the <paramref name="commands"/> buffer are smaller than the minimum size of a command for the call.
    /// </para>
    /// <para>
    /// Thrown if the <paramref name="commands"/> buffer has no counters.
    /// </para>
    /// </exception>
    /// <remarks>
    /// <para type="gpu">
    /// A shader writes the commands, and counts the commands it writes, into the <paramref name="commands"/> buffer through the views returned by 
    /// <see cref="GorgonExecuteBuffer.GetCommandsReadWriteView(int, int?)"/> and <see cref="GorgonExecuteBuffer.GetCountersReadWriteView(int, int?)"/>. The buffer must be created with at least 1 counter, and 
    /// with the <see cref="GorgonCommonBufferInfo.HasReadWriteAccess"/> property set to <b>true</b>.
    /// </para>
    /// <para type="gpu">
    /// When the call is executed, the number of commands executed is the lesser of the <see cref="GorgonExecuteCallCommon.CommandExecutionCount"/> and the value of the counter selected by the 
    /// <see cref="GorgonExecuteCallCommon.CounterIndex"/>. The counters must be reset (e.g. with 
    /// <see cref="GorgonCommandList.ClearReadWriteView(GorgonRawBufferRwView, System.Runtime.Intrinsics.Vector128{int})"/>) before the shader increments them.
    /// </para>
    /// <inheritdoc cref="CreateCpuRendering" path="/remarks/para[@type='command_size']"/>
    /// <inheritdoc cref="CreateCpuRendering" path="/remarks/para[@type='constants']"/>
    /// <para>
    /// The default values are 0 for the <paramref name="constantStart"/>, and 0 for the <paramref name="constantCount"/>.
    /// </para>
    /// </remarks>
    public static GorgonExecuteCall CreateGpuRendering(GorgonGraphicsPso pso, GorgonExecuteBuffer commands, int constantStart = 0, int constantCount = 0)
    {
        Validate(ExecuteUsage.GpuRendering, constantStart, constantCount, commands, CalculateCommandSize(constantCount));

        return new GorgonExecuteCall(pso, ExecuteUsage.GpuRendering, constantStart, constantCount, commands);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonExecuteCall"/> class.
    /// </summary>
    /// <inheritdoc cref="GorgonExecuteCallCommon(GorgonGraphicsPso, ExecuteUsage, int, int, GorgonExecuteBuffer)" path="/param"/>
    private GorgonExecuteCall(GorgonGraphicsPso pso, ExecuteUsage usage, int constantStart, int constantCount, GorgonExecuteBuffer commands)
        : base(pso, usage, constantStart, constantCount, commands)
    {
    }
}
