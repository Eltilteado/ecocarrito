using UnityEngine;

namespace EcoCarrito
{
    [DisallowMultipleComponent]
    public class EcoWaste : MonoBehaviour
    {
        public SpriteRenderer visual;
        public WasteKind Kind { get; private set; }
        public int Lane { get; private set; }
        EcoGame game;
        float speed;
        bool resolved;
        float phase;
        float animationClock;

        public void Initialize(EcoGame owner, WasteDefinition definition, int lane, float moveSpeed)
        {
            game = owner; Kind = definition.kind; Lane = lane; speed = moveSpeed; resolved = false;
            phase = Random.value * 6.28f;
            animationClock = 0;
            if (!visual) visual = GetComponent<SpriteRenderer>();
            if (!visual) visual = gameObject.AddComponent<SpriteRenderer>();
            visual.sharedMaterial = owner.spriteMaterial;
            visual.sprite = definition.sprite;
            visual.sortingOrder = 20;
            visual.color = Color.white;
            visual.transform.localScale = Vector3.one * (owner.catalog.wasteHeight / definition.sprite.bounds.size.y);
            CreateFrame();
            name = definition.label;
            transform.position = new Vector3(9.5f, EcoCatalog.LaneY[lane], 0);
        }
        void CreateFrame()
        {
            var old = transform.Find("Borde_tipo"); if (old) Destroy(old.gameObject);
            var pixel = EcoAssets.LoadSprite("UI/white"); if (!pixel) return;
            var frame = new GameObject("Borde_tipo"); frame.transform.SetParent(transform, false);
            float size = game.catalog.wasteHeight + .3f;
            FrameLayer(frame.transform, pixel, "Sombra", size + .12f, new Color(.04f, .02f, .1f), 15, new Vector3(.04f, -.05f, 0));
            FrameLayer(frame.transform, pixel, "Contorno_claro", size + .05f, new Color(1, .96f, .82f), 16, Vector3.zero);
            FrameLayer(frame.transform, pixel, "Color_categoria", size, EcoCatalog.KindColor(Kind), 17, Vector3.zero);
            FrameLayer(frame.transform, pixel, "Interior", size - .16f, new Color(.1f, .06f, .2f), 18, Vector3.zero);
        }
        void FrameLayer(Transform parent, Sprite sprite, string label, float size, Color color, int order, Vector3 position)
        {
            var obj = new GameObject(label); obj.transform.SetParent(parent, false); obj.transform.localPosition = position;
            obj.transform.localScale = Vector3.one * (size / sprite.bounds.size.x);
            var sr = obj.AddComponent<SpriteRenderer>(); sr.sprite = sprite; sr.sharedMaterial = game.spriteMaterial; sr.color = color; sr.sortingOrder = order;
        }
        void Update()
        {
            if (!game || game.State != RoundState.Playing || resolved) return;
            float previous = transform.position.x;
            var p = transform.position; p.x -= speed * Time.deltaTime; transform.position = p;
            animationClock += Time.deltaTime;
            if (visual && visual.transform != transform)
            {
                visual.transform.localRotation = Quaternion.Euler(0, 0, Mathf.Sin(animationClock * 4 + phase) * 9);
                visual.transform.localPosition = new Vector3(0, Mathf.Sin(animationClock * 6 + phase) * .06f, 0);
                if (Kind == WasteKind.Hazard) visual.color = Color.Lerp(Color.white, new Color(1, .5f, .45f), (Mathf.Sin(animationClock * 9) + 1) * .25f);
            }
            float catchX = game.player.transform.position.x;
            // Swept crossing prevents tunneling when framerate is low.
            if (previous > catchX && p.x <= catchX && game.player.InLane(Lane))
            {
                resolved = true; game.Resolve(this); Destroy(gameObject); return;
            }
            if (p.x < -10)
            {
                resolved = true;
                game.RegisterPassed();
                Destroy(gameObject);
            }
        }
        public void Cancel() { resolved = true; Destroy(gameObject); }
    }
}
