using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item itemData;
    public GameObject pickupPrompt;

    private bool canPickup = false;
    private GameObject player;

    private void Start()
    {
        if (pickupPrompt != null)
        {
            // pickupPrompt.SetActive(false);
        }
        else
        {
            Debug.LogError("Pickup prompt is not assigned in the Inspector!", this);
        }
    }

    private void Update()
    {
        if (canPickup && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("'E' key pressed. Attempting to pick up item.");
            PlayerInventory inventory = player.GetComponent<PlayerInventory>();
            if (inventory != null)
            {
                if (inventory.AddItem(itemData))
                {
                    Debug.Log("Item picked up and added to inventory.");
                    Destroy(gameObject);
                }
                else
                {
                    Debug.LogWarning("Failed to add item to inventory. Inventory might be full.", this);
                }
            }
            else
            {
                Debug.LogError("PlayerInventory component not found on the player!", this);
            }
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter called with: " + other.gameObject.name, this);
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player entered the trigger area.", this);
            player = other.gameObject;
            canPickup = true;
            if (pickupPrompt != null)
            {
                pickupPrompt.SetActive(true);
                Debug.Log("Pickup prompt displayed.", this);
            }
        }
        else
        {
            Debug.Log("Object that entered is not the player. Tag is: " + other.tag, this);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            Debug.Log("Player exited the trigger area.", this);
            canPickup = false;
            if (pickupPrompt != null)
            {
                // pickupPrompt.SetActive(false);
                Debug.Log("Pickup prompt hidden.", this);
            }
            player = null;
        }
    }
}
