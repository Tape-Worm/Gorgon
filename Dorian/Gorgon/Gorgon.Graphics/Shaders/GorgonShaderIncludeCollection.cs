#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Sunday, March 18, 2012 11:32:16 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using GorgonLibrary.Collections;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Graphics.Properties;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// A collection of shader include files.
	/// </summary>
	public class GorgonShaderIncludeCollection
		: GorgonBaseNamedObjectDictionary<GorgonShaderInclude>
	{
		#region Properties.
		/// <summary>
		/// Property to set or return an include file in the collection.
		/// </summary>
		public GorgonShaderInclude this[string includeName]
		{
			get
			{
				return GetItem(includeName);
			}
			set
			{
			    if (includeName == null)
			    {
			        return;
			    }

			    if (string.IsNullOrWhiteSpace(includeName))
			    {
			        return;
			    }

			    if (!Contains(value.Name))
				{
					AddItem(value);
					return;
				}

				SetItem(value.Name, value);
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to retrieve the include line.
		/// </summary>
		/// <param name="includeLine">Include line.</param>
		/// <param name="checkFileExists">TRUE to check if the file exists, FALSE to skip the check.</param>
		/// <returns>A path to the include file.</returns>
		private static GorgonShaderInclude ParseIncludeLine(string includeLine, bool checkFileExists)
		{
			string originalLine = includeLine;

			includeLine = includeLine.Substring(14).Trim();

			if (string.IsNullOrEmpty(includeLine))
			{
			    throw new GorgonException(GorgonResult.CannotRead,
			        string.Format(Resources.GORGFX_SHADER_INCLUDE_PATH_INVALID, originalLine));
			}

			// Get include files.
			int endQuote = 0;
			string includePath;

			if ((!includeLine.StartsWith("\"")) || (!includeLine.EndsWith("\"")))
			{
			    throw new GorgonException(GorgonResult.CannotRead,
			        string.Format(Resources.GORGFX_SHADER_INCLUDE_PATH_INVALID, originalLine));
			}

			// Get the include name.
			for (int c = 1; c < includeLine.Length; c++)
			{
				if (includeLine[c] != '\"')
				{
					continue;
				}

				endQuote = c;
				break;
			}

			if (endQuote == 0)
			{
                throw new GorgonException(GorgonResult.CannotRead,
                    string.Format(Resources.GORGFX_SHADER_INCLUDE_PATH_INVALID, originalLine));
            }

			string includeName = includeLine.Substring(1, endQuote - 1);
			includeLine = includeLine.Substring(endQuote + 1).Trim();

			if (includeLine.StartsWith(","))
			{
				includeLine = includeLine.Substring(1).Trim();

				if (!includeLine.StartsWith("\""))
				{
				    throw new GorgonException(GorgonResult.CannotRead,
				        string.Format(Resources.GORGFX_SHADER_INCLUDE_PATH_INVALID, originalLine));
				}

				endQuote = includeLine.Length - 1;

				includePath = Path.GetFullPath(includeLine.Substring(1, endQuote - 1));

				if (endQuote + 1 <= includeLine.Length)
				{
					includeLine = includeLine.Substring(endQuote + 1);
				}

				if (!string.IsNullOrEmpty(includeLine))
				{
                    throw new GorgonException(GorgonResult.CannotRead,
                        string.Format(Resources.GORGFX_SHADER_INCLUDE_PATH_INVALID, originalLine));
				}

				if ((checkFileExists) && (!File.Exists(includePath)))
				{
					throw new IOException(string.Format(Resources.GORGFX_FILE_NOT_FOUND, originalLine));
				}
			}
			else
			{
				includePath = string.Empty;
			}

			return new GorgonShaderInclude(includeName, includePath);
		}

		/// <summary>
		/// Function to retrieve a list of include files from source code.
		/// </summary>
		/// <param name="searchPath">A base path to start searching from.</param>
		/// <param name="sourceCode">Source code to examine.</param>
		/// <returns>A list of include files.</returns>
		private IList<GorgonShaderInclude> GetIncludes(string searchPath, string sourceCode)
		{
			var paths = new List<GorgonShaderInclude>();
			IList<string> lines = sourceCode.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			Directory.SetCurrentDirectory(searchPath);

		    var includeLines = (from line in lines
		                        select line.Trim()
		                        into includeLine
		                        where includeLine.StartsWith("#GorgonInclude", StringComparison.OrdinalIgnoreCase)
		                        select ParseIncludeLine(includeLine, true));

			foreach (GorgonShaderInclude includeFile in includeLines)
			{
			    // If we already have this include, then parse it and continue on.
			    if (!Contains(includeFile.Name))
			    {
			        // Otherwise, we need to load it.
			        if (string.IsNullOrEmpty(includeFile.SourceCodeFile))
			        {
			            throw new IOException(string.Format(Resources.GORGFX_FILE_NOT_FOUND, includeFile.Name));
			        }

			        sourceCode = File.ReadAllText(includeFile.SourceCodeFile);
			        paths.AddRange(
			            GetIncludes(Path.GetDirectoryName(includeFile.SourceCodeFile) + Path.DirectorySeparatorChar,
			                sourceCode));

			        if (!Contains(includeFile.Name))
			        {
			            paths.Add(new GorgonShaderInclude(includeFile.Name, sourceCode));
			        }
			    }
			    else
			    {
			        paths.AddRange(GetIncludes(searchPath, this[includeFile.Name].SourceCodeFile));
			    }
			}

			return paths;
		}

		/// <summary>
		/// Adds the item.
		/// </summary>
		/// <param name="value">The value.</param>
		protected override void AddItem(GorgonShaderInclude value)
		{
			GorgonDebug.AssertParamString(value.Name, "includeFile.Name");
			GorgonDebug.AssertParamString(value.SourceCodeFile, "includeFile.SourceCode");

			base.AddItem(value);
		}

		/// <summary>
		/// Function to load include files referenced by a file.
		/// </summary>
		/// <param name="shaderFileName">File name and path to the shader file to examine.</param>
		/// <remarks>This method will recursively look for any included files within the file.  
		/// <para>The lookup parameter is the full (absolute) path to the shader include file.  If this method encounters a file that has already been loaded, then it will skip that file.</para>
		/// </remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="shaderFileName"/> parameters is NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the shaderFileName parameter is empty.</exception>
		public void FromFile(string shaderFileName)
		{
			GorgonDebug.AssertParamString(shaderFileName, "shaderFileName");

			string currentDirectory = string.Empty;
		    IList<GorgonShaderInclude> includeFiles;

			shaderFileName = Path.GetFullPath(shaderFileName);
			string path = Path.GetDirectoryName(shaderFileName) + Path.DirectorySeparatorChar;
			path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			string fileName = Path.GetFileName(shaderFileName);

			// Load in the shader file for processing.
			string sourceCode = File.ReadAllText(path + fileName);

			try
			{
				currentDirectory = Directory.GetCurrentDirectory();
				includeFiles = GetIncludes(path, sourceCode);
			}
			finally
			{
				Directory.SetCurrentDirectory(currentDirectory);
			}

			AddRange(includeFiles);
		}

		/// <summary>
		/// Function to process the source code and set up any includes.
		/// </summary>
		/// <param name="sourceCode">Code to process.</param>
		/// <returns>The processed source.</returns>
		internal string ProcessSource(string sourceCode)
		{
			var result = new StringBuilder();
		    IList<string> lines = sourceCode.Replace("\r\n", "\n").Replace("\n\r", "\n").Split(new[]
		    {
		        '\n'
		    });
			int i = 0;

			while (i < lines.Count)
			{
				string includeLine = lines[i].Trim();

				if (includeLine.StartsWith("#GorgonInclude", StringComparison.CurrentCulture))
				{
					GorgonShaderInclude includeFile = ParseIncludeLine(includeLine, false);

				    if (!Contains(includeFile.Name))
				    {
				        throw new KeyNotFoundException(string.Format(Resources.GORGFX_SHADER_INCLUDE_NOT_FOUND, includeLine));
				    }

				    result.AppendFormat("// ------------------ Begin #include of '{0}' ------------------ \r\n", includeFile.Name);
					result.AppendFormat("{0}\r\n", ProcessSource(this[includeFile.Name].SourceCodeFile));
                    result.AppendFormat("// ------------------ End #include of '{0}'------------------ \r\n\r\n", includeFile.Name);
				}
				else
				{
					result.AppendFormat("{0}\r\n", lines[i]);
				}
				i++;
			}

			return result.ToString();
		}

		
		/// <summary>
		/// Function to add a list of shader includes to the collection.
		/// </summary>
		/// <param name="includes">Include files to add.</param>
		public void AddRange(IEnumerable<GorgonShaderInclude> includes)
		{
			AddItems(includes);
		}

		/// <summary>
		/// Function to add a new include file to the collection.
		/// </summary>
		/// <param name="includeFile">Include file to add.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="includeFile"/> parameter contains a NULL (Nothing in VB.Net) file name or NULL source code.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the includeFile parameter has an empty file name or source code.
		/// <para>-or-</para>
		/// <para>Thrown when the include file name already exists in this collection.</para>
		/// </exception>
		public void Add(GorgonShaderInclude includeFile)
		{
			AddItem(includeFile);
		}

		/// <summary>
		/// Function to add a new include file to the collection.
		/// </summary>
		/// <param name="includeName">Filename for the include file.</param>
		/// <param name="includeSourceCode">Source code for the include file.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="includeName"/> or the <paramref name="includeSourceCode"/> parameters are NULL (Nothing in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the includeFileName or the includeSource parameters are empty.
		/// <para>-or-</para>
		/// <para>Thrown when the includeFileName already exists in this collection.</para>
		/// </exception>
		public void Add(string includeName, string includeSourceCode)
		{
			AddItem(new GorgonShaderInclude(includeName, includeSourceCode));
		}

		/// <summary>
		/// Function to remove an include file from the collection.
		/// </summary>
		/// <param name="includeName">File name of the include file to remove.</param>
		public void Remove(string includeName)
		{
			RemoveItem(includeName);
		}

		/// <summary>
		/// Function to remove an include file from the collection.
		/// </summary>
		/// <param name="include">Include file to remove.</param>
		public void Remove(GorgonShaderInclude include)
		{
			RemoveItem(include.Name);
		}

		/// <summary>
		/// Function to clear the collection.
		/// </summary>
		public void Clear()
		{
			ClearItems();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonShaderIncludeCollection"/> class.
		/// </summary>
		internal GorgonShaderIncludeCollection()
			: base(false)
		{
		}
		#endregion
	}
}
