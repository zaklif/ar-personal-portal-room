
//using UnityEngine;
//using UnityEngine.UI;
//using TMPro;

//public class CustomRoomSetupUI : MonoBehaviour
//{
//    [Header("Panel")]
//    public GameObject customRoomPanel;

//    [Header("Toggle Button")]
//    public GameObject customRoomToggleButton;

//    [Header("Sliders")]
//    public Slider widthSlider;
//    public Slider heightSlider;
//    public Slider depthSlider;
//    public Slider redSlider;
//    public Slider greenSlider;
//    public Slider blueSlider;

//    [Header("Extra Controls")]
//    public Toggle ceilingToggle;

//    [Header("Target Label")]
//    public TMP_Text selectedTargetText;

//    private CustomRoomBuilder currentBuilder;
//    private bool isOwnerInside = false;

//    private enum EditTarget
//    {
//        Wall,
//        Floor,
//        Ceiling
//    }

//    private EditTarget currentTarget = EditTarget.Wall;

//    private void Start()
//    {
//        if (customRoomPanel != null)
//            customRoomPanel.SetActive(false);

//        if (customRoomToggleButton != null)
//            customRoomToggleButton.SetActive(false);
//    }

//    public void RegisterRoom(CustomRoomBuilder builder)
//    {
//        currentBuilder = builder;

//        if (currentBuilder == null)
//            return;

//        ModularRoomConfig config = currentBuilder.GetCurrentConfig();

//        widthSlider.SetValueWithoutNotify(config.width);
//        heightSlider.SetValueWithoutNotify(config.height);
//        depthSlider.SetValueWithoutNotify(config.depth);

//        //redSlider.SetValueWithoutNotify(config.colorR);
//        //greenSlider.SetValueWithoutNotify(config.colorG);
//        //blueSlider.SetValueWithoutNotify(config.colorB);

//        if (ceilingToggle != null)
//            ceilingToggle.SetIsOnWithoutNotify(config.showCeiling);

//        ConnectSliders();
//        RefreshColorSliders();
//        RefreshTargetLabel();
//    }

//    private void ConnectSliders()
//    {
//        widthSlider.onValueChanged.RemoveAllListeners();
//        heightSlider.onValueChanged.RemoveAllListeners();
//        depthSlider.onValueChanged.RemoveAllListeners();
//        redSlider.onValueChanged.RemoveAllListeners();
//        greenSlider.onValueChanged.RemoveAllListeners();
//        blueSlider.onValueChanged.RemoveAllListeners();

//        widthSlider.onValueChanged.AddListener(OnWidthChanged);
//        heightSlider.onValueChanged.AddListener(OnHeightChanged);
//        depthSlider.onValueChanged.AddListener(OnDepthChanged);
//        redSlider.onValueChanged.AddListener(OnRedChanged);
//        greenSlider.onValueChanged.AddListener(OnGreenChanged);
//        blueSlider.onValueChanged.AddListener(OnBlueChanged);



//        if (ceilingToggle != null)
//        {
//            ceilingToggle.onValueChanged.RemoveAllListeners();
//            ceilingToggle.onValueChanged.AddListener(OnCeilingToggleChanged);
//        }
//    }

//    public void SetOwnerInside(bool inside)
//    {
//        isOwnerInside = inside && RoomManager.IsOwner;

//        if (customRoomToggleButton != null)
//            customRoomToggleButton.SetActive(isOwnerInside);

//        if (!isOwnerInside)
//            HidePanel();
//    }

//    public void TogglePanel()
//    {
//        if (!isOwnerInside)
//            return;

//        if (customRoomPanel != null)
//            customRoomPanel.SetActive(!customRoomPanel.activeSelf);
//    }

//    public void ShowPanel()
//    {
//        if (!isOwnerInside)
//            return;

//        if (customRoomPanel != null)
//            customRoomPanel.SetActive(true);
//    }

