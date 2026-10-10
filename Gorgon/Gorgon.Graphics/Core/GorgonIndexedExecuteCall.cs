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
/// Defines parameters for executing a list of indexed draw commands stored in a GPU buffer.
/// </summary>
/// <remarks>
/// <para>
/// This is used by the <see cref="GorgonCommandList.Execute(GorgonIndexedExecuteCall)"/> method, and is created with <see cref="CreateCpuRendering"/> or <see cref="CreateGpuRendering"/>.
/// </para>
/// <inheritdoc cref="GorgonExecuteCallCommon" path="/remarks/para[@type='execute']"/>
/// <para>
/// Every command in the <see cref="GorgonExecuteCallCommon.Commands"/> buffer is a record of 32-bit values, laid out in this order:
/// <list type="number">
/// <item><description>The <see cref="GorgonExecuteCallCommon.ConstantCount"/> root constant values, if any, in order starting at the <see cref="GorgonExecuteCallCommon.ConstantStart"/>.</description></item>
/// <item><description>The number of indices to draw for each instance (unsigned).</description></item>
/// <item><description>The number of instances to draw (unsigned).</description></item>
/// <item><description>The location of the first index (unsigned).</description></item>
/// <item><description>The base vertex (signed, see <see cref="GorgonIndexedDrawCall.BaseVertex"/>).</description></item>
/// <item><description>The location of the first instance (unsigned).</description></item>
/// <item><description>Optional application data, up to the <see cref="GorgonExecuteCallCommon.CommandSizeInBytes"/>. The GPU ignores these bytes.</description></item>
/// </list>
/// </para>
/// <para>
/// This makes the minimum size of a command 4 bytes for each root constant, plus 20 bytes for the indexed draw arguments. Every command reads its indices from the <see cref="IndexBuffer"/>.
/// </para>
/// <para>
/// <note type="important">
/// <para>
/// Every command must contain all five indexed draw arguments after its root constants, even when the values are the same for every command. The GPU reads the arguments for each draw from the command itself, 
/// so a command without them draws with whatever values are at that location in the buffer.
/// </para>
/// </note>
/// </para>
/// </remarks>
/// <example>
/// <para>
/// The following command sets 1 root constant (the index of an object to draw), followed by the indexed draw arguments. Commands are written and executed the same way as the examples for 
/// <see cref="GorgonExecuteCall"/>, with the indexed draw arguments in place of the draw arguments.
/// </para>
/// The C# code:
/// <code language="csharp">
/// <![CDATA[
/// // One command: 1 root constant, then the indexed draw arguments (24 bytes).
/// [StructLayout(LayoutKind.Sequential, Pack = 4)]
/// public struct IndexedDrawCommand
/// {
///     public uint ObjectIndex;       // Root constant 0.
///     public uint IndexCount;        // The indexed draw arguments must always follow the root constants.
///     public uint InstanceCount;
///     public uint StartIndex;
///     public int BaseVertex;
///     public uint StartInstance;
/// }
/// ]]>
/// </code>
/// </example>
/// <seealso cref="GorgonCommandList.Execute(GorgonIndexedExecuteCall)"/>
/// <seealso cref="GorgonIndexedDrawCall"/>
/// <seealso cref="GorgonExecuteBuffer"/>
public unsafe sealed class GorgonIndexedExecuteCall
        : GorgonExecuteCallCommon
{
    /// <summary>
    /// Property to set or return the index buffer used by every command in the call.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Like the <see cref="GorgonGraphicsCallCommon.Pso"/>, this can be changed between executions to run a range of the commands with a different index buffer.
    /// </para>
    /// </remarks>
    public GorgonIndexBuffer IndexBuffer
    {
        get;
        set;
    }

    /// <summary>
    /// Function to calculate the minimum size, in bytes, of a single command for this call.
    /// </summary>
    /// <param name="constantCount">The number of root constants that each command sets.</param>
    /// <returns>The minimum size of a command, in bytes.</returns>
    private static int CalculateCommandSize(int constantCount) => (constantCount * sizeof(uint)) + sizeof(D3D12_DRAW_INDEXED_ARGUMENTS);

    /// <inheritdoc/>
    private protected override ComPtr<ID3D12CommandSignature> CreateNative()
    {
        D3D12_INDIRECT_ARGUMENT_DESC* args = stackalloc D3D12_INDIRECT_ARGUMENT_DESC[3];
        int argCount = GetConstantArguments(args);

        args[argCount++] = new D3D12_INDIRECT_ARGUMENT_DESC
        {
            Type = D3D12_INDIRECT_ARGUMENT_TYPE.D3D12_INDIRECT_ARGUMENT_TYPE_DRAW_INDEXED
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
    /// Function to create an indexed execute call whose commands are written by the application on the CPU.
    /// </summary>
    /// <param name="pso"><inheritdoc cref="GorgonExecuteCall.CreateCpuRendering" path="/param[@name='pso']"/></param>
    /// <param name="commands"><inheritdoc cref="GorgonExecuteCall.CreateCpuRendering" path="/param[@name='commands']"/></param>
    /// <param name="indexBuffer">The index buffer used by every command in the call.</param>
    /// <param name="constantStart"><inheritdoc cref="GorgonExecuteCall.CreateCpuRendering" path="/param[@name='constantStart']"/></param>
    /// <param name="constantCount"><inheritdoc cref="GorgonExecuteCall.CreateCpuRendering" path="/param[@name='constantCount']"/></param>
    /// <returns>A new <see cref="GorgonIndexedExecuteCall"/>.</returns>
    /// <inheritdoc cref="GorgonExecuteCall.CreateCpuRendering" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="GorgonExecuteCall.CreateCpuRendering" path="/remarks/para[@type='cpu']"/>
    /// <para type="command_size">
    /// The <see cref="GorgonExecuteBuffer.CommandSizeInBytes"/> of the <paramref name="commands"/> buffer must be at least the minimum size of a command for the call: 4 bytes for each root constant, plus 20 
    /// bytes for the indexed draw arguments.
    /// </para>
    /// <inheritdoc cref="GorgonExecuteCall.CreateCpuRendering" path="/remarks/para[@type='constants']"/>
    /// <para>
    /// The default values are 0 for the <paramref name="constantStart"/>, and 0 for the <paramref name="constantCount"/>.
    /// </para>
    /// </remarks>
    public static GorgonIndexedExecuteCall CreateCpuRendering(GorgonGraphicsPso pso, GorgonExecuteBuffer commands, GorgonIndexBuffer indexBuffer, int constantStart = 0, int constantCount = 0)
    {
        Validate(ExecuteUsage.CpuRendering, constantStart, constantCount, commands, CalculateCommandSize(constantCount));

        return new GorgonIndexedExecuteCall(pso, ExecuteUsage.CpuRendering, constantStart, constantCount, commands, indexBuffer);
    }

    /// <summary>
    /// Function to create an indexed execute call whose commands, and the number of commands to execute, are written by shaders on the GPU.
    /// </summary>
    /// <param name="pso"><inheritdoc cref="GorgonExecuteCall.CreateGpuRendering" path="/param[@name='pso']"/></param>
    /// <param name="commands"><inheritdoc cref="GorgonExecuteCall.CreateGpuRendering" path="/param[@name='commands']"/></param>
    /// <param name="indexBuffer">The index buffer used by every command in the call.</param>
    /// <param name="constantStart"><inheritdoc cref="GorgonExecuteCall.CreateGpuRendering" path="/param[@name='constantStart']"/></param>
    /// <param name="constantCount"><inheritdoc cref="GorgonExecuteCall.CreateGpuRendering" path="/param[@name='constantCount']"/></param>
    /// <returns>A new <see cref="GorgonIndexedExecuteCall"/>.</returns>
    /// <inheritdoc cref="GorgonExecuteCall.CreateGpuRendering" path="/exception"/>
    /// <remarks>
    /// <inheritdoc cref="GorgonExecuteCall.CreateGpuRendering" path="/remarks/para[@type='gpu']"/>
    /// <inheritdoc cref="CreateCpuRendering" path="/remarks/para[@type='command_size']"/>
    /// <inheritdoc cref="GorgonExecuteCall.CreateCpuRendering" path="/remarks/para[@type='constants']"/>
    /// <para>
    /// The default values are 0 for the <paramref name="constantStart"/>, and 0 for the <paramref name="constantCount"/>.
    /// </para>
    /// </remarks>
    public static GorgonIndexedExecuteCall CreateGpuRendering(GorgonGraphicsPso pso, GorgonExecuteBuffer commands, GorgonIndexBuffer indexBuffer, int constantStart = 0, int constantCount = 0)
    {
        Validate(ExecuteUsage.GpuRendering, constantStart, constantCount, commands, CalculateCommandSize(constantCount));

        return new GorgonIndexedExecuteCall(pso, ExecuteUsage.GpuRendering, constantStart, constantCount, commands, indexBuffer);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonIndexedExecuteCall"/> class.
    /// </summary>
    /// <param name="pso"><inheritdoc cref="GorgonExecuteCallCommon(GorgonGraphicsPso, ExecuteUsage, int, int, GorgonExecuteBuffer)" path="/param[@name='pso']"/></param>
    /// <param name="usage"><inheritdoc cref="GorgonExecuteCallCommon(GorgonGraphicsPso, ExecuteUsage, int, int, GorgonExecuteBuffer)" path="/param[@name='usage']"/></param>
    /// <param name="constantStart"><inheritdoc cref="GorgonExecuteCallCommon(GorgonGraphicsPso, ExecuteUsage, int, int, GorgonExecuteBuffer)" path="/param[@name='constantStart']"/></param>
    /// <param name="constantCount"><inheritdoc cref="GorgonExecuteCallCommon(GorgonGraphicsPso, ExecuteUsage, int, int, GorgonExecuteBuffer)" path="/param[@name='constantCount']"/></param>
    /// <param name="commands"><inheritdoc cref="GorgonExecuteCallCommon(GorgonGraphicsPso, ExecuteUsage, int, int, GorgonExecuteBuffer)" path="/param[@name='commands']"/></param>
    /// <param name="indexBuffer">The index buffer used by every command in the call.</param>
    private GorgonIndexedExecuteCall(GorgonGraphicsPso pso, ExecuteUsage usage, int constantStart, int constantCount, GorgonExecuteBuffer commands, GorgonIndexBuffer indexBuffer)
        : base(pso, usage, constantStart, constantCount, commands) => IndexBuffer = indexBuffer;
}
