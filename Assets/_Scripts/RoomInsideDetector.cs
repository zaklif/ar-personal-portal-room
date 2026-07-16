//using UnityEngine;
//using UnityEngine.Events;
//using TMPro;

///// <summary>
///// DEBUG VERSION - shows on screen what is happening.
///// Replace with clean version once confirmed working.
///// </summary>
//public class RoomInsideDetector : MonoBehaviour
//{
//    [Header("Settings")]
//    [SerializeField] private string cameraTag = "MainCamera";

//    [Header("Events")]
//    public UnityEvent onPlayerEntered;
//    public UnityEvent onPlayerExited;

//    [Header("DEBUG — assign a TMP Text on your Canvas to see status on phone")]
//    [SerializeField] private TMP_Text debugText;

//    public static bool IsInsideRoom { get; private set; } = false;

//    void Start()
//    {
//        // ── Check 1: Is the collider set up? ──────────────────────────────
//        Collider col = GetComponent<Collider>();
//        if (col == null)
//        {
//            Debug.LogError("[ROOM] NO COLLIDER on this GameObject! Add a Box Collider with Is Trigger ticked.");
//            SetDebugText("NO COLLIDER!");
//            return;
//        }

//        if (!col.isTrigger)
//        {
//            Debug.LogError("[ROOM] Collider IS NOT a trigger! Tick 'Is Trigger' on the Box Collider.");
//            SetDebugText("NOT A TRIGGER!");
//            return;
//        }

//        Debug.Log($"[ROOM] Collider OK — size={col.bounds.size}");

//        // ── Check 2: Can we find the camera? ──────────────────────────────
//        GameObject cam = GameObject.FindWithTag(cameraTag);
//        if (cam == null)
//        {
//            Debug.LogError($"[ROOM] No GameObject with tag '{cameraTag}' found! " +
//                           "Go to XR Origin > Camera Offset > Main Camera and check its Tag at the top of Inspector.");
//            SetDebugText($"NO CAM TAG '{cameraTag}'");
//            return;
//        }

//        Debug.Log($"[ROOM] Camera found: '{cam.name}' tag='{cam.tag}'");

//        // ── Fix 3: Camera needs a Collider for trigger detection ───────────
//        Collider camCol = cam.GetComponent<Collider>();
//        if (camCol == null)
//        {
//            // Auto-add a tiny sphere collider to the camera
//            SphereCollider sc = cam.AddComponent<SphereCollider>();
//            sc.radius = 0.1f;
//            sc.isTrigger = false; // must NOT be trigger — room collider is the trigger
//            Debug.Log("[ROOM] Auto-added Sphere Collider to camera.");
//            SetDebugText("Added collider to camera");
//        }

//        // ── Fix 4: One of the objects needs a Rigidbody for trigger to fire ─
//        // Best practice: put Rigidbody on the camera (kinematic)
//        Rigidbody rb = cam.GetComponent<Rigidbody>();
//        if (rb == null)
//        {
//            Rigidbody newRb = cam.AddComponent<Rigidbody>();
//            newRb.isKinematic = true;  // kinematic = physics won't move it
//            newRb.useGravity = false;
//            Debug.Log("[ROOM] Auto-added kinematic Rigidbody to camera.");
//        }

//        SetDebugText("Ready — walk into room!");
//        Debug.Log("[ROOM] Setup complete. Walk into the room trigger.");
//    }

//    private void OnTriggerEnter(Collider other)
//    {
//        // This logs ANYTHING that enters — helps debug what tag is being detected
//        Debug.Log($"[ROOM] OnTriggerEnter — object='{other.gameObject.name}' tag='{other.tag}'");

//        if (!other.CompareTag(cameraTag))
//        {
//            Debug.Log($"[ROOM] Ignored — expected tag '{cameraTag}' but got '{other.tag}'");
//            return;
//        }

//        IsInsideRoom = true;
//        Debug.Log("[ROOM] INSIDE ROOM — editing unlocked!");
//        SetDebugText("INSIDE ROOM");
//        onPlayerEntered?.Invoke();
//    }

//    private void OnTriggerExit(Collider other)
//    {
//        Debug.Log($"[ROOM] OnTriggerExit — object='{other.gameObject.name}' tag='{other.tag}'");

//        if (!other.CompareTag(cameraTag))
//            return;

//        IsInsideRoom = false;
//        Debug.Log("[ROOM] OUTSIDE ROOM — editing locked.");
//        SetDebugText("OUTSIDE ROOM");
//        onPlayerExited?.Invoke();
//    }

//    void SetDebugText(string msg)
//    {
//        if (debugText != null)
//            debugText.text = $"[ROOM] {msg}";
//    }

//    public static void Reset()
//    {
//        IsInsideRoom = false;
//    }
//}
using UnityEngine;
using TMPro;

