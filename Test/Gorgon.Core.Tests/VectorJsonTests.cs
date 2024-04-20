using System.Numerics;
using System.Text.Json;
using Gorgon.Json;

namespace Gorgon.Core.Tests;

[TestClass]
public class VectorJsonTests
{
    [TestMethod]
    public void Vector2_JsonConversion()
    {
        Vector2? testVector = new(10, 20);
        JsonSerializerOptions options = new()
        {
            Converters = { new Vector2JsonConverter() }
        };
        string json = JsonSerializer.Serialize(testVector, options);
        Vector2? result = JsonSerializer.Deserialize<Vector2?>(json, options);

        Assert.IsNotNull(result);
        Assert.AreEqual(testVector, result);

        testVector = null;

        json = JsonSerializer.Serialize(testVector, options);
        result = JsonSerializer.Deserialize<Vector2?>(json, options);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void Vector3_JsonConversion()
    {
        Vector3? testVector = new(10, 20, 30);
        JsonSerializerOptions options = new()
        {
            Converters = { new Vector3JsonConverter() }
        };
        string json = JsonSerializer.Serialize(testVector, options);
        Vector3? result = JsonSerializer.Deserialize<Vector3?>(json, options);

        Assert.IsNotNull(result);
        Assert.AreEqual(testVector, result);

        testVector = null;

        json = JsonSerializer.Serialize(testVector, options);
        result = JsonSerializer.Deserialize<Vector3?>(json, options);

        Assert.IsNull(result);
    }

    [TestMethod]
    public void Vector4_JsonConversion()
    {
        Vector4? testVector = new(10, 20, 30, 40);
        JsonSerializerOptions options = new()
        {
            Converters = { new Vector4JsonConverter() }
        };
        string json = JsonSerializer.Serialize(testVector, options);
        Vector4? result = JsonSerializer.Deserialize<Vector4?>(json, options);

        Assert.IsNotNull(result);
        Assert.AreEqual(testVector, result);

        testVector = null;

        json = JsonSerializer.Serialize(testVector, options);
        result = JsonSerializer.Deserialize<Vector4?>(json, options);

        Assert.IsNull(result);
    }
}
