using System.Collections;
using Firebase.Database;
using UnityEngine;

public class RealtimeRoomCustomizationSync : MonoBehaviour
{
    [SerializeField] private CustomRoomBuilder roomBuilder;

    private DatabaseReference dbRef;
    private string roomId;
    private bool firstSnapshot = true;

    IEnumerator Start()
    {
        Debug.Log("[REALTIME ROOM] Start called");

        yield return new WaitUntil(() =>
            RoomManager.Instance != null &&
            RoomManager.Instance.CurrentRoom != null
        );

        if (roomBuilder == null)
            roomBuilder = FindFirstObjectByType<CustomRoomBuilder>();

        if (roomBuilder == null)
        {
            Debug.LogError("[REALTIME ROOM] CustomRoomBuilder not found.");
            yield break;
        }

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        roomId = RoomManager.Instance.CurrentRoom.roomId;

        ListenCustomRoom();

        Debug.Log("[REALTIME ROOM] Listening roomData/" + roomId + "/customRoom");
    }

    void ListenCustomRoom()
    {
        dbRef.Child("roomData")
            .Child(roomId)
            .Child("customRoom")
            .ValueChanged += OnCustomRoomChanged;
    }

    void OnCustomRoomChanged(object sender, ValueChangedEventArgs e)
    {
        if (!e.Snapshot.Exists)
            return;

        if (firstSnapshot)
        {
            firstSnapshot = false;
            return;
        }

        ModularRoomConfig cfg = SnapshotToConfig(e.Snapshot);

        roomBuilder.ApplyRemoteConfig(cfg);

        Debug.Log("[REALTIME ROOM] Custom room updated from Firebase");
    }

    ModularRoomConfig SnapshotToConfig(DataSnapshot snap)
    {
        ModularRoomConfig cfg = new ModularRoomConfig();

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

    float GetFloat(DataSnapshot snap, string key, float fallback)
    {
        if (!snap.Child(key).Exists) return fallback;
        return float.TryParse(snap.Child(key).Value.ToString(), out float v) ? v : fallback;
    }

    string GetString(DataSnapshot snap, string key, string fallback)
    {
        return snap.Child(key).Exists ? snap.Child(key).Value.ToString() : fallback;
    }

    bool GetBool(DataSnapshot snap, string key, bool fallback)
    {
        if (!snap.Child(key).Exists) return fallback;
        return bool.TryParse(snap.Child(key).Value.ToString(), out bool v) ? v : fallback;
    }

    void OnDestroy()
    {
        if (dbRef != null && !string.IsNullOrEmpty(roomId))
        {
            dbRef.Child("roomData")
                .Child(roomId)
                .Child("customRoom")
                .ValueChanged -= OnCustomRoomChanged;
        }
    }
}