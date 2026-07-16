//using Firebase.Database;
//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class RealtimeRoomObjectSync : MonoBehaviour
//{
//    [SerializeField] private RoomLoader roomLoader;

//    private DatabaseReference dbRef;
//    private string roomId;

//    IEnumerator Start()
//    {
//        Debug.Log("[REALTIME OBJECT] Start called");

//        yield return new WaitUntil(() =>
//            RoomManager.Instance != null &&
//            RoomManager.Instance.CurrentRoom != null
//        );

//        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
//        roomId = RoomManager.Instance.CurrentRoom.roomId;

//        Debug.Log("[REALTIME OBJECT] RoomId = " + roomId);

//        ListenObjects();

//        Debug.Log("[REALTIME OBJECT] Listening roomObjects/" + roomId);
//        Debug.Log("[REALTIME OBJECT] Listener connected.");
//    }

//    void ListenObjects()
//    {
//        dbRef.Child("roomObjects")
//            .Child(roomId)
//            .ValueChanged += OnObjectsChanged;
//    }

//    void OnObjectsChanged(object sender, ValueChangedEventArgs e)
//    {
//        Debug.Log("[REALTIME OBJECT] Objects changed. Count=" + e.Snapshot.ChildrenCount);


//        // For first version, just tell us change detected.
//        // Next step: reload/add missing objects.
//    }

//    void OnDestroy()
//    {
//        if (dbRef != null && !string.IsNullOrEmpty(roomId))
//        {
//            dbRef.Child("roomObjects")
//                .Child(roomId)
//                .ValueChanged -= OnObjectsChanged;
//        }
//    }
//}


using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using UnityEngine;

public class RealtimeRoomObjectSync : MonoBehaviour
{
    [SerializeField] private RoomLoader roomLoader;

    private DatabaseReference dbRef;
    private string roomId;

    private HashSet<string> loadedObjectIds = new HashSet<string>();
    private bool firstSnapshot = true;

    private Dictionary<string, GameObject> spawnedObjects =
    new Dictionary<string, GameObject>();

    public static RealtimeRoomObjectSync Instance;

    IEnumerator Start()
    {
        Debug.Log("[REALTIME OBJECT] Start called");

        yield return new WaitUntil(() =>
            RoomManager.Instance != null &&
            RoomManager.Instance.CurrentRoom != null
        );

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
        roomId = RoomManager.Instance.CurrentRoom.roomId;

        if (roomLoader == null)
            roomLoader = FindFirstObjectByType<RoomLoader>();

        ListenObjects();

        Debug.Log("[REALTIME OBJECT] Listener connected: " + roomId);
    }

    void ListenObjects()
    {
        dbRef.Child("roomObjects")
            .Child(roomId)
            .ValueChanged += OnObjectsChanged;
    }
    void Awake()
    {
        Instance = this;
    }

    public static void RegisterSpawnedObject(
    string instanceId,
    GameObject obj)
    {
        if (Instance == null)
            return;

        Instance.spawnedObjects[instanceId] = obj;
    }


    void OnObjectsChanged(object sender, ValueChangedEventArgs e)
    {
        Debug.Log("[REALTIME OBJECT] Objects changed. Count=" + e.Snapshot.ChildrenCount);

        HashSet<string> firebaseIds = new HashSet<string>();

        if (e.Snapshot.Exists)
        {
            foreach (DataSnapshot objSnap in e.Snapshot.Children)
                firebaseIds.Add(objSnap.Key);
        }

        // 1. Add new objects
        if (e.Snapshot.Exists)
        {
            foreach (DataSnapshot objSnap in e.Snapshot.Children)
            {
                string objectId = objSnap.Key;


                if (loadedObjectIds.Contains(objectId))
                {
                    ApplyMetadataUpdate(objSnap);
                    ApplyTransformUpdate(objSnap);
                    continue;
                }

                if (firstSnapshot)
                {
                    loadedObjectIds.Add(objectId);
                    continue;
                }

                PlacedObjectData data = SnapshotToPlacedObjectData(objSnap);

                if (data == null)
                    continue;

                loadedObjectIds.Add(objectId);

                Debug.Log("[REALTIME OBJECT] New object detected: " + data.objectName);

                roomLoader.LoadSingleObjectFromRealtime(data);
            }
        }

        // 2. Remove deleted objects
        List<string> deletedIds = new List<string>();

        foreach (var id in loadedObjectIds)
        {
            if (!firebaseIds.Contains(id))
                deletedIds.Add(id);
        }

        foreach (var id in deletedIds)
        {
            loadedObjectIds.Remove(id);

            if (spawnedObjects.TryGetValue(id, out GameObject obj))
            {
                Destroy(obj);
                spawnedObjects.Remove(id);

                Debug.Log("[REALTIME OBJECT] Removed object: " + id);
            }
        }

        firstSnapshot = false;
    }

