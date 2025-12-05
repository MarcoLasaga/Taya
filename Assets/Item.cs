using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "TAYA/Item")]
public class Item : ScriptableObject
{
    public string itemName;

    [Header("The prefab model to show in the hotbar")]
    public GameObject prefabModel;
}



