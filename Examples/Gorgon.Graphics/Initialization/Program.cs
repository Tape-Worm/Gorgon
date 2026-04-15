
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
// Created: March 2, 2017 12:11:47 AM
// 

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Gorgon.Diagnostics;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Math;
using Gorgon.Timing;
using Gorgon.UI.WindowsForms;

namespace Gorgon.Examples;

/// <summary>
/// This is an example of using the core graphics API.  
/// 
/// It will show how to create and initialize the graphics API.  We do this by creating a GorgonGraphics object, a GorgonCommandList
/// and a GorgonSwapChain object that is used to display graphical data
/// 
/// GorgonGraphics
/// ===========================================================================================================================
/// 
/// This is the primary object used to interface with the GPU. This object can be used to define which video device can be used 
/// for rendering, and multiple GorgonGraphics objects can allow an application to use multiple video devices at once time
/// (note: This is not the same as multiple head outputs on a video device)
/// 
/// Graphics objects created with Gorgon must pass in an instance of the GorgonGraphics object for the specific video device 
/// when creating them. This associates the data created by these objects to the video device used for rendering
/// 
/// To initialize the GorgonGraphics object, an application needs to pass in an GorgonVideoAdapterInfo object associated 
/// with the desired video device. This will be retrieved from the GorgonVideoDeviceList object which is merely a collection 
/// of available video devices installed on the computer. 
/// 
/// Most importantly, it gives us command lists, which allows us to render data on the GPU, even across multiple threads 
/// (more or less).
/// 
/// Basically, this object is merely used to get things going. 
/// 
/// To build a gorgon graphics object, you need to create a GorgonGraphicsFactory object and then use its CreateGraphics 
/// method.
/// 
/// GorgonCommandList
/// ===========================================================================================================================
/// 
/// This is where the work happens. With this object we can set which render target/swap chain receives rendering data, set 
/// clipping regions, clear outputs, alter view ports, copy resource data and capture drawing commands to send to the GPU.
/// 
/// Once a command list is recorded, all you need to do is tell the GorgonGraphics object to submit it, and it will fire off 
/// your commands to the GPU.
/// 
/// You can get a command list from the GorgonGraphics object's GetCommandList() method. 
/// 
/// GorgonSwapChain
/// ===========================================================================================================================
/// 
/// A swap chain allows us to send our graphics data to the screen.  We can create multiple swap chains and even make them all 
/// full screen (provided they are different monitors)
///
/// A GorgonSwapChain requires a GorgonSwapChainInfo object that will contain the swap chain settings required for initializing 
/// a swap chain. Because a swap chain uses multiple buffers to present graphical data to the screen, a buffer size is required 
/// and is typically set to the same size as the client area on the window that will be bound to the swap chain.  This buffer 
/// size will resized when the window is resized, or can be resized manually through a method on the object. 
/// 
/// Swap chains can also enter/exit full screen exclusive mode, and in this example we use Alt+Enter to switch between full 
/// screen and windowed mode. Gorgon also has functionality to set up a borderless full screen window (which is preferable to an 
/// exclusive full screen mode)
/// 
/// For this example we're just going to assign a window (although this could be any type that inherits from 
/// System.Windows.Forms.Control) to the swap chain via its constructor, and set its initial size to the client size of the 
/// window. 
/// 
/// When setting up a swap chain, it is important to know which buffer format to use for the back buffers.  To this end, the 
/// GorgonGraphics.VideoDevice has a method called GetBufferFormatSupport that will indicate which formats are valid for use as 
/// a swap chain format. Well behaved applications should check with this method prior to setting up the swap chain
/// 
/// Finally, to see something on the screen an application needs to call the Present method on the swap chain. This flips the 
/// current backbuffer frame to the window and can have a presentation interval to lock down the presentation to the refresh 
/// rate for the current video mode (this does not apply to windowed mode, and as such is not necessary for this application)
/// </summary>
internal static class Program
{
    // The graphics interface for the application.
    private static GorgonGraphics? _graphics;
    // Our primary swap chain.
    private static GorgonSwapChain? _swap;
    // The color to clear our swap chain with.
    private static GorgonColor _clearColor = new(0, 0, 0);
    // Which color channel are we animating? (R = 0, G = 1, B = 2).
    private static int _channel;
    // The value to apply to a specific color channel.
    private static float _channelValue;
    // The direction of the color channel animation. (1 = Incrementing values, -1 = decrementing values).
    private static int _direction = 1;
    // Indicates how to cycle through the available channels (1 = Incrementing from R -> G -> B, -1 = B -> G -> R).
    private static int _channelDirection = 1;
    // Defines which regions on the swap chain to clear.
    private static readonly GorgonRectangle[] _clearRegions = new GorgonRectangle[2];
    // Clearing pattern values (0 = full swap chain, 1 upper left/lower right only, 2 upper right, lower left only)
    private static int _clearPattern;

