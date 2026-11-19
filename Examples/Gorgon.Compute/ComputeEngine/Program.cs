
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: August 1, 2017 9:35:50 PM
// 

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using Gorgon.Diagnostics;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;

namespace Gorgon.Examples;

/// <summary>
/// An example showing how to use the Compute engine functionality for Gorgon
/// </summary>
/// <remarks>
/// This is a very simple example showing how to access the compute engine functionality in Gorgon. 
/// 
/// The compute engine allows an application, through the use of Compute Shaders, to perform arbitrary computation on the GPU. The GPU is an incredibly 
/// fast number crunching device, and this functionality allows us to offload otherwise expensive computations from the CPU to the GPU. 
/// 
/// The compute shaders on the GPU perform their tasks uses multiple groups of multiple threads to allow many operations to be performed in parallel, 
/// this is a major component in delivering high performance throughput
/// 
/// For this example, we'll upload two sets of values to the GPU and the compute engine will perform a simple mathematical operation on each sets of 
/// values to produce another set of values stored as a structured buffer
/// 
/// To do this, we first create a graphics interface so we can access the GPU from the compute engine, then we create the compute engine itself and 
/// the buffers containing the input values and the output buffer. Then, finally, we execute the compute shader and retrieve the result values from 
/// the output buffer into a buffer that the CPU can read and test them for accuracy.
/// </remarks>
internal class Program
{
    // The maximum number of elements to send and receive.
    private const int MaxValues = 16384;

    /// <summary>
    /// The output data returned from the shader
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    private struct OutputData
    {
        /// <summary>
        /// The sum of integer data.
        /// </summary>
        public int Sum;
        /// <summary>
        /// The product of floating point data.
        /// </summary>
        public float Product;
    }

    /// <summary>
    /// The data to send to the compute engine.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 16)]
    private struct InputData
    {
        /// <summary>
        /// The handle for the integer point input buffer view.
        /// </summary>
        public int IntBufferHandle;
        /// <summary>
        /// The handle for the floating point input buffer view.
        /// </summary>
        public int FloatBufferHandle;
        /// <summary>
        /// The handle for the output buffer structured UAV.
        /// </summary>
        public int OutputBufferHandle;
    }

    /// <summary>
    /// A function to create our input/input buffers and populate the input buffers with the data to send to the GPU.
    /// </summary>
    /// <param name="graphics">The graphics interface for the application.</param>
    /// <returns>The buffers, pre-populated.</returns>
    private static (GorgonStructuredBufferRwView Output, GorgonGpuBuffer IntBuffer, GorgonGpuBuffer FloatBuffer) BuildInputOutputBuffers(GorgonGraphics graphics)
    {
        // This structured buffer will contain the output from the compute operation. It is only accessible to the GPU, so we'll 
        // have to copy this data to a CPU accessible buffer once we're done execution.
        // The GPU output data buffer will be sent to the compute engine as an unordered access view (UAV) so that we can write 
        // the data back into the buffer.
        GorgonStructuredBufferRwView outputBuffer = GorgonStructuredBufferRwView.CreateStructuredBuffer<OutputData>(graphics, "Output Buffer", MaxValues);
        
        // This is our input data buffers. One for integer values, and another for floating point.
        // These are just standard buffers that will be sent as a shader resource view (SRV).
        GorgonGpuBuffer intBuffer = new(graphics, "Integer Buffer", new GorgonGpuBufferInfo(sizeof(int) * MaxValues)
        {
            Alignment = sizeof(int)
        });
        GorgonGpuBuffer floatBuffer = new(graphics, "Float Buffer", new GorgonGpuBufferInfo(sizeof(float) * MaxValues)
        {
            Alignment = sizeof(float)
        });

        // Now, stage some data and upload into the buffers.
        int[] inputInts = new int[MaxValues];
        float[] inputFloats = new float[MaxValues];

        // The operation in the shader is quite simple:
        // It will add each element value to itself for the integer values, and will multiply each element value with itself.
        for (int i = 0; i < inputFloats.Length; ++i)
        {
            inputFloats[i] = i;
            inputInts[i] = i;
        }

        // Send to the buffers.
        GorgonCommandList list = graphics.GetCommandList("Upload");

        list.CopyRange(inputInts, intBuffer)
            .CopyRange(inputFloats, floatBuffer);

        graphics.Submit(list);
        graphics.WaitForGpu();

        return (outputBuffer, intBuffer, floatBuffer);
    }

