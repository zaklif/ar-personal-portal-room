//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;
//using UnityEngine.SceneManagement;

///// <summary>
///// Updated HomeScreenController.
///// 
///// CHANGES FROM PREVIOUS VERSION:
///// - "Create My Room" now opens RoomTemplateSelector first
///// - After template is picked, THEN creates room and loads AR scene
///// - "Visit a Room" flow unchanged
///// </summary>

//public class HomeScreenController : MonoBehaviour
//{
//    [Header("UI References")]
//    [SerializeField] private TMP_InputField roomIdInputField;
//    [SerializeField] private Button createRoomButton;
//    [SerializeField] private Button visitRoomButton;
//    [SerializeField] private TMP_Text errorText;
//    [SerializeField] private TMP_Text deviceIdText;

//    [Header("Room Template Selector")]
//    [SerializeField] private RoomTemplateSelector templateSelector;

//    [Header("Scene Names")]
//    [SerializeField] private string arSceneName = "SampleScene";

//    void Start()
//    {
//        errorText.gameObject.SetActive(false);

//        if (deviceIdText != null && RoomManager.Instance != null)
//            deviceIdText.text = "ID: " + RoomManager.Instance.GetDeviceId();

//        createRoomButton.onClick.AddListener(OnCreateRoomPressed);
//        visitRoomButton.onClick.AddListener(OnVisitRoomPressed);

//        roomIdInputField.characterLimit = 6;
//        roomIdInputField.characterValidation = TMP_InputField.CharacterValidation.Alphanumeric;
//    }

//    void OnCreateRoomPressed()
//    {
//        HideError();
//        // Open template selector — fires OnTemplateChosen when user picks
//        templateSelector.Show(OnTemplateChosen);
//    }

//    void OnTemplateChosen(RoomTemplateData template)
//    {
//        RoomData newRoom = RoomManager.Instance.CreateNewRoom(template.templateName);

//        PlayerPrefs.SetString("RoomIntent", "create");
//        PlayerPrefs.SetString("ActiveRoomId", newRoom.roomId);
//        PlayerPrefs.SetString("ChosenTemplateName", template.templateName);
//        PlayerPrefs.Save();

//        Debug.Log($"[HOME] Creating room {newRoom.roomId} with template '{template.templateName}'");
//        LoadARScene();
//    }

//    void OnVisitRoomPressed()
//    {
//        HideError();

//        string inputId = roomIdInputField.text.Trim().ToUpper();

//        if (inputId.Length != 6)
//        {
//            ShowError("Room ID must be 6 characters.");
//            return;
//        }

//        RoomData room = RoomManager.Instance.LoadRoom(inputId);
//        if (room == null)
//        {
//            ShowError($"Room '{inputId}' not found.\nCheck the ID and try again.");
//            return;
//        }

//        PlayerPrefs.SetString("RoomIntent", "visit");
//        PlayerPrefs.SetString("ActiveRoomId", inputId);
//        PlayerPrefs.SetString("ChosenTemplateName", room.roomTemplateName);
//        PlayerPrefs.Save();

//        Debug.Log($"[HOME] Visiting room {inputId} (template: {room.roomTemplateName})");
//        LoadARScene();
//    }

//    void LoadARScene() => SceneManager.LoadScene(arSceneName);
//    void ShowError(string msg) { errorText.text = msg; errorText.gameObject.SetActive(true); }
//    void HideError() => errorText.gameObject.SetActive(false);
//}


using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class HomeScreenController : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TMP_InputField roomIdInputField;
    [SerializeField] private Button createRoomButton;
    [SerializeField] private Button visitRoomButton;
    [SerializeField] private TMP_Text errorText;
    [SerializeField] private TMP_Text deviceIdText;

    [Header("Room Template Selector")]
    [SerializeField] private RoomTemplateSelector templateSelector;

    [Header("Scene Names")]
    [SerializeField] private string arSceneName = "SampleScene";

    private void Start()
    {
        HideError();

        if (deviceIdText != null && RoomManager.Instance != null)
            deviceIdText.text = "ID: " + RoomManager.Instance.GetDeviceId();

        createRoomButton.onClick.RemoveAllListeners();
        visitRoomButton.onClick.RemoveAllListeners();

        createRoomButton.onClick.AddListener(OnCreateRoomPressed);
        visitRoomButton.onClick.AddListener(OnVisitRoomPressed);

        roomIdInputField.characterLimit = 6;
        roomIdInputField.characterValidation = TMP_InputField.CharacterValidation.Alphanumeric;
    }

    private void OnCreateRoomPressed()
    {
        HideError();

        if (templateSelector == null)
        {
            ShowError("Template selector not assigned.");
            return;
        }

        templateSelector.Show(OnTemplateChosen);
    }

    private void OnTemplateChosen(RoomTemplateData template)
    {
        RoomCloudManager cloud = Object.FindFirstObjectByType<RoomCloudManager>();

        if (cloud == null)
        {
            ShowError("RoomCloudManager not found.");
            return;
        }

        cloud.CreateRoomFromTemplate(template.templateName);
    }

    private void OnVisitRoomPressed()
    {
        HideError();

        string inputId = roomIdInputField.text.Trim().ToUpper();

        if (inputId.Length != 6)
        {
            ShowError("Room ID must be 6 characters.");
            return;
        }

        RoomData room = RoomManager.Instance.LoadRoom(inputId);

        if (room == null)
        {
            ShowError($"Room '{inputId}' not found.\nCheck the ID and try again.");
            return;
        }

        PlayerPrefs.SetString("RoomIntent", "visit");
        PlayerPrefs.SetString("ActiveRoomId", inputId);
        PlayerPrefs.SetString("ChosenTemplateName", room.roomTemplateName);
        PlayerPrefs.Save();

        Debug.Log($"[HOME] Visiting local room {inputId} template={room.roomTemplateName}");

        LoadARScene();
    }

    private void LoadARScene()
    {
        SceneManager.LoadScene(arSceneName);
    }

    private void ShowError(string msg)
    {
        if (errorText == null) return;

        errorText.text = msg;
        errorText.gameObject.SetActive(true);
    }

    private void HideError()
    {
        if (errorText != null)
            errorText.gameObject.SetActive(false);
    }
}