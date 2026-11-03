using UnityEngine;

public class PlayerAnimationEvents : MonoBehaviour
{
    PlayerController3D player;

    void Awake()
    {
        player = GetComponentInParent<PlayerController3D>();
    }

    void OnFootstep(AnimationEvent animationEvent)
    {
        if (player != null)
        {
            player.SendMessage(
                "OnFootstep",
                animationEvent,
                SendMessageOptions.DontRequireReceiver
            );
        }
    }

    void OnLand(AnimationEvent animationEvent)
    {
        if (player != null)
        {
            player.SendMessage(
                "OnLand",
                animationEvent,
                SendMessageOptions.DontRequireReceiver
            );
        }
    }
}