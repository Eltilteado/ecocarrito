using UnityEngine;

namespace EcoCarrito
{
    public class EcoHud : MonoBehaviour
    {
        public EcoGame game;
        public Camera gameCamera;
        GUIStyle normal, small, centered, display;
        Texture2D cover;
        readonly Color ink = new Color(.065f, .025f, .14f, .98f);
        readonly Color cream = new Color(1, .96f, .82f);
        readonly Color purple = new Color(.3f, .1f, .57f);
        void Prepare()
        {
            if (normal != null) return;
            var font = Resources.Load<Font>("EcoCarrito/Art/Fonts/ArcadeBold");
            normal = new GUIStyle(GUI.skin.label) { fontSize = 29, fontStyle = FontStyle.Bold, alignment = TextAnchor.MiddleLeft };
            if (font) normal.font = font;
            normal.normal.textColor = cream;
            small = new GUIStyle(normal) { fontSize = 21 };
            centered = new GUIStyle(normal) { alignment = TextAnchor.MiddleCenter };
            display = new GUIStyle(centered) { fontSize = 48 };
            cover = Resources.Load<Texture2D>("EcoCarrito/Art/UI/cover_arcade");
        }
        void Fill(Rect r, Color c)
        { var old = GUI.color; GUI.color = c; GUI.DrawTexture(r, Texture2D.whiteTexture); GUI.color = old; }
        void Frame(Rect r, Color accent, float width = 4)
        {
            Fill(new Rect(r.x + 6, r.y + 8, r.width, r.height), ink);
            Fill(r, cream);
            Fill(new Rect(r.x + 2, r.y + 2, r.width - 4, r.height - 4), accent);
            Fill(new Rect(r.x + width + 2, r.y + width + 2, r.width - 2 * width - 4, r.height - 2 * width - 4), ink);
        }
        void Text(Rect r, string value, GUIStyle style, Color color, bool outline = false)
        {
            var old = style.normal.textColor;
            if (outline)
            {
                style.normal.textColor = ink;
                for (int x = -2; x <= 2; x += 2)
                    for (int y = -2; y <= 2; y += 2)
                        if (x != 0 || y != 0) GUI.Label(new Rect(r.x + x, r.y + y, r.width, r.height), value, style);
            }
            style.normal.textColor = color; GUI.Label(r, value, style); style.normal.textColor = old;
        }
        bool Button(Rect r, string label, Color color, int size = 29)
        {
            bool hover = r.Contains(Event.current.mousePosition);
            Color fill = hover ? Color.Lerp(color, Color.white, .2f) : color;
            Fill(new Rect(r.x + 6, r.y + 8, r.width, r.height), ink);
            Fill(r, ink); Fill(new Rect(r.x + 3, r.y + 3, r.width - 6, r.height - 6), cream);
            Fill(new Rect(r.x + 6, r.y + 6, r.width - 12, r.height - 12), fill);
            Fill(new Rect(r.x + 8, r.y + 8, r.width - 16, 8), new Color(1, 1, 1, .22f));
            var style = new GUIStyle(centered) { fontSize = size };
            Text(r, label, style, ink);
            return GUI.Button(r, GUIContent.none, GUIStyle.none);
        }
        void Sprite(Sprite sprite, Rect box)
        {
            if (!sprite || !sprite.texture) return;
            var source = sprite.textureRect; float ratio = source.width / source.height;
            float h = Mathf.Min(box.height, box.width / ratio), w = h * ratio;
            GUI.DrawTextureWithTexCoords(new Rect(box.center.x - w / 2, box.center.y - h / 2, w, h), sprite.texture,
                new Rect(source.x / sprite.texture.width, source.y / sprite.texture.height, source.width / sprite.texture.width, source.height / sprite.texture.height));
        }
        void Cover(bool intro)
        {
            Fill(new Rect(0, 0, 1600, 900), purple);
            if (cover)
            {
                float zoom = intro ? Mathf.Lerp(1.04f, 1, Mathf.Clamp01(game.PresentationTime / 3)) : 1;
                GUI.DrawTexture(new Rect((1600 - 1600 * zoom) / 2, (900 - 900 * zoom) / 2, 1600 * zoom, 900 * zoom), cover, ScaleMode.StretchToFill);
            }
            else Text(new Rect(150, 80, 1300, 150), "ECO CARRITO", display, EcoCatalog.LaneColors[2], true);
            Fill(new Rect(0, 708, 1600, 192), new Color(.08f, .025f, .17f, .95f));
            Fill(new Rect(0, 708, 1600, 5), EcoCatalog.LaneColors[2]);
            for (int i = 0; i < 3; i++)
            {
                var r = new Rect(25 + i * 300, 740, 280, 118); var color = EcoCatalog.LaneColors[i];
                Frame(r, color);
                int entry = i * 3;
                if (game.catalog.wastes.Length > entry) Sprite(game.catalog.wastes[entry].sprite, new Rect(r.x + 13, r.y + 14, 75, 84));
                var style = new GUIStyle(small) { fontSize = 23, alignment = TextAnchor.MiddleCenter };
                Text(new Rect(r.x + 92, r.y + 10, 175, 91), i == 1 ? "PAPEL /\nCARTÓN" : EcoCatalog.LaneNames[i], style, color, true);
            }
            if (intro)
            {
                Text(new Rect(930, 740, 635, 46), "RECICLA. ESQUIVA. SUPERA TU RÉCORD.", new GUIStyle(centered) { fontSize = 23 }, cream, true);
                Fill(new Rect(965, 800, 550, 7), purple);
                Fill(new Rect(965, 800, 550 * Mathf.Clamp01(game.PresentationTime / Mathf.Max(1, game.catalog.introSeconds)), 7), EcoCatalog.LaneColors[2]);
                if (Button(new Rect(1105, 825, 270, 49), "SALTAR  [ENTER]", EcoCatalog.LaneColors[0], 20)) game.SkipIntro();
            }
            else
            {
                if (Button(new Rect(960, 742, 600, 84), "¡JUGAR!  [ENTER]", EcoCatalog.LaneColors[1], 38)) game.BeginRound();
                Text(new Rect(960, 836, 600, 42), "↑ / ↓   ·   60 SEGUNDOS   ·   COMBO x4", small, cream);
            }
            Text(new Rect(30, 870, 900, 25), "EL BORDE INDICA EL TIPO DEL RESIDUO. EL CARRIL INDICA QUÉ RECOGER.", new GUIStyle(small) { fontSize = 15 }, cream);
            MusicButton(new Rect(1390, 24, 185, 46));
        }
        void MusicButton(Rect r)
        {
            if (game.music && Button(r, game.music.MusicEnabled ? "MÚSICA: ON" : "MÚSICA: OFF", EcoCatalog.LaneColors[0], 18)) game.music.Toggle();
        }
        void ScoreTile(Rect r, string label, string value, Color color)
        {
            Frame(r, color, 2);
            Text(new Rect(r.x + 14, r.y + 5, r.width - 28, 26), label, small, color);
            Text(new Rect(r.x + 14, r.y + 33, r.width - 28, 43), value, normal, cream, true);
        }
        void OnGUI()
        {
            if (!game || !game.catalog || !gameCamera || !game.player) return;
            Prepare(); var oldMatrix = GUI.matrix; var oldColor = GUI.color;
            var viewport = gameCamera.pixelRect;
            GUI.matrix = Matrix4x4.TRS(new Vector3(viewport.x, Screen.height - viewport.yMax, 0), Quaternion.identity, new Vector3(viewport.width / 1600, viewport.height / 900, 1));
            if (game.State == RoundState.Intro || game.State == RoundState.Menu) Cover(game.State == RoundState.Intro);
            else
            {
                Fill(new Rect(0, 0, 1600, 106), ink);
                Text(new Rect(20, 9, 260, 55), "ECO CARRITO", new GUIStyle(normal) { fontSize = 31 }, EcoCatalog.LaneColors[2], true);
                Text(new Rect(20, 59, 260, 27), "RECICLA CONTRARRELOJ", new GUIStyle(small) { fontSize = 17 }, cream);
                ScoreTile(new Rect(290, 9, 210, 80), "PUNTOS", game.Score.ToString("00000"), EcoCatalog.LaneColors[1]);
                ScoreTile(new Rect(515, 9, 155, 80), "COMBO", "x" + game.Multiplier, EcoCatalog.LaneColors[2]);
                Frame(new Rect(685, 9, 305, 80), EcoCatalog.LaneColors[2], 2);
                Text(new Rect(699, 15, 280, 31), "LIMPIEZA " + game.Cleanliness + "%", small, cream);
                Fill(new Rect(699, 58, 276, 16), purple);
                Fill(new Rect(699, 58, 276 * game.Cleanliness / 100f, 16), game.Cleanliness < 30 ? EcoCatalog.KindColor(WasteKind.Hazard) : EcoCatalog.LaneColors[2]);
                int total = Mathf.CeilToInt(game.Remaining);
                ScoreTile(new Rect(1005, 9, 170, 80), "TIEMPO", (total / 60).ToString("00") + ":" + (total % 60).ToString("00"), EcoCatalog.LaneColors[0]);
                MusicButton(new Rect(1195, 15, 195, 67));
                if (Button(new Rect(1410, 15, 170, 67), "PAUSA", EcoCatalog.LaneColors[1], 24)) game.TogglePause();
                for (int lane = 0; lane < 3; lane++)
                {
                    float y = (1 - gameCamera.WorldToViewportPoint(new Vector3(0, EcoCatalog.LaneY[lane], 0)).y) * 900;
                    var color = EcoCatalog.LaneColors[lane]; var r = new Rect(12, y - 80, 165, 160);
                    Frame(r, color, lane == game.player.Lane ? 7 : 3);
                    var style = new GUIStyle(centered) { fontSize = 24 };
                    Text(new Rect(22, y - 66, 145, 88), lane == 1 ? "PAPEL /\nCARTÓN" : EcoCatalog.LaneNames[lane], style, color, true);
                    Text(new Rect(22, y + 35, 145, 37), lane == game.player.Lane ? "TU CARRIL" : "↑ / ↓", new GUIStyle(centered) { fontSize = 18 }, cream);
                }
                Fill(new Rect(0, 785, 1600, 115), ink);
                Fill(new Rect(0, 785, 1600, 4), EcoCatalog.LaneColors[0]);
                Text(new Rect(25, 801, 1100, 43), "↑ / ↓ MOVER    ·    RECOGIDA AUTOMÁTICA    ·    ESC PAUSA", normal, cream, true);
                Text(new Rect(25, 851, 1170, 35), "Borde azul: plástico   ·   amarillo: papel   ·   verde: vidrio   ·   ROJO: ¡ESQUIVA!", small, cream);
                Text(new Rect(1250, 843, 330, 40), "RÉCORD " + game.Best, small, EcoCatalog.LaneColors[1], true);
                if (Time.time < game.MessageUntil)
                {
                    Frame(new Rect(465, 118, 680, 62), game.MessageColor, 3);
                    Text(new Rect(473, 120, 664, 58), game.Message, centered, game.MessageColor, true);
                }
                if (game.State == RoundState.Countdown) Countdown();
                else if (game.State == RoundState.Paused || game.State == RoundState.Finished) Overlay();
            }
            GUI.color = oldColor; GUI.matrix = oldMatrix;
        }
        void Countdown()
        {
            Fill(new Rect(180, 110, 1420, 675), new Color(.06f, .02f, .15f, .28f));
            float beat = game.CountdownLeft - Mathf.Floor(game.CountdownLeft);
            Text(new Rect(500, 230, 600, 220), Mathf.CeilToInt(game.CountdownLeft).ToString(), new GUIStyle(display) { fontSize = (int)Mathf.Lerp(130, 165, beat) }, EcoCatalog.LaneColors[1], true);
            Text(new Rect(350, 465, 900, 65), "¡A RECICLAR!", display, cream, true);
        }
        void Overlay()
        {
            Fill(new Rect(0, 106, 1600, 679), new Color(.04f, .015f, .1f, .62f));
            Frame(new Rect(350, 158, 900, 572), game.State == RoundState.Paused ? EcoCatalog.LaneColors[0] : EcoCatalog.LaneColors[1], 7);
            bool paused = game.State == RoundState.Paused;
            Text(new Rect(385, 190, 830, 91), paused ? "PAUSA" : (game.Cleanliness <= 0 ? "¡INTÉNTALO OTRA VEZ!" : "¡RONDA COMPLETADA!"), new GUIStyle(display) { fontSize = paused ? 56 : 42 }, EcoCatalog.LaneColors[1], true);
            Text(new Rect(410, 285, 780, 49), paused ? "RESPIRA. TU COMBO TE ESPERA." : game.EndReason, centered, cream);
            if (!paused)
            {
                Text(new Rect(410, 347, 780, 90), game.Score.ToString("00000") + " PUNTOS", new GUIStyle(display) { fontSize = 49 }, EcoCatalog.LaneColors[2], true);
                Text(new Rect(410, 439, 780, 42), "CLASIFICADOS: " + game.Collected + "    ·    RÉCORD: " + game.Best, new GUIStyle(centered) { fontSize = 23 }, cream);
            }
            if (Button(new Rect(485, 514, 630, 79), paused ? "CONTINUAR" : "OTRA RONDA  [ENTER]", EcoCatalog.LaneColors[2], 29)) { if (paused) game.TogglePause(); else game.BeginRound(); }
            if (Button(new Rect(565, 620, 470, 65), "VOLVER A PORTADA", EcoCatalog.LaneColors[0], 23)) game.BackToMenu();
        }
    }
}
