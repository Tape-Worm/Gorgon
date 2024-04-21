using System.Numerics;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonRandomTests
{
    [TestMethod]
    public void SimplexNoiseFloatSeedReturnsExpectedValue()
    {
        float seed = 0.5f;
        float result = GorgonRandom.SimplexNoise(seed);
        Assert.IsTrue(result >= -1.0f && result <= 1.0f);
    }

    [TestMethod]
    public void SimplexNoiseVector2SeedReturnsExpectedValue()
    {
        Vector2 seed = new(0.5f, 0.5f);
        float result = GorgonRandom.SimplexNoise(seed);
        Assert.IsTrue(result >= -1.0f && result <= 1.0f);
    }

    [TestMethod]
    public void SimplexNoiseVector3SeedReturnsExpectedValue()
    {
        Vector3 seed = new(0.5f, 0.5f, 0.5f);
        float result = GorgonRandom.SimplexNoise(seed);
        Assert.IsTrue(result >= -1.0f && result <= 1.0f);
    }

    [TestMethod]
    public void SimplexNoiseFloatSeedReturnsSameValueForSameSeed()
    {
        float seed = 0.8f;
        float result1 = GorgonRandom.SimplexNoise(seed);
        float result2 = GorgonRandom.SimplexNoise(seed);
        Assert.AreEqual(result1, result2);
    }

    [TestMethod]
    public void SimplexNoiseFloatSeedReturnsDifferentValueForDifferentSeed()
    {
        float seed1 = 0.8f;
        float seed2 = 0.9f;
        float result1 = GorgonRandom.SimplexNoise(seed1);
        float result2 = GorgonRandom.SimplexNoise(seed2);
        Assert.AreNotEqual(result1, result2);
    }

    [TestMethod]
    public void SimplexNoiseVector2SeedReturnsSameValueForSameSeed()
    {
        Vector2 seed = new(0.3f, 0.7f);
        float result1 = GorgonRandom.SimplexNoise(seed);
        float result2 = GorgonRandom.SimplexNoise(seed);
        Assert.AreEqual(result1, result2);
    }

    [TestMethod]
    public void SimplexNoiseVector2SeedReturnsDifferentValueForDifferentSeed()
    {
        Vector2 seed1 = new(0.3f, 0.7f);
        Vector2 seed2 = new(0.4f, 0.8f);
        float result1 = GorgonRandom.SimplexNoise(seed1);
        float result2 = GorgonRandom.SimplexNoise(seed2);
        Assert.AreNotEqual(result1, result2);
    }

    [TestMethod]
    public void SimplexNoiseVector3SeedReturnsSameValueForSameSeed()
    {
        Vector3 seed = new(0.1f, 0.2f, 0.3f);
        float result1 = GorgonRandom.SimplexNoise(seed);
        float result2 = GorgonRandom.SimplexNoise(seed);
        Assert.AreEqual(result1, result2);
    }

    [TestMethod]
    public void SimplexNoiseVector3SeedReturnsDifferentValueForDifferentSeed()
    {
        Vector3 seed1 = new(0.1f, 0.2f, 0.3f);
        Vector3 seed2 = new(0.4f, 0.5f, 0.6f);
        float result1 = GorgonRandom.SimplexNoise(seed1);
        float result2 = GorgonRandom.SimplexNoise(seed2);
        Assert.AreNotEqual(result1, result2);
    }
}
