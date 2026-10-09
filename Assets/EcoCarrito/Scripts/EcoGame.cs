using UnityEngine;

namespace EcoCarrito
{
    public class EcoGame : MonoBehaviour
    {
        public EcoCatalog catalog;
        public EcoPlayer player;
        public EcoSpawner spawner;
        public AudioSource audioSource;
        public Material spriteMaterial;
        public EcoEffects effects;
        public EcoMusic music;
        public float PresentationTime { get; private set; }
        public float CountdownLeft { get; private set; }
        int lastBeat;
        public RoundState State { get; private set; } = RoundState.Menu;
        public int Score { get; private set; }
        public int Combo { get; private set; }
        public int Cleanliness { get; private set; } = 100;
        public int Collected { get; private set; }
        public int Passed { get; private set; }
        public float Remaining { get; private set; }
        public float Elapsed { get { return catalog ? catalog.roundSeconds - Remaining : 0; } }
        public int Multiplier { get { return 1 + Mathf.Min(3, Combo / 4); } }
        public int Best { get; private set; }
        public string Message { get; private set; } = "";
        public float MessageUntil { get; private set; }
        public Color MessageColor { get; private set; } = Color.white;
        public string EndReason { get; private set; } = "";

        void Start()
        {
            catalog = EcoAssets.LoadCatalog(catalog);
            if (!player) player = transform.root.GetComponentInChildren<EcoPlayer>(true);
            if (!spawner) spawner = transform.root.GetComponentInChildren<EcoSpawner>(true);
            Best = PlayerPrefs.GetInt("EcoCarritoBest", 0);
            if (!catalog || !catalog.HasAssets || !player || !spawner)
            {
                Debug.LogError("EcoCarrito: falta el catálogo o sus sprites. Usa EcoCarrito > Armar escena completa.");
                enabled = false; return;
            }
            Remaining = catalog.roundSeconds;
            player.ResetPlayer();
            State = RoundState.Intro; PresentationTime = 0;
        }
        void Update()
        {
            if (State == RoundState.Intro)
            {
                PresentationTime += Time.deltaTime;
                if (PresentationTime >= catalog.introSeconds || EcoInput.Accept()) SkipIntro();
                return;
            }
            if (State == RoundState.Countdown)
            {
                CountdownLeft = Mathf.Max(0, CountdownLeft - Time.deltaTime);
                int beat = Mathf.CeilToInt(CountdownLeft);
                if (beat != lastBeat) { lastBeat = beat; Play(catalog.startSound); }
                if (CountdownLeft <= 0) State = RoundState.Playing;
                return;
            }
            if (EcoInput.Pause())
            {
                if (State == RoundState.Playing) State = RoundState.Paused;
                else if (State == RoundState.Paused) State = RoundState.Playing;
            }
            if (EcoInput.Accept())
            {
                if (State == RoundState.Menu || State == RoundState.Finished) BeginRound();
                else if (State == RoundState.Paused) State = RoundState.Playing;
            }
            if (State != RoundState.Playing) return;
            Remaining = Mathf.Max(0, Remaining - Time.deltaTime);
            if (Remaining <= 0) Finish("Tiempo completado");
        }
        public void BeginRound()
        {
            if (!catalog || !catalog.HasAssets || !player || !spawner) return;
            spawner.ResetSpawner(); player.ResetPlayer();
            Score = Combo = Collected = Passed = 0; Cleanliness = 100;
            Remaining = catalog.roundSeconds; Message = ""; EndReason = "";
            CountdownLeft = catalog.countdownSeconds; lastBeat = Mathf.CeilToInt(CountdownLeft);
            State = RoundState.Countdown; Play(catalog.startSound);
        }
        public void SkipIntro() { State = RoundState.Menu; }
        public void Resolve(EcoWaste waste)
        {
            if (State != RoundState.Playing) return;
            bool good = waste.Kind != WasteKind.Hazard && (int)waste.Kind == waste.Lane;
            player.Feedback(good);
            if (effects) effects.Burst(player.transform.position, good ? EcoCatalog.LaneColors[waste.Lane] : new Color(1, .35f, .3f), good);
            if (good)
            {
                Combo++; Collected++;
                int points = catalog.basePoints * Multiplier; Score += points;
                Notify("+" + points + "   COMBO x" + Multiplier, EcoCatalog.LaneColors[waste.Lane]);
                Play(catalog.collectSound);
            }
            else
            {
                Combo = 0;
                int damage = waste.Kind == WasteKind.Hazard ? catalog.hazardDamage : catalog.wrongDamage;
                Cleanliness = Mathf.Max(0, Cleanliness - damage);
                Notify(waste.Kind == WasteKind.Hazard ? "¡RESIDUO PELIGROSO!" : "¡TIPO INCORRECTO!", new Color(1, .4f, .35f));
                Play(catalog.errorSound);
                if (Cleanliness == 0) Finish("Limpieza agotada");
            }
        }
        public void RegisterPassed() { if (State == RoundState.Playing) Passed++; }
        public void TogglePause()
        {
            if (State == RoundState.Playing) State = RoundState.Paused;
            else if (State == RoundState.Paused) State = RoundState.Playing;
        }
        public void BackToMenu()
        {
            State = RoundState.Menu;
            if (spawner) spawner.Clear();
            if (player) player.ResetPlayer();
        }
        void Finish(string reason)
        {
            State = RoundState.Finished; EndReason = reason;
            if (Score > Best) { Best = Score; PlayerPrefs.SetInt("EcoCarritoBest", Best); PlayerPrefs.Save(); }
        }
        void Notify(string message, Color color)
        {
            Message = message; MessageColor = color; MessageUntil = Time.time + .7f;
        }
        void Play(AudioClip clip) { if (audioSource && clip) audioSource.PlayOneShot(clip, .4f); }
    }
}
