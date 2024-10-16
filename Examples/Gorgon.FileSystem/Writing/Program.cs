﻿
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Friday, January 18, 2013 8:47:05 AM
// 

using Gorgon.Diagnostics;

namespace Gorgon.Examples;

/// <summary>
/// Example entry point
/// </summary>
/// <remarks>To see a description of this example, look in formMain.cs</remarks>
internal static class Program
{
    /// <summary>
    /// Property to return the application log file.
    /// </summary>
    public static IGorgonLog Log
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the directory where we'll be writing into.
    /// </summary>
    public static DirectoryInfo WriteDirectory
    {
        get;
        private set;
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        Log = new GorgonTextFileLog("Writing", "Tape_Worm");

        Log.LogStart();

        try
        {
            Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            WriteDirectory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), "Writing"));

            if (!WriteDirectory.Exists)
            {
                WriteDirectory.Create();
                WriteDirectory.Refresh();
            }

            Application.Run(new Form());
        }
        catch (Exception ex)
        {
            GorgonExample.HandleException(ex);
        }
        finally
        {
            Log.LogEnd();
        }
    }
}
