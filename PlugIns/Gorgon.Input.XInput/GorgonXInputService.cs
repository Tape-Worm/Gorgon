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
// Created: Friday, July 15, 2011 6:22:54 AM
// 
#endregion

using System;
using System.Linq;
using System.Collections.Generic;
using Gorgon.Diagnostics;
using Gorgon.Input.XInput.Properties;
using XI = SharpDX.XInput;

namespace Gorgon.Input.XInput
{
	/// <summary>
	/// The service for XInput controllers.
	/// </summary>
	class GorgonXInputService
		: GorgonInputService2
	{
		#region Variables.
		// The logger used for debugging.
		private readonly IGorgonLog _log;
		#endregion

		#region Methods.
		/// <inheritdoc/>
		protected override IReadOnlyList<IGorgonMouseInfo2> OnEnumerateMice()
		{
			return new IGorgonMouseInfo2[0];
		}

		/// <inheritdoc/>
		protected override IReadOnlyList<IGorgonKeyboardInfo2> OnEnumerateKeyboards()
		{
			return new IGorgonKeyboardInfo2[0];
		}

		/// <inheritdoc/>
		protected override IReadOnlyList<IGorgonJoystickInfo2> OnEnumerateJoysticks()
		{
			_log.Print("Enumerating XInput controllers...", LoggingLevel.Verbose);

			// Enumerate all controllers.
			IReadOnlyList<XInputJoystickInfo> result =
				(from deviceIndex in (XI.UserIndex[])Enum.GetValues(typeof(XI.UserIndex))
				 where deviceIndex != XI.UserIndex.Any
				 orderby deviceIndex
				 select
					 new XInputJoystickInfo(string.Format(Resources.GORINP_XINP_DEVICE_NAME, (int)deviceIndex + 1), deviceIndex))
					.ToArray();

			foreach (XInputJoystickInfo info in result)
			{
				_log.Print("Found XInput controller {0}", LoggingLevel.Verbose, info.Description);
				info.GetCaps(new XI.Controller(info.ID));
			}

			return result;
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonXInputService"/> class.
		/// </summary>
		/// <param name="log">The logger used for debugging.</param>
		/// <param name="registrar">The registrar used to register the devices.</param>
		/// <param name="coordinator">The coordinator used to manage state.</param>
		public GorgonXInputService(IGorgonLog log, XInputDeviceRegistrar registrar, XInputDeviceCoordinator coordinator)
			: base(registrar, coordinator)
		{
			_log = log;
		}
		#endregion
	}
}
