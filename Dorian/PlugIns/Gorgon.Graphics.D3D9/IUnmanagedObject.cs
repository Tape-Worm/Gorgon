using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics.D3D9
{
	/// <summary>
	/// Defines an unmanaged object.
	/// </summary>
	interface IUnmanagedObject
	{
		/// <summary>
		/// Function called when a device is placed in a lost state.
		/// </summary>
		void DeviceLost();
		/// <summary>
		/// Function called when a device is reset from a lost state.
		/// </summary>
		void DeviceReset();
	}
}
