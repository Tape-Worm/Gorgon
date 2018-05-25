namespace Gorgon.Graphics.Core
{
    /// <summary>
    /// Specifies the type of primitive geometry to render from vertex data bound to the pipeline.
    /// </summary>
    public enum PrimitiveType
    {
        /// <summary>
        /// <para>
        /// The IA stage has not been initialized with a primitive topology. The IA stage will not function properly unless a primitive topology is defined.
        /// </para>
        /// </summary>
        None = SharpDX.Direct3D.PrimitiveTopology.Undefined,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a list of points.
        /// </para>
        /// </summary>
        PointList = SharpDX.Direct3D.PrimitiveTopology.PointList,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a list of lines.
        /// </para>
        /// </summary>
        LineList = SharpDX.Direct3D.PrimitiveTopology.LineList,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a line strip.
        /// </para>
        /// </summary>
        LineStrip = SharpDX.Direct3D.PrimitiveTopology.LineStrip,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a list of triangles.
        /// </para>
        /// </summary>
        TriangleList = SharpDX.Direct3D.PrimitiveTopology.TriangleList,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a triangle strip.
        /// </para>
        /// </summary>
        TriangleStrip = SharpDX.Direct3D.PrimitiveTopology.TriangleStrip,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as list of lines with adjacency data.
        /// </para>
        /// </summary>
        LineListWithAdjacency = SharpDX.Direct3D.PrimitiveTopology.LineListWithAdjacency,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as line strip with adjacency data.
        /// </para>
        /// </summary>
        LineStripWithAdjacency = SharpDX.Direct3D.PrimitiveTopology.LineStripWithAdjacency,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as list of triangles with adjacency data.
        /// </para>
        /// </summary>
        TriangleListWithAdjacency = SharpDX.Direct3D.PrimitiveTopology.TriangleListWithAdjacency,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as triangle strip with adjacency data.
        /// </para>
        /// </summary>
        TriangleStripWithAdjacency = SharpDX.Direct3D.PrimitiveTopology.TriangleStripWithAdjacency,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith1ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith1ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith2ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith2ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith3ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith3ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith4ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith4ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith5ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith5ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith6ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith6ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith7ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith7ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith8ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith8ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith9ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith9ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith10ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith10ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith11ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith11ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith12ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith12ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith13ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith13ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith14ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith14ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith15ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith15ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith16ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith16ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith17ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith17ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith18ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith18ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith19ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith19ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith20ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith20ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith21ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith21ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith22ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith22ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith23ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith23ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith24ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith24ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith25ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith25ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith26ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith26ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith27ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith27ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith28ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith28ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith29ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith29ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith30ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith30ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith31ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith31ControlPoints,
        /// <summary>
        /// <para>
        /// Interpret the vertex data as a patch list.
        /// </para>
        /// </summary>
        PatchListWith32ControlPoints = SharpDX.Direct3D.PrimitiveTopology.PatchListWith32ControlPoints
    }
}