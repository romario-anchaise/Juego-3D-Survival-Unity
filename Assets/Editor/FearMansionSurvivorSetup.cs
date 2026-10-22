#if UNITY_EDITOR
using System;
using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using Unity.Cinemachine;
using StarterAssets;

public static class FearMansionSurvivorSetup
{
    const string Base = "Assets/Starter Assets/Runtime/ThirdPersonController/";
    const string Output = "Assets/FearMansion/Personaje";
    [MenuItem("Tools/Fear Mansion/Configure Third Person Survivor %#&F8")]
    public static void Setup()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Sal de Play primero.");
        var scene=EditorSceneManager.GetActiveScene();
        if (scene.path!="Assets/Scenes/SampleScene.unity") throw new InvalidOperationException("Abre SampleScene para configurar el jugador.");
        if (GameObject.Find("Superviviente_TerceraPersona")!=null) throw new InvalidOperationException("El superviviente ya existe.");
        Directory.CreateDirectory("Logs");
        EditorSceneManager.SaveScene(scene,"Logs/Antes_TerceraPersona.unity",true);
        var source=AssetDatabase.LoadAssetAtPath<GameObject>("Assets/Shady_3d/PREFAB/Apocalyptic character (Gas Mask +hood).prefab");
        var template=AssetDatabase.LoadAssetAtPath<GameObject>(Base+"Prefabs/PlayerArmature.prefab");
        if(source==null || template==null) throw new InvalidOperationException("Falta el personaje o Starter Assets.");
        var player=(GameObject)PrefabUtility.InstantiatePrefab(source);
        PrefabUtility.UnpackPrefabInstance(player,PrefabUnpackMode.Completely,InteractionMode.AutomatedAction);
        player.name="Superviviente_TerceraPersona"; player.tag="Player";
        player.transform.SetPositionAndRotation(Vector3.zero,Quaternion.identity);
        var animator=player.GetComponent<Animator>();
        if(animator==null || animator.avatar==null || !animator.avatar.isValid || !animator.avatar.isHuman)
            throw new InvalidOperationException("El avatar del superviviente no es humanoide valido.");
        animator.runtimeAnimatorController=template.GetComponent<Animator>().runtimeAnimatorController;
        animator.applyRootMotion=false; animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
        var bounds=new Bounds(); bool first=true;
        foreach(var r in player.GetComponentsInChildren<Renderer>()) { if(first){ bounds=r.bounds;first=false;} else bounds.Encapsulate(r.bounds); }
        float scale=1.8f/bounds.size.y;
        // Preserve the imported skeleton's relative paths while normalizing its height.
        foreach(Transform child in player.transform) {child.localPosition*=scale;child.localScale*=scale;}
        player.transform.localScale=Vector3.one;
        var cc=player.AddComponent<CharacterController>();
        cc.height=1.8f;cc.radius=.27f;cc.center=new Vector3(0,.92f,0);cc.stepOffset=.28f;cc.skinWidth=.025f;cc.minMoveDistance=0;cc.slopeLimit=45;
        var inputs=player.AddComponent<StarterAssetsInputs>();
        var pi=player.AddComponent<PlayerInput>();

