using UnityEngine;

[CreateAssetMenu(fileName = "BuildingSO", menuName = "AntLord/BuildingSO")]
public class BuildingSO : ScriptableObject
{
    // this is where all building will refer to their names, desc, cost, health, 
    // and what ant it spawns (if it spawns ants)
    //in the future this will likely hold the UI sprite and a prefab of whatever building this is for
    //NOTE**when getting buildHealth for a new building type
    //**you MUST make some kind of currentHealth var
    //**else all buildings will share damage
    public string buildName;
    public string buildDesc;
    public int buildCost;
    public int buildHealth;
    public GameObject ant;
}
