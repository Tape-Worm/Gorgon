
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
// Created: July 20, 2016 11:17:52 PM
// 

using Gorgon.Core;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A named include used by the <c>#GorgonInclude</c> keyword in shader source code.
/// </summary>
/// <param name="Name">The name used to look up the include.</param>
/// <param name="SourceCodeFile">The source code for the include.</param>
/// <remarks>
/// <para>
/// The HLSL <c>#include</c> directive locates include files on the file system. This does not work when the shader source was not loaded from a file (e.g. from a <see cref="Stream"/>), because there is no 
/// path to search. To facilitate this, Gorgon provides a <c>#GorgonInclude</c> keyword that looks up the include by name in the <see cref="GorgonShaderCompiler.Includes"/> property <i>before</i> it tries to 
/// load a file.
/// </para>
/// <para>
/// To include source code from memory, add a <see cref="GorgonShaderInclude"/> containing the source code with the <see cref="GorgonShaderCompiler.AddInclude(GorgonShaderInclude)"/> method, and use its
/// name in the <c>#GorgonInclude</c> keyword. When the include is loaded from a file, then its source code will automatically be added to the <see cref="GorgonShaderCompiler.Includes"/> list.
/// </para>
/// <para>
/// The <c>#GorgonInclude</c> keyword is written as <c>#GorgonInclude "Name"</c>, or <c>#GorgonInclude "Name", "Path"</c>. The parameters are:
/// <list type="bullet">
/// <item>
/// <term>Name</term>
/// <description>
/// The name of the include. This is the key for the include in the <see cref="GorgonShaderCompiler.Includes"/> property, and is not case sensitive.
/// </description>
/// </item>
/// <item>
/// <term>(Optional) Path</term>
/// <description>
/// The path to the shader source file to include. This is ignored if an include with the same name is already in the <see cref="GorgonShaderCompiler.Includes"/> property.
/// </description>
/// </item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="GorgonShaderCompiler"/>
public readonly record struct GorgonShaderInclude(string Name, string SourceCodeFile)
    : IGorgonNamedObject
{
    /// <summary>
    /// Property to return the name of the include file.
    /// </summary>
    string IGorgonNamedObject.Name => Name;
}
