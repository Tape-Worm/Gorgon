#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Tuesday, September 11, 2012 8:43:55 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.UI;

namespace Gorgon.Examples
{
    /// <summary>
    /// Used to handle the menu options for the application.
    /// </summary>
    internal static class MenuOptions
    {
        #region Variables.
        // Computer information.
        private static readonly IGorgonComputerInfo _computerInfo = new GorgonComputerInfo();
        #endregion

        #region Methods.
        /// <summary>
        /// Function to throw the inner exception.
        /// </summary>
        private static void InnerException()
        {
            try
            {
                throw new NullReferenceException("This is a NULL reference exception.  It will be the inner exception.");
            }
            catch (Exception ex)
            {
                throw new ArgumentNullException("This is an Argument NULL exception.", ex);
            }
        }

        /// <summary>
        /// Function to throw the outer exception.
        /// </summary>
        /// <param name="inner">Inner exception.</param>
        private static void OuterException(Exception inner) => throw inner.Repackage("This will be the outer exception.\n\nLook at the 'Details' to see the full exception stack.");

        /// <summary>
        /// Function to display information about the computer.
        /// </summary>
        public static void DisplaySystemInfo()
        {
            Console.Clear();

            while (!Console.KeyAvailable)
            {
                Console.BackgroundColor = ConsoleColor.DarkBlue;
                Console.ForegroundColor = ConsoleColor.White;

                Console.CursorLeft = 0;
                Console.CursorTop = 0;

                Console.WriteLine("Gorgon architecture:\t{0}", _computerInfo.PlatformArchitecture);
                Console.WriteLine();
                Console.WriteLine("Computer name:\t\t{0}", _computerInfo.ComputerName);
                Console.WriteLine("# of processors:\t{0}", _computerInfo.ProcessorCount);
                Console.WriteLine("Total RAM:\t\t{0} ({1:#,###} bytes)", _computerInfo.TotalPhysicalRAM.FormatMemory(), _computerInfo.TotalPhysicalRAM);
                Console.WriteLine("Available RAM:\t\t{0} ({1:#,###} bytes)", _computerInfo.AvailablePhysicalRAM.FormatMemory(), _computerInfo.AvailablePhysicalRAM);
                Console.WriteLine();
                Console.WriteLine("Windows version:\t{0} {1}", _computerInfo.OperatingSystemVersionText, string.IsNullOrEmpty(_computerInfo.OperatingSystemServicePack) ? string.Empty : _computerInfo.OperatingSystemServicePack);
                Console.WriteLine("Windows architecture:\t{0}", _computerInfo.OperatingSystemArchitecture);
                Console.WriteLine("System path:\t\t{0}", _computerInfo.SystemDirectory);

                // Display exit.
                Console.CursorLeft = 0;
                Console.CursorTop = Console.WindowHeight - 1;
                Console.BackgroundColor = ConsoleColor.Black;
                Console.ForegroundColor = ConsoleColor.Yellow;

                Console.Write("Press any key to return...".PadRight(Console.WindowWidth - 1));

                // Give up CPU time.
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// Function to display the environment strings.
        /// </summary>
        public static void DisplayEnvironmentStrings()
        {
            Console.Clear();

            foreach (KeyValuePair<string, string> envString in _computerInfo.MachineEnvironmentVariables)
            {
                Console.WriteLine("{0} = {1}", envString.Key, envString.Value);
            }

            Console.BackgroundColor = ConsoleColor.Black;
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("Press any key to return...".PadRight(Console.WindowWidth - 1));
            Console.ReadKey(true);
        }

        /// <summary>
        /// Function to display an information dialog.
        /// </summary>
        /// <param name="infoMessage">Message to display.</param>
        public static void DisplayInfo(string infoMessage)
        {
            GorgonDialogs.ConfirmBox(null, "This is a question", allowCancel: true, allowToAll: true);
            GorgonDialogs.ConfirmBox(null, "This is a question", allowCancel: true);
            GorgonDialogs.InfoBox(null, infoMessage);
        }

        /// <summary>
        /// Function to display the warning dialog.
        /// </summary>
        public static void DisplayWarning() => GorgonDialogs.WarningBox(null, "This is a warning!");

        /// <summary>
        /// Function to throw, catch and display exception information.
        /// </summary>
        /// <remarks>This will show how to </remarks>
        public static void DisplayException()
        {
            try
            {
                try
                {
                    InnerException();
                }
                catch (Exception ex)
                {
                    OuterException(ex);
                }
            }
            catch (Exception ex)
            {
                GorgonDialogs.ErrorBox(null, ex);
            }
        }
        #endregion
    }
}
