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
// Created: August 1, 2017 9:35:50 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Examples.Properties;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;

namespace Gorgon.Examples
{
    /// <summary>
    /// The output data returned from the shader.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4)]
    internal struct OutputData
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
    /// An example showing how to use the Compute engine functionality for Gorgon.
    /// </summary>
    /// <remarks>
    /// This is a very simple example showing how to access the compute engine functionality in Gorgon. 
    /// 
    /// The compute engine allows an application, through the use of Compute Shaders, to perform arbitrary computation on the GPU. The GPU is an incredibly 
    /// fast number crunching device, and this functionality allows us to offload otherwise expensive computations from the CPU to the GPU. 
    /// 
    /// The compute shaders on the GPU perform their tasks uses multiple groups of multiple threads to allow many operations to be performed in parallel, 
    /// this is a major component in delivering high performance throughput.
    /// 
    /// For this example, we'll upload two sets of values to the GPU and the compute engine will perform a simple mathematical operation on each sets of 
    /// values to produce another set of values stored as a structured buffer.
    /// 
    /// To do this, we first create a graphics interface so we can access the GPU from the compute engine, then we create the compute engine itself and 
    /// bind the buffers containing the input values and the output buffer. Then, finally, we execute the compute shader and retrieve the result values from 
    /// the output buffer into a buffer that the CPU can read and test them for accuracy.
    /// </remarks>
    internal class Program
    {
        #region Constants.
        // The maximum number of elements to send and receive.
        private const int MaxValues = 10000;
        #endregion

        #region Variables.
        // The graphics device used to by the compute engine.
        private static GorgonGraphics _graphics;
        // The compute shader to run.
        private static GorgonComputeShader _computeShader;
        // Floating point data to send to the shader.
        private static GorgonBuffer _floatBuffer;
        // Integer data to send to the shader.
        private static GorgonBuffer _intBuffer;
        // The data retrieved from the shader.
        private static GorgonBuffer _outputBuffer;
        // The compute engine.
        private static GorgonComputeEngine _engine;
        // The dispatch call builder.
        private static readonly GorgonDispatchCallBuilder _dispatchBuilder = new();
        #endregion

