using UnityEngine;

/// <summary>
/// A ScriptableObject that defines one room template.
/// Create one asset per room type (Modern, Classic, Minimalist etc.)
///
/// SETUP:
/// 1. Right click in Project window → Create → AR Room → Room Template
/// 2. Fill in the fields for each room type
/// 3. Assign the room prefab (the full room with walls/floor/roof)
/// </summary>
[CreateAssetMenu(fileName = "RoomTemplate", menuName = "AR Room/Room Template")]
public class RoomTemplateData : ScriptableObject
{
    [Header("Display Info")]
    public string templateName;        // e.g. "Modern Room"
    public string description;         // e.g. "Clean lines, minimalist style"
    public Sprite previewImage;        // thumbnail shown in selection UI

    [Header("Prefab")]
    public GameObject roomPrefab;      // the full room prefab (walls/floor/roof + portal door)

    [Header("Spawn Settings")]
    public Vector3 prefabRotationOffset = new Vector3(90f, 180f, 0f); // same as your current offset
    public float spawnHeightOffset = 1.0f; // how high above floor to spawn
}