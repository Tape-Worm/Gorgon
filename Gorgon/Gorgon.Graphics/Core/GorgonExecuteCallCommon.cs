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
// Created: October 8, 2026 3:28:56 PM
//
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Math;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Common functionality for execute calls, which run a list of commands stored in a GPU buffer with a single call.
/// </summary>
/// <remarks>
/// <para type="execute">
/// An execute call reads its commands from a <see cref="GorgonExecuteBuffer"/>. Each command is a record that can set some of the root constants, followed by the arguments for the operation it performs (e.g. 
/// a draw). Executing the call is like recording a separate draw for every command, except that the arguments for each draw come from the buffer instead of from the application.
/// </para>
/// <para type="execute">
/// The commands can be written by the application on the CPU, or by a shader on the GPU. With GPU rendering, the shader also writes the number of commands to execute, so the GPU decides what is drawn without 
/// the CPU reading any results back. The <see cref="Usage"/> property returns which kind of call this is. See <see cref="ExecuteUsage"/> for more information.
/// </para>
/// <para type="execute">
/// The pipeline state object, the resources used by the shaders, and the dynamic pipeline values on this object apply to every command in the call. To run a range of the commands with a different pipeline 
/// state object or different resources, change those values on this object, set the <see cref="CommandExecutionStartIndex"/> and <see cref="CommandExecutionCount"/> to the range, and execute the call again.
/// </para>
/// <para type="execute">
/// The root constants are the only values that can change from one command to the next. To give each command more data, store a value in a root constant that the shader uses to find that data, such as an 
/// index into a buffer, or the handle of a constant buffer view (see <see cref="GorgonConstantBufferView.GetViewHandle"/>). Data shared by every command can be written with 
/// <see cref="GorgonCommandList.WriteConstant{T}(int, in T)"/> before the call is executed.
/// </para>
/// </remarks>
/// <seealso cref="GorgonExecuteCall"/>
/// <seealso cref="GorgonIndexedExecuteCall"/>
/// <seealso cref="GorgonExecuteBuffer"/>
/// <seealso cref="ExecuteUsage"/>
public abstract unsafe class GorgonExecuteCallCommon
        : GorgonGraphicsCallCommon, IDisposable
{
    private ComPtr<ID3D12CommandSignature> _d3dSignature;

    /// <summary>
    /// Function to create the command signature for this call.
    /// </summary>
    /// <returns>The command signature.</returns>
    private protected abstract ComPtr<ID3D12CommandSignature> CreateNative();

    /// <summary>
    /// Property to return the underlying command signature for the call.
    /// </summary>
    internal ref readonly ComPtr<ID3D12CommandSignature> D3DCommandSignature => ref _d3dSignature;

    /// <summary>
    /// Property to return the graphics instance associated with this execute call.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    }

    /// <summary>
    /// Property to return how the commands for this call are written.
    /// </summary>
    /// <seealso cref="ExecuteUsage"/>
    public ExecuteUsage Usage
    {
        get;
    }

    /// <summary>
    /// Property to set or return the buffer that holds the commands, and the counters, for this call.
    /// </summary>
    /// <exception cref="ArgumentException"><para>
    /// Thrown if the <see cref="GorgonExecuteBuffer.CommandSizeInBytes"/> of the buffer is not the same as the <see cref="CommandSizeInBytes"/> for this call.
    /// </para>
    /// <para>
    /// Thrown if the <see cref="Usage"/> is <see cref="ExecuteUsage.GpuRendering"/>, and the buffer has no counters.
    /// </para>
    /// </exception>
    /// <remarks>
    /// <para>
    /// Each command starts with the <see cref="ConstantCount"/> root constant values, followed by the draw arguments, and the commands are <see cref="CommandSizeInBytes"/> bytes apart.
    /// </para>
    /// <para>
    /// When the <see cref="Usage"/> is <see cref="ExecuteUsage.CpuRendering"/>, the application writes the commands into the buffer before the call is executed.
    /// </para>
    /// <para>
    /// When the <see cref="Usage"/> is <see cref="ExecuteUsage.GpuRendering"/>, shaders write the commands and the counters through the views returned by 
    /// <see cref="GorgonExecuteBuffer.GetCommandsReadWriteView(int, int?)"/> and <see cref="GorgonExecuteBuffer.GetCountersReadWriteView(int, int?)"/>. The counters must be reset (e.g. with 
    /// <see cref="GorgonCommandList.ClearReadWriteView(GorgonRawBufferRwView, System.Runtime.Intrinsics.Vector128{int})"/>) before shaders increment them.
    /// </para>
    /// <para>
    /// A different buffer can be assigned between executions, as long as its commands are the same size.
    /// </para>
    /// <para type="no_assign">
    /// <note type="warning">
    /// <para>
    /// Do not pass this buffer to <see cref="GorgonGraphicsCallCommon.AssignBuffers(ReadOnlySpan{GorgonUsedBuffer})"/> on this call. The execute call manages the buffer's state itself.
    /// </para>
    /// </note>
    /// </para>
    /// </remarks>
    public GorgonExecuteBuffer Commands
    {
        get;
        set
        {
            if (value.CommandSizeInBytes != CommandSizeInBytes)
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_EXECUTE_COMMAND_SIZE_MISMATCH, value.Name, value.CommandSizeInBytes, CommandSizeInBytes), nameof(value));
            }

            if ((Usage == ExecuteUsage.GpuRendering) && (value.CounterCount < 1))
            {
                throw new ArgumentException(string.Format(Resources.GORGFX_ERR_EXECUTE_GPU_NEEDS_COUNTERS, value.Name), nameof(value));
            }

            field = value;
        }
    }

    /// <summary>
    /// Property to return the number of counters in the <see cref="Commands"/> buffer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is only used when the <see cref="Usage"/> is <see cref="ExecuteUsage.GpuRendering"/>, and is 0 otherwise. Each counter holds the number of commands to execute for one execution of the call. 
    /// Multiple counters let shaders count the commands for different ranges of the buffer separately (for example, one range for each pipeline state object), and the counter used by an execution is selected 
    /// with the <see cref="CounterIndex"/>.
    /// </para>
    /// </remarks>
    public int CounterCount => Usage == ExecuteUsage.GpuRendering ? Commands.CounterCount : 0;

    /// <summary>
    /// Property to return the index of the first root constant that each command sets.
    /// </summary>
    /// <remarks>
    /// <para type="constants">
    /// The root constants are numbered from 0 to 7. Constants 0 to 3 are the four 32-bit values of <c>register(b0, space1)</c>, and constants 4 to 7 are the four 32-bit values of <c>register(b1, space1)</c>. 
    /// Each command sets the <see cref="ConstantCount"/> constants starting at the <see cref="ConstantStart"/>, and the range can cross from one register to the next.
    /// </para>
    /// <para type="constants">
    /// In a shader, each register is a constant buffer of 4 unsigned integers, for example: <c>cbuffer ExecuteConstants : register(b0, space1) { uint4 values; };</c>.
    /// </para>
    /// <para type="constants">
    /// The values for the constants are stored at the start of each command, in order, as 32-bit values. Once the call is executed, the root constants set by the commands are reset to 0.
    /// </para>
    /// <para>
    /// This value is the index of a root constant, not an offset in bytes.
    /// </para>
    /// </remarks>
    public int ConstantStart
    {
        get;
    }

    /// <summary>
    /// Property to return the number of root constants that each command sets.
    /// </summary>
    /// <remarks>
    /// <inheritdoc cref="ConstantStart" path="/remarks/para[@type='constants']"/>
    /// <para>
    /// If this value is 0, the commands do not set any root constants.
    /// </para>
    /// </remarks>
    public int ConstantCount
    {
        get;
    }

    /// <summary>
    /// Property to set or return the index of the first command to execute.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This value is the index of a command in the <see cref="Commands"/> buffer, not an offset in bytes. The commands from this index, up to the <see cref="CommandExecutionCount"/>, must be in the buffer.
    /// </para>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    public int CommandExecutionStartIndex
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the maximum number of commands to execute.
    /// </summary>
    /// <remarks>
    /// <para>
    /// When the <see cref="Usage"/> is <see cref="ExecuteUsage.CpuRendering"/>, this is the number of commands executed. When the <see cref="Usage"/> is <see cref="ExecuteUsage.GpuRendering"/>, this is the 
    /// maximum number of commands executed, and the counter selected by the <see cref="CounterIndex"/> decides how many are executed.
    /// </para>
    /// <para>
    /// The default value is the <see cref="CommandCount"/> of the <see cref="Commands"/> buffer the call was created with.
    /// </para>
    /// </remarks>
    public int CommandExecutionCount
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the total number of commands the <see cref="Commands"/> buffer can hold.
    /// </summary>
    public int CommandCount => Commands.CommandCount;

    /// <summary>
    /// Property to return the distance, in bytes, between the start of one command and the start of the next in the execution call.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is the <see cref="GorgonExecuteBuffer.CommandSizeInBytes"/> of the <see cref="Commands"/> buffer the call was created with. Any buffer assigned to the <see cref="Commands"/> property must use the 
    /// same size.
    /// </para>
    /// </remarks>
    public int CommandSizeInBytes
    {
        get;
    }

    /// <summary>
    /// Property to set or return the index of the counter that holds the number of commands to execute.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This is only used when the <see cref="Usage"/> is <see cref="ExecuteUsage.GpuRendering"/>. This value is the index of a counter in the <see cref="Commands"/> buffer, not an offset in bytes. The number 
    /// of commands executed is the lesser of the value in this counter and the <see cref="CommandExecutionCount"/>, which lets a shader on the GPU decide how many commands are executed.
    /// </para>
    /// <para>
    /// The default value is 0.
    /// </para>
    /// </remarks>
    public int CounterIndex
    {
        get;
        set;
    }

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool _) => _d3dSignature.Dispose();

    /// <summary>
    /// Function to create a description for an argument that sets root constants from a command.
    /// </summary>
    /// <param name="group">The group of root constants to set: 0 for <c>register(b0, space1)</c>, or 1 for <c>register(b1, space1)</c>.</param>
    /// <param name="offset">The index of the first value to set within the group.</param>
    /// <param name="count">The number of values to set.</param>
    /// <returns>The description for the argument.</returns>
    private static D3D12_INDIRECT_ARGUMENT_DESC CreateConstantArgument(int group, int offset, int count)
    {
        D3D12_INDIRECT_ARGUMENT_DESC result = new()
        {
            Type = D3D12_INDIRECT_ARGUMENT_TYPE.D3D12_INDIRECT_ARGUMENT_TYPE_CONSTANT
        };

        result.Constant.RootParameterIndex = (uint)(GorgonGraphics.MaxRootCbvCount + group);
        result.Constant.DestOffsetIn32BitValues = (uint)offset;
        result.Constant.Num32BitValuesToSet = (uint)count;

        return result;
    }

    /// <summary>
    /// Function to write the descriptions for the arguments that set the root constants from a command.
    /// </summary>
    /// <param name="arguments">The arguments to write into. This must have room for at least 2 arguments.</param>
    /// <returns>The number of arguments written: 0 if no constants are set, 1 if the constants are in one register, or 2 if they cross into the second register.</returns>
    private protected int GetConstantArguments(D3D12_INDIRECT_ARGUMENT_DESC* arguments)
    {
        if (ConstantCount < 1)
        {
            return 0;
        }

        const int groupSize = GorgonGraphics.MaxRootConstantCount / 2;
        int group = ConstantStart / groupSize;
        int offset = ConstantStart % groupSize;
        int firstCount = ConstantCount.Min(groupSize - offset);

        arguments[0] = CreateConstantArgument(group, offset, firstCount);

        if (firstCount == ConstantCount)
        {
            return 1;
        }

        arguments[1] = CreateConstantArgument(group + 1, 0, ConstantCount - firstCount);

        return 2;
    }

    /// <summary>
    /// Function to validate the values used to create an execute call.
    /// </summary>
    /// <param name="usage">How the commands for the call are written.</param>
    /// <param name="constantStart">The index of the first root constant that each command sets.</param>
    /// <param name="constantCount">The number of root constants that each command sets.</param>
    /// <param name="commands">The buffer that holds the commands.</param>
    /// <param name="minimumCommandSize">The minimum size, in bytes, of a command for the call.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown if either constant value is less than 0, or the range goes past the last root constant.</exception>
    /// <exception cref="ArgumentException">Thrown if the commands in the <paramref name="commands"/> buffer are smaller than the <paramref name="minimumCommandSize"/>, or the buffer has no counters for GPU rendering.</exception>
    private protected static void Validate(ExecuteUsage usage, int constantStart, int constantCount, GorgonExecuteBuffer commands, int minimumCommandSize)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(constantStart);
        ArgumentOutOfRangeException.ThrowIfNegative(constantCount);

        if ((constantStart + constantCount) > GorgonGraphics.MaxRootConstantCount)
        {
            throw new ArgumentOutOfRangeException(nameof(constantCount), string.Format(Resources.GORGFX_ERR_EXECUTE_CONSTANT_RANGE_INVALID, constantStart, constantCount, GorgonGraphics.MaxRootConstantCount));
        }

        if (commands.CommandSizeInBytes < minimumCommandSize)
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_EXECUTE_COMMAND_SIZE_TOO_SMALL, commands.Name, commands.CommandSizeInBytes, minimumCommandSize), nameof(commands));
        }

        if ((usage == ExecuteUsage.GpuRendering) && (commands.CounterCount < 1))
        {
            throw new ArgumentException(string.Format(Resources.GORGFX_ERR_EXECUTE_GPU_NEEDS_COUNTERS, commands.Name), nameof(commands));
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Finalizer.
    /// </summary>
    ~GorgonExecuteCallCommon() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonExecuteCallCommon"/> class.
    /// </summary>
    /// <param name="pso">The graphics pipeline state object to use for every command in the call.</param>
    /// <param name="usage">How the commands for the call are written.</param>
    /// <param name="constantStart">The index of the first root constant that each command sets.</param>
    /// <param name="constantCount">The number of root constants that each command sets.</param>
    /// <param name="commands">The buffer that holds the commands.</param>
    private protected GorgonExecuteCallCommon(GorgonGraphicsPso pso, ExecuteUsage usage, int constantStart, int constantCount, GorgonExecuteBuffer commands)
        : base(pso)
    {
        Graphics = pso.Graphics;
        Usage = usage;
        ConstantStart = constantStart;
        ConstantCount = constantCount;
        CommandSizeInBytes = commands.CommandSizeInBytes;
        Commands = commands;
        CommandExecutionCount = CommandCount;

        _d3dSignature = CreateNative();
    }
}
