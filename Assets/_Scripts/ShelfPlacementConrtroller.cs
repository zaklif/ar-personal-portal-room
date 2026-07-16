


//using UnityEngine;
//using UnityEngine.InputSystem.EnhancedTouch;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine.UI;
//using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

//public class ShelfPlacementController : MonoBehaviour
//{
//    [Header("Shelf Prefab")]
//    [SerializeField] private GameObject shelfPrefab;

//    [Header("UI")]
//    [SerializeField] private GameObject addShelfButton;
//    [SerializeField] private GameObject shelfConfirmPanel;

//    [Header("Wall Detection")]
//    [SerializeField] private LayerMask wallLayerMask;
//    [SerializeField] private float wallOffset = 0.02f;
//    [SerializeField] private float minShelfHeight = 0.5f;
//    [SerializeField] private float maxShelfHeight = 2.0f;

//    [Header("References")]
//    [SerializeField] private ObjectPlacementController objectPlacementController;

//    private bool isPlacingShelf = false;
//    private bool isVisitorMode = false;
//    private GameObject previewShelf = null;
//    private List<GameObject> placedShelves = new List<GameObject>();

//    // Reference to the spawned room — needed for local position saving
//    private GameObject portalRoom = null;

//    void OnEnable() => EnhancedTouchSupport.Enable();
//    void OnDisable() => EnhancedTouchSupport.Disable();

//    void Start()
//    {
//        addShelfButton.SetActive(false);
//        shelfConfirmPanel.SetActive(false);
//    }

//    // Called by doorFramePlacementController after room spawns
//    public void SetPortalRoom(GameObject room)
//    {
//        portalRoom = room;
//        Debug.Log("[SHELF] Portal room set: " + room.name);
//    }

//    public void SetVisitorMode(bool visitor)
//    {
//        isVisitorMode = visitor;
//        if (visitor) addShelfButton.SetActive(false);
//    }

//    // ── Called by RoomInsideDetector ──────────────────────────────────────

//    public void OnEnteredRoom()
//    {
//        if (isVisitorMode) return;
//        addShelfButton.SetActive(true);
//        objectPlacementController.ShowBottomBar();
//        Debug.Log("[SHELF] Inside room — shelf button visible");
//    }

//    public void OnExitedRoom()
//    {
//        addShelfButton.SetActive(false);
//        objectPlacementController.HideBottomBar();
//        if (isPlacingShelf) OnCancelShelf();
//        Debug.Log("[SHELF] Left room — shelf button hidden");
//    }

//    // ── Add Shelf button ──────────────────────────────────────────────────

//    public void OnAddShelfButtonPressed()
//    {
//        Debug.Log("[SHELF] Add shelf button pressed. " +
//                  "isVisitorMode=" + isVisitorMode +
//                  " IsInsideRoom=" + RoomInsideDetector.IsInsideRoom +
//                  " CanEdit=" + RoomAccessManager.CanEdit +
//                  " portalRoom=" + (portalRoom != null ? portalRoom.name : "NULL"));

//        if (isVisitorMode)
//        {
//            Debug.LogWarning("[SHELF] Blocked: visitor mode.");
//            return;
//        }

//        if (!RoomInsideDetector.IsInsideRoom)
//        {
//            Debug.LogWarning("[SHELF] Blocked: not inside room.");
//            return;
//        }

//        if (!RoomAccessManager.CanEdit)
//        {
//            Debug.LogWarning("[SHELF] Blocked: no edit permission.");
//            return;
//        }

//        isPlacingShelf = true;

//        if (addShelfButton != null)
//            addShelfButton.SetActive(false);

//        if (objectPlacementController != null)
//            objectPlacementController.HideBottomBar();

//        Debug.Log("[SHELF] Tap a wall to place shelf.");
//    }

//    // ── Update: wall raycast ──────────────────────────────────────────────

