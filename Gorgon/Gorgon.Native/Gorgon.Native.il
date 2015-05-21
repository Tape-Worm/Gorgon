
// 
// Gorgon.
// Copyright (C) 2011 Michael Winsor
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
// Created: Sunday, September 19, 2011 1:10:19 PM
// 

// This assembly contains unsafe methods for manipulating native memory.
// This assembly is meant for INTERNAL use by Gorgon only.  Use at your
// own risk.
//
// NO SUPPORT WILL BE GIVEN FOR THE USAGE OF THIS ASSEMBLY.
//


// Metadata version: v4.0.30319
.assembly extern mscorlib
{
  .publickeytoken = (B7 7A 5C 56 19 34 E0 89 )                         // .z\V.4..
  .ver 4:0:0:0
}
.assembly Gorgon.Native
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
  .custom instance void [mscorlib]System.Runtime.Versioning.TargetFrameworkAttribute::.ctor(string) = ( 01 00 1a 2e 4e 45 54 46 72 61 6d 65 77 6f 72 6b
													2c 56 65 72 73 69 6f 6e 3d 76 34 2e 35 01 00 54
													0e 14 46 72 61 6d 65 77 6f 72 6b 44 69 73 70 6c
													61 79 4e 61 6d 65 12 2e 4e 45 54 20 46 72 61 6d
													65 77 6f 72 6b 20 34 2e 35 )

  // --- The following custom attribute is added automatically, do not uncomment -------
  //  .custom instance void [mscorlib]System.Diagnostics.DebuggableAttribute::.ctor(valuetype [mscorlib]System.Diagnostics.DebuggableAttribute/DebuggingModes) = ( 01 00 02 00 00 00 00 00 ) 

  .custom instance void [mscorlib]System.Runtime.CompilerServices.CompilationRelaxationsAttribute::.ctor(int32) = ( 01 00 08 00 00 00 00 00 ) 
  .custom instance void [mscorlib]System.Runtime.CompilerServices.RuntimeCompatibilityAttribute::.ctor() = ( 01 00 01 00 54 02 16 57 72 61 70 4E 6F 6E 45 78   // ....T..WrapNonEx
                                                                                                             63 65 70 74 69 6F 6E 54 68 72 6F 77 73 01 )       // ceptionThrows.
  .hash algorithm 0x00008004
  .ver 1:0:0:0
}
.module Gorgon.Native.dll
// MVID: {CCFCC4F1-0B70-4F64-9DBC-52A4D4F1DF15}
.imagebase 0x00400000
.file alignment 0x00000200
.stackreserve 0x00100000
.subsystem 0x0003       // WINDOWS_CUI
.corflags 0x00000001    //  ILONLY
// Image base: 0x0000000002360000


// =============== CLASS MEMBERS DECLARATION ===================

