//using UnityEngine;
//using UnityEngine.Rendering;

///// <summary>
///// Attach this to your PortalMask Quad GameObject.
///// It will print detailed info to the Console every frame (while enabled).
///// Disable the component when you no longer need it.
///// </summary>
//public class PortalDebugger : MonoBehaviour
//{
//    [Header("Drag your PortalMask Quad here")]
//    public MeshRenderer maskQuadRenderer;

//    [Header("Drag your Room wall renderers here (any one of them)")]
//    public MeshRenderer anyRoomWallRenderer;

//    [Header("Check every N seconds (0 = every frame)")]
//    public float checkInterval = 2f;

//    private float _timer = 0f;

//    private void Start()
//    {
//        // Auto-find on same GameObject if not assigned
//        if (maskQuadRenderer == null)
//            maskQuadRenderer = GetComponent<MeshRenderer>();

//        RunDiagnostic();
//    }

//    private void Update()
//    {
//        _timer += Time.deltaTime;
//        if (checkInterval <= 0f || _timer >= checkInterval)
//        {
//            _timer = 0f;
//            RunDiagnostic();
//        }
//    }

//    private void RunDiagnostic()
//    {
//        Debug.Log("===== PORTAL DEBUGGER =====");

//        // ── 1. Is the mask quad active? ──────────────────────
//        Debug.Log($"[1] MaskQuad GameObject active: {maskQuadRenderer?.gameObject.activeInHierarchy}");
//        Debug.Log($"[1] MaskQuad Renderer enabled:  {maskQuadRenderer?.enabled}");

//        // ── 2. Material on mask quad ─────────────────────────
//        if (maskQuadRenderer != null)
//        {
//            Material m = maskQuadRenderer.material;
//            Debug.Log($"[2] MaskQuad material name:   {m?.name}");
//            Debug.Log($"[2] MaskQuad shader name:     {m?.shader?.name}");

//            // Check it's the right shader
//            if (m != null && m.shader.name != "Custom/PortalMask")
//                Debug.LogError("[2] !! WRONG SHADER on mask quad — expected 'Custom/PortalMask'");
//            else
//                Debug.Log("[2] Shader name is correct.");

//            // Check shader compiled OK
//            if (m != null && !m.shader.isSupported)
//                Debug.LogError("[2] !! Shader is NOT supported on this device/platform!");
//            else
//                Debug.Log("[2] Shader is supported.");
//        }
//        else
//        {
//            Debug.LogError("[2] !! maskQuadRenderer is NULL — assign it in Inspector!");
//        }

//        // ── 3. Render queue ──────────────────────────────────
//        if (maskQuadRenderer != null && maskQuadRenderer.material != null)
//        {
//            int q = maskQuadRenderer.material.renderQueue;
//            Debug.Log($"[3] MaskQuad renderQueue: {q}  (should be < 2000, e.g. 1900)");
//            if (q >= 2000)
//                Debug.LogWarning("[3] !! renderQueue is too HIGH — mask may render AFTER room walls. Set to 1900.");
//        }

//        // ── 4. Room wall material ────────────────────────────
//        if (anyRoomWallRenderer != null)
//        {
//            Material wm = anyRoomWallRenderer.material;
//            Debug.Log($"[4] Wall material name:   {wm?.name}");
//            Debug.Log($"[4] Wall shader name:     {wm?.shader?.name}");

//            if (wm != null && wm.shader.name != "Custom/PortalRoom")
//                Debug.LogError("[4] !! WRONG SHADER on room wall — expected 'Custom/PortalRoom'");
//            else
//                Debug.Log("[4] Wall shader name is correct.");

//            if (wm != null)
//            {
//                int wq = wm.renderQueue;
//                Debug.Log($"[4] Wall renderQueue: {wq}  (should be > mask queue, e.g. 2010)");
//            }
//        }
//        else
//        {
//            Debug.LogWarning("[4] anyRoomWallRenderer not assigned — skipping wall check.");
//        }

//        // ── 5. MaskQuad scale / size ─────────────────────────
//        if (maskQuadRenderer != null)
//        {
//            Vector3 s = maskQuadRenderer.transform.lossyScale;
//            Debug.Log($"[5] MaskQuad world scale: {s}  (should match door opening size)");
//            if (s.x < 0.1f || s.y < 0.1f)
//                Debug.LogWarning("[5] !! MaskQuad scale is very small — it may not cover the door opening.");
//        }

//        // ── 6. MaskQuad facing direction ─────────────────────
//        if (maskQuadRenderer != null)
//        {
//            Camera cam = Camera.main;
//            if (cam != null)
//            {
//                Vector3 toCam = cam.transform.position - maskQuadRenderer.transform.position;
//                float dot = Vector3.Dot(maskQuadRenderer.transform.forward, toCam.normalized);
//                Debug.Log($"[6] Dot(maskForward, toCam) = {dot:F2}");

//                if (dot < 0f)
//                    Debug.LogWarning("[6] !! Mask quad is facing AWAY from camera. " +
//                                     "Rotate it 180° on Y so its front face points toward the camera (outside).");
//                else
//                    Debug.Log("[6] Mask quad facing direction looks correct.");
//            }
//        }

//        // ── 7. URP Stencil override check ────────────────────
//        // If a URP Renderer Feature is overriding stencil, it can break this
//        var urpAsset = GraphicsSettings.currentRenderPipeline;
//        Debug.Log($"[7] Current render pipeline: {urpAsset?.GetType().Name ?? "NULL"}");
//        Debug.Log("[7] NOTE: Check URP Renderer (Forward Renderer asset) — " +
//                  "if 'Stencil' override is enabled under Rendering > Stencil, it may override our stencil values.");

