using System.Collections.Generic;
using UnityEngine;

namespace EcoCarrito
{
    public class EcoSpawner : MonoBehaviour
    {
        public EcoGame game;
        public EcoWaste wastePrefab;
        public Transform container;
        readonly List<EcoWaste> active = new List<EcoWaste>();
        float timer;
        public void ResetSpawner()
        {
            foreach (var w in active) if (w) w.Cancel();
            active.Clear(); timer = .55f;
        }
        void Update()
        {
            if (!game || game.State != RoundState.Playing) return;
            timer -= Time.deltaTime;
            if (timer > 0) return;
            float ramp = Mathf.Clamp01(game.Elapsed / Mathf.Max(1, game.catalog.difficultyRampSeconds));
            float speed = Mathf.Lerp(game.catalog.startSpeed, game.catalog.finalSpeed, ramp);
            // Transit time decreases smoothly. >= .35s between waves provides room for two lane hops.
            timer = Mathf.Max(.35f, Mathf.Lerp(game.catalog.startInterval, game.catalog.finalInterval, ramp));
            int first = Random.Range(0, 3);
            Spawn(first, speed);
            // No wave occupies all three lanes; the third lane remains available for avoidance.
            if (game.Elapsed > 8 && Random.value < Mathf.Lerp(.15f, .6f, ramp))
                Spawn((first + Random.Range(1, 3)) % 3, speed);
            active.RemoveAll(w => !w);
        }
        void Spawn(int lane, float speed)
        {
            var c = game.catalog;
            WasteKind kind;
            float roll = Random.value;
            if (game.Elapsed > 6 && roll < c.hazardProbability) kind = WasteKind.Hazard;
            else if (Random.value < c.matchingProbability) kind = (WasteKind)lane;
            else kind = (WasteKind)((lane + Random.Range(1, 3)) % 3);
            var choices = new List<WasteDefinition>();
            foreach (var d in c.wastes) if (d != null && d.kind == kind && d.sprite) choices.Add(d);
            if (choices.Count == 0) return;
            EcoWaste w;
            if (wastePrefab)
            {
                w = Instantiate(wastePrefab, container);
                w.gameObject.SetActive(true);
            }
            else
            {
                var obj = new GameObject("Residuo"); obj.transform.SetParent(container, false);
                w = obj.AddComponent<EcoWaste>();
            }
            w.Initialize(game, choices[Random.Range(0, choices.Count)], lane, speed);
            active.Add(w);
        }
        public void Clear() { ResetSpawner(); }
    }
}
