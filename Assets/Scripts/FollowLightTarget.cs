using UnityEngine;
public class FollowLightTarget : MonoBehaviour
{
    public Transform target;
    void LateUpdate(){if(target!=null)transform.position=target.position;}
}
