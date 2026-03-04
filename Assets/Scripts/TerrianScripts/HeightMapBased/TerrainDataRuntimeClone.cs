using UnityEngine;

[RequireComponent(typeof(Terrain))]
public class TerrainDataRuntimeClone : MonoBehaviour
{
    public bool cloneOnAwake = true;

    void Awake()
    {
        if (!cloneOnAwake) return;

        Terrain t = GetComponent<Terrain>();
        if (t.terrainData == null) return;

        // Clone the data so edits don't affect the shared asset
        TerrainData cloned = Instantiate(t.terrainData);
        cloned.name = t.terrainData.name + "_RuntimeClone";

        t.terrainData = cloned;
        
        TerrainCollider tc = GetComponent<TerrainCollider>();
        if (tc != null) tc.terrainData = cloned;
    }
}