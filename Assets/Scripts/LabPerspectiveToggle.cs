using UnityEngine;
using UnityEngine.InputSystem;

public class LabPerspectiveToggle : MonoBehaviour
{
    public Camera view;
    Vector3 originalPosition;Quaternion originalRotation;bool reveal;
    void Awake(){originalPosition=view.transform.position;originalRotation=view.transform.rotation;}
    void Update()
    {
        if(Keyboard.current!=null && Keyboard.current.pKey.wasPressedThisFrame)
        {
            reveal=!reveal;view.orthographic=!reveal;
            if(reveal){view.transform.position=new Vector3(-12,13,-12);view.transform.LookAt(new Vector3(0,2,0));}
            else view.transform.SetPositionAndRotation(originalPosition,originalRotation);
        }
    }
    void OnGUI(){GUI.Box(new Rect(15,15,680,65),"Laberinto Imposible: los caminos coinciden en pantalla, pero estan separados en 3D.\nPulsa P para revelar la profundidad con otra perspectiva.");}
}
