
//using System;
//using System.Collections.Generic;

//[Serializable]
//public class RoomData
//{
//    public string roomId;
//    public string ownerId;
//    public string roomTemplateName;
//    public List<PlacedObjectData> objects = new List<PlacedObjectData>();
//    public List<PlacedShelfData> shelves = new List<PlacedShelfData>();
//    public ModularRoomConfig customRoom = new ModularRoomConfig();
//}

//[Serializable]
//public class PlacedObjectData
//{
//    public string instanceId = ""; // FIX: unique ID to find object reliably
//    public string fileName;
//    public float posX, posY, posZ;
//    public float rotX, rotY, rotZ, rotW;
//    public float scaleX, scaleY, scaleZ;
//    public string objectName = "My Object";
//    public string description = "";
//    public int objectType = 0;      // 0=ViewOnly 1=ForSale
//    public float price = 0f;
//    public string currency = "RM";
//}

//[Serializable]
//public class PlacedShelfData
//{
//    public float posX, posY, posZ;
//    public float rotX, rotY, rotZ, rotW;
//    public float scaleX, scaleY, scaleZ;
//}

//[Serializable]
//public class ModularRoomConfig
//{
//    public float width = 4f;
//    public float height = 3f;
//    public float depth = 5f;

//    public float colorR = 1f;
//    public float colorG = 1f;
//    public float colorB = 1f;

//    public float wallColorR = 1f;
//    public float wallColorG = 1f;
//    public float wallColorB = 1f;

//    public float floorColorR = 1f;
//    public float floorColorG = 1f;
//    public float floorColorB = 1f;

//    public float ceilingColorR = 1f;
//    public float ceilingColorG = 1f;
//    public float ceilingColorB = 1f;

//    public string wallTextureName = "Plain";
//    public string floorTextureName = "Plain";
//    public string ceilingTextureName = "Plain";

//    public bool showCeiling = true;
//}


using System;
using System.Collections.Generic;

[Serializable]
public class RoomData
{
    public string roomId;
    public string ownerId;
    public string roomTemplateName;
    public List<PlacedObjectData> objects = new List<PlacedObjectData>();
    public List<PlacedShelfData> shelves = new List<PlacedShelfData>();
    public ModularRoomConfig customRoom = new ModularRoomConfig();
}

[Serializable]
public class PlacedObjectData
{
    public string instanceId = "";
    public string fileName;
    public string glbUrl = "";
    public string storagePath = "";

    public float posX, posY, posZ;
    public float rotX, rotY, rotZ, rotW;
    public float scaleX, scaleY, scaleZ;

    public string objectName = "My Object";
    public string description = "";
    public int objectType = 0;
    public float price = 0f;
    public string currency = "RM";

    public bool isBuiltIn = false;
    public string prefabKey = "";
}

[Serializable]
public class PlacedShelfData
{
    public string shelfId;

    public float posX;
    public float posY;
    public float posZ;

    public float rotX;
    public float rotY;
    public float rotZ;
    public float rotW;

    public float scaleX;
    public float scaleY;
    public float scaleZ;
}
[Serializable]
public class ModularRoomConfig
{
    public float width = 4f;
    public float height = 3f;
    public float depth = 5f;

    public float colorR = 1f;
    public float colorG = 1f;
    public float colorB = 1f;

    public float wallColorR = 1f;
    public float wallColorG = 1f;
    public float wallColorB = 1f;

    public float floorColorR = 1f;
    public float floorColorG = 1f;
    public float floorColorB = 1f;

    public float ceilingColorR = 1f;
    public float ceilingColorG = 1f;
    public float ceilingColorB = 1f;

    public string wallTextureName = "Plain";
    public string floorTextureName = "Plain";
    public string ceilingTextureName = "Plain";

    public bool showCeiling = true;
}