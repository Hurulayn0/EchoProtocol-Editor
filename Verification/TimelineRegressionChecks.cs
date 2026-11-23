using System;
using System.Collections.Generic;
using EchoProtocol;

// Engine-independent boundary and generated-recording regression checks.
public static class TimelineRegressionChecks
{
    private static int passed;
    public static string Run()
    {
        passed = 0;
        var mutable = new ReplayTimeline<float, int>();
        Expect<ArgumentOutOfRangeException>(() => mutable.AddFrame(-1, 0), "negative frame");
        Expect<ArgumentOutOfRangeException>(() => mutable.AddFrame(float.PositiveInfinity, 0), "infinite frame");
        Expect<ArgumentOutOfRangeException>(() => mutable.AddFrame(float.NaN, 0), "NaN frame");
        Expect<ArgumentOutOfRangeException>(() => mutable.AddAction(-1, 0), "negative action");
        Expect<ArgumentOutOfRangeException>(() => mutable.AddAction(float.NegativeInfinity, 0), "infinite action");
        Expect<ArgumentOutOfRangeException>(() => mutable.Sample(float.NaN, Lerp), "NaN sample");
        Expect<ArgumentOutOfRangeException>(() => mutable.Sample(float.PositiveInfinity, Lerp), "infinite sample");
        Expect<InvalidOperationException>(() => mutable.Sample(0, Lerp), "empty sample");
        Expect<InvalidOperationException>(() => mutable.CreateCursor(), "uncommitted cursor");
        mutable.AddFrame(1, 2); mutable.AddFrame(2, 4);
        Expect<ArgumentException>(() => mutable.AddFrame(.5f, 1), "out-of-order frame");
        mutable.AddAction(1, 7);
        Expect<ArgumentException>(() => mutable.AddAction(.5f, 8), "out-of-order action");
        mutable.AddAction(1, 8); mutable.AddAction(2, 9); mutable.AddFrame(2, 6); mutable.Freeze();
        Expect<InvalidOperationException>(() => mutable.AddFrame(3, 9), "frozen frames");
        Expect<InvalidOperationException>(() => mutable.AddAction(2, 9), "frozen actions");
        Check(mutable.FrameCount == 2 && mutable.Sample(2, Lerp) == 6, "same-time frame replaces");
        Check(mutable.Sample(.5f, Lerp) == 2 && mutable.Sample(0, Lerp) == 2, "before first frame clamps");
        Check(mutable.Sample(1.5f, Lerp) == 4, "linear interpolation");
        Check(mutable.Sample(2, Lerp) == 6 && mutable.Sample(200, Lerp) == 6, "end and after end hold");
        var cursor = mutable.CreateCursor(); var events = new List<int>();
        cursor.Advance(1, (t, v) => events.Add(v));
        Check(events.Count == 2 && events[0] == 7 && events[1] == 8, "same-time action stable order");
        cursor.Advance(1, (t, v) => events.Add(v));
        Check(events.Count == 2, "repeated time does not redispatch");
        cursor.Advance(2, (t, v) => events.Add(v));
        Check(events.Count == 3 && events[2] == 9, "final-time action included");
        Expect<ArgumentException>(() => cursor.Advance(1, (t, v) => {}), "rewind needs restart");
        cursor.Restart(); cursor.Advance(3, (t, v) => events.Add(v));
        Check(events.Count == 6, "restart replays full action sequence");
        int independent = 0;
        mutable.CreateCursor().Advance(3, (t, v) => independent++);
        Check(independent == 3, "independent replay cursors");
        var late = new ReplayTimeline<float, int>(); late.AddFrame(0, 0); late.AddAction(1, 1);
        Expect<InvalidOperationException>(() => late.Freeze(), "action outside recording duration");

        var random = new Random(624017);
        for (int scenario = 0; scenario < 1000; scenario++)
        {
            var generated = new ReplayTimeline<float, int>();
            int eventCount = random.Next(1, 150);
            float duration = random.Next(1, 40);
            generated.AddFrame(0, 0); generated.AddFrame(duration, duration * 3);
            var times = new List<float>();
            for (int i = 0; i < eventCount; i++) times.Add((float)Math.Round(random.NextDouble() * duration, 2));
            times.Sort();
            for (int i = 0; i < times.Count; i++) generated.AddAction(times[i], i);
            generated.Freeze();
            var playback = generated.CreateCursor(); int next = 0;
            float now = 0;
            Action<float, int> dispatch = (t, value) => {
                if (value != next || Math.Abs(t - times[next]) > .00001f)
                    throw new Exception("Generated event order/time mismatch in recording " + scenario);
                next++;
            };
            while (now < duration)
            {
                now = Math.Min(duration, now + .01f + (float)random.NextDouble() * 2);
                playback.Advance(now, dispatch); playback.Advance(now, dispatch);
                if (Math.Abs(generated.Sample(now, Lerp) - now * 3) > .0001f)
                    throw new Exception("Generated pose mismatch in recording " + scenario);
            }
            if (next != eventCount) throw new Exception("Skipped generated action in recording " + scenario);
            playback.Restart(); next = 0; playback.Advance(duration + 100, dispatch);
            if (next != eventCount) throw new Exception("Generated restart failed in recording " + scenario);
        }
        Check(true, "1000 generated recordings preserve interpolation order exactly-once dispatch and restart");
        if (passed != 25) throw new Exception("Expected 25 timeline regression checks, got " + passed);
        return passed + "/25 timeline regression checks passed (1000 generated recordings)";
    }

    private static float Lerp(float a, float b, float t) { return a + (b - a) * t; }
    private static void Check(bool condition, string name) { if (!condition) throw new Exception("FAIL: " + name); passed++; }
    private static void Expect<T>(Action action, string name) where T : Exception
    {
        try { action(); } catch (T) { passed++; return; }
        throw new Exception("FAIL: " + name);
    }
}
