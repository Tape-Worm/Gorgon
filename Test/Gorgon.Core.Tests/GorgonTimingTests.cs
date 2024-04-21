using Gorgon.Math;
using Gorgon.Timing;
using Moq;

namespace Gorgon.Core.Tests;

[TestClass]
public class GorgonTimingTests
{
    private static class Time
    {
        private static double _time;

        public static double Delta
        {
            get;
            set;
        } = (1.0 / 30) * 1000;

        public static double GetTime()
        {
            double currTime = _time;
            _time += Delta;
            return currTime;
        }
    }

    [TestMethod]
    public void TestTimingValues()
    {
        IGorgonTimer timer = Mock.Of<IGorgonTimer>(x => x.IsHighResolution == true);
        Mock<IGorgonTimer> mock = Mock.Get(timer);
        mock.SetupGet(x => x.Milliseconds)
            .Returns(() => Time.GetTime());

        GorgonTiming.StartTiming(timer);

        for (int i = 0; i < 1000; ++i)
        {
            if (i < 100)
            {
                Time.Delta = (1.0 / 16) * 1000;
            }
            else if ((i > 400) && (i < 460))
            {
                Time.Delta = (1.0 / 60) * 1000;
            }
            else
            {
                Time.Delta = (1.0 / 30) * 1000;
            }
            GorgonTiming.Update();
        }

        Assert.IsTrue(GorgonTiming.FPS.EqualsEpsilon(30.0f));
        Assert.IsTrue(GorgonTiming.Delta.EqualsEpsilon(0.0333f, 0.0001f));
        Assert.IsTrue(GorgonTiming.HighestDelta.EqualsEpsilon(0.0625f, 0.0001f));
        Assert.IsTrue(GorgonTiming.LowestDelta.EqualsEpsilon(0.0166f, 0.0001f));
        Assert.AreEqual(1000u, GorgonTiming.FrameCount);
        Assert.AreEqual(GorgonTiming.FrameCount, GorgonTiming.FrameCountULong);
        Assert.AreEqual(53, GorgonTiming.HighestFPS);
        Assert.AreEqual(16, GorgonTiming.LowestFPS);
        Assert.IsTrue(GorgonTiming.SecondsSinceStart.EqualsEpsilon(35.233f, 0.001f));
        Assert.IsTrue(GorgonTiming.MillisecondsSinceStart.EqualsEpsilon(35233.332f, 0.001f));

        GorgonTiming.Reset();
        GorgonTiming.TimeScale = 0.25f;
        GorgonTiming.Update();

        Assert.IsTrue(GorgonTiming.Delta.EqualsEpsilon(0.0833f, 0.0001f));
        Assert.IsTrue(GorgonTiming.UnscaledDelta.EqualsEpsilon(0.3333f, 0.0001f));

        GorgonTiming.Reset();
        GorgonTiming.TimeScale = 4.0f;
        GorgonTiming.Update();

        Assert.IsTrue(GorgonTiming.Delta.EqualsEpsilon(1.3333f, 0.0001f));
        Assert.IsTrue(GorgonTiming.UnscaledDelta.EqualsEpsilon(0.3333f, 0.0001f));

        GorgonTiming.Reset();
        GorgonTiming.TimeScale = 1.0f;
        GorgonTiming.MaximumFrameDelta = 0.4242f;

        Time.Delta = 16384;
        GorgonTiming.Update();

        Assert.IsTrue(GorgonTiming.Delta.EqualsEpsilon(0.4242f, 0.0001f));

        double micro = GorgonTiming.FpsToMicroseconds(30);
        double milli = GorgonTiming.FpsToMilliseconds(60);

        Assert.IsTrue(micro.EqualsEpsilon(33333.333, 0.001));
        Assert.IsTrue(milli.EqualsEpsilon(16.666, 0.001));
    }
}
