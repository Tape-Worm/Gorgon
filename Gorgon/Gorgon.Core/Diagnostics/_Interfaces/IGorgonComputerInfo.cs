#region MIT
// 
// Gorgon.
// Copyright (C) 2018 Michael Winsor
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
// Created: March 26, 2018 9:46:27 AM
// 
#endregion

namespace Gorgon.Diagnostics;

// ReSharper disable InconsistentNaming

/// <summary>
/// CPU/OS platform type.
/// </summary>
public enum PlatformArchitecture
{
    /// <summary>
    /// x86 architecture.
    /// </summary>
    x86 = 0,
    /// <summary>
    /// x64 architecture.
    /// </summary>
    x64 = 1
}

// ReSharper restore InconsistentNaming

/// <summary>
/// Provides information about the current platform.
/// </summary>
/// <remarks>
/// <para>
/// Ths interface will only be implemented on a platform specific assembly (e.g. Gorgon.Windows) since it requires platform specific functionality.
/// </para>
/// </remarks>
public interface IGorgonComputerInfo
{
    /// <summary>
    /// Property to return the total physical RAM available in bytes.
    /// </summary>
    long TotalPhysicalRAM
    {
        get;
    }

    /// <summary>
    /// Property to return the available physical RAM in bytes.
    /// </summary>
    long AvailablePhysicalRAM
    {
        get;
    }

    /// <summary>
    /// Property to return the platform that this instance of Gorgon was compiled for.
    /// </summary>
    /// <remarks>
    /// When the application that uses this class is run on a 64 bit version of windows, on a 64 bit machine, and the application is running as 64 bit, then this value will return 
    /// <see cref="PlatformArchitecture.x64"/>, otherwise it will return <see cref="PlatformArchitecture.x86"/>.
    /// </remarks>
    PlatformArchitecture PlatformArchitecture
    {
        get;
    }

    /// <summary>
    /// Property to return the architecture of the Operating System that Gorgon is running on.
    /// </summary>
    /// <remarks>
    /// When the application that uses this class is run on a 64 bit version of windows, and on a 64 bit machine then this value will return <see cref="PlatformArchitecture.x64"/>, 
    /// otherwise it will return <see cref="PlatformArchitecture.x86"/>.
    /// </remarks>
    PlatformArchitecture OperatingSystemArchitecture
    {
        get;
    }

    /// <summary>
    /// Property to return the name for the computer.
    /// </summary>
    string ComputerName
    {
        get;
    }

    /// <summary>
    /// Property to return the platform for the Operating System.
    /// </summary>
    PlatformID OperatingSystemPlatform
    {
        get;
    }

    /// <summary>
    /// Property to return the version of the operating system.
    /// </summary>
    Version OperatingSystemVersion
    {
        get;
    }

    /// <summary>
    /// Property to return the operating system version as a formatted text string.
    /// </summary>
    /// <remarks>This includes the platform, version number and service pack.</remarks>
    string OperatingSystemVersionText
    {
        get;
    }

    /// <summary>
    /// Property to return the service pack that is applied to the operating system.
    /// </summary>
    string OperatingSystemServicePack
    {
        get;
    }

    /// <summary>
    /// Property to return the number of processors in the computer.
    /// </summary>
    int ProcessorCount
    {
        get;
    }

    /// <summary>
    /// Property to return the system directory for the operating system.
    /// </summary>
    string SystemDirectory
    {
        get;
    }

    /// <summary>
    /// Property to return a list of machine specific environment variables.
    /// </summary>
    IReadOnlyDictionary<string, string> MachineEnvironmentVariables
    {
        get;
    }

    /// <summary>
    /// Property to return a list of user specific environment variables.
    /// </summary>
    IReadOnlyDictionary<string, string> UserEnvironmentVariables
    {
        get;
    }

    /// <summary>
    /// Property to return a list of process specific environment variables.
    /// </summary>
    IReadOnlyDictionary<string, string> ProcessEnvironmentVariables
    {
        get;
    }

    /// <summary>
    /// Function to refresh the list of user and machine specific environment variables.
    /// </summary>
    /// <remarks>
    /// This method will populate the <see cref="MachineEnvironmentVariables"/>, <see cref="ProcessEnvironmentVariables"/>, and the <see cref="UserEnvironmentVariables"/> properties with values from the 
    /// environment variables for the operating system. These values cannot be modified from this class since this class is meant for information gathering only.
    /// </remarks>
    void RefreshEnvironmentVariables();
}
