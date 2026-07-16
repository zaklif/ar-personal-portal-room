//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using System.Collections;

///// <summary>
///// Screen-space popup shown when visitor taps a placed object.
///// Shows: name, description (expandable), 360 spin, price if for sale, chat button.
/////
///// SETUP — Create this UI in Canvas:
///// VisitorPopupPanel (Panel, set inactive)
///// ├── CloseButton (Button)              "✕"
///// ├── ObjectNameText (TMP_Text)         object name
///// ├── TypeBadge (Image + TMP_Text)      "$" green or "👁" blue
///// ├── DescriptionToggleButton (Button)  "Description ▼"
///// ├── DescriptionPanel (Panel)
///// │   └── DescriptionText (TMP_Text)
///// ├── SpinButton (Button)               "🔄 Spin 360°"
///// ├── PricePanel (Panel)                shown only if ForSale
///// │   ├── PriceLabel (TMP_Text)         "Price"
///// │   └── PriceText (TMP_Text)          "RM 99.00"
///// └── ChatButton (Button)               "💬 Chat Owner"
/////     (hidden if ViewOnly)
///// </summary>
//public class VisitorObjectPopup : MonoBehaviour
//{
//    [Header("Panel")]
//    [SerializeField] private GameObject popupPanel;

//    [Header("Info UI")]
//    [SerializeField] private TMP_Text objectNameText;
//    [SerializeField] private TMP_Text typeBadgeText;
//    [SerializeField] private Image typeBadgeImage;

//    [Header("Description")]
//    [SerializeField] private Button descriptionToggleButton;
//    [SerializeField] private TMP_Text descriptionToggleText;
//    [SerializeField] private GameObject descriptionPanel;
//    [SerializeField] private TMP_Text descriptionText;

//    [Header("Spin")]
//    [SerializeField] private Button spinButton;
//    [SerializeField] private TMP_Text spinButtonText;

//    [Header("Price")]
//    [SerializeField] private GameObject pricePanel;
//    [SerializeField] private TMP_Text priceText;

//    [Header("Chat")]
//    [SerializeField] private GameObject chatButton;

//    [Header("Close")]
//    [SerializeField] private Button closeButton;

//    [Header("Colors")]
//    [SerializeField] private Color forSaleColor = new Color(0.2f, 0.8f, 0.2f);
//    [SerializeField] private Color viewOnlyColor = new Color(0.2f, 0.5f, 1f);

//    [Header("References")]
//    [SerializeField] private InAppChat inAppChat;

//    // Current object being viewed
//    private ObjectMetaData currentMeta;
//    private GameObject currentObject;
//    private bool isDescriptionOpen = false;
//    private bool isSpinning = false;
//    private Coroutine spinCoroutine;

//    void Start()
//    {
//        popupPanel.SetActive(false);
//        descriptionPanel.SetActive(false);

//        closeButton.onClick.AddListener(Hide);
//        descriptionToggleButton.onClick.AddListener(ToggleDescription);
//        spinButton.onClick.AddListener(ToggleSpin);
//    }

//    // ── Show popup for a tapped object ───────────────────────────────────

//    public void Show(GameObject obj, ObjectMetaData meta)
//    {
//        currentObject = obj;
//        currentMeta = meta;
//        isDescriptionOpen = false;
//        isSpinning = false;

//        // Stop any existing spin
//        if (spinCoroutine != null) { StopCoroutine(spinCoroutine); spinCoroutine = null; }

//        // Object name
//        objectNameText.text = meta.objectName;

//        // Type badge
//        if (meta.IsForSale)
//        {
//            typeBadgeText.text = "$ For Sale";
//            typeBadgeImage.color = forSaleColor;
//        }
//        else
//        {
//            typeBadgeText.text = "👁 View Only";
//            typeBadgeImage.color = viewOnlyColor;
//        }

//        // Description
//        bool hasDescription = !string.IsNullOrEmpty(meta.description);
//        descriptionToggleButton.gameObject.SetActive(hasDescription);
//        descriptionPanel.SetActive(false);
//        descriptionText.text = meta.description;
//        descriptionToggleText.text = "Description ▼";

