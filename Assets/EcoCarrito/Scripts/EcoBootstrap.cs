using UnityEngine;

namespace EcoCarrito
{
    // The supplied scene is ready for Play; this builds its named objects on demand.
    [DefaultExecutionOrder(-1000)]
    public class EcoBootstrap : MonoBehaviour
    {
        public EcoCatalog catalog;
        public Material spriteMaterial;
        public EcoWaste wastePrefab;
        public EcoPlayer playerPrefab;
        public bool buildOnAwake = true;

        void Awake() { if (buildOnAwake) BuildWorld(); }

        public EcoGame BuildWorld()
        {
            catalog = EcoAssets.LoadCatalog(catalog);
            spriteMaterial = EcoAssets.LoadMaterial(spriteMaterial);
            var existing = transform.Find("MundoEcoCarrito");
            if (existing)
            {
                var readyGame = existing.GetComponentInChildren<EcoGame>();
                // Editor samples are regular editable sprites, never live game waste.
                var preview = existing.Find("Muestras_residuos_EDITOR");
                if (preview && Application.isPlaying) preview.gameObject.SetActive(false);
                if (readyGame)
                {
                    readyGame.catalog = catalog;
                    readyGame.spriteMaterial = spriteMaterial;
                    readyGame.player = existing.GetComponentInChildren<EcoPlayer>(true);
                    readyGame.spawner = existing.GetComponentInChildren<EcoSpawner>(true);
                    readyGame.audioSource = readyGame.GetComponent<AudioSource>();
                    if (!readyGame.audioSource) readyGame.audioSource = readyGame.gameObject.AddComponent<AudioSource>();
                    readyGame.audioSource.playOnAwake = false;
                    var camera = existing.GetComponentInChildren<Camera>(true);
                    if (readyGame.player) readyGame.player.game = readyGame;
                    if (readyGame.spawner)
                    {
                        readyGame.spawner.game = readyGame;
                        if (!readyGame.spawner.wastePrefab) readyGame.spawner.wastePrefab = wastePrefab;
                        if (!readyGame.spawner.container) readyGame.spawner.container = existing.Find("Residuos_activos");
                    }
                    var readyHud = existing.GetComponentInChildren<EcoHud>(true);
                    if (readyHud) { readyHud.game = readyGame; readyHud.gameCamera = camera; }
                    var readyEffects = existing.GetComponentInChildren<EcoEffects>(true);
                    if (!readyEffects)
                    {
                        var system = existing.Find("Sistemas");
                        readyEffects = Child("Animaciones_y_efectos", system ? system : existing).AddComponent<EcoEffects>();
                    }
                    readyEffects.game = readyGame; readyEffects.gameCamera = camera; readyEffects.background = existing.Find("Fondo");
                    readyGame.effects = readyEffects;
                    var readyMusic = existing.GetComponentInChildren<EcoMusic>(true);
                    if (!readyMusic) readyMusic = Child("Musica", existing.Find("Sistemas")).AddComponent<EcoMusic>();
                    readyMusic.game = readyGame; readyGame.music = readyMusic;
                    foreach (var renderer in existing.GetComponentsInChildren<SpriteRenderer>(true))
                        renderer.sharedMaterial = spriteMaterial;
                }
                return readyGame;
            }
            if (!catalog) catalog = Resources.Load<EcoCatalog>("EcoCarrito/EcoCatalog");
            if (!catalog || !catalog.HasAssets)
            {
                Debug.LogError("EcoCarrito: catálogo incompleto. Reimporta el paquete con todos sus archivos."); return null;
            }
            if (!spriteMaterial) spriteMaterial = Resources.Load<Material>("EcoCarrito/SpriteMaterial");
            var shader = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null
                ? Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") : null;
            if (!shader) shader = Shader.Find("Sprites/Default");
            if (shader && (!spriteMaterial || spriteMaterial.shader != shader)) spriteMaterial = new Material(shader);
            if (!spriteMaterial) { Debug.LogError("EcoCarrito: no se encontró un shader de sprites."); return null; }
            var world = Child("MundoEcoCarrito", transform);
            var camObj = Child("Main Camera", world.transform);
            camObj.tag = "MainCamera"; camObj.transform.localPosition = new Vector3(0, 0, -10);
            var cam = camObj.AddComponent<Camera>(); cam.orthographic = true; cam.orthographicSize = 5;
            cam.clearFlags = CameraClearFlags.SolidColor; cam.backgroundColor = new Color(.04f, .08f, .12f);
            camObj.AddComponent<AudioListener>(); camObj.AddComponent<EcoCamera>();
            var decor = Child("Fondo", world.transform);
            if (catalog.backgroundLayers != null)
                for (int j = 0; j < catalog.backgroundLayers.Length; j++)
                {
                    var sprite = catalog.backgroundLayers[j]; if (!sprite) continue;
                    var sr = RendererObject("Ciudad_capa_" + j, decor.transform, sprite, -100 + j);
                    sr.transform.localScale = new Vector3(17.78f / sprite.bounds.size.x, 10 / sprite.bounds.size.y, 1);
                    sr.color = new Color(.88f, .8f, 1);
                }
            var pixel = Resources.Load<Sprite>("EcoCarrito/Art/UI/white");
            var lanes = Child("Carriles", world.transform);
            if (pixel)
                for (int k = 0; k < 3; k++)
                {
                    var sr = RendererObject("Carril_" + k + "_" + EcoCatalog.LaneNames[k], lanes.transform, pixel, -20);
                    sr.transform.localPosition = new Vector3(0, EcoCatalog.LaneY[k], 0);
                    sr.transform.localScale = new Vector3(17.78f / pixel.bounds.size.x, 1.9f / pixel.bounds.size.y, 1);
                    var color = EcoCatalog.LaneColors[k]; color.a = .18f; sr.color = color;
                    var line = RendererObject("Linea_" + k, lanes.transform, pixel, -19);
                    line.transform.localPosition = new Vector3(0, EcoCatalog.LaneY[k] - .8f, 0);
                    line.transform.localScale = new Vector3(17.78f / pixel.bounds.size.x, .055f / pixel.bounds.size.y, 1);
                    line.color = EcoCatalog.LaneColors[k];
                }
            var systems = Child("Sistemas", world.transform);
            var managerObj = Child("GameManager", systems.transform);
            var manager = managerObj.AddComponent<EcoGame>(); manager.catalog = catalog; manager.spriteMaterial = spriteMaterial;
            manager.audioSource = managerObj.AddComponent<AudioSource>(); manager.audioSource.playOnAwake = false; manager.audioSource.spatialBlend = 0;
            EcoPlayer p;
            if (playerPrefab) p = Instantiate(playerPrefab, world.transform);
            else
            {
                var playerObj = Child("Recolector", world.transform); p = playerObj.AddComponent<EcoPlayer>();
                p.body = RendererObject("Contenedor_con_ruedas", p.transform, catalog.collectorFrames[0], 30);
                p.body.transform.localScale = Vector3.one * (catalog.collectorHeight / catalog.collectorFrames[0].bounds.size.y);
                if (pixel)
                {
                    p.highlight = RendererObject("Color_del_carril", p.transform, pixel, 29);
                    p.highlight.transform.localPosition = new Vector3(0, -.72f, 0);
                    p.highlight.transform.localScale = new Vector3(1.1f / pixel.bounds.size.x, .06f / pixel.bounds.size.y, 1);
                }
            }
            p.gameObject.name = "Recolector"; p.gameObject.SetActive(true); p.game = manager; manager.player = p;
            if (p.body) p.body.sharedMaterial = spriteMaterial;
            if (p.highlight) p.highlight.sharedMaterial = spriteMaterial;
            p.ResetPlayer();
            var liveWaste = Child("Residuos_activos", world.transform);
            var spawnObj = Child("WasteSpawner", systems.transform);
            var spawner = spawnObj.AddComponent<EcoSpawner>(); spawner.game = manager;
            spawner.wastePrefab = wastePrefab; spawner.container = liveWaste.transform; manager.spawner = spawner;
            var hud = Child("Marcador_y_menus", world.transform).AddComponent<EcoHud>(); hud.game = manager; hud.gameCamera = cam;
            var effects = Child("Animaciones_y_efectos", systems.transform).AddComponent<EcoEffects>();
            effects.game = manager; effects.gameCamera = cam; effects.background = decor.transform;
            manager.effects = effects;
            var music = Child("Musica", systems.transform).AddComponent<EcoMusic>();
            music.game = manager; manager.music = music;
            return manager;
        }
        GameObject Child(string objectName, Transform parent)
        {
            var obj = new GameObject(objectName); obj.transform.SetParent(parent, false); return obj;
        }
        SpriteRenderer RendererObject(string objectName, Transform parent, Sprite sprite, int order)
        {
            var obj = Child(objectName, parent); var renderer = obj.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite; renderer.sortingOrder = order; renderer.sharedMaterial = spriteMaterial; return renderer;
        }
    }
}
