
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Collections.Generic;

/// <summary>
/// Static horror-mansion blockout generator.
/// Creates only geometry, colliders, simple materials and lights.
/// No gameplay, scripts, puzzles, enemies or proprietary game assets.
/// Supports the Built-in pipeline and URP.
/// </summary>
public partial class FearMansionStaticMapGenerator : EditorWindow
{
    const float H = 3.2f;
    const float T = 0.18f;
    const float DoorW = 1.15f;
    const float DoorH = 2.35f;
    const float UpperY = 3.6f;
    const float StairStart = 5.8f;
    const float StairEnd = 11.2f;
    const float StairOpeningWidth = 3.0f;

    static Transform root;
    static Dictionary<string, Material> mats = new Dictionary<string, Material>();
    const string LibraryPath = "Assets/FearMansion";

    static void EnsureFolder(string path)
    {
        if (AssetDatabase.IsValidFolder(path)) return;
        int slash = path.LastIndexOf('/');
        string parent = path.Substring(0, slash);
        EnsureFolder(parent);
        AssetDatabase.CreateFolder(parent, path.Substring(slash + 1));
    }

    [MenuItem("Tools/Fear Mansion/Create Reusable Pieces %#&F9")]
    public static void CreateReusablePieces()
    {
        if (EditorApplication.isPlayingOrWillChangePlaymode)
        {
            Debug.LogWarning("Sal del modo Play antes de crear las piezas reutilizables.");
            return;
        }
        EnsureFolder(LibraryPath + "/Prefabs/Construccion");
        EnsureFolder(LibraryPath + "/Prefabs/Muebles");
        EnsureFolder(LibraryPath + "/Prefabs/Exterior");
        CreateMaterials();
        Transform previousRoot = root;
        var preview = UnityEditor.SceneManagement.EditorSceneManager.NewPreviewScene();
        try
        {
            SavePiece("Construccion/Pared_1m", () => WallX("Pared",-.5f,.5f,0,0,mats["wall"]), preview);
            SavePiece("Construccion/Pared_2m", () => WallX("Pared",-1,1,0,0,mats["wall"]), preview);
            SavePiece("Construccion/Pared_4m", () => WallX("Pared",-2,2,0,0,mats["wall"]), preview);
            SavePiece("Construccion/Pared_4m_Clara", () => WallX("Pared",-2,2,0,0,mats["wall2"]), preview);
            SavePiece("Construccion/Pared_Con_Puerta", () => DoorWallX("Pared",-2,2,0,0,0,mats["wall"]), preview);
            SavePiece("Construccion/Marco_Puerta", () => DoorFrameX("Marco",0,0,0), preview);
            SavePiece("Construccion/Puerta_Cerrada", () => StaticDoorX("Puerta",0,0,0,false), preview);
            SavePiece("Construccion/Puerta_Abierta", () => StaticDoorX("Puerta",0,0,0,true), preview);
            SavePiece("Construccion/Ventana_Decorativa", () => WindowX(0,0,0), preview);
            SavePiece("Construccion/Piso_Madera_2x2", () => Floor("Piso",0,0,2,2,0,mats["floor"]), preview);
            SavePiece("Construccion/Piso_Madera_4x4", () => Floor("Piso",0,0,4,4,0,mats["floor"]), preview);
            SavePiece("Construccion/Piso_Baldosa_2x2", () => Floor("Piso",0,0,2,2,0,mats["tile"]), preview);
            SavePiece("Construccion/Techo_4x4", () => Ceiling("Techo",0,0,4,4,0), preview);
            SavePiece("Construccion/Escalera_3_6m", () => Stairs(0,0,0,18,2.8f,UpperY), preview);
            SavePiece("Muebles/Sofa", () => Sofa(0,0,0,0), preview);
            SavePiece("Muebles/Mesa", () => Table(0,0,0,2,1), preview);
            SavePiece("Muebles/Mesa_Comedor", () => Table(0,0,0,3,1.5f), preview);
            SavePiece("Muebles/Silla", () => Box("Silla",new Vector3(0,.45f,0),new Vector3(.55f,.9f,.55f),mats["trim"]), preview);
            SavePiece("Muebles/Chimenea", () => Fireplace(0,0,0,0), preview);
            SavePiece("Muebles/Estanteria", () => Shelf(0,0,0,2.4f), preview);
            SavePiece("Muebles/Escritorio", () => Desk(0,0,0,0), preview);
            SavePiece("Muebles/Cocina", () => KitchenCounters(0,0,0), preview);
            SavePiece("Muebles/Bano", () => SinkBath(0,0,0), preview);
            SavePiece("Muebles/Cama", () => Bed(0,0,0,0), preview);
            SavePiece("Muebles/Armario", () => Wardrobe(0,0,0,0), preview);
            SavePiece("Muebles/Comoda", () => Dresser(0,0,0,0), preview);
            SavePiece("Muebles/Mesa_Noche", () => Nightstand(0,0,0), preview);
            SavePiece("Muebles/Cuna", () => Crib(0,0,0), preview);
            SavePiece("Muebles/Mecedora", () => RockingChair(0,0,0), preview);
            SavePiece("Exterior/Columna", () => Box("Columna",new Vector3(0,1.55f,0),new Vector3(.25f,3.1f,.25f),mats["trim"]), preview);
            SavePiece("Exterior/Escalon", () => Box("Escalon",new Vector3(0,-.15f,0),new Vector3(5,.3f,1),mats["tile"]), preview);
            SavePiece("Exterior/Porche", () => Floor("Porche",0,0,7.5f,2.6f,0,mats["tile"]), preview);
            CreateDetailedPieces(preview);
            AssetDatabase.SaveAssets();
            ValidateReusablePieces();
        }
        finally
        {
            root = previousRoot;
            UnityEditor.SceneManagement.EditorSceneManager.ClosePreviewScene(preview);
        }
        Selection.activeObject = AssetDatabase.LoadAssetAtPath<DefaultAsset>(LibraryPath + "/Prefabs");
        EditorGUIUtility.PingObject(Selection.activeObject);
    }

