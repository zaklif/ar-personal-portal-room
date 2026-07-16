//using UnityEngine;
//using System;
//using System.IO;
//using System.Collections;

//public class GlbFileLoader : MonoBehaviour
//{
//    [Header("References")]
//    [SerializeField] private ObjectPlacementController objectPlacementController;

//    private string modelsFolder;

//    void Awake()
//    {
//        modelsFolder = Path.Combine(Application.persistentDataPath, "UploadedModels");
//        if (!Directory.Exists(modelsFolder))
//            Directory.CreateDirectory(modelsFolder);
//        Debug.Log("[GLB] Models folder: " + modelsFolder);
//    }

//    // Called by your file picker button
//    public void OnFilePickedFromDevice(string sourcePath)
//    {
//        StartCoroutine(CopyAndLoad(sourcePath));
//    }

//    IEnumerator CopyAndLoad(string sourcePath)
//    {
//        string fileName = Path.GetFileName(sourcePath);
//        string destPath = Path.Combine(modelsFolder, fileName);

//        if (!File.Exists(destPath))
//        {
//            File.Copy(sourcePath, destPath);
//            Debug.Log($"[GLB] Copied to: {destPath}");
//        }

//        yield return StartCoroutine(LoadGlbCoroutine(destPath, (obj) =>
//        {
//            if (obj != null)
//            {
//                objectPlacementController.SetRuntimeModel(obj, fileName);
//                Debug.Log("[GLB] Model ready: " + fileName);
//            }
//            else
//                Debug.LogError("[GLB] Failed: " + fileName);
//        }));
//    }

//    // Called by RoomLoader to restore saved objects
//    public void LoadFile(string fullPath, Action<GameObject> onLoaded)
//    {
//        StartCoroutine(LoadGlbCoroutine(fullPath, onLoaded));
//    }

//    IEnumerator LoadGlbCoroutine(string filePath, Action<GameObject> onLoaded)
//    {
//        if (!File.Exists(filePath))
//        {
//            Debug.LogError("[GLB] File not found: " + filePath);
//            onLoaded?.Invoke(null);
//            yield break;
//        }

//        var gltf = new GLTFast.GltfImport();
//        var loadTask = gltf.Load("file://" + filePath);
//        yield return new WaitUntil(() => loadTask.IsCompleted);

//        if (!loadTask.Result)
//        {
//            Debug.LogError("[GLB] Load failed: " + filePath);
//            onLoaded?.Invoke(null);
//            yield break;
//        }

//        GameObject container = new GameObject(Path.GetFileNameWithoutExtension(filePath));
//        var instantiateTask = gltf.InstantiateMainSceneAsync(container.transform);
//        yield return new WaitUntil(() => instantiateTask.IsCompleted);

//        if (!instantiateTask.Result)
//        {
//            Destroy(container);
//            onLoaded?.Invoke(null);
//            yield break;
//        }

//        onLoaded?.Invoke(container);
//    }
//}

using UnityEngine;
using System;
using System.IO;
using System.Collections;

public class GlbFileLoader : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ObjectPlacementController objectPlacementController;

    private string modelsFolder;

    void Awake()
    {
        modelsFolder = Path.Combine(Application.persistentDataPath, "UploadedModels");
        if (!Directory.Exists(modelsFolder))
            Directory.CreateDirectory(modelsFolder);
        Debug.Log("[GLB] Models folder: " + modelsFolder);
    }

    // Called by your file picker button
    public void OnFilePickedFromDevice(string sourcePath)
    {
        StartCoroutine(CopyAndLoad(sourcePath));
    }

    IEnumerator CopyAndLoad(string sourcePath)
    {
        string fileName = Path.GetFileName(sourcePath);
        string destPath = Path.Combine(modelsFolder, fileName);

        if (!File.Exists(destPath))
        {
            File.Copy(sourcePath, destPath);
            Debug.Log($"[GLB] Copied to: {destPath}");
        }

        yield return StartCoroutine(LoadGlbCoroutine(destPath, (obj) =>
        {
            if (obj != null)
            {
                objectPlacementController.SetRuntimeModel(obj, fileName, destPath);
                Debug.Log("[GLB] Model ready: " + fileName);
            }
            else
                Debug.LogError("[GLB] Failed: " + fileName);
        }));
    }

    // Called by RoomLoader to restore saved objects
    public void LoadFile(string fullPath, Action<GameObject> onLoaded)
    {
        StartCoroutine(LoadGlbCoroutine(fullPath, onLoaded));
    }

    IEnumerator LoadGlbCoroutine(string filePath, Action<GameObject> onLoaded)
    {
        if (!File.Exists(filePath))
        {
            Debug.LogError("[GLB] File not found: " + filePath);
            onLoaded?.Invoke(null);
            yield break;
        }

        var gltf = new GLTFast.GltfImport();
        var loadTask = gltf.Load("file://" + filePath);
        yield return new WaitUntil(() => loadTask.IsCompleted);

        if (!loadTask.Result)
        {
            Debug.LogError("[GLB] Load failed: " + filePath);
            onLoaded?.Invoke(null);
            yield break;
        }

        GameObject container = new GameObject(Path.GetFileNameWithoutExtension(filePath));
        var instantiateTask = gltf.InstantiateMainSceneAsync(container.transform);
        yield return new WaitUntil(() => instantiateTask.IsCompleted);

        if (!instantiateTask.Result)
        {
            Destroy(container);
            onLoaded?.Invoke(null);
            yield break;
        }

        onLoaded?.Invoke(container);
    }
}