// This file is used by Code Analysis to maintain SuppressMessage 
// attributes that are applied to this project.
// Project-level suppressions either have no target or are given 
// a specific target and scoped to a namespace, type, member, etc.
//
// To add a suppression to this file, right-click the message in the 
// Code Analysis results, point to "Suppress Message", and click 
// "In Suppression File".
// You do not need to add suppressions to this file manually.

using System.Diagnostics.CodeAnalysis;

[assembly: SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return", Scope = "member", Target = "Gorgon.Native.Win32API.#GetWindowLongx86(System.Runtime.InteropServices.HandleRef,Gorgon.Native.WindowLongType)")]
[assembly: SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist", Scope = "member", Target = "Gorgon.Native.Win32API.#GetWindowLongx64(System.Runtime.InteropServices.HandleRef,Gorgon.Native.WindowLongType)")]
[assembly: SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "return", Scope = "member", Target = "Gorgon.Native.Win32API.#SetWindowLongx86(System.Runtime.InteropServices.HandleRef,Gorgon.Native.WindowLongType,System.IntPtr)")]
[assembly: SuppressMessage("Microsoft.Portability", "CA1901:PInvokeDeclarationsShouldBePortable", MessageId = "2", Scope = "member", Target = "Gorgon.Native.Win32API.#SetWindowLongx86(System.Runtime.InteropServices.HandleRef,Gorgon.Native.WindowLongType,System.IntPtr)")]
[assembly: SuppressMessage("Microsoft.Interoperability", "CA1400:PInvokeEntryPointsShouldExist", Scope = "member", Target = "Gorgon.Native.Win32API.#SetWindowLongx64(System.Runtime.InteropServices.HandleRef,Gorgon.Native.WindowLongType,System.IntPtr)")]
