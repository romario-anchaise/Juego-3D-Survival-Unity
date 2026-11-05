using System;
using System.IO;
using UnityEditor;
using UnityEngine;

public static class SurvivalGrassCleanup
{
    [MenuItem("Tools/Survival/Quitar pasto del interior %#&g")]
    public static void Apply()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Sal de Play antes de quitar el pasto.");
        var house = GameObject.Find("MiniCasa_Amueblada");
        if (house == null) throw new InvalidOperationException("Falta la casa.");
        int cleared = 0;
        foreach (var terrain in Terrain.activeTerrains)
        {
            var data = terrain.terrainData;
            if (data.detailWidth == 0 || data.detailHeight == 0) continue;
            string source = AssetDatabase.GetAssetPath(data);
            Directory.CreateDirectory("Logs/GrassBackup");
            if (File.Exists(source))
                File.Copy(source, "Logs/GrassBackup/" + data.name + "_" +
                    DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".asset", false);
            Undo.RegisterCompleteObjectUndo(data, "Quitar pasto bajo la casa");
            float cellX = data.size.x / data.detailWidth;
            float cellZ = data.size.z / data.detailHeight;
            float margin = .45f + Mathf.Max(cellX, cellZ) * .5f /
                Mathf.Max(.01f, Mathf.Min(Mathf.Abs(house.transform.lossyScale.x), Mathf.Abs(house.transform.lossyScale.z)));
            var mask = new bool[data.detailHeight, data.detailWidth];
            for (int z = 0; z < data.detailHeight; z++)
            for (int x = 0; x < data.detailWidth; x++)
            {
                var world = terrain.transform.TransformPoint(new Vector3((x + .5f) * cellX, 0, (z + .5f) * cellZ));
                var local = house.transform.InverseTransformPoint(world);
                bool interior = Mathf.Abs(local.x) <= 4.5f + margin && Mathf.Abs(local.z) <= 4f + margin;
                bool porch = local.x >= -2f - margin && local.x <= 3f + margin &&
                    local.z >= -5.9f - margin && local.z <= -4f + margin;
                mask[z, x] = interior || porch;
            }
            for (int layer = 0; layer < data.detailPrototypes.Length; layer++)
            {
                var details = data.GetDetailLayer(0, 0, data.detailWidth, data.detailHeight, layer);
                for (int z = 0; z < data.detailHeight; z++)
                for (int x = 0; x < data.detailWidth; x++)
                    if (mask[z, x]) { if (details[z, x] > 0) cleared++; details[z, x] = 0; }
                data.SetDetailLayer(0, 0, layer, details);
                var check = data.GetDetailLayer(0, 0, data.detailWidth, data.detailHeight, layer);
                for (int z = 0; z < data.detailHeight; z++)
                for (int x = 0; x < data.detailWidth; x++)
                    if (mask[z, x] && check[z, x] != 0)
                        throw new InvalidOperationException("Quedo pasto en el interior.");
            }
            EditorUtility.SetDirty(data);
            terrain.Flush();
        }
        AssetDatabase.SaveAssets();
        SceneView.RepaintAll();
        File.WriteAllText("Logs/Pasto_Interior_Verificado.txt", "PASS: interior y porche sin pasto en todas las capas; " +
            cleared + " celdas con pasto limpiadas; detalles fuera de la zona conservados.");
        Debug.Log("Pasto retirado del interior y del porche; jardin conservado.");
    }
}
