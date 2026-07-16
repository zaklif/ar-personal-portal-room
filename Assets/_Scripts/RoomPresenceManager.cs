using System.Collections;
using System.Collections.Generic;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class RoomPresenceManager : MonoBehaviour
{
    private DatabaseReference dbRef;
    private string roomId;
    private string userId;
    private string username;
    private bool presenceActive = false;

    private Coroutine heartbeatRoutine;

    IEnumerator Start()
    {
        yield return new WaitUntil(() =>
            RoomManager.Instance != null &&
            RoomManager.Instance.CurrentRoom != null
        );

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        roomId = RoomManager.Instance.CurrentRoom.roomId;

        if (RoomManager.IsOwner)
        {
            Debug.Log("[PRESENCE] Owner detected. Not adding to visitor presence.");
            yield break;
        }

        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;

        if (user == null)
        {
            Debug.LogWarning("[PRESENCE] No logged in user.");
            yield break;
        }

        userId = user.UserId;
        username = string.IsNullOrEmpty(UserSession.Username)
            ? user.Email
            : UserSession.Username;

        EnterRoom();
    }

    void EnterRoom()
    {
        if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(userId))
            return;

        Dictionary<string, object> data = new Dictionary<string, object>
        {
            { "userId", userId },
            { "username", username },
            { "email", Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.Email },
            { "enteredAt", ServerValue.Timestamp },
            { "lastSeen", ServerValue.Timestamp }
        };

        DatabaseReference presenceRef = dbRef
    .Child("roomPresence")
    .Child(roomId)
    .Child(userId);

        presenceRef.OnDisconnect().RemoveValue();

        presenceRef.SetValueAsync(data)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[PRESENCE] Enter failed: " + task.Exception);
                    return;
                }

                presenceActive = true;

                if (heartbeatRoutine != null)
                    StopCoroutine(heartbeatRoutine);

                heartbeatRoutine = StartCoroutine(Heartbeat()); ;
                Debug.Log("[PRESENCE] Entered room: " + roomId);
            });
    }

    IEnumerator Heartbeat()
    {
        while (presenceActive)
        {
            Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "userId", userId },
            { "username", username },
            { "email", Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.Email },
            { "lastSeen", ServerValue.Timestamp }
        };

            dbRef.Child("roomPresence")
                .Child(roomId)
                .Child(userId)
                .UpdateChildrenAsync(updates);

            yield return new WaitForSeconds(10f);
        }
    }

    public void LeaveRoom()
    {
        if (!presenceActive) return;

        presenceActive = false;

        if (heartbeatRoutine != null)
            StopCoroutine(heartbeatRoutine);

        dbRef.Child("roomPresence")
            .Child(roomId)
            .Child(userId)
            .RemoveValueAsync();

        Debug.Log("[PRESENCE] Left room: " + roomId);
    }

    void OnApplicationFocus(bool hasFocus)
    {
        if (hasFocus && !RoomManager.IsOwner && presenceActive)
        {
            EnterRoom();
            Debug.Log("[PRESENCE] App focused. Re-entered presence.");
        }
    }

    void OnApplicationQuit()
    {
        LeaveRoom();
    }

    void OnDestroy()
    {
        LeaveRoom();
    }
}