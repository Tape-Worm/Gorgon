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
// Created: Saturday, September 23, 2006 1:27:12 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using DX = SlimDX;
using D3D9 = SlimDX.Direct3D9;
using SharpUtilities;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Object representing a pass within the shader.
	/// </summary>
	public class ShaderPass
		: NamedObject
	{
		#region Variables.
		private D3D9.EffectHandle _effectHandle;		// Handle to the pass.
		private ShaderTechnique _owner;					// Technique that owns this pass.
		#endregion

		#region Properties.
		/// <summary>
		/// Property to return the owning technique for this pass.
		/// </summary>
		public ShaderTechnique Owner
		{
			get
			{
				return _owner;
			}
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to get pass information.
		/// </summary>
		/// <param name="index">Index of the pass.</param>
		private void GetPass(int index)
		{
			try
			{
				// Get handle and name.
				_effectHandle = _owner.Owner.D3DEffect.GetPass(_owner.D3DEffectHandle, index);
				_objectName = _owner.Owner.D3DEffect.GetPassDescription(_effectHandle).Name;
			}
			catch (Exception ex)
			{
				throw new ShaderCannotGetPassesException(_owner, ex);
			}
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="technique">Owning technique.</param>
		/// <param name="index">Index of the pass.</param>
		internal ShaderPass(ShaderTechnique technique, int index)
			: base("NO_NAME")
		{
			_owner = technique;
			_effectHandle = null;
			GetPass(index);
		}
		#endregion
	}
}
