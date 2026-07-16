//using TMPro;
//using UnityEngine;
//using UnityEngine.UI;
//using UnityEngine.SceneManagement;

//public class RoomCardUI : MonoBehaviour
//{
//    public TMP_Text roomNameText;
//    public TMP_Text roomIdText;
//    public Button openButton;
//    public Button deleteButton;

//    private string roomId;
//    private string roomName;

//    public void Setup(string id, string name)
//    {
//        roomId = id;
//        roomName = name;

//        roomNameText.text = roomName;
//        roomIdText.text = roomId;

//        openButton.onClick.RemoveAllListeners();
//        openButton.onClick.AddListener(OpenRoom);

//        if (deleteButton != null)
//        {
//            deleteButton.onClick.RemoveAllListeners();
//            deleteButton.onClick.AddListener(DeleteRoom);
//        }
//    }

//    public void OpenRoom()
//    {
//        Debug.Log("[ROOM CARD] Open clicked: " + roomId);

//        PlayerPrefs.SetString("RoomIntent", "create");
//        PlayerPrefs.SetString("ActiveRoomId", roomId);
//        PlayerPrefs.Save();

//        SceneManager.LoadScene("SampleScene");
//    }

//    public void DeleteRoom()
//    {
//        Debug.Log("[ROOM CARD] Delete clicked: " + roomId);
//    }
//}

using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections.Generic;

public class RoomCardUI : MonoBehaviour
{
    public TMP_Text roomNameText;
    public TMP_Text roomIdText;
    public Button openButton;
    public Button deleteButton;
    public Button renameButton;

    private string roomId;
    private string roomName;

    public void Setup(string id, string name)
    {
        roomId = id;

        roomNameText.text = name;
        roomIdText.text = id;

        openButton.onClick.RemoveAllListeners();
        openButton.onClick.AddListener(OpenRoom);

        if (deleteButton != null)
        {
            deleteButton.onClick.RemoveAllListeners();
            deleteButton.onClick.AddListener(DeleteRoom);
        }

        if (renameButton != null)
        {
            renameButton.onClick.RemoveAllListeners();
            renameButton.onClick.AddListener(RenameRoom);
        }
    }

    public void OpenRoom()
    {
        Debug.Log("[ROOM CARD] Open clicked: " + roomId);

        PlayerPrefs.SetString("RoomIntent", "create");
        PlayerPrefs.SetString("ActiveRoomId", roomId);
        PlayerPrefs.Save();

        SceneManager.LoadScene("SampleScene");
    }

    //public void DeleteRoom()
    //{
    //    Debug.Log("[ROOM CARD] Delete clicked: " + roomId);

    //    var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
    //    if (user == null) return;

    //    DatabaseReference db = FirebaseDatabase.DefaultInstance.RootReference;

    //    Dictionary<string, object> updates = new Dictionary<string, object>
    //    {
    //        { "/userRooms/" + user.UserId + "/" + roomId, null },
    //        { "/rooms/" + roomId, null },
    //        { "/roomData/" + roomId, null },
    //        { "/roomObjects/" + roomId, null },
    //        { "/roomPresence/" + roomId, null },
    //        { "/editRequests/" + roomId, null },
    //        { "/roomPermissions/" + roomId, null }
    //    };

    //    db.UpdateChildrenAsync(updates).ContinueWithOnMainThread(task =>
    //    {
    //        if (task.IsFaulted || task.IsCanceled)
    //        {
    //            Debug.LogError("[ROOM CARD] Delete failed: " + task.Exception);
    //            return;
    //        }

    //        Debug.Log("[ROOM CARD] Deleted room: " + roomId);
    //        Destroy(gameObject);
    //    });
    //}

    public void DeleteRoom()
    {
        Debug.Log("[ROOM CARD] Delete clicked: " + roomId);

        if (string.IsNullOrEmpty(roomId) || roomId.Length != 6)
        {
            Debug.LogError("[ROOM CARD] BLOCKED DELETE. Invalid roomId: " + roomId);
            return;
        }

        var user = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser;
        if (user == null)
        {
            Debug.LogError("[ROOM CARD] BLOCKED DELETE. User null.");
            return;
        }

        FirebaseDatabase.DefaultInstance.RootReference
            .Child("rooms")
            .Child(roomId)
            .Child("ownerId")
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled || !task.Result.Exists)
                {
                    Debug.LogError("[ROOM CARD] BLOCKED DELETE. Cannot verify owner.");
                    return;
                }

                string ownerId = task.Result.Value.ToString();

                if (ownerId != user.UserId)
                {
                    Debug.LogError("[ROOM CARD] BLOCKED DELETE. Not owner.");
                    return;
                }

                DeleteVerifiedRoom(user.UserId);
            });
    }

    void DeleteVerifiedRoom(string uid)
    {
        Dictionary<string, object> updates = new Dictionary<string, object>
    {
        { "/userRooms/" + uid + "/" + roomId, null },

        // safer: only remove from room list first
        // do NOT delete room data yet
        { "/rooms/" + roomId + "/isDeleted", true }
    };

        FirebaseDatabase.DefaultInstance.RootReference
            .UpdateChildrenAsync(updates)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[ROOM CARD] Delete failed: " + task.Exception);
                    return;
                }

                Debug.Log("[ROOM CARD] Room removed from My Rooms: " + roomId);
                Destroy(gameObject);
            });
    }

    public void SetRoomName(string newName)
    {
        roomNameText.text = newName;
    }

    public void RenameRoom()
    {
        RenameRoomManager.Instance.Open(
            roomId,
            roomNameText.text,
            this
            );
    }


}