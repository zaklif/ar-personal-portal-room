



//using UnityEngine;
//using System.Collections;
//using System.IO;

//public class RoomLoader : MonoBehaviour
//{
//    [Header("References")]
//    [SerializeField] private ObjectPlacementController objectPlacementController;
//    [SerializeField] private ShelfPlacementController shelfPlacementController;

//    [Header("Room ID Display")]
//    [SerializeField] private TMPro.TMP_Text roomIdDisplayText;
//    [SerializeField] private TMPro.TMP_Text roomModeText;

//    [Header("GLB Loading")]
//    [SerializeField] private GlbFileLoader glbFileLoader;

//    private RoomData currentRoomData;
//    private bool isVisitMode = false;
//    private GameObject spawnedRoom;

//    IEnumerator Start()
//    {
//        Debug.Log("[ROOM LOADER] Start");

//        string intent = PlayerPrefs.GetString("RoomIntent", "create");
//        string roomId = PlayerPrefs.GetString("ActiveRoomId", "");

//        isVisitMode = (intent == "visit");

//        if (string.IsNullOrEmpty(roomId))
//        {
//            currentRoomData = RoomManager.Instance.CreateNewRoom();
//            isVisitMode = false;
//        }
//        else
//        {
//            currentRoomData = RoomManager.Instance.LoadRoom(roomId);

//            if (currentRoomData == null)
//            {
//                Debug.Log("[ROOM LOADER] Local room missing. Loading from Firebase...");

//                bool done = false;

//                CloudRoomLoader.LoadRoomFromCloud(roomId, (cloudRoom) =>
//                {
//                    currentRoomData = cloudRoom;
//                    done = true;
//                });

//                while (!done)
//                    yield return null;

//                if (currentRoomData == null)
//                {
//                    Debug.LogError("[ROOM LOADER] Cloud room not found. Returning to Home.");
//                    yield break;
//                }

//                RoomManager.Instance.SetCurrentRoomFromCloud(currentRoomData, isVisitMode);

//            }
//        }
//        RoomManager.Instance.RefreshOwnership();

//        if (roomIdDisplayText != null && currentRoomData != null)
//            roomIdDisplayText.text = "Room ID: " + currentRoomData.roomId;

//        if (roomModeText != null)
//            roomModeText.text = RoomManager.IsOwner ? "OWNER" : "VISITOR";

//        bool isOwnerNow = RoomManager.IsOwner;

//        if (!isOwnerNow)
//        {
//            Debug.Log("[ROOM LOADER] Visitor detected. Locking editing.");
//            LockEditingForVisitor();
//        }
//        else
//        {
//            Debug.Log("[ROOM LOADER] Owner detected. Editing allowed.");
//        }


//        Debug.Log($"[ROOM LOADER] Room:{currentRoomData.roomId} | Mode:{(isVisitMode ? "VISIT" : "OWNER")} | Objects:{currentRoomData.objects.Count} | Shelves:{currentRoomData.shelves.Count}");

//        Debug.Log("[ROOM LOADER OWNER CHECK] roomId=" + currentRoomData.roomId +
//          " ownerId=" + currentRoomData.ownerId +
//          " firebaseUid=" + Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser?.UserId +
//          " intent=" + PlayerPrefs.GetString("RoomIntent", ""));
//    }

//    public void OnRoomPortalPlaced(GameObject roomObject = null)
//    {
//        spawnedRoom = roomObject;
//        if (currentRoomData == null) return;

//        if (shelfPlacementController != null)
//            shelfPlacementController.SetPortalRoom(spawnedRoom);

//        // Restore shelves first
//        if (currentRoomData.shelves != null && currentRoomData.shelves.Count > 0)
//            shelfPlacementController?.RestoreShelves(currentRoomData.shelves, spawnedRoom);

//        // Then restore objects
//        if (currentRoomData.objects.Count > 0)
//            StartCoroutine(RestoreObjectsCoroutine());
//        else
//            Debug.Log("[ROOM LOADER] No objects to restore.");
//    }

//    IEnumerator RestoreObjectsCoroutine()
//    {
//        yield return null;
//        Debug.Log($"[ROOM LOADER] Restoring {currentRoomData.objects.Count} objects...");

//        foreach (var objData in currentRoomData.objects)
//        {
//            yield return StartCoroutine(LoadAndPlaceObject(objData));
//            yield return null;
//        }

//        Debug.Log("[ROOM LOADER] All objects restored.");
//    }

