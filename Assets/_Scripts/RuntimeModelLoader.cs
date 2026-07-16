

//using UnityEngine;
//using GLTFast;
//using System.IO;
//using System.Collections;
//using TMPro;

//public class RuntimeModelLoader : MonoBehaviour
//{
//    [Header("UI")]
//    [SerializeField] private GameObject loadingPanel;
//    [SerializeField] private TMP_Text loadingText;

//    [Header("Controllers")]
//    [SerializeField] private ObjectPlacementController objectPlacementController;

//    // Folder where models are saved persistently
//    private string modelsFolder;

//    void Awake()
//    {
//        // Create UploadedModels folder if it doesn't exist
//        modelsFolder = Path.Combine(Application.persistentDataPath, "UploadedModels");
//        if (!Directory.Exists(modelsFolder))
//            Directory.CreateDirectory(modelsFolder);

//        Debug.Log("[LOADER] UploadedModels folder: " + modelsFolder);
//    }

//    // Called by "Load From Phone" button
//    public void OpenFilePicker()
//    {
//        NativeFilePicker.PickFile(OnFilePicked, new string[] { "*/*" });
//    }

//    void OnFilePicked(string filePath)
//    {
//        if (filePath == null)
//        {
//            Debug.Log("[LOADER] Cancelled — no file picked");
//            return;
//        }

//        Debug.Log("[LOADER] File picked: " + filePath);
//        Debug.Log("[LOADER] File name: " + Path.GetFileName(filePath));

//        if (!File.Exists(filePath))
//        {
//            Debug.LogError("[LOADER] File does NOT exist: " + filePath);
//            return;
//        }

//        Debug.Log("[LOADER] File size: " + new FileInfo(filePath).Length + " bytes");

//        loadingPanel.SetActive(true);
//        loadingText.text = "Loading model...";
//        StartCoroutine(CopyAndLoadModel(filePath));
//    }

//    IEnumerator CopyAndLoadModel(string sourcePath)
//    {
//        // FIX 1: Get the actual filename
//        string fileName = Path.GetFileName(sourcePath);
//        string destPath = Path.Combine(modelsFolder, fileName);

//        // FIX 2: Copy file to persistent UploadedModels folder
//        if (!File.Exists(destPath))
//        {
//            loadingText.text = "Copying file...";
//            yield return null; // let UI update

//            File.Copy(sourcePath, destPath);
//            Debug.Log("[LOADER] Copied to: " + destPath);
//        }
//        else
//        {
//            Debug.Log("[LOADER] File already exists: " + destPath);
//        }

//        // Now load from the persistent path
//        loadingText.text = "Loading model...";
//        yield return StartCoroutine(LoadModel(destPath, fileName));
//    }

//    IEnumerator LoadModel(string filePath, string fileName)
//    {
//        Debug.Log("[LOADER] Loading from: " + filePath);

//        GameObject modelParent = new GameObject("LoadedModel");
//        modelParent.SetActive(false);

//        var gltf = new GltfImport();
//        var loadTask = gltf.Load("file://" + filePath);
//        yield return new WaitUntil(() => loadTask.IsCompleted);

//        Debug.Log("[LOADER] Load result: " + loadTask.Result);

//        if (!loadTask.Result)
//        {
//            Debug.LogError("[LOADER] Failed to load model!");
//            loadingPanel.SetActive(false);
//            Destroy(modelParent);
//            yield break;
//        }

//        var instantiateTask = gltf.InstantiateMainSceneAsync(modelParent.transform);
//        yield return new WaitUntil(() => instantiateTask.IsCompleted);

//        Debug.Log("[LOADER] Model loaded! Children: " + modelParent.transform.childCount);

//        modelParent.transform.localScale = Vector3.one * 0.01f;

//        loadingPanel.SetActive(false);
//        loadingText.text = "Done!";

//        // FIX 3: Pass fileName so ObjectPlacementController saves it correctly
//        // Previously called SetRuntimeModel(modelParent) with no filename
//        // Now passes the actual filename so it gets saved to room data
//        objectPlacementController.SetRuntimeModel(modelParent, fileName, filePath);

//        Debug.Log("[LOADER] Model ready for placement: " + fileName);
//    }
//}



using UnityEngine;
using GLTFast;
using System.IO;
using System.Collections;
using TMPro;

