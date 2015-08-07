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
// Created: Friday, July 15, 2011 6:22:27 AM
// 
#endregion

using Gorgon.Diagnostics;
using Gorgon.Input.WinForms;
using Gorgon.Input.WinForms.Properties;

namespace Gorgon.Input
{
	/// <summary>
	/// The entry point for the win forms input plug-in.
	/// </summary>
	public class GorgonWinFormsPlugIn
		: GorgonInputServicePlugin
	{
		#region Properties.
		/// <summary>
		/// Property to return whether the plugin supports game devices like game pads, or joysticks.
		/// </summary>
		public override bool SupportsJoysticks => false;

		/// <summary>
		/// Property to return whether the plugin supports pointing devices like mice, trackballs, etc...
		/// </summary>
		public override bool SupportsMice => true;

		/// <summary>
		/// Property to return whether the plugin supports keyboard devices.
		/// </summary>
		public override bool SupportsKeyboards => true;

		#endregion

		#region Methods.
		/// <inheritdoc/>
		protected override IGorgonInputService OnCreateInputService2(IGorgonLog log)
		{
			return new GorgonWinFormsInputService();
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonWinFormsPlugIn"/> class.
		/// </summary>
		public GorgonWinFormsPlugIn()
			: base(Resources.GORINP_WINFORMS_PLUGIN_DESC)
		{
		}
		#endregion
	}
}
