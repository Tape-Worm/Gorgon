using SharpDX.Direct3D;

namespace Gorgon.Graphics.Core;

/// <summary>
/// Specifies the type of primitive geometry to render from vertex data bound to the pipeline
/// </summary>
public enum PrimitiveType
{
    /// <summary>
    /// <para>
    /// The IA stage has not been initialized with a primitive topology. The IA stage will not function properly unless a primitive topology is defined.
    /// </para>
    /// </summary>
    None = PrimitiveTopology.Undefined,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a list of points.
    /// </para>
    /// </summary>
    PointList = PrimitiveTopology.PointList,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a list of lines.
    /// </para>
    /// </summary>
    LineList = PrimitiveTopology.LineList,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a line strip.
    /// </para>
    /// </summary>
    LineStrip = PrimitiveTopology.LineStrip,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a list of triangles.
    /// </para>
    /// </summary>
    TriangleList = PrimitiveTopology.TriangleList,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a triangle strip.
    /// </para>
    /// </summary>
    TriangleStrip = PrimitiveTopology.TriangleStrip,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as list of lines with adjacency data.
    /// </para>
    /// </summary>
    LineListWithAdjacency = PrimitiveTopology.LineListWithAdjacency,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as line strip with adjacency data.
    /// </para>
    /// </summary>
    LineStripWithAdjacency = PrimitiveTopology.LineStripWithAdjacency,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as list of triangles with adjacency data.
    /// </para>
    /// </summary>
    TriangleListWithAdjacency = PrimitiveTopology.TriangleListWithAdjacency,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as triangle strip with adjacency data.
    /// </para>
    /// </summary>
    TriangleStripWithAdjacency = PrimitiveTopology.TriangleStripWithAdjacency,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith1ControlPoints = PrimitiveTopology.PatchListWith1ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith2ControlPoints = PrimitiveTopology.PatchListWith2ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith3ControlPoints = PrimitiveTopology.PatchListWith3ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith4ControlPoints = PrimitiveTopology.PatchListWith4ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith5ControlPoints = PrimitiveTopology.PatchListWith5ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith6ControlPoints = PrimitiveTopology.PatchListWith6ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith7ControlPoints = PrimitiveTopology.PatchListWith7ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith8ControlPoints = PrimitiveTopology.PatchListWith8ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith9ControlPoints = PrimitiveTopology.PatchListWith9ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith10ControlPoints = PrimitiveTopology.PatchListWith10ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith11ControlPoints = PrimitiveTopology.PatchListWith11ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith12ControlPoints = PrimitiveTopology.PatchListWith12ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith13ControlPoints = PrimitiveTopology.PatchListWith13ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith14ControlPoints = PrimitiveTopology.PatchListWith14ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith15ControlPoints = PrimitiveTopology.PatchListWith15ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith16ControlPoints = PrimitiveTopology.PatchListWith16ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith17ControlPoints = PrimitiveTopology.PatchListWith17ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith18ControlPoints = PrimitiveTopology.PatchListWith18ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith19ControlPoints = PrimitiveTopology.PatchListWith19ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith20ControlPoints = PrimitiveTopology.PatchListWith20ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith21ControlPoints = PrimitiveTopology.PatchListWith21ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith22ControlPoints = PrimitiveTopology.PatchListWith22ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith23ControlPoints = PrimitiveTopology.PatchListWith23ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith24ControlPoints = PrimitiveTopology.PatchListWith24ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith25ControlPoints = PrimitiveTopology.PatchListWith25ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith26ControlPoints = PrimitiveTopology.PatchListWith26ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith27ControlPoints = PrimitiveTopology.PatchListWith27ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith28ControlPoints = PrimitiveTopology.PatchListWith28ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith29ControlPoints = PrimitiveTopology.PatchListWith29ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith30ControlPoints = PrimitiveTopology.PatchListWith30ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith31ControlPoints = PrimitiveTopology.PatchListWith31ControlPoints,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith32ControlPoints = PrimitiveTopology.PatchListWith32ControlPoints
}
