#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.SceneManagement;

// Geometry-based grass: no texture cutout, legacy vegetation shader or runtime script.
public static class FearMansionGrassBuilder
{
    const string Folder = "Assets/FearMansion/Pasto";

    [MenuItem("Tools/Fear Mansion/Create Grass %#&F8")]
    public static void CreateGrass()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
            throw new InvalidOperationException("Sal de Play para crear el pasto.");

        EnsureFolder(Folder + "/Materials");
        EnsureFolder(Folder + "/Meshes");
        EnsureFolder(Folder + "/Prefabs");
        var shader = Shader.Find("Universal Render Pipeline/Lit");
        if (shader == null || !shader.isSupported)
            throw new InvalidOperationException("No se encontro el shader URP/Lit compatible.");

        Material[] greens = {
            Material("Pasto_Verde", shader, new Color(.19f,.37f,.065f)),
            Material("Pasto_Claro", shader, new Color(.32f,.49f,.11f)),
            Material("Pasto_Oscuro", shader, new Color(.10f,.25f,.04f))
        };
        Material ground = Material("Suelo_Cesped", shader, new Color(.28f,.4f,.13f));
        string texturePath = "Assets/Polytope Studio/Lowpoly_Environments/Sources/Textures/PT_Ground_Grass_Green_01.png";
        var texture = AssetDatabase.LoadAssetAtPath<Texture2D>(texturePath);
        if (texture != null) {
            ground.SetTexture("_BaseMap", texture);
            ground.SetColor("_BaseColor", Color.white);
            ground.SetTextureScale("_BaseMap",new Vector2(2,2));
            EditorUtility.SetDirty(ground);
        }

        Mesh tuft = GrassMesh("Pasto_Mata",false);
        Mesh patch = GrassMesh("Pasto_Parche_4x4",true);
        var preview = EditorSceneManager.NewPreviewScene();
        try {
            Save("Pasto_Mata_3D", preview, root => Grass(root,tuft,greens));
            Save("Pasto_Parche_4x4", preview, root => Grass(root,patch,greens));
            Save("Suelo_Con_Pasto_4x4", preview, root => {
                Ground(root,4,ground);
                Grass(root,patch,greens);
            });
            Save("Suelo_Con_Pasto_20x20", preview, root => {
                Ground(root,20,ground);
                for(int z=0;z<5;z++) for(int x=0;x<5;x++) {
                    var group = new GameObject("Pasto_"+x+"_"+z).transform;
                    group.SetParent(root,false);
                    group.localPosition=new Vector3(-8+x*4,0,-8+z*4);
                    group.localRotation=Quaternion.Euler(0,((x+z)%4)*90,0);
                    Grass(group,patch,greens);
                }
            });
        } finally { EditorSceneManager.ClosePreviewScene(preview); }

