using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "BuildingSO", menuName = "AntLord/BuildingSO")]
public class BuildingSO : ScriptableObject
{
    public enum BuildingType
    {
        ResourceExtraction,
        MilitaryIndustiralComplex //praise be the all mighty dollar
    }
    [field: SerializeField] public string buildName { get; set; }
    [field: SerializeField] public string buildDesc { get; set; }
    [field: SerializeField] public int ID { get; set; }
    [field: SerializeField] public int buildCost { get; set; }
    [field: SerializeField] public int buildHealth { get; set; }
    [field: SerializeField] public Vector3Int size { get; set; } = Vector3Int.one;
    [field: SerializeField] public GameObject preFab { get; set; }
    [field: SerializeField] public BuildingType type { get; set; }
}
