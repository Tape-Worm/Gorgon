#region LGPL.
// 
// Gorgon.
// Copyright (C) 2005 Michael Winsor
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
// Created: Saturday, July 09, 2005 5:35:10 PM
// 
#endregion

#define BETA

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

[assembly: AssemblyVersion("1.0.2990.21619")]
[assembly: AssemblyFileVersion("1.0.2990.21619")]
