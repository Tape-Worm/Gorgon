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
// Created: January 19, 2025 11:55:54 PM
//

using System.Buffers;
using System.Runtime.CompilerServices;
using Gorgon.Math;
using Gorgon.Memory;
using Gorgon.Input.Properties;
using Windows.Win32;

namespace Gorgon.Input.Devices;

/// <inheritdoc cref="IGorgonKeyboard"/>
/// <param name="keyboardDevice">[Optional] The specific keyboard to retrieve data for.</param>
/// <remarks>
/// <para>
/// If the <paramref name="keyboardDevice"/> parameter is not specified, then this keyboard instance will accept input for any keyboard. Otherwise, the data passed to the keyboard from the input system will 
/// be filtered to the device specified.
/// </para>
/// <para>
/// If the <paramref name="keyboardDevice"/> is not <b>null</b>, then this object will have to be recreated if the <see cref="IGorgonInput.Enable"/> method is called.
/// </para>
/// </remarks>
public class GorgonKeyboard(IGorgonKeyboardInfo? keyboardDevice = null)
    : IGorgonKeyboard
{
    // The system keyboard information.
    private static readonly IGorgonKeyboardInfo _systemKeyboard;
    // The current state of the keyboard.
    private readonly HashSet<VirtualKeys> _keyboardStates = [];
    // The list of pressed keys.
    private readonly VirtualKeys[] _pressedKeys = new VirtualKeys[16];
    // The list of pressed keys.
    private readonly VirtualKeys[] _pressedModifierKeys = new VirtualKeys[8];

    /// <inheritdoc/>
    public int PressedKeyCount => _keyboardStates.Count;

    /// <inheritdoc/>
    public IGorgonKeyboardInfo Info
    {
        get;
    } = keyboardDevice ?? _systemKeyboard;

    /// <inheritdoc/>
    public bool this[VirtualKeys key] => _keyboardStates.Contains(key);

    /// <inheritdoc/>
    public ReadOnlySpan<VirtualKeys> GetPressedKeys()
    {
        int count = _pressedKeys.Length.Min(_keyboardStates.Count);
        int i = 0;

        foreach (VirtualKeys key in _keyboardStates)
        {
            if (key is VirtualKeys.AltModifier or VirtualKeys.ControlModifier or VirtualKeys.ShiftModifier)
            {
                continue;
            }

            _pressedKeys[i++] = key;

            if (i >= count)
            {
                break;
            }
        }

        return i == 0 ? ReadOnlySpan<VirtualKeys>.Empty : _pressedKeys.AsSpan(0, i);
    }

    /// <inheritdoc/>
    public ReadOnlySpan<VirtualKeys> GetModifiers()
    {
        int count = _pressedModifierKeys.Length.Min(_keyboardStates.Count);
        int i = 0;

        foreach (VirtualKeys key in _keyboardStates)
        {
            if (key is not VirtualKeys.AltModifier and not VirtualKeys.ControlModifier and not VirtualKeys.ShiftModifier
                and not VirtualKeys.LeftShift and not VirtualKeys.RightShift and not VirtualKeys.LeftAlt and not VirtualKeys.RightAlt
                and not VirtualKeys.LeftControl and not VirtualKeys.RightControl)
            {
                continue;
            }

            _pressedModifierKeys[i++] = key;

            if (i >= count)
            {
                break;
            }
        }

        return i == 0 ? ReadOnlySpan<VirtualKeys>.Empty : _pressedModifierKeys.AsSpan(0, i);
    }

    /// <inheritdoc/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Reset() => _keyboardStates.Clear();

    /// <inheritdoc/>
    public bool ParseData(GorgonInputEvent inputEvent)
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void UpdateKeyList(VirtualKeys key, KeyboardDataFlags flags)
        {
            if (key == VirtualKeys.None)
            {
                return;
            }

            if ((flags & KeyboardDataFlags.KeyDown) == KeyboardDataFlags.KeyDown)
            {
                _keyboardStates.Add(key);
            }
            else if ((flags & KeyboardDataFlags.KeyUp) == KeyboardDataFlags.KeyUp)
            {
                _keyboardStates.Remove(key);
            }
        }

        if ((_systemKeyboard != Info) && (Info.Handle != inputEvent.DeviceHandle))
        {
            return false;
        }

        if (inputEvent.DeviceType != InputDeviceType.Keyboard)
        {
            Reset();
            return false;
        }

        ref readonly RawKeyboardData keyboardData = ref inputEvent.RawInputKeyboardData;

        VirtualKeys specificKey = VirtualKeys.None;
        VirtualKeys modifierKey = VirtualKeys.None;

        switch (keyboardData.Key)
        {
            case VirtualKeys.Control:
                specificKey = ((keyboardData.Flags & KeyboardDataFlags.LeftKey) == KeyboardDataFlags.LeftKey) ? VirtualKeys.LeftControl : VirtualKeys.RightControl;
                modifierKey = VirtualKeys.ControlModifier;
                break;
            case VirtualKeys.Alt:
                specificKey = ((keyboardData.Flags & KeyboardDataFlags.LeftKey) == KeyboardDataFlags.LeftKey) ? VirtualKeys.LeftAlt : VirtualKeys.RightAlt;
                modifierKey = VirtualKeys.AltModifier;
                break;
            case VirtualKeys.Shift:
                specificKey = ((keyboardData.Flags & KeyboardDataFlags.LeftKey) == KeyboardDataFlags.LeftKey) ? VirtualKeys.LeftShift : VirtualKeys.RightShift;
                modifierKey = VirtualKeys.ShiftModifier;
                break;
        }

        UpdateKeyList(specificKey, keyboardData.Flags);
        UpdateKeyList(modifierKey, keyboardData.Flags);
        UpdateKeyList(keyboardData.Key, keyboardData.Flags);

        return true;
    }

    /// <inheritdoc/>
    public bool ParseData(GorgonInputEventBuffer inputEvents)
    {
        if (inputEvents.KeyboardEventCount == 0)
        {
            return false;
        }

        for (int i = 0; i < inputEvents.KeyboardEventCount; ++i)
        {
            ParseData(inputEvents.GetKeyboardEvent(i));
        }

        return true;
    }

    /// <inheritdoc/>
    public string KeyToCharacter(VirtualKeys key, VirtualKeys modifier)
    {
        const int shiftKey = (int)VirtualKeys.Shift;
        const int ctrlKey = (int)VirtualKeys.Control;
        const int altKey = (int)VirtualKeys.Alt;

        // Shift key modifier
        ArrayPool<byte> bytePool = GorgonArrayPools<byte>.GetBestPool(256);
        Span<char> characterBuffer = stackalloc char[1];
        byte[] charStates = bytePool.Rent(256);

        try
        {
            if (((modifier & VirtualKeys.ShiftModifier) == VirtualKeys.ShiftModifier)
                || ((modifier & VirtualKeys.Shift) == VirtualKeys.Shift)
                || ((modifier & VirtualKeys.LeftShift) == VirtualKeys.LeftShift)
                || ((modifier & VirtualKeys.RightShift) == VirtualKeys.RightShift))
            {
                charStates[shiftKey] = 0xff;
            }
            else
            {
                charStates[shiftKey] = 0;
            }

            if (((modifier & VirtualKeys.ControlModifier) == VirtualKeys.ControlModifier)
                || ((modifier & VirtualKeys.Control) == VirtualKeys.Control)
                || ((modifier & VirtualKeys.LeftControl) == VirtualKeys.LeftControl)
                || ((modifier & VirtualKeys.RightControl) == VirtualKeys.RightControl))
            {
                charStates[ctrlKey] = 0xff;
            }
            else
            {
                charStates[ctrlKey] = 0;
            }

            if (((modifier & VirtualKeys.AltModifier) == VirtualKeys.AltModifier)
                || ((modifier & VirtualKeys.Alt) == VirtualKeys.Alt)
                || ((modifier & VirtualKeys.LeftAlt) == VirtualKeys.LeftAlt)
                || ((modifier & VirtualKeys.RightAlt) == VirtualKeys.RightAlt))
            {
                charStates[altKey] = 0xff;
            }
            else
            {
                charStates[altKey] = 0;
            }

            return PInvoke.ToUnicodeChar(key, charStates, characterBuffer);
        }
        finally
        {
            bytePool.Return(charStates, true);
        }
    }

    /// <summary>
    /// Initializes the <see cref="GorgonKeyboard"/> class.
    /// </summary>
    static GorgonKeyboard() =>
        _systemKeyboard = new KeyboardInfo(0, "NULL", "NULL", Resources.GORINP_DESC_SYSTEM_KEYBOARD, PInvoke.GetKeyboardType(2), (KeyboardType)PInvoke.GetKeyboardType(0));
}
