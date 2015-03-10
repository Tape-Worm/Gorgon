using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary;
using GorgonLibrary.Editor;
using GorgonLibrary.IO;

namespace EditorTests
{
	class PlugInRegistryMock
		: IPlugInRegistry
	{

		#region IPlugInRegistry Members

		private IReadOnlyDictionary<string, GorgonFileSystemProviderPlugIn> _plugIns;

		/// <summary>
		/// Property to return a list of the disabled plug-ins.
		/// </summary>
		public IReadOnlyList<DisabledPlugIn> DisabledPlugIns
		{
			get
			{
				return new DisabledPlugIn[0];
			}
		}

		/// <summary>
		/// Property to return the collection of file system provider plug-ins.
		/// </summary>
		public IReadOnlyDictionary<string, GorgonFileSystemProviderPlugIn> FileSystemPlugIns
		{
			get
			{
				return _plugIns ?? (_plugIns = Gorgon.PlugIns.OfType<GorgonFileSystemProviderPlugIn>().ToDictionary(key => key.Name, value => value));
			}
		}

		/// <summary>
		/// Function to scan for and load any plug-ins located in the plug-ins folder.
		/// </summary>
		public void ScanAndLoadPlugIns()
		{
			Gorgon.PlugIns.LoadPlugInAssembly(@"..\..\..\..\PlugIns\Bin\Debug\Gorgon.FileSystem.GorPack.dll");
			Gorgon.PlugIns.LoadPlugInAssembly(@"..\..\..\..\PlugIns\Bin\Debug\Gorgon.FileSystem.Zip.dll");
		}
		#endregion

		#region IPlugInRegistry Members
		/// <summary>
		/// Event fired when an already loaded plug-in is disabled.
		/// </summary>
		public event EventHandler<PlugInDisabledEventArgs> PlugInDisabled;

		/// <summary>
		/// Function to determine if a plug-in is disabled.
		/// </summary>
		/// <param name="plugIn">The plug-in to query.</param>
		/// <returns>
		/// TRUE if disabled, FALSE if not.
		/// </returns>
		public bool IsDisabled(GorgonLibrary.PlugIns.GorgonPlugIn plugIn)
		{
			return false;
		}

		/// <summary>
		/// Function to disable a plug-in.
		/// </summary>
		/// <param name="plugIn">Plug-in to disable.</param>
		public void DisablePlugIn(GorgonLibrary.PlugIns.GorgonPlugIn plugIn)
		{
			
		}
		#endregion
	}
}