//        // Price panel
//        if (meta.IsForSale)
//        {
//            pricePanel.SetActive(true);
//            priceText.text = $"{meta.currency} {meta.price:F2}";
//        }
//        else
//        {
//            pricePanel.SetActive(false);
//        }

//        // Chat button — only show if for sale
//        chatButton.SetActive(meta.IsForSale);

//        // Spin button
//        spinButtonText.text = "🔄 Spin 360°";

//        popupPanel.SetActive(true);
//        Debug.Log($"[VISITOR POPUP] Showing: {meta.objectName}");
//    }

//    public void Hide()
//    {
//        StopSpinning();
//        popupPanel.SetActive(false);
//        currentObject = null;
//        currentMeta = null;
//    }

//    // ── Description toggle ────────────────────────────────────────────────

//    void ToggleDescription()
//    {
//        isDescriptionOpen = !isDescriptionOpen;
//        descriptionPanel.SetActive(isDescriptionOpen);
//        descriptionToggleText.text = isDescriptionOpen ? "Description ▲" : "Description ▼";
//    }

//    // ── 360 Spin ──────────────────────────────────────────────────────────

//    void ToggleSpin()
//    {
//        if (isSpinning)
//            StopSpinning();
//        else
//            StartSpinning();
//    }

//    void StartSpinning()
//    {
//        if (currentObject == null) return;
//        isSpinning = true;
//        spinButtonText.text = "⏹ Stop Spin";
//        spinCoroutine = StartCoroutine(SpinObject());
//    }

//    void StopSpinning()
//    {
//        isSpinning = false;
//        spinButtonText.text = "🔄 Spin 360°";
//        if (spinCoroutine != null) { StopCoroutine(spinCoroutine); spinCoroutine = null; }
//    }

//    IEnumerator SpinObject()
//    {
//        float spinSpeed = 60f; // degrees per second
//        while (isSpinning && currentObject != null)
//        {
//            currentObject.transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f, Space.World);
//            yield return null;
//        }
//    }

//    // ── Chat button ───────────────────────────────────────────────────────

//    public void OnChatButtonPressed()
//    {
//        if (currentMeta == null) return;
//        Hide();

//        // Open in-app chat with owner's room ID as identifier
//        inAppChat?.OpenChat(currentMeta.ownerRoomId, currentMeta.objectName, currentMeta.price, currentMeta.currency);
//        Debug.Log($"[VISITOR POPUP] Opening chat for room: {currentMeta.ownerRoomId}");
//    }
//}

using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using UnityEngine.EventSystems;

