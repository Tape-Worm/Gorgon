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

namespace GorgonLibrary.Internal
{
	/// <summary>
	/// Renderer/rendering error codes.
	/// </summary>
	public enum RendererErrors
	{
		/// <summary>We cannot perform this operation in full screen mode.</summary>
		NotLegalInFullscreen = 0x7FFF0002,
		/// <summary>Cannot set the stream source.</summary>
		CannotSetStreamData = 0x7FFF0003,
		/// <summary>Render target was set to NULL.</summary>
		InvalidRenderTarget = 0x7FFF0005
	}

	/// <summary>
	/// Base exception for renderer exceptions.
	/// </summary>
	public abstract class RendererException
		: GorgonException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Error message.</param>
		/// <param name="code">Error code.</param>
		/// <param name="ex">Source exception.</param>
		public RendererException(string message, RendererErrors code, Exception ex)
			: base(message, (int)code, ex)
		{
			_errorCodeType = code.GetType();
		}
		#endregion
	}

	/// <summary>
	/// Cannot set stream exception.
	/// </summary>
	public class CannotSetStreamException 
		: RendererException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public CannotSetStreamException(string message, Exception ex)
			: base(message, RendererErrors.CannotSetStreamData, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public CannotSetStreamException(Exception ex)
			: this("Cannot set the vertex stream source data.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Invalid render target exception.
	/// </summary>
	public class InvalidRenderTargetException 
		: RendererException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public InvalidRenderTargetException(string message, Exception ex)
			: base(message, RendererErrors.InvalidRenderTarget, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public InvalidRenderTargetException(Exception ex)
			: this("The requested render target is invalid.  It may not have been created or is NULL.", ex)
		{
		}
		#endregion
	}

	/// <summary>
	/// Not legal in fullscreen exception.
	/// </summary>
	public class NotLegalInFullScreenException
		: RendererException
	{
		#region Constructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="message">Description of the error.</param>
		/// <param name="ex">Source exception.</param>
		public NotLegalInFullScreenException(string message, Exception ex)
			: base(message, RendererErrors.NotLegalInFullscreen, ex)
		{
		}

		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="ex">Source exception.</param>
		public NotLegalInFullScreenException(Exception ex)
			: this("This functionality cannot be executed while running in full screen mode.", ex)
		{
		}
		#endregion
	}
}
