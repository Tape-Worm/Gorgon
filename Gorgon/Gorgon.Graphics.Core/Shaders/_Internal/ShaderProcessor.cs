
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: July 7, 2016 11:53:48 PM
// 


using System.Text;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;

namespace Gorgon.Graphics.Core;

/// <summary>
/// A processor used to analyze shader source code and inject any special #GorgonInclude directives
/// </summary>
internal class ShaderProcessor
{

    /// <summary>
    /// Property to return the list of cached include files.
    /// </summary>
    public Dictionary<string, GorgonShaderInclude> CachedIncludes
    {
        get;
    }



    /// <summary>
    /// Function to trim whitespace from the beginning and end of a line.
    /// </summary>
    /// <param name="line">The line to process.</param>
    private static void TrimLine(StringBuilder line)
    {
        if (line.Length == 0)
        {
            return;
        }

        while ((line.Length > 0) && (char.IsWhiteSpace(line[0])))
        {
            line.Remove(0, 1);
        }

        while ((line.Length > 0) && (char.IsWhiteSpace(line[^1])))
        {
            line.Remove(line.Length - 1, 1);
        }
    }

    /// <summary>
    /// Function to retrieve the name of the include and its path.
    /// </summary>
    /// <param name="includeLine">The line that contains the include file.</param>
    /// <param name="checkFileExists"><b>true</b> to check if the file exists, <b>false</b> to skip the check.</param>
    /// <returns>A new include object.</returns>
    private static GorgonShaderInclude ParseIncludeLine(StringBuilder includeLine, bool checkFileExists)
    {
        int length = "#GorgonInclude".Length;
        var line = new StringBuilder(includeLine.ToString(), length, includeLine.Length - length, includeLine.Length - length);

        TrimLine(line);

        if (line.Length == 0)
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_SHADER_INCLUDE_PATH_INVALID, includeLine));
        }

        // Get include files.
        int endQuote = 0;

        if ((line[0] != '"') || (line[^1] != '"'))
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_SHADER_INCLUDE_PATH_INVALID, includeLine));
        }

        // Get the include name.
        for (int c = 1; c < line.Length; c++)
        {
            if (line[c] != '\"')
            {
                continue;
            }

            endQuote = c;
            break;
        }

        if (endQuote == 0)
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_SHADER_INCLUDE_PATH_INVALID, includeLine));
        }

        // Get the name of the include file to store in our internal list.
        string includeName = line.ToString(1, endQuote - 1);

        // Get rid of the name, but only if there's more text after the end quote.
        if (endQuote + 1 < line.Length)
        {
            line.Remove(0, endQuote + 1);
            TrimLine(line);
        }

        // Check for a file path.
        if (line[0] != ',')
        {
            return new GorgonShaderInclude(includeName, string.Empty);
        }

        line.Remove(0, 1);
        TrimLine(line);

        if (line[0] != '"')
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_SHADER_INCLUDE_PATH_INVALID, includeLine));
        }

        // We've trimmed the line long before now, if we don't have a quote at the end of the line, then we'll get a file load error.
        endQuote = line.Length - 1;

        string includePath = Path.GetFullPath(line.ToString(1, endQuote - 1));

#pragma warning disable IDE0046 // Convert to conditional expression
        if (includePath.Length == 0)
        {
            throw new GorgonException(GorgonResult.CannotRead, string.Format(Resources.GORGFX_ERR_SHADER_INCLUDE_PATH_INVALID, includeLine));
        }

        return (checkFileExists) && (!File.Exists(includePath))
            ? throw new IOException(string.Format(Resources.GORGFX_ERR_FILE_NOT_FOUND, includeName))
            : new GorgonShaderInclude(includeName, includePath);
#pragma warning restore IDE0046 // Convert to conditional expression
    }

    /// <summary>
    /// Function to process the shader source.
    /// </summary>
    /// <param name="sourceCode">The source code to process.</param>
    /// <returns>The processed shader source.</returns>
    public string Process(string sourceCode)
    {
        var result = new StringBuilder();

        // Replace carriage returns with new lines.
        var code = new StringBuilder(sourceCode);
        code.Replace("\r\n", "\n");
        code.Replace("\n\r", "\n");
        code.Replace("\r", "\n");

        IList<string> lines = code.ToString().Split('\n');
        int i = 0;

        var includeLine = new StringBuilder();

        while (i < lines.Count)
        {
            includeLine.Length = 0;
            includeLine.Append(lines[i]);
            TrimLine(includeLine);

            if (includeLine.IndexOf("#GorgonInclude", comparison: StringComparison.OrdinalIgnoreCase) != 0)
            {
                result.Append($"{lines[i++]}\r\n");
                continue;
            }

            ++i;
            GorgonShaderInclude includeFile = ParseIncludeLine(includeLine, false);

            // If we have no file name, then assume we've already included it in the collection.
            if (CachedIncludes.TryGetValue(includeFile.Name, out GorgonShaderInclude cached))
            {
                result.Append($"// ------------------ Begin #include of '{includeFile.Name}' ------------------ \r\n");
                result.Append($"{Process(cached.SourceCodeFile)}\r\n");
                result.Append($"// ------------------ End #include of '{includeFile.Name}'------------------ \r\n\r\n");
                continue;
            }

            // We've got nothing to load, so skip this.
            if (string.IsNullOrWhiteSpace(includeFile.SourceCodeFile))
            {
                continue;
            }

            if (!File.Exists(includeFile.SourceCodeFile))
            {
                throw new IOException(string.Format(Resources.GORGFX_ERR_SHADER_INCLUDE_NOT_FOUND, includeLine));
            }

            string includeSourceCode = File.ReadAllText(includeFile.SourceCodeFile);

            if (string.IsNullOrWhiteSpace(includeSourceCode))
            {
                continue;
            }

            result.Append($"// ------------------ Begin #include of external include '{includeFile.SourceCodeFile}' ------------------ \r\n");
            result.Append($"{Process(includeSourceCode)}\r\n");
            result.Append($"// ------------------ End #include of extneral include '{includeFile.SourceCodeFile}'------------------ \r\n\r\n");

            // Add to the include list.
            CachedIncludes[includeFile.Name] = new GorgonShaderInclude(includeFile.Name, includeSourceCode);
        }

        return result.ToString();
    }



    /// <summary>
    /// Initializes a new instance of the <see cref="ShaderProcessor"/> class.
    /// </summary>
    public ShaderProcessor() => CachedIncludes = [];

}
