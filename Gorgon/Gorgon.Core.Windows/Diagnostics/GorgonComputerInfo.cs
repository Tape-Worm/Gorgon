
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
// Created: Wednesday, November 02, 2011 9:05:14 AM
// 

using System.Collections;
using System.Runtime.InteropServices;
using Gorgon.Core.Windows.Properties;
using Gorgon.Native;
using Microsoft.Win32;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.SystemInformation;

namespace Gorgon.Diagnostics;

/// <summary>
/// Information about the computer and operating system that is running Gorgon
/// </summary>
public class GorgonComputerInfo
    : IGorgonComputerInfo
{
    /// <summary>
    /// A record for a CPU.
    /// </summary>
    /// <param name="CpuName">The name of the CPU.</param>
    /// <param name="CpuIdentifier">The identifier for the CPU.</param>
    /// <param name="CpuVendor">The vendor of the CPU.</param>
    /// <param name="CoreCount">The number of cores on the CPU.</param>
    /// <param name="SmtCount">The number of SMT/Hyperthreaded cores on the CPU.</param>
    /// <param name="CoreEfficiency">The efficiency value for each core</param>
    private record class CpuInfo(string CpuName, string CpuIdentifier, string CpuVendor, int CoreCount, int SmtCount, IReadOnlyList<int> CoreEfficiency)
        : IGorgonCpuInfo;

    // List of machine specific environment variables.
    private readonly Dictionary<string, string> _machineVariables;
    // List of user specific environment variables.
    private readonly Dictionary<string, string> _userVariables;
    // List of process specific environment variables.
    private readonly Dictionary<string, string> _processVariables;
    // The list of CPU information.
    private readonly List<IGorgonCpuInfo> _cpuInfo = [];

    /// <inheritdoc/>
    public long TotalPhysicalRAM => KernelApi.TotalPhysicalRAM;

    /// <inheritdoc/>
    public long AvailablePhysicalRAM => KernelApi.AvailablePhysicalRAM;

    /// <inheritdoc/>
    public PlatformArchitecture PlatformArchitecture => Environment.Is64BitProcess ? PlatformArchitecture.x64 : PlatformArchitecture.x86;

    /// <inheritdoc/>
    public PlatformArchitecture OperatingSystemArchitecture => Environment.Is64BitOperatingSystem ? PlatformArchitecture.x64 : PlatformArchitecture.x86;

    /// <inheritdoc/>
    public string ComputerName => Environment.MachineName;

    /// <inheritdoc/>
    public ComputerInfoOperatingSystem OperatingSystem => ComputerInfoOperatingSystem.Windows;

    /// <inheritdoc/>
    public Version OperatingSystemVersion => Environment.OSVersion.Version;

    /// <inheritdoc/>
    public string OperatingSystemVersionText => Environment.OSVersion.VersionString;

    /// <inheritdoc/>
    public string OperatingSystemServicePack => Environment.OSVersion.ServicePack;

    /// <inheritdoc/>
    public IReadOnlyList<IGorgonCpuInfo> Cpus => _cpuInfo;

    /// <inheritdoc/>
    public string SystemDirectory => Environment.SystemDirectory;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> MachineEnvironmentVariables => _machineVariables;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> UserEnvironmentVariables => _userVariables;

    /// <inheritdoc/>
    public IReadOnlyDictionary<string, string> ProcessEnvironmentVariables => _processVariables;

    /// <summary>
    /// Function to retrieve the physical CPU information.
    /// </summary>
    /// <returns>The name data.</returns>
    private static List<(string Name, string ID, string Vendor)> GetCpuStrings()
    {
        RegistryKey root = Registry.LocalMachine;
        List<(string, string, string)> result = [];

        using RegistryKey? cpuKey = root.OpenSubKey(@$"HARDWARE\DESCRIPTION\System\CentralProcessor\");

        if (cpuKey is null)
        {
            return result;
        }

        string[] subKeys = cpuKey.GetSubKeyNames();

        for (int i = 0; i < subKeys.Length; ++i)
        {
            using RegistryKey? coreKey = cpuKey.OpenSubKey(subKeys[i]);

            if (coreKey is null)
            {
                continue;
            }

            string steppingKey = coreKey.GetValue("CPU Family Model Stepping")?.ToString() ?? string.Empty;

            if (string.IsNullOrEmpty(steppingKey))
            {
                continue;
            }

            result.Add((coreKey.GetValue("ProcessorNameString")?.ToString() ?? Resources.GOR_TEXT_UNKNOWN,
                        coreKey.GetValue("Identifier")?.ToString() ?? Resources.GOR_TEXT_UNKNOWN,
                        coreKey.GetValue("VendorIdentifier")?.ToString() ?? Resources.GOR_TEXT_UNKNOWN));
        }

        return result;
    }

    /// <summary>
    /// Function to update the information about the available CPUs in this machine.
    /// </summary>
    private unsafe void UpdateProcessorInfo()
    {
        uint cpuByteCount = 0;
        uint coreByteCount = 0;

        List<(string Name, string ID, string Vendor)> cpuText = GetCpuStrings();

        if (!PInvoke.GetLogicalProcessorInformationEx(LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorPackage, null, ref cpuByteCount))
        {
            WIN32_ERROR lastResult = (WIN32_ERROR)Marshal.GetLastWin32Error();

            if (lastResult != WIN32_ERROR.ERROR_INSUFFICIENT_BUFFER)
            {
                return;
            }
        }

        if (!PInvoke.GetLogicalProcessorInformationEx(LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorCore, null, ref coreByteCount))
        {
            WIN32_ERROR lastResult = (WIN32_ERROR)Marshal.GetLastWin32Error();

            if (lastResult != WIN32_ERROR.ERROR_INSUFFICIENT_BUFFER)
            {
                return;
            }
        }

        byte[] coreData = new byte[coreByteCount];
        byte[] cpuData = new byte[cpuByteCount];

        fixed (byte* cpuPtr = cpuData, corePtr = coreData)
        {
            SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX* cpuInfo = (SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX*)cpuPtr;
            SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX* coreInfo = (SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX*)corePtr;

            if (!PInvoke.GetLogicalProcessorInformationEx(LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorPackage, cpuInfo, ref cpuByteCount))
            {
                return;
            }

            if (!PInvoke.GetLogicalProcessorInformationEx(LOGICAL_PROCESSOR_RELATIONSHIP.RelationProcessorCore, coreInfo, ref coreByteCount))
            {
                return;
            }

            static SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX* AdvancePointer(SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX* srcPtr, uint bytes) => (SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX*)(((byte*)srcPtr) + bytes);

            for (int cpuIndex = 0; cpuIndex < cpuText.Count; ++cpuIndex)
            {
                int cpuCoreCount = 0;
                int cpuHtCount = 0;
                List<int> efficiency = [];

                for (int g = 0; g < cpuInfo->Anonymous.Processor.GroupCount; ++g)
                {
                    uint coreBytes = 0;
                    coreInfo = (SYSTEM_LOGICAL_PROCESSOR_INFORMATION_EX*)corePtr;
                    ref readonly GROUP_AFFINITY affinity = ref cpuInfo->Anonymous.Processor.GroupMask[g];

                    while (coreBytes < coreByteCount)
                    {
                        if (coreInfo->Anonymous.Processor.GroupCount > 0)
                        {
                            // We should only have 1 group in a core according to MSDN.
                            ref readonly GROUP_AFFINITY coreAffinity = ref coreInfo->Anonymous.Processor.GroupMask[0];

                            if (((affinity.Mask & coreAffinity.Mask) == coreAffinity.Mask) && (g == coreAffinity.Group))
                            {
                                ++cpuCoreCount;

                                efficiency.Add(coreInfo->Anonymous.Processor.EfficiencyClass);

                                if ((coreInfo->Anonymous.Processor.Flags & PInvoke.LTP_PC_SMT) == PInvoke.LTP_PC_SMT)
                                {
                                    ++cpuHtCount;
                                }
                            }
                        }

                        coreBytes += coreInfo->Size;
                        coreInfo = AdvancePointer(coreInfo, coreInfo->Size);
                    }
                }

                cpuInfo = AdvancePointer(cpuInfo, cpuInfo->Size);

                (string name, string id, string vendor) = cpuText[cpuIndex];

                _cpuInfo.Add(new CpuInfo(name, id, vendor, cpuCoreCount, cpuHtCount, efficiency));
            }
        }
    }

    /// <inheritdoc/>
    public void RefreshEnvironmentVariables()
    {
        IDictionary machine = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine);
        IDictionary process = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Process);
        IDictionary user = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User);

        _machineVariables.Clear();
        foreach (DictionaryEntry variable in machine)
        {
            _machineVariables.TryAdd(variable.Key?.ToString() ?? string.Empty, variable.Value?.ToString() ?? string.Empty);
        }

        _processVariables.Clear();
        foreach (DictionaryEntry variable in process)
        {
            _processVariables.TryAdd(variable.Key?.ToString() ?? string.Empty, variable.Value?.ToString() ?? string.Empty);
        }

        _userVariables.Clear();
        foreach (DictionaryEntry variable in user)
        {
            _userVariables.TryAdd(variable.Key?.ToString() ?? string.Empty, variable.Value?.ToString() ?? string.Empty);
        }
    }

    /// <summary>
    /// Initializes the <see cref="GorgonComputerInfo"/> class.
    /// </summary>
    public GorgonComputerInfo()
    {
        _machineVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _userVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        _processVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        RefreshEnvironmentVariables();
        UpdateProcessorInfo();
    }
}
