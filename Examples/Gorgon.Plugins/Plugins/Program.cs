
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.IO;
using Gorgon.Plugins;

namespace Gorgon.Examples;

/// <summary>
/// Entry point class
/// </summary>
/// <remarks>
/// This example will show how to load a plugin and how to build a simple plugin
/// 
/// The plugin in composed of 2 parts:
/// 1.  The plugin entry point object, which inherits from GorgonPlugin
/// 2.  The object that will be created by this entry point
/// 
/// This is a factory pattern that allows the plugin to create instances of objects whose functionality we 
/// wish to override and/or implement. It is these objects that can be used and swapped in and out of an 
/// application
/// 
/// The entry point object will be responsible for creating the concrete classes based on an abstract class
/// in the host application (this abstract class must implement GorgonPlugin at minimum).  From there this 
/// class should have a method that will create the object that we wish to use. An assembly (DLL, EXE, etc...) 
/// may contain multiple plugin entry points to allow for returning multiple interfaces or could have some 
/// form of input to allow the developer to determine which type of interface is returned. The entry point 
/// should be an abstract object in the host application that is implemented one or more times in the plugin 
/// assembly
/// 
/// The plugin interface is the actual interface for the functionality.  It should be an interface or class
/// that is inherited in the plugin assembly and will implement specific functionality for that plugin
/// 
/// To load a plugin, the user should first load the assembly using the GorgonPluginAssemblyCache object. 
/// This will load the assembly with the plugins that we want. Then a GorgonPluginService object should be created 
/// to create an instance of the plugin interface by using the fully qualified type name of the plugin type
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
        _log = new GorgonTextFileLog("Plugins", "Tape_Worm", typeof(Program).Assembly.GetName().Version);

        // Set up the assembly cache.
        // We'll need the assemblies loaded into this object in order to load our plugin types.
        GorgonMefPluginCache PluginCache = new(_log);

        // Create our plugin service.
        // This takes the cache of assemblies we just created.
        IGorgonPluginService PluginService = new GorgonMefPluginService(PluginCache);

        try
        {
            Console.Clear();
            Console.Title = "Plugins Example";
            Console.ForegroundColor = ConsoleColor.White;

            Console.WriteLine("This is an example to show how to create and use custom Plugins.");
            Console.WriteLine("The Plugin interface in Gorgon is quite flexible and gives the developer");
            Console.WriteLine("the ability to allow extensions to their own applications.\n");

            Console.ResetColor();

            PluginCache.LoadPluginAssemblies(AppContext.BaseDirectory.FormatDirectory(Path.DirectorySeparatorChar), "*Plugin.dll");
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"{PluginCache.PluginAssemblies.Count}");
            Console.ResetColor();
            Console.WriteLine(" Plugin assemblies found.");

            if (PluginCache.PluginAssemblies.Count == 0)
            {
                return;
            }

            // Our text writer plugin interfaces.
            IList<TextColorWriter> writers = [];

            // Create our plugin instances, we'll limit to 9 entries just for giggles.
            TextColorPlugin[] Plugins = [.. (from PluginName in PluginService.GetPluginNames()
                                         let Plugin = PluginService.GetPlugin<TextColorPlugin>(PluginName)
                                         where Plugin is not null
                                         select Plugin)];

            // Display a list of the available plugins.
            Console.ForegroundColor = ConsoleColor.Green;
            Console.Write($"\n{Plugins.Length}");
            Console.ResetColor();
            Console.WriteLine($" plugins loaded:\n");
            for (int i = 0; i < Plugins.Length; i++)
            {
                // Here's where we make use of our description.
                Console.WriteLine($"{(i + 1)}. {Plugins[i].Description} ({Plugins[i].GetType().FullName})");

                // Create the text writer interface and add it to the list.
                writers.Add(Plugins[i].CreateWriter());
            }

            Console.Write("0. Quit\n\nSelect a Plugin:  ");

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

                    // Call our text color writer to print the text in the plugin color.
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

            PluginCache.Dispose();

            _log.LogEnd();
        }
    }
}
