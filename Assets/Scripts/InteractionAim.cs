using UnityEngine;

public static class InteractionAim
{
    public static bool IsTargeted(Collider target, Transform player, float reach)
    {
        var view = Camera.main;
        if (view == null || player == null || target == null || !target.enabled
            || !target.gameObject.activeInHierarchy || Time.timeScale <= 0)
            return false;

        Vector3 eye = player.position + Vector3.up * 1.55f;
        Ray ray = view.ViewportPointToRay(new Vector3(.5f, .5f, 0));
        float length = reach + Vector3.Distance(view.transform.position, eye) + .5f;
        if (!target.Raycast(ray, out var hit, length)
            || (hit.point - eye).sqrMagnitude > reach * reach)
            return false;

        // Trigger targets do not obstruct these visibility checks.
        return !Physics.Linecast(view.transform.position, hit.point,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore)
            && !Physics.Linecast(eye, hit.point,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
    }
}