    PlacedObjectData SnapshotToPlacedObjectData(DataSnapshot snap)
    {
        try
        {
            PlacedObjectData data = new PlacedObjectData();

            data.instanceId = snap.Child("instanceId").Value?.ToString() ?? snap.Key;
            data.fileName = snap.Child("fileName").Value?.ToString() ?? "";

            data.objectName = snap.Child("objectName").Value?.ToString() ?? "Object";
            data.description = snap.Child("description").Value?.ToString() ?? "";
            data.currency = snap.Child("currency").Value?.ToString() ?? "RM";

            data.objectType = ToInt(snap.Child("objectType").Value, 0);
            data.price = ToFloat(snap.Child("price").Value, 0f);

            data.posX = ToFloat(snap.Child("posX").Value, 0f);
            data.posY = ToFloat(snap.Child("posY").Value, 0f);
            data.posZ = ToFloat(snap.Child("posZ").Value, 0f);

            data.rotX = ToFloat(snap.Child("rotX").Value, 0f);
            data.rotY = ToFloat(snap.Child("rotY").Value, 0f);
            data.rotZ = ToFloat(snap.Child("rotZ").Value, 0f);
            data.rotW = ToFloat(snap.Child("rotW").Value, 1f);

            data.scaleX = ToFloat(snap.Child("scaleX").Value, 1f);
            data.scaleY = ToFloat(snap.Child("scaleY").Value, 1f);
            data.scaleZ = ToFloat(snap.Child("scaleZ").Value, 1f);

            data.glbUrl = snap.Child("glbUrl").Value?.ToString() ?? "";
            data.storagePath = snap.Child("storagePath").Value?.ToString() ?? "";

            if (string.IsNullOrEmpty(data.fileName) || string.IsNullOrEmpty(data.glbUrl))
            {
                Debug.LogWarning("[REALTIME OBJECT] Missing fileName/glbUrl for " + data.instanceId);
                return null;
            }

            return data;
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[REALTIME OBJECT] Parse failed: " + ex.Message);
            return null;
        }
    }

    float ToFloat(object value, float fallback)
    {
        if (value == null) return fallback;
        if (float.TryParse(value.ToString(), out float result)) return result;
        return fallback;
    }

    int ToInt(object value, int fallback)
    {
        if (value == null) return fallback;
        if (int.TryParse(value.ToString(), out int result)) return result;
        return fallback;
    }

    void OnDestroy()
    {
        if (dbRef != null && !string.IsNullOrEmpty(roomId))
        {
            dbRef.Child("roomObjects")
                .Child(roomId)
                .ValueChanged -= OnObjectsChanged;
        }
    }

    void ApplyMetadataUpdate(DataSnapshot objSnap)
    {
        string objectId = objSnap.Key;

        if (!spawnedObjects.TryGetValue(objectId, out GameObject obj))
            return;

        ObjectMetaData meta = obj.GetComponent<ObjectMetaData>();
        if (meta == null)
            return;

        meta.objectName = objSnap.Child("objectName").Value?.ToString() ?? meta.objectName;
        meta.description = objSnap.Child("description").Value?.ToString() ?? meta.description;
        meta.currency = objSnap.Child("currency").Value?.ToString() ?? meta.currency;

        meta.objectType = (ObjectMetaData.ObjectType)ToInt(
            objSnap.Child("objectType").Value,
            (int)meta.objectType
        );

        meta.price = ToFloat(objSnap.Child("price").Value, meta.price);

        meta.RefreshIndicator();

        Debug.Log("[REALTIME OBJECT] Metadata updated: " + meta.objectName);
    }

    void ApplyTransformUpdate(DataSnapshot objSnap)
    {
        string objectId = objSnap.Key;

        if (!spawnedObjects.TryGetValue(objectId, out GameObject obj))
            return;

        Vector3 newPos = new Vector3(
            ToFloat(objSnap.Child("posX").Value, obj.transform.localPosition.x),
            ToFloat(objSnap.Child("posY").Value, obj.transform.localPosition.y),
            ToFloat(objSnap.Child("posZ").Value, obj.transform.localPosition.z)
        );

        Quaternion newRot = new Quaternion(
            ToFloat(objSnap.Child("rotX").Value, obj.transform.localRotation.x),
            ToFloat(objSnap.Child("rotY").Value, obj.transform.localRotation.y),
            ToFloat(objSnap.Child("rotZ").Value, obj.transform.localRotation.z),
            ToFloat(objSnap.Child("rotW").Value, obj.transform.localRotation.w)
        );

        Vector3 newScale = new Vector3(
            ToFloat(objSnap.Child("scaleX").Value, obj.transform.localScale.x),
            ToFloat(objSnap.Child("scaleY").Value, obj.transform.localScale.y),
            ToFloat(objSnap.Child("scaleZ").Value, obj.transform.localScale.z)
        );

        obj.transform.localPosition = newPos;
        obj.transform.localRotation = newRot;
        obj.transform.localScale = newScale;

        Debug.Log("[REALTIME OBJECT] Transform updated: " + objectId);
    }

}