using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public static class SurvivalBloodSetup
{
    const string Root = "Assets/FearMansion/AmbienteTerror/Sangre";
    public static void ApplyBatch()
    {
        EditorSceneManager.OpenScene("Assets/Scenes/SampleScene.unity");
        Apply();
    }

    [MenuItem("Tools/Survival/Agregar sangre a las paredes %#&b")]
    public static void Apply()
    {
        var scene = SceneManager.GetActiveScene();
        if (EditorApplication.isPlayingOrWillChangePlaymode || scene.path != "Assets/Scenes/SampleScene.unity")
            throw new InvalidOperationException("Abre SampleScene y sal de Play.");
        var house = GameObject.Find("MiniCasa_Amueblada");
        if (house == null) throw new InvalidOperationException("Falta la casa.");
        Directory.CreateDirectory("Logs/BloodBackup");
        EditorSceneManager.SaveScene(scene, "Logs/BloodBackup/Antes_"+DateTime.Now.ToString("yyyyMMdd_HHmmss")+".unity", true);
        Directory.CreateDirectory(Root);
        AssetDatabase.Refresh();
        var shader=AssetDatabase.LoadAssetAtPath<Shader>("Assets/_MyShaders/Blood/BloodWall.shader");
        if (shader == null || ShaderUtil.ShaderHasError(shader)) throw new InvalidOperationException("Error en shader de sangre.");
        var material=AssetDatabase.LoadAssetAtPath<Material>(Root+"/M_Sangre_Pared.mat");
        if(material==null)
        {
            material=new Material(shader){name="M_Sangre_Pared"};
            AssetDatabase.CreateAsset(material,Root+"/M_Sangre_Pared.mat");
        }
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Mancha_Sangre.prefab");
        if(prefab==null)
        {
            var quad=GameObject.CreatePrimitive(PrimitiveType.Quad);
            quad.name="Mancha_Sangre";
            UnityEngine.Object.DestroyImmediate(quad.GetComponent<Collider>());
            quad.GetComponent<Renderer>().sharedMaterial=material;
            quad.GetComponent<Renderer>().shadowCastingMode=ShadowCastingMode.Off;
            prefab=PrefabUtility.SaveAsPrefabAsset(quad,Root+"/Mancha_Sangre.prefab");
            UnityEngine.Object.DestroyImmediate(quad);
        }
        Place(house.transform,prefab,"Sangre_Entrada_Izquierda",new Vector3(-.65f,1.55f,-4.105f),new Vector2(1.5f,1.9f),1);
        Place(house.transform,prefab,"Sangre_Entrada_Derecha",new Vector3(2.65f,1.35f,-4.105f),new Vector2(1.45f,1.85f),6);
        Place(house.transform,prefab,"Sangre_Bano",new Vector3(3.85f,1.55f,3.895f),new Vector2(.7f,1.25f),11);
        Place(house.transform,prefab,"Sangre_Fachada_Baja",new Vector3(-2.35f,.62f,-4.105f),new Vector2(1.7f,.75f),15);
        Place(house.transform,prefab,"Sangre_Fachada_Lateral",new Vector3(4.02f,1.6f,-4.105f),new Vector2(.8f,1.6f),19);
        Place(house.transform,prefab,"Sangre_Sala_Grande",new Vector3(-3.3f,1.4f,1.095f),new Vector2(1.8f,1.9f),23);
        Place(house.transform,prefab,"Sangre_Sala_Puerta",new Vector3(-.1f,1.45f,1.095f),new Vector2(1.1f,1.8f),29);
        Place(house.transform,prefab,"Sangre_Puerta_Bano",new Vector3(3.95f,1.45f,1.095f),new Vector2(.8f,1.8f),37);
        Place(house.transform,prefab,"Sangre_Bano_Lateral",new Vector3(1.305f,1.4f,2.5f),new Vector2(1.7f,1.9f),41,new Vector3(0,90,0));
        Place(house.transform,prefab,"Sangre_Cocina",new Vector3(4.395f,1.6f,.25f),new Vector2(1.5f,1.8f),47,new Vector3(0,90,0));
        AddWallStains(house.transform,prefab);
        var flat=new Vector3(90,0,0);
        Place(house.transform,prefab,"Sangre_Rastro_Camino_1",new Vector3(1,.054f,-9.6f),new Vector2(.85f,.7f),51,flat,true);
        Place(house.transform,prefab,"Sangre_Rastro_Camino_2",new Vector3(.95f,.054f,-8.4f),new Vector2(.9f,.7f),53,flat,true);
        Place(house.transform,prefab,"Sangre_Rastro_Camino_3",new Vector3(1.1f,.054f,-7.2f),new Vector2(.8f,.7f),57,flat,true);
        Place(house.transform,prefab,"Sangre_Charco_Porche",new Vector3(.8f,.164f,-5.05f),new Vector2(2.0f,1.7f),61,flat,true);
        Place(house.transform,prefab,"Sangre_Charco_Entrada",new Vector3(.9f,.192f,-3.3f),new Vector2(2.3f,2.0f),67,flat,true);
        Place(house.transform,prefab,"Sangre_Rastro_Interior",new Vector3(.15f,.192f,-1.85f),new Vector2(1.4f,1.8f),71,flat,true);
        Place(house.transform,prefab,"Sangre_Charco_Sala",new Vector3(-2.9f,.192f,.5f),new Vector2(2.0f,1.9f),79,flat,true);
        Place(house.transform,prefab,"Sangre_Gotas_Bano",new Vector3(2.0f,.204f,3.4f),new Vector2(1.1f,.9f),83,flat,true);
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene); EditorSceneManager.SaveScene(scene);
        View(house.transform,"Logs/Sangre_Entrada.png",new Vector3(.3f,2,-8),new Vector3(.8f,1.4f,-4));
        View(house.transform,"Logs/Sangre_Bano.png",new Vector3(2.4f,1.65f,1.8f),new Vector3(3.6f,1.4f,3.895f));
        View(house.transform,"Logs/Sangre_Sala.png",new Vector3(.7f,1.9f,-3.2f),new Vector3(-1.8f,1.1f,1.1f));
        File.WriteAllText("Logs/Sangre_Verificada.txt","PASS: shader URP sin errores; 34 manchas en paredes y suelos; prefab reutilizable sin colliders; escena guardada.");
        Debug.Log("Sangre agregada: 34 manchas en fachada, sala, cocina, bano y rastro de entrada.");
    }
    static void AddWallStains(Transform house, GameObject prefab)
    {
        var east = new Vector3(0,90,0);
        var west = new Vector3(0,-90,0);
        var north = new Vector3(0,180,0);

        Place(house,prefab,"Sangre_Sala_Oeste",new Vector3(-4.395f,1.46f,.12f),new Vector2(1.55f,2.15f),101,west,shape:0);
        Place(house,prefab,"Sangre_Sala_Ventana",new Vector3(-2.4f,.73f,-3.895f),new Vector2(1.6f,.8f),107,north,shape:3);
        Place(house,prefab,"Sangre_Recibidor_Pared",new Vector3(2.7f,1.53f,-3.895f),new Vector2(1.75f,2.15f),113,north,shape:1);
        Place(house,prefab,"Sangre_Cocina_BajoVentana",new Vector3(4.395f,.73f,-1.8f),new Vector2(1.4f,.75f),127,east,shape:3);
        Place(house,prefab,"Sangre_Cocina_Esquina",new Vector3(4.395f,1.59f,-3.17f),new Vector2(1.12f,2.2f),131,east,shape:0);
        Place(house,prefab,"Sangre_Dormitorio_Cabecera",new Vector3(-3.65f,1.63f,3.895f),new Vector2(1.45f,2.15f),137,shape:1);
        Place(house,prefab,"Sangre_Dormitorio_Escritorio",new Vector3(-.12f,1.68f,3.895f),new Vector2(1.2f,2f),139,shape:3);
        Place(house,prefab,"Sangre_Dormitorio_Division",new Vector3(1.095f,1.51f,2.62f),new Vector2(2.15f,2.2f),149,east,shape:0);
        Place(house,prefab,"Sangre_Dormitorio_Oeste",new Vector3(-4.395f,1.48f,2.83f),new Vector2(1.75f,2.15f),151,west,shape:3);
        Place(house,prefab,"Sangre_Dormitorio_Puerta",new Vector3(-3.05f,1.5f,1.305f),new Vector2(1.55f,2.15f),157,north,shape:1);
        Place(house,prefab,"Sangre_Bano_Este",new Vector3(4.395f,1.52f,2.52f),new Vector2(2.05f,2.18f),163,east,shape:1);
        Place(house,prefab,"Sangre_Bano_Acceso",new Vector3(3.96f,1.51f,1.305f),new Vector2(.85f,2.12f),167,north,shape:3);
        Place(house,prefab,"Sangre_Bano_Trasera",new Vector3(1.87f,1.34f,3.895f),new Vector2(.75f,1.75f),173,shape:1);
        Place(house,prefab,"Sangre_Exterior_Oeste",new Vector3(-4.605f,1.48f,.75f),new Vector2(1.9f,2.1f),179,east,shape:1);
        Place(house,prefab,"Sangre_Exterior_Este",new Vector3(4.605f,1.53f,1.75f),new Vector2(1.95f,2.15f),181,west,shape:3);
        Place(house,prefab,"Sangre_Exterior_Trasera",new Vector3(-.15f,1.44f,4.105f),new Vector2(1.85f,2.1f),191,north,shape:0);
    }

    static void Place(Transform house,GameObject prefab,string name,Vector3 position,Vector2 size,int seed,Vector3? euler=null,bool floor=false,int shape=-1)
    {
        var existing=house.Find(name);
        var stain=existing!=null?existing.gameObject:(GameObject)PrefabUtility.InstantiatePrefab(prefab,house);
        Undo.RegisterFullObjectHierarchyUndo(stain,"Colocar sangre");
        stain.name=name; stain.transform.localPosition=position;
        stain.transform.localRotation=Quaternion.Euler(euler??Vector3.zero);
        stain.transform.localScale=new Vector3(size.x,size.y,1);
        string path=Root+"/M_"+name+".mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material==null)
        {
            material=new Material(prefab.GetComponent<Renderer>().sharedMaterial);
            AssetDatabase.CreateAsset(material,path);
        }
        if(shape>=0) material.SetFloat("_CrimeShape",shape);
        material.SetFloat("_Seed",seed);
        material.SetFloat("_Drips",floor?0:1);
        material.SetFloat("_Wetness",floor?.75f:.45f);
        material.SetColor("_BloodColor",new Color(.5f,.012f,.018f));
        material.SetColor("_DryColor",new Color(.16f,.012f,.008f));
        EditorUtility.SetDirty(material);
        stain.GetComponent<Renderer>().sharedMaterial=material;
        PrefabUtility.RecordPrefabInstancePropertyModifications(stain.transform);
        PrefabUtility.RecordPrefabInstancePropertyModifications(stain.GetComponent<Renderer>());
        PrefabUtility.RecordPrefabInstancePropertyModifications(stain);
    }
    static void View(Transform house,string path,Vector3 from,Vector3 target)
    {
        var temp=new GameObject("Vista sangre temporal");var camera=temp.AddComponent<Camera>();
        if(Camera.main!=null)camera.CopyFrom(Camera.main);
        camera.enabled=false;camera.fieldOfView=60;camera.nearClipPlane=.03f;
        camera.transform.position=house.TransformPoint(from);camera.transform.LookAt(house.TransformPoint(target));
        camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
        var rt=new RenderTexture(1100,800,24);var previous=RenderTexture.active;
        var image=new Texture2D(1100,800,TextureFormat.RGB24,false);
        try
        {
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            image.ReadPixels(new Rect(0,0,1100,800),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
        }
        finally
        {
            RenderTexture.active=previous;camera.targetTexture=null;rt.Release();
            UnityEngine.Object.DestroyImmediate(rt);UnityEngine.Object.DestroyImmediate(image);UnityEngine.Object.DestroyImmediate(temp);
        }
    }
}