/// <summary>
/// Visitor popup with:
/// - 3D object preview rendered in a RenderTexture (shown as 2D image in UI)
/// - Finger drag to rotate the 3D preview
/// - Name, description toggle, type badge, price, chat button
/// 
/// SETUP:
/// 1. Create a RenderTexture asset: Project → Create → Render Texture
///    Name: "ObjectPreviewRT", Size: 512x512
/// 2. Create a second Camera in scene called "PreviewCamera"
///    - Clear Flags: Solid Color, Background: black transparent
///    - Target Texture: ObjectPreviewRT
///    - Culling Mask: only "Preview" layer
///    - Position: (999, 999, 999) so it's far from AR scene
/// 3. Create a layer called "Preview"
/// 4. In the popup UI, add a RawImage — assign ObjectPreviewRT as its texture
/// 5. Add EventTrigger to the RawImage for drag rotation
/// </summary>
/// 
public class VisitorObjectPopup : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject popupPanel;

    [Header("3D Preview")]
    [SerializeField] private RawImage previewRawImage;      // shows RenderTexture
    [SerializeField] private RenderTexture previewRT;       // assign ObjectPreviewRT
    [SerializeField] private Camera previewCamera;          // assign PreviewCamera
    [SerializeField] private float rotationSpeed = 0.5f;

    [Header("Info UI")]
    [SerializeField] private TMP_Text objectNameText;
    [SerializeField] private TMP_Text typeBadgeText;
    [SerializeField] private Image typeBadgeImage;

    [Header("Description")]
    [SerializeField] private Button descriptionToggleButton;
    [SerializeField] private TMP_Text descriptionToggleText;
    [SerializeField] private GameObject descriptionPanel;
    [SerializeField] private TMP_Text descriptionText;

    [Header("Price")]
    [SerializeField] private GameObject pricePanel;
    [SerializeField] private TMP_Text priceText;

    [Header("Chat")]
    [SerializeField] private Button chatButton;

    [Header("Close")]
    [SerializeField] private Button closeButton;

    [Header("Colors")]
    [SerializeField] private Color forSaleColor = new Color(0.2f, 0.8f, 0.2f);
    [SerializeField] private Color viewOnlyColor = new Color(0.2f, 0.5f, 1f);

    [Header("References")]
    [SerializeField] private InAppChat inAppChat;

    // State
    private ObjectMetaData currentMeta;
    private GameObject currentObject;
    private GameObject previewClone;        // clone placed in front of PreviewCamera
    private Vector3 previewObjectPosition = new Vector3(999f, 999f, 1002f); // in front of preview cam
    private bool isDescriptionOpen = false;

    // Drag rotation
    private bool isDragging = false;
    private Vector2 lastDragPos;

    void Start()
    {
        popupPanel.SetActive(false);
        if (descriptionPanel != null) descriptionPanel.SetActive(false);
        if (closeButton != null) closeButton.onClick.AddListener(Hide);
        if (descriptionToggleButton != null) descriptionToggleButton.onClick.AddListener(ToggleDescription);

        if (chatButton != null)
        {
            chatButton.onClick.RemoveAllListeners();
            chatButton.onClick.AddListener(OnChatButtonPressed);
        }


        if (previewRawImage != null)
            previewRawImage.gameObject.SetActive(false);

        // Wire drag events to RawImage
        //if (previewRawImage != null)
        //{
        //    var trigger = previewRawImage.gameObject.GetComponent<EventTrigger>();
        //    if (trigger == null) trigger = previewRawImage.gameObject.AddComponent<EventTrigger>();

        //    // Drag begin
        //    var beginEntry = new EventTrigger.Entry { eventID = EventTriggerType.BeginDrag };
        //    beginEntry.callback.AddListener((data) => OnDragBegin((PointerEventData)data));
        //    trigger.triggers.Add(beginEntry);

        //    // Drag
        //    var dragEntry = new EventTrigger.Entry { eventID = EventTriggerType.Drag };
        //    dragEntry.callback.AddListener((data) => OnDrag((PointerEventData)data));
        //    trigger.triggers.Add(dragEntry);

        //    // Drag end
        //    var endEntry = new EventTrigger.Entry { eventID = EventTriggerType.EndDrag };
        //    endEntry.callback.AddListener((data) => OnDragEnd((PointerEventData)data));
        //    trigger.triggers.Add(endEntry);
        //}
    }

    // ── Show popup ────────────────────────────────────────────────────────

    public void Show(GameObject obj, ObjectMetaData meta)
    {
        currentObject = obj;
        currentMeta = meta;
        isDescriptionOpen = false;

        // Object name
        objectNameText.text = meta.objectName;

        // FIX: type badge now correctly reads from meta
        if (meta.IsForSale)
        {
            typeBadgeText.text = "$ For Sale";
            typeBadgeImage.color = forSaleColor;
        }
        else
        {
            typeBadgeText.text = "👁 View Only";
            typeBadgeImage.color = viewOnlyColor;
        }

        // Description
        bool hasDesc = !string.IsNullOrEmpty(meta.description);
        descriptionToggleButton.gameObject.SetActive(hasDesc);
        descriptionPanel.SetActive(false);
        if (hasDesc)
        {
            descriptionText.text = meta.description;
            descriptionToggleText.text = "Description ▼";
        }

        // FIX: price and chat only show if ForSale
        if (meta.IsForSale)
        {
            pricePanel.SetActive(true);
            priceText.text = $"{meta.currency} {meta.price:F2}";
            chatButton.gameObject.SetActive(true);
        }
        else
        {
            pricePanel.SetActive(false);
            chatButton.gameObject.SetActive(false);
        }

        popupPanel.SetActive(true);

        //// Spawn 3D preview // kasi buang dulu
        //SpawnPreviewClone(obj);

        Debug.Log($"[VISITOR POPUP] Showing: {meta.objectName} | ForSale={meta.IsForSale}");
    }

    public void Hide()
    {
        DestroyPreviewClone();
        popupPanel.SetActive(false);
        currentObject = null;
        currentMeta = null;
    }

    // ── 3D Preview ────────────────────────────────────────────────────────

    void SpawnPreviewClone(GameObject original)
    {
        DestroyPreviewClone();

        if (previewCamera == null || previewRT == null) return;

        // Clone the object
        previewClone = Instantiate(original);
        previewClone.name = "PreviewClone";

        // Set to Preview layer so only PreviewCamera sees it
        SetLayerRecursively(previewClone, LayerMask.NameToLayer("Preview"));

        // Position in front of preview camera
        previewClone.transform.position = previewObjectPosition;
        previewClone.transform.rotation = Quaternion.identity;

        // Scale to fit preview — normalize based on bounds
        Renderer[] rends = previewClone.GetComponentsInChildren<Renderer>();
        if (rends.Length > 0)
        {
            Bounds b = rends[0].bounds;
            foreach (var r in rends) b.Encapsulate(r.bounds);
            float maxSize = Mathf.Max(b.size.x, b.size.y, b.size.z);
            if (maxSize > 0)
                previewClone.transform.localScale = original.transform.localScale * (0.8f / maxSize);
        }

        // Activate clone
        previewClone.SetActive(true);

        // Point preview camera at it
        previewCamera.transform.position = previewObjectPosition + new Vector3(0, 0, -2f);
        previewCamera.transform.LookAt(previewObjectPosition);
        previewCamera.targetTexture = previewRT;
        previewRawImage.texture = previewRT;

        Debug.Log("[VISITOR POPUP] Preview clone spawned.");
    }

    void DestroyPreviewClone()
    {
        if (previewClone != null)
        {
            Destroy(previewClone);
            previewClone = null;
        }
    }

    void SetLayerRecursively(GameObject obj, int layer)
    {
        obj.layer = layer;
        foreach (Transform child in obj.transform)
            SetLayerRecursively(child.gameObject, layer);
    }

    // ── Drag rotation ─────────────────────────────────────────────────────

    void OnDragBegin(PointerEventData data)
    {
        isDragging = true;
        lastDragPos = data.position;
    }

    void OnDrag(PointerEventData data)
    {
        if (!isDragging || previewClone == null) return;

        Vector2 delta = data.position - lastDragPos;
        lastDragPos = data.position;

        // Horizontal drag = rotate around Y axis
        // Vertical drag = rotate around X axis
        previewClone.transform.Rotate(Vector3.up, -delta.x * rotationSpeed, Space.World);
        previewClone.transform.Rotate(Vector3.right, delta.y * rotationSpeed, Space.World);
    }

    void OnDragEnd(PointerEventData data)
    {
        isDragging = false;
    }

    // ── Description toggle ────────────────────────────────────────────────

    void ToggleDescription()
    {
        isDescriptionOpen = !isDescriptionOpen;
        descriptionPanel.SetActive(isDescriptionOpen);
        descriptionToggleText.text = isDescriptionOpen ? "Description ▲" : "Description ▼";
    }

    // ── Chat ──────────────────────────────────────────────────────────────
    public void OnChatButtonPressed()
    {
        Debug.Log("[CHAT] Button pressed.");

        if (currentMeta == null)
        {
            Debug.Log("[CHAT] currentMeta NULL");
            return;
        }

        if (!currentMeta.IsForSale)
        {
            Debug.Log("[CHAT] Object not for sale.");
            return;
        }

        if (inAppChat == null)
        {
            Debug.LogError("[CHAT] InAppChat NOT ASSIGNED.");
            return;
        }

        Debug.Log("[CHAT] Opening chat...");

        inAppChat.OpenChat(currentMeta);

        Hide();
    }
}