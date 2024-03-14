
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 1, 2017 11:00:30 PM
// 


using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core;

/// <summary>
/// An engine used to perform computation on the GPU
/// </summary>
/// <remarks>
/// <para>
/// This system uses multiple threads/waves to perform work in parallel on the GPU, which gives exceptional performance when executing expensive computations
/// </para>
/// <para>
/// The compute engine sends compuational work to the GPU via a <see cref="GorgonComputeShader"/>. This interface is different from the <see cref="GorgonGraphics"/> interface in that it does not rely 
/// on the standard GPU pipeline to execute, and is stateful (i.e. applications set a state, run the engine, set another state, run again, etc...)
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphics"/>
/// <remarks>
/// Initializes a new instance of the <see cref="GorgonComputeEngine"/> class
/// </remarks>
/// <param name="graphics">The graphics interface that allows access to the GPU.</param>
/// <exception cref="ArgumentNullException">Thrown when the <paramref name="graphics"/> parameter is <b>null</b>.</exception>
/// <exception cref="ArgumentException">Thrown if the device is not a feature level 11 or better device.</exception>
/// <seealso cref="GorgonGraphics"/>
public sealed class GorgonComputeEngine(GorgonGraphics graphics)
        : IGorgonGraphicsObject
{

    /// <summary>
    /// The maximum number of thread groups that can be sent when executing the shader.
    /// </summary>
    public const int MaxThreadGroupCount = D3D11.ComputeShaderStage.DispatchMaximumThreadGroupsPerDimension + 1;



    /// <summary>
    /// Property to return the graphics interface that owns this object.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    } = graphics ?? throw new ArgumentNullException(nameof(graphics));



    /// <summary>
    /// Function to execute a <see cref="GorgonComputeShader"/>.
    /// </summary>
    /// <param name="dispatchCall">The <see cref="GorgonDispatchCall"/> to execute.</param>
    /// <param name="threadGroupCountX">The number of thread groups to dispatch in the X direction.</param>
    /// <param name="threadGroupCountY">The number of thread groups to dispatch in the Y direction.</param>
    /// <param name="threadGroupCountZ">The number of thread groups to dispatch in the Z direction.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="dispatchCall"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="threadGroupCountX"/>, <paramref name="threadGroupCountY"/>, or the <paramref name="threadGroupCountZ"/> parameter 
    /// is less than 0, or not less than <see cref="MaxThreadGroupCount"/>.</exception>
    /// <remarks>
    /// <para>
    /// This will take a <see cref="GorgonDispatchCall"/> and execute it. This method will also bind any buffers set up to the GPU prior to executing the shader.
    /// </para>
    /// <para>
    /// The <see cref="GorgonDispatchCall.ComputeShader"/> will be run in parallel on many threads within a thread group. To understand how thread indexes map to the number of threads defined in the shader, please 
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
    public void Execute(GorgonDispatchCall dispatchCall, int threadGroupCountX, int threadGroupCountY, int threadGroupCountZ) => Graphics.Dispatch(dispatchCall, threadGroupCountX, threadGroupCountY, threadGroupCountZ);

    /// <summary>
    /// Function to execute a <see cref="GorgonComputeShader"/> using a buffer for argument passing.
    /// </summary>
    /// <param name="dispatchCall">The <see cref="GorgonDispatchCall"/> to execute.</param>
    /// <param name="indirectArgs">The buffer containing the arguments for the compute shader.</param>
    /// <param name="threadGroupOffset">[Optional] The offset within the buffer, in bytes, to where the arguments are stored.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="dispatchCall"/>, or the <paramref name="indirectArgs"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the <paramref name="threadGroupOffset"/> is less than 0.</exception>
    /// <remarks>
    /// <para>
    /// This will take the <see cref="GorgonDispatchCall"/> and execute it using the <paramref name="indirectArgs"/> buffer. This method will also bind any buffers set up to the GPU prior to executing 
    /// the shader.
    /// </para>
    /// <para>
    /// The <paramref name="indirectArgs"/> buffer must contain the thread group count arguments for a <see cref="GorgonDispatchCall.ComputeShader"/>. The <paramref name="threadGroupOffset"/>, will instruct the GPU 
    /// to begin reading these arguments at the specified offset.
    /// </para>
    /// <para>
    /// This method differs from the <see cref="Execute(GorgonDispatchCall,int,int,int)"/> overload in that it uses a buffer to retrieve the arguments to send to the next compute 
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
    public void Execute(GorgonDispatchCall dispatchCall, GorgonBufferCommon indirectArgs, int threadGroupOffset = 0) => Graphics.Dispatch(dispatchCall, indirectArgs, threadGroupOffset);


}
