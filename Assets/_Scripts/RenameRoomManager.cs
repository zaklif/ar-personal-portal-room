using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;

public class RenameRoomManager : MonoBehaviour
{
    public static RenameRoomManager Instance;

    public GameObject panel;
    public TMP_InputField roomNameInput;

    private string currentRoomId;
    private RoomCardUI currentCard;

    void Awake()
    {
        Instance = this;
        panel.SetActive(false);
    }

    public void Open(string roomId, string currentName, RoomCardUI card)
    {
        currentRoomId = roomId;
        currentCard = card;

        roomNameInput.text = currentName;
        panel.SetActive(true);
    }

    public void Close()
    {
        panel.SetActive(false);
    }

    public void Save()
    {
        string newName = roomNameInput.text.Trim();

        if (string.IsNullOrEmpty(newName))
            return;

        FirebaseDatabase.DefaultInstance.RootReference
            .Child("rooms")
            .Child(currentRoomId)
            .Child("roomName")
            .SetValueAsync(newName)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[ROOM] Rename failed: " + task.Exception);
                    return;
                }

                Debug.Log("[ROOM] Renamed to " + newName);

                if (currentCard != null)
                    currentCard.SetRoomName(newName);

                Close();
            });
    }
}