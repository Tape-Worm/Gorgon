#region MIT
// 
// Gorgon.
// Copyright (C) 2015 Michael Winsor
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
// Created: Monday, July 20, 2015 10:22:43 PM
// 
#endregion

using Gorgon.Diagnostics;
using System.Collections.Generic;

namespace Gorgon.Input.WinForms
{
	/// <summary>
	/// Windows forms input service for keyboard and mouse.
	/// </summary>
	class GorgonWinFormsInputService
		: GorgonInputService2
	{
		#region Variables.
		// Logger used for debugging.
		private readonly IGorgonLog _log;
		#endregion

		#region Methods.
		/// <inheritdoc/>
		protected override IReadOnlyList<IGorgonKeyboardInfo2> OnEnumerateKeyboards()
		{
			_log.Print("Win forms input plug in is using the system keyboard.", LoggingLevel.Verbose);

			return new IGorgonKeyboardInfo2[]
			       {
				       new WinFormsKeyboardInfo()
			       };
		}

		/// <inheritdoc/>
		protected override IReadOnlyList<IGorgonMouseInfo2> OnEnumerateMice()
		{
			_log.Print("Win forms input plug in is using the system mouse.", LoggingLevel.Verbose);

			return new IGorgonMouseInfo2[]
			       {
					   new WinFormsMouseInfo()
			       };
		}

		/// <inheritdoc/>
		protected override IReadOnlyList<IGorgonJoystickInfo2> OnEnumerateJoysticks()
		{
			_log.Print("Win forms input plug in does not support joystick devices.", LoggingLevel.Verbose);
			return new IGorgonJoystickInfo2[0];
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonWinFormsInputService"/> class.
		/// </summary>
		/// <param name="log">The log used for debugging.</param>
		/// <param name="registrar">The device registrar.</param>
		/// <param name="coordinator">The device event coordinator.</param>
		public GorgonWinFormsInputService(IGorgonLog log, IGorgonInputDeviceRegistrar registrar, GorgonInputDeviceDefaultCoordinator coordinator)
			: base(registrar, coordinator)
		{
			_log = log;
		}
		#endregion
	}
}
