#region LGPL.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
// 
// This library is free software; you can redistribute it and/or
// modify it under the terms of the GNU Lesser General Public
// License as published by the Free Software Foundation; either
// version 2.1 of the License, or (at your option) any later version.
// 
// This library is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
// Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public
// License along with this library; if not, write to the Free Software
// Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
// 
// Created: Sunday, July 23, 2006 1:35:48 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.InteropServices;
using Drawing = System.Drawing;
using SharpUtilities.Mathematics;
using SharpUtilities.Collections;

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Object representing a list of vertex types.
	/// </summary>
	public class VertexTypes
		: BaseCollection<VertexType>, IDisposable
	{
		#region Value Types.
		/// <summary>
		/// Value type describing a sprite vertex.
		/// </summary>
		[StructLayout(LayoutKind.Sequential, Pack = 1)]
		public struct PositionDiffuse2DTexture1
		{
			#region Variables.
			/// <summary>
			/// Position of the vertex.
			/// </summary>
			public Vector3D Position;

			/// <summary>
			/// Color of the vertex.
			/// </summary>
			public int Color;

			/// <summary>
			/// Texture coordinates.
			/// </summary>
			public Vector2D TextureCoordinates;
			#endregion

			#region Constructor.
			/// <summary>
			/// Constructor.
			/// </summary>
			/// <param name="position">Position of the vertex.</param>
			/// <param name="color">Color of the vertex.</param>
			/// <param name="textureCoordinates">Texture coordinates.</param>
			public PositionDiffuse2DTexture1(Vector3D position, Drawing.Color color, Vector2D textureCoordinates)
			{
				// Copy data.
				Position = position;
				Color = color.ToArgb();
				TextureCoordinates = textureCoordinates;
			}
			#endregion
		}
		#endregion

		#region Variables.
		#endregion

		#region Properties.
		#endregion

		#region Methods.
		/// <summary>
		/// Function to clear the list.
		/// </summary>
		protected void Clear()
		{
			// Destroy all the vertex types.
			foreach (VertexType vertexType in this)
				vertexType.Dispose();

			base.ClearItems();
		}

		/// <summary>
		/// Function to create the vertex types.
		/// </summary>
		protected void CreateVertexTypes()
		{
			VertexType newType;		// Vertex type.

			// Position, Diffuse, 1 2D Texture Coord.
			newType = new VertexType();
			newType.CreateField(0, 0, VertexFieldContext.Position, VertexFieldType.Float3);
			newType.CreateField(0, 12, VertexFieldContext.Diffuse, VertexFieldType.Color);
			newType.CreateField(0, 16, VertexFieldContext.TexCoords, VertexFieldType.Float2);

			_items.Add("PositionDiffuse2DTexture1", newType);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		internal VertexTypes()
			: base(16)
		{
			CreateVertexTypes();
		}

		/// <summary>
		/// Destructor.
		/// </summary>
		~VertexTypes()
		{
			Dispose(false);
		}
		#endregion

		#region IDisposable Members

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		/// <param name="disposing">TRUE to release all resources, FALSE to only release unmanaged.</param>
		protected virtual void Dispose(bool disposing)
		{
			if (disposing)
				Clear();
		}

		/// <summary>
		/// Function to perform clean up.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
