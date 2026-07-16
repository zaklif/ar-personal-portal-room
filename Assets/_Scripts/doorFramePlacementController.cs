

//using UnityEngine;
//using UnityEngine.XR.ARFoundation;
//using UnityEngine.XR.ARSubsystems;
//using System.Collections.Generic;
//using UnityEngine.InputSystem.EnhancedTouch;

//public class doorFramePlacementController : MonoBehaviour
//{
//    [SerializeField] private ARRaycastManager arRaycastManager;
//    [SerializeField] private ARPlaneManager arPlaneManager;

//    [Header("Room Templates")]
//    [SerializeField] private List<RoomTemplateData> roomTemplates = new List<RoomTemplateData>();
//    [SerializeField] private GameObject fallbackRoomPrefab;

//    [Header("Object Placement")]
//    [SerializeField] private ObjectPlacementController objectPlacementController;

//    private GameObject spawnedPortal;
//    private List<ARRaycastHit> hits = new List<ARRaycastHit>();
//    private bool portalPlaced = false;

//    void OnEnable() => EnhancedTouchSupport.Enable();
//    void OnDisable() => EnhancedTouchSupport.Disable();

//    void Update()
//    {
//        if (portalPlaced) return;

//        var activeTouches = UnityEngine.InputSystem.EnhancedTouch.Touch.activeTouches;
//        if (activeTouches.Count == 0) return;
//        var touch = activeTouches[0];
//        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began) return;

//        bool raycastHit = arRaycastManager.Raycast(touch.screenPosition, hits, TrackableType.PlaneWithinPolygon);

//        if (raycastHit)
//            SpawnPortal(hits[0].pose.position, hits[0].pose.rotation);
//        else
//        {
//            Vector3 camPos = Camera.main.transform.position;
//            Vector3 forward = Camera.main.transform.forward; forward.y = 0; forward.Normalize();
//            Vector3 spawnPos = camPos + forward * 2f; spawnPos.y = 0f;
//            SpawnPortal(spawnPos, Quaternion.LookRotation(forward, Vector3.up));
//        }
//    }

//    void SpawnPortal(Vector3 pos, Quaternion rot)
//    {
//        string chosenTemplateName = PlayerPrefs.GetString("ChosenTemplateName", "");
//        RoomTemplateData chosenTemplate = FindTemplate(chosenTemplateName);

//        GameObject roomPrefabToSpawn;
//        Vector3 rotOffset;
//        float heightOffset;

//        if (chosenTemplate != null)
//        {
//            roomPrefabToSpawn = chosenTemplate.roomPrefab;
//            rotOffset = chosenTemplate.prefabRotationOffset;
//            heightOffset = chosenTemplate.spawnHeightOffset;
//        }
//        else
//        {
//            roomPrefabToSpawn = fallbackRoomPrefab;
//            rotOffset = new Vector3(90f, 180f, 0f);
//            heightOffset = 1.0f;
//            Debug.LogWarning($"[PORTAL] Template '{chosenTemplateName}' not found — using fallback.");
//        }

//        if (roomPrefabToSpawn == null) { Debug.LogError("[PORTAL] No room prefab!"); return; }

//        pos.y += heightOffset;
//        spawnedPortal = Instantiate(roomPrefabToSpawn, pos, rot * Quaternion.Euler(rotOffset));
//        portalPlaced = true;

//        // Hide AR planes
//        if (arPlaneManager != null)
//        {
//            arPlaneManager.enabled = false;
//            foreach (var plane in arPlaneManager.trackables)
//                plane.gameObject.SetActive(false);
//        }

//        objectPlacementController.SetPortalRoom(spawnedPortal);

//        // FIX: Pass spawnedPortal to RoomLoader so it can parent objects to it
//        FindFirstObjectByType<RoomLoader>()?.OnRoomPortalPlaced(spawnedPortal);

//        Debug.Log("[PORTAL] SpawnPortal complete: " + spawnedPortal.name);
//    }

