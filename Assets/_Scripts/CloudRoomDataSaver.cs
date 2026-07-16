using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public static class CloudRoomDataSaver
{
    public static void SaveRoomData(RoomData room)
    {
        if (room == null || string.IsNullOrEmpty(room.roomId)) return;

        DatabaseReference dbRef = FirebaseManager.Instance.DatabaseRoot;

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "roomId", room.roomId },
            { "ownerId", room.ownerId },
            { "roomTemplateName", room.roomTemplateName },
            { "updatedAt", ServerValue.Timestamp }
        };

        if (room.customRoom != null)
        {
            data["customRoom/width"] = room.customRoom.width;
            data["customRoom/height"] = room.customRoom.height;
            data["customRoom/depth"] = room.customRoom.depth;

            data["customRoom/wallColorR"] = room.customRoom.wallColorR;
            data["customRoom/wallColorG"] = room.customRoom.wallColorG;
            data["customRoom/wallColorB"] = room.customRoom.wallColorB;

            data["customRoom/floorColorR"] = room.customRoom.floorColorR;
            data["customRoom/floorColorG"] = room.customRoom.floorColorG;
            data["customRoom/floorColorB"] = room.customRoom.floorColorB;

            data["customRoom/ceilingColorR"] = room.customRoom.ceilingColorR;
            data["customRoom/ceilingColorG"] = room.customRoom.ceilingColorG;
            data["customRoom/ceilingColorB"] = room.customRoom.ceilingColorB;

            data["customRoom/wallTextureName"] = room.customRoom.wallTextureName;
            data["customRoom/floorTextureName"] = room.customRoom.floorTextureName;
            data["customRoom/ceilingTextureName"] = room.customRoom.ceilingTextureName;
            data["customRoom/showCeiling"] = room.customRoom.showCeiling;
        }

        dbRef.Child("roomData").Child(room.roomId).UpdateChildrenAsync(data)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                    Debug.LogError("[CLOUD ROOM DATA] Save failed: " + task.Exception);
                else
                    Debug.Log("[CLOUD ROOM DATA] Saved room data: " + room.roomId);
            });
    }
}