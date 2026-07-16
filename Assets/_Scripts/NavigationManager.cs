//using UnityEngine;
//using UnityEngine.SceneManagement;
//using UnityEngine.UI;

//public class NavigationManager : MonoBehaviour
//{
//    [Header("Scene Names")]
//    [SerializeField] private string homeSceneName = "HomeScreen";

//    [Header("Confirm Leave Panel")]
//    [SerializeField] private GameObject confirmLeavePanel;
//    [SerializeField] private Button confirmLeaveYesButton;
//    [SerializeField] private Button confirmLeaveNoButton;

//    [Header("UI to restore when staying")]
//    [SerializeField] private GameObject bottomBar; // drag your BottomBar here

//    void Start()
//    {
//        if (confirmLeavePanel != null)
//        {
//            confirmLeavePanel.SetActive(false);
//            if (confirmLeaveYesButton != null)
//                confirmLeaveYesButton.onClick.AddListener(ConfirmGoHome);
//            if (confirmLeaveNoButton != null)
//                confirmLeaveNoButton.onClick.AddListener(CancelGoHome);
//        }
//    }

//    public void GoToHome()
//    {
//        // Hide bottom bar while confirm panel is showing
//        if (bottomBar != null) bottomBar.SetActive(false);

//        if (confirmLeavePanel != null)
//        {
//            confirmLeavePanel.SetActive(true);
//            return;
//        }

//        LoadHome();
//    }

//    void ConfirmGoHome()
//    {
//        if (confirmLeavePanel != null)
//            confirmLeavePanel.SetActive(false);
//        LoadHome();
//    }

//    void CancelGoHome()
//    {
//        // FIX: restore bottom bar when user chooses to stay
//        if (confirmLeavePanel != null)
//            confirmLeavePanel.SetActive(false);

//        // Only show bottom bar if inside room and is owner
//        if (bottomBar != null && RoomInsideDetector.IsInsideRoom && RoomManager.IsOwner)
//            bottomBar.SetActive(true);

//        Debug.Log("[NAV] Cancelled — staying in room, bottom bar restored.");
//    }

//    void LoadHome()
//    {
//        PlayerPrefs.DeleteKey("RoomIntent");
//        PlayerPrefs.DeleteKey("ActiveRoomId");
//        PlayerPrefs.DeleteKey("ChosenTemplateName");
//        PlayerPrefs.Save();
//        SceneManager.LoadScene(homeSceneName);

//        Debug.Log("[NAV] Returning to HomeScreen");
//    }

//    void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.Escape)) GoToHome();
//    }
//}

using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class NavigationManager : MonoBehaviour
{
    [Header("Scene Names")]
    [SerializeField] private string homeSceneName = "HomeScreen";

    [Header("Confirm Leave Panel")]
    [SerializeField] private GameObject confirmLeavePanel;
    [SerializeField] private Button confirmLeaveYesButton;
    [SerializeField] private Button confirmLeaveNoButton;

    [Header("UI to restore when staying")]
    [SerializeField] private GameObject bottomBar; // drag your BottomBar here

    void Start()
    {
        if (confirmLeavePanel != null)
        {
            confirmLeavePanel.SetActive(false);
            if (confirmLeaveYesButton != null)
                confirmLeaveYesButton.onClick.AddListener(ConfirmGoHome);
            if (confirmLeaveNoButton != null)
                confirmLeaveNoButton.onClick.AddListener(CancelGoHome);
        }
    }

    public void GoToHome()
    {
        // Hide bottom bar while confirm panel is showing
        if (bottomBar != null) bottomBar.SetActive(false);

        if (confirmLeavePanel != null)
        {
            confirmLeavePanel.SetActive(true);
            return;
        }

        LoadHome();
    }

    void ConfirmGoHome()
    {
        if (confirmLeavePanel != null)
            confirmLeavePanel.SetActive(false);
        LoadHome();
    }

    void CancelGoHome()
    {
        // FIX: restore bottom bar when user chooses to stay
        if (confirmLeavePanel != null)
            confirmLeavePanel.SetActive(false);

        // Only show bottom bar if inside room and is owner
        if (bottomBar != null && RoomInsideDetector.IsInsideRoom && RoomManager.IsOwner)
            bottomBar.SetActive(true);

        Debug.Log("[NAV] Cancelled — staying in room, bottom bar restored.");
    }

    void LoadHome()
    {
        RoomPresenceManager presence =
            FindFirstObjectByType<RoomPresenceManager>();

        if (presence != null)
            presence.LeaveRoom();

        PlayerPrefs.DeleteKey("RoomIntent");
        PlayerPrefs.DeleteKey("ActiveRoomId");
        PlayerPrefs.DeleteKey("ChosenTemplateName");
        PlayerPrefs.Save();

        SceneManager.LoadScene(homeSceneName);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape)) GoToHome();
    }
}