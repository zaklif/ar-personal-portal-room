//using UnityEngine;
//using System;

///// <summary>
///// Stores metadata for each placed object.
///// Attach automatically at runtime — no manual setup needed.
///// </summary>
//public class ObjectMetaData : MonoBehaviour
//{
//    public enum ObjectType { ViewOnly, ForSale }

//    [Header("Object Info")]
//    public string objectName = "My Object";
//    public string description = "";
//    public ObjectType objectType = ObjectType.ViewOnly;
//    public float price = 0f;
//    public string currency = "MYR";

//    // Owner contact — set from RoomManager
//    public string ownerRoomId = "";

//    // Indicator UI floating above this object
//    private GameObject indicatorInstance;
//    private ObjectIndicatorUI indicatorUI;

//    public bool IsForSale => objectType == ObjectType.ForSale;

//    public void Init(string roomId)
//    {
//        ownerRoomId = roomId;
//    }

//    public void SetIndicator(GameObject indicator)
//    {
//        indicatorInstance = indicator;
//        indicatorUI = indicator.GetComponent<ObjectIndicatorUI>();
//        RefreshIndicator();
//    }

//    public void RefreshIndicator()
//    {
//        if (indicatorUI != null)
//            indicatorUI.Refresh(this);
//    }

//    // Convert to saveable data
//    public ObjectMetaSaveData ToSaveData()
//    {
//        return new ObjectMetaSaveData
//        {
//            objectName = objectName,
//            description = description,
//            objectType = (int)objectType,
//            price = price,
//            currency = currency
//        };
//    }

//    // Load from saved data
//    public void FromSaveData(ObjectMetaSaveData data)
//    {
//        objectName = data.objectName;
//        description = data.description;
//        objectType = (ObjectType)data.objectType;
//        price = data.price;
//        currency = data.currency;
//        RefreshIndicator();
//    }
//}

//[Serializable]
//public class ObjectMetaSaveData
//{
//    public string objectName = "My Object";
//    public string description = "";
//    public int objectType = 0; // 0 = ViewOnly, 1 = ForSale
//    public float price = 0f;
//    public string currency = "MYR";
//}

using UnityEngine;
using System;

public class ObjectMetaData : MonoBehaviour
{
    public enum ObjectType { ViewOnly, ForSale }

    [Header("Object Info")]
    public string objectName = "My Object";
    public string description = "";
    public ObjectType objectType = ObjectType.ViewOnly;
    public float price = 0f;
    public string currency = "RM";
    public string ownerRoomId = "";

    public string ownerId = "";

    // FIX: unique ID per object so RoomManager can find it reliably
    public string instanceId = "";

    private GameObject indicatorInstance;
    private ObjectIndicatorUI indicatorUI;

    public bool IsForSale => objectType == ObjectType.ForSale;

    public void Init(string roomId)
    {
        ownerRoomId = roomId;

        if (RoomManager.Instance != null && RoomManager.Instance.CurrentRoom != null)
            ownerId = RoomManager.Instance.CurrentRoom.ownerId;

        if (string.IsNullOrEmpty(instanceId))
            instanceId = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();
    }

    public void SetIndicator(GameObject indicator)
    {
        indicatorInstance = indicator;
        //indicatorUI = indicator.GetComponent<ObjectIndicatorUI>();
        //RefreshIndicator();
    }

    public void DestroyIndicator()
    {
        if (indicatorInstance != null)
        {
            Destroy(indicatorInstance);
            indicatorInstance = null;
        }
    }

    private void OnDestroy()
    {
        DestroyIndicator();
    }

    public void RefreshIndicator()
    {
        if (indicatorInstance == null)
            return;

        ObjectIndicatorUI indicatorUI = indicatorInstance.GetComponent<ObjectIndicatorUI>();

        if (indicatorUI != null)
            indicatorUI.SetTarget(transform);
    }


    
}