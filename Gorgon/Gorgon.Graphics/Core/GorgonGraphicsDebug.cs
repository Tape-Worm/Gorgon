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
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.
// 
// Created: July 10, 2025 4:31:42 PM
//

using Gorgon.Core;
using Gorgon.Diagnostics;
using TerraFX.Interop.DirectX;
using TerraFX.Interop.Windows;
using Win32 = TerraFX.Interop.Windows.Windows;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Provides access to debugging information from the underlying graphics APIs (DXGI and Direct3D 12).
/// </summary>
/// <remarks>
/// <para>
/// This object is returned by the <see cref="GorgonGraphicsFactory.Debugging"/> property when debugging is turned on for the factory (see <see cref="GorgonGraphicsFactory.DebuggingFlags"/>). It reports 
/// the objects from the underlying APIs that are still alive, the memory allocation statistics for a <see cref="GorgonGraphics"/> interface, and the Device Removed Extended Data (DRED) after the device has 
/// hung.
/// </para>
/// </remarks>
/// <seealso cref="GorgonGraphicsFactory"/>
public unsafe sealed class GorgonGraphicsDebug
{
    private readonly IGorgonLog _log = GorgonLog.NullLog;
    private ComPtr<IDXGIDebug> _dxgiDebug;
    private ComPtr<ID3D12DeviceRemovedExtendedDataSettings2> _dred;

    /// <summary>
    /// Function to report the objects from the underlying APIs that are still alive.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The report is written to the debug output by the underlying APIs. If the DXGI debug interface could not be retrieved when the factory was created, this method does nothing.
    /// </para>
    /// </remarks>
    public void Report()
    {
        if (_dxgiDebug.IsNull)
        {
            return;
        }

        _dxgiDebug.Get()->ReportLiveObjects(DXGI.DXGI_DEBUG_ALL, DXGI_DEBUG_RLO_FLAGS.DXGI_DEBUG_RLO_DETAIL | DXGI_DEBUG_RLO_FLAGS.DXGI_DEBUG_RLO_IGNORE_INTERNAL);
    }

#pragma warning disable CA1822 // Mark members as static
    /// <summary>
    /// Function to report the memory allocation statistics for a <see cref="GorgonGraphics"/> interface.
    /// </summary>
    /// <param name="graphics">The graphics interface to retrieve the statistics from.</param>
    /// <remarks>
    /// <para>
    /// The statistics are written to the debug output.
    /// </para>
    /// </remarks>
    public void GetMemoryStatistics(GorgonGraphics graphics) => graphics?.ReportMemory();
#pragma warning restore CA1822 // Mark members as static

