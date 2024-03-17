
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Tuesday, September 11, 2012 8:35:18 PM
// 


using System.Windows.Forms;
using Gorgon.Timing;

namespace Gorgon.Examples;

/// <summary>
/// Entry point class
/// </summary>
internal class Program
{

    private static bool _quit;          // Flag to quit the application.



    /// <summary>
    /// Function to draw the time at the bottom of the window.
    /// </summary>
    private static void UpdateTime()
    {
        TimeSpan time = new(0, 0, (int)GorgonTiming.SecondsSinceStart);

        // Get the time.
        string timeString = $"Application up time: {time.Hours}:{time.Minutes:00}:{time.Seconds:00}";

        // Display the amount of time that the application has been running.
        Console.BackgroundColor = ConsoleColor.Black;
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.CursorLeft = 0;
        Console.CursorTop = Console.WindowHeight - 1;
        Console.Write(timeString.PadRight(Console.WindowWidth - 1));

        // Reset the colors.
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.ForegroundColor = ConsoleColor.White;

        // Give up some CPU time.
        Thread.Sleep(5);
        GorgonTiming.Update();
    }

    /// <summary>
    /// Function to write out the program options.
    /// </summary>
    private static void WriteOptions()
    {
        Console.BackgroundColor = ConsoleColor.DarkBlue;
        Console.ForegroundColor = ConsoleColor.White;

        Console.Clear();

        Console.WriteLine("This is a small example of -some- of the functionality that's provided in the");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("Gorgon.Core");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(" library:");
        Console.WriteLine();
        Console.WriteLine("1.  Display an exception.");
        Console.WriteLine("2.  Display a warning.");
        Console.WriteLine("3.  Display some information.");
        Console.WriteLine("4.  Display environment strings.");
        Console.WriteLine("5.  Display system info.");
        Console.WriteLine();
        Console.WriteLine("Q.  Quit.");
        Console.WriteLine();
        Console.WriteLine("The common library houses several utility functions such as:\n\u00b7 Basic dialogs.\n\u00b7 System information.\n\u00b7 Various extensions.\n\u00b7 Debugging utilities.\n\u00b7 Plug-in support.\n\u00b7 Log files.\n\u00b7 Base named object collections.\n\u00b7 And functions to manipulate native memory.\n\nAnd a bunch of other junk that might be useful to you...");

        Console.CursorTop = Console.WindowHeight - 2;
        Console.Write("The ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("GorgonDiagnostics.GorgonTiming");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(" class provides this timer:");
    }

    /// <summary>
    /// Function to read a key value and launch the appropriate option.
    /// </summary>
    private static void ReadValue()
    {
        while (!_quit)
        {
            ConsoleKeyInfo key = Console.KeyAvailable ? Console.ReadKey(true) : default;

            switch (key.Key)
            {
                case ConsoleKey.D1:
                    MenuOptions.DisplayException();
                    break;
                case ConsoleKey.D2:
                    MenuOptions.DisplayWarning();
                    break;
                case ConsoleKey.D3:
                    Console.BackgroundColor = ConsoleColor.Black;
                    Console.ForegroundColor = ConsoleColor.Yellow;
                    Console.CursorVisible = true;

                    Console.CursorTop = Console.WindowHeight - 2;
                    Console.CursorLeft = 0;
                    // Display a bar at the bottom.
                    Console.WriteLine("Type some text to display:".PadRight(Console.WindowWidth - 1));
                    Console.ForegroundColor = ConsoleColor.Gray;
                    Console.Write(new string(' ', Console.WindowWidth - 1));

                    // Reset the cursor so we don't wrap off the end.
                    Console.CursorTop = Console.WindowHeight - 1;
                    Console.CursorLeft = 0;

                    string infoText = Console.ReadLine();

                    Console.CursorVisible = false;

                    // Redraw the menu.
                    WriteOptions();

                    if (!string.IsNullOrEmpty(infoText))
                    {
                        MenuOptions.DisplayInfo(infoText);
                    }

                    break;
                case ConsoleKey.D4:
                    MenuOptions.DisplayEnvironmentStrings();
                    WriteOptions();
                    break;
                case ConsoleKey.D5:
                    MenuOptions.DisplaySystemInfo();
                    WriteOptions();
                    break;
                case ConsoleKey.Q:
                    _quit = true;
                    break;
            }

            // Update our timer.
            UpdateTime();
        }
    }

    /// <summary>
    /// The entry point for the application.
    /// </summary>
    private static void Main()
    {
        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);

        // This is here for any windows forms elements that get displayed.
        // Without this, the elements will not use the visual styles and will 
        // default to older styles.
        Application.EnableVisualStyles();
        Application.DoEvents();

        Console.Title = "Gorgon Example #1 - Core functionality";
        Console.CursorVisible = false;

        try
        {
            // In order to use the GorgonTiming class, we need to supply a timer to it.
            GorgonTiming.StartTiming<GorgonTimerMultimedia>();

            WriteOptions();
            ReadValue();
        }
        finally
        {
            Console.CursorVisible = true;
            Console.ResetColor();
            Console.CursorLeft = 0;
            Console.CursorTop = Console.WindowHeight - 1;
        }
    }

}
