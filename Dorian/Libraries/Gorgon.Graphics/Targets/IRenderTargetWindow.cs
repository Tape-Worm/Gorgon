using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Defines a render target that is bound to a window on the screen.
	/// </summary>
	public interface IRenderTargetWindow
	{
		#region Events.
		/// <summary>
		/// Event fired before the device is reset, so resources can be freed.
		/// </summary>
		event EventHandler BeforeDeviceReset;
		/// <summary>
		/// Event fired after the device is reset, so resources can be restored.
		/// </summary>
		event EventHandler AfterDeviceReset;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return whether the target is ready to receive rendering data.
		/// </summary>
		bool IsReady
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to display the contents of the swap chain.
		/// </summary>
		void Display();
		#endregion
	}
}
