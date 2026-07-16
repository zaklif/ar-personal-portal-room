using UnityEngine;

public class CustomRoomBuilder : MonoBehaviour
{
    [Header("Inside Parts")]
    public Transform insideFloor;
    public Transform insideCeiling;
    public Transform insideBackWall;
    public Transform insideLeftWall;
    public Transform insideRightWall;
    public Transform insideFrontWallLeft;
    public Transform insideFrontWallRight;
    public Transform insideFrontWallTop;

    [Header("Outside Parts")]
    public Transform outsideFloor;
    public Transform outsideCeiling;
    public Transform outsideBackWall;
    public Transform outsideLeftWall;
    public Transform outsideRightWall;
    public Transform outsideFrontWallLeft;
    public Transform outsideFrontWallRight;
    public Transform outsideFrontWallTop;

    [Header("Door Opening")]
    public float doorWidth = 2f;
    public float doorHeight = 2.6f;

    [Header("Wall Settings")]
    public float wallThickness = 0.1f;

    [Header("Inside Materials")]
    public Material insideWallMaterial;
    public Material insideFloorMaterial;
    public Material insideCeilingMaterial;

    [Header("Outside Materials")]
    public Material outsideWallMaterial;
    public Material outsideFloorMaterial;
    public Material outsideCeilingMaterial;

    [Header("Texture Presets")]
    public Texture plainTexture;
    public Texture woodTexture;
    public Texture concreteTexture;
    public Texture marbleTexture;

    public GameObject ceilingObject;

    private ModularRoomConfig currentConfig = new ModularRoomConfig();

    private void Start()
    {
        if (RoomManager.Instance != null && RoomManager.Instance.CurrentRoom != null)
        {
            if (RoomManager.Instance.CurrentRoom.customRoom == null)
                RoomManager.Instance.CurrentRoom.customRoom = new ModularRoomConfig();

            currentConfig = RoomManager.Instance.CurrentRoom.customRoom;
        }

        if (currentConfig == null)
            currentConfig = new ModularRoomConfig();

        Build(currentConfig);

        CustomRoomSetupUI ui = Object.FindFirstObjectByType<CustomRoomSetupUI>();
        if (ui != null)
        {
            ui.RegisterRoom(this);
        }
    }

    public void Build(ModularRoomConfig config)
    {
        if (config == null)
            config = new ModularRoomConfig();

        currentConfig = config;

        float width = config.width;
        float height = config.height;
        float depth = config.depth;
        float sideWidth = Mathf.Max(0.1f, (width - doorWidth) / 2f);
        float topHeight = Mathf.Max(0.1f, height - doorHeight);

        ApplyRoomParts(
            insideFloor,
            insideCeiling,
            insideBackWall,
            insideLeftWall,
            insideRightWall,
            insideFrontWallLeft,
            insideFrontWallRight,
            insideFrontWallTop,
            insideWallMaterial,
            insideFloorMaterial,
            insideCeilingMaterial,
            width,
            height,
            depth,
            sideWidth,
            topHeight
        );

        float outsideOffset = 0.25f;

        ApplyRoomParts(
            outsideFloor,
            outsideCeiling,
            outsideBackWall,
            outsideLeftWall,
            outsideRightWall,
            outsideFrontWallLeft,
            outsideFrontWallRight,
            outsideFrontWallTop,
            outsideWallMaterial,
            outsideFloorMaterial,
            outsideCeilingMaterial,
            width + outsideOffset,
            height + outsideOffset,
            depth + outsideOffset,
            sideWidth + outsideOffset * 0.5f,
            topHeight + outsideOffset * 0.5f
        );

        ApplyColorAndTexture(config);
        ResizeInsideTrigger(width, height, depth);

        ApplyCeilingVisibility(config.showCeiling);
    }

