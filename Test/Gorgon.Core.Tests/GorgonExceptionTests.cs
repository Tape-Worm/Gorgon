using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonExceptionTests
{
    [TestMethod]
    public void ExceptionResult()
    {
        ObjectDisposedException inner = new("Object");
        GorgonException exception = new(GorgonResult.AccessDenied, "Test message.", inner);

        Assert.AreEqual(inner, exception.InnerException);
        Assert.AreEqual(GorgonResult.AccessDenied.Code, exception.ResultCode.Code);
        Assert.AreEqual(GorgonResult.AccessDenied.Name, exception.ResultCode.Name);
        Assert.AreEqual(GorgonResult.AccessDenied.Description, exception.ResultCode.Description);
        Assert.AreEqual($"{GorgonResult.AccessDenied.Description}\nTest message.", exception.Message);

        exception = new(GorgonResult.AccessDenied, inner);

        Assert.AreEqual(inner, exception.InnerException);
        Assert.AreEqual(GorgonResult.AccessDenied.Code, exception.ResultCode.Code);
        Assert.AreEqual(GorgonResult.AccessDenied.Name, exception.ResultCode.Name);
        Assert.AreEqual(GorgonResult.AccessDenied.Description, exception.ResultCode.Description);
        Assert.AreEqual($"{GorgonResult.AccessDenied.Description}", exception.Message);

        exception = new(GorgonResult.AccessDenied, "Test message.");

        Assert.IsNull(exception.InnerException);
        Assert.AreEqual(GorgonResult.AccessDenied.Code, exception.ResultCode.Code);
        Assert.AreEqual(GorgonResult.AccessDenied.Name, exception.ResultCode.Name);
        Assert.AreEqual(GorgonResult.AccessDenied.Description, exception.ResultCode.Description);
        Assert.AreEqual($"{GorgonResult.AccessDenied.Description}\nTest message.", exception.Message);

        exception = new(GorgonResult.AccessDenied);

        Assert.IsNull(exception.InnerException);
        Assert.AreEqual(GorgonResult.AccessDenied.Code, exception.ResultCode.Code);
        Assert.AreEqual(GorgonResult.AccessDenied.Name, exception.ResultCode.Name);
        Assert.AreEqual(GorgonResult.AccessDenied.Description, exception.ResultCode.Description);
        Assert.AreEqual($"{GorgonResult.AccessDenied.Description}", exception.Message);

        GorgonResult expected = new("GorgonException", -2146233088, "Test message.");

        exception = new("Test message.");

        Assert.IsNull(exception.InnerException);
        Assert.AreEqual(expected.Code, exception.ResultCode.Code);
        Assert.AreEqual(expected.Name, exception.ResultCode.Name);
        Assert.AreEqual(expected.Description, exception.ResultCode.Description);
        Assert.AreEqual("Test message.", exception.Message);
        Assert.AreEqual(expected, exception.ResultCode);
    }
}
