using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Gorgon.Graphics;
using Gorgon.Json;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Gorgon.Core.Tests;

public class GorgonColorContractResolver
    : DefaultContractResolver
{
    protected override JsonConverter? ResolveContractConverter(Type objectType)
    {
        if (objectType == typeof(GorgonColor))
        {
            return null;
        }

        return base.ResolveContractConverter(objectType);
    }
}

public class JsonData
{
    public string Id
    {
        get;
        private set;
    } = Guid.NewGuid().ToString(); 

    public GorgonColor Color
    {
        get;
        set;
    }

    public GorgonColor? NullableColor
    {
        get;
        set;
    }
}

[TestClass]
public class GorgonColorTests
{
    [TestMethod]
    public void Ctor()
    {
        GorgonColor ctor1 = new(1.0f, 0.5f, 0.25f, 0.125f);
        GorgonColor ctor2 = new(1.0f, 0.5f, 0.25f);
        GorgonColor ctor3 = new(ctor1, 0.75f);

        Assert.AreEqual(1.0f, ctor1.Red);
        Assert.AreEqual(0.5f, ctor1.Green);
        Assert.AreEqual(0.25f, ctor1.Blue);
        Assert.AreEqual(0.125f, ctor1.Alpha);

        Assert.AreEqual(1.0f, ctor2.Red);
        Assert.AreEqual(0.5f, ctor2.Green);
        Assert.AreEqual(0.25f, ctor2.Blue);
        Assert.AreEqual(1.0f, ctor2.Alpha);

        Assert.AreEqual(1.0f, ctor3.Red);
        Assert.AreEqual(0.5f, ctor3.Green);
        Assert.AreEqual(0.25f, ctor3.Blue);
        Assert.AreEqual(0.75f, ctor3.Alpha);
    }

    [TestMethod]
    public void Deconstruct()
    {
        GorgonColor color = GorgonColors.Red;
        
        (float r, float g, float b, float a) = color;

        Assert.AreEqual(1.0f, r);
        Assert.AreEqual(0.0f, g);
        Assert.AreEqual(0.0f, b);
        Assert.AreEqual(1.0f, a);

        (int ir, int ig, int ib, int ia) = color.GetIntegerComponents();

        Assert.AreEqual(255, ir);
        Assert.AreEqual(0, ig);
        Assert.AreEqual(0, ib);
        Assert.AreEqual(255, ia);
    }

    [TestMethod]
    public void ToARGB()
    {
        GorgonColor color = new(0.5f, 0.75f, 0.25f, 1.0f);
        int signedArgb = GorgonColor.ToARGB(color);

        unchecked
        {
            Assert.AreEqual((int)0xff7fbf3f, signedArgb);
        }
    }

    [TestMethod]
    public void FromARGB()
    {
        GorgonColor color = GorgonColor.FromARGB(0xff7fbf3f);
        
        Assert.AreEqual(0.498039216f, color.Red);
        Assert.AreEqual(0.7490196f, color.Green);
        Assert.AreEqual(0.247058824f, color.Blue);
        Assert.AreEqual(1.0f, color.Alpha);
    }

    [TestMethod]
    public void ToABGR()
    {
        GorgonColor color = new(0.5f, 0.75f, 0.25f, 1.0f);
        int signedAbgr = GorgonColor.ToABGR(color);
        
        unchecked
        {
            Assert.AreEqual((int)0xff3fbf7f, signedAbgr);
        }
    }

    [TestMethod]
    public void FromABGR()
    {
        GorgonColor color = GorgonColor.FromABGR(0xff3fbf7f);

        Assert.AreEqual(0.498039216f, color.Red);
        Assert.AreEqual(0.7490196f, color.Green);
        Assert.AreEqual(0.247058824f, color.Blue);
        Assert.AreEqual(1.0f, color.Alpha);
    }

    [TestMethod]
    public void PremultiplyAlpha()
    {
        GorgonColor color = new(GorgonColors.White, 0.5f);
        GorgonColor premultiplied = GorgonColor.PremultiplyAlpha(color);
        
        Assert.AreEqual(new GorgonColor(0.5f, 0.5f, 0.5f, 0.5f), premultiplied);
    }