//    IEnumerator LoadAndPlaceObject(PlacedObjectData data)
//    {
//        if (glbFileLoader == null) { Debug.LogError("[ROOM LOADER] GlbFileLoader not assigned!"); yield break; }

//        string filePath = Path.Combine(Application.persistentDataPath, "UploadedModels", data.fileName);

//        if (!File.Exists(filePath))
//        {
//            Debug.LogWarning("[ROOM LOADER] Local file missing: " + filePath);

//            if (!string.IsNullOrEmpty(data.glbUrl))
//            {
//                Debug.Log("[ROOM LOADER] Downloading GLB from cloud...");

//                bool done = false;
//                bool success = false;

//                yield return StartCoroutine(CloudModelDownloader.DownloadGlb(
//                    data.glbUrl,
//                    data.fileName,
//                    (downloadedPath) =>
//                    {
//                        if (!string.IsNullOrEmpty(downloadedPath))
//                        {
//                            filePath = downloadedPath;
//                            success = true;
//                        }

//                        done = true;
//                    }));

//                while (!done)
//                    yield return null;

//                if (!success)
//                {
//                    Debug.LogError("[ROOM LOADER] Cloud download failed.");
//                    yield break;
//                }
//            }
//            else
//            {
//                Debug.LogError("[ROOM LOADER] No glbUrl found for object: " + data.objectName);
//                yield break;
//            }
//        }

//        bool loaded = false;
//        GameObject loadedObj = null;

//        glbFileLoader.LoadFile(filePath, (obj) => { loadedObj = obj; loaded = true; });

//        float elapsed = 0f;
//        while (!loaded && elapsed < 10f) { elapsed += Time.deltaTime; yield return null; }

//        if (loadedObj == null) { Debug.LogWarning($"[ROOM LOADER] Load failed: {data.fileName}"); yield break; }

//        // Parent to room and apply local position
//        if (spawnedRoom != null)
//        {
//            loadedObj.transform.SetParent(spawnedRoom.transform);
//            loadedObj.transform.localPosition = new Vector3(data.posX, data.posY, data.posZ);
//            loadedObj.transform.localRotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);
//            loadedObj.transform.localScale = new Vector3(data.scaleX, data.scaleY, data.scaleZ);

//        }
//        else
//        {
//            loadedObj.transform.position = new Vector3(data.posX, data.posY, data.posZ);
//            loadedObj.transform.rotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);
//            loadedObj.transform.localScale = new Vector3(data.scaleX, data.scaleY, data.scaleZ);


//        }

//        // Register with metadata restored from saved data
//        if (objectPlacementController != null)
//            objectPlacementController.RegisterRestoredObject(
//                loadedObj,
//                data.fileName,
//                data
//            );

//        RealtimeRoomObjectSync.RegisterSpawnedObject(
//            data.instanceId,
//            loadedObj
//        );

//        Debug.Log($"[ROOM LOADER] Restored: {data.fileName} | {data.objectName}");

//        Debug.Log($"[ROOM LOADER] Restored: {data.fileName} | {data.objectName} | {(ObjectMetaData.ObjectType)data.objectType}");



//    }

//    public void LoadSingleObjectFromRealtime(PlacedObjectData data)
//    {
//        StartCoroutine(LoadAndPlaceObject(data));
//    }
//    void LockEditingForVisitor()
//    {
//        if (objectPlacementController != null)
//        {
//            objectPlacementController.HideBottomBar();
//            objectPlacementController.SetVisitorMode(true);
//        }
//        if (shelfPlacementController != null)
//            shelfPlacementController.SetVisitorMode(true);
//    }
//}





using UnityEngine;
using System.Collections;
using System.IO;

