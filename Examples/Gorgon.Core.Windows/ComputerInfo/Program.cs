// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: April 6, 2025 6:10:42 PM
//

using Gorgon.Timing;

namespace Gorgon.Examples;

/// <summary>
/// Entry point class
/// </summary>
internal class Program
{
    // Flag to quit the application.
    private static bool _quit;
    // The name of the assembly that holds the concrete version of the timer.
    private static string _timerAssembly = string.Empty;

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
        Console.Write(timeString.PadRight(Console.WindowWidth));

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

        Console.Write("A platform specific example that shows some diagnostic data from the ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write(_timerAssembly);
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(" library:");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("1.");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("   Display environment strings.");
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("2.");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine("   Display system info.");
        Console.WriteLine();
        Console.ForegroundColor = ConsoleColor.Yellow;
        Console.Write("Esc.");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(" Quit.");
        Console.WriteLine();

        Console.CursorTop = Console.WindowHeight - 3;
        Console.Write("The ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("GorgonDiagnostics.GorgonTiming");
        Console.ForegroundColor = ConsoleColor.White;
        Console.WriteLine(" class in conjunction with the platform specific implementation of ");
        Console.ForegroundColor = ConsoleColor.Green;
        Console.Write("IGorgonTimer");
        Console.ForegroundColor = ConsoleColor.White;
        Console.Write(" provides this up-time value:");
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
                    MenuOptions.DisplayEnvironmentStrings();
                    WriteOptions();
                    break;
                case ConsoleKey.D2:
                    MenuOptions.DisplaySystemInfo();
                    WriteOptions();
                    break;
                case ConsoleKey.Escape:
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
        Console.Title = "Gorgon Computer Info";
        Console.CursorVisible = false;

        try
        {
            // In order to use the GorgonTiming class, we need to supply a timer to it.
            // This timer is platform specific.
            IGorgonTimer timer = new GorgonTimer();
            _timerAssembly = timer.GetType().Assembly.GetName().Name ?? string.Empty;
            GorgonTiming.StartTiming(timer);

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
