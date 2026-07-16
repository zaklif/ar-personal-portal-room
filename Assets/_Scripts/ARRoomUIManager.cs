using TMPro;
using UnityEngine;

public class ARRoomUIManager : MonoBehaviour
{
    public TMP_Text roomIdText;
    public TMP_Text roomModeText;

    private void Start()
    {
        string roomId = PlayerPrefs.GetString("ActiveRoomId", "NO ROOM");
        string intent = PlayerPrefs.GetString("RoomIntent", "visit");

        bool isOwner = RoomManager.IsOwner || intent == "create";

        roomIdText.text = "Room ID: " + roomId;
        roomModeText.text = isOwner ? "OWNER" : "VISITOR";

        Debug.Log("[AR UI] Room ID: " + roomId);
        Debug.Log("[AR UI] Mode: " + (isOwner ? "OWNER" : "VISITOR"));
    }
}