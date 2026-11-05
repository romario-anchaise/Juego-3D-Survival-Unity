using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class SurvivalAbandonedLighting
{
    [MenuItem("Tools/Survival/Bajar luz - casa abandonada %#&d")]
    public static void Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            scene.path != "Assets/Scenes/SampleScene.unity")
            throw new InvalidOperationException("Abre SampleScene y sal de Play.");
        Directory.CreateDirectory("Logs/LightingBackup");
        EditorSceneManager.SaveScene(scene, "Logs/LightingBackup/SampleScene_" +
            DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".unity", true);

        foreach (var light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if (light.gameObject.scene != scene) continue;
            float intensity;
            switch (light.name)
            {
                case "Luz_Sala": intensity = .85f; break;
                case "Luz_Cocina": intensity = .65f; break;
                case "Luz_Dormitorio": intensity = .45f; break;
                case "Luz_Bano": intensity = .55f; break;
                case "Luz_Porche": intensity = .65f; break;
                case "Acento_Puerta": intensity = 1.8f; break;
                case "Sol_Terreno": intensity = .7f; break;
                case "Reflejo_Luna_Personaje": intensity = 2.5f; break;
                default: continue;
            }
            Undo.RecordObject(light, "Bajar luz de casa abandonada");
            light.intensity = intensity;
            if (light.name.StartsWith("Luz_") || light.name == "Acento_Puerta")
                light.color = new Color(.9f, .72f, .48f);
            EditorUtility.SetDirty(light);
            PrefabUtility.RecordPrefabInstancePropertyModifications(light);
            if (light.name == "Luz_Porche" || light.name == "Acento_Puerta" || light.name == "Luz_Bano")
            {
                var flicker = light.GetComponent<OldBulbFlicker>();
                if (flicker == null) flicker = Undo.AddComponent<OldBulbFlicker>(light.gameObject);
                Undo.RecordObject(flicker, "Configurar foco viejo");
                flicker.baseIntensity = intensity;
                flicker.intervalSeconds = new Vector2(3f, 7f);
                flicker.dimMultiplier = .03f;
                flicker.patternSeed = light.name == "Luz_Bano" ? 29 : 17;
                if (light.name != "Acento_Puerta")
                    flicker.bulbRenderer = AddVisibleBulb(light);
                EditorUtility.SetDirty(flicker);
                PrefabUtility.RecordPrefabInstancePropertyModifications(flicker);
            }
        }
        RenderSettings.ambientSkyColor = new Color(.27f, .31f, .41f);
        RenderSettings.ambientEquatorColor = new Color(.19f, .22f, .29f);
        RenderSettings.ambientGroundColor = new Color(.11f, .13f, .17f);
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        SceneView.RepaintAll();
        Debug.Log("Casa abandonada: luces tenues y parpadeos ocasionales en porche y bano.");
    }

    static Renderer AddVisibleBulb(Light light)
    {
        const string path = "Assets/FearMansion/AmbienteTerror/Foco_Emisivo.mat";
        var material = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (material == null)
        {
            material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.name = "Foco_Emisivo";
            material.SetColor("_BaseColor", new Color(.65f, .5f, .28f));
            material.SetColor("_EmissionColor", new Color(5f, 3.1f, 1.2f));
            material.EnableKeyword("_EMISSION");
            material.globalIlluminationFlags = UnityEngine.MaterialGlobalIlluminationFlags.RealtimeEmissive;
            AssetDatabase.CreateAsset(material, path);
        }
        material.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
        material.EnableKeyword("_EMISSION");
        EditorUtility.SetDirty(material);
        AssetDatabase.SaveAssets();
        var existing = light.transform.Find("Bombilla_Visible");
        if (existing != null) return existing.GetComponent<Renderer>();
        var sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.name = "Bombilla_Visible";
        Undo.RegisterCreatedObjectUndo(sphere, "Crear bombilla visible");
        sphere.transform.SetParent(light.transform, false);
        sphere.transform.localScale = new Vector3(.15f, .21f, .15f);
        UnityEngine.Object.DestroyImmediate(sphere.GetComponent<Collider>());
        var renderer = sphere.GetComponent<Renderer>();
        renderer.sharedMaterial = material;
        renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        var socket = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
        socket.name = "Portalamparas";
        Undo.RegisterCreatedObjectUndo(socket, "Crear portalamparas");
        socket.transform.SetParent(light.transform, false);
        socket.transform.localPosition = new Vector3(0, .14f, 0);
        socket.transform.localScale = new Vector3(.12f, .04f, .12f);
        UnityEngine.Object.DestroyImmediate(socket.GetComponent<Collider>());
        var metal = AssetDatabase.LoadAssetAtPath<Material>("Assets/FearMansion/Materials/FM_Metal.mat");
        if (metal != null) socket.GetComponent<Renderer>().sharedMaterial = metal;
        return renderer;
    }
}
