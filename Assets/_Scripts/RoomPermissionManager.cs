using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPermissionManager : MonoBehaviour
{
    [Header("Visitor UI")]
    public GameObject requestEditButton;
    public TMP_Text permissionStatusText; // Used as the requestButtonText in your pseudo-code

    private DatabaseReference dbRef;
    private string roomId;
    private string uid;
    private string username;
    private bool isOwner;
    private Button cachedRequestButton; // Cached to safely change interactivity

    [Header("Owner UI")]
    public GameObject openRequestPanelButton;
    public GameObject requestPanel;
    public Transform requestContent;
    public GameObject requestItemPrefab;
    public Button closeRequestPanelButton;

    IEnumerator Start()
    {
        yield return new WaitUntil(() =>
            RoomManager.Instance != null &&
            RoomManager.Instance.CurrentRoom != null
        );

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        roomId = RoomManager.Instance.CurrentRoom.roomId;
        isOwner = RoomManager.IsOwner;

        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogError("[PERMISSION] No logged in user.");
            yield break;
        }

        uid = user.UserId;
        username = string.IsNullOrEmpty(UserSession.Username) ? user.Email : UserSession.Username;

        SetupUI();

        if (!isOwner)
            ListenForPermissionStatus(); // Subscribes to the status path

        Debug.Log("[PERMISSION] Ready. Room=" + roomId + " IsOwner=" + isOwner);
    }

    void SetupUI()
    {
        if (requestEditButton != null)
        {
            requestEditButton.SetActive(!isOwner);
            cachedRequestButton = requestEditButton.GetComponent<Button>();

            if (cachedRequestButton != null)
            {
                cachedRequestButton.onClick.RemoveAllListeners();
                cachedRequestButton.onClick.AddListener(RequestEditPermission);
            }
        }

        if (permissionStatusText != null)
            permissionStatusText.text = "Request Edit";

        if (openRequestPanelButton != null)
        {
            openRequestPanelButton.SetActive(isOwner);
            Button btn = openRequestPanelButton.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(OpenRequestPanel);
        }

        if (requestPanel != null)
            requestPanel.SetActive(false);

        if (closeRequestPanelButton != null)
        {
            closeRequestPanelButton.onClick.RemoveAllListeners();
            closeRequestPanelButton.onClick.AddListener(() => requestPanel.SetActive(false));
        }
    }

    void RequestEditPermission()
    {
        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null) return;

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "visitorId", uid },
            { "visitorName", username },
            { "email", user.Email },
            { "status", "pending" },
            { "requestedAt", ServerValue.Timestamp }
        };

        dbRef.Child("editRequests")
            .Child(roomId)
            .Child(uid)
            .SetValueAsync(data)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[PERMISSION] Request failed: " + task.Exception);
                    return;
                }
                Debug.Log("[PERMISSION] Edit request sent.");
            });
    }

    void OpenRequestPanel()
    {
        if (requestPanel != null)
            requestPanel.SetActive(true);

        LoadRequests();
    }

    void LoadRequests()
    {
        if (requestContent == null || requestItemPrefab == null)
        {
            Debug.LogError("[PERMISSION] Request content/prefab missing.");
            return;
        }

        foreach (Transform child in requestContent)
            Destroy(child.gameObject);

        dbRef.Child("editRequests")
            .Child(roomId)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[PERMISSION] Load requests failed: " + task.Exception);
                    return;
                }

                int count = 0;

                foreach (DataSnapshot snap in task.Result.Children)
                {
                    string status = snap.Child("status").Value?.ToString() ?? "";
                    if (status != "pending") continue;

                    string visitorId = snap.Child("visitorId").Value?.ToString() ?? "";
                    string visitorName = snap.Child("visitorName").Value?.ToString() ?? "Visitor";
                    string email = snap.Child("email").Value?.ToString() ?? "";

                    CreateRequestItem(visitorId, visitorName, email);
                    count++;
                }

                Debug.Log("[PERMISSION] Pending requests: " + count);
            });
    }

    void CreateRequestItem(string visitorId, string visitorName, string email)
    {
        GameObject item = Instantiate(requestItemPrefab, requestContent);

        TMP_Text text = item.GetComponentInChildren<TMP_Text>();
        if (text != null)
            text.text = visitorName + "\n" + email;

        Button[] buttons = item.GetComponentsInChildren<Button>();

        foreach (Button btn in buttons)
        {
            if (btn.name.Contains("Approve"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => ApproveRequest(visitorId));
            }

            if (btn.name.Contains("Reject"))
            {
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() => RejectRequest(visitorId));
            }
        }
    }

    void ApproveRequest(string visitorId)
    {
        // NOTE: Status changed from "approved" to "accepted" to match your state logic!
        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "/roomPermissions/" + roomId + "/" + visitorId + "/canEdit", true },
            { "/roomPermissions/" + roomId + "/" + visitorId + "/approvedAt", ServerValue.Timestamp },
            { "/editRequests/" + roomId + "/" + visitorId + "/status", "accepted" }
        };

        dbRef.UpdateChildrenAsync(updates).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                Debug.LogError("[PERMISSION] Approve failed: " + task.Exception);
                return;
            }

            Debug.Log("[PERMISSION] Approved visitor: " + visitorId);
            LoadRequests();
        });
    }

    void RejectRequest(string visitorId)
    {
        dbRef.Child("editRequests")
            .Child(roomId)
            .Child(visitorId)
            .Child("status")
            .SetValueAsync("rejected")
            .ContinueWithOnMainThread(task =>
            {
                Debug.Log("[PERMISSION] Rejected visitor: " + visitorId);
                LoadRequests();
            });
    }

    // ==========================================
    // YOUR NEW CUSTOM STATE LOGIC LIVES HERE
    // ==========================================

    void ListenForPermissionStatus()
    {
        // Changing listener to watch editRequests instead of roomPermissions
        dbRef.Child("editRequests")
            .Child(roomId)
            .Child(uid)
            .Child("status")
            .ValueChanged += OnStatusChanged;
    }

    void OnStatusChanged(object sender, ValueChangedEventArgs e)
    {
        if (!e.Snapshot.Exists || e.Snapshot.Value == null) return;

        string status = e.Snapshot.Value.ToString();

        if (status == "pending")
        {
            if (permissionStatusText != null) permissionStatusText.text = "Pending...";
            if (cachedRequestButton != null) cachedRequestButton.interactable = false;

            RoomAccessManager.SetEditPermission(false);
        }
        else if (status == "accepted")
        {
            if (permissionStatusText != null) permissionStatusText.text = "Editing Enabled";
            if (cachedRequestButton != null) cachedRequestButton.interactable = false;

            RoomAccessManager.SetEditPermission(true);
            UnlockVisitorEditing(); // This automatically executes your ObjectPlacementController functions
        }
        else if (status == "rejected")
        {
            if (permissionStatusText != null) permissionStatusText.text = "Rejected";
            if (cachedRequestButton != null) cachedRequestButton.interactable = true; // Let them try clicking again

            RoomAccessManager.SetEditPermission(false);
        }
    }

    void UnlockVisitorEditing()
    {
        Debug.Log("[PERMISSION] Visitor editing unlocked");

        ObjectPlacementController opc = FindFirstObjectByType<ObjectPlacementController>();
        ShelfPlacementController spc = FindFirstObjectByType<ShelfPlacementController>();

        if (opc != null)
        {
            opc.SetVisitorMode(false);
            opc.ShowBottomBar();
            opc.RefreshBottomDockButtons(); // Added your Refresh function call here
        }

        if (spc != null)
        {
            spc.SetVisitorMode(false);
            if (RoomInsideDetector.IsInsideRoom)
                spc.OnEnteredRoom();
        }
    }

    void OnDestroy()
    {
        // Clean up the dynamic Firebase event listener using the correct path when scene changes
        if (!isOwner && dbRef != null && !string.IsNullOrEmpty(roomId) && !string.IsNullOrEmpty(uid))
        {
            dbRef.Child("editRequests")
                .Child(roomId)
                .Child(uid)
                .Child("status")
                .ValueChanged -= OnStatusChanged;
        }
    }
}