//        // ── 8. Camera position relative to room ─────────────
//        if (maskQuadRenderer != null)
//        {
//            Camera cam = Camera.main;
//            if (cam != null)
//            {
//                float dist = Vector3.Distance(cam.transform.position, maskQuadRenderer.transform.position);
//                Debug.Log($"[8] Camera distance from mask quad: {dist:F2}m");
//            }
//        }

//        Debug.Log("===========================");
//    }

//    // ── Visual gizmo in Scene view ───────────────────────────
//    private void OnDrawGizmos()
//    {
//        if (maskQuadRenderer == null) return;

//        // Draw quad outline in cyan
//        Gizmos.color = Color.cyan;
//        Gizmos.matrix = maskQuadRenderer.transform.localToWorldMatrix;
//        Gizmos.DrawWireCube(Vector3.zero, new Vector3(1, 1, 0.01f));

//        // Draw forward direction arrow
//        Gizmos.color = Color.yellow;
//        Gizmos.matrix = Matrix4x4.identity;
//        Gizmos.DrawRay(maskQuadRenderer.transform.position,
//                       maskQuadRenderer.transform.forward * 0.5f);
//    }
//}

using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PortalDebugger : MonoBehaviour
{
    [Header("Drag your PortalMask Quad here")]
    public MeshRenderer maskQuadRenderer;

    [Header("Drag any room wall renderer here")]
    public MeshRenderer anyRoomWallRenderer;

    [Header("Check interval (seconds)")]
    public float checkInterval = 3f;

    private float _timer = 0f;

    private void Start() => RunDiagnostic();

    private void Update()
    {
        _timer += Time.deltaTime;
        if (_timer >= checkInterval) { _timer = 0f; RunDiagnostic(); }
    }

    private void RunDiagnostic()
    {
        Debug.Log("===== PORTAL DEBUGGER v3 =====");

        // ── Mask quad ────────────────────────────────────────
        if (maskQuadRenderer != null)
        {
            Material m = maskQuadRenderer.material;
            int q = m.renderQueue;
            Debug.Log($"[MASK] shader={m.shader.name} | queue={q} | enabled={maskQuadRenderer.enabled}");

            // Stencil ref via reflection (Unity doesn't expose it directly)
            // So we read it from shader keywords/properties if possible
            Debug.Log($"[MASK] scale={maskQuadRenderer.transform.lossyScale}");

            Vector3 toCam = Camera.main.transform.position - maskQuadRenderer.transform.position;
            float dot = Vector3.Dot(maskQuadRenderer.transform.forward, toCam.normalized);
            Debug.Log($"[MASK] dot(forward,toCam)={dot:F2}  (positive = facing camera GOOD)");
        }

        // ── Wall ─────────────────────────────────────────────
        if (anyRoomWallRenderer != null)
        {
            Material m = anyRoomWallRenderer.material;
            Debug.Log($"[WALL] shader={m.shader.name} | queue={m.renderQueue}");
        }

        // ── URP Renderer ─────────────────────────────────────
        var urpAsset = GraphicsSettings.currentRenderPipeline as UniversalRenderPipelineAsset;
        if (urpAsset != null)
        {
            // Get the renderer data via reflection
            var rendererDataList = typeof(UniversalRenderPipelineAsset)
                .GetField("m_RendererDataList",
                    System.Reflection.BindingFlags.NonPublic |
                    System.Reflection.BindingFlags.Instance)
                ?.GetValue(urpAsset) as ScriptableRendererData[];

            if (rendererDataList != null)
            {
                for (int i = 0; i < rendererDataList.Length; i++)
                {
                    var rd = rendererDataList[i];
                    if (rd == null) continue;
                    Debug.Log($"[URP] Renderer[{i}] = {rd.name} ({rd.GetType().Name})");

                    // Check for stencil override on UniversalRendererData
                    var stencilField = rd.GetType()
                        .GetField("m_DefaultStencilState",
                            System.Reflection.BindingFlags.NonPublic |
                            System.Reflection.BindingFlags.Instance);

                    if (stencilField != null)
                    {
                        var stencilState = stencilField.GetValue(rd);
                        Debug.Log($"[URP] DefaultStencilState = {stencilState}");

                        // Check overrideStencilState bool
                        var overrideProp = stencilState?.GetType()
                            .GetField("overrideStencilState");
                        if (overrideProp != null)
                        {
                            bool isOverriding = (bool)overrideProp.GetValue(stencilState);
                            if (isOverriding)
                                Debug.LogError("[URP] !! overrideStencilState = TRUE — THIS IS YOUR BUG. " +
                                               "Open the URP Renderer asset and DISABLE stencil override!");
                            else
                                Debug.Log("[URP] overrideStencilState = false (good)");
                        }
                    }

                    // List all renderer features
                    var features = rd.rendererFeatures;
                    if (features != null && features.Count > 0)
                    {
                        foreach (var f in features)
                            Debug.Log($"[URP] RendererFeature: {f?.name} ({f?.GetType().Name}) active={f?.isActive}");
                    }
                    else
                    {
                        Debug.Log("[URP] No renderer features found.");
                    }
                }
            }
            else
            {
                Debug.LogWarning("[URP] Could not read renderer data list via reflection.");
            }
        }

        // ── SystemInfo stencil support ───────────────────────
        Debug.Log($"[DEVICE] GPU: {SystemInfo.graphicsDeviceName}");
        Debug.Log($"[DEVICE] GraphicsDeviceType: {SystemInfo.graphicsDeviceType}");
        Debug.Log("[DEVICE] SupportsStencil: always true in Unity 6");
        Debug.Log($"[DEVICE] GraphicsShaderLevel: {SystemInfo.graphicsShaderLevel}");

        Debug.Log("==============================");
    }
}