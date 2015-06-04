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
// Created: Thursday, August 8, 2013 10:46:01 PM
// 
#endregion

using System;
using System.Drawing;
using System.Windows.Forms;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.UI;

namespace Gorgon.Graphics.Example
{
	/// <summary>
	/// This is an example of using the base graphics API.  It's very similar to how Direct 3D 11 works, but with some enhancements
	/// to deal with poor error support and other "gotchas" that tend to pop up.  It also has some time saving functionality to
	/// deal with mundane tasks like setting up a swap chain, pixel shaders, etc...
	/// 
	/// This example shows how to create and initialize the graphics API.  We do this by creating a GorgonGraphics object and a
	/// GorgonSwapChain object to display graphical data.
	/// 
	/// GorgonGraphics
	/// ===========================================================================================================================
	/// 
	/// To initialize Gorgon we must first create a new GorgonGraphics object.  The constructor for this object can take a value 
	/// known as a device feature level to specify the base line video device capabilities to use.  In this example we're going 
	/// to use the SM2_a_b feature level which is a Direct 3D 9 capable video card that supports a vertex shader level of 2.0a and 
	/// a pixel shader level of 2.0b.  This feature level value specifies what capabilities we have available, and in this case 
	/// we can only use Direct 3D 9 capabilities (which are quite limited).
	/// 
	/// To have Gorgon use the best available feature level for your video device, you may call the GorgonGraphics constructor 
	/// without any parameters and it will use the best available feature level for your device.  
	/// 
	/// This object is important in that it provides all access to other graphics object types.  It creates (and tracks the objects 
	/// that it creates) objects via properties that expose graphics functionality groups.  For example, since a render target is 
	/// an output for graphics data, you would find render target management functionality under the Output property.
	/// 
	/// As mentioned, the graphics object will track objects that it creates.  This means that it will handle the destruction of 
	/// these objects when the graphics interface is disposed.  This will help keep memory leaks to a minimum as you created your 
	/// application.  Please note that this is not the best way of handling memory and that you should ALWAYS dispose objects 
	/// yourself when you're done with them, graphics or otherwise.
	/// 
	/// The graphics object will also take a VideoDevice parameter to force it to use a specific video device for rendering.  If 
	/// this video device is not specified, then Gorgon will use the first detected device.  In this example we let Gorgon choose 
	/// which device to use.
	/// 
	/// GorgonSwapChain
	/// ===========================================================================================================================
	/// 
	/// A swap chain allows us to send our graphics data to the screen.  We can create multiple swap chains and even make them all 
	/// full screen (provided they are different monitors) and/or even use them as a texture in a pixel shader.
	///
	/// If you've used the previous version of Gorgon this would be analog to the SetMode method and Screen property. And while the 
	/// SetMode method had many parameters to allow for customization of the swap chain, this new version has many more which are 
	/// passed in through the GorgonSwapChainSettings object.
	///
	/// The swap chain settings object allows us to assign a window, a buffer format (basically the organization of pixels within 
	/// the buffer), a width, a height, etc...  In this example we're just going to set the Window property. When we set the window 
	/// property, but neglect to set a width or height, Gorgon will use the client area width/height of the window as its 
	/// resolution. If we had set this window to be full screen, Gorgon would have found the closest full screen video mode that 
	/// matched the size of the window.  The same would be true had we specified the width and height.
	/// 
	/// When setting up a swap chain, it is important to know which buffer format to use.  You may call the SupportsDisplayFormat 
	/// of the VideoDevice parameter on 
	/// 
	/// A swap chain in windowed mode can be resized or placed into full screen/windowed mode. But, unlike the previous version of 
	/// Gorgon, we won't need to destroy resources when the swap chain is resized.  We will only need to ensure that the application 
	/// knows of the latest size of the swap chain so that rendering uses the right dimensions.
	/// 
	/// When we render data to the swap chain, we must present it to the user via the Flip method.  If we fail to do this, then 
	/// nothing will appear on screen and our application will appear to be broken.
	/// </summary>
	static class Program
	{
		#region Variables.
		private static formMain _mainForm;										// Main application form.
		private static GorgonSwapChain _swap;									// Our primary swap chain.
		private static int _colorDirection = 1;									// Direction of our color.
		private static GorgonColor _clearColor = new GorgonColor(				// The color to clear our swap chain with.
													25.0f / 255.0f, 
													24.5f / 255.0f, 
													22.0f / 255.0f);	
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the graphics interface for the application.
		/// </summary>
		public static GorgonGraphics Graphics
		{
			get;
			private set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to handle idle time for the application.
		/// </summary>
		/// <returns><b>true</b> to continue processing, <b>false</b> to stop.</returns>
		private static bool Idle()
		{
			// Clear the swap chain.  
			_swap.Clear(_clearColor);

			// Animate our clear color so we have something to look at instead of a blank screen.
			_clearColor = new GorgonColor(
				_clearColor.Red + (GorgonTiming.Delta * _colorDirection * 0.0125f),
				_clearColor.Green + (GorgonTiming.Delta * _colorDirection * 0.0125f),
				_clearColor.Blue + (GorgonTiming.Delta * _colorDirection * 0.0125f)
			);

			if (((_clearColor.Red > (250.0f / 255.0f)) || (_clearColor.Red < (25.0f / 255.0f)))
				&& ((_clearColor.Green > (245.0f / 255.0f)) || (_clearColor.Green < (24.5f / 255.0f)))
				&& ((_clearColor.Blue > (220.0f / 255.0f)) || (_clearColor.Blue < (22.0f / 255.0f))))
			{
				_colorDirection *= -1;

				// Ensure that we don't get stuck.
				_clearColor = _colorDirection < 0
					? new GorgonColor(250.0f / 255.0f, 245f / 255.0f, 220f / 255.0f)
					: new GorgonColor(25.0f / 255.0f, 24.5f / 255.0f, 22.0f / 255.0f);
			}

			// Now we flip our buffers on the swap chain.  
			// We need to this or we won't see anything.
			// Note that we can limit this to flip on a specified number of vertical retraces.  This 
			// will enable us to lock the frame rate to that of the refresh rate of the monitor.
			_swap.Flip();
			
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


			// Create the main graphics interface for SM 4.0.
			// This is basically a Direct 3D 10 capable video device.  This is here to illustrate how to 
			// force a feature level.
			Graphics = new GorgonGraphics(DeviceFeatureLevel.SM4);
			
			// Check to ensure that we can support the format required for our swap chain.
			// If a video device can't support this format, then the odds are good it won't render anything, and thus 
			// this is here for illustration on how to determine if a format is OK for display purposes.
			if (!Graphics.VideoDevice.SupportsDisplayFormat(BufferFormat.R8G8B8A8_UIntNormal))
			{
				GorgonDialogs.ErrorBox(_mainForm, "We should not see this error.");
				return;
			}

			// Create a swap chain as our graphics output to the window.
			_swap = Graphics.Output.CreateSwapChain("Main", new GorgonSwapChainSettings
			{
				Window = _mainForm										// Assign to our form.
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
				if (Graphics != null)
				{
					Graphics.Dispose();
				}
			}
		}
	}
}
