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
// Created: Saturday, March 25, 2006 9:37:47 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using SharpUtilities;

namespace GorgonLibrary.Graphics
{
	/// <summary>
	/// Cannot convert target into an image.
	/// </summary>
	public class RenderTargetCannotConvertException
		: GorgonException 
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="targetName">Name of the target that was being converted.</param>
		/// <param name="ex">Source exception.</param>
		public RenderTargetCannotConvertException(string targetName, Exception ex)
			: base("Unable to convert the target '" + targetName + "' into a standard image.")
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="targetName">Name of the target that was being converted.</param>
		/// <param name="reason">Reason for conversion failure.</param>
		public RenderTargetCannotConvertException(string targetName, string reason)
			: base("Unable to convert the target '" + targetName + "' into a standard image.\n" + reason, null)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="targetName">Name of the target that was being converted.</param>
		public RenderTargetCannotConvertException(string targetName)
			: this(targetName, (Exception)null)
		{
		}
	}

	/// <summary>
	/// Cannot find the specified render target.
	/// </summary>
	public class RenderTargetNotFoundException
		: GorgonException
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="targetName">Name of the render target that was not found.</param>
		/// <param name="ex">Source exception.</param>
		public RenderTargetNotFoundException(string targetName, Exception ex)
			: base("The render target '" + targetName + "' does not exist.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="targetName">Name of the render target that was not found.</param>
		public RenderTargetNotFoundException(string targetName)
			: this(targetName, null)
		{
		}
	}

	/// <summary>
	/// Render target already exists.
	/// </summary>
	public class RenderTargetAlreadyExistsException
		: GorgonException
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="targetName">Name of the render target that was not found.</param>
		/// <param name="ex">Source exception.</param>
		public RenderTargetAlreadyExistsException(string targetName, Exception ex)
			: base("The render target '" + targetName + "' already exists.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="targetName">Name of the render target that was not found.</param>
		public RenderTargetAlreadyExistsException(string targetName)
			: this(targetName, null)
		{
		}
	}

	/// <summary>
	/// Render target owner form/control is not valid.
	/// </summary>
	public class RenderTargetOwnerNotValidException
		: GorgonException
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public RenderTargetOwnerNotValidException(Exception ex)
			: base("Cannot find a parent window for the render target.",ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public RenderTargetOwnerNotValidException()
			: this(null)
		{
		}
	}

	/// <summary>
	/// Render target is full screen.
	/// </summary>
	public class RenderTargetIsFullScreenException
		: GorgonException
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public RenderTargetIsFullScreenException(Exception ex)
			: base("This operation cannot be performed while the primary target is in full screen mode.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public RenderTargetIsFullScreenException()
			: this(null)
		{
		}
	}

	/// <summary>
	/// Render target is not valid.
	/// </summary>
	public class RenderTargetNotValidException
		: GorgonException
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public RenderTargetNotValidException(Exception ex)
			: base("There is no valid render target assigned.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public RenderTargetNotValidException()
			: this(null)
		{
		}
	}

	/// <summary>
	/// Renderer cannot set the stream source.
	/// </summary>
	public class RendererCannotSetStreamException
		: GorgonException
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public RendererCannotSetStreamException(Exception ex)
			: base("Cannot set the stream source.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public RendererCannotSetStreamException()
			: this(null)
		{
		}
	}

	/// <summary>
	/// Render target creation failure.
	/// </summary>
	public class RenderTargetCreationFailureException
		: GorgonException
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public RenderTargetCreationFailureException(Exception ex)
			: base("Unable to create the render target.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public RenderTargetCreationFailureException()
			: this(null)
		{
		}
	}

	/// <summary>
	/// Primary render window is missing.
	/// </summary>
	public class RendererPrimaryNotCreatedException
		: GorgonException
	{
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public RendererPrimaryNotCreatedException(Exception ex)
			: base("The primary render window has not been created, SetMode must be called first.", ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		public RendererPrimaryNotCreatedException()
			: this(null)
		{
		}
	}
}
