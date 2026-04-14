using System.Collections.Generic;
using UnityEngine;
public class InventoryScript : MonoBehaviour
{
    public static InventoryScript Instance;
    public List<Item> inventory  = new List<Item>();
    void Awake() => Instance = this;
    
    public void AddItem(Item item)
    {
        inventory.Add(item);
        Debug.Log("Added " + item.itemName);
    }

    public void RemoveItem(Item item)
    {
        inventory.Remove(item);
        Debug.Log("Removed " + item.itemName);
    }
}
