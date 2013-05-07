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
using System.Text;
using GorgonLibrary.Collections;
using GorgonLibrary.Diagnostics;

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
				if (string.IsNullOrEmpty(value.Name))
					throw new ArgumentException("File name cannot be NULL or empty.");

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
		/// Function to find the start and end to the include block
		/// </summary>
		/// <param name="startLine">Starting line.</param>
		/// <param name="lines">Lines to scan.</param>
		/// <returns>The start and end index of the include block.</returns>
		private Tuple<int, int> FindIncludeBlock(int startLine, IList<string> lines)
		{
			string currentLine = string.Empty;
			int start = startLine;
			int startBlock = -1;
			int endBlock = -1;

			// Find brackets.
			for (int i = start; i < lines.Count; i++)
			{
				currentLine = lines[i].Trim();
				if ((currentLine.StartsWith("#GorgonInclude", StringComparison.CurrentCultureIgnoreCase)) && (currentLine.Length >= 14))
					currentLine = currentLine.Substring(14).Trim();

				if ((string.IsNullOrEmpty(currentLine)) || (currentLine == "\r") || (currentLine == "\n"))
					continue;

				if (currentLine.StartsWith("{"))
					startBlock = i;

				if (currentLine.EndsWith("}"))
					endBlock = i;

				if ((!currentLine.StartsWith("\"")) && (startBlock == -1))
					throw new GorgonException(GorgonResult.CannotEnumerate, "Cannot read the file.  The line '" + lines[startLine] + "' has no starting bracket.");

				if ((startBlock > -1) && (endBlock > -1))
					break;
			}

			if (endBlock == -1)
				throw new GorgonException(GorgonResult.CannotEnumerate, "Cannot read the file.  The line '" + lines[startLine] + "' has no ending bracket.");

			return new Tuple<int, int>(startBlock, endBlock);
		}

		/// <summary>
		/// Function to retrieve the include line.
		/// </summary>
		/// <param name="includeLine">Include line.</param>
		/// <param name="checkFileExists">TRUE to check if the file exists, FALSE to skip the check.</param>
		/// <returns>A path to the include file.</returns>
		private GorgonShaderInclude ParseIncludeLine(string includeLine, bool checkFileExists)
		{
			string originalLine = includeLine;

			includeLine = includeLine.Substring(14).Trim();

			if (string.IsNullOrEmpty(includeLine))
				throw new GorgonException(GorgonResult.CannotRead, "Cannot read the shader file.  The include line has no parameters.");
			
			// Get include files.
			int startQuote = 0;
			int endQuote = 0;
			string includeName = string.Empty;
			string includePath = string.Empty;

			if (!includeLine.StartsWith("\""))
				throw new GorgonException(GorgonResult.CannotRead, "Cannot read the file.  Include name is not enclosed in quotes.");

			if (!includeLine.EndsWith("\""))
				throw new GorgonException(GorgonResult.CannotRead, "Cannot read the file.  Include path is not enclosed in quotes.");

			// Get the include name.
			for (int c = 1; c < includeLine.Length; c++)
			{
				if (includeLine[c] == '\"')
				{
					endQuote = c;
					break;
				}
			}

			if (endQuote == 0)
				throw new GorgonException(GorgonResult.CannotRead, "Cannot read the file.  Include name is not enclosed in quotes.");

			includeName = includeLine.Substring(1, endQuote - 1);
			includeLine = includeLine.Substring(endQuote + 1).Trim();

			if (includeLine.StartsWith(","))
			{
				includeLine = includeLine.Substring(1).Trim();

				if (!includeLine.StartsWith("\""))
					throw new GorgonException(GorgonResult.CannotRead, "Cannot read the file.  Include path is not enclosed in quotes.");

				endQuote = includeLine.Length - 1;

				includePath = Path.GetFullPath(includeLine.Substring(startQuote + 1, endQuote - (startQuote + 1)));

				if (endQuote + 1 <= includeLine.Length)
					includeLine = includeLine.Substring(endQuote + 1);

				if (!string.IsNullOrEmpty(includeLine))
					throw new GorgonException(GorgonResult.CannotRead, "Cannot read the file.  '" + originalLine + "' has invalid information.");

				if ((checkFileExists) && (!File.Exists(includePath)))
					throw new IOException("The include file '" + includePath + "' was not found.");
			}
			else
				includePath = string.Empty;

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
			IList<string> lines = sourceCode.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);

			Directory.SetCurrentDirectory(searchPath);

			for (int i = 0; i < lines.Count; i++)
			{
				string includeLine = lines[i].Trim();

				if (includeLine.StartsWith("#GorgonInclude", StringComparison.CurrentCultureIgnoreCase))
				{
					GorgonShaderInclude includeFile = ParseIncludeLine(includeLine, true);

					// If we already have this include, then parse it and continue on.
					if (Contains(includeFile.Name))
						paths.AddRange(GetIncludes(searchPath, this[includeFile.Name].SourceCode));
					else
					{
						// Otherwise, we need to load it.
						if (string.IsNullOrEmpty(includeFile.SourceCode))
							throw new GorgonException(GorgonResult.CannotRead, "Cannot read the file.  The include '" + includeFile.Name + "' was not found and has no path.");

						sourceCode = File.ReadAllText(includeFile.SourceCode);
						paths.AddRange(GetIncludes(Path.GetDirectoryName(includeFile.SourceCode) + Path.DirectorySeparatorChar.ToString(), sourceCode));

						if (!Contains(includeFile.Name))
							paths.Add(new GorgonShaderInclude(includeFile.Name, sourceCode));
					}
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
			GorgonDebug.AssertParamString(value.SourceCode, "includeFile.SourceCode");

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
			string sourceCode = string.Empty;
			string fileName = string.Empty;
			string path = string.Empty;
			IList<GorgonShaderInclude> includeFiles = null;

			shaderFileName = Path.GetFullPath(shaderFileName);
			path = Path.GetDirectoryName(shaderFileName) + Path.DirectorySeparatorChar.ToString();
			path = path.Replace(Path.AltDirectorySeparatorChar, Path.DirectorySeparatorChar);
			fileName = Path.GetFileName(shaderFileName);

			// Load in the shader file for processing.
			sourceCode = File.ReadAllText(path + fileName);

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
			IList<string> lines = sourceCode.Replace("\r\n", "\n").Replace("\n\r", "\n").Split(new char[] { '\n' });
			int i = 0;

			while (i < lines.Count)
			{
				string includeLine = lines[i].Trim();

				if (includeLine.StartsWith("#GorgonInclude", StringComparison.CurrentCultureIgnoreCase))
				{
					GorgonShaderInclude includeFile = ParseIncludeLine(includeLine, false);

					if (!Contains(includeFile.Name))
						throw new KeyNotFoundException("The include file in line '" + includeLine + "' was not found in the include list.");

					result.Append("// ------------------ Begin #include of '");
					result.Append(includeFile.Name);
					result.Append("' ------------------ \r\n");
					result.Append(ProcessSource(this[includeFile.Name].SourceCode));
					result.Append("\r\n");
					result.Append("// ------------------ End #include of '");
					result.Append(includeFile.Name);
					result.Append("'------------------ \r\n\r\n");
				}
				else
				{
					result.Append(lines[i]);
					result.Append("\r\n");
				}
				//result.Append("\r\n");
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
