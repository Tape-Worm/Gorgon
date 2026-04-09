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
// Created: September 29, 2025 12:59:13 PM
//

using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;

namespace Gorgon.Graphics.Core;

/// <summary>
/// The result of a shader compilation.
/// </summary>
/// <remarks>
/// <para>
/// This is similar to a functional Either type. Applications can take this result value, and implicitly convert it to a <see cref="GorgonShader"/> type if the <see cref="Success"/> flag returns 
/// <b>true</b>. Otherwise, the type can be converted implicitly to a string containing the compilation errors, or a <see cref="GorgonException"/> type containing the error information.
/// </para>
/// <para>
/// <note type="information">
/// <para>
/// An exception will be raised if the result is implicitly converted to a <see cref="GorgonShader"/> result. This exception will be of the <see cref="GorgonException"/> type, and will contain the 
/// error messages from the shader compiler.
/// </para>
/// </note>
/// </para>
/// </remarks>
/// <example>
/// The following example shows multiple ways to consume the result type:
/// <code language="csharp">
/// <![CDATA[
/// GorgonShaderCompileResult result = _shaderFactory.Compile(...);
/// 
/// // We can test for an error like this:
/// _shader = result.Success switch 
/// {
///   true => result,
///   false => Debug.Print($"The shader died horribly. Compile errors: {result}")
/// };
/// 
/// // Or we can throw an exception:
/// _shader = result.Success switch 
/// {
///   true => result,
///   false => throw result;
/// };
/// 
/// // Or we can do this. This works because it will convert to GorgonShader if no error occured, and will throw an exception if one did.
/// _shader = _shaderFactory.Compile(...);
/// // Or
/// // _shader = result;
/// ]]>
/// </code>
/// </example>
/// <seealso cref="GorgonShader"/>
/// <seealso cref="GorgonException"/>
public readonly record struct GorgonShaderCompileResult
{
    private readonly GorgonShader? _shader;
    private readonly string _message;

    /// <summary>
    /// Property to return a flag to indicate whether compilation was successful or not.
    /// </summary>    
    public readonly bool Success => _shader is not null;

    /// <summary>
    /// Operator to convert the result to a <see cref="GorgonShader"/> type.
    /// </summary>
    /// <param name="r">The shader compile result.</param>
    public static implicit operator GorgonShader(GorgonShaderCompileResult r) => r._shader ?? throw new GorgonException(GorgonResult.CannotCompile, string.Format(Resources.GORGFX_ERR_SHADER_COMPILE_ERROR, r._message));

    /// <summary>
    /// Operator to convert the result to an error message.
    /// </summary>
    /// <param name="r">Compiler result.</param>
    public static implicit operator string(GorgonShaderCompileResult r) => r._message;

    /// <summary>
    /// Function to retrieve an exception for the compilation errors.
    /// </summary>
    /// <returns>An exception for the compilation errors if not successful; <b>null</b> if successful.</returns>        
    /// <exception cref="InvalidCastException">Thrown if the compilaation result was <see cref="Success">successful</see> and no errors were generated.</exception>
    public Exception GetCompilationException()
    {
        if (Success)
        {
            throw new InvalidCastException();
        }

        return new GorgonException(GorgonResult.CannotCompile, string.Format(Resources.GORGFX_ERR_SHADER_COMPILE_ERROR, _message));
    }

    /// <inheritdoc/>
    public override string ToString() => string.IsNullOrWhiteSpace(_message) ? nameof(GorgonShaderCompileResult) : _message;

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonShaderCompileResult"/> value type.
    /// </summary>
    /// <param name="shader">The compiled shader.</param>
    public GorgonShaderCompileResult(GorgonShader shader)
    {
        _shader = shader;
        _message = string.Empty;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonShaderCompileResult"/> value type.
    /// </summary>
    /// <param name="error">Error message that was presented when compilation failed.</param>
    public GorgonShaderCompileResult(string error)
    {
        _shader = null;
        _message = error;
    }
}
