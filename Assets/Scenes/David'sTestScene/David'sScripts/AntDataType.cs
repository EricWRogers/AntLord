using UnityEngine;

[CreateAssetMenu(fileName = "NewDataType", menuName = "AntLord/AntDataType")]
public class AntDataType : ScriptableObject
{
    public string typeName;

    [Header("Base Stats")]
    public float maxHealth;
    public float damage;
    public float moveSpeed;

    [Header("Logistics")]
    public float carryCapacity; // How much food they can hold
    public float carryWeightLimit; // How heavy an object they can lift

    [Header("Visuals")]
    public GameObject modelPrefab; // The 3D model for this specific ant
}