    /// <summary>
    /// Function to write the Device Removed Extended Data (DRED) to the log after the device has hung.
    /// </summary>
    /// <param name="graphics">The graphics interface that owns the device to retrieve the data from.</param>
    /// <remarks>
    /// <para>
    /// The DRED data contains the operations recorded by each command list and command queue up to the hang (the breadcrumbs), and the memory allocations around the page fault, if there was one. Use this 
    /// data to locate the command that caused the device to hang.
    /// </para>
    /// <para>
    /// If the device has not hung, a warning is written to the log and no data is written.
    /// </para>
    /// </remarks>
    public void GetDredData(GorgonGraphics graphics)
    {
        if (graphics.D3DDevice.Get()->GetDeviceRemovedReason() != DXGI.DXGI_ERROR_DEVICE_HUNG)
        {
            _log.PrintWarning("Device is not in a hung state. DRED data cannot be evaluated.", LoggingLevel.Verbose);
            return;
        }

        using ComPtr<ID3D12DeviceRemovedExtendedData2> dredPtr = default;

        HRESULT err = graphics.D3DDevice.Get()->QueryInterface(Win32.__uuidof<ID3D12DeviceRemovedExtendedData2>(), (void**)dredPtr.GetAddressOf());

        if (err.FAILED)
        {
            _log.PrintError(err, "There was an error returning the DRED data from the device. Could not find the interface.", LoggingLevel.Verbose);
            return;
        }

        D3D12_DRED_AUTO_BREADCRUMBS_OUTPUT breadCrumbsOut = default;
        D3D12_DRED_PAGE_FAULT_OUTPUT2 pageFaultOut = default;

        err = dredPtr.Get()->GetAutoBreadcrumbsOutput(&breadCrumbsOut);

        if (err.FAILED)
        {
            _log.PrintError(err, "There was an error returning the DRED breadcrumb data from the device.", LoggingLevel.Verbose);
            return;
        }

        err = dredPtr.Get()->GetPageFaultAllocationOutput2(&pageFaultOut);
        if (err.FAILED)
        {
            _log.PrintError(err, "There was an error returning the DRED page fault allocation data from the device.", LoggingLevel.Verbose);
            return;
        }

        D3D12_AUTO_BREADCRUMB_NODE* breadNode = breadCrumbsOut.pHeadAutoBreadcrumbNode;

        _log.PrintError("DRED breadcrumb information on device hang is as follows:", LoggingLevel.All);

        while (breadNode is not null)
        {
            string commandListName = breadNode->pCommandListDebugNameW is null ? "No name" : new string(breadNode->pCommandListDebugNameW);
            string commandQueueName = breadNode->pCommandQueueDebugNameW is null ? "No name" : new string(breadNode->pCommandQueueDebugNameW);

            _log.Print($"Command list: {commandListName}", LoggingLevel.All);
            _log.Print($"Command queue: {commandQueueName}", LoggingLevel.All);

            for (int i = 0; i < breadNode->BreadcrumbCount; ++i)
            {
                _log.Print($"Operation #{i}: {breadNode->pCommandHistory[i]}", LoggingLevel.All);
            }

            breadNode = breadNode->pNext;
        }

        _log.PrintError("Page fault information on device hang is as follows:", LoggingLevel.All);

        _log.Print($"VA: {pageFaultOut.PageFaultVA}", LoggingLevel.All);
        _log.Print($"Flags: {pageFaultOut.PageFaultFlags}", LoggingLevel.All);

        D3D12_DRED_ALLOCATION_NODE1* allocNode = pageFaultOut.pHeadExistingAllocationNode;
        D3D12_DRED_ALLOCATION_NODE1* freeNode = pageFaultOut.pHeadRecentFreedAllocationNode;

        if (allocNode is not null)
        {
            _log.Print("Existing allocation nodes:", LoggingLevel.All);

            while (allocNode is not null)
            {
                _log.Print($"Object: {(allocNode->ObjectNameW is null ? "No name" : new string(allocNode->ObjectNameW))}", LoggingLevel.All);
                _log.Print($"Type: {allocNode->AllocationType}", LoggingLevel.All);
                _log.Print($"Object address: 0x{((long)allocNode->pObject).FormatHex()}", LoggingLevel.All);

                allocNode = allocNode->pNext;
            }
        }

        if (freeNode is null)
        {
            return;
        }

        _log.Print("Recently freed allocation nodes:", LoggingLevel.All);

        while (freeNode is not null)
        {
            _log.Print($"Object: {(freeNode->ObjectNameW is null ? "No name" : new string(freeNode->ObjectNameW))}", LoggingLevel.All);
            _log.Print($"Type: {freeNode->AllocationType}", LoggingLevel.All);
            _log.Print($"bject address: 0x{((long)freeNode->pObject).FormatHex()}", LoggingLevel.All);

            freeNode = freeNode->pNext;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GorgonGraphicsDebug"/> class.
    /// </summary>
    /// <param name="log">The logging interface for debug messages.</param>
    /// <param name="dxgiDebug">The DXGI debug interface.</param>
    /// <param name="dred">The DRED interface.</param>
    internal GorgonGraphicsDebug(IGorgonLog log, ComPtr<IDXGIDebug> dxgiDebug, ComPtr<ID3D12DeviceRemovedExtendedDataSettings2> dred)
    {
        _log = log;
        _dxgiDebug = dxgiDebug;
        _dred = dred;
    }
}
