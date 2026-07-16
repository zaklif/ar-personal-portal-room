using Firebase.Database;
using Firebase.Extensions;
using UnityEngine;

public static class CloudShelfSaver
{
    public static void SaveShelf(string roomId, PlacedShelfData shelf)
    {
        if (string.IsNullOrEmpty(roomId) || shelf == null)
        {
            Debug.LogWarning("[CLOUD SHELF] Save skipped: roomId or shelf null");
            return;
        }

        if (string.IsNullOrEmpty(shelf.shelfId))
            shelf.shelfId = System.Guid.NewGuid().ToString();

        DatabaseReference dbRef = FirebaseManager.Instance.DatabaseRoot;

        string json = JsonUtility.ToJson(shelf);

        dbRef.Child("roomShelves")
            .Child(roomId)
            .Child(shelf.shelfId)
            .SetRawJsonValueAsync(json)
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[CLOUD SHELF] Save failed: " + task.Exception);
                    return;
                }

                Debug.Log("[CLOUD SHELF] Saved shelf: " + shelf.shelfId);
            });
    }

    public static void DeleteShelf(string roomId, string shelfId)
    {
        if (string.IsNullOrEmpty(roomId) || string.IsNullOrEmpty(shelfId))
        {
            Debug.LogWarning("[CLOUD SHELF] Delete skipped: roomId or shelfId empty");
            return;
        }

        DatabaseReference dbRef = FirebaseManager.Instance.DatabaseRoot;

        dbRef.Child("roomShelves")
            .Child(roomId)
            .Child(shelfId)
            .RemoveValueAsync()
            .ContinueWithOnMainThread(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    Debug.LogError("[CLOUD SHELF] Delete failed: " + task.Exception);
                    return;
                }

                Debug.Log("[CLOUD SHELF] Deleted shelf: " + shelfId);
            });
    }
}