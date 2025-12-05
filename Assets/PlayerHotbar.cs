using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerHotbar : MonoBehaviour
{
    public Item[] hotbarItems = new Item[5]; // 5 slots
    public HotbarSlotUI[] hotbarSlots;

    public bool AddItem(Item item)
    {
        Debug.Log("PlayerHotbar.AddItem called with item: " + (item != null ? item.itemName : "NULL"));

        if (hotbarSlots == null || hotbarSlots.Length != hotbarItems.Length)
        {
            Debug.LogError("PlayerHotbar: hotbarSlots array is not set up correctly in the Inspector! Its size should be " + hotbarItems.Length);
            return false;
        }

        for (int i = 0; i < hotbarItems.Length; i++)
        {
            if (hotbarItems[i] == null)
            {
                Debug.Log("PlayerHotbar: Found empty slot at index " + i);
                hotbarItems[i] = item;

                if (hotbarSlots[i] != null)
                {
                    Debug.Log("PlayerHotbar: Calling SetItem on hotbarSlots[" + i + "]");
                    hotbarSlots[i].SetItem(item);
                }
                else
                {
                    Debug.LogError("PlayerHotbar: hotbarSlots[" + i + "] is NOT assigned in the Inspector!");
                }
                
                return true;
            }
        }

        Debug.LogWarning("PlayerHotbar: No empty slots found.");
        return false; // no space
    }

    public void ClearHotbar()
    {
        for (int i = 0; i < hotbarItems.Length; i++)
        {
            hotbarItems[i] = null;
            if (hotbarSlots[i] != null)
            {
                hotbarSlots[i].ClearSlot();
            }
        }
    }
}
