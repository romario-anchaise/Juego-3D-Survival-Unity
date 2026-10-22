#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class FearMansionLandscapeBuilder
{
    const string Folder="Assets/FearMansion/Naturaleza";
    const string Source="Assets/Polytope Studio/Lowpoly_Environments/Prefabs/Trees/";
    static readonly Dictionary<string,Material> remapped=new Dictionary<string,Material>();

    [MenuItem("Tools/Fear Mansion/Plant Trees Around House %#&F1")]
    public static void PlantTrees()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Sal de Play para plantar arboles.");
        StageUtility.GoToMainStage();
        var house=GameObject.Find("MiniCasa_Amueblada");
        var terrain=Terrain.activeTerrain;
        if(house==null || terrain==null) throw new InvalidOperationException("Falta la mini casa o el Terrain.");
        var existing=GameObject.Find("Arboles_Alrededor_Casa");
        if(existing!=null) { Validate(existing,terrain,house.transform.position); Focus(house); return; }
        EnsureFolder(Folder+"/Materials"); EnsureFolder(Folder+"/Prefabs");
        remapped.Clear();
        var prefabs=new[]{
            CreatePrefab("Pino_URP","PT_Pine_Tree_03_green",8),
            CreatePrefab("Arbol_Frondoso_URP","PT_Fruit_Tree_01_green",6),
            CreatePrefab("Manzano_URP","PT_Fruit_Tree_01_apples",5.5f),
            CreatePrefab("Peral_URP","PT_Fruit_Tree_01_pears",6)
        };
        var root=new GameObject("Arboles_Alrededor_Casa");
        root.transform.position=house.transform.position;
        Undo.RegisterCreatedObjectUndo(root,"Plantar arboles alrededor de la casa");
        var positions=new List<Vector2>();
        var random=new System.Random(86721);
        Vector2[] nearby={new Vector2(-11,-3),new Vector2(-12,5),new Vector2(-8,12),new Vector2(0,14),new Vector2(9,11),new Vector2(12,3),new Vector2(12,-7),new Vector2(-12,-12)};
        try {
            for(int i=0;i<nearby.Length;i++) {
                float scale=.85f+(float)random.NextDouble()*.25f;
                Plant(prefabs[1+i%3],root.transform,nearby[i],scale,(float)random.NextDouble()*360,terrain);
                positions.Add(nearby[i]);
            }
            // Scatter behind and along the sides, with a clear view toward the entry.
            for(int attempt=0;positions.Count<28 && attempt<1500;attempt++) {
                float angle=(float)random.NextDouble()*Mathf.PI*2;
                float radius=23+(float)random.NextDouble()*21;
                Vector2 p=new Vector2(Mathf.Cos(angle),Mathf.Sin(angle))*radius;
                if(p.y<0 && Mathf.Abs(p.x)<6) continue;
                Vector3 world=house.transform.position+new Vector3(p.x,0,p.y);
                Vector3 local=world-terrain.transform.position;
                if(local.x<5 || local.z<5 || local.x>terrain.terrainData.size.x-5 || local.z>terrain.terrainData.size.z-5) continue;
                bool crowded=false;
                foreach(var q in positions) if(Vector2.Distance(p,q)<6) {crowded=true;break;}
                if(crowded) continue;
                int species=positions.Count%3==0?1:0;
                Plant(prefabs[species],root.transform,p,.85f+(float)random.NextDouble()*.5f,(float)random.NextDouble()*360,terrain);
                positions.Add(p);
            }
            Validate(root,terrain,house.transform.position);
            AssetDatabase.SaveAssets();
            EditorSceneManager.MarkSceneDirty(root.scene);
            Focus(house);
            Debug.Log("Paisajismo verificado: "+root.transform.childCount+" arboles, 4 prefabs URP reutilizables, troncos con colision, bases sobre el terreno y entrada despejada.");
        } catch {
            UnityEngine.Object.DestroyImmediate(root);
            throw;
        }
    }

    static GameObject CreatePrefab(string name,string sourceName,float height)
    {
        string path=Folder+"/Prefabs/"+name+".prefab";
        var saved=AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if(saved!=null) return saved;
        var source=AssetDatabase.LoadAssetAtPath<GameObject>(Source+sourceName+".prefab");
        if(source==null) throw new InvalidOperationException("Falta el arbol "+sourceName);
        var preview=EditorSceneManager.NewPreviewScene();
        var root=new GameObject(name);
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,preview);
        try {
            var instance=(GameObject)PrefabUtility.InstantiatePrefab(source,root.transform);
            instance.transform.localPosition=Vector3.zero;
            instance.transform.localRotation=Quaternion.identity;
            var bounds=GetBounds(instance);
            instance.transform.localScale*=height/Mathf.Max(bounds.size.y,.1f);
            bounds=GetBounds(instance);
            // Keep the source trunk's X/Z origin; move only the base vertically.
            instance.transform.position-=Vector3.up*bounds.min.y;
            foreach(var renderer in instance.GetComponentsInChildren<MeshRenderer>(true)) {
                var materials=renderer.sharedMaterials;
                for(int i=0;i<materials.Length;i++) materials[i]=ConvertMaterial(materials[i]);
                renderer.sharedMaterials=materials;
                renderer.shadowCastingMode=ShadowCastingMode.TwoSided;
            }
            foreach(var lod in instance.GetComponentsInChildren<LODGroup>()) {
                lod.fadeMode=LODFadeMode.None;
                var levels=lod.GetLODs();
                float[] transitions={.22f,.09f,.008f};
                for(int i=0;i<levels.Length;i++) levels[i].screenRelativeTransitionHeight=transitions[Mathf.Min(i,2)];
                lod.SetLODs(levels); lod.RecalculateBounds();
            }
            var trunk=root.AddComponent<CapsuleCollider>();
            trunk.height=Mathf.Min(height*.6f,4); trunk.radius=height*.035f;
            trunk.center=new Vector3(0,trunk.height/2,0);
            saved=PrefabUtility.SaveAsPrefabAsset(root,path,out bool success);
            if(!success) throw new InvalidOperationException("No se pudo guardar "+name);
        } finally { EditorSceneManager.ClosePreviewScene(preview); }
        return saved;
    }
    static Material ConvertMaterial(Material source)
    {
        if(source==null) throw new InvalidOperationException("Material de arbol faltante.");
        string key=AssetDatabase.AssetPathToGUID(AssetDatabase.GetAssetPath(source));
        if(remapped.TryGetValue(key,out var cached)) return cached;
        string path=Folder+"/Materials/"+source.name+"_URP.mat";
        var material=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(material!=null) {remapped[key]=material;return material;}
        var shader=Shader.Find("Universal Render Pipeline/Lit");
        if(shader==null || !shader.isSupported) throw new InvalidOperationException("URP/Lit no disponible.");
        material=new Material(shader){name=source.name+"_URP",enableInstancing=true,doubleSidedGI=true};
        Texture texture=null;
        foreach(string property in new[]{"_BaseTexture","_BaseMap","_MainTex"})
            if(source.HasProperty(property) && source.GetTexture(property)!=null) { texture=source.GetTexture(property);break; }
        if(texture==null) throw new InvalidOperationException("No se encontro la textura de "+source.name);
        material.SetTexture("_BaseMap",texture);
        material.SetColor("_BaseColor",Color.white);
        material.SetFloat("_Smoothness",.08f);
        bool leaves=source.name.IndexOf("Leave",StringComparison.OrdinalIgnoreCase)>=0
            || source.name.IndexOf("Foliage",StringComparison.OrdinalIgnoreCase)>=0
            || source.name.IndexOf("Leaf",StringComparison.OrdinalIgnoreCase)>=0;
        if(leaves) {
            material.SetFloat("_Cull",0); material.SetFloat("_AlphaClip",1);
            material.SetFloat("_Cutoff",.45f); material.SetFloat("_AlphaToMask",1);
            material.EnableKeyword("_ALPHATEST_ON"); material.SetOverrideTag("RenderType","TransparentCutout");
            material.renderQueue=2450;
        }
        AssetDatabase.CreateAsset(material,path); remapped[key]=material;
        return material;
    }
    static void Plant(GameObject prefab,Transform root,Vector2 offset,float scale,float yaw,Terrain terrain)
    {
        var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,root);
        instance.name=prefab.name+"_"+(root.childCount).ToString("00");
        var position=root.position+new Vector3(offset.x,0,offset.y);
        position.y=terrain.SampleHeight(position)+terrain.transform.position.y-.025f;
        instance.transform.position=position;
        instance.transform.localRotation=Quaternion.Euler(0,yaw,0);
        instance.transform.localScale=Vector3.one*scale;
    }
    static Bounds GetBounds(GameObject go) {
        var renderers=go.GetComponentsInChildren<MeshRenderer>(true);
        if(renderers.Length==0) throw new InvalidOperationException("Arbol sin mallas.");
        var bounds=renderers[0].bounds;
        foreach(var renderer in renderers) bounds.Encapsulate(renderer.bounds);
        return bounds;
    }
    static void Validate(GameObject root,Terrain terrain,Vector3 house)
    {
        if(root.transform.childCount<24) throw new InvalidOperationException("No se pudieron distribuir suficientes arboles.");
        foreach(Transform tree in root.transform) {
            Vector3 p=tree.position-house;
            if(Mathf.Abs(p.x)<6 && p.z>-12 && p.z<6) throw new InvalidOperationException("Arbol demasiado cerca de la casa o el acceso.");
            float height=terrain.SampleHeight(tree.position)+terrain.transform.position.y;
            if(Mathf.Abs(tree.position.y-height)>.06f || Mathf.Abs(GetBounds(tree.gameObject).min.y-height)>.12f)
                throw new InvalidOperationException("Arbol separado del suelo: "+tree.name);
            if(tree.GetComponent<CapsuleCollider>()==null) throw new InvalidOperationException("Tronco sin colision.");
            foreach(var renderer in tree.GetComponentsInChildren<MeshRenderer>(true))
                foreach(var material in renderer.sharedMaterials)
                    if(material==null || material.shader.name!="Universal Render Pipeline/Lit" || !material.shader.isSupported || material.GetTexture("_BaseMap")==null)
                        throw new InvalidOperationException("Material incompatible en "+tree.name);
        }
    }
    static void Focus(GameObject house) {
        Selection.activeGameObject=house;
        if(SceneView.lastActiveSceneView!=null) SceneView.lastActiveSceneView.LookAt(house.transform.position+new Vector3(0,3,0),Quaternion.Euler(22,-35,0),21,false,true);
    }
    static void EnsureFolder(string path) {
        if(AssetDatabase.IsValidFolder(path)) return;
        int slash=path.LastIndexOf('/'); EnsureFolder(path.Substring(0,slash));
        AssetDatabase.CreateFolder(path.Substring(0,slash),path.Substring(slash+1));
    }
}
#endif
