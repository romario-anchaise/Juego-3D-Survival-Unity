#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class FearMansionTerrainBuilder
{
    const string Folder="Assets/FearMansion/Terreno";
    const float Width=100f, Height=24f;

    [MenuItem("Tools/Fear Mansion/Create Hills Terrain %#&F7")]
    public static void CreateTerrain()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Sal de Play antes de crear el terreno.");
        StageUtility.GoToMainStage();
        var existing=GameObject.Find("Terreno_Con_Cerros");
        if(existing!=null) {
            var current=existing.GetComponent<Terrain>();
            if(current.terrainData.detailScatterMode!=DetailScatterMode.InstanceCountMode) {
                var map=current.terrainData.GetDetailLayer(0,0,128,128,0);
                current.terrainData.SetDetailScatterMode(DetailScatterMode.InstanceCountMode);
                current.terrainData.SetDetailLayer(0,0,0,map);
                EditorUtility.SetDirty(current.terrainData);
            }
            current.drawTreesAndFoliage=true;
            current.detailObjectDistance=55;
            current.detailObjectDensity=1;
            current.Flush();
            Validate(current);
            AssetDatabase.SaveAssets();
            Focus(existing); return;
        }
        EnsureFolder(Folder);
        var grass=CreateGrassPrototype();
        var data=AssetDatabase.LoadAssetAtPath<TerrainData>(Folder+"/Terreno_100x100.asset");
        if(data==null) {
            data=new TerrainData {name="Terreno_100x100",heightmapResolution=513,alphamapResolution=128};
            data.size=new Vector3(Width,Height,Width);
            var heights=new float[513,513];
            for(int z=0;z<513;z++) for(int x=0;x<513;x++) {
                float px=x/512f*Width-Width/2, pz=z/512f*Width-Width/2;
                // Wide, gently sloping hills surround a genuinely flat building area.
                float mask=Mathf.SmoothStep(0,1,Mathf.InverseLerp(18,38,new Vector2(px,pz).magnitude));
                float elevation=Hill(px,pz,-32,-23,7,17)+Hill(px,pz,32,-12,8,18)
                    +Hill(px,pz,12,32,6,17)+Hill(px,pz,-26,32,5,16);
                heights[z,x]=mask*elevation/Height;
            }
            data.SetHeights(0,0,heights);
            data.terrainLayers=new[]{CreateLayer()};
            var paint=new float[128,128,1];
            for(int z=0;z<128;z++) for(int x=0;x<128;x++) paint[z,x,0]=1;
            data.SetAlphamaps(0,0,paint);
            data.SetDetailScatterMode(DetailScatterMode.InstanceCountMode);
            data.SetDetailResolution(128,16);
            var detail=new DetailPrototype {
                prototype=grass,usePrototypeMesh=true,useInstancing=true,
                renderMode=DetailRenderMode.VertexLit,
                minWidth=.8f,maxWidth=1.25f,minHeight=.8f,maxHeight=1.25f,
                noiseSpread=.2f,healthyColor=Color.white,dryColor=Color.white
            };
            if(!detail.Validate(out string error)) throw new InvalidOperationException(error);
            data.detailPrototypes=new[]{detail};
            var density=new int[128,128];
            for(int z=0;z<128;z++) for(int x=0;x<128;x++)
                density[z,x]=Mathf.PerlinNoise(x*.13f+4,z*.13f+7)>.35f?2:1;
            data.SetDetailLayer(0,0,0,density);
            AssetDatabase.CreateAsset(data,Folder+"/Terreno_100x100.asset");
        }
        var terrainShader=Shader.Find("Universal Render Pipeline/Terrain/Lit");
        if(terrainShader==null || !terrainShader.isSupported)
            throw new InvalidOperationException("Shader de Terrain URP no disponible.");
        var material=AssetDatabase.LoadAssetAtPath<Material>(Folder+"/Terreno_URP.mat");
        if(material==null) {
            material=new Material(terrainShader){name="Terreno_URP",enableInstancing=true};
            AssetDatabase.CreateAsset(material,Folder+"/Terreno_URP.mat");
        }
        var go=Terrain.CreateTerrainGameObject(data);
        go.name="Terreno_Con_Cerros";
        Undo.RegisterCreatedObjectUndo(go,"Crear terreno con cerros");
        var player=GameObject.Find("Jugador");
        Vector3 center=player!=null?player.transform.position:new Vector3(0,0,-10);
        go.transform.position=new Vector3(center.x-Width/2,0,center.z-Width/2);
        var terrain=go.GetComponent<Terrain>();
        terrain.materialTemplate=material;
        terrain.drawInstanced=true;
        terrain.drawTreesAndFoliage=true;
        terrain.detailObjectDistance=55;
        terrain.detailObjectDensity=1;
        terrain.heightmapPixelError=5;
        terrain.Flush();
        try { Validate(terrain); }
        catch { UnityEngine.Object.DestroyImmediate(go); throw; }

        // Preserve the old flat piece, while removing its overlapping surface.
        var oldFloor=GameObject.Find("Suelo_Con_Pasto_Listo");
        if(oldFloor!=null) {
            Undo.RecordObject(oldFloor,"Desactivar suelo plano anterior");
            oldFloor.SetActive(false);
        }
        bool sunFound=false;
        foreach(var light in UnityEngine.Object.FindObjectsByType<Light>(FindObjectsSortMode.None))
            if(light.isActiveAndEnabled && light.type==LightType.Directional) sunFound=true;
        if(!sunFound) {
            var sun=new GameObject("Sol_Terreno");
            sun.transform.rotation=Quaternion.Euler(48,-35,0);
            var light=sun.AddComponent<Light>();
            light.type=LightType.Directional; light.intensity=1.15f;
            light.color=new Color(1,.95f,.84f); light.shadows=LightShadows.Soft;
            Undo.RegisterCreatedObjectUndo(sun,"Iluminar terreno");
        }
        AssetDatabase.SaveAssets();
        EditorSceneManager.MarkSceneDirty(go.scene);
        Focus(go);
        Debug.Log("Terreno verificado: 100x100 m, 4 cerros suaves, centro plano de 36 m de diametro, TerrainCollider y pasto instanciado que sigue las pendientes.");
    }

    static float Hill(float x,float z,float cx,float cz,float height,float radius) {
        return height*Mathf.Exp(-((x-cx)*(x-cx)+(z-cz)*(z-cz))/(radius*radius));
    }
    static TerrainLayer CreateLayer() {
        string path=Folder+"/Cesped.terrainlayer";
        var layer=AssetDatabase.LoadAssetAtPath<TerrainLayer>(path);
        if(layer!=null) return layer;
        var texture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Polytope Studio/Lowpoly_Environments/Sources/Textures/PT_Ground_Grass_Green_01.png");
        if(texture==null) throw new InvalidOperationException("Falta la textura de cesped importada.");
        layer=new TerrainLayer {name="Cesped",diffuseTexture=texture,tileSize=new Vector2(6,6),smoothness=0,metallic=0};
        AssetDatabase.CreateAsset(layer,path);
        return layer;
    }
    static GameObject CreateGrassPrototype() {
        string path=Folder+"/Pasto_Para_Terrain.prefab";
        var saved=AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if(saved!=null) return saved;
        var source=AssetDatabase.LoadAssetAtPath<Mesh>("Assets/FearMansion/Pasto/Meshes/Pasto_Mata.asset");
        var material=AssetDatabase.LoadAssetAtPath<Material>("Assets/FearMansion/Pasto/Materials/Pasto_Verde.mat");
        if(source==null || material==null) throw new InvalidOperationException("Primero crea el pasto 3D.");
        var mesh=UnityEngine.Object.Instantiate(source);
        mesh.name="Pasto_Terrain";
        var triangles=source.triangles;
        mesh.subMeshCount=1;
        mesh.SetTriangles(triangles,0);
        AssetDatabase.CreateAsset(mesh,Folder+"/Pasto_Terrain.asset");
        var preview=EditorSceneManager.NewPreviewScene();
        var root=new GameObject("Pasto_Para_Terrain");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,preview);
        try {
            root.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=root.AddComponent<MeshRenderer>();
            renderer.sharedMaterial=material;
            renderer.shadowCastingMode=ShadowCastingMode.Off;
            saved=PrefabUtility.SaveAsPrefabAsset(root,path,out bool success);
            if(!success) throw new InvalidOperationException("No se pudo guardar el pasto del Terrain.");
        } finally { EditorSceneManager.ClosePreviewScene(preview); }
        return saved;
    }
    static void Validate(Terrain terrain) {
        var data=terrain.terrainData;
        if(data.detailPrototypes.Length!=1 || !data.detailPrototypes[0].useInstancing)
            throw new InvalidOperationException("Falta el pasto del Terrain.");
        var instances=data.ComputeDetailInstanceTransforms(4,4,0,1,out var grassBounds);
        if(instances.Length==0) throw new InvalidOperationException("No hay instancias de hierba sobre el centro del terreno.");
        float grassHeightError=0;
        foreach(var instance in instances)
            grassHeightError=Mathf.Max(grassHeightError,Mathf.Abs(instance.posY-data.GetInterpolatedHeight(instance.posX/Width,instance.posZ/Width)));
        if(grassHeightError>.1f) throw new InvalidOperationException("La hierba no coincide con las alturas del terreno.");
        if(data.GetHeight(256,256)>.001f) throw new InvalidOperationException("El centro no esta plano.");
        float highest=0,steepest=0;
        for(int z=0;z<=100;z++) for(int x=0;x<=100;x++) {
            highest=Mathf.Max(highest,data.GetInterpolatedHeight(x/100f,z/100f));
            steepest=Mathf.Max(steepest,data.GetSteepness(x/100f,z/100f));
        }
        if(highest<4 || steepest>40) throw new InvalidOperationException("Altura o pendiente fuera del rango caminable: "+highest+" m, "+steepest+" grados.");
        Physics.SyncTransforms();
        var collider=terrain.GetComponent<TerrainCollider>();
        var origin=terrain.transform.position+new Vector3(50,20,50);
        if(!collider.Raycast(new Ray(origin,Vector3.down),out var hit,30) || Mathf.Abs(hit.point.y)>.02f)
            throw new InvalidOperationException("La colision del centro no coincide con el suelo.");
        Debug.Log("Comprobaciones Terrain: altura maxima "+highest.ToString("F2")+" m; pendiente maxima "+steepest.ToString("F1")+" grados; raycast al suelo correcto; hierba en parcela central: "+instances.Length+" instancias, error de altura "+grassHeightError.ToString("F3")+" m.");
    }
    static void Focus(GameObject go) {
        Selection.activeGameObject=go;
        if(SceneView.lastActiveSceneView!=null) SceneView.lastActiveSceneView.FrameSelected();
    }
    static void EnsureFolder(string path) {
        if(AssetDatabase.IsValidFolder(path)) return;
        int slash=path.LastIndexOf('/'); EnsureFolder(path.Substring(0,slash));
        AssetDatabase.CreateFolder(path.Substring(0,slash),path.Substring(slash+1));
    }

}
#endif
