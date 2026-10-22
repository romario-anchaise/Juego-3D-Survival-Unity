#if UNITY_EDITOR
using UnityEngine;
using UnityEngine.SceneManagement;

// Reuses the kit's preview-scene exporter, materials and primitive colliders.
public partial class FearMansionStaticMapGenerator
{
    static void DetailBox(string name, float x, float y, float z, float w, float h, float d, string material)
    {
        Box(name, new Vector3(x,y,z), new Vector3(w,h,d), mats[material]);
    }

    static void CreateDetailedPieces(Scene preview)
    {
        foreach (string folder in new[] { "Decoracion", "Iluminacion", "Cocina", "Bano" })
            EnsureFolder(LibraryPath + "/Prefabs/" + folder);
        mats["cream"] = Mat("FM_Crema", new Color(.72f,.65f,.49f),0);
        mats["brass"] = Mat("FM_Laton", new Color(.52f,.35f,.12f),.65f);
        mats["blue"] = Mat("FM_Azul", new Color(.09f,.19f,.27f),0);
        mats["paper"] = Mat("FM_Papel", new Color(.8f,.76f,.64f),0);
        mats["leaf"] = Mat("FM_Hojas", new Color(.12f,.28f,.13f),0);
        mats["ceramic"] = Mat("FM_Ceramica", new Color(.75f,.76f,.71f),.05f);

        SavePiece("Decoracion/Cuadro_Paisaje", () => Picture(false), preview);
        SavePiece("Decoracion/Cuadro_Abstracto", () => Picture(true), preview);
        SavePiece("Decoracion/Espejo_Enmarcado", Mirror, preview);
        SavePiece("Decoracion/Alfombra_Roja", () => Rug("cloth"), preview);
        SavePiece("Decoracion/Alfombra_Azul", () => Rug("blue"), preview);
        SavePiece("Decoracion/Cortinas_Dobles", Curtains, preview);
        SavePiece("Decoracion/Reloj_Pared", Clock, preview);
        SavePiece("Decoracion/Libros_Fila", () => Books(0,0,0), preview);
        SavePiece("Decoracion/Libros_Apilados", BookStack, preview);
        SavePiece("Decoracion/Maceta_Planta", Plant, preview);
        SavePiece("Decoracion/Jarron", Vase, preview);
        SavePiece("Decoracion/Caja_Madera", Crate, preview);
        SavePiece("Decoracion/Maleta", Suitcase, preview);
        SavePiece("Decoracion/Candelabro", Candelabra, preview);
        SavePiece("Iluminacion/Lampara_Mesa", () => Lamp(.65f,false), preview);
        SavePiece("Iluminacion/Lampara_Pie", () => Lamp(1.65f,false), preview);
        SavePiece("Iluminacion/Lampara_Techo", () => Lamp(.6f,true), preview);
        SavePiece("Iluminacion/Aplique_Pared", Sconce, preview);
        SavePiece("Muebles/Silla_Con_Respaldo", () => DetailedChair(false), preview);
        SavePiece("Muebles/Silla_Con_Brazos", () => DetailedChair(true), preview);
        SavePiece("Muebles/Sillon_Individual", Armchair, preview);
        SavePiece("Muebles/Banco_Madera", Bench, preview);
        SavePiece("Muebles/Mesa_Centro", CoffeeTable, preview);
        SavePiece("Muebles/Consola_Entrada", ConsoleTable, preview);
        SavePiece("Muebles/Comoda_Con_Cajones", Drawers, preview);
        SavePiece("Muebles/Armario_Dos_Puertas", DetailedWardrobe, preview);
        SavePiece("Muebles/Estanteria_Con_Libros", Bookcase, preview);
        SavePiece("Muebles/Repisa_Pared", WallShelf, preview);
        SavePiece("Muebles/Perchero", CoatRack, preview);
        SavePiece("Muebles/Baul", Trunk, preview);
        SavePiece("Cocina/Mueble_Bajo", () => Cabinet(false), preview);
        SavePiece("Cocina/Mueble_Alto", () => Cabinet(true), preview);
        SavePiece("Cocina/Isla_Cocina", KitchenIsland, preview);
        SavePiece("Cocina/Lavaplatos", KitchenSink, preview);
        SavePiece("Cocina/Estufa_Con_Horno", Stove, preview);
        SavePiece("Cocina/Nevera", Fridge, preview);
        SavePiece("Cocina/Campana_Extractora", Hood, preview);
        SavePiece("Cocina/Tabla_Cortar", CuttingBoard, preview);
        SavePiece("Bano/Lavamanos", Washbasin, preview);
        SavePiece("Bano/Inodoro", Toilet, preview);
        SavePiece("Bano/Banera", Bathtub, preview);
        SavePiece("Bano/Ducha", Shower, preview);
        SavePiece("Bano/Toallero", TowelRack, preview);
        SavePiece("Bano/Botiquin", MedicineCabinet, preview);
        SavePiece("Construccion/Zocalo_2m", () => DetailBox("Zocalo",0,.1f,0,2,.2f,.06f,"trim"), preview);
        SavePiece("Construccion/Cornisa_2m", () => DetailBox("Cornisa",0,-.07f,0,2,.14f,.18f,"cream"), preview);
        SavePiece("Construccion/Viga_4m", () => DetailBox("Viga",0,0,0,4,.25f,.25f,"trim"), preview);
        SavePiece("Construccion/Columna_Con_Base", Column, preview);
        SavePiece("Construccion/Baranda_2m", Railing, preview);
        SavePiece("Construccion/Pared_Con_Ventana", WindowWall, preview);
        SavePiece("Exterior/Cerca_2m", Fence, preview);
        SavePiece("Exterior/Jardinera", Planter, preview);
    }

