//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using System.Collections.Generic;

///// <summary>
///// Shows a scrollable grid of room templates for the user to pick.
///// Appears AFTER "Create My Room" is pressed on HomeScreen,
///// BEFORE the AR scene loads.
/////
///// SETUP:
///// 1. In HomeScreen scene, create a new Panel called "RoomTemplateSelectorPanel"
/////    and set it inactive by default.
///// 2. Build this UI inside it:
/////
///// RoomTemplateSelectorPanel
///// ├── TitleText (TMP)              "Choose Your Room"
///// ├── Scroll View
///// │   └── Viewport → Content      (Grid Layout Group, cell 300x350)
///// │       └── [TemplateCardPrefab spawned here]
///// └── BackButton (Button)         "Back"
/////
///// 3. Create a prefab called "TemplateCardPrefab":
/////    ├── Background (Image)
/////    ├── PreviewImage (Image)     room thumbnail
/////    ├── NameText (TMP)           room name
/////    ├── DescriptionText (TMP)    short desc
/////    └── SelectButton (Button)    "Select"
/////
///// 4. Assign all fields below in Inspector.
///// 5. Create RoomTemplate assets (Right click → Create → AR Room → Room Template)
/////    and assign them to the Templates list.
///// </summary>
//public class RoomTemplateSelector : MonoBehaviour
//{
//    [Header("UI")]
//    [SerializeField] private GameObject selectorPanel;
//    [SerializeField] private Transform cardContainer;        // Grid Layout content
//    [SerializeField] private GameObject templateCardPrefab; // card UI prefab
//    [SerializeField] private Button backButton;

//    [Header("Room Templates")]
//    [SerializeField] private List<RoomTemplateData> templates = new List<RoomTemplateData>();

//    // Callback — called when user picks a template
//    private System.Action<RoomTemplateData> onTemplateSelected;

//    void Start()
//    {
//        selectorPanel.SetActive(false);

//        if (backButton != null)
//            backButton.onClick.AddListener(Hide);
//    }

//    // ── Show the selector ─────────────────────────────────────────────────

//    [SerializeField] private GameObject homePanel; // drag your HomePanel here

//    public void Show(System.Action<RoomTemplateData> onSelected)
//    {
//        onTemplateSelected = onSelected;
//        homePanel.SetActive(false);      // ← ADD THIS — hides background
//        selectorPanel.SetActive(true);
//        PopulateCards();
//    }

//    public void Hide()
//    {
//        selectorPanel.SetActive(false);
//        homePanel.SetActive(true);       // ← ADD THIS — shows background again
//    }

//    // ── Populate cards ────────────────────────────────────────────────────

//    void PopulateCards()
//    {
//        // Clear existing cards
//        foreach (Transform child in cardContainer)
//            Destroy(child.gameObject);

//        foreach (var template in templates)
//        {
//            RoomTemplateData t = template; // capture for lambda
//            GameObject card = Instantiate(templateCardPrefab, cardContainer);

//            // Set preview image
//            var previewImg = card.transform.Find("PreviewImage")?.GetComponent<Image>();
//            if (previewImg != null && t.previewImage != null)
//                previewImg.sprite = t.previewImage;

//            // Set name
//            var nameText = card.transform.Find("NameText")?.GetComponent<TMP_Text>();
//            if (nameText != null)
//                nameText.text = t.templateName;

//            // Set description
//            var descText = card.transform.Find("DescriptionText")?.GetComponent<TMP_Text>();
//            if (descText != null)
//                descText.text = t.description;

//            // Wire select button
//            var selectBtn = card.transform.Find("SelectButton")?.GetComponent<Button>();
//            if (selectBtn != null)
//                selectBtn.onClick.AddListener(() => OnCardSelected(t));
//        }
//    }

//    void OnCardSelected(RoomTemplateData template)
//    {
//        Debug.Log("[TEMPLATE] Selected: " + template.templateName);
//        Hide();
//        onTemplateSelected?.Invoke(template);
//    }
//}
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class RoomTemplateSelector : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject selectorPanel;
    [SerializeField] private GameObject homePanel;
    [SerializeField] private Transform cardContainer;
    [SerializeField] private GameObject templateCardPrefab;
    [SerializeField] private Button backButton;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private TMP_InputField roomNameInput;

    [Header("Room Templates")]
    [SerializeField] private List<RoomTemplateData> templates = new List<RoomTemplateData>();

    private System.Action<RoomTemplateData> onTemplateSelected;

    private TemplateCardUI selectedCard;
    private RoomTemplateData selectedTemplate;

    void Start()
    {
        if (selectorPanel != null)
            selectorPanel.SetActive(false);

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(Hide);
        }

        if (createRoomButton != null)
        {
            createRoomButton.onClick.RemoveAllListeners();
            createRoomButton.onClick.AddListener(ConfirmSelectedTemplate);
        }
    }

    public void Show(System.Action<RoomTemplateData> onSelected)
    {
        Debug.Log("[TEMPLATE] Show() called");

        onTemplateSelected = onSelected;

        if (homePanel != null)
            homePanel.SetActive(false);

        if (selectorPanel != null)
            selectorPanel.SetActive(true);

        PopulateCards();
    }

    public void Hide()
    {
        if (selectorPanel != null)
            selectorPanel.SetActive(false);

        if (homePanel != null)
            homePanel.SetActive(true);
    }

    void PopulateCards()
    {

        Debug.Log("[TEMPLATE] PopulateCards called. Count: " + templates.Count);
        Debug.Log("[TEMPLATE] cardContainer: " + cardContainer);
        Debug.Log("[TEMPLATE] templateCardPrefab: " + templateCardPrefab);


        selectedCard = null;
        selectedTemplate = null;

        foreach (Transform child in cardContainer)
            Destroy(child.gameObject);

        foreach (RoomTemplateData template in templates)
        {
            GameObject cardObj = Instantiate(templateCardPrefab, cardContainer);

            TemplateCardUI cardUI = cardObj.GetComponent<TemplateCardUI>();

            if (cardUI == null)
            {
                Debug.LogError("[TEMPLATE] TemplateCardUI missing on prefab.");
                continue;
            }

            cardUI.Setup(template, this);
        }

        if (cardContainer.childCount > 0)
        {
            TemplateCardUI firstCard =
                cardContainer.GetChild(0).GetComponent<TemplateCardUI>();

            if (firstCard != null)
                SelectCard(firstCard);
        }
    }

    public void SelectCard(TemplateCardUI card)
    {
        if (selectedCard != null)
            selectedCard.SetSelected(false);

        selectedCard = card;
        selectedTemplate = card.TemplateData;

        selectedCard.SetSelected(true);

        Debug.Log("[TEMPLATE] Selected: " + selectedTemplate.templateName);
    }

    void ConfirmSelectedTemplate()
    {
        if (selectedTemplate == null)
        {
            Debug.LogWarning("[TEMPLATE] No template selected.");
            return;
        }

        string roomName = roomNameInput != null
    ? roomNameInput.text.Trim()
    : "";

        if (string.IsNullOrEmpty(roomName))
            roomName = UserSession.Username + "'s Room";

        PlayerPrefs.SetString("PendingRoomName", roomName);
        PlayerPrefs.Save();

        Debug.Log("[TEMPLATE] Pending room name: " + roomName);

        Debug.Log("[TEMPLATE] Confirmed: " + selectedTemplate.templateName);

        Hide();
        onTemplateSelected?.Invoke(selectedTemplate);
    }
}