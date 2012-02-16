#region MIT.
// 
// Gorgon.
// Copyright (C) 2012 Michael Winsor
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
// Created: Sunday, January 01, 2012 9:04:36 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using SlimMath;
using GorgonLibrary.Native;
using GorgonLibrary.Diagnostics;
using GorgonLibrary.Collections.Specialized;

namespace GorgonLibrary.Graphics.Renderers
{
	/// <summary>
	/// The renderer for 2D graphics.
	/// </summary>
	/// <remarks>This is the interface to handle sprites, primitives like lines, circles, ellipses, etc... and text.  The 2D renderer allows for using sprites in a 3D space.  
	/// That is, it respects the depth axis so that placing a 2 sprites, one at coordinates (0.1, 0.1, 0.1) and the other at (0.1, 0.1, 1.0), will make the 2nd appear 
	/// smaller.
	/// <para>A default render target is required to use this interface, however that render target can be a render target texture or <see cref="M:GorgonLibrary.Graphics.Output.CreateSwapChain">swap chain</see>.  
	/// Note that this does not mean that this interface is limited to one target, the target can be changed at will via the <see cref="P:GorgonLibrary.Graphics.Renderers.Gorgon2D.Target">Target</see> property.</para>
	/// </remarks>
	public class Gorgon2D
		: IDisposable
	{
		#region Value Types.
		/// <summary>
		/// A vertex for a sprite.
		/// </summary>		
		internal struct Vertex
		{
			/// <summary>
			/// Position of the vertex.
			/// </summary>
			[InputElement(0, "SV_POSITION")]
			public Vector4 Position;
			/// <summary>
			/// Color of the vertex.
			/// </summary>
			[InputElement(1, "COLOR")]
			public Vector4 Color;
			/// <summary>
			/// Texture coordinates.
			/// </summary>
			[InputElement(2, "TEXCOORD")]
			public Vector2 UV;

			/// <summary>
			/// Initializes a new instance of the <see cref="Vertex"/> struct.
			/// </summary>
			/// <param name="position">The position.</param>
			/// <param name="color">The color.</param>
			/// <param name="uv">The texture coordinate.</param>
			public Vertex(Vector4 position, Vector4 color, Vector2 uv)
			{
				Position = position;
				Color = color;
				UV = uv;
			}
		}
		#endregion

		#region Variables.
		private Vertex[] _vertexCache = null;											// List of vertices to cache.
		private int _cacheStart = 0;													// Starting cache vertex buffer index.
		private int _cacheEnd = 0;													// Ending vertex buffer cache index.
		private int _cacheWritten = 0;													// Number of vertices written.
		private bool _disposed = false;													// Flag to indicate that the object was disposed.
		private int _cacheSize = 4096;													// Number of sprites that we can stuff into a vertex buffer.
		private Matrix _defaultProjection = Matrix.Identity;							// Default projection matrix.
		private Matrix _defaultView = Matrix.Identity;									// Default view matrix.
		private Matrix? _projection = null;												// Current projection matrix.
		private Matrix? _view = null;													// Current view matrix.
		private GorgonSwapChain _defaultTarget = null;									// Default render target.
		private GorgonSwapChain _target = null;											// Current render target.	
		private GorgonInputLayout _layout = null;										// Input layout.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return our global vertex buffer.
		/// </summary>
		internal GorgonVertexBuffer VertexBuffer
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return our global index buffer.
		/// </summary>
		internal GorgonIndexBuffer IndexBuffer
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the tracked objects interface.
		/// </summary>
		internal GorgonTrackedObjectCollection TrackedObjects
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to return the shader interface.
		/// </summary>
		public Gorgon2DShaders Shaders
		{
			get;
			private set;
		}


		/// <summary>
		/// Property to set or return the projection matrix.
		/// </summary>
		/// <remarks>The default matrix is an orthographic matrix, which allows to pixel accurate drawing (i.e. a sprite located at 100, 100 is located at the pixel coordinate of 100x100).
		/// <para>When using a perspective matrix, such as 3D, the coordinate system changes to a relative unit system (-1.0f,1.0f).  This allows for depth to be introduced to the sprites.</para>
		/// </remarks>
		public Matrix? ProjectionMatrix
		{
			get
			{
				if (_projection == null)
					return _defaultProjection;
				else
					return _projection;
			}
			set
			{
				_projection = value;
				Shaders.UpdateGorgonTransformation();
			}
		}

		/// <summary>
		/// Property to set or return the view matrix.
		/// </summary>
		/// <remarks>The view matrix is used to relocate the objects in a scene around a camera's position.</remarks>
		public Matrix? ViewMatrix
		{
			get
			{
				if (_view == null)
					return Matrix.Identity;
				else
					return _view;
			}
			set
			{
				_view = value;
				Shaders.UpdateGorgonTransformation();
			}
		}

		/// <summary>
		/// Property to set or return the number of sprites that can be cached into a vertex buffer.
		/// </summary>
		public int SpriteCacheSize
		{
			get
			{
				return _cacheSize;
			}
			set
			{
				if (_cacheSize < 1024)
					_cacheSize = 1024;
				_cacheSize = value;

				Initialize();
			}
		}

		/// <summary>
		/// Property to return the graphics interface for the 2D renderer.
		/// </summary>
		public GorgonGraphics Graphics
		{
			get;
			private set;
		}

		/// <summary>
		/// Property to set or return the active render target.
		/// </summary>
		public GorgonSwapChain Target
		{
			get
			{
				if (_target == null)
					return _defaultTarget;

				return _target;
			}
			set
			{
				if (_target != value)
				{
					_target = value;
					UpdateTarget();
				}
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to update the current render target.
		/// </summary>
		private void UpdateTarget()
		{
			// Remove any previous handler.
			if (Target != null)
				Target.Resized -= new EventHandler(target_Resized);

			Graphics.Output.RenderTargets[0] = Target;
			UpdateProjection();
			Shaders.UpdateGorgonTransformation();

			// Re-assign the event.
			if (Target != null)
				Target.Resized += new EventHandler(target_Resized);
		}

		/// <summary>
		/// Function to update the default projection matrix.
		/// </summary>
		private void UpdateProjection()
		{
			// Update based on the current render target.
			Matrix.OrthoOffCenterLH(0, Target.Settings.Width, Target.Settings.Height, 0.0f, 0.0f, 1000.0f, out _defaultProjection);
			Graphics.Rasterizer.SetViewport(Target.Viewport);

			Shaders.UpdateGorgonTransformation();
		}

		/// <summary>
		/// Function to initialize the 2D renderer.
		/// </summary>
		private void Initialize()
		{
			string shaderSource = Encoding.UTF8.GetString(Properties.Resources.BasicSprite);

			// Create the default projection matrix.
			Target.Resized -= new EventHandler(target_Resized);

			// Create shaders.
			if (Shaders == null)
				Shaders = new Gorgon2DShaders(this);

			// Create layout information so we can bind our vertices to the shader.
			if (_layout == null)
				_layout = Graphics.Input.CreateInputLayout("2D_Sprite_Vertex_Layout", typeof(Vertex), Shaders.Current.VertexShader);

			if (IndexBuffer != null)
				IndexBuffer.Dispose();
			if (VertexBuffer != null)
				VertexBuffer.Dispose();
						
			int spriteVBSize = _layout.GetSlotSize(0) * _cacheSize * 4;
			int spriteIBSize = sizeof(int) * _cacheSize * 6;

			// Set up our index buffer.
			using (GorgonDataStream ibData = new GorgonDataStream(spriteIBSize))
			{
				int index = 0;
				for (int i = 0; i < _cacheSize; i++)
				{
					ibData.Write<int>(index);
					ibData.Write<int>(index + 1);
					ibData.Write<int>(index + 2);
					ibData.Write<int>(index + 1);
					ibData.Write<int>(index + 3);
					ibData.Write<int>(index + 2);
					index += 4;
				}

				ibData.Position = 0;
				IndexBuffer = Graphics.Input.CreateIndexBuffer((int)ibData.Length, BufferUsage.Immutable, true, ibData);
			}
			
			// Create our empty vertex buffer.
			VertexBuffer = Graphics.Input.CreateVertexBuffer(spriteVBSize, BufferUsage.Dynamic);

			GorgonRasterizerStates rastState = GorgonRasterizerStates.DefaultStates;
			rastState.CullingMode = CullingMode.None;
			Graphics.Rasterizer.States = rastState;

			Graphics.Input.IndexBuffer = IndexBuffer;
			Graphics.Input.VertexBuffers[0] = new GorgonVertexBufferBinding(VertexBuffer, _layout.GetSlotSize(0));
			Graphics.Input.Layout = _layout;

			// Create the vertex cache.
			_vertexCache = new Vertex[SpriteCacheSize * 4];
			_cacheStart = 0;
			_cacheEnd = 0;
			_cacheWritten = 0;
			
			UpdateProjection();
			UpdateTarget();
			Shaders.UpdateShaders();
		}

		/// <summary>
		/// Function to handle a resize of the current render target.
		/// </summary>
		/// <param name="sender">Object that sent the event.</param>
		/// <param name="e">Event parameters.</param>
		private void target_Resized(object sender, EventArgs e)
		{
			UpdateProjection();
			UpdateTarget();
		}

		/// <summary>
		/// Function to add vertices to the vertex buffer.
		/// </summary>
		/// <param name="vertices">Vertices to add.</param>
		internal void AddVertices(IList<Vertex> vertices)
		{
			for (int i = 0; i < vertices.Count; i++)
				_vertexCache[_cacheEnd + i] = vertices[i];

			_cacheEnd += vertices.Count;
			_cacheWritten = _cacheEnd - _cacheStart;

			if (_cacheWritten + _cacheEnd > _vertexCache.Length)
			{
				Render();
				_cacheStart = 0;
				_cacheEnd = 0;
				_cacheWritten = 0;
			}
		}

		/// <summary>
		/// Function to force the renderer to render its data to the current render target.
		/// </summary>
		public void Render()
		{
			BufferLockFlags flags = BufferLockFlags.Discard | BufferLockFlags.Write;

			if (_cacheWritten == 0)
				return;

			if (_cacheStart > 0)
				flags = BufferLockFlags.NoOverwrite | BufferLockFlags.Write;

			using (GorgonDataStream stream = VertexBuffer.Lock(flags))
			{
				stream.Position = _cacheStart * 40;
				stream.WriteRange<Vertex>(_vertexCache, _cacheStart, _cacheWritten);
				VertexBuffer.Unlock();
			}

			Graphics.DrawIndexed((_cacheStart * 6) / _cacheWritten, 0, (_cacheWritten / 2) * 3);

			_cacheStart = _cacheEnd;
		}

		/// <summary>
		/// Function to create a new sprite object.
		/// </summary>
		/// <param name="name">Name of the sprite.</param>
		/// <param name="width">Width of the sprite.</param>
		/// <param name="height">Height of the sprite.</param>
		/// <returns>A new sprite.</returns>
		public GorgonSprite CreateSprite(string name, float width, float height)
		{
			return new GorgonSprite(this, name, width, height);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="Gorgon2D"/> class.
		/// </summary>
		/// <param name="target">The primary render target to use.</param>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="target"/> parameter is NULL (Nothing in VB.Net).</exception>
		public Gorgon2D(GorgonSwapChain target)
		{
			GorgonDebug.AssertNull<GorgonSwapChain>(target, "target");

			TrackedObjects = new GorgonTrackedObjectCollection();
			Graphics = target.Graphics;
			_defaultTarget = target;

			Initialize();
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
					TrackedObjects.ReleaseAll();

					if (Shaders != null)
						Shaders.CleanUp();

					if (Target != null)
						Target.Resized -= new EventHandler(target_Resized);

					if (_layout != null)
						_layout.Dispose();
					if (VertexBuffer != null)
						VertexBuffer.Dispose();
					if (IndexBuffer != null)
						IndexBuffer.Dispose();
				}

				_disposed = true;
			}
		}

		/// <summary>
		/// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
		/// </summary>
		public void Dispose()
		{
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
