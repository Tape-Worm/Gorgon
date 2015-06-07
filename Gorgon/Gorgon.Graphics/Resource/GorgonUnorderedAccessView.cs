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
// Created: Saturday, May 11, 2013 1:56:53 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Diagnostics;
using Gorgon.Graphics.Properties;
using SlimMath;
using DX = SharpDX;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
    /// <summary>
    /// An unordered access resource view.
    /// </summary>
    /// <remarks>Use a resource view to allow a multiple threads inside of a shader access to the contents of a resource (or sub resource) at the same time.  
    /// <para>Unordered access views can be read/write in the shader if the format is set to one of R32_Uint, R32_Int or R32_Float.  Otherwise the view will be read-only.  An unordered access view must 
    /// have a format that is the same bit-depth and in the same group as its bound resource.</para>
    /// <para>Unlike a <see cref="Gorgon.Graphics.GorgonTextureShaderView">GorgonTextureShaderView</see> or <see cref="Gorgon.Graphics.GorgonBufferShaderView">GorgonBufferShaderView</see>, 
    /// only one unordered access view may be applied to a resource.</para>
    /// <para>Unordered access views are only available on SM_5 or better video devices.</para>
    /// </remarks>
    public abstract class GorgonUnorderedAccessView
        : GorgonView
    {
        #region Properties.
        /// <summary>
        /// Property to return the Direct3D unordered access resource view.
        /// </summary>
        internal D3D.UnorderedAccessView D3DView
        {
            get;
            set;
        }
        #endregion

        #region Methods.
        /// <summary>
        /// Function to clean up the resources used by the view.
        /// </summary>
        protected override void OnCleanUp()
        {
            if (D3DView == null)
            {
                return;
            }

            Resource.Graphics.Shaders.Unbind(this);     // Unbind this from the compute shaders.
            Resource.Graphics.Output.Unbind(this);      // Unbind pixel shaders/render targets.

            GorgonApplication.Log.Print("Destroying unordered access resource view for {0}.",
                             LoggingLevel.Verbose,
                             Resource.Name);
            D3DView.Dispose();
            D3DView = null;
        }

        /// <summary>
        /// Function to clear the unordered access value with the specified values.
        /// </summary>
        /// <param name="value1">First value.</param>
        /// <param name="value2">Second value.</param>
        /// <param name="value3">Third value.</param>
        /// <param name="value4">Fourth value.</param>
        /// <remarks>This method will copy the lower n[i] bits (where n is the number of bits in a channel, i is the index of the channel) to the proper channel.
        /// <para>This method works on any unordered access view that does not require format conversion.  Unordered access views for raw/structured buffers only use the first value.</para>
        /// </remarks>
        public void Clear(int value1, int value2, int value3, int value4)
        {
            Resource.Graphics.Context.ClearUnorderedAccessView(D3DView, new DX.Int4(value1, value2, value3, value4));
        }

        /// <summary>
        /// Function to clear the unordered access value with the specified values.
        /// </summary>
        /// <param name="values">Values used to clear.</param>
        /// <remarks>This method will copy the lower n[i] bits (where n is the number of bits in a channel, i is the index of the channel) to the proper channel.
        /// <para>This method works on any unordered access view that does not require format conversion.  Unordered access views for raw/structured buffers only use the first value in the array</para>
        /// </remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="values"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="values"/> parameter does not contain exactly 4 elements in the array.</exception>
        public void Clear(int[] values)
        {
            values.ValidateObject("values");

            if (values.Length != 4)
            {
                throw new ArgumentException(Resources.GORGFX_VIEW_UNORDERED_CLEAR_NEED_4_VALUES, "values");
            }

            Resource.Graphics.Context.ClearUnorderedAccessView(D3DView, new DX.Int4(values));
        }

        /// <summary>
        /// Function to clear the unordered access value with the specified value.
        /// </summary>
        /// <param name="value">Value used to clear.</param>
        /// <remarks>This method will copy the lower n[i] bits (where n is the number of bits in a channel, i is the index of the channel) to the proper channel.
        /// <para>This method works on any unordered access view that does not require format conversion.</para>
        /// </remarks>
        public void Clear(int value)
        {
            Resource.Graphics.Context.ClearUnorderedAccessView(D3DView, new DX.Int4(value));
        }

        /// <summary>
        /// Function to clear the unordered access value with the specified values.
        /// </summary>
        /// <param name="value1">First value.</param>
        /// <param name="value2">Second value.</param>
        /// <param name="value3">Third value.</param>
        /// <param name="value4">Fourth value.</param>
        /// <remarks>This method will copy the lower n[i] bits (where n is the number of bits in a channel, i is the index of the channel) to the proper channel.
        /// <para>This method works on any unordered access view that does not require format conversion.  Unordered access views for raw/structured buffers only use the first value.</para>
        /// </remarks>
        public void Clear(float value1, float value2, float value3, float value4)
        {
            Resource.Graphics.Context.ClearUnorderedAccessView(D3DView, new DX.Vector4(value1, value2, value3, value4));
        }

        /// <summary>
        /// Function to clear the unordered access value with the specified values.
        /// </summary>
        /// <param name="values">Values used to clear.</param>
        /// <remarks>This method works on any unordered access view that does not require format conversion.  Unordered access views for raw/structured buffers only use the first value in the array.</remarks>
        /// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="values"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
        /// <exception cref="System.ArgumentException">Thrown when the <paramref name="values"/> parameter does not contain exactly 4 elements in the array.</exception>
        public void Clear(float[] values)
        {
            values.ValidateObject("values");

            if (values.Length != 4)
            {
                throw new ArgumentException(Resources.GORGFX_VIEW_UNORDERED_CLEAR_NEED_4_VALUES, "values");
            }

            Resource.Graphics.Context.ClearUnorderedAccessView(D3DView, new DX.Vector4(values));
        }

        /// <summary>
        /// Function to clear the unordered access value with the specified value.
        /// </summary>
        /// <param name="value">Value used to clear.</param>
        /// <remarks>This method will copy the lower n[i] bits (where n is the number of bits in a channel, i is the index of the channel) to the proper channel.
        /// <para>This method works on any unordered access view that does not require format conversion.</para>
        /// </remarks>
        public void Clear(float value)
        {
            Resource.Graphics.Context.ClearUnorderedAccessView(D3DView, new DX.Vector4(value));
        }

        /// <summary>
        /// Function to clear the unordered access value with the specified values.
        /// </summary>
        /// <param name="values">Values used to clear.</param>
        /// <remarks>This method will copy the lower n[i] bits (where n is the number of bits in a channel, i is the index of the channel) to the proper channel.
        /// <para>This method works on any unordered access view that does not require format conversion.  Unordered access views for raw/structured buffers only use the first value in the vector.</para>
        /// </remarks>
        public void Clear(Vector4 values)
        {
            Resource.Graphics.Context.ClearUnorderedAccessView(D3DView, new DX.Vector4(values.X, values.Y, values.Z, values.W));
        }
        #endregion

		#region Constructor/Destructor.
		/// <summary>
        /// Initializes a new instance of the <see cref="GorgonUnorderedAccessView"/> class.
        /// </summary>
        /// <param name="resource">The buffer to bind to the view.</param>
        /// <param name="format">The format of the view.</param>
		protected GorgonUnorderedAccessView(GorgonResource resource, BufferFormat format)
			: base(resource, format)
		{
		}
        #endregion
    }
}