        #region Methods.
        /// <summary>
        /// A function to create our input/input buffers and populate the input buffers with the data to send to the GPU.
        /// </summary>
        private static void BuildInputOutputBuffers()
        {
            // This structured buffer will contain the output from the compute operation. It is only accessible to the GPU, so we'll 
            // have to copy this data to a CPU accessible buffer once we're done, thus we have the _cpuData buffer below.
            // The GPU output data buffer will be bound to the compute engine as an unordered access view (UAV) so that we can write 
            // the data back into the buffer.
            _outputBuffer = new GorgonBuffer(_graphics,
                                             new GorgonBufferInfo("OutputData")
                                             {
                                                 Usage = ResourceUsage.Default,
                                                 SizeInBytes = Unsafe.SizeOf<OutputData>() * MaxValues,
                                                 Binding = BufferBinding.ReadWrite,
                                                 StructureSize = Unsafe.SizeOf<OutputData>(),
                                                 AllowCpuRead = true
                                             });

            // This is our input data buffers. One for integer values, and another for floating point.
            // These are just standard buffers that will be bound as a shader resource view (SRV).
            _intBuffer = new GorgonBuffer(_graphics,
                                          new GorgonBufferInfo("IntData")
                                          {
                                              Usage = ResourceUsage.Default,
                                              SizeInBytes = sizeof(int) * MaxValues,
                                              Binding = BufferBinding.Shader
                                          });

            _floatBuffer = new GorgonBuffer(_graphics,
                                            new GorgonBufferInfo("FloatData")
                                            {
                                                Usage = ResourceUsage.Default,
                                                SizeInBytes = sizeof(float) * MaxValues,
                                                Binding = BufferBinding.Shader
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
            _intBuffer.SetData<int>(inputInts);
            _floatBuffer.SetData<float>(inputFloats);
        }

        /// <summary>
        /// Function to initialize the example and prepare the required components.
        /// </summary>
        private static void Initialize()
        {
            // We will need to access the graphics device in order to use compute functionality, so we'll use the first usable device in the system.
            // Find out which devices we have installed in the system.
            IReadOnlyList<IGorgonVideoAdapterInfo> deviceList = GorgonGraphics.EnumerateAdapters();

            Console.WriteLine("Enumerating video devices...");

            IGorgonVideoAdapterInfo firstDevice = deviceList.FirstOrDefault(item => item.FeatureSet >= FeatureSet.Level_11_2);

            // If a device with a feature set of at least 12.0 not found, then we cannot run this example. The compute engine requires a minimum 
            // of feature level 12.0 to run.
            if (firstDevice is null)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("No Level 11.2 or better adapters found in the system. This example requires a FeatureLevel 11.2 or better adapter.");
                Console.ResetColor();
                return;
            }

            Console.WriteLine($"Using the '{firstDevice.Name}' video device.\n");

            // We have to create a graphics interface to allow the compute engine to communicate with the GPU.
            _graphics = new GorgonGraphics(firstDevice);

            // We will also need to compile a compute shader so we can actually perform the work.
            Console.WriteLine("Compiling the compute shader (SimpleCompute)...");
            _computeShader = GorgonShaderFactory.Compile<GorgonComputeShader>(_graphics, Resources.ComputeShader, "SimpleCompute");

            // Finally, the star of the show, the compute engine.
            Console.WriteLine("Creating compute engine...");
            _engine = new GorgonComputeEngine(_graphics);
        }

        /// <summary>
        /// Function to execute the compute shader with the compute engine.
        /// </summary>
        /// <returns>An array of <see cref="OutputData"/> values.  These contain the results from the compute engine.</returns>
        private static OutputData[] Execute()
        {
            // The first step is to provide the compute shader with the information it needs to do its job.
            GorgonDispatchCall dispatch = _dispatchBuilder.ShaderResource(_intBuffer.GetShaderResourceView(BufferFormat.R32_SInt))
                                                          .ShaderResource(_floatBuffer.GetShaderResourceView(BufferFormat.R32_Float), 1)
                                                          .ReadWriteView(new GorgonReadWriteViewBinding(_outputBuffer.GetStructuredReadWriteView()))
                                                          .ComputeShader(_computeShader)
                                                          .Build();

            // Now, we execute the shader with MaxValues threads for the first thread group.
            _engine.Execute(dispatch, MaxValues, 1, 1);

            // Finally, let's turn this into some meaningful data structures rather than raw buffer data.
            // This will enable us handle the data better in our code.
            return _outputBuffer.GetData<OutputData>();
        }

        /// <summary>
        /// Function to perform tests on the output data from the engine.
        /// </summary>
        /// <param name="results">The output data results from the engine.</param>
        /// <returns>A list of errors if any were found, or an empty list if the operation was completely successful.</returns>
        private static IReadOnlyList<(int Index, OutputData Expected, OutputData Actual)> TestResults(OutputData[] results)
        {
            var errors = new List<(int Index, OutputData Expected, OutputData Actual)>();

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
                Initialize();

                // Initialize our input and output data structures.
                Console.WriteLine("Creating data...");
                BuildInputOutputBuffers();

                // Now create the engine and execute the shader.
                Console.WriteLine("Executing...");
                OutputData[] results = Execute();

                // Now test the results to ensure we the engine did what we wanted.
                IReadOnlyList<(int Index, OutputData Expected, OutputData Actual)> errors = TestResults(results);

                if (errors.Count != 0)
                {
                    foreach ((int Index, OutputData Expected, OutputData Actual) in errors)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Failure at element {Index}.");

                        if (Expected.Sum != Actual.Sum)
                        {
                            Console.WriteLine($"Sum should be: {Expected.Sum}, Got: {Actual.Sum}");
                        }

                        if (!Expected.Product.EqualsEpsilon(Actual.Product))
                        {
                            Console.WriteLine($"Product should be: {Expected.Product}, Got: {Actual.Product}");
                        }

                        Console.ResetColor();
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
                _floatBuffer?.Dispose();
                _intBuffer?.Dispose();
                _outputBuffer?.Dispose();
                _computeShader?.Dispose();
                _graphics?.Dispose();
            }

            // Wait for the user to exit.
            Console.WriteLine("Press any key to exit.");
            Console.ReadKey();
        }
        #endregion
    }
}
