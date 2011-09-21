
//  Microsoft (R) .NET Framework IL Disassembler.  Version 4.0.30319.1
//  Copyright (c) Microsoft Corporation.  All rights reserved.



// Metadata version: v4.0.30319
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}
.assembly Gorgon.MemoryAccess
{
  .custom instance void [mscorlib]System.Reflection.AssemblyTitleAttribute::.ctor(string) = ( 01 00 0D 47 6F 72 67 6F 6E 2E 4D 65 6D 6F 72 79   // ...Gorgon.Memory
                                                                                              00 00 ) 
  .custom instance void [mscorlib]System.Reflection.AssemblyDescriptionAttribute::.ctor(string) = ( 01 00 2D 44 69 72 65 63 74 20 75 6E 6D 61 6E 61   // ..-Direct unmana
                                                                                                    67 65 64 20 6D 65 6D 6F 72 79 20 61 63 63 65 73   // ged memory acces
                                                                                                    73 20 66 75 6E 63 74 69 6F 6E 61 6C 69 74 79 2E   // s functionality.
                                                                                                    00 00 ) 
  .custom instance void [mscorlib]System.Reflection.AssemblyConfigurationAttribute::.ctor(string) = ( 01 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Reflection.AssemblyCompanyAttribute::.ctor(string) = ( 01 00 0E 4D 69 63 68 61 65 6C 20 57 69 6E 73 6F   // ...Michael Winso
                                                                                                72 00 00 )                                        // r..
  .custom instance void [mscorlib]System.Reflection.AssemblyProductAttribute::.ctor(string) = ( 01 00 0D 47 6F 72 67 6F 6E 2E 4D 65 6D 6F 72 79   // ...Gorgon.Memory
                                                                                                00 00 ) 
  .custom instance void [mscorlib]System.Reflection.AssemblyCopyrightAttribute::.ctor(string) = ( 01 00 20 43 6F 70 79 72 69 67 68 74 20 C2 A9 20   // .. Copyright .. 
                                                                                                  4D 69 63 68 61 65 6C 20 57 69 6E 73 6F 72 20 32   // Michael Winsor 2
                                                                                                  30 31 31 00 00 )                                  // 011..
  .custom instance void [mscorlib]System.Reflection.AssemblyTrademarkAttribute::.ctor(string) = ( 01 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Runtime.InteropServices.ComVisibleAttribute::.ctor(bool) = ( 01 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Runtime.InteropServices.GuidAttribute::.ctor(string) = ( 01 00 24 33 33 65 30 34 34 32 65 2D 62 64 64 62   // ..$33e0442e-bddb
                                                                                                  2D 34 63 38 66 2D 62 64 34 30 2D 39 38 31 61 34   // -4c8f-bd40-981a4
                                                                                                  35 38 37 34 61 31 35 00 00 )                      // 5874a15..
  .custom instance void [mscorlib]System.Reflection.AssemblyFileVersionAttribute::.ctor(string) = ( 01 00 07 31 2E 30 2E 30 2E 30 00 00 )             // ...1.0.0.0..
  .custom instance void [mscorlib]System.Runtime.Versioning.TargetFrameworkAttribute::.ctor(string) = ( 01 00 1A 2E 4E 45 54 46 72 61 6D 65 77 6F 72 6B   // ....NETFramework
                                                                                                        2C 56 65 72 73 69 6F 6E 3D 76 34 2E 30 01 00 54   // ,Version=v4.0..T
                                                                                                        0E 14 46 72 61 6D 65 77 6F 72 6B 44 69 73 70 6C   // ..FrameworkDispl
                                                                                                        61 79 4E 61 6D 65 10 2E 4E 45 54 20 46 72 61 6D   // ayName..NET Fram
                                                                                                        65 77 6F 72 6B 20 34 )                            // ework 4

  // --- The following custom attribute is added automatically, do not uncomment -------
  //  .custom instance void [mscorlib]System.Diagnostics.DebuggableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggableAttribute/DebuggingModes) = ( 01 00 02 00 00 00 00 00 ) 

  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                             63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.
  .hash algorithm 0x00008004
  .ver 1:0:0:0
}
.module Gorgon.MemoryAccess.dll
// MVID: {CCFCC4F1-0B70-4F64-9DBC-52A4D4F1DF15}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY
// Image base: 0x0000000002360000


// =============== CLASS MEMBERS DECLARATION ===================

.class public auto ansi GorgonLibrary.Data.DirectAccess
       extends [mscorlib]System.Object
{
	.method public hidebysig static pinvokeimpl("msvcrt.dll" as "memcpy" cdecl) 
		void*  CopyMemory(void* dest,
				  void* src,
				  int64 count) cil managed preservesig
	{
	  .custom instance void [mscorlib]System.Security.SuppressUnmanagedCodeSecurityAttribute::.ctor() = ( 01 00 00 00 ) 
	}

	.method public hidebysig static
		int32 SizeOf<valuetype .ctor T> () cil managed
	{
		IL_0000: sizeof !!T
		IL_0006: ret
	}
	
	.method public hidebysig static 
		void Write<valuetype .ctor T> (
			native int destination,
			!!T[] buffer,
			int32 offset,
			int32 size
		) cil managed 
	{
		// Method begins at RVA 0x1000
		// Code size 24 (0x18)
		.maxstack 3
		.locals (
			[0] !!T& pinned modopt([mscorlib]System.Runtime.CompilerServices.IsExplicitlyDereferenced) 
		)

		IL_0000: ldarg.1
		IL_0001: ldarg.2
		IL_0002: ldelema !!T
		IL_0007: stloc.0
		IL_0008: ldarga.s destination
		IL_000a: call instance void* [mscorlib]System.IntPtr::ToPointer()
		IL_000f: ldloc.0
		IL_0010: ldarg.3
		IL_0011: conv.i8
		IL_0012: unaligned. 1
		IL_0015: cpblk
		IL_0017: ret
	}

	.method public hidebysig static 
		void Writex86<valuetype .ctor T> (
			native int destination,
			!!T[] buffer,
			int32 offset,
			int32 size
		) cil managed 
	{
		// Method begins at RVA 0x1000
		// Code size 24 (0x18)
		.maxstack 3
		.locals (
			[0] !!T& pinned modopt([mscorlib]System.Runtime.CompilerServices.IsExplicitlyDereferenced) 
		)

		ldarg.1
		ldarg.2
		ldelema !!T
		stloc.0
		ldarga.s destination
		call instance void* [mscorlib]System.IntPtr::ToPointer()
		ldloc.0
		ldarg.3
		conv.i8
		call void * GorgonLibrary.Data.DirectAccess::CopyMemory(void *, void *, int64)
		pop
		ret
	}

	.method public hidebysig static 
		void Read<valuetype .ctor T> (
			native int source,
			!!T[] buffer,
			int32 offset,
			int32 size
		) cil managed 
	{
		// Method begins at RVA 0x1024
		// Code size 24 (0x18)
		.maxstack 3
		.locals (
			[0] !!T& pinned modopt([mscorlib]System.Runtime.CompilerServices.IsExplicitlyDereferenced) 
		)

		IL_0000: ldarg.1
		IL_0001: ldarg.2
		IL_0002: ldelema !!T
		IL_0007: stloc.0
		IL_0008: ldloc.0
		IL_0009: ldarga.s source
		IL_000b: call instance void* [mscorlib]System.IntPtr::ToPointer()
		IL_0010: ldarg.3
		IL_0011: conv.i8
		IL_0012: unaligned. 1
		IL_0015: cpblk
		IL_0017: ret
	}

	.method public hidebysig static 
		void Readx86<valuetype .ctor T> (
			native int source,
			!!T[] buffer,
			int32 offset,
			int32 size
		) cil managed 
	{
		// Method begins at RVA 0x1024
		// Code size 24 (0x18)
		.maxstack 3
		.locals (
			[0] !!T& pinned modopt([mscorlib]System.Runtime.CompilerServices.IsExplicitlyDereferenced) 
		)

		ldarg.1
		ldarg.2
		ldelema !!T
		stloc.0
		 ldloc.0
		ldarga.s source
		call instance void* [mscorlib]System.IntPtr::ToPointer()
		ldarg.3
		conv.i8
		call void* GorgonLibrary.Data.DirectAccess::CopyMemory(void *, void *, int64)
		pop
		ret
	}

	.method public hidebysig static 
		!!T Read<valuetype .ctor T> (
			native int source,
			int32 size
		) cil managed 
	{
		// Method begins at RVA 0x1070
		// Code size 26 (0x1a)
		.maxstack 3
		.locals (
			[0] !!T
		)

		IL_0000: ldloca.s 0
		IL_0002: initobj !!T
		IL_0008: ldloca.s 0
		IL_000a: ldarga.s source
		IL_000c: call instance void* [mscorlib]System.IntPtr::ToPointer()
		IL_0011: ldarg.1
		IL_0012: cpblk
		IL_0014: ldloc.0
		IL_0015: ret
	} 

	.method public hidebysig static 
		void Write<valuetype .ctor T> (
			native int destination,
			!!T 'value',
			int32 size
		) cil managed 
	{
		// Method begins at RVA 0x1048
		// Code size 25 (0x19)
		.maxstack 3

		IL_0000: ldarga.s destination
		IL_0002: call instance void* [mscorlib]System.IntPtr::ToPointer()
		IL_0007: ldarga.s 'value'
		IL_0009: ldarg.2
		IL_000a: cpblk
		IL_000c: ret
	} 
	
	.method public hidebysig static
		void Writex86
		(
			native int destination,
			native int source,
			int32 size
		) cil managed	
	{
		.maxstack 3		
			
 		ldarga.s   destination
  		call       instance void* [mscorlib]System.IntPtr::ToPointer()
  		ldarga.s   source
  		call       instance void* [mscorlib]System.IntPtr::ToPointer()
  		ldarg.2
  		conv.i8
  		call       void* GorgonLibrary.Data.DirectAccess::CopyMemory(void*,void*,int64)
  		pop	
  		ret
	}

	.method public hidebysig static
		void Write
		(
			native int destination,
			native int source,
			int32 size
		) cil managed	
	{
		.maxstack 3
		
		IL_0000: ldarga.s destination
		IL_0002: call instance void* [mscorlib]System.IntPtr::ToPointer()
		IL_0007: ldarga.s source
		IL_0009: call instance void* [mscorlib]System.IntPtr::ToPointer()
		IL_000e: ldarg.2
		IL_000f: cpblk
		IL_0011: ret
	}
	
	.method public hidebysig static
		void ZeroMemory
		(
			native int destination,
			int32 size
		) cil managed	
	{
		.maxstack 3
		
		ldarga.s destination
		call instance void* [mscorlib]System.IntPtr::ToPointer()
		ldc.i4.0
		ldarg.1
		initblk
		ret
	}	

  .method private hidebysig specialname rtspecialname static 
          void  .cctor() cil managed
  {
    // Code size       1 (0x1)
    .maxstack  8
    IL_0000:  ret
  } // end of method DirectAccess::.cctor

  .method public hidebysig specialname rtspecialname 
          instance void  .ctor() cil managed
  {
    // Code size       7 (0x7)
    .maxstack  8
    IL_0000:  ldarg.0
    IL_0001:  call       instance void [mscorlib]System.Object::.ctor()
    IL_0006:  ret
  } // end of method DirectAccess::.ctor

} // end of class GorgonLibrary.Data.DirectAccess


// =============================================================

// *********** DISASSEMBLY COMPLETE ***********************
// WARNING: Created Win32 resource file D:\Code\Current\Gorgon\Branches\2.x\Dorian\Libraries\Gorgon.Memory\Gorgon.Memory.res
