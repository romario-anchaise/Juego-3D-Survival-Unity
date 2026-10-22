using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class LabSceneMenu : MonoBehaviour
{
    bool open;
    void Update(){if(Keyboard.current!=null && Keyboard.current.f1Key.wasPressedThisFrame){open=!open;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}}
    void OnGUI()
    {
        if(!open)return;
        GUI.Box(new Rect(15,Screen.height-125,650,110),"Practicas: elegir escena (F1 muestra u oculta este menu)");
        string[] scenes={"SampleScene","S09_Orientacion","S09_Laberinto_Imposible","S10_Habitacion_Emocional"};
        string[] names={"Casa y bosque","Orientacion","Laberinto","Emociones"};
        for(int i=0;i<scenes.Length;i++)if(GUI.Button(new Rect(25+i*155,Screen.height-85,145,40),names[i]))SceneManager.LoadScene(scenes[i]);
    }
}
