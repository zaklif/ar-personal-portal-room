using Firebase.Database;
using Firebase.Extensions;
using Firebase.Storage;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomThumbnailManager : MonoBehaviour
{
    [Header("Thumbnail UI Feedback")]
    [SerializeField] private Button thumbnailButton;
    [SerializeField] private TMP_Text thumbnailButtonText;
    [SerializeField] private GameObject thumbnailDoneBadge;
    [SerializeField] private TMP_Text toastText;
    [SerializeField] private GameObject toastPanel;

    private const string BucketUrl = "gs://portalarroom.firebasestorage.app";

    private bool isCapturing = false;

    private void Start()
    {
        if (toastPanel != null)
            toastPanel.SetActive(false);

        if (thumbnailDoneBadge != null)
            thumbnailDoneBadge.SetActive(false);

        if (thumbnailButtonText != null)
            thumbnailButtonText.text = "Update Thumbnail";
    }

    public void CaptureAndUploadThumbnail()
    {
        if (isCapturing)
            return;

        if (!RoomManager.IsOwner)
        {
            Debug.LogWarning("[THUMBNAIL] Only owner can update thumbnail.");
            StartCoroutine(ShowToast("Only owner can update thumbnail"));
            return;
        }

        if (RoomManager.Instance == null || RoomManager.Instance.CurrentRoom == null)
        {
            Debug.LogWarning("[THUMBNAIL] No current room.");
            StartCoroutine(ShowToast("No room found"));
            return;
        }

        StartCoroutine(CaptureRoutine());
    }

    IEnumerator CaptureRoutine()
    {
        isCapturing = true;
        SetThumbnailCapturingUI();

        yield return new WaitForEndOfFrame();

        Texture2D tex = null;
        byte[] pngBytes = null;

        try
        {
            tex = ScreenCapture.CaptureScreenshotAsTexture();
            pngBytes = tex.EncodeToPNG();
        }
        catch (System.Exception e)
        {
            Debug.LogError("[THUMBNAIL] Capture failed: " + e.Message);

            if (tex != null)
                Destroy(tex);

            isCapturing = false;
            SetThumbnailFailedUI();
            yield break;
        }

        if (tex != null)
            Destroy(tex);

        string roomId = RoomManager.Instance.CurrentRoom.roomId;
        string storagePath = "roomThumbnails/" + roomId + ".png";

        StorageReference fileRef = FirebaseStorage
            .GetInstance(BucketUrl)
            .GetReference(storagePath);

        Debug.Log("[THUMBNAIL] Uploading: " + storagePath);

        fileRef.PutBytesAsync(pngBytes).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("[THUMBNAIL] Upload failed: " + task.Exception);
                isCapturing = false;
                SetThumbnailFailedUI();
                return;
            }

            fileRef.GetDownloadUrlAsync().ContinueWithOnMainThread(urlTask =>
            {
                if (urlTask.IsFaulted || urlTask.IsCanceled)
                {
                    Debug.LogError("[THUMBNAIL] URL failed: " + urlTask.Exception);
                    isCapturing = false;
                    SetThumbnailFailedUI();
                    return;
                }

                string url = urlTask.Result.ToString();

                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    { "/rooms/" + roomId + "/thumbnailUrl", url },
                    { "/rooms/" + roomId + "/thumbnailUpdatedAt", ServerValue.Timestamp }
                };

                FirebaseDatabase.DefaultInstance.RootReference
                    .UpdateChildrenAsync(updates)
                    .ContinueWithOnMainThread(saveTask =>
                    {
                        isCapturing = false;

                        if (saveTask.IsFaulted || saveTask.IsCanceled)
                        {
                            Debug.LogError("[THUMBNAIL] Save URL failed: " + saveTask.Exception);
                            SetThumbnailFailedUI();
                            return;
                        }

                        Debug.Log("[THUMBNAIL] Saved URL to rooms/" + roomId);
                        SetThumbnailSuccessUI();
                    });
            });
        });
    }

    private void SetThumbnailCapturingUI()
    {
        if (thumbnailButton != null)
            thumbnailButton.interactable = false;

        if (thumbnailButtonText != null)
            thumbnailButtonText.text = "Capturing...";

        if (thumbnailDoneBadge != null)
            thumbnailDoneBadge.SetActive(false);

        StartCoroutine(ShowToast("Capturing room thumbnail..."));
    }

    private void SetThumbnailSuccessUI()
    {
        if (thumbnailButton != null)
            thumbnailButton.interactable = true;

        if (thumbnailButtonText != null)
            thumbnailButtonText.text = "Updated ✓";

        if (thumbnailDoneBadge != null)
            thumbnailDoneBadge.SetActive(true);

        StartCoroutine(ShowToast("Room thumbnail updated successfully"));
        StartCoroutine(ResetThumbnailButtonText());
    }

    private void SetThumbnailFailedUI()
    {
        if (thumbnailButton != null)
            thumbnailButton.interactable = true;

        if (thumbnailButtonText != null)
            thumbnailButtonText.text = "Failed";

        StartCoroutine(ShowToast("Failed to update thumbnail"));
        StartCoroutine(ResetThumbnailButtonText());
    }

    private IEnumerator ResetThumbnailButtonText()
    {
        yield return new WaitForSeconds(2f);

        if (thumbnailButtonText != null)
            thumbnailButtonText.text = "Update Thumbnail";
    }

    private IEnumerator ShowToast(string message, float duration = 2f)
    {
        if (toastPanel == null || toastText == null)
            yield break;

        toastText.text = message;
        toastPanel.SetActive(true);

        yield return new WaitForSeconds(duration);

        toastPanel.SetActive(false);
    }
}