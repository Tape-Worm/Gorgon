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
// Created: July 26, 2016 10:35:58 PM
// 
#endregion


using System;
using System.Linq;
using Gorgon.Core;
using Gorgon.Graphics.Core.Properties;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics.Core
{
	/// <summary>
	/// A list of <see cref="GorgonUavBinding"/> values.
	/// </summary>
	/// <remarks>
	/// <para>
	/// A <see cref="GorgonUavBinding"/> is used to bind a <see cref="GorgonUnorderedAccessView"/>to the GPU pipeline so that it may be used for rendering from the pixel shader.
	/// </para>
	/// <para>
	/// When binding an unordered access view from a pixel shader it is important to note that they occupy the same slots as render targets. Thus, if the user sets an unordered access view at slot 0, and there 
	/// is a render target already bound at that slot, then the target will be unbound and the unordered access view will replace it.  Conversely, if the user binds a render target at slot 0 and there is 
	/// already an unordered access view there, then it will be unbound. It is the responsibility of the developer to ensure there are no clashes in slot management.
	/// </para>
	/// </remarks>
	public sealed class GorgonUavBindings
		: GorgonMonitoredValueTypeArray<GorgonUavBinding>
	{
		#region Constants.
		/// <summary>
		/// The maximum number of vertex buffers allow to be bound at the same time.
		/// </summary>
		public const int MaximumUavCount = 64;
		#endregion

		#region Variables.
		// The native bindings.
		private readonly D3D11.UnorderedAccessView[] _nativeBindings;
        // The list of counts to apply.
        private readonly int[] _counts;
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the native items wrapped by this list.
		/// </summary>
		internal D3D11.UnorderedAccessView[] Native => _nativeBindings;

        /// <summary>
        /// Property to return the native initial counts for append/consume buffers.
        /// </summary>
	    internal int[] Counts => _counts;
		#endregion

		#region Methods.
	    /// <summary>
	    /// Function to validate an item being assigned to a slot.
	    /// </summary>
	    protected override void OnValidate()
	    {
	        GorgonUavBinding startView = this.FirstOrDefault(target => target.Uav != null);

	        // If no other uavs are assigned, then we're done here.
	        if (startView.Uav == null)
	        {
	            return;
	        }

	        int startViewIndex = IndexOf(startView);

	        if (startViewIndex == -1)
	        {
	            return;
	        }

	        // Only check if we have more than 1 unordered access view being applied.
	        for (int i = 0; i < Count; i++)
	        {
	            GorgonUavBinding other = this[i];

	            if (other.Uav == null)
	            {
	                continue;
	            }

	            if ((other.Equals(ref startView)) && (startViewIndex != i))
	            {
	                throw new GorgonException(GorgonResult.CannotBind, string.Format(Resources.GORGFX_ERR_UAV_ALREADY_BOUND, other.Uav.Resource.Name));
	            }
	        }
        }

        /// <summary>
        /// Function to store the native item at the given index.
        /// </summary>
        /// <param name="nativeItemIndex">The index of the item in the native array.</param>
        /// <param name="value">The value containing the native item.</param>
        protected override void OnStoreNativeItem(int nativeItemIndex, GorgonUavBinding value)
	    {
	        _nativeBindings[nativeItemIndex] = value.Uav?.NativeView;
	        _counts[nativeItemIndex] = value.InitialCount;
	    }

        /// <summary>
        /// Function called when the array is cleared.
        /// </summary>
        protected override void OnClear()
		{
		    for (int i = 0; i < Count; ++i)
		    {
                this[i] = GorgonUavBinding.Empty;
		    }

			Array.Clear(_nativeBindings, 0, _nativeBindings.Length);
		    Array.Clear(_counts, 0, _counts.Length);
		}
		#endregion

		#region Constructor
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonUavBindings"/> class.
		/// </summary>
    	internal GorgonUavBindings()
			: base(MaximumUavCount)
		{
			_nativeBindings = new D3D11.UnorderedAccessView[MaximumUavCount];
            _counts = new int[MaximumUavCount];

            // Force us to clear to the empty views instead of the default for the type.
            Clear();
        }
		#endregion
	}
}
