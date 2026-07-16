
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

//public class OwnerObjectSettings : MonoBehaviour
//{
//    [Header("Panel")]
//    [SerializeField] private GameObject settingsPanel;

//    [Header("Input Fields")]
//    [SerializeField] private TMP_InputField nameInputField;
//    [SerializeField] private TMP_InputField descInputField;
//    [SerializeField] private TMP_InputField priceInputField;

//    [Header("Type Toggle")]
//    [SerializeField] private Button viewOnlyButton;
//    [SerializeField] private Button forSaleButton;
//    [SerializeField] private Image viewOnlyButtonImage;
//    [SerializeField] private Image forSaleButtonImage;
//    [SerializeField] private GameObject priceSection;

//    [Header("Buttons")]
//    [SerializeField] private Button saveButton;
//    [SerializeField] private Button closeButton;

//    [Header("Colors")]
//    [SerializeField] private Color selectedColor = new Color(0.2f, 0.6f, 1f);
//    [SerializeField] private Color unselectedColor = new Color(0.4f, 0.4f, 0.4f);

//    private ObjectMetaData currentMeta;
//    private System.Action onSaved;

//    // FIX: track if panel was explicitly opened to prevent accidental shows
//    private bool isOpen = false;

//    void Start()
//    {
//        settingsPanel.SetActive(false);
//        viewOnlyButton.onClick.AddListener(() => SetType(ObjectMetaData.ObjectType.ViewOnly));
//        forSaleButton.onClick.AddListener(() => SetType(ObjectMetaData.ObjectType.ForSale));
//        saveButton.onClick.AddListener(OnSavePressed);
//        closeButton.onClick.AddListener(Hide);
//    }

//    public bool IsOpen => isOpen;

//    // FIX: only open when explicitly called — never auto-opens
//    public void Show(ObjectMetaData meta, System.Action onSavedCallback = null)
//    {
//        // Extra safety — must be owner
//        if (!RoomManager.IsOwner)
//        {
//            Debug.LogWarning("[OWNER SETTINGS] Not owner — cannot open settings.");
//            return;
//        }

//        currentMeta = meta;
//        onSaved = onSavedCallback;

//        nameInputField.text = meta.objectName;
//        descInputField.text = meta.description;
//        priceInputField.text = meta.price > 0 ? meta.price.ToString("F2") : "";

//        // Refresh type buttons to match current state
//        RefreshTypeButtons(meta.objectType);

//        isOpen = true;
//        settingsPanel.SetActive(true);

//        Debug.Log($"[OWNER SETTINGS] Showing for: {meta.objectName} | type={meta.objectType}");
//    }

//    public void Hide()
//    {
//        isOpen = false;
//        settingsPanel.SetActive(false);
//        currentMeta = null;
//    }

//    void SetType(ObjectMetaData.ObjectType type)
//    {
//        if (currentMeta != null)
//            currentMeta.objectType = type;

//        RefreshTypeButtons(type);
//    }

//    void RefreshTypeButtons(ObjectMetaData.ObjectType type)
//    {
//        bool isForSale = (type == ObjectMetaData.ObjectType.ForSale);
//        viewOnlyButtonImage.color = isForSale ? unselectedColor : selectedColor;
//        forSaleButtonImage.color = isForSale ? selectedColor : unselectedColor;
//        priceSection.SetActive(isForSale);
//    }

//    void OnSavePressed()
//    {
//        if (currentMeta == null)
//        {
//            Debug.LogWarning("[OWNER SETTINGS] Save pressed but currentMeta is null!");
//            return;
//        }

//        string newName = nameInputField.text.Trim();
//        currentMeta.objectName = string.IsNullOrEmpty(newName) ? "My Object" : newName;
//        currentMeta.description = descInputField.text.Trim();

//        if (currentMeta.IsForSale)
//        {
//            if (float.TryParse(priceInputField.text, out float p))
//                currentMeta.price = Mathf.Max(0f, p);
//            else
//                currentMeta.price = 0f;
//        }
//        else
//        {
//            currentMeta.price = 0f;
//        }

//        currentMeta.RefreshIndicator();

//        // Save to room using instanceId for reliable lookup
//        if (RoomManager.Instance != null && RoomManager.IsOwner)
//            RoomManager.Instance.UpdateObjectMeta(currentMeta);

//        Debug.Log($"[OWNER SETTINGS] Saved: {currentMeta.objectName} | {currentMeta.objectType} | {currentMeta.price} | id={currentMeta.instanceId}");

//        Hide();
//        onSaved?.Invoke();
//    }
//}


