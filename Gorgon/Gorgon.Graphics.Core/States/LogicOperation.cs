namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Defines the type of logical operations to perform while blending a render target.
    /// </summary>
    public enum LogicOperation
    {
        /// <summary>
        /// <para>
        /// Clears the render target.
        /// </para>
        /// </summary>
        Clear = SharpDX.Direct3D11.LogicOperation.Clear,
        /// <summary>
        /// <para>
        /// Sets the render target.
        /// </para>
        /// </summary>
        Set = SharpDX.Direct3D11.LogicOperation.Set,
        /// <summary>
        /// <para>
        /// Copys the render target.
        /// </para>
        /// </summary>
        Copy = SharpDX.Direct3D11.LogicOperation.Copy,
        /// <summary>
        /// <para>
        /// Performs an inverted-copy of the render target.
        /// </para>
        /// </summary>
        CopyInverted = SharpDX.Direct3D11.LogicOperation.CopyInverted,
        /// <summary>
        /// <para>
        /// No operation is performed on the render target.
        /// </para>
        /// </summary>
        Noop = SharpDX.Direct3D11.LogicOperation.Noop,
        /// <summary>
        /// <para>
        /// Inverts the render target.
        /// </para>
        /// </summary>
        Invert = SharpDX.Direct3D11.LogicOperation.Invert,
        /// <summary>
        /// <para>
        /// Performs a logical AND operation on the render target.
        /// </para>
        /// </summary>
        And = SharpDX.Direct3D11.LogicOperation.And,
        /// <summary>
        /// <para>
        /// Performs a logical NAND operation on the render target.
        /// </para>
        /// </summary>
        Nand = SharpDX.Direct3D11.LogicOperation.Nand,
        /// <summary>
        /// <para>
        /// Performs a logical OR operation on the render target.
        /// </para>
        /// </summary>
        Or = SharpDX.Direct3D11.LogicOperation.Or,
        /// <summary>
        /// <para>
        /// Performs a logical NOR operation on the render target.
        /// </para>
        /// </summary>
        Nor = SharpDX.Direct3D11.LogicOperation.Nor,
        /// <summary>
        /// <para>
        /// Performs a logical XOR operation on the render target.
        /// </para>
        /// </summary>
        Xor = SharpDX.Direct3D11.LogicOperation.Xor,
        /// <summary>
        /// <para>
        /// Performs a logical equal operation on the render target.
        /// </para>
        /// </summary>
        Equiv = SharpDX.Direct3D11.LogicOperation.Equiv,
        /// <summary>
        /// <para>
        /// Performs a logical AND and reverse operation on the render target.
        /// </para>
        /// </summary>
        AndReverse = SharpDX.Direct3D11.LogicOperation.AndReverse,
        /// <summary>
        /// <para>
        /// Performs a logical AND and invert operation on the render target.
        /// </para>
        /// </summary>
        AndInverted = SharpDX.Direct3D11.LogicOperation.AndInverted,
        /// <summary>
        /// <para>
        /// Performs a logical OR and reverse operation on the render target.
        /// </para>
        /// </summary>
        OrReverse = SharpDX.Direct3D11.LogicOperation.OrReverse,
        /// <summary>
        /// <para>
        /// Performs a logical OR and invert operation on the render target.
        /// </para>
        /// </summary>
        OrInverted = SharpDX.Direct3D11.LogicOperation.OrInverted
    }
}