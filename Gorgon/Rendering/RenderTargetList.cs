#region LGPL.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Tuesday, August 02, 2005 12:03:56 PM
// 
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Windows.Forms;
using SharpUtilities;
using SharpUtilities.Utility;
using SharpUtilities.Collections;
using GorgonLibrary.Internal;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// List of render targets.
	/// </summary>
	public class RenderTargetList 
		: DynamicArray<RenderTarget>,IDisposable
	{
		#region Classes.
		/// <summary>
		/// Class to handle comparison sorting.
		/// </summary>
		private class RenderTargetComparer : IComparer<RenderTarget>
		{
			#region IComparer Members
			/// <summary>
			/// Function to compare two render target priorities.
			/// </summary>
			/// <param name="x">Render target 1.</param>
			/// <param name="y">Render target 2.</param>
			/// <returns>0 if the same, -1 if x is less than y, 1 is x is greater than y.</returns>
			public int Compare(RenderTarget x, RenderTarget y)
			{
				if (x.Priority < y.Priority)
					return -1;
				if (x.Priority > y.Priority)
					return 1;

				return 0;
			}
			#endregion
		}
		#endregion

		#region Variables.
		private RenderTargetComparer _comparer;			            // Comparer used for sorting.
		private SortedList<string, RenderTarget> _renderTargets;	// Sorted list of render targets.
		private RenderWindow _primary;								// Primary render window.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the primary render target window.
		/// </summary>
		internal RenderWindow Screen
		{
			get
			{
				return _primary;
			}
		}

		/// <summary>
		/// Property to return a render target by name.
		/// </summary>
		/// <param name="key">Name of the render target to return.</param>
		/// <returns>Render target.</returns>
		public RenderTarget this[string key]
		{
			get
			{
				if (!_renderTargets.ContainsKey(key))
					throw new SharpUtilities.Collections.KeyNotFoundException(key);

				return _renderTargets[key];
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the sorting of this collection.
		/// </summary>
		internal void Sort()
		{
			if (_items.Count > 1)				
				_items.Sort(_comparer);
		}

		/// <summary>
		/// Function to create a new primary render window.
		/// </summary>
		/// <param name="owner">Owning window of this render window.</param>
		/// <param name="width">Width of the render window.</param>
		/// <param name="height">Height of the render window.</param>
		/// <param name="format">Back buffer format of the render window.</param>
		/// <param name="refresh">Refresh rate.  Only applies to windowed mode.</param>
		/// <param name="windowed">TRUE to use windowed mode, FALSE to use full screen.  Only applies to primary window.</param>
		/// <param name="usedepth">TRUE to use a depth buffer, FALSE to exclude.</param>
		/// <param name="usestencil">TRUE to use a stencil buffer, FALSE to exclude.</param>
		/// <param name="vSyncInterval">V-sync interval for presentation.</param>
		internal void CreatePrimaryRenderWindow(Control owner, int width, int height, BackBufferFormats format, bool windowed, int refresh, bool usedepth, bool usestencil, VSyncIntervals vSyncInterval)
		{
			VideoMode mode;					// Requested video mode.

			mode = new VideoMode(width,height,refresh,format);

			// If we hadn't created the primary render target before, then create it,
			// else resize the device.
			if (!_renderTargets.ContainsKey("@PrimaryWindow"))
			{
				_primary = new RenderWindow("@PrimaryWindow", owner, 0);
				_items.Add(_primary);
				_renderTargets.Add(_primary.Name, _primary);
				_primary.SetMode(mode, windowed, usedepth, usestencil, false, vSyncInterval);
			}
			else
			{
				_primary = (RenderWindow)_renderTargets["@PrimaryWindow"];
				_primary.SetMode(mode, windowed, usedepth, usestencil, true, vSyncInterval);
			}			

			Sort();
		}

		/// <summary>
		/// Function to determine if a render target exists by name.
		/// </summary>
		/// <param name="name">Name to check.</param>
		/// <returns>TRUE if the render target exists, FALSE if not.</returns>
		public bool Contains(string name)
		{
			return _renderTargets.ContainsKey(name);
		}

		/// <summary>
		/// Function to create a render target image.
		/// </summary>
		/// <param name="name">Name of the image.</param>
		/// <param name="width">Width of the image.</param>
		/// <param name="height">Height of the image.</param>
		/// <param name="format">Format of the image.</param>
		/// <param name="useDepthBuffer">TRUE to use a depth buffer, FALSE to exclude.</param>
		/// <param name="useStencilBuffer">TRUE to use a stencil buffer, FALSE to exclude.</param>
		/// <param name="priority">Priority of the target.</param>
		/// <returns>A new render target image.</returns>
		public RenderImage CreateRenderImage(string name, int width, int height, ImageBufferFormats format, bool useDepthBuffer, bool useStencilBuffer, int priority)
		{
			if (_renderTargets.ContainsKey(name))
				throw new DuplicateObjectException(name);

			// Don't allow images to be rendered after the screen.
			if (priority == 0)
				priority = -1;

			if (priority > 0)
				priority = -priority;

			// Create render image.
			RenderImage newImage = new RenderImage(name, width, height, format, useDepthBuffer, useStencilBuffer, priority);
			_items.Add(newImage);
			_renderTargets.Add(name, newImage);
			Sort();

			return newImage;
		}

		/// <summary>
		/// Function to create a render target image.
		/// </summary>
		/// <param name="name">Name of the image.</param>
		/// <param name="width">Width of the image.</param>
		/// <param name="height">Height of the image.</param>
		/// <param name="format">Format of the image.</param>
		/// <param name="priority">Priority of the target.</param>
		/// <returns>A new render target image.</returns>
		public RenderImage CreateRenderImage(string name, int width, int height, ImageBufferFormats format, int priority)
		{
			return CreateRenderImage(name, width, height, format, false, false, priority);
		}

		/// <summary>
		/// Function to create a render target image.
		/// </summary>
		/// <param name="name">Name of the image.</param>
		/// <param name="width">Width of the image.</param>
		/// <param name="height">Height of the image.</param>
		/// <param name="priority">Priority of the target.</param>
		/// <returns>A new render target image.</returns>
		public RenderImage CreateRenderImage(string name, int width, int height, int priority)
		{
			return CreateRenderImage(name, width, height, Converter.ConvertToImageFormat(Gorgon.VideoMode.Format), false,false, priority);
		}	

		/// <summary>
		/// Function to create a new render window.
		/// </summary>
		/// <param name="name">Name of this render window.</param>
		/// <param name="owner">Owning window of this render window.</param>
		/// <param name="width">Width of the render window.</param>
		/// <param name="height">Height of the render window.</param>
		/// <param name="format">Back buffer format of the render window.</param>
		/// <param name="usedepth">TRUE to use a depth buffer, FALSE to exclude.</param>
		/// <param name="usestencil">TRUE to use a stencil buffer, FALSE to exclude.</param>
		/// <param name="priority">Priority of the render window.</param>
		/// <param name="preserveBackBuffer">TRUE to preserve the contents of the back buffer when the target is updated, FALSE to discard.</param>
		/// <returns>A render window.</returns>
		public RenderWindow CreateRenderWindow(string name,Control owner, int width, int height, BackBufferFormats format, bool usedepth, bool usestencil, int priority, bool preserveBackBuffer)
		{
			VideoMode mode;		// Requested video mode.

			if (_renderTargets.ContainsKey(name))
				throw new DuplicateObjectException(name);

			// Don't allow render targets to be rendered before the screen.
			if (priority == 0)
				priority = 1;

			if (priority < 0)
				priority = -priority;

			RenderWindow newTarget = new RenderWindow(name,owner,priority);
			mode = new VideoMode(width,height,0,format);
			newTarget.CreateSwapChain(mode,usedepth,usestencil,false, preserveBackBuffer);

			_items.Add(newTarget);
			_renderTargets.Add(name, newTarget);
			Sort();

			return newTarget;
		}

		/// <summary>
		/// Function to create a new render window.
		/// </summary>
		/// <param name="name">Name of this render window.</param>
		/// <param name="owner">Owning window of this render window.</param>
		/// <param name="width">Width of the render window.</param>
		/// <param name="height">Height of the render window.</param>
		/// <param name="format">Back buffer format of the render window.</param>
		/// <param name="usedepth">TRUE to use a depth buffer, FALSE to exclude.</param>
		/// <param name="priority">Priority of the render window.</param>
		/// <param name="preserveBackBuffer">TRUE to preserve the contents of the back buffer when the target is updated, FALSE to discard.</param>
		/// <returns>A render window.</returns>
		public RenderWindow CreateRenderWindow(string name, Control owner, int width, int height, BackBufferFormats format, bool usedepth, int priority, bool preserveBackBuffer)
		{
			return CreateRenderWindow(name,owner,width,height,format,usedepth,false,priority, preserveBackBuffer);
		}

		/// <summary>
		/// Function to create a new render window.
		/// </summary>
		/// <param name="name">Name of this render window.</param>
		/// <param name="owner">Owning window of this render window.</param>
		/// <param name="width">Width of the render window.</param>
		/// <param name="height">Height of the render window.</param>
		/// <param name="format">Back buffer format of the render window.</param>
		/// <param name="priority">Priority of the render window.</param>
		/// <param name="preserveBackBuffer">TRUE to preserve the contents of the back buffer when the target is updated, FALSE to discard.</param>
		/// <returns>A render window.</returns>
		public RenderWindow CreateRenderWindow(string name, Control owner, int width, int height, BackBufferFormats format, int priority, bool preserveBackBuffer)
		{
			return CreateRenderWindow(name,owner,width,height,format,false,false,priority, preserveBackBuffer);
		}

		/// <summary>
		/// Function to create a new render window.
		/// </summary>
		/// <param name="name">Name of this render window.</param>
		/// <param name="owner">Owning window of this render window.</param>
		/// <param name="width">Width of the render window.</param>
		/// <param name="height">Height of the render window.</param>
		/// <param name="priority">Priority of the render window.</param>
		/// <param name="preserveBackBuffer">TRUE to preserve the contents of the back buffer when the target is updated, FALSE to discard.</param>
		/// <returns>A render window.</returns>
		public RenderWindow CreateRenderWindow(string name, Control owner, int width, int height, int priority, bool preserveBackBuffer)
		{
			return CreateRenderWindow(name,owner,width,height,Gorgon.DesktopVideoMode.Format,false,false,priority, preserveBackBuffer);
		}

		/// <summary>
		/// Function to create a new render window.
		/// </summary>
		/// <param name="name">Name of this render window.</param>
		/// <param name="owner">Owning window of this render window.</param>
		/// <param name="priority">Priority of the render window.</param>
		/// <param name="preserveBackBuffer">TRUE to preserve the contents of the back buffer when the target is updated, FALSE to discard.</param>
		/// <returns>A render window.</returns>
		public RenderWindow CreateRenderWindow(string name,Control owner,int priority, bool preserveBackBuffer)
		{
			return CreateRenderWindow(name,owner,owner.ClientSize.Width,owner.ClientSize.Height,Gorgon.DesktopVideoMode.Format,false,false,priority,preserveBackBuffer);
		}
		
		/// <summary>
		/// Function to remove a render target by index.
		/// </summary>
		/// <param name="index">Index of render target to remove.</param>
		public override void Remove(int index)
		{
			if ((index < 0) || (index >= _items.Count))
				throw new IndexOutOfBoundsException(index);

			// If we remove the primary window, shut it all down.
			if (_items[index].Name == "@PrimaryWindow")
			{
				Clear();
				return;
			}

			_renderTargets.Remove(_items[index].Name);
			_items[index].Dispose();
			_items.RemoveAt(index);			

			Sort();
		}

		/// <summary>
		/// Function to remove a render target by name.
		/// </summary>
		/// <param name="key">Name of the render target to remove.</param>
		public void Remove(string key)
		{
			if (!_renderTargets.ContainsKey(key))
				throw new SharpUtilities.Collections.KeyNotFoundException(key);

			// If we remove the primary window, shut it all down.
			if (_renderTargets[key].Name == "@PrimaryWindow")
			{
				Clear();
				return;
			}

			_items.Remove(_renderTargets[key]);
			_renderTargets[key].Dispose();
			_renderTargets.Remove(key);
			Sort();
		}


		/// <summary>
		/// Function to clear the render target list.
		/// </summary>
		public override void Clear()
		{
			if (Count > 0)
			{
				Gorgon.Log.Print("RenderTargetList", "Destroying render targets...", LoggingLevel.Verbose);

				// Destroy all render targets except the primary.
				foreach (RenderTarget rt in this)
				{
					if (rt.Name != "@PrimaryWindow")
						rt.Dispose();
				}

				// Destroy the primary render target.
				_primary.Dispose();
				_primary = null;

				// Remove targets from the list.
				_items.Clear();
				_renderTargets.Clear();
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal RenderTargetList()
		{
			_primary = null;
			_comparer = new RenderTargetComparer();
			_renderTargets = new SortedList<string,RenderTarget>();
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~RenderTargetList()
		{
			Dispose(false);
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected void Dispose(bool disposing)
		{
			if (disposing)
				Clear();
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
