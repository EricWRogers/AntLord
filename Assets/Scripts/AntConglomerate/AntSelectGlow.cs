using UnityEngine;

public class AntSelectGlow : MonoBehaviour
{
    [Header("Glow Settings")]
    public Color glowColor = Color.green;
    public float glowIntensity = 2.5f;

    Renderer[] renderers;
    MaterialPropertyBlock mpb;
    Color[][] originalEmissions;

    static readonly int EmissionId = Shader.PropertyToID("_EmissionColor");

    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        mpb = new MaterialPropertyBlock();

        originalEmissions = new Color[renderers.Length][];
        for (int r = 0; r < renderers.Length; r++)
        {
            var mats = renderers[r].sharedMaterials;
            originalEmissions[r] = new Color[mats.Length];

            for (int i = 0; i < mats.Length; i++)
            {
                if (mats[i] != null && mats[i].HasProperty(EmissionId))
                    originalEmissions[r][i] = mats[i].GetColor(EmissionId);
                else
                    originalEmissions[r][i] = Color.black;
            }
        }
    }

    public void SetSelected(bool selected)
    {
        if (selected) EnableGlow(glowColor, glowIntensity);
        else DisableGlow();
    }

    public void EnableGlow(Color color, float intensity)
    {
        Color hdr = color * intensity;

        for (int r = 0; r < renderers.Length; r++)
        {
            
            var mats = renderers[r].sharedMaterials;
            for (int i = 0; i < mats.Length; i++)
                if (mats[i] != null) mats[i].EnableKeyword("_EMISSION");

            
            renderers[r].GetPropertyBlock(mpb);
            mpb.SetColor(EmissionId, hdr);
            renderers[r].SetPropertyBlock(mpb);
        }
    }

    public void DisableGlow()
    {
        for (int r = 0; r < renderers.Length; r++)
        {
            renderers[r].GetPropertyBlock(mpb);

        
            mpb.SetColor(EmissionId, originalEmissions[r].Length > 0 ? originalEmissions[r][0] : Color.black);

            renderers[r].SetPropertyBlock(mpb);
        }
    }
}