//    public void HidePanel()
//    {
//        if (customRoomPanel != null)
//            customRoomPanel.SetActive(false);
//    }

//    private void OnWidthChanged(float value)
//    {
//        if (currentBuilder != null)
//            currentBuilder.SetWidth(value);
//    }

//    private void OnHeightChanged(float value)
//    {
//        if (currentBuilder != null)
//            currentBuilder.SetHeight(value);
//    }

//    private void OnDepthChanged(float value)
//    {
//        if (currentBuilder != null)
//            currentBuilder.SetDepth(value);
//    }

//    private void OnRedChanged(float value)
//    {
//        if (currentBuilder == null) return;

//        ModularRoomConfig cfg = currentBuilder.GetCurrentConfig();

//        switch (currentTarget)
//        {
//            case EditTarget.Wall:
//                currentBuilder.SetWallColor(
//                    value,
//                    cfg.wallColorG,
//                    cfg.wallColorB);
//                break;

//            case EditTarget.Floor:
//                currentBuilder.SetFloorColor(
//                    value,
//                    cfg.floorColorG,
//                    cfg.floorColorB);
//                break;

//            case EditTarget.Ceiling:
//                currentBuilder.SetCeilingColor(
//                    value,
//                    cfg.ceilingColorG,
//                    cfg.ceilingColorB);
//                break;
//        }
//    }

//    private void OnGreenChanged(float value)
//    {
//        if (currentBuilder == null) return;

//        ModularRoomConfig cfg = currentBuilder.GetCurrentConfig();

//        switch (currentTarget)
//        {
//            case EditTarget.Wall:
//                currentBuilder.SetWallColor(
//                    cfg.wallColorR,
//                    value,
//                    cfg.wallColorB);
//                break;

//            case EditTarget.Floor:
//                currentBuilder.SetFloorColor(
//                    cfg.floorColorR,
//                    value,
//                    cfg.floorColorB);
//                break;

//            case EditTarget.Ceiling:
//                currentBuilder.SetCeilingColor(
//                    cfg.ceilingColorR,
//                    value,
//                    cfg.ceilingColorB);
//                break;
//        }
//    }

//    private void OnBlueChanged(float value)
//    {
//        if (currentBuilder == null) return;

//        ModularRoomConfig cfg = currentBuilder.GetCurrentConfig();

//        switch (currentTarget)
//        {
//            case EditTarget.Wall:
//                currentBuilder.SetWallColor(
//                    cfg.wallColorR,
//                    cfg.wallColorG,
//                    value);
//                break;

//            case EditTarget.Floor:
//                currentBuilder.SetFloorColor(
//                    cfg.floorColorR,
//                    cfg.floorColorG,
//                    value);
//                break;

//            case EditTarget.Ceiling:
//                currentBuilder.SetCeilingColor(
//                    cfg.ceilingColorR,
//                    cfg.ceilingColorG,
//                    value);
//                break;
//        }
//    }

//    public void ApplyTexture(string textureName)
//    {
//        if (currentBuilder == null)
//            return;

//        switch (currentTarget)
//        {
//            case EditTarget.Wall:
//                currentBuilder.SetWallTexture(textureName);
//                break;

//            case EditTarget.Floor:
//                currentBuilder.SetFloorTexture(textureName);
//                break;

//            case EditTarget.Ceiling:
//                currentBuilder.SetCeilingTexture(textureName);
//                break;
//        }
//    }

//    public void ApplyPlainTexture() => ApplyTexture("Plain");
//    public void ApplyWoodTexture() => ApplyTexture("Wood");
//    public void ApplyConcreteTexture() => ApplyTexture("Concrete");
//    public void ApplyMarbleTexture() => ApplyTexture("Marble");

//    private void OnCeilingToggleChanged(bool value)
//    {
//        if (currentBuilder != null)
//        {
//            currentBuilder.ToggleCeiling(value);
//        }
//    }

//    public void SelectWallMode()
//    {
//        currentTarget = EditTarget.Wall;
//        RefreshColorSliders();
//        RefreshTargetLabel();
//    }

