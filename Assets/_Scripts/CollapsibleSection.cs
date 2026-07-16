using UnityEngine;

public class CollapsibleSection : MonoBehaviour
{
    [SerializeField] private GameObject content;

    private bool expanded = true;

    public void Toggle()
    {
        expanded = !expanded;

        if (content != null)
            content.SetActive(expanded);
    }
}