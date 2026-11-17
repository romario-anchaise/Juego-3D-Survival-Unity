#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public static class WebGameBuild
{
    public const string Output = "Builds/WebGL";
    public const string ResultFile = "Builds/WebGL-result.txt";

    [MenuItem("Tools/El bosque te llama/Generar version WebGL")]
    public static void Build()
    {
        Directory.CreateDirectory("Builds");
        File.WriteAllText(ResultFile, "BUILDING");
        try
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode)
                throw new InvalidOperationException("Sal de Play antes de generar WebGL.");
            if (!BuildPipeline.IsBuildTargetSupported(BuildTargetGroup.WebGL, BuildTarget.WebGL))
                throw new InvalidOperationException("Instala Web Build Support para esta version de Unity.");
            if (!EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                throw new OperationCanceledException("Compilacion cancelada.");
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0) throw new InvalidOperationException("No hay escenas habilitadas.");
            PlayerSettings.WebGL.compressionFormat = WebGLCompressionFormat.Gzip;
            PlayerSettings.WebGL.decompressionFallback = true;
            PlayerSettings.WebGL.dataCaching = true;
            var report = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = Output,
                target = BuildTarget.WebGL,
                options = BuildOptions.None
            });
            if (report.summary.result != BuildResult.Succeeded)
                throw new Exception("WebGL: " + report.summary.result + "; errores: " + report.summary.totalErrors);
            if (!File.Exists(Path.Combine(Output, "index.html")))
                throw new Exception("Unity no genero index.html. Reinicia el Editor si acabas de instalar Web Build Support.");
            File.WriteAllText(ResultFile, "SUCCEEDED\nBytes: " + report.summary.totalSize
                + "\nDuracion: " + report.summary.totalTime + "\nUTC: " + DateTime.UtcNow.ToString("O"));
            Debug.Log("WebGL generado en " + Path.GetFullPath(Output));
        }
        catch (Exception error)
        {
            File.WriteAllText(ResultFile, "FAILED\n" + error);
            throw;
        }
    }
}
#endif