//    RoomTemplateData FindTemplate(string name)
//    {
//        if (string.IsNullOrEmpty(name)) return null;
//        foreach (var t in roomTemplates)
//            if (t != null && t.templateName == name) return t;
//        return null;
//    }

//    public void ResetPortal()
//    {
//        if (spawnedPortal != null) Destroy(spawnedPortal);
//        portalPlaced = false;
//        if (arPlaneManager != null) arPlaneManager.enabled = true;
//        RoomInsideDetector.Reset();
//    }
//}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TMPro;

public class doorFramePlacementController : MonoBehaviour
{
    [Header("AR")]
    [SerializeField] private ARRaycastManager arRaycastManager;
    [SerializeField] private ARPlaneManager arPlaneManager;
    [SerializeField] private ARAnchorManager arAnchorManager;

    [Header("Room Templates")]
    [SerializeField] private List<RoomTemplateData> roomTemplates = new List<RoomTemplateData>();
    [SerializeField] private GameObject fallbackRoomPrefab;

    [Header("Object Placement")]
    [SerializeField] private ObjectPlacementController objectPlacementController;

    [Header("Room Position UI")]
    [SerializeField] private GameObject roomPositionMenuPanel;
    [SerializeField] private GameObject scanMoveHintPanel;
    [SerializeField] private GameObject scanMoveConfirmPanel;
    [SerializeField] private GameObject manualMovePanel;
    [SerializeField] private GameObject restartRoomConfirmPanel;

    [Header("Optional UI To Hide During Reposition")]
    [SerializeField] private GameObject bottomDock;
    [SerializeField] private GameObject rightQuickMenu;

    [Header("Manual Movement Settings")]
    [SerializeField] private float horizontalMoveSpeed = 0.35f;
    [SerializeField] private float verticalMoveSpeed = 0.20f;
    [SerializeField] private float rotationSpeed = 45f;

    [Header("Clean AR Scan UI")]
    [SerializeField] private ARReticleController reticleController;
    [SerializeField] private GameObject arScanHintPanel;
    [SerializeField] private TMPro.TMP_Text arScanHintText;

    [Header("UI Before Room Placement")]
    [SerializeField] private GameObject[] hideUntilRoomPlaced;

    /// <summary>
    /// //////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
    /// </summary>
    [Header("Editor / Simulator Debug")]
    [SerializeField] private bool enableDebugFallbackPlacement = true;
    [SerializeField] private KeyCode debugPlaceRoomKey = KeyCode.P;
    [SerializeField] private float debugSpawnDistance = 2.0f;
    [SerializeField] private float debugSpawnHeight = 0.0f;

    private GameObject spawnedPortal;
    private Transform roomAnchorRoot;

    private readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private bool portalPlaced = false;
    private bool scanMoveMode = false;
    private bool waitingForScanConfirm = false;
    private bool manualMoveMode = false;

    private string currentTemplateName = "";
    private RoomTemplateData currentTemplate;
    private Vector3 currentRotationOffset;
    private float currentHeightOffset;

    private Vector3 oldRoomWorldPosition;
    private Quaternion oldRoomWorldRotation;

    private Vector3 stableLocalPosition;
    private Quaternion stableLocalRotation;
    private bool hasStablePose = false;

    private ManualMoveAction currentManualAction = ManualMoveAction.None;

    public static bool IsRoomRepositioning { get; private set; } = false;

    private enum ManualMoveAction
    {
        None,
        Left,
        Right,
        Farther,
        Closer,
        Up,
        Down,
        RotateLeft,
        RotateRight
    }

    private void OnEnable()
    {
        EnhancedTouchSupport.Enable();
    }

    private void OnDisable()
    {
        EnhancedTouchSupport.Disable();
    }

    private void Start()
    {
        CloseAllPositionPanels();

        // Hide owner/visitor action buttons until room is actually placed.
        //SetMainARUIVisible(false);

        if (arPlaneManager != null)
            SetARPlanesVisible(true);

        StartCoroutine(StartInitialScanAfterDelay());
    }

