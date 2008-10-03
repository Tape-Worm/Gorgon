#region MIT.
// 
// Gorgon.
// Copyright (C) 2006 Michael Winsor
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
// Created: Thursday, October 12, 2006 4:25:58 PM
// 
#endregion

using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using Microsoft.Win32;
using GorgonLibrary;
using GorgonLibrary.InputDevices.Internal;

namespace GorgonLibrary.InputDevices
{
	/// <summary>
	/// Object representing a list of joysticks.
	/// </summary>
	public class GorgonJoystickList
		: JoystickList 
	{
		#region Methods.
        /// <summary>
        /// Function to enumerate the available joysticks.
        /// </summary>
        /// <param name="includeDisconnected">TRUE to include all joysticks, include those that are disconnected, FALSE to exclude.</param>
        private void EnumerateJoysticks(bool includeDisconnected)
        {
            JOYCAPS capabilities = new JOYCAPS();	// Joystick capabilities.
            string name = string.Empty;				// Name of the joystick.
            int error = 0;							// Error code.
            bool connected = false;					// Is the joystick connected?
            int deviceCount = 0;					// Number of devices.
            int threshold = 0;						// Joystick threshold.
            GorgonJoystick joystick = null;		    // Joystick.

            // Get the number of devices.
            ClearItems();
            deviceCount = Win32API.joyGetNumDevs();
            if (deviceCount == 0)
                return;

            // Enumerate devices.
            for (int i = 0; i < deviceCount; i++)
            {
                error = Win32API.joyGetDevCaps(i, ref capabilities, Marshal.SizeOf(typeof(JOYCAPS)));

                // If the joystick has no registry key, then skip it.
                if ((capabilities.RegistryKey != null) && (capabilities.RegistryKey != string.Empty) &&
                    (capabilities.Name != null) && (capabilities.Name != string.Empty))
                {
                    // Check for error, stop enumeration.
                    if (error > 0)
                        throw new JoystickDriverNotPresentException();

                    // Get the name.
                    name = GetJoystickName(capabilities, i);
                    connected = IsJoystickConnected(i);
                    // Enumerate.
                    if (((connected) && (!includeDisconnected)) || (includeDisconnected))
                    {
                        error = Win32API.joyGetThreshold(i, out threshold);

                        // Check for error, stop enumeration.
                        if (error > 0)
                            throw new JoystickDriverNotPresentException();


                        if (!Contains(name))
                        {
                            joystick = new GorgonJoystick(i, name, capabilities, threshold, Owner);
                            AddItem(name, joystick);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Function to determine if a joystick is connected.
        /// </summary>
        /// <param name="ID">ID of the joystick to check.</param>
        /// <returns>TRUE if connected, FALSE if not.</returns>
        private bool IsJoystickConnected(int ID)
        {
            JOYINFO info = new JOYINFO();		// Joystick information.
            int error = 0;						// Error code.

            error = Win32API.joyGetPos(ID, ref info);

            switch (error)
            {
                case 0:
                    return true;
                case 6:
                    throw new JoystickDriverNotPresentException();
            }

            return false;
        }

        /// <summary>
        /// Function to retrieve a joystick name from the registry.
        /// </summary>
        /// <param name="joystickData">Joystick capability data.</param>
        /// <param name="ID">ID of the joystick to retrieve data for.</param>
        /// <returns>The name of the joystick.</returns>
        private string GetJoystickName(JOYCAPS joystickData, int ID)
        {
            RegistryKey rootKey = null;			// Root registry key.
            RegistryKey lookup = null;			// Look up key.
            RegistryKey nameKey = null;			// Name key.
            string key = string.Empty;			// Key name.
            string defaultName = string.Empty;	// Default name.

            try
            {
                defaultName = joystickData.AxisCount.ToString() + "-axis, " + joystickData.ButtonCount.ToString() + "-button joystick.";
                rootKey = Registry.CurrentUser;

                // Get the device ID.				
                lookup = rootKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\MediaResources\Joystick\" + joystickData.RegistryKey + @"\CurrentJoystickSettings");

                // Try the local machine key as a root if that lookup failed.
                if (lookup == null)
                {
                    rootKey.Close();
                    rootKey = Registry.LocalMachine;
                    lookup = rootKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\MediaResources\Joystick\" + joystickData.RegistryKey + @"\CurrentJoystickSettings");
                }

                if (lookup != null)
                {
                    key = lookup.GetValue("Joystick" + (ID + 1) + "OEMName", string.Empty).ToString();

                    // If we have no name, then build one.
                    if (key == string.Empty)
                        return defaultName;

                    // Get the name.
                    nameKey = rootKey.OpenSubKey(@"SYSTEM\CurrentControlSet\Control\MediaProperties\PrivateProperties\Joystick\OEM\" + key);
                    return nameKey.GetValue("OEMName", defaultName).ToString();
                }
                else
                    return defaultName;
            }
            catch (Exception ex)
            {
                throw new JoystickCannotGetNameException(ex);
            }
            finally
            {
                if (nameKey != null)
                    nameKey.Close();
                if (lookup != null)
                    lookup.Close();
                if (rootKey != null)
                    rootKey.Close();
            }
        }
        
		/// <summary>
		/// Function to refresh the joystick list.
		/// </summary>
		public override void Refresh()
		{
			// Remove the joysticks.
			ClearItems();

			// Enumerate joysticks.
			EnumerateJoysticks(false);
		}
		#endregion

		#region Constructor/Destructor.
		/// <summary>
		/// Constructor.
		/// </summary>
		/// <param name="owner">Owning input interface.</param>
		public GorgonJoystickList(Input owner)
			: base(owner)
		{
		}
		#endregion
	}
}
