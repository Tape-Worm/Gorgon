using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace TestMsgProc
{
	public partial class Form1 : Form
	{
		public Form1()
		{
			InitializeComponent();
		}

		private void openToolStripMenuItem_Click(object sender, EventArgs e)
		{
			MessageBox.Show(this, "Yay!");
		}

		private void Form1_DoubleClick(object sender, EventArgs e)
		{
			MessageBox.Show(this, "Yay 2!");
		}

		private void button1_Click(object sender, EventArgs e)
		{
			//GorgonLibrary.Gorgon.Quit();

			try
			{
				Application.Run(this);
			}
			catch (Exception ex)
			{
				try
				{
					throw new NotFiniteNumberException("Slutfuck", ex);
				}
				catch (Exception inner)
				{
					try
					{
						throw new NullReferenceException("Fuck you!",inner);
					}
					catch(Exception outer)
					{
						try
						{
							throw new IndexOutOfRangeException("Fuck you too!", outer);
						}
						catch (Exception iGiveUp)
						{
							inner.Data["Fuck"] = "Shit";
							ex.Data["Shit"] = "Cunt";
							GorgonLibrary.GorgonException.Catch(iGiveUp, () => GorgonLibrary.UI.GorgonDialogs.ErrorBox(this, iGiveUp));
						}
					}
				}
				//throw ex;
			}
		}
	}
}
