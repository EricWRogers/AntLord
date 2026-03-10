using UnityEngine;

public class SelectionRingController : MonoBehaviour
{
    public Transform ring;                 
    public float yOffset = 0.05f;
    public float ringThicknessWorld = 0.35f; 
    public Color ringColor = Color.green;

    MaterialPropertyBlock mpb;
    Renderer rr;

    static readonly int ColorId = Shader.PropertyToID("_Color");
    static readonly int InnerId = Shader.PropertyToID("_Inner");
    static readonly int OuterId = Shader.PropertyToID("_Outer");

    void Awake()
    {
        if (!ring) ring = transform;
        rr = ring.GetComponent<Renderer>();
        mpb = new MaterialPropertyBlock();

        Hide();
    }

    public void Show(Vector3 centerWorld, float radiusWorld)
    {
        if (radiusWorld < 0.05f)
        {
            Hide();
            return;
        }

        // Position on ground
        ring.position = new Vector3(centerWorld.x, centerWorld.y + yOffset, centerWorld.z);

       
        float diameter = radiusWorld * 2f;
        ring.localScale = new Vector3(diameter, diameter, 1f);

        
        float thicknessUV = Mathf.Clamp01((ringThicknessWorld / diameter) * 0.5f);

        float outer = 0.48f;                  // near the edge 
        float inner = Mathf.Max(0f, outer - thicknessUV);

        rr.GetPropertyBlock(mpb);
        mpb.SetColor(ColorId, ringColor);
        mpb.SetFloat(OuterId, outer);
        mpb.SetFloat(InnerId, inner);
        rr.SetPropertyBlock(mpb);

        rr.enabled = true;
    }

    public void Hide()
    {
        if (rr) rr.enabled = false;
    }
}