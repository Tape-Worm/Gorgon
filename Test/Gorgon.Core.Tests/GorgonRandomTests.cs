using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Numerics;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonRandomTests
{
    [TestMethod]
    public void SimplexNoise_FloatSeed_ReturnsExpectedValue()
    {
        float seed = 0.5f;
        float result = GorgonRandom.SimplexNoise(seed);
        Assert.IsTrue(result >= -1.0f && result <= 1.0f);
    }

    [TestMethod]
    public void SimplexNoise_Vector2Seed_ReturnsExpectedValue()
    {
        Vector2 seed = new(0.5f, 0.5f);
        float result = GorgonRandom.SimplexNoise(seed);
        Assert.IsTrue(result >= -1.0f && result <= 1.0f);
    }

    [TestMethod]
    public void SimplexNoise_Vector3Seed_ReturnsExpectedValue()
    {
        Vector3 seed = new(0.5f, 0.5f, 0.5f);
        float result = GorgonRandom.SimplexNoise(seed);
        Assert.IsTrue(result >= -1.0f && result <= 1.0f);
    }

    [TestMethod]
    public void SimplexNoise_FloatSeed_ReturnsSameValueForSameSeed()
    {
        float seed = 0.8f;
        float result1 = GorgonRandom.SimplexNoise(seed);
        float result2 = GorgonRandom.SimplexNoise(seed);
        Assert.AreEqual(result1, result2);
    }

    [TestMethod]
    public void SimplexNoise_FloatSeed_ReturnsDifferentValueForDifferentSeed()
    {
        float seed1 = 0.8f;
        float seed2 = 0.9f;
        float result1 = GorgonRandom.SimplexNoise(seed1);
        float result2 = GorgonRandom.SimplexNoise(seed2);
        Assert.AreNotEqual(result1, result2);
    }

    [TestMethod]
    public void SimplexNoise_Vector2Seed_ReturnsSameValueForSameSeed()
    {
        Vector2 seed = new(0.3f, 0.7f);
        float result1 = GorgonRandom.SimplexNoise(seed);
        float result2 = GorgonRandom.SimplexNoise(seed);
        Assert.AreEqual(result1, result2);
    }

    [TestMethod]
    public void SimplexNoise_Vector2Seed_ReturnsDifferentValueForDifferentSeed()
    {
        Vector2 seed1 = new(0.3f, 0.7f);
        Vector2 seed2 = new(0.4f, 0.8f);
        float result1 = GorgonRandom.SimplexNoise(seed1);
        float result2 = GorgonRandom.SimplexNoise(seed2);
        Assert.AreNotEqual(result1, result2);
    }

    [TestMethod]
    public void SimplexNoise_Vector3Seed_ReturnsSameValueForSameSeed()
    {
        Vector3 seed = new(0.1f, 0.2f, 0.3f);
        float result1 = GorgonRandom.SimplexNoise(seed);
        float result2 = GorgonRandom.SimplexNoise(seed);
        Assert.AreEqual(result1, result2);
    }

    [TestMethod]
    public void SimplexNoise_Vector3Seed_ReturnsDifferentValueForDifferentSeed()
    {
        Vector3 seed1 = new(0.1f, 0.2f, 0.3f);
        Vector3 seed2 = new(0.4f, 0.5f, 0.6f);
        float result1 = GorgonRandom.SimplexNoise(seed1);
        float result2 = GorgonRandom.SimplexNoise(seed2);
        Assert.AreNotEqual(result1, result2);
    }
}