    static void SavePiece(string name, System.Action build, UnityEngine.SceneManagement.Scene preview)
    {
        string path = LibraryPath + "/Prefabs/" + name + ".prefab";
        // Preserve any manual edits when the command is run again.
        if (AssetDatabase.LoadAssetAtPath<GameObject>(path) != null) return;
        var piece = new GameObject(name.Substring(name.LastIndexOf('/') + 1));
        UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(piece, preview);
        root = piece.transform;
        try
        {
            build();
            PrefabUtility.SaveAsPrefabAsset(piece, path, out bool success);
            if (!success) throw new System.InvalidOperationException("No se pudo guardar " + path);
        }
        finally { DestroyImmediate(piece); }
    }

    public static void ValidateReusablePieces()
    {
        var guids = AssetDatabase.FindAssets("t:Prefab", new[] { LibraryPath + "/Prefabs" });
        if (guids.Length < 32) throw new System.InvalidOperationException("Faltan piezas en la biblioteca.");
        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var renderers = prefab.GetComponentsInChildren<MeshRenderer>(true);
            if (renderers.Length == 0 || prefab.transform.position != Vector3.zero || prefab.transform.localScale != Vector3.one)
                throw new System.InvalidOperationException("Prefab invalido: " + path);
            foreach (var renderer in renderers)
                if (renderer.sharedMaterial == null || !AssetDatabase.Contains(renderer.sharedMaterial) ||
                    renderer.GetComponent<BoxCollider>() == null || renderer.GetComponent<MeshFilter>().sharedMesh == null)
                    throw new System.InvalidOperationException("Material, malla o collider faltante: " + path);
        }
        Debug.Log("Fear Mansion: " + guids.Length + " prefabs verificados con materiales, mallas y colliders en " + LibraryPath + "/Prefabs");
    }

    [MenuItem("Tools/Fear Mansion/Generate Static Map")]
    public static void Generate()
    {
        if (GameObject.Find("FearMansion_StaticMap") != null)
        {
            if (!EditorUtility.DisplayDialog(
                "Regenerar mapa",
                "Ya existe FearMansion_StaticMap. ¿Eliminarlo y crear uno nuevo?",
                "Sí", "Cancelar")) return;

            Undo.DestroyObjectImmediate(GameObject.Find("FearMansion_StaticMap"));
        }

        mats.Clear();
        var r = new GameObject("FearMansion_StaticMap");
        root = r.transform;
        Undo.RegisterCreatedObjectUndo(r, "Generate Fear Mansion Static Map");

        CreateMaterials();
        CreateGroundFloor();
        CreateUpperFloor();
        CreateExteriorDetails();
        CreateLighting();
        AssetDatabase.SaveAssets();

        Selection.activeGameObject = r;
        Debug.Log("Fear Mansion Static Map generado. Todo está bajo FearMansion_StaticMap.");
    }

    static void CreateMaterials()
    {
        mats["floor"] = Mat("FM_Floor_Wood", new Color(0.16f,0.105f,0.07f), 0.12f);
        mats["tile"]  = Mat("FM_Floor_Tile", new Color(0.24f,0.24f,0.22f), 0.05f);
        mats["wall"]  = Mat("FM_Wall_Green", new Color(0.23f,0.29f,0.20f), 0.05f);
        mats["wall2"] = Mat("FM_Wall_Dirty", new Color(0.31f,0.29f,0.23f), 0.02f);
        mats["trim"]  = Mat("FM_Trim_Dark", new Color(0.08f,0.055f,0.04f), 0.08f);
        mats["door"]  = Mat("FM_Door_Wood", new Color(0.14f,0.075f,0.035f), 0.14f);
        mats["ceil"]  = Mat("FM_Ceiling", new Color(0.30f,0.30f,0.27f), 0.02f);
        mats["metal"] = Mat("FM_Metal", new Color(0.10f,0.11f,0.11f), 0.45f);
        mats["cloth"] = Mat("FM_Cloth", new Color(0.18f,0.035f,0.03f), 0.03f);
        mats["glass"] = Mat("FM_Glass", new Color(0.18f,0.25f,0.28f), 0.6f);
    }

    static Material Mat(string name, Color color, float metallic)
    {
        EnsureFolder(LibraryPath + "/Materials");
        string path = LibraryPath + "/Materials/" + name + ".mat";
        var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
        if (existing != null) return existing;
        Shader sh = Shader.Find(GraphicsSettings.currentRenderPipeline == null
            ? "Standard" : "Universal Render Pipeline/Lit");
        if (!sh) throw new System.InvalidOperationException("No compatible material shader was found.");
        var m = new Material(sh);
        m.name = name;
        if (m.HasProperty("_Color")) m.color = color;
        if (m.HasProperty("_BaseColor")) m.SetColor("_BaseColor", color);
        if (m.HasProperty("_Metallic")) m.SetFloat("_Metallic", metallic);
        if (m.HasProperty("_Smoothness")) m.SetFloat("_Smoothness", 0.18f);
        if (m.HasProperty("_Glossiness")) m.SetFloat("_Glossiness", 0.18f);
        AssetDatabase.CreateAsset(m, path);
        return m;
    }

    static GameObject Box(string name, Vector3 pos, Vector3 scale, Material mat, Transform parent=null)
    {
        var g = GameObject.CreatePrimitive(PrimitiveType.Cube);
        g.name = name;
        g.transform.position = pos;
        g.transform.localScale = scale;
        g.transform.SetParent(parent ? parent : root, true);
        g.transform.localRotation = Quaternion.identity;
        if (mat) g.GetComponent<Renderer>().sharedMaterial = mat;
        return g;
    }

    static void WallX(string name, float x1, float x2, float z, float y, Material mat)
    {
        float len = Mathf.Abs(x2-x1);
        float cx = (x1+x2)*0.5f;
        Box(name, new Vector3(cx, y+H*0.5f, z), new Vector3(len,H,T), mat);
    }

    static void WallZ(string name, float z1, float z2, float x, float y, Material mat)
    {
        float len = Mathf.Abs(z2-z1);
        float cz = (z1+z2)*0.5f;
        Box(name, new Vector3(x, y+H*0.5f, cz), new Vector3(T,H,len), mat);
    }

    static void DoorWallX(string name, float x1, float x2, float z, float doorCenterX, float y, Material mat)
    {
        float a = doorCenterX - DoorW*0.5f;
        float b = doorCenterX + DoorW*0.5f;
        if (a > x1) WallX(name+"_L", x1,a,z,y,mat);
        if (b < x2) WallX(name+"_R", b,x2,z,y,mat);
        Box(name+"_Header", new Vector3(doorCenterX, y+DoorH+(H-DoorH)*0.5f, z),
            new Vector3(DoorW,H-DoorH,T), mat);
        DoorFrameX(name+"_Frame", doorCenterX,z,y);
    }

    static void DoorWallZ(string name, float z1, float z2, float x, float doorCenterZ, float y, Material mat)
    {
        float a = doorCenterZ - DoorW*0.5f;
        float b = doorCenterZ + DoorW*0.5f;
        if (a > z1) WallZ(name+"_A", z1,a,x,y,mat);
        if (b < z2) WallZ(name+"_B", b,z2,x,y,mat);
        Box(name+"_Header", new Vector3(x, y+DoorH+(H-DoorH)*0.5f, doorCenterZ),
            new Vector3(T,H-DoorH,DoorW), mat);
        DoorFrameZ(name+"_Frame", x,doorCenterZ,y);
    }

    static void DoorFrameX(string name, float x, float z, float y)
    {
        var p = new GameObject(name).transform; p.SetParent(root);
        Box("jambL", new Vector3(x-DoorW*.5f,y+DoorH*.5f,z), new Vector3(.09f,DoorH,T+.06f), mats["trim"], p);
        Box("jambR", new Vector3(x+DoorW*.5f,y+DoorH*.5f,z), new Vector3(.09f,DoorH,T+.06f), mats["trim"], p);
        Box("lintel",new Vector3(x,y+DoorH,z), new Vector3(DoorW+.18f,.10f,T+.06f), mats["trim"], p);
    }

    static void DoorFrameZ(string name, float x, float z, float y)
    {
        var p = new GameObject(name).transform; p.SetParent(root);
        Box("jambA", new Vector3(x,y+DoorH*.5f,z-DoorW*.5f), new Vector3(T+.06f,DoorH,.09f), mats["trim"], p);
        Box("jambB", new Vector3(x,y+DoorH*.5f,z+DoorW*.5f), new Vector3(T+.06f,DoorH,.09f), mats["trim"], p);
        Box("lintel",new Vector3(x,y+DoorH,z), new Vector3(T+.06f,.10f,DoorW+.18f), mats["trim"], p);
    }

    static void Floor(string name, float cx,float cz,float sx,float sz,float y, Material mat)
    {
        Box(name,new Vector3(cx,y-0.08f,cz),new Vector3(sx,0.16f,sz),mat);
    }

    static void Ceiling(string name, float cx,float cz,float sx,float sz,float y)
    {
        Box(name,new Vector3(cx,y+H+0.08f,cz),new Vector3(sx,0.16f,sz),mats["ceil"]);
    }

    // Four slabs leave an actual stair opening, including in the colliders.
    static void SlabWithStairOpening(string name, float centerY, Material mat)
    {
        float sideWidth = (30f - StairOpeningWidth) * .5f;
        float sideX = (15f + StairOpeningWidth * .5f) * .5f;
        Box(name+"_West", new Vector3(-sideX,centerY,0), new Vector3(sideWidth,.16f,24), mat);
        Box(name+"_East", new Vector3(sideX,centerY,0), new Vector3(sideWidth,.16f,24), mat);
        Box(name+"_South", new Vector3(0,centerY,(-12f+StairStart)*.5f),
            new Vector3(StairOpeningWidth,.16f,StairStart+12f), mat);
        Box(name+"_Landing", new Vector3(0,centerY,(StairEnd+12f)*.5f),
            new Vector3(StairOpeningWidth,.16f,12f-StairEnd), mat);
    }

    static void RoomLabel(string n, float x, float y, float z)
    {
        var g = new GameObject("ROOM_"+n);
        g.transform.position = new Vector3(x,y,z);
        g.transform.SetParent(root);
    }

    // ---------------- GROUND FLOOR ----------------
    static void CreateGroundFloor()
    {
        float y=0;
        // Overall footprint: x -15..15, z -12..12
        Floor("GF_MainFloor",0,0,30,24,y,mats["floor"]);
        SlabWithStairOpening("GF_Ceiling",y+H+.08f,mats["ceil"]);

        // Exterior walls and main entrance.
        DoorWallX("GF_SouthWall",-15,15,-12,0,y,mats["wall2"]);
        WallX("GF_NorthWall",-15,15,12,y,mats["wall2"]);
        WallZ("GF_WestWall",-12,12,-15,y,mats["wall2"]);
        WallZ("GF_EastWall",-12,12,15,y,mats["wall2"]);

        // Central corridor, z -9..9, width 3.4
        DoorWallZ("GF_Corridor_W",-9,0,-1.7f,-5.5f,y,mats["wall"]);
        DoorWallZ("GF_Corridor_W2",0,9,-1.7f, 4.5f,y,mats["wall"]);
        DoorWallZ("GF_Corridor_E",-9,0, 1.7f,-3.0f,y,mats["wall"]);
        DoorWallZ("GF_Corridor_E2",0,9, 1.7f, 5.7f,y,mats["wall"]);

        // Foyer cross-wall.
        DoorWallX("GF_FoyerSep",-15,-1.7f,-8.5f,-8.0f,y,mats["wall"]);
        DoorWallX("GF_FoyerSepR",1.7f,15,-8.5f,8.0f,y,mats["wall"]);

        // West wing partitions: living + study + storage/bath.
        DoorWallX("GF_West_A",-15,-1.7f,-1.8f,-8.2f,y,mats["wall"]);
        DoorWallX("GF_West_B",-15,-1.7f,5.0f,-9.8f,y,mats["wall"]);
        DoorWallZ("GF_West_StudySplit",-1.8f,5.0f,-8.8f,1.2f,y,mats["wall2"]);

        // East wing partitions: dining + kitchen + utility.
        DoorWallX("GF_East_A",1.7f,15,-0.2f,8.5f,y,mats["wall"]);
        DoorWallX("GF_East_B",1.7f,15,6.2f,9.5f,y,mats["wall"]);
        DoorWallZ("GF_East_KitchenSplit",-0.2f,6.2f,9.2f,3.1f,y,mats["wall2"]);

        // Rear stair hall opening around north center.
        DoorWallX("GF_RearSep",-15,-1.7f,9.2f,-7.8f,y,mats["wall"]);
        DoorWallX("GF_RearSepR",1.7f,15,9.2f,8.1f,y,mats["wall"]);

        // Static closed doors for visual rhythm.
        StaticDoorX("EntranceDoor",0,-11.89f,y, false);
        StaticDoorZ("LivingDoor",-1.59f,-5.5f,y, false);
        StaticDoorZ("DiningDoor",1.59f,-3.0f,y, false);
        StaticDoorZ("StudyDoor",-1.59f,4.5f,y, false);
        StaticDoorZ("KitchenDoor",1.59f,5.7f,y, false);

        // Furniture/props blockout
        Sofa(-10.4f,-5.2f,y,0);
        Sofa(-11.8f,-7.2f,y,90);
        Table(-7.8f,-5.6f,y,2.0f,1.0f);
        Fireplace(-14.65f,-4.0f,y,90);
        Shelf(-10.4f,2.3f,y,2.4f);
        Desk(-6.2f,3.5f,y,0);
        Table(6.2f,-4.3f,y,3.0f,1.5f);
        ChairRing(6.2f,-4.3f,y);
        KitchenCounters(11.2f,2.8f,y);
        Shelf(11.6f,8.1f,y,2.6f);
        SinkBath(-11.8f,8.0f,y);

        // Stairs at back center.
        Stairs(0,StairStart,y,18,2.8f,UpperY);

        // Windows
        WindowX(-8,-11.91f,y);
        WindowX(8,-11.91f,y);
        WindowZ(-14.91f,-4,y);
        WindowZ(14.91f,-4,y);
        WindowZ(-14.91f,4,y);
        WindowZ(14.91f,4,y);

        RoomLabel("Foyer",0,y,-10);
        RoomLabel("MainCorridor",0,y,0);
        RoomLabel("LivingRoom",-8,y,-5);
        RoomLabel("Study",-5,y,2);
        RoomLabel("BathStorage",-12,y,2);
        RoomLabel("DiningRoom",6,y,-4);
        RoomLabel("Kitchen",12,y,2);
        RoomLabel("Utility",11,y,8);
        RoomLabel("StairHall",0,y,10.2f);
    }

    // ---------------- UPPER FLOOR ----------------
    static void CreateUpperFloor()
    {
        float y=UpperY;
        SlabWithStairOpening("UF_MainFloor",y-.08f,mats["floor"]);
        Ceiling("UF_Ceiling",0,0,30,24,y);

        WallX("UF_South",-15,15,-12,y,mats["wall2"]);
        WallX("UF_North",-15,15,12,y,mats["wall2"]);
        WallZ("UF_West",-12,12,-15,y,mats["wall2"]);
        WallZ("UF_East",-12,12,15,y,mats["wall2"]);

        // Long central hall.
        DoorWallZ("UF_Hall_W",-10,-2.5f,-1.65f,-5.5f,y,mats["wall"]);
        DoorWallZ("UF_Hall_W2",-2.5f,4.2f,-1.65f,1.2f,y,mats["wall"]);
        DoorWallZ("UF_Hall_W3",4.2f,10,-1.65f,7.0f,y,mats["wall"]);
        DoorWallZ("UF_Hall_E",-10,-3,1.65f,-6.0f,y,mats["wall"]);
        DoorWallZ("UF_Hall_E2",-3,4.5f,1.65f,0.5f,y,mats["wall"]);
        DoorWallZ("UF_Hall_E3",4.5f,10,1.65f,6.8f,y,mats["wall"]);

        // West rooms.
        DoorWallX("UF_WA",-15,-1.65f,-2.5f,-8.5f,y,mats["wall"]);
        DoorWallX("UF_WB",-15,-1.65f,4.2f,-9.4f,y,mats["wall"]);
        // East rooms.
        DoorWallX("UF_EA",1.65f,15,-3.0f,8.6f,y,mats["wall"]);
        DoorWallX("UF_EB",1.65f,15,4.5f,8.8f,y,mats["wall"]);

        // Smaller bathroom / closet partitions.
        DoorWallZ("UF_W_Split",-2.5f,4.2f,-9.5f,0.8f,y,mats["wall2"]);
        DoorWallZ("UF_E_Split",-3.0f,4.5f,10.0f,0.4f,y,mats["wall2"]);

        // Bedrooms / nursery props.
        Bed(-8.0f,-7.0f,y,0);
        Wardrobe(-12.5f,-7.2f,y,0);
        Nightstand(-10.1f,-6.8f,y);

        Bed(7.0f,-7.0f,y,0);
        Wardrobe(12.4f,-7.0f,y,0);

        Crib(7.0f,1.0f,y);
        Dresser(12.0f,1.0f,y,90);
        RockingChair(5.2f,2.2f,y);

        Bed(-6.0f,7.3f,y,0);
        Shelf(-12.0f,7.5f,y,2.3f);
        SinkBath(-11.8f,1.0f,y);

        // doors
        StaticDoorZ("MasterDoor",-1.55f,-5.5f,y,false);
        StaticDoorZ("GuestDoor",1.55f,-6.0f,y,false);
        StaticDoorZ("BathDoor",-1.55f,1.2f,y,false);
        StaticDoorZ("NurseryDoor",1.55f,0.5f,y,false);
        StaticDoorZ("WestRearDoor",-1.55f,7.0f,y,false);
        StaticDoorZ("EastRearDoor",1.55f,6.8f,y,false);

        // windows
        WindowX(-8,-11.91f,y);
        WindowX(8,-11.91f,y);
        WindowZ(-14.91f,-6,y);
        WindowZ(14.91f,-6,y);
        WindowZ(-14.91f,6,y);
        WindowZ(14.91f,6,y);

        RoomLabel("MasterBedroom",-8,y,-7);
        RoomLabel("GuestBedroom",8,y,-7);
        RoomLabel("UpperBath",-12,y,1);
        RoomLabel("Nursery",7,y,1);
        RoomLabel("WestRearBedroom",-8,y,7);
        RoomLabel("EastRearRoom",8,y,7);
        RoomLabel("UpperHall",0,y,0);
    }

    static void StaticDoorX(string name,float x,float z,float y,bool open)
    {
        var d=Box(name,new Vector3(x,y+DoorH*.5f,z),new Vector3(DoorW*.92f,DoorH*.97f,.10f),mats["door"]);
        if(open) d.transform.Rotate(0,55,0);
    }
    static void StaticDoorZ(string name,float x,float z,float y,bool open)
    {
        var d=Box(name,new Vector3(x,y+DoorH*.5f,z),new Vector3(.10f,DoorH*.97f,DoorW*.92f),mats["door"]);
        if(open) d.transform.Rotate(0,55,0);
    }

    static void WindowX(float x,float z,float y)
    {
        Box("Window",new Vector3(x,y+1.65f,z),new Vector3(2.0f,1.25f,.07f),mats["glass"]);
        Box("WindowTop",new Vector3(x,y+2.33f,z),new Vector3(2.2f,.10f,.10f),mats["trim"]);
        Box("WindowBottom",new Vector3(x,y+0.97f,z),new Vector3(2.2f,.10f,.10f),mats["trim"]);
    }
    static void WindowZ(float x,float z,float y)
    {
        Box("Window",new Vector3(x,y+1.65f,z),new Vector3(.07f,1.25f,2.0f),mats["glass"]);
        Box("WindowTop",new Vector3(x,y+2.33f,z),new Vector3(.10f,.10f,2.2f),mats["trim"]);
        Box("WindowBottom",new Vector3(x,y+0.97f,z),new Vector3(.10f,.10f,2.2f),mats["trim"]);
    }

    static void Sofa(float x,float z,float y,float rot)
    {
        var p=new GameObject("Sofa_Blockout").transform; p.SetParent(root); p.position=new Vector3(x,y,z); p.rotation=Quaternion.Euler(0,rot,0);
        Box("seat",p.TransformPoint(new Vector3(0,.45f,0)),new Vector3(2.3f,.45f,.9f),mats["cloth"],p);
        Box("back",p.TransformPoint(new Vector3(0,.95f,.38f)),new Vector3(2.3f,.9f,.18f),mats["cloth"],p);
        Box("armL",p.TransformPoint(new Vector3(-1.1f,.65f,0)),new Vector3(.18f,.7f,.9f),mats["cloth"],p);
        Box("armR",p.TransformPoint(new Vector3(1.1f,.65f,0)),new Vector3(.18f,.7f,.9f),mats["cloth"],p);
    }

    static void Table(float x,float z,float y,float sx,float sz)
    {
        Box("TableTop",new Vector3(x,y+.78f,z),new Vector3(sx,.12f,sz),mats["trim"]);
        float hx=sx*.42f,hz=sz*.42f;
        foreach(var p in new[]{new Vector3(-hx,y+.38f,-hz),new Vector3(hx,y+.38f,-hz),new Vector3(-hx,y+.38f,hz),new Vector3(hx,y+.38f,hz)})
            Box("TableLeg",new Vector3(x+p.x,p.y,z+p.z),new Vector3(.10f,.76f,.10f),mats["trim"]);
    }

    static void ChairRing(float x,float z,float y)
    {
        for(int i=0;i<6;i++){
            float a=i*Mathf.PI*2f/6f;
            float cx=x+Mathf.Cos(a)*2.2f, cz=z+Mathf.Sin(a)*1.6f;
            var c=Box("Chair",new Vector3(cx,y+.45f,cz),new Vector3(.55f,.9f,.55f),mats["trim"]);
            c.transform.LookAt(new Vector3(x,y+.45f,z));
        }
    }

    static void Fireplace(float x,float z,float y,float rot)
    {
        var p=new GameObject("Fireplace").transform;p.SetParent(root);p.position=new Vector3(x,y,z);p.rotation=Quaternion.Euler(0,rot,0);
        Box("body",p.TransformPoint(new Vector3(0,.9f,0)),new Vector3(.45f,1.8f,2.4f),mats["wall2"],p);
        Box("opening",p.TransformPoint(new Vector3(-.25f,.65f,0)),new Vector3(.08f,.8f,1.2f),mats["metal"],p);
        Box("mantel",p.TransformPoint(new Vector3(0,1.75f,0)),new Vector3(.65f,.16f,2.7f),mats["trim"],p);
    }

    static void Shelf(float x,float z,float y,float h)
    {
        Box("Shelf",new Vector3(x,y+h*.5f,z),new Vector3(2.0f,h,.42f),mats["trim"]);
        for(int i=1;i<4;i++) Box("ShelfBoard",new Vector3(x,y+i*h/4f,z-.24f),new Vector3(2.05f,.06f,.5f),mats["wall2"]);
    }

    static void Desk(float x,float z,float y,float rot)
    {
        var p=new GameObject("Desk").transform;p.SetParent(root);p.position=new Vector3(x,y,z);p.rotation=Quaternion.Euler(0,rot,0);
        Box("deskTop",p.TransformPoint(new Vector3(0,.78f,0)),new Vector3(1.8f,.12f,.75f),mats["trim"],p);
        Box("cabL",p.TransformPoint(new Vector3(-.72f,.38f,0)),new Vector3(.35f,.75f,.68f),mats["trim"],p);
        Box("cabR",p.TransformPoint(new Vector3(.72f,.38f,0)),new Vector3(.35f,.75f,.68f),mats["trim"],p);
    }

    static void KitchenCounters(float x,float z,float y)
    {
        Box("CounterA",new Vector3(x,y+.48f,z),new Vector3(5.0f,.95f,.65f),mats["wall2"]);
        Box("CounterB",new Vector3(x+2.1f,y+.48f,z+2.0f),new Vector3(.65f,.95f,4.0f),mats["wall2"]);
        Box("Fridge",new Vector3(x-2.0f,y+1.05f,z+2.2f),new Vector3(1.0f,2.1f,1.0f),mats["metal"]);
    }

    static void SinkBath(float x,float z,float y)
    {
        Box("Sink",new Vector3(x,y+.85f,z),new Vector3(1.2f,.55f,.75f),mats["wall2"]);
        Box("Tub",new Vector3(x+1.9f,y+.35f,z+1.2f),new Vector3(2.4f,.7f,1.0f),mats["wall2"]);
    }

    static void Bed(float x,float z,float y,float rot)
    {
        var p=new GameObject("Bed").transform;p.SetParent(root);p.position=new Vector3(x,y,z);p.rotation=Quaternion.Euler(0,rot,0);
        Box("frame",p.TransformPoint(new Vector3(0,.32f,0)),new Vector3(2.1f,.35f,3.0f),mats["trim"],p);
        Box("mattress",p.TransformPoint(new Vector3(0,.58f,0)),new Vector3(1.9f,.35f,2.75f),mats["ceil"],p);
        Box("head",p.TransformPoint(new Vector3(0,.95f,1.42f)),new Vector3(2.15f,1.7f,.16f),mats["trim"],p);
    }

    static void Wardrobe(float x,float z,float y,float rot)
    {
        var g=Box("Wardrobe",new Vector3(x,y+1.15f,z),new Vector3(2.2f,2.3f,.75f),mats["trim"]);
        g.transform.rotation=Quaternion.Euler(0,rot,0);
    }
    static void Dresser(float x,float z,float y,float rot)
    {
        var g=Box("Dresser",new Vector3(x,y+.65f,z),new Vector3(1.8f,1.3f,.7f),mats["trim"]);
        g.transform.rotation=Quaternion.Euler(0,rot,0);
    }
    static void Nightstand(float x,float z,float y)
    {
        Box("Nightstand",new Vector3(x,y+.42f,z),new Vector3(.7f,.84f,.65f),mats["trim"]);
    }
    static void Crib(float x,float z,float y)
    {
        Box("CribBase",new Vector3(x,y+.32f,z),new Vector3(1.2f,.25f,2.0f),mats["ceil"]);
        Box("CribHeadA",new Vector3(x,y+.85f,z-1.0f),new Vector3(1.35f,1.25f,.12f),mats["trim"]);
        Box("CribHeadB",new Vector3(x,y+.85f,z+1.0f),new Vector3(1.35f,1.25f,.12f),mats["trim"]);
        for(int i=-3;i<=3;i++) Box("CribRail",new Vector3(x-.62f,y+.82f,z+i*.28f),new Vector3(.08f,1.0f,.08f),mats["trim"]);
    }
    static void RockingChair(float x,float z,float y)
    {
        Box("RockingChair",new Vector3(x,y+.55f,z),new Vector3(.7f,1.1f,.8f),mats["trim"]);
    }

    static void Stairs(float x,float z,float y,int steps,float width,float rise)
    {
        float d=(StairEnd-StairStart)/steps;
        for(int i=0;i<steps;i++){
            float h=(i+1)*(rise/steps);
            Box("Stair_"+i,new Vector3(x,y+h*.5f,z+(i+.5f)*d),new Vector3(width,h,d),mats["floor"]);
        }
    }

    static void CreateExteriorDetails()
    {
        // Porch
        Floor("Porch",0,-13.4f,7.5f,2.6f,0,mats["tile"]);
        for(float x=-3;x<=3;x+=3)
            Box("PorchPost",new Vector3(x,1.55f,-14.2f),new Vector3(.25f,3.1f,.25f),mats["trim"]);

        // Simple front steps
        Box("FrontStep1",new Vector3(0,-.15f,-15.0f),new Vector3(5.0f,.30f,1.0f),mats["tile"]);
        Box("FrontStep2",new Vector3(0,-.30f,-15.8f),new Vector3(6.0f,.30f,1.0f),mats["tile"]);

        // Outer terrain slab
        Box("Ground",new Vector3(0,-.55f,0),new Vector3(44,.8f,44),mats["wall2"]);
    }

    static void CreateLighting()
    {
        // Directional moon fill.
        var moon=new GameObject("MoonLight");
        moon.transform.SetParent(root);
        var dl=moon.AddComponent<Light>();
        dl.type=LightType.Directional;
        dl.intensity=.25f;
        dl.color=new Color(.55f,.63f,.78f);
        moon.transform.rotation=Quaternion.Euler(48,-35,0);

        // Warm dim point lights through corridors.
        AddPoint("GF_FoyerLight",new Vector3(0,2.55f,-9.7f),2.3f,7.5f,new Color(1f,.74f,.45f));
        AddPoint("GF_HallLight",new Vector3(0,2.55f,-1.0f),1.8f,6.5f,new Color(1f,.72f,.42f));
        AddPoint("GF_RearLight",new Vector3(0,2.55f,8.0f),1.5f,6.0f,new Color(.78f,.82f,1f));
        AddPoint("UF_HallLight",new Vector3(0,6.15f,-1.0f),1.4f,6.0f,new Color(1f,.70f,.40f));
        AddPoint("UF_NurseryLight",new Vector3(7.0f,6.0f,1.0f),1.2f,5.0f,new Color(1f,.66f,.42f));
    }

    static void AddPoint(string name, Vector3 pos, float intensity, float range, Color color)
    {
        var g=new GameObject(name);g.transform.SetParent(root);g.transform.position=pos;
        var l=g.AddComponent<Light>();l.type=LightType.Point;l.intensity=intensity;l.range=range;l.color=color;
    }
}
#endif
