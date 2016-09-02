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
// Created: July 28, 2016 11:49:51 PM
// 
#endregion

using System;
using D3D11 = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// Information used to describe a depth/stencil state.
	/// </summary>
	public class GorgonDepthStencilStateInfo 
		: IGorgonDepthStencilStateInfo
	{
		#region Variables.
		/// <summary>
		/// The default depth/stencil state.
		/// </summary>
		public static readonly IGorgonDepthStencilStateInfo Default = new GorgonDepthStencilStateInfo();

		/// <summary>
		/// Depth/stencil enabled.
		/// </summary>
		public static readonly IGorgonDepthStencilStateInfo DepthStencilEnabled = new GorgonDepthStencilStateInfo
		                                                                          {
			                                                                          IsDepthEnabled = true,
																					  IsStencilEnabled = true,
																					  IsDepthWriteEnabled = true
		                                                                          };

		/// <summary>
		/// Depth/stencil enabled, depth write disbled.
		/// </summary>
		public static readonly IGorgonDepthStencilStateInfo DepthStencilEnabledNoWrite = new GorgonDepthStencilStateInfo
		                                                                                 {
			                                                                                 IsDepthEnabled = true,
			                                                                                 IsStencilEnabled = true,
			                                                                                 IsDepthWriteEnabled = false
		                                                                                 };

		/// <summary>
		/// Depth only enabled.
		/// </summary>
		public static readonly IGorgonDepthStencilStateInfo DepthEnabled = new GorgonDepthStencilStateInfo
		{
			IsDepthEnabled = true,
			IsDepthWriteEnabled = true
		};

		/// <summary>
		/// Depth only enabled, depth write disabled.
		/// </summary>
		public static readonly IGorgonDepthStencilStateInfo DepthEnabledNoWrite = new GorgonDepthStencilStateInfo
		{
			IsDepthEnabled = true,
			IsDepthWriteEnabled = false
		};

		/// <summary>
		/// Stencil only enabled.
		/// </summary>
		public static readonly IGorgonDepthStencilStateInfo StencilEnabled = new GorgonDepthStencilStateInfo
		                                                                     {
			                                                                     IsStencilEnabled = true,
			                                                                     IsDepthWriteEnabled = true
		                                                                     };
		#endregion

		#region Properties.
		/// <summary>
		/// Property to set or return the depth comparison function.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Use this property to determine whether a depth value will be written into the buffer if the function specified evaluates to true using the data being written and existing data.
		/// </para>
		/// <para>
		/// The default value is <c>Less</c>.
		/// </para>
		/// </remarks>
		public D3D11.Comparison DepthComparison
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether to enable writing to the depth buffer or not.
		/// </summary>
		/// <remarks>
		/// The default value is <c>true</c>.
		/// </remarks>
		public bool IsDepthWriteEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the depth buffer is enabled or not.
		/// </summary>
		/// <remarks>
		/// The default value is <b>false</b>.
		/// </remarks>
		public bool IsDepthEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return whether the stencil buffer is enabled or not.
		/// </summary>
		/// <remarks>
		/// The default value is <b>false</b>.
		/// </remarks>
		public bool IsStencilEnabled
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the read mask for stencil operations.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Use this to filter out specific values from the stencil buffer during a read operation.
		/// </para>
		/// <para>
		/// The default value is <c>0xff</c>.
		/// </para>
		/// </remarks>
		public byte StencilReadMask
		{
			get;
			set;
		}

		/// <summary>
		/// Property to set or return the write mask for stencil operations.
		/// </summary>
		/// <remarks>
		/// <para>
		/// Use this to filter out specific values from the stencil buffer during a write operation.
		/// </para>
		/// <para>
		/// The default value is <c>0xff</c>.
		/// </para>
		/// </remarks>
		public byte StencilWriteMask
		{
			get;
			set;
		}

		/// <summary>
		/// Property to return the setup information for stencil operations on front facing polygons.
		/// </summary>
		IGorgonStencilOperationInfo IGorgonDepthStencilStateInfo.FrontFaceStencilOp => FrontFaceStencilOp;

		/// <summary>
		/// Property to return the setup information for stencil operations on front facing polygons.
		/// </summary>
		public GorgonStencilOperationInfo FrontFaceStencilOp
		{
			get;
		}

		/// <summary>
		/// Property to return the setup information for stencil operations on back facing polygons.
		/// </summary>
		IGorgonStencilOperationInfo IGorgonDepthStencilStateInfo.BackFaceStencilOp => BackFaceStencilOp;

		/// <summary>
		/// Property to return the setup information for stencil operations on back facing polygons.
		/// </summary>
		public GorgonStencilOperationInfo BackFaceStencilOp
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to compare equality for this and another <see cref="IGorgonDepthStencilStateInfo"/>.
		/// </summary>
		/// <param name="info">The <see cref="IGorgonDepthStencilStateInfo"/> to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public bool IsEqual(IGorgonDepthStencilStateInfo info)
		{
			return info != null
			       && BackFaceStencilOp.Comparison == info.BackFaceStencilOp.Comparison
			       && BackFaceStencilOp.DepthFailOperation == info.BackFaceStencilOp.DepthFailOperation
			       && BackFaceStencilOp.FailOperation == info.BackFaceStencilOp.FailOperation
			       && BackFaceStencilOp.PassOperation == info.BackFaceStencilOp.PassOperation
			       && FrontFaceStencilOp.Comparison == info.FrontFaceStencilOp.Comparison
			       && FrontFaceStencilOp.DepthFailOperation == info.FrontFaceStencilOp.DepthFailOperation
			       && FrontFaceStencilOp.FailOperation == info.FrontFaceStencilOp.FailOperation
			       && FrontFaceStencilOp.PassOperation == info.FrontFaceStencilOp.PassOperation
			       && DepthComparison == info.DepthComparison
			       && IsDepthEnabled == info.IsDepthEnabled
			       && IsDepthWriteEnabled == info.IsDepthWriteEnabled
			       && IsStencilEnabled == info.IsStencilEnabled
			       && StencilReadMask == info.StencilReadMask
			       && StencilWriteMask == info.StencilWriteMask;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDepthStencilStateInfo"/> class.
		/// </summary>
		/// <param name="info">A <see cref="IGorgonDepthStencilStateInfo"/> to copy the settings from.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="info"/> parameter is <b>null</b>.</exception>
		public GorgonDepthStencilStateInfo(IGorgonDepthStencilStateInfo info)
		{
			if (info == null)
			{
				throw new ArgumentNullException(nameof(info));
			}

			BackFaceStencilOp = new GorgonStencilOperationInfo(info.BackFaceStencilOp);
			FrontFaceStencilOp = new GorgonStencilOperationInfo(info.FrontFaceStencilOp);
			DepthComparison = info.DepthComparison;
			IsDepthWriteEnabled = info.IsDepthWriteEnabled;
			IsDepthEnabled = info.IsDepthEnabled;
			IsStencilEnabled = info.IsStencilEnabled;
			StencilReadMask = info.StencilReadMask;
			StencilWriteMask = info.StencilWriteMask;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonDepthStencilStateInfo"/> class.
		/// </summary>
		public GorgonDepthStencilStateInfo()
		{
			IsDepthWriteEnabled = true;
			DepthComparison = D3D11.Comparison.Less;
			StencilReadMask = 0xff;
			StencilWriteMask = 0xff;
			BackFaceStencilOp = new GorgonStencilOperationInfo();
			FrontFaceStencilOp = new GorgonStencilOperationInfo();
		}
		#endregion
	}
}
