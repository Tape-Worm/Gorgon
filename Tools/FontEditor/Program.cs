#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Thursday, August 03, 2006 4:14:30 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Windows.Forms;
using dk.dava.FontSmoothing;

namespace GorgonLibrary.Graphics.Tools
{
	/// <summary>
	/// Main entry point.
	/// </summary>
	static class Program
	{
		private static FontSmoothingController _smoother = null;	// Font smoother.

		/// <summary>
		/// Property to return the font smoothing interface.
		/// </summary>
		public static FontSmoothingController Smoother
		{
			get
			{
				return _smoother;
			}
		}

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			try
			{
				_smoother = new FontSmoothingController();
				_smoother.FontSmoothingType = FontSmoothingTypes.FE_FONTSMOOTHINGSTANDARD;
				_smoother.FontSmoothing = false;
				
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new FontEditor());
			}
			catch
			{
				if (_smoother != null)
					_smoother.Dispose();

				throw;
			}
			finally
			{
				if (_smoother != null)
					_smoother.Dispose();
			}
		}
	}
}