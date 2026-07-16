

//using System.Collections.Generic;
//using Firebase.Database;
//using Firebase.Extensions;
//using UnityEngine;

//public static class CloudRoomSummarySaver
//{
//    public static void UpdateRoomSummary(RoomData room)
//    {
//        if (room == null)
//        {
//            Debug.LogWarning("[CLOUD SUMMARY] Room is null");
//            return;
//        }

//        if (FirebaseManager.Instance == null || FirebaseManager.Instance.DatabaseRoot == null)
//        {
//            Debug.LogWarning("[CLOUD SUMMARY] Firebase not ready");
//            return;
//        }

//        int objectCount = room.objects != null ? room.objects.Count : 0;
//        int shelfCount = room.shelves != null ? room.shelves.Count : 0;

//        Dictionary<string, object> updates = new Dictionary<string, object>
//        {
//            { "objectCount", objectCount },
//            { "shelfCount", shelfCount },
//            { "roomTemplateName", room.roomTemplateName },
//            { "updatedAt", ServerValue.Timestamp }
//        };

//        if (room.customRoom != null)
//        {
//            updates["customRoom/width"] = room.customRoom.width;
//            updates["customRoom/height"] = room.customRoom.height;
//            updates["customRoom/depth"] = room.customRoom.depth;

//            updates["customRoom/wallColorR"] = room.customRoom.wallColorR;
//            updates["customRoom/wallColorG"] = room.customRoom.wallColorG;
//            updates["customRoom/wallColorB"] = room.customRoom.wallColorB;

//            updates["customRoom/floorColorR"] = room.customRoom.floorColorR;
//            updates["customRoom/floorColorG"] = room.customRoom.floorColorG;
//            updates["customRoom/floorColorB"] = room.customRoom.floorColorB;

//            updates["customRoom/ceilingColorR"] = room.customRoom.ceilingColorR;
//            updates["customRoom/ceilingColorG"] = room.customRoom.ceilingColorG;
//            updates["customRoom/ceilingColorB"] = room.customRoom.ceilingColorB;

//            updates["customRoom/wallTextureName"] = room.customRoom.wallTextureName;
//            updates["customRoom/floorTextureName"] = room.customRoom.floorTextureName;
//            updates["customRoom/ceilingTextureName"] = room.customRoom.ceilingTextureName;

//            updates["customRoom/showCeiling"] = room.customRoom.showCeiling;
//        }

//        Debug.Log($"[CLOUD SUMMARY] Updating {room.roomId} | Objects={objectCount} | Shelves={shelfCount}");

//        Debug.Log("[CLOUD SUMMARY] CustomRoom to Firebase " +
//          "W=" + room.customRoom.width +
//          " H=" + room.customRoom.height +
//          " D=" + room.customRoom.depth +
//          " WallRGB=" + room.customRoom.wallColorR + "," +
//                        room.customRoom.wallColorG + "," +
//                        room.customRoom.wallColorB +
//          " WallTexture=" + room.customRoom.wallTextureName);

//        FirebaseManager.Instance.DatabaseRoot
//            .Child("rooms")
//            .Child(room.roomId)
//            .UpdateChildrenAsync(updates)
//            .ContinueWithOnMainThread(task =>
//            {
//                if (task.IsFaulted || task.IsCanceled)
//                {
//                    Debug.LogError("[CLOUD SUMMARY] Failed: " + task.Exception);
//                    return;
//                }

//                Debug.Log("[CLOUD SUMMARY] Updated successfully");
//            });
//    }
//}



using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public static class CloudRoomSummarySaver
{
    public static void UpdateRoomSummary(RoomData room)
    {
        if (room == null)
        {
            Debug.LogWarning("[CLOUD SUMMARY] Room is null");
            return;
        }

        DatabaseReference dbRef = null;

        if (FirebaseManager.Instance != null && FirebaseManager.Instance.DatabaseRoot != null)
        {
            dbRef = FirebaseManager.Instance.DatabaseRoot;
        }
        else
        {
            Debug.LogWarning("[CLOUD SUMMARY] FirebaseManager not ready, using DefaultInstance fallback.");

            try
            {
                dbRef = FirebaseDatabase.DefaultInstance.RootReference;
            }
            catch (System.Exception e)
            {
                Debug.LogError("[CLOUD SUMMARY] Fallback failed: " + e.Message);
                return;
            }
        }

        int objectCount = room.objects != null ? room.objects.Count : 0;
        int shelfCount = room.shelves != null ? room.shelves.Count : 0;

        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "objectCount", objectCount },
            { "shelfCount", shelfCount },
            { "roomTemplateName", room.roomTemplateName },
            { "updatedAt", ServerValue.Timestamp }
        };

        if (room.customRoom != null)
        {
            updates["customRoom/width"] = room.customRoom.width;
            updates["customRoom/height"] = room.customRoom.height;
            updates["customRoom/depth"] = room.customRoom.depth;

            updates["customRoom/wallColorR"] = room.customRoom.wallColorR;
            updates["customRoom/wallColorG"] = room.customRoom.wallColorG;
            updates["customRoom/wallColorB"] = room.customRoom.wallColorB;

            updates["customRoom/floorColorR"] = room.customRoom.floorColorR;
            updates["customRoom/floorColorG"] = room.customRoom.floorColorG;
            updates["customRoom/floorColorB"] = room.customRoom.floorColorB;

            updates["customRoom/ceilingColorR"] = room.customRoom.ceilingColorR;
            updates["customRoom/ceilingColorG"] = room.customRoom.ceilingColorG;
            updates["customRoom/ceilingColorB"] = room.customRoom.ceilingColorB;

            updates["customRoom/wallTextureName"] = room.customRoom.wallTextureName;
            updates["customRoom/floorTextureName"] = room.customRoom.floorTextureName;
            updates["customRoom/ceilingTextureName"] = room.customRoom.ceilingTextureName;

            updates["customRoom/showCeiling"] = room.customRoom.showCeiling;
        }

        Debug.Log($"[CLOUD SUMMARY] Updating {room.roomId} | Objects={objectCount} | Shelves={shelfCount}");

        Debug.Log("[CLOUD SUMMARY] CustomRoom to Firebase " +
          "W=" + room.customRoom.width +
          " H=" + room.customRoom.height +
          " D=" + room.customRoom.depth +
          " WallRGB=" + room.customRoom.wallColorR + "," +
                        room.customRoom.wallColorG + "," +
                        room.customRoom.wallColorB +
          " WallTexture=" + room.customRoom.wallTextureName);

        dbRef
            .Child("rooms")
            .Child(room.roomId)
            .UpdateChildrenAsync(updates)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[CLOUD SUMMARY] Failed: " + task.Exception);
                    return;
                }

                Debug.Log("[CLOUD SUMMARY] Updated successfully");
            });
    }
}