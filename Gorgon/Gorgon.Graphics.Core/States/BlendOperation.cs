namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Defines the type of operation to perform while blending colors.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1008:EnumsShouldHaveZeroValue")]
    public enum BlendOperation
    {
        /// <summary>
        /// <para>
        /// Add source 1 and source 2.
        /// </para>
        /// </summary>
        Add = SharpDX.Direct3D11.BlendOperation.Add,
        /// <summary>
        /// <para>
        /// Subtract source 1 from source 2.
        /// </para>
        /// </summary>
        Subtract = SharpDX.Direct3D11.BlendOperation.Subtract,
        /// <summary>
        /// <para>
        /// Subtract source 2 from source 1.
        /// </para>
        /// </summary>
        ReverseSubtract = SharpDX.Direct3D11.BlendOperation.ReverseSubtract,
        /// <summary>
        /// <para>
        /// Find the minimum of source 1 and source 2.
        /// </para>
        /// </summary>
        Minimum = SharpDX.Direct3D11.BlendOperation.Minimum,
        /// <summary>
        /// <para>
        /// Find the maximum of source 1 and source 2.
        /// </para>
        /// </summary>
        Maximum = SharpDX.Direct3D11.BlendOperation.Maximum
    }
}