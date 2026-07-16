
//using System.Collections.Generic;
//using Firebase.Database;
//using Firebase.Extensions;
//using UnityEngine;

//public static class FirebaseChatService
//{

//    public static string GetChatId(string roomId, string objectId, string visitorId)
//    {
//        return roomId + "_" + objectId + "_" + visitorId;
//    }

//    public static void SendMessage(

//        string chatId,
//        string roomId,
//        string objectId,
//        string objectName,
//        string ownerId,
//        string text)
//    {

//        Debug.Log(
//    "[CHAT DEBUG] roomId=" + roomId +
//    " objectId=" + objectId +
//    " ownerId=" + ownerId +
//    " text=" + text
//);
//        DatabaseReference db;

//        if (FirebaseManager.Instance != null && FirebaseManager.Instance.DatabaseRoot != null)
//        {
//            db = FirebaseManager.Instance.DatabaseRoot;
//        }
//        else
//        {
//            Debug.LogWarning("[FIREBASE CHAT] FirebaseManager not ready, using DefaultInstance fallback.");
//            db = FirebaseDatabase.DefaultInstance.RootReference;
//        }

//        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;

//        if (user == null)
//        {
//            Debug.LogError("[FIREBASE CHAT] User not logged in.");
//            return;
//        }

//        string senderName = string.IsNullOrEmpty(UserSession.Username)
//            ? user.Email
//            : UserSession.Username;

//        string visitorId = user.UserId;
//        string visitorName = senderName;

//        string messageId = db.Child("messages").Child(chatId).Push().Key;

//        Dictionary<string, object> msg = new Dictionary<string, object>
//        {
//            { "senderId", user.UserId },
//            { "senderName", senderName },
//            { "text", text },
//            { "timestamp", ServerValue.Timestamp }
//        };

//        Dictionary<string, object> updates = new Dictionary<string, object>
//        {
//            { "/messages/" + chatId + "/" + messageId, msg },

//            { "/chats/" + chatId + "/chatId", chatId },
//            { "/chats/" + chatId + "/roomId", roomId },
//            { "/chats/" + chatId + "/objectId", objectId },
//            { "/chats/" + chatId + "/objectName", objectName },
//            { "/chats/" + chatId + "/ownerId", ownerId },
//            { "/chats/" + chatId + "/visitorId", visitorId },
//            { "/chats/" + chatId + "/visitorName", visitorName },
//            { "/chats/" + chatId + "/lastMessage", text },
//            { "/chats/" + chatId + "/updatedAt", ServerValue.Timestamp }
//        };

//        db.UpdateChildrenAsync(updates).ContinueWithOnMainThread(task =>
//        {
//            if (task.IsFaulted || task.IsCanceled)
//                Debug.LogError("[FIREBASE CHAT] Send failed: " + task.Exception);
//            else
//                Debug.Log("[FIREBASE CHAT] Message sent.");
//        });
//    }
//}
using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public static class FirebaseChatService
{
    public static string GetChatId(string roomId, string objectId, string visitorId)
    {
        return roomId + "_" + objectId + "_" + visitorId;
    }

    public static void SendMessage(
        string chatId,
        string roomId,
        string objectId,
        string objectName,
        string ownerId,
        string text)
    {
        Debug.Log(
            "[CHAT DEBUG] roomId=" + roomId +
            " objectId=" + objectId +
            " ownerId=" + ownerId +
            " text=" + text
        );

        DatabaseReference db;

        if (FirebaseManager.Instance != null && FirebaseManager.Instance.DatabaseRoot != null)
        {
            db = FirebaseManager.Instance.DatabaseRoot;
        }
        else
        {
            Debug.LogWarning("[FIREBASE CHAT] FirebaseManager not ready, using DefaultInstance fallback.");
            db = FirebaseDatabase.DefaultInstance.RootReference;
        }

        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;

        if (user == null)
        {
            Debug.LogError("[FIREBASE CHAT] User not logged in.");
            return;
        }

        string senderId = user.UserId;

        string senderName = string.IsNullOrEmpty(UserSession.Username)
            ? user.Email
            : UserSession.Username;

        if (string.IsNullOrEmpty(senderName))
            senderName = "User";

        db.Child("chats").Child(chatId).GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[FIREBASE CHAT] Failed to read chat metadata: " + task.Exception);
                    return;
                }

                DataSnapshot chatSnap = task.Result;

                bool chatExists = chatSnap != null && chatSnap.Exists;

                string finalRoomId = roomId;
                string finalObjectId = objectId;
                string finalObjectName = objectName;
                string finalOwnerId = ownerId;

                string finalVisitorId;
                string finalVisitorName;

                if (chatExists)
                {
                    finalRoomId = GetString(chatSnap, "roomId", roomId);
                    finalObjectId = GetString(chatSnap, "objectId", objectId);
                    finalObjectName = GetString(chatSnap, "objectName", objectName);
                    finalOwnerId = GetString(chatSnap, "ownerId", ownerId);

                    // IMPORTANT:
                    // Preserve the original visitor.
                    // Do not replace visitorId when owner replies.
                    finalVisitorId = GetString(chatSnap, "visitorId", "");
                    finalVisitorName = GetString(chatSnap, "visitorName", "");

                    if (string.IsNullOrEmpty(finalVisitorId))
                        finalVisitorId = senderId;

                    if (string.IsNullOrEmpty(finalVisitorName))
                        finalVisitorName = senderName;
                }
                else
                {
                    // New chat is created by visitor.
                    finalVisitorId = senderId;
                    finalVisitorName = senderName;
                }

                string messageId = db.Child("messages").Child(chatId).Push().Key;

                Dictionary<string, object> msg = new Dictionary<string, object>
                {
                    { "senderId", senderId },
                    { "senderName", senderName },
                    { "text", text },
                    { "timestamp", ServerValue.Timestamp }
                };

                Dictionary<string, object> updates = new Dictionary<string, object>
                {
                    { "/messages/" + chatId + "/" + messageId, msg },

                    { "/chats/" + chatId + "/chatId", chatId },
                    { "/chats/" + chatId + "/roomId", finalRoomId },
                    { "/chats/" + chatId + "/objectId", finalObjectId },
                    { "/chats/" + chatId + "/objectName", finalObjectName },
                    { "/chats/" + chatId + "/ownerId", finalOwnerId },

                    // Preserve original visitor identity.
                    { "/chats/" + chatId + "/visitorId", finalVisitorId },
                    { "/chats/" + chatId + "/visitorName", finalVisitorName },

                    { "/chats/" + chatId + "/lastMessage", text },
                    { "/chats/" + chatId + "/lastSenderId", senderId },
                    { "/chats/" + chatId + "/lastSenderName", senderName },
                    { "/chats/" + chatId + "/updatedAt", ServerValue.Timestamp }
                };

                db.UpdateChildrenAsync(updates).ContinueWithOnMainThread(saveTask =>
                {
                    if (saveTask.IsFaulted || saveTask.IsCanceled)
                        Debug.LogError("[FIREBASE CHAT] Send failed: " + saveTask.Exception);
                    else
                        Debug.Log("[FIREBASE CHAT] Message sent. chatId=" + chatId +
                                  " visitorId=" + finalVisitorId +
                                  " ownerId=" + finalOwnerId +
                                  " lastSender=" + senderId);
                });
            });
    }

    private static string GetString(DataSnapshot snap, string key, string fallback)
    {
        if (snap == null)
            return fallback;

        object value = snap.Child(key).Value;

        if (value == null)
            return fallback;

        return value.ToString();
    }
}