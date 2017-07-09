﻿#region MIT
// 
// Gorgon.
// Copyright (C) 2017 Michael Winsor
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
// Created: March 2, 2017 12:11:47 AM
// 
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using DXGI = SharpDX.DXGI;
using D3D11 = SharpDX.Direct3D11;
using Gorgon.Core;
using Gorgon.Graphics.Core;
using Gorgon.Timing;
using Gorgon.UI;

namespace Gorgon.Graphics.Example
{
	/// <summary>
	/// This is an example of using the core graphics API.  
	/// 
	/// It will show how to create and initialize the graphics API.  We do this by creating a GorgonGraphics object and a 
	/// GorgonSwapChain object that is used to display graphical data.
	/// 
	/// GorgonGraphics
	/// ===========================================================================================================================
	/// 
	/// This is the primary object used to send data to the GPU. This object can be used to define which video device can be used 
	/// for rendering, and multiple GorgonGraphics objects can allow an application to use multiple video devices at once time
	/// (note: This is not the same as multiple head outputs on a video device).
	/// 
	/// Graphics objects created with Gorgon must pass in an instance of the GorgonGraphics object for the specific video device 
	/// when creating them. This associates the data created by these objects to the video device used for rendering.
	/// 
	/// The GorgonGraphics object can also be used to force a specific feature level for a video device. This allows Gorgon to 
	/// be compatible with a wide range of video devices that don't support Direct3D 11.1 (at this point, this option is kind of 
	/// moot as the vast majority of video devices these days are more than capable of D3D 11.1). If the feature level is not 
	/// defined, then the maximum feature level supported by the device is used.
	/// 
	/// To initialize the GorgonGraphics object, an application needs to pass in an IGorgonVideoDeviceInfo object associated 
	/// with the desired video device. This will be retrieved from the IGorgonVideoDeviceList object which is merely a collection 
	/// of available video devices installed on the computer. 
	/// 
	/// Since this example does not focus on rendering data, this object is merely used to get things going. 
	/// 
	/// GorgonSwapChain
	/// ===========================================================================================================================
	/// 
	/// A swap chain allows us to send our graphics data to the screen.  We can create multiple swap chains and even make them all 
	/// full screen (provided they are different monitors).
	///
	/// A GorgonSwapChain requries an IGorgonSwapChainInfo object that will contain the swap chain settings required for initializing 
	/// a swap chain. Because a swap chain uses multiple buffers to present graphical data to the screen, a buffer size is required 
	/// and is typically set to the same size as the client area on the window that will be bound to the swap chain.  This buffer 
	/// size will resized when the window is resized, or can be resized manually through a method on the object. 
	/// 
	/// Swap chains can also enter/exit full screen exclusive mode, but for our purposes here we won't be doing that.
	/// 
	/// For this example we're just going to assign a window (although this could be any type that inherits from 
	/// System.Windows.Forms.Control) to the swap chain via its constructor, and set its initial size to the client size of the 
	/// window. 
	/// 
	/// When setting up a swap chain, it is important to know which buffer format to use for the back buffers.  To this end, the 
	/// GorgonGraphics.VideoDevice has a method called GetBufferFormatSupport that will indicate which formats are valid for use as 
	/// a swap chain format. Well behaved applications should check with this method prior to setting up the swap chain.
	/// 
	/// Finally, to see something on the screen an application needs to call the Present method on the swap chain. This flips the 
	/// current backbuffer frame to the window and can have a presentation interval to lock down the presentation to the refresh 
	/// rate for the current video mode (this does not apply to windowed mode, and as such is not necessary for this application).
	/// </summary>
	static class Program
	{
		#region Variables.
		// Main application form.
		private static formMain _mainForm;                                      
		// The graphics interface for the application.
		private static GorgonGraphics _graphics;
		// Our primary swap chain.
		private static GorgonSwapChain _swap;
		// The color to clear our swap chain with.
		private static GorgonColor _clearColor = new GorgonColor(0, 0, 0);
		// Which color channel are we animating? (R = 0, G = 1, B = 2).
		private static int _channel;
		// The value to apply to a specific color channel.
		private static float _channelValue;
		// The direction of the color channel animation. (1 = Incrementing values, -1 = decrementing values).
		private static int _direction = 1;
		// Indicates how to cycle through the available channels (1 = Incrementing from R -> G -> B, -1 = B -> G -> R).
		private static int _channelDirection = 1;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to handle idle time for the application.
		/// </summary>
		/// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
		private static bool Idle()
		{
			// This will clear the swap chain to the specified color.  
			// For our example, we'll cycle through multiple colors so we don't end up with a boring old screen with a static color. This will also prove that our 
			// swap chain is working and rendering data to the window.
			_swap.RenderTargetView.Clear(_clearColor);

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
			}

