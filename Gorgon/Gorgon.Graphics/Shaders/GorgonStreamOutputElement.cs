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
// Created: Thursday, June 27, 2013 9:36:21 PM
// 
#endregion

using System;
using Gorgon.Core;
using Gorgon.Graphics.Properties;
using D3D = SharpDX.Direct3D11;

namespace Gorgon.Graphics
{
	/// <summary>
	/// An element to be used in the stream output layout.
	/// </summary>
	/// <remarks>This defines the layout of an item of data in the output buffer.</remarks>
	public struct GorgonStreamOutputElement
		: IGorgonNamedObject, IEquatable<GorgonStreamOutputElement>
	{
		#region Variables.
		/// <summary>
		/// The zero-based stream index number.
		/// </summary>
		public readonly int Stream;
		/// <summary>
		/// The context of the element.
		/// </summary>
		/// <remarks>This is a string value that corresponds to a shader input.  For example, to specify a position, the user would set this to "position".  
		/// These contexts can be named whatever the user wishes.  This must map to a corresponding element in the shader.
		/// </remarks>
		public readonly string Context;
		/// <summary>
		/// The index of the context.
		/// </summary>
		/// <remarks>This is used to denote the same context but at another index.  For example, to specify a second set of texture coordinates, set this 
		/// to 1.</remarks>
		public readonly int Index;
		/// <summary>
		/// The component to begin writing out to.
		/// </summary>
		/// <remarks>This allows to start writing out mid component.  For example, to start writing at the Y component pass 1 to this value.
		/// <para>Only 0 - 3 is valid for this value.</para>
		/// </remarks>
		public readonly int StartComponent;
		/// <summary>
		/// The number of components to write out to.
		/// </summary>
		/// <remarks>This allows limiting of how much to write to a component.  For example, setting this value to 1 will only write to the x value of the component if <see cref="StartComponent"/> is 0.
		/// <para>Only 1 - 4 is valid for this value.</para>
		/// </remarks>
		public readonly int ComponentCount;
		/// <summary>
		/// The associated stream output buffer that is bound to the pipeline.
		/// </summary>
		/// <remarks>The valid range is 0 to 3.</remarks>
		public readonly int Slot;
		#endregion

		#region Method.
		/// <summary>
		/// Function to convert this Gorgon input element into a Direct3D input element.
		/// </summary>
		/// <returns>The direct 3D input element.</returns>
		internal D3D.StreamOutputElement Convert()
		{
			return new D3D.StreamOutputElement
				{
					Stream = Stream,
					SemanticName = Context,
					SemanticIndex = Index,
					StartComponent = (byte)StartComponent,
					ComponentCount = (byte)ComponentCount,
					OutputSlot = (byte)Slot
				};
		}

		/// <summary>
		/// Determines whether the specified <see cref="System.Object"/> is equal to this instance.
		/// </summary>
		/// <param name="obj">The <see cref="System.Object"/> to compare with this instance.</param>
		/// <returns>
		///   <b>true</b> if the specified <see cref="System.Object"/> is equal to this instance; otherwise, <b>false</b>.
		/// </returns>
		public override bool Equals(object obj)
		{
			if (obj is GorgonStreamOutputElement)
			{
				return Equals((GorgonStreamOutputElement)obj);
			}

			return base.Equals(obj);
		}

		/// <summary>
		/// Function to determine if two instances are equal.
		/// </summary>
		/// <param name="left">First instance to compare.</param>
		/// <param name="right">Second instance to compare.</param>
		/// <returns><b>true</b> if equal, <b>false</b> if not.</returns>
		public static bool Equals(ref GorgonStreamOutputElement left, ref GorgonStreamOutputElement right)
		{
			return ((string.Equals(left.Context, right.Context)) && (left.ComponentCount == right.ComponentCount)
			        && (left.Index == right.Index) && (left.Slot == right.Slot) 
			        && (left.StartComponent == right.StartComponent) && (left.Stream == right.Stream));
		}

		/// <summary>
		/// Implements the operator ==.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator ==(GorgonStreamOutputElement left, GorgonStreamOutputElement right)
		{
			return Equals(ref left, ref right);
		}

		/// <summary>
		/// Implements the operator !=.
		/// </summary>
		/// <param name="left">The left.</param>
		/// <param name="right">The right.</param>
		/// <returns>
		/// The result of the operator.
		/// </returns>
		public static bool operator !=(GorgonStreamOutputElement left, GorgonStreamOutputElement right)
		{
			return !Equals(ref left, ref right);
		}

		/// <summary>
		/// Returns a hash code for this instance.
		/// </summary>
		/// <returns>
		/// A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table. 
		/// </returns>
		public override int GetHashCode()
		{
			return
				281.GenerateHash(Stream).GenerateHash(Context.GetHashCode())
				   .GenerateHash(Index)
				   .GenerateHash(StartComponent)
				   .GenerateHash(ComponentCount)
				   .GenerateHash(Slot);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonStreamOutputElement"/> class.
		/// </summary>
		/// <param name="stream">The zero based stream index number.</param>
		/// <param name="context">The context for the element.</param>
		/// <param name="index">The index of the element.</param>
		/// <param name="startComponent">The first component to start writing at.</param>
		/// <param name="componentCount">The number of components to write.</param>
		/// <param name="slot">The vertex buffer slot for the element.</param>
		/// <remarks>The slot value must be between 0 and 3 (inclusive), the <paramref name="startComponent"/> must be between 0 and 3 (inclusive), and the <paramref name="componentCount"/> must be between 1 and 4 (inclusive).  
		/// A value outside of these ranges will cause an exception to be thrown.</remarks>
		/// <exception cref="System.ArgumentNullException">Thrown when the <paramref name="context"/> parameter is NULL (<i>Nothing</i> in VB.Net).</exception>
		/// <exception cref="System.ArgumentException">Thrown when the <paramref name="context"/> parameter is empty.</exception>
		/// <exception cref="System.ArgumentOutOfRangeException">Thrown when the <paramref name="slot"/> parameter is less than 0 or greater than 3.
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="startComponent"/> is less than 0 or greater than 3.</para>
		/// <para>-or-</para>
		/// <para>Thrown when the <paramref name="componentCount"/> is less than 1 or greater than 4.</para>
		/// </exception>
		public GorgonStreamOutputElement(int stream, string context, int index, int startComponent, int componentCount, int slot)
		{
			if (context == null)
			{
				throw new ArgumentNullException(nameof(context));
			}

			if (string.IsNullOrWhiteSpace(context))
			{
				throw new ArgumentException(Resources.GORGFX_PARAMETER_MUST_NOT_BE_EMPTY, nameof(context));
			}

            if ((slot < 0) || (slot > 3))
            {
				throw new ArgumentOutOfRangeException(nameof(slot), string.Format(Resources.GORGFX_VALUE_OUT_OF_RANGE, slot, 4));
            }

			if ((startComponent < 0) || (startComponent > 3))
			{
				throw new ArgumentOutOfRangeException(nameof(startComponent), string.Format(Resources.GORGFX_VALUE_OUT_OF_RANGE, slot, 4));
			}

			if ((componentCount < 1) || (componentCount > 4))
			{
				throw new ArgumentOutOfRangeException(nameof(componentCount));
			}

			Stream = stream;
            Context = context;
			Index = index;
			StartComponent = startComponent;
			ComponentCount = componentCount;
			Slot = slot;
		}
		#endregion

		#region IGorgonNamedObject Members
		/// <summary>
		/// Property to return the name of this object.
		/// </summary>
		string IGorgonNamedObject.Name => Context;

		#endregion

		#region IEquatable<GorgonStreamOutputElement> Members
		/// <summary>
		/// Indicates whether the current object is equal to another object of the same type.
		/// </summary>
		/// <param name="other">An object to compare with this object.</param>
		/// <returns>
		/// true if the current object is equal to the other parameter; otherwise, false.
		/// </returns>
		public bool Equals(GorgonStreamOutputElement other)
		{
			return Equals(ref this, ref other);
		}
		#endregion
	}
}
