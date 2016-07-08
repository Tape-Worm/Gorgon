#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 8, 2016 11:26:58 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using SharpDX;
using D3D = SharpDX.Direct3D;
using D3DCompiler = SharpDX.D3DCompiler;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Provides functionality to compile shaders from source code.
	/// </summary>
	public static class GorgonShaderCompiler
	{
		#region Variables.
		// A processor used to parse shader source code for include statements.
		private static readonly ShaderProcessor _processor = new ShaderProcessor();
		// A list of available shader types.
		private static readonly Tuple<Type, ShaderType>[] _shaderTypes =
		{
			new Tuple<Type, ShaderType>(typeof(GorgonVertexShader), ShaderType.Vertex),
			new Tuple<Type, ShaderType>(typeof(GorgonPixelShader), ShaderType.Pixel),
		};
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the list of cached included source code.
		/// </summary>
		/// <remarks>
		/// This contains source code retrieved from <c>#GorgonInclude</c> statements. It can either be populated from the main source code, or be manually populated by the user.
		/// </remarks>
		public static IDictionary<string, GorgonShaderInclude> Includes => _processor.CachedIncludes;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the shader profile to use based on the feature level.
		/// </summary>
		/// <param name="featureLevel">The feature level used to determine the profile.</param>
		/// <param name="shaderType">The type of shader.</param>
		/// <returns>A string containing the profile information.</returns>
		private static string GetProfile(FeatureLevelSupport featureLevel, ShaderType shaderType)
		{
			string prefix;

			if (((shaderType == ShaderType.Compute)
			     || (shaderType == ShaderType.Domain)
			     || (shaderType == ShaderType.Hull))
			    && (featureLevel < FeatureLevelSupport.Level_11_0))
			{
				throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_11_0));
			}

			if ((shaderType == ShaderType.Geometry)
				&& ((featureLevel < FeatureLevelSupport.Level_10_0)))
			{
				throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_REQUIRES_FEATURE_LEVEL, FeatureLevelSupport.Level_10_0));
			}

			switch (shaderType)
			{
				case ShaderType.Pixel:
					prefix = "ps";
					break;
				case ShaderType.Compute:
					prefix = "cs";
					break;
				case ShaderType.Geometry:
					prefix = "gs";
					break;
				case ShaderType.Domain:
					prefix = "ds";
					break;
				case ShaderType.Hull:
					prefix = "hs";
					break;
				case ShaderType.Vertex:
					prefix = "vs";
					break;
				default:
					throw new NotSupportedException(string.Format(Resources.GORGFX_ERR_SHADER_UNKNOWN_TYPE, shaderType));
			}

			switch (featureLevel)
			{
				case FeatureLevelSupport.Level_10_0:
					return prefix + "_4_0";
				case FeatureLevelSupport.Level_10_1:
					return prefix + "_4_1";
				default:
					return prefix + "_5_0";
			}
		}

		/// <summary>
		/// Function to build the actual shader from byte code.
		/// </summary>
		/// <param name="device">The video device used to create the shader.</param>
		/// <param name="shaderType">The type of shader to build.</param>
		/// <param name="entryPoint">The entry point function.</param>
		/// <param name="isDebug"><b>true</b> if the byte code has debug info, <b>false</b> if not.</param>
		/// <param name="byteCode">The byte code for the shader.</param>
		/// <returns>The shader based on the <paramref name="shaderType"/>.</returns>
		private static T GetShader<T>(IGorgonVideoDevice device, ShaderType shaderType, string entryPoint, bool isDebug, D3DCompiler.ShaderBytecode byteCode)
			where T : GorgonShader
		{
			switch (shaderType)
			{
				case ShaderType.Compute:
				case ShaderType.Hull:
				case ShaderType.Domain:
				case ShaderType.Geometry:
					throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_SHADER_UNKNOWN_TYPE, shaderType));
				case ShaderType.Pixel:
					return new GorgonPixelShader(device, entryPoint, isDebug, byteCode) as T;
				case ShaderType.Vertex:
					return new GorgonVertexShader(device, entryPoint, isDebug, byteCode) as T;
			}

			throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_SHADER_UNKNOWN_TYPE, shaderType));
		}

		/// <summary>
		/// Function to compile a shader from source code.
		/// </summary>
		/// <typeparam name="T">The type of shader to return.</typeparam>
		/// <param name="videoDevice">The video device used to create the shader.</param>
		/// <param name="sourceCode">A string containing the source code to compile.</param>
		/// <param name="entryPoint">The entry point function for the shader.</param>
		/// <param name="debug">[Optional] <b>true</b> to indicate whether the shader should be compiled with debug information; otherwise, <b>false</b> to strip out debug information.</param>
		/// <param name="macros">[Optional] A list of macros for conditional compilation.</param>
		/// <param name="sourceFileName">[Optional] The name of the file where the source code came from.</param>
		/// <returns>A byte array containing the compiled shader.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="sourceCode"/>, <paramref name="entryPoint"/> or the <paramref name="videoDevice"/> parameter is <b>null</b>.</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="sourceCode"/>, <paramref name="entryPoint"/> parameter is empty.</exception>
		/// <exception cref="GorgonException">Thrown when the type specified by <typeparamref name="T"/> is not a valid shader type.</exception>
		public static T Compile<T>(IGorgonVideoDevice videoDevice, string sourceCode, string entryPoint, bool debug = false, IList<GorgonShaderMacro> macros = null, string sourceFileName = "(in memory)")
			where T : GorgonShader
		{
			if (videoDevice == null)
			{
				throw new ArgumentNullException(nameof(videoDevice));
			}

			if (sourceCode == null)
			{
				throw new ArgumentNullException(nameof(sourceCode));
			}

			if (entryPoint == null)
			{
				throw new ArgumentNullException(nameof(entryPoint));
			}

			if (string.IsNullOrWhiteSpace(sourceCode))
			{
				throw new ArgumentException(Resources.GORGFX_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(sourceCode));
			}

			if (string.IsNullOrWhiteSpace(entryPoint))
			{
				throw new ArgumentException(Resources.GORGFX_ERR_PARAMETER_MUST_NOT_BE_EMPTY, nameof(entryPoint));
			}

			Tuple<Type, ShaderType> shaderType = _shaderTypes.FirstOrDefault(item => item.Item1 == typeof(T));

			if (shaderType == null)
			{
				throw new GorgonException(GorgonResult.CannotCreate, string.Format(Resources.GORGFX_ERR_SHADER_UNKNOWN_TYPE, typeof(T).FullName));
			}

			// Get shader flags based on our feature level and whether we want debug info or not.
			D3DCompiler.ShaderFlags flags = debug ? D3DCompiler.ShaderFlags.Debug : D3DCompiler.ShaderFlags.OptimizationLevel3;

			if (videoDevice.RequestedFeatureLevel < FeatureLevelSupport.Level_11_0)
			{
				flags |= D3DCompiler.ShaderFlags.EnableBackwardsCompatibility;
			}

			// Make compatible macros for the shader compiler.
			D3D.ShaderMacro[] actualMacros = null;

			if ((macros != null) && (macros.Count > 0))
			{
				actualMacros = macros.Select(item => item.D3DShaderMacro).ToArray();
			}

			string profile = GetProfile(videoDevice.RequestedFeatureLevel, shaderType.Item2);
			string processedSource = _processor.Process(sourceCode);

			try
			{
				D3DCompiler.ShaderBytecode byteCode = D3DCompiler.ShaderBytecode.Compile(processedSource,
				                                                                         entryPoint,
				                                                                         profile,
				                                                                         flags,
				                                                                         D3DCompiler.EffectFlags.None,
				                                                                         actualMacros,
				                                                                         null,
				                                                                         sourceFileName);

				return GetShader<T>(videoDevice, shaderType.Item2, entryPoint, debug, byteCode);
			}
			catch (CompilationException cEx)
			{
				throw new GorgonException(GorgonResult.CannotCompile, string.Format(Resources.GORGFX_ERR_CANNOT_COMPILE_SHADER, cEx.Message));
			}
		}
		#endregion
	}
}
