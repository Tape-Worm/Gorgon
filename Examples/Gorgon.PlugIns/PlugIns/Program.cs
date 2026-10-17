
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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Tuesday, September 18, 2012 8:00:02 PM
// 

using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.IO;
using Gorgon.PlugIns;

namespace Gorgon.Examples;

/// <summary>
/// Entry point class
/// </summary>
/// <remarks>
/// This example will show how to load a plug-in and how to build a simple plug-in
/// 
/// The plug-in in composed of 2 parts:
/// 1.  The plug-in entry point object, which inherits from GorgonPlugIn
/// 2.  The object that will be created by this entry point
/// 
/// This is a factory pattern that allows the plug-in to create instances of objects whose functionality we 
/// wish to override and/or implement. It is these objects that can be used and swapped in and out of an 
/// application
/// 
/// The entry point object will be responsible for creating the concrete classes based on an abstract class
/// in the host application (this abstract class must implement GorgonPlugIn at minimum).  From there this 
/// class should have a method that will create the object that we wish to use. An assembly (DLL, EXE, etc...) 
/// may contain multiple plug-in entry points to allow for returning multiple interfaces or could have some 
/// form of input to allow the developer to determine which type of interface is returned. The entry point 
/// should be an abstract object in the host application that is implemented one or more times in the plug-in 
/// assembly
/// 
/// The plug-in interface is the actual interface for the functionality.  It should be an interface or class
/// that is inherited in the plug-in assembly and will implement specific functionality for that plug-in
/// 
/// To load a plug-in, the user should first load the assembly using the GorgonPlugInAssemblyCache object. 
/// This will load the assembly with the plug-ins that we want. Then a GorgonPlugInService object should be created 
/// to create an instance of the plug-in interface by using the fully qualified type name of the plug-in type
/// </remarks>
internal static class Program
{

    // The logging interface for debug messaging.
    private static IGorgonLog? _log;

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        _log = new GorgonTextFileLog("PlugIns", "Tape_Worm", typeof(Program).Assembly.GetName().Version);

        // Set up the assembly cache.
        // We'll need the assemblies loaded into this object in order to load our plug-in types.
        GorgonMefPlugInCache PlugInCache = new(_log);

        // Create our plug-in service.
        // This takes the cache of assemblies we just created.
        IGorgonPlugInService PlugInService = new GorgonMefPlugInService(PlugInCache);

        try
        {
            Console.Clear();
            Console.Title = "PlugIns Example";
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("This is an example to show how to create and use custom Plug-ins.");
            Console.WriteLine("The Plug-in interface in Gorgon is quite flexible and gives the developer");
            Console.WriteLine("the ability to allow extensions to their own applications.\n");

            Console.ResetColor();

            PlugInCache.LoadPlugInAssemblies(AppContext.BaseDirectory.FormatDirectory(Path.DirectorySeparatorChar), "*PlugIn.dll");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{PlugInCache.PlugInAssemblies.Count}");
            Console.ResetColor();
            Console.WriteLine(" Plug-in assemblies found.");

            if (PlugInCache.PlugInAssemblies.Count == 0)
            {
                return;
            }

            // Our text writer plug-in interfaces.
            IList<TextColorWriter> writers = [];

            // Create our plug-in instances, we'll limit to 9 entries just for giggles.
            TextColorPlugIn[] PlugIns = [.. (from PlugInName in PlugInService.GetPlugInNames()
                                         let PlugIn = PlugInService.GetPlugIn<TextColorPlugIn>(PlugInName)
                                         where PlugIn is not null
                                         select PlugIn)];

            // Display a list of the available plug-ins.
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"\n{PlugIns.Length}");
            Console.ResetColor();
            Console.WriteLine($" plug-ins loaded:\n");
            for (int i = 0; i < PlugIns.Length; i++)
            {
                // Here's where we make use of our description.
                Console.WriteLine($"{(i + 1)}. {PlugIns[i].Description} ({PlugIns[i].GetType().FullName})");

                // Create the text writer interface and add it to the list.
                writers.Add(PlugIns[i].CreateWriter());
            }

            Console.Write("0. Quit\n\nSelect a Plug-in:  ");

            // Loop until we quit.
            while (true)
            {
                if (!Console.KeyAvailable)
                {
                    Thread.Sleep(1);
                    continue;
                }

                Console.ResetColor();

                // Remember our cursor coordinates.
                int cursorX = Console.CursorLeft; // Cursor position.
                int cursorY = Console.CursorTop;

                ConsoleKeyInfo keyValue = Console.ReadKey(false);

                if (char.IsNumber(keyValue.KeyChar))
                {
                    if (keyValue.KeyChar == '0')
                    {
                        break;
                    }

                    // Move to the next line and clear the previous line of text.
                    Console.WriteLine();
                    Console.Write(new string(' ', Console.BufferWidth - 1));
                    Console.CursorLeft = 0;

                    // Call our text color writer to print the text in the plug-in color.
                    int writerIndex = keyValue.KeyChar - '0';

                    if (writerIndex <= writers.Count)
                    {
                        writers[writerIndex - 1].WriteString($"You pressed #{writerIndex}.");
                    }
                }
                else if (keyValue.Key == ConsoleKey.Escape)
                {
                    break;
                }

                Console.CursorTop = cursorY;
                Console.CursorLeft = cursorX;
            }
        }
        catch (Exception ex)
        {
            ex.Handle(e =>
                     {
                         Console.Clear();
                         Console.ForegroundColor = ConsoleColor.Red;
                         Console.WriteLine($"Exception:\n{e.Message}\n\nStack Trace:{e.StackTrace}");
                     },
                     _log);
        }
        finally
        {
            Console.ResetColor();
            Console.WriteLine();

            PlugInCache.Dispose();

            _log.LogEnd();
        }
    }
}
