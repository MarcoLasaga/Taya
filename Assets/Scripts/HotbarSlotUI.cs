using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HotbarSlotUI : MonoBehaviour
{
    public Transform container; // where prefab is spawned
    private GameObject modelInstance;

    public void SetItem(Item item)
    {
        Debug.Log("HotbarSlotUI (" + gameObject.name + "): SetItem called.");

        ClearSlot();

        if (container == null)
        {
            Debug.LogError("HotbarSlotUI (" + gameObject.name + "): 'Container' transform is NOT assigned in the Inspector!");
            return;
        }

        if (item != null)
        {
            Debug.Log("HotbarSlotUI (" + gameObject.name + "): Item is '" + item.itemName + "'.");
            if (item.prefabModel != null)
            {
                Debug.Log("HotbarSlotUI (" + gameObject.name + "): Instantiating prefab model: " + item.prefabModel.name);
                modelInstance = Instantiate(item.prefabModel, container);
                modelInstance.transform.localPosition = Vector3.zero;
                modelInstance.transform.localRotation = Quaternion.identity;
                modelInstance.transform.localScale = Vector3.one * 150f; // adjust size for UI
            }
            else
            {
                Debug.LogError("HotbarSlotUI (" + gameObject.name + "): item.prefabModel is NULL! Assign it in the Item asset.");
            }
        }
        else
        {
            Debug.LogWarning("HotbarSlotUI (" + gameObject.name + "): SetItem was called with a NULL item.");
        }
    }

    public void ClearSlot()
    {
        if (modelInstance != null)
            Destroy(modelInstance);
    }
}
