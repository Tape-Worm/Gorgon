using System;

namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Defines the type of logical operations to perform while blending a render target.
    /// </summary>
    [Flags]
    public enum WriteMask
    {
        /// <summary>The red channel will be written.</summary>
        Red = SharpDX.Direct3D11.ColorWriteMaskFlags.Red,
        /// <summary>The green channel will be written.</summary>
        Green = SharpDX.Direct3D11.ColorWriteMaskFlags.Green,
        /// <summary>The blue channel will be written.</summary>
        Blue = SharpDX.Direct3D11.ColorWriteMaskFlags.Blue,
        /// <summary>The alpha channel will be written.</summary>
        Alpha = SharpDX.Direct3D11.ColorWriteMaskFlags.Alpha,
        /// <summary>All channels will be written.</summary>
        All = SharpDX.Direct3D11.ColorWriteMaskFlags.All
    }
}