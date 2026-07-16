using System.Collections;
using UnityEngine;

public class ChatTester : MonoBehaviour
{
    public InAppChat chat;

    IEnumerator Start()
    {
        Debug.Log("[TEST] ChatTester started");

        yield return null;

        if (chat == null)
        {
            Debug.LogError("[TEST] Chat is not assigned");
            yield break;
        }

        //chat.OpenChat(meta);
    }
}