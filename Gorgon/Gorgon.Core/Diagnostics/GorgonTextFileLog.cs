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
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Tuesday, June 14, 2011 8:50:49 PM
// 

using System.Globalization;
using Gorgon.Core;
using Gorgon.Diagnostics.LogProviders;

namespace Gorgon.Diagnostics;

/// <summary>
/// Sends logging information to a text file
/// </summary>
public class GorgonTextFileLog
    : GorgonLog
{
    /// <summary>
    /// Property to return the path to the log file.
    /// </summary>
    public string LogPath
    {
        get;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTextFileLog"/> class.
    /// </summary>
    /// <param name="appName">File name for the log file.</param>
    /// <param name="extraPath">Additional directories for the path.</param>
    /// <param name="version">[Optional] The version of the application that is logging.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="appName"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// This constructor automatically creates a <see cref="IGorgonLogProvider"/> that outputs to a text file and assigns it to the <see cref="GorgonLog.Provider"/> property.
    /// </para>
    /// <para>
    /// For Windows, the log files created by this class will reside in the <c>%LocalAppData%\</c><paramref name="extraPath"/><c>\</c><paramref name="appName"/> directory 
    /// (e.g. If <paramref name="extraPath"/> is <c>Dir1\Dir2</c> and <paramref name="appName"/> is <c>MyProgram</c> the resulting path will be 
    /// <c>%LocalAppData%\Dir1\Dir2\MyProgram\ApplicationLogging.txt</c>). The <c>%LocalAppData%</c> environment variable is usually pointed at the 
    /// <c>c:\users\&lt;username&gt;\AppData\Local\</c> directory.
    /// </para>
    /// <para>
    /// For Linux, the log files will reside in the <c>/home/&lt;username&gt;/.local/share/</c><paramref name="extraPath"/><c>/</c><paramref name="appName"/> directory
    /// (e.g. If <paramref name="extraPath"/> is <c>Dir1/Dir2</c> and <paramref name="appName"/> is <c>MyProgram</c> the resulting path will be 
    /// <c>/home/&lt;username&gt;/.local/share/Dir1/Dir2/MyProgram/ApplicationLogging.txt</c>).
    /// </para>
    /// <para>
    /// For Mac, the log files will reside in the <c>/users/&lt;username&gt;/Library/Application/</c><paramref name="extraPath"/><c>/</c><paramref name="appName"/> directory
    /// (e.g. If <paramref name="extraPath"/> is <c>Dir1/Dir2</c> and <paramref name="appName"/> is <c>MyProgram</c> the resulting path will be 
    /// <c>/users/&lt;username&gt;/Library/Application/Dir1/Dir2/MyProgram/ApplicationLogging.txt</c>).
    /// </para>
    /// </remarks>
    public GorgonTextFileLog(string appName, string? extraPath, Version? version = null)
        : base(appName, version)
    {
        string logPath = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData, Environment.SpecialFolderOption.Create);

        // Verify the extra path information.
        if (!string.IsNullOrWhiteSpace(extraPath))
        {
            // Remove any text up to and after the volume separator character.
            if (extraPath.Contains(Path.VolumeSeparatorChar.ToString(CultureInfo.InvariantCulture)))
            {
                extraPath = extraPath.IndexOf(Path.VolumeSeparatorChar) < (extraPath.Length - 1)
                                ? extraPath[(extraPath.IndexOf(Path.VolumeSeparatorChar) + 1)..]
                                : string.Empty;
            }

            if (!string.IsNullOrWhiteSpace(extraPath))
            {
                logPath = Path.Combine(logPath, extraPath);
            }
        }

        LogPath = Path.Combine(logPath, appName, "ApplicationLogging.txt");

        Provider = new LogTextFileProvider(LogPath);
    }
}