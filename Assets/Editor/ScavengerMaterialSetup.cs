using System.IO;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEditor.Rendering.Universal;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

// Asset Store materials are excluded from Git; convert each collaborator's local copy.
[InitializeOnLoad]
static class ScavengerMaterialSetup
{
    const string Folder = "Assets/Homeless_people/Free_Scavenger/Materials/";

    static ScavengerMaterialSetup()
    {
        EditorApplication.delayCall += ConvertMaterials;
    }

    [MenuItem("Tools/Survival/Preparar materiales Scavenger")]
    static void ConvertMaterials()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode ||
            !(GraphicsSettings.currentRenderPipeline is UniversalRenderPipelineAsset))
            return;

        string[] names = { "Scavenger_Body", "Scavenger_eye_transparent", "Scavenger_hairs" };
        int converted = 0;
        foreach (string name in names)
        {
            string path = Folder + name + ".mat";
            Material material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null || material.shader == null)
                continue;
            string shaderName = material.shader.name;
            if (shaderName != "Standard" && shaderName != "Standard (Specular setup)")
                continue;

            Directory.CreateDirectory("Logs/ScavengerMaterialBackup");
            string backup = "Logs/ScavengerMaterialBackup/" + name + ".mat";
            if (!File.Exists(backup)) File.Copy(path, backup);
            MaterialUpgrader.Upgrade(material, new StandardUpgrader(shaderName),
                MaterialUpgrader.UpgradeFlags.None);
            EditorUtility.SetDirty(material);
            converted++;
        }
        if (converted > 0)
        {
            AssetDatabase.SaveAssets();
            Debug.Log("Scavenger: " + converted + " materiales convertidos a URP.");
        }
    }
}
