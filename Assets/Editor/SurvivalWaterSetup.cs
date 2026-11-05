using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class SurvivalWaterSetup
{
    const string Root = "Assets/_MyShaders/IntegracionSurvival/";

    [MenuItem("Tools/Survival/Agregar agua al bano %#&w")]
    public static void AddWater()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Sal de Play antes de agregar el agua.");
        var scene = SceneManager.GetActiveScene();
        if (scene.path != "Assets/Scenes/SampleScene.unity")
            throw new InvalidOperationException("Abre SampleScene para agregar el agua.");
        var house = GameObject.Find("MiniCasa_Amueblada");
        if (house == null) throw new InvalidOperationException("No se encontro la casa.");
        var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/Charco_Agua_Bano.prefab");
        var material = AssetDatabase.LoadAssetAtPath<Material>(Root + "Materials/M_Agua_Bano.mat");
        if (prefab == null || material == null || material.shader == null || ShaderUtil.ShaderHasError(material.shader))
            throw new InvalidOperationException("El shader o el prefab de agua no esta listo.");

        Directory.CreateDirectory("Logs/WaterBackup");
        EditorSceneManager.SaveScene(scene,
            "Logs/WaterBackup/SampleScene_" + DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".unity", true);
        var water = house.transform.Find("Charco_Agua_Bano");
        if (water == null)
        {
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(prefab, house.transform);
            Undo.RegisterCreatedObjectUndo(instance, "Agregar charco de agua");
            water = instance.transform;
        }
        water.localPosition = new Vector3(2.75f, .202f, 2.4f);
        PrefabUtility.RecordPrefabInstancePropertyModifications(water);
        Selection.activeGameObject = water.gameObject;
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        SceneView.lastActiveSceneView?.FrameSelected();
        Preview(house.transform);
        Debug.Log("Agua lista: charco del bano, shader URP animado y prefab reutilizable.");
    }

    static void Preview(Transform house)
    {
        var temporary = new GameObject("Vista temporal agua");
        var camera = temporary.AddComponent<Camera>();
        if (Camera.main != null) camera.CopyFrom(Camera.main);
        camera.enabled = false;
        camera.transform.position = house.TransformPoint(new Vector3(2f, 1.25f, 1.6f));
        camera.transform.LookAt(house.TransformPoint(new Vector3(2.75f, .202f, 2.4f)));
        camera.fieldOfView = 60;
        camera.nearClipPlane = .04f;
        camera.GetUniversalAdditionalCameraData().renderPostProcessing = true;
        var target = new RenderTexture(1000, 750, 24);
        var previous = RenderTexture.active;
        Texture2D picture = null;
        try
        {
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            picture = new Texture2D(1000, 750, TextureFormat.RGB24, false);
            picture.ReadPixels(new Rect(0, 0, 1000, 750), 0, 0);
            picture.Apply();
            File.WriteAllBytes("Logs/WaterPreview.png", picture.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active = previous;
            camera.targetTexture = null;
            target.Release();
            UnityEngine.Object.DestroyImmediate(target);
            if (picture != null) UnityEngine.Object.DestroyImmediate(picture);
            UnityEngine.Object.DestroyImmediate(temporary);
        }
    }
}