//    void Update()
//    {
//        if (doorFramePlacementController.IsRoomRepositioning)
//            return;

//        if (!isPlacingShelf)
//            return;

//        EnsurePortalRoomReference();

//        var activeTouches = Touch.activeTouches;
//        if (activeTouches.Count == 0)
//            return;

//        var touch = activeTouches[0];

//        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began)
//            return;

//        if (TryRaycastRoomWall(touch.screenPosition, out Vector3 spawnPos, out Quaternion spawnRot))
//        {
//            PlaceShelfPreview(spawnPos, spawnRot);
//        }
//        else
//        {
//            Debug.LogWarning("[SHELF] No valid room wall hit. Check modular room wall colliders.");
//        }
//    }

//    void PlaceShelfPreview(Vector3 pos, Quaternion rot)
//    {
//        if (previewShelf != null)
//            Destroy(previewShelf);

//        previewShelf = Instantiate(shelfPrefab, pos, rot);

//        // ADD THIS BACK
//        previewShelf.transform.localScale = Vector3.one * 0.3f;

//        shelfConfirmPanel.SetActive(true);

//        Debug.Log($"[SHELF] Preview at {pos}");
//    }

//    // ── Confirm / Cancel ──────────────────────────────────────────────────

//    public void OnConfirmShelf()
//    {
//        if (previewShelf == null)
//            return;

//        if (portalRoom != null)
//            previewShelf.transform.SetParent(portalRoom.transform);

//        placedShelves.Add(previewShelf);

//        SaveShelfToRoom(previewShelf);

//        previewShelf = null;
//        isPlacingShelf = false;

//        if (shelfConfirmPanel != null)
//            shelfConfirmPanel.SetActive(false);

//        if (addShelfButton != null)
//            addShelfButton.SetActive(RoomInsideDetector.IsInsideRoom && !isVisitorMode && RoomAccessManager.CanEdit);

//        if (objectPlacementController != null)
//            objectPlacementController.ShowBottomBar();

//        if (objectPlacementController != null)
//        {
//            objectPlacementController.RefreshShelfSurfaces(placedShelves);
//            objectPlacementController.ShowBottomBar();
//        }

//        Debug.Log($"[SHELF] Confirmed. Total: {placedShelves.Count}");
//    }

//    public void OnCancelShelf()
//    {
//        if (previewShelf != null)
//        {
//            Destroy(previewShelf);
//            previewShelf = null;
//        }

//        isPlacingShelf = false;

//        if (shelfConfirmPanel != null)
//            shelfConfirmPanel.SetActive(false);

//        if (addShelfButton != null)
//            addShelfButton.SetActive(RoomInsideDetector.IsInsideRoom && !isVisitorMode && RoomAccessManager.CanEdit);

//        if (objectPlacementController != null)
//            objectPlacementController.ShowBottomBar();

//        Debug.Log("[SHELF] Cancelled.");
//    }

//    // ── Save shelf to RoomManager ─────────────────────────────────────────

//    void SaveShelfToRoom(GameObject shelf)
//    {
//        if (RoomManager.Instance == null || !RoomAccessManager.CanEdit)
//            return;

//        Vector3 localPos = shelf.transform.localPosition;
//        Quaternion localRot = shelf.transform.localRotation;
//        Vector3 localScale = shelf.transform.localScale;

//        RoomManager.Instance.AddShelfToRoom(new PlacedShelfData
//        {
//            posX = localPos.x,
//            posY = localPos.y,
//            posZ = localPos.z,
//            rotX = localRot.x,
//            rotY = localRot.y,
//            rotZ = localRot.z,
//            rotW = localRot.w,
//            scaleX = localScale.x,
//            scaleY = localScale.y,
//            scaleZ = localScale.z
//        });

//        RoomManager.Instance.SaveCurrentRoom();

//        Debug.Log($"[SHELF] Saved local pos={localPos}");
//    }

