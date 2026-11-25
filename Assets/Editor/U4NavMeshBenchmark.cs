using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using Unity.Profiling;
using UnityEditor;
using UnityEngine;
using UnityEngine.AI;

public sealed class U4NavMeshBenchmark : EditorWindow
{
    static readonly ProfilerMarker Submit = new("U4.NavMesh.SubmitDestinations");
    static readonly Vector3 Origin = new(800, 3, 800);
    readonly List<NavMeshAgent> agents = new();
    readonly StringBuilder csv = new();
    int agentCount = 25;
    int duration = 360;
    int optimizedInterval = 20;
    int startFrame;
    int lastFrame;
    int accepted;
    int rejected;
    int phase;
    double submissionMs;
    GameObject root;
    NavMeshData data;
    NavMeshDataInstance instance;

    [MenuItem("Tools/QA/U4 NavMesh Benchmark")]
    public static void Open() => GetWindow<U4NavMeshBenchmark>("U4 NavMesh");

    void OnGUI()
    {
        using (new EditorGUI.DisabledScope(root != null))
        {
            agentCount = EditorGUILayout.IntSlider("Agents", agentCount, 1, 100);
            duration = EditorGUILayout.IntSlider("Frames per variant", duration, 120, 1800);
            optimizedInterval = EditorGUILayout.IntSlider("Optimized interval", optimizedInterval, 2, 60);
            using (new EditorGUI.DisabledScope(!EditorApplication.isPlaying))
                if (GUILayout.Button("Run baseline and optimized")) StartRun();
        }
        if (root != null && GUILayout.Button("Stop")) Cleanup();
    }

    public void StartRun()
    {
        if (!EditorApplication.isPlaying || root != null) return;
        var settings = NavMesh.GetSettingsByIndex(0);
        var floor = new NavMeshBuildSource
        {
            shape = NavMeshBuildSourceShape.Box,
            transform = Matrix4x4.identity,
            size = new Vector3(64, 1, 64)
        };
        data = NavMeshBuilder.BuildNavMeshData(settings, new List<NavMeshBuildSource> { floor },
            new Bounds(Vector3.zero, new Vector3(68, 10, 68)), Vector3.zero, Quaternion.identity);
        if (data == null) return;
        instance = NavMesh.AddNavMeshData(data, Origin, Quaternion.identity);
        root = new GameObject("U4 NavMesh benchmark") { hideFlags = HideFlags.DontSave };
        for (int i = 0; i < agentCount; i++)
        {
            var actor = new GameObject($"Benchmark agent {i}");
            actor.transform.SetParent(root.transform);
            Vector3 spawn = Origin + new Vector3(-24 + (i % 10) * 4, .5f, -24 + (i / 10) * 4);
            if (!NavMesh.SamplePosition(spawn, out var hit, 2, NavMesh.AllAreas))
            {
                Cleanup();
                Debug.LogError("U4 benchmark: spawn outside the test NavMesh.");
                return;
            }
            actor.transform.position = hit.position;
            agents.Add(actor.AddComponent<NavMeshAgent>());
        }
        csv.Clear().AppendLine("variant,frame,agents,interval,accepted,rejected,pending,complete,submit_ms");
        phase = 0;
        startFrame = Time.frameCount;
        lastFrame = -1;
        EditorApplication.update += Sample;
        EditorApplication.playModeStateChanged += PlayModeChanged;
    }

    void Sample()
    {
        if (Time.frameCount == lastFrame) return;
        lastFrame = Time.frameCount;
        int frame = lastFrame - startFrame;
        if (frame < 30) return;
        if (frame >= duration + 30)
        {
            if (phase == 0)
            {
                phase = 1;
                for (int i = 0; i < agents.Count; i++)
                {
                    var agent = agents[i];
                    agent.ResetPath();
                    agent.Warp(Origin + new Vector3(-24 + (i % 10) * 4, .58f, -24 + (i / 10) * 4));
                }
                startFrame = lastFrame;
                return;
            }
            const string path = "Temp/U4Profiling/NavMeshBenchmark.csv";
            Directory.CreateDirectory(Path.GetDirectoryName(path));
            File.WriteAllText(path, csv.ToString());
            Debug.Log($"U4 benchmark saved: {Path.GetFullPath(path)}");
            Cleanup();
            return;
        }
        int sample = frame - 30;
        int interval = phase == 0 ? 1 : optimizedInterval;
        accepted = rejected = 0;
        submissionMs = 0;
        if (sample % interval == 0)
        {
            long start = System.Diagnostics.Stopwatch.GetTimestamp();
            using (Submit.Auto())
            {
                for (int i = 0; i < agents.Count; i++)
                {
                    var destination = Origin + new Vector3(24 - i % 10 * 4, .58f,
                        24 - i / 10 * 4 - 2 * Mathf.Sin(sample * .035f));
                    if (agents[i].SetDestination(destination)) accepted++;
                    else rejected++;
                }
            }
            submissionMs = (System.Diagnostics.Stopwatch.GetTimestamp() - start) * 1000d
                / System.Diagnostics.Stopwatch.Frequency;
        }
        int pending = 0, complete = 0;
        foreach (var agent in agents)
        {
            if (agent.pathPending) pending++;
            else if (agent.hasPath && agent.pathStatus == NavMeshPathStatus.PathComplete) complete++;
        }
        csv.AppendLine(FormattableString.Invariant(
            $"{phase},{sample},{agents.Count},{interval},{accepted},{rejected},{pending},{complete},{submissionMs:F6}"));
    }

    void PlayModeChanged(PlayModeStateChange state)
    {
        if (state == PlayModeStateChange.ExitingPlayMode) Cleanup();
    }

    void OnDisable() => Cleanup();

    void Cleanup()
    {
        EditorApplication.update -= Sample;
        EditorApplication.playModeStateChanged -= PlayModeChanged;
        if (root != null) DestroyImmediate(root);
        if (instance.valid) instance.Remove();
        if (data != null) DestroyImmediate(data);
        agents.Clear();
        root = null;
        data = null;
    }
}
