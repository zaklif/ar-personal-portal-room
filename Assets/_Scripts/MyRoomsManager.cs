using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MyRoomsManager : MonoBehaviour
{
    public Transform content;
    public GameObject roomCardPrefab;

    void Start()
    {
        LoadMyRooms();
    }

    void LoadMyRooms()
    {
        string uid =
            Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;

        FirebaseDatabase.DefaultInstance.RootReference
            .Child("userRooms")
            .Child(uid)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                    return;

                DataSnapshot snap = task.Result;

                foreach (DataSnapshot roomSnap in snap.Children)
                {
                    string roomId = roomSnap.Key;

                    CreateRoomCard(roomId);
                }
            });
    }

    void CreateRoomCard(string roomId)
    {
        GameObject card = Instantiate(roomCardPrefab, content);

        RoomCardUI cardUI = card.GetComponent<RoomCardUI>();

        if (cardUI == null)
        {
            Debug.LogError("[MY ROOMS] RoomCardUI missing on prefab.");
            return;
        }

        LoadRoomInfo(cardUI, roomId);
    }

    void LoadRoomInfo(RoomCardUI cardUI, string roomId)
    {
        FirebaseDatabase.DefaultInstance.RootReference
            .Child("rooms")
            .Child(roomId)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[MY ROOMS] Failed to load room: " + task.Exception);
                    return;
                }

                if (!task.Result.Exists)
                {
                    Debug.LogWarning("[MY ROOMS] Room not found: " + roomId);
                    return;
                }

                string roomName =
                    task.Result.Child("roomName").Value?.ToString()
                    ?? roomId;

                cardUI.Setup(roomId, roomName);

                Debug.Log("[MY ROOMS] Setup card: " + roomId + " | " + roomName);
            });
    }
}