    [TestMethod]
    public void RemovePremultipliedAlpha()
    {
        GorgonColor color = new(GorgonColors.White, 0.5f);
        GorgonColor premultiplied = GorgonColor.PremultiplyAlpha(color);
        color = GorgonColor.RemovePremultipliedAlpha(premultiplied);

        Assert.AreEqual(new GorgonColor(1.0f, 1.0f, 1.0f, 0.5f), color);
    }

    [TestMethod]
    public void ToRGBA()
    {
        GorgonColor color = new(0.5f, 0.75f, 0.25f, 1.0f);
        int signedRgba = GorgonColor.ToRGBA(color);

        unchecked
        {
            Assert.AreEqual((int)0x7fbf3fff, signedRgba);
        }
    }

    [TestMethod]
    public void FromRGBA()
    {
        GorgonColor color = GorgonColor.FromRGBA(0x7fbf3fff);

        Assert.AreEqual(0.498039216f, color.Red);
        Assert.AreEqual(0.7490196f, color.Green);
        Assert.AreEqual(0.247058824f, color.Blue);
        Assert.AreEqual(1.0f, color.Alpha);
    }

    [TestMethod]
    public void ToBGRA()
    {
        GorgonColor color = new(0.5f, 0.75f, 0.25f, 1.0f);
        int signedBgra = GorgonColor.ToBGRA(color);
        
        unchecked
        {
            Assert.AreEqual((int)0x3fbf7fff, signedBgra);
        }
    }

    [TestMethod]
    public void FromBGRA()
    {
        GorgonColor color = GorgonColor.FromBGRA(0x3fbf7fff);

        Assert.AreEqual(0.498039216f, color.Red);
        Assert.AreEqual(0.7490196f, color.Green);
        Assert.AreEqual(0.247058824f, color.Blue);
        Assert.AreEqual(1.0f, color.Alpha);
    }

    [TestMethod]
    public void ToColor ()
    {
        GorgonColor color = new(0.5f, 0.75f, 0.25f, 1.0f);
        System.Drawing.Color drawingColor = GorgonColor.ToColor(color);

        Assert.AreEqual(127, drawingColor.R);
        Assert.AreEqual(191, drawingColor.G);
        Assert.AreEqual(63, drawingColor.B);
        Assert.AreEqual(255, drawingColor.A);
    }

    [TestMethod]
    public void FromColor()
    {
        System.Drawing.Color drawingColor = System.Drawing.Color.FromArgb(255, 127, 191, 63);
        GorgonColor color = GorgonColor.FromColor(drawingColor);

        Assert.AreEqual(127, drawingColor.R);
        Assert.AreEqual(191, drawingColor.G);
        Assert.AreEqual(63, drawingColor.B);
        Assert.AreEqual(255, drawingColor.A);
    }

    [TestMethod]
    public void ToVector3()
    {
        GorgonColor color = new(0.5f, 0.75f, 0.25f, 1.0f);
        Vector3 rgb = GorgonColor.ToVector3(color);

        Assert.AreEqual(0.5f, rgb.X);
        Assert.AreEqual(0.75, rgb.Y);
        Assert.AreEqual(0.25f, rgb.Z);
    }

    [TestMethod]
    public void FromVector3()
    {
        Vector3 rgb = new(0.5f, 0.75f, 0.25f);
        GorgonColor color = GorgonColor.FromVector3(rgb);

        Assert.AreEqual(0.5f, rgb.X);
        Assert.AreEqual(0.75, rgb.Y);
        Assert.AreEqual(0.25f, rgb.Z);
    }

    [TestMethod]
    public void ToVector4()
    {
        GorgonColor color = new(0.5f, 0.75f, 0.25f, 1.0f);
        Vector4 rgb = GorgonColor.ToVector4(color);
        
        Assert.AreEqual(0.5f, rgb.X);
        Assert.AreEqual(0.75, rgb.Y);
        Assert.AreEqual(0.25f, rgb.Z);
        Assert.AreEqual(1.0f, rgb.W);
    }

    [TestMethod]
    public void FromVector4()
    {
        Vector4 rgba = new(0.5f, 0.75f, 0.25f, 1.0f);
        GorgonColor color = GorgonColor.FromVector4(rgba);
        
        Assert.AreEqual(0.5f, rgba.X);
        Assert.AreEqual(0.75, rgba.Y);
        Assert.AreEqual(0.25f, rgba.Z);
        Assert.AreEqual(1.0f, rgba.W);
    }

