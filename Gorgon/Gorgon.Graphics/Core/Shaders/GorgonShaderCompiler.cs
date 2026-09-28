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
// Created: September 28, 2025 11:58:26 PM
//

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Core.Codecs;
using Gorgon.Graphics.Core.Properties;
using Gorgon.Native;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using DX = TerraFX.Interop.DirectX.DirectX;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// The supported shader models.
/// </summary>
public enum ShaderModel
{
    /// <summary>
    /// Unsupported shader model.
    /// </summary>
    Unsupported,
    /// <summary>
    /// Shader model 6.6.
    /// </summary>
    ShaderModel_6_6,
    /// <summary>
    /// Shader model 6.7.
    /// </summary>
    ShaderModel_6_7,
    /// <summary>
    /// Shader model 6.8.
    /// </summary>
    ShaderModel_6_8,
    /// <summary>
    /// The highest version available on the GPU.
    /// </summary>
    HighestVersionAvailable
}

/// <summary>
/// Flags to be passed to the compiler.
/// </summary>
[Flags]
public enum CompileFlags
{
    /// <summary>
    /// No flags.
    /// </summary>
    None = 0,
    /// <summary>
    /// Compile with debug information.
    /// </summary>
    Debug = 1,
    /// <summary>
    /// Optimization level 0.
    /// </summary>
    OptimizationLevel0 = 2,
    /// <summary>
    /// Optimization level 1.
    /// </summary>
    OptimizationLevel1 = 4,
    /// <summary>
    /// Optimization level 2.
    /// </summary>
    OptimizationLevel2 = 8,
    /// <summary>
    /// Optimization level 3.
    /// </summary>
    OptimizationLevel3 = 16,
    /// <summary>
    /// Enable strict mode.
    /// </summary>
    StrictMode = 32,
    /// <summary>
    /// Avoid flow control constructs.
    /// </summary>
    AvoidFlowConstructs = 64,
    /// <summary>
    /// Enable IEEE strictness.
    /// </summary>
    IEEEStrictness = 128,
    /// <summary>
    /// Prefer row major matrix formatting.
    /// </summary>
    RowMajor = 256,
    /// <summary>
    /// Treat warnings as errors.
    /// </summary>
    WarningsAsErrors = 512,
    /// <summary>
    /// Do not include reflection information.
    /// </summary>
    NoReflection = 1024
}

