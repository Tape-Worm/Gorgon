#region MIT.
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Saturday, July 30, 2011 1:15:02 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// The base object used for device objects with a swap chain, and extraneous swap chains.
	/// </summary>
	/// <typeparam name="T">Type of settings to use for the window target.</typeparam>
	public abstract class GorgonWindowTarget<T>
		: GorgonRenderTarget where T : GorgonSwapChainSettings
	{
		#region Variables.
		private bool _disposed = false;			// Flag to indicate that the object was disposed.
		#endregion

		#region Events.
		/// <summary>
		/// Event fired before the device is reset, so resources can be freed.
		/// </summary>
		public event EventHandler BeforeDeviceReset;
		/// <summary>
		/// Event fired after the device is reset, so resources can be restored.
		/// </summary>
		public event EventHandler AfterDeviceReset;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the settings for this swap chain.
		/// </summary>
		public T Settings
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return whether the target is ready to receive rendering data.
		/// </summary>
		public abstract bool IsReady
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Handles the SizeChanged event of the BoundWindow control.
		/// </summary>
		/// <param name="sender">The source of the event.</param>
		/// <param name="e">The <see cref="System.EventArgs"/> instance containing the event data.</param>
		private void BoundWindow_SizeChanged(object sender, EventArgs e)
		{
			RemoveEventHandlers();
			OnWindowResized(Settings.BoundWindow.ClientSize.Width, Settings.BoundWindow.ClientSize.Height);
			AddEventHandlers();
		}

		/// <summary>
		/// Function to clear out any outstanding resources when the object is disposed.
		/// </summary>
		protected override void CleanUpResources()
		{
			base.CleanUpResources();

			RemoveEventHandlers();			
		}

		/// <summary>
		/// Function called when the window is resized.
		/// </summary>
		/// <param name="newWidth">New width of the window.</param>
		/// <param name="newHeight">New height of the window.</param>
		protected virtual void OnWindowResized(int newWidth, int newHeight)
		{
			if ((Settings.BoundForm.WindowState != FormWindowState.Minimized) && (Settings.BoundWindow.ClientSize.Width > 0) && (Settings.BoundWindow.ClientSize.Height > 0))
				UpdateResources();
		}

		/// <summary>
		/// Function to remove any event handlers assigned to the bound window.
		/// </summary>
		protected void RemoveEventHandlers()
		{
			if (Settings.BoundWindow != null)
				Settings.BoundWindow.SizeChanged -= new EventHandler(BoundWindow_SizeChanged);
		}

		/// <summary>
		/// Function to add any event handlers that need to be assigned to the bound window.
		/// </summary>
		protected void AddEventHandlers()
		{
			if (Settings.BoundWindow != null)
				Settings.BoundWindow.SizeChanged += new EventHandler(BoundWindow_SizeChanged);
		}

		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		protected override void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
					RemoveEventHandlers();

				_disposed = true;
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Function called when the device is about to be reset.
		/// </summary>
		protected virtual void OnBeforeDeviceReset()
		{
			if (BeforeDeviceReset != null)
				BeforeDeviceReset(this, EventArgs.Empty);
		}

		/// <summary>
		/// Function called when the device has been reset.
		/// </summary>
		protected virtual void OnAfterDeviceReset()
		{
			if (AfterDeviceReset != null)
				AfterDeviceReset(this, EventArgs.Empty);
		}

		/// <summary>
		/// Function to initialize the render target.
		/// </summary>
		internal override void Initialize()
		{
			RemoveEventHandlers();
			base.Initialize();
			AddEventHandlers();
		}

		/// <summary>
		/// Function to display the contents of the swap chain.
		/// </summary>
		public abstract void Display();

		#region Remove this shit.
		/// <summary>
		/// 
		/// </summary>
		public abstract void SetupTest();

		/// <summary>
		/// 
		/// </summary>
		public abstract void RunTest(float dt);

		/// <summary>
		/// 
		/// </summary>
		public abstract void CleanUpTest();

		#endregion
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonWindowTarget"/> class.
		/// </summary>
		/// <param name="graphics">The graphics instance that owns this swap chain.</param>
		/// <param name="name">The name.</param>
		/// <param name="settings">Swap chain settings.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.BoundWindow">settings.BoundWindow</see> parameter is NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> parameter is an empty string.
		/// </exception>
		protected GorgonWindowTarget(GorgonGraphics graphics, string name, T settings)
			: base(graphics, name)	
		{
			if (settings == null)
				throw new ArgumentNullException("settings");

			// Assign the settings here because the method hiding does not propagate through inheritance.
			Settings = settings;
		}
		#endregion
	}
}
