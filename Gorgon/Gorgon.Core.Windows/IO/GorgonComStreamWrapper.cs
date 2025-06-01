// Gorgon.
// Copyright (C) 2025 Michael Winsor
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
// Created: May 11, 2025 12:48:52 AM
//

using System.Buffers;
using System.Collections;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Gorgon.Math;
using Windows.Win32.Foundation;
using Windows.Win32.System.Com;

namespace Gorgon.IO;

/// <summary>
/// Wraps a .NET <see cref="Stream"/> object as a COM <c>IStream</c> type.
/// </summary>
/// <remarks>
/// <para>
/// There are scenarios where using COM objects from Native Windows APIs is necessary, and sometimes they require the use of a COM stream (i.e. <c>IStream</c>). The .NET <see cref="Stream"/> type is very 
/// similar to the <c>IStream</c> type, but it is not a COM compatible, nor does it implement the <c>IStream</c> interface.
/// </para>
/// <para>
/// This wrapper allows COM to use a .NET <see cref="Stream"/> as an <c>IStream</c> COM object. To do this the application just creates the .NET <see cref="Stream"/>, then this wrapper object and passes the 
/// .NET <see cref="Stream"/> to the constructor. When the COM <c>IStream</c> type is required, applications can just cast it to a pointer type (<see cref="nint"/> or <c>void*</c>), and then passed to 
/// the COM method that requires the stream. 
/// </para>
/// <para>
/// If applications are using the <a href="https://www.nuget.org/packages/Microsoft.Windows.CsWin32/" target="_blank">CsWin32</a> nuget package (very highly recommended for interop), then the pointer can 
/// even be cast to a <c>IStream*</c> pointer.
/// </para>
/// <para>
/// <note type="warning">
/// <para>
/// This type is used with native, unmanaged code, namely COM. Therefore this object returns raw pointers, and if improperly used, can cause application crashes or worse. Please use this object with extreme 
/// care. 
/// </para>
/// <para>
/// This object makes no attempt to verify the size of the pointers passed in. Buffer overruns are very possible and can corrupt memory.
/// </para>
/// </note>
/// <para>
/// This object <b>is in NO WAY thread safe</b>. Do not use across multiple threads.
/// </para>
/// </para>
/// </remarks>
/// <seealso cref="Stream"/>
/// <param name="stream">The stream to wrap.</param>
/// <param name="ownsStream"><b>true</b> if the wrapper takes ownership of the passed in stream, <b>false</b> if not.</param>
/// <remarks>
/// <para>
/// When the <paramref name="ownsStream"/> is <b>true</b>, then the wrapper takes ownership of the stream and will dispose of it when the <see cref="IDisposable.Dispose"/> method is called. Otherwise, the caller is 
/// responsible for managing the lifetime of the parent stream.
/// </para>
/// </remarks>
public sealed unsafe class GorgonComStreamWrapper(Stream stream, bool ownsStream)
                        : ComWrappers, IStream.Interface, IDisposable
{
    // The COM IID for the IStream interface.
    private static readonly Guid _guid = new(0xC, 0x0, 0x0, 0xC0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x0, 0x46);

    // The HRESULT codes returned by the forwarded methods.
    private static readonly HRESULT _eNotSupported = new(unchecked((int)0x80004021));
    private static readonly HRESULT _sOk = new(0);
    private static readonly HRESULT _ePointer = new(unchecked((int)0x80004003));

    // The vtables used for the COM interfaces.
    private static readonly void* _iStreamVtbl;
    private static readonly void* _iSequentialStreamVtbl;

    // The list of interfaces.
    private static readonly ComInterfaceEntry* _interfaceEntries;

    // The size of the internal transfer buffer.
    private const int MaxBufferSize = 4096;

    // The transfer internal buffer.
    private readonly byte[] _buffer = ArrayPool<byte>.Shared.Rent(MaxBufferSize);

    /// <summary>
    /// Property to return the .NET stream object wrapped by this object.
    /// </summary>
    /// <remarks>
    /// Users should be very careful when manipulating the stream directly, otherwise issues may occur on the native COM side.
    /// </remarks>
    public Stream BaseStream
    {
        get;
        private set;
    } = stream;

    /// <summary>
    /// Property to return whether the wrapper owns the base stream.
    /// </summary>
    public bool OwnsBaseStream
    {
        get;
        private set;
    } = ownsStream;

    /// <summary>
    /// Function to read the data from the stream, up to the specified number of bytes, into the supplied pointer.
    /// </summary>
    /// <param name="pv">The pointer that will receive the data.</param>
    /// <param name="cb">The number of bytes to read.</param>
    /// <param name="pcbRead">The actual number of bytes read.</param>
    /// <returns>An HRESULT error code. E_POINTER indicates that the <paramref name="pv"/> pointer is NULL, E_NOTSUPPORTED indicates that this stream is write-only.</returns>
    HRESULT IStream.Interface.Read(void* pv, uint cb, uint* pcbRead)
    {
        if (pv is null)
        {
            if (pcbRead is not null)
            {
                *pcbRead = 0;
            }
            return _ePointer;
        }

        if (!BaseStream.CanRead)
        {
            if (pcbRead is not null)
            {
                *pcbRead = 0;
            }
            return _eNotSupported;
        }

        if (cb == 0)
        {
            if (pcbRead is not null)
            {
                *pcbRead = 0;
            }
            return _sOk;
        }

        byte* dest = (byte*)pv;

        while (cb > 0)
        {
            uint readBytes = (uint)BaseStream.Read(_buffer, 0, (int)cb.Min(MaxBufferSize));

            if (readBytes == 0)
            {
                return _sOk;
            }

            fixed (byte* bytePtr = _buffer)
            {
                Unsafe.CopyBlock(dest, bytePtr, readBytes);
            }

            dest += readBytes;
            if (pcbRead is not null)
            {
                *pcbRead += readBytes;
            }
            cb -= readBytes;
        }

        return _sOk;
    }

    /// <summary>
    /// Function to write the data from the specified pointer, up to the specified number of bytes, into the stream.
    /// </summary>
    /// <param name="pv">The pointer that will send the data.</param>
    /// <param name="cb">The number of bytes to writeead.</param>
    /// <param name="pcbWritten">The actual number of bytes written.</param>
    /// <returns>An HRESULT error code. E_POINTER indicates that the <paramref name="pv"/> pointer is NULL, E_NOTSUPPORTED indicates that the stream is read only.</returns>
    HRESULT IStream.Interface.Write(void* pv, uint cb, uint* pcbWritten)
    {
        if (pv is null)
        {
            if (pcbWritten is not null)
            {
                *pcbWritten = 0;
            }
            return _ePointer;
        }

        if (!BaseStream.CanWrite)
        {
            if (pcbWritten is not null)
            {
                *pcbWritten = 0;
            }
            return _eNotSupported;
        }

        if (cb == 0)
        {
            if (pcbWritten is not null)
            {
                *pcbWritten = 0;
            }
            return _sOk;
        }

        byte* src = (byte*)pv;

        while (cb > 0)
        {
            uint writeSize = cb.Min(MaxBufferSize);

            fixed (byte* bytePtr = _buffer)
            {
                Unsafe.CopyBlock(bytePtr, src, writeSize);
            }

            BaseStream.Write(_buffer, 0, (int)writeSize);

            src += writeSize;
            if (pcbWritten is not null)
            {
                *pcbWritten += writeSize;
            }
            cb -= writeSize;
        }

        return _sOk;
    }

    /// <summary>
    /// Function to reposition the cursor position in the stream.
    /// </summary>
    /// <param name="dlibMove">The number of bytes to add or subtract from the position.</param>
    /// <param name="dwOrigin">The starting point of the seek operation.</param>
    /// <param name="plibNewPosition">The new position for the stream.</param>
    /// <returns>An HRESULT error code. E_NOTSUPPORTED indicates that the underlying stream does not support seeking.</returns>
    HRESULT IStream.Interface.Seek(long dlibMove, SeekOrigin dwOrigin, ulong* plibNewPosition)
    {
        if (!BaseStream.CanSeek)
        {
            if (plibNewPosition is not null)
            {
                *plibNewPosition = (ulong)BaseStream.Position;
            }
            return _eNotSupported;
        }

        if (plibNewPosition is not null)
        {
            *plibNewPosition = (ulong)BaseStream.Seek(dlibMove, dwOrigin);
        }
        else
        {
            BaseStream.Seek(dlibMove, dwOrigin);
        }
        return _sOk;
    }

    /// <summary>
    /// This method is not used.
    /// </summary>
    /// <param name="libNewSize">This method is not used.</param>
    /// <returns>A success code.</returns>
    HRESULT IStream.Interface.SetSize(ulong libNewSize) => _sOk;

    /// <summary>
    /// Function to copy the contents of this stream, up to the number of bytes specified, into the supplied <c>IStream</c>.
    /// </summary>
    /// <param name="pstm">The stream that will receive the data.</param>
    /// <param name="cb">The number of bytes to copy.</param>
    /// <param name="pcbRead">The number of bytes read from the source stream.</param>
    /// <param name="pcbWritten">The number of bytes written to the destination stream.</param>
    /// <returns>A HRESULT error code. E_POINTER indicates that the <paramref name="pstm"/> value is NULL, E_NOTSUPPORTED indicates that the <paramref name="pstm"/> is read only, or this stream is write-only.</returns>
    /// <exception cref="EndOfStreamException">Thrown if the the size of the data requested was larger than the available stream size remaining.</exception>
    HRESULT IStream.Interface.CopyTo(IStream* pstm, ulong cb, ulong* pcbRead, ulong* pcbWritten)
    {
        if (pstm is null)
        {
            if (pcbRead is not null)
            {
                *pcbRead = 0;
            }
            if (pcbWritten is not null)
            {
                *pcbWritten = 0;
            }
            return _ePointer;
        }

        if (!BaseStream.CanRead)
        {
            if (pcbRead is not null)
            {
                *pcbRead = 0;
            }
            if (pcbWritten is not null)
            {
                *pcbWritten = 0;
            }
            return _eNotSupported;
        }

        if (cb == 0)
        {
            if (pcbRead is not null)
            {
                *pcbRead = 0;
            }
            if (pcbWritten is not null)
            {
                *pcbWritten = 0;
            }

            return _sOk;
        }

        if (cb > (ulong)(BaseStream.Length - BaseStream.Position))
        {
            if (pcbRead is not null)
            {
                *pcbRead = 0;
            }
            if (pcbWritten is not null)
            {
                *pcbWritten = 0;
            }
            throw new EndOfStreamException();
        }

        pstm->AddRef();

        try
        {
            while (cb > 0)
            {
                uint bytesRead = (uint)BaseStream.Read(_buffer, 0, (int)cb.Min(MaxBufferSize));

                if (bytesRead == 0)
                {
                    return _sOk;
                }

                uint bytesWritten = 0;
                fixed (byte* ptr = _buffer)
                {
                    HRESULT err = pstm->Write(ptr, bytesRead, &bytesWritten);

                    if (err.Failed)
                    {
                        if (pcbRead is not null)
                        {
                            *pcbRead = 0;
                        }
                        if (pcbWritten is not null)
                        {
                            *pcbWritten = 0;
                        }
                        return err;
                    }
                }

                cb -= bytesRead;
                if (pcbRead is not null)
                {
                    *pcbRead += bytesRead;
                }
                if (pcbWritten is not null)
                {
                    *pcbWritten += bytesWritten;
                }
            }

            return _sOk;
        }
        finally
        {
            pstm->Release();
        }
    }

    /// <summary>
    /// We do not support transactional streams.
    /// </summary>
    /// <param name="grfCommitFlags"></param>
    /// <returns>E_NOTSUPPORTED</returns>
    HRESULT IStream.Interface.Commit(uint grfCommitFlags) => _eNotSupported;
    /// <summary>
    /// We do not support transactional streams.
    /// </summary>
    /// <returns>E_NOTSUPPORTED</returns>
    HRESULT IStream.Interface.Revert() => _eNotSupported;
    /// <summary>
    /// We do not support transactional streams.
    /// </summary>
    /// <param name="cb"></param>
    /// <param name="dwLockType"></param>
    /// <param name="libOffset"></param>
    /// <returns>E_NOTSUPPORTED</returns>
    HRESULT IStream.Interface.LockRegion(ulong libOffset, ulong cb, uint dwLockType) => _eNotSupported;
    /// <summary>
    /// We do not support transactional streams.
    /// </summary>
    /// <param name="cb"></param>
    /// <param name="libOffset"></param>
    /// <param name="dwLockType"></param>
    /// <returns>E_NOTSUPPORTED</returns>
    HRESULT IStream.Interface.UnlockRegion(ulong libOffset, ulong cb, uint dwLockType) => _eNotSupported;

    /// <summary>
    /// Function to return statistics about the stream storage.
    /// </summary>
    /// <param name="pstatstg">The STATSTG structure that will receive the data.</param>
    /// <param name="grfStatFlag">Flags for the operation of this method (unused).</param>
    /// <returns>A HRESULT error code. E_POINTER indicates that the <paramref name="pstatstg"/> is NULL.</returns>
    HRESULT IStream.Interface.Stat(STATSTG* pstatstg, uint grfStatFlag)
    {
        if (pstatstg is null)
        {
            return _ePointer;
        }

        ulong len = (ulong)BaseStream.Length;

        // This is a new writable stream, so it has infinite length.
        if ((BaseStream.CanWrite) && (len == 0))
        {
            len = ulong.MaxValue;
        }

        pstatstg->type = 2;
        pstatstg->cbSize = len;
        pstatstg->grfLocksSupported = LOCKTYPE.LOCK_EXCLUSIVE;
        pstatstg->grfMode = BaseStream.CanWrite ? STGM.STGM_READWRITE : STGM.STGM_READ;

        return _sOk;
    }

    /// <summary>
    /// Function to clone the stream.
    /// </summary>
    /// <param name="ppstm">The cloned stream.</param>
    /// <returns>A HRESULT error code.</returns>
    HRESULT IStream.Interface.Clone(IStream** ppstm)
    {
        GorgonComStreamWrapper newWrapper = new(BaseStream, false);

        void* ptr = ToIStream(out HRESULT result);

        if (result.Failed)
        {
            *ppstm = null;
            return result;
        }

        *ppstm = (IStream*)ptr;

        return _sOk;
    }

    /// <summary>
    /// Function to read the data from the stream, up to the specified number of bytes, into the supplied pointer.
    /// </summary>
    /// <param name="pv">The pointer that will receive the data.</param>
    /// <param name="cb">The number of bytes to read.</param>
    /// <param name="pcbRead">The actual number of bytes read.</param>
    /// <returns>An HRESULT error code.</returns>
    HRESULT ISequentialStream.Interface.Read(void* pv, uint cb, uint* pcbRead) => ((IStream.Interface)this).Read(pv, cb, pcbRead);

    /// <summary>
    /// Function to write the data from the specified pointer, up to the specified number of bytes, into the stream.
    /// </summary>
    /// <param name="pv">The pointer that will send the data.</param>
    /// <param name="cb">The number of bytes to writeead.</param>
    /// <param name="pcbWritten">The actual number of bytes written.</param>
    /// <returns>An HRESULT error code.</returns>
    HRESULT ISequentialStream.Interface.Write(void* pv, uint cb, uint* pcbWritten) => ((IStream.Interface)this).Write(pv, cb, pcbWritten);

    /// <summary>
    /// Reads data from the stream into the pointer.
    /// </summary>
    /// <param name="thiscom"></param>
    /// <param name="pv"></param>
    /// <param name="cb"></param>
    /// <param name="pcbRead"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly]
    private static HRESULT Read(void* thiscom, void* pv, uint cb, uint* pcbRead)
    {
        IStream.Interface istream = ComInterfaceDispatch.GetInstance<IStream.Interface>((ComInterfaceDispatch*)thiscom);
        return istream.Read(pv, cb, pcbRead).ThrowOnFailure();
    }

    /// <summary>
    /// Writes data from the pointer into the stream.
    /// </summary>
    /// <param name="thiscom"></param>
    /// <param name="pv"></param>
    /// <param name="cb"></param>
    /// <param name="pcbRead"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly]
    private static HRESULT Write(void* thiscom, void* pv, uint cb, uint* pcbRead)
    {
        IStream.Interface istream = ComInterfaceDispatch.GetInstance<IStream.Interface>((ComInterfaceDispatch*)thiscom);
        return istream.Write(pv, cb, pcbRead).ThrowOnFailure();
    }

    /// <summary>
    /// Sequential stream read operation.
    /// </summary>
    /// <param name="thiscom"></param>
    /// <param name="pv"></param>
    /// <param name="cb"></param>
    /// <param name="pcbRead"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly]
    private static HRESULT SeqRead(void* thiscom, void* pv, uint cb, uint* pcbRead)
    {
        ISequentialStream.Interface istream = ComInterfaceDispatch.GetInstance<ISequentialStream.Interface>((ComInterfaceDispatch*)thiscom);
        return istream.Read(pv, cb, pcbRead).ThrowOnFailure();
    }

    /// <summary>
    /// Sequential stream write operation.
    /// </summary>
    /// <param name="thiscom"></param>
    /// <param name="pv"></param>
    /// <param name="cb"></param>
    /// <param name="pcbRead"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly]
    private static HRESULT SeqWrite(void* thiscom, void* pv, uint cb, uint* pcbRead)
    {
        ISequentialStream.Interface istream = ComInterfaceDispatch.GetInstance<ISequentialStream.Interface>((ComInterfaceDispatch*)thiscom);
        return istream.Write(pv, cb, pcbRead).ThrowOnFailure();
    }

    /// <summary>
    /// Seeks to a location within the stream.
    /// </summary>
    /// <param name="thiscom"></param>
    /// <param name="dLibMove"></param>
    /// <param name="dwOrigin"></param>
    /// <param name="plibNewPosition"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly]
    private static HRESULT Seek(void* thiscom, long dLibMove, SeekOrigin dwOrigin, ulong* plibNewPosition)
    {
        IStream.Interface istream = ComInterfaceDispatch.GetInstance<IStream.Interface>((ComInterfaceDispatch*)thiscom);
        return istream.Seek(dLibMove, dwOrigin, plibNewPosition).ThrowOnFailure();
    }

    /// <summary>
    /// Sets the size of the stream, in bytes.  Not used.
    /// </summary>
    /// <param name="libNewSize"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly]
    private static HRESULT SetSize(ulong libNewSize) => _sOk;

    /// <summary>
    /// Copys the contents of one IStream* to another.
    /// </summary>
    /// <param name="thiscom"></param>
    /// <param name="pstm"></param>
    /// <param name="cb"></param>
    /// <param name="pcbRead"></param>
    /// <param name="pcbWritten"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly]
    private static HRESULT CopyTo(void* thiscom, IStream* pstm, ulong cb, ulong* pcbRead, ulong* pcbWritten)
    {
        IStream.Interface istream = ComInterfaceDispatch.GetInstance<IStream.Interface>((ComInterfaceDispatch*)thiscom);
        return istream.CopyTo(pstm, cb, pcbRead, pcbWritten).ThrowOnFailure();
    }

    /// <summary>
    /// Commits changes to a transactional stream.
    /// </summary>
    /// <param name="thiscom"></param>
    /// <param name="grfCommitFlags"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly]
    private static HRESULT Commit(void* thiscom, uint grfCommitFlags)
    {
        IStream.Interface istream = ComInterfaceDispatch.GetInstance<IStream.Interface>((ComInterfaceDispatch*)thiscom);
        return istream.Commit(grfCommitFlags).ThrowOnFailure();
    }

    /// <summary>
    /// Reverts changes up to the last commmit for transactional streams.
    /// </summary>
    /// <param name="thiscom"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly]
    private static HRESULT Revert(void* thiscom)
    {
        IStream.Interface istream = ComInterfaceDispatch.GetInstance<IStream.Interface>((ComInterfaceDispatch*)thiscom);
        return istream.Revert().ThrowOnFailure();
    }

    /// <summary>
    /// Locks a region for transactional streams.
    /// </summary>
    /// <param name="thiscom"></param>
    /// <param name="libOffset"></param>
    /// <param name="cb"></param>
    /// <param name="dwLockType"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly]
    private static HRESULT LockRegion(void* thiscom, ulong libOffset, ulong cb, uint dwLockType)
    {
        IStream.Interface istream = ComInterfaceDispatch.GetInstance<IStream.Interface>((ComInterfaceDispatch*)thiscom);
        return istream.LockRegion(libOffset, cb, dwLockType).ThrowOnFailure();
    }

    /// <summary>
    /// Unlocks a locked region for transactional streams.
    /// </summary>
    /// <param name="thiscom"></param>
    /// <param name="libOffset"></param>
    /// <param name="cb"></param>
    /// <param name="dwLockType"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly]
    private static HRESULT UnlockRegion(void* thiscom, ulong libOffset, ulong cb, uint dwLockType)
    {
        IStream.Interface istream = ComInterfaceDispatch.GetInstance<IStream.Interface>((ComInterfaceDispatch*)thiscom);
        return istream.UnlockRegion(libOffset, cb, dwLockType).ThrowOnFailure();
    }

    /// <summary>
    /// Provides storage stats.
    /// </summary>
    /// <param name="thiscom"></param>
    /// <param name="pstatstg"></param>
    /// <param name="grfStatFlag"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly]
    private static HRESULT Stat(void* thiscom, STATSTG* pstatstg, uint grfStatFlag)
    {
        IStream.Interface istream = ComInterfaceDispatch.GetInstance<IStream.Interface>((ComInterfaceDispatch*)thiscom);
        return istream.Stat(pstatstg, grfStatFlag).ThrowOnFailure();
    }

    /// <summary>
    /// Clones the stream.
    /// </summary>
    /// <param name="thiscom"></param>
    /// <param name="ppstm"></param>
    /// <returns></returns>
    [UnmanagedCallersOnly]
    private static HRESULT Clone(void* thiscom, IStream** ppstm)
    {
        IStream.Interface istream = ComInterfaceDispatch.GetInstance<IStream.Interface>((ComInterfaceDispatch*)thiscom);
        return istream.Clone(ppstm).ThrowOnFailure();
    }

    /// <summary>
    /// Function to release managed and unmanaged resources.
    /// </summary>
    /// <param name="disposing"><b>true</b> to release managed and unmanaged resources, <b>false</b> to only release unmanaged resources.</param>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (OwnsBaseStream)
            {
                BaseStream.Close();
            }
        }

        ArrayPool<byte>.Shared.Return(_buffer, disposing);
    }

    /// <summary>
    /// Function to return the COM IStream interface from this wrapper.
    /// </summary>
    /// <param name="err">The HRESULT error codes from the operations.</param>
    /// <returns>A pointer to the IStream interface, or <b>null</b> if one could not be returned.</returns>
    private void* ToIStream(out HRESULT err)
    {
        IUnknown* pUnk = (IUnknown*)GetOrCreateComInterfaceForObject(this, CreateComInterfaceFlags.None);

        if (pUnk is null)
        {
            err = _eNotSupported;
            return null;
        }

        try
        {
            err = pUnk->QueryInterface(in _guid, out void* result).ThrowOnFailure();

            return result;
        }
        finally
        {
            pUnk->Release();
        }
    }

    /// <inheritdoc/>
    /// <remarks>
    /// <para>
    /// This type does not track or handle predefiend IUnknown interfaces, an exception will be thrown if the <paramref name="flags"/> is set to anything other than <c>None</c>.
    /// </para>
    /// </remarks>
    /// <exception cref="ArgumentException">Thrown when the <paramref name="flags"/> is not set to <c>None</c>.</exception>
    protected override ComInterfaceEntry* ComputeVtables(object obj, CreateComInterfaceFlags flags, out int count)
    {
        if (flags != CreateComInterfaceFlags.None)
        {
            throw new ArgumentException("Cannot create tracked or predefined IUnknown interfaces.", nameof(flags));
        }

        if (obj is GorgonComStreamWrapper)
        {
            count = 2;
            return _interfaceEntries;
        }

        count = 0;
        return null;
    }

    /// <summary>
    /// This method is not used. We do not create these objects from IStream types, just .NET types.
    /// </summary>
    /// <param name="externalComObject"></param>
    /// <param name="flags"></param>
    /// <returns></returns>
    /// <exception cref="NotSupportedException">This method is not used by the wrapper.</exception>
    protected override object? CreateObject(nint externalComObject, CreateObjectFlags flags) => throw new NotSupportedException();

    /// <summary>
    /// This method is not used. We do not track instances.
    /// </summary>
    /// <param name="objects"></param>
    /// <exception cref="NotSupportedException">The method is not used by the wrapper.</exception>
    protected override void ReleaseObjects(IEnumerable objects) => throw new NotSupportedException();

    /// <summary>
    /// Function to return the <c>IStream</c> COM interface as a <c>void*</c> pointer.
    /// </summary>
    /// <param name="wrapper">The wrapper to retrieve the interface from.</param>
    /// <returns>The pointer to the <c>IStream</c> COM interface, or <b>null</b> if it could not be retrieved.</returns>
    public static void* ToIStreamPointer(GorgonComStreamWrapper? wrapper)
    {
        if (wrapper is null)
        {
            return null;
        }

        void* result = wrapper.ToIStream(out HRESULT err);

        err.ThrowOnFailure();

        return result;
    }

    /// <summary>
    /// Function to return the <c>IStream</c> COM interface as a <see cref="nint"/>.
    /// </summary>
    /// <param name="wrapper">The wrapper to retrieve the interface from.</param>
    /// <returns>The <see cref="nint"/> to the <c>IStream</c> COM interface, or <see cref="IntPtr.Zero"/> if it could not be retrieved.</returns>
    public static nint ToIStreamIntPtr(GorgonComStreamWrapper? wrapper) => (nint)ToIStreamPointer(wrapper);

    /// <summary>
    /// Operator to explicitly convert this object to a <c>IStream*</c> pointer type as a <c>void*</c> pointer.
    /// </summary>
    /// <param name="wrapper"></param>
    public static explicit operator void*(GorgonComStreamWrapper? wrapper) => ToIStreamPointer(wrapper);

    /// <summary>
    /// Operator to explicitly convert this object to a <c>IStream*</c> pointer type as a <c>void*</c> pointer.
    /// </summary>
    /// <param name="wrapper"></param>
    public static explicit operator nint(GorgonComStreamWrapper? wrapper) => ToIStreamIntPtr(wrapper);

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// The finalizer for the wrapper.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This ensures our native data, and any pooled data is released.
    /// </para>
    /// </remarks>
    ~GorgonComStreamWrapper() => Dispose(false);

    /// <summary>
    /// Initializes the static class data.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The vtable is set up in here because it's for the type, not the instance of the type. 
    /// </para>
    /// <para>
    /// Everything in here is set up such that if the type were unloaded (because the assembly is unloaded), then the memory for the vtables and interfaces would be correctly reclaimed.
    /// </para>
    /// </remarks>
    static GorgonComStreamWrapper()
    {
        // Retrieve the 3 methods used for IUnknown.  We have no need to make custom versions of these methods, so just use what's supplied for us.
        GetIUnknownImpl(out nint queryInterface_0, out nint addref_1, out nint release_2);

        // Generate 14 method entries, 11 for the IStream type, and 3 for the IUnknown type.
        nint* vtable = (nint*)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(GorgonComStreamWrapper), IntPtr.Size * 14);

        int index = 0;
        // Assign the methods. We use static unmanaged methods to redirect our COM IStream calls.
        vtable[index++] = queryInterface_0;
        vtable[index++] = addref_1;
        vtable[index++] = release_2;
        vtable[index++] = (nint)(delegate* unmanaged<void*, void*, uint, uint*, HRESULT>)&GorgonComStreamWrapper.Read;
        vtable[index++] = (nint)(delegate* unmanaged<void*, void*, uint, uint*, HRESULT>)&GorgonComStreamWrapper.Write;
        vtable[index++] = (nint)(delegate* unmanaged<void*, long, SeekOrigin, ulong*, HRESULT>)&GorgonComStreamWrapper.Seek;
        vtable[index++] = (nint)(delegate* unmanaged<ulong, HRESULT>)&GorgonComStreamWrapper.SetSize;
        vtable[index++] = (nint)(delegate* unmanaged<void*, IStream*, ulong, ulong*, ulong*, HRESULT>)&GorgonComStreamWrapper.CopyTo;
        vtable[index++] = (nint)(delegate* unmanaged<void*, uint, HRESULT>)&GorgonComStreamWrapper.Commit;
        vtable[index++] = (nint)(delegate* unmanaged<void*, HRESULT>)&GorgonComStreamWrapper.Revert;
        vtable[index++] = (nint)(delegate* unmanaged<void*, ulong, ulong, uint, HRESULT>)&GorgonComStreamWrapper.LockRegion;
        vtable[index++] = (nint)(delegate* unmanaged<void*, ulong, ulong, uint, HRESULT>)&GorgonComStreamWrapper.UnlockRegion;
        vtable[index++] = (nint)(delegate* unmanaged<void*, STATSTG*, uint, HRESULT>)&GorgonComStreamWrapper.Stat;
        vtable[index++] = (nint)(delegate* unmanaged<void*, IStream**, HRESULT>)&GorgonComStreamWrapper.Clone;
        _iStreamVtbl = vtable;

        // Generate 5 method entries, 2 for the ISequentialStream type, and 3 for the base IUnknown type.
        vtable = (nint*)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(GorgonComStreamWrapper), IntPtr.Size * 5);

        index = 0;
        // Assign the methods. We use static unmanaged methods to redirect our COM ISequentialStream calls.
        vtable[index++] = queryInterface_0;
        vtable[index++] = addref_1;
        vtable[index++] = release_2;
        vtable[index++] = (nint)(delegate* unmanaged<void*, void*, uint, uint*, HRESULT>)&GorgonComStreamWrapper.Read;
        vtable[index++] = (nint)(delegate* unmanaged<void*, void*, uint, uint*, HRESULT>)&GorgonComStreamWrapper.Write;
        _iSequentialStreamVtbl = vtable;

        // Finally set up our interface table with the appropriate vtables.
        _interfaceEntries = (ComInterfaceEntry*)RuntimeHelpers.AllocateTypeAssociatedMemory(typeof(GorgonComStreamWrapper), sizeof(ComInterfaceEntry) * 2);
        _interfaceEntries[0].IID = typeof(IStream).GUID;
        _interfaceEntries[0].Vtable = (nint)_iStreamVtbl;
        _interfaceEntries[1].IID = typeof(ISequentialStream).GUID;
        _interfaceEntries[1].Vtable = (nint)_iSequentialStreamVtbl;
    }
}
