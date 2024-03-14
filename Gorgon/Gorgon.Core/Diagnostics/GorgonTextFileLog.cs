#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Tuesday, June 14, 2011 8:50:49 PM
// 
#endregion

using System.Globalization;
using Gorgon.Core;
using Gorgon.Diagnostics.LogProviders;

namespace Gorgon.Diagnostics;

/// <summary>
/// Sends logging information to a text file.
/// </summary>
public class GorgonTextFileLog
    : GorgonLog
{
    #region Properties.
    /// <summary>
    /// Property to return the path to the log file.
    /// </summary>
    public string LogPath
    {
        get;
    }
    #endregion

    #region Constructor.
    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonTextFileLog"/> class.
    /// </summary>
    /// <param name="appName">File name for the log file.</param>
    /// <param name="extraPath">Additional directories for the path.</param>
    /// <param name="version">[Optional] The version of the application that is logging.</param>
    /// <exception cref="ArgumentNullException">Thrown when the <paramref name="appName"/> parameter is <b>null</b>.</exception>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="appName"/> parameter is empty.</exception>
    /// <remarks>
    /// <para>
    /// This constructor automatically creates a <see cref="IGorgonLogProvider"/> that outputs to a text file and assigns it to the <see cref="GorgonLog.Provider"/> property.
    /// </para>
    /// </remarks>
    public GorgonTextFileLog(string appName, string extraPath, Version version = null)
        : base(appName, version)
    {
        string logPath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        // Verify the extra path information.
        if (!string.IsNullOrEmpty(extraPath))
        {
            // Remove any text up to and after the volume separator character.
            if (extraPath.Contains(Path.VolumeSeparatorChar.ToString(CultureInfo.InvariantCulture)))
            {
                extraPath = extraPath.IndexOf(Path.VolumeSeparatorChar) < (extraPath.Length - 1)
                                ? extraPath[(extraPath.IndexOf(Path.VolumeSeparatorChar) + 1)..]
                                : string.Empty;
            }

            if (!string.IsNullOrEmpty(extraPath))
            {
                logPath = Path.Combine(logPath, extraPath);
            }
        }

        LogPath = Path.Combine(logPath, appName, "ApplicationLogging.txt");

        Provider = new LogTextFileProvider(LogPath);
    }
    #endregion
}
