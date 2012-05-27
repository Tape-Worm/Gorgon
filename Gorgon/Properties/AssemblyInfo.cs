#region MIT.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Saturday, July 09, 2005 5:35:10 PM
// 
#endregion

//#define BETA

using System;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security;
using System.Security.Permissions;
using System.Runtime.CompilerServices;

//
// General Information about an assembly is controlled through the following 
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
//
#if DEBUG
#if BETA
[assembly: AssemblyTitle("Gorgon - BETA - DEVELOPMENT VERSION")]
[assembly: AssemblyDescription("DirectX based 2D graphics library. - DEVELOPMENT VERSION - BETA")]
#else
[assembly: AssemblyTitle("Gorgon - DEVELOPMENT VERSION")]
[assembly: AssemblyDescription("DirectX based 2D graphics library. - DEVELOPMENT VERSION")]
#endif
[assembly: AssemblyConfiguration("DEBUG")]
#else
#if BETA
[assembly: AssemblyTitle("Gorgon - BETA VERSION")]
[assembly: AssemblyDescription("DirectX based 2D graphics library. - BETA VERSION")]
#else
[assembly: AssemblyTitle("Gorgon")]
[assembly: AssemblyDescription("DirectX based 2D graphics library.")]
#endif
[assembly: AssemblyConfiguration("RELEASE")]
#endif
[assembly: AssemblyCompany("Michael Winsor")]
#if DEBUG
[assembly: AssemblyProduct("Gorgon Graphics Library. - DEVELOPMENT VERSION")]
#else
[assembly: AssemblyProduct("Gorgon Graphics Library.")]
#endif
[assembly: AssemblyCopyright("Copyright © Michael Winsor 2006")]
[assembly: AssemblyTrademark("")]
[assembly: FileIOPermission(SecurityAction.RequestMinimum,AllFiles=FileIOPermissionAccess.AllAccess)]
[assembly: ReflectionPermission(SecurityAction.RequestMinimum,Flags=ReflectionPermissionFlag.MemberAccess)]
[assembly: SecurityPermission(SecurityAction.RequestMinimum,UnmanagedCode=true)]

// Setting ComVisible to false makes the types in this assembly not visible 
// to COM components.  If you need to access a type in this assembly from 
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("850FCA24-C086-4396-A304-42454546EF53")]

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

[assembly: AssemblyVersion("1.1.*")]
[assembly: AssemblyFileVersion("1.1.4529.31450")]
