using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Diagnostics
{
	/// <summary>
	/// A default application loop.
	/// </summary>
	internal class GorgonDefaultAppLoop
	{
		#region Methods.
		/// <summary>
		/// Function used to process idle time.
		/// </summary>
		/// <param name="timingData">Frame timing data.</param>
		/// <returns>TRUE if the application should continue processing, FALSE if not.</returns>
		public bool ApplicationIdle(GorgonFrameRate timingData)
		{
			if (!Gorgon.IsInitialized)
				return true;

			// TODO: Maybe put a default logo or timing data if the application can display it.

			return true;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDefaultAppLoop"/> class.
		/// </summary>
		public GorgonDefaultAppLoop()
		{
		}
		#endregion
	}
}
