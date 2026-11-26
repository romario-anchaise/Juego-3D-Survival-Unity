using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

/// <summary>Clips the third person camera against the nearby twisted-tree silhouettes.</summary>
[DisallowMultipleComponent]
public sealed class TreeCameraClearance : CinemachineExtension
{
    [SerializeField] Transform treeRoot;
    [SerializeField, Min(0f)] float clearance = .15f;
    readonly List<MeshCollider> probes = new();

    void Start()
    {
        if (treeRoot == null) return;
        foreach (Transform tree in treeRoot)
        {
            var mesh = tree.GetComponentInChildren<MeshFilter>();
            if (mesh == null || mesh.sharedMesh == null) continue;
            var probe = new GameObject("Camera tree probe") { layer = 2 };
            probe.transform.SetParent(mesh.transform, false);
            var collider = probe.AddComponent<MeshCollider>();
            collider.excludeLayers = ~0;
            collider.sharedMesh = mesh.sharedMesh;
            probes.Add(collider);
        }
    }

    protected override void PostPipelineStageCallback(CinemachineVirtualCameraBase vcam,
        CinemachineCore.Stage stage, ref CameraState state, float deltaTime)
    {
        if (stage != CinemachineCore.Stage.Body || vcam.Follow == null || probes.Count == 0)
            return;

        Vector3 origin = vcam.Follow.position;
        Vector3 cameraPosition = state.GetCorrectedPosition();
        Vector3 offset = cameraPosition - origin;
        float distance = offset.magnitude;
        if (distance < .001f) return;
        var ray = new Ray(origin, offset / distance);
        float nearest = distance;
        foreach (var probe in probes)
        {
            if (probe == null || !probe.bounds.IntersectRay(ray, out float boundsDistance)
                || boundsDistance > nearest) continue;
            if (probe.Raycast(ray, out var hit, nearest))
                nearest = hit.distance;
        }
        if (nearest < distance)
            state.PositionCorrection += ray.GetPoint(Mathf.Max(0f, nearest - clearance)) - cameraPosition;
    }

    protected override void OnDestroy()
    {
        foreach (var probe in probes)
            if (probe != null) Destroy(probe.gameObject);
        base.OnDestroy();
    }
}
