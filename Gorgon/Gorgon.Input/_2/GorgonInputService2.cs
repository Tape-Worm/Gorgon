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
// Created: Saturday, July 18, 2015 4:37:48 PM
// 
#endregion

using System;
using System.Collections.Generic;

namespace Gorgon.Input
{
	/// <inheritdoc/>
	public abstract class GorgonInputService2 
		: IGorgonInputService
	{
		#region Properties.
		/// <inheritdoc/>
		public IGorgonInputDeviceRegistrar Registrar
		{
			get;
		}

		/// <inheritdoc/>
		public IGorgonInputDeviceCoordinator Coordinator
		{
			get;
		}
		#endregion

		#region Methods.
		/// <summary>
		/// Function to perform enumeration of keyboard devices.
		/// </summary>
		/// <returns>A list of <see cref="IGorgonKeyboardInfo2"/> items containing information about each keyboard.</returns>
		/// <remarks>
		/// <para>
		/// This is intended for use by input plug in developers. Developers may enumerate their devices in whichever mechanism is provided by the native input system and return a list of 
		/// <see cref="IGorgonKeyboardInfo2"/> items.
		/// </para>
		/// </remarks>
		protected abstract IReadOnlyList<IGorgonKeyboardInfo2> OnEnumerateKeyboards();

		/// <summary>
		/// Function to perform enumeration of mice or other pointing devices.
		/// </summary>
		/// <returns>A list of <see cref="IGorgonMouseInfo2"/> items containing information about each mouse.</returns>
		/// <remarks>
		/// <para>
		/// This is intended for use by input plug in developers. Developers may enumerate their devices in whichever mechanism is provided by the native input system and return a list of 
		/// <see cref="IGorgonMouseInfo2"/> items.
		/// </para>
		/// </remarks>
		protected abstract IReadOnlyList<IGorgonMouseInfo2> OnEnumerateMice();

		/// <summary>
		/// Function to perform enumeration of joysticks or other gaming devices.
		/// </summary>
		/// <returns>A list of <see cref="IGorgonMouseInfo2"/> items containing information about each joystick.</returns>
		/// <remarks>
		/// <para>
		/// This is intended for use by input plug in developers. Developers may enumerate their devices in whichever mechanism is provided by the native input system and return a list of 
		/// <see cref="IGorgonJoystickInfo2"/> items.
		/// </para>
		/// </remarks>
		protected abstract IReadOnlyList<IGorgonJoystickInfo2> OnEnumerateJoysticks();

		/// <inheritdoc/>
		public IReadOnlyList<IGorgonKeyboardInfo2> EnumerateKeyboards()
		{
			return OnEnumerateKeyboards();
		}

		/// <inheritdoc/>
		public IReadOnlyList<IGorgonMouseInfo2> EnumerateMice()
		{
			return OnEnumerateMice();
		}

		/// <inheritdoc/>
		public IReadOnlyList<IGorgonJoystickInfo2> EnumerateJoysticks()
		{
			return OnEnumerateJoysticks();
		}
		#endregion

		#region Constructor.
		/// <summary>
		/// Initializes a new instance of the <see cref="GorgonInputService2"/> class.
		/// </summary>
		/// <param name="registrar">The device registration system.</param>
		/// <param name="coordinator">The device event/state coordinator for the device.</param>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="registrar"/>, or the <paramref name="coordinator"/> parameters are <b>null</b>, (<i>Nothing</i> in VB.Net).</exception>
		/// <remarks>
		/// <para>
		/// The <paramref name="registrar"/> is a custom object defined by the plug in that registers one or more input devices to a window handle.
		/// </para>
		/// <para>
		/// The <paramref name="coordinator"/> is used to forward events to the appropriate <see cref="IGorgonInputEventDrivenDevice{T}"/>, or used to receive or send state to polled devices. 
		/// </para>
		/// </remarks>
		protected GorgonInputService2(IGorgonInputDeviceRegistrar registrar, IGorgonInputDeviceCoordinator coordinator)
		{
			if (registrar == null)
			{
				throw new ArgumentNullException(nameof(registrar));
			}

			if (coordinator == null)
			{
				throw new ArgumentNullException(nameof(coordinator));
			}

			Registrar = registrar;
			Coordinator = coordinator;
		}
		#endregion
	}
}
