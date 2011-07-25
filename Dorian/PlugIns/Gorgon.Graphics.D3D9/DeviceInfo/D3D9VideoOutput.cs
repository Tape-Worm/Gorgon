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
// Created: Sunday, July 24, 2011 10:02:09 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GorgonLibrary.Graphics.D3D9
{
	/// <summary>
	/// A direct 3D9 video output.
	/// </summary>
	public class D3D9VideoOutput
		: GorgonVideoOutput
	{
		#region Properties.
		/// <summary>
		/// Property to return the device name of the output.
		/// </summary>
		public override string Name
		{
			get
			{
				throw new NotImplementedException();
			}
			protected set
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Property to return the display dimensions of the desktop.
		/// </summary>
		public override System.Drawing.Rectangle DesktopDimensions
		{
			get
			{
				throw new NotImplementedException();
			}
			protected set
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Property to return whether the output device is attached to the desktop.
		/// </summary>
		public override bool IsAttachedToDesktop
		{
			get
			{
				throw new NotImplementedException();
			}
			protected set
			{
				throw new NotImplementedException();
			}
		}

		/// <summary>
		/// Property to return the rotation in degrees for the monitor.
		/// </summary>
		public override int Rotation
		{
			get
			{
				throw new NotImplementedException();
			}
			protected set
			{
				throw new NotImplementedException();
			}
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="D3D9VideoOutput"/> class.
		/// </summary>
		public D3D9VideoOutput()
		{
			
		}
		#endregion
	}
}