    private void ApplyRoomParts(
        Transform floor,
        Transform ceiling,
        Transform backWall,
        Transform leftWall,
        Transform rightWall,
        Transform frontWallLeft,
        Transform frontWallRight,
        Transform frontWallTop,

        Material wallMaterial,
        Material floorMaterial,
        Material ceilingMaterial,

        float width,
        float height,
        float depth,
        float sideWidth,
        float topHeight)
    {
        SetPart(floor,
            new Vector3(0, 0, depth / 2f),
            new Vector3(width, wallThickness, depth),
            floorMaterial);

        SetPart(ceiling,
            new Vector3(0, height, depth / 2f),
            new Vector3(width, wallThickness, depth),
            ceilingMaterial);

        SetPart(backWall,
            new Vector3(0, height / 2f, depth),
            new Vector3(width, height, wallThickness),
            wallMaterial);

        SetPart(leftWall,
            new Vector3(-width / 2f, height / 2f, depth / 2f),
            new Vector3(wallThickness, height, depth),
            wallMaterial);

        SetPart(rightWall,
            new Vector3(width / 2f, height / 2f, depth / 2f),
            new Vector3(wallThickness, height, depth),
            wallMaterial);

        SetPart(frontWallLeft,
            new Vector3(-doorWidth / 2f - sideWidth / 2f, height / 2f, 0),
            new Vector3(sideWidth, height, wallThickness),
            wallMaterial);

        SetPart(frontWallRight,
            new Vector3(doorWidth / 2f + sideWidth / 2f, height / 2f, 0),
            new Vector3(sideWidth, height, wallThickness),
            wallMaterial);

        SetPart(frontWallTop,
            new Vector3(0, doorHeight + topHeight / 2f, 0),
            new Vector3(doorWidth, topHeight, wallThickness),
            wallMaterial);

    }

    private void SetPart(Transform part, Vector3 localPosition, Vector3 localScale, Material material)
    {
        if (part == null) return;

        part.localPosition = localPosition;
        part.localRotation = Quaternion.identity;
        part.localScale = localScale;

        part.gameObject.layer = LayerMask.NameToLayer("CustomRoom");

        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer != null && material != null)
        {
            renderer.sharedMaterial = material;
        }

        BoxCollider box = part.GetComponent<BoxCollider>();

        if (box == null)
        {
            box = part.gameObject.AddComponent<BoxCollider>();
        }

