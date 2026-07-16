using UnityEngine;

public class TouchTester : MonoBehaviour
{
    void Update()
    {
        if (Input.touchCount > 0)
            Debug.Log("TOUCH WORKS: " + Input.touches[0].phase);
    }
}