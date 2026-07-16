using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class UserProfileManager : MonoBehaviour
{
    public static UserProfileManager Instance;

    private DatabaseReference dbRef;

    private void Awake()
    {
        Instance = this;
    }

    public void LoadCurrentUserProfile(System.Action onLoaded = null)
    {
        Debug.Log("[PROFILE DEBUG] LoadCurrentUserProfile called");

        if (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsReady)
        {
            Debug.LogError("[PROFILE DEBUG] FirebaseManager is not ready yet.");
            return;
        }

        dbRef = FirebaseManager.Instance.DatabaseRoot;

        if (dbRef == null)
        {
            Debug.LogError("[PROFILE DEBUG] DatabaseRoot is NULL");
            return;
        }

        FirebaseUser user = FirebaseManager.Instance.Auth.CurrentUser;

        if (user == null)
        {
            Debug.LogWarning("[PROFILE DEBUG] No logged in Firebase user.");
            return;
        }

        string uid = user.UserId;

        Debug.Log("[PROFILE DEBUG] Loading profile for uid: " + uid);

        dbRef.Child("users").Child(uid).GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[PROFILE DEBUG] Failed to load user profile: " + task.Exception);
                    return;
                }

                DataSnapshot snapshot = task.Result;

                if (!snapshot.Exists)
                {
                    Debug.LogWarning("[PROFILE DEBUG] User profile does not exist.");
                    return;
                }

                UserSession.UserId = uid;
                UserSession.Username = snapshot.Child("username").Value?.ToString() ?? "";
                UserSession.Email = snapshot.Child("email").Value?.ToString() ?? "";

                Debug.Log("[PROFILE DEBUG] Username = " + UserSession.Username);
                Debug.Log("[PROFILE DEBUG] Email = " + UserSession.Email);
                Debug.Log("[PROFILE DEBUG] UserSession.UserId = " + UserSession.UserId);

                onLoaded?.Invoke();
            });
    }
}