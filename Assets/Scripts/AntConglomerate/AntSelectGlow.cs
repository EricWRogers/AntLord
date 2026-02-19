using UnityEngine;

public class AntSelectGlow : MonoBehaviour
{
    private Renderer rend;
    private Color originalEmission;

    void Start()
    {
        rend = GetComponent<Renderer>();
        originalEmission = rend.material.GetColor("_EmissionColor");
    }

    public void EnableGlow(Color glowColor)
    {
        rend.material.EnableKeyword("_EMISSION");
        rend.material.SetColor("_EmissionColor", glowColor);
    }

    public void DisableGlow()
    {
        rend.material.SetColor("_EmissionColor", originalEmission);
    }
}
