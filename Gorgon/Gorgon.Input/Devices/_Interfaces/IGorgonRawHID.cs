using Gorgon.Native;

namespace Gorgon.Input;

/// <summary>
/// Provides state for human interface data returned from Raw Input
/// </summary>
/// <remarks>
/// <para>
/// This allows a user to read, and parse human interface device data from an aribtrary device. It is recommended that this object be wrapped by an actual object that will be used to present the data 
/// in an easy to manipulate format
/// </para>
/// <para>
/// This object implements <see cref="IDisposable"/> because it manipulates native memory. It is necessary to call the <see cref="IDisposable.Dispose"/> method in order to ensure there is no memory leak 
/// when finished with this object
/// </para>
/// </remarks>
public interface IGorgonRawHID
    : IGorgonRawInputDevice, IGorgonRawInputDeviceData<GorgonRawHIDData>, IDisposable
{
    /// <summary>
    /// Event triggered when Raw Input receives data from the device.
    /// </summary>
    event EventHandler<GorgonHIDEventArgs> DataReceived;

    /// <summary>
    /// Property to return information about the Raw Input Human Interface Device.
    /// </summary>
    IGorgonRawHIDInfo Info
    {
        get;
    }

    /// <summary>
    /// Property to return a pointer to the block of memory that stores the HID data.
    /// </summary>
    GorgonPtr<byte> Data
    {
        get;
    }

    /// <summary>
    /// Property to return the size of an individual HID input, in bytes.
    /// </summary>
    int HIDSize
    {
        get;
    }

    /// <summary>
    /// Property to return the number of inputs in the <see cref="Data"/>.
    /// </summary>
    int Count
    {
        get;
    }

    /// <summary>
    /// Property to return the pre-parsed data for this HID.
    /// </summary>
    GorgonPtr<byte> PreParsedData
    {
        get;
    }
}
