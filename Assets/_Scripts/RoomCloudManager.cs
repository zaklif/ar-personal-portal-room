//using Firebase.Database;
//using Firebase.Extensions;
//using System.Collections;
//using System.Collections.Generic;
//using TMPro;
//using UnityEngine;
//using UnityEngine.SceneManagement;


//public class RoomCloudManager : MonoBehaviour
//{
//    public TMP_InputField roomIdInputField;
//    public TMP_Text errorText;

//    [SerializeField] private string arSceneName = "SampleScene";

//    private DatabaseReference dbRef;

//    //private void Start()
//    //{
//    //    if (FirebaseManager.Instance == null)
//    //    {
//    //        Debug.LogError("FirebaseManager.Instance is NULL in RoomCloudManager.");
//    //        return;
//    //    }

//    //    dbRef = FirebaseManager.Instance.DatabaseRoot;

//    //    if (dbRef == null)
//    //    {
//    //        Debug.LogError("DatabaseRoot is NULL in RoomCloudManager.");
//    //    }
//    //}

//    private IEnumerator Start()
//    {
//        while (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsReady)
//        {
//            yield return null;
//        }

//        dbRef = FirebaseManager.Instance.DatabaseRoot;
//        Debug.Log("[ROOM CLOUD DEBUG] DatabaseRoot ready in RoomCloudManager");
//    }

//    public void CreateRoomFromTemplate(string templateName)
//    {

//        var firebaseUser = FirebaseManager.Instance.Auth.CurrentUser;

//        if (firebaseUser == null)
//        {
//            ShowError("Please login first.");
//            Debug.LogError("[ROOM CLOUD DEBUG] Firebase CurrentUser is NULL");
//            return;
//        }

//        string uid = firebaseUser.UserId;


//        string ownerName = UserSession.Username;

//        if (string.IsNullOrEmpty(ownerName))
//        {
//            ownerName = firebaseUser.Email;
//        }

//        //string ownerName = string.IsNullOrEmpty(UserSession.Username)
//        //    ? firebaseUser.Email
//        //    : UserSession.Username;

//        Debug.Log("[ROOM CLOUD DEBUG] Creating room as uid: " + uid);
//        Debug.Log("[ROOM CLOUD DEBUG] Owner name: " + ownerName);
//        Debug.Log("[ROOM CLOUD DEBUG] UserSession.UserId: " + UserSession.UserId);

//        string roomId = GenerateRoomId();

//        Dictionary<string, object> roomData = new Dictionary<string, object>
//        {
//            { "roomId", roomId },
//            { "ownerId", uid },
//            { "ownerName", ownerName },
//            { "roomName", ownerName + "'s Room" },
//            { "roomTemplateName", templateName },
//            { "isPublic", true },
//            { "createdAt", ServerValue.Timestamp },
//            { "updatedAt", ServerValue.Timestamp }
//        };

//        Dictionary<string, object> updates = new Dictionary<string, object>
//        {
//            { "/rooms/" + roomId, roomData },
//            { "/userRooms/" + uid + "/" + roomId, true }
//        };

//        dbRef.UpdateChildrenAsync(updates).ContinueWithOnMainThread(task =>
//        {
//            if (task.IsFaulted || task.IsCanceled)
//            {
//                ShowError("Failed to create room.");
//                Debug.LogError(task.Exception);
//                return;
//            }

//            RoomManager.Instance.CreateRoomWithId(roomId, uid, templateName);

//            PlayerPrefs.SetString("RoomIntent", "create");
//            PlayerPrefs.SetString("ActiveRoomId", roomId);
//            PlayerPrefs.SetString("ChosenTemplateName", templateName);
//            PlayerPrefs.Save();

//            Debug.Log("[CLOUD ROOM] Created Firebase room: " + roomId);

//            Debug.Log("[ROOM CLOUD DEBUG] Loading SampleScene...");

//            SceneManager.LoadScene(arSceneName);
//        });
//    }

