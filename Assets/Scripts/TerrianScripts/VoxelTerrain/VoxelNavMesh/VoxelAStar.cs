using System.Collections.Generic;
using UnityEngine;

public class VoxelAStar : MonoBehaviour
{
    public VoxelNavGrid grid;

    static readonly (int dx, int dz, float w)[] N =
    {
        ( 1,0,1f),(-1,0,1f),(0, 1,1f),(0,-1,1f),
        ( 1,1,1.4142f),(1,-1,1.4142f),(-1,1,1.4142f),(-1,-1,1.4142f)
    };

    void Awake()
    {
        if (!grid) grid = FindFirstObjectByType<VoxelNavGrid>();
    }

    public bool TryPath(Vector3 startW, Vector3 endW, out List<Vector3> worldPath)
    {
        worldPath = new List<Vector3>();
        if (grid == null || !grid.IsReady) return false;

        if (!grid.WorldToGrid(startW, out int sx, out int sz)) return false;
        if (!grid.WorldToGrid(endW, out int ex, out int ez)) return false;

        if (!grid.walkable[sx, sz] || !grid.walkable[ex, ez]) return false;

        var open = new PQ();
        var came = new Dictionary<int, int>();
        var g = new Dictionary<int, float>();

        int start = Key(sx, sz);
        int goal  = Key(ex, ez);

        g[start] = 0f;
        open.Push(start, Heu(sx, sz, ex, ez));

        int guard = 0;
        while (open.Count > 0 && guard++ < 250000)
        {
            int cur = open.Pop();
            if (cur == goal)
            {
                Reconstruct(goal, start, came, ref worldPath);
                return true;
            }

            Unkey(cur, out int cx, out int cz);

            foreach (var (dx, dz, w) in N)
            {
                int nx = cx + dx, nz = cz + dz;
                if (nx < 0 || nz < 0 || nx >= grid.sizeX || nz >= grid.sizeZ) continue;
                if (!grid.walkable[nx, nz]) continue;

                float stepCost = grid.cost[nx, nz];
                float tentative = g[cur] + w * stepCost;

                int nk = Key(nx, nz);
                if (!g.TryGetValue(nk, out float old) || tentative < old)
                {
                    came[nk] = cur;
                    g[nk] = tentative;
                    float f = tentative + Heu(nx, nz, ex, ez);
                    open.Push(nk, f);
                }
            }
        }

        return false;
    }

    void Reconstruct(int goal, int start, Dictionary<int,int> came, ref List<Vector3> path)
    {
        path.Clear();
        int cur = goal;
        path.Add(ToWorld(cur));

        while (cur != start && came.TryGetValue(cur, out int prev))
        {
            cur = prev;
            path.Add(ToWorld(cur));
        }

        path.Reverse();
    }

    Vector3 ToWorld(int key)
    {
        Unkey(key, out int gx, out int gz);
        return grid.GridToWorld(gx, gz);
    }

    float Heu(int x, int z, int ex, int ez)
    {
        float dx = ex - x, dz = ez - z;
        return Mathf.Sqrt(dx*dx + dz*dz);
    }

    int Key(int x, int z) => (x << 16) ^ (z & 0xFFFF);
    void Unkey(int k, out int x, out int z) { x = (k >> 16); z = (short)(k & 0xFFFF); }

    class PQ
    {
        readonly List<(int k, float p)> h = new();
        public int Count => h.Count;
        public void Push(int k, float p) { h.Add((k,p)); Up(h.Count-1); }
        public int Pop()
        {
            int r = h[0].k;
            h[0] = h[h.Count-1];
            h.RemoveAt(h.Count-1);
            if (h.Count > 0) Down(0);
            return r;
        }
        void Up(int i)
        {
            while (i > 0)
            {
                int p = (i - 1)/2;
                if (h[p].p <= h[i].p) break;
                (h[p], h[i]) = (h[i], h[p]);
                i = p;
            }
        }
        void Down(int i)
        {
            while (true)
            {
                int l = i*2+1, r = l+1;
                if (l >= h.Count) break;
                int b = (r < h.Count && h[r].p < h[l].p) ? r : l;
                if (h[i].p <= h[b].p) break;
                (h[i], h[b]) = (h[b], h[i]);
                i = b;
            }
        }
    }
}