using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public sealed class CaserioEscape : MonoBehaviour
{
    [SerializeField] PlayerInventory inventory;
    [SerializeField] Collider latch;
    [SerializeField] Transform gate;
    [SerializeField] Light exitLight;
    [SerializeField] Text objective;
    [SerializeField] Text prompt;
    [SerializeField] GameObject[] readingPages;
    [SerializeField] Enraizado enraizado;
    [SerializeField] EscapeTruck truck;

    const float Reach = 2f;
    const float RepairTime = 2.5f;
    float repairProgress;
    float openAngle;
    bool unlocked;
    bool crossedGate;
    bool escaped;

    void Update()
    {
        var keyboard = Keyboard.current;
        bool caught = enraizado != null && enraizado.CaughtPlayer;
        if (escaped || caught)
        {
            if (caught)
            {
                if (truck != null) truck.enabled = false;
                objective.enabled = true;
                objective.text = "El Enraizado te encontró";
                prompt.text = "Pulsa R para volver a jugar";
            }
            if (keyboard != null && keyboard.rKey.wasPressedThisFrame)
                SceneManager.LoadScene(gameObject.scene.buildIndex);
            return;
        }

        bool reading = false;
        foreach (var page in readingPages)
            reading |= page != null && page.activeInHierarchy;

        objective.enabled = !reading;
        if (unlocked)
        {
            openAngle = Mathf.MoveTowards(openAngle, 95f, 100f * Time.deltaTime);
            gate.localRotation = Quaternion.Euler(0f, -openAngle, 0f);
            UpdateEscapeRoute(keyboard, reading);
            return;
        }

        bool hasTool = inventory.HasWorkshopTool;
        objective.text = enraizado != null && enraizado.Hunting
            ? "Oyó el cierre. Escóndete"
            : hasTool ? "Vuelve a la reja del sendero" : "Busca la llave inglesa en el taller";
        bool aimed = !reading && !BedroomWarningNote.InputConsumedThisFrame
            && Cursor.lockState == CursorLockMode.Locked
            && InteractionAim.IsTargeted(latch, inventory.transform, Reach);
        if (aimed && hasTool && keyboard != null && keyboard.eKey.isPressed)
        {
            repairProgress += Time.deltaTime;
            if (repairProgress >= .35f && enraizado != null)
                enraizado.HearLatch(inventory.transform.position);
            prompt.text = $"Aflojando el cierre… {Mathf.Min(100, Mathf.FloorToInt(100f * repairProgress / RepairTime))}%";
            if (repairProgress >= RepairTime)
            {
                unlocked = true;
                latch.enabled = false;
                exitLight.color = new Color(.62f, 1f, .76f);
            }
        }
        else
        {
            repairProgress = 0f;
            prompt.text = aimed
                ? (hasTool ? "Mantén E para aflojar el cierre" : "Necesitas la llave inglesa del taller")
                : string.Empty;
        }
    }

    void UpdateEscapeRoute(Keyboard keyboard, bool reading)
    {
        prompt.text = string.Empty;
        if (!crossedGate)
        {
            objective.text = "Cruza la reja iluminada";
            return;
        }
        if (truck.Finished)
        {
            FinishEscape();
            return;
        }

        bool interact = !reading && !BedroomWarningNote.InputConsumedThisFrame
            && Cursor.lockState == CursorLockMode.Locked && keyboard != null && keyboard.eKey.wasPressedThisFrame;
        if (truck.Occupied)
        {
            objective.text = truck.Starting ? "El motor tarda en arrancar…" : "Sigue los postes para salir del bosque";
            prompt.text = truck.Starting ? string.Empty : "WASD · Conducir    Espacio · Frenar    E · Bajar al detenerte";
            if (interact) truck.Leave();
        }
        else
        {
            objective.text = "Sube a la camioneta";
            if (!reading && Cursor.lockState == CursorLockMode.Locked && truck.CanBoard(inventory))
            {
                prompt.text = "E · Subir a la camioneta";
                if (interact) truck.Board(inventory);
            }
        }
    }

    void OnTriggerStay(Collider other)
    {
        if (!unlocked || escaped || (enraizado != null && enraizado.CaughtPlayer)
            || other.GetComponentInParent<PlayerInventory>() != inventory)
            return;

        crossedGate = true;
        if (truck == null) FinishEscape();
    }

    void FinishEscape()
    {
        escaped = true;
        if (enraizado != null) enraizado.StopHunting();
        objective.enabled = true;
        objective.text = "Escapaste del caserío";
        prompt.text = "Pulsa R para volver a jugar";
    }
}
