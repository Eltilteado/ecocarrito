using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace EcoCarrito.Editor
{
    public static class EcoSceneBuilder
    {
        const string Root = "Assets/EcoCarrito";
        const string CatalogPath = Root + "/Resources/EcoCarrito/EcoCatalog.asset";
        const string ScenePath = Root + "/Scenes/EcoCarrito.unity";

        [MenuItem("EcoCarrito/1. Abrir escena lista")]
        public static void OpenScene()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Sal de Play antes de abrir otra escena."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            EditorSceneManager.OpenScene(ScenePath);
        }

        [MenuItem("EcoCarrito/2. Armar escena completa")]
        public static void BuildScene()
        {
            if (EditorApplication.isPlaying) { Debug.LogWarning("Sal de Play antes de armar la escena."); return; }
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            var catalog = EcoRepair.RepairAssets();
            if (!catalog || !catalog.HasAssets) { Debug.LogError("EcoCarrito: faltan los assets. Importa todos los archivos del paquete."); return; }
            Directory.CreateDirectory(Root + "/Scenes"); Directory.CreateDirectory(Root + "/Prefabs");
            var shader = UnityEngine.Rendering.GraphicsSettings.currentRenderPipeline != null
                ? Shader.Find("Universal Render Pipeline/2D/Sprite-Unlit-Default") : null;
            if (!shader) shader = Shader.Find("Sprites/Default");
            if (!shader) { Debug.LogError("No hay un shader de sprites compatible con este proyecto."); return; }
            const string materialPath = Root + "/Resources/EcoCarrito/SpriteMaterial.mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if (!material) { material = new Material(shader); AssetDatabase.CreateAsset(material, materialPath); }
            else material.shader = shader;
            EditorUtility.SetDirty(material);
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var bootObj = new GameObject("EcoCarrito"); var boot = bootObj.AddComponent<EcoBootstrap>();
            boot.catalog = catalog; boot.spriteMaterial = material;
            var game = boot.BuildWorld();
            if (!game) { Debug.LogError("No se pudo construir la escena."); return; }
            var playerTemplate = Object.Instantiate(game.player.gameObject);
            playerTemplate.name = "Recolector"; playerTemplate.GetComponent<EcoPlayer>().game = null;
            var player = PrefabUtility.SaveAsPrefabAsset(playerTemplate, Root + "/Prefabs/Recolector.prefab");
            Object.DestroyImmediate(playerTemplate);
            boot.playerPrefab = player.GetComponent<EcoPlayer>();
            var template = new GameObject("Residuo"); var child = new GameObject("Sprite"); child.transform.SetParent(template.transform, false);
            var visual = child.AddComponent<SpriteRenderer>(); visual.sharedMaterial = material; visual.sprite = catalog.wastes[0].sprite;
            visual.sortingOrder = 20; visual.transform.localScale = Vector3.one * (.65f / visual.sprite.bounds.size.y);
            var waste = template.AddComponent<EcoWaste>(); waste.visual = visual;
            var wasteAsset = PrefabUtility.SaveAsPrefabAsset(template, Root + "/Prefabs/Residuo.prefab");
            Object.DestroyImmediate(template); boot.wastePrefab = wasteAsset.GetComponent<EcoWaste>(); game.spawner.wastePrefab = boot.wastePrefab;
            // Prefab does not retain a scene-manager reference; Bootstrap supplies it when instantiated.
            EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene, ScenePath);
            bool listed = false; foreach (var s in EditorBuildSettings.scenes) if (s.path == ScenePath) listed = true;
            if (!listed)
            {
                var old = EditorBuildSettings.scenes; var updated = new EditorBuildSettingsScene[old.Length + 1];
                old.CopyTo(updated, 0); updated[old.Length] = new EditorBuildSettingsScene(ScenePath, true); EditorBuildSettings.scenes = updated;
            }
            AssetDatabase.SaveAssets(); AssetDatabase.Refresh();
            Selection.activeGameObject = bootObj;
            Debug.Log("EcoCarrito armado: escena, cámara, carriles, recolector, prefabs, residuos y marcador. Pulsa Play.");
        }

        [MenuItem("EcoCarrito/3. Verificar recursos")]
        public static void Check()
        {
            var c = AssetDatabase.LoadAssetAtPath<EcoCatalog>(CatalogPath);
            if (!c || !c.HasAssets) { Debug.LogError("Catálogo incompleto."); return; }
            int count = 0; foreach (var w in c.wastes) if (w != null && w.sprite) count++;
            if (count < 10 || c.backgroundLayers.Length < 6 || !c.collectSound || !c.errorSound)
            { Debug.LogError("Faltan recursos. Reimporta el paquete completo."); return; }
            Debug.Log("Recursos correctos: " + count + " residuos, " + c.collectorFrames.Length + " frames del jugador, fondo y sonidos.");
        }
    }
}