public class RoomLoader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ObjectPlacementController objectPlacementController;
    [SerializeField] private ShelfPlacementController shelfPlacementController;

    [Header("Room ID Display")]
    [SerializeField] private TMPro.TMP_Text roomIdDisplayText;
    [SerializeField] private TMPro.TMP_Text roomModeText;

    [Header("GLB Loading")]
    [SerializeField] private GlbFileLoader glbFileLoader;

    private RoomData currentRoomData;
    private bool isVisitMode = false;
    private GameObject spawnedRoom;

    [Header("Mode Based UI")]
    [SerializeField] private GameObject ownerOnlyButtonsGroup;
    [SerializeField] private GameObject visitorOnlyButtonsGroup;
    [SerializeField] private GameObject updateThumbnailButton;
    [SerializeField] private GameObject visitorButton;
    [SerializeField] private GameObject requestButton;


    IEnumerator Start()
    {
        Debug.Log("[ROOM LOADER] Start");

        // FORCE HIDE editing UI immediately when scene loads so it never flashes upfront
        if (objectPlacementController != null)
        {
            objectPlacementController.HideBottomBar();
        }

        string intent = PlayerPrefs.GetString("RoomIntent", "create");
        string roomId = PlayerPrefs.GetString("ActiveRoomId", "");
        // ... (rest of your loading logic remains exactly the same)


        isVisitMode = (intent == "visit");

        if (string.IsNullOrEmpty(roomId))
        {
            currentRoomData = RoomManager.Instance.CreateNewRoom();
            isVisitMode = false;
        }
        else
        {
            currentRoomData = RoomManager.Instance.LoadRoom(roomId);

            if (currentRoomData == null)
            {
                Debug.Log("[ROOM LOADER] Local room missing. Loading from Firebase...");

                bool done = false;

                CloudRoomLoader.LoadRoomFromCloud(roomId, (cloudRoom) =>
                {
                    currentRoomData = cloudRoom;
                    done = true;
                });

                while (!done)
                    yield return null;

                if (currentRoomData == null)
                {
                    Debug.LogError("[ROOM LOADER] Cloud room not found. Returning to Home.");
                    yield break;
                }

                RoomManager.Instance.SetCurrentRoomFromCloud(currentRoomData, isVisitMode);

            }
        }
        RoomManager.Instance.RefreshOwnership();

        // IMPORTANT:
        // Make sure doorFramePlacementController uses the correct room template.
        // This fixes MyRooms / Explore opening fallback room.
        if (currentRoomData != null)
        {
            PlayerPrefs.SetString("ChosenTemplateName", currentRoomData.roomTemplateName);
            PlayerPrefs.SetString("ActiveRoomId", currentRoomData.roomId);
            PlayerPrefs.Save();

            Debug.Log("[ROOM LOADER] ChosenTemplateName fixed from room data: " + currentRoomData.roomTemplateName);
        }

        if (roomIdDisplayText != null && currentRoomData != null)
            roomIdDisplayText.text = "Room ID: " + currentRoomData.roomId;

        if (roomModeText != null)
            roomModeText.text = RoomManager.IsOwner ? "OWNER" : "VISITOR";

        bool isOwnerNow = RoomManager.IsOwner;

        ApplyModeUI(isOwnerNow);

        if (!isOwnerNow)
        {
            Debug.Log("[ROOM LOADER] Visitor detected. Locking editing.");
            LockEditingForVisitor();
        }
        else
        {
            Debug.Log("[ROOM LOADER] Owner detected. Editing allowed.");
        }


        Debug.Log($"[ROOM LOADER] Room:{currentRoomData.roomId} | Mode:{(isVisitMode ? "VISIT" : "OWNER")} | Objects:{currentRoomData.objects.Count} | Shelves:{currentRoomData.shelves.Count}");

        Debug.Log("[ROOM LOADER OWNER CHECK] roomId=" + currentRoomData.roomId +
          " ownerId=" + currentRoomData.ownerId +
          " firebaseUid=" + Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser?.UserId +
          " intent=" + PlayerPrefs.GetString("RoomIntent", ""));
    }


    //IEnumerator Start()
    //{
    //    Debug.Log("[ROOM LOADER] Start");

    //    string intent = PlayerPrefs.GetString("RoomIntent", "create");
    //    string roomId = PlayerPrefs.GetString("ActiveRoomId", "");

    //    isVisitMode = (intent == "visit");

    //    if (string.IsNullOrEmpty(roomId))
    //    {
    //        currentRoomData = RoomManager.Instance.CreateNewRoom();
    //        isVisitMode = false;
    //    }
    //    else
    //    {
    //        currentRoomData = RoomManager.Instance.LoadRoom(roomId);

    //        if (currentRoomData == null)
    //        {
    //            Debug.Log("[ROOM LOADER] Local room missing. Loading from Firebase...");

    //            bool done = false;

    //            CloudRoomLoader.LoadRoomFromCloud(roomId, (cloudRoom) =>
    //            {
    //                currentRoomData = cloudRoom;
    //                done = true;
    //            });

    //            while (!done)
    //                yield return null;

    //            if (currentRoomData == null)
    //            {
    //                Debug.LogError("[ROOM LOADER] Cloud room not found. Returning to Home.");
    //                yield break;
    //            }

    //            RoomManager.Instance.SetCurrentRoomFromCloud(currentRoomData, isVisitMode);

    //        }
    //    }
    //    RoomManager.Instance.RefreshOwnership();

    //    // IMPORTANT:
    //    // Make sure doorFramePlacementController uses the correct room template.
    //    // This fixes MyRooms / Explore opening fallback room.
    //    if (currentRoomData != null)
    //    {
    //        PlayerPrefs.SetString("ChosenTemplateName", currentRoomData.roomTemplateName);
    //        PlayerPrefs.SetString("ActiveRoomId", currentRoomData.roomId);
    //        PlayerPrefs.Save();

    //        Debug.Log("[ROOM LOADER] ChosenTemplateName fixed from room data: " + currentRoomData.roomTemplateName);
    //    }

    //    if (roomIdDisplayText != null && currentRoomData != null)
    //        roomIdDisplayText.text = "Room ID: " + currentRoomData.roomId;

    //    if (roomModeText != null)
    //        roomModeText.text = RoomManager.IsOwner ? "OWNER" : "VISITOR";

    //    bool isOwnerNow = RoomManager.IsOwner;

    //    ApplyModeUI(isOwnerNow);

    //    if (!isOwnerNow)
    //    {
    //        Debug.Log("[ROOM LOADER] Visitor detected. Locking editing.");
    //        LockEditingForVisitor();
    //    }
    //    else
    //    {
    //        Debug.Log("[ROOM LOADER] Owner detected. Editing allowed.");
    //    }


    //    Debug.Log($"[ROOM LOADER] Room:{currentRoomData.roomId} | Mode:{(isVisitMode ? "VISIT" : "OWNER")} | Objects:{currentRoomData.objects.Count} | Shelves:{currentRoomData.shelves.Count}");

    //    Debug.Log("[ROOM LOADER OWNER CHECK] roomId=" + currentRoomData.roomId +
    //      " ownerId=" + currentRoomData.ownerId +
    //      " firebaseUid=" + Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser?.UserId +
    //      " intent=" + PlayerPrefs.GetString("RoomIntent", ""));
    //}

    public void OnRoomPortalPlaced(GameObject roomObject = null)
    {
        spawnedRoom = roomObject;
        if (currentRoomData == null) return;

        if (shelfPlacementController != null)
            shelfPlacementController.SetPortalRoom(spawnedRoom);

        // Restore shelves first
        if (currentRoomData.shelves != null && currentRoomData.shelves.Count > 0)
            shelfPlacementController?.RestoreShelves(currentRoomData.shelves, spawnedRoom);

        // Then restore objects
        if (currentRoomData.objects.Count > 0)
            StartCoroutine(RestoreObjectsCoroutine());
        else
            Debug.Log("[ROOM LOADER] No objects to restore.");
    }

    private void ApplyModeUI(bool isOwner)
    {
        if (ownerOnlyButtonsGroup != null)
            ownerOnlyButtonsGroup.SetActive(isOwner);

        if (visitorOnlyButtonsGroup != null)
            visitorOnlyButtonsGroup.SetActive(!isOwner);

        if (updateThumbnailButton != null)
            updateThumbnailButton.SetActive(isOwner);

        if (visitorButton != null)
            visitorButton.SetActive(isOwner);

        if (requestButton != null)
            requestButton.SetActive(isOwner);

        Debug.Log("[ROOM LOADER UI] ApplyModeUI owner=" + isOwner);
    }

    IEnumerator RestoreObjectsCoroutine()
    {
        yield return null;
        Debug.Log($"[ROOM LOADER] Restoring {currentRoomData.objects.Count} objects...");

        foreach (var objData in currentRoomData.objects)
        {
            yield return StartCoroutine(LoadAndPlaceObject(objData));
            yield return null;
        }

        Debug.Log("[ROOM LOADER] All objects restored.");
    }

    IEnumerator LoadAndPlaceObject(PlacedObjectData data)
    {
        if (glbFileLoader == null)
        {
            Debug.LogError("[ROOM LOADER] GlbFileLoader not assigned!");
            yield break;
        }

        // ─────────────────────────────────────────────
        // BUILT-IN DECORATION OBJECT
        // ─────────────────────────────────────────────
        if (data.isBuiltIn)
        {
            if (objectPlacementController == null)
            {
                Debug.LogError("[ROOM LOADER] ObjectPlacementController missing. Cannot restore built-in object.");
                yield break;
            }

            GameObject prefab = objectPlacementController.GetBuiltInPrefabByKey(data.prefabKey);

            if (prefab == null)
            {
                Debug.LogError("[ROOM LOADER] Built-in prefab not found: " + data.prefabKey);
                yield break;
            }

            GameObject builtInObj = Instantiate(prefab);

            if (spawnedRoom != null)
            {
                builtInObj.transform.SetParent(spawnedRoom.transform);
                builtInObj.transform.localPosition = new Vector3(data.posX, data.posY, data.posZ);
                builtInObj.transform.localRotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);
                builtInObj.transform.localScale = new Vector3(data.scaleX, data.scaleY, data.scaleZ);
            }
            else
            {
                builtInObj.transform.position = new Vector3(data.posX, data.posY, data.posZ);
                builtInObj.transform.rotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);
                builtInObj.transform.localScale = new Vector3(data.scaleX, data.scaleY, data.scaleZ);
            }

            objectPlacementController.RegisterRestoredObject(
                builtInObj,
                data.fileName,
                data
            );

            RealtimeRoomObjectSync.RegisterSpawnedObject(
                data.instanceId,
                builtInObj
            );

            Debug.Log("[ROOM LOADER] Restored built-in object: " + data.prefabKey);
            yield break;
        }

        // ─────────────────────────────────────────────
        // IMPORTED GLB OBJECT
        // ─────────────────────────────────────────────
        string filePath = Path.Combine(Application.persistentDataPath, "UploadedModels", data.fileName);

        if (!File.Exists(filePath))
        {
            Debug.LogWarning("[ROOM LOADER] Local file missing: " + filePath);

            if (!string.IsNullOrEmpty(data.glbUrl))
            {
                Debug.Log("[ROOM LOADER] Downloading GLB from cloud...");

                bool done = false;
                bool success = false;

                yield return StartCoroutine(CloudModelDownloader.DownloadGlb(
                    data.glbUrl,
                    data.fileName,
                    (downloadedPath) =>
                    {
                        if (!string.IsNullOrEmpty(downloadedPath))
                        {
                            filePath = downloadedPath;
                            success = true;
                        }

                        done = true;
                    }));

                while (!done)
                    yield return null;

                if (!success)
                {
                    Debug.LogError("[ROOM LOADER] Cloud download failed.");
                    yield break;
                }
            }
            else
            {
                Debug.LogError("[ROOM LOADER] No glbUrl found for object: " + data.objectName);
                yield break;
            }
        }

        bool loaded = false;
        GameObject glbObj = null;

        glbFileLoader.LoadFile(filePath, (obj) =>
        {
            glbObj = obj;
            loaded = true;
        });

        float elapsed = 0f;

        while (!loaded && elapsed < 10f)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        if (glbObj == null)
        {
            Debug.LogWarning($"[ROOM LOADER] Load failed: {data.fileName}");
            yield break;
        }

        if (spawnedRoom != null)
        {
            glbObj.transform.SetParent(spawnedRoom.transform);
            glbObj.transform.localPosition = new Vector3(data.posX, data.posY, data.posZ);
            glbObj.transform.localRotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);
            glbObj.transform.localScale = new Vector3(data.scaleX, data.scaleY, data.scaleZ);
        }
        else
        {
            glbObj.transform.position = new Vector3(data.posX, data.posY, data.posZ);
            glbObj.transform.rotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);
            glbObj.transform.localScale = new Vector3(data.scaleX, data.scaleY, data.scaleZ);
        }

        if (objectPlacementController != null)
        {
            objectPlacementController.RegisterRestoredObject(
                glbObj,
                data.fileName,
                data
            );
        }

        RealtimeRoomObjectSync.RegisterSpawnedObject(
            data.instanceId,
            glbObj
        );

        Debug.Log($"[ROOM LOADER] Restored: {data.fileName} | {data.objectName}");
        Debug.Log($"[ROOM LOADER] Restored: {data.fileName} | {data.objectName} | {(ObjectMetaData.ObjectType)data.objectType}");
    }

    public void LoadSingleObjectFromRealtime(PlacedObjectData data)
    {
        StartCoroutine(LoadAndPlaceObject(data));
    }
    void LockEditingForVisitor()
    {
        if (objectPlacementController != null)
        {
            objectPlacementController.HideBottomBar();
            objectPlacementController.SetVisitorMode(true);
        }
        if (shelfPlacementController != null)
            shelfPlacementController.SetVisitorMode(true);
    }
}