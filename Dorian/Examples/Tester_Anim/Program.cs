using System;
using System.Windows.Forms;
using GorgonLibrary;

namespace Tester_Anim
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
			Gorgon.Run(new Form1());
			//Application.Run(new Form1());
		}
	}
}
