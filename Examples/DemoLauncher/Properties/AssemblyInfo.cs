#region MIT.
// 
// Gorgon.
// Copyright (C) 2013 Michael Winsor
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
// Created: Monday, August 26, 2013 10:51:35 PM
// 
#endregion

using System.Reflection;
using System.Runtime.InteropServices;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
#if DEBUG
[assembly: AssemblyTitle("Gorgon Example/Demo Launcher [DEBUG VERSION]")]
[assembly: AssemblyDescription("The application that will run the various examples/demos that come with Gorgon. [DEBUG VERSION]")]
[assembly: AssemblyConfiguration("DEBUG")]
#else
[assembly: AssemblyTitle("Gorgon Example/Demo Launcher")]
[assembly: AssemblyDescription("The application that will run the various examples/demos that come with Gorgon.")]
[assembly: AssemblyConfiguration("RELEASE")]
#endif
[assembly: AssemblyCompany("Michael Winsor")]
#if DEBUG
[assembly: AssemblyProduct("Gorgon [DEBUG VERSON]")]
#else
[assembly: AssemblyProduct("Gorgon")]
#endif
[assembly: AssemblyCopyright("Copyright © Michael Winsor 2013")]
[assembly: AssemblyTrademark("")]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("AC2FBCA6-375F-4DA1-A9B3-A11CCE57C992")]

//
// Version information for an assembly consists of the following four values:
//
//      Major Version
//      Minor Version 
//      Build Number
//      Revision
//
// You can specify all the values or you can default the Revision and Build Numbers 
// by using the '*' as shown below:

[assembly: AssemblyVersion("2.0.*")]
[assembly: AssemblyFileVersion("2.0.0.0")]