//    // ── Restore shelves (called by RoomLoader) ────────────────────────────

//    public void RestoreShelves(List<PlacedShelfData> shelfDataList, GameObject room)
//    {
//        if (shelfDataList == null || shelfDataList.Count == 0) return;

//        portalRoom = room;

//        foreach (var data in shelfDataList)
//        {
//            GameObject shelf = Instantiate(shelfPrefab);

//            // Parent to room then apply local position
//            shelf.transform.SetParent(room.transform);
//            shelf.transform.localPosition = new Vector3(data.posX, data.posY, data.posZ);
//            shelf.transform.localRotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);
//            shelf.transform.localScale = new Vector3(data.scaleX, data.scaleY, data.scaleZ);

//            placedShelves.Add(shelf);
//            Debug.Log($"[SHELF] Restored at local pos={shelf.transform.localPosition}");
//        }

//        // Tell ObjectPlacementController shelves exist
//        objectPlacementController.RefreshShelfSurfaces(placedShelves);
//        Debug.Log($"[SHELF] {placedShelves.Count} shelf(ves) restored.");
//    }

//    public void RemoveShelf(GameObject shelf)
//    {
//        placedShelves.Remove(shelf);
//        Destroy(shelf);
//        objectPlacementController.RefreshShelfSurfaces(placedShelves);
//    }

//    private void EnsurePortalRoomReference()
//    {
//        if (portalRoom != null)
//            return;

//        doorFramePlacementController placement =
//            FindFirstObjectByType<doorFramePlacementController>();

//        if (placement != null)
//        {
//            portalRoom = placement.GetSpawnedPortal();

//            if (portalRoom != null)
//                Debug.Log("[SHELF] Auto-found portal room: " + portalRoom.name);
//        }
//    }

//    private bool TryRaycastRoomWall(Vector2 screenPos, out Vector3 spawnPos, out Quaternion spawnRot)
//    {
//        spawnPos = Vector3.zero;
//        spawnRot = Quaternion.identity;

//        if (portalRoom == null)
//        {
//            Debug.LogWarning("[SHELF] portalRoom is NULL. Cannot detect wall.");
//            return false;
//        }

//        if (Camera.main == null)
//        {
//            Debug.LogWarning("[SHELF] Camera.main is NULL.");
//            return false;
//        }

//        Ray ray = Camera.main.ScreenPointToRay(screenPos);

//        RaycastHit[] allHits = Physics.RaycastAll(ray, 20f, ~0, QueryTriggerInteraction.Ignore);
//        System.Array.Sort(allHits, (a, b) => a.distance.CompareTo(b.distance));

//        foreach (RaycastHit hit in allHits)
//        {
//            if (hit.collider == null)
//                continue;

//            // Only accept colliders that belong to the current room
//            if (!hit.collider.transform.IsChildOf(portalRoom.transform))
//                continue;

//            Vector3 normal = hit.normal;

//            // Reject floor and ceiling. We only want walls.
//            if (Mathf.Abs(normal.y) > 0.7f)
//            {
//                Debug.Log("[SHELF] Hit floor/ceiling, not wall: " + hit.collider.name);
//                continue;
//            }

//            spawnPos = hit.point + normal * wallOffset;
//            spawnPos.y = Mathf.Clamp(spawnPos.y, minShelfHeight, maxShelfHeight);

//            float wallAngle = Mathf.Atan2(-normal.x, -normal.z) * Mathf.Rad2Deg;
//            spawnRot = Quaternion.Euler(-90f, wallAngle + 90f, 90f);

//            Debug.Log("[OBJECT] Hit room surface: " + hit.collider.name);

//            Debug.Log("[SHELF] Valid wall hit: " + hit.collider.name);
//            return true;

//        }

//        return false;
//    }
//}




using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch;
using System.Collections.Generic;
using TMPro;
using UnityEngine.UI;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;

