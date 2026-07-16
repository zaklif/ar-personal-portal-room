using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.Networking;

public class ExploreRoomCardUI : MonoBehaviour
{
    [Header("Texts")]
    public TMP_Text roomNameText;
    public TMP_Text roomIdText;
    public TMP_Text ownerNameText;
    public TMP_Text objectCountText;
    public TMP_Text visitorCountText;

    public Sprite defaultThumbnail;

    [Header("Image")]
    public Image thumbnailImage;

    [Header("Button")]
    public Button visitButton;

    private string roomId;

    public void Setup(
        string id,
        string roomName,
        string ownerName,
        int objectCount,
        int visitorCount,
        string thumbnailUrl)
    {
        roomId = id;

        roomNameText.text = roomName;
        roomIdText.text = "ID: " + roomId;
        ownerNameText.text = "by " + ownerName;
        objectCountText.text = "Objects: " + objectCount;
        visitorCountText.text = "Online: " + visitorCount;

        if (!string.IsNullOrEmpty(thumbnailUrl))
        {
            StartCoroutine(LoadThumbnail(thumbnailUrl));
        }
        else if (defaultThumbnail != null){
            thumbnailImage.sprite = defaultThumbnail;
        }

        //if (thumbnailImage != null)
        //{
        //    thumbnailImage.sprite = defaultThumbnail;
        //}

        visitButton.onClick.RemoveAllListeners();
        visitButton.onClick.AddListener(VisitRoom);
    }

    void VisitRoom()
    {
        Debug.Log("[EXPLORE] Visit room: " + roomId);

        PlayerPrefs.SetString("RoomIntent", "visit");
        PlayerPrefs.SetString("ActiveRoomId", roomId);
        PlayerPrefs.Save();

        SceneManager.LoadScene("SampleScene");
    }

    public bool MatchesSearch(string search)
    {
        if (string.IsNullOrEmpty(search))
            return true;

        string roomName = roomNameText != null ? roomNameText.text.ToLower() : "";
        string ownerName = ownerNameText != null ? ownerNameText.text.ToLower() : "";
        string id = string.IsNullOrEmpty(roomId) ? "" : roomId.ToLower();

        return roomName.Contains(search) ||
               ownerName.Contains(search) ||
               id.Contains(search);
    }

    public void SetVisitorCount(int count)
    {
        if (visitorCountText != null)
            visitorCountText.text = "Online: " + count;
    }

    IEnumerator LoadThumbnail(string url)
    {
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning("[EXPLORE] Thumbnail failed: " + request.error);

            if (defaultThumbnail != null)
                thumbnailImage.sprite = defaultThumbnail;

            yield break;
        }

        Texture2D texture = DownloadHandlerTexture.GetContent(request);

        Sprite sprite = Sprite.Create(
            texture,
            new Rect(0, 0, texture.width, texture.height),
            new Vector2(0.5f, 0.5f)
        );

        thumbnailImage.sprite = sprite;
    }
}