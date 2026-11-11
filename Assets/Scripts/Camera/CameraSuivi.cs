// CameraSuivi : la caméra suit le drone en douceur.
// Elle garde le même décalage que dans la scène et se rapproche plus vite quand le drone recule.

using UnityEngine;

public class CameraSuivi : MonoBehaviour
{
    public Transform cible;
    public Vector3 decalage = new Vector3(0f, 3f, -6f);
    public float tempsLissage = 0.25f;
    public float tempsLissageRecul = 0.08f;
    public bool regarderLaCible = true;
    public bool garderPlacementManuel = true;

    private Vector3 vitesseCamera;
    private Renderer[] rendus;
    private Vector3 dernierCentre;

    void Start()
    {
        if (cible == null) return;

        rendus = cible.GetComponentsInChildren<Renderer>();

        if (garderPlacementManuel)
            decalage = transform.position - CentreCible();
        else
            transform.position = CentreCible() + decalage;

        dernierCentre = CentreCible();
    }

    Vector3 CentreCible()
    {
        Bounds limites = new Bounds(cible.position, Vector3.zero);
        bool premier = true;
        foreach (var rendu in rendus)
        {
            // les particules (eau, tornade) faussent le centre du drone
            if (rendu == null || rendu is ParticleSystemRenderer || rendu is TrailRenderer) continue;
            if (premier)
            {
                limites = rendu.bounds;
                premier = false;
            }
            else limites.Encapsulate(rendu.bounds);
        }
        return limites.center;
    }

    void LateUpdate()
    {
        if (cible == null) return;

        Vector3 centre = CentreCible();
        Vector3 positionVoulue = centre + decalage;

        Vector3 deplacement = centre - dernierCentre;
        Vector3 versCamera = transform.position - centre;
        deplacement.y = 0f;
        versCamera.y = 0f;
        bool recule = Vector3.Dot(deplacement, versCamera) > 0.0001f;
        dernierCentre = centre;

        float lissage = recule ? tempsLissageRecul : tempsLissage;
        transform.position = Vector3.SmoothDamp(transform.position, positionVoulue, ref vitesseCamera, lissage);

        if (regarderLaCible)
            transform.LookAt(centre);
    }
}
