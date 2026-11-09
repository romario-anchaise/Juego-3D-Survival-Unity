using UnityEngine;
using UnityEngine.Rendering;

// Render the stain on the original porcelain geometry, including curved surfaces.
public sealed class BathroomBlood : MonoBehaviour
{
    [SerializeField] Material stain;
    [SerializeField] bool toilet;

    void Start()
    {
        if (stain == null) return;
        foreach (var filter in GetComponentsInChildren<MeshFilter>())
        {
            var source = filter.GetComponent<MeshRenderer>();
            if (source == null || filter.sharedMesh == null) continue;
            bool lid = toilet && filter.name == "toilet_lid";
            if (!lid && filter.name != (toilet ? "toilet_body" : "bathroomSink_body")) continue;
            if (lid)
            {
                // Raise the hinged lid towards the tank to expose the inside of the bowl.
                float sign = filter.sharedMesh.bounds.center.x >= 0 ? 1 : -1;
                filter.transform.localRotation *= Quaternion.AngleAxis(82 * sign, Vector3.forward);
            }
            var overlay = new GameObject("Sangre sobre porcelana") { layer = source.gameObject.layer };
            overlay.transform.SetParent(source.transform, false);
            var mesh = filter.sharedMesh;
            overlay.AddComponent<MeshFilter>().sharedMesh = mesh;
            var renderer = overlay.AddComponent<MeshRenderer>();
            var materials = new Material[mesh.subMeshCount];
            for (int i = 0; i < materials.Length; i++) materials[i] = stain;
            renderer.sharedMaterials = materials;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            var bounds = mesh.bounds;
            var properties = new MaterialPropertyBlock();
            properties.SetVector("_BoundsMin", bounds.min);
            properties.SetVector("_BoundsSize", bounds.size);
            properties.SetFloat("_Fixture", lid ? 2 : toilet ? 1 : 0);
            renderer.SetPropertyBlock(properties);
        }
    }
}
