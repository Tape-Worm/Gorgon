using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary
{
	/// <summary>
	/// A cloneable object interface.
	/// </summary>
	/// <typeparam name="T">Type to clone.</typeparam>
	public interface ICloneable<out T>
	{
		/// <summary>
		/// Function to clone an object.
		/// </summary>
		/// <returns>The cloned object.</returns>
		T Clone();
	}
}
