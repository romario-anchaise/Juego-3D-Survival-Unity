#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public static class FearMansionHorrorAtmosphere
{
    const string Folder="Assets/FearMansion/AmbienteTerror";

    [MenuItem("Tools/Fear Mansion/Apply Horror Night %#&F10")]
    public static void ApplyNight()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Sal de Play para aplicar el ambiente.");
        StageUtility.GoToMainStage();
        EnsureFolder(Folder);
        var skyShader=Shader.Find("FearMansion/CieloNoche");
        if(skyShader==null || !skyShader.isSupported)
            throw new InvalidOperationException("El cielo nocturno aun no ha terminado de importar.");
        var sky=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Cielo_Noche.mat");
        if(sky==null) {
            sky=new Material(skyShader){name="Cielo_Noche"};
            AssetDatabase.CreateAsset(sky,Folder+"/Cielo_Noche.mat");
        }
        sky.SetColor("_Zenith",new Color(.14f,.18f,.28f));
        sky.SetColor("_Horizon",new Color(.23f,.29f,.4f));
        sky.SetColor("_Clouds",new Color(.22f,.26f,.35f));
        EditorUtility.SetDirty(sky);
        Undo.RecordObject(Unsupported.GetRenderSettings(),"Aplicar noche de terror");
        var allSky=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Cielo_AllSky_Terror.mat");
        RenderSettings.skybox=allSky!=null?allSky:sky;
        RenderSettings.fog=true;
        RenderSettings.fogMode=FogMode.ExponentialSquared;
        RenderSettings.fogColor=new Color(.035f,.05f,.075f);
        RenderSettings.fogDensity=.023f;
        RenderSettings.ambientMode=AmbientMode.Trilight;
        RenderSettings.ambientSkyColor=new Color(.13f,.16f,.24f);
        RenderSettings.ambientEquatorColor=new Color(.08f,.1f,.15f);
        RenderSettings.ambientGroundColor=new Color(.035f,.045f,.07f);
        RenderSettings.ambientIntensity=1;
        RenderSettings.reflectionIntensity=.25f;

        Light moon=null;
        foreach(var light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None)) {
            if(light.type!=LightType.Directional || !light.isActiveAndEnabled) continue;
            Undo.RecordObject(light,"Atenuar luz para la noche");
            light.intensity=.5f;
            light.color=new Color(.48f,.63f,1);
            light.shadows=LightShadows.Soft;
            light.shadowStrength=.85f;
            if(moon==null) moon=light;
        }
        if(moon==null) {
            var g=new GameObject("Luz_Luna");
            moon=g.AddComponent<Light>(); moon.type=LightType.Directional;
            moon.color=new Color(.48f,.63f,1); moon.intensity=.5f; moon.shadows=LightShadows.Soft;
            Undo.RegisterCreatedObjectUndo(g,"Crear luz de luna");
        }
        Undo.RecordObject(moon.transform,"Orientar luz de luna");
        moon.transform.rotation=Quaternion.LookRotation(-new Vector3(.35f,.28f,.88f).normalized);
        RenderSettings.sun=moon;

        var house=GameObject.Find("MiniCasa_Amueblada");
        if(house!=null) foreach(var light in house.GetComponentsInChildren<Light>(true)) {
            if(light.type!=LightType.Point) continue;
            Undo.RecordObject(light,"Luces calidas de la casa");
            light.color=new Color(1,.69f,.36f);
            light.intensity=light.name.Contains("Porche")?2.4f:3.2f;
            light.range=light.name.Contains("Porche")?5:6;
            light.shadows=LightShadows.Soft;
            var lightData=light.GetComponent<UniversalAdditionalLightData>();
            if(lightData==null) lightData=Undo.AddComponent<UniversalAdditionalLightData>(light.gameObject);
            var serialized=new SerializedObject(lightData);
            serialized.FindProperty("m_AdditionalLightsShadowResolutionTier").intValue=UniversalAdditionalLightData.AdditionalLightsShadowResolutionTierLow;
            serialized.ApplyModifiedProperties();
            light.shadowBias=.03f; light.shadowNormalBias=.2f;
        }

        var profile=AssetDatabase.LoadAssetAtPath<VolumeProfile>(Folder+"/Noche_Terror_Profile.asset");
        if(profile==null) {
            profile=ScriptableObject.CreateInstance<VolumeProfile>();
            AssetDatabase.CreateAsset(profile,Folder+"/Noche_Terror_Profile.asset");
            var colors=profile.Add<ColorAdjustments>(true);
            colors.postExposure.value=-.15f; colors.contrast.value=14;
            colors.saturation.value=-22; colors.colorFilter.value=new Color(.88f,.94f,1);
            var vignette=profile.Add<Vignette>(true);
            vignette.intensity.value=.25f; vignette.smoothness.value=.45f;
            var bloom=profile.Add<Bloom>(true);
            bloom.threshold.value=1; bloom.intensity.value=.15f; bloom.scatter.value=.5f;
            var tonemap=profile.Add<Tonemapping>(true); tonemap.mode.value=TonemappingMode.ACES;
            foreach(var component in profile.components) AssetDatabase.AddObjectToAsset(component,profile);
            EditorUtility.SetDirty(profile);
        }
        if(profile.TryGet<ColorAdjustments>(out var grading)) {
            grading.postExposure.value=0; grading.contrast.value=5; grading.saturation.value=-18;
            EditorUtility.SetDirty(grading);
        }
        var environment=GameObject.Find("Ambiente_Terror");
        if(environment==null) {
            environment=new GameObject("Ambiente_Terror");
            Undo.RegisterCreatedObjectUndo(environment,"Crear ambiente de terror");
        }
        var volume=environment.GetComponent<Volume>();
        if(volume==null) volume=Undo.AddComponent<Volume>(environment);
        Undo.RecordObject(volume,"Configurar perfil nocturno");
        volume.isGlobal=true; volume.priority=100; volume.weight=1; volume.sharedProfile=profile;
        foreach(var camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)) {
            Undo.RecordObject(camera,"Mostrar cielo nocturno");
            camera.clearFlags=CameraClearFlags.Skybox; camera.allowHDR=true;
            var data=camera.GetComponent<UniversalAdditionalCameraData>();
            if(data==null) data=Undo.AddComponent<UniversalAdditionalCameraData>(camera.gameObject);
            Undo.RecordObject(data,"Activar posprocesado nocturno");
            data.renderPostProcessing=true;
        }
        AssetDatabase.SaveAssets();
        DynamicGI.UpdateEnvironment();
        EditorSceneManager.MarkSceneDirty(environment.scene);
        Validate(profile);
        Selection.activeGameObject=environment;
        if(SceneView.lastActiveSceneView!=null) {
            var sceneView=SceneView.lastActiveSceneView;
            sceneView.sceneLighting=true;
            sceneView.sceneViewState.showFog=true;
            sceneView.sceneViewState.showSkybox=true;
            sceneView.sceneViewState.showImageEffects=true;
            if(house!=null) sceneView.LookAt(house.transform.position+new Vector3(0,1.8f,0),Quaternion.Euler(12,-18,0),11,false,true);
            sceneView.Repaint();
        }
        Debug.Log("Ambiente de terror verificado: cielo nocturno con luna y nubes, niebla fria, luz lunar tenue, sombras y luces calidas en la casa. Sin destellos ni cambios de camara.");
    }
    [MenuItem("Tools/Fear Mansion/Apply AllSky Horror Sky %#&F6")]
    public static void ApplyAllSky()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Sal de Play para cambiar el cielo.");
        var scene=EditorSceneManager.GetActiveScene();
        if(scene.path!="Assets/Scenes/SampleScene.unity")
            throw new InvalidOperationException("Abre SampleScene antes de integrar AllSky.");
        var source=AssetDatabase.LoadAssetAtPath<Material>("Assets/AllSkyFree/Night MoonBurst/Night Moon Burst.mat");
        if(source==null || !source.shader.isSupported)
            throw new InvalidOperationException("Importa AllSky Free con el cielo Night MoonBurst.");
        foreach(var face in new[]{"_FrontTex","_BackTex","_LeftTex","_RightTex","_UpTex","_DownTex"})
            if(source.GetTexture(face)==null) throw new InvalidOperationException("Falta una textura del cielo: "+face);
        System.IO.Directory.CreateDirectory("Logs");
        EditorSceneManager.SaveScene(scene,"Logs/Antes_AllSky.unity",true);
        EnsureFolder(Folder);
        var sky=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Cielo_AllSky_Terror.mat");
        if(sky==null) {
            sky=new Material(source){name="Cielo_AllSky_Terror"};
            AssetDatabase.CreateAsset(sky,Folder+"/Cielo_AllSky_Terror.mat");
        }
        sky.shader=Shader.Find("Skybox/6 Sided");
        if(sky.shader==null || !sky.shader.isSupported)
            throw new InvalidOperationException("No esta disponible el shader Skybox/6 Sided.");
        sky.SetColor("_Tint",new Color(.48f,.49f,.52f));
        sky.SetFloat("_Exposure",1.15f);
        sky.SetFloat("_Rotation",20f);
        EditorUtility.SetDirty(sky);
        Undo.RecordObject(Unsupported.GetRenderSettings(),"Integrar cielo AllSky");
        RenderSettings.skybox=sky;
        // Keep the existing ambient light and fog so changing the backdrop does not darken the player.
        foreach(var camera in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)) {
            Undo.RecordObject(camera,"Mostrar AllSky");
            camera.clearFlags=CameraClearFlags.Skybox;
        }
        if(SceneView.lastActiveSceneView!=null) {
            SceneView.lastActiveSceneView.sceneViewState.showSkybox=true;
            SceneView.lastActiveSceneView.Repaint();
        }
        DynamicGI.UpdateEnvironment();
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);
        RenderPreview();
        System.IO.File.WriteAllText("Logs/AllSky_Integracion.txt",
            "OK: Night MoonBurst; Shader="+sky.shader.name+"; Faces=6; Exposure="+sky.GetFloat("_Exposure")+
            "; Fog="+RenderSettings.fog+"; Ambient="+RenderSettings.ambientMode+
            "; Camera="+(Camera.main!=null?Camera.main.name:"missing"));
        Debug.Log("AllSky integrado: Night MoonBurst, nubes oscuras y luna. Escena guardada.");
    }
    [MenuItem("Tools/Fear Mansion/Render Horror Preview %#&F11")]
    static void RenderPreview() {
        var house=GameObject.Find("MiniCasa_Amueblada");
        if(house==null) return;
        var go=new GameObject("Vista_Temporal_Nocturna");
        go.hideFlags=HideFlags.HideAndDontSave;
        var camera=go.AddComponent<Camera>();
        camera.enabled=false; camera.allowHDR=true; camera.clearFlags=CameraClearFlags.Skybox;
        camera.fieldOfView=65; camera.nearClipPlane=.1f; camera.farClipPlane=150;
        go.transform.position=house.transform.position+new Vector3(10,3.2f,-15);
        go.transform.LookAt(house.transform.position+new Vector3(0,1.6f,0));
        var data=go.AddComponent<UniversalAdditionalCameraData>(); data.renderPostProcessing=true;
        var target=new RenderTexture(1024,768,24,RenderTextureFormat.ARGB32,RenderTextureReadWrite.sRGB);
        var previous=RenderTexture.active;
        Texture2D pixels=null;
        try {
            target.Create();
            RenderPipeline.SubmitRenderRequest(camera,new UniversalRenderPipeline.SingleCameraRequest {destination=target});
            RenderTexture.active=target;
            pixels=new Texture2D(1024,768,TextureFormat.RGB24,false);
            pixels.ReadPixels(new Rect(0,0,1024,768),0,0); pixels.Apply();
            System.IO.File.WriteAllBytes("Logs/Noche_Terror_Vista.png",pixels.EncodeToPNG());
            Debug.Log("Vista nocturna renderizada en Logs/Noche_Terror_Vista.png");
        } finally {
            RenderTexture.active=previous;
            if(pixels!=null) UnityEngine.Object.DestroyImmediate(pixels);
            target.Release(); UnityEngine.Object.DestroyImmediate(target); UnityEngine.Object.DestroyImmediate(go);
        }
    }
    static void Validate(VolumeProfile profile) {
        if(!RenderSettings.fog || RenderSettings.skybox==null || !RenderSettings.skybox.shader.isSupported || RenderSettings.sun==null)
            throw new InvalidOperationException("Configuracion nocturna incompleta.");
        if(!profile.TryGet<Vignette>(out var v) || !v.active || !profile.TryGet<ColorAdjustments>(out var c) || !c.active)
            throw new InvalidOperationException("Faltan los efectos del perfil nocturno.");
        var camera=Camera.main;
        var terrain=Terrain.activeTerrain;
        if(camera!=null && terrain!=null) {
            float ground=terrain.SampleHeight(camera.transform.position)+terrain.transform.position.y;
            Debug.Log("Revision de camara: altura "+camera.transform.position.y.ToString("F2")+" m; suelo en esa posicion "+ground.ToString("F2")+" m.");
        }
    }
    static void EnsureFolder(string path) {
        if(AssetDatabase.IsValidFolder(path)) return;
        int slash=path.LastIndexOf('/'); EnsureFolder(path.Substring(0,slash));
        AssetDatabase.CreateFolder(path.Substring(0,slash),path.Substring(slash+1));
    }
}
#endif
