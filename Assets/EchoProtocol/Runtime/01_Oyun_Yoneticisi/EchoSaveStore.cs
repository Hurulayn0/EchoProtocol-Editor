using System;
using System.IO;
using UnityEngine;

namespace EchoProtocol
{
    [Serializable] public sealed class EchoSavedFrame { public float Time; public EchoPose Pose; }
    [Serializable] public sealed class EchoSavedAction { public float Time; public EchoAction Action; }
    [Serializable] public sealed class EchoSavedTape
    {
        public EchoSavedFrame[] Frames;
        public EchoSavedAction[] Actions;
        public ReplayTimeline<EchoPose, EchoAction> Restore(int level, bool sample = false, int stage = 0)
        {
            if (Frames == null || Frames.Length < 2 || Frames.Length > 2048 || Actions == null || Actions.Length > 512)
                throw new InvalidDataException("Invalid tape size.");
            var tape = new ReplayTimeline<EchoPose, EchoAction>(); float previous = -1;
            foreach (var frame in Frames)
            {
                if (frame == null || !ValidTime(frame.Time) || frame.Time <= previous || !ValidPose(frame.Pose))
                    throw new InvalidDataException("Invalid recording frame.");
                if (previous == -1 && frame.Time != 0) throw new InvalidDataException("Tape must start at zero.");
                tape.AddFrame(frame.Time, frame.Pose); previous = frame.Time;
            }
            if (tape.Duration < .25f) throw new InvalidDataException("Recording is too short.");
            previous = -1;
            foreach (var item in Actions)
            {
                if (item == null || !ValidTime(item.Time) || item.Time < previous || item.Time > tape.Duration)
                    throw new InvalidDataException("Invalid recording event time.");
                var action = item.Action;
                if (sample && action.Kind == EchoActionKind.Device)
                {
                    int count = stage == 0 ? 1 : stage == 1 ? 2 : 3;
                    if (action.TargetId < 0 || action.TargetId >= count) throw new InvalidDataException("Invalid maintenance device.");
                }
                else if (!sample && action.Kind == EchoActionKind.Console)
                {
                    if (action.TargetId < 0 || action.TargetId >= EchoLevelDefinition.Campaign[level].Plates.Length)
                        throw new InvalidDataException("Invalid console id.");
                }
                else if (action.Kind == EchoActionKind.Fire)
                {
                    if (!Finite(action.Origin) || !Finite(action.Direction) || Mathf.Abs(action.Direction.magnitude - 1) > .01f || action.Origin.sqrMagnitude > 10000)
                        throw new InvalidDataException("Invalid shot ray.");
                }
                else if (!sample && action.Kind == EchoActionKind.Archive) { if (action.TargetId != level) throw new InvalidDataException("Invalid archive id."); }
                else throw new InvalidDataException("Unknown action kind.");
                tape.AddAction(item.Time, action); previous = item.Time;
            }
            tape.Freeze(); return tape;
        }
        private static bool ValidTime(float value) { return Finite(value) && value >= 0 && value <= EchoPrototypeGame.LoopLength; }
        private static bool Finite(float value) { return !float.IsNaN(value) && !float.IsInfinity(value); }
        private static bool Finite(Vector3 value) { return Finite(value.x) && Finite(value.y) && Finite(value.z); }
        private static bool ValidPose(EchoPose pose)
        {
            Quaternion q = pose.Rotation;
            float norm = q.x*q.x + q.y*q.y + q.z*q.z + q.w*q.w;
            return Finite(pose.Position) && Mathf.Abs(pose.Position.x) <= 8.5f && pose.Position.z >= -9.5f && pose.Position.z <= 15.5f &&
                pose.Position.y >= -.3f && pose.Position.y <= 4.5f && Finite(pose.Pitch) && Mathf.Abs(pose.Pitch) <= 80 && Finite(norm) && Mathf.Abs(norm - 1) < .02f;
        }
    }
    [Serializable] public sealed class EchoSessionSave
    {
        public int Version = 1, Level, Loops, SectorLoops;
        public int Layout, Stage, CompletedMask, Deaths;
        public bool Memory;
        public string[] FinalEvents;
        public float Elapsed, SectorElapsed;
        public EchoSavedTape[] Tapes;
        public void Validate()
        {
            if (FinalEvents != null)
            {
                if (FinalEvents.Length > 8 || (Version == 1 && FinalEvents.Length != 0)) throw new InvalidDataException("Invalid event summary.");
                foreach (string line in FinalEvents) if (line == null || line.Length > 200) throw new InvalidDataException("Invalid event text.");
            }
            bool legacy = Version == 1 && Layout == 0 && Stage == 0 && CompletedMask == 0 && Deaths == 0 && !Memory;
            bool sample = Version == 2 && Layout == 1 && Level == 1 && Stage >= 0 && Stage <= 2 &&
                (CompletedMask == (1 << Stage) - 1 || (Stage == 2 && CompletedMask == 7)) && Deaths >= 0 && Deaths <= 1000000;
            if ((!legacy && !sample) || Level < 0 || Level >= EchoLevelDefinition.Campaign.Length || Tapes == null || Tapes.Length > 2 ||
                Loops < 0 || SectorLoops < 0 || Loops > 1000000 || SectorLoops > Loops || !ValidSeconds(Elapsed) || !ValidSeconds(SectorElapsed) || SectorElapsed > Elapsed + .01f)
                throw new InvalidDataException("Invalid session save.");
            foreach (var tape in Tapes) { if (tape == null) throw new InvalidDataException("Missing tape."); tape.Restore(Level, sample, Stage); }
        }
        private static bool ValidSeconds(float value) { return !float.IsNaN(value) && !float.IsInfinity(value) && value >= 0 && value <= 10000000; }
    }
    // Keep the recording engine untouched. Serialize the exact captured frames/events alongside it.
    public sealed class EchoSaveStore
    {
        public readonly string Path;
        public EchoSaveStore(string path) { Path = path; }
        public bool ImportLegacyIfMissing(EchoSaveStore legacy)
        {
            EchoSessionSave present; bool recovered;
            if (TryRead(out present, out recovered)) return false;
            if (!legacy.TryRead(out present, out recovered) || present.Version != 1) return false;
            Write(present); return true;
        }
        public void Write(EchoSessionSave value)
        {
            value.Validate(); string json = JsonUtility.ToJson(value);
            if (json.Length > 1024 * 1024) throw new InvalidDataException("Save exceeds size limit.");
            Directory.CreateDirectory(System.IO.Path.GetDirectoryName(Path));
            string temporary = Path + ".tmp"; File.WriteAllText(temporary, json);
            if (File.Exists(Path)) File.Replace(temporary, Path, Read(Path) != null ? Path + ".bak" : null);
            else File.Move(temporary, Path);
        }
        public bool TryRead(out EchoSessionSave value, out bool recovered)
        {
            recovered = false; value = Read(Path);
            if (value != null) return true;
            value = Read(Path + ".bak"); recovered = value != null; return recovered;
        }
        private static EchoSessionSave Read(string path)
        {
            try
            {
                if (!File.Exists(path) || new FileInfo(path).Length > 1024 * 1024) return null;
                var data = JsonUtility.FromJson<EchoSessionSave>(File.ReadAllText(path));
                if (data == null) return null; data.Validate(); return data;
            }
            catch (Exception ex) when (ex is InvalidDataException || ex is IOException || ex is ArgumentException || ex is UnauthorizedAccessException) { return null; }
        }
        public void Clear()
        {
            foreach (string suffix in new[] { "", ".bak", ".tmp" }) if (File.Exists(Path + suffix)) File.Delete(Path + suffix);
        }
    }
}
