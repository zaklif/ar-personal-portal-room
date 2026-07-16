
//using Firebase.Database;
//using Firebase.Extensions;
//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;

//public class OwnerInboxManager : MonoBehaviour
//{
//    [Header("Panel")]
//    public GameObject inboxPanel;
//    public Button inboxButton;
//    public Button closeButton;

//    [Header("List")]
//    public Transform content;
//    public GameObject chatListItemPrefab;

//    [Header("Chat")]
//    public InAppChat inAppChat;

//    private DatabaseReference dbRef;

//    void Start()
//    {
//        inboxPanel.SetActive(false);

//        inboxButton.onClick.AddListener(OpenInbox);
//        closeButton.onClick.AddListener(CloseInbox);

//        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
//    }

//    public void OpenInbox()
//    {
//        inboxPanel.SetActive(true);
//        LoadOwnerChats();
//    }

//    public void CloseInbox()
//    {
//        inboxPanel.SetActive(false);
//    }

//    void LoadOwnerChats()
//    {
//        foreach (Transform child in content)
//            Destroy(child.gameObject);

//        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;

//        if (user == null)
//        {
//            Debug.LogError("[INBOX] User not logged in.");
//            return;
//        }

//        string uid = user.UserId;

//        LoadChatsByField("ownerId", uid, "Owner");
//        LoadChatsByField("visitorId", uid, "Visitor");
//    }

//    void LoadChatsByField(string fieldName, string uid, string roleLabel)
//    {
//        dbRef.Child("chats")
//            .OrderByChild(fieldName)
//            .EqualTo(uid)
//            .GetValueAsync()
//            .ContinueWithOnMainThread(task =>
//            {
//                if (task.IsFaulted || task.IsCanceled)
//                {
//                    Debug.LogError("[INBOX] Failed loading " + roleLabel + " chats: " + task.Exception);
//                    return;
//                }

//                DataSnapshot snapshot = task.Result;

//                foreach (DataSnapshot chatSnap in snapshot.Children)
//                {
//                    CreateChatItem(chatSnap, roleLabel);
//                }

//                Debug.Log("[INBOX] Loaded " + roleLabel + " chats: " + snapshot.ChildrenCount);
//            });
//    }

//    void CreateChatItem(DataSnapshot chatSnap, string roleLabel)
//    {
//        GameObject item = Instantiate(chatListItemPrefab, content);

//        string chatId = chatSnap.Child("chatId").Value?.ToString() ?? "";
//        string objectName = chatSnap.Child("objectName").Value?.ToString() ?? "Object";
//        string visitorName = chatSnap.Child("visitorName").Value?.ToString() ?? "Visitor";
//        string lastMessage = chatSnap.Child("lastMessage").Value?.ToString() ?? "";

//        TMP_Text[] texts = item.GetComponentsInChildren<TMP_Text>();

//        foreach (TMP_Text t in texts)
//        {
//            if (t.name == "ObjectNameText")
//                t.text = "[" + roleLabel + "] " + objectName;

//            if (t.name == "VisitorNameText")
//                t.text = roleLabel == "Owner"
//                    ? "From: " + visitorName
//                    : "You asked about this item";

//            if (t.name == "LastMessageText")
//                t.text = lastMessage;
//        }

