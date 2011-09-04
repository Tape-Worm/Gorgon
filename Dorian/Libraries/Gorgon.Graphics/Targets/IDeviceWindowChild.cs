using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Identifies objects that are children of device windows.
	/// </summary>
	public interface IDeviceWindowChild
	{
		/// <summary>
		/// Property to return the device window that owns this object.
		/// </summary>
		GorgonDeviceWindow DeviceWindow
		{
			get;
		}
	}
}
