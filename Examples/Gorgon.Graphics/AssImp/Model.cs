using System.Numerics;
using Assimp;
using Gorgon.Graphics;
using Gorgon.Graphics.Core;
using Gorgon.Graphics.Imaging.Codecs;
using Gorgon.Math;
using Gorgon.Renderers.Geometry;

namespace Gorgon.Examples;

/// <summary>
/// Model data to render
/// </summary>
internal class Model
    : IDisposable
{

    // The list of meshes to render.
    private readonly Dictionary<Material, List<(int BaseStart, int Start, int Count)>> _meshes = [];
    // The world matrix for this model.
    private System.Numerics.Matrix4x4 _worldMatrix;

    /// <summary>
    /// Property to return the list of meshes to render.
    /// </summary>
    public IReadOnlyDictionary<Material, List<(int BaseStart, int Start, int Count)>> Meshes => _meshes;

    /// <summary>
    /// Property to return the vertex buffer for the model.
    /// </summary>
    public GorgonVertexBuffer VertexData
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
    /// Function to retrieve the list of materials for the model.
    /// </summary>
    /// <param name="sceneMaterials">The asset importer material list.</param>
    /// <param name="textures">The global texture list.</param>
    /// <returns>A list of model materials associated with their textures.</returns>
    private static IReadOnlyList<Material> GetMaterials(List<Assimp.Material> sceneMaterials, IReadOnlyDictionary<string, GorgonTexture2DView> textures)
    {
        List<Material> materials = [];

        for (int m = 0; m < sceneMaterials.Count; ++m)
        {
            Assimp.Material mat = sceneMaterials[m];
            Material material = new()
            {
                Diffuse = new GorgonColor(mat.ColorDiffuse.R, mat.ColorDiffuse.G, mat.ColorDiffuse.B, mat.ColorDiffuse.A),
                Emissive = new GorgonColor(mat.ColorEmissive.R, mat.ColorEmissive.G, mat.ColorEmissive.B, 1.0f),
                Specular = new GorgonColor(mat.ColorSpecular.R, mat.ColorSpecular.G, mat.ColorSpecular.B, 1.0f),
                SpecularPower = mat.Shininess,
                TextureSampler = GorgonSamplerState.Wrapping
            };

            materials.Add(material);

            string id = string.Empty;

            if (!string.IsNullOrWhiteSpace(mat.TextureDiffuse.FilePath))
            {
                id = Path.GetFileName(mat.TextureDiffuse.FilePath);
            }
            else if (!string.IsNullOrWhiteSpace(mat.TextureEmissive.FilePath))
            {
                id = Path.GetFileName(mat.TextureEmissive.FilePath);
            }

            if ((string.IsNullOrWhiteSpace(id)) || (!textures.TryGetValue(id, out GorgonTexture2DView texture)))
            {
                continue;
            }

            material.Texture = texture;
        }

        return materials;
    }

    /// <summary>
    /// Function to import data from the asset importer.
    /// </summary>
    /// <param name="graphics">The graphics interface.</param>
    /// <param name="scene">The scene containing the mesh data.</param>
    /// <param name="materials">The list of materials for the model.</param>
    private void ImportData(GorgonGraphics graphics, Scene scene, IReadOnlyList<Material> materials)
    {
        int vertexCount = scene.Meshes.Sum(item => item.VertexCount);
        int indexCount = scene.Meshes.SelectMany(item => item.Faces).Sum(item => item.IndexCount);

        VertexData = new GorgonVertexBuffer(graphics, new GorgonVertexBufferInfo(vertexCount * GorgonVertexPosNormColorUv.SizeInBytes));
        IndexData = new GorgonIndexBuffer(graphics, new GorgonIndexBufferInfo(indexCount)
        {
            Use16BitIndices = true
        });

        int indexStart = 0;
        int vertexIndex = 0;

        GorgonVertexPosNormColorUv[] vertices = new GorgonVertexPosNormColorUv[vertexCount];
        short[] indices = new short[indexCount];

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

            Material material = materials[mesh.MaterialIndex];

            if (!_meshes.TryGetValue(material, out List<(int BaseStart, int start, int count)> indexList))
            {
                _meshes[material] = indexList = [];
            }

            indexList.Add((vertexIndex, indexStart, meshIndices.Length));

            for (int j = 0; j < meshIndices.Length; ++j)
            {
                indices[j + indexStart] = meshIndices[j];
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

        VertexData.SetData<GorgonVertexPosNormColorUv>(vertices);
        IndexData.SetData<short>(indices);

        MinX = minX;
        MinY = minY;
        MinZ = minZ;
        MaxX = maxX;
        MaxY = maxY;
        MaxZ = maxZ;
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

    /// <summary>Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.</summary>
    public void Dispose()
    {
        IndexData?.Dispose();
        VertexData?.Dispose();
    }

    /// <summary>
    /// Function to update the global texture resources.
    /// </summary>
    /// <param name="graphics">The graphics interface used for creating textures.</param>
    /// <param name="modelPath">The path to the model. The textures must be located in the same place.</param>
    /// <param name="materials">The list of materials for the scene provided by asset importer.</param>
    /// <param name="textures">The global list of textures.</param>
    private static void PopulateTextureList(GorgonGraphics graphics, string modelPath, List<Assimp.Material> materials, Dictionary<string, GorgonTexture2DView> textures)
    {
        GorgonCodecTga tgaCodec = new();
        string modelDir = Path.GetDirectoryName(modelPath);

        foreach (Assimp.Material material in materials)
        {
            string id = null;

            if (!string.IsNullOrWhiteSpace(material.TextureDiffuse.FilePath))
            {
                id = Path.GetFileName(material.TextureDiffuse.FilePath);
            }
            else if (!string.IsNullOrWhiteSpace(material.TextureEmissive.FilePath))
            {
                id = Path.GetFileName(material.TextureEmissive.FilePath);
            }

            if ((string.IsNullOrWhiteSpace(id))
                || (textures.ContainsKey(id)))
            {
                continue;
            }

            string texturePath = Path.Combine(modelDir, id);
            textures[id] = GorgonTexture2DView.FromFile(graphics, texturePath, tgaCodec);
        }
    }

    /// <summary>
    /// Function to load the data for a model.
    /// </summary>
    /// <param name="graphics">The graphics interface.</param>
    /// <param name="filePath">The path to the file.</param>
    /// <param name="textures">The texture resources.</param>
    /// <returns>The model to render.</returns>
    public static Model Load(GorgonGraphics graphics, string filePath, Dictionary<string, GorgonTexture2DView> textures)
    {
        using AssimpContext context = new();
        Scene scene = context.ImportFile(filePath, PostProcessSteps.MakeLeftHanded | PostProcessSteps.FlipWindingOrder | PostProcessSteps.GenerateSmoothNormals);

        Model result = new();

        PopulateTextureList(graphics, filePath, scene.Materials, textures);
        IReadOnlyList<Material> materials = GetMaterials(scene.Materials, textures);

        result.ImportData(graphics, scene, materials);

        return result;
    }
}
