#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;

public static class FearMansionMiniHouseBuilder
{
    const string Folder="Assets/FearMansion/MiniCasa";
    const string Source="Assets/nappin/HouseInteriorPack/Prefabs/";
    const float FloorY=.18f, WallHeight=2.8f;
    static readonly Dictionary<string,Material> materials=new Dictionary<string,Material>();
    static Transform shell, furniture;

    [MenuItem("Tools/Fear Mansion/Create Mini House %#&F5")]
    public static void CreateHouse()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode) throw new InvalidOperationException("Sal de Play para crear la casa.");
        StageUtility.GoToMainStage();
        var existing=GameObject.Find("MiniCasa_Amueblada");
        if(existing!=null) { Focus(existing); return; }
        EnsureFolder(Folder+"/Materials");
        EnsureFolder(Folder+"/Prefabs");
        materials.Clear();
        materials["wall"]=Mat("Pared_Marfil",new Color(.78f,.76f,.67f));
        materials["wood"]=Mat("Madera_Roble",new Color(.31f,.19f,.10f));
        materials["floor"]=Mat("Piso_Madera",new Color(.43f,.31f,.19f));
        materials["roof"]=Mat("Techo_Pizarra",new Color(.09f,.15f,.17f));
        materials["trim"]=Mat("Marcos_Crema",new Color(.88f,.86f,.77f));
        materials["tile"]=Mat("Baldosa_Bano",new Color(.38f,.45f,.43f));
        materials["glass"]=Mat("Vidrio",new Color(.65f,.83f,.87f,.18f),true);
        var preview=EditorSceneManager.NewPreviewScene();
        var root=new GameObject("MiniCasa_Amueblada");
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(root,preview);
        string prefabPath=Folder+"/Prefabs/MiniCasa_Amueblada.prefab";
        try {
            shell=Group(root.transform,"Estructura");
            furniture=Group(root.transform,"Muebles_nappin");
            BuildShell(root.transform);
            BuildFurniture();
            Validate(root);
            PrefabUtility.SaveAsPrefabAsset(root,prefabPath,out bool success);
            if(!success) throw new InvalidOperationException("No se pudo guardar la mini casa.");
        } finally { EditorSceneManager.ClosePreviewScene(preview); }
        AssetDatabase.SaveAssets();
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
        var placed=(GameObject)PrefabUtility.InstantiatePrefab(prefab);
        placed.name="MiniCasa_Amueblada";
        var player=GameObject.Find("Jugador");
        Vector3 center=player!=null?player.transform.position:new Vector3(0,0,-10);
        placed.transform.position=new Vector3(center.x,0,center.z+9);
        var terrain=Terrain.activeTerrain;
        if(terrain!=null) placed.transform.position+=Vector3.up*(terrain.SampleHeight(placed.transform.position)+terrain.transform.position.y);
        if(terrain!=null) ClearGrass(terrain,placed.transform.position);
        Undo.RegisterCreatedObjectUndo(placed,"Crear mini casa amueblada");
        EditorSceneManager.MarkSceneDirty(placed.scene);
        Focus(placed);
        Debug.Log("Mini casa lista: 9x8 m, sala, cocina, dormitorio, bano y porche; muebles de House Interior Free, materiales URP y accesos verificados.");
    }

    static Transform Group(Transform parent,string name) {
        var t=new GameObject(name).transform; t.SetParent(parent,false); return t;
    }
    static GameObject Box(Transform parent,string name,Vector3 pos,Vector3 size,string material) {
        var box=GameObject.CreatePrimitive(PrimitiveType.Cube);
        box.name=name; box.transform.SetParent(parent,false);
        box.transform.localPosition=pos; box.transform.localScale=size;
        box.GetComponent<MeshRenderer>().sharedMaterial=materials[material];
        return box;
    }
    struct Opening {
        public float x,w,bottom,h;
        public bool window;
        public Opening(float x,float w,float bottom,float h,bool window=false) {this.x=x;this.w=w;this.bottom=bottom;this.h=h;this.window=window;}
    }
    static void Wall(string name,Vector3 pos,float length,float rotation,params Opening[] openings) {
        var wall=Group(shell,name); wall.localPosition=pos; wall.localRotation=Quaternion.Euler(0,rotation,0);
        Array.Sort(openings,(a,b)=>a.x.CompareTo(b.x));
        float cursor=-length/2;
        foreach(var hole in openings) {
            float left=hole.x-hole.w/2,right=hole.x+hole.w/2;
            WallSegment(wall,cursor,left,0,WallHeight);
            WallSegment(wall,left,right,0,hole.bottom);
            WallSegment(wall,left,right,hole.bottom+hole.h,WallHeight);
            string trim=hole.window?"wood":"trim";
            foreach(float x in new[]{left-.035f,right+.035f})
                Box(wall,"Marco_Vertical",new Vector3(x,FloorY+hole.bottom+hole.h/2,-.015f),new Vector3(.07f,hole.h+.1f,.25f),trim);
            Box(wall,"Marco_Superior",new Vector3(hole.x,FloorY+hole.bottom+hole.h+.03f,-.015f),new Vector3(hole.w+.18f,.09f,.25f),trim);
            if(hole.window) {
                Box(wall,"Alfeizar",new Vector3(hole.x,FloorY+hole.bottom,-.04f),new Vector3(hole.w+.25f,.09f,.36f),"wood");
                var glass=Box(wall,"Cristal",new Vector3(hole.x,FloorY+hole.bottom+hole.h/2,0),new Vector3(hole.w-.04f,hole.h-.04f,.025f),"glass");
                glass.GetComponent<MeshRenderer>().shadowCastingMode=ShadowCastingMode.Off;
                Box(wall,"Division_Ventana",new Vector3(hole.x,FloorY+hole.bottom+hole.h/2,-.03f),new Vector3(.045f,hole.h,.08f),"wood");
            }
            cursor=right;
        }
        WallSegment(wall,cursor,length/2,0,WallHeight);
        Box(wall,"Cornisa",new Vector3(0,FloorY+WallHeight-.06f,0),new Vector3(length,.12f,.24f),"trim");
    }
    static void WallSegment(Transform wall,float a,float b,float bottom,float top) {
        if(b-a<.001f || top-bottom<.001f) return;
        Box(wall,"Pared",new Vector3((a+b)/2,FloorY+(bottom+top)/2,0),new Vector3(b-a,top-bottom,.18f),"wall");
        if(bottom==0) Box(wall,"Zocalo",new Vector3((a+b)/2,FloorY+.055f,0),new Vector3(b-a,.11f,.22f),"wood");
    }
    static void BuildShell(Transform root) {
        Box(shell,"Cimentacion",new Vector3(0,.03f,0),new Vector3(9.25f,.18f,8.25f),"tile");
        Box(shell,"Piso",new Vector3(0,FloorY-.06f,0),new Vector3(9,.12f,8),"floor");
        // Floorboards add readable scale without requiring external textures.
        for(int i=0;i<36;i++) Box(shell,"Junta_Piso",new Vector3(-4.375f+i*.25f,FloorY+.001f,0),new Vector3(.006f,.002f,8),"wood");
        Wall("Fachada",new Vector3(0,0,-4),9,0,new Opening(-2.4f,1.8f,1.0f,1.25f,true),new Opening(1,1.3f,0,2.3f));
        Wall("Pared_Trasera",new Vector3(0,0,4),9,180,new Opening(1.8f,1.8f,1,1.25f,true),new Opening(-2.8f,.9f,1.35f,.8f,true));
        Wall("Pared_Oeste",new Vector3(-4.5f,0,0),8,90,new Opening(1.6f,1.5f,1,1.25f,true));
        Wall("Pared_Este",new Vector3(4.5f,0,0),8,-90,new Opening(-1.8f,1.5f,1,1.25f,true));
        Wall("Dormitorio_Frente",new Vector3(-1.65f,0,1.2f),5.7f,0,new Opening(.05f,1.15f,0,2.3f));
        Wall("Bano_Frente",new Vector3(2.85f,0,1.2f),3.3f,0,new Opening(.05f,1.15f,0,2.3f));
        Wall("Division_Dormitorio_Bano",new Vector3(1.2f,0,2.6f),2.8f,90);
        Box(shell,"Piso_Bano",new Vector3(2.85f,FloorY+.005f,2.6f),new Vector3(3.1f,.01f,2.6f),"tile");
        var roof=Group(root,"Techo");
        Box(roof,"Cielo_Raso",new Vector3(0,3.02f,0),new Vector3(9,.12f,8),"trim");
        for(int side=-1;side<=1;side+=2) {
            var panel=Box(roof,"Faldon",new Vector3(side*2.4f,3.72f,0),new Vector3(5.1f,.16f,8.9f),"roof");
            panel.transform.localRotation=Quaternion.Euler(0,0,-side*18);
            for(int i=0;i<16;i++) {
                var rib=Box(roof,"Junta_Techo",new Vector3(side*2.4f,3.82f,-4.3f+i*.57f),new Vector3(5.1f,.035f,.035f),"roof");
                rib.transform.localRotation=panel.transform.localRotation;
            }
        }
        Box(roof,"Cumbrera",new Vector3(0,4.55f,0),new Vector3(.18f,.12f,9),"roof");
        Gables(roof);
        var porch=Group(root,"Porche");
        Box(porch,"Tarima",new Vector3(.5f,.08f,-4.9f),new Vector3(5,.16f,1.8f),"wood");
        Box(porch,"Escalon",new Vector3(1,.04f,-6),new Vector3(2,.08f,.5f),"tile");
        Box(porch,"Marquesina",new Vector3(.5f,2.8f,-4.85f),new Vector3(5.3f,.15f,2.1f),"roof");
        foreach(float x in new[]{-1.8f,2.8f}) Box(porch,"Pilar",new Vector3(x,1.46f,-5.6f),new Vector3(.12f,2.6f,.12f),"wood");
        for(int i=0;i<6;i++) Box(porch,"Camino",new Vector3(1,.025f,-6.6f-i*.6f),new Vector3(1.2f,.05f,.45f),"tile");
        Light(root,"Luz_Sala",new Vector3(-1.5f,2.55f,-1.6f),2.2f,6);
        Light(root,"Luz_Cocina",new Vector3(3,2.55f,-1.2f),1.8f,4);
        Light(root,"Luz_Dormitorio",new Vector3(-1.5f,2.55f,2.6f),1.6f,4);
        Light(root,"Luz_Bano",new Vector3(2.8f,2.55f,2.6f),1.4f,3);
        Light(root,"Luz_Porche",new Vector3(1,2.55f,-4.25f),1.2f,3);
    }
    static void Gables(Transform roof) {
        string path=Folder+"/Frontones.asset";
        var mesh=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(mesh==null) {
            mesh=new Mesh {name="Frontones"};
            mesh.vertices=new[]{new Vector3(-4.5f,3, -4),new Vector3(4.5f,3,-4),new Vector3(0,4.5f,-4),new Vector3(-4.5f,3,4),new Vector3(0,4.5f,4),new Vector3(4.5f,3,4)};
            mesh.triangles=new[]{0,2,1,3,4,5}; mesh.RecalculateNormals(); mesh.RecalculateBounds();
            AssetDatabase.CreateAsset(mesh,path);
        }
        var g=new GameObject("Frontones"); g.transform.SetParent(roof,false);
        g.AddComponent<MeshFilter>().sharedMesh=mesh;
        g.AddComponent<MeshRenderer>().sharedMaterial=materials["wall"];
    }
    static void BuildFurniture() {
        Place("Sofa",new Vector3(-3.1f,FloorY,-.65f),90,new Vector3(2.3f,1.2f,.95f));
        Place("CoffeTable",new Vector3(-1.55f,FloorY,-.8f),0,new Vector3(1.1f,.6f,.65f));
        Place("CoffeTable_smallPlant",new Vector3(-1.55f,FloorY+.45f,-.8f),0,new Vector3(.28f,.5f,.28f));
        Place("MediaConsole",new Vector3(.3f,FloorY,-.8f),-90,new Vector3(1.3f,1,.45f));
        Place("ShoeRack",new Vector3(3.3f,FloorY,-3.3f),0,new Vector3(1,.8f,.4f));
        Place("KitchenSink",new Vector3(3.65f,FloorY,-1),-90,new Vector3(1.5f,1.05f,.6f));
        Place("Stove",new Vector3(3.65f,FloorY,.25f),-90,new Vector3(.75f,1.05f,.65f));
        Place("Minifridge",new Vector3(3.65f,FloorY,-2.35f),-90,new Vector3(.65f,1.05f,.65f));
        Place("Shelf1",new Vector3(4.05f,1.65f,-1),-90,new Vector3(1.4f,.65f,.3f));
        Place("LaunchTable",new Vector3(1.5f,FloorY,-1.4f),0,new Vector3(1,.9f,.7f));
        Place("DiningChair",new Vector3(1.5f,FloorY,-2.3f),0,new Vector3(.48f,1,.5f));
        Place("DiningChair",new Vector3(1.5f,FloorY,-.5f),180,new Vector3(.48f,1,.5f));
        Place("SingleBed",new Vector3(-3.15f,FloorY,2.65f),0,new Vector3(1.3f,1.25f,2.05f));
        Place("BedsideTable",new Vector3(-1.95f,FloorY,3.3f),0,new Vector3(.5f,.6f,.45f));
        Place("BedsideLight",new Vector3(-1.95f,FloorY+.5f,3.3f),0,new Vector3(.28f,.55f,.28f));
        Place("Wardrobe",new Vector3(.65f,FloorY,2.8f),-90,new Vector3(1.2f,2.15f,.5f));
        Place("Desk",new Vector3(-1.15f,FloorY,3.5f),0,new Vector3(1.2f,.85f,.5f));
        Place("Chair1",new Vector3(-1.15f,FloorY,2.85f),0,new Vector3(.5f,1,.5f));
        Place("Toilet",new Vector3(3.7f,FloorY,3.2f),180,new Vector3(.6f,1,.8f));
        Place("BathroomSink",new Vector3(1.75f,FloorY,3.45f),180,new Vector3(.75f,1.1f,.5f));
        Place("Mirror1",new Vector3(1.75f,1.35f,3.86f),180,new Vector3(.65f,.9f,.06f));
        Place("SoapDispenser",new Vector3(1.95f,FloorY+.9f,3.4f),0,new Vector3(.09f,.2f,.09f));
        Place("WaterGarden",new Vector3(-1.35f,.16f,-5),0,new Vector3(.45f,1,.45f));
        Place("Door",new Vector3(.15f,FloorY,-4.55f),90,new Vector3(1.05f,2.3f,.16f));
    }
    static void Place(string name,Vector3 bottomCenter,float angle,Vector3 fit) {
        var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Source+"(Prb)"+name+".prefab");
        if(prefab==null) throw new InvalidOperationException("Falta el prefab "+name);
        var holder=Group(furniture,name);
        var instance=(GameObject)PrefabUtility.InstantiatePrefab(prefab,holder);
        instance.transform.localPosition=Vector3.zero;
        instance.transform.localRotation=Quaternion.identity;
        var bounds=Bounds(instance);
        float regular=Mathf.Min(fit.x/Mathf.Max(bounds.size.x,.001f),fit.z/Mathf.Max(bounds.size.z,.001f));
        float turned=Mathf.Min(fit.x/Mathf.Max(bounds.size.z,.001f),fit.z/Mathf.Max(bounds.size.x,.001f));
        if(turned>regular*1.1f) { instance.transform.localRotation=Quaternion.Euler(0,90,0); bounds=Bounds(instance); }
        float scale=Mathf.Min(fit.x/Mathf.Max(bounds.size.x,.001f),fit.y/Mathf.Max(bounds.size.y,.001f),fit.z/Mathf.Max(bounds.size.z,.001f));
        instance.transform.localScale*=scale;
        bounds=Bounds(instance);
        instance.transform.position-=new Vector3(bounds.center.x,bounds.min.y,bounds.center.z);
        holder.localPosition=bottomCenter; holder.localRotation=Quaternion.Euler(0,angle,0);
        foreach(var renderer in instance.GetComponentsInChildren<Renderer>(true)) {
            var mapped=renderer.sharedMaterials;
            for(int i=0;i<mapped.Length;i++) mapped[i]=ConvertMaterial(mapped[i]);
            renderer.sharedMaterials=mapped;
        }
        if(instance.GetComponentsInChildren<Collider>(true).Length==0) {
            var collider=instance.AddComponent<BoxCollider>();
            var world=Bounds(instance);
            collider.center=instance.transform.InverseTransformPoint(world.center);
            // Root has no rotation beyond the holder. Convert each size axis by scale.
            collider.size=new Vector3(world.size.x,world.size.y,world.size.z)/Mathf.Max(scale,.001f);
        }
    }
    static Bounds Bounds(GameObject go) {
        var renderers=go.GetComponentsInChildren<Renderer>(true);
        if(renderers.Length==0) throw new InvalidOperationException("Objeto sin geometria: "+go.name);
        var bounds=renderers[0].bounds;
        foreach(var r in renderers) bounds.Encapsulate(r.bounds);
        return bounds;
    }
    static Material ConvertMaterial(Material source) {
        if(source==null) throw new InvalidOperationException("Material faltante en el paquete.");
        string key="nappin_"+source.name;
        if(materials.TryGetValue(key,out var saved)) return saved;
        bool glass=source.name.IndexOf("Glass",StringComparison.OrdinalIgnoreCase)>=0;
        Color color=source.HasProperty("_Color")?source.GetColor("_Color"):Color.white;
        if(glass) color=new Color(.7f,.83f,.85f,.2f);
        var mat=Mat(key,color,glass);
        Texture texture=source.HasProperty("_MainTex")?source.GetTexture("_MainTex"):null;
        if(texture==null && source.HasProperty("_BaseMap")) texture=source.GetTexture("_BaseMap");
        if(texture!=null) { mat.SetTexture("_BaseMap",texture); mat.SetTextureScale("_BaseMap",source.mainTextureScale); }
        if(source.name.Contains("Emissive")) { mat.EnableKeyword("_EMISSION"); mat.SetColor("_EmissionColor",new Color(1,.7f,.35f)*1.5f); }
        EditorUtility.SetDirty(mat); materials[key]=mat; return mat;
    }
    static Material Mat(string name,Color color,bool transparent=false) {
        string path=Folder+"/Materials/"+name+".mat";
        var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(mat!=null) return mat;
        var shader=Shader.Find("Universal Render Pipeline/Lit");
        mat=new Material(shader){name=name}; mat.SetColor("_BaseColor",color); mat.SetFloat("_Smoothness",.2f); mat.SetFloat("_Cull",0);
        if(transparent) {
            mat.SetFloat("_Surface",1); mat.SetFloat("_SrcBlend",(float)BlendMode.SrcAlpha); mat.SetFloat("_DstBlend",(float)BlendMode.OneMinusSrcAlpha);
            mat.SetFloat("_ZWrite",0); mat.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); mat.SetOverrideTag("RenderType","Transparent"); mat.renderQueue=3000;
        }
        AssetDatabase.CreateAsset(mat,path); return mat;
    }
    static void Light(Transform root,string name,Vector3 position,float intensity,float range) {
        var t=Group(root,name); t.localPosition=position; var light=t.gameObject.AddComponent<Light>();
        light.type=LightType.Point; light.color=new Color(1,.83f,.62f); light.intensity=intensity; light.range=range;
    }
    static void Validate(GameObject root) {
        Physics.SyncTransforms();
        foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
            foreach(var mat in renderer.sharedMaterials)
                if(mat==null || mat.shader.name!="Universal Render Pipeline/Lit" || !mat.shader.isSupported)
                    throw new InvalidOperationException("Material incompatible en "+renderer.name);
        // Probe passage widths at body height, ignoring the floor and furniture elsewhere.
        foreach(var passage in new[]{new Vector3(1,1.2f,-4),new Vector3(-1.6f,1.2f,1.2f),new Vector3(2.9f,1.2f,1.2f)})
            foreach(var collider in root.GetComponentsInChildren<Collider>())
                if(collider.bounds.Intersects(new Bounds(passage,new Vector3(.65f,1.8f,.18f))))
                    throw new InvalidOperationException("Paso bloqueado por "+collider.name+" en "+passage);
        Debug.Log("Mini casa verificada: "+furniture.childCount+" muebles del paquete, materiales URP y tres pasos libres de 65 cm.");
    }
    static void ClearGrass(Terrain terrain,Vector3 housePosition) {
        var data=terrain.terrainData;
        if(data.detailPrototypes.Length==0) return;
        Undo.RegisterCompleteObjectUndo(data,"Limpiar hierba bajo la casa");
        for(int layer=0;layer<data.detailPrototypes.Length;layer++) {
            var map=data.GetDetailLayer(0,0,data.detailWidth,data.detailHeight,layer);
            for(int z=0;z<data.detailHeight;z++) for(int x=0;x<data.detailWidth;x++) {
                float wx=terrain.transform.position.x+(x+.5f)/data.detailWidth*data.size.x-housePosition.x;
                float wz=terrain.transform.position.z+(z+.5f)/data.detailHeight*data.size.z-housePosition.z;
                if((Mathf.Abs(wx)<4.9f && wz>-6.2f && wz<4.4f) || (Mathf.Abs(wx-1)<.9f && wz>-10.1f && wz<-6)) map[z,x]=0;
            }
            data.SetDetailLayer(0,0,layer,map);
        }
        EditorUtility.SetDirty(data); AssetDatabase.SaveAssets(); terrain.Flush();
    }
    static void Focus(GameObject go) {
        Selection.activeGameObject=go;
        if(SceneView.lastActiveSceneView!=null) SceneView.lastActiveSceneView.LookAt(go.transform.position+new Vector3(0,1.5f,0),Quaternion.Euler(25,-30,0),12,false,true);
    }
    [MenuItem("Tools/Fear Mansion/Preview Mini House Interior %#&F4")]
    public static void PreviewInterior() {
        var go=GameObject.Find("MiniCasa_Amueblada");
        if(go==null) return;
        var roof=go.transform.Find("Techo");
        SceneVisibilityManager.instance.Hide(roof.gameObject,true);
        Selection.activeGameObject=null;
        if(SceneView.lastActiveSceneView!=null) SceneView.lastActiveSceneView.LookAt(go.transform.position+new Vector3(0,1,0),Quaternion.Euler(70,0,0),7,false,true);
    }
    [MenuItem("Tools/Fear Mansion/Show Mini House Roof %#&F3")]
    public static void ShowRoof() {
        var go=GameObject.Find("MiniCasa_Amueblada");
        if(go==null) return;
        SceneVisibilityManager.instance.Show(go.transform.Find("Techo").gameObject,true);
        Focus(go);
    }
    static void EnsureFolder(string path) {
        if(AssetDatabase.IsValidFolder(path)) return;
        int slash=path.LastIndexOf('/'); EnsureFolder(path.Substring(0,slash));
        AssetDatabase.CreateFolder(path.Substring(0,slash),path.Substring(slash+1));
    }
}
#endif
