// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Tuesday, September 18, 2012 8:00:02 PM
// 

using Gorgon.Timing;

namespace Gorgon.Examples;

/// <summary>
/// Timing functionality example.
/// </summary>
/// <remarks>
/// Gorgon has a type that is used to track the time elapsed between each cycle (or frame) of an idle period. This is exposed via the <see cref="GorgonTiming"/> type. 
/// 
/// In order to use this type, the application must define a timer. Often these are provided by the platform specific APIs in Gorgon. However, users may also define their own timers by defining their own 
/// timer objects by implementing the <see cref="IGorgonTimer"/> type and passing that to the <see cref="GorgonTiming"/> type.
/// </remarks>
internal static class Program
{
    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        IGorgonTimer timer = new BasicTimer();
        IGorgonTimer frameLimitTimer = new BasicTimer();
        bool quit = false;
        double currentFPSLimit = 0;

        try
        {
            Console.Title = "Timing";
            Console.CursorVisible = false;
            Console.ForegroundColor = ConsoleColor.White;
            Console.Clear();

            // Start our timing functionality before we start our "idle" loop.
            GorgonTiming.StartTiming(timer);

            Console.CursorLeft = 0;
            Console.CursorTop = 0;

            Console.Write("This example shows how to use the ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("GorgonTiming");
            Console.ForegroundColor = ConsoleColor.White;
            Console.Write(" data structure, and the ");
            Console.ForegroundColor = ConsoleColor.Yellow;
            Console.Write("IGorgonTiming");
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine(" type.\n");

            Console.WriteLine("Time elapsed in:");

            // Record the beginning line so we can update only what's necessary.
            int topLine = Console.CursorTop;

            Console.ResetColor();
            Console.WriteLine("Milliseconds: ");
            Console.WriteLine("Seconds:      ");
            Console.WriteLine();

            Console.WriteLine("Frames Per Second:          ");
            Console.WriteLine("Frame Delta Time (seconds): ");

            Console.WriteLine("Highest FPS:                ");
            Console.WriteLine("Lowest FPS:                 ");

            Console.WriteLine("Highest Delta (seconds):    ");
            Console.WriteLine("Lowest Delta (seconds):     ");

            Console.WriteLine();
            Console.ForegroundColor = ConsoleColor.White;
            Console.WriteLine("Press 1 for unlimited FPS");
            Console.WriteLine("Press 2 for 1000 FPS");
            Console.WriteLine("Press 3 for 30 FPS");
            Console.WriteLine("Press 4 for 60 FPS");
            Console.WriteLine("Press 5 for 1 FPS");
            Console.WriteLine("Press any other key to stop.");

            Console.ForegroundColor = ConsoleColor.Green;

            while (!quit)
            {
                if (Console.KeyAvailable)
                {
                    ConsoleKeyInfo key = Console.ReadKey(true);

                    switch (key.Key)
                    {
                        case ConsoleKey.D1:
                            currentFPSLimit = 0;
                            break;
                        case ConsoleKey.D2:
                            currentFPSLimit = GorgonTiming.FpsToMilliseconds(1000);
                            break;
                        case ConsoleKey.D3:
                            currentFPSLimit = GorgonTiming.FpsToMilliseconds(30);
                            break;
                        case ConsoleKey.D4:
                            currentFPSLimit = GorgonTiming.FpsToMilliseconds(60);
                            break;
                        case ConsoleKey.D5:
                            currentFPSLimit = 1000;
                            break;
                        default:
                            quit = true;
                            continue;
                    }
                }

                Console.CursorLeft = 14;
                Console.CursorTop = topLine;
                Console.Write(timer.Milliseconds);

                Console.CursorTop = topLine + 1;
                Console.CursorLeft = 14;
                Console.Write($"{timer.Seconds:0.0}                                                 ");

                Console.CursorTop = topLine + 3;
                Console.CursorLeft = 28;
                Console.Write($"{GorgonTiming.FPS:0.0}                                                 ");

                Console.CursorTop = topLine + 4;
                Console.CursorLeft = 28;
                Console.Write($"{GorgonTiming.Delta:0.0####}                                                 ");

                Console.CursorTop = topLine + 5;
                Console.CursorLeft = 28;
                Console.Write($"{GorgonTiming.HighestFPS:0.0}                                                 ");

                Console.CursorTop = topLine + 6;
                Console.CursorLeft = 28;
                Console.Write($"{GorgonTiming.LowestFPS:0.0}                                                 ");

                Console.CursorTop = topLine + 7;
                Console.CursorLeft = 28;
                Console.Write($"{GorgonTiming.HighestDelta:0.0####}                                                 ");

                Console.CursorTop = topLine + 8;
                Console.CursorLeft = 28;
                Console.Write($"{GorgonTiming.LowestDelta:0.0####}                                                 ");

                while (frameLimitTimer.Milliseconds < currentFPSLimit)
                {
                    // Wait.
                    // This is a spin loop, don't do this. For our example, it'll suit the purpose, but if you 
                    // make a real application please avoid doing this.
                    Thread.Sleep(0);
                }

                // Reset our frame rate delay.
                frameLimitTimer.Reset();

                // Tell the timing system that the loop iteration is at its end, so we should
                // calculate the timing values.
                GorgonTiming.Update();
            }
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine($"There was an error:\n{ex.Message}\n\nStack Trace:\n{ex.StackTrace}");
        }
        finally
        {
            Console.ResetColor();
            Console.CursorLeft = 0;
            Console.CursorTop = 19;
            Console.CursorVisible = true;
        }
    }
}
