using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ExploreRoomsManager : MonoBehaviour
{
    public GameObject panel;
    public Button openButton;
    public Button closeButton;
    public TMP_InputField searchInput;
    public Transform content;
    public GameObject roomCardPrefab;

    void Start()
    {
        panel.SetActive(false);

        openButton.onClick.RemoveAllListeners();
        openButton.onClick.AddListener(OpenPanel);

        closeButton.onClick.RemoveAllListeners();
        closeButton.onClick.AddListener(ClosePanel);

        searchInput.onValueChanged.AddListener(FilterRooms);
    }

    void OpenPanel()
    {
        panel.SetActive(true);
        LoadRooms();
    }

    void ClosePanel()
    {
        panel.SetActive(false);
    }

    void LoadRooms()
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);

        FirebaseDatabase.DefaultInstance.RootReference
            .Child("rooms")
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                Debug.Log("[EXPLORE] Total rooms = " + task.Result.ChildrenCount);

                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[EXPLORE] Load failed: " + task.Exception);
                    return;
                }

                foreach (DataSnapshot snap in task.Result.Children)
                    CreateCard(snap);
            });


    }

    void CreateCard(DataSnapshot snap)
    {
        string roomId = snap.Child("roomId").Value?.ToString() ?? snap.Key;
        string roomName = snap.Child("roomName").Value?.ToString() ?? roomId;
        string ownerName = snap.Child("ownerName").Value?.ToString() ?? "Unknown";
        string thumbnailUrl = snap.Child("thumbnailUrl").Value?.ToString() ?? "";

        int objectCount = 0;
        if (snap.Child("objectCount").Exists)
            int.TryParse(snap.Child("objectCount").Value.ToString(), out objectCount);

        GameObject card = Instantiate(roomCardPrefab, content);

        ExploreRoomCardUI cardUI = card.GetComponent<ExploreRoomCardUI>();

        if (cardUI == null)
        {
            Debug.LogError("[EXPLORE] Missing ExploreRoomCardUI on prefab.");
            return;
        }

        cardUI.Setup(roomId, roomName, ownerName, objectCount, 0, thumbnailUrl);
        LoadVisitorCount(cardUI, roomId);

        Debug.Log("[EXPLORE] Card setup: " + roomId + " | " + roomName);
    }

    void FilterRooms(string value)
    {
        value = value.ToLower().Trim();

        foreach (Transform card in content)
        {
            ExploreRoomCardUI cardUI = card.GetComponent<ExploreRoomCardUI>();

            if (cardUI == null)
                continue;

            bool visible =
                cardUI.MatchesSearch(value);

            card.gameObject.SetActive(visible);
        }
    }

    void LoadVisitorCount(ExploreRoomCardUI cardUI, string roomId)
    {
        FirebaseDatabase.DefaultInstance.RootReference
            .Child("roomPresence")
            .Child(roomId)
            .GetValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                    return;

                int visitorCount = task.Result.Exists
                    ? (int)task.Result.ChildrenCount
                    : 0;

                cardUI.SetVisitorCount(visitorCount);
            });
    }
}