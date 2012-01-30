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
// Created: Tuesday, July 19, 2011 8:55:06 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Drawing;
using System.Runtime.InteropServices;
using SharpDX;
using GI = SharpDX.DXGI;
using D3D = SharpDX.Direct3D11;
using GorgonLibrary.Collections;
using GorgonLibrary.Collections.Specialized;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Native;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Operators used for comparison operations.
	/// </summary>
	public enum ComparisonOperators
	{
		/// <summary>
		/// Never pass the comparison.
		/// </summary>
		Never = 1,
		/// <summary>
		/// If the source data is less than the destination data, the comparison passes.
		/// </summary>
		Less = 2,
		/// <summary>
		/// If the source data is equal to the destination data, the comparison passes.
		/// </summary>
		Equal = 3,
		/// <summary>
		/// If the source data is less than or equal to the destination data, the comparison passes.
		/// </summary>
		LessEqual = 4,
		/// <summary>
		/// If the source data is greater than the destination data, the comparison passes.
		/// </summary>
		Greater = 5,
		/// <summary>
		/// If the source data is not equal to the destination data, the comparison passes.
		/// </summary>
		NotEqual = 6,
		/// <summary>
		/// If the source data is greater than or equal to the destination data, the comparison passes.
		/// </summary>
		GreaterEqual = 7,
		/// <summary>
		/// Always pass the comparison.
		/// </summary>
		Always = 8,
	}

	/// <summary>
	/// The primary object for the graphics sub system.
	/// </summary>
	/// <remarks>This interface is used to create all objects (buffers, shaders, etc...) that are to be used for graphics.  An interface is tied to a single physical video device, to use 
	/// multiple video devices, create additional graphics interfaces and assign the device to the <see cref="GorgonLibrary.Graphics.GorgonGraphics.VideoDevice">VideoDevice</see> property.
	/// <para>This object will enumerate video devices, monitor outputs (for multi-head adapters), and video modes for each of the video devices in the system upon creation.  These
	/// items are accessible in the <see cref="P:GorgonLibrary.Graphics.GorgonGraphics.VideoDevices">VideoDevices</see> property.  The user may force a new enumeration by calling the 
	/// <see cref="M:GorgonLibrary.Graphics.GorgonGraphics.GorgonVideoDeviceCollection.Refresh">VideoDevices.Refresh</see> method.  Please note that doing so will invalidate any objects that were created with 
	/// this interface, and consequently they will need to be <see cref="E:GorgonLibrary.Graphics.GorgonGraphics.BeforeDeviceEnumeration">destroyed before enumeration</see> and <see cref="E:GorgonLibrary.Graphics.GorgonGraphics.AfterDeviceEnumeration">recreated</see>.  This will also reset the current video device will be set to the first video device in the list.</para>
	/// <para>When switching video devices, ensure that all of your objects created by this interface are destroyed.  If they are not, then the graphics interface will attempt to 
	/// destroy any objects that it is aware of for you.  While this may be a convenience, it is better practice to handle the <see cref="E:GorgonLibrary.Graphics.GorgonGraphics.BeforeVideoDeviceChange">BeforeVideoDeviceChange</see> 
	/// event yourself.  You may recreate the objects in the <see cref="E:GorgonLibrary.Graphics.GorgonGraphics.AfterVideoDeviceChange">AfterVideoDeviceChange</see> event using the same interface.</para>
	/// <para>Please note that this object requires Direct3D 11 (but not necessarily a Direct3D 11 video card) and at least Windows Vista Service Pack 2 or higher.  Windows XP and operating systems before it will not work, and an exception will be thrown 
	/// if this object is created on those platforms.</para>
	/// </remarks>
	public class GorgonGraphics
		: IDisposable
	{
		#region Variables.
		private static bool _isDWMEnabled = true;						// Flag to indicate that the desktop window manager compositor is enabled.
		private static bool _dontEnableDWM = false;						// Flag to indicate that we should not enable the DWM.
		private bool _disposed = false;									// Flag to indicate that the object was disposed.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the DX GI factory.
		/// </summary>
		internal GI.Factory1 GIFactory
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the list of tracked objects.
		/// </summary>
		internal GorgonTrackedObjectCollection TrackedObjects
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the current device context.
		/// </summary>
		internal D3D.DeviceContext Context
		{
			get
			{
#if DEBUG
				if ((VideoDevice == null) || (VideoDevice.D3DDevice == null))
					return null;
#endif

				return VideoDevice.D3DDevice.ImmediateContext;
			}
		}

		/// <summary>
		/// Property to return the Direct3D 11 device object.
		/// </summary>
		internal D3D.Device D3DDevice
		{
			get;
			private set;
		}
		
		/// <summary>
		/// Property to set or return whether DWM composition is enabled or not.
		/// </summary>
		/// <remarks>This property will have no effect on systems that initially have the desktop window manager compositor disabled.</remarks>
		public static bool IsDWMCompositionEnabled
		{
			get
			{
				return _isDWMEnabled;
			}
			set
			{
				if (!value)
				{
					if (_isDWMEnabled)
					{
						Win32API.DwmEnableComposition(0);
						_isDWMEnabled = false;
					}
				}
				else
				{
					if ((!_isDWMEnabled) && (!_dontEnableDWM))
					{
						Win32API.DwmEnableComposition(1);
						_isDWMEnabled = true;
					}
				}
			}
		}

		/// <summary>
		/// Property to set or return the input bindings.
		/// </summary>
		public GorgonInputBindings InputBindings
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the current rasterizer states.
		/// </summary>
		public GorgonRasterizerRenderState Rasterizer
		{
			get;
			private set;
		}		

		/// <summary>
		/// Property to return the current blending states.
		/// </summary>
		public GorgonBlendRenderState Blending
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the current depth/stencil states.
		/// </summary>
		public GorgonDepthStencilRenderState DepthStencil
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the current vertex shader states.
		/// </summary>
		public GorgonVertexShaderState VertexShader
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the current vertex shader states.
		/// </summary>
		public GorgonPixelShaderState PixelShader
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return whether object tracking is disabled.
		/// </summary>
		/// <remarks>This will enable SharpDX's object tracking to ensure references are destroyed upon application exit.
		/// <para>The default value for DEBUG mode is TRUE, and for RELEASE it is set to FALSE.  Disabling object tracking will
		/// give a slight performance increase.</para>
		/// </remarks>
		public bool IsObjectTrackingEnabled
		{
			get
			{
				return SharpDX.Configuration.EnableObjectTracking;
			}
			set
			{
				SharpDX.Configuration.EnableObjectTracking = value;
			}
		}

		/// <summary>
		/// Property to set or return the video device to use for this graphics interface.
		/// </summary>
		/// <remarks>When this value is set to NULL (Nothing in VB.Net), then the first video device in the <see cref="P:GorgonLibrary.Graphics.GorgonGraphics.VideoDevices">VideoDevices</see> collection will be returned.
		/// <para>When the device is changed, all resources associated with the device (swap chains, buffers, etc...) will be destroyed.  The user will be responsible for re-creating these resources, and should do so in the 
		/// <see cref="E:GorgonLibrary.Graphics.GorgonGraphics.AfterDeviceChange">AfterDeviceChange</see> event.</para>
		/// </remarks>
		public GorgonVideoDevice VideoDevice
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return whether swap chains should reset their full screen setting on regaining focus.
		/// </summary>
		/// <remarks>
		/// This will control whether Gorgon will try to reacquire full screen mode when a full screen swap chain window regains focus.  When this is set to FALSE, and the window 
		/// containing the full screen swap chain loses focus, it will revert to windowed mode and remain in windowed mode.  When set to TRUE, it will try to reacquire full screen mode.
		/// <para>The default value for this is TRUE.  However, for a full screen multimonitor scenario, this should be set to FALSE.</para>
		/// </remarks>
		public bool ResetFullscreenOnFocus
		{
			get;
			set;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clean up the state objects.
		/// </summary>
		private void DestroyStates()
		{
			if (PixelShader != null)
				PixelShader.Dispose();
			if (VertexShader != null)
				VertexShader.Dispose();

			if (Rasterizer != null)
				((IDisposable)Rasterizer).Dispose();
			if (DepthStencil != null)
				((IDisposable)DepthStencil).Dispose();
			if (Blending != null)
				((IDisposable)Blending).Dispose();
		}

		/// <summary>
		/// Function to create any default states.
		/// </summary>
		private void CreateDefaultStates()
		{
			InputBindings = new GorgonInputBindings(this);
			VertexShader = new GorgonVertexShaderState(this);
			PixelShader = new GorgonPixelShaderState(this);

			Rasterizer = new GorgonRasterizerRenderState(this);
			Rasterizer.States = GorgonRasterizerStates.DefaultStates;

			DepthStencil = new GorgonDepthStencilRenderState(this);
			DepthStencil.States = GorgonDepthStencilStates.DefaultStates;

			Blending = new GorgonBlendRenderState(this);
			Blending.States = GorgonBlendStates.DefaultStates;
		}

		/// <summary>
		/// Function to return the currently active full screen swap chains.
		/// </summary>
		/// <returns>A list of full screen swap chains.</returns>
		internal IEnumerable<GorgonSwapChain> GetFullscreenSwapChains()
		{
			return (from item in TrackedObjects
					let swapChain = item as GorgonSwapChain
					where (swapChain != null) && (!swapChain.Settings.IsWindowed)
					select swapChain);
		}

		/// <summary>
		/// Function to remove a tracked object from the collection.
		/// </summary>
		/// <param name="trackedObject">Tracked object to remove.</param>
		internal void RemoveTrackedObject(IDisposable trackedObject)
		{
			TrackedObjects.Remove(trackedObject);
		}

		/// <summary>
		/// Function to create an input layout object.
		/// </summary>
		/// <param name="name">Name of the input layout.</param>
		/// <param name="shader">The shader that holds the input layout signature.</param>
		/// <returns>The input layout object to create.</returns>
		/// <exception cref="System.ArgumentException">Thrown when then name parameter is an empty string.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="shader"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="name"/> parameter is NULL.</para>
		/// </exception>
		/// <remarks>The shader parameter is used to compare input layout on the shader side with the input layout.  If the layout is mismatched, a warning will appear in the debug output.
		/// <para>Note that any shader can be used with the input layout as long as the shader contains the same layout for the input, i.e. there is no need to create a new layout for each shader if the element layouts are identical.</para>
		/// </remarks>
		public GorgonInputLayout CreateInputLayout(string name, GorgonShader shader)
		{
			GorgonInputLayout layout = null;

			GorgonDebug.AssertNull<GorgonShader>(shader, "shader");
			layout = new GorgonInputLayout(this, name, shader);

			TrackedObjects.Add(layout);
			return layout;
		}

		/// <summary>
		/// Function to create an input layout object from a predefined type.
		/// </summary>
		/// <param name="name">Name of the input layout.</param>
		/// <param name="type">Type to evaluate.</param>
		/// <param name="shader">The shader that holds the input layout signature.</param>
		/// <returns>The input layout object to create.</returns>
		/// <exception cref="System.ArgumentException">Thrown when then <paramref name="name"/> parameter is an empty string.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="shader"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="type"/> parameter is NULL.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the name parameter is NULL.</para>
		/// </exception>
		/// <remarks>The shader parameter is used to compare input layout on the shader side with the input layout.  If the layout is mismatched, a warning will appear in the debug output.
		/// <para>Note that any shader can be used with the input layout as long as the shader contains the same layout for the input, i.e. there is no need to create a new layout for each shader if the element layouts are identical.</para>
		/// </remarks>
		public GorgonInputLayout CreateInputLayout(string name, Type type, GorgonShader shader)
		{
			GorgonInputLayout layout = null;

			GorgonDebug.AssertNull<Type>(type, "type");
			GorgonDebug.AssertNull<GorgonShader>(shader, "shader");

			layout = new GorgonInputLayout(this, name, shader);
			layout.GetLayoutFromType(type);

			TrackedObjects.Add(layout);

			return layout;
		}


		/// <summary>
		/// Function to create a constant buffer.
		/// </summary>
		/// <param name="size">Size of the buffer, in bytes.</param>
		/// <param name="allowCPUWrite">TRUE to allow the CPU to write to the buffer, FALSE to disallow.</param>
		/// <returns>A new constant buffer.</returns>
		public GorgonConstantBuffer CreateConstantBuffer(int size, bool allowCPUWrite)
		{
			GorgonConstantBuffer buffer = new GorgonConstantBuffer(this, size, allowCPUWrite);
			buffer.Initialize(null);

			TrackedObjects.Add(buffer);
			return buffer;
		}

		/// <summary>
		/// Function to create a constant buffer and initializes it with data.
		/// </summary>
		/// <typeparam name="T">Type of data to pass to the constant buffer.</typeparam>
		/// <param name="value">Value to write to the buffer</param>
		/// <param name="allowCPUWrite">TRUE to allow the CPU to write to the buffer, FALSE to disallow.</param>
		/// <returns>A new constant buffer.</returns>
		public GorgonConstantBuffer CreateConstantBuffer<T>(T value, bool allowCPUWrite)
			where T : struct
		{
			using (GorgonDataStream stream = GorgonDataStream.ValueTypeToStream<T>(value))
			{
				GorgonConstantBuffer buffer = new GorgonConstantBuffer(this, (int)stream.Length, allowCPUWrite);
				buffer.Initialize(stream);

				TrackedObjects.Add(buffer);
				return buffer;
			}
		}

		/// <summary>
		/// Function to create a index buffer.
		/// </summary>
		/// <param name="size">Size of the buffer, in bytes.</param>
		/// <param name="is32bit">TRUE to indicate that we're using 32 bit indices, FALSE to use 16 bit indices </param>
		/// <param name="usage">Usage of the buffer.</param>
		/// <returns>A new index buffer.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than 1.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="usage"/> parameter is set to Staging or Immutable.
		/// </exception>
		public GorgonIndexBuffer CreateIndexBuffer(int size, bool is32bit, BufferUsage usage)
		{
			if ((usage == BufferUsage.Staging) || (usage == BufferUsage.Immutable))
				throw new ArgumentException("A index buffer cannot be used as a staging or immutable buffer.", "usage");

			return CreateIndexBuffer(size, usage, is32bit, null);
		}

		/// <summary>
		/// Function to create a index buffer.
		/// </summary>
		/// <param name="usage">Usage of the buffer.</param>
		/// <param name="is32bit">TRUE to indicate that we're using 32 bit indices, FALSE to use 16 bit indices </param>
		/// <param name="data">Data used to initialize the buffer.</param>
		/// <typeparam name="T">Type of data used to populate the buffer.</typeparam>
		/// <returns>A new index buffer.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="usage"/> parameter is set to Staging.
		/// <para>-or-</para>
		/// <para>Thrown when the usage parameter is set to Immutable and the <paramref name="data"/> is NULL (Nothing in VB.Net) or empty.</para>
		/// </exception>
		/// <remarks>If creating an immutable index buffer, be sure to pre-populate it via the initialData parameter.</remarks>
		public GorgonIndexBuffer CreateIndexBuffer<T>(BufferUsage usage, bool is32bit, IList<T> data)
			where T : struct
		{
			int size = data.Count * DirectAccess.SizeOf<T>();

			using (GorgonDataStream dataStream = new GorgonDataStream(size))
			{
				for (int i = 0; i < data.Count; i++)
					dataStream.Write<T>(data[i]);
				dataStream.Position = 0;
				return CreateIndexBuffer(size, usage, is32bit, dataStream);				
			}
		}

		/// <summary>
		/// Function to create a index buffer.
		/// </summary>
		/// <param name="size">Size of the buffer, in bytes.</param>
		/// <param name="usage">Usage of the buffer.</param>
		/// <param name="is32bit">TRUE to indicate that we're using 32 bit indices, FALSE to use 16 bit indices </param>
		/// <param name="initialData">Initial data to populate the index buffer with.</param>
		/// <returns>A new index buffer.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than 1.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="usage"/> parameter is set to Staging.
		/// <para>-or-</para>
		/// <para>Thrown when the usage parameter is set to Immutable and the <paramref name="initialData"/> is NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <remarks>If creating an immutable index buffer, be sure to pre-populate it via the initialData parameter.</remarks>
		public GorgonIndexBuffer CreateIndexBuffer(int size, BufferUsage usage, bool is32bit, GorgonDataStream initialData)
		{
			if (size < 1)
				throw new ArgumentOutOfRangeException("size", "A index buffer needs at least 1 byte.");

			if (usage == BufferUsage.Staging)
				throw new ArgumentException("A index buffer cannot be used as a staging buffer.", "usage");

			if ((usage == BufferUsage.Immutable) && ((initialData == null) || (initialData.Length == 0)))
				throw new ArgumentException("Cannot create an immutable buffer without initial data to populate it.", "usage");

			GorgonIndexBuffer buffer = new GorgonIndexBuffer(this, usage, size, is32bit);
			buffer.Initialize(initialData);

			TrackedObjects.Add(buffer);
			return buffer;
		}

		/// <summary>
		/// Function to create a vertex buffer.
		/// </summary>
		/// <param name="size">Size of the buffer, in bytes.</param>
		/// <param name="usage">Usage of the buffer.</param>
		/// <returns>A new vertex buffer.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than 1.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="usage"/> parameter is set to Staging or Immutable.
		/// </exception>
		public GorgonVertexBuffer CreateVertexBuffer(int size, BufferUsage usage)
		{
			if ((usage == BufferUsage.Staging) || (usage == BufferUsage.Immutable))
				throw new ArgumentException("A vertex buffer cannot be used as a staging or immutable buffer.", "usage");

			return CreateVertexBuffer(size, usage, null);
		}

		/// <summary>
		/// Function to create a vertex buffer.
		/// </summary>
		/// <param name="usage">Usage of the buffer.</param>
		/// <param name="data">Data used to initialize the buffer.</param>
		/// <typeparam name="T">Type of data used to populate the buffer.</typeparam>
		/// <returns>A new vertex buffer.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="usage"/> parameter is set to Staging.
		/// <para>-or-</para>
		/// <para>Thrown when the usage parameter is set to Immutable and the <paramref name="data"/> is NULL (Nothing in VB.Net) or empty.</para>
		/// </exception>
		/// <remarks>If creating an immutable vertex buffer, be sure to pre-populate it via the initialData parameter.</remarks>
		public GorgonVertexBuffer CreateVertexBuffer<T>(BufferUsage usage, IList<T> data)
			where T : struct
		{
			GorgonDebug.AssertNull<IList<T>>(data, "data");
			int size = data.Count * DirectAccess.SizeOf<T>();

			using (GorgonDataStream dataStream = new GorgonDataStream(size))
			{
				for (int i = 0; i < data.Count; i++)
					dataStream.Write<T>(data[i]);
				dataStream.Position = 0;
				return CreateVertexBuffer(size, usage, dataStream);
			}
		}

		/// <summary>
		/// Function to create a vertex buffer.
		/// </summary>
		/// <param name="size">Size of the buffer, in bytes.</param>
		/// <param name="usage">Usage of the buffer.</param>
		/// <param name="initialData">Initial data to populate the vertex buffer with.</param>
		/// <returns>A new vertex buffer.</returns>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="size"/> parameter is less than 1.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="usage"/> parameter is set to Staging.
		/// <para>-or-</para>
		/// <para>Thrown when the usage parameter is set to Immutable and the <paramref name="initialData"/> is NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <remarks>If creating an immutable vertex buffer, be sure to pre-populate it via the initialData parameter.</remarks>
		public GorgonVertexBuffer CreateVertexBuffer(int size, BufferUsage usage, GorgonDataStream initialData)
		{
			if (size < 1)
				throw new ArgumentOutOfRangeException("size", "A vertex buffer needs at least 1 byte.");

			if (usage == BufferUsage.Staging)
				throw new ArgumentException("A vertex buffer cannot be used as a staging buffer.", "usage");

			if ((usage == BufferUsage.Immutable) && ((initialData == null) || (initialData.Length == 0)))
				throw new ArgumentException("Cannot create an immutable buffer without initial data to populate it.", "usage");

			GorgonVertexBuffer buffer = new GorgonVertexBuffer(this, usage, size);
			buffer.Initialize(initialData);

			TrackedObjects.Add(buffer);
			return buffer;
		}

		/// <summary>
		/// Function to create a vertex shader.
		/// </summary>
		/// <param name="name">Name of the vertex shader.</param>
		/// <param name="entryPoint">Entry point for the shader.</param>
		/// <param name="sourceCode">Source code for the shader.</param>
		/// <returns>A new vertex shader.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> or <paramref name="entryPoint"/> parameters are empty strings.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the name or entryPoint parameters are NULL (Nothing in VB.Net).</exception>
		public GorgonVertexShader CreateVertexShader(string name, string entryPoint, string sourceCode)
		{
			GorgonVertexShader shader = null;

			GorgonDebug.AssertParamString(name, "name");
			GorgonDebug.AssertParamString(entryPoint, "entryPoint");

			shader = new GorgonVertexShader(this, name, entryPoint);
			shader.SourceCode = sourceCode;
			shader.Compile();
			TrackedObjects.Add(shader);

			return shader;
		}

		/// <summary>
		/// Function to create a pixel shader.
		/// </summary>
		/// <param name="name">Name of the pixel shader.</param>
		/// <param name="entryPoint">Entry point for the shader.</param>
		/// <param name="sourceCode">Source code for the shader.</param>
		/// <returns>A new pixel shader.</returns>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="name"/> or <paramref name="entryPoint"/> parameters are empty strings.</exception>
		/// <exception cref="System.ArgumentNullException">Thrown when the name or entryPoint parameters are NULL (Nothing in VB.Net).</exception>
		public GorgonPixelShader CreatePixelShader(string name, string entryPoint, string sourceCode)
		{
			GorgonPixelShader shader = null;

			GorgonDebug.AssertParamString(name, "name");
			GorgonDebug.AssertParamString(entryPoint, "entryPoint");

			shader = new GorgonPixelShader(this, name, entryPoint);
			shader.SourceCode = sourceCode;
			shader.Compile();
			TrackedObjects.Add(shader);

			return shader;
		}

		/// <summary>
		/// Function to create a swap chain.
		/// </summary>
		/// <param name="name">Name of the swap chain.</param>
		/// <param name="settings">Settings for the swap chain.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.Window">GorgonSwapChainSettings.Window</see> property is NULL (Nothing in VB.Net), and the <see cref="P:GorgonLibrary.Gorgon.ApplicationForm">Gorgon application window</see> is NULL.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonVideoMode.Format">GorgonSwapChainSettings.VideoMode.Format</see> property cannot be used by the video device for displaying data.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultSamples.Quality">GorgonSwapChainSettings.MultiSamples.Quality</see> property is less than 0 or not less than the value returned by <see cref="M:GorgonLibrary.Graphics.GorgonVideoDevice">GorgonVideoDevice.GetMultiSampleQuality</see>.</para>
		/// </exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when the video output could not be determined from the window.
		/// <para>-or-</para>
		/// <para>Thrown when the swap chain is going to full screen mode and another swap chain is already on the video output.</para>
		/// </exception>
		/// <remarks>This will create our output swap chains for display to a window or control.  All functionality for sending or retrieving data from the video device can be accessed through the swap chain.
		/// <para>Passing default settings for the <see cref="GorgonLibrary.Graphics.GorgonSwapChainSettings">settings parameters</see> will make Gorgon choose the closest possible settings appropriate for the video device and output that the window is on.  For example, passing NULL (Nothing in VB.Net) to 
		/// the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.VideoMode">GorgonSwapChainSettings.VideoMode</see> parameter will make Gorgon find the closest video mode available to the current window size and desktop format (for the output).</para>
		/// <para>If the multisampling quality in the <see cref="P:GorgonLibrary.Graphics.GorgonSwapChainSettings.MultSamples.Quality">GorgonSwapChainSettings.MultiSamples.Quality</see> property is higher than what the video device can support, an exception will be raised.  To determine 
		/// what the maximum quality for the sample count for the video device should be, call the <see cref="M:GorgonLibrary.Graphics.GorgonVideoDevice.GetMultiSampleQuality">GorgonVideoDevice.GetMultiSampleQuality</see> method.</para>
		/// </remarks>
		public GorgonSwapChain CreateSwapChain(string name, GorgonSwapChainSettings settings)
		{
			GorgonSwapChain swapChain = null;

			GorgonDebug.AssertNull<GorgonSwapChainSettings>(settings, "settings");

			GorgonSwapChain.ValidateSwapChainSettings(this, settings);

			swapChain = new GorgonSwapChain(this, name, settings);
			TrackedObjects.Add(swapChain);
			swapChain.Initialize();

			return swapChain;
		}

		/// <summary>
		/// Function to create a depth/stencil buffer.
		/// </summary>
		/// <param name="name">Name of the depth/stencil buffer.</param>
		/// <param name="settings">Settings to apply to the depth/stencil buffer.</param>
		/// <returns>A new depth/stencil buffer.</returns>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="name"/> parameter is NULL (Nothing in VB.Net).
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="settings"/> parameter is NULL (Nothing in VB.Net).</para>
		/// </exception>
		/// <exception cref="System.ArgumentException">Thrown when the name parameter is an empty string.
		/// <para>-or-</para>
		/// <para>Thrown when the <see cref="P:GorgonLibrary.Graphics.GorgonDepthStencilSettings.Format">GorgonDepthStencilSettings.Format</see> property is set to Unknown or is unsupported.</para>
		/// </exception>
		/// <remarks>
		/// A depth buffer may be paired with a swapchain or render target through its DepthStencil property.  When pairing the depth/stencil to the render target, Ensure that the depth/stencil buffer width, height and multisample settings match that of the render target that it is paired with.
		/// <para>The texture for a depth/stencil may be used in a shader for cards that have a feature level of SM_4_1 or better, and can be set to do so by setting the <see cref="P:GorgonLibrary.Graphics.GorgonDepthStencilSettings.TextureFormat">GorgonDepthStencilSettings.TextureFormat</see> property to a typeless format. 
		/// If this is attempted on a video device that has a feature level of SM_4_0 or below, then an exception will be raised.</para>
		/// </remarks>
		public GorgonDepthStencil CreateDepthStencil(string name, GorgonDepthStencilSettings settings)
		{
			GorgonDepthStencil depthBuffer = null;

			GorgonDebug.AssertNull<GorgonDepthStencilSettings>(settings, "settings");

			GorgonDepthStencil.ValidateSettings(this, settings);

			depthBuffer = new GorgonDepthStencil(this, name, settings);
			TrackedObjects.Add(depthBuffer);
			depthBuffer.UpdateSettings();

			return depthBuffer;
		}

		/// <summary>
		/// Function to draw the scene.
		/// </summary>
		/// <remarks>This method applies the various states, shaders and renders the polygons in the vertex buffer to the scene.</remarks>
		/// <exception cref="System.InvalidOperationException">Thrown when there is no valid context to draw with.</exception>
		public void Draw()
		{
#if DEBUG			
			if (Context == null)
				throw new InvalidOperationException("There is no context available for the draw commands.");
#endif
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes the <see cref="GorgonGraphics"/> class.
		/// </summary>
		/// <param name="device">Video device to use.</param>
		/// <param name="featureLevel">The maximum feature level to support for the devices enumerated.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="featureLevel"/> parameter is invalid.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when Gorgon could not find any video devices that are Shader Model 5, or the down level interfaces (Shader Model 4, and lesser).
		/// <para>-or-</para>
		/// <para>Thrown if the operating system version is not supported.  Gorgon Graphics requires at least Windows Vista Service Pack 2 or higher.</para>
		/// </exception>
		/// <remarks>
		/// The <paramref name="device"/> parameter is the video device that should be used with Gorgon.  If the user passes NULL (Nothing in VB.Net), then the primary device will be used. 
		/// To determine the devices on the system, check the <see cref="P:GorgonLibrary.Graphics.GorgonVideoDeviceCollection">GorgonVideoDeviceCollection</see> object.  The primary device will be the first device in this collection. 
		/// <para>The user may pass in a feature level to the featureLevel parameter to limit the feature levels available.  Note that the feature levels imply all feature levels up until the feature level passed in, for example, passing <c>DeviceFeatureLevel.SM4</c> will only allow functionality 
		/// for both Shader Model 4, and Shader Model 2/3 capable video devices, while DeviceFeatureLevel.SM4_1 will include Shader Model 4 with a 4.1 profile and Shader model 2/3 video devices.</para>
		/// <para>If a feature level is not supported by the hardware, then Gorgon will not use that feature level.  That is, passing a SM5 feature level with a SM4 card will only use a SM4 feature level.  If the user omits the feature level (in one of the constructor 
		/// overloads), then Gorgon will use the best available feature level for the video device being used.</para>
		/// </remarks>
		public GorgonGraphics(GorgonVideoDevice device, DeviceFeatureLevel featureLevel)
		{
			GorgonVideoDeviceCollection enumerator = null;

			if (featureLevel == DeviceFeatureLevel.Unsupported)
				throw new ArgumentException("Must supply a known feature level.", "featureLevel");

			if (GorgonComputerInfo.OperatingSystemVersion.Major < 6)
				throw new GorgonException(GorgonResult.CannotCreate, "The Gorgon Graphics interface requires Windows Vista Service Pack 2 or greater.");

			TrackedObjects = new GorgonTrackedObjectCollection();
			ResetFullscreenOnFocus = true;

			Gorgon.Log.Print("Gorgon Graphics initializing...", Diagnostics.LoggingLevel.Simple);
			
#if DEBUG
			SharpDX.Configuration.EnableObjectTracking = true;
#else
			SharpDX.Configuration.EnableObjectTracking = false;
#endif

			if (device == null)
			{
				enumerator = new GorgonVideoDeviceCollection(false, false);
				device = enumerator[0];
			}

			if (device != null)
			{
				GIFactory = device.GIFactory;
				VideoDevice = device;
				D3DDevice = device.D3DDevice = device.CreateD3DDevice(featureLevel);				
			}

			if (enumerator != null)
				enumerator.Dispose();

			Gorgon.AddTrackedObject(this);

			// Create default state objects.
			CreateDefaultStates();

			Gorgon.Log.Print("Gorgon Graphics initialized.", Diagnostics.LoggingLevel.Simple);
		}

		/// <summary>
		/// Initializes the <see cref="GorgonGraphics"/> class.
		/// </summary>
		/// <param name="device">Video device to use.</param>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when Gorgon could not find any video devices that are Shader Model 5, or the down level interfaces (Shader Model 4, and lesser).
		/// <para>-or-</para>
		/// <para>Thrown if the operating system version is not supported.  Gorgon Graphics requires at least Windows Vista Service Pack 2 or higher.</para>
		/// </exception>
		/// <remarks>
		/// The <paramref name="device"/> parameter is the video device that should be used with Gorgon.  If the user passes NULL (Nothing in VB.Net), then the primary device will be used. 
		/// To determine the devices on the system, check the <see cref="P:GorgonLibrary.Graphics.GorgonVideoDeviceCollection">GorgonVideoDeviceCollection</see> object.  The primary device will be the first device in this collection. 
		/// </remarks>
		public GorgonGraphics(GorgonVideoDevice device)
			: this(device, DeviceFeatureLevel.SM5)
		{
		}

		/// <summary>
		/// Initializes the <see cref="GorgonGraphics"/> class.
		/// </summary>
		/// <param name="featureLevel">The maximum feature level to support for the devices enumerated.</param>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="featureLevel"/> parameter is invalid.</exception>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when Gorgon could not find any video devices that are Shader Model 5, or the down level interfaces (Shader Model 4, and lesser).
		/// <para>-or-</para>
		/// <para>Thrown if the operating system version is not supported.  Gorgon Graphics requires at least Windows Vista Service Pack 2 or higher.</para>
		/// </exception>
		/// <remarks>The user may pass in a feature level to the featureLevel parameter to limit the feature levels available.  Note that the feature levels imply all feature levels up until the feature level passed in, for example, passing <c>DeviceFeatureLevel.SM4</c> will only allow functionality 
		/// for both Shader Model 4, and Shader Model 2/3 capable video devices, while DeviceFeatureLevel.SM4_1 will include Shader Model 4 with a 4.1 profile and Shader model 2/3 video devices.
		/// <para>If a feature level is not supported by the hardware, then Gorgon will not use that feature level.</para>
		/// </remarks>
		public GorgonGraphics(DeviceFeatureLevel featureLevel)
			: this(null, featureLevel)
		{
		}

		/// <summary>
		/// Initializes the <see cref="GorgonGraphics"/> class.
		/// </summary>
		/// <exception cref="GorgonLibrary.GorgonException">Thrown when Gorgon could not find any video devices that are Shader Model 5, or the down level interfaces (Shader Model 4, and lesser).
		/// <para>-or-</para>
		/// <para>Thrown if the operating system version is not supported.  Gorgon Graphics requires at least Windows Vista Service Pack 2 or higher.</para>
		/// </exception>
		public GorgonGraphics()
			: this(null, DeviceFeatureLevel.SM5)
		{
		}

		/// <summary>
		/// Initializes the <see cref="GorgonGraphics"/> class.
		/// </summary>
		static GorgonGraphics()
		{
			Win32API.DwmIsCompositionEnabled(out _isDWMEnabled);
			if (!_isDWMEnabled)
				_dontEnableDWM = true;
		}
		#endregion

		#region IDisposable Members
		/// <summary>
		/// Releases unmanaged and - optionally - managed resources
		/// </summary>
		/// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
		private void Dispose(bool disposing)
		{
			if (!_disposed)
			{
				if (disposing)
				{
					Gorgon.Log.Print("Gorgon Graphics shutting down...", Diagnostics.LoggingLevel.Simple);

					TrackedObjects.ReleaseAll();
					DestroyStates();

					Gorgon.Log.Print("Removing D3D11 Device object...", LoggingLevel.Verbose);
					if (D3DDevice != null)
					{
						D3DDevice.Dispose();
						D3DDevice = null;
					}

					// Destroy the video device interface.
					if (VideoDevice != null)
					{
						Context.ClearState();
						VideoDevice.D3DDevice = null;
						((IDisposable)VideoDevice).Dispose();
					}

					Gorgon.Log.Print("Removing DXGI factory interface...", LoggingLevel.Verbose);
					if (GIFactory != null)
					{
						GIFactory.Dispose();
						GIFactory = null;
					}

					// Remove us from the object tracker.
					Gorgon.RemoveTrackedObject(this);

					Gorgon.Log.Print("Gorgon Graphics shut down successfully", Diagnostics.LoggingLevel.Simple);
				}					

				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
