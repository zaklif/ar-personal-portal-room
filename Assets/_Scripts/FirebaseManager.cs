//using Firebase;
//using Firebase.Auth;
//using Firebase.Database;
//using Firebase.Extensions;
//using UnityEngine;

//public class FirebaseManager : MonoBehaviour
//{
//    public static FirebaseManager Instance;

//    public bool IsReady { get; private set; }

//    public FirebaseAuth Auth { get; private set; }
//    public DatabaseReference DatabaseRoot { get; private set; }

//    private void Awake()
//    {
//        if (Instance != null)
//        {
//            Destroy(gameObject);
//            return;
//        }

//        Instance = this;
//        DontDestroyOnLoad(gameObject);

//        InitializeFirebase();
//    }

//    private void InitializeFirebase()
//    {
//        Debug.Log("Checking Firebase dependencies...");

//        FirebaseApp.CheckAndFixDependenciesAsync()
//            .ContinueWithOnMainThread(task =>
//            {
//                Debug.Log("Firebase dependency check finished.");

//                if (task.Result == DependencyStatus.Available)
//                {
//                    Auth = FirebaseAuth.DefaultInstance;
//                    DatabaseRoot = FirebaseDatabase.DefaultInstance.RootReference;

//                    IsReady = true;
//                    Debug.Log("Firebase is ready.");
//                }
//                else
//                {
//                    Debug.LogError("Firebase dependency error: " + task.Result);
//                }
//            });
//    }
//}
using Firebase;
using Firebase.Auth;
using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public class FirebaseManager : MonoBehaviour
{
    public static FirebaseManager Instance;

    public bool IsReady { get; private set; }

    public FirebaseAuth Auth { get; private set; }
    public DatabaseReference DatabaseRoot { get; private set; }

    private void Awake()
    {
        Debug.Log("[FIREBASE DEBUG] FirebaseManager Awake");

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeFirebase();
    }

    private void InitializeFirebase()
    {
        Debug.Log("[FIREBASE DEBUG] Checking Firebase dependencies...");

        FirebaseApp.CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(task =>
            {
                Debug.Log("[FIREBASE DEBUG] Dependency check finished.");

                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[FIREBASE DEBUG] Dependency task failed: " + task.Exception);
                    return;
                }

                if (task.Result != DependencyStatus.Available)
                {
                    Debug.LogError("[FIREBASE DEBUG] Dependency error: " + task.Result);
                    return;
                }

                try
                {
                    Auth = FirebaseAuth.DefaultInstance;
                    Debug.Log("[FIREBASE DEBUG] Auth ready.");

                    DatabaseRoot = FirebaseDatabase.DefaultInstance.RootReference;
                    Debug.Log("[FIREBASE DEBUG] Database ready.");

                    IsReady = true;
                    Debug.Log("[FIREBASE DEBUG] FirebaseManager is ready.");
                }
                catch (System.Exception e)
                {
                    IsReady = false;
                    Debug.LogError("[FIREBASE DEBUG] Firebase init failed: " + e.Message);
                }
            });
    }
}