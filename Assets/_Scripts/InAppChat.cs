using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Firebase.Database;
using Firebase.Extensions;

public class InAppChat : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject chatPanel;

    [Header("Navigation")]
    [SerializeField] private OwnerInboxManager ownerInboxManager;
    [SerializeField] private Button backButton;

    [Header("Header - Old / Existing")]
    [SerializeField] private TMP_Text chatTitleText;
    [SerializeField] private TMP_Text itemInfoText;
    [SerializeField] private Button closeButton;

    [Header("Header - New Design")]
    [SerializeField] private TMP_Text senderNameText;
    [SerializeField] private TMP_Text objectNameText;
    [SerializeField] private TMP_Text senderInitialText;

    [Header("Messages")]
    [SerializeField] private Transform messageContainer;
    [SerializeField] private ScrollRect scrollRect;

    [Header("Message Prefabs")]
    [SerializeField] private GameObject leftBubblePrefab;
    [SerializeField] private GameObject rightBubblePrefab;

    [Header("Input")]
    [SerializeField] private TMP_InputField messageInputField;
    [SerializeField] private Button sendButton;

    [Header("Note")]
    [SerializeField] private TMP_Text noteText;

    private readonly List<ChatMessage> messages = new List<ChatMessage>();

    private string currentChatId = "";
    private string currentRoomId = "";
    private string currentObjectId = "";
    private string currentOwnerId = "";
    private string currentItemName = "";

    private bool isListening = false;

    private void Start()
    {
        if (chatPanel != null)
            chatPanel.SetActive(false);

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(BackToInbox);
        }

        if (sendButton != null)
        {
            sendButton.onClick.RemoveAllListeners();
            sendButton.onClick.AddListener(OnSendPressed);
        }

        if (messageInputField != null)
        {
            messageInputField.onSubmit.RemoveAllListeners();
            messageInputField.onSubmit.AddListener((_) => OnSendPressed());
        }

        if (noteText != null)
            noteText.gameObject.SetActive(false);

        Debug.Log("[CHAT INSTANCE] Start instance=" + GetInstanceID() + " obj=" + gameObject.name);
    }

    private void OnDisable()
    {
        StopListening();
    }

    public void OpenChat(ObjectMetaData meta)
    {
        if (meta == null)
        {
            Debug.LogWarning("[CHAT] Cannot open: meta is null.");
            return;
        }

        StopListening();
        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;

        if (user == null)
        {
            Debug.LogError("[CHAT] User not logged in.");
            return;
        }

        currentOwnerId = meta.ownerId;
        currentRoomId = meta.ownerRoomId;
        currentObjectId = meta.instanceId;
        currentItemName = meta.objectName;
        currentChatId = FirebaseChatService.GetChatId(
            currentRoomId,
            currentObjectId,
            user.UserId
        );

        ClearMessagesUI();
        ShowChatPanel();

        string objectSubtitle = meta.objectType == ObjectMetaData.ObjectType.ForSale
            ? $"Re: {meta.objectName} — {meta.currency} {meta.price:F2}"
            : "Re: " + meta.objectName;

        SetHeader(
            mainName: "Room Owner",
            objectSubtitle: objectSubtitle,
            initial: "O"
        );

        ListenForMessages();
        Debug.Log("[CHAT] Opened Firebase chat: " + currentChatId);
    }

    public void OpenChatById(string chatId, string objectName)
    {
        if (string.IsNullOrEmpty(chatId))
        {
            Debug.LogWarning("[CHAT] Cannot open chat: chatId is empty.");
            return;
        }

        StopListening();

        currentChatId = chatId;
        currentRoomId = "";
        currentObjectId = "";
        currentOwnerId = "";
        currentItemName = objectName;

        ClearMessagesUI();
        ShowChatPanel();

        if (sendButton != null)
            sendButton.interactable = false;

        SetHeader(
            mainName: "Loading...",
            objectSubtitle: "Re: " + objectName,
            initial: "?"
        );

        Debug.Log("[CHAT INSTANCE] OpenChatById instance=" + GetInstanceID());

        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        dbRef.Child("chats").Child(chatId).GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[CHAT] Failed to load chat info: " + task.Exception);

                    if (sendButton != null)
                        sendButton.interactable = false;

                    return;
                }

                DataSnapshot snap = task.Result;

                currentRoomId = GetString(snap, "roomId", "");
                currentObjectId = GetString(snap, "objectId", "");
                currentOwnerId = GetString(snap, "ownerId", "");
                currentItemName = GetString(snap, "objectName", objectName);

                string visitorName = GetString(snap, "visitorName", "Visitor");

                SetHeader(
                    mainName: visitorName,
                    objectSubtitle: "Re: " + currentItemName,
                    initial: GetInitial(visitorName)
                );

                if (sendButton != null)
                    sendButton.interactable = true;

                ListenForMessages();

                Debug.Log("[CHAT] Owner opened chat: " + chatId);
                Debug.Log("[CHAT] Loaded chat info room=" + currentRoomId +
                          " object=" + currentObjectId +
                          " owner=" + currentOwnerId);
            });
    }

    public void Hide()
    {
        StopListening();

        if (chatPanel != null)
            chatPanel.SetActive(false);
    }

    public void BackToInbox()
    {
        StopListening();

        if (chatPanel != null)
            chatPanel.SetActive(false);

        if (ownerInboxManager != null)
            ownerInboxManager.ShowInboxFromChat();
        else
            Debug.LogWarning("[CHAT] ownerInboxManager not assigned.");
    }

    private void ShowChatPanel()
    {
        if (chatPanel != null)
        {
            chatPanel.SetActive(true);
            chatPanel.transform.SetAsLastSibling();
        }

        if (noteText != null)
            noteText.gameObject.SetActive(false);

        if (messageInputField != null)
        {
            messageInputField.text = "";
            messageInputField.ActivateInputField();
        }
    }

    private void SetHeader(string mainName, string objectSubtitle, string initial)
    {
        if (chatTitleText != null)
            chatTitleText.text = mainName;

        if (itemInfoText != null)
            itemInfoText.text = objectSubtitle;

        if (senderNameText != null)
            senderNameText.text = mainName;

        if (objectNameText != null)
            objectNameText.text = objectSubtitle;

        if (senderInitialText != null)
            senderInitialText.text = initial;
    }

    private void OnSendPressed()
    {
        if (messageInputField == null)
            return;

        string text = messageInputField.text.Trim();

        if (string.IsNullOrEmpty(text))
            return;

        if (string.IsNullOrEmpty(currentChatId))
        {
            Debug.LogWarning("[CHAT] Chat not ready yet. currentChatId is empty.");
            return;
        }

        if (string.IsNullOrEmpty(currentRoomId) ||
            string.IsNullOrEmpty(currentObjectId) ||
            string.IsNullOrEmpty(currentOwnerId))
        {
            Debug.LogWarning("[CHAT] Chat metadata not ready. " +
                             "room=" + currentRoomId +
                             " object=" + currentObjectId +
                             " owner=" + currentOwnerId);
            return;
        }

        Debug.Log("[CHAT INSTANCE] OnSendPressed instance=" + GetInstanceID() + " obj=" + gameObject.name);
        Debug.Log("[CHAT DEBUG] currentRoomId=" + currentRoomId +
                  " currentObjectId=" + currentObjectId +
                  " currentOwnerId=" + currentOwnerId);

        FirebaseChatService.SendMessage(
            currentChatId,
            currentRoomId,
            currentObjectId,
            currentItemName,
            currentOwnerId,
            text
        );

        messageInputField.text = "";
        messageInputField.ActivateInputField();
    }

    private void ListenForMessages()
    {
        if (string.IsNullOrEmpty(currentChatId))
        {
            Debug.LogWarning("[CHAT] Cannot listen: currentChatId is empty.");
            return;
        }

        StopListening();

        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        dbRef.Child("messages")
            .Child(currentChatId)
            .ValueChanged += OnMessagesChanged;

        isListening = true;

        Debug.Log("[CHAT] Listening to messages: " + currentChatId);
    }

    private void StopListening()
    {
        if (!isListening)
            return;

        if (string.IsNullOrEmpty(currentChatId))
            return;

        DatabaseReference dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        dbRef.Child("messages")
            .Child(currentChatId)
            .ValueChanged -= OnMessagesChanged;

        isListening = false;

        Debug.Log("[CHAT] Stopped listening: " + currentChatId);
    }

    private void OnMessagesChanged(object sender, ValueChangedEventArgs e)
    {
        ClearMessagesUI();

        if (e.DatabaseError != null)
        {
            Debug.LogError("[CHAT] Listen error: " + e.DatabaseError.Message);
            return;
        }

        if (!e.Snapshot.Exists)
        {
            ScrollToBottom();
            return;
        }

        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;

        if (user == null)
        {
            Debug.LogError("[CHAT] User not logged in while receiving messages.");
            return;
        }

        string myUid = user.UserId;

        foreach (DataSnapshot msgSnap in e.Snapshot.Children)
        {
            string senderId = GetString(msgSnap, "senderId", "");
            string senderName = GetString(msgSnap, "senderName", "User");
            string text = GetString(msgSnap, "text", "");
            long timestamp = GetLong(msgSnap, "timestamp", 0);

            ChatMessage msg = new ChatMessage
            {
                text = text,
                senderName = senderName,
                senderInitial = GetInitial(senderName),
                isOwn = senderId == myUid,
                timestamp = FormatTime(timestamp)
            };

            messages.Add(msg);
            SpawnBubble(msg);
        }

        ScrollToBottom();
    }

    private void SpawnBubble(ChatMessage msg)
    {
        GameObject prefab = msg.isOwn ? rightBubblePrefab : leftBubblePrefab;

        if (prefab == null)
        {
            Debug.LogError("[CHAT] Bubble prefab missing. isOwn=" + msg.isOwn);
            return;
        }

        if (messageContainer == null)
        {
            Debug.LogError("[CHAT] messageContainer is not assigned.");
            return;
        }

        GameObject bubble = Instantiate(prefab, messageContainer);

        SetBubbleText(bubble, "MessageText", msg.text);
        SetBubbleText(bubble, "TimeText", msg.timestamp);
        SetBubbleText(bubble, "SenderText", msg.isOwn ? "you" : msg.senderName);
        SetBubbleText(bubble, "AvatarText", msg.isOwn ? GetMyInitial() : msg.senderInitial);
    }

    private void ClearMessagesUI()
    {
        if (messageContainer == null)
            return;

        foreach (Transform child in messageContainer)
            Destroy(child.gameObject);

        messages.Clear();
    }

    private void ScrollToBottom()
    {
        if (!gameObject.activeInHierarchy)
            return;

        StartCoroutine(ScrollCoroutine());
    }

    private IEnumerator ScrollCoroutine()
    {
        yield return null;
        yield return new WaitForEndOfFrame();

        if (scrollRect == null)
        {
            Debug.LogWarning("[CHAT] ScrollRect is NULL");
            yield break;
        }

        if (scrollRect.content == null)
        {
            Debug.LogWarning("[CHAT] ScrollRect content is NULL");
            yield break;
        }

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(scrollRect.content);

        scrollRect.verticalNormalizedPosition = 0f;
    }

    private void SetBubbleText(GameObject root, string childName, string value)
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

        // Not an error because some bubble prefabs may not use all optional fields.
        Debug.Log("[CHAT] Optional text not found in bubble: " + childName);
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

    private string GetMyInitial()
    {
        string username = UserSession.Username;

        if (string.IsNullOrEmpty(username))
        {
            var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
            username = user != null ? user.Email : "Me";
        }

        return GetInitial(username);
    }

    private string FormatTime(long firebaseTimestamp)
    {
        if (firebaseTimestamp <= 0)
            return "";

        System.DateTimeOffset dateTime =
            System.DateTimeOffset.FromUnixTimeMilliseconds(firebaseTimestamp).ToLocalTime();

        return dateTime.ToString("HH:mm");
    }

    [System.Serializable]
    private class ChatMessage
    {
        public string text;
        public string senderName;
        public string senderInitial;
        public bool isOwn;
        public string timestamp;
    }
}