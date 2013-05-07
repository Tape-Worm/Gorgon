using System;
using System.Windows.Forms;
using GorgonLibrary;

namespace Tester_Graphics
{
	static class Program
	{
		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			Gorgon.Run(new Form2());
		}
	}
}
