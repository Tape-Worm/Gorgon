
// 
// Gorgon
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
// all copies or substantial portions of the Software
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE
// 
// Created: Wednesday, June 3, 2015 9:01:48 PM
// 

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security;

namespace Gorgon.PlugIns;

/// <summary>
/// An interface for interacting with the CLR
/// </summary>
[ComImport, Guid("9FD93CCF-3280-4391-B3A9-96E1CDE77C8D"), InterfaceType(ComInterfaceType.InterfaceIsIUnknown), SecurityCritical]
#pragma warning disable SYSLIB1096 // Convert to 'GeneratedComInterface'
internal interface IClrStrongName
#pragma warning restore SYSLIB1096 // Convert to 'GeneratedComInterface'
{
    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    int GetHashFromAssemblyFile([MarshalAs(UnmanagedType.LPStr)][In] string pszFilePath, [MarshalAs(UnmanagedType.U4)][In][Out] ref int piHashAlg, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)][Out] byte[] pbHash, [MarshalAs(UnmanagedType.U4)][In] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    int GetHashFromAssemblyFileW([MarshalAs(UnmanagedType.LPWStr)][In] string pwzFilePath, [MarshalAs(UnmanagedType.U4)][In][Out] ref int piHashAlg, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)][Out] byte[] pbHash, [MarshalAs(UnmanagedType.U4)][In] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    int GetHashFromBlob([In] nint pbBlob, [MarshalAs(UnmanagedType.U4)][In] int cchBlob, [MarshalAs(UnmanagedType.U4)][In][Out] ref int piHashAlg, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 4)][Out] byte[] pbHash, [MarshalAs(UnmanagedType.U4)][In] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    int GetHashFromFile([MarshalAs(UnmanagedType.LPStr)][In] string pszFilePath, [MarshalAs(UnmanagedType.U4)][In][Out] ref int piHashAlg, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)][Out] byte[] pbHash, [MarshalAs(UnmanagedType.U4)][In] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    int GetHashFromFileW([MarshalAs(UnmanagedType.LPWStr)][In] string pwzFilePath, [MarshalAs(UnmanagedType.U4)][In][Out] ref int piHashAlg, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)][Out] byte[] pbHash, [MarshalAs(UnmanagedType.U4)][In] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    int GetHashFromHandle([In] nint hFile, [MarshalAs(UnmanagedType.U4)][In][Out] ref int piHashAlg, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)][Out] byte[] pbHash, [MarshalAs(UnmanagedType.U4)][In] int cchHash, [MarshalAs(UnmanagedType.U4)] out int pchHash);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.U4)]
    int StrongNameCompareAssemblies([MarshalAs(UnmanagedType.LPWStr)][In] string pwzAssembly1, [MarshalAs(UnmanagedType.LPWStr)][In] string pwzAssembly2, [MarshalAs(UnmanagedType.U4)] out int dwResult);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    int StrongNameFreeBuffer([In] nint pbMemory);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    int StrongNameGetBlob([MarshalAs(UnmanagedType.LPWStr)][In] string pwzFilePath, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)][Out] byte[] pbBlob, [MarshalAs(UnmanagedType.U4)][In][Out] ref int pcbBlob);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    int StrongNameGetBlobFromImage([In] nint pbBase, [MarshalAs(UnmanagedType.U4)][In] int dwLength, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)][Out] byte[] pbBlob, [MarshalAs(UnmanagedType.U4)][In][Out] ref int pcbBlob);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    int StrongNameGetPublicKey([MarshalAs(UnmanagedType.LPWStr)][In] string pwzKeyContainer, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)][In] byte[] pbKeyBlob, [MarshalAs(UnmanagedType.U4)][In] int cbKeyBlob, out nint ppbPublicKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbPublicKeyBlob);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.U4)]
    int StrongNameHashSize([MarshalAs(UnmanagedType.U4)][In] int ulHashAlg, [MarshalAs(UnmanagedType.U4)] out int cbSize);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    int StrongNameKeyDelete([MarshalAs(UnmanagedType.LPWStr)][In] string pwzKeyContainer);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    int StrongNameKeyGen([MarshalAs(UnmanagedType.LPWStr)][In] string pwzKeyContainer, [MarshalAs(UnmanagedType.U4)][In] int dwFlags, out nint ppbKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbKeyBlob);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    int StrongNameKeyGenEx([MarshalAs(UnmanagedType.LPWStr)][In] string pwzKeyContainer, [MarshalAs(UnmanagedType.U4)][In] int dwFlags, [MarshalAs(UnmanagedType.U4)][In] int dwKeySize, out nint ppbKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbKeyBlob);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    int StrongNameKeyInstall([MarshalAs(UnmanagedType.LPWStr)][In] string pwzKeyContainer, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 2)][In] byte[] pbKeyBlob, [MarshalAs(UnmanagedType.U4)][In] int cbKeyBlob);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    int StrongNameSignatureGeneration([MarshalAs(UnmanagedType.LPWStr)][In] string pwzFilePath, [MarshalAs(UnmanagedType.LPWStr)][In] string pwzKeyContainer, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)][In] byte[] pbKeyBlob, [MarshalAs(UnmanagedType.U4)][In] int cbKeyBlob, [In][Out] nint ppbSignatureBlob, [MarshalAs(UnmanagedType.U4)] out int pcbSignatureBlob);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    int StrongNameSignatureGenerationEx([MarshalAs(UnmanagedType.LPWStr)][In] string wszFilePath, [MarshalAs(UnmanagedType.LPWStr)][In] string wszKeyContainer, [MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 3)][In] byte[] pbKeyBlob, [MarshalAs(UnmanagedType.U4)][In] int cbKeyBlob, [In][Out] nint ppbSignatureBlob, [MarshalAs(UnmanagedType.U4)] out int pcbSignatureBlob, [MarshalAs(UnmanagedType.U4)][In] int dwFlags);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    int StrongNameSignatureSize([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)][In] byte[] pbPublicKeyBlob, [MarshalAs(UnmanagedType.U4)][In] int cbPublicKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbSize);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.U4)]
    int StrongNameSignatureVerification([MarshalAs(UnmanagedType.LPWStr)][In] string pwzFilePath, [MarshalAs(UnmanagedType.U4)][In] int dwInFlags, [MarshalAs(UnmanagedType.U4)] out int dwOutFlags);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.U4)]
    int StrongNameSignatureVerificationEx([MarshalAs(UnmanagedType.LPWStr)][In] string pwzFilePath, [MarshalAs(UnmanagedType.I1)][In] bool fForceVerification, [MarshalAs(UnmanagedType.I1)] out bool fWasVerified);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    [return: MarshalAs(UnmanagedType.U4)]
    int StrongNameSignatureVerificationFromImage([In] nint pbBase, [MarshalAs(UnmanagedType.U4)][In] int dwLength, [MarshalAs(UnmanagedType.U4)][In] int dwInFlags, [MarshalAs(UnmanagedType.U4)] out int dwOutFlags);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    int StrongNameTokenFromAssembly([MarshalAs(UnmanagedType.LPWStr)][In] string pwzFilePath, out nint ppbStrongNameToken, [MarshalAs(UnmanagedType.U4)] out int pcbStrongNameToken);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    int StrongNameTokenFromAssemblyEx([MarshalAs(UnmanagedType.LPWStr)][In] string pwzFilePath, out nint ppbStrongNameToken, [MarshalAs(UnmanagedType.U4)] out int pcbStrongNameToken, out nint ppbPublicKeyBlob, [MarshalAs(UnmanagedType.U4)] out int pcbPublicKeyBlob);

    [MethodImpl(MethodImplOptions.PreserveSig | MethodImplOptions.InternalCall)]
    int StrongNameTokenFromPublicKey([MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)][In] byte[] pbPublicKeyBlob, [MarshalAs(UnmanagedType.U4)][In] int cbPublicKeyBlob, out nint ppbStrongNameToken, [MarshalAs(UnmanagedType.U4)] out int pcbStrongNameToken);
}
