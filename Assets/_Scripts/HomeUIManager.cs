//using System.Collections;
//using TMPro;
//using UnityEngine;

//public class HomeUIManager : MonoBehaviour
//{
//    public TMP_Text welcomeText;

//    private IEnumerator Start()
//    {
//        Debug.Log("[HOME DEBUG] HomeUIManager Start");

//        if (welcomeText != null)
//            welcomeText.text = "Welcome, ...";

//        while (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsReady)
//        {
//            yield return null;
//        }

//        Debug.Log("[HOME DEBUG] Firebase is ready in HomeUIManager");

//        if (FirebaseManager.Instance.Auth.CurrentUser == null)
//        {
//            Debug.LogError("[HOME DEBUG] Firebase CurrentUser is NULL");
//            if (welcomeText != null)
//                welcomeText.text = "Welcome, Guest";
//            yield break;
//        }

//        UserProfileManager profile = UserProfileManager.Instance;

//        if (profile == null)
//        {
//            Debug.LogError("[HOME DEBUG] UserProfileManager.Instance is NULL");
//            yield break;
//        }

//        profile.LoadCurrentUserProfile(() =>
//        {
//            Debug.Log("[HOME DEBUG] Loaded username: " + UserSession.Username);

//            if (welcomeText != null)
//                welcomeText.text = "Welcome, " + UserSession.Username;
//        });
//    }
//}

using System.Collections;
using TMPro;
using UnityEngine;

public class HomeUIManager : MonoBehaviour
{
    public TMP_Text welcomeText;

    private IEnumerator Start()
    {
        Debug.Log("[HOME DEBUG] HomeUIManager Start");

        if (welcomeText != null)
            welcomeText.text = "Welcome, ...";

        while (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsReady)
            yield return null;

        Debug.Log("[HOME DEBUG] Firebase ready");

        while (UserProfileManager.Instance == null)
            yield return null;

        Debug.Log("[HOME DEBUG] UserProfileManager ready");

        UserProfileManager.Instance.LoadCurrentUserProfile(() =>
        {
            Debug.Log("[HOME DEBUG] Username loaded = " + UserSession.Username);

            if (welcomeText != null)
                welcomeText.text = "Welcome, " + UserSession.Username;
        });
    }
}