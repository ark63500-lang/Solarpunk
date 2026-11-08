using UnityEngine;

// voyant qui clignote en haut de l'antenne
public class AntenneVoyant : MonoBehaviour
{
    public Renderer voyant;
    public Light lumiere;
    public Color couleur = new Color(1f, 0.1f, 0.05f);
    public float periode = 1.2f;
    public float intensiteEmission = 6f;

    Material materiau;
    bool etatPrecedent;
    bool premier = true;

    void Start()
    {
        if (voyant == null) return;
        materiau = voyant.material;
        materiau.EnableKeyword("_EMISSION");
    }

    void Update()
    {
        bool allume = (Time.time % periode) < periode * 0.4f;
        if (!premier && allume == etatPrecedent) return;
        premier = false;
        etatPrecedent = allume;
        if (materiau != null) materiau.SetColor("_EmissionColor", allume ? couleur * intensiteEmission : couleur * 0.05f);
        if (lumiere != null) lumiere.enabled = allume;
    }
}
