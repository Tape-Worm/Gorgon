#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Saturday, October 14, 2006 5:41:37 PM
// 
#endregion

using System;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Enumeration containing shader compilation flags.
	/// </summary>
	[Flags()]
	public enum ShaderCompileOptions
	{
		/// <summary>
		/// No optimization or options.
		/// </summary>
		None = 0,
		/// <summary>
		/// Debug mode.
		/// </summary>
		Debug = 1,
		/// <summary>
		/// Instruct the compiler to skip optimization steps during code generation.
		/// Unless you are trying to isolate a problem in your code and you suspect
		/// the compiler, using this option is not recommended.
		/// </summary>
		SkipValidation = 2,
		/// <summary>
		/// Do not validate the generated code against known capabilities and constraints.
		/// This option is recommended only when compiling shaders that are known to
		/// work (that is, shaders that have compiled before without this option). Shaders
		/// are always validated by the runtime before they are set to the device.
		/// </summary>
		SkipOptimization = 4,
		/// <summary>
		/// Unless explicitly specified, matrices will be packed in row-major order (each
		/// vector will be in a single row) when passed to or from the shader.
		/// </summary>
		PackMatrixRowMajor = 8,
		/// <summary>
		/// Unless explicitly specified, matrices will be packed in column-major order
		/// (each vector will be in a single column) when passed to and from the shader.
		/// This is generally more efficient because it allows vector-matrix multiplication
		/// to be performed using a series of dot products.
		/// </summary>
		PackMatrixColumnMajor = 16,
		/// <summary>
		/// Force all computations in the resulting shader to occur at partial precision
		/// This may result in faster evaluation of shaders on some hardware.
		/// </summary>
		PartialPrecision = 32,
		/// <summary>
		/// Force the compiler to compile against the next highest available software
		/// target for vertex shaders.  This flag also turns optimizations off and debugging
		/// on.
		/// </summary>
		ForceVSSoftwareNoOpt = 64,
		/// <summary>
		/// Force the compiler to compile against the next highest available software
		/// target for pixel shaders.  This flag also turns optimizations off and debugging
		/// on.
		/// </summary>
		ForcePSSoftwareNoOpt = 128,
		/// <summary>
		/// Disables preshaders. The compiler will not pull out static expressions for
		/// evaluation on the host CPU.  Additionally, the compiler will not loft any
		/// expressions when compiling stand-alone functions.
		/// </summary>
		NoPreshader = 256,
		/// <summary>
		/// This is a hint to the compiler to avoid using flow-control instructions.
		/// </summary>
		AvoidFlowControl = 512,
		/// <summary>
		/// This is a hint to the compiler to prefer using flow-control instructions.
		/// </summary>
		PreferFlowControl = 1024,
		/// <summary>
		/// Disable optimizations that may cause the output of a compiled shader program
		/// to differ from the output of a program compiled with the DirectX 9 shader
		/// compiler due to small precision erros in floating point math.
		/// </summary>
		IeeeStrictness = 8192,
		/// <summary>
		/// Lowest optimization level. May produce slower code but will do so more quickly.
		/// This may be useful in a highly iterative shader development cycle.
		/// </summary>
		OptimizationLevel0 = 16384,
		/// <summary>
		/// Highest optimization level. Will produce best possible code but may take
		/// significantly longer to do so.  This will be useful for final builds of an
		/// application where performance is the most important factor.
		/// </summary>
		OptimizationLevel3 = 32768,
		/// <summary>
		/// Second highest optimization level.
		/// </summary>
		OptimizationLevel2 = 49152
	}

	/// <summary>
	/// Enumeration containing shader parameter types.
	/// </summary>
	public enum ShaderParameterType
	{
		/// <summary>
		/// Boolean value.
		/// </summary>
		Boolean = 1,
		/// <summary>
		/// Floating point value.
		/// </summary>
		Float = 2,
		/// <summary>
		/// Integer value.
		/// </summary>
		Integer = 3,
		/// <summary>
		/// Image.
		/// </summary>
		Image = 4,
		/// <summary>
		/// Render image.
		/// </summary>
		RenderImage = 5,
		/// <summary>
		/// Color value.
		/// </summary>
		Color = 6,
		/// <summary>
		/// Matrix value.
		/// </summary>
		Matrix = 7,
		/// <summary>
		/// 4 dimensional vector.
		/// </summary>
		Vector4D = 8,
		/// <summary>
		/// String value.
		/// </summary>
		String = 9,
		/// <summary>
		/// Unknown type.
		/// </summary>
		Unknown = 0
	}
}