public class RoomInsideDetector : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private string cameraTag = "MainCamera";

    [Header("DEBUG — optional, assign a TMP Text to see status on phone")]
    [SerializeField] private TMP_Text debugText;

    public static bool IsInsideRoom { get; private set; } = false;

    private ShelfPlacementController shelfController;
    private ObjectPlacementController objectPlacementController;
    private GameObject arCamera;
    private BoxCollider roomTrigger;

    private bool lastInsideState = false;
    private float checkTimer = 0f;
    private const float checkInterval = 0.15f;

    void Start()
    {
        shelfController = FindFirstObjectByType<ShelfPlacementController>();
        if (shelfController == null)
            Debug.LogError("[ROOM] ShelfPlacementController not found!");
        else
            Debug.Log("[ROOM] ShelfPlacementController found automatically.");

        objectPlacementController = FindFirstObjectByType<ObjectPlacementController>();
        if (objectPlacementController == null)
            Debug.LogError("[ROOM] ObjectPlacementController not found!");
        else
            Debug.Log("[ROOM] ObjectPlacementController found automatically.");

        arCamera = GameObject.FindWithTag(cameraTag);
        if (arCamera == null)
        {
            Debug.LogError($"[ROOM] No camera with tag '{cameraTag}' found!");
            SetDebugText($"NO CAM TAG '{cameraTag}'");
            return;
        }

        Debug.Log($"[ROOM] Camera found: '{arCamera.name}'");

        if (arCamera.GetComponent<Collider>() == null)
        {
            SphereCollider sc = arCamera.AddComponent<SphereCollider>();
            sc.radius = 0.1f;
            sc.isTrigger = false;
            Debug.Log("[ROOM] Auto-added Sphere Collider to camera.");
        }

        if (arCamera.GetComponent<Rigidbody>() == null)
        {
            Rigidbody rb = arCamera.AddComponent<Rigidbody>();
            rb.isKinematic = true;
            rb.useGravity = false;
            Debug.Log("[ROOM] Auto-added kinematic Rigidbody to camera.");
        }

        roomTrigger = GetComponent<BoxCollider>();

        if (roomTrigger == null)
        {
            Debug.LogError("[ROOM] No Box Collider on room prefab root!");
            SetDebugText("NO ROOM TRIGGER!");
            return;
        }

        if (!roomTrigger.isTrigger)
        {
            Debug.LogWarning("[ROOM] Root Box Collider was not trigger. Fixing automatically.");
            roomTrigger.isTrigger = true;
        }

        ForceRefreshInsideState();

        SetDebugText("Ready — walk into room!");
        Debug.Log("[ROOM] Setup complete!");
    }

    void Update()
    {
        checkTimer += Time.deltaTime;

        if (checkTimer < checkInterval)
            return;

        checkTimer = 0f;
        ForceRefreshInsideState();
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"[ROOM] OnTriggerEnter — '{other.gameObject.name}' tag='{other.tag}'");

        if (!other.CompareTag(cameraTag))
            return;

        SetInsideState(true, "trigger enter");
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log($"[ROOM] OnTriggerExit — '{other.gameObject.name}' tag='{other.tag}'");

        if (!other.CompareTag(cameraTag))
            return;

        ForceRefreshInsideState();
    }

    public void ForceRefreshInsideState()
    {
        if (arCamera == null || roomTrigger == null)
            return;

        bool inside = IsPointInsideBoxCollider(roomTrigger, arCamera.transform.position);

        SetInsideState(inside, "bounds check");
    }

    private bool IsPointInsideBoxCollider(BoxCollider box, Vector3 worldPoint)
    {
        Vector3 localPoint = box.transform.InverseTransformPoint(worldPoint) - box.center;

        Vector3 halfSize = box.size * 0.5f;

        return Mathf.Abs(localPoint.x) <= halfSize.x &&
               Mathf.Abs(localPoint.y) <= halfSize.y &&
               Mathf.Abs(localPoint.z) <= halfSize.z;
    }

    private void SetInsideState(bool inside, string reason)
    {
        if (lastInsideState == inside && IsInsideRoom == inside)
            return;

        lastInsideState = inside;
        IsInsideRoom = inside;

        if (inside)
        {
            Debug.Log("[ROOM] INSIDE ROOM — editing unlocked! Reason: " + reason);
            SetDebugText("INSIDE ROOM");

            CustomRoomSetupUI ui = Object.FindFirstObjectByType<CustomRoomSetupUI>();

            if (ui != null && RoomManager.IsOwner)
                ui.SetOwnerInside(true);

            if (shelfController != null)
                shelfController.OnEnteredRoom();

            if (objectPlacementController != null)
                objectPlacementController.ShowBottomBar();
        }
        else
        {
            Debug.Log("[ROOM] OUTSIDE ROOM — editing locked. Reason: " + reason);
            SetDebugText("OUTSIDE ROOM");

            CustomRoomSetupUI ui = Object.FindFirstObjectByType<CustomRoomSetupUI>();

            if (ui != null)
                ui.SetOwnerInside(false);

            if (shelfController != null)
                shelfController.OnExitedRoom();

            if (objectPlacementController != null)
                objectPlacementController.HideBottomBar();
        }
    }

    void SetDebugText(string msg)
    {
        if (debugText != null)
            debugText.text = $"[ROOM] {msg}";
    }

    public static void Reset()
    {
        IsInsideRoom = false;
    }
}