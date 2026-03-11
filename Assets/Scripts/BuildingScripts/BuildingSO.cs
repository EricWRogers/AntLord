using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BuildingSO", menuName = "AntLord/BuildingSO")]
public class BuildingSO : ScriptableObject
{
    // this is where all building will refer to their names, desc, cost, health, 
    // and what ant it spawns (if it spawns ants)
    //in the future this will likely hold the UI sprite and a prefab of whatever building this is for
    //NOTE**when getting buildHealth for a new building type
    //**you MUST make some kind of currentHealth var
    //**else all buildings will share damage
    [field: SerializeField] public string buildName { get; set; }
    [field: SerializeField] public string buildDesc { get; set; }
    [field: SerializeField] public int ID { get; set; }
    [field: SerializeField] public int buildCost { get; set; }
    [field: SerializeField] public int buildHealth { get; set; }
    [field: SerializeField] public Vector3Int size { get; set; } = Vector3Int.one;
    [field: SerializeField] public GameObject preFab { get; set; }
}
