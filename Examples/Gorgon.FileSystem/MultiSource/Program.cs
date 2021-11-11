#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: Thursday, January 17, 2013 11:06:54 PM
// 
#endregion

using System;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.UI;

namespace Gorgon.Examples
{
    /// <summary>
    /// Example entry point.
    /// </summary>
    /// <remarks>To see a description of this example, look in formMain.cs</remarks>
    internal static class Program
    {
        #region Properties.
        /// <summary>
        /// Property to return the log used for debug log messages.
        /// </summary>
	    public static IGorgonLog Log
        {
            get;
            private set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Log = new GorgonTextFileLog("MultiSource", "Tape_Worm");
            Log.LogStart();

            try
            {
#if NET6_0_OR_GREATER
                Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
#endif
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new Form());
            }
            catch (Exception ex)
            {
                ex.Catch(_ => GorgonDialogs.ErrorBox(null, _), Log);
            }
            finally
            {
                Log.LogEnd();
            }
        }
        #endregion
    }
}