//    public void VisitRoom()
//    {
//        string roomId = roomIdInputField.text.Trim().ToUpper();

//        if (string.IsNullOrEmpty(roomId))
//        {
//            ShowError("Please enter Room ID.");
//            return;
//        }

//        if (roomId.Length != 6)
//        {
//            ShowError("Room ID must be 6 characters.");
//            return;
//        }

//        dbRef.Child("rooms").Child(roomId).GetValueAsync()
//            .ContinueWithOnMainThread(task =>
//            {
//                if (task.IsFaulted || task.IsCanceled)
//                {
//                    ShowError("Failed to check room.");
//                    Debug.LogError(task.Exception);
//                    return;
//                }

//                DataSnapshot snapshot = task.Result;

//                if (!snapshot.Exists)
//                {
//                    ShowError("Room not found.");
//                    return;
//                }

//                string ownerId = snapshot.Child("ownerId").Value.ToString();
//                string templateName = "";

//                if (snapshot.Child("roomTemplateName").Exists)
//                    templateName = snapshot.Child("roomTemplateName").Value.ToString();

//                bool isOwner = ownerId == UserSession.UserId;

//                PlayerPrefs.SetString("RoomIntent", isOwner ? "create" : "visit");
//                PlayerPrefs.SetString("ActiveRoomId", roomId);
//                PlayerPrefs.SetString("ChosenTemplateName", templateName);
//                PlayerPrefs.Save();

//                Debug.Log("[CLOUD ROOM] Visiting room: " + roomId);

//                SceneManager.LoadScene(arSceneName);
//            });
//    }

//    private string GenerateRoomId()
//    {
//        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
//        char[] result = new char[6];

//        for (int i = 0; i < result.Length; i++)
//            result[i] = chars[Random.Range(0, chars.Length)];

//        return new string(result);
//    }

//    private void ShowError(string msg)
//    {
//        if (errorText != null)
//        {
//            errorText.text = msg;
//            errorText.gameObject.SetActive(true);
//        }

//        Debug.LogWarning(msg);
//    }
//}

using Firebase.Database;
using Firebase.Extensions;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;


public class RoomCloudManager : MonoBehaviour
{
    public TMP_InputField roomIdInputField;
    public TMP_Text errorText;

    [SerializeField] private string arSceneName = "SampleScene";

    private DatabaseReference dbRef;

    //private void Start()
    //{
    //    if (FirebaseManager.Instance == null)
    //    {
    //        Debug.LogError("FirebaseManager.Instance is NULL in RoomCloudManager.");
    //        return;
    //    }

    //    dbRef = FirebaseManager.Instance.DatabaseRoot;

    //    if (dbRef == null)
    //    {
    //        Debug.LogError("DatabaseRoot is NULL in RoomCloudManager.");
    //    }
    //}

    private IEnumerator Start()
    {
        while (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsReady)
        {
            yield return null;
        }

        dbRef = FirebaseManager.Instance.DatabaseRoot;
        Debug.Log("[ROOM CLOUD DEBUG] DatabaseRoot ready in RoomCloudManager");
    }