.class public auto ansi Gorgon.Native.DirectAccess
       extends [mscorlib]System.Object
{
	// Function to return the size of a type (value types only) in bytes.
	.method public hidebysig static
		int32 SizeOf<valuetype .ctor T> () cil managed
	{
		.maxstack 1
		
		IL_0000: sizeof !!T
		IL_0006: ret
	}
	
	// Function to write an array of value types to native memory.
	.method public hidebysig static 
		void WriteArray<valuetype .ctor T> (
			native int destination,
			!!T[] buffer,
			int32 arrayIndex,
			int32 sizeInBytes
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
		unaligned. 1
		cpblk
		ret
	}

	// Function to write an array of value types to native memory.
	.method public hidebysig static 
		void WriteArray<valuetype .ctor T> (
			void *destination,
			!!T[] buffer,
			int32 arrayIndex,
			int32 sizeInBytes
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
		ldarg.0
		ldloc.0
		ldarg.3
		unaligned. 1
		cpblk
		ret
	}

	// Function to read an array of value types from native memory.
	.method public hidebysig static 
		void ReadArray<valuetype .ctor T> (
			native int source,
			!!T[] buffer,
			int32 arrayIndex,
			int32 sizeInBytes
		) cil managed 
	{
		// Method begins at RVA 0x1024
		// Code size 24 (0x18)
		.maxstack 8
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
		unaligned. 1
		cpblk
		ret
	}

	// Function to read an array of value types from native memory.
	.method public hidebysig static 
		void ReadArray<valuetype .ctor T> (
			void *source,
			!!T[] buffer,
			int32 arrayIndex,
			int32 sizeInBytes
		) cil managed 
	{
		// Method begins at RVA 0x1024
		// Code size 24 (0x18)
		.maxstack 8
		.locals (
			[0] !!T& pinned modopt([mscorlib]System.Runtime.CompilerServices.IsExplicitlyDereferenced) 
		)

		ldarg.1
		ldarg.2
		ldelema !!T
		stloc.0
		ldloc.0
		ldarg.0
		ldarg.3
		unaligned. 1
		cpblk
		ret
	}

	// Function to read a value types to native memory.
	.method public hidebysig static 
		void ReadValue<valuetype .ctor T> (
			native int source,
			[out] !!T& retValue 
		) cil managed 
	{
		// Method begins at RVA 0x1070
		// Code size 26 (0x1a)
		.maxstack 3
		.locals (
			[0] !!T& pinned 
		)

		ldarg.1
		stloc.0
		ldloc.0
		ldarga.s source
		call instance void* [mscorlib]System.IntPtr::ToPointer()
		sizeof !!T
		unaligned. 1
		cpblk
		ret
	} 
	// Function to read a value types to native memory.
	.method public hidebysig static 
		void ReadValue<valuetype .ctor T> (
			void *source,
			[out] !!T& retValue 
		) cil managed 
	{
		// Method begins at RVA 0x1070
		// Code size 26 (0x1a)
		.maxstack 3
		.locals (
			[0] !!T& pinned 
		)

		ldarg.1
		stloc.0
		ldloc.0
		ldarg.0
		sizeof !!T
		unaligned. 1
		cpblk
		ret
	} 

	// Function to write a value type to native memory.
	.method public hidebysig static 
		void WriteValue<valuetype .ctor T> (
			native int destination,
			!!T& 'value'
		) cil managed 
	{
		// Method begins at RVA 0x1048
		// Code size 25 (0x19)
		.maxstack 3
		.locals (
			[0] !!T& pinned 
		)
		
		ldarg.1
		stloc.0
		ldarga.s destination
		call instance void* [mscorlib]System.IntPtr::ToPointer()
		ldloc.0
		sizeof !!T
		unaligned. 1
		cpblk
		ret
	} 
	// Function to write a value type to native memory.
	.method public hidebysig static 
		void WriteValue<valuetype .ctor T> (
			void *destination,
			!!T& 'value'
		) cil managed 
	{
		.maxstack 3
		.locals (
			[0] !!T& pinned 
		)
		
		ldarg.1
		stloc.0
		ldarg.0
		ldloc.0
		sizeof !!T
		unaligned. 1
		cpblk
		ret
	} 
	// Function to copy data from one area of native memory to another.
	.method public hidebysig static
		void MemoryCopy
		(
			native int destination,
			native int source,
			int32 size
		) cil managed	
	{
		.maxstack 3
		
		ldarga.s destination
		call instance void* [mscorlib]System.IntPtr::ToPointer()
		ldarga.s source
		call instance void* [mscorlib]System.IntPtr::ToPointer()
		ldarg.2
		unaligned. 1
		cpblk		
		ret
	}

	// Function to copy data from one area of native memory to another.
	.method public hidebysig static
		void MemoryCopy
		(
			void *destination,
			void *source,
			int32 size
		) cil managed	
	{
		.maxstack 3
		
		ldarg.0
		ldarg.1
		ldarg.2
		unaligned. 1
		cpblk
		ret
	}	
	// Function to initialize memory with zeroes.
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
		unaligned. 1
		initblk
		ret
	}	
	// Function to initialize memory with zeroes.
	.method public hidebysig static
		void ZeroMemory
		(
			void *destination,
			int32 size
		) cil managed	
	{
		.maxstack 3
		
		ldarg.0
		ldc.i4.0
		ldarg.1
		unaligned. 1
		initblk
		ret
	}	
	// Function to initialize memory with a byte value.
	.method public hidebysig static
		void FillMemory
		(
			native int destination,
			unsigned int8 fillValue,
			int32 size
		) cil managed	
	{
		.maxstack 3
		
		ldarga.s destination
		call instance void* [mscorlib]System.IntPtr::ToPointer()
		ldarg.1
		ldarg.2
		unaligned. 1
		initblk
		ret
	}	
	// Function to initialize memory with a byte value.
	.method public hidebysig static
		void FillMemory
		(
			void *destination,
			unsigned int8 fillValue,
			int32 size
		) cil managed	
	{
		.maxstack 3
		
		ldarg.0
		ldarg.1
		ldarg.2
		unaligned. 1
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

} // end of class GorgonLibrary.Native.DirectAccess


// =============================================================