    /// <summary>
    /// Function to handle idle time for the application.
    /// </summary>
    /// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
    private static bool Idle()
    {
        Debug.Assert(_graphics is not null && _swap is not null, "The graphics objects are not initialized.");

        // This will clear the swap chain to the specified color.  
        // For our example, we'll cycle through multiple colors so we don't end up with a boring old screen with a static color. This will also prove that our 
        // swap chain is working and rendering data to the window.

        GorgonRectangle[]? regions;

        // Set up the clear to clear the upper left and lower right of the swap chain:
        switch (_clearPattern)
        {
            case 1:
                _clearRegions[0] = new GorgonRectangle(0, 0, _swap.Width / 2, _swap.Height / 2);
                _clearRegions[1] = new GorgonRectangle(_swap.Width / 2, _swap.Height / 2, _swap.Width / 2, _swap.Height / 2);
                regions = _clearRegions;
                break;
            case 2:
                _clearRegions[0] = new GorgonRectangle(_swap.Width / 2, 0, _swap.Width / 2, _swap.Height / 2);
                _clearRegions[1] = new GorgonRectangle(0, _swap.Height / 2, _swap.Width / 2, _swap.Height / 2);
                regions = _clearRegions;
                break;
            default:
                regions = null;
                break;
        }

        // The first step in any graphics operation is to get ourselves a command list to work with.
        // A command list is used to send our rendering commands and state data to the GPU.

        GorgonCommandList commandList = _graphics.GetCommandList("Initialization Example Command List");

        // Command lists use a fluent interface to allow us to chain multiple commands together at once.

                   // This tells the command list that we want to present the back buffer to the screen at the end of the execution of the command list.
        commandList.AddPresenter(_swap)
                   // This clears our swap chain to the colour, using the region, we asked for.
                   .ClearSwapChain(_swap, _clearColor, regions);

        _graphics.Submit(commandList);

        // This specifies how much color to apply to the channel.
        // We're using the GorgonTiming.Delta property here to retrieve the number of seconds that it takes to draw a single frame. This allows us to smooth 
        // the animation speed by basing it on the frame rate of the device. If we didn't do this, the colors would cycle way too quickly.
        _channelValue = GorgonTiming.Delta * _direction * 0.4f;

        switch (_channel)
        {
            case 0:
                _channelValue = _clearColor.Red + _channelValue;
                _clearColor = new GorgonColor(_channelValue, 0, 0);
                break;
            case 1:
                _channelValue = _clearColor.Green + _channelValue;
                _clearColor = new GorgonColor(0, _channelValue, 0);
                break;
            case 2:
                _channelValue = _clearColor.Blue + _channelValue;
                _clearColor = new GorgonColor(0, 0, _channelValue);
                break;
        }

        // If we've exceeded the min/max amount of color for the channel, move on to the next.
        if (_channelValue > 1.0f)
        {
            _direction = -1;
            _channelValue = 1.0f;
        }

        if (_channelValue < 0.0f)
        {
            _direction = 1;
            _channelValue = 0.0f;
            _channel += _channelDirection;
        }

        // Flip directions and set to the middle channel.
        if (_channel > 2)
        {
            _channel = 1;
            _channelDirection = -1;

            ++_clearPattern;
            if (_clearPattern > 2)
            {
                _clearPattern = 0;
            }
        }

        if (_channel < 0)
        {
            _channel = 1;
            _channelDirection = 1;

            ++_clearPattern;
            if (_clearPattern > 2)
            {
                _clearPattern = 0;
            }
        }

#warning FIXME: This can't work until we have the 2D renderer up and running.
        //GorgonExample.BlitLogo(_graphics);

        return true;
    }

