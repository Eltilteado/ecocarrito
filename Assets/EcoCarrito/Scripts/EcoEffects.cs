using System.Collections.Generic;
using UnityEngine;

namespace EcoCarrito
{
    // Lightweight sprite effects: no imported particle package or extra shaders.
    public class EcoEffects : MonoBehaviour
    {
        public EcoGame game;
        public Camera gameCamera;
        public Transform background;
        public float shakeStrength = .08f;
        public float parallaxAmount = .12f;
        public int burstParticles = 12;
        class Spark { public SpriteRenderer sprite; public Vector3 velocity; public float life; public float total; }
        readonly List<Spark> sparks = new List<Spark>();
        Transform[] layers;
        Vector3[] origins;
        Vector3 cameraOrigin;
        float clock, shakeLeft;
        Sprite pixel;
        void Start()
        {
            pixel = Resources.Load<Sprite>("EcoCarrito/Art/UI/white");
            if (gameCamera) cameraOrigin = gameCamera.transform.localPosition;
            if (background)
            {
                layers = new Transform[background.childCount]; origins = new Vector3[layers.Length];
                for (int i = 0; i < layers.Length; i++) { layers[i] = background.GetChild(i); origins[i] = layers[i].localPosition; }
            }
        }
        public void Burst(Vector3 position, Color color, bool good)
        {
            if (!game || !pixel) return;
            if (!good) shakeLeft = .2f;
            for (int i = 0; i < burstParticles; i++)
            {
                var obj = new GameObject(good ? "Brillo_acierto" : "Chispa_error");
                obj.transform.SetParent(transform, false); obj.transform.position = position;
                var sprite = obj.AddComponent<SpriteRenderer>(); sprite.sprite = pixel;
                sprite.sharedMaterial = game.spriteMaterial; sprite.color = color; sprite.sortingOrder = 50;
                float size = Random.Range(.035f, .09f); obj.transform.localScale = Vector3.one * (size / pixel.bounds.size.x);
                float angle = Random.Range(0, Mathf.PI * 2), force = Random.Range(1, 3);
                float life = Random.Range(.25f, .55f);
                sparks.Add(new Spark { sprite = sprite, velocity = new Vector3(Mathf.Cos(angle), Mathf.Sin(angle), 0) * force, life = life, total = life });
            }
        }
        void Update()
        {
            if (!game || game.State == RoundState.Paused) return;
            float dt = Time.deltaTime; clock += dt;
            if (layers != null)
                for (int i = 0; i < layers.Length; i++)
                    if (layers[i]) layers[i].localPosition = origins[i] + Vector3.right * (Mathf.Sin(clock * .35f) * parallaxAmount * i / Mathf.Max(1, layers.Length - 1));
            for (int i = sparks.Count - 1; i >= 0; i--)
            {
                var s = sparks[i]; s.life -= dt;
                if (!s.sprite || s.life <= 0) { if (s.sprite) Destroy(s.sprite.gameObject); sparks.RemoveAt(i); continue; }
                s.velocity += Vector3.down * dt * 4;
                s.sprite.transform.position += s.velocity * dt;
                s.sprite.transform.Rotate(0, 0, dt * 140);
                var color = s.sprite.color; color.a = s.life / s.total; s.sprite.color = color;
            }
            shakeLeft = Mathf.Max(0, shakeLeft - dt);
            if (gameCamera) gameCamera.transform.localPosition = cameraOrigin + (shakeLeft > 0 ? new Vector3(Mathf.Sin(clock * 90), Mathf.Cos(clock * 110), 0) * shakeStrength * shakeLeft / .2f : Vector3.zero);
        }
        void OnDisable() { if (gameCamera) gameCamera.transform.localPosition = cameraOrigin; }
    }
}