    public void CreateRoomFromTemplate(string templateName)
    {

        var firebaseUser = FirebaseManager.Instance.Auth.CurrentUser;

        if (firebaseUser == null)
        {
            ShowError("Please login first.");
            Debug.LogError("[ROOM CLOUD DEBUG] Firebase CurrentUser is NULL");
            return;
        }

        string uid = firebaseUser.UserId;


        string ownerName = UserSession.Username;

        if (string.IsNullOrEmpty(ownerName))
        {
            ownerName = firebaseUser.Email;
        }

        //string ownerName = string.IsNullOrEmpty(UserSession.Username)
        //    ? firebaseUser.Email
        //    : UserSession.Username;

        string chosenRoomName = PlayerPrefs.GetString(
    "PendingRoomName",
    ownerName + "'s Room"
);

        Debug.Log("[ROOM CLOUD DEBUG] Creating room as uid: " + uid);
        Debug.Log("[ROOM CLOUD DEBUG] Owner name: " + ownerName);
        Debug.Log("[ROOM CLOUD DEBUG] UserSession.UserId: " + UserSession.UserId);

        string roomId = GenerateRoomId();

        Dictionary<string, object> roomData = new Dictionary<string, object>
        {
            { "roomId", roomId },
            { "ownerId", uid },
            { "ownerName", ownerName },
            { "roomName", chosenRoomName },
            { "roomTemplateName", templateName },
            { "isPublic", true },
            { "createdAt", ServerValue.Timestamp },
            { "updatedAt", ServerValue.Timestamp }
        };

        Dictionary<string, object> updates = new Dictionary<string, object>
        {
            { "/rooms/" + roomId, roomData },
            { "/userRooms/" + uid + "/" + roomId, true }
        };

        dbRef.UpdateChildrenAsync(updates).ContinueWithOnMainThread(task =>
        {
            if (task.IsFaulted || task.IsCanceled)
            {
                ShowError("Failed to create room.");
                Debug.LogError(task.Exception);
                return;
            }

            RoomManager.Instance.CreateRoomWithId(roomId, uid, templateName);

            PlayerPrefs.SetString("RoomIntent", "create");
            PlayerPrefs.SetString("ActiveRoomId", roomId);
            PlayerPrefs.SetString("ChosenTemplateName", templateName);
            PlayerPrefs.Save();

            Debug.Log("[CLOUD ROOM] Created Firebase room: " + roomId);

            Debug.Log("[ROOM CLOUD DEBUG] Loading SampleScene...");

            SceneManager.LoadScene(arSceneName);
        });
    }

    public void VisitRoom()
    {
        string roomId = roomIdInputField.text.Trim().ToUpper();

        if (string.IsNullOrEmpty(roomId))
        {
            ShowError("Please enter Room ID.");
            return;
        }

        if (roomId.Length != 6)
        {
            ShowError("Room ID must be 6 characters.");
            return;
        }

        dbRef.Child("rooms").Child(roomId).GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    ShowError("Failed to check room.");
                    Debug.LogError(task.Exception);
                    return;
                }

                DataSnapshot snapshot = task.Result;

                if (!snapshot.Exists)
                {
                    ShowError("Room not found.");
                    return;
                }

                string ownerId = snapshot.Child("ownerId").Value.ToString();
                string templateName = "";

                if (snapshot.Child("roomTemplateName").Exists)
                    templateName = snapshot.Child("roomTemplateName").Value.ToString();

                string currentUid = Firebase.Auth.FirebaseAuth.DefaultInstance.CurrentUser.UserId;

                bool isOwner = ownerId == currentUid;

                PlayerPrefs.SetString("RoomIntent", isOwner ? "create" : "visit");
                PlayerPrefs.SetString("ActiveRoomId", roomId);
                PlayerPrefs.SetString("ChosenTemplateName", templateName);
                PlayerPrefs.Save();

                Debug.Log("[CLOUD ROOM] Entering room: " + roomId +
                          " | ownerId=" + ownerId +
                          " | currentUid=" + currentUid +
                          " | isOwner=" + isOwner);

         

                Debug.Log("[CLOUD ROOM] Visiting room: " + roomId);

                Debug.Log(
    "[VISIT DEBUG] room=" + roomId +
    " ownerId=" + ownerId +
    " currentUid=" + currentUid +
    " isOwner=" + isOwner
);

                SceneManager.LoadScene(arSceneName);
            });
    }

    private string GenerateRoomId()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        char[] result = new char[6];

        for (int i = 0; i < result.Length; i++)
            result[i] = chars[Random.Range(0, chars.Length)];

        return new string(result);
    }

    private void ShowError(string msg)
    {
        if (errorText != null)
        {
            errorText.text = msg;
            errorText.gameObject.SetActive(true);
        }

        Debug.LogWarning(msg);
    }
}