        // Reference the asset itself so the saved prefab has a persistent InputActionAsset.
        pi.actions=template.GetComponent<PlayerInput>().actions;
        pi.notificationBehavior=PlayerNotifications.SendMessages;pi.defaultActionMap="Player";pi.defaultControlScheme="KeyboardMouse";
        var controller=player.AddComponent<ThirdPersonController>();
        EditorUtility.CopySerialized(template.GetComponent<ThirdPersonController>(),controller);
        controller.MoveSpeed=2.1f;controller.SprintSpeed=4.4f;controller.GroundedRadius=.25f;controller.GroundLayers=1;
        var target=new GameObject("CameraTarget").transform;target.SetParent(player.transform,false);target.localPosition=new Vector3(0,1.5f,0);
        controller.CinemachineCameraTarget=target.gameObject;controller.BottomClamp=-35;controller.TopClamp=65;
        player.AddComponent<SurvivorCursor>();
        SetLayer(player,2);
        var spawn=new Vector3(0,.15f,-10);
        var old=GameObject.Find("Jugador"); if(old!=null) spawn=old.transform.position;
        if(Terrain.activeTerrain!=null) spawn.y=Terrain.activeTerrain.SampleHeight(spawn)+Terrain.activeTerrain.transform.position.y+.15f;
        player.transform.position=spawn;
        var camera=Camera.main;
        if(camera==null) camera=new GameObject("Main Camera",typeof(Camera),typeof(AudioListener)).GetComponent<Camera>();
        camera.transform.SetParent(null,true);camera.tag="MainCamera";camera.enabled=true;camera.nearClipPlane=.08f;camera.fieldOfView=55;
        camera.transform.SetPositionAndRotation(spawn+new Vector3(.45f,1.9f,-3),Quaternion.Euler(8,0,0));
        camera.GetUniversalAdditionalCameraData().renderPostProcessing=true;
        if(camera.GetComponent<CinemachineBrain>()==null) camera.gameObject.AddComponent<CinemachineBrain>();
        if(old!=null) {old.SetActive(false);EditorUtility.SetDirty(old);}
        foreach(var other in UnityEngine.Object.FindObjectsByType<Camera>(FindObjectsSortMode.None)) if(other!=camera) {other.enabled=false;var listener=other.GetComponent<AudioListener>();if(listener!=null)listener.enabled=false;}
        var rig=(GameObject)PrefabUtility.InstantiatePrefab(AssetDatabase.LoadAssetAtPath<GameObject>(Base+"Prefabs/PlayerFollowCamera.prefab"));
        rig.name="Camara_Superviviente";
        var cm=rig.GetComponent<CinemachineCamera>();cm.Follow=target;cm.Lens.FieldOfView=55;cm.Lens.NearClipPlane=.08f;
        var follow=rig.GetComponent<CinemachineThirdPersonFollow>();follow.ShoulderOffset=new Vector3(.4f,.1f,0);follow.CameraDistance=3;follow.CameraSide=1;follow.VerticalArmLength=.2f;
        follow.AvoidObstacles.Enabled=true;follow.AvoidObstacles.CollisionFilter=1;follow.AvoidObstacles.IgnoreTag="Player";
        var noise=rig.GetComponent<CinemachineBasicMultiChannelPerlin>();if(noise!=null) noise.enabled=false;
        if(!AssetDatabase.IsValidFolder(Output)) AssetDatabase.CreateFolder("Assets/FearMansion","Personaje");
        PrefabUtility.SaveAsPrefabAssetAndConnect(player,Output+"/Superviviente_TerceraPersona.prefab",InteractionMode.AutomatedAction);
        EditorSceneManager.MarkSceneDirty(scene);EditorSceneManager.SaveScene(scene);AssetDatabase.SaveAssets();
        Finish();
        Selection.activeGameObject=player;
        File.WriteAllText("Logs/survivor-setup.txt",$"OK Avatar={animator.avatar.name}; HeightBefore={bounds.size.y}; Scale={scale}; Spawn={spawn}");
        Debug.Log("Superviviente en tercera persona configurado. WASD, Shift, Espacio y mouse. Escape libera el cursor.");
    }
    [MenuItem("Tools/Fear Mansion/Update Survivor Materials")]
    public static void Finish()
    {
        var player=GameObject.Find("Superviviente_TerceraPersona");
        foreach(var r in player.GetComponentsInChildren<Renderer>(true))
        {
            var mats=r.sharedMaterials;
            for(int i=0;i<mats.Length;i++)
            {
                var original=mats[i]; if(original==null)continue;
                string path=Output+"/"+original.name.Replace("_URP", "")+"_URP.mat";
                var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
                if(mat==null) {mat=new Material(original);AssetDatabase.CreateAsset(mat,path);}
                mat.SetTexture("_MetallicGlossMap",null);mat.DisableKeyword("_METALLICSPECGLOSSMAP");
                mat.SetFloat("_Metallic",original.name.Contains("Clothes")?0:.25f);
                mat.SetFloat("_Smoothness",.22f);EditorUtility.SetDirty(mat);mats[i]=mat;
            }
            r.sharedMaterials=mats;
        }
        var cam=Camera.main;
        if(cam.transform.Find("Reflejo_Luna_Personaje")==null)
        {
            var fill=new GameObject("Reflejo_Luna_Personaje",typeof(Light));fill.transform.SetParent(cam.transform,false);
            var light=fill.GetComponent<Light>();light.type=LightType.Point;light.color=new Color(.6f,.7f,.9f);
            light.intensity=1.2f;light.range=6;light.cullingMask=1<<2;light.shadows=LightShadows.None;
        }
        var rim=cam.transform.Find("Reflejo_Luna_Personaje").GetComponent<Light>();rim.intensity=8;rim.range=8;rim.renderMode=LightRenderMode.ForcePixel;
        PrefabUtility.ApplyPrefabInstance(player,InteractionMode.AutomatedAction);
        EditorSceneManager.MarkSceneDirty(player.scene);EditorSceneManager.SaveScene(player.scene);AssetDatabase.SaveAssets();
    }
    static void SetLayer(GameObject g,int layer) {g.layer=layer;foreach(Transform child in g.transform)SetLayer(child.gameObject,layer);}
}
#endif






