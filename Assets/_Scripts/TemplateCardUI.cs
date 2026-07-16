using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TemplateCardUI : MonoBehaviour
{
    [Header("Main UI")]
    public Image previewImage;
    public TMP_Text nameText;
    public TMP_Text descriptionText;

    [Header("Selected UI")]
    public GameObject selectedBadge;
    public GameObject radioDot;
    public Image cardBackground;

    [Header("Button")]
    public Button selectButton;

    private RoomTemplateData templateData;
    private RoomTemplateSelector selector;

    public RoomTemplateData TemplateData => templateData;

    public void Setup(RoomTemplateData data, RoomTemplateSelector owner)
    {
        templateData = data;
        selector = owner;

        if (previewImage != null && data.previewImage != null)
            previewImage.sprite = data.previewImage;

        if (nameText != null)
            nameText.text = data.templateName;

        if (descriptionText != null)
            descriptionText.text = data.description;

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(() =>
            {
                selector.SelectCard(this);
            });
        }

        SetSelected(false);
    }

    public void SetSelected(bool selected)
    {
        if (selectedBadge != null)
            selectedBadge.SetActive(selected);

        if (radioDot != null)
            radioDot.SetActive(selected);

        if (cardBackground != null)
        {
            cardBackground.color = selected
                ? new Color32(17, 40, 38, 255)
                : new Color32(17, 24, 48, 255);
        }
    }
}