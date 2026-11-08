using UnityEngine;

// Inventory belongs to this player and resets when a new game starts.
public sealed class PlayerInventory : MonoBehaviour
{
    public bool HasWorkshopTool { get; private set; }

    public bool TryCollectWorkshopTool()
    {
        if (HasWorkshopTool) return false;
        HasWorkshopTool = true;
        return true;
    }
}
