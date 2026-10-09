using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EcoCarrito.Editor
{
    // Repair inside Unity, using its own serializer and asset importer.
    public static class EcoRepair
    {
        const string Root = "Assets/EcoCarrito/Resources/EcoCarrito/";
        static bool repairing;
        [InitializeOnLoadMethod]
        static void Register()
        {
            EditorSceneManager.sceneOpened -= SceneOpened;
            EditorSceneManager.sceneOpened += SceneOpened;
        }
        static void SceneOpened(Scene scene, OpenSceneMode mode)
        {
            if (scene.path == "Assets/EcoCarrito/Scenes/EcoCarrito.unity")
                EditorApplication.delayCall += RepairCurrentScene;
        }
        public static EcoCatalog RepairAssets()
        {
            var c = AssetDatabase.LoadAssetAtPath<EcoCatalog>(Root + "EcoCatalog.asset");
            if (!c)
            {
                c = AssetDatabase.LoadAssetAtPath<EcoCatalog>(Root + "EcoCatalog_Repaired.asset");
                if (!c) { c = ScriptableObject.CreateInstance<EcoCatalog>(); AssetDatabase.CreateAsset(c, Root + "EcoCatalog_Repaired.asset"); }
            }
            c = EcoAssets.LoadCatalog(c);
            var shader = EcoAssets.SpriteShader();
            if (!shader) { Debug.LogError("EcoCarrito: no se encuentra un shader de sprites."); return c; }
            var mat = AssetDatabase.LoadAssetAtPath<Material>(Root + "SpriteMaterial.mat");
            if (!mat)
            {
                mat = AssetDatabase.LoadAssetAtPath<Material>(Root + "SpriteMaterial_Repaired.mat");
                if (!mat) { mat = new Material(shader); AssetDatabase.CreateAsset(mat, Root + "SpriteMaterial_Repaired.mat"); }
            }
            mat.shader = shader;
            if (mat.HasProperty("_Color")) mat.SetColor("_Color", Color.white);
            EditorUtility.SetDirty(c); EditorUtility.SetDirty(mat); AssetDatabase.SaveAssets();
            return c;
        }
        [MenuItem("EcoCarrito/0. Reparar materiales y conexiones")]
        public static void RepairCurrentScene()
        {
            if (repairing || EditorApplication.isPlayingOrWillChangePlaymode) return;
            repairing = true;
            try
            {
                var scene = SceneManager.GetActiveScene(); EcoBootstrap boot = null;
                foreach (var root in scene.GetRootGameObjects())
                {
                    boot = root.GetComponentInChildren<EcoBootstrap>(true);
                    if (boot) break;
                }
                if (!boot) { Debug.LogWarning("EcoCarrito: abre la escena EcoCarrito antes de usar Reparar."); return; }
                Undo.RegisterFullObjectHierarchyUndo(boot.gameObject, "Reparar EcoCarrito");
                boot.catalog = RepairAssets();
                boot.spriteMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "SpriteMaterial.mat");
                if (!boot.spriteMaterial) boot.spriteMaterial = AssetDatabase.LoadAssetAtPath<Material>(Root + "SpriteMaterial_Repaired.mat");
                var residue = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EcoCarrito/Prefabs/Residuo.prefab");
                if (residue) boot.wastePrefab = residue.GetComponent<EcoWaste>();
                var collector = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/EcoCarrito/Prefabs/Recolector.prefab");
                if (collector) boot.playerPrefab = collector.GetComponent<EcoPlayer>();
                var game = boot.BuildWorld();
                if (!game || !game.catalog || !game.catalog.HasAssets) { Debug.LogError("EcoCarrito: faltan sprites. Importa todos los archivos del paquete."); return; }
                foreach (var renderer in boot.GetComponentsInChildren<SpriteRenderer>(true))
                    renderer.sharedMaterial = boot.spriteMaterial;
                if (game.player)
                {
                    if (!game.player.body)
                    {
                        var body = game.player.transform.Find("Contenedor_con_ruedas");
                        if (body) game.player.body = body.GetComponent<SpriteRenderer>();
                    }
                    if (!game.player.highlight)
                    {
                        var highlight = game.player.transform.Find("Color_del_carril");
                        if (highlight) game.player.highlight = highlight.GetComponent<SpriteRenderer>();
                    }
                }
                var preview = boot.transform.Find("MundoEcoCarrito/Muestras_residuos_EDITOR");
                if (preview) preview.gameObject.SetActive(true);
                EditorSceneManager.MarkSceneDirty(scene);
                // Keep edits in memory until the user saves, preserving their scene changes.
                Selection.activeGameObject = boot.gameObject;
                if (SceneView.lastActiveSceneView) SceneView.lastActiveSceneView.Frame(new Bounds(Vector3.zero, new Vector3(18, 10, 1)), false);
                Debug.Log("EcoCarrito: materiales y referencias reparados. Ctrl+S; después Play > Game. La intro se muestra al iniciar.");
            }
            finally { repairing = false; }
        }
    }
}
