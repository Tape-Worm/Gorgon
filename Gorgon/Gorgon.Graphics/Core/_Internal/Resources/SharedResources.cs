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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: June 24, 2026 11:37:34 PM
//

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides common resources used by various pieces of functionality in Gorgon.
/// </summary>
internal sealed class SharedResources
    : IDisposable
{
    /// <summary>
    /// Property to return the global shader compiler for building shaders.
    /// </summary>
    public GorgonShaderCompiler ShaderCompiler
    {
        get;
    }

    /// <summary>
    /// Property to return the vertex shader for the blitter command.
    /// </summary>
    public GorgonShader BlitterVertexShader
    {
        get;
    }

    /// <summary>
    /// Property to return the pixel shader for the blitter command.
    /// </summary>
    public GorgonShader BlitterPixelShader
    {
        get;
    }

    /// <summary>
    /// Property to return the vertex shader for the full screen blitter command.
    /// </summary>
    public GorgonShader FullScreenBlitterVertexShader
    {
        get;
    }

    /// <summary>
    /// Property to return the pixel shader for the full screen blitter command.
    /// </summary>
    public GorgonShader FullScreenBlitterPixelShader
    {
        get;
    }

    /// <inheritdoc cref="GorgonGraphicsFactory.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            ShaderCompiler.Dispose();
        }
    }

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SharedResources"/> class.
    /// </summary>
    /// <param name="graphics">The graphics interface that owns this instance.</param>
    public SharedResources(GorgonGraphics graphics)
    {
        ShaderCompiler = new GorgonShaderCompiler(graphics);
        ShaderCompiler.AddInclude(new GorgonShaderInclude(GorgonTextureBlitter.GorgonTextureBlitterShadersName, GorgonTextureBlitter.GorgonTextureBlitterShader));

        CompileFlags flags = graphics.IsInDebugMode ? CompileFlags.Debug : CompileFlags.OptimizationLevel3;
        
        BlitterVertexShader = ShaderCompiler.Compile(GorgonTextureBlitter.GorgonTextureBlitterShader, "GorgonBlitterVS", ShaderType.VertexShader, flags: flags);
        BlitterPixelShader = ShaderCompiler.Compile(GorgonTextureBlitter.GorgonTextureBlitterShader, "GorgonBlitterPS", ShaderType.PixelShader, flags: flags);
        FullScreenBlitterVertexShader = ShaderCompiler.Compile(GorgonTextureBlitter.GorgonTextureBlitterShader, "GorgonFullScreenVertexShader", ShaderType.VertexShader, flags: flags);
        FullScreenBlitterPixelShader = ShaderCompiler.Compile(GorgonTextureBlitter.GorgonTextureBlitterShader, "GorgonFullScreenPixelShader", ShaderType.PixelShader, flags: flags);
    }
}
