using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;

public class EmotionRoom : MonoBehaviour
{
    public Volume volume;
    public VolumeProfile[] profiles;
    public Light keyLight, accentLight;
    public string[] names={"Terror","Calma","Accion","Esperanza","Misterio"};
    public int selected;
    public void Select(int index)
    {
        selected=Mathf.Clamp(index,0,profiles.Length-1);volume.sharedProfile=profiles[selected];
        Color[] colors={new Color(.4f,.6f,1),new Color(1,.79f,.55f),new Color(1,.4f,.15f),new Color(1,.87f,.62f),new Color(.4f,.85f,.82f)};
        float[] energy={.75f,1.4f,1.6f,1.65f,.65f};
        keyLight.color=colors[selected];keyLight.intensity=energy[selected];
        keyLight.transform.rotation=Quaternion.Euler(selected==0?new Vector3(-20,-25,0):selected==4?new Vector3(12,85,0):new Vector3(55,-25,0));
        accentLight.color=selected==2?Color.cyan:new Color(1,.7f,.35f);
        accentLight.intensity=selected==4?0:2;
        RenderSettings.ambientMode=AmbientMode.Flat;
        RenderSettings.ambientLight=selected==0||selected==4?new Color(.12f,.14f,.19f):new Color(.35f,.33f,.3f);
    }
    void Start(){Select(selected);}
    void Update()
    {
        var k=Keyboard.current;if(k==null)return;
        if(k.digit1Key.wasPressedThisFrame)Select(0);if(k.digit2Key.wasPressedThisFrame)Select(1);
        if(k.digit3Key.wasPressedThisFrame)Select(2);if(k.digit4Key.wasPressedThisFrame)Select(3);
        if(k.digit5Key.wasPressedThisFrame)Select(4);
    }
    void OnGUI()
    {
        GUI.Box(new Rect(15,15,560,95),"La misma habitacion, distintas emociones: "+names[selected]);
        for(int i=0;i<names.Length;i++) if(GUI.Button(new Rect(25+i*108,55,103,32),(i+1)+" "+names[i]))Select(i);
    }
}
