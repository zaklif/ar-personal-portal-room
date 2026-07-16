using Firebase.Database;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomPresenceViewer : MonoBehaviour
{
    [Header("UI")]
    public GameObject visitorsButton;
    public GameObject presencePanel;
    public TMP_Text visitorCountText;
    public Transform content;
    public GameObject visitorItemPrefab;
    public Button closeButton;

    private DatabaseReference dbRef;
    private string roomId;

    void Start()
    {

        Debug.Log("[PRESENCE VIEWER] IsOwner = " + RoomManager.IsOwner);
        

        dbRef = FirebaseDatabase.DefaultInstance.RootReference;

        presencePanel.SetActive(false);

        bool isOwner = RoomManager.IsOwner;

        visitorsButton.SetActive(isOwner);
        //visitorsButton.SetActive(true);
        
        Debug.Log("[PRESENCE VIEWER] Button = " + visitorsButton);

        if (closeButton != null)
            closeButton.onClick.AddListener(() => presencePanel.SetActive(false));

        if (isOwner)
        {
            roomId = RoomManager.Instance.CurrentRoom.roomId;
            visitorsButton.GetComponent<Button>().onClick.AddListener(OpenPanel);
        }
    }

    void OpenPanel()
    {
        presencePanel.SetActive(true);
        ListenPresence();
    }

    void ListenPresence()
    {
        dbRef.Child("roomPresence")
            .Child(roomId)
            .ValueChanged += OnPresenceChanged;
    }

    void OnPresenceChanged(object sender, ValueChangedEventArgs e)
    {
        foreach (Transform child in content)
            Destroy(child.gameObject);

        int count = 0;

        if (e.Snapshot.Exists)
        {
            foreach (DataSnapshot userSnap in e.Snapshot.Children)
            {
                string username = userSnap.Child("username").Value?.ToString() ?? "Unknown";
                string email = userSnap.Child("email").Value?.ToString() ?? "";

                GameObject item = Instantiate(visitorItemPrefab, content);

                Debug.Log("[PRESENCE VIEWER] Created item: " + item.name);

                TMP_Text text = item.GetComponentInChildren<TMP_Text>();
                if (text != null)
                    text.text = username + "\n" + email;

                count++;
            }
        }
        Debug.Log("[PRESENCE VIEWER] Snapshot exists = " + e.Snapshot.Exists);
        Debug.Log("[PRESENCE VIEWER] Count = " + e.Snapshot.ChildrenCount);
        Debug.Log("[PRESENCE VIEWER] Content = " + content);
        Debug.Log("[PRESENCE VIEWER] Prefab = " + visitorItemPrefab);

        if (visitorCountText != null)
            visitorCountText.text = count + " visitor(s) inside";
    }

    void OnDestroy()
    {
        if (!string.IsNullOrEmpty(roomId) && dbRef != null)
        {
            dbRef.Child("roomPresence")
                .Child(roomId)
                .ValueChanged -= OnPresenceChanged;
        }
    }
}