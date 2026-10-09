using UnityEngine;

namespace EcoCarrito
{
    public class EcoMusic : MonoBehaviour
    {
        public EcoGame game;
        AudioSource source;
        public bool MusicEnabled { get; private set; } = true;
        void Start()
        {
            source = GetComponent<AudioSource>();
            if (!source) source = gameObject.AddComponent<AudioSource>();
            source.playOnAwake = false; source.loop = true; source.spatialBlend = 0;
            MusicEnabled = PlayerPrefs.GetInt("EcoCarritoMusic", 1) == 1;
            var catalog = game ? game.catalog : EcoAssets.LoadCatalog();
            source.clip = catalog ? catalog.musicClip : null;
            source.volume = 0;
            if (source.clip) source.Play();
        }
        void Update()
        {
            if (!source || !game || !game.catalog) return;
            float target = MusicEnabled ? game.catalog.musicVolume : 0;
            if (game.State == RoundState.Paused) target *= .25f;
            source.volume = Mathf.MoveTowards(source.volume, target, Time.unscaledDeltaTime * .7f);
        }
        public void Toggle()
        {
            MusicEnabled = !MusicEnabled;
            PlayerPrefs.SetInt("EcoCarritoMusic", MusicEnabled ? 1 : 0);
            PlayerPrefs.Save();
        }
    }
}
