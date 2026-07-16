using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SaveRoomButtonController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject saveButton;
    [SerializeField] private TMP_Text saveButtonText;
    [SerializeField] private Image saveButtonImage;

    [Header("Colors")]
    [SerializeField] private Color normalColor = new Color(0.2f, 0.6f, 1f);
    [SerializeField] private Color savingColor = new Color(1f, 0.8f, 0.2f);
    [SerializeField] private Color savedColor = new Color(0.2f, 0.8f, 0.4f);
    [SerializeField] private Color errorColor = new Color(1f, 0.3f, 0.3f);

    [SerializeField] private float savedDisplayDuration = 2f;

    private bool isSaving = false;



    IEnumerator Start()
{
    if (saveButton != null)
        saveButton.SetActive(false);

    yield return null;
    yield return null;

    if (saveButton != null)
        saveButton.SetActive(RoomManager.IsOwner);

    if (saveButtonImage != null) saveButtonImage.color = normalColor;
    if (saveButtonText != null) saveButtonText.text = "Save Room";

    Debug.Log("[SAVE BUTTON] IsOwner = " + RoomManager.IsOwner);

        Debug.Log("[SAVE BUTTON] Active = " + saveButton.activeSelf);
        Debug.Log("[SAVE BUTTON] IsOwner = " + RoomManager.IsOwner);
    }

    public void OnSavePressed()
    {
        if (isSaving) return;

        if (!RoomManager.IsOwner)
        {
            ShowFeedback("Not Owner!", errorColor);
            return;
        }

        if (RoomManager.Instance?.CurrentRoom == null)
        {
            ShowFeedback("No Room!", errorColor);
            return;
        }

        StartCoroutine(SaveWithFeedback());
    }

    IEnumerator SaveWithFeedback()
    {
        isSaving = true;
        ShowFeedback("Saving...", savingColor);
        yield return null;

        bool success = false;
        try { RoomManager.Instance.SaveCurrentRoom(); success = true; }
        catch (System.Exception e) { Debug.LogError("[SAVE] Failed: " + e.Message); }

        yield return new WaitForSeconds(0.3f);

        if (success)
        {
            ShowFeedback("Saved! ✓", savedColor);
            Debug.Log($"[SAVE] Saved! Objects: {RoomManager.Instance.CurrentRoom.objects.Count}");
        }
        else
            ShowFeedback("Failed!", errorColor);

        yield return new WaitForSeconds(savedDisplayDuration);
        ShowFeedback("Save Room", normalColor);
        isSaving = false;
    }

    void ShowFeedback(string text, Color color)
    {
        if (saveButtonText != null) saveButtonText.text = text;
        if (saveButtonImage != null) saveButtonImage.color = color;
    }
}