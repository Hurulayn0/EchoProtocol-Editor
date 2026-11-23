using UnityEngine;

namespace EchoProtocol
{
    // Settings and campaign progress live in PlayerPrefs; exact Echo tapes use EchoSaveStore.
    public sealed class EchoPreferences
    {
        private readonly string prefix;
        private float defaultSensitivity = 1, defaultFov = 75;
        public float Sensitivity = 1, Volume = .7f, MusicVolume = .65f, FieldOfView = 75;
        public bool InvertLook, Completed;
        public bool SampleMemory, V2Initialized;
        public int ResumeLevel = -1, UnlockedLevel, ArchiveMask;
        public float[] BestSeconds = new float[3];
        public int[] BestLoops = new int[3];
        public int[] BestRanks = new int[4]; // 0=None, 1=C, 2=B, 3=A, 4=S (indices 0,1,2=Sectors; 3=Sample)

        public EchoPreferences(string keyPrefix = "EchoProtocol.v1.") { prefix = keyPrefix; }

        public void Load(float sensitivity = 1, float fov = 75)
        {
            defaultSensitivity = SafeFloat(sensitivity, .25f, 3, 1);
            defaultFov = SafeFloat(fov, 60, 100, 75);
            Sensitivity = SafeFloat(PlayerPrefs.GetFloat(prefix + "Sensitivity", defaultSensitivity), .25f, 3, defaultSensitivity);
            Volume = SafeFloat(PlayerPrefs.GetFloat(prefix + "Volume", .7f), 0, 1, .7f);
            MusicVolume = SafeFloat(PlayerPrefs.GetFloat(prefix + "MusicVolume", .65f), 0, 1, .65f);
            FieldOfView = SafeFloat(PlayerPrefs.GetFloat(prefix + "FieldOfView", defaultFov), 60, 100, defaultFov);
            ArchiveMask = PlayerPrefs.GetInt(prefix + "ArchiveMask", 0) & 7;
            SampleMemory = PlayerPrefs.GetInt(prefix + "SampleMemory", 0) == 1;
            V2Initialized = PlayerPrefs.GetInt(prefix + "V2Initialized", 0) == 1;
            InvertLook = PlayerPrefs.GetInt(prefix + "InvertLook", 0) == 1;
            Completed = PlayerPrefs.GetInt(prefix + "Completed", 0) == 1;
            int checkpoint = PlayerPrefs.GetInt(prefix + "ResumeLevel", -1);
            ResumeLevel = checkpoint >= 0 && checkpoint < EchoLevelDefinition.Campaign.Length ? checkpoint : -1;
            UnlockedLevel = Mathf.Clamp(PlayerPrefs.GetInt(prefix + "UnlockedLevel", Completed ? 2 : Mathf.Max(0, ResumeLevel)), 0, 2);
            for (int i = 0; i < 3; i++) {
                BestSeconds[i] = SafeFloat(PlayerPrefs.GetFloat(prefix + "BestSeconds" + i, 0), 0, 10000000, 0);
                BestLoops[i] = Mathf.Clamp(PlayerPrefs.GetInt(prefix + "BestLoops" + i, 0), 0, 1000000);
            }
            for (int i = 0; i < 4; i++) BestRanks[i] = Mathf.Clamp(PlayerPrefs.GetInt(prefix + "BestRank" + i, 0), 0, 4);
        }

        public void Save()
        {
            Sensitivity = SafeFloat(Sensitivity, .25f, 3, 1);
            Volume = SafeFloat(Volume, 0, 1, .7f);
            MusicVolume = SafeFloat(MusicVolume, 0, 1, .65f);
            FieldOfView = SafeFloat(FieldOfView, 60, 100, 75);
            ResumeLevel = ResumeLevel >= 0 && ResumeLevel < EchoLevelDefinition.Campaign.Length ? ResumeLevel : -1;
            PlayerPrefs.SetFloat(prefix + "Sensitivity", Sensitivity);
            PlayerPrefs.SetFloat(prefix + "Volume", Volume);
            PlayerPrefs.SetFloat(prefix + "MusicVolume", MusicVolume);
            PlayerPrefs.SetFloat(prefix + "FieldOfView", FieldOfView);
            PlayerPrefs.SetInt(prefix + "InvertLook", InvertLook ? 1 : 0);
            PlayerPrefs.SetInt(prefix + "ArchiveMask", ArchiveMask & 7);
            PlayerPrefs.SetInt(prefix + "SampleMemory", SampleMemory ? 1 : 0);
            PlayerPrefs.SetInt(prefix + "V2Initialized", V2Initialized ? 1 : 0);
            PlayerPrefs.SetInt(prefix + "Completed", Completed ? 1 : 0);
            PlayerPrefs.SetInt(prefix + "ResumeLevel", ResumeLevel);
            PlayerPrefs.SetInt(prefix + "UnlockedLevel", Mathf.Clamp(UnlockedLevel, 0, 2));
            for (int i = 0; i < 3; i++) {
                PlayerPrefs.SetFloat(prefix + "BestSeconds" + i, SafeFloat(BestSeconds[i], 0, 10000000, 0));
                PlayerPrefs.SetInt(prefix + "BestLoops" + i, Mathf.Clamp(BestLoops[i], 0, 1000000));
            }
            for (int i = 0; i < 4; i++) PlayerPrefs.SetInt(prefix + "BestRank" + i, Mathf.Clamp(BestRanks[i], 0, 4));
            PlayerPrefs.Save();
        }

        public void RecordRank(int slot, int rankValue)
        {
            if (slot < 0 || slot >= BestRanks.Length) return;
            BestRanks[slot] = Mathf.Max(BestRanks[slot], Mathf.Clamp(rankValue, 1, 4));
        }

        public static string RankLetter(int rankValue)
        {
            return rankValue >= 4 ? "S" : rankValue == 3 ? "A" : rankValue == 2 ? "B" : rankValue == 1 ? "C" : "-";
        }

        public void RecordClear(int level, float seconds, int loops)
        {
            if (level < 0 || level > 2 || float.IsNaN(seconds) || float.IsInfinity(seconds) || seconds <= 0 || loops < 0) return;
            UnlockedLevel = Mathf.Max(UnlockedLevel, Mathf.Min(2, level + 1));
            bool firstClear = BestSeconds[level] <= 0;
            if (firstClear || seconds < BestSeconds[level]) BestSeconds[level] = seconds;
            if (firstClear || loops < BestLoops[level]) BestLoops[level] = loops;
        }

        public void ResetSettings() { Sensitivity = defaultSensitivity; Volume = .7f; MusicVolume = .65f; FieldOfView = defaultFov; InvertLook = false; }

        private static float SafeFloat(float value, float min, float max, float fallback)
        {
            return float.IsNaN(value) || float.IsInfinity(value) ? fallback : Mathf.Clamp(value, min, max);
        }
    }
}
