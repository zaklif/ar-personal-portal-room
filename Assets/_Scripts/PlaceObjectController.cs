//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

///// <summary>
///// Attach this to every placed object prefab.
///// When the user taps a placed object while inside the room,
///// an Edit / Delete popup appears near the object.
/////
///// SETUP:
///// 1. Attach this script to each object prefab.
///// 2. Make sure each prefab has a Collider (BoxCollider or MeshCollider) — needed for tap detection.
///// 3. In the scene, create a world-space Canvas (or screen-space) with:
/////    - A panel containing "Edit" and "Delete" buttons.
/////    - Assign it to editPopupPanel below.
///// 4. Wire the Edit and Delete buttons to OnEditPressed() and OnDeletePressed().
///// </summary>
//public class PlacedObjectController : MonoBehaviour
//{
//    [Header("UI Popup (assign a screen-space panel)")]
//    [SerializeField] private GameObject editPopupPanel;
//    [SerializeField] private Button editButton;
//    [SerializeField] private Button deleteButton;

//    // Back-reference so ObjectPlacementController knows this object exists
//    private ObjectPlacementController placementController;
//    private bool isSelected = false;

//    public void Init(ObjectPlacementController controller, GameObject popupPanel)
//    {
//        placementController = controller;
//        editPopupPanel = popupPanel;

//        // Wire buttons if not wired via Inspector
//        var buttons = popupPanel.GetComponentsInChildren<Button>();
//        foreach (var btn in buttons)
//        {
//            if (btn.name.ToLower().Contains("edit"))
//                btn.onClick.AddListener(OnEditPressed);
//            else if (btn.name.ToLower().Contains("delete"))
//                btn.onClick.AddListener(OnDeletePressed);
//        }

//        HidePopup();
//    }

//    // ─── Called by ObjectPlacementController when this object is tapped ────

//    public void OnTapped()
//    {
//        if (!RoomInsideDetector.IsInsideRoom)
//        {
//            Debug.Log("[OBJECT] Must be inside room to edit.");
//            return;
//        }

//        isSelected = !isSelected;

//        if (isSelected)
//            ShowPopup();
//        else
//            HidePopup();
//    }

//    public void Deselect()
//    {
//        isSelected = false;
//        HidePopup();
//    }

//    void ShowPopup()
//    {
//        if (editPopupPanel != null)
//            editPopupPanel.SetActive(true);
//    }

//    void HidePopup()
//    {
//        if (editPopupPanel != null)
//            editPopupPanel.SetActive(false);
//    }

//    // ─── Edit: re-enter adjusting mode for this object ────────────────────

//    public void OnEditPressed()
//    {
//        HidePopup();
//        isSelected = false;
//        placementController.BeginEditExistingObject(gameObject);
//        Debug.Log("[OBJECT] Editing: " + gameObject.name);
//    }

//    // ─── Delete: remove this object from scene ────────────────────────────

//    public void OnDeletePressed()
//    {
//        HidePopup();
//        placementController.DeleteObject(gameObject);
//        Debug.Log("[OBJECT] Deleted: " + gameObject.name);
//    }
//}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class PlacedObjectController : MonoBehaviour
{
    private GameObject editPopupPanel;
    private ObjectPlacementController placementController;
    private bool isSelected = false;
    private bool isInitialized = false;

    public void Init(ObjectPlacementController controller, GameObject popupPanel)
    {
        placementController = controller;
        editPopupPanel = popupPanel;

        if (!isInitialized)
        {
            // Find buttons including inactive ones (true = includeInactive)
            var buttons = popupPanel.GetComponentsInChildren<Button>(true);
            foreach (var btn in buttons)
            {
                string btnName = btn.name.ToLower();
                if (btnName.Contains("edit"))
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(OnEditPressed);
                    Debug.Log("[POC] Wired Edit: " + btn.name);
                }
                else if (btnName.Contains("delete"))
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(OnDeletePressed);
                    Debug.Log("[POC] Wired Delete: " + btn.name);
                }
            }
            isInitialized = true;
        }

        HidePopup();
    }

    public void OnTapped()
    {
        if (!RoomInsideDetector.IsInsideRoom)
        {
            Debug.Log("[POC] Must be inside room to edit.");
            return;
        }

        isSelected = !isSelected;

        if (isSelected)
            ShowPopup();
        else
            HidePopup();

        Debug.Log($"[POC] '{gameObject.name}' selected={isSelected}");
    }

    public void Deselect()
    {
        isSelected = false;
        HidePopup();
    }

    void ShowPopup()
    {
        if (editPopupPanel == null) return;
        editPopupPanel.SetActive(true);

        // FIX: Tell ObjectPlacementController popup is open
        // so it stops processing taps while buttons are visible
        placementController.SetPopupOpen(true);
    }

    void HidePopup()
    {
        if (editPopupPanel == null) return;
        editPopupPanel.SetActive(false);

        // FIX: Tell ObjectPlacementController popup is closed
        placementController.SetPopupOpen(false);
    }

    public void OnEditPressed()
    {
        Debug.Log("[POC] Edit pressed on: " + gameObject.name);
        isSelected = false;

        // Hide popup FIRST before calling BeginEdit
        // (BeginEdit also hides it but this ensures correct order)
        if (editPopupPanel != null)
            editPopupPanel.SetActive(false);

        placementController.SetPopupOpen(false);
        placementController.BeginEditExistingObject(gameObject);
    }

    public void OnDeletePressed()
    {
        Debug.Log("[POC] Delete pressed on: " + gameObject.name);

        if (editPopupPanel != null)
            editPopupPanel.SetActive(false);

        placementController.SetPopupOpen(false);
        placementController.DeleteObject(gameObject);
    }
}