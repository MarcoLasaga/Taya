using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public List<Item> items = new List<Item>();
    public int maxSlots = 5;
    public PlayerHotbar playerHotbar;

    private string saveFile;

    private void Awake()
    {
        saveFile = Application.persistentDataPath + "/inventory.json";
    }

    private void Start()
    {
        LoadInventory();
    }

    public bool AddItem(Item item)
    {
        Debug.Log("PlayerInventory.AddItem called with item: " + (item != null ? item.itemName : "NULL"));

        if (items.Count >= maxSlots)
        {
            Debug.LogWarning("PlayerInventory: Inventory is full. Cannot add item.");
            return false;
        }

        items.Add(item);
        Debug.Log("PlayerInventory: Item added to inventory list.");

        if (playerHotbar != null)
        {
            Debug.Log("PlayerInventory: Calling playerHotbar.AddItem...");
            playerHotbar.AddItem(item);
        }
        else
        {
            Debug.LogError("PlayerInventory: playerHotbar is NOT assigned in the Inspector!");
        }

        SaveInventory();
        return true;
    }

    public void SaveInventory()
    {
        InventoryData data = new InventoryData { items = this.items };
        string json = JsonUtility.ToJson(data);
        File.WriteAllText(saveFile, json);
    }

    public void LoadInventory()
    {
        if (File.Exists(saveFile))
        {
            string json = File.ReadAllText(saveFile);
            InventoryData data = JsonUtility.FromJson<InventoryData>(json);
            this.items.Clear(); // Clear existing items before loading
            if (playerHotbar != null)
            {
                playerHotbar.ClearHotbar(); // Clear hotbar display
            }
            foreach (var item in data.items)
            {
                this.items.Add(item);
                if (playerHotbar != null)
                {
                    playerHotbar.AddItem(item);
                }
            }
        }
    }
}

[System.Serializable]
public class InventoryData
{
    public List<Item> items;
}