        AssetDatabase.SaveAssets();
        Validate();
        StageUtility.GoToMainStage();
        var scene = SceneManager.GetActiveScene();
        if(!scene.IsValid() || !scene.isLoaded)
            throw new InvalidOperationException("No hay una escena abierta para colocar el suelo.");
        GameObject placed = GameObject.Find("Suelo_Con_Pasto_Listo");
        if(placed==null) {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Folder+"/Prefabs/Suelo_Con_Pasto_20x20.prefab");
            placed=(GameObject)PrefabUtility.InstantiatePrefab(prefab,scene);
            placed.name="Suelo_Con_Pasto_Listo";
            var player=GameObject.Find("Jugador");
            if(player!=null) placed.transform.position=new Vector3(player.transform.position.x,0,player.transform.position.z);
            Undo.RegisterCreatedObjectUndo(placed,"Crear suelo con pasto");
            EditorSceneManager.MarkSceneDirty(scene);
        }
        Selection.activeGameObject=placed;
        if(SceneView.lastActiveSceneView!=null) SceneView.lastActiveSceneView.FrameSelected();
        Debug.Log("Pasto listo: 4 prefabs URP verificados, hierba 3D y suelo de 20x20 colocado. Guarda tu escena con Ctrl+S.");
    }

    static void EnsureFolder(string path) {
        if(AssetDatabase.IsValidFolder(path)) return;
        int slash=path.LastIndexOf('/');
        EnsureFolder(path.Substring(0,slash));
        AssetDatabase.CreateFolder(path.Substring(0,slash),path.Substring(slash+1));
    }
    static Material Material(string name,Shader shader,Color color) {
        string path=Folder+"/Materials/"+name+".mat";
        var mat=AssetDatabase.LoadAssetAtPath<Material>(path);
        if(mat!=null) return mat;
        mat=new Material(shader){name=name,enableInstancing=true,doubleSidedGI=true};
        mat.SetColor("_BaseColor",color);
        mat.SetFloat("_Smoothness",0);
        mat.SetFloat("_Cull",0);
        AssetDatabase.CreateAsset(mat,path);
        return mat;
    }
    static Mesh GrassMesh(string name,bool patch) {
        string path=Folder+"/Meshes/"+name+".asset";
        var saved=AssetDatabase.LoadAssetAtPath<Mesh>(path);
        if(saved!=null) return saved;
        var random=new System.Random(patch?9173:420);
        var vertices=new List<Vector3>();
        var tris=new[]{new List<int>(),new List<int>(),new List<int>()};
        int count=patch?144:1;
        for(int tuft=0;tuft<count;tuft++) {
            // Jittered cells distribute grass evenly without a rigid grid.
            float x=patch?(-2f+(tuft%12+.15f+(float)random.NextDouble()*.7f)/3f):0;
            float z=patch?(-2f+(tuft/12+.15f+(float)random.NextDouble()*.7f)/3f):0;
            for(int blade=0;blade<12;blade++) {
                float a=(float)random.NextDouble()*Mathf.PI*2;
                var direction=new Vector3(Mathf.Cos(a),0,Mathf.Sin(a));
                var side=new Vector3(-direction.z,0,direction.x);
                float height=.18f+(float)random.NextDouble()*.23f;
                float width=.025f+(float)random.NextDouble()*.025f;
                Vector3 foot=new Vector3(x,0,z)+direction*((float)random.NextDouble()*.1f);
                Vector3 mid=foot+Vector3.up*height*.55f+direction*height*.15f;
                Vector3 tip=foot+Vector3.up*height+direction*height*.45f;
                int i=vertices.Count;
                vertices.Add(foot-side*width); vertices.Add(foot+side*width);
                vertices.Add(mid-side*width*.55f); vertices.Add(mid+side*width*.55f);
                vertices.Add(tip);
                var indices=tris[random.Next(3)];
                indices.AddRange(new[]{i,i+2,i+1,i+1,i+2,i+3,i+2,i+4,i+3});
            }
        }
        var mesh=new Mesh {name=name};
        mesh.SetVertices(vertices);
        mesh.subMeshCount=3;
        for(int i=0;i<3;i++) mesh.SetTriangles(tris[i],i);
        mesh.RecalculateNormals(); mesh.RecalculateBounds();
        AssetDatabase.CreateAsset(mesh,path);
        return mesh;
    }
    static void Grass(Transform parent,Mesh mesh,Material[] materials) {
        var grass=new GameObject("Hierba_3D"); grass.transform.SetParent(parent,false);
        grass.AddComponent<MeshFilter>().sharedMesh=mesh;
        var renderer=grass.AddComponent<MeshRenderer>(); renderer.sharedMaterials=materials;
        renderer.shadowCastingMode=ShadowCastingMode.Off;
    }
    static void Ground(Transform parent,float size,Material mat) {
        var floor=GameObject.CreatePrimitive(PrimitiveType.Cube);
        floor.name="Suelo_Caminable"; floor.transform.SetParent(parent,false);
        floor.transform.localPosition=new Vector3(0,-.1f,0);
        floor.transform.localScale=new Vector3(size,.2f,size);
        floor.GetComponent<MeshRenderer>().sharedMaterial=mat;
    }
    static void Save(string name,Scene scene,Action<Transform> build) {
        string path=Folder+"/Prefabs/"+name+".prefab";
        if(AssetDatabase.LoadAssetAtPath<GameObject>(path)!=null) return;
        var root=new GameObject(name);
        SceneManager.MoveGameObjectToScene(root,scene);
        try {
            build(root.transform);
            PrefabUtility.SaveAsPrefabAsset(root,path,out bool success);
            if(!success) throw new InvalidOperationException("No se pudo guardar "+path);
        } finally { UnityEngine.Object.DestroyImmediate(root); }
    }
    static void Validate() {
        var guids=AssetDatabase.FindAssets("t:Prefab",new[]{Folder+"/Prefabs"});
        if(guids.Length!=4) throw new InvalidOperationException("Faltan prefabs de pasto.");
        foreach(var guid in guids) {
            var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(AssetDatabase.GUIDToAssetPath(guid));
            foreach(var renderer in prefab.GetComponentsInChildren<MeshRenderer>()) {
                var mesh=renderer.GetComponent<MeshFilter>().sharedMesh;
                if(mesh==null || mesh.vertexCount==0) throw new InvalidOperationException("Malla de pasto vacia.");
                foreach(var mat in renderer.sharedMaterials)
                    if(mat==null || mat.shader.name!="Universal Render Pipeline/Lit" || !mat.shader.isSupported)
                        throw new InvalidOperationException("Material de pasto incompatible.");
            }
            if(prefab.name.StartsWith("Suelo") && prefab.GetComponentInChildren<BoxCollider>()==null)
                throw new InvalidOperationException("Falta la colision del suelo.");
        }
    }
}
#endif
