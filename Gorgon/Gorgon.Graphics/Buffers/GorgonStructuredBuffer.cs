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
// Created: Monday, November 5, 2012 8:58:47 PM
// 
#endregion

namespace Gorgon.Graphics
{
	/// <summary>
	/// A structured buffer for shaders.
	/// </summary>
	/// <remarks>Structured buffers are similar to <see cref="Gorgon.Graphics.GorgonConstantBuffer">constant buffers</see> in that they're used to convey data to 
	/// a shader.  However, unlike constant buffers, these buffers allow for unordered access and are meant as containers for structured data (hence, structured buffers).
	/// <para>Structured buffers are only available to SM5 and above.</para>
	/// </remarks>
	public class GorgonStructuredBuffer
		: GorgonBaseBuffer
	{
		#region Variables.
		private GorgonShaderView _defaultView;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the type of buffer.
		/// </summary>
		public override BufferType BufferType
		{
			get
			{
				return BufferType.Structured;
			}
		}

		/// <summary>
		/// Property to return the settings for a structured shader buffer.
		/// </summary>
		public new GorgonStructuredBufferSettings Settings
		{
			get
			{
				return (GorgonStructuredBufferSettings)base.Settings;
			}
		}

		/// <summary>
		/// Property to return the default shader view for this buffer.
		/// </summary>
		public override GorgonShaderView DefaultShaderView
		{
			get
			{
				return _defaultView;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to create the default shader view.
		/// </summary>
		protected override void OnCreateDefaultShaderView()
		{
			if ((Settings.Usage == BufferUsage.Staging) || (!Settings.AllowShaderViews) || (!Settings.CreateDefaultShaderView))
			{
				return;
			}

			_defaultView = GetShaderView(0, Settings.SizeInBytes / Settings.StructureSize);
		}

        /// <summary>
        /// Function to retrieve a new shader view for the buffer.
        /// </summary>
        /// <param name="start">Starting element.</param>
        /// <param name="count">Element count.</param>
        /// <returns>A shader view for the buffer.</returns>
        /// <exception cref="Gorgon.GorgonException">Thrown when the usage for this buffer is set to Staging.
        /// <para>-or-</para>
        /// <para>Thrown when the view could not be created.</para>
        /// </exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="start"/> or <paramref name="count"/> parameters are less than 0 or greater than or equal to the 
        /// number of elements in the buffer.</exception>
		/// <exception cref="Gorgon.GorgonException">Thrown when the view could not be created or retrieved from the cache.</exception>
		/// <remarks>Use this to create/retrieve additional shader views for the buffer.  Multiple views of the same resource can be bound to multiple stages in the pipeline.
        /// <para>This function only applies to buffers that have not been created with a Usage of Staging.</para>
        /// </remarks>
        public GorgonBufferShaderView GetShaderView(int start, int count)
        {
	        return OnGetShaderView(BufferFormat.Unknown, start, count, false);
        }

		/// <summary>
		/// Function to retrieve an unordered access view for this buffer.
		/// </summary>
		/// <param name="start">First element to map to the view.</param>
		/// <param name="count">The number of elements to map to the view.</param>
		/// <param name="viewType">The type of unordered view to apply to the structured buffer.</param>
		/// <returns>A new unordered access view for the buffer.</returns>
		/// <remarks>Use this to create/retrieve an unordered access view that will allow shaders to access the view using multiple threads at the same time.  Unlike a <see cref="GetShaderView">Shader View</see>, only one 
		/// unordered access view can be bound to the pipeline at any given time.
		/// <para>Unordered access views require a video device feature level of SM_5 or better.</para>
		/// </remarks>
		/// <exception cref="Gorgon.GorgonException">Thrown when the view could not be created or retrieved from the cache.</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="start"/> or <paramref name="count"/> parameters are less than 0 or greater than or equal to the 
		/// number of elements in the buffer.</exception>
		public GorgonStructuredBufferUnorderedAccessView GetUnorderedAccessView(int start, int count, UnorderedAccessViewType viewType)
		{
		    return (GorgonStructuredBufferUnorderedAccessView)OnGetUnorderedAccessView(BufferFormat.Unknown, start, count, false, viewType);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonStructuredBuffer" /> class.
		/// </summary>
		/// <param name="graphics">Graphics interface that owns this buffer.</param>
		/// <param name="name">Name of the buffer.</param>
		/// <param name="settings">The settings for the structured buffer.</param>
		internal GorgonStructuredBuffer(GorgonGraphics graphics, string name, GorgonStructuredBufferSettings settings)
			: base(graphics, name, settings)
		{
		}
		#endregion
	}
}
