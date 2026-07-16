using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using UnityEngine.InputSystem.EnhancedTouch;
using TMPro;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ObjectPlacementController : MonoBehaviour
{
    [Header("AR")]
    [SerializeField] private ARRaycastManager arRaycastManager;

    [Header("Prefab Objects")]
    [SerializeField] private GameObject[] objectPrefabs;
    [SerializeField] private string[] objectNames;

    [Header("UI")]
    [SerializeField] private GameObject bottomBar;
    [SerializeField] private GameObject objectListPanel;
    [SerializeField] private Transform listContent;
    [SerializeField] private GameObject listItemPrefab;
    [SerializeField] private GameObject confirmPanel;

    [Header("Owner Edit/Delete Popup")]
    [SerializeField] private GameObject editPopupPanel;
    [SerializeField] private Button editButton;
    [SerializeField] private Button deleteButton;
    [SerializeField] private Button settingsButton;

    [Header("Room Bounds")]
    [SerializeField] private GameObject portalRoom;

    [Header("Shelf Snapping")]
    [SerializeField] private LayerMask shelfLayerMask;
    [SerializeField] private float shelfSnapRayDistance = 5f;

    [Header("Object Features")]
    [SerializeField] private GameObject indicatorPrefab;
    [SerializeField] private VisitorObjectPopup visitorPopup;
    [SerializeField] private OwnerObjectSettings ownerSettings;

    [Header("Bottom Dock Buttons")]
    [SerializeField] private GameObject addObjectButton;
    [SerializeField] private GameObject importObjectButton;
    [SerializeField] private GameObject addShelfButton;
    [SerializeField] private GameObject customRoomButton;

    [SerializeField] private Button closePopupButton;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
    private int selectedIndex = -1;
    private bool isPlacingMode = false;
    private bool isAdjustingMode = false;
    private bool isVisitorMode = false;

    private GameObject runtimeModelPrefab = null;
    private string runtimeModelFileName = "";
    private string runtimeModelFilePath = "";
    private GameObject previewObject = null;

    private float lastPinchDistance;
    private float lastTwoFingerAngle;



    private List<GameObject> placedObjects = new List<GameObject>();
    private Dictionary<GameObject, string> objectFileNames = new Dictionary<GameObject, string>();
    private GameObject currentlySelectedObject = null;
    private List<GameObject> shelfObjects = new List<GameObject>();
    private bool isPopupOpen = false;

    private bool isEditingExistingObject = false;
    void OnEnable() => EnhancedTouchSupport.Enable();
    void OnDisable() => EnhancedTouchSupport.Disable();

    //void Start()
    //{
    //    bottomBar.SetActive(false);
    //    objectListPanel.SetActive(false);
    //    confirmPanel.SetActive(false);
    //    editPopupPanel.SetActive(false);
    //    PopulateList();

    //    if (editButton != null) { editButton.onClick.RemoveAllListeners(); editButton.onClick.AddListener(OnEditPopupPressed); }
    //    if (deleteButton != null) { deleteButton.onClick.RemoveAllListeners(); deleteButton.onClick.AddListener(OnDeletePopupPressed); }
    //    if (settingsButton != null) { settingsButton.onClick.RemoveAllListeners(); settingsButton.onClick.AddListener(OnSettingsPopupPressed); }
    //}

    void Start()
    {
        bottomBar.SetActive(false);
        objectListPanel.SetActive(false);
        confirmPanel.SetActive(false);
        editPopupPanel.SetActive(false);
        PopulateList();

        if (editButton != null)
        {
            editButton.onClick.RemoveAllListeners();
            editButton.onClick.AddListener(OnEditPopupPressed);
            Debug.Log("[POPUP UI] Edit button connected.");
        }
        else
        {
            Debug.LogError("[POPUP UI] Edit button is NOT assigned.");
        }

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(OnDeletePopupPressed);
            Debug.Log("[POPUP UI] Delete button connected.");
        }
        else
        {
            Debug.LogError("[POPUP UI] Delete button is NOT assigned.");
        }

        if (settingsButton != null)
        {
            settingsButton.onClick.RemoveAllListeners();
            settingsButton.onClick.AddListener(OnSettingsPopupPressed);
            Debug.Log("[POPUP UI] Settings button connected.");
        }
        else
        {
            Debug.LogError("[POPUP UI] Settings button is NOT assigned.");
        }

        if (closePopupButton != null)
        {
            closePopupButton.onClick.RemoveAllListeners();
            closePopupButton.onClick.AddListener(OnClosePopupPressed);
            Debug.Log("[POPUP UI] Close button connected.");
        }
        else
        {
            Debug.LogWarning("[POPUP UI] Close button not assigned.");
        }
    }

    public void OnClosePopupPressed()
    {
        Debug.Log("[POPUP UI] Close pressed.");

        ClosePopup();
        currentlySelectedObject = null;

        if (RoomInsideDetector.IsInsideRoom && RoomAccessManager.CanEdit)
            ShowBottomBar();
    }

    public void SetVisitorMode(bool visitor)
    {
        isVisitorMode = visitor;
        if (visitor) { bottomBar.SetActive(false); editPopupPanel.SetActive(false); }
    }

    public void RegisterRestoredObject(GameObject obj, string fileName, PlacedObjectData data = null)
    {
        EnsureCollider(obj);
        AttachMetaData(obj, fileName, data);
        SpawnIndicator(obj);
        placedObjects.Add(obj);
        objectFileNames[obj] = fileName;
        Debug.Log($"[OBJECT] Registered: {fileName} | name={data?.objectName} | type={data?.objectType}");
    }

    void AttachMetaData(GameObject obj, string fileName, PlacedObjectData data = null)
    {
        var meta = obj.GetComponent<ObjectMetaData>();
        if (meta == null) meta = obj.AddComponent<ObjectMetaData>();

        meta.Init(RoomManager.Instance?.CurrentRoom?.roomId ?? "");

        if (data != null)
        {
            // Restore saved instanceId so updates work correctly
            if (!string.IsNullOrEmpty(data.instanceId))
                meta.instanceId = data.instanceId;

            meta.objectName = data.objectName;
            meta.description = data.description;
            meta.objectType = (ObjectMetaData.ObjectType)data.objectType;
            meta.price = data.price;
            meta.currency = data.currency;
        }
        else
        {
            meta.objectName = System.IO.Path.GetFileNameWithoutExtension(fileName);
        }
    }

    void SpawnIndicator(GameObject obj)
    {
        if (indicatorPrefab == null) return;
        var meta = obj.GetComponent<ObjectMetaData>();
        if (meta == null) return;
        GameObject indicator = Instantiate(indicatorPrefab);
        var indUI = indicator.GetComponent<ObjectIndicatorUI>();
        if (indUI != null) indUI.SetTarget(obj.transform);
        meta.SetIndicator(indicator);
    }

    // ── Popup handlers ────────────────────────────────────────────────────

    public void OnEditPopupPressed()
    {
        Debug.Log("[POPUP UI] Edit pressed.");

        if (currentlySelectedObject == null)
        {
            Debug.LogWarning("[POPUP UI] Edit blocked: no selected object.");
            return;
        }

        GameObject objToEdit = currentlySelectedObject;

        ClosePopup();
        BeginEditExistingObject(objToEdit);
    }


    public void OnDeletePopupPressed()
    {
        Debug.Log("[POPUP UI] Delete pressed.");

        if (currentlySelectedObject == null)
        {
            Debug.LogWarning("[POPUP UI] Delete blocked: no selected object.");
            return;
        }

        GameObject objToDelete = currentlySelectedObject;

        ClosePopup();
        DeleteObject(objToDelete);
    }

    public void OnSettingsPopupPressed()
    {
        Debug.Log("[POPUP UI] Settings pressed.");

        if (currentlySelectedObject == null)
        {
            Debug.LogWarning("[POPUP UI] Settings blocked: no selected object.");
            return;
        }

        GameObject objRef = currentlySelectedObject;
        ObjectMetaData meta = objRef.GetComponent<ObjectMetaData>();

        if (meta == null)
        {
            Debug.LogWarning("[POPUP UI] Settings blocked: selected object has no ObjectMetaData.");
            return;
        }

        ClosePopup();

        if (ownerSettings != null)
            ownerSettings.Show(meta, () => SaveMetaToRoom(objRef));
        else
            Debug.LogError("[POPUP UI] OwnerObjectSettings is not assigned.");
    }

    public void RefreshBottomDockButtons()
    {
        bool inside = RoomInsideDetector.IsInsideRoom;
        bool canEdit = RoomAccessManager.CanEdit;
        bool isOwner = RoomManager.IsOwner;

        if (addObjectButton != null)
            addObjectButton.SetActive(inside && canEdit);

        if (importObjectButton != null)
            importObjectButton.SetActive(inside && canEdit);

        if (addShelfButton != null)
            addShelfButton.SetActive(inside && canEdit);

        // Custom Room is owner only.
        if (customRoomButton != null)
            customRoomButton.SetActive(inside && isOwner);
    }

   

    public void OpenPopup(GameObject obj)
    {
        // FIX: don't open if settings or visitor popup already open
        if (ownerSettings != null && ownerSettings.IsOpen) return;

        currentlySelectedObject = obj;
        editPopupPanel.SetActive(true);
        isPopupOpen = true;
    }

    public void ClosePopup()
    {
        editPopupPanel.SetActive(false);
        isPopupOpen = false;
    }

    public void SetPopupOpen(bool open) => isPopupOpen = open;

    public void ShowBottomBar()
    {
        if (bottomBar == null)
            return;

        bool inside = RoomInsideDetector.IsInsideRoom;
        bool canEdit = RoomAccessManager.CanEdit;
        bool isOwner = RoomManager.IsOwner;

        bottomBar.SetActive(inside && canEdit);

        if (addObjectButton != null)
            addObjectButton.SetActive(inside && canEdit);

        if (importObjectButton != null)
            importObjectButton.SetActive(inside && canEdit);

        if (addShelfButton != null)
            addShelfButton.SetActive(inside && canEdit);

        // Custom Room is owner only.
        if (customRoomButton != null)
            customRoomButton.SetActive(inside && isOwner);
    }

    public void HideBottomBar()
    {
        if (bottomBar != null)
            bottomBar.SetActive(false);
    }
    public void SetPortalRoom(GameObject room) { portalRoom = room; }
    public void RefreshShelfSurfaces(List<GameObject> shelves) { shelfObjects = new List<GameObject>(shelves); }

    public void OnAddObjectButtonPressed()
    {
        if (isVisitorMode || !RoomInsideDetector.IsInsideRoom) return;
        objectListPanel.SetActive(true);
        bottomBar.SetActive(false);
    }

    void PopulateList()
    {
        foreach (Transform child in listContent) Destroy(child.gameObject);
        for (int i = 0; i < objectPrefabs.Length; i++)
        {
            int index = i;
            GameObject item = Instantiate(listItemPrefab, listContent);
            item.GetComponentInChildren<TMP_Text>().text = objectNames[i];
            item.GetComponent<Button>().onClick.AddListener(() => OnObjectSelected(index));
        }
    }

    void OnObjectSelected(int index) { selectedIndex = index; isPlacingMode = true; objectListPanel.SetActive(false); }

    public void SetRuntimeModel(GameObject model, string fileName = "", string filePath = "")
    {
        runtimeModelPrefab = model;
        runtimeModelFileName = fileName;
        runtimeModelFilePath = filePath;
        runtimeModelPrefab.SetActive(false);
        isPlacingMode = true;
        bottomBar.SetActive(false);
    }

    void Update()
    {
        if (doorFramePlacementController.IsRoomRepositioning)
            return;

        var activeTouches = Touch.activeTouches;

        if (isPlacingMode)
        {
            if (activeTouches.Count == 0) return;
            var touch = activeTouches[0];
            if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began) return;
            TryPlaceObject(touch.screenPosition);
            return;
        }

        if (isAdjustingMode && previewObject != null) { HandleAdjustment(activeTouches); return; }

        // FIX: also block tap if settings panel is open
        if (!isPlacingMode && !isAdjustingMode && !isPopupOpen &&
            !(ownerSettings != null && ownerSettings.IsOpen))
        {
            if (activeTouches.Count == 1)
            {
                var touch = activeTouches[0];
                if (touch.phase == UnityEngine.InputSystem.TouchPhase.Began)
                {
                    if (IsAnyBlockingPanelOpen())
                    {
                        Debug.Log("[OBJECT SELECT] Blocked because a panel is open.");
                        return;
                    }

                    TrySelectPlacedObject(touch.screenPosition);
                }
            }
        }
    }

    private bool IsAnyBlockingPanelOpen()
    {
//        roomPositionMenuPanel
//manualMovePanel
//shelfConfirmPanel
//chatPanel
//profilePanel
//requestPanel
//visitorPopupPanel
//customRoomPanel

        if (objectListPanel != null && objectListPanel.activeInHierarchy)
            return true;

        if (confirmPanel != null && confirmPanel.activeInHierarchy)
            return true;

        if (editPopupPanel != null && editPopupPanel.activeInHierarchy)
            return true;

        if (ownerSettings != null && ownerSettings.IsOpen)
            return true;

        return false;
    }

    void TryPlaceObject(Vector2 screenPos)
    {
        Vector3 spawnPos;
        Quaternion spawnRot;
        // 1. Shelf first
        if (TryRaycastShelf(screenPos, out Vector3 shelfPos, out Quaternion shelfRot))
        {
            spawnPos = shelfPos;
            spawnRot = shelfRot;
            SpawnPreview(spawnPos, spawnRot);
            return;
        }
        // 2. Virtual room floor / room surface
        if (TryRaycastRoomSurface(screenPos, out Vector3 roomPos, out Quaternion roomRot))
        {
            spawnPos = roomPos;
            spawnRot = roomRot;
            SpawnPreview(spawnPos, spawnRot);
            return;
        }
        // 3. AR plane fallback
        if (arRaycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            spawnPos = hits[0].pose.position;
            spawnRot = hits[0].pose.rotation;
            spawnPos = ClampToRoom(spawnPos);
            SpawnPreview(spawnPos, spawnRot);
            return;
        }
        // 4. Last fallback in front of camera
        Vector3 fwd = Camera.main.transform.forward;
        fwd.y = 0f;

        if (fwd.sqrMagnitude < 0.01f)
            fwd = Vector3.forward;
        fwd.Normalize();
        spawnPos = Camera.main.transform.position + fwd * 1.5f;
        spawnPos = ClampToRoom(spawnPos);
        spawnRot = Quaternion.LookRotation(fwd, Vector3.up);
        SpawnPreview(spawnPos, spawnRot);
    }

    bool TryRaycastRoomSurface(Vector2 screenPos, out Vector3 hitPos, out Quaternion hitRot)
    {
        hitPos = Vector3.zero;
        hitRot = Quaternion.identity;

        if (portalRoom == null || Camera.main == null)
            return false;

        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        RaycastHit[] allHits = Physics.RaycastAll(ray, 20f);
        System.Array.Sort(allHits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in allHits)
        {
            if (hit.collider == null)
                continue;

            // Only accept surfaces that are inside the current room object
            if (!hit.collider.transform.IsChildOf(portalRoom.transform))
                continue;

            // Ignore wall-like vertical surfaces for floor placement
            // This prevents placing object on wall/ceiling accidentally.
            if (hit.normal.y < 0.5f)
                continue;

            hitPos = hit.point + Vector3.up * 0.02f;

            Vector3 dir = Camera.main.transform.forward;
            dir.y = 0f;

            if (dir.sqrMagnitude < 0.01f)
                dir = portalRoom.transform.forward;

            dir.Normalize();

            hitRot = Quaternion.LookRotation(dir, Vector3.up);
            return true;
        }

        return false;
    }

    bool TryRaycastShelf(Vector2 screenPos, out Vector3 hitPos, out Quaternion hitRot)
    {
        hitPos = Vector3.zero; hitRot = Quaternion.identity;
        if (shelfObjects.Count == 0) return false;
        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, shelfSnapRayDistance, shelfLayerMask))
        {
            hitPos = hit.point;
            Vector3 dir = Camera.main.transform.forward; dir.y = 0; dir.Normalize();
            hitRot = Quaternion.LookRotation(dir, Vector3.up);
            return true;
        }
        return false;
    }

    void SpawnPreview(Vector3 pos, Quaternion rot)
    {
        if (runtimeModelPrefab != null)
        {
            previewObject = runtimeModelPrefab;
            previewObject.transform.position = pos;
            previewObject.transform.rotation = rot;
            previewObject.transform.localScale = Vector3.one * 0.01f;
            previewObject.SetActive(true);
        }
        else previewObject = Instantiate(objectPrefabs[selectedIndex], pos, rot);
        isPlacingMode = false; isAdjustingMode = true; confirmPanel.SetActive(true);
    }

    void HandleAdjustment(IReadOnlyList<Touch> activeTouches)
    {
        if (activeTouches.Count == 1)
        {
            var touch = activeTouches[0];

            if (touch.phase == UnityEngine.InputSystem.TouchPhase.Moved)
            {
                if (TryRaycastShelf(touch.screenPosition, out Vector3 shelfPos, out _))
                {
                    previewObject.transform.position = ClampToRoom(shelfPos);
                }
                else if (TryRaycastRoomSurface(touch.screenPosition, out Vector3 roomPos, out _))
                {
                    previewObject.transform.position = ClampToRoom(roomPos);
                }
                else if (arRaycastManager.Raycast(touch.screenPosition, hits, TrackableType.PlaneWithinPolygon))
                {
                    previewObject.transform.position = ClampToRoom(hits[0].pose.position);
                }
            }
        }
        else if (activeTouches.Count == 2)
        {
            var t1 = activeTouches[0]; var t2 = activeTouches[1];
            float dist = Vector2.Distance(t1.screenPosition, t2.screenPosition);
            float angle = Mathf.Atan2(t2.screenPosition.y - t1.screenPosition.y, t2.screenPosition.x - t1.screenPosition.x) * Mathf.Rad2Deg;
            if (t1.phase == UnityEngine.InputSystem.TouchPhase.Began || t2.phase == UnityEngine.InputSystem.TouchPhase.Began)
            { lastPinchDistance = dist; lastTwoFingerAngle = angle; }
            else
            {
                float scale = Mathf.Clamp(previewObject.transform.localScale.x * (dist / lastPinchDistance), 0.001f, 2f);
                previewObject.transform.localScale = Vector3.one * scale;
                previewObject.transform.Rotate(0f, -(angle - lastTwoFingerAngle), 0f, Space.World);
                lastPinchDistance = dist; lastTwoFingerAngle = angle;
            }
        }
    }

    Vector3 ClampToRoom(Vector3 pos)
    {
        if (portalRoom == null)
            return pos;

        Bounds b = GetRoomBounds();

        float wallPadding = 0.25f;

        pos.x = Mathf.Clamp(pos.x, b.min.x + wallPadding, b.max.x - wallPadding);
        pos.z = Mathf.Clamp(pos.z, b.min.z + wallPadding, b.max.z - wallPadding);

        return pos;
    }

    Bounds GetRoomBounds()
    {
        Renderer[] r = portalRoom.GetComponentsInChildren<Renderer>();
        if (r.Length == 0) return new Bounds(portalRoom.transform.position, Vector3.one * 3f);
        Bounds b = r[0].bounds; foreach (var rr in r) b.Encapsulate(rr.bounds); return b;
    }

    //public void OnConfirmPressed()
    //{
    //    Debug.Log("[CONFIRM] BUTTON PRESSED");
    //    if (previewObject == null) return;
    //    EnsureCollider(previewObject);
    //    if (portalRoom != null) previewObject.transform.SetParent(portalRoom.transform);

    //    string fname = !string.IsNullOrEmpty(runtimeModelFileName) ? runtimeModelFileName
    //        : (selectedIndex >= 0 ? objectNames[selectedIndex] : "unknown");

    //    if (!isEditingExistingObject)
    //    {
    //        AttachMetaData(previewObject, fname);
    //        SpawnIndicator(previewObject);
    //        placedObjects.Add(previewObject);
    //        objectFileNames[previewObject] = fname;
    //    }

    //    Debug.Log("[CONFIRM] Saving object");
    //    string uploadPath = runtimeModelFilePath;

    //    if (isEditingExistingObject)
    //    {
    //        SaveExistingObjectTransform(previewObject);
    //    }
    //    else
    //    {
    //        SaveObjectToRoom(previewObject, fname, uploadPath);
    //    }


    //    if (!isVisitorMode && !isEditingExistingObject)
    //    {
    //        var meta = previewObject.GetComponent<ObjectMetaData>();
    //        GameObject objRef = previewObject;

    //        if (meta != null)
    //            ownerSettings?.Show(meta, () => SaveMetaToRoom(objRef));
    //    }

    //    previewObject = null;
    //    runtimeModelPrefab = null;
    //    runtimeModelFileName = "";
    //    runtimeModelFilePath = "";
    //    isEditingExistingObject = false;
    //    selectedIndex = -1; isAdjustingMode = false;
    //    confirmPanel.SetActive(false);
    //    ShowBottomBar();

    //}

    public void OnConfirmPressed()
    {
        Debug.Log("[CONFIRM] BUTTON PRESSED");
        if (previewObject == null) return;
        EnsureCollider(previewObject);
        if (portalRoom != null) previewObject.transform.SetParent(portalRoom.transform);

        string fname = !string.IsNullOrEmpty(runtimeModelFileName) ? runtimeModelFileName
            : (selectedIndex >= 0 ? objectNames[selectedIndex] : "unknown");

        if (!isEditingExistingObject)
        {
            AttachMetaData(previewObject, fname);
            SpawnIndicator(previewObject);
            placedObjects.Add(previewObject);
            objectFileNames[previewObject] = fname;
        }

        Debug.Log("[CONFIRM] Saving object");
        string uploadPath = runtimeModelFilePath;

        if (isEditingExistingObject)
        {
            SaveExistingObjectTransform(previewObject);
        }
        else
        {
            SaveObjectToRoom(previewObject, fname, uploadPath);
        }

        // FIX: Only show settings popup if the user is actively inside the room placing things!
        if (!isVisitorMode && !isEditingExistingObject && RoomInsideDetector.IsInsideRoom)
        {
            var meta = previewObject.GetComponent<ObjectMetaData>();
            GameObject objRef = previewObject;

            if (meta != null)
                ownerSettings?.Show(meta, () => SaveMetaToRoom(objRef));
        }

        previewObject = null;
        runtimeModelPrefab = null;
        runtimeModelFileName = "";
        runtimeModelFilePath = "";
        isEditingExistingObject = false;
        selectedIndex = -1;
        isAdjustingMode = false;
        confirmPanel.SetActive(false);

        // FIX: Replace unconditional ShowBottomBar() with a smart conditional check
        if (RoomInsideDetector.IsInsideRoom && RoomAccessManager.CanEdit)
        {
            ShowBottomBar();
        }
        else
        {
            HideBottomBar();
        }
    }

    public void OnCancelPressed()
    {
        if (previewObject != null) { Destroy(previewObject); previewObject = null; }
        runtimeModelPrefab = null;
        runtimeModelFileName = "";
        runtimeModelFilePath = "";
        selectedIndex = -1; isAdjustingMode = false; isPlacingMode = false;
        confirmPanel.SetActive(false);
        ShowBottomBar();
    }
    ///
    void SaveObjectToRoom(GameObject obj, string fileName, string uploadPath)
    {
        if (RoomManager.Instance == null || !RoomAccessManager.CanEdit)
            return;

        if (RoomManager.Instance.CurrentRoom == null)
        {
            Debug.LogWarning("[OBJECT SAVE] CurrentRoom is null.");
            return;
        }

        var meta = obj.GetComponent<ObjectMetaData>();

        string roomId = RoomManager.Instance.CurrentRoom.roomId;

        PlacedObjectData placedData = new PlacedObjectData
        {
            instanceId = meta?.instanceId ?? "",
            fileName = fileName,

            posX = obj.transform.localPosition.x,
            posY = obj.transform.localPosition.y,
            posZ = obj.transform.localPosition.z,

            rotX = obj.transform.localRotation.x,
            rotY = obj.transform.localRotation.y,
            rotZ = obj.transform.localRotation.z,
            rotW = obj.transform.localRotation.w,

            scaleX = obj.transform.localScale.x,
            scaleY = obj.transform.localScale.y,
            scaleZ = obj.transform.localScale.z,

            objectName = meta?.objectName ?? fileName,
            description = meta?.description ?? "",
            objectType = (int)(meta?.objectType ?? ObjectMetaData.ObjectType.ViewOnly),
            price = meta?.price ?? 0f,
            currency = meta?.currency ?? "RM"
        };

        // Built-in decoration object
        if (string.IsNullOrEmpty(uploadPath))
        {
            placedData.isBuiltIn = true;
            placedData.prefabKey = fileName;
            placedData.glbUrl = "";
            placedData.storagePath = "";

            RoomManager.Instance.AddObjectToRoom(placedData);
            CloudObjectSaver.SaveObject(roomId, placedData);

            Debug.Log("[BUILT-IN OBJECT] Saved built-in decoration: " + fileName);
            return;
        }

        // Imported GLB object
        placedData.isBuiltIn = false;
        placedData.prefabKey = "";

        Debug.Log("[CONFIRM] Uploading GLB first...");

        CloudModelUploader.UploadGlb(
            roomId,
            placedData.instanceId,
            uploadPath,
            (downloadUrl, storagePath) =>
            {
                if (string.IsNullOrEmpty(downloadUrl))
                {
                    Debug.LogError("[CLOUD MODEL] Upload failed. Object not saved: " + placedData.objectName);
                    return;
                }

                placedData.glbUrl = downloadUrl;
                placedData.storagePath = storagePath;

                RoomManager.Instance.AddObjectToRoom(placedData);
                CloudObjectSaver.SaveObject(roomId, placedData);

                Debug.Log("[CLOUD MODEL] Upload complete and object saved: " + placedData.objectName);
            });
    }

    void SaveMetaToRoom(GameObject obj)
    {
        /////////////////////if (RoomManager.Instance == null || !RoomManager.IsOwner) return;
        ///
        if (RoomManager.Instance == null || !RoomAccessManager.CanEdit) return;

        var meta = obj.GetComponent<ObjectMetaData>();
        if (meta != null) RoomManager.Instance.UpdateObjectMeta(meta);
    }

    void RemoveObjectFromRoom(GameObject obj)
    {
        //////////////////////////if (RoomManager.Instance == null || !RoomManager.IsOwner) return;
        ///
        if (RoomManager.Instance == null || !RoomAccessManager.CanEdit) return;
        var meta = obj.GetComponent<ObjectMetaData>();
        if (meta != null) RoomManager.Instance.RemoveObjectFromRoom(meta.instanceId);
    }

    void EnsureCollider(GameObject obj)
    {
        if (obj.GetComponentInChildren<Collider>() != null) return;
        Renderer[] rends = obj.GetComponentsInChildren<Renderer>();
        if (rends.Length == 0) { obj.AddComponent<BoxCollider>().size = Vector3.one * 0.3f; return; }
        Bounds b = rends[0].bounds; foreach (var r in rends) b.Encapsulate(r.bounds);
        BoxCollider col = obj.AddComponent<BoxCollider>();
        col.center = obj.transform.InverseTransformPoint(b.center);
        Vector3 s = obj.transform.InverseTransformVector(b.size);
        col.size = new Vector3(Mathf.Abs(s.x), Mathf.Abs(s.y), Mathf.Abs(s.z));
    }

    void TrySelectPlacedObject(Vector2 screenPos)
    {
        if (!RoomInsideDetector.IsInsideRoom)
        {
            Debug.LogWarning("[OBJECT SELECT] Cannot select object because IsInsideRoom = false.");
            return;
        }

        Ray ray = Camera.main.ScreenPointToRay(screenPos);
        RaycastHit[] allHits = Physics.RaycastAll(ray, 10f);
        System.Array.Sort(allHits, (a, b) => a.distance.CompareTo(b.distance));

        Debug.Log("[OBJECT SELECT] Hits count: " + allHits.Length);

        foreach (var hit in allHits)
        {
            Debug.Log("[OBJECT SELECT] Hit: " + hit.collider.name);

            GameObject placed = FindPlacedObjectInParents(hit.collider.gameObject);

            if (placed != null)
            {
                Debug.Log("[OBJECT SELECT] Found placed object: " + placed.name);

                if (currentlySelectedObject == placed && isPopupOpen)
                {
                    ClosePopup();
                    currentlySelectedObject = null;
                    return;
                }

                if (isPopupOpen)
                    ClosePopup();

                if (!isVisitorMode)
                    OpenPopup(placed);
                else
                {
                    var meta = placed.GetComponent<ObjectMetaData>();
                    if (meta != null)
                        visitorPopup?.Show(placed, meta);
                }

                return;
            }
        }

        if (isPopupOpen)
        {
            ClosePopup();
            currentlySelectedObject = null;
        }
    }



    GameObject FindPlacedObjectInParents(GameObject obj)
    {
        Transform t = obj.transform;
        while (t != null) { if (placedObjects.Contains(t.gameObject)) return t.gameObject; t = t.parent; }
        return null;
    }

    public void BeginEditExistingObject(GameObject obj)
    {
        if (obj == null) return;

        isEditingExistingObject = true;

        if (portalRoom != null)
            obj.transform.SetParent(null);

        previewObject = obj;
        isAdjustingMode = true;
        isPopupOpen = false;
        currentlySelectedObject = null;

        editPopupPanel.SetActive(false);
        confirmPanel.SetActive(true);
        bottomBar.SetActive(false);

        if (objectFileNames.ContainsKey(obj))
            runtimeModelFileName = objectFileNames[obj];
    }

    void SaveExistingObjectTransform(GameObject obj)
    {
        if (RoomManager.Instance == null || !RoomAccessManager.CanEdit) return;

        var meta = obj.GetComponent<ObjectMetaData>();
        if (meta == null)
        {
            Debug.LogError("[OBJECT EDIT] Missing metadata.");
            return;
        }

        PlacedObjectData existing = RoomManager.Instance.CurrentRoom.objects
            .Find(o => o.instanceId == meta.instanceId);

        if (existing == null)
        {
            Debug.LogError("[OBJECT EDIT] Existing object not found in room data: " + meta.instanceId);
            return;
        }

        existing.posX = obj.transform.localPosition.x;
        existing.posY = obj.transform.localPosition.y;
        existing.posZ = obj.transform.localPosition.z;

        existing.rotX = obj.transform.localRotation.x;
        existing.rotY = obj.transform.localRotation.y;
        existing.rotZ = obj.transform.localRotation.z;
        existing.rotW = obj.transform.localRotation.w;

        existing.scaleX = obj.transform.localScale.x;
        existing.scaleY = obj.transform.localScale.y;
        existing.scaleZ = obj.transform.localScale.z;

        existing.objectName = meta.objectName;
        existing.description = meta.description;
        existing.objectType = (int)meta.objectType;
        existing.price = meta.price;
        existing.currency = meta.currency;

        CloudObjectSaver.SaveObject(
            RoomManager.Instance.CurrentRoom.roomId,
            existing
        );

        RoomManager.Instance.SaveCurrentRoom();

        Debug.Log("[OBJECT EDIT] Transform updated: " + meta.instanceId);


    }

    public void DeleteObject(GameObject obj)
    {
        if (obj == null) return;

        ObjectMetaData meta = obj.GetComponent<ObjectMetaData>();

        if (meta != null)
            meta.DestroyIndicator();

        RemoveObjectFromRoom(obj);
        objectFileNames.Remove(obj);
        placedObjects.Remove(obj);

        currentlySelectedObject = null;
        isPopupOpen = false;

        if (editPopupPanel != null)
            editPopupPanel.SetActive(false);

        Destroy(obj);
    }

    private bool IsPointerOverAnyUI(Vector2 screenPosition)
    {
        if (UnityEngine.EventSystems.EventSystem.current == null)
            return false;

        var eventData = new UnityEngine.EventSystems.PointerEventData(
            UnityEngine.EventSystems.EventSystem.current
        );

        eventData.position = screenPosition;

        var results = new List<UnityEngine.EventSystems.RaycastResult>();

        UnityEngine.EventSystems.EventSystem.current.RaycastAll(eventData, results);

        return results.Count > 0;
    }

    public GameObject GetBuiltInPrefabByKey(string key)
    {
        if (string.IsNullOrEmpty(key))
            return null;

        for (int i = 0; i < objectNames.Length; i++)
        {
            if (i >= objectPrefabs.Length)
                continue;

            if (objectNames[i] == key)
                return objectPrefabs[i];
        }

        Debug.LogWarning("[BUILT-IN OBJECT] Prefab not found for key: " + key);
        return null;
    }
}

