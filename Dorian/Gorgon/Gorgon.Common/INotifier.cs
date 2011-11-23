using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary
{
	/// <summary>
	/// An interface used to define an object that can notify other objects of a change.
	/// </summary>
	public interface INotifier
	{
		/// <summary>
		/// Property to set or return whether an object has been updated.
		/// </summary>
		bool HasChanged
		{
			get;
			set;
		}
	}
}