public class ShelfPlacementController : MonoBehaviour
{
    [Header("Shelf Prefab")]
    [SerializeField] private GameObject shelfPrefab;

    [Header("UI")]
    [SerializeField] private GameObject addShelfButton;
    [SerializeField] private GameObject shelfConfirmPanel;

    [Header("Wall Detection")]
    [SerializeField] private LayerMask wallLayerMask;
    [SerializeField] private float wallOffset = 0.02f;
    [SerializeField] private float minShelfHeight = 0.5f;
    [SerializeField] private float maxShelfHeight = 2.0f;

    [Header("References")]
    [SerializeField] private ObjectPlacementController objectPlacementController;

    [Header("Shelf Delete UI")]
    [SerializeField] private GameObject shelfDeletePanel;

    private bool isPlacingShelf = false;
    private bool isVisitorMode = false;
    private GameObject previewShelf = null;
    private List<GameObject> placedShelves = new List<GameObject>();

    private GameObject selectedShelf = null;

    // Reference to the spawned room — needed for local position saving
    private GameObject portalRoom = null;

    void OnEnable() => EnhancedTouchSupport.Enable();
    void OnDisable() => EnhancedTouchSupport.Disable();

    void Start()
    {
        addShelfButton.SetActive(false);
        shelfConfirmPanel.SetActive(false);

        if (shelfDeletePanel != null)
            shelfDeletePanel.SetActive(false);
    }

    // Called by doorFramePlacementController after room spawns
    public void SetPortalRoom(GameObject room)
    {
        portalRoom = room;
        Debug.Log("[SHELF] Portal room set: " + room.name);
    }

    public void SetVisitorMode(bool visitor)
    {
        isVisitorMode = visitor;
        if (visitor) addShelfButton.SetActive(false);
    }

    // ── Called by RoomInsideDetector ──────────────────────────────────────

    //public void OnEnteredRoom()
    //{
    //    bool canEdit = RoomAccessManager.CanEdit;

    //    if (addShelfButton != null)
    //        addShelfButton.SetActive(canEdit);

    //    if (objectPlacementController != null)
    //    {
    //        objectPlacementController.ShowBottomBar();
    //        objectPlacementController.RefreshBottomDockButtons();
    //    }

    //    Debug.Log("[SHELF] Inside room. CanEdit=" + canEdit);
    //}
    public void OnEnteredRoom()
    {
        bool canEdit = RoomAccessManager.CanEdit;

        if (addShelfButton != null)
            addShelfButton.SetActive(RoomInsideDetector.IsInsideRoom && canEdit);

        if (objectPlacementController != null)
            objectPlacementController.ShowBottomBar();

        Debug.Log("[SHELF] Inside room. CanEdit=" + canEdit);
    }


    public void OnExitedRoom()
    {
        if (addShelfButton != null)
            addShelfButton.SetActive(false);

        if (objectPlacementController != null)
            objectPlacementController.HideBottomBar();

        if (isPlacingShelf)
            OnCancelShelf();

        Debug.Log("[SHELF] Left room — shelf button hidden");
    }

    // ── Add Shelf button ──────────────────────────────────────────────────

    public void OnAddShelfButtonPressed()
    {
        Debug.Log("[SHELF] Add shelf button pressed. " +
                  "isVisitorMode=" + isVisitorMode +
                  " IsInsideRoom=" + RoomInsideDetector.IsInsideRoom +
                  " CanEdit=" + RoomAccessManager.CanEdit +
                  " portalRoom=" + (portalRoom != null ? portalRoom.name : "NULL"));

        if (isVisitorMode)
        {
            Debug.LogWarning("[SHELF] Blocked: visitor mode.");
            return;
        }

        if (!RoomInsideDetector.IsInsideRoom)
        {
            Debug.LogWarning("[SHELF] Blocked: not inside room.");
            return;
        }

        if (!RoomAccessManager.CanEdit)
        {
            Debug.LogWarning("[SHELF] Blocked: no edit permission.");
            return;
        }

        isPlacingShelf = true;

        if (addShelfButton != null)
            addShelfButton.SetActive(false);

        if (objectPlacementController != null)
            objectPlacementController.HideBottomBar();

        Debug.Log("[SHELF] Tap a wall to place shelf.");
    }

