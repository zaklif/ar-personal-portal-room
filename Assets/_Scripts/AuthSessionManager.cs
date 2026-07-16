using Firebase.Auth;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthSessionManager : MonoBehaviour
{
    public void Logout()
    {
        if (FirebaseManager.Instance != null && FirebaseManager.Instance.Auth != null)
        {
            FirebaseManager.Instance.Auth.SignOut();
            Debug.Log("User logged out.");
        }
        else
        {
            FirebaseAuth.DefaultInstance.SignOut();
            Debug.Log("User logged out using default FirebaseAuth.");
        }

        UserSession.Clear();

        UnityEngine.SceneManagement.SceneManager.LoadScene("LoginScene");
    }


}