        box.isTrigger = false;
        box.center = Vector3.zero;
        box.size = Vector3.one;
    }

    private void ApplyColorAndTexture(ModularRoomConfig config)
    {
        Color wallColor = new Color(config.wallColorR, config.wallColorG, config.wallColorB, 1f);
        Color floorColor = new Color(config.floorColorR, config.floorColorG, config.floorColorB, 1f);
        Color ceilingColor = new Color(config.ceilingColorR, config.ceilingColorG, config.ceilingColorB, 1f);

        Texture wallTexture = GetTextureByName(config.wallTextureName);
        Texture floorTexture = GetTextureByName(config.floorTextureName);
        Texture ceilingTexture = GetTextureByName(config.ceilingTextureName);

        // INSIDE
        ApplyMaterialSettings(insideWallMaterial, wallColor, wallTexture, config);
        ApplyMaterialSettings(insideFloorMaterial, floorColor, floorTexture, config);
        ApplyMaterialSettings(insideCeilingMaterial, ceilingColor, ceilingTexture, config);

        // OUTSIDE
        ApplyMaterialSettings(outsideWallMaterial, wallColor, wallTexture, config);
        ApplyMaterialSettings(outsideFloorMaterial, floorColor, floorTexture, config);
        ApplyMaterialSettings(outsideCeilingMaterial, ceilingColor, ceilingTexture, config);
    }

    private void ApplyMaterialSettings(Material material, Color color, Texture texture, ModularRoomConfig config)
    {
        if (material == null) return;

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);
        else
            material.color = color;

        if (material.HasProperty("_BaseMap"))
        {
            material.SetTexture("_BaseMap", texture);

            float tileX = Mathf.Max(1f, config.width / 2f);
            float tileY = Mathf.Max(1f, config.height / 2f);

            material.SetTextureScale("_BaseMap", new Vector2(tileX, tileY));
        }
    }

    private void ApplyTextureToPart(Transform part, Texture texture, ModularRoomConfig config)
    {
        if (part == null || texture == null) return;

        Renderer renderer = part.GetComponent<Renderer>();
        if (renderer == null) return;

        Material mat = renderer.sharedMaterial;
        if (mat == null) return;

        if (mat.HasProperty("_BaseMap"))
        {
            mat.SetTexture("_BaseMap", texture);

            float tileX = Mathf.Max(1f, config.width / 2f);
            float tileY = Mathf.Max(1f, config.depth / 2f);

            mat.SetTextureScale("_BaseMap", new Vector2(tileX, tileY));
        }
    }

    private Texture GetTextureByName(string textureName)
    {
        switch (textureName)
        {
            case "Wood":
                return woodTexture;
            case "Concrete":
                return concreteTexture;
            case "Marble":
                return marbleTexture;
            case "Plain":
                return null;
            default:
                return null;
        }
    }

    private void ResizeInsideTrigger(float width, float height, float depth)
    {
        BoxCollider trigger = GetComponent<BoxCollider>();
        if (trigger == null) return;

        trigger.isTrigger = true;
        trigger.center = new Vector3(0, height / 2f, depth / 2f);
        trigger.size = new Vector3(width, height, depth);
    }

    public void SetWidth(float value)
    {
        currentConfig.width = value;
        Build(currentConfig);
        SaveConfig();

        Debug.LogWarning("[CUSTOM ROOM] Room size changed. Shelves may need repositioning.");
    }

    public void SetHeight(float value)
    {
        currentConfig.height = value;
        Build(currentConfig);
        SaveConfig();

        Debug.LogWarning("[CUSTOM ROOM] Room size changed. Shelves may need repositioning.");
    }

    public void SetDepth(float value)
    {
        currentConfig.depth = value;
        Build(currentConfig);
        SaveConfig();

        Debug.LogWarning("[CUSTOM ROOM] Room size changed. Shelves may need repositioning.");
    }

    public void SetRed(float value)
    {
        currentConfig.colorR = value;
        Build(currentConfig);
        SaveConfig();
    }

    public void SetGreen(float value)
    {
        currentConfig.colorG = value;
        Build(currentConfig);
        SaveConfig();
    }

    public void SetBlue(float value)
    {
        currentConfig.colorB = value;
        Build(currentConfig);
        SaveConfig();
    }

    public void SetWallTexture(string textureName)
    {
        currentConfig.wallTextureName = textureName;
        Build(currentConfig);
        SaveConfig();
    }

    public void SetFloorTexture(string textureName)
    {
        currentConfig.floorTextureName = textureName;
        Build(currentConfig);
        SaveConfig();
    }

    public void SetCeilingTexture(string textureName)
    {
        currentConfig.ceilingTextureName = textureName;
        Build(currentConfig);
        SaveConfig();
    }

    public ModularRoomConfig GetCurrentConfig()
    {
        return currentConfig;
    }

    public void ToggleCeiling(bool visible)
    {
        currentConfig.showCeiling = visible;

        if (insideCeiling != null)
            insideCeiling.gameObject.SetActive(visible);

        if (outsideCeiling != null)
            outsideCeiling.gameObject.SetActive(visible);

        SaveConfig();
    }

    private void ApplyCeilingVisibility(bool visible)
    {
        if (insideCeiling != null)
            insideCeiling.gameObject.SetActive(visible);

        if (outsideCeiling != null)
            outsideCeiling.gameObject.SetActive(visible);
    }

    //private void SaveConfig()
    //{
    //    if (RoomManager.Instance == null) return;
    //    if (RoomManager.Instance.CurrentRoom == null) return;
    //    if (!RoomManager.IsOwner) return;

    //    RoomManager.Instance.CurrentRoom.customRoom = currentConfig;
    //    RoomManager.Instance.SaveCurrentRoom();
    //}

    private void SaveConfig()
    {
        if (RoomManager.Instance == null)
        {
            Debug.LogWarning("[CUSTOM ROOM] Save skipped: RoomManager null");
            return;
        }

        if (RoomManager.Instance.CurrentRoom == null)
        {
            Debug.LogWarning("[CUSTOM ROOM] Save skipped: CurrentRoom null");
            return;
        }

        if (!RoomAccessManager.CanEdit)
        {
            Debug.LogWarning("[CUSTOM ROOM] Save skipped: Not owner");
            return;
        }

        RoomManager.Instance.CurrentRoom.customRoom = currentConfig;

        Debug.Log("[CUSTOM ROOM] Saving config " +
                  "W=" + currentConfig.width +
                  " H=" + currentConfig.height +
                  " D=" + currentConfig.depth +
                  " WallRGB=" + currentConfig.wallColorR + "," +
                                currentConfig.wallColorG + "," +
                                currentConfig.wallColorB +
                  " WallTexture=" + currentConfig.wallTextureName);

        RoomManager.Instance.SaveCurrentRoom();
    }

    public void SetWallColor(float r, float g, float b)
    {
        currentConfig.wallColorR = r;
        currentConfig.wallColorG = g;
        currentConfig.wallColorB = b;
        Build(currentConfig);
        SaveConfig();
    }

    public void SetFloorColor(float r, float g, float b)
    {
        currentConfig.floorColorR = r;
        currentConfig.floorColorG = g;
        currentConfig.floorColorB = b;
        Build(currentConfig);
        SaveConfig();
    }

    public void SetCeilingColor(float r, float g, float b)
    {
        currentConfig.ceilingColorR = r;
        currentConfig.ceilingColorG = g;
        currentConfig.ceilingColorB = b;
        Build(currentConfig);
        SaveConfig();
    }

    public void ResetSize()
    {
        currentConfig.width = 4f;
        currentConfig.height = 3f;
        currentConfig.depth = 5f;

        Build(currentConfig);
        SaveConfig();
    }

    public void ResetCurrentTargetColor(string target)
    {
        switch (target)
        {
            case "Wall":
                currentConfig.wallColorR = 1f;
                currentConfig.wallColorG = 1f;
                currentConfig.wallColorB = 1f;
                break;

            case "Floor":
                currentConfig.floorColorR = 1f;
                currentConfig.floorColorG = 1f;
                currentConfig.floorColorB = 1f;
                break;

            case "Ceiling":
                currentConfig.ceilingColorR = 1f;
                currentConfig.ceilingColorG = 1f;
                currentConfig.ceilingColorB = 1f;
                break;
        }

        Build(currentConfig);
        SaveConfig();
    }

    public void ResetCurrentTargetTexture(string target)
    {
        switch (target)
        {
            case "Wall":
                currentConfig.wallTextureName = "Plain";
                break;

            case "Floor":
                currentConfig.floorTextureName = "Plain";
                break;

            case "Ceiling":
                currentConfig.ceilingTextureName = "Plain";
                break;
        }

        Build(currentConfig);
        SaveConfig();
    }

    public void ResetAllCustomRoom()
    {
        currentConfig.width = 4f;
        currentConfig.height = 3f;
        currentConfig.depth = 5f;

        currentConfig.wallColorR = 1f;
        currentConfig.wallColorG = 1f;
        currentConfig.wallColorB = 1f;

        currentConfig.floorColorR = 1f;
        currentConfig.floorColorG = 1f;
        currentConfig.floorColorB = 1f;

        currentConfig.ceilingColorR = 1f;
        currentConfig.ceilingColorG = 1f;
        currentConfig.ceilingColorB = 1f;

        currentConfig.wallTextureName = "Plain";
        currentConfig.floorTextureName = "Plain";
        currentConfig.ceilingTextureName = "Plain";

        currentConfig.showCeiling = true;

        Build(currentConfig);
        SaveConfig();
    }

    //PRESETTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT
    public void ApplyDefaultPreset()
    {
        currentConfig.wallTextureName = "Plain";
        currentConfig.floorTextureName = "Plain";
        currentConfig.ceilingTextureName = "Plain";

        SetAllColors(1f, 1f, 1f);
    }

    public void ApplyWoodPreset()
    {
        currentConfig.wallTextureName = "Wood";
        currentConfig.floorTextureName = "Wood";
        currentConfig.ceilingTextureName = "Plain";

        SetAllColors(1f, 0.92f, 0.82f);
    }

    public void ApplyConcretePreset()
    {
        currentConfig.wallTextureName = "Concrete";
        currentConfig.floorTextureName = "Concrete";
        currentConfig.ceilingTextureName = "Plain";

        SetAllColors(0.75f, 0.75f, 0.75f);
    }

    public void ApplyMarblePreset()
    {
        currentConfig.wallTextureName = "Marble";
        currentConfig.floorTextureName = "Marble";
        currentConfig.ceilingTextureName = "Plain";

        SetAllColors(1f, 1f, 1f);
    }

    public void ApplyModernPreset()
    {
        currentConfig.wallTextureName = "Plain";
        currentConfig.floorTextureName = "Marble";
        currentConfig.ceilingTextureName = "Plain";

        currentConfig.wallColorR = 0.92f;
        currentConfig.wallColorG = 0.92f;
        currentConfig.wallColorB = 0.95f;

        currentConfig.floorColorR = 1f;
        currentConfig.floorColorG = 1f;
        currentConfig.floorColorB = 1f;

        currentConfig.ceilingColorR = 1f;
        currentConfig.ceilingColorG = 1f;
        currentConfig.ceilingColorB = 1f;

        Build(currentConfig);
        SaveConfig();
    }

    private void SetAllColors(float r, float g, float b)
    {
        currentConfig.wallColorR = r;
        currentConfig.wallColorG = g;
        currentConfig.wallColorB = b;

        currentConfig.floorColorR = r;
        currentConfig.floorColorG = g;
        currentConfig.floorColorB = b;

        currentConfig.ceilingColorR = r;
        currentConfig.ceilingColorG = g;
        currentConfig.ceilingColorB = b;

        Build(currentConfig);
        SaveConfig();
    }

    public void ApplyRemoteConfig(ModularRoomConfig config)
    {
        if (config == null) return;

        currentConfig = config;
        Build(currentConfig);

        if (RoomManager.Instance != null && RoomManager.Instance.CurrentRoom != null)
            RoomManager.Instance.CurrentRoom.customRoom = currentConfig;

        Debug.Log("[CUSTOM ROOM] Remote config applied");
    }

    //END PRESETTTTTTTTTTTTTTTT
    //////////////////////////////////////////////////////////////////
}