    // ── Update: wall raycast ──────────────────────────────────────────────

    void Update()
    {
        if (doorFramePlacementController.IsRoomRepositioning)
            return;

        if (!isPlacingShelf)
        {
            HandleShelfSelectionTouch();
            return;
        }


        EnsurePortalRoomReference();

        var activeTouches = Touch.activeTouches;
        if (activeTouches.Count == 0)
            return;

        var touch = activeTouches[0];

        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began)
            return;

        if (TryRaycastRoomWall(touch.screenPosition, out Vector3 spawnPos, out Quaternion spawnRot))
        {
            PlaceShelfPreview(spawnPos, spawnRot);
        }
        else
        {
            Debug.LogWarning("[SHELF] No valid room wall hit. Check modular room wall colliders.");
        }
    }

    private void HandleShelfSelectionTouch()
    {
        if (isVisitorMode)
            return;

        if (!RoomInsideDetector.IsInsideRoom)
            return;

        if (!RoomAccessManager.CanEdit)
            return;

        if (placedShelves == null || placedShelves.Count == 0)
            return;

        var activeTouches = Touch.activeTouches;

        if (activeTouches.Count == 0)
            return;

        var touch = activeTouches[0];

        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began)
            return;

        Ray ray = Camera.main.ScreenPointToRay(touch.screenPosition);

        RaycastHit[] hits = Physics.RaycastAll(ray, 20f, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in hits)
        {
            GameObject shelf = FindShelfInParents(hit.collider.gameObject);

            if (shelf != null)
            {
                selectedShelf = shelf;

                if (shelfDeletePanel != null)
                    shelfDeletePanel.SetActive(true);

                if (objectPlacementController != null)
                    objectPlacementController.HideBottomBar();

                Debug.Log("[SHELF] Selected shelf for delete: " + shelf.name);
                return;
            }
        }
    }

    public void OnDeleteSelectedShelf()
    {
        if (selectedShelf == null)
        {
            Debug.LogWarning("[SHELF] No selected shelf to delete.");
            return;
        }

        RemoveShelf(selectedShelf);

        selectedShelf = null;

        if (shelfDeletePanel != null)
            shelfDeletePanel.SetActive(false);

        if (objectPlacementController != null)
            objectPlacementController.ShowBottomBar();
    }

    public void OnCancelDeleteShelf()
    {
        selectedShelf = null;

        if (shelfDeletePanel != null)
            shelfDeletePanel.SetActive(false);

        if (objectPlacementController != null)
            objectPlacementController.ShowBottomBar();

        Debug.Log("[SHELF] Delete cancelled.");
    }

    private GameObject FindShelfInParents(GameObject obj)
    {
        Transform t = obj.transform;

        while (t != null)
        {
            if (placedShelves.Contains(t.gameObject))
                return t.gameObject;

            t = t.parent;
        }

        return null;
    }

    void PlaceShelfPreview(Vector3 pos, Quaternion rot)
    {
        if (previewShelf != null)
            Destroy(previewShelf);

        previewShelf = Instantiate(shelfPrefab, pos, rot);

        // ADD THIS BACK
        previewShelf.transform.localScale = Vector3.one * 0.3f;

        shelfConfirmPanel.SetActive(true);

        Debug.Log($"[SHELF] Preview at {pos}");
    }

    // ── Confirm / Cancel ──────────────────────────────────────────────────

    public void OnConfirmShelf()
    {
        if (previewShelf == null)
            return;

        if (portalRoom != null)
            previewShelf.transform.SetParent(portalRoom.transform);

        placedShelves.Add(previewShelf);

        SaveShelfToRoom(previewShelf);

        previewShelf = null;
        isPlacingShelf = false;

        if (shelfConfirmPanel != null)
            shelfConfirmPanel.SetActive(false);

        if (addShelfButton != null)
            addShelfButton.SetActive(RoomInsideDetector.IsInsideRoom && !isVisitorMode && RoomAccessManager.CanEdit);

        if (objectPlacementController != null)
            objectPlacementController.ShowBottomBar();

        if (objectPlacementController != null)
        {
            objectPlacementController.RefreshShelfSurfaces(placedShelves);
            objectPlacementController.ShowBottomBar();
        }

        Debug.Log($"[SHELF] Confirmed. Total: {placedShelves.Count}");
    }

    public void OnCancelShelf()
    {
        if (previewShelf != null)
        {
            Destroy(previewShelf);
            previewShelf = null;
        }

        isPlacingShelf = false;

        if (shelfConfirmPanel != null)
            shelfConfirmPanel.SetActive(false);

        if (addShelfButton != null)
            addShelfButton.SetActive(RoomInsideDetector.IsInsideRoom && !isVisitorMode && RoomAccessManager.CanEdit);

        if (objectPlacementController != null)
            objectPlacementController.ShowBottomBar();

        Debug.Log("[SHELF] Cancelled.");
    }

    // ── Save shelf to RoomManager ─────────────────────────────────────────

    void SaveShelfToRoom(GameObject shelf)
    {
        if (RoomManager.Instance == null)
        {
            Debug.LogWarning("[SHELF CLOUD] Save skipped: RoomManager null");
            return;
        }

        if (RoomManager.Instance.CurrentRoom == null)
        {
            Debug.LogWarning("[SHELF CLOUD] Save skipped: CurrentRoom null");
            return;
        }

        if (!RoomAccessManager.CanEdit)
        {
            Debug.LogWarning("[SHELF CLOUD] Save skipped: no edit permission");
            return;
        }

        Vector3 localPos = shelf.transform.localPosition;
        Quaternion localRot = shelf.transform.localRotation;
        Vector3 localScale = shelf.transform.localScale;

        PlacedShelfData shelfData = new PlacedShelfData
        {
            shelfId = System.Guid.NewGuid().ToString(),

            posX = localPos.x,
            posY = localPos.y,
            posZ = localPos.z,

            rotX = localRot.x,
            rotY = localRot.y,
            rotZ = localRot.z,
            rotW = localRot.w,

            scaleX = localScale.x,
            scaleY = localScale.y,
            scaleZ = localScale.z
        };

        RoomManager.Instance.AddShelfToRoom(shelfData);
        RoomManager.Instance.SaveCurrentRoom();

        CloudShelfSaver.SaveShelf(RoomManager.Instance.CurrentRoom.roomId, shelfData);

        ShelfMetaData meta = shelf.GetComponent<ShelfMetaData>();
        if (meta == null)
            meta = shelf.AddComponent<ShelfMetaData>();

        meta.shelfId = shelfData.shelfId;

        Debug.Log("[SHELF CLOUD] Shelf saved to cloud. " +
                  "shelfId=" + shelfData.shelfId +
                  " localPos=" + localPos);
    }

    // ── Restore shelves (called by RoomLoader) ────────────────────────────

    public void RestoreShelves(List<PlacedShelfData> shelfDataList, GameObject room)
    {
        if (shelfDataList == null || shelfDataList.Count == 0) return;

        portalRoom = room;

        foreach (var data in shelfDataList)
        {
            GameObject shelf = Instantiate(shelfPrefab);

            // Parent to room then apply local position
            shelf.transform.SetParent(room.transform);
            shelf.transform.localPosition = new Vector3(data.posX, data.posY, data.posZ);
            shelf.transform.localRotation = new Quaternion(data.rotX, data.rotY, data.rotZ, data.rotW);
            shelf.transform.localScale = new Vector3(data.scaleX, data.scaleY, data.scaleZ);

            ShelfMetaData meta = shelf.GetComponent<ShelfMetaData>();
            if (meta == null)
                meta = shelf.AddComponent<ShelfMetaData>();

            meta.shelfId = data.shelfId;

            placedShelves.Add(shelf);
            Debug.Log($"[SHELF] Restored at local pos={shelf.transform.localPosition}");
        }

        // Tell ObjectPlacementController shelves exist
        objectPlacementController.RefreshShelfSurfaces(placedShelves);
        Debug.Log($"[SHELF] {placedShelves.Count} shelf(ves) restored.");
    }

    public void RemoveShelf(GameObject shelf)
    {
        if (shelf == null)
            return;

        ShelfMetaData meta = shelf.GetComponent<ShelfMetaData>();

        string shelfId = meta != null ? meta.shelfId : "";

        placedShelves.Remove(shelf);

        if (!string.IsNullOrEmpty(shelfId) && RoomManager.Instance != null)
        {
            RoomManager.Instance.RemoveShelfById(shelfId);
        }
        else
        {
            Debug.LogWarning("[SHELF] Removed locally, but shelfId missing. Cloud delete skipped.");
        }

        Destroy(shelf);

        if (objectPlacementController != null)
        {
            objectPlacementController.RefreshShelfSurfaces(placedShelves);
            objectPlacementController.ShowBottomBar();
        }

        Debug.Log("[SHELF] Deleted shelf id=" + shelfId);
    }

    private void EnsurePortalRoomReference()
    {
        if (portalRoom != null)
            return;

        doorFramePlacementController placement =
            FindFirstObjectByType<doorFramePlacementController>();

        if (placement != null)
        {
            portalRoom = placement.GetSpawnedPortal();

            if (portalRoom != null)
                Debug.Log("[SHELF] Auto-found portal room: " + portalRoom.name);
        }
    }

    private bool TryRaycastRoomWall(Vector2 screenPos, out Vector3 spawnPos, out Quaternion spawnRot)
    {
        spawnPos = Vector3.zero;
        spawnRot = Quaternion.identity;

        if (portalRoom == null)
        {
            Debug.LogWarning("[SHELF] portalRoom is NULL. Cannot detect wall.");
            return false;
        }

        if (Camera.main == null)
        {
            Debug.LogWarning("[SHELF] Camera.main is NULL.");
            return false;
        }

        Ray ray = Camera.main.ScreenPointToRay(screenPos);

        RaycastHit[] allHits = Physics.RaycastAll(ray, 20f, ~0, QueryTriggerInteraction.Ignore);
        System.Array.Sort(allHits, (a, b) => a.distance.CompareTo(b.distance));

        foreach (RaycastHit hit in allHits)
        {
            if (hit.collider == null)
                continue;

            // Only accept colliders that belong to the current room
            if (!hit.collider.transform.IsChildOf(portalRoom.transform))
                continue;

            Vector3 normal = hit.normal;

            // Reject floor and ceiling. We only want walls.
            if (Mathf.Abs(normal.y) > 0.7f)
            {
                Debug.Log("[SHELF] Hit floor/ceiling, not wall: " + hit.collider.name);
                continue;
            }

            spawnPos = hit.point + normal * wallOffset;
            spawnPos.y = Mathf.Clamp(spawnPos.y, minShelfHeight, maxShelfHeight);

            float wallAngle = Mathf.Atan2(-normal.x, -normal.z) * Mathf.Rad2Deg;
            spawnRot = Quaternion.Euler(-90f, wallAngle + 90f, 90f);

            Debug.Log("[OBJECT] Hit room surface: " + hit.collider.name);

            Debug.Log("[SHELF] Valid wall hit: " + hit.collider.name);
            return true;

        }

        return false;
    }
}