    /// <summary>
    /// Function to initialize the application.
    /// </summary>
    /// <param name="factory">The factory used to create the graphics objects.</param>
    /// <returns>The main form for the application.</returns>
    private static FormMain Initialize(GorgonGraphicsFactory factory)
    {
        // First, create our form.
        FormMain result = GorgonExample.Initialize(new GorgonPoint(640, 480), "Initialization");

        try
        {
            result.KeyUp += MainForm_KeyUp;

            // Now we create and enumerate the list of video devices installed in the computer.
            // We must do this in order to tell Gorgon which video device we intend to use. Note that this method may be quite slow (particularly when running DEBUG versions of 
            // Direct 3D). To counter this, this object and its Enumerate method are thread safe so this can be run in the background while keeping the main UI responsive.
            //
            // If no suitable device was found (no Direct 3D 12 Ultimate support) in the computer, this method will return an empty list. However, if it succeeds, then the devices
            // list will be populated with an GorgonVideoAdapterInfo for each suitable video device in the system.
            //
            // Using this method, we could also enumerate the WARP software rasterizer. This device is typically used to determine if there's a driver error, and can be terribly
            // slow to render. It is recommended that it only be used in diagnostic scenarios.
            IReadOnlyList<GorgonVideoAdapterInfo> devices = factory.EnumerateAdapters();

            if (devices.Count == 0)
            {
                GorgonDialogs.Error(result, "This example requires a video adapter that supports Direct3D 12 Ultimate or better.");
                return result;
            }

            // Now we create the main graphics interface with the first applicable video device.
            _graphics = factory.CreateGraphics(devices[0]);

            // Check to ensure that we can support the format required for our swap chain.
            // If a video device can't support this format, then the odds are good it won't render anything. Since we're asking for a very common display format, this will 
            // succeed nearly 100% of the time. Regardless, it's good form to the check for a working display format prior to setting up the swap chain.
            //
            // This is also used to determine if a format can be used for other objects (e.g. a texture, render target, etc...) And like the swap chain format, it is also best 
            // practice to check if the object you're creating supports the desired format.
            if (!_graphics.FormatSupport[BufferFormat.R8G8B8A8_UNorm].IsDisplayFormat)
            {
                // We should never see this unless you've got some very esoteric hardware.
                GorgonDialogs.Error(result, "We should not see this error.");
                return result;
            }

            // Finally, create a swap chain to display our output.
            // In this case we're setting up our swap chain to bind with our main window, and we use its client size to determine the width/height of the swap chain back buffers.
            // This width/height does not need to be the same size as the window, but, except for some scenarios, that would produce undesirable image quality.
            _swap = new GorgonSwapChain(_graphics,
                                        "Main Swap Chain",
                                        result.Handle,
                                        new GorgonSwapChainInfo(result.ClientSize.Width,
                                                                     result.ClientSize.Height,
                                                                     BufferFormat.R8G8B8A8_UNorm));

#warning FIXME: This can't work until we have the 2D renderer up and running.
            //GorgonExample.LoadResources(_graphics);

            GorgonExample.Loop.Run(Idle);
        }
        finally
        {
            GorgonExample.EndInit();
        }

        return result;
    }

    /// <summary>
    /// Handles the KeyUp event of the MainForm control.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="e">The <see cref="KeyEventArgs"/> instance containing the event data.</param>
    private static void MainForm_KeyUp(object? sender, KeyEventArgs e)
    {
        if ((e.KeyCode != Keys.Enter)
            || (!e.Alt)
            || (_swap is null)
            || (sender is not Form form))
        {
            return;
        }

        // Gorgon used to handle managing the window state for you when shifting between full screen and windowed mode.
        // However, since Gorgon can now be used with WPF and Avalonia, it is not practical for it to handle this state 
        // transition. So, users must ensure the window is set up correctly before entering/exiting fullscreen mode.

        // Setting full screen state requires that the user size the window prior to calling EnterFullscreen(). The 
        // method uses the client window area to determine the best video mode to use when changing the resolution.
        // To that end: While Direct3D -will- change your desktop resolution and colour depth, exclusive full screen
        // mode, which Gorgon used to use under Direct 3D 11, and used to be used for performance reasons is no longer
        // a thing in Direct 3D 12 (which Gorgon now uses). The ideal thing to do is to size the window the size of
        // your desired monitor, and then enter full screen mode.
        //
        // This causes far fewer issues with desktop composition.

        GorgonVideoOutputInfo? output = _graphics?.Adapter.Outputs.GetOutputFromWindowHandle(form.Handle);

        if (output is null)
        {
            return;
        }

        Size screenSize = new(output.Bounds.Width, output.Bounds.Height);

        if (!_swap.IsWindowed)
        {            
            _swap.ExitFullscreen();

            // Our original window size should now be set back.
            form.ClientSize = new Size(640, 480);
            return;
        }

        // Change to the size of our desktop.
        form.ClientSize = screenSize;
        _swap.EnterFullscreen();
    }

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    [STAThread]
    private static void Main()
    {
        IGorgonLog log = GorgonLog.NullLog;
        GorgonGraphicsFactory? _factory = null;

        Application.SetHighDpiMode(HighDpiMode.PerMonitorV2);
        Application.EnableVisualStyles();
        Application.SetCompatibleTextRenderingDefault(false);

        try
        {
            log = new GorgonTextFileLog("Initialization", "Tape_Worm", typeof(Program).Assembly.GetName().Version);
            log.LogStart(new GorgonComputerInfo());

#if DEBUG
            _factory = new GorgonGraphicsFactory(GorgonGraphicsDebugFlags.SynchronizedCommandQueueValidation 
                | GorgonGraphicsDebugFlags.GpuBasedValidation
                | GorgonGraphicsDebugFlags.GpuBasedStateTrackingValidation
                | GorgonGraphicsDebugFlags.ObjectTracking, log: log);
#else
            _factory = new GorgonGraphicsFactory(log: log);
#endif

            Application.Run(Initialize(_factory));
        }
        catch (Exception ex)
        {
            GorgonExample.HandleException(ex);
        }
        finally
        {
            GorgonExample.ShutDown();

            // Always clean up when you're done.
            // Since Gorgon uses Direct 3D 12, we must be careful to dispose of any objects that implement IDisposable. 
            // Failure to do so can lead to warnings from the Direct 3D runtime when running in DEBUG mode.
            _swap?.Dispose();
            _graphics?.Dispose();
            _factory?.Dispose();
            log.LogEnd();
        }
    }
}
