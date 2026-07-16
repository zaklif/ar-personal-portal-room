using System;
using System.IO;
using Firebase.Storage;
using Firebase.Extensions;
using UnityEngine;

public static class CloudModelUploader
{
    private const string BucketUrl = "gs://portalarroom.firebasestorage.app";

    public static void UploadGlb(
        string roomId,
        string instanceId,
        string localFilePath,
        Action<string, string> onUploaded)
    {
        if (string.IsNullOrEmpty(roomId) ||
            string.IsNullOrEmpty(instanceId) ||
            string.IsNullOrEmpty(localFilePath) ||
            !File.Exists(localFilePath))
        {
            Debug.LogError("[CLOUD MODEL] Invalid upload data.");
            onUploaded?.Invoke("", "");
            return;
        }

        string fileName = Path.GetFileName(localFilePath);
        string storagePath = $"models/{roomId}/{instanceId}_{fileName}";

        FirebaseStorage storage = FirebaseStorage.GetInstance(BucketUrl);
        StorageReference fileRef = storage.GetReference(storagePath);

        Debug.Log("[CLOUD MODEL] Uploading: " + storagePath);
        Debug.Log("[CLOUD MODEL] Local file path: " + localFilePath);

        string uploadUri = "file://" + localFilePath;

        fileRef.PutFileAsync(uploadUri)
            .ContinueWithOnMainThread(uploadTask =>
            {
                if (uploadTask.IsFaulted || uploadTask.IsCanceled)
                {
                    Debug.LogError("[CLOUD MODEL] Upload failed: " + uploadTask.Exception);
                    onUploaded?.Invoke("", "");
                    return;
                }

                Debug.Log("[CLOUD MODEL] Upload complete. Getting URL...");

                fileRef.GetDownloadUrlAsync()
                    .ContinueWithOnMainThread(urlTask =>
                    {
                        if (urlTask.IsFaulted || urlTask.IsCanceled)
                        {
                            Debug.LogError("[CLOUD MODEL] URL failed: " + urlTask.Exception);
                            onUploaded?.Invoke("", storagePath);
                            return;
                        }

                        string downloadUrl = urlTask.Result.ToString();
                        Debug.Log("[CLOUD MODEL] URL ready: " + downloadUrl);

                        onUploaded?.Invoke(downloadUrl, storagePath);
                    });
            });
    }
}