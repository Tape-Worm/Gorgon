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
// Created: Monday, November 28, 2011 8:28:24 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using D3D = SharpDX.Direct3D11;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Operations applied to stencil buffers.
	/// </summary>
	public enum StencilOperations
	{
		/// <summary>
		/// Keep existing stencil data.
		/// </summary>
		Keep = 1,
		/// <summary>
		/// Set the stencil data to 0.
		/// </summary>
		Zero = 2,
		/// <summary>
		/// Set the stencil data to a reference value defined in the depth/stencil state object.
		/// </summary>
		Replace = 3,
		/// <summary>
		/// Increment the stencil value by 1 and clamp the result.
		/// </summary>
		IncrementClamp = 4,
		/// <summary>
		/// Decrement the stencil value by 1 and clamp the result.
		/// </summary>
		DecrementClamp = 5,
		/// <summary>
		/// Invert the stencil value.
		/// </summary>
		Invert = 6,
		/// <summary>
		/// Increment the stencil value by 1 and wrap if necessary.
		/// </summary>
		Increment = 7,
		/// <summary>
		/// Decrement the stencil value by 1 and wrap if necessary.
		/// </summary>
		Decrement = 8
	}

	/// <summary>
	/// Depth/stencil buffer state.
	/// </summary>
	/// <remarks>Used to control how depth/stencil testing is applied to a scene.
	/// <para>State objects are immutable.  Therefore, when an application requires a different state, the user must create a new state object and give it the necessary parameters.  
	/// This is different from previous methods of applying state, where one would modify a render state variable and it would apply immediately.  This model incurred performance penalties 
	/// from too many state changes.  This method does not suffer from that as states can be reused.
	/// </para>
	/// </remarks>
	public class GorgonDepthStencilState
		: GorgonStateObject<D3D.DepthStencilState>
	{
		#region Variables.
		private bool _disposed = false;							// Flag to determine if the object was disposed or not. 
		private D3D.DepthStencilStateDescription _desc;			// Description for the state.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return whether the depth buffer is enabled or not.
		/// </summary>
		/// <remarks>The default value is TRUE.</remarks>
		public bool IsDepthEnabled
		{
			get
			{
				return _desc.IsDepthEnabled;
			}
			set
			{
				if (_desc.IsDepthEnabled != value)
				{
					_desc.IsDepthEnabled = value;
					HasChanged = value;
				}
			}
		}

		/// <summary>
		/// Property to set or return whether the stencil buffer is enabled or not.
		/// </summary>
		/// <remarks>The default value is FALSE.</remarks>
		public bool IsStencilEnabled
		{
			get
			{
				return _desc.IsStencilEnabled;
			}
			set
			{
				if (_desc.IsStencilEnabled != value)
				{
					_desc.IsStencilEnabled = value;
					HasChanged = value;
				}
			}
		}

		/// <summary>
		/// Property to set or return whether depth writing is enabled.
		/// </summary>
		/// <remarks>The default value is TRUE.</remarks>
		public bool IsDepthWriteEnabled
		{
			get
			{
				return (_desc.DepthWriteMask == D3D.DepthWriteMask.All);
			}
			set
			{
				if (((_desc.DepthWriteMask == D3D.DepthWriteMask.All) && (!value)) ||
					((_desc.DepthWriteMask == D3D.DepthWriteMask.Zero) && (value)))
				{
					if (value)
						_desc.DepthWriteMask = D3D.DepthWriteMask.All;
					else
						_desc.DepthWriteMask = D3D.DepthWriteMask.Zero;

					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the comparison operator for the depth buffer test.
		/// </summary>
		/// <remarks>The default value is Less</remarks>
		public ComparisonOperators DepthComparison
		{
			get
			{
				return (ComparisonOperators)_desc.DepthComparison;
			}
			set
			{
				if (_desc.DepthComparison != (D3D.Comparison)value)
				{
					_desc.DepthComparison = (D3D.Comparison)value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the mask used to read from the stencil buffer.
		/// </summary>
		/// <remarks>The default value is 0xFF.</remarks>
		public byte StencilReadMask
		{
			get
			{
				return _desc.StencilReadMask;
			}
			set
			{
				if (_desc.StencilReadMask != value)
				{
					_desc.StencilReadMask = value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the mask used to write to the stencil buffer.
		/// </summary>
		/// <remarks>The default value is 0xFF.</remarks>
		public byte StencilWriteMask
		{
			get
			{
				return _desc.StencilWriteMask;
			}
			set
			{
				if (_desc.StencilWriteMask != value)
				{
					_desc.StencilWriteMask = value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the operation to perform when the front face test fails.
		/// </summary>
		/// <remarks>The default value is Keep.</remarks>
		public StencilOperations StencilFrontFace_FailOperation
		{
			get
			{
				return (StencilOperations)_desc.FrontFace.FailOperation;
			}
			set
			{
				if (_desc.FrontFace.FailOperation != (D3D.StencilOperation)value)
				{
					_desc.FrontFace.FailOperation = (D3D.StencilOperation)value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the operation to perform when the back face test fails.
		/// </summary>
		/// <remarks>The default value is Keep.</remarks>
		public StencilOperations StencilBackFace_FailOperation
		{
			get
			{
				return (StencilOperations)_desc.BackFace.FailOperation;
			}
			set
			{
				if (_desc.BackFace.FailOperation != (D3D.StencilOperation)value)
				{
					_desc.BackFace.FailOperation = (D3D.StencilOperation)value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the operation to perform when the front face depth test fails.
		/// </summary>
		/// <remarks>The default value is Keep.</remarks>
		public StencilOperations StencilFrontFace_DepthFailOperation
		{
			get
			{
				return (StencilOperations)_desc.FrontFace.DepthFailOperation;
			}
			set
			{
				if (_desc.FrontFace.DepthFailOperation != (D3D.StencilOperation)value)
				{
					_desc.FrontFace.DepthFailOperation = (D3D.StencilOperation)value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the operation to perform when the back face depth test fails.
		/// </summary>
		/// <remarks>The default value is Keep.</remarks>
		public StencilOperations StencilBackFace_DepthFailOperation
		{
			get
			{
				return (StencilOperations)_desc.BackFace.DepthFailOperation;
			}
			set
			{
				if (_desc.BackFace.DepthFailOperation != (D3D.StencilOperation)value)
				{
					_desc.BackFace.DepthFailOperation = (D3D.StencilOperation)value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the operation to perform when the front face test succeeds.
		/// </summary>
		/// <remarks>The default value is Keep.</remarks>
		public StencilOperations StencilFrontFace_PassOperation
		{
			get
			{
				return (StencilOperations)_desc.FrontFace.PassOperation;
			}
			set
			{
				if (_desc.FrontFace.PassOperation != (D3D.StencilOperation)value)
				{
					_desc.FrontFace.PassOperation = (D3D.StencilOperation)value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the operation to perform when the back depth test succeeds.
		/// </summary>
		/// <remarks>The default value is Keep.</remarks>
		public StencilOperations StencilBackFace_PassOperation
		{
			get
			{
				return (StencilOperations)_desc.BackFace.PassOperation;
			}
			set
			{
				if (_desc.BackFace.PassOperation != (D3D.StencilOperation)value)
				{
					_desc.BackFace.PassOperation = (D3D.StencilOperation)value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the comparison operatpr for the front face stencil testing.
		/// </summary>
		/// <remarks>The default value is Always.</remarks>
		public ComparisonOperators StencilFrontFace_ComparisonOperator
		{
			get
			{
				return (ComparisonOperators)_desc.FrontFace.Comparison;
			}
			set
			{
				if (_desc.FrontFace.Comparison != (D3D.Comparison)value)
				{
					_desc.FrontFace.Comparison = (D3D.Comparison)value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the comparison operatpr for the back face stencil testing.
		/// </summary>
		/// <remarks>The default value is Always.</remarks>
		public ComparisonOperators StencilBackFace_ComparisonOperator
		{
			get
			{
				return (ComparisonOperators)_desc.BackFace.Comparison;
			}
			set
			{
				if (_desc.BackFace.Comparison != (D3D.Comparison)value)
				{
					_desc.BackFace.Comparison = (D3D.Comparison)value;
					HasChanged = true;
				}
			}
		}

		/// <summary>
		/// Property to set or return the depth stencil reference value.
		/// </summary>
		/// <remarks>This is the value used when the stencil state is set to Replace.
		/// <para>The default value is 0.</para>
		/// </remarks>
		public int DepthStencilReference
		{
			get
			{
				return Graphics.Context.OutputMerger.DepthStencilReference;
			}
			set
			{
				Graphics.Context.OutputMerger.DepthStencilReference = value;				
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to apply any changes immediately if this state is the current state.
		/// </summary>
		protected override void ApplyImmediate()
		{
			if (Graphics.DepthStencilState != this)
				Graphics.DepthStencilState = this;

			Graphics.Context.OutputMerger.DepthStencilState = Convert();
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
				{
					if ((Graphics != null) && (Graphics.DepthStencilState == this))
						Graphics.DepthStencilState = null;
				}

				_disposed = true;
			}

			base.Dispose(disposing);
		}

		/// <summary>
		/// Function to convert this state object into a Direct 3D state object.
		/// </summary>
		/// <returns>The Direct 3D state object.</returns>
		protected internal override D3D.DepthStencilState Convert()
		{
			if (HasChanged)
			{
				if (State != null)
				{
					State.Dispose();
					State = null;
				}
				
				State = new D3D.DepthStencilState(Graphics.VideoDevice.D3DDevice, _desc);

				HasChanged = false;
			}

			return State;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDepthStencilState"/> class.
		/// </summary>
		/// <param name="graphics">The graphics interface that owns this object.</param>
		internal GorgonDepthStencilState(GorgonGraphics graphics)
			: base(graphics)
		{
			_desc = new D3D.DepthStencilStateDescription();
			_desc.FrontFace = new D3D.DepthStencilOperationDescription();
			_desc.BackFace = new D3D.DepthStencilOperationDescription();

			_desc.IsDepthEnabled = true;
			_desc.DepthWriteMask = D3D.DepthWriteMask.All;
			_desc.DepthComparison = D3D.Comparison.Less;
			_desc.StencilReadMask = 0xff;
			_desc.StencilWriteMask = 0xff;

			_desc.FrontFace.FailOperation = D3D.StencilOperation.Keep;
			_desc.FrontFace.DepthFailOperation = D3D.StencilOperation.Keep;
			_desc.FrontFace.PassOperation = D3D.StencilOperation.Keep;
			_desc.DepthComparison = D3D.Comparison.Always;

			_desc.BackFace = _desc.FrontFace;
		}
		#endregion
	}
}
