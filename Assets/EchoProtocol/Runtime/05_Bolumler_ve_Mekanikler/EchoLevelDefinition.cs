using UnityEngine;

namespace EchoProtocol
{
    public sealed class EchoLevelDefinition
    {
        public readonly string Name;
        public readonly string Objective;
        public readonly Color Accent;
        public readonly Vector3[] Plates;
        public readonly Vector3[] Enemies;
        public readonly Vector3[] Covers;

        private EchoLevelDefinition(string name, string objective, Color accent,
            Vector3[] plates, Vector3[] enemies, Vector3[] covers)
        {
            Name = name; Objective = objective; Accent = accent;
            Plates = plates; Enemies = enemies; Covers = covers;
        }

        public static readonly EchoLevelDefinition[] Campaign =
        {
            new EchoLevelDefinition("01 / ARAŞTIRMA LABI", "Konsolu etkinleştir. Plakada bir Echo bırak. Kapıdan geç.",
                new Color(.05f, .75f, .85f), new[] { new Vector3(-4, 0, -2) },
                new Vector3[0], new[] { new Vector3(5.4f, .65f, 1) }),
            new EchoLevelDefinition("02 / BAKIM HATTI", "Sağ bakım hattından ilerle. Konsolu ve muhafızı Echo’ya devret.",
                new Color(1, .55f, .16f), new[] { new Vector3(4.8f, 0, -4.5f) },
                new[] { new Vector3(-1, .02f, 1) },
                new[] { new Vector3(2.6f, .65f, -.5f), new Vector3(-6.2f, .65f, 2) }),
            new EchoLevelDefinition("03 / REAKTÖR", "İki konsol, iki plaka. İki Echo bırak ve muhafızları temizle.",
                new Color(.65f, .35f, 1), new[] { new Vector3(-4, 0, -2), new Vector3(4, 0, -2) },
                new[] { new Vector3(-1, .02f, 1), new Vector3(2, .02f, 2.5f) },
                new[] { new Vector3(-2.1f, .65f, -4.3f), new Vector3(2.4f, .65f, -1.2f), new Vector3(-6.0f, .65f, -4.3f), new Vector3(-6.2f, .65f, -.5f), new Vector3(-6.2f, .65f, 2.5f), new Vector3(5.8f, .65f, 3) })
        };
    }
}