    [TestMethod]
    public void ToSrgb()
    {
        GorgonColor color = GorgonColor.ToSRgb(GorgonColors.DarkRed);
        Assert.AreEqual(0.76431364f, color.Red);
    }

    [TestMethod]
    public void ToLinear()
    {
        GorgonColor color = new GorgonColor(0.76431364f, 0, 0, 1);
        color = GorgonColor.ToLinear(color);
        
        Assert.AreEqual(0.545098f, color.Red);
    }

    [TestMethod]
    public void Lerp()
    {
        GorgonColor colorStart = GorgonColors.Red;
        GorgonColor colorEnd = GorgonColors.Blue;

        GorgonColor result = GorgonColor.Lerp(colorStart, colorEnd, 0.5f);

        Assert.AreEqual(0.5f, result.Red);
        Assert.AreEqual(0.5f, result.Blue);
    }

    [TestMethod]
    public void Clamp()
    {
        GorgonColor color = new GorgonColor(-1, 0, 5, 1);
        GorgonColor result = GorgonColor.Clamp(color);

        Assert.AreEqual(0.0f, result.Red);
        Assert.AreEqual(1.0f, result.Blue);
    }

    [TestMethod]
    public void Serialization()
    {
        JsonData data = new()
        {
            Color = GorgonColors.Purple
        };

        string expected = "{\"Id\":\""+ data.Id +"\",\"Color\":-65281,\"NullableColor\":null}";
        string json = Newtonsoft.Json.JsonConvert.SerializeObject(data);

        Assert.AreEqual(expected, json);

        JsonData? actual = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonData>(json);

        Assert.IsNotNull(actual);
        Assert.IsNull(actual.NullableColor);
        Assert.AreEqual(data.Color, actual.Color);

        data.NullableColor = GorgonColors.Blue;

        expected = "{\"Id\":\"" + data.Id + "\",\"Color\":-65281,\"NullableColor\":-16776961}";
        json = Newtonsoft.Json.JsonConvert.SerializeObject(data);

        Assert.AreEqual(expected, json);

        actual = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonData>(json);

        Assert.IsNotNull(actual);
        Assert.IsNotNull(actual.NullableColor);
        Assert.AreEqual(data.Color, actual.Color);

        data.NullableColor = null;

        expected = "{\"Id\":\"" + data.Id + "\",\"Color\":{\"r\":255,\"g\":0,\"b\":255,\"a\":255},\"NullableColor\":null}";
        json = Newtonsoft.Json.JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            ContractResolver = new GorgonColorContractResolver(),
            Converters = new[] { new GorgonColorComponentsJsonConverter() }
        });

        Assert.AreEqual(expected, json);


        actual = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonData>(json, new JsonSerializerSettings
        {
            ContractResolver = new GorgonColorContractResolver(),
            Converters = new[] { new GorgonColorComponentsJsonConverter() }
        });
        Assert.IsNotNull(actual);
        Assert.IsNull(actual.NullableColor);
        Assert.AreEqual(data.Color, actual.Color);

        data.NullableColor = GorgonColors.Red;

        expected = "{\"Id\":\"" + data.Id + "\",\"Color\":{\"r\":255,\"g\":0,\"b\":255,\"a\":255},\"NullableColor\":{\"r\":255,\"g\":0,\"b\":0,\"a\":255}}";
        json = Newtonsoft.Json.JsonConvert.SerializeObject(data, new JsonSerializerSettings
        {
            ContractResolver = new GorgonColorContractResolver(),
            Converters = new[] { new GorgonColorComponentsJsonConverter() }
        });

        Assert.AreEqual(expected, json);

        actual = Newtonsoft.Json.JsonConvert.DeserializeObject<JsonData>(json, new JsonSerializerSettings
        {
            ContractResolver = new GorgonColorContractResolver(),
            Converters = new[] { new GorgonColorComponentsJsonConverter() }
        });
        Assert.IsNotNull(actual);
        Assert.IsNotNull(actual.NullableColor);
        Assert.AreEqual(data.Color, actual.Color);
    }
}
