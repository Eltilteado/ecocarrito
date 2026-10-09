using UnityEngine;

namespace EcoCarrito
{
    public static class EcoAssets
    {
        public static readonly string[] WasteFiles = { "botella_plastica", "envase_leche", "envase_detergente", "caja_carton", "periodico", "bolsa_papel", "botella_vidrio_verde", "frasco_vidrio", "botella_vidrio_marron", "aerosol_peligroso" };
        public static readonly string[] WasteLabels = { "Botella plástica", "Envase de leche", "Detergente", "Cartón", "Periódico", "Bolsa de papel", "Vidrio verde", "Frasco de vidrio", "Vidrio marrón", "Aerosol peligroso" };
        public static readonly string[] CityFiles = { "Sky", "Buildings_4", "Buildings_3", "Buildings_2", "Buildings_1", "Lights" };
        public static Sprite LoadSprite(string path) { return Resources.Load<Sprite>("EcoCarrito/Art/" + path); }
        public static EcoCatalog LoadCatalog(EcoCatalog current = null)
        {
            var c = current ? current : Resources.Load<EcoCatalog>("EcoCarrito/EcoCatalog");
            if (!c) { c = ScriptableObject.CreateInstance<EcoCatalog>(); c.name = "EcoCatalog_recuperado"; }
            if (c.wastes == null || c.wastes.Length == 0)
            {
                c.wastes = new WasteDefinition[WasteFiles.Length];
                for (int i = 0; i < c.wastes.Length; i++)
                    c.wastes[i] = new WasteDefinition { label = WasteLabels[i], kind = i == 9 ? WasteKind.Hazard : (WasteKind)(i / 3), sprite = LoadSprite("Waste/" + WasteFiles[i]) };
            }
            else
                for (int i = 0; i < c.wastes.Length && i < WasteFiles.Length; i++)
                    if (c.wastes[i] != null && !c.wastes[i].sprite) c.wastes[i].sprite = LoadSprite("Waste/" + WasteFiles[i]);
            if (c.collectorFrames == null || c.collectorFrames.Length == 0) c.collectorFrames = new Sprite[6];
            for (int i = 0; i < c.collectorFrames.Length; i++)
                if (!c.collectorFrames[i]) c.collectorFrames[i] = LoadSprite("Collector/contenedor_2_frame_" + (i % 6));
            if (c.backgroundLayers == null || c.backgroundLayers.Length == 0) c.backgroundLayers = new Sprite[CityFiles.Length];
            for (int i = 0; i < c.backgroundLayers.Length && i < CityFiles.Length; i++)
                if (!c.backgroundLayers[i]) c.backgroundLayers[i] = LoadSprite("City/" + CityFiles[i]);
            if (!c.collectSound) c.collectSound = Resources.Load<AudioClip>("EcoCarrito/Art/Audio/confirmation_001");
            if (!c.errorSound) c.errorSound = Resources.Load<AudioClip>("EcoCarrito/Art/Audio/error_001");
            if (!c.startSound) c.startSound = Resources.Load<AudioClip>("EcoCarrito/Art/Audio/click_001");
            if (!c.musicClip) c.musicClip = Resources.Load<AudioClip>("EcoCarrito/Art/Audio/Beyond_the_Final_Limit");
            return c;
        }
        public static Shader SpriteShader()
        {
            Shader shader = null;
            if (UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null)
            {
                shader = Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default");
                if (!shader) shader = Shader.Find("Universal Render Pipeline/Unlit");
            }
            if (!shader) shader = Shader.Find("Sprites/Default");
            return shader;
        }
        public static Material LoadMaterial(Material current = null)
        {
            var material = current ? current : Resources.Load<Material>("EcoCarrito/SpriteMaterial");
            var shader = SpriteShader();
            if (!shader) { Debug.LogError("EcoCarrito: no se encuentra un shader compatible de sprites."); return material; }
            if (!material || material.shader != shader) material = new Material(shader) { name = "EcoCarrito_Sprites_Runtime" };
            return material;
        }
    }
}
