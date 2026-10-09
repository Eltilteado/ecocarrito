using UnityEngine;

namespace EcoCarrito
{
    [DisallowMultipleComponent]
    public class EcoPlayer : MonoBehaviour
    {
        public EcoGame game;
        public SpriteRenderer body;
        public SpriteRenderer highlight;
        public float switchSeconds = .11f;
        public int Lane { get; private set; } = 1;
        float hitUntil;
        float positiveUntil;
        public float framesPerSecond = 12;
        public float bobAmount = .025f;
        float animationClock;
        float feedbackDuration;

        public void ResetPlayer()
        {
            Lane = 1;
            transform.localPosition = new Vector3(-5.95f, EcoCatalog.LaneY[Lane], 0);
            hitUntil = positiveUntil = 0;
            animationClock = 0;
            if (body) { body.color = Color.white; body.transform.localPosition = Vector3.zero; }
            if (body && game && game.catalog && game.catalog.collectorFrames != null && game.catalog.collectorFrames.Length > 0 && game.catalog.collectorFrames[0])
                body.transform.localScale = Vector3.one * (game.catalog.collectorHeight / game.catalog.collectorFrames[0].bounds.size.y);
            if (highlight && game && game.catalog)
                highlight.transform.localPosition = new Vector3(0, -game.catalog.collectorHeight * .5f - .15f, 0);
            UpdateColor();
        }
        void Update()
        {
            if (!game || !game.catalog || !game.catalog.HasAssets || game.State == RoundState.Paused || game.State == RoundState.Finished) return;
            if (game.State == RoundState.Playing)
            {
                if (EcoInput.Up()) ChangeLane(-1);
                else if (EcoInput.Down()) ChangeLane(1);
            }
            animationClock += Time.deltaTime;
            var p = transform.localPosition;
            p.y = Mathf.MoveTowards(p.y, EcoCatalog.LaneY[Lane], 2.5f / Mathf.Max(.05f, switchSeconds) * Time.deltaTime);
            transform.localPosition = p;
            if (body)
            {
                body.color = Time.time < hitUntil ? new Color(1, .4f, .35f) : Color.white;
                var sprites = game.catalog.collectorFrames;
                int frame = 0;
                if (Time.time < positiveUntil && sprites.Length > 1)
                {
                    float elapsed = feedbackDuration - (positiveUntil - Time.time);
                    frame = Mathf.RoundToInt(Mathf.PingPong(elapsed * Mathf.Max(1, framesPerSecond) * 2, sprites.Length - 1));
                }
                body.sprite = sprites[Mathf.Clamp(frame, 0, sprites.Length - 1)];
                bool moving = Mathf.Abs(p.y - EcoCatalog.LaneY[Lane]) > .02f;
                float bounce = Mathf.Sin(animationClock * (moving ? 32 : 9)) * bobAmount * (moving ? 3 : 1);
                body.transform.localPosition = new Vector3(0, bounce, 0);
                body.transform.localRotation = Quaternion.Euler(0, 0, moving ? Mathf.Sin(animationClock * 24) * 5 : 0);
            }
            UpdateColor();
        }
        public void ChangeLane(int delta)
        {
            if (!game || game.State != RoundState.Playing) return;
            Lane = Mathf.Clamp(Lane + delta, 0, 2);
        }
        public bool InLane(int lane)
        {
            return lane == Lane && Mathf.Abs(transform.localPosition.y - EcoCatalog.LaneY[lane]) < .4f;
        }
        public void Feedback(bool good)
        {
            if (good)
            {
                feedbackDuration = Mathf.Max(.15f, (game.catalog.collectorFrames.Length - 1) / Mathf.Max(1, framesPerSecond));
                positiveUntil = Time.time + feedbackDuration;
            }
            else hitUntil = Time.time + .2f;
        }
        void UpdateColor()
        {
            if (highlight) highlight.color = EcoCatalog.LaneColors[Lane];
        }
    }
}