    /// <summary>
    /// Function to initialize the dispatch operation.
    /// </summary>
    /// <param name="psoFactory">The factory used to create a PSO for the compute shader.</param>
    /// <param name="threadGroupCount">The number of thread groups to execute.</param>
    /// <returns>The dispatch call to execute.</returns>
    private static GorgonDispatchCall BuildDispatch(GorgonComputePsoFactory psoFactory, int threadGroupCount)
    {
        // We will also need to compile a compute shader so we can actually perform the work.
        Console.WriteLine("Compiling the compute shader (SimpleCompute)...");

        using GorgonShaderCompiler compiler = new(psoFactory.Graphics);
        GorgonShader computeShader = compiler.Compile(Resources.ComputeShader, "SimpleCompute", ShaderType.ComputeShader, flags: (psoFactory.Graphics.IsInDebugMode ? CompileFlags.Debug : CompileFlags.OptimizationLevel3));

        GorgonComputePso pso = psoFactory.CreateOrGetPso("ComputePSO", computeShader);

        // We'll send 10 thread groups of 1,000 threads (MaxValues = 10,000) to the GPU.
        return new(pso, threadGroupCount, 1, 1);
    }

    /// <summary>
    /// Function to execute the compute shader with the compute engine.
    /// </summary>
    /// <param name="engine">The compute engine.</param>
    /// <param name="dispatch">The dispatch call for the compute engine to execute.</param>
    /// <param name="data">The data to pass to the compute engine.</param>
    private static void Execute(GorgonComputeEngine engine, GorgonDispatchCall dispatch, InputData data)
    {
        // Set up the command list for the compute operation.
        GorgonComputeCommandList list = engine.GetCommandList("Main");

        list.WriteConstant(0, data)
            .Dispatch(dispatch);

        // Send to the GPU for execution.
        engine.Submit(list);

        // Wait until the GPU is done.
        engine.WaitForGpu();
    }

    /// <summary>
    /// Function to perform tests on the output data from the engine.
    /// </summary>
    /// <param name="graphics">The graphics interface for the application.</param>
    /// <param name="outputData">The output data from the compute shader.</param>
    /// <returns>A list of errors if any were found, or an empty list if the operation was completely successful.</returns>
    private static IReadOnlyList<(int Index, OutputData Expected, OutputData Actual)> TestResults(GorgonStructuredBufferRwView outputData)
    {
        OutputData[] results = new OutputData[MaxValues];

        // Download the contents of the output buffer into CPU accessible memory.
        using GorgonResourceCopier copier = new(outputData.Graphics);
        
        copier.CopyToRange(outputData.Buffer, results);        

        List<(int Index, OutputData Expected, OutputData Actual)> errors = [];

        // Compare the results.
        for (int i = 0; i < MaxValues; ++i)
        {
            OutputData data = results[i];

            if ((data.Sum == (i + i)) && (data.Product.EqualsEpsilon(i * i)))
            {
                continue;
            }

            errors.Add((i, new OutputData
            {
                Product = i * i,
                Sum = i + i
            }, data));
        }

        return errors;
    }

