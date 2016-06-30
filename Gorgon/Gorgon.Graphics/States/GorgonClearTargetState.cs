using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Math;

namespace Gorgon.Graphics
{
	/// <summary>
	/// A state used to define what color to use, depth value to use and stencil value to use when clearing a target.
	/// </summary>
	struct GorgonClearTargetState
	{
		#region Variables.
		/// <summary>
		/// The color to use when clearing the color buffer.
		/// </summary>
		public readonly GorgonColor Color;

		/// <summary>
		/// The depth buffer value to use when clearing the depth buffer.
		/// </summary>
		public readonly float DepthValue;

		/// <summary>
		/// The stencil buffer value to use when clearing the stencil buffer.
		/// </summary>
		public readonly int StencilValue;

		/// <summary>
		/// The render target view to clear.
		/// </summary>
		public readonly GorgonRenderTargetView TargetView;
		#endregion

		#region Methods.
		/// <summary>
		/// Function to test two instances for equality.
		/// </summary>
		/// <param name="left">Left instance to compare.</param>
		/// <param name="right">Right instance to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool Equals(ref GorgonClearTargetState left, ref GorgonClearTargetState right)
		{
			return left.Color.Equals(right.Color) && left.DepthValue.EqualsEpsilon(right.DepthValue) && left.StencilValue == right.StencilValue && left.TargetView == right.TargetView;
		}
		#endregion

		#region Constructor/Finalizer.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonClearTargetState"/> struct.
		/// </summary>
		/// <param name="targetView">The render target view to clear.</param>
		/// <param name="color">The color.</param>
		/// <param name="depthValue">The depth value.</param>
		/// <param name="stencilValue">The stencil value.</param>
		public GorgonClearTargetState(GorgonRenderTargetView targetView, GorgonColor color, float depthValue, int stencilValue)
		{
			TargetView = targetView;
			Color = color;
			DepthValue = depthValue;
			StencilValue = stencilValue;
		}
		#endregion
	}
}