/// <summary>
/// Functionality to compile shader programs from HLSL source code.
/// </summary>
/// <remarks>
/// <para>
/// Shaders typically come in source code form as strings. To compile them for use in a scene, applications should use this compiler to build <see cref="GorgonShader"/> objects. This compiles the shaders
/// into an intermediate format which is contained in a resulting <see cref="GorgonShader"/>. When that shader is used by a <see cref="GorgonGraphicsPso"/>, <see cref="GorgonComputePso"/>, or 
/// <see cref="GorgonMeshPso"/>, then it is converted into the correct native format for the GPU. 
/// </para>
/// <para>
/// Because the compilation can take time, it is best to compile the shaders to the intermediate format before rendering starts. Preferably, the compiled shaders should be stored on disk directly, or in
/// a PSO cache.
/// </para>
/// </remarks>
/// <seealso cref="GorgonShader"/>
public unsafe class GorgonShaderCompiler
    : IDisposable
{
    /// <summary>
    /// A shader profile used for compilation.
    /// </summary>
    /// <param name="ShaderType">The type of shader.</param>
    /// <param name="ShaderModel">The shader model.</param>
    /// <param name="Profile">The D3D compiler profile.</param>
    private record class ShaderProfiles(ShaderType ShaderType, ShaderModel ShaderModel, string Profile);

    private static ComPtr<IDxcCompiler3> _compiler;
    private static ComPtr<IDxcUtils> _utils;
    private static ComPtr<IDxcIncludeHandler> _includeHandler;

    private static readonly Lock _compilerLock = new();
    private static int _compilerCreateCounter;
    private readonly ShaderProcessor _processor = new();

    private static readonly Dictionary<CompileFlags, List<string>> _compileFlagValues = new()
    {
        { CompileFlags.None, [] },
        { CompileFlags.Debug, [DXC.DXC_ARG_DEBUG, DXC.DXC_ARG_SKIP_OPTIMIZATIONS]},
        { CompileFlags.OptimizationLevel0, [DXC.DXC_ARG_OPTIMIZATION_LEVEL0]},
        { CompileFlags.OptimizationLevel1, [DXC.DXC_ARG_OPTIMIZATION_LEVEL1]},
        { CompileFlags.OptimizationLevel2, [DXC.DXC_ARG_OPTIMIZATION_LEVEL2]},
        { CompileFlags.OptimizationLevel3, [DXC.DXC_ARG_OPTIMIZATION_LEVEL3]},
        { CompileFlags.StrictMode, [DXC.DXC_ARG_ENABLE_STRICTNESS]},
        { CompileFlags.AvoidFlowConstructs, [DXC.DXC_ARG_AVOID_FLOW_CONTROL]},
        { CompileFlags.IEEEStrictness, [DXC.DXC_ARG_IEEE_STRICTNESS]},
        { CompileFlags.RowMajor, [DXC.DXC_ARG_PACK_MATRIX_ROW_MAJOR]},
        { CompileFlags.WarningsAsErrors, [DXC.DXC_ARG_WARNINGS_ARE_ERRORS]},
        { CompileFlags.NoReflection, ["-Qstrip_reflect"]},
    };

    private static readonly Dictionary<ShaderModel, Dictionary<ShaderType, ShaderProfiles>> _shaderMatrix = new()
    {
        { ShaderModel.ShaderModel_6_6, new Dictionary<ShaderType, ShaderProfiles>()
        {
            { ShaderType.VertexShader, new ShaderProfiles(ShaderType.VertexShader, ShaderModel.ShaderModel_6_6, "vs_6_6") },
            { ShaderType.PixelShader, new ShaderProfiles(ShaderType.PixelShader, ShaderModel.ShaderModel_6_6, "ps_6_6") },
            { ShaderType.GeometryShader, new ShaderProfiles(ShaderType.GeometryShader, ShaderModel.ShaderModel_6_6, "gs_6_6") },
            { ShaderType.HullShader, new ShaderProfiles(ShaderType.HullShader, ShaderModel.ShaderModel_6_6, "hs_6_6") },
            { ShaderType.DomainShader, new ShaderProfiles(ShaderType.DomainShader, ShaderModel.ShaderModel_6_6, "ds_6_6") },
            { ShaderType.ComputeShader, new ShaderProfiles(ShaderType.ComputeShader, ShaderModel.ShaderModel_6_6, "cs_6_6") },
            { ShaderType.MeshShader, new ShaderProfiles(ShaderType.MeshShader, ShaderModel.ShaderModel_6_6, "ms_6_6") },
            { ShaderType.AmplificationShader, new ShaderProfiles(ShaderType.AmplificationShader, ShaderModel.ShaderModel_6_6, "as_6_6") }
        }
        },
        { ShaderModel.ShaderModel_6_7, new Dictionary<ShaderType, ShaderProfiles>()
        {
            { ShaderType.VertexShader, new ShaderProfiles(ShaderType.VertexShader, ShaderModel.ShaderModel_6_7, "vs_6_7") },
            { ShaderType.PixelShader, new ShaderProfiles(ShaderType.PixelShader, ShaderModel.ShaderModel_6_7, "ps_6_7") },
            { ShaderType.GeometryShader, new ShaderProfiles(ShaderType.GeometryShader, ShaderModel.ShaderModel_6_7, "gs_6_7") },
            { ShaderType.HullShader, new ShaderProfiles(ShaderType.HullShader, ShaderModel.ShaderModel_6_7, "hs_6_7") },
            { ShaderType.DomainShader, new ShaderProfiles(ShaderType.DomainShader, ShaderModel.ShaderModel_6_7, "ds_6_7") },
            { ShaderType.ComputeShader, new ShaderProfiles(ShaderType.ComputeShader, ShaderModel.ShaderModel_6_7, "cs_6_7") },
            { ShaderType.MeshShader, new ShaderProfiles(ShaderType.MeshShader, ShaderModel.ShaderModel_6_7, "ms_6_7") },
            { ShaderType.AmplificationShader, new ShaderProfiles(ShaderType.AmplificationShader, ShaderModel.ShaderModel_6_7, "as_6_7") }
        }
        },
        { ShaderModel.ShaderModel_6_8, new Dictionary<ShaderType, ShaderProfiles>()
        {
            { ShaderType.VertexShader, new ShaderProfiles(ShaderType.VertexShader, ShaderModel.ShaderModel_6_8, "vs_6_8") },
            { ShaderType.PixelShader, new ShaderProfiles(ShaderType.PixelShader, ShaderModel.ShaderModel_6_8, "ps_6_8") },
            { ShaderType.GeometryShader, new ShaderProfiles(ShaderType.GeometryShader, ShaderModel.ShaderModel_6_8, "gs_6_8") },
            { ShaderType.HullShader, new ShaderProfiles(ShaderType.HullShader, ShaderModel.ShaderModel_6_8, "hs_6_8") },
            { ShaderType.DomainShader, new ShaderProfiles(ShaderType.DomainShader, ShaderModel.ShaderModel_6_8, "ds_6_8") },
            { ShaderType.ComputeShader, new ShaderProfiles(ShaderType.ComputeShader, ShaderModel.ShaderModel_6_8, "cs_6_8") },
            { ShaderType.MeshShader, new ShaderProfiles(ShaderType.MeshShader, ShaderModel.ShaderModel_6_8, "ms_6_8") },
            { ShaderType.AmplificationShader, new ShaderProfiles(ShaderType.AmplificationShader, ShaderModel.ShaderModel_6_8, "as_6_8") }
        }
        }
    };

    /// <summary>
    /// Property to return the graphics object that is associated with this compiler.
    /// </summary>
    public GorgonGraphics Graphics
    {
        get;
    }

    /// <summary>
    /// Property to return the list of <see cref="GorgonShaderInclude"/> definitions to include with compiled shaders.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Gorgon uses a special keyword in shaders to allow shader files to include other files as part of the source. This keyword is named <c>#GorgonInclude</c> and is similar to the HLSL <c>#include</c> 
    /// keyword. The difference is that this keyword allows users to include shader source from memory instead of a separate source file. This is done by assigning a name to the included source code in the 
    /// <c>#GorgonInclude</c> keyword, and adding a <see cref="GorgonShaderInclude"/> containing the source code to this property with the same name. Names are not case sensitive.
    /// </para>
    /// <para>
    /// Includes can also be loaded from a file by passing a path to the <c>#GorgonInclude</c> keyword (e.g. <c>#GorgonInclude "Name", "Path\To\The\File.hlsl"</c>). The source code for the file is loaded when 
    /// the include is processed, and is added to this property with the name from the keyword.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonShaderInclude"/>
    public IDictionary<string, GorgonShaderInclude> Includes => _processor.CachedIncludes;

    /// <summary>
    /// Function to gather all the compilation flags and build a list of equivalent string arguments to be passed to the compiler.
    /// </summary>
    /// <param name="flags">The flags passed to the compiler.</param>
    /// <param name="args">The list of strings to populate.</param>
    private static void GetFlagArgs(CompileFlags flags, List<string> args)
    {
        if (flags == CompileFlags.None)
        {
            return;
        }

        CompileFlags highestOpt = CompileFlags.None;
        string optimizationString = string.Empty;

        bool isDebug = (flags & CompileFlags.Debug) == CompileFlags.Debug;

        foreach (KeyValuePair<CompileFlags, List<string>> flag in _compileFlagValues)
        {
            if (!isDebug)
            {
                if ((flag.Key & CompileFlags.OptimizationLevel0) == CompileFlags.OptimizationLevel0)
                {
                    if (highestOpt < CompileFlags.OptimizationLevel0)
                    {
                        highestOpt = CompileFlags.OptimizationLevel0;
                        optimizationString = flag.Value[0];
                    }
                    continue;
                }

                if ((flag.Key & CompileFlags.OptimizationLevel1) == CompileFlags.OptimizationLevel1)
                {
                    if (highestOpt < CompileFlags.OptimizationLevel1)
                    {
                        highestOpt = CompileFlags.OptimizationLevel1;
                        optimizationString = flag.Value[0];
                    }
                    continue;
                }

                if ((flag.Key & CompileFlags.OptimizationLevel2) == CompileFlags.OptimizationLevel2)
                {
                    if (highestOpt < CompileFlags.OptimizationLevel2)
                    {
                        highestOpt = CompileFlags.OptimizationLevel2;
                        optimizationString = flag.Value[0];
                    }
                    continue;
                }

                if ((flag.Key & CompileFlags.OptimizationLevel3) == CompileFlags.OptimizationLevel3)
                {
                    if (highestOpt < CompileFlags.OptimizationLevel3)
                    {
                        highestOpt = CompileFlags.OptimizationLevel3;
                        optimizationString = flag.Value[0];
                    }
                    continue;
                }
            }

            if ((flags & flag.Key) == flag.Key)
            {
                for (int i = 0; i < flag.Value.Count; ++i)
                {
                    args.Add(flag.Value[i].ToString());
                }
            }
        }

        if (!string.IsNullOrWhiteSpace(optimizationString))
        {
            args.Add(optimizationString);
        }
    }

    /// <summary>
    /// Function to build the macro parameters for the compiler.
    /// </summary>
    /// <param name="macros">The list of macros to retrieve.</param>
    /// <param name="args">The argument list to update.</param>
    private static void GetMacros(IReadOnlyList<GorgonShaderMacro> macros, List<string> args)
    {
        if (macros.Count == 0)
        {
            return;
        }

        for (int i = 0; i < macros.Count; ++i)
        {
            args.Add("-D");
            if (macros[i].Value is not null)
            {
                args.Add($"{macros[i].Name}={macros[i].Value}");
            }
            else
            {
                args.Add($"{macros[i].Name}");
            }
        }
    }

    /// <summary>
    /// Function to retrieve any compilation errors.
    /// </summary>
    /// <param name="result">The result of the compilation process.</param>
    /// <returns>The string containing the compiler errors.</returns>
    /// <exception cref="GorgonException">Thrown if there was an error reading the compilation errors.</exception>
    private string GetCompilationErrors(ComPtr<IDxcResult> result)
    {
        if (!result.Get()->HasOutput(DXC_OUT_KIND.DXC_OUT_ERRORS))
        {
            return string.Empty;
        }

        using ComPtr<IDxcBlobUtf8> errors = default;

        result.Get()->GetOutput(DXC_OUT_KIND.DXC_OUT_ERRORS, Win32.__uuidof<IDxcBlobUtf8>(), (void**)errors.GetAddressOf(), null)
            .ThrowIfFailed(GorgonResult.CannotRead, () => Resources.GORGFX_ERR_CANNOT_RETRIEVE_SHADER_COMPILATION_ERRORS);

        if ((errors.Get()->GetBufferSize() == 0) || (errors.Get()->GetBufferPointer() is null))
        {
            return string.Empty;
        }

        return Encoding.UTF8.GetString((byte*)errors.Get()->GetBufferPointer(), (int)errors.Get()->GetBufferSize());
    }

    /// <summary>
    /// Function to return the shader source blob for the shader source code.
    /// </summary>
    /// <param name="source">The source code for the shader.</param>
    /// <param name="profile">The shader model profile.</param>
    /// <param name="buffer">The buffer definition for the compiler.</param>
    /// <returns>A UTF-16 blob containing the shader source.</returns>
    private ComPtr<IDxcBlobEncoding> GetShaderSourceBlob(string source, ShaderProfiles profile, out DxcBuffer buffer)
    {
        string processedSource = _processor.Process(source);

        ComPtr<IDxcBlobEncoding> result = default;

        fixed (char* shaderCodePtr = processedSource)
        {
            _utils.Get()->CreateBlob(shaderCodePtr, (uint)(Encoding.Unicode.GetByteCount(processedSource)), DXC.DXC_CP_UTF16, result.GetAddressOf())
                .ThrowIfFailed(GorgonResult.CannotCompile, () => string.Format(Resources.GORGFX_ERR_CANNOT_COMPILE_SHADER, profile));
        }

        buffer = new DxcBuffer
        {
            Encoding = DXC.DXC_CP_UTF16,
            Ptr = result.Get()->GetBufferPointer(),
            Size = result.Get()->GetBufferSize()
        };

        return result;
    }

    /// <summary>
    /// Function to build up the parameters to pass to the compiler as C style strings.
    /// </summary>
    /// <param name="entryPoint">The entrypoint for the shader.</param>
    /// <param name="shaderModelProfile">The shader model profile for the shader.</param>
    /// <param name="macros">Any macros defined for the shader.</param>
    /// <param name="flags">Compilation flags.</param>
    /// <param name="argCount">The number of arguments to pass.</param>
    /// <returns>A buffer containing the pointers to all parameters.</returns>
    private char** GetCompilerArguments(string entryPoint, string shaderModelProfile, IReadOnlyList<GorgonShaderMacro> macros, CompileFlags flags, out uint argCount)
    {
        List<string> args = [];

        args.Add("-E");
        args.Add(entryPoint);
        args.Add("-T");
        args.Add(shaderModelProfile);
        if ((flags & CompileFlags.Debug) != CompileFlags.Debug)
        {
            args.Add(DXC.DXC_ARG_ALL_RESOURCES_BOUND);
        }

        if (!Graphics.IsInDebugMode)
        {
            // The debug and reflection info are still available, just removes from the main shader blob.
            args.Add("-Qstrip_debug");
            args.Add("-Qstrip_reflect");
        }

        GetFlagArgs(flags, args);
        GetMacros(macros, args);

        argCount = (uint)args.Count;

        char** buffer = (char**)NativeMemory.Alloc(argCount, (nuint)UIntPtr.Size);

        nint* bufferPtr = (nint*)buffer;
        for (int i = 0; i < args.Count; ++i)
        {
            *bufferPtr = Marshal.StringToHGlobalUni(args[i]);
            bufferPtr++;
        }

        return buffer;
    }

    /// <summary>
    /// Function to free the compiler arguments generated by <see cref="GetCompilerArguments"/>.
    /// </summary>
    /// <param name="args">The argument data to free.</param>
    /// <param name="argCount">The number of arguments.</param>
    private static void FreeCompilerArguments(char** args, uint argCount)
    {
        nint* bufferPtr = (nint*)args;
        for (int i = 0; i < argCount; ++i)
        {
            Marshal.FreeHGlobal(*bufferPtr);
            bufferPtr++;
        }
        NativeMemory.Free(args);
    }

    /// <summary>
    /// Function to build the native compiler objects.
    /// </summary>
    /// <remarks>
    /// <para>
    /// We only ever create one instance of these, shared amongst all factories.  
    /// </para>
    /// </remarks>
    private void BuildNativeObjects()
    {
        using (_compilerLock.EnterScope())
        {
            if ((_compilerCreateCounter++) > 0)
            {
                return;
            }

            DX.DxcCreateInstance((Guid*)Unsafe.AsPointer(ref Unsafe.AsRef(in CLSID.CLSID_DxcUtils)), Win32.__uuidof<IDxcUtils>(), (void**)_utils.GetAddressOf())
                .ThrowIfFailed(GorgonResult.CannotCreate, () => Resources.GORGFX_ERR_CANNOT_CREATE_DXCOMPILER);
            DX.DxcCreateInstance((Guid*)Unsafe.AsPointer(ref Unsafe.AsRef(in CLSID.CLSID_DxcCompiler)), Win32.__uuidof<IDxcCompiler3>(), (void**)_compiler.GetAddressOf())
                .ThrowIfFailed(GorgonResult.CannotCreate, () => Resources.GORGFX_ERR_CANNOT_CREATE_DXCOMPILER);
            _utils.Get()->CreateDefaultIncludeHandler(_includeHandler.GetAddressOf())
                .ThrowIfFailed(GorgonResult.CannotCreate, () => Resources.GORGFX_ERR_CANNOT_CREATE_DXCOMPILER);
        }
    }

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        this.UnregisterDisposable(Graphics);

        if (disposing)
        {
            if (_compilerCreateCounter == 1)
            {
                Graphics.Log.Print("Destroying DX Compiler, default include handler and utilities instances.", LoggingLevel.Verbose);
            }
        }

        if (Interlocked.Decrement(ref _compilerCreateCounter) > 0)
        {
            return;
        }

        _includeHandler.Dispose();
        _compiler.Dispose();
        _utils.Dispose();
    }

    /// <summary>
    /// Function to compile a shader from source code.
    /// </summary>
    /// <param name="sourceCode">The string containing the shader source code.</param>
    /// <param name="entryPoint">The main entry point for the shader.</param>
    /// <param name="shaderType">The type of shader.</param>
    /// <param name="shaderModel">[Optional] The shader model to use.</param>
    /// <param name="flags">[Optional] Flags used when compiling the shader.</param>
    /// <param name="macros">[Optional] Macros to pass to the shader.</param>
    /// <returns>A shader compilation result containing either the shader, or an error message.</returns>
    /// <exception cref="ArgumentEmptyException">Thrown if the <paramref name="sourceCode"/>, or <paramref name="entryPoint"/> parameters are empty.</exception>
    /// <exception cref="GorgonException"><para>
    /// Thrown if the <paramref name="shaderType"/> or <paramref name="shaderModel"/> are not supported.
    /// </para>
    /// <para>
    /// Thrown if a <c>#GorgonInclude</c> keyword in the <paramref name="sourceCode"/> is malformed.
    /// </para>
    /// <para>
    /// Thrown if the shader compiler had an internal error.
    /// </para>
    /// </exception>
    /// <exception cref="IOException">Thrown if the file specified by a <c>#GorgonInclude</c> keyword could not be found.</exception>
    /// <remarks>
    /// <para>
    /// This converts the source code for a shader, and turns it into an intermediate binary format via a <see cref="GorgonShader"/> for use with a <see cref="GorgonGraphicsPso"/>, 
    /// <see cref="GorgonComputePso"/> or other pipeline state object types.
    /// </para>
    /// <para>
    /// The <paramref name="entryPoint"/> is the name of the function to execute when the shader is running on the GPU.
    /// </para>
    /// <para>
    /// The <paramref name="shaderType"/> is the type of shader to generate. See the <see cref="ShaderType"/> enum for the possible values.
    /// </para>
    /// <para>
    /// The <paramref name="shaderModel"/> parameter should normally be left alone as it defaults to <see cref="ShaderModel.HighestVersionAvailable"/>. But if required, this parameter can force the compiler to 
    /// compile for older shader model versions.
    /// </para>
    /// <para>
    /// The method returns a <see cref="GorgonShaderCompileResult"/> composite type which contains either the <see cref="GorgonShader"/>, or, if there was an error, the error message(s) from the compiler. 
    /// Errors in the shader source code are returned in this result, and do not throw an exception.
    /// </para>
    /// <h3>Compilation flags</h3>
    /// <para>
    /// There are several compilation flags that can be passed to this method via its <paramref name="flags"/> parameter to enable/disable sets of functionality. The flags are typically OR'd together to allow 
    /// for multiple flags at once. However, if a <see cref="CompileFlags.Debug"/> is combined with an optimization flag, the <see cref="CompileFlags.Debug"/> flag takes precedence and the optimization flags 
    /// are dropped. The flags are as follows:
    /// <list type="bullet">
    ///     <item>
    ///         <term><see cref="CompileFlags.None"/></term>
    ///         <description>
    /// Default flags - Whatever values the compiler uses as default (except when running in <see cref="GorgonGraphics.IsInDebugMode">Debug mode</see>, then this defaults to <see cref="CompileFlags.Debug"/>).
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="CompileFlags.Debug"/></term>
    ///         <description>Compiles the shader with debug information to allow for debugging the shader in external applications like Microsoft PIX.</description>
    ///     </item>
    ///     <item>
    ///         <term>
    /// <see cref="CompileFlags.OptimizationLevel0"/>, <see cref="CompileFlags.OptimizationLevel1"/>, <see cref="CompileFlags.OptimizationLevel2"/>, <see cref="CompileFlags.OptimizationLevel3"/>
    ///         </term>
    ///         <description>
    /// Instructs the compiler to perform optimizations on the code while compiling. Where <see cref="CompileFlags.OptimizationLevel0"/> has no optimizations, but compiles very fast, and 
    /// <see cref="CompileFlags.OptimizationLevel3"/> applies the most extensive optimizations.
    ///         </description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="CompileFlags.WarningsAsErrors"/></term>
    ///         <description>Instructs the compiler to treat warnings as errors.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="CompileFlags.NoReflection"/></term>
    ///         <description>Instructs the compiler to strip out any reflection data from the shader binary.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="CompileFlags.RowMajor"/></term>
    ///         <description>Instructs the compiler to pack matrices in row major order.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="CompileFlags.StrictMode"/></term>
    ///         <description>Instruct the compiler to disallow legacy syntax.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="CompileFlags.AvoidFlowConstructs"/></term>
    ///         <description>Instructs the compiler to avoid using flow control as much as possible.</description>
    ///     </item>
    ///     <item>
    ///         <term><see cref="CompileFlags.IEEEStrictness"/></term>
    ///         <description>Instructs the compiler to use strict IEEE to avoid optimizations that may break IEEE rules.</description>
    ///     </item>
    /// </list>
    /// </para>
    /// <h3>Includes</h3>
    /// <para>
    /// The source code passed to the compiler is capable of including other source files as input, much like the C/C++ compilers. Using this allows for reuse of common code across multiple shader sources. 
    /// However, Gorgon has a special keyword that it uses to import these includes (either as a string in memory via the <see cref="Includes"/> property, or a file on disk): <c>#GorgonInclude "Name"</c>. The 
    /// <c>Name</c> is the key name of the include in the <see cref="Includes"/> property.
    /// </para>
    /// <para>
    /// The <c>#GorgonInclude</c> keyword has two parameters: a name, and an optional path. To have the keyword only look in the <see cref="Includes"/> property, just use <c>#GorgonInclude "Name"</c>. 
    /// Otherwise, applications can load external files into the <see cref="Includes"/> property by using <c>#GorgonInclude "Name", "Path\To\The\File.hlsl"</c>. If an include with the same name is already in 
    /// the <see cref="Includes"/> property, then the file is not loaded.
    /// </para>
    /// <para>
    /// <note type="warning">
    /// <para>
    /// These includes are merged in with the source file at compile time, changing locations of functions. Therefore, error message line number information may not be correct.
    /// </para>
    /// <para>
    /// Also, if using a string in memory as an include, ensure that the include is present in the <see cref="Includes"/> property, and has the same key name as the name in the <c>#GorgonInclude</c>. 
    /// Otherwise, the include is skipped.
    /// </para>
    /// </note>
    /// </para>
    /// <h3>Macros</h3>
    /// <para>
    /// This method also has the ability to pass in a list of <see cref="GorgonShaderMacro"/> values, which allows conditional compilation on a shader, which can be used to create a variety of shader functions 
    /// while still using the same code base.
    /// </para>
    /// </remarks>
    /// <seealso cref="GorgonShader"/>
    /// <seealso cref="GorgonShaderCompileResult"/>
    /// <seealso cref="Includes"/>
    /// <seealso cref="GorgonShaderInclude"/>
    /// <seealso cref="GorgonGraphicsPso"/>
    /// <seealso cref="GorgonComputePso"/>
    public GorgonShaderCompileResult Compile(string sourceCode, string entryPoint, ShaderType shaderType, ShaderModel shaderModel = ShaderModel.HighestVersionAvailable, CompileFlags flags = CompileFlags.None, IReadOnlyList<GorgonShaderMacro>? macros = null)
    {
        using ComPtr<IDxcResult> result = default;

        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(sourceCode);
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(entryPoint);

        if (shaderModel == ShaderModel.HighestVersionAvailable)
        {
            shaderModel = Graphics.Adapter.ShaderModelSupport;
        }

        if ((shaderModel == ShaderModel.Unsupported)
             || (shaderModel > Graphics.Adapter.ShaderModelSupport))
        {
            throw new GorgonException(GorgonResult.CannotCompile, string.Format(Resources.GORGFX_ERR_SHADER_MODEL_NOT_SUPPORTED, shaderModel));
        }

        if (!_shaderMatrix.TryGetValue(shaderModel, out Dictionary<ShaderType, ShaderProfiles>? shaderTypes))
        {
            throw new GorgonException(GorgonResult.CannotCompile, string.Format(Resources.GORGFX_ERR_SHADER_MODEL_NOT_SUPPORTED, shaderModel));
        }

        if (!shaderTypes.TryGetValue(shaderType, out ShaderProfiles? shaderModelProfile))
        {
            throw new GorgonException(GorgonResult.CannotCompile, string.Format(Resources.GORGFX_ERR_SHADER_TYPE_NOT_SUPPORTED, shaderType));
        }

        if (((flags & CompileFlags.Debug) == CompileFlags.Debug) && (((flags & CompileFlags.OptimizationLevel0) == CompileFlags.OptimizationLevel0)
            || ((flags & CompileFlags.OptimizationLevel1) == CompileFlags.OptimizationLevel1)
            || ((flags & CompileFlags.OptimizationLevel2) == CompileFlags.OptimizationLevel2)
            || ((flags & CompileFlags.OptimizationLevel3) == CompileFlags.OptimizationLevel3)))
        {
            Graphics.Log.PrintWarning($"Shader {shaderModelProfile.ShaderType} {shaderModelProfile.ShaderModel} has a debug flag, but also specifies an optimization flag. " +
                $"The debug flag will take precedence and optimizations will be disabled.", LoggingLevel.Intermediate);

            flags &= ~(CompileFlags.OptimizationLevel0 | CompileFlags.OptimizationLevel1 | CompileFlags.OptimizationLevel2 | CompileFlags.OptimizationLevel3);
        }

        // If we're in debug mode, then compile shaders with debug info as well.
        // This way we're not surprised when we run our app in debug and have shaders 
        // that aren't giving us debugging data.
        if ((flags == CompileFlags.None) && (Graphics.IsInDebugMode))
        {
            Graphics.Log.PrintWarning("Debugging is enabled for Gorgon. No compilation flags were specified, so shaders will be compiled with debug information.",
                LoggingLevel.Intermediate);
            flags = CompileFlags.Debug;
        }

        Graphics.Log.Print($"Creating a {shaderModelProfile.ShaderType} ({shaderModelProfile.ShaderModel}).", LoggingLevel.Simple);

        using ComPtr<IDxcBlobEncoding> srcBlob = GetShaderSourceBlob(sourceCode, shaderModelProfile, out DxcBuffer buffer);
        char** cstrs = GetCompilerArguments(entryPoint, shaderModelProfile.Profile, macros ?? [], flags, out uint argCount);

        try
        {
            _compiler.Get()->Compile(&buffer, cstrs, argCount, _includeHandler.Get(), Win32.__uuidof<IDxcResult>(), (void**)result.GetAddressOf())
                .ThrowIfFailed(GorgonResult.CannotCompile, () => string.Format(Resources.GORGFX_ERR_CANNOT_COMPILE_SHADER, shaderModelProfile));
        }
        finally
        {
            FreeCompilerArguments(cstrs, argCount);
        }

        string compilerErrors = GetCompilationErrors(result);

        HRESULT compileErr;
        result.Get()->GetStatus(&compileErr)
            .ThrowIfFailed(GorgonResult.CannotCompile, () => string.Format(Resources.GORGFX_ERR_CANNOT_COMPILE_SHADER, shaderModelProfile));

        if (compileErr.FAILED)
        {
            Graphics.Log.PrintError($"Compilation errors for the shader:\n{compilerErrors}", LoggingLevel.Simple);
            return new GorgonShaderCompileResult(compilerErrors);
        }
        else if (!string.IsNullOrWhiteSpace(compilerErrors))
        {
            Graphics.Log.PrintWarning($"Compilation warnings for the shader:\n{compilerErrors}", LoggingLevel.Simple);
        }

        using ComPtr<ID3DBlob> shaderBlob = default;

        Graphics.Log.Print($"Creating a D3D blob containing the shader data.", LoggingLevel.Verbose);

        result.Get()->GetResult((IDxcBlob**)shaderBlob.GetAddressOf())
            .ThrowIfFailed(GorgonResult.CannotCompile, () => string.Format(Resources.GORGFX_ERR_CANNOT_COMPILE_SHADER, shaderModelProfile));

        byte[] hash = [];
        byte[] pdbData = [];
        byte[] reflectionData = [];
        string pdbName = string.Empty;
        HRESULT err;

        if (result.Get()->HasOutput(DXC_OUT_KIND.DXC_OUT_SHADER_HASH))
        {
            using ComPtr<ID3DBlob> hashBlob = default;

            err = result.Get()->GetOutput(DXC_OUT_KIND.DXC_OUT_SHADER_HASH, Win32.__uuidof<ID3DBlob>(), (void**)hashBlob.GetAddressOf(), null);

            if (err.FAILED)
            {
                Graphics.Log.PrintError(err, "There was an error retrieving the shader hash.", LoggingLevel.Verbose);
            }
            else
            {                
                GorgonPtr<byte> hashDigest = hashBlob.ToGorgonPtr<byte>();
                hash = new byte[hashDigest.Length];
                hashDigest.CopyTo(hash);
            }
        }

        if (((flags & CompileFlags.Debug) == CompileFlags.Debug) && (result.Get()->HasOutput(DXC_OUT_KIND.DXC_OUT_PDB)))
        {
            using ComPtr<ID3DBlob> pdbBlob = default;
            using ComPtr<IDxcBlobUtf16> blobName = default;

            err = result.Get()->GetOutput(DXC_OUT_KIND.DXC_OUT_PDB, Win32.__uuidof<ID3DBlob>(), (void**)pdbBlob.GetAddressOf(), blobName.ReleaseAndGetAddressOf());

            if (!blobName.IsNull)
            {
                pdbName = new(blobName.Get()->GetStringPointer(), 0, (int)blobName.Get()->GetStringLength());
            }
            
            if (err.FAILED)
            {
                Graphics.Log.PrintError(err, "There was an error retrieving the PDB information for the shader.", LoggingLevel.Verbose);
            }
            else
            {
                GorgonPtr<byte> pdbPtr = pdbBlob.ToGorgonPtr<byte>();
                pdbData = new byte[pdbPtr.Length];
                pdbPtr.CopyTo(pdbData);
            }
        }

        if (((flags & CompileFlags.NoReflection) != CompileFlags.NoReflection) && (result.Get()->HasOutput(DXC_OUT_KIND.DXC_OUT_REFLECTION)))
        {
            using ComPtr<ID3DBlob> reflectBlob = default;

            err = result.Get()->GetOutput(DXC_OUT_KIND.DXC_OUT_REFLECTION, Win32.__uuidof<ID3DBlob>(), (void**)reflectBlob.GetAddressOf(), null);

            if (err.FAILED)
            {
                Graphics.Log.PrintError(err, "There was an error retrieving the reflection information for the shader.", LoggingLevel.Verbose);
            }
            else
            {
                GorgonPtr<byte> reflectPtr = reflectBlob.ToGorgonPtr<byte>();
                reflectionData = new byte[reflectPtr.Length];
                reflectPtr.CopyTo(reflectionData);
            }
        }

        GorgonPtr<byte> shaderPtr = shaderBlob.ToGorgonPtr<byte>();
        byte[] shaderData = new byte[(long)shaderBlob.Get()->GetBufferSize()];
        shaderPtr.CopyTo(shaderData);

        return new GorgonShaderCompileResult(new GorgonShader(Graphics, shaderData, hash, pdbData, pdbName, reflectionData, shaderType, shaderModelProfile.ShaderModel));
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
    ~GorgonShaderCompiler() => Dispose(false);

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonShaderCompiler"/> class.
    /// </summary>
    /// <param name="graphics">The graphics object that is associated with this compiler.</param>
    public GorgonShaderCompiler(GorgonGraphics graphics)
    {
        Graphics = graphics;
        BuildNativeObjects();

        this.RegisterDisposable(graphics);
    }
}