    /// <summary>
    /// Defines the entry point of the application.
    /// </summary>
    private static void Main()
    {
        GorgonStructuredBufferRwView? outputView = null;
        GorgonGpuBuffer? intInputBuffer = null;
        GorgonGpuBuffer? floatInputBuffer = null;

        IGorgonLog log = new GorgonTextFileLog("ComputeEngine", "Tape_Worm", typeof(Program).Assembly.GetName().Version);
        log.LogStart(new GorgonComputerInfo());

        try
        {
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("For this example, we'll upload two sets of values to the GPU and the compute engine will perform a simple mathematical");
            Console.WriteLine("operation on each sets of values to produce another set of values stored as a structured buffer.\n");

            Console.WriteLine("To do this, we first create a graphics interface so we can access the GPU from the compute engine, then we create the");
            Console.WriteLine("compute engine itself and bind the buffers containing the input values and the output buffer. Then, finally, we execute");
            Console.WriteLine("the compute shader and retrieve the result values from the output buffer into a buffer that the CPU can read and test");
            Console.WriteLine("them for accuracy.\n");
            Console.ResetColor();

            // Initialize the required objects.
            Console.WriteLine("Initializing...");

            // Create the factory used to create our graphics functionality.
            // This is required to create the primary graphics object used for accessing the GPU.
#if DEBUG
            using GorgonGraphicsFactory factory = new(GorgonGraphicsDebugFlags.SynchronizedCommandQueueValidation
                | GorgonGraphicsDebugFlags.GpuBasedValidation
                | GorgonGraphicsDebugFlags.GpuBasedStateTrackingValidation
                | GorgonGraphicsDebugFlags.ObjectTracking, log: log);
#else
            using GorgonGraphicsFactory factory = new(log: log);
#endif     
            Console.WriteLine("Enumerating video devices...");
            
            // We will need to access the graphics device in order to use compute functionality, so we'll use the first usable device in the system.
            // Find out which devices we have installed in the system.
            IReadOnlyList<GorgonVideoAdapterInfo> deviceList = factory.EnumerateAdapters();

            // Error out if no direct 3D 12 capable devices are available.
            if (deviceList.Count == 0)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No Direct3D 12 or better adapters found in the system. This example requires a Direct3D 12 or better adapter.");
                Console.ResetColor();
                return;
            }

            using GorgonGraphics graphics = factory.CreateGraphics(deviceList[0]);
            using GorgonComputePsoFactory psoFactory = new(graphics);

            // Finally, the star of the show, the compute engine.
            Console.WriteLine("Creating compute engine...");
            using GorgonComputeEngine engine = new(graphics);

            // Initialize our input and output data structures.
            Console.WriteLine("Creating data...");
            (outputView, intInputBuffer, floatInputBuffer) = BuildInputOutputBuffers(graphics);

            // Initialize and configure our dispatch call.
            // We'll execute 64 threads across 256 thread groups (MaxValues = 16384, numthreads = 64).
            GorgonDispatchCall dispatch = BuildDispatch(psoFactory, MaxValues / 64);

            // Tell the GPU how we'll be using these buffers.
            dispatch.AssignBuffers([
                new GorgonUsedBuffer(outputView, ShaderStage.Compute, BufferUsage.ReadWrite),
                new GorgonUsedBuffer(intInputBuffer, ShaderStage.Compute, BufferUsage.ReadOnly),
                new GorgonUsedBuffer(floatInputBuffer, ShaderStage.Compute, BufferUsage.ReadOnly)
            ]);

            GorgonTypedBufferView intView = intInputBuffer.GetTypedBufferView(BufferFormat.R32_SInt);
            GorgonTypedBufferView floatView = floatInputBuffer.GetTypedBufferView(BufferFormat.R32_Float);

            // Now execute the compute shader.
            Console.WriteLine($"Executing {MaxValues / 64} thread groups...");

            Execute(engine, dispatch, new InputData()
            {
                OutputBufferHandle = outputView.GetViewHandle(),
                IntBufferHandle = intView.GetViewHandle(),
                FloatBufferHandle = floatView.GetViewHandle()
            });

            // Now test the results to ensure we the engine did what we wanted.
            IReadOnlyList<(int Index, OutputData Expected, OutputData Actual)> errors = TestResults(outputView);

            if (errors.Count != 0)
            {
                foreach ((int Index, OutputData Expected, OutputData Actual) in errors)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failure at element {Index}.");

                    if (Expected.Sum != Actual.Sum)
                    {
                        Console.WriteLine($"Sum should be: {Expected.Sum}, Got: {Actual.Sum}");
                        Console.ResetColor();
                        break;
                    }

                    if (!Expected.Product.EqualsEpsilon(Actual.Product))
                    {
                        Console.WriteLine($"Product should be: {Expected.Product}, Got: {Actual.Product}");
                        Console.ResetColor();
                        break;
                    }                    
                }
            }
            else
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine("Success. All compute engine output values match expected values.");
                Console.ResetColor();
            }
        }
        catch (Exception ex)
        {
            // And, of course, if we have errors, then we should obviously handle them.
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"There was an error running the application:\n{ex.Message}\n\nStack trace:\n{ex.StackTrace}");
            Console.ResetColor();
        }
        finally
        {
            floatInputBuffer?.Dispose();
            intInputBuffer?.Dispose();
            outputView?.Dispose();

            log.LogEnd();
        }

        // Wait for the user to exit.
        Console.WriteLine("Press any key to exit.");
        Console.ReadKey();
    }
}
