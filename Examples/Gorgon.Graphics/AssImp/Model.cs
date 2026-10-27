using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using Assimp;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;

namespace Gorgon.Examples;

/// <summary>
/// Model data to render
/// </summary>
internal class Model
    : IDisposable
{
    // The world matrix for this model.
    private System.Numerics.Matrix4x4 _worldMatrix;

    /// <summary>
    /// Property to return the vertex buffer for the model.
    /// </summary>
    public GorgonStructuredBufferView VertexData
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the index buffer for the model.
    /// </summary>
    public GorgonIndexBuffer IndexData
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the minimum horizontal vertex value.
    /// </summary>
    public float MinX
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the maximum horizontal vertex value.
    /// </summary>
    public float MaxX
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the minimum vertical vertex value.
    /// </summary>
    public float MinY
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the maximum vertical vertex value.
    /// </summary>
    public float MaxY
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the minimum depth vertex value.
    /// </summary>
    public float MinZ
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to return the maximum depth vertex value.
    /// </summary>
    public float MaxZ
    {
        get;
        private set;
    }

    /// <summary>
    /// Property to set or return the angle of rotation, in degrees, around the Y axis.
    /// </summary>
    public float RotateY
    {
        get;
        set;
    }

    /// <summary>
    /// Property to set or return the angle of rotation, in degrees, around the X axis.
    /// </summary>
    public float RotateX
    {
        get;
        set;
    }

    /// <summary>
    /// Property to return the list of meshes to render.
    /// </summary>
    public IReadOnlyDictionary<Material, List<(int BaseStart, int Start, int Count)>> Meshes
    {
        get;
    }

    /// <summary>
    /// Property to return the list of textures used by the model.
    /// </summary>
    public IReadOnlyDictionary<string, IGorgonTextureView<GorgonTexture>> Textures
    {
        get;        
    }

    /// <summary>
    /// Function to retrieve the list of materials for the model.
    /// </summary>
    /// <param name="graphics">The current graphics interface.</param>
    /// <param name="sceneMaterials">The asset importer material list.</param>
    /// <param name="textures">The global texture list.</param>
    /// <returns>A list of model materials associated with their textures.</returns>
    private static IReadOnlyList<Material> GetMaterials(GorgonGraphics graphics, List<Assimp.Material> sceneMaterials, IReadOnlyDictionary<string, IGorgonTextureView<GorgonTexture>> textures)
    {
        List<Material> materials = [];

        for (int m = 0; m < sceneMaterials.Count; ++m)
        {
            Assimp.Material mat = sceneMaterials[m];

            string id = string.Empty;

            if (!string.IsNullOrWhiteSpace(mat.TextureDiffuse.FilePath))
            {
                id = Path.GetFileName(mat.TextureDiffuse.FilePath);
            }
            else if (!string.IsNullOrWhiteSpace(mat.TextureEmissive.FilePath))
            {
                id = Path.GetFileName(mat.TextureEmissive.FilePath);
            }

            textures.TryGetValue(id, out IGorgonTextureView<GorgonTexture>? texture);

            Material material = new(new GorgonColor(mat.ColorDiffuse.R, mat.ColorDiffuse.G, mat.ColorDiffuse.B, mat.ColorDiffuse.A),
                new GorgonColor(mat.ColorEmissive.R, mat.ColorEmissive.G, mat.ColorEmissive.B, 1.0f),
                new GorgonColor(mat.ColorSpecular.R, mat.ColorSpecular.G, mat.ColorSpecular.B, 1.0f),
                mat.Shininess,
                texture,
                GorgonSampler.LinearWrapping(graphics));
            materials.Add(material);
        }

        return materials;
    }

    /// <summary>
    /// Function to import data from the asset importer.
    /// </summary>
    /// <param name="graphics">The graphics interface.</param>
    /// <param name="scene">The scene containing the mesh data.</param>
    /// <param name="materials">The list of materials for the model.</param>
    /// <returns>The vertex, index buffer, aabb, and the mesh ranges.</returns>
    private static (GorgonStructuredBufferView VertexBuffer, GorgonIndexBuffer IndexBuffer, GorgonBoundingBox Aabb, IReadOnlyDictionary<Material, List<(int BaseStart, int Start, int Count)>> MeshData) ImportData(GorgonGraphics graphics, Scene scene, IReadOnlyList<Material> materials)
    {
        int vertexCount = scene.Meshes.Sum(item => item.VertexCount);
        int indexCount = scene.Meshes.SelectMany(item => item.Faces).Sum(item => item.IndexCount);

        GorgonStructuredBufferView vertexData = GorgonStructuredBufferView.CreateStructuredBuffer<GorgonVertexPosNormColorUv>(graphics, "Model VB", vertexCount);
        GorgonIndexBuffer indexData = new(graphics, "Model IB", new GorgonIndexBufferInfo(indexCount * sizeof(int), true));
        Dictionary<Material, List<(int BaseStart, int Start, int Count)>> meshData = [];

        int indexStart = 0;
        int vertexIndex = 0;

        GorgonVertexPosNormColorUv[] vertices = new GorgonVertexPosNormColorUv[vertexCount];
        int[] indices = new int[indexCount];

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float minZ = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;
        float maxZ = float.MinValue;

        for (int i1 = 0; i1 < scene.Meshes.Count; i1++)
        {
            Mesh mesh = scene.Meshes[i1];
            short[] meshIndices = mesh.GetShortIndices();
            Material material;
            material = materials[mesh.MaterialIndex];

            if (!meshData.TryGetValue(material, out List<(int BaseStart, int start, int count)>? indexList))
            {
                meshData[material] = indexList = [];
            }

            indexList.Add((vertexIndex, indexStart, meshIndices.Length));

            for (int j = 0; j < meshIndices.Length; ++j)
            {
                if (graphics.Adapter.SupportsExtendedCommandInfo)
                {
                    indices[j + indexStart] = meshIndices[j];
                }
                else
                {
                    indices[j + indexStart] = meshIndices[j] + vertexIndex;
                }
            }

            indexStart += meshIndices.Length;

            for (int i = 0; i < mesh.VertexCount; ++i)
            {
                Vector3D pos = mesh.Vertices[i];
                Vector3D uv = mesh.TextureCoordinateChannels[0][i];
                Color4D color = mesh.VertexColorChannels[0].Count == 0 ? new Color4D(1, 1, 1, 1) : mesh.VertexColorChannels[0][i];
                Vector3D norm = mesh.Normals[i];

                minX = pos.X.Min(minX);
                minY = pos.Y.Min(minY);
                minZ = pos.Z.Min(minZ);
                maxX = pos.X.Max(maxX);
                maxY = pos.Y.Max(maxY);
                maxZ = pos.Z.Max(maxZ);

                GorgonVertexPosNormColorUv vertex = new(new Vector3(pos.X, pos.Y, pos.Z),
                                                                            new Vector3(norm.X, norm.Y, norm.Z),
                                                                            new GorgonColor(color.R, color.G, color.B, color.A),
                                                                            new Vector2(uv.X, -uv.Y));

                vertices[vertexIndex++] = vertex;
            }
        }

        GorgonCommandList copyList = graphics.GetCommandList("Asset Import Upload List")
                                             .CopyRange(vertices, vertexData.Buffer)
                                             .CopyRange(indices, indexData);

        graphics.Submit(copyList);
        graphics.WaitForGpu();

        GorgonBoundingBox aabb = new(minX, minY, minZ, maxX, maxY, maxZ);

        return (vertexData, indexData, aabb, meshData);
    }

    /// <summary>
    /// Property to return the world matrix for the model.
    /// </summary>
    /// <returns>The read only reference to the world matrix for the model.</returns>
    public ref readonly System.Numerics.Matrix4x4 GetWorldMatrix()
    {
        System.Numerics.Quaternion quatRotation = System.Numerics.Quaternion.CreateFromYawPitchRoll(RotateY.ToRadians(), RotateX.ToRadians(), 0);
        _worldMatrix = System.Numerics.Matrix4x4.CreateFromQuaternion(quatRotation);
        return ref _worldMatrix;
    }        

    /// <inheritdoc cref="Program.Dispose(bool)"/>
    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            foreach (IGorgonTextureView<GorgonTexture> texture in Textures.Values)
            {
                texture.Dispose();
            }

            IndexData?.Dispose();
            VertexData?.Dispose();
        }
    }

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Function to update the global texture resources.
    /// </summary>
    /// <param name="graphics">The graphics interface used for creating textures.</param>
    /// <param name="modelPath">The path to the model. The textures must be located in the same place.</param>
    /// <param name="materials">The list of materials for the scene provided by asset importer.</param>
    /// <param name="model">The model.</param>
    /// <returns>The list of textures used by the model.</returns>
    private static Dictionary<string, IGorgonTextureView<GorgonTexture>> PopulateTextureList(GorgonGraphics graphics, string modelPath, List<Assimp.Material> materials)
    {
        Dictionary<string, IGorgonTextureView<GorgonTexture>> result = [];
        GorgonCodecTga tgaCodec = new();
        string modelDir = Path.GetDirectoryName(modelPath) ?? throw new IOException($"Invalid path '{modelPath}'");

        foreach (Assimp.Material material in materials)
        {
            string id = string.Empty;

            if (!string.IsNullOrWhiteSpace(material.TextureDiffuse.FilePath))
            {
                id = Path.GetFileName(material.TextureDiffuse.FilePath);
            }
            else if (!string.IsNullOrWhiteSpace(material.TextureEmissive.FilePath))
            {
                id = Path.GetFileName(material.TextureEmissive.FilePath);
            }

            if ((string.IsNullOrWhiteSpace(id))
                || (result.ContainsKey(id)))
            {
                continue;
            }

            string texturePath = Path.Combine(modelDir, id);

            using IGorgonImage image = tgaCodec.FromFile(texturePath);
            result[id] = IGorgonTextureView<GorgonTexture>.CreateTexture(graphics, texturePath, image);
        }        


        return result;
    }

    /// <summary>
    /// Function to load the data for a model.
    /// </summary>
    /// <param name="graphics">The graphics interface.</param>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="textures">The texture resources.</param>
    /// <returns>The model to render.</returns>
    public static Model Load(GorgonGraphics graphics, string filePath)
    {
        using AssimpContext context = new();
        Scene scene = context.ImportFile(filePath, PostProcessSteps.MakeLeftHanded | PostProcessSteps.FlipWindingOrder | PostProcessSteps.GenerateSmoothNormals);

        IReadOnlyDictionary<string, IGorgonTextureView<GorgonTexture>> textures = PopulateTextureList(graphics, filePath, scene.Materials);
        IReadOnlyList<Material> materials = GetMaterials(graphics, scene.Materials, textures);
        (GorgonStructuredBufferView vertexBuffer, GorgonIndexBuffer indexBuffer, GorgonBoundingBox aabb, IReadOnlyDictionary<Material, List<(int BaseStart, int Start, int Count)>> meshData) = ImportData(graphics, scene, materials);

        return new(textures, vertexBuffer, indexBuffer, aabb, meshData);        
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Model"/> type.
    /// </summary>
    /// <param name="textures">The textures used by the model.</param>
    /// <param name="vertexData">The vertex buffer for the model.</param>
    /// <param name="indexData">The index buffer for the model.</param>
    /// <param name="aabb">The axis aligned bounding box for the model.</param>
    /// <param name="meshData">The ranges within the vertex buffer to draw.</param>
    private Model(IReadOnlyDictionary<string, IGorgonTextureView<GorgonTexture>> textures, GorgonStructuredBufferView vertexData, GorgonIndexBuffer indexData, GorgonBoundingBox aabb, IReadOnlyDictionary<Material, List<(int BaseStart, int Start, int Count)>> meshData)
    {
        Textures = textures;
        Meshes = meshData;
        VertexData = vertexData;
        IndexData = indexData;

        MinX = aabb.Minimum.X;
        MinY = aabb.Minimum.Y;
        MinZ = aabb.Minimum.Z;
        MaxX = aabb.Maximum.X;
        MaxY = aabb.Maximum.Y;
        MaxZ = aabb.Maximum.Z;
    }
}