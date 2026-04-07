using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(MarchingCubes))]
public class VoxelNavGrid : MonoBehaviour
{
    [Header("Sampling")]
    public LayerMask groundMask;
    public float rayStartHeight = 120f;
    public float rayLength = 300f;

    [Header("Grid")]
    public float cellSize = 1f;

    [Header("Walkability")]
    public float maxStepHeight = 1.25f;
    public float maxSlopeDeg = 60f;

    [Header("Cost")]
    public float slopePenalty = 2.0f;
    public float slopePower = 2.0f;

    public Vector3 origin;
    public int sizeX, sizeZ;

    public float[,] heightY;
    public bool[,] walkable;
    public float[,] cost;

    readonly Queue<RectInt> dirty = new();
    public bool IsReady => heightY != null && dirty.Count == 0;

    MarchingCubes terrain;

    void Awake()
    {
        terrain = GetComponent<MarchingCubes>();
        RebuildAll();
    }

    public void RebuildAll()
    {
        origin = terrain.transform.position;

        float worldX = terrain.WorldSizeX;
        float worldZ = terrain.WorldSizeZ;

        sizeX = Mathf.CeilToInt(worldX / cellSize);
        sizeZ = Mathf.CeilToInt(worldZ / cellSize);

        heightY = new float[sizeX, sizeZ];
        walkable = new bool[sizeX, sizeZ];
        cost     = new float[sizeX, sizeZ];

        for (int z = 0; z < sizeZ; z++)
        for (int x = 0; x < sizeX; x++)
            heightY[x, z] = float.NaN;

        dirty.Clear();
        dirty.Enqueue(new RectInt(0, 0, sizeX, sizeZ));
    }

    public void MarkDirty(Vector3 worldPos, float radiusWorld)
    {
        int cx = Mathf.FloorToInt((worldPos.x - origin.x) / cellSize);
        int cz = Mathf.FloorToInt((worldPos.z - origin.z) / cellSize);
        int r = Mathf.CeilToInt(radiusWorld / cellSize) + 3;

        int x0 = Mathf.Clamp(cx - r, 0, sizeX - 1);
        int z0 = Mathf.Clamp(cz - r, 0, sizeZ - 1);
        int x1 = Mathf.Clamp(cx + r, 0, sizeX - 1);
        int z1 = Mathf.Clamp(cz + r, 0, sizeZ - 1);

        dirty.Enqueue(new RectInt(x0, z0, x1 - x0 + 1, z1 - z0 + 1));
    }

    void Update()
    {
        if (dirty.Count == 0) return;

        int budget = 2500;
        int done = 0;

        while (dirty.Count > 0 && done < budget)
        {
            RectInt rect = dirty.Dequeue();

            for (int z = rect.yMin; z < rect.yMax; z++)
            for (int x = rect.xMin; x < rect.xMax; x++)
            {
                SampleHeight(x, z);
                done++;
                if (done >= budget) break;
            }

            int pad = 1;
            int x0 = Mathf.Clamp(rect.xMin - pad, 0, sizeX - 1);
            int x1 = Mathf.Clamp(rect.xMax + pad, 0, sizeX);
            int z0 = Mathf.Clamp(rect.yMin - pad, 0, sizeZ - 1);
            int z1 = Mathf.Clamp(rect.yMax + pad, 0, sizeZ);

            for (int z = z0; z < z1; z++)
            for (int x = x0; x < x1; x++)
                RecomputeCell(x, z);
        }
    }

    void SampleHeight(int gx, int gz)
    {
        Vector3 p = origin + new Vector3((gx + 0.5f) * cellSize, rayStartHeight, (gz + 0.5f) * cellSize);
        if (Physics.Raycast(p, Vector3.down, out RaycastHit hit, rayLength, groundMask, QueryTriggerInteraction.Ignore))
            heightY[gx, gz] = hit.point.y;
        else
            heightY[gx, gz] = float.NaN;
    }

    void RecomputeCell(int x, int z)
    {
        float hC = heightY[x, z];
        if (float.IsNaN(hC))
        {
            walkable[x, z] = false;
            cost[x, z] = float.PositiveInfinity;
            return;
        }

        int x0 = Mathf.Max(0, x - 1);
        int x1 = Mathf.Min(sizeX - 1, x + 1);
        int z0 = Mathf.Max(0, z - 1);
        int z1 = Mathf.Min(sizeZ - 1, z + 1);

        float hL = heightY[x0, z];
        float hR = heightY[x1, z];
        float hD = heightY[x, z0];
        float hU = heightY[x, z1];

        if (float.IsNaN(hL) || float.IsNaN(hR) || float.IsNaN(hD) || float.IsNaN(hU))
        {
            walkable[x, z] = false;
            cost[x, z] = float.PositiveInfinity;
            return;
        }

        float dHx = (hR - hL) / (2f * cellSize);
        float dHz = (hU - hD) / (2f * cellSize);
        float grad = Mathf.Sqrt(dHx * dHx + dHz * dHz);
        float slopeDeg = Mathf.Atan(grad) * Mathf.Rad2Deg;

        bool stepBad =
            Mathf.Abs(hL - hC) > maxStepHeight ||
            Mathf.Abs(hR - hC) > maxStepHeight ||
            Mathf.Abs(hD - hC) > maxStepHeight ||
            Mathf.Abs(hU - hC) > maxStepHeight;

        bool tooSteep = slopeDeg > maxSlopeDeg;

        walkable[x, z] = !(stepBad || tooSteep);

        float s01 = Mathf.Clamp01(slopeDeg / maxSlopeDeg);
        float slopeCost = 1f + slopePenalty * Mathf.Pow(s01, slopePower);

        cost[x, z] = walkable[x, z] ? slopeCost : float.PositiveInfinity;
    }

    public bool WorldToGrid(Vector3 w, out int gx, out int gz)
    {
        gx = Mathf.FloorToInt((w.x - origin.x) / cellSize);
        gz = Mathf.FloorToInt((w.z - origin.z) / cellSize);
        return gx >= 0 && gz >= 0 && gx < sizeX && gz < sizeZ;
    }

    public Vector3 GridToWorld(int gx, int gz)
    {
        float y = heightY[gx, gz];
        if (float.IsNaN(y)) y = origin.y;
        return new Vector3(origin.x + (gx + 0.5f) * cellSize, y, origin.z + (gz + 0.5f) * cellSize);
    }
}