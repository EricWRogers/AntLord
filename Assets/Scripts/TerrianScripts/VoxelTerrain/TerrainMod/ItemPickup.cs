using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    public Item item;

    public void OnMouseDown() // Triggers when clicked
    {
        InventoryScript.Instance.AddItem(item);
        Destroy(gameObject); // Remove item from scene
    }
}