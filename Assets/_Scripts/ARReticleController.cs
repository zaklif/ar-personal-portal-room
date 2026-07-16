using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARReticleController : MonoBehaviour
{
    [Header("AR")]
    [SerializeField] private ARRaycastManager arRaycastManager;

    [Header("Reticle")]
    [SerializeField] private GameObject reticleRoot;

    [Header("Settings")]
    [SerializeField] private float reticleYOffset = 0.01f;
    [SerializeField] private bool rotateWithPlane = true;

    private readonly List<ARRaycastHit> hits = new List<ARRaycastHit>();

    public bool HasValidHit { get; private set; }
    public Pose CurrentPose { get; private set; }

    private bool reticleEnabled = false;

    private void Awake()
    {
        HideReticle();
    }

    private void Update()
    {
        if (!reticleEnabled)
            return;

        UpdateReticlePose();
    }

    public void ShowReticle()
    {
        reticleEnabled = true;

        if (reticleRoot != null)
            reticleRoot.SetActive(true);
    }

    public void HideReticle()
    {
        reticleEnabled = false;
        HasValidHit = false;

        if (reticleRoot != null)
            reticleRoot.SetActive(false);
    }

    private void UpdateReticlePose()
    {
        if (arRaycastManager == null || reticleRoot == null)
            return;

        Vector2 screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);

        bool hit = arRaycastManager.Raycast(
            screenCenter,
            hits,
            TrackableType.PlaneWithinPolygon
        );

        HasValidHit = hit;

        if (!hit)
        {
            reticleRoot.SetActive(false);
            return;
        }

        Pose pose = hits[0].pose;

        Vector3 pos = pose.position;
        pos.y += reticleYOffset;

        Quaternion rot;

        if (rotateWithPlane)
            rot = pose.rotation;
        else
            rot = Quaternion.identity;

        CurrentPose = new Pose(pos, rot);

        reticleRoot.SetActive(true);
        reticleRoot.transform.SetPositionAndRotation(pos, rot);
    }

    public bool TryGetPlacementPose(out Pose pose)
    {
        pose = CurrentPose;
        return HasValidHit;
    }
}