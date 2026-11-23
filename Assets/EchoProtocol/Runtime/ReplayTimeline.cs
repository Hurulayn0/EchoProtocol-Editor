using System;
using System.Collections.Generic;

namespace EchoProtocol
{
    // Engine-independent timeline. Events are dispatched once even after a long frame.
    public sealed class ReplayTimeline<TPose, TAction>
    {
        public struct Frame
        {
            public readonly float Time;
            public readonly TPose Pose;
            public Frame(float time, TPose pose) { Time = time; Pose = pose; }
        }
        public struct ActionEvent
        {
            public readonly float Time;
            public readonly TAction Action;
            public ActionEvent(float time, TAction action) { Time = time; Action = action; }
        }
        private readonly List<Frame> frames = new List<Frame>();
        private readonly List<ActionEvent> actions = new List<ActionEvent>();
        public int FrameCount { get { return frames.Count; } }
        public int ActionCount { get { return actions.Count; } }
        public float Duration { get { return frames.Count == 0 ? 0 : frames[frames.Count - 1].Time; } }
        public bool Frozen { get; private set; }

        public void AddFrame(float time, TPose pose)
        {
            CheckTime(time);
            if (Frozen) throw new InvalidOperationException("A committed recording is immutable.");
            if (frames.Count > 0 && time < Duration) throw new ArgumentException("Frames must be chronological.");
            if (frames.Count > 0 && time == Duration) frames[frames.Count - 1] = new Frame(time, pose);
            else frames.Add(new Frame(time, pose));
        }
        public void AddAction(float time, TAction action)
        {
            CheckTime(time);
            if (Frozen) throw new InvalidOperationException("A committed recording is immutable.");
            if (actions.Count > 0 && time < actions[actions.Count - 1].Time)
                throw new ArgumentException("Actions must be chronological.");
            actions.Add(new ActionEvent(time, action));
        }
        public void Freeze()
        {
            if (frames.Count == 0) throw new InvalidOperationException("Record an initial pose before committing.");
            if (actions.Count > 0 && actions[actions.Count - 1].Time > Duration)
                throw new InvalidOperationException("Record a final pose after the final action.");
            Frozen = true;
        }
        public TPose Sample(float time, Func<TPose, TPose, float, TPose> interpolate)
        {
            CheckTime(time);
            if (frames.Count == 0) throw new InvalidOperationException("Cannot sample an empty recording.");
            if (time <= frames[0].Time) return frames[0].Pose;
            if (time >= Duration) return frames[frames.Count - 1].Pose;
            int lo = 0, hi = frames.Count - 1;
            while (hi - lo > 1)
            {
                int mid = (lo + hi) / 2;
                if (frames[mid].Time <= time) lo = mid; else hi = mid;
            }
            float t = (time - frames[lo].Time) / (frames[hi].Time - frames[lo].Time);
            return interpolate(frames[lo].Pose, frames[hi].Pose, t);
        }
        public Cursor CreateCursor()
        {
            if (!Frozen) throw new InvalidOperationException("Commit before playback.");
            return new Cursor(this);
        }
        public sealed class Cursor
        {
            private readonly ReplayTimeline<TPose, TAction> timeline;
            private int next;
            private float previous = -1;
            internal Cursor(ReplayTimeline<TPose, TAction> timeline) { this.timeline = timeline; }
            public void Restart() { next = 0; previous = -1; }
            public void Advance(float time, Action<float, TAction> dispatch)
            {
                CheckTime(time);
                if (time < previous) throw new ArgumentException("Restart cursor before rewinding.");
                previous = time;
                while (next < timeline.actions.Count && timeline.actions[next].Time <= time)
                {
                    ActionEvent action = timeline.actions[next++];
                    dispatch(action.Time, action.Action);
                }
            }
        }
        private static void CheckTime(float time)
        {
            if (float.IsNaN(time) || float.IsInfinity(time) || time < 0)
                throw new ArgumentOutOfRangeException("time");
        }
    }
}