//        Button openButton = item.GetComponentInChildren<Button>();
//        openButton.onClick.AddListener(() =>
//        {
//            inboxPanel.SetActive(false);
//            inAppChat.OpenChatById(chatId, objectName);
//        });
//    }
//}
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class OwnerInboxManager : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject inboxPanel;

    [Header("Buttons")]
    [SerializeField] private Button inboxButton;
    [SerializeField] private Button closeButton;

    [Header("List")]
    [SerializeField] private Transform content;
    [SerializeField] private GameObject chatListItemPrefab;
    [SerializeField] private TMP_Text conversationCountText;

    [Header("Chat")]
    [SerializeField] private InAppChat inAppChat;

    private DatabaseReference dbRef;
    private int loadedChatCount = 0;

    private HashSet<string> loadedChatIds = new HashSet<string>();

    private void Start()
    {
        if (inboxPanel != null)
            inboxPanel.SetActive(false);

        if (inboxButton != null)
        {
            inboxButton.onClick.RemoveAllListeners();
            inboxButton.onClick.AddListener(OpenInbox);
        }
        else
        {
            Debug.LogWarning("[INBOX] inboxButton is not assigned.");
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseInbox);
        }
        else
        {
            Debug.LogWarning("[INBOX] closeButton is not assigned.");
        }

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;
    }

    public void OpenInbox()
    {
        if (homePanel != null)
            homePanel.SetActive(false);

        if (inboxPanel != null)
        {
            inboxPanel.SetActive(true);
            inboxPanel.transform.SetAsLastSibling();
        }

        LoadOwnerChats();
    }

    public void CloseInbox()
    {
        if (inboxPanel != null)
            inboxPanel.SetActive(false);

        if (homePanel != null)
            homePanel.SetActive(true);
    }

    public void ShowInboxFromChat()
    {
        if (homePanel != null)
            homePanel.SetActive(false);

        if (inboxPanel != null)
        {
            inboxPanel.SetActive(true);
            inboxPanel.transform.SetAsLastSibling();
        }

        LoadOwnerChats();
    }

    private void LoadOwnerChats()
    {
        loadedChatCount = 0;
        loadedChatIds.Clear();
        RefreshConversationCount();

        foreach (Transform child in content)
            Destroy(child.gameObject);

        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;

        if (user == null)
        {
            Debug.LogError("[INBOX] User not logged in.");
            return;
        }

        string uid = user.UserId;

        LoadChatsByField("ownerId", uid, "Owner");
        LoadChatsByField("visitorId", uid, "Visitor");
    }

    private void LoadChatsByField(string fieldName, string uid, string roleLabel)
    {
        if (dbRef == null)
            dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        dbRef.Child("chats")
            .OrderByChild(fieldName)
            .EqualTo(uid)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[INBOX] Failed loading " + roleLabel + " chats: " + task.Exception);
                    return;
                }

                DataSnapshot snapshot = task.Result;

                foreach (DataSnapshot chatSnap in snapshot.Children)
                {
                    CreateChatItem(chatSnap, roleLabel);
                    loadedChatCount++;
                }

                RefreshConversationCount();

                Debug.Log("[INBOX] Loaded " + roleLabel + " chats: " + snapshot.ChildrenCount);
            });
    }

    private void CreateChatItem(DataSnapshot chatSnap, string roleLabel)
    {
        if (chatListItemPrefab == null)
        {
            Debug.LogError("[INBOX] chatListItemPrefab is not assigned.");
            return;
        }

        if (content == null)
        {
            Debug.LogError("[INBOX] content is not assigned.");
            return;
        }

        GameObject item = Instantiate(chatListItemPrefab, content);

        string chatId = GetString(chatSnap, "chatId", "");

        if (string.IsNullOrEmpty(chatId))
        {
            Debug.LogWarning("[INBOX] Skipped chat with empty chatId.");
            return;
        }

        if (loadedChatIds.Contains(chatId))
        {
            Debug.Log("[INBOX] Duplicate skipped: " + chatId);
            return;
        }

        loadedChatIds.Add(chatId);
        string objectName = GetString(chatSnap, "objectName", "Object");
        string visitorName = GetString(chatSnap, "visitorName", "Visitor");
        string lastMessage = GetString(chatSnap, "lastMessage", "");
        long updatedAt = GetLong(chatSnap, "updatedAt", 0);

        string title = "Re: " + objectName;

        string preview;
        string subtitle;
        string avatarName;

        if (roleLabel == "Owner")
        {
            preview = visitorName + ": " + lastMessage;
            subtitle = "From " + visitorName;
            avatarName = visitorName;
        }
        else
        {
            preview = "You: " + lastMessage;
            subtitle = "You asked about this item";
            avatarName = "You";
        }

        SetText(item, "AvatarText", GetInitial(avatarName));
        SetText(item, "ObjectNameText", title);
        SetText(item, "VisitorNameText", subtitle);
        SetText(item, "LastMessageText", preview);
        SetText(item, "TimeText", FormatRelativeTime(updatedAt));

        Transform unreadDot = FindChildRecursive(item.transform, "UnreadDot");
        if (unreadDot != null)
            unreadDot.gameObject.SetActive(false);

        Button openButton = item.GetComponent<Button>();

        if (openButton == null)
            openButton = item.GetComponentInChildren<Button>(true);

        if (openButton == null)
        {
            Debug.LogError("[INBOX] Chat item prefab has no Button component.");
            return;
        }

        openButton.onClick.RemoveAllListeners();
        openButton.onClick.AddListener(() =>
        {
            if (string.IsNullOrEmpty(chatId))
            {
                Debug.LogWarning("[INBOX] Cannot open chat because chatId is empty.");
                return;
            }

            if (inboxPanel != null)
                inboxPanel.SetActive(false);

            if (inAppChat != null)
                inAppChat.OpenChatById(chatId, objectName);
            else
                Debug.LogError("[INBOX] InAppChat is not assigned.");
        });
    }

    private void RefreshConversationCount()
    {
        if (conversationCountText == null)
            return;

        conversationCountText.text = loadedChatCount == 1
            ? "1 conversation"
            : loadedChatCount + " conversations";
    }

    private void SetText(GameObject root, string childName, string value)
    {
        TMP_Text[] texts = root.GetComponentsInChildren<TMP_Text>(true);

        foreach (TMP_Text text in texts)
        {
            if (text.name == childName)
            {
                text.text = value;
                return;
            }
        }

        Debug.LogWarning("[INBOX] Text not found in prefab: " + childName);
    }

    private Transform FindChildRecursive(Transform parent, string childName)
    {
        foreach (Transform child in parent)
        {
            if (child.name == childName)
                return child;

            Transform found = FindChildRecursive(child, childName);

            if (found != null)
                return found;
        }

        return null;
    }

    private string GetString(DataSnapshot snap, string key, string fallback)
    {
        object value = snap.Child(key).Value;

        if (value == null)
            return fallback;

        return value.ToString();
    }

    private long GetLong(DataSnapshot snap, string key, long fallback)
    {
        object value = snap.Child(key).Value;

        if (value == null)
            return fallback;

        if (long.TryParse(value.ToString(), out long result))
            return result;

        return fallback;
    }

    private string GetInitial(string name)
    {
        if (string.IsNullOrEmpty(name))
            return "?";

        return name.Substring(0, 1).ToUpper();
    }

    private string FormatRelativeTime(long firebaseTimestamp)
    {
        if (firebaseTimestamp <= 0)
            return "";

        long nowMs = System.DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        long diffMs = System.Math.Max(0L, nowMs - firebaseTimestamp); ;

        double minutes = diffMs / 1000.0 / 60.0;

        if (minutes < 1)
            return "now";

        if (minutes < 60)
            return Mathf.FloorToInt((float)minutes) + "m ago";

        double hours = minutes / 60.0;

        if (hours < 24)
            return Mathf.FloorToInt((float)hours) + "h ago";

        double days = hours / 24.0;

        return Mathf.FloorToInt((float)days) + "d ago";
    }
}