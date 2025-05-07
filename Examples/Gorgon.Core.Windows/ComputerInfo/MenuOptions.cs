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
// Created: April 6, 2025 6:10:34 PM
//

using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Math;

namespace Gorgon.Examples;

/// <summary>
/// Used to handle the menu options for the application
/// </summary>
internal static class MenuOptions
{
    // Computer information.
    private static readonly IGorgonComputerInfo _computerInfo = new GorgonComputerInfo();

    /// <summary>
    /// Function to display information about the computer.
    /// </summary>
    public static void DisplaySystemInfo()
    {
        Console.Clear();

        while (!Console.KeyAvailable)
        {
            Console.BackgroundColor = ConsoleColor.DarkBlue;
            Console.ForegroundColor = ConsoleColor.White;

            Console.CursorLeft = 0;
            Console.CursorTop = 0;

            string os = _computerInfo.OperatingSystem switch
            {
                ComputerInfoOperatingSystem.Windows => "Microsoft Windows",
                ComputerInfoOperatingSystem.MacOSX => "Apple Mac OS X",
                ComputerInfoOperatingSystem.Linux => "Linux",
                ComputerInfoOperatingSystem.FreeBSD => "FreeBSD",
                _ => "Other"
            };

            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Gorgon Architecture:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"  {_computerInfo.PlatformArchitecture}");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Computer Name:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"        {_computerInfo.ComputerName}");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("# of CPUs:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"            {_computerInfo.Cpus.Count}");
            for (int i = 0; i < _computerInfo.Cpus.Count.Min(4); ++i)
            {
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"   CPU {i}:");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"             {_computerInfo.Cpus[i].CpuName}");
                Console.ForegroundColor = ConsoleColor.Green;
                Console.Write($"   CPU {i} Core Count:");
                Console.ForegroundColor = ConsoleColor.White;
                Console.WriteLine($"  {_computerInfo.Cpus[i].CoreCount} + SMT/HT ({_computerInfo.Cpus[i].SmtCount}) = {_computerInfo.Cpus[i].CoreCount + _computerInfo.Cpus[i].SmtCount} total cores.");
            }
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Total RAM:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"            {_computerInfo.TotalPhysicalRAM.FormatMemory()} ({_computerInfo.TotalPhysicalRAM:#,###} bytes)");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Available RAM:");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write("        ");
            Console.Write($"{_computerInfo.AvailablePhysicalRAM.FormatMemory()} ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write("(");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{_computerInfo.AvailablePhysicalRAM:#,###}");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" bytes)");
            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Operating System:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" {os}");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"Version:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" {_computerInfo.OperatingSystemVersionText} {(string.IsNullOrEmpty(_computerInfo.OperatingSystemServicePack) ? string.Empty : _computerInfo.OperatingSystemServicePack)}");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"Architecture:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" {_computerInfo.OperatingSystemArchitecture}");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"System path:");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($" {_computerInfo.SystemDirectory}");

            // Display exit.
            Console.CursorLeft = 0;
            Console.CursorTop = Console.WindowHeight - 1;
            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.Write("Press any key to return...".PadRight(Console.WindowWidth));

            // Give up CPU time.
            Thread.Sleep(100);
        }
    }

    /// <summary>
    /// Function to display the environment strings.
    /// </summary>
    public static void DisplayEnvironmentStrings()
    {
        Console.Clear();

        foreach (KeyValuePair<string, string> envString in _computerInfo.MachineEnvironmentVariables)
        {
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write($"{envString.Key} = ");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine($"{envString.Value}");
        }

        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.CursorLeft = 0;
        Console.CursorTop = Console.WindowHeight - 1;
        Console.Write("Press any key to return...".PadRight(Console.WindowWidth));
        Console.ReadKey(true);
    }
}