			if (_channel < 0)
			{
				_channel = 1;
				_channelDirection = 1;
			}

			// Now we flip our buffers on the swap chain.  
			// We need to this or we won't see anything at all except the standard window background color. Clearly, we don't want that. 
			// This method will take the current frame back buffer and flip it to the front buffer (the window). If we had more than one swap chain tied to multiple 
			// windows, then we'd need to do this for every swap chain.
			_swap.Present(1);
			
			return true;
		}

		/// <summary>
		/// Function to initialize the application.
		/// </summary>
		private static void Initialize()
		{
			// Create our form and center on the primary monitor.
			_mainForm = new formMain
			            {
				            ClientSize = new Size(640, 480)
			            };

			_mainForm.Location = new Point(Screen.PrimaryScreen.WorkingArea.Width / 2 - _mainForm.Width / 2,
				Screen.PrimaryScreen.WorkingArea.Height / 2 - _mainForm.Height / 2);

			// First we create and enumerate the list of video devices installed in the computer.
			// We must do this in order to tell Gorgon which video device we intend to use. Note that this method may be quite slow (particularly when running DEBUG versions of 
			// Direct 3D). To counter this, this object and its Enumerate method are thread safe so this can be run in the background while keeping the main UI responsive.
			IGorgonVideoDeviceList devices = new GorgonVideoDeviceList();

			// If no suitable device was found (no Direct 3D 11.1 support) in the computer, this method will throw an exception. However, if it succeeds, then the devices object 
			// will be populated with the IGorgonVideoDeviceInfo for each video device in the system.
			//
			// Using this method, we could also enumerate the WARP software rasterizer, and/of the D3D Reference device (only if the DEBUG functionality provided by the Windows 
			// SDK is installed). These devices are typically used to determine if there's a driver error, and can be terribly slow to render (reference moreso than WARP). It is 
			// recommended that these only be used in diagnostic scenarios only.
			devices.Enumerate();

			// Now we create the main graphics interface with the first applicable video device.
			_graphics = new GorgonGraphics(devices[0]);

			// Check to ensure that we can support the format required for our swap chain.
			// If a video device can't support this format, then the odds are good it won't render anything. Since we're asking for a very common display format, this will 
			// succeed nearly 100% of the time (unless you've somehow gotten an ancient video device to work with Direct 3D 11.1). Regardless, it's good form to the check for a 
			// working display format prior to setting up the swap chain.
			//
			// This method is also used to determine if a format can be used for other objects (e.g. a texture, render target, etc...) Like the swap chain format, this is also a 
			// best practice to check if the object you're creating supports the desired format.
			if ((_graphics.VideoDevice.GetBufferFormatSupport(DXGI.Format.R8G8B8A8_UNorm) & D3D11.FormatSupport.Display) != D3D11.FormatSupport.Display)
			{
				// We should never see this unless you've performed some form of black magic.
				GorgonDialogs.ErrorBox(_mainForm, "We should not see this error.");
				return;
			}

			// Finally, create a swap chain to display our output.
			// In this case we're setting up our swap chain to bind with our main window, and we use its client size to determine the width/height of the swap chain back buffers.
			// This width/height does not need to be the same size as the window, but, except for some scenarios, that would produce undesirable image quality.
			_swap = new GorgonSwapChain("Main Swap Chain", _graphics, _mainForm, new GorgonSwapChainInfo
			                                                                     {
				                                                                     Format = DXGI.Format.R8G8B8A8_UNorm,
																					 Width = _mainForm.ClientSize.Width,
																					 Height = _mainForm.ClientSize.Height
			                                                                     });
		}
		#endregion

		/// <summary>
		/// The main entry point for the application.
		/// </summary>
		[STAThread]
		static void Main()
		{
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			try
			{
				// Initialize the application.
				Initialize();

				// Now begin running the application idle loop.
				GorgonApplication.Run(_mainForm, Idle);
			}
			catch (Exception ex)
			{
				ex.Catch(_ => GorgonDialogs.ErrorBox(null, _), GorgonApplication.Log);
			}
			finally
			{
				// Always clean up when you're done.
				// Since Gorgon uses Direct 3D 11.1, which allocate objects that use native memory and COM objects, we must be careful to dispose of any objects that implement 
				// IDisposable. Failure to do so can lead to warnings from the Direct 3D runtime when running in DEBUG mode.
				_swap?.Dispose();
				_graphics?.Dispose();
			}
		}
	}
}