    private IEnumerator StartInitialScanAfterDelay()
    {
        yield return new WaitForSeconds(0.3f);

        if (portalPlaced)
            yield break;

        SetARPlanesVisible(true);

        if (reticleController != null)
            reticleController.ShowReticle();

        if (arScanHintPanel != null)
            arScanHintPanel.SetActive(true);

        if (arScanHintText != null)
            arScanHintText.text = "Move your phone to scan the floor";

        Debug.Log("[PORTAL] Initial clean scan UI started.");
    }

    private void Update()
    {
        if (manualMoveMode)
        {
            ApplyManualMovement();
            return;
        }

#if UNITY_EDITOR
        if (enableDebugFallbackPlacement && !portalPlaced)
        {
            if (Input.GetKeyDown(debugPlaceRoomKey))
            {
                Debug.Log("[DEBUG PLACE] Spawning room in front of camera for simulator testing.");
                DebugSpawnRoomInFrontOfCamera();
                return;
            }
        }
#endif

        if (IsPointerOverUI())
            return;

        if (scanMoveMode && !waitingForScanConfirm)
        {
            HandleScanMoveTouch();
            return;
        }

        if (portalPlaced)
            return;

        HandleInitialPlacementTouch();
    }

#if UNITY_EDITOR
    public void DebugSpawnRoomInFrontOfCamera()
    {
        if (portalPlaced)
        {
            Debug.Log("[DEBUG PLACE] Room already placed.");
            return;
        }

        Camera cam = Camera.main;

        if (cam == null)
        {
            Debug.LogError("[DEBUG PLACE] Camera.main is null. Cannot spawn debug room.");
            return;
        }

        Vector3 forward = cam.transform.forward;
        forward.y = 0f;

        if (forward.sqrMagnitude < 0.01f)
            forward = Vector3.forward;

        forward.Normalize();

        Vector3 spawnPosition = cam.transform.position + forward * debugSpawnDistance;
        spawnPosition.y += debugSpawnHeight;

        Quaternion spawnRotation = Quaternion.LookRotation(forward, Vector3.up);

        SpawnPortal(spawnPosition, spawnRotation);

        Debug.Log("[DEBUG PLACE] Room spawned in front of camera.");
    }
#endif

    private void HandleInitialPlacementTouch()
    {
        UpdateScanHintText();

        var activeTouches = Touch.activeTouches;

        if (activeTouches.Count == 0)
            return;

        var touch = activeTouches[0];

        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began)
            return;

        if (reticleController != null && reticleController.TryGetPlacementPose(out Pose reticlePose))
        {
            SpawnPortal(reticlePose.position, reticlePose.rotation);
            return;
        }

        bool raycastHit = arRaycastManager.Raycast(
            touch.screenPosition,
            hits,
            TrackableType.PlaneWithinPolygon
        );

