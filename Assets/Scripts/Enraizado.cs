using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public sealed class Enraizado : MonoBehaviour
{
    [SerializeField] GameObject model;
    [SerializeField] PlayerInventory inventory;
    [SerializeField] AudioClip rustle;
    [SerializeField] Vector3 spawn = new Vector3(64f, .45f, -8.6f);

    NavMeshSurface surface;
    NavMeshAgent agent;
    Animation motion;
    PlayerController3D player;
    Transform visualRoot;
    Transform hips;
    Vector3 visualOrigin;
    Vector3 hipsOrigin;
    Vector3 home;
    Vector3 destination;
    float attentionUntil;
    float nextPathUpdate;
    bool stopped;

    public bool CaughtPlayer { get; private set; }
    public bool Hunting => agent != null && Time.time < attentionUntil;

    public void HearLatch(Vector3 source)
    {
        if (stopped || (agent == null && !Wake())) return;
        destination = source;
        attentionUntil = Time.time + 4f;
    }

    bool Wake()
    {
        if (!model || !inventory || surface != null) return false;
        player = inventory.GetComponent<PlayerController3D>();
        if (!player) return false;
        surface = gameObject.AddComponent<NavMeshSurface>();
        surface.collectObjects = CollectObjects.Volume;
        surface.center = transform.InverseTransformPoint(new Vector3(64f, 2f, -10f));
        surface.size = new Vector3(42f, 16f, 38f);
        surface.layerMask = 1;
        surface.useGeometry = NavMeshCollectGeometry.PhysicsColliders;
        surface.BuildNavMesh();
        if (!NavMesh.SamplePosition(spawn, out var start, 3f, NavMesh.AllAreas))
        {
            Debug.LogWarning("El Enraizado no encontró suelo navegable.", this);
            return false;
        }

        home = start.position;
        var body = new GameObject("El Enraizado");
        body.transform.SetParent(transform);
        body.transform.SetPositionAndRotation(home, Quaternion.identity);
        agent = body.AddComponent<NavMeshAgent>();
        agent.radius = .35f;
        agent.height = 2.3f;
        agent.speed = 2.65f;
        agent.angularSpeed = 420f;
        agent.acceleration = 7f;
        agent.stoppingDistance = .25f;
        var visual = Instantiate(model, body.transform);
        var skin = visual.GetComponentInChildren<SkinnedMeshRenderer>();
        if (skin != null && skin.bounds.size.y > .01f)
        {
            float scale = 2.45f / skin.bounds.size.y;
            float bottom = skin.bounds.min.y - body.transform.position.y;
            visual.transform.localScale *= scale;
            visual.transform.localPosition = Vector3.down * (bottom * scale);
        }
        motion = visual.GetComponentInChildren<Animation>();
        if (motion != null && motion.clip != null)
        {
            motion.wrapMode = WrapMode.Loop;
            motion.Play();
            motion.Sample();
            visualRoot = visual.transform;
            visualOrigin = visualRoot.localPosition;
            foreach (Transform bone in visual.GetComponentsInChildren<Transform>())
                if (bone.name.EndsWith("Hips", System.StringComparison.Ordinal))
                {
                    hips = bone;
                    hipsOrigin = visualRoot.InverseTransformPoint(hips.position);
                    break;
                }
        }
        if (rustle != null) AudioSource.PlayClipAtPoint(rustle, home, .85f);
        return true;
    }

    void Update()
    {
        if (stopped || agent == null || !agent.isOnNavMesh) return;
        Vector3 delta = player.transform.position - agent.transform.position;
        bool seesPlayer = delta.sqrMagnitude < 49f
            && !Physics.Linecast(agent.transform.position + Vector3.up * 1.65f,
                player.transform.position + Vector3.up * 1.3f, 1, QueryTriggerInteraction.Ignore);
        if (seesPlayer)
        {
            if (delta.sqrMagnitude < 1.25f * 1.25f)
            {
                CaughtPlayer = true;
                StopHunting();
                player.enabled = false;
                return;
            }
            destination = player.transform.position;
            attentionUntil = Time.time + 4f;
        }

        if (Time.time >= nextPathUpdate)
        {
            nextPathUpdate = Time.time + .2f;
            if (NavMesh.SamplePosition(Hunting ? destination : home, out var point, 2f, agent.areaMask))
                agent.SetDestination(point.position);
        }
        if (motion != null && motion.clip != null)
            motion[motion.clip.name].speed = Mathf.Max(.08f, agent.velocity.magnitude / 1.1f);
    }

    void LateUpdate()
    {
        if (hips == null) return;
        // The imported clip walks forward. Navigation supplies the actual displacement.
        Vector3 drift = visualRoot.InverseTransformPoint(hips.position) - hipsOrigin;
        drift.y = 0f;
        visualRoot.localPosition = visualOrigin - Vector3.Scale(drift, visualRoot.localScale);
    }

    public void StopHunting()
    {
        stopped = true;
        if (agent != null && agent.isOnNavMesh) agent.isStopped = true;
        if (motion != null) motion.Stop();
    }
}
