#Region "LGPL."
' 
' Examples.
' Copyright (C) 2008 Michael Winsor
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
' Created: Friday, June 13, 2008 2:21:31 PM
' 
#End Region

Imports System
Imports System.Reflection
Imports System.Runtime.InteropServices
Imports System.Security.Permissions

' General Information about an assembly is controlled through the following 
' set of attributes. Change these attribute values to modify the information
' associated with an assembly.

' Review the values of the assembly attributes

<Assembly: AssemblyTitle("Teh Shadow Gn0s")> 
<Assembly: AssemblyDescription("Example to show how to use Gorgon in Visual Basic.NET")> 
<Assembly: AssemblyCompany("Michael Winsor")> 
<Assembly: AssemblyProduct("Gorgon Graphics Library.")> 
<Assembly: AssemblyCopyright("Copyright © Michael Winsor 2008")> 
<Assembly: AssemblyTrademark("")> 
<Assembly: CLSCompliant(False)> 
<Assembly: FileIOPermission(SecurityAction.RequestMinimum, AllFiles:=FileIOPermissionAccess.AllAccess)> 
<Assembly: ReflectionPermission(SecurityAction.RequestMinimum, Flags:=ReflectionPermissionFlag.MemberAccess)> 
<Assembly: SecurityPermission(SecurityAction.RequestMinimum, UnmanagedCode:=True)> 

<Assembly: ComVisible(False)>

'The following GUID is for the ID of the typelib if this project is exposed to COM
<Assembly: Guid("2c0532b2-3c18-49d6-8e06-09e54b4187a8")> 

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

<Assembly: AssemblyVersion("1.0.3014.31665")> 
<Assembly: AssemblyFileVersion("1.0.3014.31665")> 
