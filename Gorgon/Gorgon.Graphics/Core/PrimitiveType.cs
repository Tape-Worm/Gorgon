// Gorgon.
// Copyright (C) 2026 Michael Winsor
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
// Created: March 19, 2026 2:18:33 PM
//

using TerraFX.Interop.DirectX;

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
    None = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_UNDEFINED,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a list of points.
    /// </para>
    /// </summary>
    PointList = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_POINTLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a list of lines.
    /// </para>
    /// </summary>
    LineList = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_LINELIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a line strip.
    /// </para>
    /// </summary>
    LineStrip = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_LINESTRIP,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a list of triangles.
    /// </para>
    /// </summary>
    TriangleList = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a triangle strip.
    /// </para>
    /// </summary>
    TriangleStrip = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as list of lines with adjacency data.
    /// </para>
    /// </summary>
    LineListWithAdjacency = D3D_PRIMITIVE_TOPOLOGY.D3D10_PRIMITIVE_TOPOLOGY_LINELIST_ADJ,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as line strip with adjacency data.
    /// </para>
    /// </summary>
    LineStripWithAdjacency = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_LINESTRIP_ADJ,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as list of triangles with adjacency data.
    /// </para>
    /// </summary>
    TriangleListWithAdjacency = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_TRIANGLELIST_ADJ,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as triangle strip with adjacency data.
    /// </para>
    /// </summary>
    TriangleStripWithAdjacency = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_TRIANGLESTRIP_ADJ,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith1ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_1_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith2ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_2_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith3ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_3_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith4ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_4_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith5ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_5_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith6ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_6_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith7ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_7_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith8ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_8_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith9ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_9_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith10ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_10_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith11ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_11_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith12ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_12_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith13ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_13_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith14ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_14_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith15ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_15_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith16ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_16_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith17ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_17_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith18ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_18_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith19ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_19_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith20ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_20_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith21ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_21_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith22ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_22_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith23ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_23_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith24ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_24_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith25ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_25_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith26ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_26_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith27ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_27_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith28ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_28_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith29ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_29_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith30ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_30_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith31ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_31_CONTROL_POINT_PATCHLIST,
    /// <summary>
    /// <para>
    /// Interpret the vertex data as a patch list.
    /// </para>
    /// </summary>
    PatchListWith32ControlPoints = D3D_PRIMITIVE_TOPOLOGY.D3D_PRIMITIVE_TOPOLOGY_32_CONTROL_POINT_PATCHLIST
}
