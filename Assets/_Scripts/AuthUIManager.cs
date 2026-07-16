using System;
using System.Collections.Generic;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AuthUIManager : MonoBehaviour
{
    [Header("Panels")]
    public GameObject loginPanel;
    public GameObject signupPanel;

    [Header("Login UI")]
    public TMP_InputField loginEmailInput;
    public TMP_InputField loginPasswordInput;
    public TMP_Text loginErrorText;

    [Header("Signup UI")]
    public TMP_InputField signupUsernameInput;
    public TMP_InputField signupEmailInput;
    public TMP_InputField signupPasswordInput;
    public TMP_InputField signupConfirmPasswordInput;
    public TMP_Text signupErrorText;

    private FirebaseAuth auth;
    private DatabaseReference dbRef;

    private void Start()
    {
        auth = FirebaseManager.Instance.Auth;
        dbRef = FirebaseManager.Instance.DatabaseRoot;

        ShowLogin();
        ClearErrors();
    }

    public void ShowLogin()
    {
        loginPanel.SetActive(true);
        signupPanel.SetActive(false);
        ClearErrors();
    }

    public void ShowSignup()
    {
        loginPanel.SetActive(false);
        signupPanel.SetActive(true);
        ClearErrors();
    }

    public void LoginClicked()
    {
        ClearErrors();

        string email = loginEmailInput.text.Trim();
        string password = loginPasswordInput.text;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            loginErrorText.text = "Please enter email and password.";
            return;
        }

        auth.SignInWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    loginErrorText.text = "Login failed. Check your email or password.";
                    Debug.LogError(task.Exception);
                    return;
                }

                Debug.Log("Login success: " + auth.CurrentUser.UserId);
                SceneManager.LoadScene("HomeScreen");
            });
    }

    public void SignupClicked()
    {
        ClearErrors();

        string username = signupUsernameInput.text.Trim();
        string email = signupEmailInput.text.Trim();
        string password = signupPasswordInput.text;
        string confirmPassword = signupConfirmPasswordInput.text;

        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            signupErrorText.text = "Please fill in all fields.";
            return;
        }

        if (password != confirmPassword)
        {
            signupErrorText.text = "Passwords do not match.";
            return;
        }

        if (password.Length < 6)
        {
            signupErrorText.text = "Password must be at least 6 characters.";
            return;
        }

        auth.CreateUserWithEmailAndPasswordAsync(email, password)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    signupErrorText.text = "Signup failed. Email may already be used.";
                    Debug.LogError(task.Exception);
                    return;
                }

                FirebaseUser user = auth.CurrentUser;
                SaveUserProfile(user.UserId, username, email);
            });
    }

    private void SaveUserProfile(string uid, string username, string email)
    {
        Dictionary<string, object> userData = new Dictionary<string, object>
    {
        { "username", username },
        { "email", email },
        { "createdAt", ServerValue.Timestamp },
        { "lastOnline", ServerValue.Timestamp },
        { "userType", "normal" }
    };

        dbRef.Child("users").Child(uid).SetValueAsync(userData)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    signupErrorText.text = "Account created, but profile save failed.";
                    Debug.LogError(task.Exception);
                    return;
                }

                Debug.Log("User profile saved.");
                SceneManager.LoadScene("HomeScreen");
            });
    }

    private void ClearErrors()
    {
        if (loginErrorText != null) loginErrorText.text = "";
        if (signupErrorText != null) signupErrorText.text = "";
    }
}