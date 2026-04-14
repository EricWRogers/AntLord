using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item/Inventory")]

public class Item : ScriptableObject
{
    public string itemName;
    public Sprite icon;
    public bool isStackable;
}