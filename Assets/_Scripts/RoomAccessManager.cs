public static class RoomAccessManager
{
    public static bool CanEdit => RoomManager.IsOwner || HasEditPermission;
    public static bool HasEditPermission { get; private set; } = false;

    public static void SetEditPermission(bool value)
    {
        HasEditPermission = value;
        UnityEngine.Debug.Log("[ACCESS] CanEdit=" + CanEdit);
    }

    public static void Reset()
    {
        HasEditPermission = false;
    }
}