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
// Created: Wednesday, November 02, 2011 9:05:14 AM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using Gorgon.Collections.Specialized;
using Gorgon.Native;

namespace Gorgon.Diagnostics;

/// <summary>
/// Information about the computer and operating system that is running Gorgon.
/// </summary>
public class GorgonComputerInfo
    : IGorgonComputerInfo
{
    #region Variables.
    // List of machine specific environment variables.
    private readonly GorgonConcurrentDictionary<string, string> _machineVariables;
    // List of user specific environment variables.
    private readonly GorgonConcurrentDictionary<string, string> _userVariables;
    // List of process specific environment variables.
    private readonly GorgonConcurrentDictionary<string, string> _processVariables;
    #endregion

    #region Properties.
    /// <summary>
    /// Property to return the total physical RAM available in bytes.
    /// </summary>
    public long TotalPhysicalRAM => KernelApi.TotalPhysicalRAM;

    /// <summary>
    /// Property to return the available physical RAM in bytes.
    /// </summary>
    public long AvailablePhysicalRAM => KernelApi.AvailablePhysicalRAM;

    /// <summary>
    /// Property to return the platform that this instance of Gorgon was compiled for.
    /// </summary>
    /// <remarks>
    /// When the application that uses this class is run on a 64 bit version of windows, on a 64 bit machine, and the application is running as 64 bit, then this value will return 
    /// <see cref="PlatformArchitecture.x64"/>, otherwise it will return <see cref="PlatformArchitecture.x86"/>.
    /// </remarks>
    public PlatformArchitecture PlatformArchitecture => Environment.Is64BitProcess ? PlatformArchitecture.x64 : PlatformArchitecture.x86;

    /// <summary>
    /// Property to return the architecture of the Operating System that Gorgon is running on.
    /// </summary>
    /// <remarks>
    /// When the application that uses this class is run on a 64 bit version of windows, and on a 64 bit machine then this value will return <see cref="PlatformArchitecture.x64"/>, 
    /// otherwise it will return <see cref="PlatformArchitecture.x86"/>.
    /// </remarks>
    public PlatformArchitecture OperatingSystemArchitecture => Environment.Is64BitOperatingSystem ? PlatformArchitecture.x64 : PlatformArchitecture.x86;

    /// <summary>
    /// Property to return the name for the computer.
    /// </summary>
    public string ComputerName => Environment.MachineName;

    /// <summary>
    /// Property to return the platform for the Operating System.
    /// </summary>
    public PlatformID OperatingSystemPlatform => Environment.OSVersion.Platform;

    /// <summary>
    /// Property to return the version of the operating system.
    /// </summary>
    public Version OperatingSystemVersion => Environment.OSVersion.Version;

    /// <summary>
    /// Property to return the operating system version as a formatted text string.
    /// </summary>
    /// <remarks>This includes the platform, version number and service pack.</remarks>
    public string OperatingSystemVersionText => Environment.OSVersion.VersionString;

    /// <summary>
    /// Property to return the service pack that is applied to the operating system.
    /// </summary>
    public string OperatingSystemServicePack => Environment.OSVersion.ServicePack;

    /// <summary>
    /// Property to return the number of processors in the computer.
    /// </summary>
    public int ProcessorCount => Environment.ProcessorCount;

    /// <summary>
    /// Property to return the system directory for the operating system.
    /// </summary>
    public string SystemDirectory => Environment.SystemDirectory;

    /// <summary>
    /// Property to return a list of machine specific environment variables.
    /// </summary>
    public IReadOnlyDictionary<string, string> MachineEnvironmentVariables => _machineVariables;

    /// <summary>
    /// Property to return a list of user specific environment variables.
    /// </summary>
    public IReadOnlyDictionary<string, string> UserEnvironmentVariables => _userVariables;

    /// <summary>
    /// Property to return a list of process specific environment variables.
    /// </summary>
    public IReadOnlyDictionary<string, string> ProcessEnvironmentVariables => _processVariables;

    #endregion

    #region Methods.
    /// <summary>
    /// Function to refresh the list of user and machine specific environment variables.
    /// </summary>
    /// <remarks>
    /// This method will populate the <see cref="MachineEnvironmentVariables"/>, <see cref="ProcessEnvironmentVariables"/>, and the <see cref="UserEnvironmentVariables"/> properties with values from the 
    /// environment variables for the operating system. These values cannot be modified from this class since this class is meant for information gathering only.
    /// </remarks>
    public void RefreshEnvironmentVariables()
    {
        IDictionary machine = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine);
        IDictionary process = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);
        IDictionary user = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User);

        _machineVariables.Clear();
        foreach (DictionaryEntry variable in machine)
        {
            _machineVariables.TryAdd(variable.Key.ToString(), variable.Value.ToString());
        }

        _processVariables.Clear();
        foreach (DictionaryEntry variable in process)
        {
            _processVariables.TryAdd(variable.Key.ToString(), variable.Value.ToString());
        }

        _userVariables.Clear();
        foreach (DictionaryEntry variable in user)
        {
            _userVariables.TryAdd(variable.Key.ToString(), variable.Value.ToString());
        }
    }
    #endregion

    #region Constructor/Destructor.
    /// <summary>
    /// Initializes the <see cref="GorgonComputerInfo"/> class.
    /// </summary>
    public GorgonComputerInfo()
    {
        _machineVariables = new GorgonConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _userVariables = new GorgonConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _processVariables = new GorgonConcurrentDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        
        RefreshEnvironmentVariables();
    }
    #endregion
}