        if (raycastHit)
        {
            SpawnPortal(hits[0].pose.position, hits[0].pose.rotation);
        }
        else
        {
            Debug.LogWarning("[PORTAL] No floor detected yet. Move phone slowly to scan.");
        }
    }

    private void SpawnPortal(Vector3 planePosition, Quaternion planeRotation)
    {
        PrepareTemplateData();

        GameObject roomPrefabToSpawn;

        if (currentTemplate != null)
        {
            roomPrefabToSpawn = currentTemplate.roomPrefab;
            Debug.Log("[PORTAL] Using template: " + currentTemplate.templateName);
        }
        else
        {
            roomPrefabToSpawn = fallbackRoomPrefab;
            Debug.LogWarning("[PORTAL] Template not found. Using fallback room prefab.");
        }

        if (roomPrefabToSpawn == null)
        {
            Debug.LogError("[PORTAL] No room prefab available.");
            return;
        }

        Vector3 finalPosition = planePosition;
        finalPosition.y += currentHeightOffset;

        Quaternion finalRotation = planeRotation * Quaternion.Euler(currentRotationOffset);

        spawnedPortal = Instantiate(roomPrefabToSpawn);
        AttachRoomToNewAnchor(spawnedPortal, finalPosition, finalRotation);

        portalPlaced = true;
        scanMoveMode = false;
        waitingForScanConfirm = false;
        manualMoveMode = false;
        IsRoomRepositioning = false;

        SetARPlanesVisible(false);
        CloseAllPositionPanels();
        SetMainARUIVisible(true);

        if (reticleController != null)
            reticleController.HideReticle();

        if (arScanHintPanel != null)
            arScanHintPanel.SetActive(false);

        if (objectPlacementController != null)
            objectPlacementController.SetPortalRoom(spawnedPortal);
        else
            Debug.LogError("[PORTAL] ObjectPlacementController is not assigned.");

        RoomLoader roomLoader = FindFirstObjectByType<RoomLoader>();

        if (roomLoader != null)
            roomLoader.OnRoomPortalPlaced(spawnedPortal);

        RoomInsideDetector.Reset();

        SaveCurrentRoomPoseAsStable();

        Debug.Log("[PORTAL] Room spawned and anchored: " + spawnedPortal.name);
    }

    private void PrepareTemplateData()
    {
        currentTemplateName = PlayerPrefs.GetString("ChosenTemplateName", "");

        // Backup: if PlayerPrefs is empty, use RoomManager.CurrentRoom
        if (string.IsNullOrEmpty(currentTemplateName) &&
            RoomManager.Instance != null &&
            RoomManager.Instance.CurrentRoom != null)
        {
            currentTemplateName = RoomManager.Instance.CurrentRoom.roomTemplateName;
            PlayerPrefs.SetString("ChosenTemplateName", currentTemplateName);
            PlayerPrefs.Save();

            Debug.Log("[PORTAL] Template recovered from RoomManager: " + currentTemplateName);
        }

        currentTemplate = FindTemplate(currentTemplateName);

        if (currentTemplate != null)
        {
            currentRotationOffset = currentTemplate.prefabRotationOffset;
            currentHeightOffset = currentTemplate.spawnHeightOffset;
        }
        else
        {
            Debug.LogWarning("[PORTAL] Template still not found: " + currentTemplateName);

            currentRotationOffset = new Vector3(90f, 180f, 0f);
            currentHeightOffset = 1.0f;
        }
    }

    private RoomTemplateData FindTemplate(string templateName)
    {
        if (string.IsNullOrEmpty(templateName))
            return null;

        foreach (var template in roomTemplates)
        {
            if (template != null && template.templateName == templateName)
                return template;
        }

        return null;
    }

    private void AttachRoomToNewAnchor(GameObject room, Vector3 worldPosition, Quaternion worldRotation)
    {
        if (room == null)
            return;

        if (roomAnchorRoot != null)
        {
            room.transform.SetParent(null, true);
            Destroy(roomAnchorRoot.gameObject);
            roomAnchorRoot = null;
        }

        GameObject anchorObject = new GameObject("RoomPlacementAnchor");
        anchorObject.transform.SetPositionAndRotation(worldPosition, worldRotation);

        if (arAnchorManager != null)
        {
            ARAnchor anchor = anchorObject.AddComponent<ARAnchor>();
            Debug.Log("[ANCHOR] ARAnchor added: " + anchor.name);
        }
        else
        {
            Debug.LogWarning("[ANCHOR] ARAnchorManager is not assigned. Using normal transform root.");
        }

        roomAnchorRoot = anchorObject.transform;

        room.transform.SetParent(roomAnchorRoot, false);
        room.transform.localPosition = Vector3.zero;
        room.transform.localRotation = Quaternion.identity;

        RoomInsideDetector.Reset();
    }

    private void ReanchorExistingRoom(Vector3 worldPosition, Quaternion worldRotation)
    {
        if (spawnedPortal == null)
            return;

        AttachRoomToNewAnchor(spawnedPortal, worldPosition, worldRotation);
        SaveCurrentRoomPoseAsStable();

        Debug.Log("[ANCHOR] Room re-anchored.");
    }

    // ─────────────────────────────────────────────────────────────
    // POSITION MENU
    // ─────────────────────────────────────────────────────────────

    public void OpenRoomPositionMenu()
    {
        if (spawnedPortal == null)
        {
            Debug.LogWarning("[POSITION] Room has not been placed yet.");
            return;
        }

        CloseAllPositionPanels();

        if (roomPositionMenuPanel != null)
            roomPositionMenuPanel.SetActive(true);
    }

    public void CloseRoomPositionMenu()
    {
        if (roomPositionMenuPanel != null)
            roomPositionMenuPanel.SetActive(false);
    }

    // ─────────────────────────────────────────────────────────────
    // SCAN FLOOR AGAIN MODE
    // ─────────────────────────────────────────────────────────────

    public void StartScanFloorAgain()
    {
        if (spawnedPortal == null)
        {
            Debug.LogWarning("[SCAN MOVE] No room to move.");
            return;
        }

        SaveOldRoomWorldPose();

        CloseAllPositionPanels();

        scanMoveMode = true;
        waitingForScanConfirm = false;
        manualMoveMode = false;
        currentManualAction = ManualMoveAction.None;
        IsRoomRepositioning = true;

        // We still enable plane detection, but the plane visual should be hidden
        // by disabling MeshRenderer/LineRenderer on the AR plane prefab.
        SetARPlanesVisible(true);

        // Option B: show clean reticle instead of ugly AR plane mesh.
        if (reticleController != null)
            reticleController.ShowReticle();

        if (arScanHintPanel != null)
            arScanHintPanel.SetActive(true);

        if (arScanHintText != null)
            arScanHintText.text = "Move your phone to scan the floor";

        // Hide old prototype hint if you are using the new clean scan UI.
        if (scanMoveHintPanel != null)
            scanMoveHintPanel.SetActive(false);

        // Hide owner/visitor normal controls while choosing new position.
        SetMainARUIVisible(false);

        Debug.Log("[SCAN MOVE] Scan floor again started. Use reticle and tap to reposition.");
    }

    private void HandleScanMoveTouch()
    {
        UpdateScanHintText();

        var activeTouches = Touch.activeTouches;

        if (activeTouches.Count == 0)
            return;

        var touch = activeTouches[0];

        if (touch.phase != UnityEngine.InputSystem.TouchPhase.Began)
            return;

        Pose placementPose;

        if (reticleController != null && reticleController.TryGetPlacementPose(out Pose reticlePose))
        {
            placementPose = reticlePose;
        }
        else
        {
            bool raycastHit = arRaycastManager.Raycast(
                touch.screenPosition,
                hits,
                TrackableType.PlaneWithinPolygon
            );

            if (!raycastHit)
            {
                Debug.Log("[SCAN MOVE] No floor detected yet. Move your phone to scan.");
                return;
            }

            placementPose = hits[0].pose;
        }

        PrepareTemplateData();

        Vector3 finalPosition = placementPose.position;
        finalPosition.y += currentHeightOffset;

        Quaternion finalRotation = placementPose.rotation * Quaternion.Euler(currentRotationOffset);

        ReanchorExistingRoom(finalPosition, finalRotation);

        waitingForScanConfirm = true;

        if (scanMoveHintPanel != null)
            scanMoveHintPanel.SetActive(false);

        if (arScanHintPanel != null)
            arScanHintPanel.SetActive(false);

        if (reticleController != null)
            reticleController.HideReticle();

        if (scanMoveConfirmPanel != null)
            scanMoveConfirmPanel.SetActive(true);

        Debug.Log("[SCAN MOVE] Preview moved. Waiting for confirm/cancel.");
    }

    private void UpdateScanHintText()
    {
        if (arScanHintPanel != null && !arScanHintPanel.activeSelf)
            arScanHintPanel.SetActive(true);

        if (arScanHintText == null)
            return;

        if (reticleController != null && reticleController.HasValidHit)
            arScanHintText.text = "Tap to place room";
        else
            arScanHintText.text = "Move your phone to scan the floor";
    }

    public void ConfirmScanMove()
    {
        ConfirmRoomPosition();
    }

    public void CancelScanMove()
    {
        CancelRoomPosition();
    }

    // ─────────────────────────────────────────────────────────────
    // MANUAL HOLD-TO-MOVE MODE
    // ─────────────────────────────────────────────────────────────

    public void StartManualMoveMode()
    {
        if (spawnedPortal == null)
        {
            Debug.LogWarning("[MANUAL MOVE] No room to move.");
            return;
        }

        SaveOldRoomWorldPose();

        CloseAllPositionPanels();

        manualMoveMode = true;
        scanMoveMode = false;
        waitingForScanConfirm = false;
        currentManualAction = ManualMoveAction.None;
        IsRoomRepositioning = true;

        SetARPlanesVisible(false);
        SetMainARUIVisible(false);

        if (manualMovePanel != null)
            manualMovePanel.SetActive(true);

        Debug.Log("[MANUAL MOVE] Manual move mode started.");
    }

    private void ApplyManualMovement()
    {
        if (spawnedPortal == null)
            return;

        if (currentManualAction == ManualMoveAction.None)
            return;

        Transform cam = Camera.main != null ? Camera.main.transform : null;

        Vector3 forward = Vector3.forward;
        Vector3 right = Vector3.right;

        if (cam != null)
        {
            forward = cam.forward;
            forward.y = 0f;

            if (forward.sqrMagnitude < 0.01f)
                forward = spawnedPortal.transform.forward;

            forward.Normalize();

            right = cam.right;
            right.y = 0f;

            if (right.sqrMagnitude < 0.01f)
                right = spawnedPortal.transform.right;

            right.Normalize();
        }

        float dt = Time.deltaTime;
        Vector3 movement = Vector3.zero;
        float rotationAmount = 0f;

        switch (currentManualAction)
        {
            case ManualMoveAction.Left:
                movement = -right * horizontalMoveSpeed * dt;
                break;

            case ManualMoveAction.Right:
                movement = right * horizontalMoveSpeed * dt;
                break;

            case ManualMoveAction.Farther:
                movement = forward * horizontalMoveSpeed * dt;
                break;

            case ManualMoveAction.Closer:
                movement = -forward * horizontalMoveSpeed * dt;
                break;

            case ManualMoveAction.Up:
                movement = Vector3.up * verticalMoveSpeed * dt;
                break;

            case ManualMoveAction.Down:
                movement = Vector3.down * verticalMoveSpeed * dt;
                break;

            case ManualMoveAction.RotateLeft:
                rotationAmount = -rotationSpeed * dt;
                break;

            case ManualMoveAction.RotateRight:
                rotationAmount = rotationSpeed * dt;
                break;
        }

        if (movement != Vector3.zero)
            spawnedPortal.transform.position += movement;

        if (Mathf.Abs(rotationAmount) > 0.001f)
            spawnedPortal.transform.Rotate(Vector3.up, rotationAmount, Space.World);

        RoomInsideDetector.Reset();
    }

    public void StartMoveLeft()
    {
        currentManualAction = ManualMoveAction.Left;
    }

    public void StartMoveRight()
    {
        currentManualAction = ManualMoveAction.Right;
    }

    public void StartMoveFarther()
    {
        currentManualAction = ManualMoveAction.Farther;
    }

    public void StartMoveForward()
    {
        currentManualAction = ManualMoveAction.Farther;
    }

    public void StartMoveCloser()
    {
        currentManualAction = ManualMoveAction.Closer;
    }

    public void StartMoveBackward()
    {
        currentManualAction = ManualMoveAction.Closer;
    }

    public void StartMoveUp()
    {
        currentManualAction = ManualMoveAction.Up;
    }

    public void StartMoveDown()
    {
        currentManualAction = ManualMoveAction.Down;
    }

    public void StartRotateLeft()
    {
        currentManualAction = ManualMoveAction.RotateLeft;
    }

    public void StartRotateRight()
    {
        currentManualAction = ManualMoveAction.RotateRight;
    }

    public void StopManualMove()
    {
        currentManualAction = ManualMoveAction.None;
    }

    public void ConfirmManualMove()
    {
        ConfirmRoomPosition();
    }

    public void CancelManualMove()
    {
        CancelRoomPosition();
    }

    // ─────────────────────────────────────────────────────────────
    // CONFIRM / CANCEL SHARED
    // ─────────────────────────────────────────────────────────────

    private void ConfirmRoomPosition()
    {
        scanMoveMode = false;
        waitingForScanConfirm = false;
        manualMoveMode = false;
        currentManualAction = ManualMoveAction.None;
        IsRoomRepositioning = false;

        SetARPlanesVisible(false);
        CloseAllPositionPanels();
        SetMainARUIVisible(true);

        if (reticleController != null)
            reticleController.HideReticle();

        if (arScanHintPanel != null)
            arScanHintPanel.SetActive(false);

        SaveCurrentRoomPoseAsStable();
        RoomInsideDetector.Reset();

        Debug.Log("[POSITION] Room position confirmed.");
    }

    private void CancelRoomPosition()
    {
        if (spawnedPortal != null)
        {
            ReanchorExistingRoom(oldRoomWorldPosition, oldRoomWorldRotation);
        }

        scanMoveMode = false;
        waitingForScanConfirm = false;
        manualMoveMode = false;
        currentManualAction = ManualMoveAction.None;
        IsRoomRepositioning = false;

        SetARPlanesVisible(false);
        CloseAllPositionPanels();
        SetMainARUIVisible(true);

        if (reticleController != null)
            reticleController.HideReticle();

        if (arScanHintPanel != null)
            arScanHintPanel.SetActive(false);

        SaveCurrentRoomPoseAsStable();
        RoomInsideDetector.Reset();

        Debug.Log("[POSITION] Room position cancelled. Restored old position.");
    }

    private void SaveOldRoomWorldPose()
    {
        if (spawnedPortal == null)
            return;

        oldRoomWorldPosition = spawnedPortal.transform.position;
        oldRoomWorldRotation = spawnedPortal.transform.rotation;
    }

    // ─────────────────────────────────────────────────────────────
    // RESTART ROOM PLACEMENT
    // ─────────────────────────────────────────────────────────────

    public void OpenRestartRoomConfirm()
    {
        if (spawnedPortal == null)
        {
            Debug.LogWarning("[RESTART] Room has not been placed yet.");
            return;
        }

        CloseAllPositionPanels();

        if (restartRoomConfirmPanel != null)
            restartRoomConfirmPanel.SetActive(true);
    }

    public void CancelRestartRoom()
    {
        if (restartRoomConfirmPanel != null)
            restartRoomConfirmPanel.SetActive(false);

        if (roomPositionMenuPanel != null)
            roomPositionMenuPanel.SetActive(true);
    }

    public void ConfirmRestartRoom()
    {
        if (restartRoomConfirmPanel != null)
            restartRoomConfirmPanel.SetActive(false);

        RestartRoomPlacement();
    }

    public void RestartRoomPlacement()
    {
        if (roomAnchorRoot != null)
        {
            Destroy(roomAnchorRoot.gameObject);
            roomAnchorRoot = null;
        }
        else if (spawnedPortal != null)
        {
            Destroy(spawnedPortal);
        }

        spawnedPortal = null;
        portalPlaced = false;

        scanMoveMode = false;
        waitingForScanConfirm = false;
        manualMoveMode = false;
        currentManualAction = ManualMoveAction.None;
        IsRoomRepositioning = false;

        hasStablePose = false;

        CloseAllPositionPanels();
        SetMainARUIVisible(false);
        SetARPlanesVisible(true);

        if (reticleController != null)
            reticleController.ShowReticle();

        if (arScanHintPanel != null)
            arScanHintPanel.SetActive(true);

        RoomInsideDetector.Reset();

        Debug.Log("[RESTART] Local room placement restarted. Tap a plane to place room again.");
    }

    // Compatibility with your old buttons/scripts
    public void ResetPortal()
    {
        RestartRoomPlacement();
    }

    // ─────────────────────────────────────────────────────────────
    // STABILITY / PAUSE RESUME PROTECTION
    // ─────────────────────────────────────────────────────────────

    public void SaveCurrentRoomPoseAsStable()
    {
        if (spawnedPortal == null)
            return;

        stableLocalPosition = spawnedPortal.transform.localPosition;
        stableLocalRotation = spawnedPortal.transform.localRotation;
        hasStablePose = true;

        Debug.Log("[STABILITY] Stable room pose saved.");
    }

    public void RestoreLastStablePose()
    {
        if (spawnedPortal == null)
            return;

        if (!hasStablePose)
            return;

        if (IsRoomRepositioning)
            return;

        spawnedPortal.transform.localPosition = stableLocalPosition;
        spawnedPortal.transform.localRotation = stableLocalRotation;

        RoomInsideDetector.Reset();

        Debug.Log("[STABILITY] Stable room pose restored.");
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
        {
            if (!IsRoomRepositioning)
                SaveCurrentRoomPoseAsStable();
        }
        else
        {
            StartCoroutine(RestorePoseAfterResume());
        }
    }

    private IEnumerator RestorePoseAfterResume()
    {
        yield return new WaitForSeconds(0.25f);
        RestoreLastStablePose();

        yield return new WaitForSeconds(0.5f);
        RestoreLastStablePose();
    }

    // ─────────────────────────────────────────────────────────────
    // UI / AR PLANE HELPERS
    // ─────────────────────────────────────────────────────────────

    private void SetARPlanesVisible(bool visible)
    {
        if (arPlaneManager == null)
            return;

        arPlaneManager.enabled = visible;

        foreach (var plane in arPlaneManager.trackables)
        {
            if (plane == null)
                continue;

            // Keep plane object active for raycast/tracking,
            // but hide all prototype visuals.
            plane.gameObject.SetActive(true);

            MeshRenderer meshRenderer = plane.GetComponent<MeshRenderer>();
            if (meshRenderer != null)
                meshRenderer.enabled = false;

            LineRenderer lineRenderer = plane.GetComponent<LineRenderer>();
            if (lineRenderer != null)
                lineRenderer.enabled = false;

            ARPlaneMeshVisualizer meshVisualizer = plane.GetComponent<ARPlaneMeshVisualizer>();
            if (meshVisualizer != null)
                meshVisualizer.enabled = false;
        }
    }

    private void SetMainARUIVisible(bool visible)
    {
        if (bottomDock != null)
            bottomDock.SetActive(visible);

        if (rightQuickMenu != null)
            rightQuickMenu.SetActive(visible);
    }

    private void CloseAllPositionPanels()
    {
        if (roomPositionMenuPanel != null)
            roomPositionMenuPanel.SetActive(false);

        if (scanMoveHintPanel != null)
            scanMoveHintPanel.SetActive(false);

        if (scanMoveConfirmPanel != null)
            scanMoveConfirmPanel.SetActive(false);

        if (manualMovePanel != null)
            manualMovePanel.SetActive(false);
        
        if (restartRoomConfirmPanel != null)
            restartRoomConfirmPanel.SetActive(false);

        if (arScanHintPanel != null)
            arScanHintPanel.SetActive(false);
    }

    private bool IsPointerOverUI()
    {
        if (EventSystem.current == null)
            return false;

        if (Touch.activeTouches.Count == 0)
            return false;

        int fingerId = Touch.activeTouches[0].finger.index;
        return EventSystem.current.IsPointerOverGameObject(fingerId);
    }

    public GameObject GetSpawnedPortal()
    {
        return spawnedPortal;
    }
}