//    public void SelectFloorMode()
//    {
//        currentTarget = EditTarget.Floor;
//        RefreshColorSliders();
//        RefreshTargetLabel();
//    }

//    public void SelectCeilingMode()
//    {
//        currentTarget = EditTarget.Ceiling;
//        RefreshColorSliders();
//        RefreshTargetLabel();
//    }

//    private void RefreshColorSliders()
//    {
//        if (currentBuilder == null) return;

//        ModularRoomConfig cfg = currentBuilder.GetCurrentConfig();

//        switch (currentTarget)
//        {
//            case EditTarget.Wall:
//                redSlider.SetValueWithoutNotify(cfg.wallColorR);
//                greenSlider.SetValueWithoutNotify(cfg.wallColorG);
//                blueSlider.SetValueWithoutNotify(cfg.wallColorB);
//                break;

//            case EditTarget.Floor:
//                redSlider.SetValueWithoutNotify(cfg.floorColorR);
//                greenSlider.SetValueWithoutNotify(cfg.floorColorG);
//                blueSlider.SetValueWithoutNotify(cfg.floorColorB);
//                break;

//            case EditTarget.Ceiling:
//                redSlider.SetValueWithoutNotify(cfg.ceilingColorR);
//                greenSlider.SetValueWithoutNotify(cfg.ceilingColorG);
//                blueSlider.SetValueWithoutNotify(cfg.ceilingColorB);
//                break;
//        }
//    }

//    private void RefreshTargetLabel()
//    {
//        if (selectedTargetText == null) return;

//        selectedTargetText.text = "Editing: " + currentTarget.ToString();
//    }

//    public void ResetSize()
//    {
//        if (currentBuilder == null) return;

//        currentBuilder.ResetSize();

//        ModularRoomConfig cfg = currentBuilder.GetCurrentConfig();

//        widthSlider.SetValueWithoutNotify(cfg.width);
//        heightSlider.SetValueWithoutNotify(cfg.height);
//        depthSlider.SetValueWithoutNotify(cfg.depth);
//    }

//    public void ResetCurrentColor()
//    {
//        if (currentBuilder == null) return;

//        currentBuilder.ResetCurrentTargetColor(currentTarget.ToString());
//        RefreshColorSliders();
//    }

//    public void ResetCurrentTexture()
//    {
//        if (currentBuilder == null) return;

//        currentBuilder.ResetCurrentTargetTexture(currentTarget.ToString());
//    }

//    public void ResetAll()
//    {
//        if (currentBuilder == null) return;

//        currentBuilder.ResetAllCustomRoom();

//        ModularRoomConfig cfg = currentBuilder.GetCurrentConfig();

//        widthSlider.SetValueWithoutNotify(cfg.width);
//        heightSlider.SetValueWithoutNotify(cfg.height);
//        depthSlider.SetValueWithoutNotify(cfg.depth);

//        if (ceilingToggle != null)
//            ceilingToggle.SetIsOnWithoutNotify(cfg.showCeiling);

//        RefreshColorSliders();
//        RefreshTargetLabel();
//    }

//    ////PRESETTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT
//    public void ApplyDefaultPreset()
//    {
//        if (currentBuilder == null) return;
//        currentBuilder.ApplyDefaultPreset();
//        RefreshColorSliders();
//    }

//    public void ApplyModernPreset()
//    {
//        if (currentBuilder == null) return;
//        currentBuilder.ApplyModernPreset();
//        RefreshColorSliders();
//    }

//    public void ApplyWoodPreset()
//    {
//        if (currentBuilder == null) return;
//        currentBuilder.ApplyWoodPreset();
//        RefreshColorSliders();
//    }

//    public void ApplyConcretePreset()
//    {
//        if (currentBuilder == null) return;
//        currentBuilder.ApplyConcretePreset();
//        RefreshColorSliders();
//    }

