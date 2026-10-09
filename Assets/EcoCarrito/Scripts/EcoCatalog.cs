using System;
using UnityEngine;

namespace EcoCarrito
{
    public enum WasteKind { Plastic, Paper, Glass, Hazard }
    public enum RoundState { Menu, Playing, Paused, Finished, Intro, Countdown }

    [Serializable]
    public class WasteDefinition
    {
        public string label;
        public WasteKind kind;
        public Sprite sprite;
    }

    [CreateAssetMenu(menuName = "EcoCarrito/Catalogo", fileName = "EcoCatalog")]
    public class EcoCatalog : ScriptableObject
    {
        public WasteDefinition[] wastes;
        public Sprite[] collectorFrames;
        public Sprite[] backgroundLayers;
        public AudioClip collectSound;
        public AudioClip errorSound;
        public AudioClip startSound;
        public AudioClip musicClip;
        [Range(0, 1)] public float musicVolume = .24f;
        [Header("Legibilidad")]
        [Range(.65f, 1.4f)] public float wasteHeight = 1.1f;
        [Range(1.25f, 2)] public float collectorHeight = 1.65f;
        [Header("Presentacion")]
        [Min(1)] public float introSeconds = 6.5f;
        [Range(1, 5)] public float countdownSeconds = 3;
        [Header("Partida")]
        [Min(10)] public float roundSeconds = 60;
        [Min(1)] public int basePoints = 100;
        [Range(1, 100)] public int wrongDamage = 12;
        [Range(1, 100)] public int hazardDamage = 20;
        [Header("Ritmo")]
        [Min(.35f)] public float startInterval = .9f;
        [Min(.35f)] public float finalInterval = .38f;
        [Min(1)] public float startSpeed = 6;
        [Min(1)] public float finalSpeed = 10;
        [Min(1)] public float difficultyRampSeconds = 40;
        [Range(.2f, .9f)] public float matchingProbability = .6f;
        [Range(0, .2f)] public float hazardProbability = .08f;
        public static readonly string[] LaneNames = { "PLÁSTICO", "PAPEL / CARTÓN", "VIDRIO" };
        public static readonly float[] LaneY = { 2.65f, .15f, -2.35f };
        public static readonly Color[] LaneColors =
        {
            new Color(.18f, .82f, 1), new Color(1, .78f, .15f), new Color(.3f, .95f, .55f)
        };
        public static Color KindColor(WasteKind kind)
        { return kind == WasteKind.Hazard ? new Color(1, .28f, .32f) : LaneColors[Mathf.Clamp((int)kind, 0, 2)]; }
        public bool HasAssets
        {
            get
            {
                if (collectorFrames == null || collectorFrames.Length == 0 || !collectorFrames[0]) return false;
                if (wastes == null || wastes.Length == 0) return false;
                for (int k = 0; k < 4; k++)
                {
                    bool found = false;
                    foreach (var w in wastes) if (w != null && w.sprite && (int)w.kind == k) found = true;
                    if (!found) return false;
                }
                return true;
            }
        }
    }
}