using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class OwnerObjectSettings : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject settingsPanel;

    [Header("Input Fields")]
    [SerializeField] private TMP_InputField nameInputField;
    [SerializeField] private TMP_InputField descInputField;
    [SerializeField] private TMP_InputField priceInputField;

    [Header("Type Toggle")]
    [SerializeField] private Button viewOnlyButton;
    [SerializeField] private Button forSaleButton;
    [SerializeField] private Image viewOnlyButtonImage;
    [SerializeField] private Image forSaleButtonImage;
    [SerializeField] private GameObject priceSection;

    [Header("Buttons")]
    [SerializeField] private Button saveButton;
    [SerializeField] private Button closeButton;

    [Header("Colors")]
    [SerializeField] private Color selectedColor = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color unselectedColor = new Color(0.4f, 0.4f, 0.4f);

    private ObjectMetaData currentMeta;
    private System.Action onSaved;

    // FIX: track if panel was explicitly opened to prevent accidental shows
    private bool isOpen = false;

    void Start()
    {
        settingsPanel.SetActive(false);
        viewOnlyButton.onClick.AddListener(() => SetType(ObjectMetaData.ObjectType.ViewOnly));
        forSaleButton.onClick.AddListener(() => SetType(ObjectMetaData.ObjectType.ForSale));
        saveButton.onClick.AddListener(OnSavePressed);
        closeButton.onClick.AddListener(Hide);
    }

    public bool IsOpen => isOpen;

    // FIX: only open when explicitly called — never auto-opens
    public void Show(ObjectMetaData meta, System.Action onSavedCallback = null)
    {
        if (meta == null)
        {
            Debug.LogWarning("[OWNER SETTINGS] Cannot show: meta is null.");
            return;
        }

        if (RoomManager.Instance != null)
            RoomManager.Instance.RefreshOwnership();

        Debug.Log("[OWNER SETTINGS] IsOwner=" + RoomManager.IsOwner);

        if (!RoomAccessManager.CanEdit)
        {
            Debug.LogWarning("[OWNER SETTINGS] No edit permission — cannot open settings.");
            return;
        }

        currentMeta = meta;
        onSaved = onSavedCallback;

        if (settingsPanel == null)
        {
            Debug.LogError("[OWNER SETTINGS] settingsPanel is NOT assigned.");
            return;
        }

        // Put panel in front of other UI
        settingsPanel.transform.SetAsLastSibling();

        // Force visible
        settingsPanel.SetActive(true);

        CanvasGroup cg = settingsPanel.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = settingsPanel.AddComponent<CanvasGroup>();

        cg.alpha = 1f;
        cg.interactable = true;
        cg.blocksRaycasts = true;

        // Force RectTransform to be visible in center
        RectTransform rt = settingsPanel.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.localScale = Vector3.one;
            rt.anchoredPosition = Vector2.zero;
        }

        nameInputField.text = meta.objectName;
        descInputField.text = meta.description;
        priceInputField.text = meta.price > 0 ? meta.price.ToString("F2") : "";

        RefreshTypeButtons(meta.objectType);

        isOpen = true;

        Debug.Log("[OWNER SETTINGS] Panel active=" + settingsPanel.activeInHierarchy);
        Debug.Log($"[OWNER SETTINGS] Showing for: {meta.objectName} | type={meta.objectType}");
    }

    public void Hide()
    {
        isOpen = false;

        if (settingsPanel != null)
            settingsPanel.SetActive(false);

        currentMeta = null;

        Debug.Log("[OWNER SETTINGS] Hidden.");
    }

    void SetType(ObjectMetaData.ObjectType type)
    {
        if (currentMeta != null)
            currentMeta.objectType = type;

        RefreshTypeButtons(type);
    }

    void RefreshTypeButtons(ObjectMetaData.ObjectType type)
    {
        bool isForSale = (type == ObjectMetaData.ObjectType.ForSale);
        viewOnlyButtonImage.color = isForSale ? unselectedColor : selectedColor;
        forSaleButtonImage.color = isForSale ? selectedColor : unselectedColor;
        priceSection.SetActive(isForSale);
    }

    void OnSavePressed()
    {
        if (currentMeta == null)
        {
            Debug.LogWarning("[OWNER SETTINGS] Save pressed but currentMeta is null!");
            return;
        }

        string newName = nameInputField.text.Trim();
        currentMeta.objectName = string.IsNullOrEmpty(newName) ? "My Object" : newName;
        currentMeta.description = descInputField.text.Trim();

        if (currentMeta.IsForSale)
        {
            if (float.TryParse(priceInputField.text, out float p))
                currentMeta.price = Mathf.Max(0f, p);
            else
                currentMeta.price = 0f;
        }
        else
        {
            currentMeta.price = 0f;
        }

        currentMeta.RefreshIndicator();

        // Save to room using instanceId for reliable lookup
        if (RoomManager.Instance != null && RoomAccessManager.CanEdit)
            RoomManager.Instance.UpdateObjectMeta(currentMeta);

        Debug.Log($"[OWNER SETTINGS] Saved: {currentMeta.objectName} | {currentMeta.objectType} | {currentMeta.price} | id={currentMeta.instanceId}");

        Hide();
        onSaved?.Invoke();
    }
}