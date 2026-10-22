#if UNITY_EDITOR
using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.InputSystem;
using Unity.Cinemachine;
using StarterAssets;

public static class CourseLabBuilder
{
    const string Root="Assets/Practicas_S09_S10";
    const string Evidence="Entregas/Semanas9_10/Evidencias";
    static string Main="Assets/Scenes/SampleScene.unity";
    [MenuItem("Tools/Practicas/Completar semanas 9 y 10 %#&F12")]
    public static void Build()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new InvalidOperationException("Sal de Play primero.");
        if(SceneManager.GetActiveScene().path!=Main)throw new InvalidOperationException("Abre SampleScene primero.");
        Directory.CreateDirectory(Evidence);Directory.CreateDirectory("Logs");
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),"Logs/Antes_Practicas_S09_S10.unity",true);
        Folder(Root);Folder(Root+"/Materiales");Folder(Root+"/Perfiles");Folder(Root+"/Escenas");
        Tags();TerrainPath();Player();Lighting();Cameras();
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
        EvidenceMain();Orientation();Labyrinth();EmotionScenes();
        EditorSceneManager.OpenScene(Main);
        EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene(Main,true),new EditorBuildSettingsScene(Root+"/Escenas/S09_Orientacion.unity",true),new EditorBuildSettingsScene(Root+"/Escenas/S09_Laberinto_Imposible.unity",true),new EditorBuildSettingsScene(Root+"/Escenas/S10_Habitacion_Emocional.unity",true)};
        AssetDatabase.SaveAssets();
        File.WriteAllText("Logs/Practicas_Build.txt","OK: terreno con dos capas; controlador propio; dos camaras; trigger; composer; luces agrupadas; tres escenas didacticas y cinco perfiles; evidencias guardadas.");
        Debug.Log("Practicas S09 y S10 preparadas. Evidencias en Entregas/Semanas9_10. Falta el playtest humano y la entrega al foro.");
    }
    static T Component<T>(GameObject owner) where T:UnityEngine.Component { var c=owner.GetComponent<T>();return c!=null?c:owner.AddComponent<T>(); }
    static void Folder(string p){if(AssetDatabase.IsValidFolder(p))return;int k=p.LastIndexOf('/');Folder(p.Substring(0,k));AssetDatabase.CreateFolder(p.Substring(0,k),p.Substring(k+1));}
    static void Tags()
    {
        var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
        var a=tags.FindProperty("tags");foreach(var name in new[]{"Ground","Obstacle","Interactable"})
        {bool exists=false;for(int i=0;i<a.arraySize;i++)if(a.GetArrayElementAtIndex(i).stringValue==name)exists=true;if(!exists){a.InsertArrayElementAtIndex(a.arraySize);a.GetArrayElementAtIndex(a.arraySize-1).stringValue=name;}}
        tags.ApplyModifiedPropertiesWithoutUndo();
        foreach(var c in UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsSortMode.None))
            if(c.GetComponentInParent<CharacterController>()==null)c.gameObject.tag=c is TerrainCollider?"Ground":"Obstacle";
    }
    static Vector2[] PathPoints={new Vector2(0,-20),new Vector2(0,-12),new Vector2(0,-6)};
    static Vector2[] HillPoints={new Vector2(0,-12),new Vector2(-8,-7),new Vector2(-15,2),new Vector2(-25,16)};
    static float Distance(Vector2 p,Vector2[] line)
    {
        float best=1000;for(int i=1;i<line.Length;i++){var d=line[i]-line[i-1];float t=Mathf.Clamp01(Vector2.Dot(p-line[i-1],d)/d.sqrMagnitude);best=Mathf.Min(best,Vector2.Distance(p,line[i-1]+d*t));}return best;
    }
    static void TerrainPath()
    {
        var t=Terrain.activeTerrain;var d=t.terrainData;
        var dirt=AssetDatabase.LoadAssetAtPath<TerrainLayer>(Root+"/Tierra.terrainlayer");
        if(dirt==null){dirt=new TerrainLayer{name="Tierra",diffuseTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Lowpoly_Environments/Sources/Textures/PT_Ground_Generic_03.png"),tileSize=new Vector2(5,5),smoothness=0};AssetDatabase.CreateAsset(dirt,Root+"/Tierra.terrainlayer");}
        if(dirt.diffuseTexture==null)throw new InvalidOperationException("Falta la textura de tierra.");
        d.terrainLayers=new[]{d.terrainLayers[0],dirt};
        var paint=new float[d.alphamapHeight,d.alphamapWidth,2];
        for(int z=0;z<d.alphamapHeight;z++)for(int x=0;x<d.alphamapWidth;x++)
        {
            var p=new Vector2(t.transform.position.x+x/(float)(d.alphamapWidth-1)*d.size.x,t.transform.position.z+z/(float)(d.alphamapHeight-1)*d.size.z);
            float dist=Mathf.Min(Distance(p,PathPoints),Distance(p,HillPoints));float value=1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(.85f,1.8f,dist));paint[z,x,0]=1-value;paint[z,x,1]=value;
        }
        d.SetAlphamaps(0,0,paint);
        for(int layer=0;layer<d.detailPrototypes.Length;layer++)
        {
            var grass=d.GetDetailLayer(0,0,d.detailWidth,d.detailHeight,layer);
            for(int z=0;z<d.detailHeight;z++)for(int x=0;x<d.detailWidth;x++)
            {var p=new Vector2(t.transform.position.x+(x+.5f)/d.detailWidth*d.size.x,t.transform.position.z+(z+.5f)/d.detailHeight*d.size.z);if(Mathf.Min(Distance(p,PathPoints),Distance(p,HillPoints))<1.7f)grass[z,x]=0;}
            d.SetDetailLayer(0,0,layer,grass);
        }
        // Un borde de relieve real cierra el mapa; no se usan paredes invisibles.
        var heights=d.GetHeights(0,0,d.heightmapResolution,d.heightmapResolution);
        for(int z=0;z<d.heightmapResolution;z++)for(int x=0;x<d.heightmapResolution;x++)
        {
            float edge=Mathf.Min(Mathf.Min(x,z),Mathf.Min(d.heightmapResolution-1-x,d.heightmapResolution-1-z))/(float)(d.heightmapResolution-1)*d.size.x;
            float ridge=(1-Mathf.SmoothStep(0,1,Mathf.InverseLerp(0,9,edge)))*13/d.size.y;
            heights[z,x]=Mathf.Max(heights[z,x],ridge);
        }
        d.SetHeights(0,0,heights);EditorUtility.SetDirty(d);t.Flush();
        var trees=GameObject.Find("Arboles_Alrededor_Casa");if(trees!=null)foreach(Transform tree in trees.transform){var p=tree.position;p.y=t.SampleHeight(p)+t.transform.position.y;tree.position=p;}
    }
    static void Player()
    {
        var p=GameObject.Find("Superviviente_TerceraPersona");if(p==null)throw new InvalidOperationException("Falta el superviviente.");
        var old=p.GetComponent<ThirdPersonController>();var own=p.GetComponent<PlayerController3D>();
        if(own==null)own=p.AddComponent<PlayerController3D>();
        own.cameraTarget=p.transform.Find("CameraTarget");own.moveSpeed=2.1f;own.sprintSpeed=4.4f;own.jumpHeight=1.2f;own.gravity=-15;
        if(old!=null){own.footsteps=old.FootstepAudioClips;own.landing=old.LandingAudioClip;UnityEngine.Object.DestroyImmediate(old);}
        if(p.GetComponent<LabSceneMenu>()==null)p.AddComponent<LabSceneMenu>();
        p.tag="Player";PrefabUtility.RecordPrefabInstancePropertyModifications(own);
        PrefabUtility.ApplyPrefabInstance(p,InteractionMode.AutomatedAction);
    }
    static void Lighting()
    {
        var group=GameObject.Find("--- LIGHTING ---")??new GameObject("--- LIGHTING ---");
        var house=GameObject.Find("MiniCasa_Amueblada");
        if(PrefabUtility.IsPartOfPrefabInstance(house))PrefabUtility.UnpackPrefabInstance(house,PrefabUnpackMode.OutermostRoot,InteractionMode.AutomatedAction);
        foreach(var l in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
        {
            if(l.name=="Reflejo_Luna_Personaje") {var follow=Component<FollowLightTarget>(l.gameObject);follow.target=Camera.main.transform;}
            l.transform.SetParent(group.transform,true);
            l.shadows=l.type==LightType.Directional?LightShadows.Soft:LightShadows.None;
        }
        var obj=GameObject.Find("Acento_Puerta")??new GameObject("Acento_Puerta");obj.transform.SetParent(group.transform,true);
        obj.transform.position=new Vector3(0,3.5f,-7.5f);obj.transform.LookAt(new Vector3(0,1,-5.2f));
        var spot=Component<Light>(obj);spot.type=LightType.Spot;spot.color=new Color(1,.68f,.32f);spot.intensity=8;spot.range=10;spot.spotAngle=65;spot.innerSpotAngle=35;spot.shadows=LightShadows.Soft;
        var data=spot.GetUniversalAdditionalLightData();var so=new SerializedObject(data);so.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue=0;so.ApplyModifiedPropertiesWithoutUndo();
        int shadowLights=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Count(l=>l.isActiveAndEnabled && l.shadows!=LightShadows.None);
        if(shadowLights>2)throw new InvalidOperationException("Presupuesto de sombras excedido.");
    }
    static void Composer(CinemachineCamera cam)
    {
        var aim=Component<CinemachineRotationComposer>(cam.gameObject);
        aim.Damping=new Vector2(.5f,.5f);aim.Composition=ScreenComposerSettings.Default;
        aim.Composition.ScreenPosition=new Vector2(-.17f,-.05f);
        aim.Composition.DeadZone.Enabled=true;aim.Composition.DeadZone.Size=new Vector2(.15f,.12f);
    }
    static void Cameras()
    {
        var target=GameObject.Find("Superviviente_TerceraPersona").transform.Find("CameraTarget");
        var cam=GameObject.Find("Camara_Superviviente").GetComponent<CinemachineCamera>();cam.Priority=10;cam.LookAt=target;Composer(cam);
        cam.GetComponent<CinemachineThirdPersonFollow>().Damping=new Vector3(.35f,.5f,.5f);
        var zoneObject=GameObject.Find("Camara_Tension_Porche")??new GameObject("Camara_Tension_Porche");
        var zone=Component<CinemachineCamera>(zoneObject);
        zone.Follow=target;zone.LookAt=target;zone.Priority=5;zone.Lens.FieldOfView=40;zone.Lens.NearClipPlane=.08f;
        var follow=Component<CinemachineThirdPersonFollow>(zoneObject);
        follow.CameraDistance=3.3f;follow.ShoulderOffset=new Vector3(.6f,.1f,0);follow.CameraSide=1;follow.VerticalArmLength=.2f;follow.Damping=Vector3.one*.5f;
        follow.AvoidObstacles.Enabled=true;follow.AvoidObstacles.CollisionFilter=1;follow.AvoidObstacles.IgnoreTag="Player";follow.AvoidObstacles.CameraRadius=.15f;
        Composer(zone);
        Camera.main.GetComponent<CinemachineBrain>().DefaultBlend=new CinemachineBlendDefinition(CinemachineBlendDefinition.Styles.EaseInOut,.8f);
        var triggerObject=GameObject.Find("Zona_Tension_Porche")??new GameObject("Zona_Tension_Porche");triggerObject.transform.position=new Vector3(0,1.5f,-6.8f);
        var collider=Component<BoxCollider>(triggerObject);collider.isTrigger=true;collider.size=new Vector3(3,3,2.6f);
        // Un Rigidbody cinemático garantiza mensajes de trigger con CharacterController.
        var body=Component<Rigidbody>(triggerObject);body.isKinematic=true;body.useGravity=false;
        var script=Component<CameraZoneTrigger>(triggerObject);script.zoneCam=zone;
        triggerObject.tag="Interactable";
        PrefabUtility.RecordPrefabInstancePropertyModifications(cam);
        PrefabUtility.RecordPrefabInstancePropertyModifications(cam.GetComponent<CinemachineThirdPersonFollow>());
    }
    static Material Mat(string name,Color color)
    {
        var path=Root+"/Materiales/"+name+".mat";var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(mat==null){mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(mat,path);}
        mat.color=color;mat.SetFloat("_Smoothness",.18f);return mat;
    }
    static GameObject Cube(string name,Vector3 p,Vector3 scale,Material mat)
    {
        var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.position=p;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;g.tag="Obstacle";return g;
    }
    static Camera NewCamera(Vector3 position,Vector3 look)
    {
        var cam=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();cam.tag="MainCamera";cam.transform.position=position;cam.transform.LookAt(look);cam.fieldOfView=55;cam.nearClipPlane=.08f;
        cam.GetUniversalAdditionalCameraData().renderPostProcessing=true;cam.gameObject.AddComponent<LabSceneMenu>();return cam;
    }
    static void SceneLight()
    {
        RenderSettings.skybox=null;RenderSettings.fog=false;RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=new Color(.35f,.38f,.43f);
        var light=new GameObject("Luz_Principal",typeof(Light)).GetComponent<Light>();light.type=LightType.Directional;light.color=new Color(.9f,.94f,1);light.intensity=1.2f;light.shadows=LightShadows.Soft;light.transform.rotation=Quaternion.Euler(50,-30,0);
    }
    static void Orientation()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);SceneLight();
        var mat=Mat("Artifact_Azul",new Color(.08f,.45f,.8f));Cube("Suelo",new Vector3(0,-.75f,0),new Vector3(16,.5f,16),Mat("Suelo_Gris",new Color(.4f,.42f,.43f)));
        Cube("Artifact",Vector3.zero,Vector3.one,mat);
        Vector3[] positions={new Vector3(3,1,2),new Vector3(-4,2,3),new Vector3(1,4,-3),new Vector3(-2,1,-5),new Vector3(5,3,-1)};
        for(int i=0;i<positions.Length;i++){var sphere=GameObject.CreatePrimitive(PrimitiveType.Sphere);sphere.name="Target_"+(i+1);sphere.transform.position=positions[i];sphere.GetComponent<MeshRenderer>().enabled=false;sphere.GetComponent<Collider>().enabled=false;}
        NewCamera(new Vector3(11,10,-14),new Vector3(0,1,0));
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),Root+"/Escenas/S09_Orientacion.unity");
        Capture(Camera.main,Evidence+"/S09_Orientacion.png");
    }
    static void Labyrinth()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);SceneLight();
        var stone=Mat("Laberinto_Piedra",new Color(.65f,.73f,.8f));var path=Mat("Laberinto_Camino",new Color(.35f,.62f,.65f));
        Cube("Base",new Vector3(0,-.6f,0),new Vector3(15,1,15),Mat("Base_Oscura",new Color(.1f,.14f,.2f)));
        Cube("Camino_A",new Vector3(-2,1,0),new Vector3(1,.4f,9),path);
        Cube("Camino_B",new Vector3(1,1,4),new Vector3(7,.4f,1),path);
        var cam=NewCamera(new Vector3(14,13,-14),new Vector3(0,2,0));cam.orthographic=true;cam.orthographicSize=10;
        // Desplazar una pieza a lo largo del eje visual conserva su proyección ortográfica.
        var apparentJoin=new Vector3(4,1,4);var elevated=apparentJoin-cam.transform.forward*4;
        Cube("Camino_C_Separado_En_Profundidad",elevated+new Vector3(0,0,-3),new Vector3(1,.4f,7),path);
        for(int i=0;i<5;i++)Cube("Muro_"+i,new Vector3(-4+i*2,2,-3+i%2*3),new Vector3(1,3,1),stone);
        foreach(var g in new[]{new Vector3(-2,0,-4),new Vector3(4,0,4),new Vector3(1,0,4)})Cube("Columna",g,new Vector3(.7f,2,.7f),stone);
        var toggle=new GameObject("Revelar_Perspectiva").AddComponent<LabPerspectiveToggle>();toggle.view=cam;
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),Root+"/Escenas/S09_Laberinto_Imposible.unity");
        Capture(cam,Evidence+"/S09_Laberinto_Ortografico.png");
        cam.orthographic=false;cam.transform.position=new Vector3(-12,13,-12);cam.transform.LookAt(new Vector3(0,2,0));Capture(cam,Evidence+"/S09_Laberinto_Profundidad.png");
    }
    static VolumeProfile Profile(string name,float exposure,float contrast,float saturation,Color filter,float vignette,float bloom)
    {
        var path=Root+"/Perfiles/"+name+".asset";var p=AssetDatabase.LoadAssetAtPath<VolumeProfile>(path);
        if(p==null){p=ScriptableObject.CreateInstance<VolumeProfile>();AssetDatabase.CreateAsset(p,path);}
        var c=p.TryGet<ColorAdjustments>(out var cc)?cc:p.Add<ColorAdjustments>(true);c.postExposure.Override(exposure);c.contrast.Override(contrast);c.saturation.Override(saturation);c.colorFilter.Override(filter);
        var v=p.TryGet<Vignette>(out var vv)?vv:p.Add<Vignette>(true);v.intensity.Override(vignette);v.smoothness.Override(.4f);
        var b=p.TryGet<Bloom>(out var bb)?bb:p.Add<Bloom>(true);b.intensity.Override(bloom);b.threshold.Override(1);
        var t=p.TryGet<Tonemapping>(out var tt)?tt:p.Add<Tonemapping>(true);t.mode.Override(TonemappingMode.ACES);
        foreach(var component in p.components){if(!AssetDatabase.Contains(component))AssetDatabase.AddObjectToAsset(component,p);EditorUtility.SetDirty(component);}EditorUtility.SetDirty(p);return p;
    }
    static void EmotionScenes()
    {
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);SceneLight();
        var material=Mat("Habitacion_Neutra",new Color(.65f,.65f,.65f));
        Cube("Suelo",new Vector3(0,-.15f,0),new Vector3(8,.3f,8),material);
        Cube("Muro_Izquierdo",new Vector3(-4,2,0),new Vector3(.25f,4,8),material);Cube("Muro_Derecho",new Vector3(4,2,0),new Vector3(.25f,4,8),material);
        Cube("Muro_Fondo",new Vector3(0,2,4),new Vector3(8,4,.25f),material);
        Cube("Muro_Frontal_Izq",new Vector3(-2.5f,2,-4),new Vector3(3,4,.25f),material);Cube("Muro_Frontal_Der",new Vector3(2.5f,2,-4),new Vector3(3,4,.25f),material);
        Cube("Dintel_Puerta",new Vector3(0,3.5f,-4),new Vector3(2,1,.25f),material);
        Cube("Cubo_Central",new Vector3(0,.75f,0),new Vector3(1.5f,1.5f,1.5f),material);
        var cam=NewCamera(new Vector3(0,2,-8),new Vector3(0,1.2f,1));
        var volume=new GameObject("Global Volume").AddComponent<Volume>();volume.isGlobal=true;
        var room=new GameObject("Control_Emociones").AddComponent<EmotionRoom>();room.volume=volume;room.keyLight=UnityEngine.Object.FindFirstObjectByType<Light>();
        var accent=new GameObject("Acento_Puerta",typeof(Light)).GetComponent<Light>();accent.type=LightType.Point;accent.transform.position=new Vector3(0,2.5f,-3.5f);accent.range=6;room.accentLight=accent;
        room.profiles=new[]{Profile("Terror",-.25f,18,-25,new Color(.72f,.84f,1),.3f,.1f),Profile("Calma",.25f,-12,5,new Color(1,.94f,.84f),.08f,.35f),Profile("Accion",.2f,20,15,new Color(1,.95f,.9f),.12f,.2f),Profile("Esperanza",.4f,-8,8,new Color(1,.95f,.82f),.07f,.3f),Profile("Misterio",-.3f,15,-15,new Color(.77f,.9f,1),.25f,.1f)};
        for(int i=0;i<room.profiles.Length;i++){room.Select(i);Capture(cam,Evidence+"/S10_Habitacion_"+room.names[i]+".png");}
        room.Select(0);EditorSceneManager.SaveScene(SceneManager.GetActiveScene(),Root+"/Escenas/S10_Habitacion_Emocional.unity");
    }
    static void EvidenceMain()
    {
        var temp=new GameObject("Camara_Evidencia",typeof(Camera));var cam=temp.GetComponent<Camera>();cam.enabled=false;cam.transform.position=new Vector3(10,3.2f,-16);cam.transform.LookAt(new Vector3(0,1.6f,-1));cam.fieldOfView=65;cam.GetUniversalAdditionalCameraData().renderPostProcessing=true;
        var lights=UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None);var states=lights.Select(l=>l.enabled).ToArray();var vols=UnityEngine.Object.FindObjectsByType<Volume>(FindObjectsSortMode.None);var weights=vols.Select(v=>v.weight).ToArray();
        var sky=RenderSettings.skybox;var ambient=RenderSettings.ambientMode;var ambientColor=RenderSettings.ambientLight;bool fog=RenderSettings.fog;
        try
        {
            for(int i=0;i<lights.Length;i++)lights[i].enabled=false;foreach(var v in vols)v.weight=0;
            RenderSettings.skybox=null;RenderSettings.ambientMode=AmbientMode.Flat;RenderSettings.ambientLight=Color.black;RenderSettings.fog=false;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.08f,.08f,.08f);
            Capture(cam,Evidence+"/S10_Nivel_Antes_Sin_Luces.png");
        }
        finally
        {
            for(int i=0;i<lights.Length;i++)lights[i].enabled=states[i];for(int i=0;i<vols.Length;i++)vols[i].weight=weights[i];RenderSettings.skybox=sky;RenderSettings.ambientMode=ambient;RenderSettings.ambientLight=ambientColor;RenderSettings.fog=fog;cam.clearFlags=CameraClearFlags.Skybox;
        }
        Capture(cam,Evidence+"/S10_Nivel_Despues.png");UnityEngine.Object.DestroyImmediate(temp);
        EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
    }
    [MenuItem("Tools/Practicas/Crear build Windows de entrega")]
    public static void BuildDemo()
    {
        Directory.CreateDirectory("Builds/Practicas_S09_S10");
        var settings=new BuildPlayerOptions {
            scenes=new[]{Root+"/Escenas/S10_Habitacion_Emocional.unity",Root+"/Escenas/S09_Laberinto_Imposible.unity",Root+"/Escenas/S09_Orientacion.unity",Main},
            locationPathName="Builds/Practicas_S09_S10/Practicas.exe",target=BuildTarget.StandaloneWindows64,options=BuildOptions.None
        };
        var report=BuildPipeline.BuildPlayer(settings);
        if(report.summary.result!=UnityEditor.Build.Reporting.BuildResult.Succeeded)throw new InvalidOperationException("Build de entrega: "+report.summary.result);
        File.WriteAllText("Logs/CourseBuildWindows.txt","OK: "+report.summary.totalSize+" bytes; "+report.summary.totalTime);
    }
    public static void Capture(Camera camera,string path)
    {
        bool previousAsync=ShaderUtil.allowAsyncCompilation;ShaderUtil.allowAsyncCompilation=false;
        Directory.CreateDirectory(Path.GetDirectoryName(path));
        var target=new RenderTexture(1024,768,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);var previous=RenderTexture.active;Texture2D pixels=null;
        try{target.Create();RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest{destination=target});RenderTexture.active=target;pixels=new Texture2D(1024,768,TextureFormat.RGB24,false);pixels.ReadPixels(new Rect(0,0,1024,768),0,0);pixels.Apply();File.WriteAllBytes(path,pixels.EncodeToPNG());}
        finally{ShaderUtil.allowAsyncCompilation=previousAsync;RenderTexture.active=previous;if(pixels!=null)UnityEngine.Object.DestroyImmediate(pixels);target.Release();UnityEngine.Object.DestroyImmediate(target);}
    }
}
#endif




