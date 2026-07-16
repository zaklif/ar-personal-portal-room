using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

public static class CloudModelDownloader
{
    public static IEnumerator DownloadGlb(string url, string fileName, Action<string> onDownloaded)
    {
        if (string.IsNullOrEmpty(url))
        {
            onDownloaded?.Invoke("");
            yield break;
        }

        string modelsFolder = Path.Combine(Application.persistentDataPath, "UploadedModels");

        if (!Directory.Exists(modelsFolder))
            Directory.CreateDirectory(modelsFolder);

        string savePath = Path.Combine(modelsFolder, fileName);

        Debug.Log("[CLOUD DOWNLOAD] Downloading: " + url);
        Debug.Log("[CLOUD DOWNLOAD] Saving to: " + savePath);

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            request.downloadHandler = new DownloadHandlerFile(savePath);

            yield return request.SendWebRequest();

            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("[CLOUD DOWNLOAD] Failed: " + request.error);
                onDownloaded?.Invoke("");
                yield break;
            }
        }

        Debug.Log("[CLOUD DOWNLOAD] Done: " + savePath);
        onDownloaded?.Invoke(savePath);
    }
}