//    public void ApplyMarblePreset()
//    {
//        if (currentBuilder == null) return;
//        currentBuilder.ApplyMarblePreset();
//        RefreshColorSliders();
//    }

//    //END PRESETTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTTT

//}

using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CustomRoomSetupUI : MonoBehaviour
{
    [Header("Panel")]
    public GameObject customRoomPanel;

    [Header("Toggle Button")]
    public GameObject customRoomToggleButton;

    [Header("Close Button")]
    public Button closeButton;

    [Header("Sliders")]
    public Slider widthSlider;
    public Slider heightSlider;
    public Slider depthSlider;
    public Slider redSlider;
    public Slider greenSlider;
    public Slider blueSlider;

    [Header("Live Value Labels")]
    public TMP_Text widthValueText;
    public TMP_Text heightValueText;
    public TMP_Text depthValueText;
    public TMP_Text redValueText;
    public TMP_Text greenValueText;
    public TMP_Text blueValueText;

    [Header("Extra Controls")]
    public Toggle ceilingToggle;

    [Header("Target Label")]
    public TMP_Text selectedTargetText;

    [Header("Colour Preview")]
    public Image colourPreviewImage;

    [Header("Surface Tab Buttons")]
    public Button wallTabButton;
    public Button floorTabButton;
    public Button ceilingTabButton;

    [Header("Surface Tab Images")]
    public Image wallTabImage;
    public Image floorTabImage;
    public Image ceilingTabImage;

    [Header("Surface Tab Text")]
    public TMP_Text wallTabText;
    public TMP_Text floorTabText;
    public TMP_Text ceilingTabText;

    [Header("Texture Buttons")]
    public Button plainTextureButton;
    public Button woodTextureButton;
    public Button concreteTextureButton;
    public Button marbleTextureButton;

    [Header("Texture Button Images")]
    public Image plainTextureImage;
    public Image woodTextureImage;
    public Image concreteTextureImage;
    public Image marbleTextureImage;

    [Header("Texture Button Text")]
    public TMP_Text plainTextureText;
    public TMP_Text woodTextureText;
    public TMP_Text concreteTextureText;
    public TMP_Text marbleTextureText;

    [Header("Reset All Button")]
    public Button resetAllButton;

    [Header("Selected State Colours")]
    public Color selectedBgColor = new Color32(10, 34, 24, 255);       // #0A2218
    public Color unselectedBgColor = new Color32(12, 16, 32, 255);    // #0C1020
    public Color selectedTextColor = new Color32(29, 158, 117, 255);  // #1D9E75
    public Color unselectedTextColor = new Color32(128, 144, 187, 255); // #8090BB

    private CustomRoomBuilder currentBuilder;
    private bool isOwnerInside = false;

    private enum EditTarget
    {
        Wall,
        Floor,
        Ceiling
    }

    private EditTarget currentTarget = EditTarget.Wall;
    private string currentTexture = "Plain";

    private void Start()
    {
        if (customRoomPanel != null)
            customRoomPanel.SetActive(false);

        if (customRoomToggleButton != null)
            customRoomToggleButton.SetActive(false);

        ConnectStaticButtons();
    }

    private void ConnectStaticButtons()
    {
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(HidePanel);
        }

        if (wallTabButton != null)
        {
            wallTabButton.onClick.RemoveAllListeners();
            wallTabButton.onClick.AddListener(SelectWallMode);
        }

        if (floorTabButton != null)
        {
            floorTabButton.onClick.RemoveAllListeners();
            floorTabButton.onClick.AddListener(SelectFloorMode);
        }

        if (ceilingTabButton != null)
        {
            ceilingTabButton.onClick.RemoveAllListeners();
            ceilingTabButton.onClick.AddListener(SelectCeilingMode);
        }

        if (plainTextureButton != null)
        {
            plainTextureButton.onClick.RemoveAllListeners();
            plainTextureButton.onClick.AddListener(ApplyPlainTexture);
        }

        if (woodTextureButton != null)
        {
            woodTextureButton.onClick.RemoveAllListeners();
            woodTextureButton.onClick.AddListener(ApplyWoodTexture);
        }

        if (concreteTextureButton != null)
        {
            concreteTextureButton.onClick.RemoveAllListeners();
            concreteTextureButton.onClick.AddListener(ApplyConcreteTexture);
        }

        if (marbleTextureButton != null)
        {
            marbleTextureButton.onClick.RemoveAllListeners();
            marbleTextureButton.onClick.AddListener(ApplyMarbleTexture);
        }

        if (resetAllButton != null)
        {
            resetAllButton.onClick.RemoveAllListeners();
            resetAllButton.onClick.AddListener(ResetAll);
        }
    }

    public void RegisterRoom(CustomRoomBuilder builder)
    {
        currentBuilder = builder;

        if (currentBuilder == null)
            return;

        ModularRoomConfig config = currentBuilder.GetCurrentConfig();

        widthSlider.SetValueWithoutNotify(config.width);
        heightSlider.SetValueWithoutNotify(config.height);
        depthSlider.SetValueWithoutNotify(config.depth);

        if (ceilingToggle != null)
            ceilingToggle.SetIsOnWithoutNotify(config.showCeiling);

        ConnectSliders();
        RefreshColorSliders();
        RefreshTargetLabel();
        RefreshLiveValueLabels();
        RefreshColourPreview();
        RefreshSurfaceTabs();
        RefreshTextureButtons();
    }

    private void ConnectSliders()
    {
        widthSlider.onValueChanged.RemoveAllListeners();
        heightSlider.onValueChanged.RemoveAllListeners();
        depthSlider.onValueChanged.RemoveAllListeners();
        redSlider.onValueChanged.RemoveAllListeners();
        greenSlider.onValueChanged.RemoveAllListeners();
        blueSlider.onValueChanged.RemoveAllListeners();

        widthSlider.onValueChanged.AddListener(OnWidthChanged);
        heightSlider.onValueChanged.AddListener(OnHeightChanged);
        depthSlider.onValueChanged.AddListener(OnDepthChanged);
        redSlider.onValueChanged.AddListener(OnRedChanged);
        greenSlider.onValueChanged.AddListener(OnGreenChanged);
        blueSlider.onValueChanged.AddListener(OnBlueChanged);

        if (ceilingToggle != null)
        {
            ceilingToggle.onValueChanged.RemoveAllListeners();
            ceilingToggle.onValueChanged.AddListener(OnCeilingToggleChanged);
        }
    }

    public void SetOwnerInside(bool inside)
    {
        isOwnerInside = inside && RoomManager.IsOwner;

        if (customRoomToggleButton != null)
            customRoomToggleButton.SetActive(isOwnerInside);

        if (!isOwnerInside)
            HidePanel();
    }

    public void TogglePanel()
    {
        if (!isOwnerInside)
            return;

        if (customRoomPanel != null)
        {
            bool newState = !customRoomPanel.activeSelf;
            customRoomPanel.SetActive(newState);

            if (newState)
            {
                customRoomPanel.transform.SetAsLastSibling();
                RefreshLiveValueLabels();
                RefreshColourPreview();
                RefreshSurfaceTabs();
                RefreshTextureButtons();
            }
        }
    }

    public void ShowPanel()
    {
        if (!isOwnerInside)
            return;

        if (customRoomPanel != null)
        {
            customRoomPanel.SetActive(true);
            customRoomPanel.transform.SetAsLastSibling();
        }

        RefreshLiveValueLabels();
        RefreshColourPreview();
        RefreshSurfaceTabs();
        RefreshTextureButtons();
    }

    public void HidePanel()
    {
        if (customRoomPanel != null)
            customRoomPanel.SetActive(false);
    }

    private void OnWidthChanged(float value)
    {
        if (currentBuilder != null)
            currentBuilder.SetWidth(value);

        RefreshLiveValueLabels();
    }

    private void OnHeightChanged(float value)
    {
        if (currentBuilder != null)
            currentBuilder.SetHeight(value);

        RefreshLiveValueLabels();
    }

    private void OnDepthChanged(float value)
    {
        if (currentBuilder != null)
            currentBuilder.SetDepth(value);

        RefreshLiveValueLabels();
    }

    private void OnRedChanged(float value)
    {
        if (currentBuilder == null) return;

        ModularRoomConfig cfg = currentBuilder.GetCurrentConfig();

        switch (currentTarget)
        {
            case EditTarget.Wall:
                currentBuilder.SetWallColor(value, cfg.wallColorG, cfg.wallColorB);
                break;

            case EditTarget.Floor:
                currentBuilder.SetFloorColor(value, cfg.floorColorG, cfg.floorColorB);
                break;

            case EditTarget.Ceiling:
                currentBuilder.SetCeilingColor(value, cfg.ceilingColorG, cfg.ceilingColorB);
                break;
        }

        RefreshLiveValueLabels();
        RefreshColourPreview();
    }

    private void OnGreenChanged(float value)
    {
        if (currentBuilder == null) return;

        ModularRoomConfig cfg = currentBuilder.GetCurrentConfig();

        switch (currentTarget)
        {
            case EditTarget.Wall:
                currentBuilder.SetWallColor(cfg.wallColorR, value, cfg.wallColorB);
                break;

            case EditTarget.Floor:
                currentBuilder.SetFloorColor(cfg.floorColorR, value, cfg.floorColorB);
                break;

            case EditTarget.Ceiling:
                currentBuilder.SetCeilingColor(cfg.ceilingColorR, value, cfg.ceilingColorB);
                break;
        }

        RefreshLiveValueLabels();
        RefreshColourPreview();
    }

    private void OnBlueChanged(float value)
    {
        if (currentBuilder == null) return;

        ModularRoomConfig cfg = currentBuilder.GetCurrentConfig();

        switch (currentTarget)
        {
            case EditTarget.Wall:
                currentBuilder.SetWallColor(cfg.wallColorR, cfg.wallColorG, value);
                break;

            case EditTarget.Floor:
                currentBuilder.SetFloorColor(cfg.floorColorR, cfg.floorColorG, value);
                break;

            case EditTarget.Ceiling:
                currentBuilder.SetCeilingColor(cfg.ceilingColorR, cfg.ceilingColorG, value);
                break;
        }

        RefreshLiveValueLabels();
        RefreshColourPreview();
    }

    public void ApplyTexture(string textureName)
    {
        if (currentBuilder == null)
            return;

        currentTexture = textureName;

        switch (currentTarget)
        {
            case EditTarget.Wall:
                currentBuilder.SetWallTexture(textureName);
                break;

            case EditTarget.Floor:
                currentBuilder.SetFloorTexture(textureName);
                break;

            case EditTarget.Ceiling:
                currentBuilder.SetCeilingTexture(textureName);
                break;
        }

        RefreshTextureButtons();
    }

    public void ApplyPlainTexture()
    {
        ApplyTexture("Plain");
    }

    public void ApplyWoodTexture()
    {
        ApplyTexture("Wood");
    }

    public void ApplyConcreteTexture()
    {
        ApplyTexture("Concrete");
    }

    public void ApplyMarbleTexture()
    {
        ApplyTexture("Marble");
    }

    private void OnCeilingToggleChanged(bool value)
    {
        if (currentBuilder != null)
            currentBuilder.ToggleCeiling(value);
    }

    public void SelectWallMode()
    {
        currentTarget = EditTarget.Wall;
        LoadCurrentTargetTexture();
        RefreshColorSliders();
        RefreshTargetLabel();
        RefreshColourPreview();
        RefreshSurfaceTabs();
        RefreshTextureButtons();
        RefreshLiveValueLabels();
    }

    public void SelectFloorMode()
    {
        currentTarget = EditTarget.Floor;
        LoadCurrentTargetTexture();
        RefreshColorSliders();
        RefreshTargetLabel();
        RefreshColourPreview();
        RefreshSurfaceTabs();
        RefreshTextureButtons();
        RefreshLiveValueLabels();
    }

    public void SelectCeilingMode()
    {
        currentTarget = EditTarget.Ceiling;
        LoadCurrentTargetTexture();
        RefreshColorSliders();
        RefreshTargetLabel();
        RefreshColourPreview();
        RefreshSurfaceTabs();
        RefreshTextureButtons();
        RefreshLiveValueLabels();
    }

    private void RefreshColorSliders()
    {
        if (currentBuilder == null) return;

        ModularRoomConfig cfg = currentBuilder.GetCurrentConfig();

        switch (currentTarget)
        {
            case EditTarget.Wall:
                redSlider.SetValueWithoutNotify(cfg.wallColorR);
                greenSlider.SetValueWithoutNotify(cfg.wallColorG);
                blueSlider.SetValueWithoutNotify(cfg.wallColorB);
                break;

            case EditTarget.Floor:
                redSlider.SetValueWithoutNotify(cfg.floorColorR);
                greenSlider.SetValueWithoutNotify(cfg.floorColorG);
                blueSlider.SetValueWithoutNotify(cfg.floorColorB);
                break;

            case EditTarget.Ceiling:
                redSlider.SetValueWithoutNotify(cfg.ceilingColorR);
                greenSlider.SetValueWithoutNotify(cfg.ceilingColorG);
                blueSlider.SetValueWithoutNotify(cfg.ceilingColorB);
                break;
        }
    }

    private void RefreshTargetLabel()
    {
        if (selectedTargetText == null) return;

        selectedTargetText.text = "Editing: " + currentTarget.ToString();
    }

    private void RefreshLiveValueLabels()
    {
        if (widthValueText != null)
            widthValueText.text = widthSlider.value.ToString("F1") + "m";

        if (heightValueText != null)
            heightValueText.text = heightSlider.value.ToString("F1") + "m";

        if (depthValueText != null)
            depthValueText.text = depthSlider.value.ToString("F1") + "m";

        if (redValueText != null)
            redValueText.text = Mathf.RoundToInt(redSlider.value * 255f).ToString();

        if (greenValueText != null)
            greenValueText.text = Mathf.RoundToInt(greenSlider.value * 255f).ToString();

        if (blueValueText != null)
            blueValueText.text = Mathf.RoundToInt(blueSlider.value * 255f).ToString();
    }

    private void RefreshColourPreview()
    {
        if (colourPreviewImage == null)
            return;

        colourPreviewImage.color = new Color(
            redSlider.value,
            greenSlider.value,
            blueSlider.value,
            1f
        );
    }

    private void RefreshSurfaceTabs()
    {
        SetTabVisual(wallTabImage, wallTabText, currentTarget == EditTarget.Wall);
        SetTabVisual(floorTabImage, floorTabText, currentTarget == EditTarget.Floor);
        SetTabVisual(ceilingTabImage, ceilingTabText, currentTarget == EditTarget.Ceiling);
    }

    private void SetTabVisual(Image img, TMP_Text txt, bool selected)
    {
        if (img != null)
            img.color = selected ? selectedBgColor : unselectedBgColor;

        if (txt != null)
            txt.color = selected ? selectedTextColor : unselectedTextColor;
    }

    private void RefreshTextureButtons()
    {
        SetTextureVisual(plainTextureImage, plainTextureText, currentTexture == "Plain");
        SetTextureVisual(woodTextureImage, woodTextureText, currentTexture == "Wood");
        SetTextureVisual(concreteTextureImage, concreteTextureText, currentTexture == "Concrete");
        SetTextureVisual(marbleTextureImage, marbleTextureText, currentTexture == "Marble");
    }

    private void SetTextureVisual(Image img, TMP_Text txt, bool selected)
    {
        if (img != null)
            img.color = selected ? selectedBgColor : unselectedBgColor;

        if (txt != null)
            txt.color = selected ? selectedTextColor : unselectedTextColor;
    }

    private void LoadCurrentTargetTexture()
    {
        if (currentBuilder == null)
            return;

        ModularRoomConfig cfg = currentBuilder.GetCurrentConfig();

        switch (currentTarget)
        {
            case EditTarget.Wall:
                currentTexture = string.IsNullOrEmpty(cfg.wallTextureName) ? "Plain" : cfg.wallTextureName;
                break;

            case EditTarget.Floor:
                currentTexture = string.IsNullOrEmpty(cfg.floorTextureName) ? "Plain" : cfg.floorTextureName;
                break;

            case EditTarget.Ceiling:
                currentTexture = string.IsNullOrEmpty(cfg.ceilingTextureName) ? "Plain" : cfg.ceilingTextureName;
                break;
        }
    }

    public void ResetSize()
    {
        if (currentBuilder == null) return;

        currentBuilder.ResetSize();

        ModularRoomConfig cfg = currentBuilder.GetCurrentConfig();

        widthSlider.SetValueWithoutNotify(cfg.width);
        heightSlider.SetValueWithoutNotify(cfg.height);
        depthSlider.SetValueWithoutNotify(cfg.depth);

        RefreshLiveValueLabels();
    }

    public void ResetCurrentColor()
    {
        if (currentBuilder == null) return;

        currentBuilder.ResetCurrentTargetColor(currentTarget.ToString());
        RefreshColorSliders();
        RefreshLiveValueLabels();
        RefreshColourPreview();
    }

    public void ResetCurrentTexture()
    {
        if (currentBuilder == null) return;

        currentBuilder.ResetCurrentTargetTexture(currentTarget.ToString());
        LoadCurrentTargetTexture();
        RefreshTextureButtons();
    }

    public void ResetAll()
    {
        if (currentBuilder == null) return;

        currentBuilder.ResetAllCustomRoom();

        ModularRoomConfig cfg = currentBuilder.GetCurrentConfig();

        widthSlider.SetValueWithoutNotify(cfg.width);
        heightSlider.SetValueWithoutNotify(cfg.height);
        depthSlider.SetValueWithoutNotify(cfg.depth);

        if (ceilingToggle != null)
            ceilingToggle.SetIsOnWithoutNotify(cfg.showCeiling);

        currentTarget = EditTarget.Wall;
        LoadCurrentTargetTexture();

        RefreshColorSliders();
        RefreshTargetLabel();
        RefreshLiveValueLabels();
        RefreshColourPreview();
        RefreshSurfaceTabs();
        RefreshTextureButtons();
    }

    public void ApplyDefaultPreset()
    {
        if (currentBuilder == null) return;
        currentBuilder.ApplyDefaultPreset();
        RefreshAfterPreset();
    }

    public void ApplyModernPreset()
    {
        if (currentBuilder == null) return;
        currentBuilder.ApplyModernPreset();
        RefreshAfterPreset();
    }

    public void ApplyWoodPreset()
    {
        if (currentBuilder == null) return;
        currentBuilder.ApplyWoodPreset();
        RefreshAfterPreset();
    }

    public void ApplyConcretePreset()
    {
        if (currentBuilder == null) return;
        currentBuilder.ApplyConcretePreset();
        RefreshAfterPreset();
    }

    public void ApplyMarblePreset()
    {
        if (currentBuilder == null) return;
        currentBuilder.ApplyMarblePreset();
        RefreshAfterPreset();
    }

    private void RefreshAfterPreset()
    {
        LoadCurrentTargetTexture();
        RefreshColorSliders();
        RefreshTargetLabel();
        RefreshLiveValueLabels();
        RefreshColourPreview();
        RefreshSurfaceTabs();
        RefreshTextureButtons();
    }
}