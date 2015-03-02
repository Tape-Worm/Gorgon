using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GorgonLibrary.Editor;
using GorgonLibrary.IO;

namespace EditorTests.Mocking
{
	/// <summary>
	/// Mock object for a file system proxy object.
	/// </summary>
	class FileSystemProxyMock
		: IProxyObject<GorgonFileSystem>
	{
		private static readonly GorgonFileSystem _mockFs = new GorgonFileSystem();

		#region IProxyObject<GorgonFileSystem> Members
		/// <summary>
		/// Property to return the short lived object.
		/// </summary>
		public GorgonFileSystem Item
		{
			get
			{
				return _mockFs;
			}
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			
		}
		#endregion
	}
}
