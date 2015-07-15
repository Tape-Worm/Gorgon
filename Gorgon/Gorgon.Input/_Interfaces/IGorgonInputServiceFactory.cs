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
// Created: Saturday, July 11, 2015 9:32:45 AM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using Gorgon.Core;
using Gorgon.Plugins;

namespace Gorgon.Input
{
	/// <summary>
	/// The input service factory is used to create new input services from <see cref="IGorgonInputService"/> plug ins
	/// </summary>
	/// <remarks>
	/// <para>
	/// Use this object to load the desired input service from a plug in assembly (previously loaded through <see cref="IGorgonPluginAssemblyCache"/>). 
	/// </para>
	/// <para>
	/// An input service provides access to the various input devices attached to the computer, such as a keyboard, mouse, and/or gaming devices. A <see cref="IGorgonInputService"/> plug in may contain 1 or more 
	/// interfaces for providing access to the aforementioned devices. If support is not present for a device type, then TODO: PUT PROPERTY NAMES HERE will return <b>false</b>.
	/// </para>
	/// </remarks>
	public interface IGorgonInputServiceFactory
	{
		/// <summary>
		/// Function to create a new <see cref="IGorgonInputService"/> from a plugin.
		/// </summary>
		/// <param name="servicePluginName">The fully qualified type name of the plugin that contains the <see cref="GorgonInputService"/>.</param>
		/// <returns>The new <see cref="GorgonInputService"/>, or if it was previously created, the previously created instance.</returns>
		/// <exception cref="ArgumentNullException">Thrown when the <paramref name="servicePluginName"/> is <b>null</b> (<i>Nothing</i> in VB.Net)</exception>
		/// <exception cref="ArgumentException">Thrown when the <paramref name="servicePluginName"/> is empty.</exception>
		/// <exception cref="GorgonException">Thrown when the plugin specified by the <paramref name="servicePluginName"/> parameter was not found.</exception>
		IGorgonInputService CreateService(string servicePluginName);

		/// <summary>
		/// Function to create and retrieve all the input services from the available plugins in the plugin service.
		/// </summary>
		/// <param name="pluginAssembly">[Optional] The name of the assembly to load file system providers from.</param>
		/// <returns>A list of input services created from the plugins.</returns>
		/// <remarks>
		/// When the <paramref name="pluginAssembly"/> parameter is set to <b>null</b> (<i>Nothing</i> in VB.Net), then only the <see cref="GorgonInputService"/> objects within that assembly will 
		/// be loaded. Otherwise, all services available in the <see cref="GorgonPluginService"/> that was passed to the object constructor will be created (or have a previously created instance returned).
		/// </remarks>
		IEnumerable<IGorgonInputService> CreateServices(AssemblyName pluginAssembly = null);
	}
}
