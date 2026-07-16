using UnityEngine;
using System.IO;
using System.Collections.Generic;

public class RoomManager : MonoBehaviour
{
    public static RoomManager Instance { get; private set; }
    public RoomData CurrentRoom { get; private set; }
    public static bool IsOwner { get; private set; } = false;

    private string deviceId;
    private string saveFolder;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        deviceId = PlayerPrefs.GetString("DeviceId", "");
        if (string.IsNullOrEmpty(deviceId))
        {
            deviceId = System.Guid.NewGuid().ToString("N").Substring(0, 12).ToUpper();
            PlayerPrefs.SetString("DeviceId", deviceId);
            PlayerPrefs.Save();
        }

        saveFolder = Path.Combine(Application.persistentDataPath, "Rooms");
        if (!Directory.Exists(saveFolder))
            Directory.CreateDirectory(saveFolder);

        Debug.Log($"[ROOM MGR] Device ID: {deviceId}");
    }

    public RoomData CreateNewRoom(string templateName = "")
    {
        string newId = GenerateRoomId();
        CurrentRoom = new RoomData
        {
            roomId = newId,
            ownerId = deviceId,
            roomTemplateName = templateName,
            objects = new List<PlacedObjectData>(),
            shelves = new List<PlacedShelfData>()
        };
        IsOwner = true;
        SaveCurrentRoom();
        Debug.Log($"[ROOM MGR] Created: {newId}");
        return CurrentRoom;
    }

    public RoomData CreateRoomWithId(string roomId, string ownerId, string templateName = "")
    {
        roomId = roomId.Trim().ToUpper();

        CurrentRoom = new RoomData
        {
            roomId = roomId,
            ownerId = ownerId,
            roomTemplateName = templateName,
            objects = new List<PlacedObjectData>(),
            shelves = new List<PlacedShelfData>()
        };

        IsOwner = true;
        SaveCurrentRoom();

        Debug.Log($"[ROOM MGR] Created with cloud ID: {roomId}");
        return CurrentRoom;
    }

    public RoomData LoadRoom(string roomId)
    {

        roomId = roomId.Trim().ToUpper();
        string path = GetRoomFilePath(roomId);

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[ROOM MGR] Room '{roomId}' not found.");
            return null;
        }

        RoomData data = JsonUtility.FromJson<RoomData>(File.ReadAllText(path));
        CurrentRoom = data;

        string firebaseUid = "";

        if (Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser != null)
        {
            firebaseUid = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;
        }

        IsOwner = data.ownerId == firebaseUid;

        string intent = PlayerPrefs.GetString("RoomIntent", "");

        

        Debug.Log($"[ROOM MGR] Loaded: {roomId} | Owner: {IsOwner} | intent={intent} | ownerId={data.ownerId} | deviceId={deviceId} | firebaseUid={firebaseUid}");

        Debug.Log($"[ROOM MGR] Loaded: {roomId} | Owner: {IsOwner} | ownerId={data.ownerId} | deviceId={deviceId} | firebaseUid={firebaseUid}");

        return data;
    }

    public void SaveCurrentRoom()
    {
        if (CurrentRoom == null || !IsOwner) return;

        File.WriteAllText(GetRoomFilePath(CurrentRoom.roomId),
            JsonUtility.ToJson(CurrentRoom, prettyPrint: true));

        Debug.Log($"[ROOM MGR] Saved: {CurrentRoom.roomId} | Objects: {CurrentRoom.objects.Count}");

        CloudRoomSummarySaver.UpdateRoomSummary(CurrentRoom);
        CloudRoomDataSaver.SaveRoomData(CurrentRoom);
    }

    public void AddObjectToRoom(PlacedObjectData data)
    {
        if (CurrentRoom == null || !RoomAccessManager.CanEdit) return;
        CurrentRoom.objects.Add(data);
        SaveCurrentRoom();
        CloudObjectSaver.SaveObject(CurrentRoom.roomId, data);
        Debug.Log($"[ROOM MGR] Added object: {data.objectName} id={data.instanceId}");
    }

    public void RemoveObjectFromRoom(string instanceId)
    {
        if (CurrentRoom == null || !IsOwner) return;

        int removed = CurrentRoom.objects.RemoveAll(o => o.instanceId == instanceId);

        if (removed > 0)
        {
            SaveCurrentRoom();
            CloudObjectSaver.DeleteObject(CurrentRoom.roomId, instanceId);
        }

        Debug.Log($"[ROOM MGR] Removed object id={instanceId}");
    }

    // FIX: use instanceId for reliable lookup
    public void UpdateObjectMeta(ObjectMetaData meta)
    {
        if (CurrentRoom == null || !RoomAccessManager.CanEdit) return;



        var obj = CurrentRoom.objects.Find(o => o.instanceId == meta.instanceId);
        if (obj != null)
        {
            obj.objectName = meta.objectName;
            obj.description = meta.description;
            obj.objectType = (int)meta.objectType;
            obj.price = meta.price;
            obj.currency = meta.currency;
            SaveCurrentRoom();
            CloudObjectSaver.SaveObject(CurrentRoom.roomId, obj);
            Debug.Log($"[ROOM MGR] Updated meta: {meta.objectName} | {meta.objectType} | {meta.price} | id={meta.instanceId}");
        }
        else
        {
            Debug.LogWarning($"[ROOM MGR] Object not found for id={meta.instanceId}. Trying to add.");
        }
    }

    public void AddShelfToRoom(PlacedShelfData shelfData)
    {
        if (CurrentRoom == null)
        {
            Debug.LogWarning("[ROOM MANAGER] Cannot add shelf: CurrentRoom null");
            return;
        }

        if (CurrentRoom.shelves == null)
            CurrentRoom.shelves = new List<PlacedShelfData>();

        if (string.IsNullOrEmpty(shelfData.shelfId))
            shelfData.shelfId = System.Guid.NewGuid().ToString();

        CurrentRoom.shelves.Add(shelfData);

        Debug.Log("[ROOM MANAGER] Shelf added. Total shelves=" + CurrentRoom.shelves.Count);
    }

    public void RemoveShelfFromRoom(PlacedShelfData data)
    {
        if (CurrentRoom == null || !IsOwner) return;
        CurrentRoom.shelves.Remove(data);
        SaveCurrentRoom();
    }

    public void RemoveShelfById(string shelfId)
    {
        if (CurrentRoom == null)
        {
            Debug.LogWarning("[ROOM MANAGER] Cannot remove shelf: CurrentRoom null");
            return;
        }

        if (!RoomAccessManager.CanEdit)
        {
            Debug.LogWarning("[ROOM MANAGER] Cannot remove shelf: no edit permission");
            return;
        }

        if (CurrentRoom.shelves == null)
            return;

        int removed = CurrentRoom.shelves.RemoveAll(s => s.shelfId == shelfId);

        if (removed > 0)
        {
            SaveCurrentRoom();
            CloudShelfSaver.DeleteShelf(CurrentRoom.roomId, shelfId);
            Debug.Log("[ROOM MANAGER] Removed shelf id=" + shelfId);
        }
        else
        {
            Debug.LogWarning("[ROOM MANAGER] Shelf not found id=" + shelfId);
        }
    }


    public string GetDeviceId() => deviceId;

    string GenerateRoomId()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        string id;
        do
        {
            char[] result = new char[6];
            for (int i = 0; i < 6; i++)
                result[i] = chars[Random.Range(0, chars.Length)];
            id = new string(result);
        }
        while (File.Exists(GetRoomFilePath(id)));
        return id;
    }

    string GetRoomFilePath(string roomId) =>
        Path.Combine(saveFolder, roomId.ToUpper() + ".json");

    public void SetCurrentRoomFromCloud(RoomData data, bool visitorMode)
    {
        CurrentRoom = data;

        string firebaseUid = "";

        if (Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser != null)
            firebaseUid = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        IsOwner = data.ownerId == firebaseUid;

        Debug.Log(
            $"[ROOM MGR] Cloud room set: {data.roomId} | " +
            $"Owner={IsOwner} | ownerId={data.ownerId} | firebaseUid={firebaseUid} | visitorMode={visitorMode}"
        );
    }

    public void RefreshOwnership()
    {
        if (CurrentRoom == null) return;

        string firebaseUid = "";

        if (Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser != null)
            firebaseUid = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        IsOwner = CurrentRoom.ownerId == firebaseUid;

        Debug.Log($"[ROOM MGR] Ownership refreshed | Owner={IsOwner} | ownerId={CurrentRoom.ownerId} | firebaseUid={firebaseUid}");
    }

}