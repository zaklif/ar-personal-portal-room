//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

///// <summary>
///// Floating world-space indicator above each placed object.
///// Shows $ for ForSale, 👁 for ViewOnly.
/////
///// SETUP:
///// 1. Create a prefab called "ObjectIndicatorPrefab":
/////    - Root: Canvas (World Space, scale 0.002, 0.002, 0.002)
/////    - Child: Panel (small, e.g. 80x40)
/////      - IconText (TMP_Text) — shows "$" or "👁"
/////      - BgImage (Image) — background color
///// 2. Assign this prefab to ObjectPlacementController → Indicator Prefab
/////
///// The indicator always faces the camera (billboarding).
///// </summary>
//public class ObjectIndicatorUI : MonoBehaviour
//{
//    [Header("UI References")]
//    [SerializeField] private TMP_Text iconText;
//    [SerializeField] private Image backgroundImage;

//    [Header("Colors")]
//    [SerializeField] private Color forSaleColor = new Color(0.2f, 0.8f, 0.2f, 0.9f);   // green
//    [SerializeField] private Color viewOnlyColor = new Color(0.2f, 0.5f, 1f, 0.9f);    // blue

//    [Header("Position")]
//    [SerializeField] private float heightOffset = 0.3f; // how high above object

//    private Transform targetObject; // the object this indicator belongs to
//    private Camera arCamera;

//    void Start()
//    {
//        arCamera = Camera.main;
//    }

//    void LateUpdate()
//    {
//        if (targetObject == null) return;

//        // Float above the object
//        transform.position = targetObject.position + Vector3.up * heightOffset;

//        // Always face camera (billboard effect)
//        if (arCamera != null)
//            transform.LookAt(arCamera.transform);
//    }

//    public void SetTarget(Transform obj)
//    {
//        targetObject = obj;
//    }

//    public void Refresh(ObjectMetaData meta)
//    {
//        if (meta.IsForSale)
//        {
//            iconText.text = "$";
//            backgroundImage.color = forSaleColor;
//        }
//        else
//        {
//            iconText.text = "👁";
//            backgroundImage.color = viewOnlyColor;
//        }
//    }
//}

using UnityEngine;
using TMPro;

public class ObjectIndicatorUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text statusText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private GameObject badgeRoot;

    [Header("Position")]
    //[SerializeField] private Vector3 worldOffset = new Vector3(-0.25f, 0.45f, 0f);
    [SerializeField] private float baseScale = 0.006f;
    [SerializeField] private float minScale = 0.004f;
    [SerializeField] private float maxScale = 0.012f;

    [Header("Position")]
    [SerializeField] private Vector3 worldOffset = new Vector3(-0.05f, 0.08f, 0f);

    [Header("Scale")]
    [SerializeField] private float fixedWorldScale = 0.0015f;

    private Transform target;
    private ObjectMetaData meta;
    private Camera cam;

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;

        if (target != null)
            meta = target.GetComponent<ObjectMetaData>();

        cam = Camera.main;

        RefreshDisplay();
    }

    //private void LateUpdate()
    //{
    //    if (target == null)
    //    {
    //        Destroy(gameObject);
    //        return;
    //    }

    //    if (cam == null)
    //        cam = Camera.main;

    //    if (cam == null)
    //        return;

    //    Bounds bounds = GetTargetBounds(target.gameObject);

    //    Vector3 badgePos = bounds.center;

    //    // Top-left-ish position of object
    //    badgePos.y = bounds.max.y + 0.08f;
    //    badgePos.x = bounds.min.x - 0.04f;

    //    transform.position = badgePos;

    //    // Face camera
    //    Vector3 dirToCam = transform.position - cam.transform.position;

    //    if (dirToCam.sqrMagnitude > 0.001f)
    //        transform.rotation = Quaternion.LookRotation(dirToCam.normalized, Vector3.up);

    //    // Small AR world-space UI scale
    //    float distance = Vector3.Distance(cam.transform.position, transform.position);
    //    float scale = Mathf.Clamp(distance * baseScale, minScale, maxScale);
    //    transform.localScale = Vector3.one * scale;

    //    RefreshDisplay();
    //}

    private void LateUpdate()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }

        if (cam == null)
            cam = Camera.main;

        if (cam == null)
            return;

        Bounds bounds = GetTargetBounds(target.gameObject);

        // Start from top center of the object
        Vector3 badgePos = bounds.center;
        badgePos.y = bounds.max.y + 0.08f;

        // Move badge slightly to the camera-left and upward
        Vector3 cameraLeft = -cam.transform.right;
        Vector3 cameraUp = cam.transform.up;
        Vector3 cameraForward = cam.transform.forward;

        badgePos += cameraLeft * 0.06f;
        badgePos += cameraUp * 0.03f;

        // Pull badge slightly toward the camera so it does not clip into object/wall
        badgePos -= cameraForward * 0.03f;

        transform.position = badgePos;

        // Face camera
        Vector3 dirToCam = transform.position - cam.transform.position;

        if (dirToCam.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dirToCam.normalized, Vector3.up);

        // Keep small fixed size
        transform.localScale = Vector3.one * fixedWorldScale;

        RefreshDisplay();
    }

    private void RefreshDisplay()
    {
        if (meta == null)
            return;

        if (statusText != null)
        {
            statusText.text = meta.objectType == ObjectMetaData.ObjectType.ForSale
                ? "FOR SALE"
                : "VIEW ONLY";
        }

        if (priceText != null)
        {
            if (meta.objectType == ObjectMetaData.ObjectType.ForSale)
                priceText.text = meta.currency + " " + meta.price.ToString("0.00");
            else
                priceText.text = "";
        }

        if (badgeRoot != null)
            badgeRoot.SetActive(true);
    }

    private Bounds GetTargetBounds(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return new Bounds(obj.transform.position, Vector3.one * 0.3f);

        Bounds b = renderers[0].bounds;

        foreach (Renderer r in renderers)
            b.Encapsulate(r.bounds);

        return b;
    }
}