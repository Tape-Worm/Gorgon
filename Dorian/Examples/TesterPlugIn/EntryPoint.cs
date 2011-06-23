using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TesterPlugIn
{
	public class EntryPoint
		: GorgonLibrary.PlugIns.GorgonPlugIn
	{
		public void ShowShit()
		{
			System.Windows.Forms.MessageBox.Show("Test");
		}

		public EntryPoint()
			: base("Test plug-in.")
		{
		}
	}
}
