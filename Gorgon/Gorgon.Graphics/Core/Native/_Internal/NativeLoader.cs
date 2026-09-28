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
// Created: October 2, 2025 3:01:10 PM
//

using System.ComponentModel;
using System.Runtime.InteropServices;
using Gorgon.Core;
using Gorgon.IO;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Functionality to load native DLLs using a standardized folder structure.
/// </summary>
internal unsafe static class NativeLoader
{
    /// <summary>
    /// Function to load the D3D WARP library.
    /// </summary>
    /// <param name="path">The path to the library.</param>
    /// <param name="throwOnError"><b>true</b> to throw an exception when loading the DLL, <b>false</b> to return a NULL module handle.</param>
    /// <returns>The module handle.</returns>
    private static HMODULE LoadLibrary(string path, bool throwOnError)
    {
        fixed (char* pathPtr = path)
        {
            HMODULE handle = Win32.LoadLibraryW(pathPtr);

            if ((throwOnError) && ((handle == HMODULE.NULL) || (handle == HMODULE.INVALID_VALUE)))
            {
                int err = Marshal.GetLastWin32Error();
                throw new Win32Exception(err);
            }

            return handle;
        }
    }

    /// <summary>
    /// Function to load a native DLL.
    /// </summary>
    /// <param name="dllFileName">The name of the DLL file to load.</param>
    /// <param name="throwExceptionOnError"><b>true</b> to throw an exception when loading the DLL, <b>false</b> to return a NULL module handle.</param>
    /// <exception cref="ArgumentEmptyException">Thrown when the <paramref name="dllFileName"/> is empty.</exception>
    public static HMODULE Load(string dllFileName, bool throwExceptionOnError)
    {
        ArgumentEmptyException.ThrowIfNullOrWhiteSpace(dllFileName);

        string assemblyPath = Path.GetDirectoryName(AppContext.BaseDirectory)?.FormatDirectory(Path.DirectorySeparatorChar) ?? Path.GetFullPath("." + Path.DirectorySeparatorChar);
        string platform = Environment.Is64BitProcess ? "win-x64" : "win-x86";
        dllFileName = Path.GetFileName(dllFileName);
        dllFileName = Path.ChangeExtension(dllFileName, ".dll");

        string path = Path.Combine(assemblyPath, "runtimes", platform, "native", dllFileName);

        HMODULE handle = LoadLibrary(path, false);

        if ((handle != HMODULE.NULL) && (handle != HMODULE.INVALID_VALUE))
        {
            return handle;
        }

        platform = Environment.Is64BitProcess ? "x64" : "x86";
        path = Path.Combine(assemblyPath, platform, dllFileName);

        handle = LoadLibrary(path, false);

        if ((handle != HMODULE.NULL) && (handle != HMODULE.INVALID_VALUE))
        {
            return handle;
        }

        return LoadLibrary(dllFileName, throwExceptionOnError);
    }
}
