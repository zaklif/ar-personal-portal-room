//using System.Collections.Generic;
//using Firebase.Database;
//using Firebase.Extensions;
//using UnityEngine;

//public static class CloudObjectSaver
//{
//    public static void SaveObject(string roomId, PlacedObjectData obj)
//    {
//        Debug.Log("[CLOUD OBJECT] SaveObject called for " + obj.objectName);

//        if (string.IsNullOrEmpty(roomId) || obj == null || string.IsNullOrEmpty(obj.instanceId))
//            return;

//        DatabaseReference dbRef = null;

//        if (FirebaseManager.Instance != null && FirebaseManager.Instance.DatabaseRoot != null)
//            dbRef = FirebaseManager.Instance.DatabaseRoot;
//        else
//            dbRef = FirebaseDatabase.DefaultInstance.RootReference;

//        Dictionary<string, object> data = new Dictionary<string, object>
//        {
//            { "instanceId", obj.instanceId },
//            { "glbUrl", obj.glbUrl },
//            { "storagePath", obj.storagePath },
//            { "fileName", obj.fileName },
//            { "objectName", obj.objectName },
//            { "description", obj.description },
//            { "objectType", obj.objectType },
//            { "isForSale", obj.objectType == 1 },
//            { "price", obj.price },
//            { "currency", "RM" },

//            { "posX", obj.posX },
//            { "posY", obj.posY },
//            { "posZ", obj.posZ },

//            { "rotX", obj.rotX },
//            { "rotY", obj.rotY },
//            { "rotZ", obj.rotZ },
//            { "rotW", obj.rotW },

//            { "scaleX", obj.scaleX },
//            { "scaleY", obj.scaleY },
//            { "scaleZ", obj.scaleZ },

//            { "updatedAt", ServerValue.Timestamp }


//        };

//        Debug.Log("[CLOUD OBJECT] Writing to roomObjects/" + roomId + "/" + obj.instanceId);
//        dbRef.Child("roomObjects")
//            .Child(roomId)
//            .Child(obj.instanceId)
//            .UpdateChildrenAsync(data)
//            .ContinueWithOnMainThread(task =>
//            {
//                if (task.IsFaulted || task.IsCanceled)
//                    Debug.LogError("[CLOUD OBJECT] Save failed: " + task.Exception);
//                else
//                    Debug.Log("[CLOUD OBJECT] Saved object: " + obj.objectName);
//            });
//    }

//    public static void DeleteObject(string roomId, string instanceId)
//    {
//        if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(instanceId))
//            return;

//        DatabaseReference dbRef = FirebaseManager.Instance.DatabaseRoot;

//        dbRef.Child("roomObjects")
//            .Child(roomId)
//            .Child(instanceId)
//            .RemoveValueAsync();
//    }
//}

using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public static class CloudObjectSaver
{
    public static void SaveObject(string roomId, PlacedObjectData obj)
    {
        Debug.Log("[CLOUD OBJECT] SaveObject called for " + obj.objectName);

        if (string.IsNullOrEmpty(roomId) || obj == null || string.IsNullOrEmpty(obj.instanceId))
        {
            Debug.LogWarning("[CLOUD OBJECT] Save skipped. roomId/obj/instanceId missing.");
            return;
        }

        DatabaseReference dbRef = null;

        if (FirebaseManager.Instance != null && FirebaseManager.Instance.DatabaseRoot != null)
            dbRef = FirebaseManager.Instance.DatabaseRoot;
        else
            dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "instanceId", obj.instanceId },

            // GLB / built-in object info
            { "glbUrl", obj.glbUrl },
            { "storagePath", obj.storagePath },
            { "fileName", obj.fileName },

            // NEW: built-in decoration support
            { "isBuiltIn", obj.isBuiltIn },
            { "prefabKey", obj.prefabKey },

            // Metadata
            { "objectName", obj.objectName },
            { "description", obj.description },
            { "objectType", obj.objectType },
            { "isForSale", obj.objectType == 1 },
            { "price", obj.price },
            { "currency", obj.currency },

            // Transform
            { "posX", obj.posX },
            { "posY", obj.posY },
            { "posZ", obj.posZ },

            { "rotX", obj.rotX },
            { "rotY", obj.rotY },
            { "rotZ", obj.rotZ },
            { "rotW", obj.rotW },

            { "scaleX", obj.scaleX },
            { "scaleY", obj.scaleY },
            { "scaleZ", obj.scaleZ },

            { "updatedAt", ServerValue.Timestamp }
        };

        Debug.Log("[CLOUD OBJECT] Writing to roomObjects/" + roomId + "/" + obj.instanceId);

        dbRef.Child("roomObjects")
            .Child(roomId)
            .Child(obj.instanceId)
            .UpdateChildrenAsync(data)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                    Debug.LogError("[CLOUD OBJECT] Save failed: " + task.Exception);
                else
                    Debug.Log("[CLOUD OBJECT] Saved object: " + obj.objectName +
                              " | isBuiltIn=" + obj.isBuiltIn +
                              " | prefabKey=" + obj.prefabKey);
            });
    }

    public static void DeleteObject(string roomId, string instanceId)
    {
        if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(instanceId))
            return;

        DatabaseReference dbRef = null;

        if (FirebaseManager.Instance != null && FirebaseManager.Instance.DatabaseRoot != null)
            dbRef = FirebaseManager.Instance.DatabaseRoot;
        else
            dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        dbRef.Child("roomObjects")
            .Child(roomId)
            .Child(instanceId)
            .RemoveValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                    Debug.LogError("[CLOUD OBJECT] Delete failed: " + task.Exception);
                else
                    Debug.Log("[CLOUD OBJECT] Deleted object id=" + instanceId);
            });
    }
}