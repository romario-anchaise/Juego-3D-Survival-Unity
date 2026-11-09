using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

static class SurvivalCoveredBodySetup
{
    const string Folder = "Assets/FearMansion/AmbienteTerror/CuerpoCubierto/";

    public static void PlaceBatch()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        Place();
    }

    [MenuItem("Tools/Survival/Colocar cuerpo cubierto %#&j")]
    static void Place()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Sal de Play antes de colocar el cuerpo.");
        var house = GameObject.Find("MiniCasa_Amueblada");
        if (house == null || house.scene.path != "Assets/Scenes/SampleScene.unity")
            throw new InvalidOperationException("Abre SampleScene.");
        const string modelName = "DeadBodyCovered";
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(Folder + modelName + ".fbx");
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (model == null || shader == null) throw new InvalidOperationException("Falta el FBX o URP/Lit.");

        var normalPath = Folder + modelName + "_Normal.png";
        if (AssetImporter.GetAtPath(normalPath) is TextureImporter normal &&
            normal.textureType != TextureImporterType.NormalMap)
        {
            normal.textureType = TextureImporterType.NormalMap;
            normal.SaveAndReimport();
        }
        var aoPath = Folder + modelName + "_AO.png";
        if (AssetImporter.GetAtPath(aoPath) is TextureImporter ao && ao.sRGBTexture)
        {
            ao.sRGBTexture = false;
            ao.SaveAndReimport();
        }
        var material = AssetDatabase.LoadAssetAtPath<Material>(Folder + "M_CuerpoCubierto_URP.mat");
        if (material == null)
        {
            material = new Material(shader);
            AssetDatabase.CreateAsset(material, Folder + "M_CuerpoCubierto_URP.mat");
        }
        var albedo = AssetDatabase.LoadAssetAtPath<Texture2D>(Folder + modelName + "_BaseColor.png");
        var normalMap = AssetDatabase.LoadAssetAtPath<Texture2D>(normalPath);
        var occlusion = AssetDatabase.LoadAssetAtPath<Texture2D>(aoPath);
        if (albedo == null || normalMap == null || occlusion == null)
            throw new InvalidOperationException("Faltan los mapas de la tela.");
        material.SetTexture("_BaseMap", albedo);
        material.SetTexture("_BumpMap", normalMap);
        material.SetTexture("_OcclusionMap", occlusion);
        material.EnableKeyword("_NORMALMAP");
        material.EnableKeyword("_OCCLUSIONMAP");
        material.SetFloat("_BumpScale", .9f);
        material.SetFloat("_Smoothness", .28f);
        EditorUtility.SetDirty(material);
        const string prefabPath = Folder + "Cuerpo_Cubierto.prefab";
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        if (prefab == null)
        {
            var root = new GameObject("Cuerpo_Cubierto");
            try
            {
                var mesh = (GameObject)PrefabUtility.InstantiatePrefab(model, root.transform);
                foreach (var renderer in mesh.GetComponentsInChildren<MeshRenderer>())
                    renderer.sharedMaterials = Enumerable.Repeat(material, renderer.sharedMaterials.Length).ToArray();
                var size = BoundsOf(mesh).size;
                if (size.y > Mathf.Max(size.x, size.z)) mesh.transform.localRotation = Quaternion.Euler(0, 0, 90);
                else if (size.z > size.x) mesh.transform.localRotation = Quaternion.Euler(0, 90, 0);
                mesh.transform.localScale *= 1.76f / BoundsOf(mesh).size.x;
                var bounds = BoundsOf(mesh);
                mesh.transform.localPosition -= new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);
                prefab = PrefabUtility.SaveAsPrefabAsset(root, prefabPath);
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
        if (prefab == null) throw new InvalidOperationException("No se pudo guardar el prefab.");
        Directory.CreateDirectory("Logs/BodyBackup");
        EditorSceneManager.SaveScene(house.scene, "Logs/BodyBackup/Antes_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".unity", true);
        var body = house.transform.Find("Cuerpo_Cubierto");
        if (body == null)
        {
            body = ((GameObject)PrefabUtility.InstantiatePrefab(prefab, house.transform)).transform;
            Undo.RegisterCreatedObjectUndo(body.gameObject, "Colocar cuerpo cubierto");
        }
        body.localPosition = new Vector3(1.18f, .184f, -.70f);
        body.localRotation = Quaternion.Euler(0, -13, 0);
        PrefabUtility.RecordPrefabInstancePropertyModifications(body);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(house.scene);
        EditorSceneManager.SaveScene(house.scene);
        var placed = BoundsOf(body.gameObject);
        if (Mathf.Abs(placed.min.y - house.transform.TransformPoint(new Vector3(0, .184f, 0)).y) > .01f)
            Debug.LogWarning("Revisa la altura del cuerpo.");
        File.WriteAllText("Logs/Cuerpo_Cubierto_Verificado.txt", "Material: " + material.shader.name +
            "; dimensiones: " + placed.size + "; altura inferior: " + placed.min.y);
        Selection.activeGameObject = body.gameObject;
        SceneView.lastActiveSceneView?.FrameSelected();
        Debug.Log("Cuerpo cubierto guardado en el comedor.");
    }

    static Bounds BoundsOf(GameObject obj)
    {
        var renderers = obj.GetComponentsInChildren<MeshRenderer>();
        if (renderers.Length == 0) throw new InvalidOperationException("El FBX no contiene una malla visible.");
        var bounds = renderers[0].bounds;
        foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
        return bounds;
    }
}
