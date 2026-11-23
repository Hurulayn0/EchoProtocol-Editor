using System;

namespace EchoProtocol
{
    public static class TimelineSelfChecks
    {
        public static string Run()
        {
            int passed = 0;
            var tape = new ReplayTimeline<float, string>();
            tape.AddFrame(0, 0); tape.AddFrame(1, 10); tape.AddFrame(2, 20);
            tape.AddAction(0, "start"); tape.AddAction(.3f, "button"); tape.AddAction(.31f, "shot");
            tape.AddAction(2, "end"); tape.Freeze();
            Check(Math.Abs(tape.Sample(.5f, Lerp) - 5) < .0001f, "interpolation", ref passed);
            Check(tape.Sample(30, Lerp) == 20, "final pose holds", ref passed);
            var cursor = tape.CreateCursor(); int fired = 0; string order = "";
            cursor.Advance(0, (time, action) => { fired++; order += action + ";"; });
            Check(fired == 1, "time-zero event", ref passed);
            cursor.Advance(1.5f, (time, action) => { fired++; order += action + ";"; });
            Check(fired == 3 && order == "start;button;shot;", "frame skip preserves events/order", ref passed);
            cursor.Advance(1.5f, (time, action) => fired++);
            Check(fired == 3, "no duplicate dispatch", ref passed);
            cursor.Advance(2, (time, action) => fired++);
            Check(fired == 4, "end event included", ref passed);
            cursor.Restart(); cursor.Advance(3, (time, action) => fired++);
            Check(fired == 8, "reset replays every event", ref passed);
            var second = tape.CreateCursor(); int independent = 0;
            second.Advance(0, (time, action) => independent++);
            Check(independent == 1, "independent echoes", ref passed);
            Expect<InvalidOperationException>(() => tape.AddFrame(3, 30), "immutable tape", ref passed);
            Expect<ArgumentException>(() => cursor.Advance(1, (time, action) => {}), "rewind guard", ref passed);
            var duplicate = new ReplayTimeline<float, string>();
            duplicate.AddFrame(0, 1); duplicate.AddFrame(0, 2); duplicate.Freeze();
            Check(duplicate.FrameCount == 1 && duplicate.Sample(0, Lerp) == 2, "duplicate timestamp", ref passed);
            var invalid = new ReplayTimeline<float, string>();
            invalid.AddFrame(0, 0); invalid.AddAction(1, "late");
            Expect<InvalidOperationException>(() => invalid.Freeze(), "event after final pose rejected", ref passed);
            Expect<ArgumentOutOfRangeException>(() => invalid.AddFrame(float.NaN, 0), "invalid time rejected", ref passed);
            var empty = new ReplayTimeline<float, string>();
            Expect<InvalidOperationException>(() => empty.Freeze(), "empty commit rejected", ref passed);
            return passed + "/14 timeline checks passed";
        }
        private static float Lerp(float a, float b, float t) { return a + (b - a) * t; }
        private static void Check(bool ok, string name, ref int passed)
        {
            if (!ok) throw new Exception("FAIL: " + name);
            passed++;
        }
        private static void Expect<T>(Action action, string name, ref int passed) where T : Exception
        {
            try { action(); } catch (T) { passed++; return; }
            throw new Exception("FAIL: " + name);
        }
    }
}
