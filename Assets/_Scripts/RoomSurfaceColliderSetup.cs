using UnityEngine;

public class RoomSurfaceColliderSetup : MonoBehaviour
{
    [Header("Surface Parent")]
    [SerializeField] private Transform insideBox;

    [Header("Settings")]
    [SerializeField] private bool refreshOnStart = true;

    private void Start()
    {
        if (refreshOnStart)
            RefreshSurfaceColliders();
    }

    public void RefreshSurfaceColliders()
    {
        if (insideBox == null)
        {
            Transform found = transform.Find("InsideBox");

            if (found != null)
                insideBox = found;
        }

        if (insideBox == null)
        {
            Debug.LogWarning("[ROOM COLLIDER] InsideBox not assigned/found.");
            return;
        }

        Renderer[] renderers = insideBox.GetComponentsInChildren<Renderer>(true);

        foreach (Renderer rend in renderers)
        {
            GameObject obj = rend.gameObject;

            // Skip tiny decorative objects if needed
            if (obj.name.ToLower().Contains("light"))
                continue;

            BoxCollider col = obj.GetComponent<BoxCollider>();

            if (col == null)
                col = obj.AddComponent<BoxCollider>();

            col.isTrigger = false;

            // Fit collider to renderer bounds
            Bounds worldBounds = rend.bounds;

            col.center = obj.transform.InverseTransformPoint(worldBounds.center);

            Vector3 localSize = obj.transform.InverseTransformVector(worldBounds.size);
            col.size = new Vector3(
                Mathf.Abs(localSize.x),
                Mathf.Abs(localSize.y),
                Mathf.Abs(localSize.z)
            );

            Debug.Log("[ROOM COLLIDER] Collider refreshed: " + obj.name);
        }
    }
}