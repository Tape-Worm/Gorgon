#region "LGPL."
' 
' Gorgon.
' Copyright (C) 2006 Michael Winsor
' 
' This library is free software; you can redistribute it and/or
' modify it under the terms of the GNU Lesser General Public
' License as published by the Free Software Foundation; either
' version 2.1 of the License, or (at your option) any later version.
' 
' This library is distributed in the hope that it will be useful,
' but WITHOUT ANY WARRANTY; without even the implied warranty of
' MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the GNU
' Lesser General Public License for more details.
' 
' You should have received a copy of the GNU Lesser General Public
' License along with this library; if not, write to the Free Software
' Foundation, Inc., 59 Temple Place, Suite 330, Boston, MA 02111-1307 USA
' 
' Created: Thursday, October 19, 2006 1:08:45 AM
' 
#end region

Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Security
Imports System.Security.Permissions

' General Information about an assembly is controlled through the following 
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes

<Assembly: AssemblyTitle("Example 1 - Initializing.")> 
<Assembly: AssemblyDescription("Example showing how to initialize Gorgon.")> 
<Assembly: AssemblyCompany("Michael Winsor")> 
<Assembly: AssemblyProduct("Gorgon Graphics Library.")> 
<Assembly: AssemblyCopyright("Copyright © Michael Winsor 2006")> 
<Assembly: AssemblyTrademark("")> 

<Assembly: ComVisible(False)> 
<Assembly: FileIOPermission(SecurityAction.RequestMinimum, AllFiles:=FileIOPermissionAccess.AllAccess)> 
<Assembly: ReflectionPermission(SecurityAction.RequestMinimum, Flags:=ReflectionPermissionFlag.MemberAccess)> 

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("cfb64f6e-758b-4642-a2d0-0046a9271c04")> 

' Version information for an assembly consists of the following four values:
'
'      Major Version
'      Minor Version 
'      Build Number
'      Revision
'
' You can specify all the values or you can default the Build and Revision Numbers 
' by using the '*' as shown below:
' <Assembly: AssemblyVersion("1.0.*")> 

<Assembly: AssemblyVersion("1.0.*")> 
<Assembly: AssemblyFileVersion("1.0.0.0")> 
