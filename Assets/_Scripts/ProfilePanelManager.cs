using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ProfilePanelManager : MonoBehaviour
{
    [Header("Panel")]
    public GameObject profilePanel;

    [Header("Texts")]
    public TMP_Text usernameText;
    public TMP_Text emailText;
    public TMP_Text roomsCreatedText;

    [Header("Buttons")]
    public Button openProfileButton;
    public Button closeButton;
    public Button logoutButton;

    public TMP_InputField editUsernameInput;
    public Button saveUsernameButton;

    public TMP_Text welcomeText;

    void Start()
    {
        profilePanel.SetActive(false);

        openProfileButton.onClick.RemoveAllListeners();
        openProfileButton.onClick.AddListener(OpenProfile);

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(CloseProfile);

        logoutButton.onClick.RemoveAllListeners();
        logoutButton.onClick.AddListener(Logout);

        saveUsernameButton.onClick.RemoveAllListeners();
        saveUsernameButton.onClick.AddListener(SaveUsername);

    }

    public void OpenProfile()
    {
        profilePanel.SetActive(true);

        usernameText.text = "Username: " + UserSession.Username;
        emailText.text = "Email: " + UserSession.Email;
        roomsCreatedText.text = "Rooms Created: ...";
        editUsernameInput.text = UserSession.Username;

        LoadRoomCount();
    }

    public void CloseProfile()
    {
        profilePanel.SetActive(false);
    }

    void SaveUsername()
    {
        string newUsername = editUsernameInput.text.Trim();

        if (string.IsNullOrEmpty(newUsername))
            return;

        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) return;

        FirebaseDatabase.DefaultInstance.RootReference
            .Child("users")
            .Child(user.UserId)
            .Child("username")
            .SetValueAsync(newUsername)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[PROFILE] Username update failed: " + task.Exception);
                    return;
                }

                UserSession.Username = newUsername;
                usernameText.text = "Username: " + newUsername;

                if (welcomeText != null)
                {
                    welcomeText.text = "Welcome, " + newUsername;
                }

                Debug.Log("[PROFILE] Username updated: " + newUsername);
            });
    }

    void LoadRoomCount()
    {
        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) return;

        FirebaseDatabase.DefaultInstance.RootReference
            .Child("userRooms")
            .Child(user.UserId)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                    return;

                int count = task.Result.Exists ? (int)task.Result.ChildrenCount : 0;
                roomsCreatedText.text = "Rooms Created: " + count;
            });
    }

    void Logout()
    {
        Firebase.Auth.FirebaseAuth.DefaultInstance.SignOut();

        UserSession.UserId = "";
        UserSession.Username = "";
        UserSession.Email = "";

        SceneManager.LoadScene("LoginScene");
    }
}