    // Wall decorations have their back at Z=0 and project toward negative Z.
    static void Frame(float w,float h,string material)
    {
        DetailBox("Marco_I",-w/2+.04f,h/2,-.055f,.08f,h,.11f,material);
        DetailBox("Marco_D",w/2-.04f,h/2,-.055f,.08f,h,.11f,material);
        DetailBox("Marco_S",0,h-.04f,-.055f,w,.08f,.11f,material);
        DetailBox("Marco_B",0,.04f,-.055f,w,.08f,.11f,material);
    }
    static void Picture(bool abstractArt)
    {
        Frame(1.2f,.9f,"brass");
        DetailBox("Lienzo",0,.45f,-.035f,1.06f,.76f,.03f,abstractArt?"cream":"blue");
        if(abstractArt) {
            DetailBox("Forma_Roja",-.22f,.43f,-.06f,.24f,.55f,.018f,"cloth");
            DetailBox("Forma_Azul",.18f,.57f,-.065f,.39f,.27f,.018f,"blue");
            DetailBox("Forma_Oro",.08f,.25f,-.07f,.55f,.08f,.018f,"brass");
        } else {
            DetailBox("Horizonte",0,.25f,-.06f,1.02f,.3f,.02f,"leaf");
            DetailBox("Luna",.3f,.65f,-.06f,.14f,.14f,.02f,"paper");
            DetailBox("Tronco",-.25f,.4f,-.08f,.07f,.45f,.02f,"trim");
            DetailBox("Copa",-.25f,.6f,-.09f,.32f,.22f,.02f,"leaf");
        }
    }
    static void Mirror() { Frame(.8f,1.25f,"trim"); DetailBox("Cristal_Decorativo",0,.625f,-.04f,.65f,1.1f,.03f,"glass"); }
    static void Rug(string color) {
        DetailBox("Borde",0,.012f,0,2.4f,.024f,1.6f,"cream");
        DetailBox("Tejido",0,.026f,0,2.2f,.012f,1.4f,color);
        for(int i=-4;i<=4;i++) DetailBox("Patron",i*.22f,.035f,0,.04f,.006f,1.15f,"brass");
    }
    static void Curtains() {
        DetailBox("Barra",0,2.3f,0,2.5f,.06f,.06f,"brass");
        for(int side=-1;side<=1;side+=2)
            for(int i=0;i<5;i++) DetailBox("Pliegue",side*(.7f+i*.1f),1.15f,-.06f,.12f,2.2f,i%2==0?.15f:.1f,"cloth");
    }
    static void Clock() {
        Frame(.55f,.65f,"trim"); DetailBox("Esfera",0,.325f,-.04f,.4f,.5f,.025f,"paper");
        DetailBox("Aguja_Hora",-.06f,.33f,-.065f,.14f,.018f,.02f,"metal");
        DetailBox("Aguja_Minuto",0,.43f,-.065f,.018f,.22f,.02f,"metal");
        DetailBox("Marca_12",0,.53f,-.065f,.025f,.05f,.02f,"metal");
    }
    static void Books(float x,float y,float z) {
        string[] colors={"cloth","blue","leaf","cream","door","blue"};
        for(int i=0;i<6;i++) {
            float h=.25f+(i%3)*.035f;
            DetailBox("Libro_"+i,x-.25f+i*.1f,y+h/2,z,.085f,h,.2f,colors[i]);
            DetailBox("Lomo",x-.25f+i*.1f,y+h*.7f,z-.104f,.06f,.015f,.01f,"brass");
        }
    }
    static void BookStack() {
        for(int i=0;i<4;i++) {
            DetailBox("Tapa",(i%2)*.025f,.035f+i*.085f,0,.32f,.065f,.23f,i%2==0?"cloth":"blue");
            DetailBox("Paginas",(i%2)*.025f,.035f+i*.085f,-.009f,.3f,.045f,.22f,"paper");
        }
    }
    static void Vase() {
        DetailBox("Base",0,.04f,0,.2f,.08f,.2f,"ceramic");
        DetailBox("Cuerpo",0,.2f,0,.28f,.26f,.28f,"ceramic");
        DetailBox("Cuello",0,.37f,0,.14f,.12f,.14f,"ceramic");
        DetailBox("Boca",0,.44f,0,.19f,.04f,.19f,"brass");
    }
    static void Plant() {
        DetailBox("Maceta",0,.19f,0,.4f,.38f,.4f,"door");
        DetailBox("Tierra",0,.385f,0,.34f,.02f,.34f,"trim");
        DetailBox("Tallo",0,.72f,0,.045f,.65f,.045f,"trim");
        for(int i=0;i<5;i++) DetailBox("Hojas",i%2==0?-.12f:.12f,.65f+i*.11f,(i%3-1)*.07f,.3f,.13f,.22f,"leaf");
    }
    static void Crate() {
        DetailBox("Caja",0,.3f,0,.65f,.6f,.65f,"door");
        foreach(float x in new[]{-.23f,.23f}) {
            DetailBox("Refuerzo_Frente",x,.3f,-.34f,.08f,.6f,.04f,"trim");
            DetailBox("Refuerzo_Atras",x,.3f,.34f,.08f,.6f,.04f,"trim");
        }
    }
    static void Suitcase() {
        DetailBox("Maleta",0,.22f,0,.7f,.44f,.25f,"door");
        foreach(float x in new[]{-.23f,.23f}) DetailBox("Correa",x,.22f,-.135f,.045f,.44f,.02f,"trim");
        DetailBox("Asa",0,.49f,0,.2f,.06f,.05f,"brass");
    }
    static void Candelabra() {
        DetailBox("Base",0,.025f,0,.3f,.05f,.2f,"brass");
        DetailBox("Soporte",0,.2f,0,.045f,.35f,.045f,"brass");
        DetailBox("Brazos",0,.36f,0,.5f,.045f,.05f,"brass");
        for(int i=-1;i<=1;i++) DetailBox("Vela",i*.22f,.48f,0,.055f,.22f,.055f,"paper");
    }
    static void Lamp(float height,bool hanging) {
        if(hanging) {
            DetailBox("Roseta",0,-.025f,0,.2f,.05f,.2f,"brass");
            DetailBox("Cable",0,-height*.4f,0,.025f,height*.8f,.025f,"metal");
            DetailBox("Pantalla",0,-height,0,.5f,.25f,.5f,"cream");
        } else {
            DetailBox("Base",0,.025f,0,.3f,.05f,.3f,"brass");
            DetailBox("Pie",0,height*.45f,0,.045f,height*.85f,.045f,"brass");
            DetailBox("Pantalla",0,height-.14f,0,.45f,.28f,.45f,"cream");
        }
        // Small range and no shadows keep repeated lamps inexpensive.
        AddPoint("Luz_Calida",new Vector3(0,hanging?-height-.18f:height-.3f,0),1.2f,3f,new Color(1f,.78f,.5f));
    }
    static void Sconce() {
        DetailBox("Placa",0,.22f,-.035f,.16f,.44f,.07f,"brass");
        DetailBox("Brazo",0,.15f,-.18f,.04f,.04f,.3f,"brass");
        DetailBox("Pantalla",0,.32f,-.3f,.25f,.3f,.25f,"cream");
        AddPoint("Luz_Calida",new Vector3(0,.2f,-.45f),1.1f,2.5f,new Color(1,.78f,.5f));
    }
    static void Legs(float w,float depth,float height,string material) {
        foreach(float x in new[]{-w/2+.07f,w/2-.07f})
            foreach(float z in new[]{-depth/2+.07f,depth/2-.07f})
                DetailBox("Pata",x,height/2,z,.08f,height,.08f,material);
    }
    static void DetailedChair(bool arms) {
        Legs(.52f,.52f,.45f,"trim");
        DetailBox("Asiento",0,.47f,0,.56f,.1f,.56f,"cloth");
        DetailBox("Respaldo",0,.79f,.24f,.56f,.55f,.1f,"trim");
        DetailBox("Tapizado",0,.79f,.18f,.43f,.4f,.04f,"cloth");
        if(arms) foreach(float x in new[]{-.3f,.3f}) {
            DetailBox("Apoyabrazos",x,.7f,0,.08f,.08f,.58f,"trim");
            DetailBox("Soporte",x,.59f,-.2f,.06f,.22f,.06f,"trim");
        }
    }
    static void Armchair() {
        Legs(.85f,.85f,.18f,"trim");
        DetailBox("Asiento",0,.35f,0,.9f,.35f,.85f,"cloth");
        DetailBox("Respaldo",0,.75f,.34f,.95f,.65f,.2f,"cloth");
        foreach(float x in new[]{-.46f,.46f}) DetailBox("Brazo",x,.55f,0,.18f,.45f,.9f,"cloth");
        DetailBox("Cojin",0,.56f,-.06f,.65f,.12f,.58f,"cream");
    }
    static void Bench() { Legs(1.5f,.48f,.43f,"trim"); DetailBox("Asiento",0,.46f,0,1.6f,.08f,.5f,"door"); }
    static void CoffeeTable() { Legs(1.2f,.7f,.4f,"trim"); DetailBox("Tablero",0,.44f,0,1.3f,.08f,.8f,"door"); DetailBox("Repisa",0,.16f,0,1.12f,.06f,.6f,"trim"); }
    static void ConsoleTable() { Legs(1.4f,.4f,.8f,"trim"); DetailBox("Tablero",0,.84f,0,1.5f,.08f,.5f,"door"); DetailBox("Cajon",0,.69f,0,1.3f,.2f,.37f,"door"); DetailBox("Tirador",0,.69f,-.21f,.2f,.04f,.04f,"brass"); }
    static void Drawers() {
        Legs(1.2f,.5f,.14f,"trim"); DetailBox("Cuerpo",0,.65f,0,1.2f,1.05f,.5f,"door");
        DetailBox("Tapa",0,1.2f,0,1.3f,.08f,.56f,"trim");
        for(int i=0;i<3;i++) {
            DetailBox("Frente_Cajon",0,.32f+i*.31f,-.27f,1.08f,.27f,.04f,"trim");
            foreach(float x in new[]{-.3f,.3f}) DetailBox("Tirador",x,.32f+i*.31f,-.31f,.1f,.04f,.05f,"brass");
        }
    }
    static void DetailedWardrobe() {
        DetailBox("Cuerpo",0,1.05f,0,1.5f,2.1f,.6f,"trim");
        foreach(float x in new[]{-.37f,.37f}) {
            DetailBox("Puerta",x,1.08f,-.32f,.7f,1.94f,.06f,"door");
            DetailBox("Panel",x,1.08f,-.36f,.52f,1.65f,.03f,"trim");
            DetailBox("Tirador",x<0?-.09f:.09f,1.03f,-.4f,.035f,.18f,.06f,"brass");
        }
        DetailBox("Cornisa",0,2.15f,0,1.65f,.1f,.7f,"door");
    }
    static void Bookcase() {
        foreach(float x in new[]{-.58f,.58f}) DetailBox("Lateral",x,1,0,.08f,2,.34f,"trim");
        DetailBox("Fondo",0,1,.15f,1.2f,2,.04f,"door");
        for(int i=0;i<5;i++) {
            DetailBox("Balda",0,.04f+i*.48f,0,1.2f,.08f,.35f,"trim");
            if(i<4) Books(i%2==0?-.2f:.15f,.08f+i*.48f,-.01f);
        }
    }
    static void WallShelf() { DetailBox("Repisa",0,.04f,-.15f,1.2f,.08f,.3f,"trim"); foreach(float x in new[]{-.4f,.4f}) DetailBox("Soporte",x,-.1f,-.1f,.06f,.2f,.15f,"brass"); }
    static void CoatRack() {
        DetailBox("Pie_X",0,.04f,0,.6f,.08f,.1f,"trim"); DetailBox("Pie_Z",0,.04f,0,.1f,.08f,.6f,"trim");
        DetailBox("Poste",0,.95f,0,.08f,1.9f,.08f,"door");
        DetailBox("Perchas_X",0,1.7f,0,.65f,.06f,.06f,"brass"); DetailBox("Perchas_Z",0,1.6f,0,.06f,.06f,.65f,"brass");
    }
    static void Trunk() {
        DetailBox("Baul",0,.28f,0,1,.56f,.55f,"door"); DetailBox("Tapa",0,.59f,0,1.04f,.08f,.59f,"trim");
        foreach(float x in new[]{-.33f,.33f}) DetailBox("Herraje",x,.31f,-.29f,.06f,.56f,.035f,"brass");
        DetailBox("Cerradura",0,.5f,-.32f,.09f,.12f,.04f,"brass");
    }
    static void Cabinet(bool upper) {
        float h=upper?.7f:.9f;
        DetailBox("Cuerpo",0,h/2,0,1,h,.55f,"door");
        foreach(float x in new[]{-.245f,.245f}) {
            DetailBox("Puerta",x,h/2,-.3f,.46f,h-.12f,.05f,"cream");
            DetailBox("Tirador",x<0?-.06f:.06f,h*.65f,-.345f,.025f,.14f,.04f,"brass");
        }
        if(!upper) DetailBox("Encimera",0,.94f,0,1.06f,.08f,.61f,"tile");
    }
    static void KitchenIsland() { DetailBox("Base",0,.45f,0,1.8f,.9f,.8f,"door"); DetailBox("Encimera",0,.94f,0,2,.08f,1,"tile"); for(int i=-1;i<=1;i++) DetailBox("Panel",i*.56f,.48f,-.42f,.5f,.7f,.04f,"cream"); }
    static void Basin(float y,float width,float depth) {
        DetailBox("Fondo",0,y-.12f,0,width,.06f,depth,"metal");
        foreach(float x in new[]{-width/2,width/2}) DetailBox("Borde",x,y-.04f,0,.045f,.16f,depth,"ceramic");
        foreach(float z in new[]{-depth/2,depth/2}) DetailBox("Borde",0,y-.04f,z,width,.16f,.045f,"ceramic");
        DetailBox("Grifo",0,y+.15f,depth/2+.05f,.045f,.3f,.045f,"metal");
        DetailBox("Salida",0,y+.28f,depth/2-.03f,.045f,.045f,.2f,"metal");
    }
    static void KitchenSink() {
        DetailBox("Mueble",0,.42f,0,1,.84f,.6f,"door");
        foreach(float x in new[]{-.39f,.39f}) DetailBox("Encimera_Lateral",x,.9f,0,.22f,.08f,.65f,"tile");
        Basin(.94f,.55f,.4f);
    }
    static void Stove() {
        DetailBox("Cuerpo",0,.45f,0,.75f,.9f,.65f,"ceramic");
        DetailBox("Puerta_Horno",0,.4f,-.34f,.61f,.48f,.04f,"metal");
        DetailBox("Cristal",0,.4f,-.37f,.46f,.29f,.02f,"glass");
        DetailBox("Asa",0,.65f,-.4f,.48f,.04f,.06f,"metal");
        foreach(float x in new[]{-.2f,.2f}) foreach(float z in new[]{-.17f,.17f}) DetailBox("Quemador",x,.915f,z,.19f,.03f,.19f,"metal");
        for(int i=0;i<4;i++) DetailBox("Mando",-.24f+i*.16f,.79f,-.35f,.055f,.055f,.04f,"brass");
    }
    static void Fridge() {
        DetailBox("Cuerpo",0,.95f,0,.85f,1.9f,.75f,"ceramic");
        DetailBox("Congelador",0,1.57f,-.4f,.79f,.55f,.06f,"cream");
        DetailBox("Puerta",0,.65f,-.4f,.79f,1.2f,.06f,"cream");
        DetailBox("Asa_Alta",.29f,1.43f,-.46f,.035f,.25f,.06f,"metal");
        DetailBox("Asa_Baja",.29f,1,-.46f,.035f,.35f,.06f,"metal");
    }
    static void Hood() { DetailBox("Campana",0,.1f,0,.9f,.2f,.6f,"metal"); DetailBox("Conducto",0,.5f,.12f,.35f,.6f,.3f,"metal"); }
    static void CuttingBoard() { DetailBox("Tabla",0,.015f,0,.4f,.03f,.27f,"door"); DetailBox("Mango",.24f,.015f,0,.13f,.03f,.08f,"door"); }
    static void Washbasin() { DetailBox("Pedestal",0,.38f,0,.22f,.76f,.24f,"ceramic"); Basin(.86f,.58f,.4f); }
    static void Toilet() {
        DetailBox("Base",0,.15f,0,.3f,.3f,.4f,"ceramic");
        DetailBox("Taza",0,.34f,-.08f,.43f,.24f,.58f,"ceramic");
        DetailBox("Asiento",0,.48f,-.1f,.46f,.05f,.58f,"cream");
        DetailBox("Tanque",0,.66f,.22f,.46f,.5f,.2f,"ceramic");
        DetailBox("Tapa_Tanque",0,.93f,.22f,.5f,.05f,.24f,"ceramic");
        DetailBox("Descarga",.16f,.85f,.105f,.06f,.035f,.035f,"metal");
    }
    static void Bathtub() {
        DetailBox("Fondo",0,.15f,0,1.75f,.12f,.7f,"ceramic");
        foreach(float x in new[]{-.88f,.88f}) DetailBox("Lateral",x,.35f,0,.12f,.5f,.82f,"ceramic");
        foreach(float z in new[]{-.36f,.36f}) DetailBox("Lateral",0,.35f,z,1.75f,.5f,.12f,"ceramic");
        DetailBox("Grifo",.65f,.72f,.28f,.05f,.25f,.05f,"metal");
        DetailBox("Salida",.65f,.82f,.2f,.05f,.05f,.2f,"metal");
    }
    static void Shower() {
        DetailBox("Plato",0,.06f,0,1,.12f,1,"ceramic");
        DetailBox("Panel_Fondo",0,1.1f,.48f,1,2.1f,.06f,"tile");
        DetailBox("Tubo",0,1.25f,.41f,.035f,1.4f,.035f,"metal");
        DetailBox("Brazo",0,1.94f,.26f,.035f,.035f,.3f,"metal");
        DetailBox("Cabezal",0,1.92f,.1f,.22f,.04f,.22f,"metal");
        DetailBox("Mando",0,1,.36f,.12f,.12f,.06f,"metal");
    }
    static void TowelRack() {
        foreach(float x in new[]{-.32f,.32f}) DetailBox("Soporte",x,.5f,-.08f,.04f,.08f,.16f,"metal");
        DetailBox("Barra",0,.5f,-.16f,.68f,.035f,.035f,"metal");
        DetailBox("Toalla",0,.28f,-.18f,.45f,.45f,.035f,"cream");
    }
    static void MedicineCabinet() { DetailBox("Mueble",0,.35f,-.1f,.65f,.7f,.2f,"ceramic"); Frame(.65f,.7f,"brass"); DetailBox("Espejo_Decorativo",0,.35f,-.22f,.5f,.55f,.03f,"glass"); }
    static void Column() { DetailBox("Fuste",0,1.6f,0,.32f,3.2f,.32f,"wall2"); DetailBox("Base",0,.12f,0,.5f,.24f,.5f,"cream"); DetailBox("Capitel",0,3.08f,0,.5f,.24f,.5f,"cream"); }
    static void Railing() {
        foreach(float x in new[]{-.96f,.96f}) DetailBox("Poste",x,.55f,0,.08f,1.1f,.08f,"trim");
        DetailBox("Pasamanos",0,1.1f,0,2.08f,.09f,.12f,"door");
        for(int i=-4;i<=4;i++) DetailBox("Barrote",i*.2f,.55f,0,.04f,1,.04f,"trim");
    }
    static void WindowWall() {
        DetailBox("Pared_I",-1.55f,1.6f,0,.9f,3.2f,.18f,"wall");
        DetailBox("Pared_D",1.55f,1.6f,0,.9f,3.2f,.18f,"wall");
        DetailBox("Antepecho",0,.45f,0,2.2f,.9f,.18f,"wall");
        DetailBox("Dintel",0,2.8f,0,2.2f,.8f,.18f,"wall");
        DetailBox("Marco_I",-1.06f,1.65f,0,.08f,1.5f,.25f,"trim");
        DetailBox("Marco_D",1.06f,1.65f,0,.08f,1.5f,.25f,"trim");
        DetailBox("Marco_S",0,2.36f,0,2.2f,.08f,.25f,"trim");
        DetailBox("Marco_B",0,.94f,0,2.2f,.08f,.25f,"trim");
        DetailBox("Travesano",0,1.65f,0,.06f,1.5f,.2f,"trim");
        DetailBox("Cristal_Decorativo",0,1.65f,.015f,2.04f,1.34f,.025f,"glass");
    }
    static void Fence() {
        for(int i=-4;i<=4;i++) DetailBox("Tabla",i*.23f,.6f,0,.14f,1.2f,.06f,"door");
        foreach(float y in new[]{.3f,.9f}) DetailBox("Travesano",0,y,.06f,2,.1f,.06f,"trim");
    }
    static void Planter() {
        DetailBox("Base",0,.08f,0,1.4f,.16f,.5f,"door");
        foreach(float x in new[]{-.66f,.66f}) DetailBox("Lateral",x,.25f,0,.08f,.5f,.5f,"door");
        foreach(float z in new[]{-.22f,.22f}) DetailBox("Lateral",0,.25f,z,1.4f,.5f,.06f,"door");
        DetailBox("Tierra",0,.42f,0,1.2f,.05f,.35f,"trim");
        for(int i=-2;i<=2;i++) DetailBox("Planta",i*.23f,.62f,0,.18f,.35f,.23f,"leaf");
    }
}
#endif
