#region MIT
// 
// Gorgon.
// Copyright (C) 2016 Michael Winsor
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
// Created: July 4, 2016 1:05:13 AM
// 
#endregion

using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A list of texture sampler states to apply to the pipeline.
	/// </summary>
	public sealed class GorgonSamplerStates
		: GorgonMonitoredArray<GorgonSamplerState>
	{
		#region Constants.
		/// <summary>
		/// The maximum number of allowed sampler states that can be bound at the same time.
		/// </summary>
		public const int MaximumSamplerStateCount = D3D11.CommonShaderStage.SamplerSlotCount;
        #endregion

        #region Methods.
	    /// <summary>
	    /// Function called when an item is assigned to an index.
	    /// </summary>
	    /// <param name="index">The index of the item that was assigned.</param>
	    /// <param name="value">The value that was assigned.</param>
	    /// <param name="oldItem">The previous item in the slot.</param>
	    protected override void OnItemSet(int index, GorgonSamplerState value, GorgonSamplerState oldItem)
	    {
	        if (value != null)
	        {
	            value.IsLocked = true;
	        }
	    }
	    #endregion

        #region Constructor/Finalizer.
        /// <summary>
        /// Initializes a new instance of the <see cref="GorgonSamplerStates"/> class.
        /// </summary>
        internal GorgonSamplerStates()
			: base(MaximumSamplerStateCount)
		{
		}
		#endregion
	}
}