public class RuntimeModelLoader : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text loadingText;

    [Header("Controllers")]
    [SerializeField] private ObjectPlacementController objectPlacementController;

    // Folder where models are saved persistently
    private string modelsFolder;

    void Awake()
    {
        // Create UploadedModels folder if it doesn't exist
        modelsFolder = Path.Combine(Application.persistentDataPath, "UploadedModels");
        if (!Directory.Exists(modelsFolder))
            Directory.CreateDirectory(modelsFolder);

        Debug.Log("[LOADER] UploadedModels folder: " + modelsFolder);
    }

    // Called by "Load From Phone" button
    public void OpenFilePicker()
    {
        doorFramePlacementController roomPlacement =
            FindFirstObjectByType<doorFramePlacementController>();

        if (roomPlacement != null)
            roomPlacement.SaveCurrentRoomPoseAsStable();

        NativeFilePicker.PickFile(OnFilePicked, new string[] { "*/*" });
    }

    void OnFilePicked(string filePath)
    {
        StartCoroutine(RestoreRoomPoseAfterFilePicker());

        if (filePath == null)
        {
            Debug.Log("[LOADER] Cancelled — no file picked");
            return;
        }

        Debug.Log("[LOADER] File picked: " + filePath);
        Debug.Log("[LOADER] File name: " + Path.GetFileName(filePath));

        if (!File.Exists(filePath))
        {
            Debug.LogError("[LOADER] File does NOT exist: " + filePath);
            return;
        }

        Debug.Log("[LOADER] File size: " + new FileInfo(filePath).Length + " bytes");

        loadingPanel.SetActive(true);
        loadingText.text = "Loading model...";
        StartCoroutine(CopyAndLoadModel(filePath));
    }

    IEnumerator RestoreRoomPoseAfterFilePicker()
    {
        yield return new WaitForSeconds(0.25f);

        doorFramePlacementController roomPlacement =
            FindFirstObjectByType<doorFramePlacementController>();

        if (roomPlacement != null)
            roomPlacement.RestoreLastStablePose();

        yield return new WaitForSeconds(0.5f);

        if (roomPlacement != null)
            roomPlacement.RestoreLastStablePose();
    }

    IEnumerator CopyAndLoadModel(string sourcePath)
    {
        // FIX 1: Get the actual filename
        string fileName = Path.GetFileName(sourcePath);
        string destPath = Path.Combine(modelsFolder, fileName);

        // FIX 2: Copy file to persistent UploadedModels folder
        if (!File.Exists(destPath))
        {
            loadingText.text = "Copying file...";
            yield return null; // let UI update

            File.Copy(sourcePath, destPath);
            Debug.Log("[LOADER] Copied to: " + destPath);
        }
        else
        {
            Debug.Log("[LOADER] File already exists: " + destPath);
        }

        // Now load from the persistent path
        loadingText.text = "Loading model...";
        yield return StartCoroutine(LoadModel(destPath, fileName));
    }

    IEnumerator LoadModel(string filePath, string fileName)
    {
        Debug.Log("[LOADER] Loading from: " + filePath);

        GameObject modelParent = new GameObject("LoadedModel");
        modelParent.SetActive(false);

        var gltf = new GltfImport();
        var loadTask = gltf.Load("file://" + filePath);
        yield return new WaitUntil(() => loadTask.IsCompleted);

        Debug.Log("[LOADER] Load result: " + loadTask.Result);

        if (!loadTask.Result)
        {
            Debug.LogError("[LOADER] Failed to load model!");
            loadingPanel.SetActive(false);
            Destroy(modelParent);
            yield break;
        }

        var instantiateTask = gltf.InstantiateMainSceneAsync(modelParent.transform);
        yield return new WaitUntil(() => instantiateTask.IsCompleted);

        Debug.Log("[LOADER] Model loaded! Children: " + modelParent.transform.childCount);

        modelParent.transform.localScale = Vector3.one * 0.01f;

        loadingPanel.SetActive(false);
        loadingText.text = "Done!";

        // FIX 3: Pass fileName so ObjectPlacementController saves it correctly
        // Previously called SetRuntimeModel(modelParent) with no filename
        // Now passes the actual filename so it gets saved to room data
        objectPlacementController.SetRuntimeModel(modelParent, fileName, filePath);

        Debug.Log("[LOADER] Model ready for placement: " + fileName);
    }
}