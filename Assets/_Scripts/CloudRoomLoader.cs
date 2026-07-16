using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public static class CloudRoomLoader
{
    public static void LoadRoomFromCloud(string roomId, System.Action<RoomData> onLoaded)
    {
        if (string.IsNullOrEmpty(roomId))
        {
            onLoaded?.Invoke(null);
            return;
        }

        DatabaseReference dbRef = FirebaseManager.Instance.DatabaseRoot;

        dbRef.Child("roomData").Child(roomId).GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[CLOUD ROOM LOADER] Failed to load roomData: " + task.Exception);
                    onLoaded?.Invoke(null);
                    return;
                }

                DataSnapshot snapshot = task.Result;

                if (!snapshot.Exists)
                {
                    Debug.LogWarning("[CLOUD ROOM LOADER] No roomData found for: " + roomId);
                    onLoaded?.Invoke(null);
                    return;
                }

                RoomData room = new RoomData();

                room.roomId = snapshot.Child("roomId").Value?.ToString() ?? roomId;
                room.ownerId = snapshot.Child("ownerId").Value?.ToString() ?? "";
                room.roomTemplateName = snapshot.Child("roomTemplateName").Value?.ToString() ?? "";

                room.objects = new List<PlacedObjectData>();
                room.shelves = new List<PlacedShelfData>();
                room.customRoom = ReadCustomRoom(snapshot.Child("customRoom"));

                LoadObjects(room, onLoaded);
            });
    }

    private static void LoadObjects(RoomData room, System.Action<RoomData> onLoaded)
    {
        DatabaseReference dbRef = FirebaseManager.Instance.DatabaseRoot;

        dbRef.Child("roomObjects").Child(room.roomId).GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[CLOUD ROOM LOADER] Failed to load objects: " + task.Exception);
                    onLoaded?.Invoke(room);
                    return;
                }

                DataSnapshot snapshot = task.Result;

                if (snapshot.Exists)
                {
                    foreach (DataSnapshot objSnap in snapshot.Children)
                    {
                        PlacedObjectData obj = ReadObject(objSnap);
                        room.objects.Add(obj);
                    }
                }

                Debug.Log("[CLOUD ROOM LOADER] Loaded objects for room " + room.roomId +
           " | Objects=" + room.objects.Count);

                LoadShelves(room, onLoaded);
            });
    }

    private static ModularRoomConfig ReadCustomRoom(DataSnapshot snap)
    {
        ModularRoomConfig cfg = new ModularRoomConfig();

        if (snap == null || !snap.Exists)
            return cfg;

        cfg.width = GetFloat(snap, "width", cfg.width);
        cfg.height = GetFloat(snap, "height", cfg.height);
        cfg.depth = GetFloat(snap, "depth", cfg.depth);

        cfg.wallColorR = GetFloat(snap, "wallColorR", cfg.wallColorR);
        cfg.wallColorG = GetFloat(snap, "wallColorG", cfg.wallColorG);
        cfg.wallColorB = GetFloat(snap, "wallColorB", cfg.wallColorB);

        cfg.floorColorR = GetFloat(snap, "floorColorR", cfg.floorColorR);
        cfg.floorColorG = GetFloat(snap, "floorColorG", cfg.floorColorG);
        cfg.floorColorB = GetFloat(snap, "floorColorB", cfg.floorColorB);

        cfg.ceilingColorR = GetFloat(snap, "ceilingColorR", cfg.ceilingColorR);
        cfg.ceilingColorG = GetFloat(snap, "ceilingColorG", cfg.ceilingColorG);
        cfg.ceilingColorB = GetFloat(snap, "ceilingColorB", cfg.ceilingColorB);

        cfg.wallTextureName = GetString(snap, "wallTextureName", cfg.wallTextureName);
        cfg.floorTextureName = GetString(snap, "floorTextureName", cfg.floorTextureName);
        cfg.ceilingTextureName = GetString(snap, "ceilingTextureName", cfg.ceilingTextureName);

        cfg.showCeiling = GetBool(snap, "showCeiling", cfg.showCeiling);

        return cfg;
    }

    private static void LoadShelves(RoomData room, System.Action<RoomData> onLoaded)
    {
        DatabaseReference dbRef = FirebaseManager.Instance.DatabaseRoot;

        dbRef.Child("roomShelves").Child(room.roomId).GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[CLOUD ROOM LOADER] Failed to load shelves: " + task.Exception);
                    onLoaded?.Invoke(room);
                    return;
                }

                DataSnapshot snapshot = task.Result;

                room.shelves = new List<PlacedShelfData>();

                if (snapshot.Exists)
                {
                    foreach (DataSnapshot shelfSnap in snapshot.Children)
                    {
                        PlacedShelfData shelf = ReadShelf(shelfSnap);
                        room.shelves.Add(shelf);
                    }
                }

                Debug.Log("[CLOUD ROOM LOADER] Loaded cloud room " + room.roomId +
                          " | Objects=" + room.objects.Count +
                          " | Shelves=" + room.shelves.Count);

                onLoaded?.Invoke(room);
            });
    }

    private static PlacedObjectData ReadObject(DataSnapshot snap)
    {
        PlacedObjectData obj = new PlacedObjectData();

        obj.instanceId = GetString(snap, "instanceId", snap.Key);
        obj.fileName = GetString(snap, "fileName", "");
        obj.glbUrl = GetString(snap, "glbUrl", "");
        obj.storagePath = GetString(snap, "storagePath", "");
        obj.isBuiltIn = GetBool(snap, "isBuiltIn", false);
        obj.prefabKey = GetString(snap, "prefabKey", "");
        obj.objectName = GetString(snap, "objectName", "My Object");
        obj.description = GetString(snap, "description", "");
        obj.objectType = GetInt(snap, "objectType", 0);
        obj.price = GetFloat(snap, "price", 0f);
        obj.currency = GetString(snap, "currency", "RM");

        obj.posX = GetFloat(snap, "posX", 0f);
        obj.posY = GetFloat(snap, "posY", 0f);
        obj.posZ = GetFloat(snap, "posZ", 0f);

        obj.rotX = GetFloat(snap, "rotX", 0f);
        obj.rotY = GetFloat(snap, "rotY", 0f);
        obj.rotZ = GetFloat(snap, "rotZ", 0f);
        obj.rotW = GetFloat(snap, "rotW", 1f);

        obj.scaleX = GetFloat(snap, "scaleX", 1f);
        obj.scaleY = GetFloat(snap, "scaleY", 1f);
        obj.scaleZ = GetFloat(snap, "scaleZ", 1f);

        return obj;
    }

    private static PlacedShelfData ReadShelf(DataSnapshot snap)
    {
        PlacedShelfData shelf = new PlacedShelfData();

        shelf.shelfId = GetString(snap, "shelfId", snap.Key);

        shelf.posX = GetFloat(snap, "posX", 0f);
        shelf.posY = GetFloat(snap, "posY", 0f);
        shelf.posZ = GetFloat(snap, "posZ", 0f);

        shelf.rotX = GetFloat(snap, "rotX", 0f);
        shelf.rotY = GetFloat(snap, "rotY", 0f);
        shelf.rotZ = GetFloat(snap, "rotZ", 0f);
        shelf.rotW = GetFloat(snap, "rotW", 1f);

        shelf.scaleX = GetFloat(snap, "scaleX", 1f);
        shelf.scaleY = GetFloat(snap, "scaleY", 1f);
        shelf.scaleZ = GetFloat(snap, "scaleZ", 1f);

        return shelf;
    }

    private static string GetString(DataSnapshot snap, string key, string fallback)
    {
        return snap.Child(key).Exists ? snap.Child(key).Value.ToString() : fallback;
    }

    private static float GetFloat(DataSnapshot snap, string key, float fallback)
    {
        if (!snap.Child(key).Exists) return fallback;
        return float.TryParse(snap.Child(key).Value.ToString(), out float v) ? v : fallback;
    }

    private static int GetInt(DataSnapshot snap, string key, int fallback)
    {
        if (!snap.Child(key).Exists) return fallback;
        return int.TryParse(snap.Child(key).Value.ToString(), out int v) ? v : fallback;
    }

    private static bool GetBool(DataSnapshot snap, string key, bool fallback)
    {
        if (!snap.Child(key).Exists) return fallback;
        return bool.TryParse(snap.Child(key).Value.ToString(), out bool v) ? v : fallback;
    }
}