using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using GorgonLibrary;

namespace Tester
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			try
			{
				Application.EnableVisualStyles();
				Application.SetCompatibleTextRenderingDefault(false);
				Application.Run(new Form1());
			}
			catch (Exception ex)
			{
				GorgonException.Catch(ex, () => GorgonLibrary.UI.GorgonDialogs.ErrorBox(null, ex));
			}
		}
	}
}
