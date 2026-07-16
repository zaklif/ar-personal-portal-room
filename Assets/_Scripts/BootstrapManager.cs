using System.Collections;
using Firebase.Auth;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BootstrapManager : MonoBehaviour
{
    public TMP_Text loadingText;

    private IEnumerator Start()
    {
        loadingText.text = "Starting Firebase...";

        float timer = 0f;

        while (FirebaseManager.Instance == null || !FirebaseManager.Instance.IsReady)
        {
            timer += Time.deltaTime;

            if (timer > 10f)
            {
                loadingText.text = "Firebase failed to start. Check Console.";
                Debug.LogError("FirebaseManager did not become ready after 10 seconds.");
                yield break;
            }

            yield return null;
        }

        loadingText.text = "Checking login...";

        FirebaseUser user = FirebaseManager.Instance.Auth.CurrentUser;

        yield return new WaitForSeconds(0.5f);

        if (user != null)
        {
            SceneManager.LoadScene("HomeScreen");
        }
        else
        {
            SceneManager.LoadScene("LoginScene");
        }
    }
}