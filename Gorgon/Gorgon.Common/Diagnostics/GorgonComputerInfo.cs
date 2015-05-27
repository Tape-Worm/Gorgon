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
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Gorgon.Native;

namespace Gorgon.Diagnostics
{
	#region Enumerations.
    // ReSharper disable InconsistentNaming

    /// <summary>
	/// CPU/OS platform type.
	/// </summary>
	/// <remarks>This is a replacement for the old PlatformID code in the 1.x version of </remarks>
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
    #endregion

	/// <summary>
	/// Information about the computer and operating system that is running Gorgon.
	/// </summary>
	public static class GorgonComputerInfo
	{
		#region Variables.
		// List of machine specific environment variables.
		private static Dictionary<string, string> _machineVariables;
		// List of user specific environment variables.
		private static Dictionary<string, string> _userVariables;
		// Synchronization primitive for threads.
		private static int _syncPrimitive;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the total physical RAM available in bytes.
		/// </summary>
		public static long TotalPhysicalRAM
		{
			get
			{
				return Win32API.TotalPhysicalRAM;
			}
		}

		/// <summary>
		/// Property to return the available physical RAM in bytes.
		/// </summary>
		public static long AvailablePhysicalRAM
		{
			get
			{
				return Win32API.AvailablePhysicalRAM;
			}			
		}

		/// <summary>
		/// Property to return the platform that this instance of Gorgon was compiled for.
		/// </summary>
		/// <remarks>When the library is compiled for 64-bit processors, then this will read x64, otherwise it'll be x86.  If the platform cannot be determined it will return unknown.</remarks>
		public static PlatformArchitecture PlatformArchitecture
		{
			get
			{
			    return Environment.Is64BitProcess ? PlatformArchitecture.x64 : PlatformArchitecture.x86;
			}
		}

		/// <summary>
		/// Property to return the architecture of the Operating System that Gorgon is running on.
		/// </summary>
		public static PlatformArchitecture OperatingSystemArchitecture
		{
			get
			{
			    return Environment.Is64BitOperatingSystem ? PlatformArchitecture.x64 : PlatformArchitecture.x86;
			}
		}

		/// <summary>
		/// Property to return the name for the computer.
		/// </summary>
		public static string ComputerName
		{
			get
			{
				return Environment.MachineName;
			}
		}

		/// <summary>
		/// Property to return the platform for the Operating System.
		/// </summary>
		public static PlatformID OperatingSystemPlatform
		{
			get
			{
				return Environment.OSVersion.Platform;
			}
		}

		/// <summary>
		/// Property to return the version of the operating system.
		/// </summary>
		public static Version OperatingSystemVersion
		{
			get
			{
				return Environment.OSVersion.Version;
			}
		}

		/// <summary>
		/// Property to return the operating system version as a formatted text string.
		/// </summary>
		/// <remarks>This includes the platform, version number and service pack.</remarks>
		public static string OperatingSystemVersionText
		{
			get
			{
				
				return Environment.OSVersion.VersionString;
			}
		}

		/// <summary>
		/// Property to return the service pack that is applied to the operating system.
		/// </summary>
		public static string OperatingSystemServicePack
		{
			get
			{
				return Environment.OSVersion.ServicePack;
			}
		}

		/// <summary>
		/// Property to return the number of processors in the computer.
		/// </summary>
		public static int ProcessorCount
		{
			get
			{				
				return Environment.ProcessorCount;
			}
		}

		/// <summary>
		/// Property to return the system directory for the operating system.
		/// </summary>
		public static string SystemDirectory
		{
			get
			{
				return Environment.SystemDirectory;
			}
		}

		/// <summary>
		/// Property to return a list of machine specific environment variables.
		/// </summary>
		public static IReadOnlyDictionary<string, string> SystemEnvironmentVariables
		{
			get
			{
				return _machineVariables;
			}
		}

		/// <summary>
		/// Property to return a list of user specific environment variables.
		/// </summary>
		public static IReadOnlyDictionary<string, string> UserEnvironmentVariables
		{
			get
			{
				return _userVariables;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to refresh the list of user and machine specific environment variables.
		/// </summary>
		public static void RefreshEnvironmentVariables()
		{
			try
			{
				if (Interlocked.Increment(ref _syncPrimitive) > 1)
				{
					return;
				}

				_machineVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
				_userVariables = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

				IDictionary systemVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.Machine);
				IDictionary userVariables = Environment.GetEnvironmentVariables(EnvironmentVariableTarget.User);

				foreach (DictionaryEntry variable in systemVariables)
				{
					_machineVariables.Add(variable.Key.ToString(), variable.Value.ToString());
				}


				foreach (DictionaryEntry variable in userVariables)
				{
					_userVariables.Add(variable.Key.ToString(), variable.Value.ToString());
				}
			}
			finally
			{
				Interlocked.Decrement(ref _syncPrimitive);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="GorgonComputerInfo"/> class.
		/// </summary>
		static GorgonComputerInfo()
		{
			RefreshEnvironmentVariables();
		}
		#endregion
	}
}
